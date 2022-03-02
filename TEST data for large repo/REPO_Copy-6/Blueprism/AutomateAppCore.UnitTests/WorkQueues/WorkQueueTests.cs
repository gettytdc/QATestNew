#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib.Data;
using BluePrism.Core.Utility;
using BluePrism.UnitTesting;
using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using BluePrism.Server.Domain.Models;

namespace AutomateAppCore.UnitTests.WorkQueues
{
    public class WorkQueueTests
    {
        [SetUp]
        public void SetUp()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            SetupMockServer();
        }

        [TearDown]
        public void TearDown()
        {
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, null);
        }

        [Test]
        public void Constructs_DefaultQueue_InitialisedCorrectly()
        {
            var sut = new clsWorkQueue();
            sut.Id.Should().Be(Guid.Empty);
            sut.Name.Should().Be(string.Empty);
            sut.KeyField.Should().Be(string.Empty);
            sut.MaxAttempts.Should().Be(3);
            sut.IsRunning.Should().BeTrue();
            sut.EncryptionKeyID.Should().Be(default(int));
        }

        [Test]
        public void Constructs_FromProvider_InitialisedCorrectly()
        {
            var provider = new Mock<IDataProvider>();
            provider.Setup(p => p.GetGuid("id")).Returns(Guid.Parse("0ef276d0-933f-4088-b6fe-3e9f89d5c54e"));
            provider.Setup(p => p.GetValue("name", "")).Returns("testName");
            provider.Setup(p => p.GetValue("keyfield", "")).Returns("testKeyField");
            provider.Setup(p => p.GetValue("maxattempts", 3)).Returns(5);
            provider.Setup(p => p.GetValue("running", true)).Returns(false);
            provider.Setup(p => p.GetValue("encryptid", 0)).Returns(1234);
            provider.Setup(p => p.GetValue("ident", 0)).Returns(999);
            provider.Setup(p => p.GetGuid("processid")).Returns(Guid.Parse("2ff9ab04-6619-4315-9b36-676937e4d750"));
            provider.Setup(p => p.GetString("processname")).Returns("testProcessName");
            provider.Setup(p => p.GetGuid("resourcegroupid")).Returns(Guid.Parse("f6dd9d32-f36a-4474-966d-22e7076b8576"));
            provider.Setup(p => p.GetString("resourcegroupname")).Returns("testResourceName");
            provider.Setup(p => p.GetValue("total", 0)).Returns(5);
            provider.Setup(p => p.GetValue("completed", 0)).Returns(2);
            provider.Setup(p => p.GetValue("pending", 0)).Returns(1);
            provider.Setup(p => p.GetValue("deferred", 0)).Returns(1);
            provider.Setup(p => p.GetValue("exceptioned", 0)).Returns(1);
            provider.Setup(p => p.GetValue("totalworktime", 0L)).Returns(60);
            provider.Setup(p => p.GetValue("averageworkedtime", 0.0)).Returns(30);
            provider.Setup(p => p.GetValue("sessionexceptionretry",true)).Returns(false);
            var sut = new clsWorkQueue(provider.Object);
            sut.Id.Should().Be(Guid.Parse("0ef276d0-933f-4088-b6fe-3e9f89d5c54e"));
            sut.Name.Should().Be("testName");
            sut.KeyField.Should().Be("testKeyField");
            sut.MaxAttempts.Should().Be(5);
            sut.IsRunning.Should().BeFalse();
            sut.EncryptionKeyID.Should().Be(1234);
            sut.Ident.Should().Be(999);
            sut.ProcessId.Should().Be(Guid.Parse("2ff9ab04-6619-4315-9b36-676937e4d750"));
            sut.ProcessName.Should().Be("testProcessName");
            sut.ResourceGroupId.Should().Be(Guid.Parse("f6dd9d32-f36a-4474-966d-22e7076b8576"));
            sut.ResourceGroupName.Should().Be("testResourceName");
            sut.TotalAttempts.Should().Be(5);
            sut.Completed.Should().Be(2);
            sut.Pending.Should().Be(1);
            sut.Deferred.Should().Be(1);
            sut.Exceptioned.Should().Be(1);
            sut.TotalWorkTime.Should().Be(TimeSpan.FromSeconds(60));
            sut.AverageWorkedTime.Should().Be(TimeSpan.FromSeconds(30));
            sut.IsActive.Should().BeTrue();
            sut.IsEncrypted.Should().BeTrue();
            sut.EncryptionKeyChanged.Should().BeFalse();
            sut.SessionExceptionRetry.Should().BeFalse();
        }

        [Test]
        public void UpdateStats_UpdatesCorrectly()
        {
            var sut = new clsWorkQueue()
            {
                TotalAttempts = 5,
                Completed = 3,
                Pending = 1,
                Deferred = 1,
                Exceptioned = 0,
                TotalWorkTime = TimeSpan.FromSeconds(100),
                AverageWorkedTime = TimeSpan.FromSeconds(20)
            };

            var sutUpdated = new clsWorkQueue()
            {
                TotalAttempts = 10,
                Completed = 5,
                Pending = 2,
                Deferred = 1,
                Exceptioned = 2,
                TotalWorkTime = TimeSpan.FromSeconds(120),
                AverageWorkedTime = TimeSpan.FromSeconds(24)
            };
            sut.UpdateStats(sutUpdated);
            sut.TotalAttempts.Should().Be(10);
            sut.Completed.Should().Be(5);
            sut.Pending.Should().Be(2);
            sut.Deferred.Should().Be(1);
            sut.Exceptioned.Should().Be(2);
            sut.TotalWorkTime.Should().Be(TimeSpan.FromSeconds(120));
            sut.AverageWorkedTime.Should().Be(TimeSpan.FromSeconds(24));
        }

        [Test]
        public void UpdateActiveData_UpdatesCorrectly()
        {
            var sut = new clsWorkQueue();
            sut.UpdateActiveData();
            sut.RunningSessions.Should().HaveCount(1);
            sut.TargetSessionCount.Should().Be(2);
        }

        [Test]
        public void CheckSessions_FilterProperties()
        {
            var sut = new clsWorkQueue();
            sut.Sessions = new[] { new clsProcessSession() { Status = SessionStatus.All }, new clsProcessSession() { Status = SessionStatus.Archived }, new clsProcessSession() { Status = SessionStatus.Completed }, new clsProcessSession() { Status = SessionStatus.Debugging }, new clsProcessSession() { Status = SessionStatus.Failed }, new clsProcessSession() { Status = SessionStatus.Pending }, new clsProcessSession() { Status = SessionStatus.Running }, new clsProcessSession() { Status = SessionStatus.Stalled }, new clsProcessSession() { Status = SessionStatus.Stopped }, new clsProcessSession() { Status = SessionStatus.StopRequested }, new clsProcessSession() { Status = SessionStatus.Terminated } }.ToList();
            sut.RunningSessions.Should().HaveCount(2);
            sut.ContinuingSessions.Should().HaveCount(1);
            sut.ActiveSessions.Should().HaveCount(3);
            sut.StoppingSessions.Should().HaveCount(1);
        }

        [Test]
        public void CheckRunningLabel_IsCorrect()
        {
            var sut = new clsWorkQueue();
            sut.RunningLabel.Should().Be("Running");
            sut.IsRunning = false;
            sut.RunningLabel.Should().Be("Paused");
            sut.IsRunning = true;
            sut.RunningLabel.Should().Be("Running");
        }

        [Test]
        public void CheckTimeRemaining_Correct()
        {
            var clockMock = new Mock<ISystemClock>();
            var testDate = new DateTime(2020, 1, 1);
            clockMock.Setup(x => x.Now).Returns(testDate);
            clockMock.Setup(x => x.UtcNow).Returns(testDate);
            var mockClsWorkQueue = new clsWorkQueue
            {
                TotalAttempts = 10,
                Completed = 5,
                Pending = 2,
                Deferred = 1,
                Exceptioned = 2,
                TotalWorkTime = TimeSpan.FromSeconds(120),
                AverageWorkedTime = TimeSpan.FromSeconds(24)
            };
            var sut = new clsWorkQueue();
            sut.Clock = clockMock.Object;
            sut.UpdateStats(mockClsWorkQueue);
            sut.Sessions = new[] { new clsProcessSession() { Status = SessionStatus.Running }, new clsProcessSession() { Status = SessionStatus.StopRequested } }.ToList();
            sut.TimeRemaining.Should().Be(TimeSpan.FromSeconds(24));
            sut.TimeRemainingDisplay.Should().Be("00:00:24");
            sut.ElapsedTimeRemaining.Should().Be(TimeSpan.FromSeconds(12));
            sut.ElapsedTimeRemainingDisplay.Should().Be("00:00:12");
            sut.EndTime.Should().Be(testDate + TimeSpan.FromSeconds(12));
            sut.EndTimeUTC.Should().Be(testDate + TimeSpan.FromSeconds(12));
        }

        [TestCase(0, 2)]
        [TestCase(3, 3)]
        public void CheckReservedResourceCount_IsCorrect(int targetSessionsCount, int expectedResult)
        {
            var sut = new clsWorkQueue();
            sut.Sessions = new[] { new clsProcessSession() { Status = SessionStatus.Running }, new clsProcessSession() { Status = SessionStatus.StopRequested } }.ToList();
            sut.TargetSessionCount = targetSessionsCount;
            sut.ReservedResourceCount.Should().Be(expectedResult);
        }

        [Test]
        public void ResetTargetSessionsCount_UpdatesCorrectly()
        {
            var sut = new clsWorkQueue();
            sut.Sessions = new[] { new clsProcessSession() { Status = SessionStatus.Pending }, new clsProcessSession() { Status = SessionStatus.Running }, new clsProcessSession() { Status = SessionStatus.StopRequested } }.ToList();
            sut.TargetSessionCount.Should().Be(0);
            var count = sut.ResetTargetSessionCount();
            count.Should().Be(2);
            sut.TargetSessionCount.Should().Be(2);
        }

        private void SetupMockServer()
        {
            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(CreateMockServer());
            var serverFactoryMock = new Mock<BluePrism.AutomateAppCore.ClientServerConnection.IServerFactory>();
            serverFactoryMock.SetupGet<ServerManager>(m => m.ServerManager).Returns(serverManagerMock.Object);
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, serverFactoryMock.Object);
        }

        private IServer CreateMockServer()
        {
            var server = new Mock<IServer>();
            var mockClsWorkQueue = new Collection<clsWorkQueue>(){
               new clsWorkQueue{ TotalAttempts = 10,
                Completed = 5,
                Pending = 2,
                Deferred = 1,
                Exceptioned = 2,
                TotalWorkTime = TimeSpan.FromSeconds(120),
                AverageWorkedTime = TimeSpan.FromSeconds(24) }
            };
            server.Setup<ICollection<clsWorkQueue>>(s => s.GetQueueStatsList(It.IsAny<ICollection<clsWorkQueue>>())).Returns(mockClsWorkQueue);
            server.Setup(s => s.UpdateActiveQueueData(It.IsAny<clsWorkQueue>())).Returns((clsWorkQueue q) =>
            {
                q.Sessions = new[] { new clsProcessSession() { Status = SessionStatus.Pending }, new clsProcessSession() { Status = SessionStatus.Running }, new clsProcessSession() { Status = SessionStatus.Stopped } }.ToList();
                q.TargetSessionCount = 5;
                return q;
            });
            return server.Object;
        }
    }
}
#endif
