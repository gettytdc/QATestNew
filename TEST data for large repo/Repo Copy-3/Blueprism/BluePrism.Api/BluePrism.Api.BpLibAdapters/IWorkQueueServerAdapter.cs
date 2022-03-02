namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IWorkQueueServerAdapter : IServerAdapter
    {
        Task<Result<IEnumerable<string>>> WorkQueueGetQueueNames();
        Task<Result<ItemsPage<WorkQueue>>> WorkQueueGetQueues(WorkQueueParameters workQueueParameters);
        Task<Result<WorkQueue>> WorkQueueGetQueueById(Guid workQueueId);
        Task<Result<WorkQueue>> CreateWorkQueue(WorkQueue workQueue);
        Task<Result> DeleteWorkQueue(Guid workQueueId);
        Task<Result<WorkQueueItem>> WorkQueueGetItem(Guid itemId);
        Task<Result> UpdateWorkQueue(WorkQueue workQueue);
        Task<Result<ItemsPage<WorkQueueItemNoDataXml>>> WorkQueueGetQueueItems(Guid workQueueId, WorkQueueItemParameters workQueueItemParameters);
        Task<Result<IEnumerable<Guid>>> WorkQueueAddItemsAPI(string queueName, IEnumerable<CreateWorkQueueItem> workQueueItems);
    }
}
