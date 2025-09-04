using System.Text;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc;
using SeroGlint.DotNet.Ipc.Interfaces;
using SeroGlint.DotNet.Ipc.Objects;
using SeroGlint.DotNet.Security;
using SeroGlint.DotNet.Security.Interfaces;
using SeroGlint.DotNet.Tests.TestClasses.NamedPipes.TestObjects;
using SeroGlint.DotNet.Tests.TestObjects;
using SeroGlint.DotNet.Tests.Utilities;
using Shouldly;
using static NSubstitute.Arg;
#pragma warning disable CA1416

namespace SeroGlint.DotNet.Tests.TestClasses.NamedPipes
{
    public class NamedPipeClientTests
    {
        private readonly ILogger _logger = Substitute.For<ILogger>();

        [Fact]
        public async Task NamedPipeClient_WhenServerNameIsNull_ThenLogsValidationError()
        {
            // Arrange
            var config = Substitute.For<INamedPipeConfiguration>();
            config.ServerName.Returns((string)null!);
            config.PipeName.Returns("PipeName");
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());

            var capturedMessage = string.Empty;

            _logger
                .When(x => x.Log(
                    LogLevel.Error,
                    Any<EventId>(),
                    Do<object>(state => capturedMessage = state.ToString()),
                    Any<Exception>(),
                    Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            var envelope = new PipeEnvelope<SerializationObject>(_logger)
            {
                Payload = new SerializationObject
                {
                    Id = 1,
                    Name = "Test Object"
                }
            };

            var client = new NamedPipeClient(config, _logger);

            // Act
            await client.SendAsync(envelope);

            // Assert
            Assert.Contains("Server name cannot be null or whitespace", capturedMessage);
        }

        [Fact]
        public async Task NamedPipeClient_WhenPipeNameIsEmpty_ThenLogsValidationError()
        {
            // Arrange
            var config = Substitute.For<INamedPipeConfiguration>();
            config.ServerName.Returns("ValidServer");
            config.PipeName.Returns(""); // Invalid pipe name
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());

            var capturedMessage = string.Empty;
            _logger
                .When(x => x.Log(
                    LogLevel.Error,
                    Any<EventId>(),
                    Do<object>(state => capturedMessage = state.ToString()),
                    Any<Exception>(),
                    Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            var envelope = new PipeEnvelope<SerializationObject>(_logger)
            {
                Payload = new SerializationObject { Id = 1, Name = "Test" }
            };

            var client = new NamedPipeClient(config, _logger);

            // Act
            await client.SendAsync(envelope);

            // Assert
            capturedMessage.ShouldContain("Pipe name cannot be null or whitespace");
        }

        [Fact]
        public async Task NamedPipeClient_WhenWriteFails_ThenReturnsWrappedErrorResponse()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var config = Substitute.For<INamedPipeConfiguration>();
            var envelope = Substitute.For<IPipeEnvelope<SerializationObject>>();

            var payload = new SerializationObject { Id = 1, Name = "Failure" };
            envelope.Payload.Returns(payload);
            envelope.Serialize().Returns(new byte[] { 1, 2, 3, 4 });

            config.ServerName.Returns("TestServer");
            config.PipeName.Returns("TestPipe");
            config.CancellationTokenSource.Returns(new CancellationTokenSource(1000));
            config.UseEncryption.Returns(false);
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());

            var client = new NamedPipeClient(config, logger);

            // Act & Assert
            var response = await client.SendAsync(envelope);
            response.ShouldNotContain("Message sent");
            response.ShouldContain("Error");
        }


        [Fact]
        public async Task NamedPipeClient_WhenSendingUnencryptedMessage_ThenSerializesAndLogsResponse()
        {
            // Arrange
            var config = Substitute.For<INamedPipeConfiguration>();
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));

            var envelope = new PipeEnvelope<SerializationObject>(_logger)
            {
                Payload = new SerializationObject
                {
                    Id = 1,
                    Name = "Test Object"
                }
            };

            config.ServerName.Returns(".");
            config.PipeName.Returns("Pipe");
            config.UseEncryption.Returns(false);
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());

            var capturedMessage = string.Empty;
            _logger
                .When(x => x.Log(
                    LogLevel.Information,
                    Any<EventId>(),
                    Do<object>(state => capturedMessage = state.ToString()),
                    Any<Exception>(),
                    Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            var serverTask = TestNamedPipeServer.StartTestServerAsync(config.PipeName, config.CancellationTokenSource.Token);
            var client = new NamedPipeClient(config, _logger);

            // Act & Assert
            await client.SendAsync(envelope);
            capturedMessage.ShouldContain("received");
            capturedMessage.ShouldContain("response");
            await serverTask;
        }

        [Fact]
        public async Task NamedPipeClient_WhenEncryptionIsEnabled_ThenEncryptsMessageAndLogs()
        {
            var config = Substitute.For<INamedPipeConfiguration>();
            var encryption = Substitute.For<IEncryptionService>();
            encryption.Encrypt(Any<byte[]>()).Returns(call => call.Arg<byte[]>());

            var envelope = new PipeEnvelope<SerializationObject>(_logger)
            {
                Payload = new SerializationObject
                {
                    Id = 1,
                    Name = "Test Object"
                }
            };

            config.ServerName.Returns(".");
            config.PipeName.Returns("Pipe_" + Guid.NewGuid().ToString("N"));
            config.UseEncryption.Returns(true);
            config.EncryptionService.Returns(encryption);
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));

            var serverTask = TestNamedPipeServer.StartTestServerAsync(config.PipeName, config.CancellationTokenSource.Token);

            await Task.Delay(100); // Let server start

            var client = new NamedPipeClient(config, _logger);
            await client.SendAsync(envelope);

            encryption.Received().Encrypt(Any<byte[]>());
            _logger.Received().LogInformation("Encrypting message before sending.");

            await serverTask;
        }

        [Fact]
        public async Task NamedPipeClient_WhenInvalidServerInfoProvided_ThenFailsValidationAndLogsAllErrors()
        {
            // Arrange
            var capturedMessage = string.Empty;

            var config = Substitute.For<INamedPipeConfiguration>();
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));

            var envelope = new PipeEnvelope<SerializationObject>(_logger);

            config.ServerName.Returns("");
            config.PipeName.Returns("");
            config.UseEncryption.Returns(false);
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());

            _logger
                .When(x => x.Log(
                    LogLevel.Error,
                    Any<EventId>(),
                    Do<object>(state => capturedMessage = state.ToString()),
                    Any<Exception>(),
                    Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            var client = new NamedPipeClient(config, _logger);

            // Act
            await client.SendAsync(envelope);

            // Assert
            Assert.Contains("Server name cannot be null or whitespace", capturedMessage);
            Assert.Contains("Pipe name cannot be null or whitespace", capturedMessage);
            Assert.Contains("Message content cannot be null or whitespace", capturedMessage);
        }

        [Fact]
        public async Task NamedPipeClient_WhenServerResponseIsEncrypted_ThenClientDecryptsMessageSuccessfully()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var clientStreamWrapper = Substitute.For<IPipeClientStreamWrapper>();

            var key = AesEncryptionService.GenerateKey();
            var encServ = new AesEncryptionService(key, logger);
            var config = new PipeServerConfiguration();

            var plaintext = "This is a decrypted message.";
            var translatedOriginalBytes = Encoding.UTF8.GetBytes(plaintext);
            var encryptedBytes = encServ.Encrypt(translatedOriginalBytes);

            config.Initialize(".", "EncryptedPipe", logger, encServ, new CancellationTokenSource(3000));
            config.UseEncryption = true;

            clientStreamWrapper.ReadAsync(
                    Any<byte[]>(),
                    Any<int>(),
                    Any<int>(),
                    Any<CancellationToken>())
                .Returns(call =>
                {
                    var buffer = call.Arg<byte[]>();
                    Buffer.BlockCopy(encryptedBytes, 0, buffer, 0, encryptedBytes.Length);
                    return Task.FromResult(encryptedBytes.Length);
                });

            var client = new TestableNamedPipeClient(config, logger, clientStreamWrapper);

            // Act
            var result = await client.RetrieveResponse(clientStreamWrapper);

            // Assert
            result.ShouldBe(Encoding.UTF8.GetString(translatedOriginalBytes));
        }

        [Fact]
        public void NamedPipeClient_WhenDisconnecting_ThenDisposeStreamWrapper()
        {
            // Arrange
            var wrapper = Substitute.For<IPipeClientStreamWrapper>();
            wrapper.IsConnected.Returns(true);
            var config = Substitute.For<INamedPipeConfiguration>();
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));
            config.ServerName.Returns("TestServer");
            config.PipeName.Returns("TestPipe");
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());
            var client = new NamedPipeClient(config, _logger, wrapper);

            // Act
            client.Disconnect();

            // Assert
            wrapper.Received(1).Dispose();
        }

        [Fact]
        public void NamedPipeClient_WhenNotConnected_ThenDoesNotDisposeStreamWrapper()
        {
            // Arrange
            var wrapper = Substitute.For<IPipeClientStreamWrapper>();
            wrapper.IsConnected.Returns(false);
            var config = Substitute.For<INamedPipeConfiguration>();
            config.CancellationTokenSource.Returns(new CancellationTokenSource(3000));
            config.ServerName.Returns("TestServer");
            config.PipeName.Returns("TestPipe");
            config.EncryptionService.Returns(Substitute.For<IEncryptionService>());
            var client = new NamedPipeClient(config, _logger, wrapper);

            // Act
            client.Disconnect();

            // Assert
            wrapper.DidNotReceive().Dispose();
        }

        [Fact]
        public async Task NamedPipeClient_WhenEstablishingClientStreamWithNotNullWrapper_ThenUseExistingWrapper()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var wrapper = Substitute.For<IPipeClientStreamWrapper>();
            wrapper.Id.Returns(guid);
            wrapper.IsConnected.Returns(false);

            var config = new PipeServerConfiguration();
            config.Initialize(
                "TestServer", 
                "TestPipe", 
                _logger, 
                new AesEncryptionService(AesEncryptionService.GenerateKey(), _logger), 
                new CancellationTokenSource(3000));
            
            var server = Substitute.For<INamedPipeServer>();
            server.Configuration.Returns(config);
            server.IsListening.Returns(true);
            server.StartAsync().Returns(Task.CompletedTask);

            var client = new NamedPipeClient(config, _logger, wrapper);

            var capturedMessage = string.Empty;
            _logger
                .When(x => x.Log(
                    LogLevel.Information,
                    LoggingEventId.ReusingExistingPipe.GetValue(),
                    Do<object>(state => capturedMessage = state.ToString()),
                    Any<Exception>(),
                    Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            // Act
            await client.SendAsync(new PipeEnvelope<TestObject>(_logger)
            {
                Payload = new TestObject()
            });

            // Assert
            capturedMessage.ShouldContain("Reusing existing client");
            capturedMessage.ShouldContain(guid.ToString());
        }

        [Fact]
        public async Task SendThroughPipe_WhenWriteAsyncThrows_Returns_ErrorMessage()
        {
            // Arrange
            var cfg = Substitute.For<INamedPipeConfiguration>();
            cfg.PipeName.Returns("mypipe");
            cfg.ServerName.Returns("myserver");
            cfg.CancellationTokenSource.Returns(new CancellationTokenSource(3000));

            var logger = Substitute.For<ILogger>();
            var wrapper = Substitute.For<IPipeClientStreamWrapper>();
            wrapper.IsConnected.Returns(true);
            wrapper
                .When(w => w.WriteAsync(
                    Any<byte[]>(), Any<int>(), Any<int>(), Any<CancellationToken>()))
                .Do(_ => throw new IOException("Fail the send for testing purposes"));

            logger
                .When(x => x.Log(
                    LogLevel.Error,
                    Is<EventId>(e => e.Id == LoggingEventId.WritePipeError.GetValue()),
                    Any<object>(),
                    Any<Exception>(),
                    Any<Func<object, Exception, string>>()!))
                .Do(ci => { });

            var client = new NamedPipeClient(cfg, logger, wrapper);
            var envelope = new PipeEnvelope<object>(_logger) { Payload = new { } };

            // Act
            await client.SendAsync(envelope);

            // Assert
            logger.Received(1).Log(
                LogLevel.Error,
                LoggingEventId.WritePipeError.GetValue(),
                Is<object>(state => state.ToString()!.Contains("Error writing to pipe mypipe")),
                Is<IOException>(ex => ex.Message == "Fail the send for testing purposes"),
                Any<Func<object, Exception, string>>()!);
        }
    }
}
