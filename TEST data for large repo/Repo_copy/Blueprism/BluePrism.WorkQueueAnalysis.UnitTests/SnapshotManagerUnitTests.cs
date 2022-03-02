#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.DataMonitor;
using BluePrism.AutomateProcessCore;
using BluePrism.Data.DataModels.WorkQueueAnalysis;
using BluePrism.WorkQueueAnalysis.Classes;
using BluePrism.WorkQueueAnalysis.Enums;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BluePrism.WorkQueueAnalysis.UnitTests
{
    public class SnapshotManagerUnitTests
    {
        //FunctionalityBeingTested_StateUnderTest_ExpectedResult.
        //Assemble, Act, Assert.

        private Mock<IServer> _mockServer;
        private Mock<IDataMonitor> _mockDataMonitor;
        private QueueSnapshot _snapshot1, _snapshot2, _snapshot3, _snapshot4;
        private TimeZoneInfo _gmtTimeZone;

        [SetUp]
        public void Setup()
        {
            _mockServer = new Mock<IServer>();
            _mockDataMonitor = new Mock<IDataMonitor>();

            _snapshot1 = new QueueSnapshot(1,
                1,
                LocalTime.FromSecondsSinceMidnight(0),
                IsoDayOfWeek.Monday,
                SnapshotInterval.TwelveHours,
                SnapshotTriggerEventType.InterimSnapshot);
            _snapshot2 = new QueueSnapshot(2,
                1,
                LocalTime.FromSecondsSinceMidnight(43200),
                IsoDayOfWeek.Monday,
                SnapshotInterval.TwelveHours,
                SnapshotTriggerEventType.InterimSnapshot);
            _snapshot3 = new QueueSnapshot(3,
                1,
                LocalTime.FromSecondsSinceMidnight(0),
                IsoDayOfWeek.Tuesday,
                SnapshotInterval.TwelveHours,
                SnapshotTriggerEventType.InterimSnapshot);
            _snapshot4 = new QueueSnapshot(4,
                1,
                LocalTime.FromSecondsSinceMidnight(43200),
                IsoDayOfWeek.Tuesday,
                SnapshotInterval.TwelveHours,
                SnapshotTriggerEventType.InterimSnapshot);

            _gmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }

        [Test]
        public void GetQueueSnapshotRequirements_LastSnapshotId2_Monday1300_NoRefreshRequired()
        {
            var queue = new WorkQueueSnapshotInformation(1, 2, _gmtTimeZone);
            var configuredSnapshots = new List<QueueSnapshot> {_snapshot1, _snapshot2, _snapshot3};
            var mockClock = new Mock<IClock>();
            const QueueSnapshotRequirements expectedResult = QueueSnapshotRequirements.NoRefresh;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 13, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetQueueSnapshotRequirements(configuredSnapshots, queue);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetQueueSnapshotRequirements_LastSnapshotId1_Tuesday0200_FromLastSnapshotRequired()
        {
            var queue = new WorkQueueSnapshotInformation(1, 1, _gmtTimeZone);
            var configuredSnapshots = new List<QueueSnapshot> {_snapshot1, _snapshot2, _snapshot3};
            var mockClock = new Mock<IClock>();
            const QueueSnapshotRequirements expectedResult = QueueSnapshotRequirements.FromLastSnapshot;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetQueueSnapshotRequirements(configuredSnapshots, queue);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetQueueSnapshotRequirements_NoLastSnapshotId_Tuesday0200_NextSnapshotRequired()
        {
            var queue = new WorkQueueSnapshotInformation(1, -1, _gmtTimeZone);
            var configuredSnapshots = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3 };
            var mockClock = new Mock<IClock>();
            const QueueSnapshotRequirements expectedResult = QueueSnapshotRequirements.InitialSnapshot;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetQueueSnapshotRequirements(configuredSnapshots, queue);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetQueueSnapshotRequirements_SnapshotCycleComplete_NotRestartedWeek_NoRefreshRequired()
        {
            var queue = new WorkQueueSnapshotInformation(1, 3, _gmtTimeZone);
            var configuredSnapshots = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3 };
            var mockClock = new Mock<IClock>();
            const QueueSnapshotRequirements expectedResult = QueueSnapshotRequirements.NoRefresh;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetQueueSnapshotRequirements(configuredSnapshots, queue);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetQueueSnapshotRequirements_SnapshotCycleComplete_RestartedWeek_CycleRefreshRequired()
        {
            var queue = new WorkQueueSnapshotInformation(1, 3, _gmtTimeZone);
            var configuredSnapshots = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3 };
            var mockClock = new Mock<IClock>();
            const QueueSnapshotRequirements expectedResult = QueueSnapshotRequirements.CycleRestart;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 7, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetQueueSnapshotRequirements(configuredSnapshots, queue);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetQueueSnapshotRequirements_NullInformationProvided_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() => snapshotManager.GetQueueSnapshotRequirements(null, null));
        }

        [Test]
        public void GetOffsetDateTimeForSnapshot_ValidInformationProvided_CorrectDateTimeOffset()
        {
            var mockClock = new Mock<IClock>();
            var configuredSnapshot = new QueueSnapshot(1, 1, LocalTime.Midnight, IsoDayOfWeek.Monday,
                SnapshotInterval.TwelveHours, SnapshotTriggerEventType.InterimSnapshot);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetOffsetDateTimeForSnapshot(_gmtTimeZone, configuredSnapshot);
            var expectedResult =
                OffsetDateTime.FromDateTimeOffset(new DateTimeOffset(2018, 12, 31, 0, 0, 0, new TimeSpan(0, 0, 0)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetOffsetDateTimeForSnapshot_IncorrectDate_IncorrectDateTimeOffset()
        {
            var mockClock = new Mock<IClock>();
            var configuredSnapshot = new QueueSnapshot(1, 1, LocalTime.Midnight, IsoDayOfWeek.Monday,
                SnapshotInterval.TwelveHours, SnapshotTriggerEventType.InterimSnapshot);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetOffsetDateTimeForSnapshot(_gmtTimeZone, configuredSnapshot);
            var expectedResult =
                OffsetDateTime.FromDateTimeOffset(new DateTimeOffset(2019, 5, 22, 0, 0, 0, new TimeSpan(0, 0, 0)));

            Assert.AreNotEqual(expectedResult, result);
        }

        [Test]
        public void GetOffsetDateTimeForSnapshot_IncorrectTime_IncorrectDateTimeOffset()
        {
            var mockClock = new Mock<IClock>();
            var configuredSnapshot = new QueueSnapshot(1, 1, LocalTime.Midnight, IsoDayOfWeek.Monday,
                SnapshotInterval.TwelveHours, SnapshotTriggerEventType.InterimSnapshot);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetOffsetDateTimeForSnapshot(_gmtTimeZone, configuredSnapshot);
            var expectedResult =
                OffsetDateTime.FromDateTimeOffset(new DateTimeOffset(2018, 12, 31, 12, 5, 52, new TimeSpan(0, 0, 0)));

            Assert.AreNotEqual(expectedResult, result);
        }

        [Test]
        public void GetOffsetDateTimeForSnapshot_IncorrectOffset_IncorrectDateTimeOffset()
        {
            var mockClock = new Mock<IClock>();
            var configuredSnapshot = new QueueSnapshot(1, 1, LocalTime.Midnight, IsoDayOfWeek.Monday,
                SnapshotInterval.TwelveHours, SnapshotTriggerEventType.InterimSnapshot);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetOffsetDateTimeForSnapshot(_gmtTimeZone, configuredSnapshot);
            var expectedResult =
                OffsetDateTime.FromDateTimeOffset(new DateTimeOffset(2018, 12, 31, 0, 0, 0, new TimeSpan(11, 5, 0)));

            Assert.AreNotEqual(expectedResult, result);
        }

        [Test]
        public void GetOffsetDateTimeForSnapshot_NullParametersProvided_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() =>
                snapshotManager.GetOffsetDateTimeForSnapshot(null, null));
        }

        [Test]
        public void AcquireSnapshotLock_LockIsNotCurrentlyHeld_LockSuccessfullyObtained()
        {
            string comment = null;
            const string testToken = "this-is-a-test-token";
            _mockServer.Setup(x => x.IsEnvLockHeld(It.IsAny<string>(), It.IsAny<string>(), ref comment)).Returns(false);
            _mockServer.Setup(x => x.AcquireEnvLock(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SessionIdentifier>(),
                It.IsAny<string>(), It.IsAny<int>())).Returns(testToken);

            var snapshotManager = new SnapshotManager(_mockServer.Object, null, _mockDataMonitor.Object);
            var result = snapshotManager.AcquireSnapshotLock();

            Assert.AreEqual(testToken, result);
        }

        [Test]
        public void AcquireSnapshotLock_LockIsHeldAndNotExpired_LockUnableToBeObtained()
        {
            string comment = null;
            _mockServer.Setup(x => x.IsEnvLockHeld(It.IsAny<string>(), It.IsAny<string>(), ref comment)).Returns(true);
            _mockServer.Setup(x => x.HasEnvLockExpired(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            var snapshotManager = new SnapshotManager(_mockServer.Object, null, _mockDataMonitor.Object);
            var result = snapshotManager.AcquireSnapshotLock();

            Assert.IsNull(result);
        }

        [Test]
        public void AcquireSnapshotLock_LockIsHeldButHasExpired_LockSuccessfullyObtained()
        {
            string comment = null;
            const string testToken = "this-is-a-test-token";
            _mockServer.Setup(x => x.IsEnvLockHeld(It.IsAny<string>(), It.IsAny<string>(), ref comment)).Returns(true);
            _mockServer.Setup(x => x.HasEnvLockExpired(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);
            _mockServer.Setup(x => x.AcquireEnvLock(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SessionIdentifier>(),
                It.IsAny<string>(), It.IsAny<int>())).Returns(testToken);

            var snapshotManager = new SnapshotManager(_mockServer.Object, null, _mockDataMonitor.Object);
            var result = snapshotManager.AcquireSnapshotLock();

            Assert.AreEqual(testToken, result);
        }

        [Test]
        public void AcquireSnapshotLock_ServerCannotBeAccessed_ExceptionThrown()
        {
            string comment = null;
            _mockServer.Setup(x => x.IsEnvLockHeld(It.IsAny<string>(), It.IsAny<string>(), ref comment))
                .Throws<Exception>();

            var snapshotManager = new SnapshotManager(_mockServer.Object, null, _mockDataMonitor.Object);

            Assert.Throws<Exception>(() => snapshotManager.AcquireSnapshotLock());
        }

        [Test]
        public void GetCurrentTimeInTimezone_GmtTimezoneProvided_CorrectTime()
        {
            var mockClock = new Mock<IClock>();
            var expectedResult = LocalTime.Midnight;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetCurrentTimeInTimezone(_gmtTimeZone);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetCurrentTimeInTimezone_GreenlandTimezoneProvided_CorrectTime()
        {
            var mockClock = new Mock<IClock>();
            var expectedResult = LocalTime.FromHourMinuteSecondTick(21, 0, 0, 0);
            var greenlandTimezone = TimeZoneInfo.FindSystemTimeZoneById("Greenland Standard Time");

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetCurrentTimeInTimezone(greenlandTimezone);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetCurrentTimeInTimezone_GmtTimezoneProvided_IncorrectTime()
        {
            var mockClock = new Mock<IClock>();
            var expectedResult = LocalTime.FromHourMinuteSecondTick(21, 0, 0, 0);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetCurrentTimeInTimezone(_gmtTimeZone);

            Assert.AreNotEqual(expectedResult, result);
        }

        [Test]
        public void GetCurrentTimeInTimezone_NullTimezoneProvided_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() => snapshotManager.GetCurrentTimeInTimezone(null));
        }

        [Test]
        public void GetCurrentDayOfWeekInTimezone_GmtTimezoneProvided_CorrectDay()
        {
            var mockClock = new Mock<IClock>();
            const IsoDayOfWeek expectedResult = IsoDayOfWeek.Tuesday;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetCurrentDayOfWeekInTimezone(_gmtTimeZone);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetCurrentDayOfWeekInTimezone_GreenlandTimezoneProvided_CorrectDay()
        {
            var mockClock = new Mock<IClock>();
            const IsoDayOfWeek expectedResult = IsoDayOfWeek.Monday;
            var greenlandTimezone = TimeZoneInfo.FindSystemTimeZoneById("Greenland Standard Time");

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetCurrentDayOfWeekInTimezone(greenlandTimezone);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetCurrentDayOfWeekInTimezone_GmtTimezoneProvided_IncorrectDay()
        {
            var mockClock = new Mock<IClock>();
            const IsoDayOfWeek expectedResult = IsoDayOfWeek.Monday;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetCurrentDayOfWeekInTimezone(_gmtTimeZone);

            Assert.AreNotEqual(expectedResult, result);
        }

        [Test]
        public void GetCurrentDayOfWeekInTimezone_NullDataProvided_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() => snapshotManager.GetCurrentDayOfWeekInTimezone(null));
        }

        [Test]
        public void GetSnapshotsDueByNowInDayAndTimeOrder_Wednesday0000AM_AllConfiguredSnapshotsReturned()
        {
            var workQueueSnapshotInformation = new WorkQueueSnapshotInformation(1, 1, _gmtTimeZone);
            var configuredSnapshotsForQueue = new List<QueueSnapshot> {_snapshot1, _snapshot2, _snapshot3};
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 2, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result =
                snapshotManager.GetSnapshotsDueByNowInDayAndTimeOrder(workQueueSnapshotInformation,
                    configuredSnapshotsForQueue);

            Assert.IsInstanceOf<List<QueueSnapshot>>(result);
            Assert.Contains(_snapshot1, result);
            Assert.Contains(_snapshot2, result);
            Assert.Contains(_snapshot3, result);
            Assert.IsTrue(result.Count == 3);
        }

        [Test]
        public void GetSnapshotsDueByNowInDayAndTimeOrder_Monday1200PM_FilteredConfiguredSnapshotsReturned()
        {
            var workQueueSnapshotInformation = new WorkQueueSnapshotInformation(1, 1, _gmtTimeZone);
            var configuredSnapshotsForQueue = new List<QueueSnapshot> {_snapshot1, _snapshot2, _snapshot3};
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 12, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result =
                snapshotManager.GetSnapshotsDueByNowInDayAndTimeOrder(workQueueSnapshotInformation,
                    configuredSnapshotsForQueue);

            Assert.IsInstanceOf<List<QueueSnapshot>>(result);
            Assert.Contains(_snapshot1, result);
            Assert.Contains(_snapshot2, result);
            Assert.IsTrue(result.Count == 2);
        }

        [Test]
        public void GetSnapshotsDueByNowInDayAndTimeOrder_Monday1100AM_NoConfiguredSnapshotsReturned()
        {
            var workQueueSnapshotInformation = new WorkQueueSnapshotInformation(1, 1, _gmtTimeZone);
            var configuredSnapshotsForQueue = new List<QueueSnapshot> {_snapshot2, _snapshot3, _snapshot4};
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 11, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result =
                snapshotManager.GetSnapshotsDueByNowInDayAndTimeOrder(workQueueSnapshotInformation,
                    configuredSnapshotsForQueue);

            Assert.IsInstanceOf<List<QueueSnapshot>>(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetSnapshotsDueByNowInDayAndTimeOrder_NullParametersProvided_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 11, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() =>
                snapshotManager.GetSnapshotsDueByNowInDayAndTimeOrder(null, null));
        }

        [Test]
        public void GetOutstandingSnapshotsForQueue_Tuesday1300PM_ConfiguredSnapshotsOutstanding()
        {
            var mockClock = new Mock<IClock>();
            var configuredSnapshotsForQueue = new List<QueueSnapshot> {_snapshot1, _snapshot2, _snapshot3, _snapshot4};
            var queue = new WorkQueueSnapshotInformation(1, _snapshot1.SnapshotId, _gmtTimeZone);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 1, 13, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetOutstandingSnapshotsForQueue(queue, configuredSnapshotsForQueue);

            Assert.IsInstanceOf<List<WorkQueueSnapshotInformation.SnapshotTriggerInformation>>(result);
            Assert.IsTrue(result.Count == 3);
        }

        [Test]
        public void GetOutstandingSnapshotsForQueue_Monday1100AM_NoConfiguredSnapshotsOutstanding()
        {
            var mockClock = new Mock<IClock>();
            var configuredSnapshotsForQueue = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3, _snapshot4 };
            var queue = new WorkQueueSnapshotInformation(1, _snapshot1.SnapshotId, _gmtTimeZone);

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 11, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.GetOutstandingSnapshotsForQueue(queue, configuredSnapshotsForQueue);

            Assert.IsFalse(result.Any());
        }

        [Test]
        public void GetOutstandingSnapshotsForQueue_NullParameters_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 11, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() =>
                snapshotManager.GetOutstandingSnapshotsForQueue(null, null));
        }

        [Test]
        public void FindInitialConfiguredSnapshot_Monday2AM_NextSnapshotFoundIsNextConfiguredSnapshot()
        {
            var mockClock = new Mock<IClock>();
            var queueWithSnapshotMetadata = new WorkQueueSnapshotInformation(1, -1, _gmtTimeZone); 

            var snapshot5= new QueueSnapshot(5,
                1,
                LocalTime.FromSecondsSinceMidnight(0),
                IsoDayOfWeek.Wednesday,
                SnapshotInterval.TwelveHours,
                SnapshotTriggerEventType.Snapshot);
            var configuredSnapshotsForQueue = new List<QueueSnapshot>{_snapshot1, _snapshot2, _snapshot3, _snapshot4, snapshot5};
            var expectedResult = snapshot5;

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.FindInitialConfiguredSnapshot(queueWithSnapshotMetadata, configuredSnapshotsForQueue);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void FindInitialConfiguredSnapshot_Monday2AM_DoesNotReturnNextInterimSnapshot()
        {
            var mockClock = new Mock<IClock>();
            var queueWithSnapshotMetadata = new WorkQueueSnapshotInformation(1, 3, _gmtTimeZone);

            var configuredSnapshotsForQueue = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3, _snapshot4};
            
            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2018, 12, 31, 2, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.FindInitialConfiguredSnapshot(queueWithSnapshotMetadata, configuredSnapshotsForQueue);

            Assert.IsNull(result);
        }

        [Test]
        public void FindInitialConfiguredSnapshot_ThursdayMidnight_NoSnapshotFound()
        {
            var mockClock = new Mock<IClock>();
            var queueWithSnapshotMetadata = new WorkQueueSnapshotInformation(1, -1, _gmtTimeZone);
            var snapshot5 = new QueueSnapshot(5,
                1,
                LocalTime.FromSecondsSinceMidnight(0),
                IsoDayOfWeek.Wednesday,
                SnapshotInterval.TwelveHours,
                SnapshotTriggerEventType.Snapshot);
            var configuredSnapshotsForQueue = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3, _snapshot4, snapshot5 };

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 3, 0, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.FindInitialConfiguredSnapshot(queueWithSnapshotMetadata, configuredSnapshotsForQueue);

            Assert.IsNull(result);
        }

        [Test]
        public void FindInitialConfiguredSnapshot_Wednesday12PM_TrendSnapshotAdded_NoSnapshotFound()
        {
            var mockClock = new Mock<IClock>();
            var queueWithSnapshotMetadata = new WorkQueueSnapshotInformation(1, -1, _gmtTimeZone);
            var trendSnapshot = new QueueSnapshot(5, 1, LocalTime.FromSecondsSinceMidnight(51300),
                IsoDayOfWeek.Wednesday, SnapshotInterval.SixHours, SnapshotTriggerEventType.Trend);
            var configuredSnapshotsForQueue = new List<QueueSnapshot> { _snapshot1, _snapshot2, _snapshot3, _snapshot4, trendSnapshot };

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 2, 12, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);
            var result = snapshotManager.FindInitialConfiguredSnapshot(queueWithSnapshotMetadata, configuredSnapshotsForQueue);

            Assert.IsNull(result);
        }

        [Test]
        public void FindInitialConfiguredSnapshot_NullParameters_ArgumentNullException()
        {
            var mockClock = new Mock<IClock>();

            mockClock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2019, 1, 2, 12, 0));
            var snapshotManager = new SnapshotManager(_mockServer.Object, mockClock.Object, _mockDataMonitor.Object);

            Assert.Throws<ArgumentNullException>(() => snapshotManager.FindInitialConfiguredSnapshot(null, null));
        }
    }
}

#endif