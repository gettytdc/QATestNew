#if UNITTESTS

using System;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Commands;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Commands
{
    [TestFixture]
    public class UserCommandTests
    {
        private Mock<IServer> _serverMock;
        private Mock<IListenerClient> _clientMock;
        private UserCommand _userCommand;
        private readonly Guid _userId = Guid.NewGuid();
        private const string UserName = "testy.mctesterson@my.test.com";

        [SetUp]
        public void SetUp()
        {
            _serverMock = new Mock<IServer>();
            _clientMock = new Mock<IListenerClient>();
            _clientMock.SetupProperty(x => x.RequestedUserName);
            _clientMock.SetupProperty(x => x.UserRequested);
            _clientMock.SetupProperty(x => x.RequestedAuthenticationMode);

            _userCommand = new UserCommand(_clientMock.Object, null, _serverMock.Object, null);
        }

        private void SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserFound()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.NativeAndExternal);
            _serverMock.Setup(x => x.TryGetUserID(UserName)).Returns(_userId);
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(UserName);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndFindsUser_ShouldSetRequestedUserName()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedUserName.Should().Be(UserName);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndFindsUser_ShouldSetUserRequestedFlag()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndFindsUser_ShouldNotSpecifyAuthenticationMode()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndFindsUser_ShouldReturnUserSet()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserFound();
            var result = _userCommand.Execute($"user name {UserName}");
            result.output.Should().Be("USER SET");
        }

        private void SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserNotFound()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.NativeAndExternal);
            _serverMock.Setup(x => x.TryGetUserID(UserName)).Returns(Guid.Empty);
            _serverMock.Setup(x => x.GetUserName(Guid.Empty)).Returns(string.Empty);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndCouldNotFindUser_ShouldReturnUserSet()
        {

            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserNotFound();
            var result = _userCommand.Execute($"user name {UserName}");
            result.output.Should().Be("USER SET");
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndCouldNotFindUser_ShouldSetUserRequestedFlag()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserNotFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndCouldNotFindUser_ShouldNotSpecifyAuthenticationMode()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserNotFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndNativeDatabaseAndCouldNotFindUser_ShouldNotSetRequestedUserName()
        {
            SetUpScenarioWhere_CommandStartsWithName_NativeDatabase_UserNotFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedUserName.Should().Be(string.Empty);
        }

        private void SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserFound()
        {
            ReflectionHelper.SetPrivateField("_findUserPrincipalName", _userCommand,
                                                (Func<string, string>)((x) => UserName));
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.SingleSignOn);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldFindUser_ShouldSetRequestedUserName()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedUserName.Should().Be(UserName);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldFindUser_ShouldNotSpecifyAuthenticationMode()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
        }
        
        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldFindUser_ShouldSetUserRequestedFlag()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldFindUser_ShouldReturnUserSet()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserFound();
            var result = _userCommand.Execute($"user name {UserName}");
            result.output.Should().Be("USER SET");
        }

        private void SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserNotFound()
        {
            ReflectionHelper.SetPrivateField("_findUserPrincipalName", _userCommand,
                                            (Func<string, string>)((x) => throw new InvalidOperationException("Could not find user in domain")));
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.SingleSignOn);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldNotFindUser_ShouldNotSetRequestedUserName()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserNotFound();
            _userCommand.Execute($"user name {UserName}");          
            _clientMock.Object.RequestedUserName.Should().Be(string.Empty);
        }

        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldNotFindUser_ShouldNotSpecifyAuthenticationMode()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserNotFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
        }


        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldNotFindUser_ShouldSetUserRequestedFlag()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserNotFound();
            _userCommand.Execute($"user name {UserName}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandStartsWithNameAndSsoDatabaseAndCouldNotFindUser_ShouldReturnUserSet()
        {
            SetUpScenarioWhere_CommandStartsWithName_SsoDatabase_UserNotFound();
            var result = _userCommand.Execute($"user name {UserName}");
            result.output.Should().Be("USER SET");
        }                

        [Test]
        public void Execute_CommandStartsWithUpnAndNativeDatabase_ShouldReturnUserSet()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.NativeAndExternal);
            var result = _userCommand.Execute($"user upn {UserName}");
            result.output.Should().Be("USER SET");
        }

        [Test]
        public void Execute_CommandStartsWithUpnAndNativeDatabase_ShouldSetRequestedUsername()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.NativeAndExternal);
            _userCommand.Execute($"user upn {UserName}");
            _clientMock.Object.RequestedUserName.Should().Be(UserName);
        }


        [Test]
        public void Execute_CommandStartsWithUpnAndNativeDatabase_ShouldSetUserRequestedFlag()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.NativeAndExternal);
            _userCommand.Execute($"user upn {UserName}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandStartsWithUpnAndNativeDatabase_ShouldSetRequestedAuthenticationMode()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.NativeAndExternal);
            _userCommand.Execute($"user upn {UserName}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.MappedActiveDirectory);
        }

        [Test]
        public void Execute_CommandStartsWithUpnAndSsoDatabase_ShouldNotSetRequestedUser()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.SingleSignOn);
            _userCommand.Execute($"user upn {UserName}");
            _clientMock.Object.RequestedUserName.Should().Be(string.Empty);
        }

        [Test]
        public void Execute_CommandStartsWithUpnAndSsoDatabase_ShouldSetUserRequestedFlag()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.SingleSignOn);
            _userCommand.Execute($"user upn {UserName}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandStartsWithUpnAndSsoDatabase_ShouldReturnUserSet()
        {
            _serverMock.Setup(x => x.DatabaseType()).Returns(DatabaseType.SingleSignOn);
            var result = _userCommand.Execute($"user upn {UserName}");
            result.output.Should().Be("USER SET");
        }


        [Test]
        public void Execute_CommandContainsUserIdAndCanFindUser_ShouldSetRequestedUserName()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(UserName);
            _userCommand.Execute($"user {_userId}");
            _clientMock.Object.RequestedUserName.Should().Be(UserName);
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCanFindUser_ShouldSetUserRequestedFlag()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(UserName);
            _userCommand.Execute($"user {_userId}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCanFindUser_ShouldNotSpecifyAuthenticationMode()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(UserName);
            _userCommand.Execute($"user {_userId}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCanFindUser_ShouldReturnUserSet()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(UserName);
            var result = _userCommand.Execute($"user {_userId}");
            result.output.Should().Be("USER SET");
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCannotFindUser_ShouldNotSetRequestedUserName()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(string.Empty);
            _userCommand.Execute($"user {_userId}");
            _clientMock.Object.RequestedUserName.Should().Be(string.Empty);
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCannotFindUser_ShouldSetUserRequestedFlag()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(string.Empty);
            _userCommand.Execute($"user {_userId}");
            _clientMock.Object.UserRequested.Should().BeTrue();
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCannotFindUser_ShouldNotSpecifyAuthenticationMode()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(string.Empty);
            _userCommand.Execute($"user {_userId}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
        }

        [Test]
        public void Execute_CommandContainsUserIdAndCannotFindUser_ShouldReturnUserSet()
        {
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(string.Empty);
            var result = _userCommand.Execute($"user {_userId}");
            result.output.Should().Be("USER SET");
        }

        [Test]
        public void Execute_CommandContainsStubbedUserId_ShouldStillReturnCorrectUser()
        {
            var userIdStub = _userId.ToString().Substring(0, 8);
            _serverMock.Setup(x => x.GetCompleteUserID(userIdStub)).Returns(_userId);
            _serverMock.Setup(x => x.GetUserName(_userId)).Returns(UserName);
            _userCommand.Execute($"user {userIdStub}");
            _clientMock.Object.RequestedUserName.Should().Be(UserName);
        }

    }
}

#endif
