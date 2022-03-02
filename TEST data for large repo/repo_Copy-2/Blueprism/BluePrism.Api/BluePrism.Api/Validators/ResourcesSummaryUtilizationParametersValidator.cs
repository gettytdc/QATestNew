namespace BluePrism.Api.Validators
{
    using System;
    using System.Data.SqlTypes;
    using System.Linq;
    using FluentValidation;
    using Models;

    public class ResourcesSummaryUtilizationParametersValidator : AbstractValidator<ResourcesSummaryUtilizationParameters>
    {
        private static readonly DateTime MinValidDateTime = SqlDateTime.MinValue.Value;
        private static readonly DateTime MaxValidDateTime = SqlDateTime.MaxValue.Value;
        private static readonly int AllowedMaximumResourceIdsCount = 20;
        private static readonly int AllowedDaysBetweenStartAndEndDate = 90;

        public ResourcesSummaryUtilizationParametersValidator()
        {
            RuleFor(x => x.StartDate).Must(IsValidDateTime)
                .WithMessage(x => "Invalid 'StartDate' parameter value");

            RuleFor(x => x.EndDate).Must(IsValidDateTime)
                .WithMessage(x => "Invalid 'EndDate' parameter value");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage(x => "Invalid 'EndDate' parameter value. 'EndDate' must be greater than 'StartDate'");

            RuleFor(x => (x.EndDate - x.StartDate).TotalDays)
                .LessThanOrEqualTo(AllowedDaysBetweenStartAndEndDate)
                .WithMessage($"Invalid 'EndDate' parameter value. 'EndDate' must be no more than {AllowedDaysBetweenStartAndEndDate} days from 'StartDate'");

            When(x => x.ResourceIds != null && x.ResourceIds.Any(), () =>
            {
                RuleFor(x => x.ResourceIds.Count)
                    .LessThanOrEqualTo(AllowedMaximumResourceIdsCount)
                    .WithMessage($"'ResourceIds' can only contain up to {AllowedMaximumResourceIdsCount} resource ids.");
            });
        }
            

        private static bool IsValidDateTime(DateTime dateTimeOffset) =>
            (dateTimeOffset.CompareTo(MinValidDateTime) > 0 && dateTimeOffset.CompareTo(MaxValidDateTime) < 0);
    }
}
