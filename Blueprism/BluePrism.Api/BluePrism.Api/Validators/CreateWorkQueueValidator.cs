namespace BluePrism.Api.Validators
{
    using FluentValidation;
    using Models;

    public class CreateWorkQueueValidator : AbstractValidator<CreateWorkQueueRequestModel>
    {
        public CreateWorkQueueValidator()
        {
            RuleFor(x => x.Name).NotNull().Length(1, 255);
            RuleFor(x => x.KeyField).NotNull().MaximumLength(255);
            RuleFor(x => x.MaxAttempts).InclusiveBetween(1, 999999).WithMessage("'maxAttempts' must be a whole number between 1 and 999999");
        }
    }
}
