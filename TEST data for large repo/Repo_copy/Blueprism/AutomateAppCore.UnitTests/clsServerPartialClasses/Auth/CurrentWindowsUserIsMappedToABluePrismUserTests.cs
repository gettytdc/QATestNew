#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.Core.WindowsSecurity;
using BluePrism.Data;
using BluePrism.UnitTesting;
using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.Auth
{
    [TestFixture]
    public class CurrentWindowsUserIsMappedToABluePrismUserTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IWindowsIdentity> _windowsIdentityMock;
        private Mock<IDataReader> _mappedUserDataReaderMock;
        private Mock<IDataReader> _userDataReaderMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly Guid _userId = Guid.NewGuid();
        private int _isDeletedColumnIndex;
        private readonly string _userName = "Testy McTesterson";

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
        }

        [Test]
        public void CurrentWindowsUserIsMappedToABluePrismUser_SingleSignOnDatabase_ShouldReturnFalse()
        {
            SetServerMockDatabaseType(DatabaseType.SingleSignOn);
            var result = _server.CurrentWindowsUserIsMappedToABluePrismUser();
            result.Should().Be(false);
        }

        [Test]
        public void CurrentWindowsUserIsMappedToABluePrismUser_CouldNotWorkOutClientIdentity_ShouldReturnFalse()
        {
            ReflectionHelper.SetPrivateField("mGetClientIdentity", _server, (Func<IWindowsIdentity>)(() => null));
            var result = _server.CurrentWindowsUserIsMappedToABluePrismUser();
            result.Should().Be(false);
        }

        [Test]
        public void CurrentWindowsUserIsMappedToABluePrismUser_ClientIdentityIsNotMappedToABluePrismUser_ShouldReturnFalse()
        {
            _mappedUserDataReaderMock.Setup(x => x.Read()).Returns(false);
            var result = _server.CurrentWindowsUserIsMappedToABluePrismUser();
            result.Should().Be(false);
        }

        [Test]
        public void CurrentWindowsUserIsMappedToABluePrismUser_UserHasBeenDeleted_ShouldReturnFalse()
        {
            _userDataReaderMock.Setup(x => x[_isDeletedColumnIndex]).Returns(true);
            var result = _server.CurrentWindowsUserIsMappedToABluePrismUser();
            result.Should().Be(false);
        }

        [Test]
        public void CurrentWindowsUserIsMappedToABluePrismUser_UserNotFound_ShouldReturnFalse()
        {
            _userDataReaderMock.Setup(x => x.Read()).Returns(false);
            var result = _server.CurrentWindowsUserIsMappedToABluePrismUser();
            result.Should().Be(false);
        }

        [Test]
        public void CurrentWindowsUserIsMappedToABluePrismUser_ClientIdentityMappedToABluePrismUser_ShouldReturnTrue()
        {
            var result = _server.CurrentWindowsUserIsMappedToABluePrismUser();
            result.Should().Be(true);
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
    }
}
#endif
