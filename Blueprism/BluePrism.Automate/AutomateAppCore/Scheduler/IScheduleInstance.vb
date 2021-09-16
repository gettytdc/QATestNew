Imports BluePrism.Scheduling
Imports BluePrism.BPCoreLib.Diary
Imports BluePrism.Server.Domain.Models

#Region "Interface IScheduleInstance"

''' Project  : AutomateAppCore
''' Class    : IScheduleInstance
''' <summary>
''' Interface describing a single instance of a schedule, ie
''' the schedule its start and end time and the instance's status.
''' </summary>
Public Interface IScheduleInstance
    Inherits IComparable(Of IScheduleInstance)
    Inherits IDiaryEntry

    ''' <summary>
    ''' The schedule that this is an instance of.
    ''' </summary>
    ReadOnly Property Schedule() As ISchedule

    ''' <summary>
    ''' The reason for the activation of the trigger causing this instance.
    ''' This could be an activation which suppresses the execution of the
    ''' schedule, or it could be that it was skipped for another reason.
    ''' In normal running, this would simply be 'Execute' indicating that the
    ''' reason for activation was execution of the schedule.
    ''' </summary>
    ReadOnly Property ActivationReason() As TriggerActivationReason

    ''' <summary>
    ''' The current status of the instance.
    ''' </summary>
    ReadOnly Property Status() As ItemStatus

    ''' <summary>
    ''' The start time of the instance, or Date.MinValue if no start time
    ''' is set for this schedule instance (ie. if it has not been executed)
    ''' </summary>
    ReadOnly Property StartTime() As Date

    ''' <summary>
    ''' The end time of the instance, or Date.MaxValue if no end time
    ''' is set for this schedule instance (ie. it has not been executed
    ''' or it is still runnning)
    ''' </summary>
    ReadOnly Property EndTime() As Date

    ''' <summary>
    ''' The date that this instance is scheduled to run - this may differ
    ''' from the actual StartTime due to the nature of the scheduler.
    ''' This will always be set.
    ''' </summary>
    ReadOnly Property InstanceTime() As Date

    ''' <summary>
    ''' The log entries for this schedule instance, if there are any.
    ''' This will return an empty collection if there are no log entries for
    ''' this instance
    ''' </summary>
    ReadOnly Property LogEntries() As ICollection(Of ScheduleLogEntry)

    ''' <summary>
    ''' The entries in this log as a collection compound task log entries.
    ''' This provides a hierarchical view of the log entries represented by this
    ''' historical log.
    ''' Note that no schedule entry is provided in this log - only the task
    ''' entries and any session entries owned by those tasks.
    ''' </summary>
    ReadOnly Property CompoundLogEntries() As ICollection(Of TaskCompoundLogEntry)

End Interface

#End Region
