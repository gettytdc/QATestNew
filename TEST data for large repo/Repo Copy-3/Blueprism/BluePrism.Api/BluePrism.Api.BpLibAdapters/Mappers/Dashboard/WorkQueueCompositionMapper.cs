namespace BluePrism.Api.BpLibAdapters.Mappers.Dashboard
{
    using Domain.Dashboard;

    public static class WorkQueueCompositionMapper
    {
        public static WorkQueueComposition ToDomainObject(this Server.Domain.Models.Dashboard.WorkQueueComposition @this) =>
            new WorkQueueComposition
            {
                Id = @this.Id,
                Name = @this.Name,
                Completed = @this.Completed,
                Locked = @this.Locked,
                Pending = @this.Pending,
                Deferred = @this.Deferred,
                Exceptioned = @this.Exceptioned,
            };
    }
}
