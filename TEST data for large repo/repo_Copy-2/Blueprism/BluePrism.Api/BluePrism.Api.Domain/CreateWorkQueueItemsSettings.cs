namespace BluePrism.Api.Domain
{
    public class CreateWorkQueueItemsSettings
    {
        public int MaxCreateWorkQueueRequestsInBatch { get; set; }
        public int MaxStatusLength { get; set; }
        public int MaxTagLength { get; set; }
    }
}
