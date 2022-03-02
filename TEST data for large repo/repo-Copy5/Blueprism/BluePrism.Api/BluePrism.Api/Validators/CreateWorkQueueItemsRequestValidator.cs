namespace BluePrism.Api.Validators
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using Models;

    public class CreateWorkQueueItemsRequestValidator : AbstractValidator<IEnumerable<CreateWorkQueueItemModel>> 
    {
        public CreateWorkQueueItemsRequestValidator(Domain.CreateWorkQueueItemsSettings settings)
        {
            RuleFor(x => x.Count()).LessThanOrEqualTo(settings.MaxCreateWorkQueueRequestsInBatch)
                .WithMessage($"The batch size exceeds the permitted number of create work queue items. The maximum batch size is {settings.MaxCreateWorkQueueRequestsInBatch}");

        }
    }
}
