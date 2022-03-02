namespace BluePrism.Api.Validators
{
    using System;
    using System.Data.SqlTypes;
    using FluentValidation;
    using Models;

    public class ResourceUtilizationParametersValidator : AbstractValidator<ResourceUtilizationParameters>
    {
        private static readonly DateTime MinValidDateTimeOffset = SqlDateTime.MinValue.Value;
        private static readonly DateTime MaxValidDateTimeOffset = SqlDateTime.MaxValue.Value;

        public ResourceUtilizationParametersValidator(Domain.PagingSettings pagingSettings)
        {
            When(x => x.PageSize != null, () =>
                RuleFor(x => x.PageSize).InclusiveBetween(1, pagingSettings.MaxItemsPerPage)
                    .WithMessage(x => $"Invalid 'PageSize' parameter value: {x.PageSize}"));

            When(x => x.PageNumber != null, () =>
                RuleFor(x => x.PageNumber).GreaterThan(0)
                    .WithMessage(x => $"Invalid 'PageNumber' parameter value: {x.PageNumber}"));

            RuleFor(x => x.StartDate).Must(IsValidDateTimeOffset)
                .WithMessage(x => "Invalid 'StartDate' parameter value");
        }

        private static bool IsValidDateTimeOffset(DateTime dateTimeOffset) =>
            (dateTimeOffset.CompareTo(MinValidDateTimeOffset) > 0 && dateTimeOffset.CompareTo(MaxValidDateTimeOffset) < 0);
    }
}
