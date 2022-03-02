using BluePrism.AutomateAppCore.Config;
using BluePrism.Core.Utility;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateUI.UnitTests.Classes
{
    public static class BrandingTests
    {
        [TestFixture]
        public class The_GetTitle_Method
        {
            private readonly Mock<IOptions> _optionsMock;
            private Branding_WithOverrides _branding;
            private readonly string _version;

            public The_GetTitle_Method()
            {
                _optionsMock = new Mock<IOptions>();
                _version = this.GetBluePrismVersion(fieldCount: 3);
            }

            [SetUp]
            public void InitialiseBranding_WithOverrides()
            {
                _branding = new Branding_WithOverrides(_optionsMock.Object);
            }

            [Test]
            public void TestDefaultTitleHasVersion()
            {
                _branding.DefaultTitle_Override = "Default title";

                var title = _branding.GetTitle();

                title.Should().Be($"{_branding.DefaultTitle_Override} - v{_version}");
            }

            [Test]
            public void TestLicenseOverrideHasVersion()
            {
                _branding.LicenseOverrideTitle_Override = "Override title from license";
                _branding.IsLicensed_Override = true;

                var title = _branding.GetTitle();

                title.Should().Be($"{_branding.LicenseOverrideTitle_Override} - v{_version}");
            }

            [Test]
            public void TestUnlicensedLastBrandingHasVersion()
            {
                _optionsMock.Setup(o => o.LastBrandingTitle).Returns("Title form unlicensed last branding");

                var title = _branding.GetTitle();

                title.Should().Be($"Title form unlicensed last branding - v{_version}");
            }

            [Test]
            public void TestUnbrandedHasVersion()
            {
                _optionsMock.Setup(o => o.Unbranded).Returns(true);
                _branding.UnbrandedTitle_Override = "Unbranded test title";

                var title = _branding.GetTitle();

                title.Should().Be($"{_branding.UnbrandedTitle_Override} - v{_version}");
            }
        }
    }
}
