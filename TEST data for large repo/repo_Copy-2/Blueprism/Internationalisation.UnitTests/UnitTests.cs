using System.Globalization;

namespace BluePrism.UnitTests
{
    using System;
    using NUnit.Framework;
    using LocaleTools;

    [TestFixture]
    public class LocaleToolsTests
    {
        // This test implicitly tests that resx files for all "prefixes" are set up correctly.
        // It's using pseudo values, which could change if original values are updated -
        // slightly annoying.
        [TestCase("description_The new value", "actions", "gsw-LI",
            ExpectedResult = @"ˁThe neẁ ṽalueˀ")]
        [TestCase("helptext_A label", "elements", "gsw-LI",
            ExpectedResult = @"ˁA laḃelˀ")]
        [TestCase("August Bank Holiday", "holiday", "gsw-LI",
            ExpectedResult = @"ˁAuḡuṡt Banḵ Holḭḍaẏˀ")]
        [TestCase("Developers", "misc", "gsw-LI",
            ExpectedResult = @"ˁDeṽeloperṡˀ")]
        [TestCase("group_Control Room", "roleperms", "gsw-LI",
            ExpectedResult = @"ˁControl Rooṁˀ")]
        [TestCase("Concurrent Sessions", "tile", "gsw-LI",
            ExpectedResult = @"ˁConḉurrent Seṡṡḭonṡˀ")]
        [TestCase("type_Error", "validation", "gsw-LI",
            ExpectedResult = @"ˁErrorˀ")]
        [TestCase("AddTableRow", "vbo", "gsw-LI",
            ExpectedResult = @"ˁÂdḍ Taḃlé Roẁˀ")]
        public string GetByKeyPrefixAndLocale_WithExistingKeyUsingSpecificLanguage_ReturnsTranslatedValue(string key, string prefix, string locale)
        {
            string result = LTools.Get(key, prefix, locale);
            return result;
        }
        
        [TestCase("ja-JP")]
        [TestCase("gsw-LI")]
        public void GetByKeyPrefixAndLocale_LanguageWithSpecificResx_ReturnsTranslatedValue(string locale)
        {
            const string original = "Developers";
            string result = LTools.Get(original, "misc", locale);
            Assert.AreNotEqual(original, result);
        }

        [TestCase("zh-CN")]
        [TestCase("zh-SG")]
        [TestCase("zh-Hans-HK")]
        public void GetByKeyPrefixAndLocale_LanguagesWithFallbackResx_ReturnsTranslatedValue(string locale)
        {
            const string original = "Developers";
            string result = LTools.Get(original, "misc", locale);
            Assert.AreNotEqual(original, result);
        }

        [Test]
        public void GetByKeyPrefixAndLocale_WithUnsupportedLanguage_ReturnsOriginalValue()
        {
            const string original = "Developers";
            string result = LTools.Get(original, "misc", "ar-lb");
            Assert.AreEqual(original, result);
        }

        [TestCase("The new value", "actions", "gsw-LI", "description",
            ExpectedResult = @"ˁThe neẁ ṽalueˀ")]
        [TestCase("A label", "elements", "gsw-LI", "helptext",
            ExpectedResult = @"ˁA laḃelˀ")]
        [TestCase("Blue Prism Expression", "misc", "gsw-LI", "code_language",
            ExpectedResult = @"ˁBlue Prḭṡṁ Eẋpreṡṡḭonˀ")]
        [TestCase("Control Room", "roleperms", "gsw-LI", "group",
            ExpectedResult = @"ˁControl Rooṁˀ")]
        [TestCase("Queue Name", "tile", "gsw-LI", "param",
            ExpectedResult = @"ˁ@QùeùeNaṁeˀ")]
        [TestCase("Error", "validation", "gsw-LI", "type",
            ExpectedResult = @"ˁErrorˀ")]
        public string GetByKeyPrefixLocaleAndIdPrefix_WithExistingKeyUsingSpecificLanguage_ReturnsTranslatedValue
            (string key, string prefix, string locale, string id_prefix)
        {
            string result = LTools.Get(key, prefix, locale, id_prefix);
            return result;
        }

        [Test]
        public void GetByKeyPrefixAndLocale_WithNullLocale_ReturnsTranslatedValueUsingCurrentUICulture()
        {
            using (new TempUICulture("gsw-LI"))
            {
                string result = LTools.Get("ConcurrentSessions", "tile", null);
                Assert.AreEqual(@"ˁConḉurrent Seṡṡḭonṡˀ", result);
            }
        }

        [Test]
        public void GetCByKeyAndPrefix_ReturnsTranslatedValueUsingCurrentUICulture()
        {
            using (new TempUICulture("gsw-LI"))
            {
                string result = LTools.GetC("ConcurrentSessions", "tile");
                Assert.AreEqual(@"ˁConḉurrent Seṡṡḭonṡˀ", result);
            }
        }

        [Test]
        public void GetCByKeyPrefixAndIdPrefix_ReturnsTranslatedValueUsingCurrentUICulture()
        {
            using (new TempUICulture("gsw-LI"))
            {
                string result = LTools.GetC("Error", "validation", "type");
                Assert.AreEqual(@"ˁErrorˀ", result);
            }
        }

        private class TempUICulture : IDisposable
        {
            private CultureInfo previous = CultureInfo.CurrentUICulture;

            public TempUICulture(string cultureName) : this(new CultureInfo(cultureName))
            {

            }

            public TempUICulture(CultureInfo culture)
            {
                CultureInfo.CurrentUICulture = culture;
            }

            public void Dispose()
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }

    }
}
