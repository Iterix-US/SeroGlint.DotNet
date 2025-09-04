using System.Text;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Tests.TestObjects;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void ContainsIgnoreCase_ShouldReturnTrue_WhenStringContainsValueIgnoringCase()
        {
            // Arrange
            var baseString = "Hello World";
            var value = "hello";

            // Act
            var result = baseString.ContainsIgnoreCase(value);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void EqualsIgnoreCase_ShouldReturnTrue_WhenStringsAreEqualIgnoringCase()
        {
            // Arrange
            var baseString = "Hello";
            var value = "hello";

            // Act
            var result = baseString.EqualsIgnoreCase(value);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void StartsWithIgnoreCase_ShouldReturnTrue_WhenStringStartsWithValueIgnoringCase()
        {
            // Arrange
            var baseString = "Hello World";
            var value = "hello";

            // Act
            var result = baseString.StartsWithIgnoreCase(value);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void EndsWithIgnoreCase_ShouldReturnTrue_WhenStringEndsWithValueIgnoringCase()
        {
            // Arrange
            var baseString = "Hello World";
            var value = "WORLD";

            // Act
            var result = baseString.EndsWithIgnoreCase(value);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void ToTitleCase_ShouldConvertStringToTitleCase()
        {
            // Arrange
            var baseString = "hello world";

            // Act
            var result = baseString.ToTitleCase();

            // Assert
            result.ShouldBe("Hello World");
        }

        [Fact]
        public void ToInt32_ShouldConvertStringToInt32()
        {
            // Arrange
            var baseString = "123";

            // Act
            var success = baseString.ToInt32(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBe(123);
        }

        [Fact]
        public void ToInt64_ShouldConvertStringToInt64()
        {
            // Arrange
            var baseString = "123456789012345";

            // Act
            var success = baseString.ToInt64(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBe(123456789012345);
        }

        [Fact]
        public void ToDouble_ShouldConvertStringToDouble()
        {
            // Arrange
            var baseString = "123.45";

            // Act
            var success = baseString.ToDouble(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBe(123.45);
        }

        [Fact]
        public void ToDecimal_ShouldConvertStringToDecimal()
        {
            // Arrange
            var baseString = "123.45";

            // Act
            var success = baseString.ToDecimal(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBe(123.45m);
        }

        [Fact]
        public void ToDateTime_ShouldConvertStringToDateTime()
        {
            // Arrange
            var baseString = "2025-03-06";

            // Act
            var success = baseString.ToDateTime(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBe(new DateTime(2025, 3, 6));
        }

        [Fact]
        public void ToBoolean_ShouldConvertStringToTrueBoolean()
        {
            // Arrange
            var baseString = "true";

            // Act
            var success = baseString.ToBoolean(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBeTrue();
        }

        [Fact]
        public void ToBoolean_ShouldConvertStringToFalseBoolean()
        {
            // Arrange
            var baseString = "false";

            // Act
            var success = baseString.ToBoolean(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBeFalse();
        }

        [Fact]
        public void ToBoolean_ShouldConvertStringTo1TrueBoolean()
        {
            // Arrange
            var baseString = "1";

            // Act
            var success = baseString.ToBoolean(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBeTrue();
        }

        [Fact]
        public void ToBoolean_ShouldConvertStringTo0FalseBoolean()
        {
            // Arrange
            var baseString = "0";

            // Act
            var success = baseString.ToBoolean(out var result);

            // Assert
            success.ShouldBeTrue();
            result.ShouldBeFalse();
        }

        [Fact]
        public void FromJsonToType_ShouldDeserializeJsonStringToObject()
        {
            // Arrange
            var jsonString = "{\"Id\":1,\"Name\":\"Test\"}";
            var expectedObject = new SerializationObject { Id = 1, Name = "Test" };

            // Act
            var result = jsonString.FromJsonToType<SerializationObject>();

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(expectedObject.Id);
            result.Name.ShouldBe(expectedObject.Name);
        }

        [Fact]
        public void FromJsonToType_ShouldThrowExceptionForInvalidJson()
        {
            // Arrange
            var invalidJsonString = "{\"Id\":1,\"Name\":\"Test\"";

            // Act & Assert
            var exception = Should.Throw<Exception>(() => invalidJsonString.FromJsonToType<SerializationObject>());
            exception.Message.ShouldContain("An error occurred while deserializing the JSON string.");
        }

        [Fact]
        public void FromXmlToType_ShouldDeserializeXmlStringToObject()
        {
            // Arrange
            var xmlString = "<SerializationObject><Id>1</Id><Name>Test</Name></SerializationObject>";
            var expectedObject = new SerializationObject { Id = 1, Name = "Test" };

            // Act
            var result = xmlString.FromXmlToType<SerializationObject>();

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(expectedObject.Id);
            result.Name.ShouldBe(expectedObject.Name);
        }

        [Fact]
        public void FromXmlToType_ShouldThrowExceptionForInvalidXml()
        {
            // Arrange
            var invalidXmlString = "<SerializationObject><Id>1<Id><Name>Test</Name></SerializationObject>";

            // Act & Assert
            var exception = Should.Throw<InvalidOperationException>(() => invalidXmlString.FromXmlToType<SerializationObject>());
            exception.Message.ShouldContain($"Error deserializing XML to {typeof(SerializationObject)}.");
        }

        [Fact]
        public void IsValidEnumValue_ShouldReturnTrue_WhenValidEnumString()
        {
            var input = "ValueOne";
            var success = input.IsValidEnumValue<SampleEnum>(out var result);

            success.ShouldBeTrue();
            result.ShouldBe(SampleEnum.ValueOne);
        }

        [Fact]
        public void IsValidEnumValue_ShouldReturnFalse_WhenInvalidEnumString()
        {
            var input = "Invalid";
            var success = input.IsValidEnumValue<SampleEnum>(out _);

            success.ShouldBeFalse();
        }

        [Fact]
        public void GetEnumFromDescription_ShouldReturnEnum_WhenDescriptionMatches()
        {
            var input = "First Option";
            var result = input.GetEnumFromDescription<SampleEnum>();

            result.ShouldBe(SampleEnum.ValueOne);
        }

        [Fact]
        public void GetEnumFromDescription_ShouldReturnNull_WhenNoMatch()
        {
            var input = "Nonexistent";
            var result = input.GetEnumFromDescription<SampleEnum>();

            result.ShouldBeNull();
        }

        [Fact]
        public void EncodeAsHttp_ShouldUrlEncodeString()
        {
            var input = "this is a test!";
            var result = input.EncodeAsHttp();

            result.ShouldBe("this%20is%20a%20test%21");
        }

        [Fact]
        public void ToSha256_ShouldReturnExpectedHash()
        {
            var input = "test";
            var result = input.ToSha256();

            result.ShouldBe("n4bQgYhMfWWaL+qgxVrQFaO/TxsrC4Is0V1sFbDwCgg=");
        }

        [Fact]
        public void ToHexString_ShouldConvertStringToHex()
        {
            var input = "abc";
            var result = input.ToHexString();

            result.ShouldBe("616263");
        }

        [Fact]
        public void ToHexString_ShouldRespectEncodingParameter()
        {
            var input = "abc";
            var result = input.ToHexString(Encoding.ASCII);

            result.ShouldBe("616263");
        }
    }
}
