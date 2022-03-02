using Jeffijoe.MessageFormat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LocaleTools
{
    public static class LTools
    {
        private static readonly Dictionary<string, ResourceManager> ResourceManagers = 
            new Dictionary<string, ResourceManager>(StringComparer.OrdinalIgnoreCase)
        {
            {"actions", new ResourceManager("LocaleTools.actions", typeof(LTools).Assembly)},
            {"elements", new ResourceManager("LocaleTools.elements", typeof(LTools).Assembly)},
            {"holiday", new ResourceManager("LocaleTools.holiday", typeof(LTools).Assembly)},
            {"misc", new ResourceManager("LocaleTools.misc", typeof(LTools).Assembly)},
            {"roleperms", new ResourceManager("LocaleTools.roleperms", typeof(LTools).Assembly)},
            {"tile", new ResourceManager("LocaleTools.tile", typeof(LTools).Assembly)},
            {"validation", new ResourceManager("LocaleTools.validation", typeof(LTools).Assembly)},
            {"vbo", new ResourceManager("LocaleTools.vbo", typeof(LTools).Assembly)}
        };

        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            Cache = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        // get a string based on english/keyword reference, handle everything and fall through to existing string
        public static string Get(string key, string prefix, string locale, string id_prefix)
        {
            if (key == null) return null;
            if (key.Length == 0) return key;
            var idkey = id_prefix + "_" + key;
            var result = GetCache(idkey, prefix, locale);
            if (result != null)
            {
             //   Console.WriteLine("CACHE: HIT: " + idkey + " " + result);
                return result;
            }
            var culture = GetCulture(locale);
            try
            {
                var mgr = GetManager(prefix);
                result = mgr?.GetString(GetKey(idkey), culture) ?? key;
                SetCache(idkey, prefix, locale, result);
            //    Console.WriteLine("CACHE: MISS: " + idkey);
                return result;
            }
            catch (MissingManifestResourceException)
            {
            //    Console.WriteLine("CACHE: NOT FOUND MISS: " + idkey);
                SetCache(idkey, prefix, locale, key);
                return key;
            }
        }

        // get a string based on english/keyword reference with id_prefix, use current CultureInfo.CurrentCulture as locale
        public static string GetC(string key, string prefix, string id_prefix)
        {
            return Get(key, prefix, null, id_prefix);
        }

        // get a string based on english/keyword reference, use current CultureInfo.CurrentCulture as locale
        public static string GetC(string key, string prefix)
        {
            return Get(key, prefix, null);
        }


        // get a string based on english/keyword reference, use CultureInfo.CurrentCulture as locale
        public static string Get(string key, string prefix)
        {
            return Get(key, prefix, null);
        }

        // get a string based on english/keyword reference, handle everything and fall through to existing string
        public static string Get(string key, string prefix, string locale)
        {
            if (key == null) return null;
            if (key.Length == 0) return key;
            var culture = GetCulture(locale);
            try
            {
                var mgr = GetManager(prefix);
                return mgr?.GetString(GetKey(key), culture) ?? key;
            }
            catch (MissingManifestResourceException)
            {
                return key;
            }
        }

        // store a cached result value
        private static void SetCache(string key, string prefix, string locale, string value)
        {
            locale = EnsureLocale(locale);

            if (!Cache.TryGetValue(locale, out var localeCache))
            {
                Cache[locale] = new Dictionary<string, Dictionary<string, string>>();
                localeCache = Cache[locale];
            }

            if (!localeCache.TryGetValue(prefix, out var prefixCache))
            {
                localeCache[prefix] = new Dictionary<string, string>();
                prefixCache = localeCache[prefix];
            }

            prefixCache[key] = value;

        }

        // get a cached value
        private static string GetCache(string key, string prefix, string locale)
        {
            locale = EnsureLocale(locale);

            if (!Cache.TryGetValue(locale, out var localeCache)) return null;
            if (!localeCache.TryGetValue(prefix, out var prefixCache)) return null;
            return !prefixCache.TryGetValue(key, out var result) ? null : result;
        }


        // get a string based on english/keyword reference, handle everything and fall through to null
        public static string GetOrNull(string key, string prefix, string locale)
        {
            var culture = GetCulture(locale);
            try
            {
                return string.IsNullOrEmpty(key) ? null : GetManager(prefix).GetString(GetKey(key), culture);
            }
            catch (MissingManifestResourceException e)
            {
                return null;
            }
        }


        // basic string get 
        public static string Get(object key, string prefix, string locale)
        {
            return key == null ? null : Get((string)key, prefix, locale);
        }


        /*

         // Example with embedded prefix and custom fallback option 
          Public Shared Function GetString(ByVal value As String, Optional ByVal fallback As String = "") As String
                Try
                    Return If(LTools.GetOrNull(value, "validation", clsOptions.CurrentLocale), If(String.IsNullOrEmpty(fallback), value, fallback))
                Catch __exception1__ As Exception
                    Return If(String.IsNullOrEmpty(fallback), value, fallback)
                End Try
          End Function


         */

        public static string Format(string format, Dictionary<string, object> args)
        {
            return MessageFormatter.Format(format, args);
        }

        public static string Format(string format, params object[] parms)
        {
            var args = new Dictionary<string, object>();
            for (var i = 0; i < parms.Length - 1; i = i + 2)
            {
                args.Add((string)parms[i], parms[i + 1]);
            }
            return MessageFormatter.Format(format, args);
        }

        public static string Ordinal(int value, string locale)
        {
            return Ordinal("Ordinal", value, locale);
        }

        public static string OrdinalWithNumber(int value, string locale)
        {
            return Ordinal("OrdinalWithNumber", value, locale);
        }

        public static string OrdinalWithNumberDayOfMonth(int value, string locale)
        {
            return Ordinal("OrdinalWithNumberDayOfMonth", value, locale);
        }

        public static string OrdinalWithNumberMonthOfYear(int value, string locale)
        {
            return Ordinal("OrdinalWithNumberMonthOfYear", value, locale);
        }

        public static string Ordinal(string key, int value, string locale)
        {
            var fmt = GetOrNull(key, "misc", locale);
            if (fmt == null)
            {
                // fallback to english
                if (key.Equals("Ordinal"))
                {
                    fmt = "{ORDINAL, select, one {st} two {nd} few {rd} other {th}}";
                }
                else
                {
                    fmt = "{ORDINAL, select, one {{VALUE}st} two {{VALUE}nd} few {{VALUE}rd} other {{VALUE}th}}";
                }
            }
            return Format(fmt, "ORDINAL", GetOrdType(value, locale), "VALUE", value);
        }

        public static string GetOrdType(int n, string locale)
        {
            return GetOrdTypeEnum(n, locale).ToString();
        }

        private static OrdType GetOrdTypeEnum(int n, string locale)
        {
            // plural rules from
            // http://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html
            switch (locale)
            {
                case "en-US":
                case "en-GB":
                case "en":
                    if (n % 10 == 1 && n % 100 != 11) return OrdType.one;
                    else
                        if (n % 10 == 2 && n % 100 != 12) return OrdType.two;
                    else
                        if (n % 10 == 3 && n % 100 != 13) return OrdType.few;
                    else
                        return OrdType.other;

                default:
                    return OrdType.other;
            }
        }

        enum OrdType { one, two, few, many, other }

        // get a suitable resource name, once specified this cannot be changed
        // or existing strings will get new keys.
        public static string GetKey(string value)
        {
            var key = Regex.Replace(value, @"[)(]", "_");
            key = Regex.Replace(key, @"[^a-zA-Z0-9_]", "");

            if (Regex.IsMatch(key, @"^[0-9]"))
            {
                key = "n" + key;
            }

            if (key.Length > 50)
            {
                return key.Substring(0, 34) + "_" +
                       CreateSHA2_256(Regex.Replace(value, @"[^a-zA-Z0-9_)(-=@%]", "").Trim()).Substring(0, 16);
            }

            return key;
        }


        // Create SHA256 hash string from input
        private static string CreateSHA2_256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = sha.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private static ResourceManager GetManager(string prefix)
        {
            if (ResourceManagers.TryGetValue(prefix, out var manager))
                return manager;

            return null;
        }

        private static CultureInfo GetCulture(string locale)
        {
            if (locale == null) return CultureInfo.CurrentUICulture;
            try
            {
                return new CultureInfo(locale);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.CurrentUICulture;
            }
        }
        
        private static string EnsureLocale(string locale) => locale ?? CultureInfo.CurrentUICulture.Name;
    }
}
