namespace BluePrism.Api.Domain
{
    using System;
    using System.Collections.Generic;

    public class CreateWorkQueueItem
    {
        public DataCollection Data { get; set; }
        public DateTimeOffset? DeferredDate { get; set; }
        public int Priority { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public string Status { get; set; }
    }
}
