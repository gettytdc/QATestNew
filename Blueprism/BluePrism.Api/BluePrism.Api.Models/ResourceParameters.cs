namespace BluePrism.Api.Models
{
    public class ResourceParameters : IPagingModel<string>
    {
        public ResourceSortBy? SortBy { get; set; }

        public int? ItemsPerPage { get; set; }

        public StartsWithStringFilterModel Name { get; set; }

        public StartsWithStringFilterModel GroupName { get; set; }

        public StartsWithStringFilterModel PoolName { get; set; }

        public RangeFilterModel<int?> ActiveSessionCount { get; set; }

        public RangeFilterModel<int?> PendingSessionCount { get; set; }

        public CommaDelimitedCollection<ResourceDisplayStatus> DisplayStatus { get; set; }

        public PagingTokenModel<string> PagingToken { get; set; }
    }
}
