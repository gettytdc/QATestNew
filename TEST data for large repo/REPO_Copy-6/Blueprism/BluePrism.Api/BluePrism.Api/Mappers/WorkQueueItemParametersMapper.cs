namespace BluePrism.Api.Mappers
{
    using System;
    using FilterMappers;
    using Func;
    using Models;
    using static WorkQueueItemsSortByMapper;
    public static class WorkQueueItemParametersMapper
    {
        private const int DefaultItemsPerPage = 10;

        public static Domain.WorkQueueItemParameters ToDomainObject(this WorkQueueItemParameters workQueueItemParameters) =>
            new Domain.WorkQueueItemParameters
            {
                SortBy = workQueueItemParameters.SortBy.ToDomainSortBy(),
                KeyValue = workQueueItemParameters.KeyValue.ToDomain(),
                Status = workQueueItemParameters.Status.ToDomain(),
                ExceptionReason = workQueueItemParameters.ExceptionReason.ToDomain(),
                WorkTime = workQueueItemParameters.TotalWorkTime.ToDomain(v => v ?? default(int)),
                Attempt = workQueueItemParameters.Attempt.ToDomain(v => v ?? default(int)),
                Priority = workQueueItemParameters.Priority.ToDomain(v => v ?? default(int)),
                CompletedDate = workQueueItemParameters.CompletedDate.ToDomain(x => x ?? DateTimeOffset.MinValue),
                DeferredDate = workQueueItemParameters.DeferredDate.ToDomain(x => x ?? DateTimeOffset.MinValue),
                ExceptionedDate = workQueueItemParameters.ExceptionedDate.ToDomain(x => x ?? DateTimeOffset.MinValue),
                LastUpdated = workQueueItemParameters.LastUpdated.ToDomain(x => x ?? DateTimeOffset.MinValue),
                LoadedDate = workQueueItemParameters.LoadedDate.ToDomain(x => x ?? DateTimeOffset.MinValue),
                LockedDate = workQueueItemParameters.LockedDate.ToDomain(x => x ?? DateTimeOffset.MinValue),
                ItemsPerPage = workQueueItemParameters.ItemsPerPage ?? DefaultItemsPerPage,
                PagingToken = workQueueItemParameters.PagingToken.ToDomainPagingToken(),
            };

        private static Domain.WorkQueueItemSortByProperty ToDomainSortBy(this WorkQueueItemSortBy? @this)
        {
            switch (GetWorkQueueItemsSortByModelName(@this))
            {
                case Some<Domain.WorkQueueItemSortByProperty> x:
                    return x.Value;
                default:
                    throw new ArgumentException("Unexpected sort by", nameof(WorkQueueItemParameters.SortBy));
            }
        }
    }
}
