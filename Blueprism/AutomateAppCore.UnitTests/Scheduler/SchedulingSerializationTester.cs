#if UNITTESTS
using System;
using System.Collections;
using System.IO;
using System.Text;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using BluePrism.BPCoreLib.Data;
using BluePrism.Scheduling;
using BluePrism.Scheduling.Calendar;
using BluePrism.Scheduling.Triggers;
using BluePrism.UnitTesting.TestSupport;
using BPScheduler.UnitTests;
using NUnit.Framework;
using ScheduledTask = BluePrism.AutomateAppCore.ScheduledTask;
using ScheduledSession = BluePrism.AutomateAppCore.ScheduledSession;

namespace AutomateAppCore.UnitTests.Scheduler
{
    /// <summary>
    /// Tests to ensure that the serialization of scheduling-related classes works
    /// as it should, implying that the use of such classes over the BPServer /
    /// .net remoting boundary should work.
    /// </summary>
    [TestFixture]
    public class SchedulingSerializationTester
    {
        #region "Event Handling"
        // Most of these events are just here to ensure that events don't fail
        // when the subscribed delegate is not serializable (which this class isn't)

        private bool _nameChanged;

        private void HandleNameChanging(ScheduleRenameEventArgs args)
        {
            _nameChanged = true;
        }

        #endregion
        #region "Utility Methods"
        /// <summary>
        /// Checks if the given trigger group contains the specified trigger.
        /// This is done by value, which is not done by default in the scheduler
        /// architecture.
        /// </summary>
        /// <param name="group">The group to check</param>
        /// <param name="trig">The trigger which should be searched for in the
        /// given group.</param>
        /// <returns>True if the group contained a trigger with the same primary
        /// metadata as the specified trigger; false otherwise.</returns>
        private bool TriggerGroupContainsTrigger(ITriggerGroup group, ITrigger trig)
        {
            if (trig is null)
                return false;
            foreach (ITrigger t in group.Members)
            {
                if (t.PrimaryMetaData.Equals(trig.PrimaryMetaData))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Asserts that the 2 tasks are equal, causing assertion failures otherwise.
        /// </summary>
        /// <param name="t1">The first task to check</param>
        /// <param name="t2">The second task to check</param>
        private void AssertTasksAreEqual(ScheduledTask t1, ScheduledTask t2)
        {
            if (t1 is null)
                Assert.Null(t2);
            if (t2 is null)
                Assert.Null(t1);

            // No point in checking further.
            if (t1 is null)
                return;
            Assert.That(t1.Name, Is.EqualTo(t2.Name));
            Assert.That(t1.Description, Is.EqualTo(t2.Description));
            Assert.That(t1.Id, Is.EqualTo(t2.Id));
            Assert.That(t1.Sessions.Count, Is.EqualTo(t2.Sessions.Count));
            foreach (var session in t1.Sessions)
                Assert.That(t2.Sessions, Contains.Item(session));
        }

        /// <summary>
        /// Generates a schedule log entry with the given parameters.
        /// </summary>
        /// <param name="tp">The event type to create</param>
        /// <param name="entryTime">The time of the entry</param>
        /// <param name="taskId">The ID of the task.</param>
        /// <param name="sessionNo">The session number</param>
        /// <param name="termReason">The termination reason, or null</param>
        /// <param name="stackTrace">The stack trace, or null</param>
        /// <returns>A clsScheduleLogEntry object containing the given parameters.
        /// </returns>
        private ScheduleLogEntry GenLogEntry(ScheduleLogEventType tp, DateTime entryTime, int taskId, int sessionNo, string termReason, string stackTrace)
        {
            var ht = new Hashtable
            {
                ["entrytype"] = tp,
                ["entrytime"] = entryTime,
                ["taskid"] = taskId,
                ["logsessionnumber"] = sessionNo,
                ["terminationreason"] = termReason,
                ["stacktrace"] = stackTrace
            };
            return new ScheduleLogEntry(new DictionaryDataProvider(ht));
        }

        #endregion
        /// <summary>
        /// Tests the serialization of the schedule
        /// </summary>
        [Test]
        public void TestSchedule()
        {
            var store = new DummyStore
            {
                Owner = new InertScheduler()
            };

            // Set up a calendar for the test.
            var cal = new ScheduleCalendar(store.GetSchema())
            {
                Name = "SerializationTest Calendar",
                WorkingWeek = DaySet.FiveDayWeek,
                PublicHolidayGroup = "Scotland"
            };
            PublicHoliday hol = null;
            foreach (var h in store.GetSchema().GetHolidays("Scotland"))
            {
                hol = h;
                break;
            }

            if (hol is object)
            {
                cal.PublicHolidayOverrides.Add(hol);
            }

            cal.NonWorkingDays.Add(new DateTime(2010, 12, 14));
            store.SaveCalendar(cal);

            // we want it to have an ID...
            Assert.That(cal.Id, Is.Not.EqualTo(0));

            // Start testing the schedule
            var sched = new SessionRunnerSchedule(store.Owner)
            {
                Name = "Test Schedule"
            };
            var sched2 = ServiceUtil.DoBinarySerializationRoundTrip(sched);
            // After serialization, the owner must be set...
            sched2.Owner = sched.Owner;
            Assert.That(ReferenceEquals(sched, sched2), Is.False);
            Assert.That(Equals(sched, sched2), Is.True);
            sched.NameChanging += HandleNameChanging;
            sched2 = ServiceUtil.DoBinarySerializationRoundTrip(sched);
            sched2.Owner = sched.Owner;

            // Event handlers should *not* be carried over.
            _nameChanged = false;
            sched.Name = sched.Name + " [changed]";
            Assert.That(_nameChanged, Is.True);
            _nameChanged = false;
            sched2.Name = sched2.Name + " [changed]";
            Assert.That(_nameChanged, Is.False);

            // Add some arbitrary triggers...
            sched.AddTrigger(new EveryNDays(5));
            sched.AddTrigger(new EveryNDaysWithinDaySet(new DaySet()));
            sched.AddTrigger(new EveryNDaysWithinIdentifiedCalendar(cal.Id));
            sched.AddTrigger(new EveryNHours(1));
            sched.AddTrigger(new EveryNHoursWithinRange(1, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)));
            sched.AddTrigger(new EveryNMinutes(1));
            sched.AddTrigger(new EveryNSeconds(2000));
            sched.AddTrigger(new EveryNthIdentifiedCalendarDayInNthMonth(NthOfMonth.First, cal.Id));
            sched.AddTrigger(new EveryNthKnownCalendarDayInNthMonth(NthOfMonth.First, new DaySetCalendar(DaySet.FiveDayWeek)));
            sched.AddTrigger(new EveryNthOfNthMonth(1, NonExistentDatePolicy.Skip));
            sched.AddTrigger(new EveryNWeeks(2));
            sched.AddTrigger(new EveryNWeeksOnNthDayInIdentifiedCalendar(cal.Id));
            sched.AddTrigger(new EveryNWeeksOnNthDayInKnownCalendar(new DaySetCalendar(DaySet.FiveDayWeek)));
            sched.AddTrigger(new EveryNYears(10));

            // and some tasks.
            var task = new ScheduledTask
            {
                Name = "Serial Test Task 1"
            };
            // These don't have to match anything since we're not saving this schedule
            // to the database - just serializing it.
            task.AddSession(new ScheduledSession(0, Guid.NewGuid(), "1", Guid.NewGuid(), false, false, null));
            task.AddSession(new ScheduledSession(0, Guid.NewGuid(), "2", Guid.NewGuid(), false, false, null));
            sched.Add(task);
            sched.InitialTask = task;
            var task2 = new ScheduledTask
            {
                Name = "Serial Test Task 2"
            };
            task2.AddSession(new ScheduledSession(0, Guid.NewGuid(), "1", Guid.NewGuid(), false, false, null));
            task2.AddSession(new ScheduledSession(0, Guid.NewGuid(), "2", Guid.NewGuid(), false, false, null));
            sched.Add(task2);
            task.OnSuccess = task2;
            sched2 = ServiceUtil.DoBinarySerializationRoundTrip(sched);
            sched2.Owner = sched.Owner;

            // It should be equal
            Assert.That(sched, Is.EqualTo(sched2));

            // Check the triggers
            foreach (ITrigger trig in sched.Triggers.Members)
                Assert.That(TriggerGroupContainsTrigger(sched2.Triggers, trig), Is.True);

            // Check the tasks
            AssertTasksAreEqual(sched.InitialTask, sched2.InitialTask);
            AssertTasksAreEqual(sched.InitialTask.OnSuccess, sched2.InitialTask.OnSuccess);
            AssertTasksAreEqual(sched.InitialTask.OnFailure, sched2.InitialTask.OnFailure);

            // Clean up after ourselves...
            store.DeleteCalendar(cal);
        }


        /// <summary>
        /// Tests the serialization of the historical log.
        /// </summary>
        [Test]
        public void TestLogs()
        {

            // Build up the log
            var log = new HistoricalScheduleLog(1, "Dummy", TriggerActivationReason.Execute, DateTime.Now);
            var dt = DateTime.Now;
            var rand = new Random();

            // Generate a few entries with randomly spaced times.
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.ScheduleStarted, dt, 0, 0, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.TaskStarted, dt, 1, 0, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.SessionStarted, dt, 1, 1, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.SessionStarted, dt, 1, 2, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.SessionStarted, dt, 1, 3, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.SessionCompleted, dt, 1, 1, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.SessionCompleted, dt, 1, 3, null, null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.SessionTerminated, dt, 1, 2, "Error of some kind occurred", new Exception().StackTrace));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.TaskTerminated, dt, 1, 0, "Session 1/3 failed", null));
            dt = dt.AddSeconds(rand.Next(0, 3));
            log.Add(GenLogEntry(ScheduleLogEventType.ScheduleTerminated, dt, 0, 0, "Task Terminated", null));

            // Check all the entries are there...
            Assert.That(log.Count, Is.EqualTo(10));

            // OK, we have ourselves a log. Check that its serialized and deserialized twin 
            // has the same entries as the original
            var log2 = ServiceUtil.DoBinarySerializationRoundTrip(log);

            // Not the same object...
            Assert.That(object.ReferenceEquals(log, log2), Is.False);

            // All the properties match up...
            Assert.That(log.ActivationReason, Is.EqualTo(log2.ActivationReason));
            Assert.That(log.EndTime, Is.EqualTo(log2.EndTime));
            Assert.That(log.Id, Is.EqualTo(log2.Id));
            Assert.That(log.InstanceTime, Is.EqualTo(log2.InstanceTime));
            Assert.That(log.StartTime, Is.EqualTo(log2.StartTime));
            Assert.That(log.Status, Is.EqualTo(log2.Status));

            // Check that the log entries match up.
            Assert.That(log, Is.EquivalentTo(log2));
        }

        [Test]
        public void CsvOutputBuilder_WriteEntry_HeaderWithShownTimeInstance()
        {
            var testTextWriter = new TestTextWriter();
            var sutOutputBuilder = new CsvOutputBuilder(testTextWriter);
            sutOutputBuilder.WriteHeader(true);
            Assert.That(testTextWriter.GetWrittenString()
                .Equals("Type,Status,Name,Instance,Start,End,Server,Termination Reason"));
        }

        [Test]
        public void CsvOutputBuilder_WriteEntry_HeaderWithoutShownTimeInstance()
        {
            var testTextWriter = new TestTextWriter();
            var sutOutputBuilder = new CsvOutputBuilder(testTextWriter);
            sutOutputBuilder.WriteHeader(false);
            Assert.That(testTextWriter.GetWrittenString().Equals("Type,Status,Name,Start,End,Server,Termination Reason"));
        }

        [Test]
        public void ReadableOutputBuilder_WriteEntry_GenericValueRoundTrip()
        {
            var testTextWriter = new TestTextWriter();
            var sutReadableOutputBuilder = new ReadableOutputBuilder(testTextWriter);
            sutReadableOutputBuilder.SetFinishedSchedule();
            var fakeStatus = ItemStatus.Completed;
            var fakeString = "data";
            var fakeDate = DateTime.Now;
            var successChar = '-';
            sutReadableOutputBuilder.SetFinishedSchedule();
            sutReadableOutputBuilder.WriteEntry(fakeStatus, fakeString, fakeDate, fakeDate, fakeDate, fakeString, fakeString);
            Assert.That(testTextWriter.GetWrittenString().Equals($"{successChar} {fakeString,-40} | {fakeDate:HH:mm:ss} | {fakeDate:HH:mm:ss} | {fakeDate:HH:mm:ss} | {fakeString} | {fakeString}"));
        }

        [Test]
        public void ReadableOutputBuilder_WriteEntry_NullValuesDoNotThrowAnException()
        {
            var testTextWriter = new TestTextWriter();
            var sutReadableOutputBuilder = new ReadableOutputBuilder(testTextWriter);
            sutReadableOutputBuilder.SetFinishedSchedule();
            var fakeStatus = ItemStatus.All;
            string fakeString = null;
            var fakeDate = default(DateTime);
            sutReadableOutputBuilder.SetFinishedSchedule();
            Assert.DoesNotThrow(() => sutReadableOutputBuilder.WriteEntry(fakeStatus, fakeString, fakeDate, fakeDate, fakeDate, fakeString, fakeString));
        }
    }

    class TestTextWriter : TextWriter
    {
        private string WrittenString { get; set; }

        public override Encoding Encoding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string GetWrittenString()
        {
            return WrittenString;
        }

        public override void WriteLine(string entry)
        {
            WrittenString = entry;
        }
    }
}
#endif
