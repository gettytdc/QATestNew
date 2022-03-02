Imports System.ComponentModel
''' <summary>
''' The different statuses that a session can be in.
''' </summary>
Public Enum SessionStatus
    All = -1
    Pending = 0
    Running = 1
    Failed = 2
    Terminated = Failed
    Stopped = 3
    Completed = 4
    Debugging = 5
    Archived = 6
    <Description("Stopping")>
    StopRequested = 7
    <Description("Warning")>
    Stalled = 8
End Enum


