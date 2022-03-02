namespace BluePrism.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IWorkQueuesService
    {
        Task<Result<ItemsPage<WorkQueue>>> GetWorkQueues(WorkQueueParameters workQueueParameters);
        Task<Result<Guid>> CreateWorkQueue(WorkQueue workQueue);
        Task<Result> DeleteWorkQueue(Guid workQueueId);
        Task<Result<WorkQueueItem>> GetWorkQueueItem(Guid workQueueItemId);
        Task<Result> UpdateWorkQueue(Guid workQueueId, Func<WorkQueue, Result<WorkQueue>> applyWorkQueueUpdates);
        Task<Result<ItemsPage<WorkQueueItemNoDataXml>>> WorkQueueGetQueueItems(Guid workQueueId, WorkQueueItemParameters workQueueItemParameters);
        Task<Result<WorkQueue>> GetWorkQueue(Guid workQueueId);
        Task<Result<IEnumerable<Guid>>> CreateWorkQueueItems(Guid workQueueId, IEnumerable<CreateWorkQueueItem> workQueueItems);
    }
}
