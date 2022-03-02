namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutomateAppCore.Resources;
    using Domain;
    using Domain.PagingTokens;
    using Func;
    using Server.Domain.Models.Pagination;

    public static class ResourceMapper
    {
        public static Resource ToDomainObject(this ResourceInfo @this) =>
            new Resource
            {
                Id = @this.ID,
                Name = @this.Name,
                Attributes = (ResourceAttribute)@this.Attributes,
                PoolId = @this.Pool,
                PoolName = @this.PoolName,
                GroupId = @this.GroupID,
                GroupName = @this.GroupName,
                ActiveSessionCount = @this.ActiveSessions,
                WarningSessionCount = @this.WarningSessions,
                PendingSessionCount = @this.PendingSessions,
                DatabaseStatus = (ResourceDbStatus)@this.Status,
                DisplayStatus = (ResourceDisplayStatus)@this.DisplayStatus
            };

        public static ItemsPage<Resource> ToItemsPage(this IReadOnlyList<Resource> resources, ResourceParameters resourceParameters) =>
            new ItemsPage<Resource>
            {
                Items = resources,
                PagingToken = GetPagingToken(resources, resourceParameters)
            };

        private static Option<string> GetPagingToken(IReadOnlyList<Resource> items, ResourceParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
            {
                return OptionHelper.None<string>();
            }

            var lastItem = items.Last();
            var lastItemSortValue = GetPropertyValue(lastItem, parameters.SortBy);
            var pagingToken = new PagingToken<string>
            {
                PreviousIdValue = lastItem.Name,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemSortValue),
                DataType = lastItem.Name.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };
            return OptionHelper.Some(pagingToken.ToString());
        }

        private static object GetPropertyValue(Resource item, ResourceSortBy sortBy)
        {
            switch (sortBy)
            {
                case ResourceSortBy.NameAscending:
                case ResourceSortBy.NameDescending:
                    return item.Name;

                case ResourceSortBy.PoolNameAscending:
                case ResourceSortBy.PoolNameDescending:
                    return item.PoolName;

                case ResourceSortBy.GroupNameAscending:
                case ResourceSortBy.GroupNameDescending:
                    return item.GroupName;

                case ResourceSortBy.PendingCountAscending:
                case ResourceSortBy.PendingCountDescending:
                    return item.PendingSessionCount;

                case ResourceSortBy.ActiveCountAscending:
                case ResourceSortBy.ActiveCountDescending:
                    return item.ActiveSessionCount;

                case ResourceSortBy.DisplayStatusAscending:
                case ResourceSortBy.DisplayStatusDescending:
                    return GetDisplayStatusOrder(item.DisplayStatus);

                default:
                    throw new ArgumentException($"Unknown {nameof(ResourceSortBy)} index", nameof(sortBy));
            }
        }

        private static int GetDisplayStatusOrder(ResourceDisplayStatus status)
        {
            switch (status)
            {
                case ResourceDisplayStatus.Working:
                    return 1;
                case ResourceDisplayStatus.Idle:
                    return 2;
                case ResourceDisplayStatus.Offline:
                    return 3;
                case ResourceDisplayStatus.Warning:
                    return 4;
                case ResourceDisplayStatus.LoggedOut:
                    return 5;
                case ResourceDisplayStatus.Missing:
                    return 6;
                case ResourceDisplayStatus.Private:
                    return 7;
                default:
                    return 8;
            };
        }
    }
}
