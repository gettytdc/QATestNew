#if UNITTESTS
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
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
    public class CreateMappedActiveDirectoryUserTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IDataReader> _mappedUserDataReaderMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly Guid _userId = Guid.NewGuid();
        private User _validMappedActiveDirectoryUser;
        
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
            SetUpUserMappingDataReaderMock();

            SetUpAuditRecordWriterMock();

            _validMappedActiveDirectoryUser = User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid());
            _validMappedActiveDirectoryUser.Name = "Testy McTesterson";
            _validMappedActiveDirectoryUser.ExternalId = "S-1-2-34-1234567890-1234567890-1234567890-1234";
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_NullUser_ShouldThrowException()
        {
            Action createMappedUser = () => _server.CreateNewMappedActiveDirectoryUser(null);
            createMappedUser.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_NullExternalId_ShouldThrowException()
        {
            var user = User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid());
            user.ExternalId = null;
            Action createMappedUser = () => _server.CreateNewMappedActiveDirectoryUser(user);
            createMappedUser.ShouldThrow<InvalidArgumentException>();
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_EmptyExternalId_ShouldThrowException()
        {
            var user = User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid());
            user.ExternalId = string.Empty;
            Action createMappedUser = () => _server.CreateNewMappedActiveDirectoryUser(user);
            createMappedUser.ShouldThrow<InvalidArgumentException>();
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_IncorrectAuthType_ShouldThrowException()
        {
            var user = new User(AuthMode.External, Guid.NewGuid(), "Testy Smith")
            {
                ExternalId = "S-1-2-34-1234567890-1234567890-1234567890-1234"
            };
            Action createMappedUser = () => _server.CreateNewMappedActiveDirectoryUser(user);
            createMappedUser.ShouldThrow<InvalidArgumentException>();
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_ValidUser_ShouldCreateNewUserAuditRecord()
        {
            SetupAuditLocaleDataReaderMock();

            _server.CreateNewMappedActiveDirectoryUser(_validMappedActiveDirectoryUser);

            _databaseConnectionMock
               .Verify(m => m.ExecuteReturnRecordsAffected(It.Is<SqlCommand>
               (command => command
                            .Parameters
                            .Cast<SqlParameter>()
                            .Any(parameter => parameter.ParameterName == "@code" && (parameter.Value as string) == "U001"))));
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_ValidUser_ShouldInsertMappingRecordWithSid()
        {
            SetupAuditLocaleDataReaderMock();

            _server.CreateNewMappedActiveDirectoryUser(_validMappedActiveDirectoryUser);

            _databaseConnectionMock
               .Verify(m => m.Execute(It.Is<SqlCommand>
               (command => command
                            .Parameters
                            .Cast<SqlParameter>()
                            .Any(parameter => parameter.ParameterName == "@sid" && (parameter.Value as string) == _validMappedActiveDirectoryUser.ExternalId))));
        }

        [Test]
        public void CreateMappedActiveDirectoryUser_ValidUser_ShouldInsertMappingAuditRecord()
        {
            SetupAuditLocaleDataReaderMock();

            _server.CreateNewMappedActiveDirectoryUser(_validMappedActiveDirectoryUser);

            _databaseConnectionMock
               .Verify(m => m.ExecuteReturnRecordsAffected(It.Is<SqlCommand>
               (command => command
                            .Parameters
                            .Cast<SqlParameter>()
                            .Any(parameter => parameter.ParameterName == "@code" && (parameter.Value as string) == "U013"))));
        }

        private void SetServerMockDatabaseType(DatabaseType databaseType)
        {
            var activeDirectoryProvider = (databaseType == DatabaseType.SingleSignOn) ? "my.domain.com" : string.Empty;

            _databaseConnectionMock
               .Setup(m => m.ExecuteReturnScalar(It.Is<SqlCommand>(x => x.CommandText.Contains("select ActiveDirectoryProvider from BPASysConfig"))))
               .Returns(activeDirectoryProvider);
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

        private void SetupAuditLocaleDataReaderMock()
        {
            _databaseConnectionMock.Setup(m => m.ExecuteReturnScalar(It.Is<IDbCommand>(c => c.CommandText == "select name from sysobjects where id = object_id(@name)")))
                .Returns("MockTable");

            var mockDataReader = new Mock<IDataReader>();
            mockDataReader.Setup(m => m.Read()).Returns(true);
            mockDataReader.SetupGet(m => m[0]).Returns("");

            _databaseConnectionMock.Setup(m => m.ExecuteReturnDataReader(It.IsAny<IDbCommand>())).Returns(mockDataReader.Object);
        }

        private void SetUpAuditRecordWriterMock()
        {
            app.gAuditingEnabled = true;
            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnRecordsAffected(It.Is<SqlCommand>(x => x.CommandText.StartsWith("INSERT INTO BPAAuditEvents"))))
                .Returns(1);
        }
    }
}
#endif
