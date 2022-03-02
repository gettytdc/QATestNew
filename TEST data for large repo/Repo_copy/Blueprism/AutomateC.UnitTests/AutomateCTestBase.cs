#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.clsServerPartialClasses;
using BluePrism.Common.Security;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport;
using Moq;
using NUnit.Framework;

namespace AutomateC.UnitTests
{

    /// <summary>
    /// Base class for testing the various (unit-testable) aspects of AutomateC.
    /// </summary>
    public abstract class AutomateCTestBase
    {

        /// <summary>
    /// The mocked IServer object used by AutomateC in this test
    /// </summary>
    /// <returns></returns>
        protected Mock<IServer> ServerMock { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            SetupServerMock();
        }

        private void SetupServerMock()
        {
            ServerMock = new Mock<IServer>();
            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(ServerMock.Object);
            var serverFactoryMock = new Mock<BluePrism.AutomateAppCore.ClientServerConnection.IServerFactory>();
            serverFactoryMock.SetupGet(m => m.ServerManager).Returns(serverManagerMock.Object);
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, serverFactoryMock.Object);
        }

        /// <summary>
    /// Sets up a user and assigns it to a role with the specified permissions, then
    /// sets up the relevant server security methods. Finally, User.Login is used
    /// to set the currently logged in user.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="permissions"></param>
        protected void SetCurrentUser(string userName, IEnumerable<string> permissions)
        {
            var permissionsDictionary = permissions.Select((p, i) => new TestPermission(i + 1, p)).Cast<Permission>().ToDictionary(p => p.Name);
            ReflectionHelper.SetPrivateField<Permission>("mByName", null, permissionsDictionary);
            var userId = Guid.NewGuid();
            var user = new User(AuthMode.System, userId, "userName");
            var role = new Role("Test Role");
            role.AddAll(permissionsDictionary.Values);
            user.Roles.Add(role);
            ServerMock.Setup(s => s.Login(userName, It.IsAny<SafeString>(), "thisMachine", "en-GB")).Returns(new LoginResult(LoginResultCode.Success, user));
            ServerMock.Setup(s => s.GetUser(userId)).Returns(user);

            var locale = Options.Instance;
            locale.Init(ConfigLocator.Instance());
            ServerMock.Setup(s => s.GetEnforceEditSummariesSetting()).Returns(true);
            ServerMock.Setup(s => s.GetCompressProcessXMLSetting()).Returns(true);

            User.Login("thisMachine", userName, new SafeString("password123"), "en-GB", ServerMock.Object);
        }

        private class TestPermission : Permission
        {
            public TestPermission(int id, string nm) : base(id, nm, Feature.None)
            {
            }
        }
    }
}

#endif
