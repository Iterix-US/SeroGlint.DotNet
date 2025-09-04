using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NLog;
using NLog.Targets;
using Serilog;
using Serilog.Events;

namespace SeroGlint.DotNet.Common.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the description attribute of an enum value, or the enum value's name if no description is provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDescription<T>(this T enumValue) where T : Enum
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? enumValue.ToString();
        }

        /// <summary>
        /// Retrieves all values of an enum as a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static List<T> GetAllValues<T>(this T enumValue) where T : Enum
        {
            return Enum.GetValues(enumValue.GetType()).Cast<T>().ToList();
        }

        /// <summary>
        /// Retrieves the integer value of an enum, assuming the enum is defined with integer values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static int GetValue<T>(this T enumValue) where T : Enum
        {
            try
            {
                return Convert.ToInt32(enumValue);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Converts a <see cref="LoggingLevel"/> to a <see cref="LogEventLevel"/> for use with Serilog.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static LogEventLevel ToSerilogLevel(this LoggingLevel level)
        {
            switch (level)
            {
                case LoggingLevel.Debug:
                    return LogEventLevel.Debug;
                case LoggingLevel.Information:
                    return LogEventLevel.Information;
                case LoggingLevel.Warning:
                    return LogEventLevel.Warning;
                case LoggingLevel.Error:
                    return LogEventLevel.Error;
                case LoggingLevel.Fatal:
                    return LogEventLevel.Fatal;
                case LoggingLevel.Verbose:
                    return LogEventLevel.Verbose;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, "Invalid logging level specified.");
            }
        }

        /// <summary>
        /// Converts a <see cref="LogRollInterval"/> to a <see cref="RollingInterval"/> for use with Serilog.
        /// </summary>
        /// <param name="logRollInterval"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static RollingInterval ToSerilogRollingInterval(this LogRollInterval logRollInterval)
        {
            switch (logRollInterval)
            {
                case LogRollInterval.Daily:
                    return RollingInterval.Day;
                case LogRollInterval.Hourly:
                    return RollingInterval.Hour;
                case LogRollInterval.Yearly:
                    return RollingInterval.Year;
                case LogRollInterval.Indefinite:
                    return RollingInterval.Infinite;
                default:
                case LogRollInterval.Monthly:
                    return RollingInterval.Month;
            }
        }

        /// <summary>
        /// Converts a <see cref="LoggingLevel"/> to a <see cref="LogLevel"/> for use with NLog.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static LogLevel ToNLogLevel(this LoggingLevel level)
        {
            switch (level)
            {
                case LoggingLevel.Verbose:
                    return LogLevel.Trace;
                case LoggingLevel.Debug:
                    return LogLevel.Debug;
                case LoggingLevel.Warning:
                    return LogLevel.Warn;
                case LoggingLevel.Error:
                    return LogLevel.Error;
                case LoggingLevel.Fatal:
                    return LogLevel.Fatal;
                default:
                case LoggingLevel.Information:
                    return LogLevel.Info;
            }
        }

        /// <summary>
        /// Converts a <see cref="LogRollInterval"/> to a <see cref="FileArchivePeriod"/> for use with NLog.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static FileArchivePeriod ToNLogArchivePeriod(this LogRollInterval interval)
        {
            switch (interval)
            {
                case LogRollInterval.Hourly:
                    return FileArchivePeriod.Hour;
                case LogRollInterval.Daily:
                    return FileArchivePeriod.Day;
                case LogRollInterval.Yearly:
                    return FileArchivePeriod.Year;
                case LogRollInterval.Indefinite:
                    return FileArchivePeriod.None;
                default:
                case LogRollInterval.Monthly:
                    return FileArchivePeriod.Month;
            }
        }
    }
}
