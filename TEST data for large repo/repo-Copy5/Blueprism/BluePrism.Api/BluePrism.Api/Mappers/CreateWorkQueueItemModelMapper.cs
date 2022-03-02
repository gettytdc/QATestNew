namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Models;

    public static class CreateWorkQueueItemModelMapper
    {
        public static IEnumerable<CreateWorkQueueItem> ToDomainModel(this IEnumerable<CreateWorkQueueItemModel> @this) =>
            @this.Select(ToDomainModel);

        public static CreateWorkQueueItem ToDomainModel(this CreateWorkQueueItemModel workQueueItem) =>
            new CreateWorkQueueItem 
            {
                Data = workQueueItem.Data.ToDomainModel(),
                DeferredDate = workQueueItem.DeferredDate,
                Tags = workQueueItem.Tags == null ? new string[] { } : workQueueItem.Tags.ToArray(),
                Priority = workQueueItem.Priority,
                Status = workQueueItem.Status
            };
    }
}
