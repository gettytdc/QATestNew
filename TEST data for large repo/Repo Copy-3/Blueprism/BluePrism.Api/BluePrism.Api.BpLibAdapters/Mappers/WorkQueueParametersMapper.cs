namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using FilterMappers;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;

    public static class WorkQueueParametersMapper
    {
        public static WorkQueueParameters ToBluePrismObject(this Domain.WorkQueueParameters workQueueParameters) =>
            new WorkQueueParameters
            {
                SortBy = (WorkQueueSortByProperty)workQueueParameters.SortBy,
                ItemsPerPage = workQueueParameters.ItemsPerPage,
                NameFilter = workQueueParameters.NameFilter.ToBluePrismObject(),
                QueueStatusFilter = workQueueParameters.QueueStatusFilter.ToBluePrismObject(x => (QueueStatus)x),
                KeyFieldFilter = workQueueParameters.KeyFieldFilter.ToBluePrismObject(),
                MaxAttemptsFilter = workQueueParameters.MaxAttemptsFilter.ToBluePrismObject(),
                AverageItemWorkTimeFilter = workQueueParameters.AverageWorkTimeFilter.ToBluePrismObject(),
                CompletedItemsCountFilter = workQueueParameters.CompletedItemCountFilter.ToBluePrismObject(),
                ExceptionedItemsCountFilter = workQueueParameters.ExceptionedItemCountFilter.ToBluePrismObject(),
                LockedItemsCountFilter = workQueueParameters.LockedItemCountFilter.ToBluePrismObject(),
                TotalCaseDurationFilter = workQueueParameters.TotalCaseDurationFilter.ToBluePrismObject(),
                TotalItemCountFilter = workQueueParameters.TotalItemCountFilter.ToBluePrismObject(),
                PendingItemsCountFilter = workQueueParameters.PendingItemCountFilter.ToBluePrismObject(),
                PagingToken = PagingTokenMapper<WorkQueuePagingToken, int>.ToBluePrismPagingToken(workQueueParameters.PagingToken)
            };
    }
}
