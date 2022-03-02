using System;
using System.Globalization;

namespace BluePrism.Core
{
    /// <summary>
    /// An internal culture info which can be used for formatting and parsing within
    /// the ProcessValue and documents its formats in a standard manner.
    /// </summary>
    /// <remarks>This is a singleton class and therefore it can only be accessed
    /// using the static <see cref="InternalCulture.Instance"/> property.
    /// </remarks>
    public class InternalCulture : CultureInfo
    {
        /// <summary>
        /// The single instance of the internal culture info object.
        /// </summary>
        public static readonly CultureInfo Instance = new InternalCulture();

        /// <summary>
        /// Creates a new internal culture info for use in parsing and formatting
        /// dates as required by the internal workings of Blue Prism data handling
        /// </summary>
        private InternalCulture()
            // Historically we were based on GB culture, so lean towards that
            // (without accepting any user changes)
            : base("en-GB", false)
        {
            DateTimeFormatInfo fmt = DateTimeFormat;
            fmt.ShortDatePattern = "yyyy'/'MM'/'dd";
            fmt.LongDatePattern = "dddd, dd MMMM yyyy";
            fmt.ShortTimePattern = "HH':'mm";
            fmt.LongTimePattern = "HH':'mm':'ss";
            fmt.FullDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
        }

        /// <summary>
        /// The name of this culture. A non-standard culture is prefixed by "x-",
        /// and this is a Blue Prism English variant.
        /// </summary>
        public override string Name { get { return "x-en-BP"; } }

        /// <summary>
        /// The native name of this culture
        /// </summary>
        public override string NativeName { get { return "English/BluePrism"; } }

        /// <summary>
        /// Parses a string into a <see cref="Single">float</see> value
        /// </summary>
        /// <param name="s">The string to parse into a floating point value.
        /// </param>
        /// <returns>The floating point value found within the given string.
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="s"/> is null
        /// </exception>
        /// <exception cref="FormatException">if <paramref name="s"/> is not a
        /// numeric value</exception>
        public static float Sng(string s)
        {
            return Single.Parse(s, NumberStyles.Number, Instance);
        }

        /// <summary>
        /// Attempts to parse a string value into a <see cref="Single">float</see>
        /// </summary>
        /// <param name="s">The string to parse into a floating point value.</param>
        /// <param name="val">The resultant floating point value, or 0 if the string
        /// could not be parsed into a valid floating point value.</param>
        /// <returns>True if the string was successfully parsed into a floating point
        /// value; False if it could not be so parsed.</returns>
        public static bool TrySng(string s, out float val)
        {
            return Single.TryParse(s, NumberStyles.Number, Instance, out val);
        }

        /// <summary>
        /// Parses the given string into a <see cref="Double">double</see> value
        /// </summary>
        /// <param name="s">The string to parse into a double-length floating point
        /// value.</param>
        /// <returns>The double value found within the given string</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="s"/> is null
        /// </exception>
        /// <exception cref="FormatException">if <paramref name="s"/> is not a
        /// numeric value</exception>
        public static double Dbl(string s)
        {
            return Double.Parse(s, NumberStyles.Number, Instance);
        }

        /// <summary>
        /// Attempts to parse a string value into a <see cref="Double">double</see>
        /// </summary>
        /// <param name="s">The string to parse into a floating point value.</param>
        /// <param name="val">The resultant floating point value, or 0 if the string
        /// could not be parsed into a valid floating point value.</param>
        /// <returns>True if the string was successfully parsed into a floating point
        /// value; False if it could not be so parsed.</returns>
        public static bool TryDbl(string s, out double val)
        {
            return Double.TryParse(s, NumberStyles.Number, Instance, out val);
        }

        /// <summary>
        /// Parses the given string into a <see cref="Decimal">decimal</see> value
        /// </summary>
        /// <param name="s">The string to parse into a decimal value.</param>
        /// <returns>The decimal value found within the given string</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="s"/> is null
        /// </exception>
        /// <exception cref="FormatException">if <paramref name="s"/> is not a
        /// numeric value</exception>
        public static decimal Dec(string s)
        {
            return Decimal.Parse(s, NumberStyles.Number, Instance);
        }

        /// <summary>
        /// Attempts to parse a string value into a <see cref="Decimal">decimal</see>
        /// </summary>
        /// <param name="s">The string to parse into a decimal value.</param>
        /// <param name="val">The resultant decimal value, or 0 if the string
        /// could not be parsed into a valid decimal value.</param>
        /// <returns>True if the string was successfully parsed into a decimal value;
        /// False if it could not be so parsed.</returns>
        public static bool TryDec(string s, out decimal val)
        {
            return Decimal.TryParse(s, NumberStyles.Number, Instance, out val);
        }

    }
}
