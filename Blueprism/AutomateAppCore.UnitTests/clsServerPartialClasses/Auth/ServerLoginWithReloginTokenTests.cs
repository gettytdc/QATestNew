#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.ClientServerConnection;
using BluePrism.AutomateAppCore.DataMonitor;
using BluePrism.Common.Security;
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
    public class ServerLoginWithReloginTokenTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IDataReader> _userDataReaderMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly Guid _userId = Guid.NewGuid();
        private int _authTypeColumnIndex;
        private int _isDeletedColumnIndex;
        private Mock<IDataReader> _userRoleDataReaderMock;
        private readonly SafeString _reloginToken = new SafeString(Guid.NewGuid().ToString());
        private readonly string _userName = "Testy McTesterson";
        private readonly string _roleName = "Special Role";
        
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

            SetUpUserDataReaderMock();
            SetUpUserRoleDataReaderMock();
            SetUpSystemRoleSetMock();
            SetUpAuditRecordWriterMock();
            
            var logonOptions = new LogonOptions() { SingleSignon = false, AuthenticationServerUrl = "https://authenticationserver" };
            SetLogOnOptions(logonOptions);
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldReturnSucess()
        {          
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            var reloginResult = _server.LoginWithReloginToken("en-us", reloginTokenRequest);
            reloginResult.LoginResult.Code.Should().Be(LoginResultCode.Success);
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldReturnNewReloginToken()
        {
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            var reloginResult = _server.LoginWithReloginToken("en-us", reloginTokenRequest);
            reloginResult.ReloginToken.AsString().Should().NotBeNull().And.Should().NotBe(_reloginToken);
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldReturnUserWithCorrectRoles()
        {
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            var reloginResult = _server.LoginWithReloginToken("en-us", reloginTokenRequest);
            reloginResult.LoginResult.User.Roles.Select(x => x.Name).Should().BeEquivalentTo(new[] { _roleName });
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldSetLoggedInUserOnServer()
        {
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            _server.LoginWithReloginToken("en-us", reloginTokenRequest);

            var loggedInUser = GetPrivateServerField("mLoggedInUser");
            loggedInUser.Should().BeAssignableTo<IUser>();
            ((IUser)loggedInUser).Name.Should().Be(_userName);
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldSetLoggedInMachineOnServer()
        {
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            _server.LoginWithReloginToken("en-us", reloginTokenRequest);

            var loggedInMachine = GetPrivateServerField("mLoggedInMachine");
            loggedInMachine.Should().BeOfType<string>();
            ((string)loggedInMachine).Should().Be("thismachine");
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldSetLoggedInModeOnServer()
        {
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            _server.LoginWithReloginToken("en-us", reloginTokenRequest);

            var loggedInUser = GetPrivateServerField("mLoggedInMode");
            loggedInUser.Should().BeOfType<AuthMode>();
            ((AuthMode)loggedInUser).Should().Be(AuthMode.AuthenticationServer);
        }

        [Test]
        public void LoginWithReloginToken_ValidReloginToken_ShouldSetLoggedInLocaleOnServer()
        {
            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            _server.LoginWithReloginToken("en-us", reloginTokenRequest);

            var loggedInLocale = GetPrivateServerField("mLoggedInUserLocale");
            loggedInLocale.Should().BeOfType<string>();
            ((string)loggedInLocale).Should().Be("en-us");
        }

        [Test]
        public void LoginWithReloginToken_InvalidReloginToken_ShouldReturnInvalidReloginToken()
        {
            ReflectionHelper.SetPrivateField("mValidateReloginToken", _server,
                (Func<IDatabaseConnection, ReloginTokenRequest, Guid>)((con, tokenRequest) => Guid.Empty));

            var invalidReloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, new SafeString("InvalidToken"));
            var reloginResult = _server.LoginWithReloginToken("en-us", invalidReloginTokenRequest);
            reloginResult.LoginResult.Code.Should().Be(LoginResultCode.InvalidReloginToken);
        }

        [Test]
        public void LoginWithReloginToken_NotAuthenticationServerUser_ShouldReturnTypeMismatch()
        {
            _userDataReaderMock.Setup(x => x[_authTypeColumnIndex]).Returns(0);

            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            var reloginResult = _server.LoginWithReloginToken("en-us", reloginTokenRequest);
            reloginResult.LoginResult.Code.Should().Be(LoginResultCode.TypeMismatch);
        }

        [Test]
        public void LoginWithReloginToken_UserDeleted_ShouldThrow()
        {
            _userDataReaderMock.Setup(x => x[_isDeletedColumnIndex]).Returns(true);

            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            Action reloginAction = () => _server.LoginWithReloginToken("en-us", reloginTokenRequest);
            reloginAction.ShouldThrow<DeletedException>();
        }

        [Test]
        public void LoginWithReloginToken_UserNotFound_ShouldThrow()
        {
            _userDataReaderMock.Setup(x => x.Read()).Returns(false);

            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            Action reloginAction = () => _server.LoginWithReloginToken("en-us", reloginTokenRequest);
            reloginAction.ShouldThrow<UserNotFoundException>();
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

            logOnOptionsDataReaderMock.Setup(x => x.GetName(3)).Returns("enablemappedactivedirectoryauth");
            logOnOptionsDataReaderMock.Setup(x => x[3]).Returns(logonOptions.MappedActiveDirectoryAuthenticationEnabled);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(4)).Returns("authenticationserverurl");
            logOnOptionsDataReaderMock.Setup(x => x[4]).Returns(logonOptions.AuthenticationServerUrl);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(5)).Returns("enableauthenticationserverauth");
            logOnOptionsDataReaderMock.Setup(x => x[5]).Returns(logonOptions.AuthenticationServerAuthenticationEnabled);

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
            yield return ("authtype", 7);
        }

        private void SetUpUserDataReaderMock()
        {
            _userDataReaderMock = new Mock<IDataReader>();

            _userDataReaderMock.SetupSequence(x => x.Read()).Returns(true).Returns(false).Returns(true).Returns(false);
            var userTableQueryResults = UserTableQueryResults().ToList();
            _userDataReaderMock.Setup(x => x.FieldCount).Returns(userTableQueryResults.Count);
            for (var i = 0; i < userTableQueryResults.Count; i++)
            {
                var (fieldName, value) = userTableQueryResults[i];
                if (fieldName == "isdeleted")
                {
                    _isDeletedColumnIndex = i;
                }
                if (fieldName == "authtype")
                {
                    _authTypeColumnIndex = i;
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
