using System;
using System.Collections.Generic;
using System.Data;
using System.Timers;
using Autofac;
using Autofac.Extras.Moq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.Core.Resources;
using BluePrism.Core.Utility;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Resources
{
    [TestFixture]
    public class OnDemandConnectionManagerSessionEndEventTests
    {

        private Mock<IServer> _serverMock;

        [SetUp]
        public void Setup()
        {
            _serverMock = new Mock<IServer>();
            _serverMock.Setup(x => x.GetPref(It.IsAny<string>(), It.IsAny<int>())).Returns(0);
            _serverMock.Setup(x => x.GetPref(It.IsAny<string>(), It.IsAny<bool>())).Returns(false);
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


        [Test]
        public void OnDatabasePoll_RecentlyEndedSessionReturnedFromDatabase_SessionEndedEventRaised()
        {

            var controller = new Mock<IInstructionalHostController>();

            using (var mock = AutoMock.GetLoose(cfg => cfg.RegisterInstance(controller.Object).Keyed<IInstructionalHostController>(CallbackConnectionProtocol.Wcf)))
            {

                DependencyResolver.SetContainer(mock.Container);

                var datatable = new DataTable();
                datatable.Columns.Add("sessionId", typeof(Guid));
                datatable.Columns.Add("enddatetime", typeof(DateTime));
                datatable.Rows.Add(Guid.NewGuid(), DateTime.UtcNow - TimeSpan.FromSeconds(10));

                _serverMock.Setup(x => x.GetResourceInfo(Moq.It.IsAny<ResourceAttribute>(), Moq.It.IsAny<ResourceAttribute>(), null)).Returns(new List<ResourceInfo>());
                _serverMock.Setup(x => x.GetSessionsEndedAfter(It.IsAny<DateTime>())).Returns(datatable);

                var refreshTimer = new Mock<ITimer>();

                var resourceManager = new OnDemandConnectionManager(IResourceConnectionManager.Modes.Normal, new User(AuthMode.Anonymous, Guid.NewGuid(), "Test"), new Guid(), new Guid(), refreshTimer.Object, new TimerWrapper(new Timer()));


                var sessionEndEventInvokedTimes = 0;
                resourceManager.SessionEnd += (sender, e) =>
                {
                    sessionEndEventInvokedTimes++;
                };

                refreshTimer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

                Assert.AreEqual(1, sessionEndEventInvokedTimes);
            }
              
        }



        [Test]
        public void OnDatabasePoll_NoRecentlyEndedSessionReturnedFromDatabase_SessionEndedEventNotRaised()
        {

            var controller = new Mock<IInstructionalHostController>();

            using (var mock = AutoMock.GetLoose(cfg => cfg.RegisterInstance(controller.Object).Keyed<IInstructionalHostController>(CallbackConnectionProtocol.Wcf)))
            {

                DependencyResolver.SetContainer(mock.Container);


                var datatable = new DataTable();
                datatable.Columns.Add("sessionId", typeof(Guid));
                datatable.Columns.Add("enddatetime", typeof(DateTime));


                _serverMock.Setup(x => x.GetResourceInfo(Moq.It.IsAny<ResourceAttribute>(), Moq.It.IsAny<ResourceAttribute>(), null)).Returns(new List<ResourceInfo>());
                _serverMock.Setup(x => x.GetSessionsEndedAfter(It.IsAny<DateTime>())).Returns(datatable);

                var refreshTimer = new Mock<ITimer>();

                var resourceManager = new OnDemandConnectionManager(IResourceConnectionManager.Modes.Normal, new User(AuthMode.Anonymous, Guid.NewGuid(), "Test"), new Guid(), new Guid(), refreshTimer.Object, new TimerWrapper(new Timer()));

                var sessionEndEventInvokedTimes = 0;
                resourceManager.SessionEnd += (sender, e) =>
                {
                    sessionEndEventInvokedTimes++;
                };

                refreshTimer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

                Assert.AreEqual(0, sessionEndEventInvokedTimes);
            }

        }


        [Test]
        public void OnDatabasePoll_MultipleRecentlyEndedSessionsReturnedFromDatabase_SessionEndedEventRaisedOnlyOnce()
        {

            var controller = new Mock<IInstructionalHostController>();

            using (var mock = AutoMock.GetLoose(cfg => cfg.RegisterInstance(controller.Object).Keyed<IInstructionalHostController>(CallbackConnectionProtocol.Wcf)))
            {

                DependencyResolver.SetContainer(mock.Container);


                var datatable = new DataTable();
                datatable.Columns.Add("sessionId", typeof(Guid));
                datatable.Columns.Add("enddatetime", typeof(DateTime));
                datatable.Rows.Add(Guid.NewGuid(), DateTime.UtcNow);
                datatable.Rows.Add(Guid.NewGuid(), DateTime.UtcNow);
                datatable.Rows.Add(Guid.NewGuid(), DateTime.UtcNow);

                _serverMock.Setup(x => x.GetResourceInfo(Moq.It.IsAny<ResourceAttribute>(), Moq.It.IsAny<ResourceAttribute>(), null)).Returns(new List<ResourceInfo>());
                _serverMock.Setup(x => x.GetSessionsEndedAfter(It.IsAny<DateTime>())).Returns(datatable);

                var refreshTimer = new Mock<ITimer>();

                var resourceManager = new OnDemandConnectionManager(IResourceConnectionManager.Modes.Normal, new User(AuthMode.Anonymous, Guid.NewGuid(), "Test"), new Guid(), new Guid(), refreshTimer.Object, new TimerWrapper(new Timer()));

                var sessionEndEventInvokedTimes = 0;
                resourceManager.SessionEnd += (sender, e) =>
                {
                    sessionEndEventInvokedTimes++;
                };

                refreshTimer.Raise(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);

                Assert.AreEqual(1, sessionEndEventInvokedTimes);
            }

        }

    }
}
