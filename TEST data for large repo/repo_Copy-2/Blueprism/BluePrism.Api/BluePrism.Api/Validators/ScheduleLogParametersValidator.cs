namespace BluePrism.Api.Validators
{
    using FluentValidation;
    using Mappers;
    using Models;
    using static FilterValidationHelper;

    public class ScheduleLogParametersValidator : PagingTokenValidator<int, ScheduleLogParameters>
    {
        public ScheduleLogParametersValidator(Domain.PagingSettings pagingSetting)
        {
            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSetting.MaxItemsPerPage));

            RuleFor(x => x.StartTime).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage("Invalid 'startTime' filter value");
            RuleFor(x => x.EndTime).Must(BeValidFilter).Must(BeValidRangeFilterOfDateTimeOffset)
                .WithMessage("Invalid 'endTime' filter value");

            RuleForEach(x => x.ScheduleLogStatus).IsInEnum()
                .WithMessage((_, field) => $"Invalid 'scheduleLogStatus' filter value: '{field}'");
        }
        
        protected override string GetItemHashCodeForValidation(ScheduleLogParameters model) => model.ToDomainObject().GetHashCodeForValidation();
    }
}
