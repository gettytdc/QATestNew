namespace BluePrism.Api.Domain
{
    using Filters;
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class ScheduleParameters: IProvideHashCodeForPagingTokenValidation
    {
        public Filter<string> Name { get; set; }

        public Filter<RetirementStatus> RetirementStatus { get; set; }

        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }
        
        [JsonIgnore]
        public Option<PagingToken<string>> PagingToken { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ItemsPerPage.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RetirementStatus != null ? RetirementStatus.GetHashCode() : 0);
                return hashCode;
            }
        }

        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
