using System.ComponentModel;

namespace BluePrism.WorkQueueAnalysis.Enums
{
    public enum QueueSnapshotRequirements
    {
        [Description("Default option.")]
        Unknown = 0,
        [Description("Queue has not been snapshotted previously so the next snapshot in the timezone is required.")]
        InitialSnapshot = 1,
        [Description("There are snapshots outstanding between the last snapshot and the current date/time in the queues timezone.")]
        FromLastSnapshot = 2,
        [Description("There are no snapshots outstanding between the last snapshot and the current date/time in the queues timezone.")]
        NoRefresh = 3,
        [Description("The last snapshot id of the queue is the last configured snapshot in the cycle, so the cycle needs restarting.")]
        CycleRestart = 4,
    }
}