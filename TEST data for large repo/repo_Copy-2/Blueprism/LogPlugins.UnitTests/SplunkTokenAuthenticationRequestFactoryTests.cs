#if UNITTESTS
namespace LogPlugins.UnitTests
{
    using BluePrism.Core.Plugins;
    using BluePrism.Core.Utility;
    using BluePrism.Utilities.Testing;
    using NUnit.Framework;

    [TestFixture]
    public class SplunkTokenAuthenticationRequestFactoryTests : UnitTestBase<SplunkTokenAuthenticationRequestFactory>
    {
        [Test]
        public void TestConfigurationIsSuitableWhenSuitable()
        {
            var configurationMock = GetMock<IConfiguration>();

            configurationMock
                .Setup(m => m.Elements)
                .Returns(new[]
                {
                    new TestConfigElement("Token", "Test"),
                });

            var result = SplunkTokenAuthenticationRequestFactory.ConfigurationIsSuitable(configurationMock.Object);

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

            var result = SplunkTokenAuthenticationRequestFactory.ConfigurationIsSuitable(configurationMock.Object);

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
                    new TestConfigElement("Token", "1234567890ABCDEF".ToSecureString()),
                    new TestConfigElement("Password", "TestPass".ToSecureString())
                });

            var result = ClassUnderTest.GetRequestForUri(new System.Uri("http://example.com"));

            var expectedHeader = "Splunk 1234567890ABCDEF";

            Assert.AreEqual(expectedHeader, result.Headers["Authorization"]);
        }
    }
}
#endif