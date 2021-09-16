namespace BluePrism.Api.Domain.Dashboard
{
    using System;

    public class WorkQueueComposition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Deferred { get; set; }
        public int Locked { get; set; }
        public int Exceptioned { get; set; }
    }
}
