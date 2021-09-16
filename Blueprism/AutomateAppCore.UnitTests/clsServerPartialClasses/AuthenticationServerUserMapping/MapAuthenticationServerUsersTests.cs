using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.BackgroundJobs;
using BluePrism.AutomateAppCore.BackgroundJobs.Monitoring;
using BluePrism.AutomateAppCore.ClientServerConnection;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer;
using BluePrism.AutomateProcessCore;
using BluePrism.Data;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting;
using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.AuthenticationServerUserMapping
{
    [TestFixture]
    public class MapAuthenticationServerUsersTests
    {
        private User _testUser;
        private const string AuthenticationServerUserMappingStartedAuditCode = "U021";
        private const string AuthenticationServerUserMappingFinishedAuditCode = "U022";

        private static readonly AuthenticationServerUser AuthenticationServerUser = new AuthenticationServerUser
        {
            Id = Guid.NewGuid(),
            Username = "johnNew",
            FirstName = "John",
            LastName = "Wayne",
            Email = "john@email.com",
            CurrentPassword = string.Empty,
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };

        private static readonly UserMappingRecord BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord =
            new UserMappingRecord("john", null, "John", "Wayne", "john@email.com");

        private static readonly UserMappingRecord BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord
            = new UserMappingRecord(string.Empty, AuthenticationServerUser.Id, AuthenticationServerUser.FirstName,
                AuthenticationServerUser.LastName, AuthenticationServerUser.Email);

        private static readonly UserMappingRecord BluePrismUserExists_AuthenticationServerUserExists_MappingRecord
            = new UserMappingRecord("johnny", AuthenticationServerUser.Id, AuthenticationServerUser.FirstName,
                AuthenticationServerUser.LastName, AuthenticationServerUser.Email);

        private static readonly UserMappingRecord NoUserName_NoAuthenticationServerUserId_MappingRecord
            = new UserMappingRecord(null, null, "John", "Smith", "test@email.com");

        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IAuthenticationServerHttpRequester> _authenticationServerHttpRequesterMock;
        private LogonOptions _testLogonOptions;
        private clsServer _classUnderTest;
        private int _convertToAuthenticationServerUserCount;
        private int _createNewUserCallCount;

        [SetUp]
        public void SetUp()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            app.gAuditingEnabled = true;

            _testUser = new User(AuthMode.Native, Guid.NewGuid(), "john");

            _classUnderTest = new clsServer();

            _authenticationServerHttpRequesterMock = new Mock<IAuthenticationServerHttpRequester>();

            _authenticationServerHttpRequesterMock
                .Setup(m => m.GetUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<clsCredential>()))
                .ReturnsAsync(AuthenticationServerUser);
            _authenticationServerHttpRequesterMock
                .Setup(m => m.PostUser(It.IsAny<UserMappingRecord>(), It.IsAny<string>(), It.IsAny<clsCredential>()))
                .ReturnsAsync(AuthenticationServerUser);

            ReflectionHelper.SetPrivateField("mGetUserByName", _classUnderTest, (Func<IDatabaseConnection, string, User>)((con, name) => _testUser));
            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest, (Func<Guid, clsCredential>)(id => new clsCredential() { Type = CredentialType.OAuth2ClientCredentials }));
            ReflectionHelper.SetPrivateField("mAuthenticationServerHttpRequester", _classUnderTest, _authenticationServerHttpRequesterMock.Object);
            ReflectionHelper.SetPrivateField("mConvertToAuthenticationServerUser", _classUnderTest, (Action<IDatabaseConnection, IUser, Guid, string>)((con, user, id, name) => { _convertToAuthenticationServerUserCount++; }));
            ReflectionHelper.SetPrivateField("mCreateNewAuthenticationServerUserWithUniqueName", _classUnderTest, (Action<IDatabaseConnection, string, Guid>)((con, user, id) => { _createNewUserCallCount++; }));
            ReflectionHelper.SetPrivateField("mPermissionValidator", _classUnderTest, new Mock<IPermissionValidator>().Object);
            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", _classUnderTest, (Func<IDatabaseConnection>)(() => _databaseConnectionMock.Object));

            _databaseConnectionMock = new Mock<IDatabaseConnection>();
            _databaseConnectionMock.Setup(x => x.Execute(It.IsAny<SqlCommand>()));

            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(x => x.Server).Returns(_classUnderTest);

            var serverFactoryMock = new Mock<IServerFactory>();
            serverFactoryMock.SetupGet(x => x.ServerManager).Returns(serverManagerMock.Object);
            Assembly.Load("AutomateAppCore")
                .GetType("BluePrism.AutomateAppCore.app")
                .GetField("ServerFactory", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, serverFactoryMock.Object);


            _testLogonOptions = new LogonOptions
            {
                AuthenticationServerUrl = "https://ims:5000",
                AuthenticationServerApiCredentialId = Guid.Parse("C474B268-EC2D-4CED-83F0-5488D1DAB128"),
                SingleSignon = false
            };
            SetLogOnOptions(_testLogonOptions);

            _convertToAuthenticationServerUserCount = 0;
            _createNewUserCallCount = 0;
        }

        [Test]
        public void MapUsers_AuthenticationServerAuthenticationEnabled_ShouldReturnFailedBackgroundJob()
        {
            _testLogonOptions.AuthenticationServerAuthenticationEnabled = true;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Failure);
        }

        [Test]
        public void MapUsers_AuthenticationServerAuthenticationEnabled_ShouldCreateAuthenticationServerUserMappingFinishedAudit()
        {
            _testLogonOptions.AuthenticationServerAuthenticationEnabled = true;
            SetLogOnOptions(_testLogonOptions);

            MapUsers(new List<UserMappingRecord>());

            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [Test]
        public void MapUsers_AuthenticationServerAuthenticationEnabled_ShouldReturnSingleSignOnDatabaseErrorResult()
        {
            var expectedResultData = MapUsersResult.Failed(MapUsersErrorCode.MappingNotAvailableWhenAuthenticationServerEnabled);

            _testLogonOptions.AuthenticationServerAuthenticationEnabled = true;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.ResultData.Should().BeOfType<MapUsersResult>();
            result.Data.ResultData.ShouldBeEquivalentTo(expectedResultData);
        }

        [Test]
        public void MapUsers_ForSingleSignOnDatabase_ShouldReturnFailedBackgroundJob()
        {
            _testLogonOptions.SingleSignon = true;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Failure);
        }

        [Test]
        public void MapUsers_ForSingleSignOnDatabase_ShouldCreateAuthenticationServerUserMappingFinishedAudit()
        {
            _testLogonOptions.SingleSignon = true;
            SetLogOnOptions(_testLogonOptions);

            MapUsers(new List<UserMappingRecord>());

            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [Test]
        public void MapUsers_ForSingleSignOnDatabase_ShouldReturnSingleSignOnDatabaseErrorResult()
        {
            var expectedResultData = MapUsersResult.Failed(MapUsersErrorCode.InvalidActionInSsoEnvironment);

            _testLogonOptions.SingleSignon = true;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.ResultData.Should().BeOfType<MapUsersResult>();
            result.Data.ResultData.ShouldBeEquivalentTo(expectedResultData);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public void MapUsers_NoAuthenticationServerUrl_ShouldReturnFailedBackgroundJob(string authenticationServerUrl)
        {
            _testLogonOptions.AuthenticationServerUrl = authenticationServerUrl;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Failure);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public void MapUsers_NoAuthenticationServerUrl_ShouldCreateAuthenticationServerUserMappingFinishedAudit(string authenticationServerUrl)
        {
            _testLogonOptions.AuthenticationServerUrl = authenticationServerUrl;
            SetLogOnOptions(_testLogonOptions);

            MapUsers(new List<UserMappingRecord>());

            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public void MapUsers_NoAuthenticationServerUrl_ShouldReturnUrlNotSetErrorResult(string authenticationServerUrl)
        {
            var expectedResultData = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerUrlNotSet);

            _testLogonOptions.AuthenticationServerUrl = authenticationServerUrl;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.ResultData.Should().BeOfType<MapUsersResult>();
            result.Data.ResultData.ShouldBeEquivalentTo(expectedResultData);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialIdNotSet_ShouldReturnFailedBackgroundJob()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.Empty;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Failure);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialIdNotSet_ShouldCreateAuthenticationServerUserMappingFinishedAudit()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.Empty;
            SetLogOnOptions(_testLogonOptions);

            MapUsers(new List<UserMappingRecord>());

            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialIdNotSet_ShouldReturnAuthenticationServerCredentialIdNotSetErrorResult()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.Empty;
            SetLogOnOptions(_testLogonOptions);

            var result = MapUsers(new List<UserMappingRecord>());

            var expectedResultData = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerCredentialIdNotSet);
            result.Data.ResultData.ShouldBeEquivalentTo(expectedResultData);
        }


        [Test]
        public void MapUsers_AuthenticationServerCredentialDoesNotExist_ShouldReturnFailedBackgroundJob()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.NewGuid();
            SetLogOnOptions(_testLogonOptions);

            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest,
                (Func<Guid, clsCredential>)(id => throw new NoSuchCredentialException("some credential")));

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Failure);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialDoesNotExist_ShouldCreateAuthenticationServerUserMappingFinishedAudit()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.NewGuid();
            SetLogOnOptions(_testLogonOptions);

            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest,
                (Func<Guid, clsCredential>)(id => throw new NoSuchCredentialException("some credential")));

            MapUsers(new List<UserMappingRecord>());

            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialDoesNotExist_ShouldReturnAuthenticationServerCredentialIdNotSetErrorResult()
        {
            var expectedResultData = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerCredentialIdNotSet);

            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.NewGuid();
            SetLogOnOptions(_testLogonOptions);

            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest,
                (Func<Guid, clsCredential>)(id => throw new NoSuchCredentialException("some credential")));

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.ResultData.ShouldBeEquivalentTo(expectedResultData);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialTypeIsIncorrect_ShouldReturnFailedBackgroundJob()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.NewGuid();
            SetLogOnOptions(_testLogonOptions);

            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest,
                (Func<Guid, clsCredential>)(id => new clsCredential()));

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Failure);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialTypeIsIncorrect_ShouldCreateAuthenticationServerUserMappingFinishedAudit()
        {
            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.NewGuid();
            SetLogOnOptions(_testLogonOptions);

            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest,
                (Func<Guid, clsCredential>)(id => new clsCredential()));

            MapUsers(new List<UserMappingRecord>());

            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [Test]
        public void MapUsers_AuthenticationServerCredentialTypeIsIncorrect_ShouldReturnAuthenticationServerCredentialIdNotSetErrorResult()
        {
            var expectedResultData = MapUsersResult.Failed(MapUsersErrorCode.AuthenticationServerCredentialIdNotSet);

            _testLogonOptions.AuthenticationServerApiCredentialId = Guid.NewGuid();
            SetLogOnOptions(_testLogonOptions);

            ReflectionHelper.SetPrivateField("mGetCredential", _classUnderTest,
                (Func<Guid, clsCredential>)(id => new clsCredential()));

            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.ResultData.ShouldBeEquivalentTo(expectedResultData);
        }

        [Test]
        public void MapUsers_WithNoUsersToMap_Succeeds()
        {
            var result = MapUsers(new List<UserMappingRecord>());

            result.Data.Status.Should().Be(BackgroundJobStatus.Success);
        }

        [Test]
        public void MapUsers_WithAtLeastOneUserToMap_ShouldCreateAuthenticationServerUserMappingStartedAudit()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            MapUsers(inputUsers);
            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingStartedAuditCode);
        }

        [Test]
        public void MapUsers_WithAtLeastOneUserToMap_ShouldCreateAuthenticationServerUserMappingFinishedAudit()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            MapUsers(inputUsers);
            VerifyAuditCodePersistedToDatabase(AuthenticationServerUserMappingFinishedAuditCode);
        }

        [Test]
        public void MapUsers_CreateNewAuthenticationServerUser_ShouldReturnSuccessfulBackgroundJob()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.Status.Should().Be(BackgroundJobStatus.Success);
        }

        [Test]
        public void MapUsers_CreateNewAuthenticationServerUser_ShouldReturnSuccessfulCount()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.ResultData.As<MapUsersResult>().SuccessfullyMappedRecordsCount.Should().Be(1);
        }

        [Test]
        public void MapUsers_CreateNewAuthenticationServerUser_ShouldReturnSuccessfulMapUsersResult()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.ResultData.As<MapUsersResult>().Status.Should().Be(MapUsersStatus.Completed);
        }

        [Test]
        public void MapUsers_CreateNewAuthenticationServerUser_ShouldReturnNoRecordsThatFailedToMap()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.ResultData.As<MapUsersResult>().RecordsThatFailedToMap.Count().Should().Be(0);
        }

        [Test]
        public void MapUsers_CreateNewAuthenticationServerUser_ShouldCallConvertToAuthenticationServer()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            MapUsers(inputUsers);
            _convertToAuthenticationServerUserCount.Should().Be(1);
        }

        [Test]
        public void MapUsers_CreateAuthenticationServerUserButNoEmail_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord> { new UserMappingRecord("john", null, "John", "Wayne", string.Empty) };
            var result = MapUsers(inputUsers);
            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.MissingMappingRecordValues);
        }

        [Test]
        public void MapUsers_CreateAuthenticationServerUserButNoFirstName_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord> { new UserMappingRecord("john", null, string.Empty, "Wayne", "test@email.com") };
            var result = MapUsers(inputUsers);
            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.MissingMappingRecordValues);
        }

        [Test]
        public void MapUsers_CreateAuthenticationServerUserButNoLastName_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord> { new UserMappingRecord("john", null, "John", string.Empty, "test@email.com") };
            var result = MapUsers(inputUsers);
            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.MissingMappingRecordValues);
        }

        [Test]
        public void MapUsers_NoBluePrismUserNameOrAuthenticationServerUserIdInMappingRecord_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord> { NoUserName_NoAuthenticationServerUserId_MappingRecord };
            var result = MapUsers(inputUsers);
            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.MissingMappingRecordValues);
        }

        [Test]
        public void MapUsers_CreateAuthenticationServerUserButAuthenticationServerAlreadyMapped_ShouldRecordThatFailedToMapWithExpectedError()
        {
            ReflectionHelper.SetPrivateField("mConvertToAuthenticationServerUser", _classUnderTest,
                (Action<IDatabaseConnection, IUser, Guid, string>)((con, user, id, name) => { throw new AuthenticationServerUserIdAlreadyInUseException(); }));

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.AuthenticationServerUserAlreadyMappedToAnotherUser);
        }

        [Test]
        public void MapUsers_CreateAuthenticationServerUserButUnexpectedErrorUpdatingBluePrismUser_ShouldRecordThatFailedToMapWithExpectedError()
        {
            ReflectionHelper.SetPrivateField("mConvertToAuthenticationServerUser", _classUnderTest,
                (Action<IDatabaseConnection, IUser, Guid, string>)((con, user, id, name) => { throw new Exception(); }));

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.UnexpectedError);
        }


        [Test]
        public void MapUsers_UnableToCreateAuthenticationServerUser_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserDoesNotExist_MappingRecord };

            _authenticationServerHttpRequesterMock
                .Setup(m => m.PostUser(It.IsAny<UserMappingRecord>(), It.IsAny<string>(), It.IsAny<clsCredential>()))
                .ReturnsAsync((AuthenticationServerUser)null);

            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.ErrorCreatingAuthenticationServerUserRecord);
        }

        [Test]
        public void MapUsers_CreateBluePrismUser_ShouldReturnSuccessfulBackgroundJob()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.Status.Should().Be(BackgroundJobStatus.Success);
        }

        [Test]
        public void MapUsers_CreateBluePrismUser_ShouldReturnCorrectSuccessfulCount()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.ResultData.Should().BeOfType<MapUsersResult>();
            result.Data.ResultData.As<MapUsersResult>().SuccessfullyMappedRecordsCount.Should().Be(1);
        }

        [Test]
        public void MapUsers_CreateBluePrismUser_ShouldCallCreateUser()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord };
            MapUsers(inputUsers);
            _createNewUserCallCount.Should().Be(1);
        }

        [Test]
        public void MapUsers_CreateBluePrismUserButFailedToGetAuthenticationServerUser_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord>
            {
                new UserMappingRecord(string.Empty, AuthenticationServerUser.Id,
                    AuthenticationServerUser.FirstName, AuthenticationServerUser.LastName, AuthenticationServerUser.Email)
            };

            _authenticationServerHttpRequesterMock
                .Setup(m => m.GetUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<clsCredential>()))
                .ReturnsAsync((AuthenticationServerUser)null);

            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.AuthenticationServerUserNotLoaded);
        }

        [Test]
        public void MapUsers_CreateBluePrismUserButExceptionCreatingUser_ShouldRecordThatFailedToMapWithExpectedError()
        {
            ReflectionHelper.SetPrivateField("mCreateNewAuthenticationServerUserWithUniqueName", _classUnderTest, (Action<IDatabaseConnection, string, Guid>)((con, name, id) => { throw new Exception(); }));
            var inputUsers = new List<UserMappingRecord> { BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.UnexpectedError);
        }

        [Test]
        public void MapUsers_MapExistingUsers_ShouldReturnSuccessfulBackgroundJob()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);
            result.Data.Status.Should().Be(BackgroundJobStatus.Success);
        }

        [Test]
        public void MapUsers_MapExistingUsers_ShouldReturnExpectedSuccessfulCount()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            result.Data.ResultData.Should().BeOfType<MapUsersResult>();
            result.Data.ResultData.As<MapUsersResult>().SuccessfullyMappedRecordsCount.Should().Be(1);
        }

        [Test]
        public void MapUsers_MapExistingUsers_ShouldCallConvertToAuthenticationServerUser()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            MapUsers(inputUsers);
            _convertToAuthenticationServerUserCount.Should().Be(1);
        }

        [Test]
        public void MapUsers_MapExistingUsersButAuthenticationServerAlreadyMapped_ShouldRecordThatFailedToMapWithExpectedError()
        {
            ReflectionHelper.SetPrivateField("mConvertToAuthenticationServerUser", _classUnderTest,
                (Action<IDatabaseConnection, IUser, Guid, string>)((con, user, id, name) => { throw new AuthenticationServerUserIdAlreadyInUseException(); }));

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            var resultData = result.Data.ResultData.As<MapUsersResult>();
            resultData.RecordsThatFailedToMap.First().ResultCode.Should().Be(UserMappingResultCode.AuthenticationServerUserAlreadyMappedToAnotherUser);
        }

        [Test]
        public void MapUsers_MapExistingUsersButUnexpectedErrorUpdatingBluePrismUser_ShouldRecordThatFailedToMapWithExpectedError()
        {
            ReflectionHelper.SetPrivateField("mConvertToAuthenticationServerUser", _classUnderTest,
                (Action<IDatabaseConnection, IUser, Guid, string>)((con, user, id, name) => { throw new Exception(); }));

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.UnexpectedError);
        }

        [Test]
        public void MapUsers_MapExistingUsersButButFailedToGetAuthenticationServerUser_ShouldRecordThatFailedToMapWithExpectedError()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };

            _authenticationServerHttpRequesterMock
                .Setup(m => m.GetUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<clsCredential>()))
                .ReturnsAsync((AuthenticationServerUser)null);

            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.AuthenticationServerUserNotLoaded);
        }

        [Test]
        public void MapUsers_BluePrismUserNotFound_ShouldReturnBluePrismUserNotFoundCode()
        {
            ReflectionHelper.SetPrivateField("mGetUserByName", _classUnderTest, (Func<IDatabaseConnection, string, User>)((con, name) => throw new NoSuchElementException()));

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.BluePrismUserNotFound);
        }

        [TestCase(AuthMode.External)]
        [TestCase(AuthMode.MappedActiveDirectory)]
        [TestCase(AuthMode.ActiveDirectory)]
        public void MapUsers_BluePrismUserHasInvalidAuthType_ShouldReturnBluePrismUsersAuthTypeDoesNotSupportMapping(AuthMode authType)
        {
            _testUser = new User(authType, Guid.NewGuid(), "someuser");

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.BluePrismUsersAuthTypeDoesNotSupportMapping);
        }

        [Test]
        public void MapUsers_DeletedBluePrismUser_ShouldReturnBluePrismUserDeleted()
        {
            _testUser.Deleted = true;

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.BluePrismUserDeleted);
        }

        [Test]
        public void MapUsers_BluePrismUserIsSystemUser_ShouldReturnCannotMapSystemUser()
        {
            _testUser = new User(AuthMode.System, Guid.NewGuid(), "scheduler");

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.CannotMapSystemUser);
        }

        [Test]
        public void MapUsers_BluePrismUserIsAnonymousUser_ShouldReturnCannotMapSystemUser()
        {
            _testUser = new User(AuthMode.Anonymous, Guid.NewGuid(), "anonymoususer");

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.CannotMapSystemUser);
        }

        [Test]
        public void MapUsers_BluePrismUserHasAlreadyBeenMapped_ShouldReturnBluePrismUserHasAlreadyBeenMapped()
        {
            _testUser = new User(Guid.NewGuid(), "johnny", Guid.NewGuid(), "johnny");

            var inputUsers = new List<UserMappingRecord> { BluePrismUserExists_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.BluePrismUserHasAlreadyBeenMapped);
        }

        [Test]
        public void MapUsers_UnexpectedExceptionCallingAuthenticationServerApi_ShouldReturnsUnexpectedError()
        {
            _authenticationServerHttpRequesterMock
                .Setup(m => m.GetUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<clsCredential>()))
                .Throws(new Exception());

            var inputUsers = new List<UserMappingRecord> { BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord };
            var result = MapUsers(inputUsers);

            ShouldContainRecordThatFailedToMapWithExpectedResultCode(result, UserMappingResultCode.UnexpectedError);
        }

        [Test]
        public void MapUsers_OneSuccesfulMappingAndOneFailure_ShouldReturnExpectedResult()
        {
            var inputUsers = new List<UserMappingRecord> { BluePrismUserDoesNotExist_AuthenticationServerUserExists_MappingRecord, NoUserName_NoAuthenticationServerUserId_MappingRecord };
            var result = MapUsers(inputUsers);

            result.Data.Status.Should().Be(BackgroundJobStatus.Success);
            var resultData = result.Data.ResultData.As<MapUsersResult>();
            resultData.Status.Should().Be(MapUsersStatus.CompletedWithErrors);
            resultData.RecordsThatFailedToMap.Single().ResultCode.Should().Be(UserMappingResultCode.MissingMappingRecordValues);
            resultData.SuccessfullyMappedRecordsCount.Should().Be(1);
        }

        private void ShouldContainRecordThatFailedToMapWithExpectedResultCode(BackgroundJobResult result, UserMappingResultCode expectedUserMappingResult)
        {
            result.Data.Status.Should().Be(BackgroundJobStatus.Success);
            var resultData = result.Data.ResultData.As<MapUsersResult>();
            resultData.Status.Should().Be(MapUsersStatus.CompletedWithErrors);
            resultData.RecordsThatFailedToMap.First().ResultCode.Should().Be(expectedUserMappingResult);
            resultData.SuccessfullyMappedRecordsCount.Should().Be(0);
        }

        private BackgroundJobResult MapUsers(List<UserMappingRecord> inputUsers)
        {
            var notifier = new BackgroundJobNotifier();

            var job = _classUnderTest.MapAuthenticationServerUsers(inputUsers, notifier);

            return job.Wait(notifier, null).Result;
        }

        private void SetLogOnOptions(LogonOptions logonOptions)
        {
            var logOnOptionsDataReaderMock = new Mock<IDataReader>();
            logOnOptionsDataReaderMock.Setup(x => x.FieldCount).Returns(9);

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

            logOnOptionsDataReaderMock.Setup(x => x.GetName(7)).Returns("enableauthenticationserverauth");
            logOnOptionsDataReaderMock.Setup(x => x[7]).Returns(logonOptions.AuthenticationServerAuthenticationEnabled);

            logOnOptionsDataReaderMock.Setup(x => x.GetName(8)).Returns("authenticationserverapicredential");
            logOnOptionsDataReaderMock.Setup(x => x[8]).Returns(logonOptions.AuthenticationServerApiCredentialId);

            logOnOptionsDataReaderMock.Setup(x => x.Read()).Returns(true);

            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.Is<SqlCommand>(x => x.CommandText.Contains("from BPASysConfig"))))
                .Returns(logOnOptionsDataReaderMock.Object);
        }

        private void VerifyAuditCodePersistedToDatabase(string code) =>
       _databaseConnectionMock.Verify(m => m.ExecuteReturnRecordsAffected(It.Is<SqlCommand>(c =>
           c.CommandText.Contains(
               " insert into BPAAuditEvents (   eventdatetime, sCode, sNarrative, gSrcUserID") &&
           c.Parameters["@code"].Value.Equals(code))), Times.Once);
    }
}
