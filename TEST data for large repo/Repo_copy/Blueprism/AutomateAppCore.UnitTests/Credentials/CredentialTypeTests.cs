using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System.Linq;
using System;

namespace AutomateAppCore.UnitTests.Credentials
{
    [TestFixture]
    public class CredentialTypeTests
    {
        [Test]
        public void GetAll_HasAllTypes()
        {
            var all = CredentialType.GetAll().ToList();

            Assert.That(all.Count, Is.EqualTo(6));
            Assert.That(all[0], Is.EqualTo(CredentialType.General));
            Assert.That(all[1], Is.EqualTo(CredentialType.BasicAuthentication));
            Assert.That(all[2], Is.EqualTo(CredentialType.OAuth2ClientCredentials));
            Assert.That(all[3], Is.EqualTo(CredentialType.OAuth2JwtBearerToken));
            Assert.That(all[4], Is.EqualTo(CredentialType.BearerToken));
            Assert.That(all[5], Is.EqualTo(CredentialType.DataGatewayCredentials));
        }

        [Test]
        public void GeneralCredentialType_ConfiguredCorrectly()
        {
            var sut = CredentialType.General;

            Assert.That(sut.Name, Is.EqualTo("General"));
            Assert.That(sut.IsUsernameVisible, Is.EqualTo(true));
            Assert.That(sut.LocalisedTitle, Is.EqualTo(CredentialsResources.CredentialTypes_General_Title));
            Assert.That(sut.LocalisedUsernamePropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_General_UsernamePropertyTitle));
            Assert.That(sut.LocalisedPasswordPropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_General_PasswordPropertyTitle));
            Assert.That(sut.LocalisedDescription, Is.EqualTo(CredentialsResources.CredentialTypes_General_Description));
        }

        [Test]
        public void BasicAuthenticationCredentialType_ConfiguredCorrectly()
        {
            var sut = CredentialType.BasicAuthentication;

            Assert.That(sut.Name, Is.EqualTo("BasicAuthentication"));
            Assert.That(sut.IsUsernameVisible, Is.EqualTo(true));
            Assert.That(sut.LocalisedTitle, Is.EqualTo(CredentialsResources.CredentialTypes_BasicAuthentication_Title));
            Assert.That(sut.LocalisedUsernamePropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_BasicAuthentication_UsernamePropertyTitle));
            Assert.That(sut.LocalisedPasswordPropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_BasicAuthentication_PasswordPropertyTitle));
            Assert.That(sut.LocalisedDescription, Is.EqualTo(CredentialsResources.CredentialTypes_BasicAuthentication_Description));
        }

        [Test]
        public void OAuth2ClientCredentialsCredentialType_ConfiguredCorrectly()
        {
            var sut = CredentialType.OAuth2ClientCredentials;

            Assert.That(sut.Name, Is.EqualTo("OAuth2ClientCredentials"));
            Assert.That(sut.IsUsernameVisible, Is.EqualTo(true));
            Assert.That(sut.LocalisedTitle, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2ClientCredentials_Title));
            Assert.That(sut.LocalisedUsernamePropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2ClientCredentials_UsernamePropertyTitle));
            Assert.That(sut.LocalisedPasswordPropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2ClientCredentials_PasswordPropertyTitle));
            Assert.That(sut.LocalisedDescription, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2ClientCredentials_Description));
        }

        [Test]
        public void OAuth2JwtBearerTokenCredentialType_ConfiguredCorrectly()
        {
            var sut = CredentialType.OAuth2JwtBearerToken;

            Assert.That(sut.Name, Is.EqualTo("OAuth2JwtBearerToken"));
            Assert.That(sut.IsUsernameVisible, Is.EqualTo(true));
            Assert.That(sut.LocalisedTitle, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2JwtBearerToken_Title));
            Assert.That(sut.LocalisedUsernamePropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2JwtBearerToken_UsernamePropertyTitle));
            Assert.That(sut.LocalisedPasswordPropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2JwtBearerToken_PasswordPropertyTitle));
            Assert.That(sut.LocalisedDescription, Is.EqualTo(CredentialsResources.CredentialTypes_OAuth2JwtBearerToken_Description));
        }

        [Test]
        public void BearerTokenCredentialType_ConfiguredCorrectly()
        {
            var sut = CredentialType.BearerToken;

            Assert.That(sut.Name, Is.EqualTo("BearerToken"));
            Assert.That(sut.IsUsernameVisible, Is.EqualTo(false));
            Assert.That(sut.LocalisedTitle, Is.EqualTo(CredentialsResources.CredentialTypes_BearerToken_Title));
            Assert.That(sut.LocalisedUsernamePropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_BearerToken_UsernamePropertyTitle));
            Assert.That(sut.LocalisedPasswordPropertyTitle, Is.EqualTo(CredentialsResources.CredentialTypes_BearerToken_PasswordPropertyTitle));
            Assert.That(sut.LocalisedDescription, Is.EqualTo(CredentialsResources.CredentialTypes_BearerToken_Description));
        }

        [Test]
        public void GetByName_General_ReturnsCorrectly()
        {
            Assert.That(CredentialType.GetByName("General"), Is.EqualTo(CredentialType.General));
        }

        [Test]
        public void GetByName_BasicAuthentication_ReturnsCorrectly()
        {
            Assert.That(CredentialType.GetByName("BasicAuthentication"), Is.EqualTo(CredentialType.BasicAuthentication));
        }

        [Test]
        public void GetByName_OAuth2ClientCredentials_ReturnsCorrectly()
        {
            Assert.That(CredentialType.GetByName("OAuth2ClientCredentials"), Is.EqualTo(CredentialType.OAuth2ClientCredentials));
        }

        [Test]
        public void GetByName_OAuth2JwtBearerToken_ReturnsCorrectly()
        {
            Assert.That(CredentialType.GetByName("OAuth2JwtBearerToken"), Is.EqualTo(CredentialType.OAuth2JwtBearerToken));
        }

        [Test]
        public void GetByName_BearerToken_ReturnsCorrectly()
        {
            Assert.That(CredentialType.GetByName("BearerToken"), Is.EqualTo(CredentialType.BearerToken));
        }

        [Test]
        public void GetByName_Unknown_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => CredentialType.GetByName("unknown"));
        }

        [Test]
        public void GetByName_Nothing_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => CredentialType.GetByName(null));
        }
    }
}
