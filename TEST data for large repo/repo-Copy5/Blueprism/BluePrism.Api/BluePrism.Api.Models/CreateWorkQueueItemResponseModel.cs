namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;

    public class CreateWorkQueueItemResponseModel
    {
        public IEnumerable<Guid> Ids { get; }
        public CreateWorkQueueItemResponseModel(IEnumerable<Guid> ids) => Ids = ids;
    }
}
