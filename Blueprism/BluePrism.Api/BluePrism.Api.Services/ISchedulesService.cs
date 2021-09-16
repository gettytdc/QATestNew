namespace BluePrism.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface ISchedulesService
    {
        Task<Result<ItemsPage<Schedule>>> GetSchedules(ScheduleParameters scheduleParameters);
        Task<Result<Schedule>> GetSchedule(int scheduleId);
        Task<Result<ItemsPage<ScheduleLog>>> GetScheduleLogs(ScheduleLogParameters scheduleLogParameters);
        Task<Result<DateTimeOffset>> ScheduleOneOffScheduleRun(int scheduleId, DateTimeOffset runTime);
        Task<Result> ModifySchedule(int scheduleId, Schedule scheduleChanges);
        Task<Result<IEnumerable<ScheduledSession>>> GetScheduledSessions(int taskId);
        Task<Result<IEnumerable<ScheduledTask>>> GetScheduledTasks(int scheduleId);
    }
}
