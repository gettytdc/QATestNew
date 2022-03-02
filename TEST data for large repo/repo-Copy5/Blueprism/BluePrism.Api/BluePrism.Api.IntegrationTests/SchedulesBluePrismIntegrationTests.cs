namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Threading.Tasks;
    using Apps72.Dev.Data.DbMocker;
    using Autofac;
    using AutomateAppCore;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Scheduling;
    using Server.Domain.Models;
    using Services;

    using IntervalType = Domain.IntervalType;
    using NthOfMonth = Domain.NthOfMonth;

    using static Func.OptionHelper;

    [TestFixture]
    public class SchedulesBluePrismIntegrationTests : BluePrismIntegrationTestBase<SchedulesService>
    {
        public override void Setup() =>
            base.Setup(builder =>
            {
                builder.RegisterType<DatabaseBackedScheduleStore>().As<Scheduling.IScheduleStore>();
            });

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnSuccess_WritesExpectedValueToDatabase()
        {
            var scheduleId = new Random().Next();
            var triggerDate = DateTimeOffset.UtcNow;

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)
                    && cmd.CommandText.Contains("FROM BPASchedule")
                    && cmd.Parameters.ByName("@id").Value.As<int>() == scheduleId
                )
                .ReturnsDataset(cmd => new[]
                {
                    MockTable.WithColumns("Id", "Name", "Description", "InitialTaskId",  "VersionNumber", "Retired", "Stopped")
                        .AddRow(scheduleId, "Test Schedule", "This is a test", 1, 2, false, false),
                    MockTable.WithColumns("Id", "Name", "Description", "FailFastOnError", "DelayAfterEnd", "OnSuccess", "OnFailure")
                        .AddRow(1, "Test Schedule Task", "This is a test", false, false, 0, 0),
                    MockTable.WithColumns("TaskId", "Id", "ProcessId", "ResourceName", "ProcessParams", "ResourceId"),
                    MockTable.WithColumns("UnitType", "Mode", "Priority", "StartDate", "EndDate", "DaySet", "CalendarId", "NthOfMonth", "MissingDatePolicy", "UserTrigger", "TimeZoneId", "UtcOffset"),
                });

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && (
                        cmd.CommandText.StartsWith("delete from BPATaskSession where taskid=@taskid;", StringComparison.InvariantCultureIgnoreCase)
                        || cmd.CommandText.StartsWith("delete from BPATask where scheduleid = @scheduleid and name is null", StringComparison.InvariantCultureIgnoreCase)
                        || cmd.CommandText.Equals("delete from BPAScheduleTrigger where scheduleid = @scheduleid", StringComparison.InvariantCultureIgnoreCase)
                    ))
                .ReturnsScalar(1);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.StartsWith("select 1 from BPASchedule where id != @scheduleid and name = @name"))
                .ReturnsTable(MockTable.Empty());

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select versionno from BPASchedule where id=@scheduleid;"))
                .ReturnsScalar(123);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select versionno from BPADataTracker where dataname = @dname"))
                .ReturnsScalar(123);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("select isnull(u.username, '['+u.systemusername+']') as username  from BPAUser u where u.userid=@id", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar("testuser");

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("select name from BPASchedule where id=@id", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar("Test Schedule");

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("SELECT name from BPATask WHERE id=@id", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar("Test Task");

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("select r.name from BPAResource r where r.resourceid=@id", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar("Test Resource");

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("select p.name from BPAProcess p where p.processid=@id", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar("Test Process");

            var hasBeenCalled = false;

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("insert into BPAScheduleTrigger"))
                .ReturnsScalar(cmd =>
                {
                    cmd.Parameters.ByName("@unittype").Value.Should().Be(Scheduling.IntervalType.Once);
                    cmd.Parameters.ByName("@startdate").Value.As<SqlDateTime>().Value.Should().BeCloseTo(triggerDate.DateTime, 1000);
                    cmd.Parameters.ByName("@usertrigger").Value.Should().Be(false);
                    cmd.Parameters.ByName("@utcoffset").Value.Should().Be(DBNull.Value);

                    hasBeenCalled = true;

                    return 0;
                });

            ConfigureFallbackForUpdateAndInsert();

            _ = await Subject.ScheduleOneOffScheduleRun(scheduleId, triggerDate);

            hasBeenCalled.Should().BeTrue();
        }

        [Test]
        public async Task ScheduleOneOffScheduleRun_OnMissingSchedule_ReturnsScheduleNotFoundError()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)
                    && cmd.CommandText.Contains("FROM BPASchedule")
                )
                .ReturnsDataset(cmd => new[]
                {
                    MockTable.WithColumns("Id", "Name", "Description", "InitialTaskId",  "VersionNumber", "Retired", "Stopped"),
                    MockTable.WithColumns("Id", "Name", "Description", "FailFastOnError", "DelayAfterEnd", "OnSuccess", "OnFailure"),
                    MockTable.WithColumns("TaskId", "Id", "ProcessId", "ResourceName", "ProcessParams", "ResourceId"),
                    MockTable.WithColumns("UnitType", "Mode", "Priority", "StartDate", "EndDate", "DaySet", "CalendarId", "NthOfMonth", "MissingDatePolicy", "UserTrigger", "TimeZoneId", "UtcOffset"),
                });

            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.ScheduleOneOffScheduleRun(123, DateTimeOffset.UtcNow);

            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }

        [Test]
        public async Task GetScheduledTasks_OnScheduledTasksFound_ReturnsScheduleTasks()
        {
            SetupMockData(1);
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetScheduledTasks(1);

            result.Should().BeAssignableTo<Success<IEnumerable<Domain.ScheduledTask>>>();

            var resultValue = ((Success<IEnumerable<Domain.ScheduledTask>>)result).Value.ToList();
            var expectedValue = new Domain.ScheduledTask
            {
                Id = 1,
                Name = "name",
                Description = "desc",
                DelayAfterEnd = 1,
                FailFastOnError = true,
                OnSuccessTaskId = 1,
                OnSuccessTaskName = "Task1",
                OnFailureTaskId = 2,
                OnFailureTaskName = "Task2"
            };

            resultValue.Count.Should().Be(1);
            resultValue[0].ShouldBeEquivalentTo(expectedValue);
        }

        [Test]
        public async Task GetScheduledTasks_OnEmptyScheduledTasksList_ReturnsEmptyList()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select t.id")
                    && cmd.CommandText.Contains("from BPATask")
                    && (int)cmd.Parameters.ByName("@id").Value == 1)
                .ReturnsTable(MockTable.Empty());
            SetupMockData(1);
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetScheduledTasks(1);
            var resultValue = ((Success<IEnumerable<Domain.ScheduledTask>>)result).Value.ToList();
            resultValue.Should().BeEmpty();
        }

        [Test]
        public async Task GetScheduledTasks_OnScheduleNotFound_ThrowScheduleNotFoundError()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select t.id")
                    && cmd.CommandText.Contains("from BPATask")
                    && (int)cmd.Parameters.ByName("@id").Value != 1)
                .ThrowsException(new NoSuchScheduleException(1));
            SetupMockData(1);
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetScheduledTasks(2);
            result.Should().BeAssignableTo<Failure<ScheduleNotFoundError>>();
        }

        [Test]
        public async Task GetScheduledTasks_OnScheduleDeleted_ThrowDeletedScheduleError()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select t.id")
                    && cmd.CommandText.Contains("from BPATask")
                    && (int)cmd.Parameters.ByName("@id").Value == 1)
                .ThrowsException(new DeletedScheduleException(1));
            SetupMockData(1);

            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetScheduledTasks(1);
            result.Should().BeAssignableTo<Failure<DeletedScheduleError>>();
        }

        [Test]
        public async Task GetSchedule_CorrectlyRetrievesDataFromDatabase()
        {
            var startPointTotalSeconds = (int)new TimeSpan(8, 10, 22).TotalSeconds;
            var endPointTotalSeconds = (int)new TimeSpan(10, 15, 44).TotalSeconds;

            var testSchedule = new Schedule
            {
                Id = 5,
                Description = "test description",
                Name = "test name",
                InitialTaskId = 8,
                IntervalType = IntervalType.Hour,
                CalendarId = 2,
                CalendarName = "test name",
                DayOfMonth = NthOfMonth.Second,
                DayOfWeek = Some(DayOfWeek.Wednesday),
                EndDate = Some(new DateTimeOffset(new DateTime(2021, 04, 29, 15, 44, 12))),
                EndPoint = new DateTimeOffset(DateTime.Now.Date.AddSeconds(endPointTotalSeconds)),
                IsRetired = false,
                StartDate = Some(new DateTimeOffset(new DateTime(2021, 04, 29, 12, 16, 44))),
                StartPoint = new DateTimeOffset(DateTime.Now.Date.AddSeconds(startPointTotalSeconds)),
                TasksCount = 4,
                TimePeriod = 3,
            };

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.IndexOf("from BPASchedule", StringComparison.OrdinalIgnoreCase) >= 0)
                .ReturnsTable(MockTable
                    .WithColumns("id", "name", "description", "initialtaskid", "retired", "taskscount", "unittype", "period", "startpoint", "endpoint", "dayset", "nthofmonth", "startdate", "enddate", "timezoneid", "utcoffset", "calendarid", "calendarname")
                    .AddRow(
                        testSchedule.Id,
                        testSchedule.Name,
                        testSchedule.Description,
                        testSchedule.InitialTaskId,
                        testSchedule.IsRetired,
                        testSchedule.TasksCount,
                        (int)testSchedule.IntervalType,
                        testSchedule.TimePeriod,
                        startPointTotalSeconds,
                        endPointTotalSeconds,
                        new DaySet(((Some<DayOfWeek>)testSchedule.DayOfWeek).Value).ToInt(),
                        (int)testSchedule.DayOfMonth,
                        ((Some<DateTimeOffset>)testSchedule.StartDate).Value.DateTime,
                        ((Some<DateTimeOffset>)testSchedule.EndDate).Value.DateTime,
                        "GMT Standard Time",
                        DBNull.Value,
                        testSchedule.CalendarId,
                        testSchedule.CalendarName));

            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetSchedule(testSchedule.Id);

            ((Success<Schedule>)result).Value.ShouldBeEquivalentTo(testSchedule);
        }

        [Test]
        public async Task GetScheduledTasks_OnScheduleDeleted_ThrowPermissionError()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select t.id")
                    && cmd.CommandText.Contains("from BPATask")
                    && (int)cmd.Parameters.ByName("@id").Value == 1)
                .ThrowsException(new PermissionException());
            SetupMockData(1);

            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetScheduledTasks(1);
            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        private void SetupMockData(int scheduleId)
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select t.id")
                    && cmd.CommandText.Contains("from BPATask")
                    && (int)cmd.Parameters.ByName("@id").Value == scheduleId)
                .ReturnsTable(MockTable
                    .WithColumns("id", "name", "deletedname", "description", "successTaskID", "successTaskName", "FailureTaskID", "FailureTaskName", "failfastonerror", "delayafterend")
                    .AddRow(
                        1,
                        "name",
                        null,
                        "desc",
                        1,
                        "Task1",
                        2,
                        "Task2",
                        true,
                        1));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select id from BPASchedule where id = @scheduleid"))
                .ReturnsScalar(1);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains(
                        "select id from BPASchedule where id = @scheduleId and deletedname is not null"))
                .ReturnsDataset();
        }
    }
}
