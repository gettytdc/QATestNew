namespace BluePrism.Api.Domain
{
    using System;
    using Filters;
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class SessionParameters: IProvideHashCodeForPagingTokenValidation
    {
        public SessionSortByProperty SortBy { get; set; }

        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }

        [JsonIgnore]
        public Option<PagingToken<long>> PagingToken { get; set; }

        public Filter<string> ProcessName { get; set; }

        public Filter<string> SessionNumber { get; set; }

        public Filter<string> ResourceName { get; set; }

        public Filter<string> User { get; set; }

        public Filter<SessionStatus> Status { get; set; }

        public Filter<DateTimeOffset> StartTime { get; set; }

        public Filter<DateTimeOffset> EndTime { get; set; }

        public Filter<string> LatestStage { get; set; }

        public Filter<DateTimeOffset> StageStarted { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)SortBy;
                hashCode = (hashCode * 397) ^ (ProcessName != null ? ProcessName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SessionNumber != null ? SessionNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResourceName != null ? ResourceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (User != null ? User.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StartTime != null ? StartTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EndTime != null ? EndTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LatestStage != null ? LatestStage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StageStarted != null ? StageStarted.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ItemsPerPage.GetHashCode();
                return hashCode;
            }
        }

        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
