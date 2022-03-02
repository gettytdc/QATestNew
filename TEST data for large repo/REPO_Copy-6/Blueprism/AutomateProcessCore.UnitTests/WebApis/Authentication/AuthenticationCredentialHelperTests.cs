#if UNITTESTS
using System;
using System.Collections.Generic;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.UnitTests.WebApis;
using BluePrism.AutomateProcessCore.WebApis.Authentication;
using BluePrism.AutomateProcessCore.WebApis.Credentials;
using BluePrism.Server.Domain.Models;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.Authentication
{
    public class AuthenticationCredentialHelperTests
    {
        private ICredentialStore _credentialStoreMock;
        private readonly Guid _sessionId = new Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722");
        private const string CredentialName = "Credential Name";
        private const string DefaultCredential = "Default Credential";

        [SetUp]
        public void SetUp()
        {
            var credentialStoreMock = new Mock<ICredentialStore>();
            credentialStoreMock.Setup(s => s.GetCredential("Frank Credential", _sessionId)).Returns(TestCredential.Frank);
            credentialStoreMock.Setup(s => s.GetCredential("Barry Credential", _sessionId)).Returns(TestCredential.Barry);
            credentialStoreMock.Setup(s => s.GetCredential("Dave Credential", _sessionId)).Returns((ICredential)null);
            _credentialStoreMock = credentialStoreMock.Object;
        }

        [Test]
        public void GetCredential_CredentialNotFound_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("Dave Credential", false,string.Empty);
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredential(credential, new Dictionary<string, clsProcessValue>(), _sessionId));
        }

        [Test]
        public void GetCredential_SessionIDIsEmptyGuid_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("Frank Credential", false, string.Empty);
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredential(credential, new Dictionary<string, clsProcessValue>(), Guid.Empty));
        }

        [Test]
        public void GetCredential_IncorrectSessionId_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("Frank Credential", false, string.Empty);
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredential(credential, new Dictionary<string, clsProcessValue>(), Guid.NewGuid()));
        }

        [Test]
        public void GetCredential_CredentialInStoreAndCredentialNotExposedToProcess_ShouldReturnCredential()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("Frank Credential", false, string.Empty);
            Assert.That(classUnderTest.GetCredential(credential, new Dictionary<string, clsProcessValue>(), _sessionId), Is.EqualTo(TestCredential.Frank));
        }

        [Test]
        public void GetCredential_CredentialInStoreAndCredentialExposedToProcess_ShouldReturnCredential()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("Frank Credential", true, "Credential Override");
            var actionParameters = new Dictionary<string, clsProcessValue>
            {
                { "Credential Override", new clsProcessValue("Barry Credential") }
            };
            Assert.That(classUnderTest.GetCredential(credential, actionParameters, _sessionId), Is.EqualTo(TestCredential.Barry));
        }

        [Test]
        public void GetCredentialName_NotExposeToProcessAndEmptyCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(string.Empty, false, CredentialName);
            var emptyParameters = new Dictionary<string, clsProcessValue>();
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, emptyParameters));
        }

        [Test]
        public void GetCredentialName_NotExposeToProcessAndWhiteSpaceCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("  ", false, CredentialName);
            var emptyParameters = new Dictionary<string, clsProcessValue>();
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, emptyParameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndMissingParameterAndEmptyCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(string.Empty, true, CredentialName);
            var emptyParameters = new Dictionary<string, clsProcessValue>();
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, emptyParameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndMissingParameterAndWhiteSpaceCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential("  ", true, CredentialName);
            var emptyParameters = new Dictionary<string, clsProcessValue>();
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, emptyParameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterValueIsNullAndEmptyCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(string.Empty, true, CredentialName);
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, null }
            };
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, parameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterValueIsEmptyAndEmptyCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(string.Empty, true, CredentialName);
            var parameters = new Dictionary<string, clsProcessValue> {{ CredentialName, new clsProcessValue(string.Empty)}};
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, parameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterValueIsWhiteSpaceAndEmptyCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(string.Empty, true, CredentialName);
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, new clsProcessValue(" ") }
            };
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, parameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterValueIsNullAndWhitespaceCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(" ", true, CredentialName);
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, null }
            };
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, parameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterValueIsEmptyAndWhitespaceCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(" ", true, CredentialName);
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, new clsProcessValue(string.Empty) }
            };
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, parameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterValueIsWhiteSpaceAndWhitespaceCredentialName_ShouldThrowException()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(" ", true, "Credential Name");
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, new clsProcessValue(" ") }
            };
            Assert.Throws<MissingItemException>(() => classUnderTest.GetCredentialName(credential, parameters));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterHasValidValue_ShouldReturnParameterValue()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(DefaultCredential, true, "Credential Name");
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, new clsProcessValue("Some other value") }
            };
            var credentialName = classUnderTest.GetCredentialName(credential, parameters);
            Assert.That(credentialName, Is.EqualTo("Some other value"));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterHasNullValue_ShouldReturnCredentialName()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(DefaultCredential, true, "Credential Name");
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, null }
            };
            var credentialName = classUnderTest.GetCredentialName(credential, parameters);
            Assert.That(credentialName, Is.EqualTo(DefaultCredential));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterHasEmptyValue_ShouldReturnCredentialName()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(DefaultCredential, true, "Credential Name");
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, new clsProcessValue(string.Empty) }
            };
            var credentialName = classUnderTest.GetCredentialName(credential, parameters);
            Assert.That(credentialName, Is.EqualTo(DefaultCredential));
        }

        [Test]
        public void GetCredentialName_ExposeToProcessAndParameterHasWhitespaceValue_ShouldReturnCredentialName()
        {
            var classUnderTest = new AuthenticationCredentialHelper(_credentialStoreMock);
            var credential = new AuthenticationCredential(DefaultCredential, true, "Credential Name");
            var parameters = new Dictionary<string, clsProcessValue>
            {
                { CredentialName, new clsProcessValue(" ") }
            };
            var credentialName = classUnderTest.GetCredentialName(credential, parameters);
            Assert.That(credentialName, Is.EqualTo(DefaultCredential));
        }
    }
}
#endif
