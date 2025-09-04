using SeroGlint.DotNet.Common.Extensions;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class DecimalExtensionsTests
    {
        [Fact]
        public void IsPositive_ShouldReturnTrue_WhenValueIsPositive()
        {
            // Arrange
            var value = 10.5m;

            // Act
            var result = value.IsPositive();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsNegative_ShouldReturnTrue_WhenValueIsNegative()
        {
            // Arrange
            var value = -10.5m;

            // Act
            var result = value.IsNegative();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsZero_ShouldReturnTrue_WhenValueIsZero()
        {
            // Arrange
            var value = 0m;

            // Act
            var result = value.IsZero();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsBetween_ShouldReturnTrue_WhenValueIsBetweenMinAndMax()
        {
            // Arrange
            var value = 5m;
            var minValue = 1m;
            var maxValue = 10m;

            // Act
            var result = value.IsBetween(minValue, maxValue);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsMultipleOf_ShouldReturnTrue_WhenValueIsMultipleOfGivenNumber()
        {
            // Arrange
            var value = 10m;
            var multiple = 2m;

            // Act
            var result = value.IsMultipleOf(multiple);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsEven_ShouldReturnTrue_WhenValueIsEven()
        {
            // Arrange
            var value = 4m;

            // Act
            var result = value.IsEven();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsOdd_ShouldReturnTrue_WhenValueIsOdd()
        {
            // Arrange
            var value = 3m;

            // Act
            var result = value.IsOdd();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void IsInteger_ShouldReturnTrue_WhenValueIsInteger()
        {
            // Arrange
            var value = 5m;

            // Act
            var result = value.IsInteger();

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public void ToIntegerCeiling_ShouldReturnCeilingValue()
        {
            // Arrange
            var value = 5.3m;

            // Act
            var result = value.ToIntegerCeiling();

            // Assert
            result.ShouldBe(6);
        }

        [Fact]
        public void ToIntegerFloor_ShouldReturnFloorValue()
        {
            // Arrange
            var value = 5.7m;

            // Act
            var result = value.ToIntegerFloor();

            // Assert
            result.ShouldBe(5);
        }

        [Fact]
        public void ToIntegerRound_ShouldReturnRoundedValue()
        {
            // Arrange
            var value = 5.5m;

            // Act
            var result = value.ToIntegerRound();

            // Assert
            result.ShouldBe(6);
        }

        [Fact]
        public void ToIntegerTruncate_ShouldReturnTruncatedValue()
        {
            // Arrange
            var value = 5.9m;

            // Act
            var result = value.ToIntegerTruncate();

            // Assert
            result.ShouldBe(5);
        }

        [Fact]
        public void ToDecimalString_ShouldReturnStringWithSpecifiedDecimalPoints()
        {
            // Arrange
            var value = 5.12345m;
            var decimalPoints = 2;

            // Act
            var result = value.ToDecimalString(decimalPoints);

            // Assert
            result.ShouldBe("5.12");
        }
    }
}
