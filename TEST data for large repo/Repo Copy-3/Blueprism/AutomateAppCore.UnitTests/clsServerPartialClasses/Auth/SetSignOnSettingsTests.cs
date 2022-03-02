#if UNITTESTS
using System;
using System.Data;
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
    public class SetSignOnSettingsTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private Mock<IPermissionValidator> _permissionValidatorMock;
        private readonly PasswordRules _passwordRules = new PasswordRules { PasswordLength = 8, UseDigits = true, UseUpperCase = true };

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
            SetUpAuditRecordWriterMock();
        }

        [Test]
        public void SetSignonSettings_ValidLoginData_ShouldSaveAsExpected()
        {
            SetupAuditLocaleDataReaderMock();
            var authServerCredentialId = Guid.Empty;
            var logonOptions = new LogonOptions()
            {
                MappedActiveDirectoryAuthenticationEnabled = true,
                ExternalAuthenticationEnabled = true,
                AuthenticationGatewayUrl = "https://mydomain",
                AuthenticationServerAuthenticationEnabled = true,
                AuthenticationServerUrl = "https://mydomain",
                AuthenticationServerApiCredentialId = authServerCredentialId
            };

            _server.SetSignonSettings(_passwordRules, logonOptions);
            _databaseConnectionMock
                .Verify(connection => connection.Execute(
                    It.Is<SqlCommand>(x => GetValueOrDefault<string>(x.Parameters, "@AuthenticationGatewayUrl") == "https://mydomain" &&
                                            GetValueOrDefault<bool>(x.Parameters, "@EnableExternalAuth") &&
                                            GetValueOrDefault<bool>(x.Parameters, "@EnableMappedActiveDirectoryAuth") &&
                                            GetValueOrDefault<bool>(x.Parameters, "@EnableAuthenticationServerAuthentication") &&
                                            GetValueOrDefault<string>(x.Parameters, "@AuthenticationServerUrl") == "https://mydomain" &&
                                            GetValueOrDefault<Guid>(x.Parameters, "@AuthenticationServerApiCredentialId") == authServerCredentialId))
                );
        }
        
        [Test]
        public void SetSignonSettings_ValidLoginData_ShouldLogAuditData()
        {
            SetupAuditLocaleDataReaderMock();

            const string expectedAuditText =
                "Enable Active Directory Authentication: True, Enable Authentication Server: True, Authentication Server URL: https://mydomain, Authentication Server credential: ";
            var logonOptions = new LogonOptions()
            {
                MappedActiveDirectoryAuthenticationEnabled = true,
                AuthenticationServerAuthenticationEnabled = true,
                AuthenticationServerUrl = "https://mydomain",
                AuthenticationServerApiCredentialId = Guid.Empty
            };

            _server.SetSignonSettings(_passwordRules, logonOptions);
            _databaseConnectionMock
                .Verify(connection => connection.ExecuteReturnRecordsAffected(
                    It.Is<SqlCommand>(x => x.CommandText.StartsWith("INSERT INTO BPAAuditEvents") &&
                                        GetValueOrDefault<string>(x.Parameters, "@Comments").Contains(expectedAuditText))));
        }

        #region AuthenticationServerAuthentication
       
        [TestCase(@"C:\somedirectory")]
        [TestCase(@"general rubbish")]
        [TestCase(@"http://www.incorrectlyescaped.com/path???/file name")]
        [TestCase(@"www.noprotocol.com")]
        [TestCase(@"/relativepath")]
        public void SetSignonSettings_EnableAuthServerAuthButInvalidUrl_ShouldThrowException(string invalidUrl)
        {
            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = true, AuthenticationServerUrl = invalidUrl };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldThrow<InvalidUrlException>();
        }

        [TestCase(@"C:\somedirectory")]
        [TestCase(@"general rubbish")]
        [TestCase(@"http://www.incorrectlyescaped.com/path???/file name")]
        [TestCase(@"www.noprotocol.com")]
        [TestCase(@"/relativepath")]
        public void SetSignonSettings_DisableAuthServerAuthButInvalidUrl_ShouldThrowException(string invalidUrl)
        {
            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = false, AuthenticationServerUrl = invalidUrl };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldThrow<InvalidUrlException>();
        }

        [Test]
        public void SetSignonSettings_EnableAuthServerAuthButUrlIsBlank_ShouldThrowException()
        {
            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = true, AuthenticationServerUrl = string.Empty };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldThrow<BlankUrlException>();
        }
        [Test]
        public void SetSignonSettings_DisableAuthServerAuthButUrlIsBlank_ShouldNotThrow()
        {
            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = false, AuthenticationServerUrl = string.Empty };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldNotThrow<BlankUrlException>();
        }

        [Test]
        public void SetSignonSettings_AuthServerUrlHasTrailingSlashes_ShouldSaveWithSlashesTrimmed()
        {
            SetupAuditLocaleDataReaderMock();

            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = true, AuthenticationServerUrl = "https://mydomain/", AuthenticationServerApiCredentialId = Guid.Empty };
            _server.SetSignonSettings(_passwordRules, logonOptions);
            _databaseConnectionMock
                .Verify(connection => connection.Execute(
                    It.Is<SqlCommand>(x => GetValueOrDefault<string>(x.Parameters, "@AuthenticationServerUrl") == "https://mydomain")));
        }

        [Test]
        public void SetSignonSettings_DisableAuthServerButStillHaveAuthServerUsers_ShouldThrowException()
        {
            SetUpDoesActiveUserExistDataReaderMock(AuthMode.AuthenticationServer, true);
            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = false };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldThrow<CannotDisableAuthTypeException>().And.AuthType.Should().Be(AuthMode.AuthenticationServer);
        }

        [Test]
        public void SetSignonSettings_DisableAuthServerButStillHaveAuthServerServiceAccounts_ShouldThrowException()
        {
            SetUpDoesActiveUserExistDataReaderMock(AuthMode.AuthenticationServerServiceAccount, true);
            var logonOptions = new LogonOptions() { AuthenticationServerAuthenticationEnabled = false };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldThrow<CannotDisableAuthTypeException>().And.AuthType.Should().Be(AuthMode.AuthenticationServerServiceAccount);
        }

        #endregion

        #region ActiveDirectoryAuthentication

        [Test]
        public void SetSignonSettings_DisableMappedActiveDirectoryAuthButStillHaveMappedAciveDirectoryUsers_ShouldThrowException()
        {
            SetUpDoesActiveUserExistDataReaderMock(AuthMode.MappedActiveDirectory, true);
            var logonOptions = new LogonOptions() { MappedActiveDirectoryAuthenticationEnabled = false };
            Action setSignonSettings = () => _server.SetSignonSettings(_passwordRules, logonOptions);
            setSignonSettings.ShouldThrow<CannotDisableAuthTypeException>().And.AuthType.Should().Be(AuthMode.MappedActiveDirectory);
        }

        #endregion

        private T GetValueOrDefault<T>(SqlParameterCollection parameters, string parameterName)
        {
            try
            {
                return (T)parameters[parameterName].Value;
            }
            catch
            {
                return default(T);
            }
        }

        private void SetServerMockDatabaseType(DatabaseType databaseType)
        {
            var activeDirectoryProvider = (databaseType == DatabaseType.SingleSignOn) ? "my.domain.com" : string.Empty;

            _databaseConnectionMock
               .Setup(m => m.ExecuteReturnScalar(It.Is<SqlCommand>(x => x.CommandText.Contains("select ActiveDirectoryProvider from BPASysConfig"))))
               .Returns(activeDirectoryProvider);
        }

        private void SetUpDoesActiveUserExistDataReaderMock(AuthMode authType, bool hasActiveUsers)
        {
            var numberOfUsersToReturn = hasActiveUsers ? 1 : 0;
            const string activeUserExistsSql = " select count(userid) from BPAUser where isdeleted = 0 and authtype = @authtype";
            _databaseConnectionMock
                     .Setup(connection => connection.
                                            ExecuteReturnScalar(It.Is<SqlCommand>(x => x.CommandText == activeUserExistsSql &&
                                                                                        (AuthMode)x.Parameters["@authtype"].Value == authType)))
                     .Returns(numberOfUsersToReturn);
        }

        private void SetUpAuditRecordWriterMock()
        {
            app.gAuditingEnabled = true;
            _databaseConnectionMock
                .Setup(m => m.ExecuteReturnRecordsAffected(It.Is<SqlCommand>(x => x.CommandText.StartsWith("INSERT INTO BPAAuditEvents"))))
                .Returns(1);
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

    }
}
#endif
