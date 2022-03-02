namespace BluePrism.Api.Validators
{
    using FluentValidation;
    using Models;
    using Func;
    using Logging;
    using FluentValidation.Results;
    using System.Linq;
    using Mappers;
    using static Mappers.WorkQueueSortByMapper;
    using static FilterValidationHelper;

    public class WorkQueueParametersValidator : PagingTokenValidator<int, WorkQueueParameters>
    {
        private readonly ILogger<WorkQueueParametersValidator> _logger;

        public WorkQueueParametersValidator(Domain.PagingSettings pagingSettings, ILogger<WorkQueueParametersValidator> logger)
        {
            _logger = logger;

            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSettings.MaxItemsPerPage));
            
            When(x => x.SortBy.HasValue,() =>
                RuleFor(x => x.SortBy)
                .Must(SortByIsExpectedInput)
                .WithMessage(x=> $"Invalid 'sortBy' value: '{x.SortBy}'"));

            RuleFor(x => x.Name).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'name' filter value: '{x.Name}'");

            RuleFor(x => x.Status).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'status' filter value: '{x.Status}'");

            RuleFor(x => x.KeyField).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'keyField' filter value: '{x.KeyField}'");

            RuleFor(x => x.MaxAttempts).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'maxAttempts' filter value: '{x.MaxAttempts}'");

            RuleFor(x => x.PendingItemCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'pendingItemCount' filter value: '{x.PendingItemCount}'");

            RuleFor(x => x.LockedItemCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'lockedItemCount' filter value: '{x.LockedItemCount}'");

            RuleFor(x => x.CompletedItemCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'completedItemCount' filter value: '{x.CompletedItemCount}'");

            RuleFor(x => x.ExceptionedItemCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'exceptionedItemCount' filter value: '{x.ExceptionedItemCount}'");

            RuleFor(x => x.TotalItemCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'totalItemCount' filter value: '{x.TotalItemCount}'");

            RuleFor(x => x.AverageWorkTime).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'averageWorkTime' filter value: '{x.AverageWorkTime}'");

            RuleFor(x => x.TotalCaseDuration).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'totalCaseDuration' filter value: '{x.TotalCaseDuration}'");
        }

        public override ValidationResult Validate(ValidationContext<WorkQueueParameters> context)
        {
            var result = base.Validate(context);

            if (!result.IsValid)
            {
                _logger.Debug("Validation for WorkQueueParameters failed with invalid fields:\r\n{0}", result.Errors.Select(x => $"{x.PropertyName} ({x.AttemptedValue})").Map(string.Join, "\r\n"));
            }

            return result;
        }

        protected override string GetItemHashCodeForValidation(WorkQueueParameters model) => model.ToDomainObject().GetHashCodeForValidation();

        private static bool SortByIsExpectedInput(WorkQueueSortBy? sortBy) =>
            GetWorkQueueSortByModelName(sortBy) is Some<Domain.WorkQueueSortByProperty>;
    }
}
