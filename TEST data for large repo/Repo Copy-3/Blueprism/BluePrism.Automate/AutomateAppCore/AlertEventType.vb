Imports System.Runtime.Serialization
''' <summary>
''' The types of events that can produce alerts.
''' </summary>
<Flags, DataContract([Namespace]:="bp")>
Public Enum AlertEventType

    <EnumMember> None = 0

    <EnumMember> Stage = 1

    <EnumMember> ProcessPending = 2
    <EnumMember> ProcessRunning = 4
    <EnumMember> ProcessStopped = 8
    <EnumMember> ProcessComplete = 16
    <EnumMember> ProcessFailed = 32

    <EnumMember> ScheduleStarted = 64
    <EnumMember> ScheduleCompleted = 128
    <EnumMember> ScheduleTerminated = 256

    <EnumMember> TaskStarted = 512
    <EnumMember> TaskCompleted = 1024
    <EnumMember> TaskTerminated = 2048

End Enum
