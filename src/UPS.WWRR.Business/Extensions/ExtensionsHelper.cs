using System.ComponentModel;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace UPS.WWRR.Business.Extensions
{
    /// <summary>
    /// Provides extension methods for DateTime.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a DateTime to UNIX time.
        /// </summary>
        /// <param name="dateTime">The DateTime instance.</param>
        /// <returns>A string representation of the UNIX time.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the DateTime instance is outside the range of UNIX time.</exception>
        public static string ToUnixTime(this DateTime dateTime)
        {
            DateTimeOffset dto = new(dateTime.ToUniversalTime());
            return dto.ToUnixTimeSeconds().ToString();
        }

        /// <summary>
        /// Converts a DateTime to UNIX time including milliseconds.
        /// </summary>
        /// <param name="dateTime">The DateTime instance.</param>
        /// <returns>A string representation of the UNIX time in milliseconds.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the DateTime instance is outside the range of UNIX time.</exception>
        public static string ToUnixTimeMilliSeconds(this DateTime dateTime)
        {
            DateTimeOffset dto = new(dateTime.ToUniversalTime());
            return dto.ToUnixTimeMilliseconds().ToString();
        }
    }

    /// <summary>
    /// Provides helper methods for working with Enumerations.
    /// </summary>
    public static class Enumerations
    {
        /// <summary>
        /// Retrieves the description of an Enum value.
        /// </summary>
        /// <param name="value">The Enum value.</param>
        /// <returns>The description of the Enum value.</returns>
        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }

    /// <summary>
    /// Provides extension methods for the String class.
    /// </summary>
    public static class StringExtension
    {

        /// <summary>
        /// Converts a UNIX time in seconds to a DateTime.
        /// </summary>
        /// <param name="UnixTimeSeconds">The UNIX time in seconds.</param>
        /// <returns>A DateTime representing the UNIX time.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the UNIX time is outside the range of DateTime.</exception>
        /// <exception cref="System.FormatException">Thrown when the UNIX time is not in the correct format.</exception>
        public static DateTime UnixTimeSecondsToDateTime(this string UnixTimeSeconds)
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(UnixTimeSeconds));
            return dto.DateTime;
        }

        /// <summary>
        /// Converts a UNIX time in milliseconds to a DateTime.
        /// </summary>
        /// <param name="UnixTimeMilliseconds">The UNIX time in milliseconds.</param>
        /// <returns>A DateTime representing the UNIX time.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the UNIX time is outside the range of DateTime.</exception>
        /// <exception cref="System.FormatException">Thrown when the UNIX time is not in the correct format.</exception>
        public static DateTime UnixTimeMillisecondsToDateTime(this string UnixTimeMilliseconds)
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(UnixTimeMilliseconds));
            return dto.DateTime;
        }

        /// <summary>
        /// Decompresses a byte array that was compressed using GZip compression.
        /// </summary>
        /// <param name="bytes">The compressed byte array.</param>
        /// <returns>The decompressed byte array.</returns>
        public static byte[] DecompressBytes(byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            using var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            decompressStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        /// <summary>
        /// Decompresses a Base64-encoded GZip-compressed string into its original form.
        /// </summary>
        /// <param name="compressed">The compressed string encoded in Base64.</param>
        /// <returns>The original uncompressed string.</returns>
        public static string DecompressMessage(string compressed)
        {
            var bytes = Convert.FromBase64String(compressed);
            var returnBytes = DecompressBytes(bytes);
            return Encoding.UTF8.GetString(returnBytes);
        }
    }
}

