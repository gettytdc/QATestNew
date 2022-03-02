namespace BluePrism.Api.Models
{
    using System;

    public class CreateWorkQueueResponseModel
    {
        public Guid Id { get; }

        public CreateWorkQueueResponseModel(Guid id) => Id = id;
    }
}
