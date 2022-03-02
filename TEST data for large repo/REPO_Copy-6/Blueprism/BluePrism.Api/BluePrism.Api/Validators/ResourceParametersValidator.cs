namespace BluePrism.Api.Validators
{
    using System.Linq;
    using FluentValidation;
    using FluentValidation.Results;
    using Func;
    using Logging;
    using Mappers;
    using ResourceParameters = Models.ResourceParameters;
    using static FilterValidationHelper;

    public class ResourceParametersValidator : PagingTokenValidator<string, ResourceParameters>
    {
        private readonly ILogger<ResourceParametersValidator> _logger;

        protected override string GetItemHashCodeForValidation(ResourceParameters model) => model.ToDomainObject().GetHashCodeForValidation();

        public ResourceParametersValidator(ILogger<ResourceParametersValidator> logger, Domain.PagingSettings pagingSettings)
        {
            _logger = logger;

            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSettings.MaxItemsPerPage));

            RuleFor(x => x.Name).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'name' filter value: '{x.Name}'");

            RuleFor(x => x.GroupName).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'groupName' filter value: '{x.GroupName}'");

            RuleFor(x => x.PoolName).Must(BeValidFilter)
                .WithMessage(x => $"Invalid 'poolName' filter value: '{x.PoolName}'");

            RuleFor(x => x.ActiveSessionCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'activeSessionCount' filter value: '{x.ActiveSessionCount}'");

            RuleFor(x => x.PendingSessionCount).Must(BeValidRangeFilter)
                .WithMessage(x => $"Invalid 'pendingSessionCount' filter value: '{x.PendingSessionCount}'");

            RuleForEach(x => x.DisplayStatus).IsInEnum()
                 .WithMessage((_, field) => $"Invalid 'displayStatus' filter value: '{field}'");
        }

        public override ValidationResult Validate(ValidationContext<ResourceParameters> context)
        {
            var result = base.Validate(context);

            if (!result.IsValid)
            {
                _logger.Debug("Validation for ResourceParameters failed with invalid fields:\r\n{0}", result.Errors.Select(x => $"{x.PropertyName} ({x.AttemptedValue})").Map(string.Join, "\r\n"));
            }

            return result;
        }
    }
}
