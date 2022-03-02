namespace BluePrism.Api.BpLibAdapters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Domain;
    using Domain.Errors;
    using Func;
    using Mappers;
    using Server.Domain.Models;
    using static ServerResultTask;
    using ScheduledTask = Domain.ScheduledTask;
    using ScheduleParameters = Domain.ScheduleParameters;

    public class ScheduleServerAdapter : IScheduleServerAdapter
    {
        private readonly IServer _server;

        public ScheduleServerAdapter(IServer server) =>
            _server = server;

        public Task<Result<ItemsPage<Schedule>>> GetSchedules(ScheduleParameters scheduleParameters) =>
            RunOnServer(() => _server.SchedulerGetScheduleSummaries(scheduleParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()).ToArray()
                    .Map(x => x.ToItemsPage(scheduleParameters)))
                .Catch<PermissionException>(ex => ResultHelper<ItemsPage<Schedule>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<Schedule>> GetSchedule(int scheduleId) =>
            RunOnServer(() => _server.SchedulerGetScheduleSummary(scheduleId)
                    .Map(x => x.ToDomainObject()))
                .Catch<NoSuchScheduleException>(ex => ResultHelper<Schedule>.Fail(new ScheduleNotFoundError()))
                .Catch<PermissionException>(ex => ResultHelper<Schedule>.Fail(new PermissionError(ex.Message)));

        public Task<Result<ItemsPage<Domain.ScheduleLog>>> GetScheduleLogs(Domain.ScheduleLogParameters scheduleLogParameters) =>
            RunOnServer(() => _server.SchedulerGetCurrentAndPassedLogs(scheduleLogParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()).ToArray()
                    .Map(x => x.ToItemsPage(scheduleLogParameters)))
                .Catch<NoSuchScheduleException>(ex => ResultHelper<ItemsPage<Domain.ScheduleLog>>.Fail(new ScheduleNotFoundError()))
                .Catch<PermissionException>(ex => ResultHelper<ItemsPage<Domain.ScheduleLog>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<IEnumerable<Domain.ScheduledSession>>> GetScheduledSessions(int taskId) =>
            RunOnServer(() => _server.SchedulerGetSessionsWithinTask(taskId).Select(x => x.ToDomainModel()))
                .Catch<NoSuchTaskException>(ex => ResultHelper<IEnumerable<Domain.ScheduledSession>>.Fail(new TaskNotFoundError()))
                .Catch<DeletedScheduleException>(ex => ResultHelper<IEnumerable<Domain.ScheduledSession>>.Fail(new DeletedScheduleError()))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<Domain.ScheduledSession>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<IEnumerable<ScheduledTask>>> GetScheduledTasks(int scheduleId) =>
            RunOnServer(() =>
                _server.SchedulerGetScheduledTasks(scheduleId).Select(x => x.ToDomainModel()))
                .Catch<NoSuchScheduleException>(ex => ResultHelper<IEnumerable<ScheduledTask>>.Fail(new ScheduleNotFoundError()))
                .Catch<DeletedScheduleException>(ex => ResultHelper<IEnumerable<ScheduledTask>>.Fail(new DeletedScheduleError()))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<ScheduledTask>>.Fail(new PermissionError(ex.Message)));
    }
}
