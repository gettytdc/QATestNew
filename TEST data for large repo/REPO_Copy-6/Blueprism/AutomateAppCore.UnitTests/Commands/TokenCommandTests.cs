using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Commands;
using BluePrism.Common.Security;
using BluePrism.Server.Domain.Models;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace AutomateAppCore.UnitTests.Commands
{
    [TestFixture]
    public class TokenCommandTests
    {
        private clsAuthToken _authTokenForSomeUser;
        private readonly (Guid userId, string userName, string password) _otherUser
            = (userId: Guid.NewGuid(), userName: "some user", password: "qwerty");

        private clsAuthToken _authTokenForActiveDirectoryUser;
        private readonly (Guid userId, string upn, string password) _activeDirectoryUser
            = (userId: Guid.NewGuid(), upn: "someotheruser@domain.com", password: "fancypassw0rd");

        private Mock<IServer> _serverMock;
        private Mock<IListenerClient> _clientMock;
        private TokenCommand _tokenCommand;        
        private readonly Guid _processId = Guid.NewGuid();
        
        [SetUp]
        public void SetUp()
        {           
            _authTokenForSomeUser = new clsAuthToken(_otherUser.userId, Guid.NewGuid(), DateTime.UtcNow.AddDays(1), _processId);
            _authTokenForActiveDirectoryUser = new clsAuthToken(_activeDirectoryUser.userId, Guid.NewGuid(), DateTime.UtcNow.AddDays(1), _processId);
            _serverMock = new Mock<IServer>();
            _serverMock
                .Setup(x => x.RegisterAuthorisationToken(_activeDirectoryUser.upn, It.Is<SafeString>(s => _activeDirectoryUser.password.Equals(s.AsString())), _processId, AuthMode.MappedActiveDirectory))
                .Returns(_authTokenForActiveDirectoryUser);
            _serverMock
                .Setup(x => x.RegisterAuthorisationToken(_otherUser.userName, It.Is<SafeString>(s => _otherUser.password.Equals(s.AsString())), _processId, AuthMode.Unspecified))
                .Returns(_authTokenForSomeUser);
            _serverMock
                .Setup(x => x.GetUserName(_otherUser.userId))
                .Returns(_otherUser.userName);

            _clientMock = new Mock<IListenerClient>();
            
            _tokenCommand = new TokenCommand(_clientMock.Object, null, _serverMock.Object, null);
        }


        [Test]
        public void ExecuteWithUserId_CouldNotRegisterToken_ShouldReturnAuthenticationFailed()
        {
            _clientMock.Setup(x => x.AuthenticatedUser).Returns((IUser)null);
            _serverMock
                .Setup(x => x.RegisterAuthorisationToken(_otherUser.userName, It.Is<SafeString>(s => _otherUser.password.Equals(s.AsString())), _processId, AuthMode.Unspecified))
                .Returns((clsAuthToken)null);
            var result = _tokenCommand.Execute($"getauthtoken {_processId} {_otherUser.userId} {_otherUser.password}");
            result.output.Should().Be("AUTHENTICATION FAILED");
        }
        
        [Test]
        public void ExecuteWithUserId_CouldRegisterToken_ShouldReturnTokenForUserWhoseCredentialsWereSupplied()
        {
            var result = _tokenCommand.Execute($"getauthtoken {_processId} {_otherUser.userId} {_otherUser.password}");
            result.output.Should().Be(_authTokenForSomeUser.ToString());
        }

        [Test]
        public void ExecuteWithUserId_InvalidNumberOfParameters_ShouldReturnInvalidParameters()
        {
            var result = _tokenCommand.Execute($"getauthtoken {_otherUser.userId} {_otherUser.password}");
            result.output.Should().Be("INVALID PARAMETERS");
        }

        [Test]
        public void ExecuteWithUserId_UnexpectedException_ShouldReturnExceptionMessage()
        {
            var exceptionMessage = "It all went wrong";
            _serverMock
                .Setup(x => x.RegisterAuthorisationToken(_otherUser.userName, It.Is<SafeString>(s => _otherUser.password.Equals(s.AsString())), _processId, AuthMode.Unspecified))
                .Throws(new Exception(exceptionMessage));
            var result = _tokenCommand.Execute($"getauthtoken {_processId} {_otherUser.userId} {_otherUser.password}");
            result.output.Should().Contain(exceptionMessage);
        }

        [Test]
        public void ExecuteWithUpn_CouldNotRegisterToken_ShouldReturnAuthenticationFailed()
        {
            _clientMock.Setup(x => x.AuthenticatedUser).Returns((IUser)null);
            _serverMock
                .Setup(x => x.RegisterAuthorisationToken(_activeDirectoryUser.upn, It.Is<SafeString>(s => _activeDirectoryUser.password.Equals(s.AsString())), _processId, AuthMode.MappedActiveDirectory))
                .Returns((clsAuthToken)null);
            var result = _tokenCommand.Execute($"getauthtoken upn {_activeDirectoryUser.upn} {_activeDirectoryUser.password} {_processId}");
            result.output.Should().Be("AUTHENTICATION FAILED");
        }
        
        [Test]
        public void ExecuteWithUpn_CouldRegisterToken_ShouldReturnTokenForUserWhoseCredentialsWereSupplied()
        {
            var result = _tokenCommand.Execute($"getauthtoken upn {_activeDirectoryUser.upn} {_activeDirectoryUser.password} {_processId}");
            result.output.Should().Be(_authTokenForActiveDirectoryUser.ToString());
        }

        [Test]
        public void ExecuteWithUpn_SingleSignOnDatabase_ShouldReturnUpnNotSupported()
        {
            _serverMock
                .Setup(x => x.DatabaseType())
                .Returns(DatabaseType.SingleSignOn);
            var result = _tokenCommand.Execute($"getauthtoken upn {_activeDirectoryUser.upn} {_activeDirectoryUser.password} {_processId}");
            result.output.Should().Be("UPN NOT SUPPORTED FOR SSO DATABASE");
        }

        [Test]
        public void ExecuteWithUpn_InvalidNumberOfParameters_ShouldReturnInvalidParameters()
        {
            var result = _tokenCommand.Execute($"getauthtoken upn {_activeDirectoryUser.upn} {_activeDirectoryUser.password}");
            result.output.Should().Be("INVALID PARAMETERS");
        }

        [Test]
        public void ExecuteWithUpn_UnexpectedException_ShouldReturnExceptionMessage()
        {
            var exceptionMessage = "It all went wrong";
            _serverMock
                .Setup(x => x.RegisterAuthorisationToken(_activeDirectoryUser.upn, It.Is<SafeString>(s => _activeDirectoryUser.password.Equals(s.AsString())), _processId, AuthMode.MappedActiveDirectory))
                .Throws(new Exception(exceptionMessage));
            var result = _tokenCommand.Execute($"getauthtoken upn {_activeDirectoryUser.upn} {_activeDirectoryUser.password} {_processId}");
            result.output.Should().Contain(exceptionMessage);
        }

    }
}
