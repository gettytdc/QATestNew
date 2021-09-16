namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using Func;

    public static class WorkQueueItemsSortByMapper
    {
        private static readonly IDictionary<Models.WorkQueueItemSortBy, Domain.WorkQueueItemSortByProperty> WorkQueueItemsSortByMappings =
            new Dictionary<Models.WorkQueueItemSortBy, Domain.WorkQueueItemSortByProperty>
            {
                {Models.WorkQueueItemSortBy.AttemptAsc, Domain.WorkQueueItemSortByProperty.AttemptAsc},
                {Models.WorkQueueItemSortBy.AttemptDesc, Domain.WorkQueueItemSortByProperty.AttemptDesc},
                {Models.WorkQueueItemSortBy.AttemptWorkTimeAsc, Domain.WorkQueueItemSortByProperty.AttemptWorkTimeAsc},
                {Models.WorkQueueItemSortBy.AttemptWorkTimeDesc, Domain.WorkQueueItemSortByProperty.AttemptWorkTimeDesc},
                {Models.WorkQueueItemSortBy.CompletedAsc, Domain.WorkQueueItemSortByProperty.CompletedAsc},
                {Models.WorkQueueItemSortBy.CompletedDesc, Domain.WorkQueueItemSortByProperty.CompletedDesc},
                {Models.WorkQueueItemSortBy.DeferredAsc, Domain.WorkQueueItemSortByProperty.DeferredAsc},
                {Models.WorkQueueItemSortBy.DeferredDesc, Domain.WorkQueueItemSortByProperty.DeferredDesc},
                {Models.WorkQueueItemSortBy.ExceptionAsc, Domain.WorkQueueItemSortByProperty.ExceptionAsc},
                {Models.WorkQueueItemSortBy.ExceptionDesc, Domain.WorkQueueItemSortByProperty.ExceptionDesc},
                {Models.WorkQueueItemSortBy.ExceptionReasonAsc, Domain.WorkQueueItemSortByProperty.ExceptionReasonAsc},
                {Models.WorkQueueItemSortBy.ExceptionReasonDesc, Domain.WorkQueueItemSortByProperty.ExceptionReasonDesc},
                {Models.WorkQueueItemSortBy.KeyValueAsc, Domain.WorkQueueItemSortByProperty.KeyValueAsc},
                {Models.WorkQueueItemSortBy.KeyValueDesc, Domain.WorkQueueItemSortByProperty.KeyValueDesc},
                {Models.WorkQueueItemSortBy.LastUpdatedAsc, Domain.WorkQueueItemSortByProperty.LastUpdatedAsc},
                {Models.WorkQueueItemSortBy.LastUpdatedDesc, Domain.WorkQueueItemSortByProperty.LastUpdatedDesc},
                {Models.WorkQueueItemSortBy.LoadedAsc, Domain.WorkQueueItemSortByProperty.LoadedAsc},
                {Models.WorkQueueItemSortBy.LoadedDesc, Domain.WorkQueueItemSortByProperty.LoadedDesc},
                {Models.WorkQueueItemSortBy.LockedAsc, Domain.WorkQueueItemSortByProperty.LockedAsc},
                {Models.WorkQueueItemSortBy.LockedDesc, Domain.WorkQueueItemSortByProperty.LockedDesc},
                {Models.WorkQueueItemSortBy.PriorityAsc, Domain.WorkQueueItemSortByProperty.PriorityAsc},
                {Models.WorkQueueItemSortBy.PriorityDesc, Domain.WorkQueueItemSortByProperty.PriorityDesc},
                {Models.WorkQueueItemSortBy.StatusAsc, Domain.WorkQueueItemSortByProperty.StatusAsc},
                {Models.WorkQueueItemSortBy.StatusDesc, Domain.WorkQueueItemSortByProperty.StatusDesc},
                {Models.WorkQueueItemSortBy.WorkTimeAsc, Domain.WorkQueueItemSortByProperty.WorkTimeAsc},
                {Models.WorkQueueItemSortBy.WorkTimeDesc, Domain.WorkQueueItemSortByProperty.WorkTimeDesc},
            };
        public static Option<Domain.WorkQueueItemSortByProperty> GetWorkQueueItemsSortByModelName(Models.WorkQueueItemSortBy? value)
        {
            var sortBy = value ?? Models.WorkQueueItemSortBy.LastUpdatedDesc;

            return WorkQueueItemsSortByMappings.TryGetValue(sortBy, out var workQueueItemSortByProperty)
                ? OptionHelper.Some(workQueueItemSortByProperty)
                : OptionHelper.None<Domain.WorkQueueItemSortByProperty>();
        }
    }
}
