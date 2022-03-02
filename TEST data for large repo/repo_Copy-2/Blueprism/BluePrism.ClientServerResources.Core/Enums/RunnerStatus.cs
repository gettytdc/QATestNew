using System;

namespace BluePrism.ClientServerResources.Core.Enums
{
    [Flags]
    public enum RunnerStatus
    {
        UNKNOWN = -1,
        PENDING = 0,
        RUNNING,
        FAILED,
        STOPPED,
        STOPPING,
        COMPLETED,
        STARTFAILED,
        IDLE
    }
}
