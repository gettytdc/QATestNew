namespace BluePrism.Api.Validators
{
    
    using System.Linq;
    using FluentValidation;
    using FluentValidation.Results;
    using Models;
    using Func;
    using Logging;
    using Mappers;
    using static Mappers.SessionSortByMapper;
    using static FilterValidationHelper;

    public class SessionParametersValidator : PagingTokenValidator<long, SessionParameters>
    {
        private readonly ILogger<SessionParametersValidator> _logger;

        protected override string GetItemHashCodeForValidation(SessionParameters model) => model.ToDomainObject().GetHashCodeForValidation();

        public SessionParametersValidator(Domain.PagingSettings pagingSettings, ILogger<SessionParametersValidator> logger)
        {
            _logger = logger;

            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSettings.MaxItemsPerPage));

            When(x => x.SortBy.HasValue,
                () =>
                    RuleFor(x => x.SortBy)
                    .Must(SortByIsExpectedInput)
                    .WithMessage(x => $"Invalid 'sortBy' value: '{x.SortBy}'"));

            RuleFor(x => x.ProcessName).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'processName' filter value: '{x.ProcessName}'");

            RuleFor(x => x.SessionNumber).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'sessionNumber' filter value: '{x.SessionNumber}'");

            RuleFor(x => x.UserName).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'userName' filter value: '{x.UserName}'");

            RuleFor(x => x.LatestStage).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'latestStage' filter value: '{x.LatestStage}'");

            RuleFor(x => x.ResourceName).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'resourceName' filter value: '{x.ResourceName}'");

            RuleFor(x => x.StageStarted).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'stageStarted' filter value");

            RuleFor(x => x.StartTime).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'startTime' filter value");

            RuleFor(x => x.EndTime).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage(x => "Invalid 'endTime' filter value");
        }

        public override ValidationResult Validate(ValidationContext<SessionParameters> context)
        {
            var result = base.Validate(context);

            if (!result.IsValid)
            {
                _logger.Debug("Validation for SessionParameters failed with invalid fields:\r\n{0}", result.Errors.Select(x => $"{x.PropertyName} ({x.AttemptedValue})").Map(string.Join, "\r\n"));
            }

            return result;
        }

        private static bool SortByIsExpectedInput(SessionSortBy? sortBy) =>
            GetProcessSessionSortByModelName(sortBy) is Some<Domain.SessionSortByProperty>;
    }
}
