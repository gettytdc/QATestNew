namespace BluePrism.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using Utilities.Functional;

    public static class StringExtensionMethods
    {
        /// <summary>
        /// Parses the string as the given enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="this">The string to parse.</param>
        /// <returns>The string parsed as the given enum.</returns>
        public static TEnum ParseEnum<TEnum>(this string @this)
            => (TEnum) Enum.Parse(typeof(TEnum), @this);

        /// <summary>
        /// Converts the given characters to a secure string.
        /// </summary>
        /// <param name="this">The characters to convert.</param>
        /// <returns>A secure string containing the given characters</returns>
        public static SecureString ToSecureString(this IEnumerable<char> @this)
            => new SecureString().Tee(x => @this.ForEach(x.AppendChar).Evaluate());

        /// <summary>
        /// Makes the given secure string insecure.
        /// </summary>
        /// <param name="this">The secure string.</param>
        /// <returns>The insecure string from the secure string</returns>
        public static string MakeInsecure(this SecureString @this)
            => 
            Marshal.SecureStringToGlobalAllocUnicode(@this)
            .Map(ptr =>
                Enumerable.Range(0, @this.Length)
                .Select(i => Marshal.ReadInt16(ptr, i * 2)))
            .Select(x => (char)x)
            .ToArray()
            .Map(x => new string(x));

        /// <summary>
        /// If the string exceeds the character limit, return the truncated string 
        /// with an ellipsis appended. Otherwise, return the string.
        /// </summary>
        /// <param name="this">The string</param>
        /// <param name="characterLimit">The maximum length of the string</param>
        /// <returns>A string that is truncated if it exceeds the character limit</returns>
        public static string TruncateWithEllipsis(this string @this, int characterLimit)
        {
            if (characterLimit < 0)
                throw new ArgumentException($"{nameof(characterLimit)} cannot be less than zero");

            if (string.IsNullOrEmpty(@this))
                return "";

            const string ellipsis = "...";

            if (characterLimit < ellipsis.Length)
                return new string('.', characterLimit);

            if (@this.Length <= characterLimit)
                return @this;
                        
            return @this.Substring(0, characterLimit - ellipsis.Length) + ellipsis;
        }

        private static string[] HttpProtocols = new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };

        public static bool IsValidAndWellFormedAbsoluteUrl(this string @this)
            => Uri.IsWellFormedUriString(@this, UriKind.Absolute) && 
            Uri.TryCreate(@this, UriKind.Absolute, out var uri) &&
            HttpProtocols.Contains(uri.Scheme);


        public static bool ContainsLeadingOrTrailingWhitespace(this string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck))
                return false;

            return stringToCheck.StartsWith(" ")
                || stringToCheck.EndsWith(" ");
        }
    }
}
