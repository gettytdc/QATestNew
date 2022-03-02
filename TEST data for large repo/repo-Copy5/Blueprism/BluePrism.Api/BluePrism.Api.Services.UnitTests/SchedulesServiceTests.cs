namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using CommonTestClasses.Extensions;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Logging;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;
    using static Func.ResultHelper;

    [TestFixture]
    public class SchedulesServiceTests : UnitTestBase<SchedulesService>
    {
        public override void Setup() =>
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>))
                    .As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });

        [Test]
        public async Task GetSchedules_ShouldReturnSuccess_WhenSuccessful()
        {
            var schedulesPage = new ItemsPage<Schedule> { Items = new List<Schedule> { new Schedule { Id = 1 } } };

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetSchedules(It.IsAny<ScheduleParameters>()))
                .ReturnsAsync(Succeed(schedulesPage));

            var result = await ClassUnderTest.GetSchedules(new ScheduleParameters());

            (result is Success).Should().BeTrue();
            result.OnSuccess(x => x.ShouldBeEquivalentTo(schedulesPage));
        }

        [Test]
        public async Task GetSchedules_ShouldReturnSuccess_WhenNoSchedules()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetSchedules(It.IsAny<ScheduleParameters>()))
                .ReturnsAsync(Succeed(new ItemsPage<Schedule> { Items = Array.Empty<Schedule>() }));

            var result = await ClassUnderTest.GetSchedules(new ScheduleParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedules_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetSchedules(It.IsAny<ScheduleParameters>()))
                .ReturnsAsync(ResultHelper<ItemsPage<Schedule>>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetSchedules(new ScheduleParameters());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedule_ShouldReturnSuccess_WhenSuccessful()
        {
            const int scheduleId = 5;
            var schedule =  new Schedule { Id = scheduleId };

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetSchedule(scheduleId))
                .ReturnsAsync(Succeed(schedule));

            var result = await ClassUnderTest.GetSchedule(scheduleId);

            (result is Success).Should().BeTrue();
            result.OnSuccess(x => x.ShouldBeEquivalentTo(schedule));
        }

        [Test]
        public async Task GetSchedule_OnScheduleNotFound_ReturnsScheduleNotFoundError()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetSchedule(7))
                .ReturnsAsync(ResultHelper<Schedule>.Fail<ScheduleNotFoundError>());

            var result = await ClassUnderTest.GetSchedule(7);

            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }

        [Test]
        public async Task GetSchedule_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetSchedule(5))
                .ReturnsAsync(ResultHelper<Schedule>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetSchedule(5);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnSuccess_WhenSuccessful()
        {
            var scheduleLogs = new List<ScheduleLog> { new ScheduleLog { ScheduleId = 1 }, new ScheduleLog { ScheduleId = 2 }, };

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduleLogs(It.IsAny<ScheduleLogParameters>()))
                .ReturnsAsync(Succeed(new ItemsPage<ScheduleLog> {Items = scheduleLogs}));

            var result = await ClassUnderTest.GetScheduleLogs(new ScheduleLogParameters());

            (result is Success).Should().BeTrue();
            result.OnSuccess(x => x.Items.Should().BeEquivalentTo(scheduleLogs));
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnSuccess_WhenNoScheduleLogs()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduleLogs(It.IsAny<ScheduleLogParameters>()))
                .ReturnsAsync(Succeed( new ItemsPage<ScheduleLog>{ Items = Array.Empty<ScheduleLog>() }));

            var result = await ClassUnderTest.GetScheduleLogs(new ScheduleLogParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_ReturnsSuccess()
        {
            var scheduleId = new Random().Next();
            var runTime = DateTimeOffset.UtcNow;

            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(scheduleId, runTime.DateTime))
                .ReturnsAsync(Succeed(runTime.DateTime));

            var result = await ClassUnderTest.ScheduleOneOffScheduleRun(scheduleId, runTime);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_ReturnsScheduledDateTime()
        {
            var random = new Random();
            var scheduleId = random.Next();
            var runTime = DateTimeOffset.UtcNow;
            var expectedResult = runTime.AddMilliseconds(-random.Next());

            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(scheduleId, runTime.DateTime))
                .ReturnsAsync(Succeed(expectedResult.DateTime));

            var result = await ClassUnderTest.ScheduleOneOffScheduleRun(scheduleId, runTime);

            result.OnSuccess(x => x.Should().Be(expectedResult));
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnScheduleNotFound_ReturnsScheduleNotFoundError()
        {
            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(ResultHelper<DateTime>.Fail<ScheduleNotFoundError>());

            var result = await ClassUnderTest.ScheduleOneOffScheduleRun(123, DateTimeOffset.UtcNow);

            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_LogsInfoMessage()
        {
            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(Succeed(DateTime.UtcNow));

            _ = await ClassUnderTest.ScheduleOneOffScheduleRun(123, DateTimeOffset.UtcNow);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnScheduleNotFound_LogsDebugMessage()
        {
            GetMock<IScheduleStore>()
                .Setup(m => m.ScheduleOneOffScheduleRun(It.IsAny<int>(), It.IsAny<DateTime>()))
                .ReturnsAsync(ResultHelper<DateTime>.Fail<ScheduleNotFoundError>());

            _ = await ClassUnderTest.ScheduleOneOffScheduleRun(123, DateTimeOffset.UtcNow);

            GetMock<ILogger<SchedulesService>>().ShouldLogDebugMessages();
        }


        [Test]
        public async Task ModifySchedule_ShouldReturnSuccess_WhenSuccessful()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(x => x.ModifySchedule(scheduleId, scheduleChanges))
                .ReturnsAsync(Succeed());

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenScheduleDoesntExist()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(x => x.ModifySchedule(scheduleId, scheduleChanges))
                .ReturnsAsync(Fail<ScheduleNotFoundError>());

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Failure<ScheduleNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenRetireRetiredSchedule()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(x => x.ModifySchedule(scheduleId, scheduleChanges))
                .ReturnsAsync(Fail<ScheduleAlreadyRetiredError>());

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Failure<ScheduleAlreadyRetiredError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldReturnFailure_WhenUnretireNotRetiredSchedule()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IScheduleStore>()
                .Setup(x => x.ModifySchedule(scheduleId, scheduleChanges))
                .ReturnsAsync(Fail<ScheduleNotRetiredError>());

            var result = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            (result is Failure<ScheduleNotRetiredError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifySchedule_ShouldLogInfoMessage_WhenSuccessful()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Succeed());

            _ = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task ModifySchedule_ShouldLogDebugMessage_WhenScheduleNotFound()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail<ScheduleNotFoundError>());

            _ = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            GetMock<ILogger<SchedulesService>>().ShouldLogDebugMessages();
        }

        [Test]
        public async Task ModifySchedule_ShouldLogInfoMessage_WhenUserDoesntHavePermissionToRetireSchedule()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail(new PermissionError("")));

            _ = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task ModifySchedule_ShouldLogInfoMessage_WhenUserDoesntHavePermissionToUnretireSchedule()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail(new PermissionError("")));

            _ = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task ModifySchedule_ShouldLogInfoMessage_WhenUserRetireAlreadyRetiredSchedule()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = true };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail<ScheduleAlreadyRetiredError>);

            _ = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task ModifySchedule_ShouldLogInfoMessage_WhenUserUnretireNotRetiredSchedule()
        {
            var scheduleId = 1;
            var scheduleChanges = new Schedule { IsRetired = false };

            GetMock<IScheduleStore>()
                .Setup(m => m.ModifySchedule(It.IsAny<int>(), It.IsAny<Schedule>()))
                .ReturnsAsync(Fail<ScheduleNotRetiredError>);

            _ = await ClassUnderTest.ModifySchedule(scheduleId, scheduleChanges);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnSuccess_WhenSuccessful()
        {
            var scheduledSessions = new List<ScheduledSession> { new ScheduledSession() };

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(Succeed<IEnumerable<ScheduledSession>>(scheduledSessions));

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnExpectedData_WhenSuccessful()
        {
            var scheduledSession = new List<ScheduledSession>
            {
                new ScheduledSession
                {
                    ProcessName = "name1",
                    ResourceName = "name1"
                },
                new ScheduledSession
                {
                    ProcessName = "name2",
                    ResourceName = "name2"
                },
            };

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(Succeed<IEnumerable<ScheduledSession>>(scheduledSession));

            var result = await ClassUnderTest.GetScheduledSessions(1);

            result.OnSuccess(x => x.Should().BeEquivalentTo(scheduledSession));
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnSuccess_WhenNoScheduledSessions()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(Succeed<IEnumerable<ScheduledSession>>(Array.Empty<ScheduledSession>()));

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnTaskNotFoundError_WhenTaskNotFound()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledSession>>.Fail<TaskNotFoundError>());

            var result = await ClassUnderTest.GetScheduledSessions(1);

            result.Should().BeAssignableTo<Failure<TaskNotFoundError>>();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnDeletedScheduleError_WhenScheduleHasBeenDeleted()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledSession>>.Fail<DeletedScheduleError>());

            var result = await ClassUnderTest.GetScheduledSessions(1);

            result.Should().BeAssignableTo<Failure<DeletedScheduleError>>();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnPermissionsError_WhenUserDoesntHavePermission()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledSession>>.Fail(new PermissionError(string.Empty)));

            var result = await ClassUnderTest.GetScheduledSessions(1);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldLogDebugMessage_WhenTaskNotFound()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledSession>>.Fail<TaskNotFoundError>());

            await ClassUnderTest.GetScheduledSessions(1);

            GetMock<ILogger<SchedulesService>>().ShouldLogDebugMessages();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldLogDebugMessage_WhenScheduleHasBeenDeleted()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledSession>>.Fail<DeletedScheduleError>());

            await ClassUnderTest.GetScheduledSessions(1);

            GetMock<ILogger<SchedulesService>>().ShouldLogDebugMessages();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldLogInfoMessage_WhenUserDoesntHavePermission()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledSessions(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledSession>>.Fail(new PermissionError(string.Empty)));

            await ClassUnderTest.GetScheduledSessions(1);

            GetMock<ILogger<SchedulesService>>().ShouldLogInfoMessages();
        }

        [Test]
        public async Task ScheduledTask_ShouldReturnSuccess_WhenSuccessful()
        {
            var scheduledTasks = new List<ScheduledTask> {new ScheduledTask()};

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledTasks(It.IsAny<int>()))
                .ReturnsAsync(Succeed<IEnumerable<ScheduledTask>>(scheduledTasks));

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ScheduledTask_ShouldReturnExpectedData_WhenSuccessful()
        {
            var scheduledTasks = new List<ScheduledTask>
            {
                new ScheduledTask
                {
                    Name = "name1",
                    Description = "desc1",
                    DelayAfterEnd = 1,
                    FailFastOnError = true
                },
                new ScheduledTask
                {
                    Name = "name2",
                    Description = "desc2",
                    DelayAfterEnd = 2,
                    FailFastOnError = true
                },
            };

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledTasks(It.IsAny<int>()))
                .ReturnsAsync(Succeed<IEnumerable<ScheduledTask>>(scheduledTasks));

            var result = await ClassUnderTest.GetScheduledTasks(1);

            result.OnSuccess(x => x.Should().BeEquivalentTo(scheduledTasks));

        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnSuccess_WhenNoScheduledTasks()
        {
            var scheduledTasks = new List<ScheduledTask>();

            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledTasks(It.IsAny<int>()))
                .ReturnsAsync(Succeed<IEnumerable<ScheduledTask>>(scheduledTasks));

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledTasks_OnScheduleNotFound_ReturnsScheduleNotFoundError()
        {
            GetMock<IScheduleServerAdapter>()
                .Setup(x => x.GetScheduledTasks(It.IsAny<int>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ScheduledTask>>.Fail<ScheduleNotFoundError>());

            var result = await ClassUnderTest.GetScheduledTasks(1);

            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }
    }
}
