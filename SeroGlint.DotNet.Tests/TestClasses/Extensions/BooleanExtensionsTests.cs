using SeroGlint.DotNet.Common.Extensions;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class BooleanExtensionsTests
    {
        [Fact]
        public void ToInteger_ShouldReturnOneForTrue()
        {
            // Arrange
            var boolean = true;

            // Act
            var result = boolean.ToInteger();

            // Assert
            result.ShouldBe(1);
        }

        [Fact]
        public void ToInteger_ShouldReturnZeroForFalse()
        {
            // Arrange
            var boolean = false;

            // Act
            var result = boolean.ToInteger();

            // Assert
            result.ShouldBe(0);
        }

        [Fact]
        public void ToYesNo_ShouldReturnYesForTrue()
        {
            // Arrange
            var boolean = true;

            // Act
            var result = boolean.ToYesNo();

            // Assert
            result.ShouldBe("Yes");
        }

        [Fact]
        public void ToYesNo_ShouldReturnNoForFalse()
        {
            // Arrange
            var boolean = false;

            // Act
            var result = boolean.ToYesNo();

            // Assert
            result.ShouldBe("No");
        }

        [Fact]
        public void ToOneZero_ShouldReturnOneForTrue()
        {
            // Arrange
            var boolean = true;

            // Act
            var result = boolean.ToOneZero();

            // Assert
            result.ShouldBe("1");
        }

        [Fact]
        public void ToOneZero_ShouldReturnZeroForFalse()
        {
            // Arrange
            var boolean = false;

            // Act
            var result = boolean.ToOneZero();

            // Assert
            result.ShouldBe("0");
        }

        [Fact]
        public void ToOnOff_ShouldReturnOnForTrue()
        {
            // Arrange
            var boolean = true;

            // Act
            var result = boolean.ToOnOff();

            // Assert
            result.ShouldBe("On");
        }

        [Fact]
        public void ToOnOff_ShouldReturnOffForFalse()
        {
            // Arrange
            var boolean = false;

            // Act
            var result = boolean.ToOnOff();

            // Assert
            result.ShouldBe("Off");
        }

        [Fact]
        public void ToEnabledDisabled_ShouldReturnEnabledForTrue()
        {
            // Arrange
            var boolean = true;

            // Act
            var result = boolean.ToEnabledDisabled();

            // Assert
            result.ShouldBe("Enabled");
        }

        [Fact]
        public void ToEnabledDisabled_ShouldReturnDisabledForFalse()
        {
            // Arrange
            var boolean = false;

            // Act
            var result = boolean.ToEnabledDisabled();

            // Assert
            result.ShouldBe("Disabled");
        }

        [Fact]
        public void ToActiveInactive_ShouldReturnActiveForTrue()
        {
            // Arrange
            var boolean = true;

            // Act
            var result = boolean.ToActiveInactive();

            // Assert
            result.ShouldBe("Active");
        }

        [Fact]
        public void ToActiveInactive_ShouldReturnInactiveForFalse()
        {
            // Arrange
            var boolean = false;

            // Act
            var result = boolean.ToActiveInactive();

            // Assert
            result.ShouldBe("Inactive");
        }
    }
}
