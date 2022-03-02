namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;

    public class WorkQueueCompositionParameters
    {
        public IReadOnlyCollection<Guid> WorkQueueIds { get; set; }
    }
}
