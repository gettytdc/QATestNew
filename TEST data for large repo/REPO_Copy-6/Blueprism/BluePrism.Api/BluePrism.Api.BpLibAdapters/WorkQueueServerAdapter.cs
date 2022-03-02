namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BluePrism.Server.Domain.Models;
    using Domain;
    using Domain.Errors;
    using Func;
    using Mappers;
    using static ServerResultTask;
    using static Func.ResultHelper;

    public class WorkQueueServerAdapter : IWorkQueueServerAdapter
    {
        private readonly IServer _server;

        public WorkQueueServerAdapter(IServer server)
        {
            _server = server;
        }

        public Task<Result<IEnumerable<string>>> WorkQueueGetQueueNames() =>
            RunOnServer(() => _server.WorkQueueGetAllQueues()
                .Select(x => x.Name))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<string>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<ItemsPage<WorkQueue>>> WorkQueueGetQueues(Domain.WorkQueueParameters workQueueParameters) =>
            RunOnServer(() => _server.WorkQueueGetQueues(workQueueParameters.ToBluePrismObject())
                .Select(x => x.ToDomainObject()).ToArray()
                .Map(x => x.ToItemsPage(workQueueParameters)))
                .Catch<PermissionException>(ex => ResultHelper<ItemsPage<WorkQueue>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<WorkQueue>> WorkQueueGetQueueById(Guid workQueueId) =>
            RunOnServer(() => _server.WorkQueueGetQueue(workQueueId)?.ToDomainObject())
                .Catch<PermissionException>(ex => ResultHelper<WorkQueue>.Fail(new PermissionError(ex.Message)))
                .OnNull(() => ResultHelper<WorkQueue>.Fail(new QueueNotFoundError("Could not find queue with the given ID")));

        public Task<Result<WorkQueue>> CreateWorkQueue(WorkQueue workQueue) =>
            RunOnServer(() => _server.CreateWorkQueue(workQueue.ToBluePrismObject()).ToDomainObject())
                .Catch<PermissionException>(ex => ResultHelper<WorkQueue>.Fail(new PermissionError(ex.Message)));

        public Task<Result> DeleteWorkQueue(Guid workQueueId) =>
            RunOnServer(() => _server.DeleteWorkQueue(workQueueId))
            .Catch<NoSuchQueueException>(ex => Fail(new QueueNotFoundError(ex.Message)))
            .Catch<QueueNotEmptyException>(ex => Fail(new QueueNotEmptyError(ex.Message)))
            .Catch<ForeignKeyDependencyException>(ex => Fail(new QueueStillContainsSessionsError(ex.Message)))
            .Catch<PermissionException>(ex => Fail(new PermissionError(ex.Message)));

        public Task<Result<WorkQueueItem>> WorkQueueGetItem(Guid itemId) =>
            RunOnServer(() => _server.WorkQueueGetItem(itemId)?.ToDomainObject())
                // A bug in Blue Prism means that it throws an invalid cast exception rather than returning null. This line can be removed if that is fixed.
                .Catch<PermissionException>(ex => ResultHelper<WorkQueueItem>.Fail(new PermissionError(ex.Message)))
                .Catch<InvalidCastException>(_ => ResultHelper<WorkQueueItem>.Fail<QueueItemNotFoundError>())
                .OnNull(() => ResultHelper<WorkQueueItem>.Fail<QueueItemNotFoundError>());

        public Task<Result> UpdateWorkQueue(WorkQueue workQueue) =>
            RunOnServer(() => _server.UpdateWorkQueueWithStatus(workQueue.ToBluePrismObject()))
                .Catch<ArgumentNullException>(ex => Fail(new InvalidArgumentsError(ex.Message)))
                .Catch<BluePrismException>(ex => Fail(new NotOnlyOneQueueUpdatedError(ex.Message)))
                .Catch<NameAlreadyExistsException>(_ => Fail(new QueueAlreadyExistsError()))
                .Catch<PermissionException>(ex => Fail(new PermissionError(ex.Message)));

        public Task<Result<ItemsPage<WorkQueueItemNoDataXml>>> WorkQueueGetQueueItems(Guid workQueueId, Domain.WorkQueueItemParameters workQueueItemParameters) =>
            RunOnServer(() => _server.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters.ToBluePrismObject())
                .Select(x => x.ToDomainObjectNoDataXml()).ToArray()
                .Map(x => x.ToWorkQueueItemsPage(workQueueItemParameters)))
                .Catch<NoSuchQueueException>(ex => ResultHelper<ItemsPage<WorkQueueItemNoDataXml>>.Fail(new QueueNotFoundError(ex.Message)))
                .Catch<PermissionException>(ex => ResultHelper<ItemsPage<WorkQueueItemNoDataXml>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<IEnumerable<Guid>>> WorkQueueAddItemsAPI(string queueName, IEnumerable<CreateWorkQueueItem> workQueueItems) =>
            RunOnServer(() => _server.WorkQueueAddItemsAPI(queueName, workQueueItems.ToBluePrismObject()))
                .Catch<ArgumentNullException>(ex => ResultHelper<IEnumerable<Guid>>.Fail(new InvalidArgumentsError(ex.Message)))
                .Catch<InvalidOperationException>(ex => ResultHelper<IEnumerable<Guid>>.Fail(new InvalidArgumentsError(ex.Message)))
                .Catch<NoSuchQueueException>(ex => ResultHelper<IEnumerable<Guid>>.Fail(new QueueNotFoundError(ex.Message)))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<Guid>>.Fail(new PermissionError(ex.Message)));

    }
}
