#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.Common.Security;
using BluePrism.UnitTesting;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateC.UnitTests
{

    /// <summary>
/// Tests various (unit-testable) credential-related aspects of AutomateC.
/// </summary>
    [TestFixture]
    public class AutomateCCredentialTests : AutomateCTestBase
    {
        private List<clsCredential> mExistingCredentials;
        private List<clsCredential> mCreatedCredentials;
        private Dictionary<string, clsCredential> mUpdatedCredentials;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            LegacyUnitTestHelper.SetupDependencyResolver();
            SetCurrentUser("testuser", new string[] { Permission.SystemManager.Security.Credentials });
            SetUpCredentialsOnServer();
        }

        private void SetUpCredentialsOnServer()
        {
            mExistingCredentials = new List<clsCredential>();
            mCreatedCredentials = new List<clsCredential>();
            mUpdatedCredentials = new Dictionary<string, clsCredential>();
            ServerMock.Setup(s => s.GetCredentialID(It.IsAny<string>())).Returns((string name) => mExistingCredentials.FirstOrDefault(c => (c.Name ?? "") == (name ?? ""))?.ID ?? Guid.Empty);
            ServerMock.Setup(s => s.GetCredentialIncludingLogon(It.IsAny<Guid>())).Returns((Guid id) => mExistingCredentials.FirstOrDefault(c => c.ID == id));
            ServerMock.Setup(s => s.CreateCredential(It.IsAny<clsCredential>())).Callback((clsCredential newCred) => mCreatedCredentials.Add(newCred));
            ServerMock.Setup(s => s.UpdateCredential(It.IsAny<clsCredential>(), It.IsAny<string>(), It.IsAny<ICollection<CredentialProperty>>(), It.IsAny<bool>())).Callback((clsCredential cred, string oldName, ICollection<CredentialProperty> props, bool passwordChanged) => mUpdatedCredentials.Add(oldName, cred));
        }

        [Test]
        public void CreateCredential_NoName_ActionFails()
        {
            int result = AutomateC.Run(new string[] { "/createcredential" });
            result.Should().Be(1);
            mCreatedCredentials.Count.Should().Be(0);
        }

        [Test]
        public void CreateCredential_SimpleCredential_AddsNewCredentialCorrectly()
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "test", "password" });
            result.Should().Be(0);
            mCreatedCredentials.Count.Should().Be(1);
            var createdCredential = mCreatedCredentials.First();
            createdCredential.Name.Should().Be("testCred");
            createdCredential.Username.Should().Be("test");
            createdCredential.Password.AsString().Should().Be("password");
            createdCredential.ExpiryDate.Should().Be(DateTime.MinValue);
            createdCredential.IsInvalid.Should().BeFalse();
            createdCredential.Description.Should().BeEmpty();
            createdCredential.Roles.Count.Should().Be(1);
            createdCredential.Roles.First().Should().BeNull();
            createdCredential.ProcessIDs.Count.Should().Be(1);
            createdCredential.ProcessIDs.First().Should().BeEmpty();
            createdCredential.ResourceIDs.Count.Should().Be(1);
            createdCredential.ResourceIDs.First().Should().BeEmpty();
            createdCredential.Type.Should().Be(CredentialType.General);
        }

        [Test]
        public void CreateCredential_SimpleCredentialWithoutUsernameOrPassword_AddsNewCredentialCorrectly()
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "", "" });
            result.Should().Be(0);
            mCreatedCredentials.Count.Should().Be(1);
            AssertCredentialSetUpCorrectly(mCreatedCredentials.First(), "testCred", "", "", CredentialType.General);
        }

        [Test]
        public void CreateCredential_SimpleCredentialWithoutUsername_AddsNewCredentialCorrectly()
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "", "password" });
            result.Should().Be(0);
            mCreatedCredentials.Count.Should().Be(1);
            AssertCredentialSetUpCorrectly(mCreatedCredentials.First(), "testCred", "", "password", CredentialType.General);
        }

        [Test]
        public void CreateCredential_SimpleCredentialWithoutPassword_AddsNewCredentialCorrectly()
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "fred", "" });
            result.Should().Be(0);
            mCreatedCredentials.Count.Should().Be(1);
            AssertCredentialSetUpCorrectly(mCreatedCredentials.First(), "testCred", "fred", "", CredentialType.General);
        }

        [Test]
        public void CreateCredential_CredentialWithoutPassword_WithSpecificType_AddsNewCredentialCorrectly()
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "fred", "", "/credentialtype", "BearerToken" });
            result.Should().Be(0);
            mCreatedCredentials.Count.Should().Be(1);
            AssertCredentialSetUpCorrectly(mCreatedCredentials.First(), "testCred", "fred", "", CredentialType.BearerToken);
        }

        [TestCase("General")]
        [TestCase("BasicAuthentication")]
        [TestCase("OAuth2ClientCredentials")]
        [TestCase("OAuth2JwtBearerToken")]
        [TestCase("BearerToken")]
        public void CreateCredential_SetAllAvailableOptions_AddsNewCredentialCorrectly(string credentialTypeName)
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "test", "password", "/description", "description", "/expirydate", "20190101", "/invalid", "true", "/credentialtype", credentialTypeName });
            result.Should().Be(0);
            mCreatedCredentials.Count.Should().Be(1);
            var createdCredential = mCreatedCredentials.First();
            AssertCredentialSetUpCorrectly(createdCredential, "testCred", "test", "password", CredentialType.GetByName(credentialTypeName));
            createdCredential.ExpiryDate.Should().Be(new DateTime(2019, 1, 1));
            createdCredential.IsInvalid.Should().BeTrue();
            createdCredential.Description.Should().Be("description");
            createdCredential.Roles.Count.Should().Be(1);
            createdCredential.Roles.First().Should().BeNull();
            createdCredential.ProcessIDs.Count.Should().Be(1);
            createdCredential.ProcessIDs.First().Should().BeEmpty();
            createdCredential.ResourceIDs.Count.Should().Be(1);
            createdCredential.ResourceIDs.First().Should().BeEmpty();
        }

        [Test]
        public void CreateCredential_InvalidTypeName_ActionFails()
        {
            int result = AutomateC.Run(new string[] { "/createcredential", "testCred", "test", "password", "/credentialType", "invalidType" });
            result.Should().Be(1);
            mCreatedCredentials.Count.Should().Be(0);
        }

        [Test]
        public void UpdateCredential_NoParams_ActionFails()
        {
            int result = AutomateC.Run(new string[] { "/updatecredential" });
            result.Should().Be(1);
            mUpdatedCredentials.Count.Should().Be(0);
        }

        [Test]
        public void UpdateCredential_CredentialDoesNotExist_ActionFails()
        {
            int result = AutomateC.Run(new string[] { "/updatecredential", "differentCred" });
            result.Should().Be(1);
            mUpdatedCredentials.Count.Should().Be(0);
        }

        [Test]
        public void UpdateCredential_CredentialExists_CorrectlyUpdatesCredential()
        {
            var existingCred = new clsCredential();
            existingCred.Name = "existingCred";
            existingCred.Username = "username";
            existingCred.Password = new SafeString("password");
            existingCred.ExpiryDate = new DateTime(2019, 1, 1);
            existingCred.IsInvalid = true;
            existingCred.Description = "description";
            existingCred.ID = Guid.NewGuid();
            existingCred.Type = CredentialType.BasicAuthentication;
            mExistingCredentials.Add(existingCred);
            int result = AutomateC.Run(new string[] { "/updatecredential", "existingCred", "/username", "newUsername", "/password", "newPassword", "/description", "newDescription", "/expirydate", "20190202", "/invalid", "false", "/credentialtype", "BearerToken" });
            result.Should().Be(0);
            mUpdatedCredentials.Count.Should().Be(1);
            mUpdatedCredentials.First().Key.Should().Be("existingCred");
            var updatedCredential = mUpdatedCredentials.First().Value;
            AssertCredentialSetUpCorrectly(updatedCredential, "existingCred", "newUsername", "newPassword", CredentialType.BearerToken);
            updatedCredential.ExpiryDate.Should().Be(new DateTime(2019, 2, 2));
            updatedCredential.IsInvalid.Should().BeFalse();
            updatedCredential.Description.Should().Be("newDescription");
        }

        [Test]
        public void UpdateCredential_CredentialTypeNotSpecified_DoesNotUpdateType()
        {
            var existingCred = new clsCredential();
            existingCred.Name = "existingCred";
            existingCred.ID = Guid.NewGuid();
            existingCred.Type = CredentialType.BasicAuthentication;
            mExistingCredentials.Add(existingCred);
            int result = AutomateC.Run(new string[] { "/updatecredential", "existingCred" });
            result.Should().Be(0);
            AssertCredentialSetUpCorrectly(mUpdatedCredentials.First().Value, "existingCred", "", "", CredentialType.BasicAuthentication);
        }

        [Test]
        public void UpdateCredential_CredentialExists_UpdatesCorrectCredential()
        {
            var existingCred = new clsCredential();
            existingCred.Name = "existingCred";
            existingCred.ID = Guid.NewGuid();
            mExistingCredentials.Add(existingCred);
            int result = AutomateC.Run(new string[] { "/updatecredential", "existingCred" });
            result.Should().Be(0);
            mUpdatedCredentials.Count.Should().Be(1);
            mUpdatedCredentials.First().Key.Should().Be("existingCred");
            mUpdatedCredentials.First().Value.ShouldBeEquivalentTo(existingCred);
        }

        [Test]
        public void UpdateCredential_InvalidTypeName_ActionFails()
        {
            var existingCred = new clsCredential();
            existingCred.Name = "existingCred";
            existingCred.ID = Guid.NewGuid();
            mExistingCredentials.Add(existingCred);
            int result = AutomateC.Run(new string[] { "/updatecredential", "existingCred", "/credentialType", "invalidType" });
            result.Should().Be(1);
            mUpdatedCredentials.Count.Should().Be(0);
        }

        private void AssertCredentialSetUpCorrectly(clsCredential credential, string name, string username, string password, CredentialType type)
        {
            credential.Name.Should().Be(name);
            credential.Username.Should().Be(username);
            credential.Password.AsString().Should().Be(password);
            credential.Type.Should().Be(type);
        }
    }
}

#endif