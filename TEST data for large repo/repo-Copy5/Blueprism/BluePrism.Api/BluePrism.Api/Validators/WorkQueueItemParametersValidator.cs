namespace BluePrism.Api.Validators
{
    using FluentValidation;
    using Func;
    using Mappers;
    using Models;
    using WorkQueueItemParameters = Models.WorkQueueItemParameters;
    using static FilterValidationHelper;
    using static Mappers.WorkQueueItemsSortByMapper;

    public class WorkQueueItemParametersValidator : PagingTokenValidator<long, WorkQueueItemParameters>
    {
        public WorkQueueItemParametersValidator(Domain.PagingSettings pagingSettings)
        {
            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSettings.MaxItemsPerPage));

            When(x => x.SortBy.HasValue,
                () =>
                    RuleFor(x => x.SortBy)
                        .Must(SortByIsExpectedInput)
                        .WithMessage(x => $"Invalid 'sortBy' value: '{x.SortBy}'"));

            RuleFor(x => x.Status).Must(BeValidFilter)
                .WithMessage(x => "Invalid 'status' filter value");

            RuleFor(x => x.KeyValue).Must(BeValidFilter)
                .WithMessage(x => "Invalid 'keyValue' filter value");

            RuleFor(x => x.ExceptionReason).Must(BeValidFilter)
                .WithMessage(x => "Invalid 'exceptionReason' filter value");

            RuleFor(x => x.TotalWorkTime).Must(BeValidRangeFilter)
                .WithMessage(x => "Invalid 'totalWorkTime' filter value");

            RuleFor(x => x.Attempt).Must(BeValidRangeFilter)
                .WithMessage(x => "Invalid 'attempt' filter value");

            RuleFor(x => x.Priority).Must(BeValidRangeFilter)
                .WithMessage(x => "Invalid 'priority' filter value");

            RuleFor(x => x.LoadedDate).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'loadedDate' filter value");

            RuleFor(x => x.DeferredDate).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'deferredDate' filter value");

            RuleFor(x => x.LockedDate).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'lockedDate' filter value");

            RuleFor(x => x.CompletedDate).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'completedDate' filter value");

            RuleFor(x => x.ExceptionedDate).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'exceptionedDate' filter value");

            RuleFor(x => x.LastUpdated).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'lastUpdated' filter value");

        }
        protected override string GetItemHashCodeForValidation(WorkQueueItemParameters model) => model.ToDomainObject().GetHashCodeForValidation();

        private static bool SortByIsExpectedInput(WorkQueueItemSortBy? sortBy) =>
            GetWorkQueueItemsSortByModelName(sortBy) is Some<Domain.WorkQueueItemSortByProperty>;
    }
}
