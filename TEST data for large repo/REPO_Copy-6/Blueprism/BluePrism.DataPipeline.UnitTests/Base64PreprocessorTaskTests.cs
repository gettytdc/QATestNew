#if UNITTESTS

using BluePrism.Common.Security;
using BluePrism.Datapipeline.Logstash.Configuration;
using BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks;
using Moq;
using NUnit.Framework;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class Base64PreprocessorTaskTests
    {
        private Base64PreprocessorTask _classUnderTest;
        private Mock<ILogstashSecretStore> _mockLogstashStore;

        [SetUp]
        public void Setup()
        {
            _mockLogstashStore = new Mock<ILogstashSecretStore>();
            int i = 0;
            _mockLogstashStore.Setup(x => x.AddSecret(It.IsAny<SafeString>())).Returns($"$KEY{{{i++}}}");

            _classUnderTest = new Base64PreprocessorTask(_mockLogstashStore.Object);
        }

        [Test]
        public void Base64PreprocessorTask_ProcessConfiguration_OneBase64Tag_ValidConfiguration()
        {
            string configString = "headers => {\"Authorization\" => \"Basic <base64>username:password</base64>\"}";
            string expected = "headers => {\"Authorization\" => \"Basic dXNlcm5hbWU6cGFzc3dvcmQ=\"}";
            string actual = _classUnderTest.ProcessConfiguration(configString);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Base64PreprocessorTask_ProcessConfiguration_TwoBase64TagsSpecialChars_ValidConfiguration()
        {
            string configString = "headers => {\"Authorization\" => \"Basic <base64>username:password</base64>\"} headers => {\"Authorization\" => \"Basic <base64>anotherU$3rn&m3:an0th£rP&$$w0rd</base64>\"}";
            string expected = "headers => {\"Authorization\" => \"Basic dXNlcm5hbWU6cGFzc3dvcmQ=\"} headers => {\"Authorization\" => \"Basic YW5vdGhlclUkM3JuJm0zOmFuMHRowqNyUCYkJHcwcmQ=\"}";
            string actual = _classUnderTest.ProcessConfiguration(configString);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Base64PreprocessorTask_ProcessConfiguration_NoBase64Tags_ValidConfiguration()
        {
            string configString = "headers => {\"Authorization\" => \"Basic username:password\"}";
            string expected = "headers => {\"Authorization\" => \"Basic username:password\"}";
            string actual = _classUnderTest.ProcessConfiguration(configString);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Base64PreprocessorTask_ProcessConfiguration_OneBase64Tag_ConstainsSecret_ValidConfiguration()
        {
            _mockLogstashStore.Setup(x => x.AddSecret(It.IsAny<SafeString>())).Returns("${KEY1}");
            _mockLogstashStore.Setup(x => x.GetSecret("${KEY0}")).Returns(new SafeString("password123"));

            string configString = "headers => {\"Authorization\" => \"Basic <base64>my password is ${KEY0}</base64>\"}";
            string expected = "headers => {\"Authorization\" => \"Basic ${KEY1}\"}";
            string actual = _classUnderTest.ProcessConfiguration(configString);


            Assert.AreEqual(expected, actual);
            _mockLogstashStore.Verify(x => x.AddSecret(new SafeString("bXkgcGFzc3dvcmQgaXMgcGFzc3dvcmQxMjM=")));
        }
    }
}

#endif