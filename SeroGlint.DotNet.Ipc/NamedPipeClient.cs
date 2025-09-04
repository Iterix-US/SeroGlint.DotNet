using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc.Interfaces;
using SeroGlint.DotNet.Ipc.Wrappers;

namespace SeroGlint.DotNet.Ipc
{
    /// <summary>
    /// A client for sending messages through a named pipe to a server.
    /// </summary>
    public class NamedPipeClient : INamedPipeClient
    {
        private readonly ILogger _logger;
        private IPipeClientStreamWrapper _pipeClientStreamWrapper;

        public INamedPipeConfiguration Configuration { get; }

        public string PipeName => Configuration.PipeName;
        public bool IsConnected => _pipeClientStreamWrapper?.IsConnected ?? false;

        public NamedPipeClient()
        {
            
        }

        public NamedPipeClient(INamedPipeConfiguration configuration, ILogger logger,
            IPipeClientStreamWrapper pipeClientStreamWrapper = null)
        {
            _logger = logger;
            Configuration = configuration;
            _pipeClientStreamWrapper = pipeClientStreamWrapper;
        }

        /// <summary>
        /// Sends a message through the named pipe to the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task<string> SendAsync<T>(IPipeEnvelope<T> message)
        {
            if (!ValidateMessageSettings(Configuration.ServerName, Configuration.PipeName, message))
            {
                return "Pipe message was invalid. Check log for more details.";
            }

            var serializedEnvelope = PrepareMessage(message);
            var client = EstablishClientStream();

            var sendResult = await WriteToStream(client, serializedEnvelope);
            _logger.LogInformation(sendResult);

            var response = await ParseResponse(client);
            _logger.LogInformation("Response received from server.");

            return response;
        }

        public void Disconnect()
        {
            if (!_pipeClientStreamWrapper.IsConnected)
            {
                Configuration.Logger.LogInformation("Client is not connected");
                return;
            }

            Configuration.Logger.LogInformation("Client is disconnected");
            _pipeClientStreamWrapper.Dispose();
        }

        private async Task<string> ParseResponse(IPipeClientStreamWrapper client)
        {
            try
            {
                var response = await RetrieveResponse(client);

                _logger.LogInformation(
                    $"Client ({client.Id}) received response from server ({Configuration.ServerName}) on pipe ({Configuration.PipeName})");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while reading response from pipe {Configuration.PipeName}");
                return $"Error reading response: {ex.Message}";
            }
        }

        public async Task<string> RetrieveResponse(IPipeClientStreamWrapper client)
        {
            var buffer = new byte[4096];
            var bytesRead = await client.ReadAsync(
                buffer,
                0,
                buffer.Length,
                Configuration.CancellationTokenSource.Token);
            var responseBytes = new byte[bytesRead];

            if (bytesRead <= 0)
            {
                _logger.LogWarning($"No response received from server on pipe {Configuration.PipeName}");
                return Encoding.UTF8.GetString(responseBytes);
            }

            Array.Copy(buffer, responseBytes, bytesRead);

            if (Configuration.UseEncryption)
            {
                responseBytes = Configuration.EncryptionService.Decrypt(responseBytes);
            }

            return Encoding.UTF8.GetString(responseBytes);
        }

        private async Task<string> WriteToStream(IPipeClientStreamWrapper client, byte[] serializedEnvelope)
        {
            try
            {
                if (!client.IsConnected)
                {
                    await client.ConnectAsync(Configuration.CancellationTokenSource.Token);
                }

                _logger.LogInformation(
                    $"Connected to pipe {Configuration.PipeName} on server {Configuration.ServerName}"
                );

                await client.WriteAsync(serializedEnvelope, 0, serializedEnvelope.Length,
                    Configuration.CancellationTokenSource.Token);

                await client.FlushAsync(Configuration.CancellationTokenSource.Token);
                return "Message sent.";
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEventId.WritePipeError.GetValue(), ex, $"Error writing to pipe {Configuration.PipeName} on server {Configuration.ServerName}");
            }

            return "Could not send message from client. An error occurred.";
        }

        private byte[] PrepareMessage<T>(IPipeEnvelope<T> message)
        {
            var serializedEnvelope = message.Serialize();

            if (!Configuration.UseEncryption)
            {
                return serializedEnvelope;
            }

            _logger.LogInformation("Encrypting message before sending.");
            serializedEnvelope = Configuration.EncryptionService.Encrypt(serializedEnvelope);

            return serializedEnvelope;
        }

        private IPipeClientStreamWrapper EstablishClientStream()
        {
            if (_pipeClientStreamWrapper != null)
            {
                _logger.LogInformation(LoggingEventId.ReusingExistingPipe.GetValue(),
                    message: $"Reusing existing client stream (ID: {_pipeClientStreamWrapper.Id})");

                return _pipeClientStreamWrapper;
            }

            var clientStream = new NamedPipeClientStream(
                    Configuration.ServerName,
                    Configuration.PipeName,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous);

            _pipeClientStreamWrapper = new PipeClientStreamWrapper(clientStream);

            _logger.LogInformation($"Using client (server name: {Configuration.ServerName}) and pipe '{Configuration.PipeName}'");
            return _pipeClientStreamWrapper;
        }

        public bool ValidateMessageSettings<T>(string serverName, string pipeName, IPipeEnvelope<T> messageContent)
        {
            var stringBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(serverName))
            {
                const string errorMessage = "Server name cannot be null or whitespace.";
                stringBuilder.AppendLine(errorMessage);
            }

            if (string.IsNullOrWhiteSpace(pipeName))
            {
                const string errorMessage = "Pipe name cannot be null or whitespace.";
                stringBuilder.AppendLine(errorMessage);
            }

            if (messageContent == null || messageContent.Payload == null || string.IsNullOrWhiteSpace(messageContent.Payload.ToJson()))
            {
                const string errorMessage = "Message content cannot be null or whitespace.";
                stringBuilder.AppendLine(errorMessage);
            }

            var errors = stringBuilder.ToString();
            if (errors.IsNullOrWhitespace())
            {
                return true;
            }

            _logger.LogError($"Validation failed: {errors}");
            return false;
        }
    }
}
