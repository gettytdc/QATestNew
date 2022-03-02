namespace BluePrism.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using Func;
    using Logging;
    using static Func.ResultHelper;

    public class WorkQueuesService : IWorkQueuesService
    {
        private readonly IAdapterAuthenticatedMethodRunner<IWorkQueueServerAdapter> _workQueueMethodRunner;
        private readonly ILogger<WorkQueuesService> _logger;

        public WorkQueuesService(IAdapterAuthenticatedMethodRunner<IWorkQueueServerAdapter> workQueueMethodRunner, ILogger<WorkQueuesService> logger)
        {
            _workQueueMethodRunner = workQueueMethodRunner;
            _logger = logger;
        }

        public Task<Result<ItemsPage<WorkQueue>>> GetWorkQueues(WorkQueueParameters workQueueParameters) =>
            _workQueueMethodRunner
            .ExecuteForUser(s => s.WorkQueueGetQueues(workQueueParameters));

        public Task<Result<Guid>> CreateWorkQueue(WorkQueue workQueue) =>
            EnsureQueueDoesNotExist(workQueue.Name)
            .Then(() => _workQueueMethodRunner.ExecuteForUser(s => s.CreateWorkQueue(workQueue)))
            .Then(x => Succeed(x.Id))
            .OnSuccess(id => _logger.Debug("Successfully created work queue with name '{0}' and ID {1}", workQueue.Name, id));

        public Task<Result> DeleteWorkQueue(Guid workQueueId) =>
            _workQueueMethodRunner
            .ExecuteForUser(s => s.DeleteWorkQueue(workQueueId))
            .OnSuccess(() => _logger.Debug("Successfully deleted queue with ID {0}", workQueueId))
            .OnError<QueueNotFoundError>(_ => _logger.Warn("Attempted to delete queue with ID {0}, but no matching queue was found", workQueueId))
            .OnError<QueueNotEmptyError>(_ => _logger.Warn("Attempted to delete queue with ID {0}, but the queue still contained items", workQueueId))
            .OnError<QueueStillContainsSessionsError>(_ => _logger.Warn("Attempted to delete queue with ID {0}, but the queue was still linked to sessions", workQueueId));

        public Task<Result<WorkQueueItem>> GetWorkQueueItem(Guid workQueueItemId) =>
            _workQueueMethodRunner
            .ExecuteForUser(s => s.WorkQueueGetItem(workQueueItemId))
            .OnSuccess(() => _logger.Debug("Successfully retrieved work queue item with ID {0}", workQueueItemId));

        public Task<Result> UpdateWorkQueue(Guid workQueueId, Func<WorkQueue, Result<WorkQueue>> applyWorkQueueUpdates) =>
            _workQueueMethodRunner.ExecuteForUser(s => s.WorkQueueGetQueueById(workQueueId))
                .Then(applyWorkQueueUpdates)
                .Then(queue => _workQueueMethodRunner.ExecuteForUser(s => s.UpdateWorkQueue(queue)))
                .OnSuccess(() => _logger.Debug("Successfully updated work queue '{0}'", workQueueId))
                .OnError<QueueNotFoundError>(_ => _logger.Warn("Attempt to update queue with ID '{0}', but no matching queue was found", workQueueId))
                .OnError<InvalidArgumentsError>(_ => _logger.Warn("Attempt to update queue with ID '{0}', but invalid arguments supplied", workQueueId))
                .OnError<NotOnlyOneQueueUpdatedError>(_ => _logger.Warn("Attempt to update queue with ID '{0}', but either none or more than one queue was updated", workQueueId))
                .OnError<QueueAlreadyExistsError>(_ => _logger.Warn("Attempt to update queue with ID '{0}', but a queue with the same name already exists", workQueueId));

        public Task<Result<ItemsPage<WorkQueueItemNoDataXml>>> WorkQueueGetQueueItems(Guid workQueueId, WorkQueueItemParameters workQueueItemParameters) =>
            _workQueueMethodRunner.ExecuteForUser(s => s.WorkQueueGetQueueItems(workQueueId, workQueueItemParameters))
                .OnSuccess(() => _logger.Debug("Successfully retrieved work queue items for queue with ID {0}", workQueueId))
                .OnError<QueueNotFoundError, ItemsPage<WorkQueueItemNoDataXml>>(_ => _logger.Warn("Attempt to retrieve items for work queue with ID '{0}', but no matching queue was found", workQueueId));

        public Task<Result<WorkQueue>> GetWorkQueue(Guid workQueueId) =>
            _workQueueMethodRunner.ExecuteForUser(s => s.WorkQueueGetQueueById(workQueueId))
                .OnSuccess(() => _logger.Debug("Successfully retrieved work queue with ID {0}", workQueueId))
                .OnError<QueueNotFoundError, WorkQueue>(_ => _logger.Debug("Attempt to retrieve queue with ID '{0}', but no matching queue was found", workQueueId));

        private Task<Result> EnsureQueueDoesNotExist(string queueName) =>
            _workQueueMethodRunner
                .ExecuteForUser(s => s.WorkQueueGetQueueNames())
                .Then(x => x.Any(y => y == queueName) ? Fail<QueueAlreadyExistsError>() : Succeed());

        public Task<Result<IEnumerable<Guid>>> CreateWorkQueueItems(Guid workQueueId, IEnumerable<CreateWorkQueueItem> workQueueItems) =>
            GetWorkQueue(workQueueId)
                .Then(queue => _workQueueMethodRunner.ExecuteForUser(s => s.WorkQueueAddItemsAPI(queue.Name, workQueueItems)))
                .Then(x => Succeed(x))
                .OnError<QueueNotFoundError,IEnumerable<Guid>>(_ => _logger.Warn("Attempted to create work queue items for queue with ID '{0}', but no matching queue was found", workQueueId))
                .OnError<InvalidArgumentsError, IEnumerable<Guid>>(_ => _logger.Warn("Attempted to create work queue items for queue with ID '{0}', but invalid arguments received", workQueueId))
                .OnSuccess(_ => _logger.Debug("Successfully created work queue items for queue with ID {0}", workQueueId));

    }
}
