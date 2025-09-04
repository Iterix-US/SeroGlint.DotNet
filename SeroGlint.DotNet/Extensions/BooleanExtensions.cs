namespace SeroGlint.DotNet.Common.Extensions
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// Converts a boolean to an integer.
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>True: 1, False: 0</returns>
        public static int ToInteger(this bool boolean)
        {
            return boolean ? 1 : 0;
        }

        /// <summary>
        /// Converts a boolean to a Yes or No string
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>True: Yes, False: No</returns>
        public static string ToYesNo(this bool boolean)
        {
            return boolean ? "Yes" : "No";
        }

        /// <summary>
        /// Converts a boolean to a One or Zero string
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>True: 1, False: 0</returns>
        public static string ToOneZero(this bool boolean)
        {
            return boolean ? "1" : "0";
        }

        /// <summary>
        /// Converts a boolean to an On or Off string
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>True: On, False: Off</returns>
        public static string ToOnOff(this bool boolean)
        {
            return boolean ? "On" : "Off";
        }

        /// <summary>
        /// Converts a boolean to an Enabled or Disabled string
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>True: Enabled, False: Disabled</returns>
        public static string ToEnabledDisabled(this bool boolean)
        {
            return boolean ? "Enabled" : "Disabled";
        }

        /// <summary>
        /// Converts a boolean to an Active or Inactive string
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns>True: Active, False: Inactive</returns>
        public static string ToActiveInactive(this bool boolean)
        {
            return boolean ? "Active" : "Inactive";
        }
    }
}
