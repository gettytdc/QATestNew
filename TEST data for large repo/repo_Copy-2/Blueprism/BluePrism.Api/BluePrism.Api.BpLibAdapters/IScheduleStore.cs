namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IScheduleStore : IServerAdapter
    {
        Task<Result<DateTime>> ScheduleOneOffScheduleRun(int scheduleId, DateTime utcRunTime);
        Task<Result> ModifySchedule(int scheduleId, Schedule scheduleChanges);
    }
}
