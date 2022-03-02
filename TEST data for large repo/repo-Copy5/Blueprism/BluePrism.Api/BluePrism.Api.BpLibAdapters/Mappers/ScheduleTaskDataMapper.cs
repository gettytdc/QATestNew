namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using Domain;

    public static class ScheduleTaskDataMapper
    {
        public static ScheduledTask ToDomainModel(this BluePrism.Server.Domain.Models.ScheduledTask scheduleTask) =>
            new ScheduledTask
            {
                Id = scheduleTask.Id,
                Name = scheduleTask.Name,
                Description = scheduleTask.Description,
                DelayAfterEnd = scheduleTask.DelayAfterEnd,
                FailFastOnError = scheduleTask.FailFastOnError,
                OnSuccessTaskId = scheduleTask.OnSuccessTaskId,
                OnSuccessTaskName = scheduleTask.OnSuccessTaskName,
                OnFailureTaskId = scheduleTask.OnFailureTaskId,
                OnFailureTaskName = scheduleTask.OnFailureTaskName
            };
    }
}
