namespace BluePrism.Server.Domain.Models.Pagination
{
    public class ScheduleLogsPagingToken : BasePagingToken<int>
    {
        public const string IdColumnName = "scheduleLogId";
        public override int PreviousIdValue { get; set; }
    }
}
