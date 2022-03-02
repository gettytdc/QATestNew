namespace BluePrism.Server.Domain.Models.Pagination
{
    public class ResourcePagingToken : BasePagingToken<string>
    {
        public const string IdColumnName = "r.name";
        public override string PreviousIdValue { get; set; }
    }
}
