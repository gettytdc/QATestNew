using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Commands;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Common.Security;
using BluePrism.Server.Domain.Models;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace AutomateAppCore.UnitTests.Commands
{
    [TestFixture]
    public class PasswordCommandTests
    {
        private Mock<IServer> _serverMock;
        private Mock<IListenerClient> _clientMock;
        private Mock<IListener> _listenerMock;
        private PasswordCommand _passwordCommand;
        private const string Password = "qwerty";
        private const string UserName = "testy.mctesterson@my.test.com";
        private Mock<IMemberPermissions> _memberPermissionsMock;
        private Mock<IUser> _userMock;

        [SetUp]
        public void SetUp()
        {
            var authMode = AuthMode.MappedActiveDirectory;

            _userMock = new Mock<IUser>();
            _userMock.Setup(x => x.Name).Returns(UserName);
            _userMock.Setup(x => x.AuthType).Returns(authMode);

            _serverMock = new Mock<IServer>();
            _serverMock
                .Setup(x => x.ValidateCredentials(UserName, It.Is<SafeString>(s => Password.Equals(s.AsString())), authMode))
                .Returns(_userMock.Object);

            var clientUserId = Guid.NewGuid();

            _clientMock = new Mock<IListenerClient>();
            _clientMock.Setup(x => x.UserSet).Returns(true);
            _clientMock.Setup(x => x.UserRequested).Returns(true);
            _clientMock.Setup(x => x.UserId).Returns(clientUserId);
            _clientMock.SetupProperty(x => x.RequestedUserName, UserName);
            _clientMock.SetupProperty(x => x.AuthenticatedUser);
            _clientMock.SetupProperty(x => x.RequestedAuthenticationMode, authMode);

            _listenerMock = new Mock<IListener>();
            _listenerMock.Setup(x => x.ResourceId).Returns(Guid.NewGuid());
            _listenerMock.Setup(x => x.ResourceOptions).Returns(new ResourcePCStartUpOptions() { IsPublic = true });
            _listenerMock.Setup(x => x.UserId).Returns(clientUserId);

            _memberPermissionsMock = new Mock<IMemberPermissions>();
            _memberPermissionsMock
                .Setup(x => x.HasPermission(It.IsAny<IUser>(), It.IsAny<string[]>()))
                .Returns(true);

            var memberPermissionsFactory = (Func<IGroupPermissions, IMemberPermissions>)((x) => _memberPermissionsMock.Object);

            _passwordCommand = new PasswordCommand(_clientMock.Object, _listenerMock.Object, _serverMock.Object, memberPermissionsFactory);
        }

        [Test]
        public void Execute_UserNotAlreadyRequested_ShouldReturnWrongAuthSequence()
        {
            _clientMock.Setup(x => x.UserRequested).Returns(false);
            var result = _passwordCommand.Execute($"password {Password}");
            result.output.Should().Be("WRONG AUTH SEQUENCE");
        }

        [Test]
        public void Execute_UserNotSet_ShouldReturnAuthenticationFailed()
        {
            _clientMock.Setup(x => x.UserSet).Returns(false);
            var result = _passwordCommand.Execute($"password {Password}");
            result.output.Should().Be("AUTHENTICATION FAILED");
        }

        [Test]
        public void Execute_UserSetButRequestedUserNameIsBlank_ShouldReturnAuthenticationFailed()
        {
            _clientMock.SetupProperty(x => x.RequestedUserName, string.Empty);
            var result = _passwordCommand.Execute($"password {Password}");
            result.output.Should().Be("AUTHENTICATION FAILED");
        }

        [Test]
        public void Execute_ListenerResourceNotPublicAndLoggedInWithDifferentUser_ShouldReturnRestricted()
        {
            var differentUserId = Guid.NewGuid();
            _listenerMock.Setup(x => x.ResourceOptions).Returns(new ResourcePCStartUpOptions() { IsPublic = false });
            _listenerMock.Setup(x => x.UserId).Returns(differentUserId);

            var result = _passwordCommand.Execute($"password {Password}");

            result.output.Should().StartWith("RESTRICTED");
        }

        [Test]
        public void Execute_ListenerResourceNotPublicAndLoggedInWithDifferentUser_ShouldClearOutAuthenticatedUser()
        {
            var differentUserId = Guid.NewGuid();
            _listenerMock.Setup(x => x.ResourceOptions).Returns(new ResourcePCStartUpOptions() { IsPublic = false });
            _listenerMock.Setup(x => x.UserId).Returns(differentUserId);

            _passwordCommand.Execute($"password {Password}");

            _clientMock.Object.AuthenticatedUser.Should().BeNull();
        }

        [Test]
        public void Execute_UserHasNoPermissionOnResource_ShouldReturnUserDoesNotHaveAccess()
        {
            _memberPermissionsMock
                .Setup(x => x.HasPermission(It.IsAny<IUser>(), It.IsAny<string[]>()))
                .Returns(false);
            var result = _passwordCommand.Execute($"password {Password}");

            result.output.Should().Be("USER DOES NOT HAVE ACCESS TO THIS RESOURCE");
        }

        [Test]
        public void Execute_UserHasNoPermissionOnResource_ShouldClearOutAuthenticatedUser()
        {
            _memberPermissionsMock
                .Setup(x => x.HasPermission(It.IsAny<IUser>(), It.IsAny<string[]>()))
                .Returns(false);

            _passwordCommand.Execute($"password {Password}");

            _clientMock.Object.AuthenticatedUser.Should().BeNull();
        }

        [Test]
        public void Execute_SystemUser_ShouldReturnUserAuthenticated()
        {
            _memberPermissionsMock
                .Setup(x => x.HasPermission(It.IsAny<IUser>(), It.IsAny<string[]>()))
                .Returns(false);
            _userMock.Setup(x => x.AuthType).Returns(AuthMode.System);
            var result = _passwordCommand.Execute($"password {Password}");
            result.output.Should().Be("USER AUTHENTICATED");
        }

        [Test]
        public void Execute_SystemUser_ShouldSetAuthenticatedUser()
        {
            _memberPermissionsMock
                .Setup(x => x.HasPermission(It.IsAny<IUser>(), It.IsAny<string[]>()))
                .Returns(false);
            _userMock.Setup(x => x.AuthType).Returns(AuthMode.System);
            _passwordCommand.Execute($"password {Password}");
            _clientMock.Object.AuthenticatedUser.Name.Should().Be(UserName);
        }

        [Test]
        public void Execute_SystemUser_ShouldClearOutRequestedUserNameAndAuthMode()
        {
            _memberPermissionsMock
                .Setup(x => x.HasPermission(It.IsAny<IUser>(), It.IsAny<string[]>()))
                .Returns(false);
            _userMock.Setup(x => x.AuthType).Returns(AuthMode.System);
            _passwordCommand.Execute($"password {Password}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
            _clientMock.Object.RequestedUserName.Should().Be(string.Empty);
        }

        [Test]
        public void Execute_UserHasPermission_ShouldReturnUserAuthenticated()
        {
            var result = _passwordCommand.Execute($"password {Password}");
            result.output.Should().Be("USER AUTHENTICATED");
        }

        [Test]
        public void Execute_UserHasPermission_ShouldSetAuthenticatedUser()
        {
            _passwordCommand.Execute($"password {Password}");
            _clientMock.Object.AuthenticatedUser.Name.Should().Be(UserName);
        }

        [Test]
        public void Execute_UserHasPermission_ShouldClearOutRequestedUserNameAndAuthMode()
        {
            _passwordCommand.Execute($"password {Password}");
            _clientMock.Object.RequestedAuthenticationMode.Should().Be(AuthMode.Unspecified);
            _clientMock.Object.RequestedUserName.Should().Be(string.Empty);
        }
    }
}
