namespace BluePrism.Api.Domain
{
    using Filters;
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class WorkQueueParameters: IProvideHashCodeForPagingTokenValidation
    {
        public WorkQueueSortByProperty SortBy { get; set; }
        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }
        public Filter<string> NameFilter { get; set; }
        public Filter<QueueStatus> QueueStatusFilter { get; set; }
        public Filter<string> KeyFieldFilter { get; set; }
        public Filter<int> MaxAttemptsFilter { get; set; }
        public Filter<int> PendingItemCountFilter { get; set; }
        public Filter<int> LockedItemCountFilter { get; set; }
        public Filter<int> CompletedItemCountFilter { get; set; }
        public Filter<int> ExceptionedItemCountFilter { get; set; }
        public Filter<int> TotalItemCountFilter { get; set; }
        public Filter<int> AverageWorkTimeFilter {  get; set; }
        public Filter<int> TotalCaseDurationFilter { get; set; }
        [JsonIgnore]
        public Option<PagingToken<int>> PagingToken { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)SortBy;
                hashCode = (hashCode * 397) ^ (NameFilter != null ? NameFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QueueStatusFilter != null ? QueueStatusFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (KeyFieldFilter != null ? KeyFieldFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MaxAttemptsFilter != null ? MaxAttemptsFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PendingItemCountFilter != null ? PendingItemCountFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LockedItemCountFilter != null ? LockedItemCountFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CompletedItemCountFilter != null ? CompletedItemCountFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionedItemCountFilter != null ? ExceptionedItemCountFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TotalItemCountFilter != null ? TotalItemCountFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AverageWorkTimeFilter != null ? AverageWorkTimeFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TotalCaseDurationFilter != null ? TotalCaseDurationFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ItemsPerPage.GetHashCode();
                return hashCode;
            }
        }

        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
