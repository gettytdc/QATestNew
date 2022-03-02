#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.ClientServerConnection;
using BluePrism.AutomateAppCore.DataMonitor;
using BluePrism.Data;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting;
using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.Auth
{
    [TestFixture]
    public class ServerLoginWithAccessTokenTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IAccessTokenValidator> _accessTokenValidatorMock;
        private Mock<IDataReader> _userDataReaderMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly Guid _userId = Guid.NewGuid();
        private int _isDeletedColumnIndex;
        private Mock<IDataReader> _userRoleDataReaderMock;
        private ClaimsPrincipal _validAuthGatewayClaims;
        private ClaimsPrincipal _validAuthServerClaims;
        private Mock<IDataReader> _userIdDataReaderMock;
        private readonly string _userName = "Testy McTesterson";
        private readonly string _roleName = "Special Role";
        private readonly Claim _nicknameClaim = new Claim("nickname", "Testy McTesterson");
        private readonly Claim _authTimeClaim = new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        private readonly Claim _issuerClaim = new Claim("iss", "https://secureserver");
        private readonly Claim _idClaim = new Claim("Id", Guid.NewGuid().ToString());
        private readonly ReloginTokenRequest _emptyReloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, null);

        [SetUp]
        public void Setup()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();

            _server = new clsServer();

            _databaseConnectionMock = new Mock<IDatabaseConnection>();
            _databaseConnectionMock.Setup(x => x.Execute(It.IsAny<SqlCommand>()));

            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", _server, (Func<IDatabaseConnection>)(() => _databaseConnectionMock.Object));
            ReflectionHelper.SetPrivateField("mLoggedInUser", _server, null);

            _permissionValidatorMock = new Mock<IPermissionValidator>();
            _permissionValidatorMock.Setup(validator => validator.EnsurePermissions(It.IsAny<ServerPermissionsContext>()));
            ReflectionHelper.SetPrivateField("mPermissionValidator", _server, _permissionValidatorMock.Object);
            ReflectionHelper.SetPrivateField("mAccessTokenClaimsParser", _server, new AccessTokenClaimsParser());

            SetUpAccessTokenValidatorMock();
            SetUpGetActiveUserIdDataReaderMock();
            SetUpGetActiveAuthServerUserIdDataReaderMock();
            SetUpUserDataReaderMock();
            SetUpUserRoleDataReaderMock();
            SetUpSystemRoleSetMock();
            SetUpAuditRecordWriterMock();

            _validAuthGatewayClaims = new ClaimsPrincipal();
            var nameClaim = new Claim(ClaimTypes.NameIdentifier, _userId.ToString());
            _validAuthGatewayClaims.AddIdentity(new ClaimsIdentity(new Claim[] { nameClaim }));

            _validAuthServerClaims = new ClaimsPrincipal();
            _validAuthServerClaims.AddIdentity(new ClaimsIdentity(new Claim[] { _nicknameClaim, _issuerClaim, _authTimeClaim, _idClaim }));
        }

        [Test]
        public void Login_SingleSignOnDatabase_ShouldReturnTypeMismatch()
        {
            var logonOptions = new LogonOptions() { SingleSignon = true };
            SetLogOnOptions(logonOptions);
            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.TypeMismatch);
        }

        [Test]
        public void Login_AlreadyLoggedIn_ShouldReturnAlreadyLoggedIn()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false };
            SetLogOnOptions(logonOptions);
            ReflectionHelper.SetPrivateField("mLoggedInUser", _server, new Mock<IUser>().Object);
            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.Already);
        }

        [Test]
        public void Login_AlreadyLoggedIn_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false };
            SetLogOnOptions(logonOptions);
            ReflectionHelper.SetPrivateField("mLoggedInUser", _server, new Mock<IUser>().Object);
            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_InvalidToken_ShouldReturnInvalidAccessToken()
        {

            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            _accessTokenValidatorMock
                .Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<SecurityTokenValidationException>();

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.InvalidAccessToken);

        }

        [Test]
        public void Login_AuthenticationServerUrl_InvalidToken_ShouldNotReturnReloginToken()
        {

            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            _accessTokenValidatorMock
                .Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<SecurityTokenValidationException>();

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_TokenHasNoNickName_ShouldReturnInvalidAccessToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            var claimsWithoutNickName = new ClaimsPrincipal();
            claimsWithoutNickName.AddIdentity(new ClaimsIdentity(new Claim[] { _issuerClaim, _authTimeClaim }));
            SetUpAccessTokenClaims(claimsWithoutNickName);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.InvalidAccessToken);
        }

        [Test]
        public void Login_AuthenticationServerUrl_TokenHasNoNickName_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            var claimsWithoutNickName = new ClaimsPrincipal();
            claimsWithoutNickName.AddIdentity(new ClaimsIdentity(new Claim[] { _issuerClaim, _authTimeClaim }));
            SetUpAccessTokenClaims(claimsWithoutNickName);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_TokenHasNoIssuer_ShouldReturnInvalidAccessToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            var claimsWithoutIssuer = new ClaimsPrincipal();
            claimsWithoutIssuer.AddIdentity(new ClaimsIdentity(new Claim[] { _nicknameClaim, _authTimeClaim }));
            SetUpAccessTokenClaims(claimsWithoutIssuer);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.InvalidAccessToken);
        }

        [Test]
        public void Login_AuthenticationServerUrl_TokenHasNoIssuer_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            var claimsWithoutIssuer = new ClaimsPrincipal();
            claimsWithoutIssuer.AddIdentity(new ClaimsIdentity(new Claim[] { _nicknameClaim, _authTimeClaim }));
            SetUpAccessTokenClaims(claimsWithoutIssuer);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_TokenHasNoAuthTime_ShouldReturnInvalidAccessToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            var claimsWithoutAuthTime = new ClaimsPrincipal();
            claimsWithoutAuthTime.AddIdentity(new ClaimsIdentity(new Claim[] { _nicknameClaim, _issuerClaim }));
            SetUpAccessTokenClaims(claimsWithoutAuthTime);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.InvalidAccessToken);
        }

        [Test]
        public void Login_AuthenticationServerUrl_TokenHasNoAuthTime_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            var claimsWithoutAuthTime = new ClaimsPrincipal();
            claimsWithoutAuthTime.AddIdentity(new ClaimsIdentity(new Claim[] { _nicknameClaim, _issuerClaim }));
            SetUpAccessTokenClaims(claimsWithoutAuthTime);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_CannotFindUserIdForName_ShouldReturnUserNotFound()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
            _userIdDataReaderMock.Setup(x => x.Read()).Returns(false);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.UnableToFindUser);
        }

        [Test]
        public void Login_AuthenticationServerUrl_CannotFindUserIdForName_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
            _userIdDataReaderMock.Setup(x => x.Read()).Returns(false);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_UserNotFound_ShouldReturnUserNotFound()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
            _userDataReaderMock.Setup(x => x.Read()).Returns(false);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.UnableToFindUser);
        }

        [Test]
        public void Login_AuthenticationServerUrl_UserNotFound_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
            _userDataReaderMock.Setup(x => x.Read()).Returns(false);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_DeletedUserNotFound_ShouldReturnDeleted()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
            _userDataReaderMock.Setup(x => x[_isDeletedColumnIndex]).Returns(true);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.Deleted);
        }

        [Test]
        public void Login_AuthenticationServerUrl_DeletedUserNotFound_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
            _userDataReaderMock.Setup(x => x[_isDeletedColumnIndex]).Returns(true);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser_ShouldReturnSuccess()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.Success);
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser_ShouldReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            result.ReloginToken.Should().NotBeNull();
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser_ShouldReturnUserWithCorrectRoles()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;

            loginResult.User.Roles.Select(x => x.Name).Should().BeEquivalentTo(new[] { _roleName });
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser_ShouldSetLoggedInUserOnServer()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            var loggedInUser = GetPrivateServerField("mLoggedInUser");
            loggedInUser.Should().BeAssignableTo<IUser>();
            ((IUser)loggedInUser).Name.Should().Be(_userName);
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser_ShouldSetLoggedInMachineOnServer()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            var loggedInMachine = GetPrivateServerField("mLoggedInMachine");
            loggedInMachine.Should().BeOfType<string>();
            ((string)loggedInMachine).Should().Be("thismachine");
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser_ShouldSetLoggedInModeOnServer()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            var loggedInUser = GetPrivateServerField("mLoggedInMode");
            loggedInUser.Should().BeOfType<AuthMode>();
            ((AuthMode)loggedInUser).Should().Be(AuthMode.AuthenticationServer);
        }

        [Test]
        public void Login_AuthenticationServerUrl_ValidUser__ShouldSetLoggedInLocaleOnServer()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            var loggedInLocale = GetPrivateServerField("mLoggedInUserLocale");
            loggedInLocale.Should().BeOfType<string>();
            ((string)loggedInLocale).Should().Be("en-us");
        }


        private T GetValueOrDefault<T>(SqlParameterCollection parameters, string parameterName)
        {
            try
            {
                return (T)parameters[parameterName].Value;
            }
            catch
            {
                return default(T);
            }
        }

        private void SetUpAccessTokenClaims(ClaimsPrincipal claims)
        {
            _accessTokenValidatorMock
                .Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(claims);
        }

        private void SetUpAccessTokenValidatorMock()
        {
            _accessTokenValidatorMock = new Mock<IAccessTokenValidator>();
            _accessTokenValidatorMock.Setup(x => x.Validate(It.IsAny<string>(), It.IsAny<string>()));
            ReflectionHelper.SetPrivateField("mAccessTokenValidator", _server, _accessTokenValidatorMock.Object);
        }

        private void SetLogOnOptions(LogonOptions logonOptions)
        {
            var logOnOptionsDataReaderMock = new Mock<IDataReader>();
            logOnOptionsDataReaderMock.Setup(x => x.FieldCount).Returns(7);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(0)).Returns("populateusernameusing");
            logOnOptionsDataReaderMock.Setup(x => x[0]).Returns(logonOptions.AutoPopulate);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(1)).Returns("showusernamesonlogin");
            logOnOptionsDataReaderMock.Setup(x => x[1]).Returns(logonOptions.ShowUserList);

            var activeDirectoryProvider = logonOptions.SingleSignon ? "my.domain.com" : string.Empty;
            logOnOptionsDataReaderMock.Setup(x => x.GetName(2)).Returns("activedirectoryprovider");
            logOnOptionsDataReaderMock.Setup(x => x[2]).Returns(activeDirectoryProvider);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(3)).Returns("authenticationgatewayurl");
            logOnOptionsDataReaderMock.Setup(x => x[3]).Returns(logonOptions.AuthenticationGatewayUrl);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(4)).Returns("enableexternalauth");
            logOnOptionsDataReaderMock.Setup(x => x[4]).Returns(logonOptions.ExternalAuthenticationEnabled);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(5)).Returns("enablemappedactivedirectoryauth");
            logOnOptionsDataReaderMock.Setup(x => x[5]).Returns(logonOptions.MappedActiveDirectoryAuthenticationEnabled);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(6)).Returns("authenticationserverurl");
            logOnOptionsDataReaderMock.Setup(x => x[6]).Returns(logonOptions.AuthenticationServerUrl);

            logOnOptionsDataReaderMock.Setup(x => x.Read()).Returns(true);

            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.Contains("from BPASysConfig"))))
                .Returns(logOnOptionsDataReaderMock.Object);
        }

        private IEnumerable<(string fieldName, object value)> UserTableQueryResults()
        {
            var currentTime = DateTime.UtcNow;
            yield return ("userid", _userId);
            yield return ("username", _userName);
            yield return ("created", currentTime);
            yield return ("expiry", currentTime.AddMonths(3));
            yield return ("passwordexpiry", currentTime.AddMonths(3));
            yield return ("alerteventtypes", AlertEventType.None);
            yield return ("alertnotificationtypes", AlertNotificationType.None);
            yield return ("lastsignedin", currentTime);
            yield return ("isdeleted", false);
            yield return ("passwordexpirywarninginterval", 0);
            yield return ("locked", false);
            yield return ("passworddurationweeks", 5);
            yield return ("authtype", AuthMode.AuthenticationServer);
        }

        private void SetUpGetActiveUserIdDataReaderMock()
        {
            _userIdDataReaderMock = new Mock<IDataReader>();
            _userIdDataReaderMock.Setup(x => x.FieldCount).Returns(1);
            _userIdDataReaderMock.Setup(x => x.GetName(0)).Returns("userid");
            _userIdDataReaderMock.Setup(x => x[0]).Returns(_userId);
            _userIdDataReaderMock.Setup(x => x.Read()).Returns(true);
            ;

            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.StartsWith(" select top 1 u.userid from BPAUser"))))
                .Returns(_userIdDataReaderMock.Object);
        }

        private void SetUpGetActiveAuthServerUserIdDataReaderMock()
        {
            _userIdDataReaderMock = new Mock<IDataReader>();
            _userIdDataReaderMock.Setup(x => x.FieldCount).Returns(1);
            _userIdDataReaderMock.Setup(x => x.GetName(0)).Returns("userid");
            _userIdDataReaderMock.Setup(x => x[0]).Returns(_userId);
            _userIdDataReaderMock.Setup(x => x.Read()).Returns(true);
            ;

            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.StartsWith(" select u.userid from BPAUser"))))
                .Returns(_userIdDataReaderMock.Object);
        }

        private void SetUpUserDataReaderMock()
        {
            _userDataReaderMock = new Mock<IDataReader>();

            _userDataReaderMock.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            var userTableQueryResults = UserTableQueryResults().ToList();
            _userDataReaderMock.Setup(x => x.FieldCount).Returns(userTableQueryResults.Count);
            for (var i = 0; i < userTableQueryResults.Count; i++)
            {
                var (fieldName, value) = userTableQueryResults[i];
                if (fieldName == "isdeleted")
                {
                    _isDeletedColumnIndex = i;
                }
                _userDataReaderMock.Setup(x => x.GetName(i)).Returns(fieldName);
                _userDataReaderMock.Setup(x => x[i]).Returns(value);
            };

            _databaseConnectionMock
             .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.Contains("select   u.userid,   u.username,   u.loginattempts,   u.isdeleted,"))))
             .Returns(_userDataReaderMock.Object);
        }

        private void SetUpUserRoleDataReaderMock()
        {
            _userRoleDataReaderMock = new Mock<IDataReader>();
            _userRoleDataReaderMock.Setup(x => x.FieldCount).Returns(1);
            _userRoleDataReaderMock.Setup(x => x.GetName(0)).Returns("name");
            _userRoleDataReaderMock.Setup(x => x[0]).Returns(_roleName);
            _userRoleDataReaderMock.Setup(x => x.GetString(0)).Returns(_roleName);
            _userRoleDataReaderMock.SetupSequence(x => x.Read()).Returns(true).Returns(false).Returns(true).Returns(false);

            _databaseConnectionMock
             .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.Contains("join BPAUserRoleAssignment"))))
             .Returns(_userRoleDataReaderMock.Object);
        }

        private void SetUpSystemRoleSetMock()
        {
            var dataMonitorMock = new Mock<IDataMonitor>();
            var roleSet = new RoleSet() { new Role(_roleName) { Id = 0 } };

            Assembly.Load("AutomateAppCore")
                .GetType("BluePrism.AutomateAppCore.Auth.SystemRoleSet")
                .GetField("mDataMonitorFactory", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, (Func<IDataMonitor>)(() => dataMonitorMock.Object));

            var serverMock = new Mock<IServer>();
            serverMock.Setup(x => x.GetRoles()).Returns(roleSet);

            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(x => x.Server).Returns(serverMock.Object);

            var serverFactoryMock = new Mock<IServerFactory>();
            serverFactoryMock.SetupGet(x => x.ServerManager).Returns(serverManagerMock.Object);
            Assembly.Load("AutomateAppCore")
                .GetType("BluePrism.AutomateAppCore.app")
                .GetField("ServerFactory", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, serverFactoryMock.Object);
        }

        private void SetUpAuditRecordWriterMock()
        {
            app.gAuditingEnabled = true;
            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnRecordsAffected(It.Is<SqlCommand>(x => x.CommandText.StartsWith("INSERT INTO BPAAuditEvents"))))
                .Returns(1);
        }

        private object GetPrivateServerField(string fieldName)
            => Assembly.Load("AutomateAppCore")
                       .GetType("BluePrism.AutomateAppCore.clsServer")
                       .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                       .GetValue(_server);
    }
}
#endif
