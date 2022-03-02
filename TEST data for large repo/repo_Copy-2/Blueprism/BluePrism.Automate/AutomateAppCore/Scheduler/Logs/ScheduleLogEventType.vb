''' Project  : AutomateAppCore
''' Class    : ScheduleLogEventType [Enum]
''' <summary>
''' Enumeration of the event types which are supported in
''' the schedule log.
''' </summary>
Public Enum ScheduleLogEventType

    ScheduleStarted = 0
    ScheduleCompleted = 1
    ScheduleTerminated = 2

    TaskStarted = 3
    TaskCompleted = 4
    TaskTerminated = 5

    SessionStarted = 6
    SessionCompleted = 7
    SessionTerminated = 8
    SessionFailedToStart = 9

End Enum
