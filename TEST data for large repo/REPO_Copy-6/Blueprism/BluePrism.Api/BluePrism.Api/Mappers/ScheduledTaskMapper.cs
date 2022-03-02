namespace BluePrism.Api.Mappers
{
    using Domain;
    using Models;

    public static class ScheduledTaskMapper
    {
        public static ScheduledTaskModel ToModelObject(this ScheduledTask scheduledTask) =>
            new ScheduledTaskModel
            {
                Id = scheduledTask.Id,
                Name = scheduledTask.Name,
                Description = scheduledTask.Description,
                DelayAfterEnd = scheduledTask.DelayAfterEnd,
                FailFastOnError = scheduledTask.FailFastOnError,
                OnSuccessTaskId = scheduledTask.OnSuccessTaskId,
                OnSuccessTaskName = scheduledTask.OnSuccessTaskName,
                OnFailureTaskId = scheduledTask.OnFailureTaskId,
                OnFailureTaskName = scheduledTask.OnFailureTaskName
            };
    }
}
