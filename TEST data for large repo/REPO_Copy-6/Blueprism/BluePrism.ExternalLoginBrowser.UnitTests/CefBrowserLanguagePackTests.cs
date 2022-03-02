namespace BluePrism.ExternalLoginBrowser.UnitTests
{
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;

    [Category("Cef Browser Setup Tests")]
    public class CefBrowserLanguagePackTests
    {
        [Test]
        public void Setup_CefBrowserLanguagePacks_NoLanguagePackFilenameMustNotBeEmpty()
        {
            var langPacks = Internationalisation.Locales.CefLanguagePacks;
            langPacks.ForEach(pack => pack.Should().NotBeNullOrWhiteSpace());
        }

        [Test]
        public void Setup_CefBrowserLanguagePacks_MustHaveValidFileExtension()
        {
            var langPacks = Internationalisation.Locales.CefLanguagePacks;
            langPacks.ForEach(pack => Path.GetExtension(pack).Should().Be(".pak"));
        }

        [Test]
        public void Setup_CefBrowserLanguagePacks_ExpectedDeploymentPackagesMustBePresent()
        {
            var langPacks = Internationalisation.Locales.CefLanguagePacks;
            var expected = new[] { "en-US.pak", "fr.pak", "ja.pak", "zh-CN.pak", "de.pak", "es.pak" };

            langPacks.Count.Should().Be(expected.Length);
            foreach (string pack in expected)
            {
                langPacks.Should().Contain(pack);
            }
        }

        [Test]
        public void Setup_CefBrowserLanguagePacks_SetupLocalesShouldMapToLanguagePacks()
        {
            Internationalisation.Locales.SupportedLocales.ToList().Count.Should().Be(14);

            Internationalisation.Locales.GetCefLanguagePack("de-DE").Should().Be("de.pak");

            Internationalisation.Locales.GetCefLanguagePack("es-419").Should().Be("es.pak");

            Internationalisation.Locales.GetCefLanguagePack("en-GB").Should().Be("en-US.pak");
            Internationalisation.Locales.GetCefLanguagePack("en-US").Should().Be("en-US.pak");
            Internationalisation.Locales.GetCefLanguagePack("en-CA").Should().Be("en-US.pak");
            Internationalisation.Locales.GetCefLanguagePack("en-ZA").Should().Be("en-US.pak");
            Internationalisation.Locales.GetCefLanguagePack("en-001").Should().Be("en-US.pak");
            Internationalisation.Locales.GetCefLanguagePack("en-150").Should().Be("en-US.pak");

            Internationalisation.Locales.GetCefLanguagePack("fr-FR").Should().Be("fr.pak");
            Internationalisation.Locales.GetCefLanguagePack("ja-JP").Should().Be("ja.pak");

            Internationalisation.Locales.GetCefLanguagePack("zh-CN").Should().Be("zh-CN.pak");
            Internationalisation.Locales.GetCefLanguagePack("zh-SG").Should().Be("zh-CN.pak");
            Internationalisation.Locales.GetCefLanguagePack("zh-Hans-HK").Should().Be("zh-CN.pak");
            Internationalisation.Locales.GetCefLanguagePack("zh-Hans-MO").Should().Be("zh-CN.pak");
        }
    }
}

