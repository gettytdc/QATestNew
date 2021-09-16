namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    public static class WorkQueueMapper
    {
        public static Domain.WorkQueue ToDomainObject(this Models.CreateWorkQueueRequestModel workQueue) =>
            new Domain.WorkQueue
            {
                Name = workQueue.Name,
                KeyField = workQueue.KeyField,
                MaxAttempts = workQueue.MaxAttempts,
                Status = (Domain.QueueStatus) workQueue.Status,
                EncryptionKeyId = workQueue.EncryptionKeyId,
            };

        public static WorkQueueModel ToModel(this Domain.WorkQueue workQueue) =>
            new WorkQueueModel
            {
                Id = workQueue.Id,
                Name = workQueue.Name,
                Status = (QueueStatus) workQueue.Status,
                IsEncrypted = workQueue.IsEncrypted,
                KeyField = workQueue.KeyField,
                MaxAttempts = workQueue.MaxAttempts,
                PendingItemCount = workQueue.PendingItemCount,
                CompletedItemCount = workQueue.CompletedItemCount,
                LockedItemCount = workQueue.LockedItemCount,
                ExceptionedItemCount = workQueue.ExceptionedItemCount,
                TotalItemCount = workQueue.TotalItemCount,
                AverageWorkTime = workQueue.AverageWorkTime,
                TotalCaseDuration = workQueue.TotalCaseDuration,
                GroupName = workQueue.GroupName,
                GroupId = workQueue.GroupId,
            };

        public static IEnumerable<WorkQueueModel> ToModel(this IEnumerable<Domain.WorkQueue> @this) =>
            @this.Select(x => x.ToModel());
    }
}
