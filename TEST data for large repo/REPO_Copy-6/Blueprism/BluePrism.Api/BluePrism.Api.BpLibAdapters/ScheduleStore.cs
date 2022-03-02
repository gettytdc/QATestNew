namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Server.Domain.Models;
    using Domain;
    using Domain.Errors;
    using Func;

    using static ServerResultTask;
    using static Func.ResultHelper;

    // This class differs from others in this project as it isn't just a wrapper. This is because exposing scheduler at this stage
    // is difficult so some logic needs to go here to keep Schedule objects within the Blue Prism context.
    // However, logic should be kept to an absolute minimum and put in the domain services layer wherever possible.
    public class ScheduleStore : IScheduleStore
    {
        private readonly IServer _server;
        private readonly Scheduling.IScheduleStore _scheduleStore;

        public ScheduleStore(IServer server, Func<IServer, Scheduling.IScheduleStore> scheduleStoreFactory)
        {
            _server = server;
            _scheduleStore = scheduleStoreFactory(server);
        }

        public Task<Result<DateTime>> ScheduleOneOffScheduleRun(int scheduleId, DateTime utcRunTime) =>
            GetSchedule(scheduleId)
                .Then(schedule => AddOneTimeTrigger(schedule, utcRunTime));

        public Task<Result> ModifySchedule(int scheduleId, Schedule scheduleChanges) =>
            GetSessionRunnerSchedule(scheduleId)
                .Then(scheduleChanges.IsRetired ? RetireSchedule() : UnretireSchedule());

        private Task<Result<Scheduling.ISchedule>> GetSchedule(int scheduleId) =>
            RunOnServer(() => _server.SchedulerGetSchedule(scheduleId))
                .Execute()
                .Then(x => string.IsNullOrEmpty(x.Name)
                    ? ResultHelper<Scheduling.ISchedule>.Fail<ScheduleNotFoundError>()
                    : Succeed(x));

        private Task<Result<DateTime>> AddOneTimeTrigger(Scheduling.ISchedule schedule, DateTime utcRunTime) =>
            RunOnServer(() => _scheduleStore.TriggerSchedule(schedule, utcRunTime));

        private Task<Result<SessionRunnerSchedule>> GetSessionRunnerSchedule(int scheduleId) =>
            GetSchedule(scheduleId)
                .ThenMap(x => (SessionRunnerSchedule)x);

        private Func<SessionRunnerSchedule, Task<Result>> RetireSchedule() =>
            originalSchedule =>
                !originalSchedule.Retired
                    ? RetireSchedule(originalSchedule)
                    : Fail<ScheduleAlreadyRetiredError>().ToTask();

        private Task<Result> RetireSchedule(Scheduling.ISchedule schedule) =>
            RunOnServer(() => _scheduleStore.RetireSchedule(schedule))
                .Catch<PermissionException>(ex => Fail(new PermissionError(ex.Message)));

        private Func<SessionRunnerSchedule, Task<Result>> UnretireSchedule() =>
            originalSchedule =>
                originalSchedule.Retired
                    ? UnretireSchedule(originalSchedule)
                    : Fail<ScheduleNotRetiredError>().ToTask();

        private Task<Result> UnretireSchedule(Scheduling.ISchedule schedule) =>
            RunOnServer(() => _scheduleStore.UnretireSchedule(schedule))
                .Catch<PermissionException>(ex => Fail(new PermissionError(ex.Message)));
    }
}
