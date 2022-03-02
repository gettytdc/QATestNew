using BluePrism.Common.Security;
using BluePrism.Core.Encryption;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Cryptography.X509Certificates;

namespace BluePrism.Core.UnitTests.Encryption
{
    [TestFixture]
    public class CertificateServicesTests
    {
        private CertificateServices _certificateServices;
        private readonly Mock<ICertificateStoreService> _mockCertificateStoreService = new Mock<ICertificateStoreService>();
        private readonly Mock<X509Certificate2> _mockX509Certificate2 = new Mock<X509Certificate2>();

        [SetUp]
        public void Setup()
        {
            _mockCertificateStoreService.Setup(m => m.GetCertificate(It.IsAny<string>())).Returns(_mockX509Certificate2.Object);
            _certificateServices = new CertificateServices(_mockCertificateStoreService.Object);
        }

        [Test]
        public void CertificateExpiryDateTime_InvalidArgumentShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => _certificateServices.CertificateExpiryDateTime(string.Empty));
        }

        [Test]
        public void CertificateExpiryDateTime_ParaseValidCertificate()
        {
            var currentDate = DateTime.UtcNow;
            _mockX509Certificate2.Setup(m => m.GetExpirationDateString()).Returns(currentDate.ToString());
            var result = _certificateServices.CertificateExpiryDateTime("9310c739449ac7351c5149943630bac477898d77");
            Assert.AreEqual(result.ToString(), currentDate.ToString());
        }

    }
}
