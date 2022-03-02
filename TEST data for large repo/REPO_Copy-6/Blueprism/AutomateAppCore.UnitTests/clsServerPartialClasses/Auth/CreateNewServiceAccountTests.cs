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
    public class CreateNewServiceAccountTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private Mock<IUniqueUsernameGenerator> _uniqueUsernameGenerator;
        private IUser _createNewUserArgument;

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

            _createNewUserArgument = null;
            ReflectionHelper.SetPrivateField("mCreateNewUser", _server, (Action<IDatabaseConnection, IUser>)((con, user) => _createNewUserArgument = user));
        }

        [Test]
        public void CreateNewServiceAccount_NameIsUnique_ShouldCreateNewServiceAccountUserWithUnchangedName()
        {
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));
            _server.CreateNewServiceAccount("client1", "client1Id", true);
            _createNewUserArgument.Name.Should().Be("client1");
            _createNewUserArgument.AuthServerName.Should().Be("client1");
            _createNewUserArgument.AuthType.Should().Be(AuthMode.AuthenticationServerServiceAccount);
        }

        [Test]
        public void CreateNewServiceAccount_AnotherServiceAccountWithSameClientIdExists_ExceptionThrown()
        {
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));
            ReflectionHelper.SetPrivateField("mCreateNewUser", _server, (Action<IDatabaseConnection, IUser>)((con, user) => throw new AuthenticationServerClientIdAlreadyInUseException()));

            _server.Invoking((server) => server.CreateNewServiceAccount("client1", "client1Id", true))
                .ShouldThrow<AuthenticationServerClientIdAlreadyInUseException>();
        }

        [Test]
        public void CreateNewServiceAccount_AnotherServiceAccountWithSameName_ShouldCreateNewServiceAccountWithUniqueName()
        {
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.NewGuid()));
            _server.CreateNewServiceAccount("client1", "client1Id", true);
            _createNewUserArgument.Name.Should().Be("unique username");
            _createNewUserArgument.AuthServerName.Should().Be("client1");
            _createNewUserArgument.AuthType.Should().Be(AuthMode.AuthenticationServerServiceAccount);
        }

        [Test]
        public void CreateNewServiceAccount_hasBluePrismApiScopeIsFalse_ShouldCreateNewServiceAccountUserWithBluePrismApiScopeSetToFalse()
        {
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));
            _server.CreateNewServiceAccount("client1", "client1Id", false);
            _createNewUserArgument.HasBluePrismApiScope.Should().Be(false);
        }

        [Test]
        public void CreateNewServiceAccount_hasBluePrismApiScopeIsTrue_ShouldCreateNewServiceAccountUserWithBluePrismApiScopeSetToTrue()
        {
            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) => Guid.Empty));
            _server.CreateNewServiceAccount("client1", "client1Id", true);
            _createNewUserArgument.HasBluePrismApiScope.Should().Be(true);
        }

        [Test]
        public void CreateNewServiceAccount_FirstAttemptWithUniqueNameGeneratorDoesNotGenerateUniqueName_ShouldCreateNewServiceAccountWithAnotherUniqueName()
        {

            _uniqueUsernameGenerator.SetupSequence(x => x.GenerateUsername(It.IsAny<string>()))
                .Returns("unique name 1")
                .Returns("unique name 2");


            ReflectionHelper.SetPrivateField("mTryGetUserId", _server, (Func<IDatabaseConnection, string, Guid>)((con, username) =>
            {
                if (username == "client1" || username == "unique name 1")
                {
                    return Guid.NewGuid();
                }

                return Guid.Empty;
            }));

            ReflectionHelper.SetPrivateField("mCreateNewUser", _server, (Action<IDatabaseConnection, IUser>)((con, user) =>
            {
                if (user.Name == "user1" || user.Name == "unique name 1")
                {
                    throw new NameAlreadyExistsException();
                }

                _createNewUserArgument = user;
            }));

            _server.CreateNewServiceAccount("client1", "client1Id", true);

            _createNewUserArgument.Name.Should().Be("unique name 2");
            _createNewUserArgument.AuthType.Should().Be(AuthMode.AuthenticationServerServiceAccount);
        }

    }
}

