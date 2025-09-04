using SeroGlint.DotNet.Common.Extensions;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class IntegerExtensionsTests
    {
        [Fact]
        public void IsBetween_ShouldReturnTrue_WhenValueIsBetweenMinAndMax()
        {
            // Arrange
            var value = 5;
            var minValue = 1;
            var maxValue = 10;

            // Act
            var result = value.IsBetween(minValue, maxValue);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsMultipleOf_ShouldReturnTrue_WhenValueIsMultipleOfGivenNumber()
        {
            // Arrange
            var value = 10;
            var multiple = 2;

            // Act
            var result = value.IsMultipleOf(multiple);

            // Assert
            result.ShouldBeTrue();
        }
    }
}
