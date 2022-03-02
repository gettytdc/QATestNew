namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Domain.PagingTokens;
    using Func;
    using Server.Domain.Models.Pagination;

    public static class WorkQueuesItemsPageMapper
    {
        public static ItemsPage<WorkQueue> ToItemsPage(this IReadOnlyCollection<WorkQueue> workQueues, WorkQueueParameters workQueueParameters) =>
            new ItemsPage<WorkQueue>()
            {
                Items = workQueues,
                PagingToken = GetPagingToken(workQueues, workQueueParameters)
            };

        private static Option<string> GetPagingToken(IReadOnlyCollection<WorkQueue> items, WorkQueueParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
                return OptionHelper.None<string>();

            var lastItem = items.Last();
            var lastItemSortValue = GetPropertyValue(lastItem, parameters.SortBy);

            var pagingToken = new PagingToken<int>
            {
                PreviousIdValue = lastItem.Ident,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemSortValue),
                DataType = lastItemSortValue.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };
            return OptionHelper.Some(pagingToken.ToString());
        }

        private static object GetPropertyValue(WorkQueue item, WorkQueueSortByProperty index)
        {
            switch (index)
            {
                case WorkQueueSortByProperty.NameAsc:
                case WorkQueueSortByProperty.NameDesc:
                    return item.Name;
                case WorkQueueSortByProperty.RunningAsc:
                case WorkQueueSortByProperty.RunningDesc:
                    return item.Status;
                case WorkQueueSortByProperty.KeyFieldAsc:
                case WorkQueueSortByProperty.KeyFieldDesc:
                    return item.KeyField;
                case WorkQueueSortByProperty.MaxAttemptsAsc:
                case WorkQueueSortByProperty.MaxAttemptsDesc:
                    return item.MaxAttempts;
                case WorkQueueSortByProperty.EncryptIdAsc:
                case WorkQueueSortByProperty.EncryptIdDesc:
                    return item.EncryptionKeyId;
                case WorkQueueSortByProperty.TotalAsc:
                case WorkQueueSortByProperty.TotalDesc:
                    return item.TotalItemCount;
                case WorkQueueSortByProperty.CompletedAsc:
                case WorkQueueSortByProperty.CompletedDesc:
                    return item.CompletedItemCount;
                case WorkQueueSortByProperty.PendingAsc:
                case WorkQueueSortByProperty.PendingDesc:
                    return item.PendingItemCount;
                case WorkQueueSortByProperty.ExceptionedAsc:
                case WorkQueueSortByProperty.ExceptionedDesc:
                    return item.ExceptionedItemCount;
                case WorkQueueSortByProperty.TotalWorkTimeAsc:
                case WorkQueueSortByProperty.TotalWorkTimeDesc:
                    return item.TotalCaseDuration;
                case WorkQueueSortByProperty.AverageWorkedTimeAsc:
                case WorkQueueSortByProperty.AverageWorkedTimeDesc:
                    return item.AverageWorkTime;
                case WorkQueueSortByProperty.LockedAsc:
                case WorkQueueSortByProperty.LockedDesc:
                    return item.LockedItemCount;
                default:
                    throw new ArgumentException($"Unknown {nameof(WorkQueueSortByProperty)} index");
            }
        }

    }
}
