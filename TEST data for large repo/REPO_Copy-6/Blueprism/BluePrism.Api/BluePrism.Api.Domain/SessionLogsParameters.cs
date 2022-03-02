namespace BluePrism.Api.Domain
{
    using Func;
    using Newtonsoft.Json;
    using PagingTokens;

    public class SessionLogsParameters: IProvideHashCodeForPagingTokenValidation
    {
        [JsonConverter(typeof(ItemsPerPageConverter))]
        public ItemsPerPage ItemsPerPage { get; set; }

        [JsonIgnore]
        public Option<PagingToken<long>> PagingToken { get; set; }

        public override int GetHashCode() => ItemsPerPage.GetHashCode();
        public string GetHashCodeForValidation() => this.ToHmacSha256();
    }
}
