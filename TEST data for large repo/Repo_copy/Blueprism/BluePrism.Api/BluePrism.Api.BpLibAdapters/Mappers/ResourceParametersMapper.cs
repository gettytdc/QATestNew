namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using FilterMappers;
    using Func;
    using Server.Domain.Models.Pagination;

    public static class ResourceParametersMapper
    {
        public static Server.Domain.Models.ResourceParameters ToBluePrismObject(this Domain.ResourceParameters @this) =>
            new Server.Domain.Models.ResourceParameters
            {
                SortBy = (Server.Domain.Models.ResourceSortBy)@this.SortBy,
                ItemsPerPage = @this.ItemsPerPage,
                Name = @this.Name.ToBluePrismObject(),
                GroupName = @this.GroupName.ToBluePrismObject(),
                PoolName = @this.PoolName.ToBluePrismObject(),
                ActiveSessionCount = @this.ActiveSessionCount.ToBluePrismObject(),
                PendingSessionCount = @this.PendingSessionCount.ToBluePrismObject(),
                DisplayStatus = @this.DisplayStatus.ToBluePrismObject(x => x.ToBluePrismObject()),
                PagingToken = @this.PagingToken.ToBluePrismPagingToken()
            };

        private static Option<ResourcePagingToken> ToBluePrismPagingToken(this Option<Domain.PagingTokens.PagingToken<string>> pagingToken) =>
            pagingToken is Some<Domain.PagingTokens.PagingToken<string>> t
                ? OptionHelper.Some(new ResourcePagingToken
                {
                    PreviousIdValue = t.Value.PreviousIdValue,
                    PreviousSortColumnValue = t.Value.PreviousSortColumnValue,
                    DataType = t.Value.DataType
                })
                : OptionHelper.None<ResourcePagingToken>();

        private static readonly IDictionary<Domain.ResourceDisplayStatus, Server.Domain.Models.ResourceDisplayStatus> DisplayStatusMappings =
            new Dictionary<Domain.ResourceDisplayStatus, Server.Domain.Models.ResourceDisplayStatus>
            {
                [Domain.ResourceDisplayStatus.Working] = Server.Domain.Models.ResourceDisplayStatus.Working,
                [Domain.ResourceDisplayStatus.Idle] = Server.Domain.Models.ResourceDisplayStatus.Idle,
                [Domain.ResourceDisplayStatus.Warning] = Server.Domain.Models.ResourceDisplayStatus.Warning,
                [Domain.ResourceDisplayStatus.Offline] = Server.Domain.Models.ResourceDisplayStatus.Offline,
                [Domain.ResourceDisplayStatus.Missing] = Server.Domain.Models.ResourceDisplayStatus.Missing,
                [Domain.ResourceDisplayStatus.LoggedOut] = Server.Domain.Models.ResourceDisplayStatus.LoggedOut,
                [Domain.ResourceDisplayStatus.Private] = Server.Domain.Models.ResourceDisplayStatus.Private,
            };

        private static Server.Domain.Models.ResourceDisplayStatus ToBluePrismObject(this Domain.ResourceDisplayStatus status) =>
            DisplayStatusMappings.TryGetValue(status, out var displayStatus)
                ? displayStatus
                : throw new ArgumentException("Unexpected display status", nameof(status));
    }
}
