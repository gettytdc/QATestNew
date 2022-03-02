namespace BluePrism.Api.Models.Dashboard
{
    using System;

    public class WorkQueueCompositionModel
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
