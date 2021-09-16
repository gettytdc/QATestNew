namespace BluePrism.Api.Models
{
    public enum WorkQueueItemState
    {
        None,
        Pending,
        Locked,
        Deferred,
        Completed,
        Exceptioned,
    }
}
