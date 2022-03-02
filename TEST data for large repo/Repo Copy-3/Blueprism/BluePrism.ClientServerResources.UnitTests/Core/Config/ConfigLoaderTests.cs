using System.Xml;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.ClientServerResources.Core.Enums;
using NUnit.Framework;

namespace BluePrism.ClientServerResources.Core.UnitTests.Config
{
    [TestFixture]
    public class ConfigLoaderTests
    {
        [Test]
        public void Save_Load()
        {
            var originalConfig = new ConnectionConfig
            {
                CallbackProtocol = CallbackConnectionProtocol.Grpc,
                HostName = "123.123.123.123",
                Port = 1001,
                Mode = InstructionalConnectionModes.Certificate,
                ClientStore = System.Security.Cryptography.X509Certificates.StoreName.AddressBook,
                ServerStore = System.Security.Cryptography.X509Certificates.StoreName.CertificateAuthority,
                CertificateName = "MyCertificate123",
                ClientCertificateName = "MyClientCertificate123"
            };

            var xml = ConfigLoader.SaveXML(originalConfig, new XmlDocument());

            var loadedConfig = ConfigLoader.LoadXML(xml);

            Assert.AreEqual(loadedConfig.CallbackProtocol,      originalConfig.CallbackProtocol);
            Assert.AreEqual(loadedConfig.HostName,              originalConfig.HostName);
            Assert.AreEqual(loadedConfig.Port,                  originalConfig.Port);
            Assert.AreEqual(loadedConfig.Mode,                  originalConfig.Mode);
            Assert.AreEqual(loadedConfig.CertificateName,       originalConfig.CertificateName);
            Assert.AreEqual(loadedConfig.ClientCertificateName, originalConfig.ClientCertificateName);
            Assert.AreEqual(loadedConfig.ServerStore,           originalConfig.ServerStore);
            Assert.AreEqual(loadedConfig.ClientStore,           originalConfig.ClientStore);
        }
    }
}
