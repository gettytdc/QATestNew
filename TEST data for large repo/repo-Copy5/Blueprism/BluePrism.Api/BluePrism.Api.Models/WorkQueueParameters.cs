namespace BluePrism.Api.Models
{
    public class WorkQueueParameters: IPagingModel<int>
    {
        public WorkQueueSortBy? SortBy { get; set; }

        public int? ItemsPerPage { get; set; }

        public StartsWithOrContainsStringFilterModel Name { get; set; }

        public BasicFilterModel<QueueStatus> Status { get; set; }

        public StartsWithOrContainsStringFilterModel KeyField { get; set; }

        public RangeFilterModel<int?> MaxAttempts { get; set; }

        public RangeFilterModel<int?> PendingItemCount { get; set; }

        public RangeFilterModel<int?> LockedItemCount { get; set; }

        public RangeFilterModel<int?> CompletedItemCount { get; set; }

        public RangeFilterModel<int?>  ExceptionedItemCount { get; set; }

        public RangeFilterModel<int?>  TotalItemCount { get; set; }

        public RangeFilterModel<int?>  AverageWorkTime{  get; set; }

        public RangeFilterModel<int?>  TotalCaseDuration { get; set; }

        public PagingTokenModel<int> PagingToken { get; set; }
    }
}
