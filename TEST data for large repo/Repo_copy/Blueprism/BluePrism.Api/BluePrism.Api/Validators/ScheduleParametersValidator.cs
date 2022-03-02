namespace BluePrism.Api.Validators
{
    using System.Linq;
    using FluentValidation;
    using FluentValidation.Results;
    using Func;
    using Logging;
    using Mappers;
    using Models;

    using static FilterValidationHelper;

    public class ScheduleParametersValidator : PagingTokenValidator<string, ScheduleParameters>
    {
        private readonly ILogger<ScheduleParametersValidator> _logger;

        protected override string GetItemHashCodeForValidation(ScheduleParameters model) => model.ToDomainObject().GetHashCodeForValidation();

        public ScheduleParametersValidator(ILogger<ScheduleParametersValidator> logger, Domain.PagingSettings pagingSettings)
        {
            _logger = logger;

            When(x => x.Name != null, () =>
                RuleFor(x => x.Name).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'name' filter value: '{x.Name}'"));
            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSettings.MaxItemsPerPage));
        }

        public override ValidationResult Validate(ValidationContext<ScheduleParameters> context)
        {
            var result = base.Validate(context);

            if (!result.IsValid)
            {
                _logger.Debug("Validation for ScheduleParameters failed with invalid fields:\r\n{0}", result.Errors.Select(x => $"{x.PropertyName} ({x.AttemptedValue})").Map(string.Join, "\r\n"));
            }

            return result;
        }
    }
}
