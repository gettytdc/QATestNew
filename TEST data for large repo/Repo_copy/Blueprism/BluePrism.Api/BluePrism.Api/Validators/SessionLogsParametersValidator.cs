namespace BluePrism.Api.Validators
{
    using FluentValidation;
    using Mappers;
    using Models;

    public class SessionLogsParametersValidator : PagingTokenValidator<long, SessionLogsParameters>
    {
        public SessionLogsParametersValidator(Domain.PagingSettings pagingSettings)
        {
            When(x => x.ItemsPerPage != null, () =>
                RuleFor(x => x.ItemsPerPage).InclusiveBetween(1, pagingSettings.MaxItemsPerPage));
        }

        protected override string GetItemHashCodeForValidation(SessionLogsParameters model) => model.ToDomainObject().GetHashCodeForValidation();
    }
}
