using System.Text;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Ipc.Objects;
using SeroGlint.DotNet.Tests.TestObjects;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.NamedPipes
{
    public class PipeEnvelopeTests
    {
        [Fact]
        public void PipeEnvelope_WhenSerialized_ThenReturnsValidJsonBytes()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var envelope = new PipeEnvelope<SerializationObject>(logger)
            {
                Payload = new SerializationObject { Id = 1, Name = "Test" }
            };
            
            // Act
            var expectedBytes = Encoding.UTF8.GetBytes(envelope.ToJson());
            var serializedEnvelope = envelope.Serialize();

            // Assert
            Assert.Equal(expectedBytes, serializedEnvelope);
        }

        [Fact]
        public void PipeEnvelope_WhenDeserializedWithValidJson_ThenReturnsCorrectObject()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var original = new PipeEnvelope<SerializationObject>
            {
                Payload = new SerializationObject { Id = 42, Name = "Demo" }
            };

            var json = original.ToJson();

            // Act
            var result = PipeEnvelope<SerializationObject>.Deserialize<SerializationObject>(json, logger);

            // Assert
            Assert.Equal(original.Payload.Id, result.Payload.Id);
            Assert.Equal(original.Payload.Name, result.Payload.Name);
            Assert.Equal(original.TypeName, result.TypeName);
        }

        [Fact]
        public void PipeEnvelope_WhenDeserializedWithInvalidJson_ThenLogsErrorAndReturnsNull()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var json = "Invalid JSON";
            var capturedMessage = string.Empty;
            logger
                .When(x => x.Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Do<object>(state => capturedMessage = state.ToString()),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<object, Exception, string>>()!))
                .Do(_ => { });

            // Act & Assert
            var response = 
                PipeEnvelope<SerializationObject>.Deserialize<SerializationObject>(json, logger);
            response.ShouldBeNull();
            capturedMessage.ShouldBeEquivalentTo("Failed to deserialize message.");
        }
    }
}
