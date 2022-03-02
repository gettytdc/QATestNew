namespace BluePrism.Api.Validators
{
    using FluentValidation;
    using Models;

    public class UpdateWorkQueueValidator : AbstractValidator<UpdateWorkQueueModel>
    {
        public UpdateWorkQueueValidator()
        {
            When(x => x.Name != null, () =>
                RuleFor(x => x.Name).NotEmpty().MinimumLength(1));
            When(x => x.KeyField != null, () =>
                RuleFor(x => x.KeyField).NotEmpty());
            When(x => x.MaxAttempts != null, () =>
                RuleFor(x => x.MaxAttempts).GreaterThanOrEqualTo(0));
        }
    }
}
