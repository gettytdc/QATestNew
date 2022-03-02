Imports BluePrism.Scheduling
Imports BluePrism.BPCoreLib.Diary
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateAppCore
''' Class    : clsScheduleTimetableInstance
''' <summary>
''' Utility class to represent a schedule instance in a timetable.
''' This is here just so that the same interface can be used for both timetables
''' and reports of a schedulers timed activities.
''' </summary>
Public Class ScheduleTimetableInstance
    Implements IScheduleInstance

#Region "Member vars"

    ' The schedule that this is an instance of
    Private mSchedule As ISchedule

    ' The date/time that this instance should execute
    Private mInstanceTime As Date

    ' The reason for the schedule instance
    Private mReason As TriggerActivationReason

#End Region

#Region "Constructors and static helper"

    ''' <summary>
    ''' Estimates the activation reason for the given trigger instance.
    ''' It assumes 'Execute' on a Fire trigger; 'Suppress' on a suppress trigger
    ''' and 'Indeterminate' on anything else.
    ''' </summary>
    ''' <param name="inst">The instance for which an activation reason is
    ''' required.</param>
    ''' <returns>The activation reason intuited from the given instance</returns>
    Private Shared Function GetActivationReason(ByVal inst As ITriggerInstance) _
     As TriggerActivationReason

        Select Case inst.Mode
            Case TriggerMode.Fire
                Return TriggerActivationReason.Execute
            Case TriggerMode.Suppress
                Return TriggerActivationReason.Suppress
            Case Else
                Return TriggerActivationReason.Indeterminate
        End Select

    End Function

    ''' <summary>
    ''' Creates a new timetable instance for the given schedule at the given
    ''' time.
    ''' </summary>
    ''' <param name="schedule">The schedule that this is an instance of.</param>
    ''' <param name="reason">The reason for this timetabled instance</param>
    ''' <param name="instanceTime">The point in time that the schedule should
    ''' execute.</param>
    Public Sub New(schedule As ISchedule, reason As TriggerActivationReason,
     instanceTime As Date)

        mSchedule = schedule
        mReason = reason
        mInstanceTime = instanceTime

    End Sub

    ''' <summary>
    ''' Creates a new timetable instance for the given trigger instance.
    ''' </summary>
    ''' <param name="inst">The instance for which a timetable instance is
    ''' required.</param>
    ''' <exception cref="NullReferenceException">If the given trigger instance
    ''' is null, or its trigger is null.</exception>
    Public Sub New(ByVal inst As ITriggerInstance)
        Me.New(inst.Trigger.Schedule, GetActivationReason(inst), inst.When)
    End Sub

    Public Sub New(instance As ITriggerInstance, clientInstanceTime As Date)
        Me.New(instance.Trigger.Schedule, GetActivationReason(instance), instance.When)
        mInstanceTime = clientInstanceTime
    End Sub

#End Region

#Region "IScheduleInstance/IDiaryEntry implementations"

    ''' <summary>
    ''' The schedule that this instance represents.
    ''' </summary>
    Public ReadOnly Property Schedule() As ISchedule Implements IScheduleInstance.Schedule
        Get
            Return mSchedule
        End Get
    End Property

    ''' <summary>
    ''' The title to display for this diary entry. In this case, this is the
    ''' schedule name.
    ''' </summary>
    Public ReadOnly Property Title() As String Implements IDiaryEntry.Title
        Get
            Return Schedule.Name
        End Get
    End Property

    ''' <summary>
    ''' The point in time when this instance should execute.
    ''' </summary>
    Public ReadOnly Property InstanceTime() As Date _
     Implements IScheduleInstance.InstanceTime, IDiaryEntry.Time
        Get
            Return mInstanceTime
        End Get
    End Property

    ''' <summary>
    ''' Compares this instance to the given instance.
    ''' </summary>
    ''' <param name="other">The schedule instance to compare this instance
    ''' to.</param>
    ''' <returns>A negative integer, zero or a positive integer if this instance
    ''' occurs before, the same time as, or after the given instance.</returns>
    Public Function CompareTo(ByVal other As IScheduleInstance) As Integer _
     Implements IComparable(Of IScheduleInstance).CompareTo

        If Me.InstanceTime < other.InstanceTime Then
            Return -1
        ElseIf Me.InstanceTime > other.InstanceTime Then
            Return 1
        End If
        ' Set to run at the same time - just go in (case-sensitive) alpha order
        Return String.Compare(mSchedule.Name, other.Schedule.Name)
    End Function

    ''' <summary>
    ''' The reason that this log was created.
    ''' Note that anything other than TriggerActivationReason.Execute implies
    ''' that the schedule was not executed, or at least, its execution was not
    ''' recorded by this log, and thus GetEntries() will return an empty list.
    ''' </summary>
    Public ReadOnly Property ActivationReason() As TriggerActivationReason _
     Implements IScheduleInstance.ActivationReason
        Get
            Return mReason
        End Get
    End Property

    ''' <summary>
    ''' The end time for the timetable instance. This will always be
    ''' Date.MaxValue, since a timetable instance represents an instance of
    ''' the schedule which is scheduled to run, not which has already run.
    ''' </summary>
    Public ReadOnly Property EndTime() As Date Implements IScheduleInstance.EndTime
        Get
            Return Date.MaxValue
        End Get
    End Property

    ''' <summary>
    ''' The log entries for the timetable instance. This will always be
    ''' an empty collection, since a timetable instance represents an instance
    ''' of the schedule which is scheduled to run, not which has already run.
    ''' </summary>
    Public ReadOnly Property LogEntries() As ICollection(Of ScheduleLogEntry) _
     Implements IScheduleInstance.LogEntries
        Get
            Return New List(Of ScheduleLogEntry)
        End Get
    End Property

    ''' <summary>
    ''' The entries in this log as a collection compound task log entries. This
    ''' will always be an empty collection, since a timetable instance represents
    ''' an instance of the schedule which is scheduled to run, not which has
    ''' already run.
    ''' </summary>
    Public ReadOnly Property CompoundLogEntries() As ICollection(Of TaskCompoundLogEntry) _
     Implements IScheduleInstance.CompoundLogEntries
        Get
            Return New List(Of TaskCompoundLogEntry)
        End Get
    End Property

    ''' <summary>
    ''' The actual start time for the timetable instance. This will always be
    ''' an Date.MinValue, since a timetable instance represents an instance
    ''' of the schedule which is scheduled to run, not which has already run.
    ''' </summary>
    Public ReadOnly Property StartTime() As Date Implements IScheduleInstance.StartTime
        Get
            Return Date.MinValue
        End Get
    End Property

    ''' <summary>
    ''' The status for the timetable instance. This will always be
    ''' Pending, since a timetable instance represents an instance
    ''' of the schedule which is scheduled to run, not which has already run.
    ''' </summary>
    Public ReadOnly Property Status() As ItemStatus _
     Implements IScheduleInstance.Status
        Get
            If InstanceTime < Now Then
                Return ItemStatus.All
            End If
            Return ItemStatus.Pending
        End Get
    End Property

#End Region

End Class
