namespace BluePrism.Api.Domain
{
    public enum SessionStatus
    {
        Pending = 0,
        Running = 1,
        Failed = 2,
        Terminated = Failed,
        Stopped = 3,
        Completed = 4,
        Debugging = 5,
        Archived = 6,
        Stopping = 7,
        Warning = 8
    }
}
