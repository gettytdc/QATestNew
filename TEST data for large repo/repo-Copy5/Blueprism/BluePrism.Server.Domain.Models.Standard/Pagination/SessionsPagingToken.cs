namespace BluePrism.Server.Domain.Models.Pagination
{
    public class SessionsPagingToken : BasePagingToken<long>
    {
        public const string IdColumnName = "sessionnumber";

        public override long PreviousIdValue { get; set; }
    }
}
