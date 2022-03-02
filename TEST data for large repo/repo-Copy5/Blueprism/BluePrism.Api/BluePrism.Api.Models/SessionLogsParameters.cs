namespace BluePrism.Api.Models
{
    public class SessionLogsParameters : IPagingModel<long>
    {
        public int? ItemsPerPage { get; set; }
        public PagingTokenModel<long> PagingToken { get; set; }
    }
}
