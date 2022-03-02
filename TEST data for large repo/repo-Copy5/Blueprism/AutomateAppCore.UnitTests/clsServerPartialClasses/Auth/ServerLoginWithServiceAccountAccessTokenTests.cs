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
using Moq;
using NUnit.Framework;


namespace AutomateAppCore.UnitTests.clsServerPartialClasses.Auth
{
    [TestFixture]
    public class ServerLoginWithServiceAccountAccessTokenTests 
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;        
        private Mock<IAccessTokenValidator> _accessTokenValidatorMock;        
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly Guid _userId = Guid.NewGuid();        
        private Mock<IDataReader> _userRoleDataReaderMock;       
        private ClaimsPrincipal _validAuthServerClaims;        
        private readonly string _userName = "Testy McTesterson";
        private readonly string _authenticationServerClientId = "Test Application";
        private readonly string _roleName = "Special Role";
        private readonly Claim _nicknameClaim = new Claim("nickname", "Testy McTesterson");
        private readonly Claim _authTimeClaim = new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        private readonly Claim _issuerClaim = new Claim("iss", "https://secureserver");
        private readonly Claim _clientIdClaim = new Claim("client_id", "Test Application");
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

            ReflectionHelper.SetPrivateField("mValidateReloginToken", _server,
            (Func<IDatabaseConnection, ReloginTokenRequest, Guid>)((con, tokenRequest) => Guid.NewGuid()));

            ReflectionHelper.SetPrivateField("mGetActiveAuthServerServiceAccountUser", _server,
            (Func<IDatabaseConnection, string, User>)((con, authServerClientId) =>
            new User(AuthMode.AuthenticationServerServiceAccount,
            Guid.NewGuid(),
            _userName)));

            SetUpAccessTokenValidatorMock();
            SetUpUserRoleDataReaderMock();
            SetUpSystemRoleSetMock();
            SetUpAuditRecordWriterMock();

            _validAuthServerClaims = new ClaimsPrincipal();
            _validAuthServerClaims.AddIdentity(new ClaimsIdentity(new Claim[] { _nicknameClaim, _issuerClaim, _authTimeClaim, _clientIdClaim }));
        }

        [Test]
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser_ShouldReturnSuccess()
        {
            
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.Success);
        }

        [Test]
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser_ShouldNotReturnReloginToken()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            result.ReloginToken.Should().BeNull();
        }

        [Test]
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser_ShouldReturnUserWithCorrectRoles()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;

            loginResult.User.Roles.Select(x => x.Name).Should().BeEquivalentTo(new[] { _roleName });
        }

        [Test]
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser_ShouldSetLoggedInUserOnServer()
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
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser_ShouldSetLoggedInMachineOnServer()
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
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser_ShouldSetLoggedInModeOnServer()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            var loggedInUser = GetPrivateServerField("mLoggedInMode");
            loggedInUser.Should().BeOfType<AuthMode>();
            ((AuthMode)loggedInUser).Should().Be(AuthMode.AuthenticationServerServiceAccount);
        }

        [Test]
        public void LoginWithServiceAccountAccessToken_ValidServiceAccountUser__ShouldSetLoggedInLocaleOnServer()
        {
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);

            var loggedInLocale = GetPrivateServerField("mLoggedInUserLocale");
            loggedInLocale.Should().BeOfType<string>();
            ((string)loggedInLocale).Should().Be("en-us");
        }


        [Test]
        public void LoginWithServiceAccountAccessToken_UserNotFound_ShouldReturnServiceAccountUserNotFound()
        {
            ReflectionHelper.SetPrivateField("mGetActiveAuthServerServiceAccountUser", _server,
            (Func<IDatabaseConnection, string, User>)((con, authServerClientId) =>
            { throw new ServiceAccountUserNotFoundException(); }));

            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.UnableToFindServiceAccountUser);
        }      


        [Test]
        public void LoginWithServiceAccountAccessToken_DeletedServiceAccountUserNotFound_ShouldReturnDeleted()
        {
            ReflectionHelper.SetPrivateField("mGetActiveAuthServerServiceAccountUser", _server,
          (Func<IDatabaseConnection, string, User>)((con, authServerClientId) =>
          { throw new DeletedException(); }));
            
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://gateway" };
            SetLogOnOptions(logonOptions);
            SetUpAccessTokenClaims(_validAuthServerClaims);
          

            var result = _server.Login("thismachine", "en-us", "1234", _emptyReloginTokenRequest);
            var loginResult = result.LoginResult;
            loginResult.Code.Should().Be(LoginResultCode.Deleted);
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
            yield return ("authtype", AuthMode.AuthenticationServerServiceAccount);
            yield return ("authenticationServerClientId", _authenticationServerClientId);
            
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
