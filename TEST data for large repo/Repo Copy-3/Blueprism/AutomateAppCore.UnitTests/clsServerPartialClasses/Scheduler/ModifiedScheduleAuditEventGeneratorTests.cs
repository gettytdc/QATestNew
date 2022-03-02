using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.clsServerPartialClasses.Scheduler;
using BluePrism.BPCoreLib;
using BluePrism.Scheduling;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.Scheduler
{
    public class ModifiedScheduleAuditEventGeneratorTests
    {
        private SessionRunnerSchedule _oldSchedule;
        private SessionRunnerSchedule _newSchedule;
        private ScheduledTask _oldTask;
        private ScheduledTask _newTask;
        private IUser _user;

        [SetUp]
        public void SetUp()
        {
            _oldSchedule = CreateNewSchedule();
            _newSchedule = CreateNewSchedule();
            _oldTask = CreateNewTask(1, _oldSchedule);
            _newTask = CreateNewTask(1, _newSchedule);
            var mockUser = new Mock<IUser>();
            _user = mockUser.Object;
        }

        [Test]
        public void Generate_ScheduleAndTasksNotModified_ShouldReturnNoEvents()
        {
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(0);
        }

        [Test]
        public void Generate_ScheduleNameChanged_ShouldReturnEventWithCorrectComment()
        {
            _newSchedule.Name = "Some other name";
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Name");
        }

        [Test]
        public void Generate_ScheduleDescriptionChanged_ShouldReturnEventWithCorrectComment()
        {
            _newSchedule.Description = "Some other description";
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Description");
        }

        [Test]
        public void Generate_ScheduleInitialTaskIdChanged_ShouldReturnEventWithCorrectComment()
        {
            _newSchedule.InitialTaskId = 2;
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Initial Task");
        }

        [Test]
        public void Generate_NoMatchingTasks_ShouldReturnNoEvents()
        {
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(0);
        }

        [Test]
        public void Generate_TaskNameChanged_ShouldReturnEventWithCorrectComment()
        {
            _newTask.Name = "Some other name";
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Name");
        }

        [Test]
        public void Generate_TaskDelayAfterEndChanged_ShouldReturnEventWithCorrectComment()
        {
            _newTask.DelayAfterEnd = 2;
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Delay After End");
        }

        [TestCase("")]
        [TestCase("Some new description")]
        [TestCase(null)]
        public void GetAuditEventsForModifiedTasks_TaskDescriptionChanged_ShouldReturnEventWithCorrectComment(string newDescription)
        {
            _newTask.Description = newDescription;
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Description");
        }

        [TestCaseSource("GetTestCasesForOnSuccessOrFailure")]
        public void Generate_TaskOnSuccessChanged_ShouldReturnEventWithCorrectComment(ScheduledTask oldValue, ScheduledTask newValue)
        {
            _oldTask.OnSuccess = oldValue;
            _newTask.OnSuccess = newValue;
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("On Completed");
        }

        [TestCaseSource("GetTestCasesForOnSuccessOrFailure")]
        public void Generate_TaskOnFailureChanged_ShouldReturnEventWithCorrectComment(ScheduledTask oldValue, ScheduledTask newValue)
        {
            _oldTask.OnFailure = oldValue;
            _newTask.OnFailure = newValue;
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("On Failure");
        }

        [Test]
        public void Generate_AllowedHoursStartChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldAllowedHours = new clsTimeRange(TimeSpan.FromDays(5d), TimeSpan.FromDays(6d));
            var newAllowedHours = new clsTimeRange(TimeSpan.FromDays(4d), TimeSpan.FromDays(6d));
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { AllowedHours = oldAllowedHours });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { AllowedHours = newAllowedHours });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Allowed Hours - Start");
        }

        [Test]
        public void Generate_CalendarChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { CalendarId = 1 });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { CalendarId = 2 });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Calendar");
        }

        [Test]
        public void Generate_DaysChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldDays = new DaySet(DayOfWeek.Monday, DayOfWeek.Tuesday);
            var newDays = new DaySet(DayOfWeek.Wednesday, DayOfWeek.Thursday);
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Days = oldDays });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Days = newDays });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Days");
        }

        [Test]
        public void Generate_IntervalChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Interval = IntervalType.Day });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Interval = IntervalType.Month });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Interval");
        }

        [Test]
        public void Generate_MissingDatePolicyChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { MissingDatePolicy = NonExistentDatePolicy.Skip });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { MissingDatePolicy = NonExistentDatePolicy.LastSupportedDayInMonth });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Missing Date Policy");
        }

        [Test]
        public void Generate_NthOfMonthChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Nth = NthOfMonth.First });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Nth = NthOfMonth.Second });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Nth of Month");
        }

        [Test]
        public void Generate_PeriodChanged_ShouldReturnEventWithCorrectComment()
        {
            var oldTriggerMock = new Mock<ITrigger>();
            var newTriggerMock = new Mock<ITrigger>();
            oldTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Period = 2 });
            newTriggerMock.Setup(x => x.PrimaryMetaData).Returns(new TriggerMetaData() { Period = 3 });
            oldTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            newTriggerMock.Setup(x => x.IsUserTrigger).Returns(true);
            _oldSchedule.SetTrigger(oldTriggerMock.Object);
            _newSchedule.SetTrigger(newTriggerMock.Object);
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Period");
        }

        [Test]
        public void Generate_TaskFailFastOnErrorChanged_ShouldReturnEventWithCorrectComment()
        {
            _newTask.FailFastOnError = false;
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(1);
            auditEvents.FirstOrDefault()?.Comment.Should().StartWith("Fail Fast On Error");
        }

        [Test]
        public void Generate_ScheduleAndTaskModified_ShouldReturnTwoEvents()
        {
            _newSchedule.Name = "Another name";
            _newTask.Name = "Another task name";
            var generator = new ModifiedScheduleAuditEventGenerator(_oldSchedule, _newSchedule, _user);
            var auditEvents = generator.Generate();
            auditEvents.Should().HaveCount(2);
        }

        protected static IEnumerable<ScheduledTask[]> GetTestCasesForOnSuccessOrFailure()
        {
            var task3 = CreateNewTask(3, new SessionRunnerSchedule(null));
            var task4 = CreateNewTask(4, new SessionRunnerSchedule(null));
            yield return new[] { null, task3 };
            yield return new[] { task3, null };
            yield return new[] { task3, task4 };
        }

        private static SessionRunnerSchedule CreateNewSchedule()
        {
            var schedule = new SessionRunnerSchedule(null)
            {
                Id = 1,
                Name = "Schedule 1",
                Description = "A bit of this and a bit of that",
                InitialTaskId = 1
            };
            return schedule;
        }

        private static ScheduledTask CreateNewTask(int id, SessionRunnerSchedule schedule)
        {
            var mockUser = new Mock<IUser>();
            var task = schedule.NewTask();
            task.Id = id;
            task.Name = "Task 1";
            task.Description = "Some description";
            task.FailFastOnError = true;
            task.OnSuccess = null;
            task.OnFailure = null;
            schedule.Add(task);
            return task;
        }
    }
}
