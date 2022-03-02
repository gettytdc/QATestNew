namespace BluePrism.Api.Validators
{
    using System;
    using System.Linq;
    using FluentValidation;
    using Models;

    public class WorkQueueCompositionParametersValidator : AbstractValidator<WorkQueueCompositionParameters>
    {
        private const int MaxQueueCompositionNumber = 20;

        public WorkQueueCompositionParametersValidator()
        {
            RuleFor(x => x.WorkQueueIds)
                .NotNull()
                .WithMessage("'WorkQueueIds' parameter is required");

            When(x => x.WorkQueueIds != null, () =>
            {
                RuleFor(x => x.WorkQueueIds)
                    .NotEmpty()
                    .Must(x => x.All(y => y != Guid.Empty))
                    .WithMessage("'WorkQueueIds' parameter is required and cannot be empty");

                RuleFor(x => x.WorkQueueIds)
                    .Must(x => x.Count <= MaxQueueCompositionNumber)
                    .WithMessage($"'WorkQueueIds' parameter cannot have more than {MaxQueueCompositionNumber} items");

                RuleFor(x => x.WorkQueueIds)
                    .Must(x => x.Count == x.Distinct().Count())
                    .WithMessage("'WorkQueueIds' cannot contain duplicate values");
            });
        }
    }
}
