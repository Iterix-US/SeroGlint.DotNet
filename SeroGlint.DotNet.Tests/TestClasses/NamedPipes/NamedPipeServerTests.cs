using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc;
using SeroGlint.DotNet.Ipc.Interfaces;
using SeroGlint.DotNet.Ipc.Objects;
using SeroGlint.DotNet.Security.Interfaces;
using SeroGlint.DotNet.Tests.TestObjects;
using Shouldly;
using Xunit.Abstractions;

namespace SeroGlint.DotNet.Tests.TestClasses.NamedPipes
{
    public class NamedPipeServerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public NamedPipeServerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task NamedPipeServer_WhenMessageIsReceived_ThenTriggersMessageReceivedEvent()
        {
            var logger = Substitute.For<ILogger>();
            var encryptionService = Substitute.For<IEncryptionService>();
            var pipeName = "TestPipe_" + Guid.NewGuid().ToString("N");

            var config = new PipeServerConfiguration();
            config.Initialize(".", pipeName, logger, encryptionService, new CancellationTokenSource());

            var pipeServer = new NamedPipeServer(config);
            var messageReceived = false;

            pipeServer.MessageReceived += (_, _) =>
            {
                _testOutputHelper.WriteLine("MessageReceived triggered");
                logger.LogInformation("MessageReceived triggered");
                messageReceived = true;
            };

            var envelope = BuildTestPayload();
            var decryptedBytes = DecryptMessage(envelope, encryptionService);

            var serverTask = Task.Run(pipeServer.StartAsync);
            await Task.Delay(200);
            _testOutputHelper.WriteLine("Server started, waiting for client to connect...");

            await SimulateClient(pipeName, decryptedBytes);
            await CleanupServer(config, serverTask);

            _testOutputHelper.WriteLine("Asserting that message was received...");
            Assert.True(messageReceived);
        }

        [Fact]
        public async Task NamedPipeServer_WhenInitializationFails_ThenLogsGeneralException()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var encryptionService = Substitute.For<IEncryptionService>();

            var config = new PipeServerConfiguration();
            config.Initialize(".", null, logger, encryptionService, new CancellationTokenSource(5000));

            var pipeServer = new NamedPipeServer(config);

            // Act
            var captured = string.Empty;
            logger
                .When(x => x.Log(
                    LogLevel.Trace,
                    Arg.Any<EventId>(),
                    Arg.Do<object>(state => captured = state.ToString()),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            await pipeServer.StartAsync();

            // Assert
            captured.ShouldContain("Error occurred");
        }

        [Fact]
        public async Task NamedPipeServer_WhenMessageDeserializationFails_ThenLogsAndContinuesGracefully()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var encryptionService = Substitute.For<IEncryptionService>();
            var pipeName = "TestPipe_" + Guid.NewGuid().ToString("N");

            var config = new PipeServerConfiguration();
            config.Initialize(".", pipeName, logger, encryptionService, new CancellationTokenSource(5000));

            var completion = new TaskCompletionSource();
            var captured = string.Empty;

            logger.When(x => x.Log(
                LogLevel.Trace,
                Arg.Any<EventId>(),
                Arg.Do<object>(state =>
                {
                    captured = state.ToString();
                    completion.TrySetResult();
                }),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()!)).Do(_ => { });

            var pipeServer = new NamedPipeServer(config);

            // Start server in background so it remains running while client sends
            var serverTask = Task.Run(pipeServer.StartAsync);

            await Task.Delay(200); // Let server spin up

            await using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                await client.ConnectAsync(config.CancellationTokenSource.Token);
                var bogusBytes = "{ \"not\": \"valid\" }"u8.ToArray();
                await client.WriteAsync(bogusBytes, 0, bogusBytes.Length);
                await client.FlushAsync();
            }

            // Wait for the error to be logged or timeout
            var timeout = Task.Delay(3000);
            _ = await Task.WhenAny(completion.Task, timeout);

            // Cancel server and await shutdown
            await config.CancellationTokenSource.CancelAsync();
            await serverTask;

            // Assert
            captured.ShouldNotBeNullOrWhiteSpace();
            captured.ShouldContain("Error");
        }

        [Fact]
        public async Task NamedPipeServer_WhenValidMessageReceived_ThenExitsNormally()
        {
            var logger = Substitute.For<ILogger>();
            var encryptionService = Substitute.For<IEncryptionService>();
            var pipeName = "TestPipe_" + Guid.NewGuid().ToString("N");

            var config = new PipeServerConfiguration();
            config.Initialize(".", pipeName, logger, encryptionService, new CancellationTokenSource(2000));

            var pipeServer = new NamedPipeServer(config);

            var messageReceived = false;
            pipeServer.MessageReceived += (_, _) => messageReceived = true;

            var serverTask = pipeServer.StartAsync();
            await Task.Delay(200);

            await using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                await client.ConnectAsync(config.CancellationTokenSource.Token);

                var payload = new SerializationObject { Id = 42, Name = "Valid" };
                var envelope = new PipeEnvelope<SerializationObject>
                {
                    Payload = payload
                };

                var json = envelope.ToJson();
                var bytes = Encoding.UTF8.GetBytes(json);
                encryptionService.Decrypt(Arg.Any<byte[]>()).Returns(bytes);

                await client.WriteAsync(bytes);
                await client.FlushAsync();
            }

            await Task.Delay(500);
            await config.CancellationTokenSource.CancelAsync();
            await serverTask;

            Assert.True(messageReceived);
        }

        [Fact]
        public async Task NamedPipeServer_WhenSendResponseAsyncGivenNullStream_ThenReturnsWithoutWriting()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var config = new PipeServerConfiguration
            {
                Logger = logger
            };
            var envelope = new PipeEnvelope<dynamic> { Payload = "No-op" };

            // Act
            var namedPipeServer = new NamedPipeServer(config);
            var exception = await Record.ExceptionAsync(() =>
                namedPipeServer.SendResponseAsync(envelope.MessageId));

            // Assert
            exception.ShouldBeNull();
        }

        [Fact]
        public async Task NamedPipeServer_WhenSendResponseAsyncGivenDisconnectedStream_ThenReturnsWithoutWriting()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var config = new PipeServerConfiguration
            {
                Logger = logger
            };

            var namedPipeServer = new NamedPipeServer(config);
            var stream = Substitute.For<IPipeServerStreamWrapper>();
            stream.IsConnected.Returns(false);

            var envelope = new PipeEnvelope<dynamic> { Payload = "Ignored" };

            // Act
            var exception = await Record.ExceptionAsync(() =>
                namedPipeServer.SendResponseAsync(envelope.MessageId));

            // Assert
            exception.ShouldBeNull();
        }

        [Fact]
        public void NamedPipeServer_WhenHandleMessageGivenZeroBytesRead_ThenLogsAndExitsEarly()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var encryptionService = Substitute.For<IEncryptionService>();
            var pipeName = $"TestPipe_{Guid.NewGuid():N}";

            var config = new PipeServerConfiguration();
            config.Initialize(".", pipeName, logger, encryptionService, new CancellationTokenSource());

            var buffer = new byte[4096];
            var pipeStream = Substitute.For<IPipeServerStreamWrapper>();
            var server = new NamedPipeServer(config, pipeStream);

            var captured = string.Empty;
            logger
                .When(x => x.Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Do<object>(state => captured = state.ToString()),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            // Act
            server.ProcessReceivedMessage<dynamic>(0, buffer);

            // Assert
            captured.ShouldContain("No bytes read from pipe");
        }

        [Fact]
        public async Task NamedPipeServer_WhenStoppingServerInstance_ThenServerStateShouldChange()
        {
            // Arrange
            var config = new PipeServerConfiguration();
            config.Initialize(
                ".",
                "TestPipe",
                Substitute.For<ILogger>(),
                Substitute.For<IEncryptionService>(),
                new CancellationTokenSource(3000));

            var wrapper = Substitute.For<IPipeServerStreamWrapper>();
            wrapper.IsConnected.Returns(true);
            wrapper.IsListening.Returns(true);

            var pipeServer = new NamedPipeServer(config, wrapper);

            // Act
            await pipeServer.StartAsync();

            // Asserts contained here.
            pipeServer.ServerStateChanged += async (e, s) =>
            {
                e.ShouldNotBeNull();
                s.ContextLabel.ShouldBe("Stopped");
            };

            pipeServer.Stop();
        }

        [Fact]
        public async Task NamedPipeServer_WhenStoppingNonRunningServerInstance_ThenServerStateShouldChange()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var config = new PipeServerConfiguration();
            config.Initialize(
                ".",
                "TestPipe",
                logger,
                Substitute.For<IEncryptionService>(),
                new CancellationTokenSource(3000));

            var wrapper = Substitute.For<IPipeServerStreamWrapper>();
            wrapper.IsConnected.Returns(false);
            wrapper.IsListening.Returns(false);

            var pipeServer = new NamedPipeServer(config, wrapper);

            var captured = string.Empty;
            logger
                .When(x => x.Log(
                    LogLevel.Information,
                    LoggingEventId.ServerNotRunning.GetValue(),
                    Arg.Do<object>(state => captured = state.ToString()),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            // Act
            pipeServer.Stop();

            // Assert
            captured.ShouldContain("not running");
        }

        [Fact]
        public async Task NamedPipeServer_WhenInitializingServerInstance_ThenServerStateShouldChange()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var config = new PipeServerConfiguration();
            config.Initialize(
                ".",
                "TestPipe",
                logger,
                Substitute.For<IEncryptionService>(),
                new CancellationTokenSource(3000));

            var wrapper = Substitute.For<IPipeServerStreamWrapper>();
            wrapper.IsConnected.Returns(false);
            wrapper.IsListening.Returns(false);

            var pipeServer = new NamedPipeServer(config, wrapper);

            var captured = string.Empty;
            logger
                .When(x => x.Log(
                    LogLevel.Information,
                    LoggingEventId.DisposingExistingPipe.GetValue(),
                    Arg.Do<object>(state => captured = state.ToString()),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            // Act
            await pipeServer.StartAsync();

            // Assert
            captured.ShouldContain("Disposing existing");
        }

        private async Task CleanupServer(PipeServerConfiguration config, Task serverTask)
        {
            _testOutputHelper.WriteLine("Client disconnected, waiting for server to process message...");
            await Task.Delay(500);

            await config.CancellationTokenSource.CancelAsync();

            try
            {
                await serverTask;
            }
            catch (OperationCanceledException ex)
            {
                _testOutputHelper.WriteLine("Server canceled as expected: " + ex.Message);
            }

            _testOutputHelper.WriteLine("Server task completed.");
        }

        private byte[] DecryptMessage(PipeEnvelope<SerializationObject> envelope, IEncryptionService encryptionService)
        {
            var json = envelope.ToJson();
            _testOutputHelper.WriteLine($"Serialized JSON: {json}");
            var decryptedBytes = Encoding.UTF8.GetBytes(json);
            _testOutputHelper.WriteLine($"Decrypted Bytes: {BitConverter.ToString(decryptedBytes)}");
            encryptionService.Decrypt(Arg.Any<byte[]>()).Returns(decryptedBytes);
            return decryptedBytes;
        }

        private static PipeEnvelope<SerializationObject> BuildTestPayload()
        {
            var serializationObject = new SerializationObject
            {
                Id = 123456,
                Name = "TestObject",
            };

            return new PipeEnvelope<SerializationObject>
            {
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                TypeName = typeof(SerializationObject).FullName,
                Payload = serializationObject
            };
        }

        private async Task SimulateClient(string pipeName, byte[] decryptedBytes)
        {
            await using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                _testOutputHelper.WriteLine("Connecting client to server...");
                await client.ConnectAsync(2000);
                Assert.True(client.IsConnected, "Client failed to connect to server.");

                _testOutputHelper.WriteLine("Client connected, sending message...");
                await client.WriteAsync(decryptedBytes, 0, decryptedBytes.Length);
                await client.FlushAsync();
                _testOutputHelper.WriteLine("Message sent from client to server.");
            }
        }
    }
}