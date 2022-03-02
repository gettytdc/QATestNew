namespace BluePrism.Api.Domain
{
    using System;
    using Filters;
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class WorkQueueItemParameters: IProvideHashCodeForPagingTokenValidation
    {
        public WorkQueueItemSortByProperty SortBy { get; set; }
        public Filter<string> KeyValue { get; set; }
        public Filter<string> Status { get; set; }
        public Filter<string> ExceptionReason { get; set; }
        public Filter<int> WorkTime { get; set; }
        public Filter<int> Attempt { get; set; }
        public Filter<int> Priority { get; set; }
        public Filter<DateTimeOffset> LoadedDate { get; set; }
        public Filter<DateTimeOffset> DeferredDate { get; set; }
        public Filter<DateTimeOffset> LockedDate { get; set; }
        public Filter<DateTimeOffset> CompletedDate { get; set; }
        public Filter<DateTimeOffset> LastUpdated { get; set; }
        public Filter<DateTimeOffset> ExceptionedDate { get; set; }

        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }

        [JsonIgnore]
        public Option<PagingToken<long>> PagingToken { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)SortBy;
                hashCode = (hashCode * 397) ^ (KeyValue != null ? KeyValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionReason != null ? ExceptionReason.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WorkTime != null ? WorkTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Attempt != null ? Attempt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Priority != null ? Priority.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LoadedDate != null ? LoadedDate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DeferredDate != null ? DeferredDate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LockedDate != null ? LockedDate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CompletedDate != null ? CompletedDate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LastUpdated != null ? LastUpdated.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionedDate != null ? ExceptionedDate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ItemsPerPage.GetHashCode();
                return hashCode;
            }
        }

        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
