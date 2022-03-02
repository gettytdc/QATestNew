namespace BluePrism.Api.Setup.LocalizationUtility
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var localizationDictionaries = new Dictionary<string, Dictionary<string, string>>
            {
                { "", GetLocalizationDictionary("en-us") },
                { "de-DE", GetLocalizationDictionary("de-de") },
                { "es-419", GetLocalizationDictionary("es-es") },
                { "fr-FR", GetLocalizationDictionary("fr-fr") },
                { "ja-JP", GetLocalizationDictionary("ja-jp") },
                { "zh-Hans", GetLocalizationDictionary("zh-CN") },
            };

            var resourceFiles = Directory.GetFiles(@"..\..\..\BluePrism.Api.Setup\Dialogs", "*.resx");
            foreach (var file in resourceFiles)
            {
                var doc = XDocument.Load(file);
                var elements = doc.Root.Elements("data")
                    .Select(x => x.Element("value"))
                    .Where(x => x.Value.StartsWith("[") && x.Value.EndsWith("]"))
                    .ToList();

                var culture = Regex.Match(file, @".+\.(\w{2}-(?:\w|\d){2,})\.resx").Groups[1].Value;
                foreach (var element in elements)
                {
                    var placeholder = element.Value.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (localizationDictionaries[culture].TryGetValue(placeholder, out var translation))
                    {
                        element.Value = SanitizeTranslation(translation);
                    }
                }

                doc.Save(file);
            }
        }

        private static string SanitizeTranslation(string translation) =>
            RemoveCurlyBracePlaceholders(translation)
                .Replace("[ProductName]", "Blue Prism API");

        private static string RemoveCurlyBracePlaceholders(string translation)
        {
            var match = new Regex("{(.*)}").Match(translation);

            return match.Success
                ? translation.Replace(match.Value, string.Empty)
                : translation;
        }

        private static Dictionary<string, string> GetLocalizationDictionary(string culture)
        {
            var doc = XDocument.Load($"https://raw.githubusercontent.com/AnalogJ/Wix3.6Toolset/master/RC0-source/wix36-sources/src/ext/UIExtension/wixlib/WixUI_{culture}.wxl");
            var ns = doc.Root.Name.Namespace;

            return doc.Root.Elements(ns + "String").ToDictionary(x => x.Attribute("Id").Value, x => x.Value);
        }
    }
}
