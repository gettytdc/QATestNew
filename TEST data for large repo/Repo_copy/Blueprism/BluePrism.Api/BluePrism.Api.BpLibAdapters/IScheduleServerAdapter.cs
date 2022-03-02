namespace BluePrism.Api.BpLibAdapters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IScheduleServerAdapter : IServerAdapter
    {
        Task<Result<ItemsPage<ScheduleLog>>> GetScheduleLogs(ScheduleLogParameters scheduleLogParameters);
        Task<Result<ItemsPage<Schedule>>> GetSchedules(ScheduleParameters scheduleParameters);
        Task<Result<Schedule>> GetSchedule(int scheduleId);
        Task<Result<IEnumerable<ScheduledSession>>> GetScheduledSessions(int taskId);
        Task<Result<IEnumerable<ScheduledTask>>> GetScheduledTasks(int scheduleId);
    }
}
