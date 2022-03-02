namespace BluePrism.Api.Validators
{
    using Models;
    using FluentValidation;
    using FluentValidation.Results;

    public abstract class PagingTokenValidator<TIdType, TModel> : AbstractValidator<TModel> where TModel : IPagingModel<TIdType>
    {
        public override ValidationResult Validate(ValidationContext<TModel> context)
        {
            var pagingToken = context.InstanceToValidate.PagingToken;

            var result = base.Validate(context);

            if (pagingToken != null)
            {
                if (!PagingTokenFormatIsCorrect(pagingToken))
                    result.Errors.Add(new ValidationFailure(nameof(context.InstanceToValidate.PagingToken), "Paging Token is malformed"));

                if (!PagingTokenIsValid(context.InstanceToValidate))
                    result.Errors.Add(new ValidationFailure(
                        nameof(context.InstanceToValidate.PagingToken),
                        "Previously generated Paging Token cannot be used because paging, filtering and/ or sorting parameters have been updated"));
            }

            return result;
        }

        protected abstract string GetItemHashCodeForValidation(TModel model);

        private static bool PagingTokenFormatIsCorrect(PagingTokenModel<TIdType> pagingToken) =>
            pagingToken.GetPagingTokenState() != PagingTokenState.Malformed;

        private bool PagingTokenIsValid(TModel model) =>
            model.PagingToken.ParametersHashCode == GetItemHashCodeForValidation(model);
    }
}
