using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Text;

namespace Internationalisation
{

    public class Locales
    {
        /// <summary>
        /// Locales required for to the product setup.
        /// These are mapped to the External Login (Cef Sharp) browser language packs
        /// that also must be installed.
        /// </summary>
        private static readonly Dictionary<string, (string Pak, bool IsUnicode)> _locales =
            new Dictionary<string, (string, bool)>
            {
                { "de-DE", ("de.pak", true)},
                { "en-GB", ("en-US.pak", false)},
                { "en-US", ("en-US.pak", false)},
                { "en-CA", ("en-US.pak", false)},
                { "en-ZA", ("en-US.pak", false)},
                { "en-001", ("en-US.pak", false) },
                { "en-150", ("en-US.pak", false)},
                { "es-419", ("es.pak", true)},
                { "fr-FR", ("fr.pak", true)},
                { "ja-JP", ("ja.pak", true)},
                { "zh-CN", ("zh-CN.pak", true)},
                { "zh-SG", ("zh-CN.pak", true)},
                { "zh-Hans-HK", ("zh-CN.pak", true)},
                { "zh-Hans-MO", ("zh-CN.pak", true)}
            };

        private static readonly IEnumerable<string> _unicodeLocales = _locales.Where(x => x.Value.IsUnicode).Select(t => t.Key);

        public static IEnumerable<string> SupportedLocales => _locales.Select(x => x.Key);
        public static List<KeyValuePair<string, (string culture, bool isUnicode)>> LocaleList => _locales.ToList();

        /// <summary>
        /// Get the list of language packs we require for the Cef Browser
        /// </summary>
        /// <returns>List of the language pack files</returns>
        public static List<string> CefLanguagePacks => _locales.Select(l => l.Value.Pak).Distinct().ToList();

        /// <summary>
        /// Get the CEF language pack for the locale.
        /// </summary>
        /// <param name="locale">The locale</param>
        /// <returns>The matching lang pack file. null if none.</returns>
        public static string GetCefLanguagePack(string locale)
        {
            if (string.IsNullOrEmpty(locale)) throw new ArgumentNullException(nameof(locale));

            _locales.TryGetValue(locale, out (string pak, bool) value);
            return value.pak;
        }

        /// <summary>
        /// Determine if a given locale requires unicode
        /// </summary>
        /// <param name="nativeName">Native name for the locale i.e. English (United Kingdom)</param>
        /// <returns>False if native name is not found</returns>
        public static bool IsUnicode(string nativeName)
        {
            if (nativeName is null) throw new ArgumentNullException(nameof(nativeName));

            bool registeredUnicode = false;
            try
            {
                registeredUnicode = _unicodeLocales.Any(x
                    => string.Compare(nativeName, new CultureInfo(x).NativeName, ignoreCase: true) == 0);
            }
            // Some cultures such as zh-Hans-HK are not supported in windows7
            catch (CultureNotFoundException) { }

            bool bytesCheck = Encoding.ASCII.GetByteCount(nativeName) != Encoding.UTF8.GetByteCount(nativeName);

            return registeredUnicode || bytesCheck;
        }
    }
}
