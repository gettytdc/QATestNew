#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.ClientServerConnection;
using BluePrism.AutomateAppCore.DataMonitor;
using BluePrism.Core.WindowsSecurity;
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
    public class ServerLoginWithMappedActiveDirectoryUserTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IWindowsIdentity> _windowsIdentityMock;
        private Mock<IDataReader> _mappedUserDataReaderMock;
        private Mock<IDataReader> _userDataReaderMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly Guid _userId = Guid.NewGuid();
        private int _isDeletedColumnIndex;
        private Mock<IDataReader> _userRoleDataReaderMock;
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

            SetServerMockDatabaseType(DatabaseType.NativeAndExternal);
            SetUpClientIdentityMock();
            SetUpUserMappingDataReaderMock();
            SetUpUserDataReaderMock();
            SetUpUserRoleDataReaderMock();
            SetUpSystemRoleSetMock();
            SetUpAuditRecordWriterMock();
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_SingleSignOnDatabase_ShouldReturnTypeMismatch()
        {
            SetServerMockDatabaseType(DatabaseType.SingleSignOn);
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.TypeMismatch);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_AlreadyLoggedIn_ShouldReturnAlreadyLoggedIn()
        {
            ReflectionHelper.SetPrivateField("mLoggedInUser", _server, new Mock<IUser>().Object);
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.Already);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_CouldNotWorkOutClientIdentity_ShouldReturnUnableToValidateClientIdentity()
        {
            ReflectionHelper.SetPrivateField("mGetClientIdentity", _server, (Func<IWindowsIdentity>)(() => null));
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.UnableToValidateClientIdentity);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_ClientIdentityIsNotAuthenticated_ShouldReturnNotAuthenticated()
        {
            _windowsIdentityMock.Setup(x => x.IsAuthenticated).Returns(false);
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.NotAuthenticated);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_NoUserMappingFound_ShouldReturnNoMappedActiveDirectoryUser()
        {
            _mappedUserDataReaderMock.Setup(x => x.Read()).Returns(false);
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.NoMappedActiveDirectoryUser);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_NoUserUserFound_ShouldThrowUserNotFoundException()
        {
            _userDataReaderMock.Setup(x => x.Read()).Returns(false);

            Action login = () => _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            login.ShouldThrow<UnknownLoginException>();
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_DeletedUserFound_ShouldReturnDeleted()
        {
            _userDataReaderMock.Setup(x => x[_isDeletedColumnIndex]).Returns(true);
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.Deleted);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_LoggedInWithValidUser_ShouldReturnSuccess()
        {
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.Code.Should().Be(LoginResultCode.Success);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_LoggedInWithValidUser_ShouldReturnUserWithCorrectRoles()
        {
            var result = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            result.User.Roles.Select(x => x.Name).Should().BeEquivalentTo(new[] { _roleName });
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_LoggedInWithValidUser_ShouldSetLoggedInUserOnServer()
        {
            _ = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            var loggedInUser = GetPrivateServerField("mLoggedInUser");
            loggedInUser.Should().BeAssignableTo<IUser>();
            ((IUser)loggedInUser).Name.Should().Be(_userName);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_LoggedInWithValidUser_ShouldSetLoggedInMachineOnServer()
        {
            _ = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            var loggedInMachine = GetPrivateServerField("mLoggedInMachine");
            loggedInMachine.Should().BeOfType<string>();
            ((string)loggedInMachine).Should().Be("thismachine");
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_LoggedInWithValidUser_ShouldSetLoggedInModeOnServer()
        {
            _ = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            var loggedInUser = GetPrivateServerField("mLoggedInMode");
            loggedInUser.Should().BeOfType<AuthMode>();
            ((AuthMode)loggedInUser).Should().Be(AuthMode.MappedActiveDirectory);
        }

        [Test]
        public void LoginWithMappedActiveDirectoryUser_LoggedInWithValidUser_ShouldSetLoggedInLocaleOnServer()
        {
            _ = _server.LoginWithMappedActiveDirectoryUser("thismachine", "en-us");
            var loggedInLocale = GetPrivateServerField("mLoggedInUserLocale");
            loggedInLocale.Should().BeOfType<string>();
            ((string)loggedInLocale).Should().Be("en-us");
        }

        private void SetServerMockDatabaseType(DatabaseType databaseType)
        {
            var activeDirectoryProvider = (databaseType == DatabaseType.SingleSignOn) ? "my.domain.com" : string.Empty;

            _databaseConnectionMock
               .Setup(m => m.ExecuteReturnScalar(It.Is<SqlCommand>(x => x.CommandText.Contains("select ActiveDirectoryProvider from BPASysConfig"))))
               .Returns(activeDirectoryProvider);
        }

        private void SetUpClientIdentityMock()
        {
            ReflectionHelper.SetPrivateField("mGetClientIdentity", _server, (Func<IWindowsIdentity>)(() => _windowsIdentityMock.Object));

            _windowsIdentityMock = new Mock<IWindowsIdentity>();
            _windowsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);

            var testSid = "S-1-1-76-18423748-3438888550-264708130-6117";
            _windowsIdentityMock.Setup(x => x.Sid).Returns(new SecurityIdentifier(testSid));
        }

        private void SetUpUserMappingDataReaderMock()
        {
            _mappedUserDataReaderMock = new Mock<IDataReader>();
            _mappedUserDataReaderMock.Setup(x => x.FieldCount).Returns(1);
            _mappedUserDataReaderMock.Setup(x => x.GetName(0)).Returns("bpuserid");
            _mappedUserDataReaderMock.Setup(x => x[0]).Returns(_userId);
            _mappedUserDataReaderMock.Setup(x => x.Read()).Returns(true);

            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.StartsWith("select bpuserid from BPAMappedActiveDirectoryUser where sid"))))
                .Returns(_mappedUserDataReaderMock.Object);
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
            }

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
            _userRoleDataReaderMock.SetupSequence(x => x.Read()).Returns(true).Returns(false);

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
