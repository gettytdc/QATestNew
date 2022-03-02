#if UNITTESTS
namespace LogPlugins.UnitTests
{
    using System;
    using System.Text;

    using BluePrism.Core.Plugins;
    using BluePrism.Core.Utility;
    using BluePrism.Utilities.Testing;
    using BluePrism.Utilities.Functional;
    using NUnit.Framework;

    [TestFixture]
    public class SplunkBasicHttpAuthenticationRequestFactoryTests : UnitTestBase<SplunkBasicHttpAuthenticationRequestFactory>
    {
        [Test]
        public void TestConfigurationIsSuitableWhenSuitable()
        {
            var configurationMock = GetMock<IConfiguration>();

            configurationMock
                .Setup(m => m.Elements)
                .Returns(new[]
                {
                    new TestConfigElement("Username", "Test"),
                    new TestConfigElement("Password", "Test")
                });

            var result = SplunkBasicHttpAuthenticationRequestFactory.ConfigurationIsSuitable(configurationMock.Object);

            Assert.IsTrue(result);
        }

        [Test]
        public void TestConfigurationIsSuitableWhenNotSuitable()
        {
            var configurationMock = GetMock<IConfiguration>();

            configurationMock
                .Setup(m => m.Elements)
                .Returns(new[]
                {
                    new TestConfigElement("OtherSetting", "Test"),
                });

            var result = SplunkBasicHttpAuthenticationRequestFactory.ConfigurationIsSuitable(configurationMock.Object);

            Assert.IsFalse(result);
        }

        [Test]
        public void TestGetRequestForUriSetsCorrectContentType()
        {
            var result = ClassUnderTest.GetRequestForUri(new System.Uri("http://example.com"));

            Assert.AreEqual("application/json", result.ContentType);
        }

        [Test]
        public void TestGetRequestForUriSetsCorrectMethod()
        {
            var result = ClassUnderTest.GetRequestForUri(new System.Uri("http://example.com"));

            Assert.AreEqual("POST", result.Method);
        }

        [Test]
        public void TestGetRequestForUriAddsCorrectAuthorizationHeader()
        {
            GetMock<IConfiguration>()
                .Setup(m => m.Elements)
                .Returns(new[]
                {
                    new TestConfigElement("Username", "TestUser".ToSecureString()),
                    new TestConfigElement("Password", "TestPass".ToSecureString())
                });

            var result = ClassUnderTest.GetRequestForUri(new System.Uri("http://example.com"));

            var expectedHeader =
                "TestUser:TestPass"
                .Map(Encoding.UTF8.GetBytes)
                .Map(Convert.ToBase64String)
                .Map(x => $"Basic {x}");

            Assert.AreEqual(expectedHeader, result.Headers["Authorization"]);
        }
    }
}
#endif
