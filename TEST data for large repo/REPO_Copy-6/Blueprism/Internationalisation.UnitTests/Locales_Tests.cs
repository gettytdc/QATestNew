using FluentAssertions;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;

namespace Internationalisation.UnitTests
{
    [TestFixture]
    public class Locales_Tests
    {
        [Test]
        public void Locales_CulturesAreValid()
        {
            var cultures = Locales.SupportedLocales;
            foreach (var culture in cultures)
            {
                Action actNewCultureInfo = () => new CultureInfo(culture);
                actNewCultureInfo.ShouldNotThrow<CultureNotFoundException>(because: "Cultures should be supported by System.Globalization");
            }
        }

        [Test]
        public void Locales_IsUnicode()
        {
            var localeList = Locales.LocaleList;
            var cultures = localeList.Select(x => (x.Key, x.Value.isUnicode));
            foreach (var (culture, isUnicode) in cultures)
            {
                var cultureInfo = new CultureInfo(culture);

                var requiresUnicode = Locales.IsUnicode(cultureInfo.NativeName);

                Assert.AreEqual(isUnicode, requiresUnicode);
            }
        }
    }
}
