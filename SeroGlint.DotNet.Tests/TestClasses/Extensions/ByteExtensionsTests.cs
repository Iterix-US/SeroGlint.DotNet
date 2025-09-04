using System.Diagnostics.CodeAnalysis;
using SeroGlint.DotNet.Common.Extensions;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class ByteExtensionsTests
    {
        [Fact]
        public void ToHexString_ShouldReturnCorrectHexRepresentation()
        {
            // Arrange
            byte[] input = [0xDE, 0xAD, 0xBE, 0xEF];

            // Act
            var result = input.ToHexString();

            // Assert
            result.ShouldBe("deadbeef");
        }

        [Fact]
        public void ToHexString_ShouldHandleEmptyArray()
        {
            // Arrange
            byte[] input = [];

            // Act
            var result = input.ToHexString();

            // Assert
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public void ToHexString_ShouldProduceLowercaseOutput()
        {
            // Arrange
            byte[] input = [0xAB, 0xCD, 0xEF];

            // Act
            var result = input.ToHexString();

            // Assert
            result.ShouldBe("abcdef");
            result.ShouldBe(result.ToLowerInvariant()); // Explicitly ensure lowercase
        }

        [Fact]
        public void ToHexString_ShouldHandleSingleByte()
        {
            // Arrange
            byte[] input = [0x0A];

            // Act
            var result = input.ToHexString();

            // Assert
            result.ShouldBe("0a");
        }

        [Fact]
        public void ToHexString_ShouldHandleLeadingZeros()
        {
            // Arrange
            byte[] input = [0x00, 0x0F];

            // Act
            var result = input.ToHexString();

            // Assert
            result.ShouldBe("000f");
        }

        [Fact]
        [ExcludeFromCodeCoverage]
        public void ToHexString_ShouldThrowArgumentNullException_WhenNullInput()
        {
            // Arrange
            byte[] input = null;

            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
            {
                var _ = input.ToHexString();
            });
        }
    }
}
