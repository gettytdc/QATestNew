#if UNITTESTS
using System;
using System.Drawing;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Core.Resources;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Resources
{
    /// <summary>
    /// A suite of tests that check the logic in the formation of control room information.
    /// </summary>
    [TestFixture]
    public class ResourcesTests
    {
        [SetUp]
        public void Setup() => LegacyUnitTestHelper.SetupDependencyResolver();

        [Test]
        [Description("Test that the info is set correctly for a login agent resource")]
        public void TestLoginAgent()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.LoginAgent
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.LoggedOut, r.DisplayStatus, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a offline resource")]
        public void TestOffline()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow.AddHours(-24),
                Attributes = ResourceAttribute.None
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("Connection Lost", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Missing, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Red.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a pooled resource")]
        public void TestPool()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.Pool
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid), true);
            Assert.AreEqual("", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Pool, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Black.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a pool with out Resources")]
        public void TestPoolWithoutResources()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.Pool
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid), false);
            Assert.AreEqual("", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Offline, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Black.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that hasn't been heard from in 180s")]
        public void TestMissing()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow.AddSeconds(-180),
                Attributes = ResourceAttribute.None,
                ActiveSessions = 1,
                PendingSessions = 1,
                WarningSessions = 1
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("Connection Lost - 180s", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Missing, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Red.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is active but has no jobs")]
        public void TestIdle()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 0,
                PendingSessions = 0,
                WarningSessions = 0
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("No sessions", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Idle, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Black.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is active and has work")]
        public void TestBusy()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 2,
                PendingSessions = 0,
                WarningSessions = 0
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("2 active", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Working, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Green.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is active and has work and pending sessions")]
        public void TestBusyWithPending()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 2,
                PendingSessions = 3,
                WarningSessions = 0
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("2 active, 3 pending", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Working, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Green.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is active and has work and pending and warning sessions")]
        public void TestBusyWithPendingAndWarning()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 2,
                PendingSessions = 3,
                WarningSessions = 1
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("2 active (1 warning), 3 pending", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Warning, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Purple.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is active and has work and pending and warning sessions")]
        public void TestBusyWithWarning()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 2,
                PendingSessions = 0,
                WarningSessions = 4
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("2 active (4 warning)", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Warning, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Purple.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that has no active sessions, but sessions pending.")]
        public void TestPending()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 0,
                PendingSessions = 1,
                WarningSessions = 0
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("1 pending", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Idle, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Black.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a login agent resource that has been missing for 180 seconds.")]
        public void TestLoginAgentMissing()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow.AddSeconds(-180),
                Attributes = ResourceAttribute.LoginAgent,
                ActiveSessions = 0,
                PendingSessions = 0,
                WarningSessions = 0
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("Connection Lost - 180s", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Missing, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Red.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is private and does not belong to the logged in user")]
        public void TestPrivateNotYours()
        {
            var myUserId = Guid.NewGuid();
            var myOtherUser = new User(AuthMode.Native, Guid.NewGuid(), "Test User - abcdefghijklmnopqrstuvwxyz");
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.Private,
                ActiveSessions = 2,
                PendingSessions = 0,
                WarningSessions = 4,
                UserID = myUserId
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => myOtherUser.Name, Guid.NewGuid());
            Assert.AreEqual("Owned By - " + myOtherUser.Name, r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Private, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Gray.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is private and does belong to the logged in user")]
        public void TestPrivateYours()
        {
            var myUserId = new Guid();
            var myOtherUser = new User(AuthMode.Native, myUserId, "Test User");
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.Private,
                ActiveSessions = 1,
                PendingSessions = 0,
                WarningSessions = 0,
                UserID = myUserId
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => myOtherUser.Name, myUserId);
            Assert.AreEqual("1 active", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Working, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Green.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a resource that is public but also logged in as a user")]
        public void TestPublicNotAnonymous()
        {
            var myUserId = new Guid();
            var myUser = new User(AuthMode.Native, myUserId, "Test User");
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 1,
                PendingSessions = 0,
                WarningSessions = 0,
                UserID = myUserId
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => myUser.Name, myUserId);
            Assert.AreEqual("1 active", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Working, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Green.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a pooled resource that is the controller of a pool")]
        public void TestPoolController()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 2,
                PendingSessions = 0,
                WarningSessions = 4,
                Controller = true,
                Pool = Guid.NewGuid()
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("* 2 active (4 warning)", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Warning, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Purple.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }

        [Test]
        [Description("Test that the info is set correctly for a pooled resource that is not the controller of a pool")]
        public void TestPoolNotController()
        {
            var r = new ResourceInfo
            {
                Name = "MyResource:8100",
                LastUpdated = DateTime.UtcNow,
                Attributes = ResourceAttribute.None,
                ActiveSessions = 1,
                PendingSessions = 0,
                WarningSessions = 0,
                Controller = false
            };
            var server = new clsServer();
            server.UpdateResourceStatusInfo(r, userId => null, default(Guid));
            Assert.AreEqual("1 active", r.Information, "Check the info string is set as expected");
            Assert.AreEqual(ResourceStatus.Working, r.DisplayStatus, "Check the Display status is set correctly");
            Assert.AreEqual(Color.Green.ToArgb(), (object)r.InfoColour, "Check the Display status is set correctly");
        }
    }
}
#endif
