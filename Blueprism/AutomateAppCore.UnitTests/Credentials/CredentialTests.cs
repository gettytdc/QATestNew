using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System;

namespace AutomateAppCore.UnitTests.Credentials
{
    [TestFixture]
    public class CredentialTests
    {
        [Test]
        public void NewCredential_HasTypeGeneral()
        {
            var sut = new clsCredential();

            Assert.That(sut.Type, Is.EqualTo(CredentialType.General));
        }

        [Test]
        public void SetType_DoesSetType()
        {
            var sut = new clsCredential
            {
                Type = CredentialType.BearerToken
            };

            Assert.That(sut.Type, Is.EqualTo(CredentialType.BearerToken));
        }

        [Test]
        public void SetType_SetToNothing_Throws()
        {
            var sut = new clsCredential();
            Assert.Throws<ArgumentNullException>(() => sut.Type = null);
        }
    }
}
