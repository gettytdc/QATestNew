namespace BluePrism.Server.Domain.Models.Pagination
{
    public class WorkQueueItemPagingToken : BasePagingToken<long>
    {
        public const string IdColumnName = "ident";

        public override long PreviousIdValue { get; set; }

    }
}
