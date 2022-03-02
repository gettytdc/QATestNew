namespace BluePrism.Api.Models
{
    using System;

    public class SessionParameters : IPagingModel<long>
    {
        public SessionSortBy? SortBy { get; set; }

        public int? ItemsPerPage { get; set; }

        public PagingTokenModel<long> PagingToken { get; set; }

        public StartsWithStringFilterModel ProcessName { get; set; }

        public StartsWithStringFilterModel SessionNumber { get; set; }

        public StartsWithStringFilterModel ResourceName { get; set; }

        public StartsWithStringFilterModel UserName { get; set; }

        public CommaDelimitedCollection<SessionStatus> Status { get; set; }

        public RangeFilterModel<DateTimeOffset?> StartTime { get; set; }

        public RangeFilterModel<DateTimeOffset?> EndTime { get; set; }

        public StartsWithStringFilterModel LatestStage { get; set; }

        public RangeFilterModel<DateTimeOffset?> StageStarted { get; set; }
    }
}
