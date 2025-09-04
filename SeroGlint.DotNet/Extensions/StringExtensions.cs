using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using static System.Double;
using static System.Int32;

namespace SeroGlint.DotNet.Common.Extensions
{
    /// <summary>
    /// Extension methods for the string data type.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines if a string contains another string regardless of capitalization.
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string baseString, string value)
        {
            return baseString.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Determines if a string is equal to another string regardless of capitalization.
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string baseString, string value)
        {
            return baseString.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if a string starts with another string regardless of capitalization.
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StartsWithIgnoreCase(this string baseString, string value)
        {
            return baseString.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if a string ends with another string regardless of capitalization.
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool EndsWithIgnoreCase(this string baseString, string value)
        {
            return baseString.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a string to title case.
        /// </summary>
        /// <param name="baseString"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string baseString)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(baseString.ToLower());
        }

        /// <summary>
        /// Converts a string to an integer (32-bit)
        /// </summary>
        /// <param name="baseString">The string to be converted</param>
        /// <param name="convertedValue">The result of the conversion</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToInt32(this string baseString, out int convertedValue)
        {
            return TryParse(baseString, out convertedValue);
        }

        /// <summary>
        /// Converts a string to an integer (64-bit)
        /// </summary>
        /// <param name="baseString">The string to be converted</param>
        /// <param name="convertedValue">The result of the conversion</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToInt64(this string baseString, out long convertedValue)
        {
            return long.TryParse(baseString, out convertedValue);
        }

        /// <summary>
        /// Converts a string to a double
        /// </summary>
        /// <param name="baseString">The string to be converted</param>
        /// <param name="convertedValue">The result of the conversion</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToDouble(this string baseString, out double convertedValue)
        {
            return TryParse(baseString, out convertedValue);
        }

        /// <summary>
        /// Converts a string to a decimal
        /// </summary>
        /// <param name="baseString">The string to be converted</param>
        /// <param name="convertedValue">The result of the conversion</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToDecimal(this string baseString, out decimal convertedValue)
        {
            return decimal.TryParse(baseString, out convertedValue);
        }

        /// <summary>
        /// Converts a string to a DateTime
        /// </summary>
        /// <param name="baseString">The string to be converted</param>
        /// <param name="convertedValue">The result of the conversion</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToDateTime(this string baseString, out DateTime convertedValue)
        {
            return DateTime.TryParse(baseString, out convertedValue);
        }

        /// <summary>
        /// Converts a string to a boolean if the string value is "1", "0", "true", or "false"
        /// </summary>
        /// <param name="baseString">The string to be converted</param>
        /// <param name="convertedValue">The result of the conversion</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToBoolean(this string baseString, out bool convertedValue)
        {
            switch (baseString)
            {
                case "1":
                    convertedValue = true;
                    return true;
                case "0":
                    convertedValue = false;
                    return true;
                default:
                    return bool.TryParse(baseString, out convertedValue);
            }
        }

        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <typeparam name="T">The object to deserialize the JSON into</typeparam>
        /// <param name="jsonString">The JSON string being deserialized into an object</param>
        /// <returns>The deserialized and hydrated object</returns>
        /// <exception cref="Exception">Converting from string to given type experienced an error.</exception>
        public static T FromJsonToType<T>(this string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deserializing the JSON string.", ex);
            }
        }

        /// <summary>
        /// Converts an XML string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The XML string being deserialized into an object</typeparam>
        /// <param name="xmlString">The XML string being deserialized into an object</param>
        /// <param name="logger"></param>
        /// <returns>The deserialized and hydrated object</returns>
        /// <exception cref="InvalidOperationException">Converting from string to given type experienced an error.</exception>
        public static T FromXmlToType<T>(this string xmlString, ILogger logger = null)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(xmlString))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger?.LogError(ex, "Failed to deserialize XML to type {TypeName}", typeof(T).Name);
                throw new InvalidOperationException($"Error deserializing XML to {typeof(T)}.", ex);
            }
        }


        /// <summary>
        /// Determines if a string is a valid Enum value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsValidEnumValue<T>(this string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value, out result);
        }

        /// <summary>
        /// Converts a string to an enum value based on the description attribute of the Enum value
        /// Requires the enum to have values defined on its fields with the Description attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T? GetEnumFromDescription<T>(this string description) where T : struct, Enum
        {
            var type = typeof(T);

            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null && attribute.Description.Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }

            return null;
        }

        /// <summary>
        /// Encodes a string as a URL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeAsHttp(this string value)
        {
            return Uri.EscapeDataString(value);
        }

        /// <summary>
        /// Hashes a string using SHA256
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSha256(this string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(sha256.ComputeHash(bytes));
            }
        }

        /// <summary>
        /// Converts a string to a hexadecimal representation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToHexString(this string input, Encoding encoding = null)
        {
            var intendedEncoding = encoding ?? Encoding.UTF8;
            var bytes = intendedEncoding.GetBytes(input);

            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Checks if a string is null or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhitespace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}