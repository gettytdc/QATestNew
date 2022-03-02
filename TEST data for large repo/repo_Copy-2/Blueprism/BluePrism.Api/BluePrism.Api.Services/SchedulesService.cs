namespace BluePrism.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using Domain.Filters;
    using Func;
    using Logging;

    public class SchedulesService : ISchedulesService
    {
        private readonly IAdapterAuthenticatedMethodRunner<IScheduleServerAdapter> _scheduleMethodRunner;
        private readonly IAdapterAuthenticatedMethodRunner<IScheduleStore> _scheduleStoreMethodRunner;
        private readonly ILogger<SchedulesService> _logger;

        public SchedulesService(
            IAdapterAuthenticatedMethodRunner<IScheduleServerAdapter> scheduleMethodRunner,
            IAdapterAuthenticatedMethodRunner<IScheduleStore> scheduleStoreMethodRunner,
            ILogger<SchedulesService> logger)
        {
            _scheduleMethodRunner = scheduleMethodRunner;
            _scheduleStoreMethodRunner = scheduleStoreMethodRunner;
            _logger = logger;
        }

        public Task<Result<ItemsPage<ScheduleLog>>> GetScheduleLogs(ScheduleLogParameters scheduleLogParameters) =>
            _scheduleMethodRunner.ExecuteForUser(x => x.GetScheduleLogs(scheduleLogParameters))
                .OnSuccess(() => _logger.Info("Successfully retrieved schedule logs"))
                .OnError((PermissionError _) => _logger.Debug("Attempted to retrieve schedule logs without permission"))
                .OnError((ScheduleNotFoundError _) => _logger.Debug("Logs for requested schedule {0} could not be found", ((EqualsFilter<int>)scheduleLogParameters.ScheduleId).EqualTo));

        public Task<Result<ItemsPage<Schedule>>> GetSchedules(ScheduleParameters scheduleParameters) =>
            _scheduleMethodRunner.ExecuteForUser(x => x.GetSchedules(scheduleParameters))
                .OnSuccess(() => _logger.Debug("Successfully retrieved schedules"))
                .OnError((PermissionError _) => _logger.Info("Attempted to retrieve schedules without permission"));

        public Task<Result<Schedule>> GetSchedule(int scheduleId) =>
            _scheduleMethodRunner.ExecuteForUser(x => x.GetSchedule(scheduleId))
                .OnSuccess(() => _logger.Debug("Successfully retrieved schedule"))
                .OnError((ScheduleNotFoundError _) => _logger.Info("Requested schedule {0} could not be found", scheduleId))
                .OnError((PermissionError _) => _logger.Info("Attempted to retrieve schedule without permission"));

        public Task<Result<DateTimeOffset>> ScheduleOneOffScheduleRun(int scheduleId, DateTimeOffset runTime) =>
            _scheduleStoreMethodRunner.ExecuteForUser(x => x.ScheduleOneOffScheduleRun(scheduleId, runTime.UtcDateTime))
                .ThenMap(x => new DateTimeOffset(x, TimeSpan.Zero))
                .OnSuccess(x => _logger.Info("Schedule {0} scheduled for one-time run at {1}", scheduleId, x))
                .OnError((ScheduleNotFoundError _) => _logger.Debug("Requested schedule {0} could not be found", scheduleId));

        public Task<Result> ModifySchedule(int scheduleId, Schedule scheduleChanges) =>
            _scheduleStoreMethodRunner.ExecuteForUser(x => x.ModifySchedule(scheduleId, scheduleChanges))
                .OnSuccess(() => _logger.Info("Schedule {0} was {1}", scheduleId, scheduleChanges.IsRetired ? "retired" : "unretired"))
                .OnError((ScheduleNotFoundError _) => _logger.Debug("Requested schedule {0} could not be found", scheduleId))
                .OnError((ScheduleAlreadyRetiredError _) => _logger.Info("Schedule {0} is already retired", scheduleId))
                .OnError((ScheduleNotRetiredError _) => _logger.Info("Schedule {0} is already not retired", scheduleId))
                .OnError((PermissionError _) => _logger.Info("Attempted to {0} schedule without permission", scheduleChanges.IsRetired ? "retire" : "unretire"));

        public Task<Result<IEnumerable<ScheduledSession>>> GetScheduledSessions(int taskId) =>
            _scheduleMethodRunner.ExecuteForUser(x => x.GetScheduledSessions(taskId))
                .OnSuccess(x => _logger.Info("Successfully retrieved sessions for task {0}", taskId))
                .OnError((TaskNotFoundError _) => _logger.Debug("Requested task {0} could not be found", taskId))
                .OnError((DeletedScheduleError _) => _logger.Debug("Attempted to retrieve scheduled sessions from deleted schedule"))
                .OnError((PermissionError _) => _logger.Info("Attempted to retrieve scheduled sessions without permission"));
        public Task<Result<IEnumerable<ScheduledTask>>> GetScheduledTasks(int scheduleId) =>
            _scheduleMethodRunner.ExecuteForUser(x => x.GetScheduledTasks(scheduleId))
                .OnSuccess(x => _logger.Info("Successfully retrieved tasks for schedule {0}", scheduleId))
                .OnError((ScheduleNotFoundError _) => _logger.Debug("Requested schedule {0} could not be found", scheduleId))
                .OnError((DeletedScheduleError _) => _logger.Debug("Requested schedule {0} is deleted", scheduleId))
                .OnError((PermissionError _) => _logger.Debug("Attempted to retrieve scheduled tasks without permission"));
    }
}
