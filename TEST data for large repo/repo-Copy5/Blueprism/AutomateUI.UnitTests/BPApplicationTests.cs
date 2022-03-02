using System;
using System.Reflection;
using AutomateUI.Classes;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.ClientServerConnection;
using BluePrism.AutomateAppCore.Config;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Common.Security;
using BluePrism.DigitalWorker;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateUI.UnitTests
{
    [TestFixture]
    public partial class BpApplicationTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            LegacyUnitTestHelper.UnsetDependencyResolver();
        }

        [Test]
        public void StartProcessAlerts_NoNativeUserCredentialsOrSsoArgumentSupplied_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/alerts" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
        }

        [Test]
        public void StartProcessAlerts_SsoDatabaseWithNativeUserCredentialsSupplied_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            SetupServerMock(DatabaseType.SingleSignOn);
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/alerts", "/user", "test", "test" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
        }

        [Test]
        public void StartProcessAlerts_SsoUserDoesNotHaveSubscribeToProcessAlertsPermission_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userMock = GetMock<IUser>();
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(l => l.LoginWithMappedActiveDirectoryUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            Func<bool> isUserLoggedIn = () => true;
            SetApplicationFieldValue("IsUserLoggedIn", isUserLoggedIn);
            Func<bool> isUserSubscribedToProcessAlertsPermission = () => false;
            SetApplicationFieldValue("IsUserSubscribedToProcessAlertsPermission", isUserSubscribedToProcessAlertsPermission);
            Func<string> currentUserName = () => "test";
            SetApplicationFieldValue("CurrentUserName", currentUserName);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/alerts", "/sso" });
            userLoginMock.Verify(m => m.LoginWithMappedActiveDirectoryUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>()), Times.Once);
            userMessageMock.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            isUserLoggedIn = () => false;
            SetApplicationFieldValue("IsUserLoggedIn", isUserLoggedIn);
            currentUserName = () => null;
            SetApplicationFieldValue("CurrentUserName", currentUserName);
        }

        [Test]
        public void StartProcessAlerts_NativeUserDoesNotHaveSubscribeToProcessAlertsPermission_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userMock = GetMock<IUser>();
            var userLoginMock = GetMock<IUserLogin>() as Mock<IUserLogin>;
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            Func<bool> isUserLoggedIn = () => true;
            SetApplicationFieldValue("IsUserLoggedIn", isUserLoggedIn);
            Func<bool> isUserSubscribedToProcessAlertsPermission = () => false;
            SetApplicationFieldValue("IsUserSubscribedToProcessAlertsPermission", isUserSubscribedToProcessAlertsPermission);
            Func<string> currentUserName = () => "test";
            SetApplicationFieldValue("CurrentUserName", currentUserName);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/alerts", "/user", "test", "test" });
            userLoginMock.Verify(m => m.Login(It.IsAny<string>(), "test", "test".AsSecureString(), It.IsAny<string>(), It.IsAny<IServer>()), Times.Once);
            userMessageMock.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            isUserLoggedIn = () => false;
            SetApplicationFieldValue("IsUserLoggedIn", isUserLoggedIn);
            currentUserName = () => null;
            SetApplicationFieldValue("CurrentUserName", currentUserName);
        }

        [Test]
        public void StartResourcePc_NoUserAndNoPublicArgument_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var result = BPApplication.Start(new[] { "/resourcepc" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
        }

        [Test]
        public void StartResorcePc_WithPublicArgumentButNoResourcePc_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var result = BPApplication.Start(new[] { "/public" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
        }

        [Test]
        [TestCase("/setdbserver")]
        [TestCase("/setdbname")]
        [TestCase("/setdbusername")]
        public void StartResourcePc_IllegalArguments_ShouldFail(string argument)
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var result = BPApplication.Start(new[] { "/resourcepc", argument, "test" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
        }

        [Test]
        public void StartResourcePc_SsoDatabaseWithUserCredentialsSupplied_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            SetupServerMock(DatabaseType.SingleSignOn);
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/user", "test", "test" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_NativeDatabaseWithUserCredentialArguments_ShouldAttemptLoginWithUserCredentials()
        {
            var passedUsername = "";
            var passedPassword = "".AsSecureString();
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(), It.IsAny<IServer>())).Callback((string machine, string username, SafeString password, string locale, IServer server) =>
            {
                passedUsername = username;
                passedPassword = password;
            }).Returns(new LoginResult(LoginResultCode.BadCredentials));
            SetUserLoginMock(userLoginMock );
            SetupServerMock();
            SetupOptionsMock();
            BPApplication.Start(new[] { "/resourcepc", "/user", "username", "password" });
            passedUsername.Should().Be("username");
            passedPassword.AsString().Should().Be("password");
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_NativeDatabaseWithSsoArgument_ShouldAttemptMappedActiveDirectoryUserLogin()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.LoginWithMappedActiveDirectoryUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.UnableToFindUser));
            SetUserLoginMock(userLoginMock );
            SetupServerMock();
            SetupOptionsMock();
            BPApplication.Start(new[] { "/resourcepc", "/sso" });
            userLoginMock.Verify(m => m.LoginWithMappedActiveDirectoryUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>()), Times.Once);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_SingleSignonDatabaseWithSsoArgument_ShouldAttemptActiveDirectoryUserLogin()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.UnableToFindUser));
            SetUserLoginMock(userLoginMock);
            SetupServerMock(DatabaseType.SingleSignOn);
            SetupOptionsMock();
            BPApplication.Start(new[] { "/resourcepc", "/sso" });
            userLoginMock.Verify(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>()), Times.Once);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_NativeUserLoginWithBadCredentials_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.BadCredentials));
            SetUserLoginMock(userLoginMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/user", "username", "password" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_NativeUserLoginThrowsException_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(), It.IsAny<IServer>())).Throws(new UnknownLoginException(new Exception()));
            SetUserLoginMock(userLoginMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/user", "username", "password" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_AnonymousLoginWhenAnonymousLoginIsDisabled_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.LoginAsAnonResource(It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.AnonymousDisabled));
            SetUserLoginMock(userLoginMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/public" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_AnonymousLoginThrowsException_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.LoginAsAnonResource(It.IsAny<string>(), It.IsAny<IServer>())).Throws(new UnknownLoginException(new Exception()));
            SetUserLoginMock(userLoginMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/public" });
            userMessageMock.Verify(m => m.Show(It.IsAny<string>()), Times.Once);
            result.Should().Be(1);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_SuccessfulNativeUserLogin_ShouldStartResource()
        {
            var userMock = GetMock<IUser>();
            userMock.Setup(m => m.HasPermission(Permission.Resources.AuthenticateAsResource)).Returns(true);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            var coordinatorMock = GetMock<IResourceRunner>();
            coordinatorMock.Setup(r => r.IsRunning()).Returns(false);
            SetResourceRunnerMock(coordinatorMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/user", "username", "password" });
            coordinatorMock.Verify(m => m.Init(It.IsAny<Action>()), Times.Once);
            result.Should().Be(0);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_SuccessfulActiveDirectoryUserLogin_ShouldStartResource()
        {
            var userMock = GetMock<IUser>();
            userMock.Setup(m => m.HasPermission(Permission.Resources.AuthenticateAsResource)).Returns(true);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            var coordinatorMock = GetMock<IResourceRunner>();
            coordinatorMock.Setup(r => r.IsRunning()).Returns(false);
            SetResourceRunnerMock(coordinatorMock);
            SetupServerMock(DatabaseType.SingleSignOn);
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/sso" });
            coordinatorMock.Verify(m => m.Init(It.IsAny<Action>()), Times.Once);
            result.Should().Be(0);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_SuccessfulMappedActiveDirectoryUserLogin_ShouldStartResource()
        {
            var userMock = GetMock<IUser>();
            userMock.Setup(m => m.HasPermission(Permission.Resources.AuthenticateAsResource)).Returns(true);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.LoginWithMappedActiveDirectoryUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            var coordinatorMock = GetMock<IResourceRunner>();
            coordinatorMock.Setup(r => r.IsRunning()).Returns(false);
            SetResourceRunnerMock(coordinatorMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/sso" });
            coordinatorMock.Verify(m => m.Init(It.IsAny<Action>()), Times.Once);
            result.Should().Be(0);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_SuccessfulAnonymousLogin_ShouldStartResource()
        {
            var userMock = GetMock<IUser>();
            userMock.Setup(m => m.HasPermission(Permission.Resources.AuthenticateAsResource)).Returns(true);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.LoginAsAnonResource(It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            var coordinator = GetMock<IResourceRunner>();
            coordinator.Setup(r => r.IsRunning()).Returns(false);
            SetResourceRunnerMock(coordinator);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/public" });
            coordinator.Verify(m => m.Init(It.IsAny<Action>()), Times.Once);
            result.Should().Be(0);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_UserDoesNotHaveAuthenticateAsResourcePermission_ShouldFail()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            var userMock = GetMock<IUser>();
            userMock.Setup(m => m.HasPermission(Permission.Resources.AuthenticateAsResource)).Returns(false);
            var userLoginMock = GetMock<IUserLogin>();
            userLoginMock.Setup(m => m.LoginAsAnonResource(It.IsAny<string>(), It.IsAny<IServer>())).Returns(new LoginResult(LoginResultCode.Success, userMock.Object));
            SetUserLoginMock(userLoginMock);
            var resourceRunnerMock = GetMock<IResourceRunner>();
            resourceRunnerMock.Setup(r => r.IsRunning()).Returns(false);
            SetResourceRunnerMock(resourceRunnerMock);
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/resourcepc", "/public" });
            userMessageMock.Verify(m => m.ShowPermissionMessage(), Times.Once);
            result.Should().Be(1);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_DigitalWorkerArgumentSpecified_ShouldAllowBasedOnFeatureToggle()
        {
            SetupServerMock();
            SetupOptionsMock();
            var result = BPApplication.Start(new[] { "/digitalworker", "worker1" });
            var featureToggle = new DigitalWorkerFeatureToggle();
            var expectedResult = featureToggle.FeatureEnabled ? 0 : 1;
            result.Should().Be(expectedResult);
            SetServerFactoryMock(null);
        }

        [Test]
        public void StartResourcePc_DigitalWorkerExceedingMaxLengthWhenFeatureEnabled_ShouldFailWithWarning()
        {
            var userMessageMock = GetMock<IUserMessage>();
            SetUserMessageMock(userMessageMock);
            SetupServerMock();
            SetupOptionsMock();
            var digitalWorkerName = "".PadRight(1025, 'x');
            var result = BPApplication.Start(new[] { "/digitalworker", digitalWorkerName });
            var featureToggle = new DigitalWorkerFeatureToggle();
            if (featureToggle.FeatureEnabled)
            {
                result.Should().Be(1);
                userMessageMock.Verify(m => m.Show(My.Resources.Resources.StartupParams_DigitalWorker_ValidationWarning), Times.Once);
            }

            SetServerFactoryMock(null);
        }

        private static void SetupServerMock(DatabaseType databaseType = DatabaseType.NativeAndExternal)
        {
            var serverMock = GetMock<IServer>();
            serverMock.Setup(m => m.DatabaseType()).Returns(databaseType);
            var serverManagerMock = GetMock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(serverMock.Object);
            var serverFactoryMock = GetMock<IServerFactory>();
            serverFactoryMock.SetupGet(m => m.ServerManager).Returns(serverManagerMock.Object);
            SetServerFactoryMock(serverFactoryMock);
        }

        private static void SetupOptionsMock()
        {
            var mockConnectionSetting = new clsDBConnectionSetting("Test", "Test", 1234, ServerConnection.Mode.WCFInsecure, 4321);
            var optionsMock = GetMock<IOptions>();
            optionsMock.Setup(m => m.DbConnectionSetting).Returns(mockConnectionSetting);
            optionsMock.SetupGet(m => m.SelectedConfigEncryptionMethod).Returns(MachineConfig.ConfigEncryptionMethod.BuiltIn);
            SetApplicationFieldMock("ConfigOptions", optionsMock);
        }

        private static void SetUserMessageMock(Mock<IUserMessage> mock)
        {
            SetApplicationFieldMock("UserMessage", mock);
        }

        private static void SetServerFactoryMock(Mock<IServerFactory> mock)
        {
            SetApplicationFieldMock("ServerFactory", mock);
            SetGlobalFieldMock("ServerFactory", mock);
        }

        private static void SetUserLoginMock(Mock<IUserLogin> mock)
        {
            SetApplicationFieldMock("UserLogin", mock);
        }

        private static void SetResourceRunnerMock(Mock<IResourceRunner> runnerMock)
        {
            var factory = GetMock<IResourceRunnerFactory>();
            var viewMock = GetMock<IResourcePCView>();
            var components = new ResourceRunnerComponents(viewMock.Object as IResourcePCView, runnerMock.Object);
            factory.Setup(x => x.Create(It.IsAny<ResourcePCStartUpOptions>())).Returns(components);
            SetApplicationFieldValue("ResourceRunnerFactory", factory.Object);
        }

        private static void SetApplicationFieldMock<T>(string fieldName, IMock<T> mock) where T : class
        {
            SetApplicationFieldValue(fieldName, mock?.Object);
        }

        private static void SetApplicationFieldValue(string fieldName, object value)
        {
            SetFieldValue(Assembly.Load("AutomateUI"), "AutomateUI.BPApplication", fieldName, value);
        }

        private static void SetGlobalFieldMock<T>(string fieldName, IMock<T> mock) where T : class
        {
            SetFieldMock(Assembly.Load("AutomateAppCore"), "BluePrism.AutomateAppCore.app", fieldName, mock);
        }

        private static void SetFieldMock<T>(Assembly assembly, string typeName, string fieldName, IMock<T> mock) where T : class
        {
            SetFieldValue(assembly, typeName, fieldName, mock?.Object);
        }

        private static void SetFieldValue(Assembly assembly, string typeName, string fieldName, object value)
        {
            assembly.
                GetType(typeName).
                GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).
                SetValue(null, value);
        }

        private static Mock<T> GetMock<T>() where T : class
        {
            return new Mock<T>();
        }
    }
}