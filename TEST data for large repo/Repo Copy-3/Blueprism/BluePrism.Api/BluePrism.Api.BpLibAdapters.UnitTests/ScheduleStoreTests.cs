namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BluePrism.Api.Domain;
    using BluePrism.Server.Domain.Models;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class ScheduleStoreTests : UnitTestBase<ScheduleStore>
    {
        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_ReturnsSuccess()
        {
            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(123))
                .Returns(() => GetMock<Scheduling.ISchedule>().Object);

            GetMock<Scheduling.ISchedule>()
                .SetupGet(m => m.Name)
                .Returns("Test");

            var result = await ClassUnderTest.ScheduleOneOffScheduleRun(123, DateTime.UtcNow);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_AddsTriggerToSchedule()
        {
            var scheduleId = new Random().Next(1, 10000);
            var triggerTime = DateTime.UtcNow;

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(scheduleId))
                .Returns(() => GetMock<Scheduling.ISchedule>().Object);

            GetMock<Scheduling.ISchedule>()
                .SetupGet(m => m.Name)
                .Returns("Test Schedule");

            _ = await ClassUnderTest.ScheduleOneOffScheduleRun(scheduleId, triggerTime);

            GetMock<Scheduling.IScheduleStore>()
                .Verify(m => m.TriggerSchedule(It.Is<Scheduling.ISchedule>(s => s.Name == "Test Schedule"), triggerTime), Times.Once);
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_ReturnsScheduledRunTime()
        {
            var scheduleId = new Random().Next(1, 10000);
            var expectedTriggerTime = DateTime.UtcNow;

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(scheduleId))
                .Returns(() => GetMock<Scheduling.ISchedule>().Object);

            GetMock<Scheduling.ISchedule>()
                .SetupGet(m => m.Name)
                .Returns("Test");

            GetMock<Scheduling.IScheduleStore>()
                .Setup(m => m.TriggerSchedule(It.IsAny<Scheduling.ISchedule>(), It.IsAny<DateTime>()))
                .Returns(expectedTriggerTime);

            var result = await ClassUnderTest.ScheduleOneOffScheduleRun(scheduleId, DateTime.MinValue);

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.Should().Be(expectedTriggerTime));
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnScheduleNotFound_ReturnsScheduleNotFoundError()
        {
            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => GetMock<Scheduling.ISchedule>().Object);

            GetMock<Scheduling.ISchedule>()
                .SetupGet(m => m.Name)
                .Returns(string.Empty);

            var result = await ClassUnderTest.ScheduleOneOffScheduleRun(123, DateTime.UtcNow);

            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnSuccess_WhenRetireScheduleSuccessful()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null) { Name = "name", Retired = false });

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenScheduleAlreadyRetiredErrorThrown()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null) { Name = "name", Retired = true });

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            result.Should().BeAssignableTo<Failure<ScheduleAlreadyRetiredError>>();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnSuccess_WhenUnretireScheduleSuccessful()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null) { Name = "name", Retired = true });

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenScheduleNotRetiredErrorThrown()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null) { Name = "name", Retired = false });

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            result.Should().BeAssignableTo<Failure<ScheduleNotRetiredError>>();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenScheduleNotFoundErrorThrown()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null));

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenRetirePermissionExceptionThrown()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null) { Name = "name", Retired = false });

            GetMock<Scheduling.IScheduleStore>()
                .Setup(m => m.RetireSchedule(It.IsAny<Scheduling.ISchedule>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenUnretirePermissionExceptionThrown()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetSchedule(It.IsAny<int>()))
                .Returns(() => new SessionRunnerSchedule(null) { Name = "name", Retired = true });

            GetMock<Scheduling.IScheduleStore>()
                .Setup(m => m.UnretireSchedule(It.IsAny<Scheduling.ISchedule>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Failure<PermissionError>).Should().BeTrue();
        }
    }
}
