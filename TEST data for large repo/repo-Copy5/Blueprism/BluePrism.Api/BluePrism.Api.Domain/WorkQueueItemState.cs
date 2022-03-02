namespace BluePrism.Api.Domain
{
    public enum WorkQueueItemState
    {
        None = 0,
        Pending = 1,
        Locked = 2,
        Deferred = 3,
        Completed = 4,
        Exceptioned = 5,
    }
}
