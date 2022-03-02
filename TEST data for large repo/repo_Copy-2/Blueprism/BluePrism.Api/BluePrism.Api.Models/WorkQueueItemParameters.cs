namespace BluePrism.Api.Models
{
    using System;

    public class WorkQueueItemParameters : IPagingModel<long>
    {
        public WorkQueueItemSortBy? SortBy { get; set; }
        
        public StartsWithOrContainsStringFilterModel KeyValue { get; set; }

        public StartsWithOrContainsStringFilterModel Status { get; set; }

        public StartsWithOrContainsStringFilterModel ExceptionReason { get; set; }

        public RangeFilterModel<int?> TotalWorkTime { get; set; }

        public RangeFilterModel<int?> Attempt { get; set; }

        public RangeFilterModel<int?> Priority { get; set; }

        public RangeFilterModel<DateTimeOffset?> LoadedDate { get; set; }

        public RangeFilterModel<DateTimeOffset?> DeferredDate { get; set; }

        public RangeFilterModel<DateTimeOffset?> LockedDate { get; set; }

        public RangeFilterModel<DateTimeOffset?> CompletedDate { get; set; }

        public RangeFilterModel<DateTimeOffset?> ExceptionedDate { get; set; }

        public RangeFilterModel<DateTimeOffset?> LastUpdated { get; set; }

        public int? ItemsPerPage { get; set; }

        public PagingTokenModel<long> PagingToken { get; set; }
    }
}
