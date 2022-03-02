namespace BluePrism.Server.Domain.Models.Pagination
{
    public class WorkQueuePagingToken : BasePagingToken<int>
    {
        public const string IdColumnName = "ident";

        public override int PreviousIdValue { get; set; }

    }
}
