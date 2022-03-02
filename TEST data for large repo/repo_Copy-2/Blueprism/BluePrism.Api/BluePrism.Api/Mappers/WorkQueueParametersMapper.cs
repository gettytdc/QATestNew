namespace BluePrism.Api.Mappers
{
    using System;
    using FilterMappers;
    using Func;
    using Models;
    using static WorkQueueSortByMapper;

    public static class WorkQueueParametersMapper
    {
        private const int DefaultItemsPerPage = 10;

        public static Domain.WorkQueueParameters ToDomainObject(this WorkQueueParameters workQueueParameters) =>
            new Domain.WorkQueueParameters
            {
                ItemsPerPage = workQueueParameters.ItemsPerPage ?? DefaultItemsPerPage,
                SortBy = workQueueParameters.SortBy.ToDomainSortBy(),
                NameFilter = workQueueParameters.Name.ToDomain(),
                QueueStatusFilter = workQueueParameters.Status.ToDomain(v => (Domain.QueueStatus)v),
                KeyFieldFilter = workQueueParameters.KeyField.ToDomain(),
                MaxAttemptsFilter = workQueueParameters.MaxAttempts.ToDomain(v => v ?? default(int)),
                AverageWorkTimeFilter = workQueueParameters.AverageWorkTime.ToDomain(v => v ?? default(int)),
                CompletedItemCountFilter = workQueueParameters.CompletedItemCount.ToDomain(v => v ?? default(int)),
                ExceptionedItemCountFilter = workQueueParameters.ExceptionedItemCount.ToDomain(v => v ?? default(int)),
                LockedItemCountFilter = workQueueParameters.LockedItemCount.ToDomain(v => v ?? default(int)),
                TotalCaseDurationFilter = workQueueParameters.TotalCaseDuration.ToDomain(v => v ?? default(int)),
                TotalItemCountFilter = workQueueParameters.TotalItemCount.ToDomain(v => v ?? default(int)),
                PendingItemCountFilter = workQueueParameters.PendingItemCount.ToDomain(v => v ?? default(int)),
                PagingToken  = workQueueParameters.PagingToken.ToDomainPagingToken()
            };

        private static Domain.WorkQueueSortByProperty ToDomainSortBy(this WorkQueueSortBy? @this) =>
            GetWorkQueueSortByModelName(@this) is Some<Domain.WorkQueueSortByProperty> s
                ? s.Value
                : throw new ArgumentException("Unexpected sort by", nameof(WorkQueueParameters.SortBy));
    }
}
