namespace BluePrism.Api.Domain
{
    using Filters;
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class ResourceParameters : IProvideHashCodeForPagingTokenValidation
    {
        public ResourceSortBy SortBy { get; set; }

        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }

        [JsonIgnore]
        public Option<PagingToken<string>> PagingToken { get; set; }

        public Filter<string> Name { get; set; }

        public Filter<string> GroupName { get; set; }

        public Filter<string> PoolName { get; set; }

        public Filter<int> ActiveSessionCount { get; set; }

        public Filter<int> PendingSessionCount { get; set; }

        public Filter<ResourceDisplayStatus> DisplayStatus { get; set; }

        public override bool Equals(object obj) => (obj is ResourceParameters other && Equals(other));

        private bool Equals(ResourceParameters other) =>
            SortBy == other.SortBy &&
            ItemsPerPage == other.ItemsPerPage &&
            PagingToken.ToString() == other.PagingToken.ToString();

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)SortBy;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (GroupName != null ? GroupName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PoolName != null ? PoolName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ActiveSessionCount != null ? ActiveSessionCount.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PendingSessionCount != null ? PendingSessionCount.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayStatus != null ? DisplayStatus.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ItemsPerPage.GetHashCode();
                hashCode = (hashCode * 397) ^ (PagingToken != null ? PagingToken.GetHashCode() : 0);
                return hashCode;
            }
        }

        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
