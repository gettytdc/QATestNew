namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using FilterMappers;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;

    public static class WorkQueueItemParametersMapper
    {
        public static WorkQueueItemParameters ToBluePrismObject(this Domain.WorkQueueItemParameters workQueueItemParameters) =>
            new WorkQueueItemParameters
            {
                SortBy = (WorkQueueItemSortByProperty)workQueueItemParameters.SortBy,
                KeyValue = workQueueItemParameters.KeyValue.ToBluePrismObject(),
                Status = workQueueItemParameters.Status.ToBluePrismObject(),
                ExceptionReason = workQueueItemParameters.ExceptionReason.ToBluePrismObject(),
                Attempt = workQueueItemParameters.Attempt.ToBluePrismObject(),
                Priority = workQueueItemParameters.Priority.ToBluePrismObject(),
                WorkTime = workQueueItemParameters.WorkTime.ToBluePrismObject(),
                LastUpdated = workQueueItemParameters.LastUpdated.ToBluePrismObject(),
                LoadedDate = workQueueItemParameters.LoadedDate.ToBluePrismObject(),
                LockedDate = workQueueItemParameters.LockedDate.ToBluePrismObject(),
                CompletedDate = workQueueItemParameters.CompletedDate.ToBluePrismObject(),
                DeferredDate = workQueueItemParameters.DeferredDate.ToBluePrismObject(),
                ExceptionedDate = workQueueItemParameters.ExceptionedDate.ToBluePrismObject(),
                ItemsPerPage = workQueueItemParameters.ItemsPerPage,
                PagingToken = PagingTokenMapper<WorkQueueItemPagingToken, long>.ToBluePrismPagingToken(workQueueItemParameters.PagingToken)
            };
    }
}
