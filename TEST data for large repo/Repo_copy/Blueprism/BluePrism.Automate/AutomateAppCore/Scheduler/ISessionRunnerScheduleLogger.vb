Imports BluePrism.Scheduling

''' Project  : AutomateAppCore
''' Class    : ISessionRunnerScheduleLogger
''' <summary>
''' Interface describing the event logging required for a session runner schedule.
''' This extends IScheduleLog by adding a single method to log an event,
''' describing the event type, task and session involved (the latter 2 optionally)
''' </summary>
''' <remarks>This interface is used within the scheduler code proper - <see 
''' cref="ScheduleLogger"/>- as well as the unit test classes in 
''' AutomateAppCore/_UnitTests.</remarks>
Public Interface ISessionRunnerScheduleLogger
    Inherits IScheduleLog

    ''' <summary>
    ''' Logs the given event for the current time
    ''' </summary>
    ''' <param name="entry">The entry to log</param>
    Sub LogEvent(ByVal entry As ScheduleLogEntry)

End Interface
