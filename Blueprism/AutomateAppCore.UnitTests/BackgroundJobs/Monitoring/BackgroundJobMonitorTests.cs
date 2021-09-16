using System;
using System.Collections.Generic;
using System.Threading;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.BackgroundJobs;
using BluePrism.AutomateAppCore.BackgroundJobs.Monitoring;
using BluePrism.Core.Utility;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.BackgroundJobs.Monitoring
{
    public class BackgroundJobMonitorTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30d);

        // Properties used within test methods - they are set up prior to each test
        private BackgroundJob Job { get; set; } = new BackgroundJob(Guid.NewGuid());
        private TestUpdateTrigger Trigger { get; set; }
        private Mock<IServer> ServerMock { get; set; }
        private BackgroundJobMonitor Monitor { get; set; }

        [SetUp]
        public void Setup()
        {
            Trigger = new TestUpdateTrigger();
            ServerMock = new Mock<IServer>();
            var clockMock = new Mock<ISystemClock>();
            Monitor = new BackgroundJobMonitor(Job, ServerMock.Object, Trigger, Timeout, clockMock.Object);
        }

        /// <summary>
        /// Sets up the mock data source to return the specified data and triggers an update
        /// </summary>
        /// <param name="jobData">Data to return</param>
        private void TriggerUpdateWithData(BackgroundJobData jobData) => TriggerUpdateWithData(() => jobData);

        /// <summary>
        /// Sets up the mock data source to return the specified data and triggers an update
        /// </summary>
        /// <param name="getJobData">Function to create job data</param>
        private void TriggerUpdateWithData(Func<BackgroundJobData> getJobData)
        {
            ServerMock.Setup(ds => ds.GetBackgroundJob(Job.Id, true)).Returns(getJobData);
            Trigger.OnUpdate();
        }

        /// <summary>
        /// Convenience assertion method
        /// </summary>
        /// <param name="jobResult">Result to check</param>
        /// <param name="status">Expected status</param>
        /// <param name="data">Expected data</param>
        private void AssertExpectedResult(BackgroundJobResult jobResult, JobMonitoringStatus status, BackgroundJobData data)
        {
            Assert.That(jobResult, Is.Not.Null);
            Assert.That(jobResult.Status, Is.EqualTo(status));
            Assert.That(jobResult.Data, Is.EqualTo(data));
        }

        [Test]
        public void Start_WhenRunning_ShouldRaiseRunningWithLatestData()
        {
            BackgroundJobData jobData = null;
            Monitor.Running += (sender, args) => jobData = args.Data;
            Monitor.Start();
            var runningData = new BackgroundJobData(BackgroundJobStatus.Running, 100, "", DateTime.UtcNow);
            TriggerUpdateWithData(runningData);
            Assert.That(jobData, Is.Not.Null);
            Assert.That(jobData, Is.EqualTo(runningData));
        }

        [Test]
        public void Start_WhenJobRunning_ShouldOnlyRaiseRunningWhenDataUpdated()
        {
            bool runningRaised;
            BackgroundJobData receivedData = null;
            Monitor.Running += (sender, args) =>
            {
                runningRaised = true;
                receivedData = args.Data;
            };
            Monitor.Start();
            var runningData1 = new BackgroundJobData(BackgroundJobStatus.Running, 50, "", DateTime.UtcNow);
            var runningData2 = new BackgroundJobData(BackgroundJobStatus.Running, 60, "", DateTime.UtcNow.AddSeconds(2d));
            TriggerUpdateWithData(runningData1);
            Assert.That(receivedData, Is.EqualTo(runningData1));
            runningRaised = false;
            receivedData = null;
            TriggerUpdateWithData(runningData1);
            Assert.That(runningRaised, Is.False);
            Assert.That(receivedData, Is.Null);
            TriggerUpdateWithData(runningData2);
            Assert.That(receivedData, Is.EqualTo(runningData2));
        }

        [Test]
        public void Start_WhenSuccess_ShouldRaiseDone()
        {
            BackgroundJobResult jobResult = null;
            Monitor.Done += (sender, args) => jobResult = args.Result;
            Monitor.Start();
            var data = new BackgroundJobData(BackgroundJobStatus.Success, 100, "", DateTime.UtcNow);
            TriggerUpdateWithData(data);
            AssertExpectedResult(jobResult, JobMonitoringStatus.Success, data);
        }

        [Test]
        public void Start_WhenFailed_ShouldRaiseDone()
        {
            BackgroundJobResult jobResult = null;
            Monitor.Done += (sender, args) => jobResult = args.Result;
            Monitor.Start();
            var data = new BackgroundJobData(BackgroundJobStatus.Failure, 20, "", DateTime.UtcNow);
            TriggerUpdateWithData(data);
            AssertExpectedResult(jobResult, JobMonitoringStatus.Failure, data);
        }

        [Test]
        public void Start_WhenUnknown_ShouldRaiseDone()
        {
            BackgroundJobResult jobResult = null;
            Monitor.Done += (sender, args) => jobResult = args.Result;
            Monitor.Start();
            TriggerUpdateWithData(BackgroundJobData.Unknown);
            AssertExpectedResult(jobResult, JobMonitoringStatus.Missing, BackgroundJobData.Unknown);
        }

        [Test]
        public void Start_WhenErrorDuringUpdate_ShouldRaiseDoneWithError()
        {
            BackgroundJobResult jobResult = null;
            Monitor.Done += (sender, args) => jobResult = args.Result;
            Monitor.Start();
            TriggerUpdateWithData(() => { throw new InvalidOperationException("Bad"); });
            AssertExpectedResult(jobResult, JobMonitoringStatus.MonitoringError, BackgroundJobData.Unknown);
        }

        [Test]
        public void Start_WhenDone_ShouldNotRaiseEventsWhenTriggeredAgain()
        {
            Monitor.Start();

            // This will trigger Done event
            var doneData = new BackgroundJobData(BackgroundJobStatus.Success, 100, "", DateTime.UtcNow);
            TriggerUpdateWithData(doneData);

            // No further events should be raised
            Monitor.Running += (sender, args) => Assert.Fail("Running raised");
            Monitor.Done += (sender, args) => Assert.Fail("Done raised");
            Trigger.OnUpdate();
            Trigger.OnUpdate();
        }

        [Test]
        public void Start_WhenUpdateTriggeredConcurrently_ShouldLimitUpdatesAndRaiseSingleDoneEvent()
        {
            Monitor.Start();

            // No further events should be raised
            var updateCount = 0;
            var runningRaisedCount = 0;
            var doneRaisedCount = 0;
            Monitor.Running += (sender, args) => Interlocked.Increment(ref runningRaisedCount);
            Monitor.Done += (sender, args) => Interlocked.Increment(ref doneRaisedCount);
            var doneData = new BackgroundJobData(BackgroundJobStatus.Success, 100, "", DateTime.UtcNow);
            ServerMock.Setup(ds => ds.GetBackgroundJob(Job.Id, true)).Returns(() =>
                            {
                                Interlocked.Increment(ref updateCount);
                                return doneData;
                            });
            var count = 10;
            var countdown = new CountdownEvent(count);
            var threads = new List<Thread>(count);
            for (int counter = 1, loopTo = count; counter <= loopTo; counter++)
            {
                var thread = new Thread(() =>
                {
                    Trigger.OnUpdate();
                    countdown.Signal();
                });
                threads.Add(thread);
            }

            threads.ForEach(x => x.Start());
            countdown.Wait();
            Assert.That(updateCount, Is.EqualTo(1), "Should not make multiple updates");
            Assert.That(runningRaisedCount, Is.EqualTo(0), "Should not raise running event");
            Assert.That(doneRaisedCount, Is.EqualTo(1), "Should raise single done event");
        }

        [Test]
        public void Start_WhenRunningJobNotUpdatedWithinTimeout_ShouldRaiseTimeout()
        {
            var startDate = DateTime.UtcNow;
            var clockMock = new Mock<ISystemClock>();
            clockMock.SetupSequence(x => x.UtcNow).Returns(startDate).Returns(startDate.AddMinutes(10d)).Returns(startDate.AddMinutes(31d));
            Monitor = new BackgroundJobMonitor(Job, ServerMock.Object, Trigger, Timeout, clockMock.Object);
            var doneRaised = false;
            BackgroundJobResult jobResult = null;
            Monitor.Done += (sender, args) =>
            {
                doneRaised = true;
                jobResult = args.Result;
            };
            Monitor.Start();
            var runningData = new BackgroundJobData(BackgroundJobStatus.Running, 50, "", startDate);
            TriggerUpdateWithData(runningData);
            Assert.That(doneRaised, Is.False);

            // 10 minutes since update
            TriggerUpdateWithData(runningData);
            Assert.That(doneRaised, Is.False);

            // 31 minutes since update (30 min timeout)
            TriggerUpdateWithData(runningData);
            Assert.That(doneRaised, Is.True);
            AssertExpectedResult(jobResult, JobMonitoringStatus.Timeout, runningData);
        }
    }
}
