#if UNITTESTS

using BluePrism.AutomateProcessCore.WebApis.Credentials;
using BluePrism.Common.Security;
using BluePrism.Datapipeline.Logstash;
using BluePrism.Datapipeline.Logstash.Configuration;
using BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class CredentialPreprocessorTaskTests
    {

        private Mock<ICredentialService> _mockCredentialService;
        private CredentialPreprocessorTask _classUnderTest;
        private Mock<ILogstashSecretStore> _mockLogstashStore;

        [SetUp]
        public void Setup()
        {

            _mockCredentialService = new Mock<ICredentialService>();

            List<ICredential> credentials = new List<ICredential>() { TestCredential.Barry, TestCredential.Frank, TestCredential.BobWithSecret };

            _mockCredentialService.Setup(x => x.GetAllCredentialsInfo()).Returns(credentials.Select(x => x.Name).ToList());
            _mockCredentialService.Setup(x => x.GetCredential(credentials[0].Name)).Returns(credentials[0]);
            _mockCredentialService.Setup(x => x.GetCredential(credentials[1].Name)).Returns(credentials[1]);
            _mockCredentialService.Setup(x => x.GetCredential(credentials[2].Name)).Returns(credentials[2]);

            _mockLogstashStore = new Mock<ILogstashSecretStore>();

            int i = 0;
            _mockLogstashStore.Setup(x => x.AddSecret(It.IsAny<SafeString>())).Returns($"${{KEY{i++}}}");

            _classUnderTest = new CredentialPreprocessorTask(_mockCredentialService.Object, _mockLogstashStore.Object);
        }


        [Test]
        
        public void CredentialPreprocessorTask_CredentialUsernameAndPasswordParamters_ReplacedWithValuesFromCredential()
        {
            var configString = "input { stdin { username: <%Frank.username%> password: <%Frank.password%>} } output { stdout{} }";

            var processedConfig = _classUnderTest.ProcessConfiguration(configString);

            var expected = "input { stdin { username: FrankM password: ${KEY0}} } output { stdout{} }";
            Assert.AreEqual(expected, processedConfig);
            _mockLogstashStore.Verify(x => x.AddSecret(It.IsAny<SafeString>()));
        }

        [Test]
        public void CredentialPreprocessorTask_CredentialDoesntExist_ApplicationException()
        {
            var configString = "input { stdin { username: <%doesntexist.username%> password: <%Frank.password%>} } output { stdout{} }";

            var exception = Assert.Throws<InvalidOperationException>(() => _classUnderTest.ProcessConfiguration(configString));
            Assert.AreEqual("Credential not found: doesntexist", exception.Message);

        }


        [Test]
        public void CredentialPreprocessorTask_CredentialPropertyParameter_ReplacedWithValuesFromCredential()
        {
            var configString = "input { stdin { } } output { stdout{ token: <%Bob.Secret%>} }";

            var config = _classUnderTest.ProcessConfiguration(configString);
            var expected = "input { stdin { } } output { stdout{ token: ${KEY0}} }";
            Assert.AreEqual(expected, config);

        }


        [Test]
        public void CredentialPreprocessorTask_CredentialProperyDoesntExist_ApplicationException()
        {
            var configString = "input { stdin { } } output { stdout{ token: <%Bob.fakeProp%>} }";

            var exception = Assert.Throws<InvalidOperationException>(() => _classUnderTest.ProcessConfiguration(configString));
            Assert.AreEqual("Unable to find credential property: Bob.fakeProp", exception.Message);

        }


    }

    internal class TestCredential : ICredential
    {
        public static readonly TestCredential Frank = new TestCredential("Frank", "FrankM", "secureaf");

        public static readonly TestCredential Barry = new TestCredential("Barry", "BarryS", "b4rryru13z");

        public static readonly TestCredential BobWithSecret = CreateBobWithSecret();

        private static TestCredential CreateBobWithSecret()
        {
            var bob = new TestCredential("Bob", "bob", "secret");
            bob.Properties.Add("Secret", new SafeString("theSecret"));
            return bob;
        }

        public TestCredential(string name, string username, string password)
        {
            Name = name;
            Username = username;
            Password = new SafeString(password);
            Properties = new Dictionary<string, SafeString>();
        }
            
        public string Name { get; }
        public string Username { get; }
        public SafeString Password { get; }

        public IDictionary<string, SafeString> Properties { get; }
    }
}

#endif