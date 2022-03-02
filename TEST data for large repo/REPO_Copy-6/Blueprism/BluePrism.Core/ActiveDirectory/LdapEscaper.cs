
using System.Collections.Generic;
using System.Linq;

namespace BluePrism.Core.ActiveDirectory
{
    public class LdapEscaper
    {
        /// <summary>
        /// Mapping of escaped strings onto the chars that they are escaping.
        /// The mapping is basically taken from the rules laid out on MSDN at:
        /// https://msdn.microsoft.com/en-us/library/aa746475.aspx#Special_Characters
        /// </summary>
        private static IDictionary<char, string> _ldapEscapes = new Dictionary<char, string>()
        {
            {'*', @"\2a"}, {'(', @"\28"}, {')', @"\29"},
            { '\\', @"\5c"}, {'\0', @"\00"}, {'/', @"\2f"}
        };

        public static string EscapeSearchTerm(string searchTerm)
            => EscapeSearchTerm(searchTerm, true);
                
        public static string EscapeSearchTerm(string searchTerm, bool escapeWildcardCharacter)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return searchTerm;

            var charsToIgnore = escapeWildcardCharacter ? new char[] { } : new char[] {'*'};
            var escapedChars = searchTerm.SelectMany(c => EscapeChar(c, charsToIgnore));

            return new string(escapedChars.ToArray());
        }

        private static string EscapeChar(char c, IEnumerable<char> charsToIgnore)
            =>_ldapEscapes
                    .Where(escape => !charsToIgnore.Contains(escape.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    .TryGetValue(c, out var escapedChar) ? escapedChar : c.ToString();
   
    }
}
