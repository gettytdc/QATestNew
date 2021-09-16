#if UNITTESTS
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.Core.Network;
using BluePrism.Core.Resources;
using BluePrism.Core.Utility;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting;
using BluePrism.UnitTesting.TestSupport;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Resources
{
    [TestFixture]
    public class ResourceConnectionTests
    {
        private const string V = "Disabling until build is fixed";
        private Mock<IServer> _serverMock;

        public ResourceConnectionTests()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            Setup();
        }

        [Test]
        [Ignore("Ignore test until GRPC build is complete")]
        
        public void GetActiveResourcesTest()
        {
            var resourceManager = new OnDemandConnectionManager(IResourceConnectionManager.Modes.Normal, new User(AuthMode.Anonymous, Guid.NewGuid(), "Test"), new Guid(), new Guid(), new Mock<ITimer>().Object, new Mock<ITimer>().Object);
            var localResources = new ConcurrentDictionary<Guid, OnDemandConnection>();
            var firstGuid = Guid.NewGuid();

            localResources.GetOrAdd(firstGuid, new OnDemandConnection(Guid.NewGuid(), "Unit Test", ResourceMachine.ResourceDBStatus.Ready, ResourceAttribute.Private, Guid.NewGuid(), resourceManager, 0, 0,5,100));

            ReflectionHelper.SetPrivateField(typeof(ResourceConnectionBase), "mConnectionState", localResources[firstGuid], ResourceConnectionState.Connected);
            ReflectionHelper.SetPrivateField(typeof(OnDemandConnectionManager), "mResourceConnections", resourceManager, localResources);

            Assert.That(resourceManager.GetActiveResources(false), Has.Count.EqualTo(1));

        }

        private List<ResourceInfo> ResourceList() 
        {
            var resources = new List<ResourceInfo>
            {
                new ResourceInfo()
                {
                    ID = new Guid(),
                    Name = "First Resource",
                    DisplayStatus = ResourceStatus.Working,
                    ActiveSessions = 3,
                    Attributes = ResourceAttribute.Private,
                    UserID = Guid.NewGuid()
                }
            };

            return resources;
        }

        public void Setup()
        {
            _serverMock = new Mock<IServer>();
            _serverMock.Setup(x => x.GetPref(Moq.It.IsAny<string>(), Moq.It.IsAny<int>())).Returns(0);
            _serverMock.Setup(x => x.GetPref(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(false);
            _serverMock.Setup(x => x.GetResourceInfo(Moq.It.IsAny<ResourceAttribute>(), Moq.It.IsAny<ResourceAttribute>(),null)).Returns(ResourceList());
            SetupMockServer(_serverMock.Object);
        }


        private void SetupMockServer(IServer serverMock)
        {
            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(serverMock);
            var serverFactoryMock = new Mock<BluePrism.AutomateAppCore.ClientServerConnection.IServerFactory>();
            serverFactoryMock.SetupGet(m => m.ServerManager).Returns(serverManagerMock.Object);
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, serverFactoryMock.Object);
        }

    }
}
#endif
