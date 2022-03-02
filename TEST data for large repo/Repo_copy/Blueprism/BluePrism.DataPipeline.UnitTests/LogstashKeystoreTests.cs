using BluePrism.Datapipeline.Logstash;
using BluePrism.Datapipeline.Logstash.Configuration;
using Moq;
using NUnit.Framework;
using System;
using BluePrism.Common.Security;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class LogstashKeystoreTests
    {
        [Test]
        public void AddSecret_ReturnsExpectedKey()
        {
            Mock<IProcessFactory> processFactory = new Mock<IProcessFactory>();
            LogstashSecretStore store = new LogstashSecretStore(processFactory.Object, "path/to/logstash/dir");
            string key = store.AddSecret(new SafeString("password123"));
            Assert.AreEqual("${KEY1}", key);
        }


        [Test]
        public void AddSecret_NullArgument_ThrowsNullArgumentException()
        {
            Mock<IProcessFactory> processFactory = new Mock<IProcessFactory>();
            LogstashSecretStore store = new LogstashSecretStore(processFactory.Object, "path/to/logstash/dir");

            Assert.Throws<ArgumentNullException>(() => store.AddSecret(null));
        }


        [Test]
        public void GetSecret_ReturnsExpectedValue()
        {
            Mock<IProcessFactory> processFactory = new Mock<IProcessFactory>();
            LogstashSecretStore store = new LogstashSecretStore(processFactory.Object, "path/to/logstash/dir");
            var secretToAdd = new SafeString("password123");
            string key = store.AddSecret(secretToAdd);

            var secret = store.GetSecret(key);

            Assert.AreEqual(secretToAdd, secret);
        }

        [Test]
        public void GetSecret_KeyNotInStore_ReturnsNull()
        {
            Mock<IProcessFactory> processFactory = new Mock<IProcessFactory>();
            LogstashSecretStore store = new LogstashSecretStore(processFactory.Object, "path/to/logstash/dir");

            var secret = store.GetSecret("${KEY1}");

            Assert.IsNull(secret);
        }

    }
}
