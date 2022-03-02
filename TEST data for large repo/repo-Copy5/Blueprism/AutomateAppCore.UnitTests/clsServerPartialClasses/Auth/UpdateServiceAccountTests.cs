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
    public class UpdateServiceAccountTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private Mock<IUniqueUsernameGenerator> _uniqueUsernameGenerator;
        private IUser _updateMappedUserArgument;

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

            _uniqueUsernameGenerator = new Mock<IUniqueUsernameGenerator>();
            _uniqueUsernameGenerator.Setup(x => x.GenerateUsername(It.IsAny<string>())).Returns("unique username");
            ReflectionHelper.SetPrivateField("mUniqueUserNameGenerator", _server, _uniqueUsernameGenerator.Object);

            _updateMappedUserArgument = null;
            ReflectionHelper.SetPrivateField("mUpdateMappedUser", _server, (Action<IDatabaseConnection, IUser>)((con, user) => _updateMappedUserArgument = user));
        }

        [Test]
        public void UpdateServiceAccount_UserExistsInDatabaseAndNewNameIsUnique_ShouldUpdateServiceAccountUserWithProvidedNameAndScope()
        {
            var existingUser = new User(AuthMode.AuthenticationServerServiceAccount, Guid.NewGuid(), "client1")
            {
                HasBluePrismApiScope = false
            };

            ReflectionHelper.SetPrivateField("mGetUserByClientId", _server, (Func<IDatabaseConnection, string, User>)((con, username) => existingUser));
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));

            _server.UpdateServiceAccount("client1Id", "client2", true, DateTimeOffset.Now);
            _updateMappedUserArgument.Name.Should().Be("client2");
            _updateMappedUserArgument.AuthServerName.Should().Be("client2");
            _updateMappedUserArgument.HasBluePrismApiScope.Should().BeTrue();
        }

        [Test]
        public void UpdateServiceAccount_SynchronizationDateIsEarlierThanLastSynchronizationDate_SynchronizationOutOfSequenceExceptionThrown()
        {
            var existingUser = new User(AuthMode.AuthenticationServerServiceAccount, Guid.NewGuid(), "client1")
            {
                UpdatedLastSynchronizationDate = DateTimeOffset.Now + TimeSpan.FromMinutes(30)
            };

            ReflectionHelper.SetPrivateField("mGetUserByClientId", _server, (Func<IDatabaseConnection, string, User>)((con, username) => existingUser));
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));

            _server.Invoking((server) => server.UpdateServiceAccount("client1Id", "client2", true, DateTimeOffset.Now))
                .ShouldThrow<SynchronizationOutOfSequenceException>();
        }

        [Test]
        public void UpdateServiceAccount_ClientNameHasNotChangedHasScopeFlagHasChanged_HasScopeFlagIsUpdatedNameRemainsTheSame()
        {
            var existingUser = new User(AuthMode.AuthenticationServerServiceAccount, Guid.NewGuid(), "client1")
            {
                HasBluePrismApiScope = true
            };

            ReflectionHelper.SetPrivateField("mGetUserByClientId", _server, (Func<IDatabaseConnection, string, User>)((con, username) => existingUser));
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));

            _server.UpdateServiceAccount("client1Id", "client1", false, DateTimeOffset.Now);
            _updateMappedUserArgument.Name.Should().Be("client1");
            _updateMappedUserArgument.AuthServerName.Should().Be("client1");
            _updateMappedUserArgument.HasBluePrismApiScope.Should().BeFalse();
        }

        [Test]
        public void UpdateServiceAccount_NewClientNameIsNotUnique_UniqueClientNameIsGenerated()
        {
            var existingUser = new User(AuthMode.AuthenticationServerServiceAccount, Guid.NewGuid(), "client1");

            ReflectionHelper.SetPrivateField("mGetUserByClientId", _server, (Func<IDatabaseConnection, string, User>)((con, username) => existingUser));
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.NewGuid()));

            _server.UpdateServiceAccount("client1Id", "client2", false, DateTimeOffset.Now);
            _updateMappedUserArgument.Name.Should().Be("unique username");
            _updateMappedUserArgument.AuthServerName.Should().Be("client2");
        }


        [Test]
        public void UpdateServiceAccount_FirstAttemptWithUniqueNameGeneratorDoesNotGenerateUniqueName_ShouldCreateNewUniqueNameToUpdateServiceAccountWith()
        {
            var existingUser = new User(AuthMode.AuthenticationServerServiceAccount, Guid.NewGuid(), "client1");

            ReflectionHelper.SetPrivateField("mGetUserByClientId", _server, (Func<IDatabaseConnection, string, User>)((con, username) => existingUser));

            _uniqueUsernameGenerator.SetupSequence(x => x.GenerateUsername(It.IsAny<string>()))
                .Returns("unique name 1")
                .Returns("unique name 2");


            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) =>
            {
                if (username == "client2" || username == "unique name 1")
                {
                    return Guid.NewGuid();
                }

                return Guid.Empty;
            }));

            ReflectionHelper.SetPrivateField("mUpdateMappedUser", _server, (Action<IDatabaseConnection, IUser>)((con, user) =>
            {
                if (user.Name == "client2" || user.Name == "unique name 1")
                {
                    throw new NameAlreadyExistsException();
                }

                _updateMappedUserArgument = user;
            }));

            _server.UpdateServiceAccount("client1Id", "client2", false, DateTimeOffset.Now);
            _updateMappedUserArgument.Name.Should().Be("unique name 2");
            _updateMappedUserArgument.AuthServerName.Should().Be("client2");
        }
        
    }
}

