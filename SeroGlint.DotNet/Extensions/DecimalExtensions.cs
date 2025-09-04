using System;

namespace SeroGlint.DotNet.Common.Extensions
{
    /// <summary>
    /// Extension methods for the decimal data type.
    /// </summary>
    public static class DecimalExtensions
    {
        /// <summary>
        /// Determines if the decimal value is positive.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsPositive(this decimal value)
        {
            return value >= 0;
        }

        /// <summary>
        /// Determines if the decimal value is negative.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNegative(this decimal value)
        {
            return value < 0;
        }

        /// <summary>
        /// Determines if the decimal value is zero.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsZero(this decimal value)
        {
            return value == 0;
        }

        /// <summary>
        /// Determines if the decimal value is between the specified minimum and maximum values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static bool IsBetween(this decimal value, decimal minValue, decimal maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        /// <summary>
        /// Determines if the decimal value is a multiple of the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="multiple"></param>
        /// <returns></returns>
        public static bool IsMultipleOf(this decimal value, decimal multiple)
        {
            return value % multiple == 0;
        }

        /// <summary>
        /// Determines if the decimal value is even.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEven(this decimal value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// Determines if the decimal value is odd.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this decimal value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// Determines if the decimal value is an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInteger(this decimal value)
        {
            return value % 1 == 0;
        }

        /// <summary>
        /// Determines if the decimal value is a whole number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToIntegerCeiling(this decimal value)
        {
            return (int)Math.Ceiling(value);
        }

        /// <summary>
        /// Determines if the decimal value is a whole number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToIntegerFloor(this decimal value)
        {
            return (int)Math.Floor(value);
        }

        /// <summary>
        /// Determines if the decimal value is a whole number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToIntegerRound(this decimal value)
        {
            return (int)Math.Round(value);
        }

        /// <summary>
        /// Determines if the decimal value is a whole number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToIntegerTruncate(this decimal value)
        {
            return (int)Math.Truncate(value);
        }

        /// <summary>
        /// Converts the decimal value to a string with the specified number of decimal points.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPoints"></param>
        /// <returns></returns>
        public static string ToDecimalString(this decimal value, int decimalPoints = 2)
        {
            var zeroes = new string('0', decimalPoints);
            return value.ToString($"0.{zeroes}");
        }
    }
}
