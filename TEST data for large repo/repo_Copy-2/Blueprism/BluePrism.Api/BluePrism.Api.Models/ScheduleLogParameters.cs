namespace BluePrism.Api.Models
{
    using System;

    public class ScheduleLogParameters : IPagingModel<int>
    {
        public int? ItemsPerPage { get; set; }
        public CommaDelimitedCollection<ScheduleLogStatus> ScheduleLogStatus { get; set; }
        public RangeFilterModel<DateTimeOffset?> StartTime { get; set; }
        public RangeFilterModel<DateTimeOffset?> EndTime { get; set; }
        public PagingTokenModel<int> PagingToken { get; set; }
    }
}
