#if UNITTESTS
using BluePrism.AutomateAppCore;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Scheduler
{
    /// <summary>
    /// Test class for the 'DataChange' aspect of the scheduler.
    /// This tests that the HasChanged() methods work as expected if any 
    /// changes occur to the scheduler or its host tasks.
    /// </summary>
    [TestFixture]
    public class DataChangeTests
    {
        ///<summary>
        /// Test changes to the schedule itself, and test that changes to underlying
        /// tasks are reflected within the schedule.
        ///</summary>
        [Test]
        public void TestScheduleData()
        {
            var schedule = new SessionRunnerSchedule(null);
            Assert.That(schedule.HasChanged(), Is.False);

            schedule.Name = "Test Schedule";
            Assert.That(schedule.HasChanged(), Is.True);

            schedule.ResetChanged();
            Assert.That(schedule.HasChanged(), Is.False);

            schedule.Name = "Test Schedule";
            Assert.That(schedule.HasChanged(), Is.False);

            schedule.Description = "This is a test schedule";
            Assert.That(schedule.HasChanged(), Is.True);

            schedule.ResetChanged();
            schedule.Version = 1;
            Assert.That(schedule.HasChanged(), Is.False);
            
            var t = schedule.NewTask();
            t.Name = "Dim Task";
            t.Description = "Basic task description";
            t.ResetChanged();
            Assert.That(t.HasChanged(), Is.False);
            schedule.Add(t);
            Assert.That(schedule.HasChanged(), Is.True);

            schedule.ResetChanged();
            t.Name = "Changed Task Name";
            Assert.That(t.HasChanged(), Is.True);
            Assert.That(schedule.HasChanged(), Is.True);

            schedule.ResetChanged();
            schedule.InitialTask = t;
            Assert.That(schedule.HasChanged(), Is.True);

            schedule.ResetChanged();
            schedule.InitialTask = null;
            Assert.That(schedule.HasChanged(), Is.True);

            schedule.ResetChanged();
            schedule.InitialTask = null;
            Assert.That(schedule.HasChanged(), Is.False);
        }

        /// <summary>
        ///  Test that changes to the task are correctly reported.
        ///  </summary>
        [Test]
        public void TestTask()
        {
            var task = new ScheduledTask();
            Assert.That(task.HasChanged(), Is.False);

            task.Name = "Dummy Test";
            task.Description = "Dummy Description";
            Assert.That(task.HasChanged(), Is.True);

            task.ResetChanged();
            Assert.That(task.HasChanged(), Is.False);

            task.Description = "Dummy Description";
            Assert.That(task.HasChanged(), Is.False);
        }
    }
}
#endif