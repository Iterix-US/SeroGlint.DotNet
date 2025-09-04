using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Tests.TestObjects;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void ToJson_ShouldSerializeObjectToJsonString()
        {
            // Arrange
            var testObject = new SerializationObject { Id = 1, Name = "Test" };
            var expectedJsonString = "{\"Id\":1,\"Name\":\"Test\"}";

            // Act
            var result = testObject.ToJson();

            // Assert
            result.ShouldBe(expectedJsonString);
        }

        [Fact]
        public void ToXml_ShouldThrowExceptionForInvalidObject()
        {
            var invalidObject = new InvalidObject();

            Should.Throw<InvalidOperationException>(() => invalidObject.ToXml())
                .Message.ShouldStartWith("Error serializing object");
        }

        [Fact]
        public void ToXml_ShouldSerializeObjectToXmlString()
        {
            // Arrange
            var testObject = new SerializationObject { Id = 1, Name = "Test" };

            var expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                              "<SerializationObject xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                              "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                              "<Id>1</Id><Name>Test</Name></SerializationObject>";

            // Act
            var result = testObject.ToXml();

            // Assert
            result.ShouldBe(expectedXml);
        }
    }
}

