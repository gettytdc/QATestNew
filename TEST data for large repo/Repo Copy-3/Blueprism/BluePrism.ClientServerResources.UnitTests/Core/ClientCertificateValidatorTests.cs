using System.Security;
using System.Security.Cryptography.X509Certificates;
using BluePrism.Core;
using Moq;
using NUnit.Framework;

namespace BluePrism.ClientServerResources.UnitTests.Core
{
    [TestFixture(Ignore = "Issues with mocking x509Certificate2")]
    public class ClientCertificateValidatorTests
    {
        private TestX509Certificate2 CreateMockedCertificate(string thumbprint)
        {
            var mock = new Mock<TestX509Certificate2>();
            mock.Setup(x => x.Thumbprint)
                .Returns(thumbprint);
            return mock.Object;
        }

        [Ignore("Issues with mocking x509Certificate2")]
        [Test]
        [TestCase("F7353FC26D1A5229142665C4EBEF9DE438B3CE9C")]
        [TestCase("f7353fc26d1a5229142665c4ebef9de438b3ce9c")]
        [TestCase("767a6c1f975a5f008240ef6e75e7b79a64aec2a7")]
        public void Valid(string thumbprint)
        {
            var mockCert = CreateMockedCertificate(thumbprint);
            var mockCert2 = CreateMockedCertificate(thumbprint);

            var validator = new TrustedCertificateValidator(new[] { mockCert });

            Assert.DoesNotThrow(() => validator.Validate(mockCert2));
        }

        [Ignore("Issues with mocking x509Certificate2")]
        [Test]
        [TestCase("F7353FC26D1A5229142665C4EBEF9DE438B3CE9C")]
        [TestCase("f7353fc26d1a5229142665c4ebef9de438b3ce9c")]
        [TestCase("767a6c1f975a5f008240ef6e75e7b79a64aec2a7")]
        public void Invalid(string thumbprint)
        {
            var mockCert = CreateMockedCertificate(thumbprint);
            var mockCert2 = CreateMockedCertificate("these are not the certificates you're looking for");

            var validator = new TrustedCertificateValidator(new[] { mockCert });

            Assert.Throws<SecurityException>(() => validator.Validate(mockCert2));
        }
    }
    public class TestX509Certificate2 : X509Certificate2
    {
        public virtual new string Thumbprint { get; set; }
    }


}
