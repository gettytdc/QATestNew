namespace BluePrism.Api.Domain
{
    using System;
    using Filters;
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class ScheduleLogParameters :IProvideHashCodeForPagingTokenValidation
    {
        [JsonIgnore]
        public Filter<int> ScheduleId { get; set; }
        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }
        public Filter<ScheduleLogStatus> ScheduleLogStatus { get; set; }
        public Filter<DateTimeOffset> StartTime { get; set; }
        public Filter<DateTimeOffset> EndTime { get; set; }
        [JsonIgnore]
        public Option<PagingToken<int>> PagingToken { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ItemsPerPage.GetHashCode();
                hashCode = (hashCode * 397) ^ (ScheduleLogStatus != null ? ScheduleLogStatus.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StartTime != null ? StartTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EndTime != null ? EndTime.GetHashCode() : 0);
                return hashCode;
            }
        }

        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
