using System;
using System.Data.SqlClient;
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
    public class RetireAuthenticationServerUserTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private DateTimeOffset _synchronizationDate;
        private bool _retiredArgument;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

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

            _synchronizationDate = _now;
            _retiredArgument = false;

            ReflectionHelper.SetPrivateField("mUpdateRetireAuthenticationServerUser", _server, (Action<IDatabaseConnection, DateTimeOffset, bool, Guid>)((con, synchronizationDate, retired, user) =>
            {
                _retiredArgument = retired;
                _synchronizationDate = synchronizationDate;
            }));
        }

        [Test]
        public void RetireAuthenticationServerUser_NotTheMostRecentSyncrhonizationDate_ShouldThrowException()
        {
            ReflectionHelper.SetPrivateField("mGetUserByAuthenticationServerUserId", _server, (Func<IDatabaseConnection, Guid, User>)((con, username) => throw new SynchronizationOutOfSequenceException()));

            _server.Invoking((server) => server.RetireAuthenticationServerUser(Guid.NewGuid(), It.IsAny<DateTimeOffset>()))
                .ShouldThrow<SynchronizationOutOfSequenceException>();
        }

        [Test]
        public void RetireAuthenticationServerUser_IsTheMostRecentSynchronizationDate_ShouldSetUserToDeleted()
        {
            var user = new User(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>());

            ReflectionHelper.SetPrivateField("mGetUserByAuthenticationServerUserId", _server, (Func<IDatabaseConnection, Guid, User>)((con, username) => user));

            _server.RetireAuthenticationServerUser(Guid.NewGuid(), It.IsAny<DateTimeOffset>());
            _retiredArgument.Should().BeTrue();
        }

        [Test]
        public void RetireAuthenticationServerUser_IsTheMostRecentSynchronizationDate_ShouldSetLastSyncrhonizationDate()
        {
            var user = new User(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>());

            ReflectionHelper.SetPrivateField("mGetUserByAuthenticationServerUserId", _server, (Func<IDatabaseConnection, Guid, User>)((con, username) => user));

            _server.RetireAuthenticationServerUser(Guid.NewGuid(), _now);
            _synchronizationDate.Should().Be(_now);
        }

        [Test]
        public void RetireAuthenticationServerUser_NoUserWithAuthenticationServerUserIdExists_ShouldThrowException()
        {
            ReflectionHelper.SetPrivateField("mGetUserByAuthenticationServerUserId", _server, (Func<IDatabaseConnection, Guid, User>)((con, username) => throw new NoSuchElementException()));

            _server.Invoking((server) => server.RetireAuthenticationServerUser(Guid.NewGuid(), It.IsAny<DateTimeOffset>()))
                .ShouldThrow<NoSuchElementException>();
        }
    }
}
