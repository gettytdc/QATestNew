namespace BluePrism.Server.Domain.Models.Pagination
{
    public class SchedulePagingToken : BasePagingToken<string>
    {
        public const string IdColumnName = "s.name";
        public override string PreviousIdValue { get; set; }
    }
}
