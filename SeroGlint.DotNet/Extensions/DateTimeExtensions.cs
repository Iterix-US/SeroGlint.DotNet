using System;

namespace SeroGlint.DotNet.Common.Extensions
{
    /// <summary>
    /// Extension methods for the DateTime data type.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Subtracts a specified number of years, months, days, hours, minutes, seconds, and milliseconds from the base DateTime.
        /// </summary>
        /// <param name="dateTime">The base DateTime to subtract from</param>
        /// <param name="years"></param>
        /// <param name="months"></param>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime Minus(this DateTime dateTime, int years = 0, int months = 0, int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            var result = dateTime
                .AddYears(-years)
                .AddMonths(-months)
                .AddDays(-days)
                .AddHours(-hours)
                .AddMinutes(-minutes)
                .AddSeconds(-seconds)
                .AddMilliseconds(-milliseconds);

            return result;
        }

        /// <summary>
        /// Subtracts a specified DateTime from the base DateTime.
        /// </summary>
        /// <param name="dateTime">The base DateTime to subtract from</param>
        /// <param name="subtractionDateTime">The DateTime being subtracted from the base</param>
        /// <returns></returns>
        public static DateTime Minus(this DateTime dateTime, DateTime subtractionDateTime)
        {
            return dateTime.Minus(
                subtractionDateTime.Year,
                subtractionDateTime.Month,
                subtractionDateTime.Day,
                subtractionDateTime.Hour,
                subtractionDateTime.Minute,
                subtractionDateTime.Second,
                subtractionDateTime.Millisecond
            );
        }

        /// <summary>
        /// Adds a specified number of years, months, days, hours, minutes, seconds, and milliseconds to the base DateTime.
        /// </summary>
        /// <param name="dateTime">The base DateTime to add to</param>
        /// <param name="years"></param>
        /// <param name="months"></param>
        /// <param name="days"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime Plus(this DateTime dateTime, int years = 0, int months = 0, int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            var result = dateTime
                .AddYears(years)
                .AddMonths(months)
                .AddDays(days)
                .AddHours(hours)
                .AddMinutes(minutes)
                .AddSeconds(seconds)
                .AddMilliseconds(milliseconds);

            return result;
        }

        /// <summary>
        /// Adds a specified DateTime to the base DateTime.
        /// </summary>
        /// <param name="dateTime">The base DateTime to add to</param>
        /// <param name="additionDateTime">The DateTime being added to the base</param>
        /// <returns></returns>
        public static DateTime Plus(this DateTime dateTime, DateTime additionDateTime)
        {
            return dateTime.Plus(
                additionDateTime.Year,
                additionDateTime.Month,
                additionDateTime.Day,
                additionDateTime.Hour,
                additionDateTime.Minute,
                additionDateTime.Second,
                additionDateTime.Millisecond
            );
        }

        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekday(this DateTime date)
        {
            return !date.IsWeekend();
        }

        /// <summary>
        /// Give the difference between "Now" and the give time in a human-readable format.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToReadableTimeAgo(this DateTime date)
        {
            var timespan = DateTime.UtcNow - date;

            if (timespan.TotalSeconds < 60)
            {
                return $"{timespan.Seconds} seconds ago";
            }

            if (timespan.TotalMinutes < 60)
            {
                return $"{timespan.Minutes} minutes ago";
            }

            return timespan.TotalHours < 24 ?
                $"{timespan.Hours} hours ago" :
                $"{timespan.Days} days ago";
        }

        /// <summary>
        /// Determines the friendly-name of the given time based on the hour of the day.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static TimeOfDay GetTimeOfDay(this DateTime dateTime)
        {
            var hour = dateTime.Hour;

            if (hour >= 0 && hour < 6)
                return TimeOfDay.EarlyMorning;
            if (hour >= 6 && hour < 7)
                return TimeOfDay.Dawn;
            if (hour >= 7 && hour < 12)
                return TimeOfDay.Morning;
            if (hour >= 12 && hour < 17)
                return TimeOfDay.Afternoon;
            if (hour >= 17 && hour < 18)
                return TimeOfDay.Dusk;
            if (hour >= 18 && hour < 21)
                return TimeOfDay.Evening;

            return TimeOfDay.Night;
        }
    }
}