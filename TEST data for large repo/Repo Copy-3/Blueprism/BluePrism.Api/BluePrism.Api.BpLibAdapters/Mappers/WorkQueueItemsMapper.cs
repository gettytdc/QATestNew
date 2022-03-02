namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Domain;
    using Domain.PagingTokens;
    using Func;
    using Server.Domain.Models.Pagination;
    using static Func.OptionHelper;

    public static class WorkQueueItemsMapper
    {
        public static ItemsPage<WorkQueueItemNoDataXml> ToWorkQueueItemsPage(this IReadOnlyCollection<WorkQueueItemNoDataXml> workQueueItems, WorkQueueItemParameters parameters) =>
            new ItemsPage<WorkQueueItemNoDataXml>
            {
                Items = workQueueItems,
                PagingToken = GetPagingToken(workQueueItems, parameters),
            };

        public static string GetTypeName(this object obj) =>
            obj is Option _
                ? obj.GetType().GenericTypeArguments.Single().Name
                : obj.GetType().Name;
        
        private static Option<string> GetPagingToken(IReadOnlyCollection<WorkQueueItemNoDataXml> items, WorkQueueItemParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
                return None<string>();

            var lastItem = items.Last();
            var lastItemSortValue = GetPropertyValue(lastItem, parameters.SortBy);

            var pagingToken = new PagingToken<long>
            {
                PreviousIdValue = lastItem.Ident,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemSortValue),
                DataType = lastItemSortValue.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };
            return Some(pagingToken.ToString());
        }

        private static object GetPropertyValue(WorkQueueItemNoDataXml item, WorkQueueItemSortByProperty index)
        {
            switch (index)
            {
                case WorkQueueItemSortByProperty.AttemptAsc:
                case WorkQueueItemSortByProperty.AttemptDesc:
                    return item.AttemptNumber;
                case WorkQueueItemSortByProperty.AttemptWorkTimeAsc:
                case WorkQueueItemSortByProperty.AttemptWorkTimeDesc:
                    return item.AttemptWorkTimeInSeconds;
                case WorkQueueItemSortByProperty.CompletedAsc:
                case WorkQueueItemSortByProperty.CompletedDesc:
                    return item.CompletedDate;
                case WorkQueueItemSortByProperty.DeferredAsc:
                case WorkQueueItemSortByProperty.DeferredDesc:
                    return item.DeferredDate;
                case WorkQueueItemSortByProperty.ExceptionAsc:
                case WorkQueueItemSortByProperty.ExceptionDesc:
                    return item.ExceptionedDate;
                case WorkQueueItemSortByProperty.ExceptionReasonAsc:
                case WorkQueueItemSortByProperty.ExceptionReasonDesc:
                    return item.ExceptionReason;
                case WorkQueueItemSortByProperty.KeyValueAsc:
                case WorkQueueItemSortByProperty.KeyValueDesc:
                    return item.KeyValue;
                case WorkQueueItemSortByProperty.LastUpdatedAsc:
                case WorkQueueItemSortByProperty.LastUpdatedDesc:
                    return item.LastUpdated;
                case WorkQueueItemSortByProperty.LoadedAsc:
                case WorkQueueItemSortByProperty.LoadedDesc:
                    return item.LoadedDate;
                case WorkQueueItemSortByProperty.LockedAsc:
                case WorkQueueItemSortByProperty.LockedDesc:
                    return item.LoadedDate;
                case WorkQueueItemSortByProperty.PriorityAsc:
                case WorkQueueItemSortByProperty.PriorityDesc:
                    return item.Priority;
                case WorkQueueItemSortByProperty.StatusAsc:
                case WorkQueueItemSortByProperty.StatusDesc:
                    return item.Status;
                case WorkQueueItemSortByProperty.WorkTimeAsc:
                case WorkQueueItemSortByProperty.WorkTimeDesc:
                    return item.WorkTimeInSeconds;
                default:
                    throw new ArgumentException($"Unknown {nameof(WorkQueueItemSortByProperty)} index");
            }
        }
    }
}
