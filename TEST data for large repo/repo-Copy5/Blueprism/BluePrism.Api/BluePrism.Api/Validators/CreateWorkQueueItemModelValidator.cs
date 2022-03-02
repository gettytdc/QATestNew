namespace BluePrism.Api.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using Models;

    public class CreateWorkQueueItemModelValidator : AbstractValidator<CreateWorkQueueItemModel>
    {
        public CreateWorkQueueItemModelValidator(Domain.CreateWorkQueueItemsSettings settings)
        {
            RuleFor(x => x.Data).NotNull().NotEmpty()
                .WithMessage(x =>
                    "The request contains empty Data items – Data items for create work queue requests cannot be empty.");

            When(x => x.Data != null, () =>
            {
                RuleFor(x => x.Data.Rows).NotNull().NotEmpty()
                    .WithMessage(x =>
                        "The request contains empty Row items – Row items for create work queue requests cannot be empty.");

                When(x => x.Data.Rows != null && x.Data.Rows.Any(), () =>
                {
                    RuleForEach(x => x.Data.Rows)
                        .SetValidator(new DataCollectionModelValidator());
                });
            });

            When(x => x.Status != null, () =>
                RuleFor(x => x.Status).NotEmpty().MaximumLength(settings.MaxStatusLength));

            When(x => x.Tags != null, () =>
            {
                RuleForEach(x => x.Tags).NotEmpty().MaximumLength(settings.MaxTagLength)
                    .WithMessage("At least one tag in this request exceeds the maximum limit of 255 characters.");
            });

            RuleFor(x => x.Priority).GreaterThanOrEqualTo(0)
                .WithMessage($"The priority level must be a valid integer value: 0 - 9999")
                .LessThanOrEqualTo(9999)
                .WithMessage($"The priority level must be a valid integer value: 0 - 9999");

        }
    }
}
