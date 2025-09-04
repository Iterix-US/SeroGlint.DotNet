using System.ComponentModel;
using NLog;
using NLog.Targets;
using Serilog;
using Serilog.Events;
using SeroGlint.DotNet.Common;
using SeroGlint.DotNet.Common.Extensions;
using SeroGlint.DotNet.Tests.TestObjects;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.Extensions
{
    public class EnumExtensionsTests
    {
        private enum TestEnum
        {
            [Description("First Value")]
            First,

            [Description("Second Value")]
            Second,

            Third // No description
        }

        [Fact]
        public void GetDescription_ShouldReturnDescriptionAttribute_WhenPresent()
        {
            // Arrange
            var value = TestEnum.First;

            // Act
            var description = value.GetDescription();

            // Assert
            description.ShouldBe("First Value");
        }

        [Fact]
        public void GetDescription_ShouldFallbackToEnumName_WhenNoDescriptionAttribute()
        {
            // Arrange
            var value = TestEnum.Third;

            // Act
            var description = value.GetDescription();

            // Assert
            description.ShouldBe("Third");
        }

        [Fact]
        public void GetAllValues_ShouldReturnAllEnumValues()
        {
            // Arrange
            var dummy = TestEnum.First;

            // Act
            var values = dummy.GetAllValues();

            // Assert
            values.ShouldBe([TestEnum.First, TestEnum.Second, TestEnum.Third]);
        }

        [Theory]
        [InlineData(LoggingLevel.Debug, LogEventLevel.Debug)]
        [InlineData(LoggingLevel.Information, LogEventLevel.Information)]
        [InlineData(LoggingLevel.Warning, LogEventLevel.Warning)]
        [InlineData(LoggingLevel.Error, LogEventLevel.Error)]
        [InlineData(LoggingLevel.Fatal, LogEventLevel.Fatal)]
        [InlineData(LoggingLevel.Verbose, LogEventLevel.Verbose)]
        public void ToSerilogLevel_ValidInputs_ReturnsExpected(LoggingLevel input, LogEventLevel expected)
        {
            var result = input.ToSerilogLevel();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToSerilogLevel_InvalidInput_Throws()
        {
            var invalid = (LoggingLevel)(-999);
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.ToSerilogLevel());
        }

        [Theory]
        [InlineData(LogRollInterval.Daily, RollingInterval.Day)]
        [InlineData(LogRollInterval.Hourly, RollingInterval.Hour)]
        [InlineData(LogRollInterval.Monthly, RollingInterval.Month)]
        [InlineData(LogRollInterval.Yearly, RollingInterval.Year)]
        [InlineData(LogRollInterval.Indefinite, RollingInterval.Infinite)]
        public void ToSerilogRollingInterval_ValidInputs_ReturnsExpected(LogRollInterval input, RollingInterval expected)
        {
            var result = input.ToSerilogRollingInterval();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(LoggingLevel.Verbose)]
        [InlineData(LoggingLevel.Debug)]
        [InlineData(LoggingLevel.Information)]
        [InlineData(LoggingLevel.Warning)]
        [InlineData(LoggingLevel.Error)]
        [InlineData(LoggingLevel.Fatal)]
        public void ToNLogLevel_ReturnsExpected(LoggingLevel input)
        {
            var expected = input switch
            {
                LoggingLevel.Verbose => LogLevel.Trace,
                LoggingLevel.Debug => LogLevel.Debug,
                LoggingLevel.Information => LogLevel.Info,
                LoggingLevel.Warning => LogLevel.Warn,
                LoggingLevel.Error => LogLevel.Error,
                LoggingLevel.Fatal => LogLevel.Fatal,
                _ => throw new ArgumentOutOfRangeException()
            };

            var result = input.ToNLogLevel();
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(LogRollInterval.Hourly, FileArchivePeriod.Hour)]
        [InlineData(LogRollInterval.Daily, FileArchivePeriod.Day)]
        [InlineData(LogRollInterval.Monthly, FileArchivePeriod.Month)]
        [InlineData(LogRollInterval.Yearly, FileArchivePeriod.Year)]
        [InlineData(LogRollInterval.Indefinite, FileArchivePeriod.None)]
        public void ToNLogArchivePeriod_ValidInputs_ReturnsExpected(LogRollInterval input, FileArchivePeriod expected)
        {
            var result = input.ToNLogArchivePeriod();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(LoggingEventId.None)]
        [InlineData(LoggingEventId.ReusingExistingPipe)]
        [InlineData(LoggingEventId.ServerNotRunning)]
        [InlineData(LoggingEventId.DisposingExistingPipe)]
        public void Enum_WhenGettingValue_ShouldReturnIntegerValue(LoggingEventId input)
        {
            var testValue = input.GetValue();
            var confirmedValue = (int)input;

            testValue.ShouldBeEquivalentTo(confirmedValue);
        }

        [Fact]
        public void Enum_WhenGettingValueFromInvalidEnum_ShouldReturnNegativeOne()
        {
            var testValue = SampleLongEnum.ValueOne.GetValue();
            testValue.ShouldBe(-1);
        }
    }
}