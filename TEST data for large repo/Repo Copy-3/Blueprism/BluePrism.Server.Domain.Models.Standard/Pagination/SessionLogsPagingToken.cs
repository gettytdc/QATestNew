namespace BluePrism.Server.Domain.Models.Pagination
{
    public class SessionLogsPagingToken : BasePagingToken<long>
    {
        public const string IdColumnName = "logid";
        public override long PreviousIdValue { get; set; }
    }
}
