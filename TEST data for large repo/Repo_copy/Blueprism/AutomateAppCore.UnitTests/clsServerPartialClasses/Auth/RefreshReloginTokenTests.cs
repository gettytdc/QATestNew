#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
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
    public class RefreshReloginTokenTests
    {
        private clsServer _server;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly string _userName = "Testy McTesterson";        
        private Mock<IDatabaseConnection> _databaseConnectionMock;        
        private readonly SafeString _reloginToken = new SafeString(Guid.NewGuid().ToString());

        [SetUp]
        public void Setup()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();

            _server = new clsServer();

            _databaseConnectionMock = new Mock<IDatabaseConnection>();
            _databaseConnectionMock.Setup(x => x.Execute(It.IsAny<SqlCommand>()));

            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", _server, (Func<IDatabaseConnection>)(() => _databaseConnectionMock.Object));
            ReflectionHelper.SetPrivateField("mLoggedInUser", _server, new User(AuthMode.External, _userId, _userName));
        }

        [Test]
        public void RefreshLoginToken_UserIdDoesNotMatchLoggedInUser_ShouldThrow()
        {
            var otherUserId = Guid.NewGuid();
            ReflectionHelper.SetPrivateField("mValidateReloginToken", _server,
                 (Func<IDatabaseConnection, ReloginTokenRequest, Guid>)((con, tokenRequest) => otherUserId));

            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            Action newReloginTokenAction = () => _server.RefreshReloginToken(reloginTokenRequest);
            newReloginTokenAction.ShouldThrow<NoValidTokenFoundException>();
        }

        [Test]
        public void RefreshLoginToken_ValidToken_ShouldReturnNewReloginToken()
        {
            ReflectionHelper.SetPrivateField("mValidateReloginToken", _server,
                (Func<IDatabaseConnection, ReloginTokenRequest, Guid>)((con, tokenRequest) => _userId));

            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, _reloginToken);
            var newReloginToken = _server.RefreshReloginToken(reloginTokenRequest).AsString();
            newReloginToken.Should().NotBeNull().And.Should().NotBe(_reloginToken);
        }

        [Test]
        public void RefreshLoginToken_InvalidToken_ShouldThrow()
        {
            ReflectionHelper.SetPrivateField("mValidateReloginToken", _server,
               (Func<IDatabaseConnection, ReloginTokenRequest, Guid>)((con, tokenRequest) => Guid.Empty));

            var reloginTokenRequest = new ReloginTokenRequest("thismachine", 1234, new SafeString("invalid token"));
            Action loginAction = () => _server.RefreshReloginToken(reloginTokenRequest);
            loginAction.ShouldThrow<NoValidTokenFoundException>();
        }

    }
}
#endif
