using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BluePrism.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToSentenceCase(this string sentence)
        {
            return Char.ToUpperInvariant(sentence[0]) + sentence.Substring(1).ToLowerInvariant();
        }

        
        /// <summary>
        /// Returns the given phrase as ToLowerInvariant unless the language is German in which case it will be Title case
        /// </summary>
        /// <param name="phrase">The noun</param>
        /// <returns>the formatted phrase</returns>
        public static string ToConditionalLowerNoun(this string phrase)
        {
            return CultureInfo.CurrentUICulture.Parent.Name == "de" ? phrase.ToTitleCase() : phrase.ToLowerInvariant();
        }

        /// <summary>
        /// Returns the given phrase as culture specific Title Case
        /// </summary>
        /// <param name="phrase">The noun</param>
        /// <returns>the formatted phrase</returns>
        public static string ToTitleCase(this string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) return phrase;

            //split the phrase into words
            var textInfo = CultureInfo.CurrentUICulture.TextInfo;
            return textInfo.ToTitleCase(phrase.ToLowerInvariant());
        }


        /// <summary>
        /// Returns a nullable int of the first occurence of a 
        /// number contained within the given string
        /// </summary>
        /// <returns>Null if the string is null or empty. Zero if no integer is contained</returns>
        public static int? GetInt(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var regexResult = Regex.Match(str, @"\d+").Value;
            int.TryParse(regexResult, out int val);
            return val;
        }
        
        /// <summary>
        /// Evaluates if a string is a valid URL
        /// </summary>
        /// <returns>True if the string is a valid URL, False if not</returns>
        public static bool IsValidUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            var pattern = @"^((?:^|\s)((https?:\/\/)?(?:localhost|[\w-]+(?:\.[\w-]+)+)(:\d+)?(\/\S*)?))?$";
            if (Regex.IsMatch(url, pattern))
            {
                return true;
            }

            // Try with Legacy Pattern
            pattern = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";

            return Regex.IsMatch(url, pattern);
        }
    }
}
