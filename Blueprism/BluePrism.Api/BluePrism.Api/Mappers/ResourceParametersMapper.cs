namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Filters;
    using FilterMappers;

    public static class ResourceParametersMapper
    {
        private const int DefaultItemsPerPage = 10;

        public static Domain.ResourceParameters ToDomainObject(this Models.ResourceParameters @this) =>
            new Domain.ResourceParameters
            {
                SortBy = @this.SortBy?.ToDomainObject() ?? Domain.ResourceSortBy.NameAscending,
                ItemsPerPage = @this.ItemsPerPage ?? DefaultItemsPerPage,
                Name = @this.Name.ToDomain(),
                GroupName = @this.GroupName.ToDomain(),
                PoolName = @this.PoolName.ToDomain(),
                ActiveSessionCount = @this.ActiveSessionCount.ToDomain(v => v ?? default(int)),
                PendingSessionCount = @this.PendingSessionCount.ToDomain(v => v ?? default(int)),
                DisplayStatus = @this.DisplayStatus?.Select(x => x.ToDomainObject()).ToMultiValueFilter()
                         ?? new MultiValueFilter<Domain.ResourceDisplayStatus>(Array.Empty<Filter<Domain.ResourceDisplayStatus>>()),
                PagingToken = @this.PagingToken.ToDomainPagingToken(),
            };

        private static readonly IDictionary<Models.ResourceSortBy, Domain.ResourceSortBy> SortByMappings =
            new Dictionary<Models.ResourceSortBy, Domain.ResourceSortBy>
            {
                [Models.ResourceSortBy.NameAsc] = Domain.ResourceSortBy.NameAscending,
                [Models.ResourceSortBy.NameDesc] = Domain.ResourceSortBy.NameDescending,
                [Models.ResourceSortBy.PoolAsc] = Domain.ResourceSortBy.PoolNameAscending,
                [Models.ResourceSortBy.PoolDesc] = Domain.ResourceSortBy.PoolNameDescending,
                [Models.ResourceSortBy.GroupAsc] = Domain.ResourceSortBy.GroupNameAscending,
                [Models.ResourceSortBy.GroupDesc] = Domain.ResourceSortBy.GroupNameDescending,
                [Models.ResourceSortBy.PendingAsc] = Domain.ResourceSortBy.PendingCountAscending,
                [Models.ResourceSortBy.PendingDesc] = Domain.ResourceSortBy.PendingCountDescending,
                [Models.ResourceSortBy.ActiveAsc] = Domain.ResourceSortBy.ActiveCountAscending,
                [Models.ResourceSortBy.ActiveDesc] = Domain.ResourceSortBy.ActiveCountDescending,
                [Models.ResourceSortBy.DisplayStatusAsc] = Domain.ResourceSortBy.DisplayStatusAscending,
                [Models.ResourceSortBy.DisplayStatusDesc] = Domain.ResourceSortBy.DisplayStatusDescending,
            };

        private static readonly IDictionary<Models.ResourceDisplayStatus, Domain.ResourceDisplayStatus> DisplayStatusMappings =
            new Dictionary<Models.ResourceDisplayStatus, Domain.ResourceDisplayStatus>
            {
                [Models.ResourceDisplayStatus.Working] = Domain.ResourceDisplayStatus.Working,
                [Models.ResourceDisplayStatus.Idle] = Domain.ResourceDisplayStatus.Idle,
                [Models.ResourceDisplayStatus.Warning] = Domain.ResourceDisplayStatus.Warning,
                [Models.ResourceDisplayStatus.Offline] = Domain.ResourceDisplayStatus.Offline,
                [Models.ResourceDisplayStatus.Missing] = Domain.ResourceDisplayStatus.Missing,
                [Models.ResourceDisplayStatus.LoggedOut] = Domain.ResourceDisplayStatus.LoggedOut,
                [Models.ResourceDisplayStatus.Private] = Domain.ResourceDisplayStatus.Private,
            };

        private static Domain.ResourceSortBy ToDomainObject(this Models.ResourceSortBy sortBy) =>
            SortByMappings.TryGetValue(sortBy, out var domainSortBy)
                ? domainSortBy
                : throw new ArgumentException("Unexpected sort order", nameof(sortBy));

        private static Domain.ResourceDisplayStatus ToDomainObject(this Models.ResourceDisplayStatus status) =>
            DisplayStatusMappings.TryGetValue(status, out var displayStatus)
                ? displayStatus
                : throw new ArgumentException("Unexpected display status", nameof(status));

    }
}
