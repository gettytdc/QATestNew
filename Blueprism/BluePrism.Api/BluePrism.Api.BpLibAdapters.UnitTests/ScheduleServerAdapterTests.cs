namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Domain.Errors;
    using Domain.Filters;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Scheduling;
    using Server.Domain.Models;
    using Utilities.Testing;

    using static CommonTestClasses.SchedulesHelper;
    using static Func.OptionHelper;
    using BpLibScheduleParameters = Server.Domain.Models.ScheduleParameters;
    using ScheduledSession = Domain.ScheduledSession;
    using ScheduledTask = Domain.ScheduledTask;
    using ScheduleParameters = Domain.ScheduleParameters;

    [TestFixture]
    public class ScheduleServerAdapterTests : UnitTestBase<ScheduleServerAdapter>
    {
        [SetUp]
        public void SetUp() =>
            FilterMapper.SetFilterMappers(new IFilterMapper[]
            {
                new NullFilterMapper(),
                new StringStartsWithFilterMapper(),
                new MultiValueFilterMapper(),
                new EqualsFilterMapper()
            });

        [Test]
        public async Task GetSchedules_ShouldReturnSuccess_WhenSuccessful()
        {
            var schedules = new List<Scheduling.ScheduleSummary> { new Scheduling.ScheduleSummary() };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(schedules);

            var result = await ClassUnderTest.GetSchedules(GetTestScheduleParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedules_ShouldReturnSuccess_WhenNoSchedules()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(Array.Empty<ScheduleSummary>());

            var result = await ClassUnderTest.GetSchedules(GetTestScheduleParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedules_ShouldReturnExpectedItems_WhenSuccessful()
        {
            var scheduleSummaries = GetTestBluePrismScheduleSummary(3).ToArray();
            var scheduleParameters = GetTestScheduleParameters();
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Returns(scheduleSummaries);
            var result = await ClassUnderTest.GetSchedules(scheduleParameters);
            ValidateModelsAreEqual(scheduleSummaries, ((Success<ItemsPage<Schedule>>)result).Value.Items.ToArray());
        }

        [Test]
        public async Task GetSchedules_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummaries(It.IsAny<BpLibScheduleParameters>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetSchedules(GetTestScheduleParameters());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedule_ShouldReturnSuccess_WhenSuccessful()
        {
            const int scheduleId = 7;
            var schedule =  new ScheduleSummary { Id = scheduleId };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Returns(schedule);

            var result = await ClassUnderTest.GetSchedule(scheduleId);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedule_ShouldReturnReturnFailure_WhenScheduleNotFound()
        {
            const int scheduleId = 7;
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Throws(new NoSuchScheduleException(scheduleId));

            var result = await ClassUnderTest.GetSchedule(scheduleId);

            (result is Failure<ScheduleNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedule_ShouldReturnExpectedItems_WhenSuccessful()
        {
            const int scheduleId = 5;
            var scheduleSummary = GetTestBluePrismScheduleSummary(1).Single();
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(scheduleId))
                .Returns(scheduleSummary);

            var result = await ClassUnderTest.GetSchedule(5);

            ValidateModelsAreEqual(scheduleSummary, ((Success<Schedule>)result).Value);
        }

        [Test]
        public async Task GetSchedule_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduleSummary(4))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetSchedule(4);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnSuccess_WhenSuccessful()
        {
            var scheduleLogs = new List<Server.Domain.Models.ScheduleLog>
            {
                new Server.Domain.Models.ScheduleLog(), new Server.Domain.Models.ScheduleLog()
            };

            var scheduleLogParameters = GetTestScheduleLogItemParameters();
            scheduleLogParameters.ItemsPerPage = 4;

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<Server.Domain.Models.ScheduleLogParameters>()))
                .Returns(scheduleLogs);

            var result = await ClassUnderTest.GetScheduleLogs(scheduleLogParameters);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnSuccess_WhenNoScheduleLogs()
        {
            var scheduleLogParameters = GetTestScheduleLogItemParameters();

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<Server.Domain.Models.ScheduleLogParameters>()))
                .Returns(Array.Empty<Server.Domain.Models.ScheduleLog>());

            var result = await ClassUnderTest.GetScheduleLogs(scheduleLogParameters);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduleLogs_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            var scheduleLogParameters = GetTestScheduleLogItemParameters();

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<Server.Domain.Models.ScheduleLogParameters>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetScheduleLogs(scheduleLogParameters);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        private ScheduleParameters GetTestScheduleParameters() =>
            new ScheduleParameters()
            {
                Name = new NullFilter<string>(),
                RetirementStatus = new NullFilter<Domain.RetirementStatus>(),
                ItemsPerPage = 10,
                PagingToken = None<PagingToken<string>>()
            };

        [Test]
        public async Task GetScheduleLogs_ShouldReturnFailure_WhenNoSuchScheduleExceptionThrown()
        {
            var scheduleLogParameters = GetTestScheduleLogItemParameters();

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetCurrentAndPassedLogs(It.IsAny<Server.Domain.Models.ScheduleLogParameters>()))
                .Throws(new NoSuchScheduleException(5));

            var result = await ClassUnderTest.GetScheduleLogs(scheduleLogParameters);

            (result is Failure<ScheduleNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedules_OnSuccess_ReturnsExpectedPagingToken()
        {
            var scheduleSummary = GetTestBluePrismScheduleSummary(3, DateTimeOffset.UtcNow).ToList();

            var scheduleParameters = GetTestDomainScheduleParameters(itemsPerPage: 2);

            var testPagingToken = new PagingToken<string>
            {
                PreviousIdValue = scheduleSummary.OrderBy(x => x.Name).Last().Name,
                DataType = scheduleSummary.OrderBy(x => x.Name).Last().StartDate.GetType().Name,
                ParametersHashCode = scheduleParameters.GetHashCodeForValidation(),
            }.ToString();

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<Server.Domain.Models.ScheduleParameters>()))
                .Returns(scheduleSummary);

            var result = await ClassUnderTest.GetSchedules(scheduleParameters);
            var resultValue = ((Success<ItemsPage<Schedule>>)result).Value.PagingToken;

            ((Some<string>)resultValue).Value.Should().Be(testPagingToken);
        }

        [Test]
        public async Task GetSchedules_OnSuccessWhenNoMoreItemsLeftToReturn_ReturnsNonePagingToken()
        {
            var scheduleSummary = GetTestBluePrismScheduleSummary(3, DateTimeOffset.UtcNow).ToList();

            var scheduleParameters = GetTestDomainScheduleParameters();

            GetMock<IServer>()
                .Setup(m => m.SchedulerGetScheduleSummaries(It.IsAny<Server.Domain.Models.ScheduleParameters>()))
                .Returns(scheduleSummary);

            var result = await ClassUnderTest.GetSchedules(scheduleParameters);
            var resultValue = ((Success<ItemsPage<Schedule>>)result).Value.PagingToken;

            resultValue.Should().BeAssignableTo<None<string>>();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnSuccess_WhenSuccessful()
        {
            var scheduleTasks = new List<Server.Domain.Models.ScheduledTask>
            {
                new Server.Domain.Models.ScheduledTask()
            };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Returns(scheduleTasks);

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnExpectedData_WhenSuccessful()
        {
            var domainScheduledTasks = GetDomainScheduledTasks();

            var scheduledTasks = GetServerScheduledTasks();

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Returns(scheduledTasks);

            var result = await ClassUnderTest.GetScheduledTasks(1);

            ((Success<IEnumerable<ScheduledTask>>)result).Value.ShouldBeEquivalentTo(domainScheduledTasks);
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnEmptyList_WhenThereAreNoScheduledTasks()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Returns(new List<Server.Domain.Models.ScheduledTask>());

            var result = await ClassUnderTest.GetScheduledTasks(1);
            var resultValue = ((Success<IEnumerable<ScheduledTask>>)result).Value.ToList();

            resultValue.Should().BeEmpty();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnSuccess_WhenThereAreNoScheduledTasks()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Returns(new List<Server.Domain.Models.ScheduledTask>());

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnFailure_WhenNoSuchScheduleExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Throws(new NoSuchScheduleException(1));

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Failure<ScheduleNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnFailure_WhenDeletedScheduleExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Throws(new DeletedScheduleException(1));

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Failure<DeletedScheduleError>).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledTasks_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetScheduledTasks(It.IsAny<int>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetScheduledTasks(1);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        private static List<Server.Domain.Models.ScheduledTask> GetServerScheduledTasks() =>
           new List<Server.Domain.Models.ScheduledTask>
           {
                new Server.Domain.Models.ScheduledTask
                {
                    Name = "name1",
                    Description = "desc1",
                    DelayAfterEnd = 1,
                    FailFastOnError = true,
                    OnSuccessTaskName = "name",
                    OnSuccessTaskId = 1,
                    OnFailureTaskName = "name",
                    OnFailureTaskId = 1
                },
                new Server.Domain.Models.ScheduledTask
                {
                    Name = "name2",
                    Description = "desc2",
                    DelayAfterEnd = 2,
                    FailFastOnError = true,
                    OnSuccessTaskName = "name",
                    OnSuccessTaskId = 1,
                    OnFailureTaskName = "name",
                    OnFailureTaskId = 1
                },
           };

        private static List<ScheduledTask> GetDomainScheduledTasks() =>
            new List<ScheduledTask>
            {
                new ScheduledTask
                {
                    Name = "name1",
                    Description = "desc1",
                    DelayAfterEnd = 1,
                    FailFastOnError = true,
                    OnSuccessTaskName = "name",
                    OnSuccessTaskId = 1,
                    OnFailureTaskName = "name",
                    OnFailureTaskId = 1
                },
                new ScheduledTask
                {
                    Name = "name2",
                    Description = "desc2",
                    DelayAfterEnd = 2,
                    FailFastOnError = true,
                    OnSuccessTaskName = "name",
                    OnSuccessTaskId = 1,
                    OnFailureTaskName = "name",
                    OnFailureTaskId = 1
                },
            };

        private static Domain.ScheduleLogParameters GetTestScheduleLogItemParameters() =>
            new Domain.ScheduleLogParameters
            {
                ItemsPerPage = 10,
                ScheduleId = new EqualsFilter<int>(5),
                StartTime = new NullFilter<DateTimeOffset>(),
                EndTime = new NullFilter<DateTimeOffset>(),
                ScheduleLogStatus = new NullFilter<ScheduleLogStatus>()
            };

        [Test]
        public async Task GetScheduledSessions_ShouldReturnSuccess_WhenSuccess()
        {
            var scheduledSessions = new List<Server.Domain.Models.ScheduledSession>
            {
                new Server.Domain.Models.ScheduledSession()
            };

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Returns(scheduledSessions);

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnExpectedData_WhenSuccessful()
        {
            var domainScheduledSessions = GetDomainScheduledSession();
            var scheduledSessions = GetServerScheduledSessions();

            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Returns(scheduledSessions);

            var result = await ClassUnderTest.GetScheduledSessions(1);

            ((Success<IEnumerable<ScheduledSession>>)result).Value.ShouldBeEquivalentTo(domainScheduledSessions);
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnSuccess_WhenThereAreNoScheduledSessions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Returns(Array.Empty<Server.Domain.Models.ScheduledSession>());

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnEmptyList_WhenThereAreNoScheduledSessions()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Returns(new List<Server.Domain.Models.ScheduledSession>());

            var result = await ClassUnderTest.GetScheduledSessions(1);
            var resultValue = ((Success<IEnumerable<ScheduledSession>>)result).Value.ToList();

            resultValue.Should().BeEmpty();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnFailure_WhenNoSuchTaskExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Throws(new NoSuchTaskException(1));

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Failure<TaskNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnFailure_WhenDeletedScheduleExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Throws(new DeletedScheduleException(1));

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Failure<DeletedScheduleError>).Should().BeTrue();
        }

        [Test]
        public async Task GetScheduledSessions_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.SchedulerGetSessionsWithinTask(It.IsAny<int>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetScheduledSessions(1);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        private static List<Server.Domain.Models.ScheduledSession> GetServerScheduledSessions() =>
            new List<Server.Domain.Models.ScheduledSession>
            {
                new Server.Domain.Models.ScheduledSession {ProcessName = "name1", ResourceName = "name1"},
                new Server.Domain.Models.ScheduledSession {ProcessName = "name2", ResourceName = "name2"},
            };

        private static List<ScheduledSession> GetDomainScheduledSession() =>
            new List<ScheduledSession>
            {
                new ScheduledSession {ProcessName = "name1", ResourceName = "name1"},
                new ScheduledSession {ProcessName = "name2", ResourceName = "name2"},
            };
    }
}
