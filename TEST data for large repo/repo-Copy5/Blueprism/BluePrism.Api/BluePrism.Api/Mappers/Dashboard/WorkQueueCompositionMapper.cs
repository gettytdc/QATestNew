namespace BluePrism.Api.Mappers.Dashboard
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Dashboard;
    using Models.Dashboard;

    public static class WorkQueueCompositionMapper
    {
        public static IEnumerable<WorkQueueCompositionModel> ToModel(this IEnumerable<WorkQueueComposition> @this) =>
            @this.Select(x => x.ToModel());

        public static WorkQueueCompositionModel ToModel(this WorkQueueComposition @this) =>
            new WorkQueueCompositionModel
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
