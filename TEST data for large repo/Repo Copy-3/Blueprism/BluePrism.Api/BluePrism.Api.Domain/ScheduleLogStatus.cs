namespace BluePrism.Api.Domain
{
    public enum ScheduleLogStatus
    {
        All = -2,
        Unknown = -1,
        Pending = 0,
        Running = 1,
        Exceptioned = 2,
        Failed = 2,
        Terminated = 2,
        Stopped = 3,
        Completed = 4,
        Debugging = 5,
        Deferred = 6,
        Locked = 7,
        Queried = 8,
        PartExceptioned = 9,
    }
}
