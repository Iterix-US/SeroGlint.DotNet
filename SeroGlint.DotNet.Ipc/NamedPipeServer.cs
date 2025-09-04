using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc.Delegates;
using SeroGlint.DotNet.Ipc.EventArguments;
using SeroGlint.DotNet.Ipc.Interfaces;
using SeroGlint.DotNet.Ipc.Objects;
using SeroGlint.DotNet.Ipc.Wrappers;

namespace SeroGlint.DotNet.Ipc
{
    /// <summary>
    /// A named pipe server that handles incoming messages and processes them asynchronously.
    /// </summary>
    public class NamedPipeServer : INamedPipeServer
    {
        private IPipeServerStreamWrapper _pipeServerStreamWrapper;

        public Guid Id { get; private set; } = Guid.NewGuid();
        public bool IsListening => _pipeServerStreamWrapper?.IsListening ?? false;
        public PipeServerConfiguration Configuration { get; }

        public event PipeMessageReceivedHandler MessageReceived;
        public event PipeResponseRequestedHandler ResponseRequested;
        public event PipeServerStateChangedHandler ServerStateChanged;

        public NamedPipeServer(PipeServerConfiguration configuration, IPipeServerStreamWrapper pipeServerStreamWrapper = null)
        {
            Configuration = configuration;
            _pipeServerStreamWrapper = pipeServerStreamWrapper;
        }

        public void Stop()
        {
            if (!IsListening)
            {
                Configuration.Logger.LogInformation(LoggingEventId.ServerNotRunning.GetValue(), LoggingEventId.ServerNotRunning.GetDescription());
                return;
            }

            NotifyServerStopped();
            Dispose();
        }

        /// <summary>
        /// Starts the named pipe server and listens for incoming messages.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task StartAsync()
        {
            InitializeServerStream();
            NotifyServerStarted();

            Configuration.Logger.LogInformation("Waiting for connection on pipe '{PipeName}'...", Configuration.PipeName);

            await MaintainPipeStream();
        }

        private void InitializeServerStream()
        {
            try
            {
                if (_pipeServerStreamWrapper?.IsConnected == false)
                {
                    Configuration.Logger.LogInformation(LoggingEventId.DisposingExistingPipe.GetValue(), "Disposing existing pipe server stream wrapper for '{PipeName}'", Configuration.PipeName);
                    _pipeServerStreamWrapper?.Dispose();
                }

                Configuration.Logger.LogInformation("Attempting to start named pipe server on '{PipeName}'", Configuration.PipeName);
                var serverStream = new NamedPipeServerStream(
                    Configuration.PipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                _pipeServerStreamWrapper = new PipeServerStreamWrapper(serverStream);
            }
            catch (Exception ex)
            {
                Configuration.Logger.LogError(ex, "Failed to start named pipe server on '{PipeName}'", Configuration.PipeName);
            }
        }

        private async Task MaintainPipeStream()
        {
            try
            {
                if (!_pipeServerStreamWrapper.IsListening)
                {
                    await _pipeServerStreamWrapper.WaitForConnectionAsync(Configuration.CancellationTokenSource.Token);
                }

                Configuration.Logger.LogInformation("Client connected on '{PipeName}'", Configuration.PipeName);

                while (_pipeServerStreamWrapper.IsConnected)
                {
                    var buffer = new byte[4096];
                    var bytesRead = await _pipeServerStreamWrapper.ReadAsync(buffer, 0, buffer.Length,
                        Configuration.CancellationTokenSource.Token);
                    var receivedMessage = ProcessReceivedMessage<dynamic>(bytesRead, buffer);
                    await SendResponseAsync(receivedMessage.MessageId);
                }
            }
            catch (Exception ex)
            {
                Configuration.Logger.LogTrace(ex, "Error occurred while handling pipe '{PipeName}'", Configuration.PipeName);

                var envelope = new PipeEnvelope<dynamic>
                {
                    Payload = $"Error handling pipe: {ex.Message}"
                };

                ResponseRequested?.Invoke(this, new PipeResponseRequestedEventArgs(
                    Guid.Empty,
                    envelope,
                    _pipeServerStreamWrapper.ServerStream));
            }
        }

        private void NotifyServerStopped()
        {
            Configuration.Logger.LogInformation($"Pipe server manually stopped. Pipe Id: {Id}");
            ServerStateChanged?.Invoke(this, PipeServerStateChangedEventArgs.SetPipeServerStopped(this));
        }

        private void NotifyServerStarted()
        {
            Configuration.Logger.LogInformation($"Pipe server manually started. Pipe Id: {Id}");
            ServerStateChanged?.Invoke(this, PipeServerStateChangedEventArgs.SetPipeServerStarted(this));
        }

        public PipeEnvelope<TTargetType> ProcessReceivedMessage<TTargetType>(int bytesRead, byte[] buffer)
        {
            if (bytesRead > 0)
            {
                return IngestMessage<TTargetType>(bytesRead, buffer);
            }

            Configuration.Logger.LogInformation("No bytes read from pipe. Exiting message handling.");
            return null;
        }

        private PipeEnvelope<TTargetType> IngestMessage<TTargetType>(int bytesRead, byte[] buffer)
        {
            try
            {
                var messageBytes = new byte[bytesRead];
                Array.Copy(buffer, 0, messageBytes, 0, bytesRead);

                var decryptedMessage = Configuration.EncryptionService.Decrypt(messageBytes);
                var json = Encoding.UTF8.GetString(decryptedMessage);
                var deserialized =
                    PipeEnvelope<TTargetType>.Deserialize<TTargetType>(json);

                Configuration.Logger.LogInformation($"Received message on pipe. Message Id: {deserialized.MessageId}");
                MessageReceived?.Invoke(this, new PipeMessageReceivedEventArgs(json, deserialized));
                return deserialized;
            }
            catch (Exception ex)
            {
                Configuration.Logger.LogError(ex, "Failed to deserialize message from pipe");
            }

            return new PipeEnvelope<TTargetType>
            {
                MessageId = Guid.Empty,
                Payload = default,
                TypeName = typeof(TTargetType).FullName,
                Timestamp = DateTime.MinValue
            };
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            Configuration.Reset();
            Id = Guid.NewGuid();
            _pipeServerStreamWrapper?.Dispose();
            Configuration.Logger.LogInformation(
                $"Server disposed [{Configuration.ServerName} -> {Configuration.PipeName}]");
        }

        public async Task SendResponseAsync(Guid messageId)
        {
            if (_pipeServerStreamWrapper == null || !_pipeServerStreamWrapper.IsConnected)
            {
                Configuration.Logger.LogWarning("Pipe server stream is not connected. Cannot send response.");
                return;
            }

            var envelope = TriggerServerSideResponseEvent(messageId);

            try
            {
                var json = envelope.ToJson();
                var bytes = Encoding.UTF8.GetBytes(json);

                if (Configuration.UseEncryption)
                {
                    bytes = Configuration.EncryptionService.Encrypt(bytes);
                }

                Configuration.Logger.LogInformation("Responding to client on pipe {pipeName}", Configuration.PipeName);
                await _pipeServerStreamWrapper.WriteAsync(bytes, 0, bytes.Length,
                    Configuration.CancellationTokenSource.Token);
                await _pipeServerStreamWrapper.FlushAsync(Configuration.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Configuration.Logger.LogError(ex, "Failed to send response on pipe {pipeName}", Configuration.PipeName);
            }
        }

        private PipeEnvelope<dynamic> TriggerServerSideResponseEvent(Guid messageId)
        {
            var envelope = new PipeEnvelope<dynamic>
            {
                Payload = $"Received message on pipe. Message Id: {messageId}"
            };

            ResponseRequested?
                .Invoke(this,
                    new PipeResponseRequestedEventArgs(
                        messageId,
                        envelope,
                        _pipeServerStreamWrapper.ServerStream));
            return envelope;
        }
    }
}
