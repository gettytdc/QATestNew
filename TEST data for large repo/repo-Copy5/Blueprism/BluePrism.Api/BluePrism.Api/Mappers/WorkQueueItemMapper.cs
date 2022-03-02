namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Func;

    public static class WorkQueueItemMapper
    {
        
        public static Models.WorkQueueItemModel ToModel(this Domain.WorkQueueItem workQueueItem) =>
            new Models.WorkQueueItemModel
            {
                Id = workQueueItem.Id,
                Ident = workQueueItem.Ident,
                Priority = workQueueItem.Priority,
                State = (Models.WorkQueueItemState)workQueueItem.State,
                KeyValue = workQueueItem.KeyValue,
                Status = workQueueItem.Status,
                Tags = workQueueItem.Tags.ToArray(),
                AttemptNumber = workQueueItem.AttemptNumber,
                LoadedDate = workQueueItem.LoadedDate is Some<DateTimeOffset> loadedDate ? loadedDate.Value : (DateTimeOffset?)null,
                DeferredDate = workQueueItem.DeferredDate is Some<DateTimeOffset> deferredDate ? deferredDate.Value : (DateTimeOffset?)null,
                LockedDate = workQueueItem.LockedDate is Some<DateTimeOffset> lockedDate ? lockedDate.Value : (DateTimeOffset?)null,
                CompletedDate = workQueueItem.CompletedDate is Some<DateTimeOffset> completedDate ? completedDate.Value : (DateTimeOffset?)null,
                WorkTimeInSeconds = workQueueItem.WorkTimeInSeconds,
                AttemptWorkTimeInSeconds = workQueueItem.AttemptWorkTimeInSeconds,
                ExceptionedDate = workQueueItem.ExceptionedDate is Some<DateTimeOffset> exceptionedDate ? exceptionedDate.Value : (DateTimeOffset?)null,
                LastUpdated = workQueueItem.LastUpdated is Some<DateTimeOffset> lastUpdated ? lastUpdated.Value : (DateTimeOffset?)null,
                ExceptionReason = workQueueItem.ExceptionReason,
                Data = workQueueItem.Data.ToModel(),
                Resource = workQueueItem.Resource,
            };

        public static IEnumerable<Models.WorkQueueItemNoDataXmlModel> ToModelNoDataXml(this IEnumerable<Domain.WorkQueueItemNoDataXml> @this) =>
            @this.Select(x => x.ToModelNoDataXml());

        
        public static Models.WorkQueueItemNoDataXmlModel ToModelNoDataXml(this Domain.WorkQueueItemNoDataXml workQueueItem) =>
            new Models.WorkQueueItemNoDataXmlModel
            {
                Id = workQueueItem.Id,
                Ident = workQueueItem.Ident,
                Priority = workQueueItem.Priority,
                State = (Models.WorkQueueItemState)workQueueItem.State,
                KeyValue = workQueueItem.KeyValue,
                Status = workQueueItem.Status,
                Tags = workQueueItem.Tags.ToArray(),
                AttemptNumber = workQueueItem.AttemptNumber,
                LoadedDate = workQueueItem.LoadedDate is Some<DateTimeOffset> loadedDate ? loadedDate.Value : (DateTimeOffset?)null,
                DeferredDate = workQueueItem.DeferredDate is Some<DateTimeOffset> deferredDate ? deferredDate.Value : (DateTimeOffset?)null,
                LockedDate = workQueueItem.LockedDate is Some<DateTimeOffset> lockedDate ? lockedDate.Value : (DateTimeOffset?)null,
                CompletedDate = workQueueItem.CompletedDate is Some<DateTimeOffset> completedDate ? completedDate.Value : (DateTimeOffset?)null,
                WorkTimeInSeconds = workQueueItem.WorkTimeInSeconds,
                AttemptWorkTimeInSeconds = workQueueItem.AttemptWorkTimeInSeconds,
                ExceptionedDate = workQueueItem.ExceptionedDate is Some<DateTimeOffset> exceptionedDate ? exceptionedDate.Value : (DateTimeOffset?)null,
                LastUpdated = workQueueItem.LastUpdated is Some<DateTimeOffset> lastUpdated ? lastUpdated.Value : (DateTimeOffset?)null,
                ExceptionReason = workQueueItem.ExceptionReason,
                Resource = workQueueItem.Resource,
            };
    }
}
