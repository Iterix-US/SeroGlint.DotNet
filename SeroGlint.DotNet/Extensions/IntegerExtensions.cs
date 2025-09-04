namespace SeroGlint.DotNet.Common.Extensions
{
    /// <summary>
    /// Extension methods for the integer data type.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Determines if the integer value is between the specified minimum and maximum values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static bool IsBetween(this int value, int minValue, int maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        /// <summary>
        /// Determines if the integer value is a multiple of the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="multiple"></param>
        /// <returns></returns>
        public static bool IsMultipleOf(this int value, int multiple)
        {
            return value % multiple == 0;
        }
    }
}
