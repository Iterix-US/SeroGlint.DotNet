using System.Diagnostics.CodeAnalysis;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    [ExcludeFromCodeCoverage]
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void Minus_WithYearsMonthsDays_ShouldReturnCorrectDateTime()
        {
            // Arrange
            var dateTime = new DateTime(2023, 10, 10);
            var expectedDateTime = new DateTime(2020, 7, 9);

            // Act
            var result = dateTime.Minus(years: 3, months: 3, days: 1);

            // Assert
            result.Date.ShouldBe(expectedDateTime.Date);
        }

        [Fact]
        public void Minus_WithDateTime_ShouldReturnCorrectDateTime()
        {
            // Arrange
            var dateTime = new DateTime(2023, 10, 10);
            var subtractionDateTime = new DateTime(2020, 8, 10);
            var expectedDateTime = DateTime.Parse("0003-1-31");

            // Act
            var result = dateTime.Minus(subtractionDateTime);

            // Assert
            result.ShouldBe(expectedDateTime);
        }

        [Fact]
        public void Plus_WithYearsMonthsDays_ShouldReturnCorrectDateTime()
        {
            // Arrange
            var dateTime = new DateTime(2020, 1, 1);
            var expectedDateTime = new DateTime(2023, 4, 1);

            // Act
            var result = dateTime.Plus(years: 3, months: 3, days: 0);

            // Assert
            result.ShouldBe(expectedDateTime);
        }

        [Fact]
        public void Plus_WithDateTime_ShouldReturnCorrectDateTime()
        {
            // Arrange
            var dateTime = new DateTime(2023, 10, 10);
            var additionDateTime = new DateTime(2020, 8, 10);
            var expectedDateTime = DateTime.Parse("4044-6-20");

            // Act
            var result = dateTime.Plus(additionDateTime);

            // Assert
            result.ShouldBe(expectedDateTime);
        }

        [Theory]
        [InlineData(DayOfWeek.Saturday, true)]
        [InlineData(DayOfWeek.Sunday, true)]
        [InlineData(DayOfWeek.Monday, false)]
        [InlineData(DayOfWeek.Friday, false)]
        public void IsWeekend_ShouldReturnExpectedResult(DayOfWeek dayOfWeek, bool expected)
        {
            // Arrange
            var date = new DateTime(2023, 9, 4).AddDays((int)dayOfWeek -
                                                        (int)DayOfWeek.Monday); // normalize to test day

            // Act
            var result = date.IsWeekend();

            // Assert
            result.ShouldBe(expected);
        }

        [Theory]
        [InlineData(DayOfWeek.Monday, true)]
        [InlineData(DayOfWeek.Tuesday, true)]
        [InlineData(DayOfWeek.Sunday, false)]
        public void IsWeekday_ShouldReturnExpectedResult(DayOfWeek dayOfWeek, bool expected)
        {
            // Arrange
            var date = new DateTime(2023, 9, 4).AddDays((int)dayOfWeek - (int)DayOfWeek.Monday);

            // Act
            var result = date.IsWeekday();

            // Assert
            result.ShouldBe(expected);
        }

        [Fact]
        [ExcludeFromCodeCoverage]
        public void Minus_ShouldThrow_WhenResultIsBeforeMinValue()
        {
            // Arrange
            var baseDate = new DateTime(10, 1, 1);

            // Act & Assert

            Should.Throw<ArgumentOutOfRangeException>(() =>
            {
                var _ = baseDate.Minus(years: 20);
            });
        }


        [Fact]
        public void ToReadableTimeAgo_ShouldReturnSecondsAgo_ForRecentTime()
        {
            // Arrange
            var date = DateTime.UtcNow.AddSeconds(-45);

            // Act
            var result = date.ToReadableTimeAgo();

            // Assert
            result.ShouldBe("45 seconds ago");
        }

        [Fact]
        public void ToReadableTimeAgo_ShouldReturnMinutesAgo_ForRecentTime()
        {
            // Arrange
            var date = DateTime.UtcNow.AddMinutes(-15);

            // Act
            var result = date.ToReadableTimeAgo();

            // Assert
            result.ShouldBe("15 minutes ago");
        }

        [Fact]
        public void ToReadableTimeAgo_ShouldReturnHoursAgo_ForRecentTime()
        {
            // Arrange
            var date = DateTime.UtcNow.AddHours(-3);

            // Act
            var result = date.ToReadableTimeAgo();

            // Assert
            result.ShouldBe("3 hours ago");
        }

        [Fact]
        public void ToReadableTimeAgo_ShouldReturnDaysAgo_ForOldTime()
        {
            // Arrange
            var date = DateTime.UtcNow.AddDays(-5);

            // Act
            var result = date.ToReadableTimeAgo();

            // Assert
            result.ShouldBe("5 days ago");
        }

        [Theory]
        [InlineData(1, TimeOfDay.EarlyMorning)]
        [InlineData(6, TimeOfDay.Dawn)]
        [InlineData(9, TimeOfDay.Morning)]
        [InlineData(13, TimeOfDay.Afternoon)]
        [InlineData(17, TimeOfDay.Dusk)]
        [InlineData(19, TimeOfDay.Evening)]
        [InlineData(22, TimeOfDay.Night)]
        public void GetTimeOfDay_ShouldReturnExpectedEnum(int hour, TimeOfDay expected)
        {
            // Arrange
            var date = new DateTime(2023, 1, 1, hour, 0, 0);

            // Act
            var result = date.GetTimeOfDay();

            // Assert
            result.ShouldBe(expected);
        }
    }
}