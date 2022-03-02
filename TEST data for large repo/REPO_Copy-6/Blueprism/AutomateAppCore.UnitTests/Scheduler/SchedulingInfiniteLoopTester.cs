using BluePrism.AutomateAppCore;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Scheduler
{
    [TestFixture]
    class SchedulingInfiniteLoopTester
    {
        [Test]
        public void TestLoopingSchedule_OnSuccess_IsDetectedSuccessfully()
        {
            var schedule = new SessionRunnerSchedule(null);

            var taskOne = new ScheduledTask();
            var taskTwo = new ScheduledTask();

            taskOne.OnSuccess = taskTwo;
            taskTwo.OnSuccess = taskOne;

            schedule.Add(taskOne, true);
            schedule.Add(taskTwo, false);

            var scheduleIsLooping = schedule.IsLoopingSchedule();
            Assert.That(scheduleIsLooping, Is.EqualTo(true));

        }

        [Test]
        public void TestNonLoopingSchedule_OnSuccess_IsDetectedSuccessfully()
        {
            var schedule = new SessionRunnerSchedule(null);

            var taskOne = new ScheduledTask();
            var taskTwo = new ScheduledTask();
            var taskThree = new ScheduledTask();

            taskOne.OnSuccess = taskTwo;
            taskTwo.OnSuccess = taskThree;

            schedule.Add(taskOne, true);
            schedule.Add(taskTwo, false);
            schedule.Add(taskThree, false);

            var scheduleIsLooping = schedule.IsLoopingSchedule();
            Assert.That(scheduleIsLooping, Is.EqualTo(false));
        }

        [Test]
        public void TestNonLoopingSchedule_OnFailure_IsDetectedSuccessfully()
        {
            var schedule = new SessionRunnerSchedule(null);

            var taskOne = new ScheduledTask();
            var taskTwo = new ScheduledTask();
            var taskThree = new ScheduledTask();

            taskOne.OnFailure = taskTwo;
            taskTwo.OnFailure = taskThree;

            schedule.Add(taskOne, true);
            schedule.Add(taskTwo, false);
            schedule.Add(taskThree, false);

            var scheduleIsLooping = schedule.IsLoopingSchedule();
            Assert.That(scheduleIsLooping, Is.EqualTo(false));
        }

        [Test]
        public void TestLoopingSchedule_OnFailure_IsDetectedSuccessfully()
        {
            var schedule = new SessionRunnerSchedule(null);

            var taskOne = new ScheduledTask();
            var taskTwo = new ScheduledTask();

            taskOne.OnFailure = taskTwo;
            taskTwo.OnFailure = taskOne;

            schedule.Add(taskOne, true);
            schedule.Add(taskTwo, false);

            var scheduleIsLooping = schedule.IsLoopingSchedule();
            Assert.That(scheduleIsLooping, Is.EqualTo(true));
        }

        [Test]
        public void TestNonLoopingSchedule_OnBranchedTask_IsDetectedSuccessfully()
        {
            var schedule = new SessionRunnerSchedule(null);
            var taskOne = new ScheduledTask();
            var taskTwo = new ScheduledTask();
            var taskThree = new ScheduledTask();
            var taskFour = new ScheduledTask();
            taskOne.OnSuccess = taskTwo;
            taskTwo.OnSuccess = taskFour;
            taskTwo.OnFailure = taskThree;
            taskThree.OnSuccess = taskFour;
            taskThree.OnFailure = taskFour;
            schedule.Add(taskOne, true);
            schedule.Add(taskTwo, false);
            schedule.Add(taskThree, false);
            schedule.Add(taskFour, false);
            var scheduleIsLooping = schedule.IsLoopingSchedule();
            Assert.That(scheduleIsLooping, Is.EqualTo(false));
        }

        [Test]
        public void TestLoopingSchedule_OnDividedTasksLoop()
        {
            var schedule = new SessionRunnerSchedule(null);
            var taskOne = new ScheduledTask();
            var taskTwo = new ScheduledTask();
            var taskThree = new ScheduledTask();
            var taskFour = new ScheduledTask();
            taskOne.OnSuccess = taskTwo;
            taskThree.OnSuccess = taskFour;
            taskFour.OnSuccess = taskThree;
            schedule.Add(taskOne, true);
            schedule.Add(taskTwo, false);
            schedule.Add(taskThree, false);
            schedule.Add(taskFour, false);
            var scheduleIsLooping = schedule.IsLoopingSchedule();
            Assert.That(scheduleIsLooping, Is.EqualTo(true));
        }
    }
}
