Imports BluePrism.Scheduling

''' Project  : AutomateAppCore
''' Class    : clsScheduleLogger
''' <summary>
''' Class to represent a schedule logger - this can be used to record
''' a schedule log as it progresses.
''' </summary>
Public Class ScheduleLogger
    Implements ISessionRunnerScheduleLogger

    ' The start time for this log
    Private mStartTime As Date

    ' The end time for this log
    Private mEndTime As Date

    ' The last time that this logger was pulsed
    Private mLastPulse As Date

    ' The instance which initiated the schedule, and thus this log.
    Private mInstance As ITriggerInstance

    ' The ID of this schedule log on the database
    Private mScheduleLogId As Integer

    ''' <summary>
    ''' Creates a new schedule log with the given ID, created as a result of
    ''' the given trigger instance.
    ''' </summary>
    ''' <param name="logId">The schedule log ID on the database, which uniquely
    ''' identifies this log.</param>
    ''' <param name="instance">The trigger instance which caused this log to
    ''' be created.</param>
    Friend Sub New(ByVal logId As Integer, ByVal instance As ITriggerInstance)
        mScheduleLogId = logId
        mInstance = instance
        mStartTime = Date.MaxValue
        mEndTime = Date.MaxValue
    End Sub

    ''' <summary>
    ''' Logs the given log entry to the registered schedule log.
    ''' </summary>
    ''' <param name="entry">The log entry to write to the schedule log.</param>
    ''' <exception cref="ArgumentNullException">If the given log entry was null
    ''' </exception>
    ''' <exception cref="UnassignedItemException">If this log has no schedule
    ''' associated with it.</exception>
    Public Sub LogEvent(ByVal entry As ScheduleLogEntry) _
     Implements ISessionRunnerScheduleLogger.LogEvent

        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))

        Dim sched As SessionRunnerSchedule = DirectCast(Me.Schedule, SessionRunnerSchedule)
        If sched Is Nothing Then
            Throw New UnassignedItemException(My.Resources.ScheduleLogger_NoScheduleToLogTheEventAgainst)
        End If

        gSv.SchedulerAddLogEntry(mScheduleLogId, entry)

    End Sub

    ''' <summary>
    ''' The name of the scheduler which created and executed the schedule referred to
    ''' by this log, or an empty string if that name is not known.
    ''' </summary>
    Public ReadOnly Property SchedulerName As String _
     Implements IScheduleLog.SchedulerName
        Get
            Return mInstance.Trigger.Schedule.Owner.Name
        End Get
    End Property

    ''' <summary>
    ''' The start time of this log if it is set - Date.MaxValue if it is not
    ''' </summary>
    Public ReadOnly Property StartTime() As Date Implements IScheduleLog.StartTime
        Get
            Return mStartTime
        End Get
    End Property

    ''' <summary>
    ''' The end time of this log if it is set - Date.MaxValue if not.
    ''' </summary>
    Public ReadOnly Property EndTime() As Date Implements IScheduleLog.EndTime
        Get
            Return mEndTime
        End Get
    End Property

    ''' <summary>
    ''' The time of the trigger instance for which this log was created.
    ''' </summary>
    Public ReadOnly Property InstanceTime() As Date Implements IScheduleLog.InstanceTime
        Get
            Return mInstance.When
        End Get
    End Property

    ''' <summary>
    ''' Checks when this log was last updated - pretty much by definition, this
    ''' is the end date/time of the log, since there can be no updates after that
    ''' </summary>
    Public ReadOnly Property LastUpdated() As Date Implements IScheduleLog.LastUpdated
        Get
            ' The start date / time updates the last pulse time when the log is
            ' started / finished. Otherwise Pulse() updates it.
            Return mLastPulse
        End Get
    End Property

    ''' <summary>
    ''' The schedule to which this logger belongs
    ''' </summary>
    Public ReadOnly Property Schedule() As ISchedule Implements IScheduleLog.Schedule
        Get
            Return mInstance.Trigger.Schedule
        End Get
    End Property

    ''' <summary>
    ''' Starts this log
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If this log has already been
    ''' started or is a historical log, rather than an open one.</exception>
    Public Sub Start() Implements IScheduleLog.Start
        If mStartTime <> Date.MaxValue Then
            Throw New InvalidOperationException(My.Resources.ScheduleLogger_ThisLogIsAlreadyStarted)
        End If
        mStartTime = Date.Now
        mLastPulse = mStartTime ' treat this as a pulse
        LogEvent(New ScheduleLogEntry(ScheduleLogEventType.ScheduleStarted))
    End Sub

    ''' <summary>
    ''' Checks if this log has been stopped.
    ''' </summary>
    ''' <returns>True if this log has been marked as either completed or
    ''' terminated; false if it is unstarted or still in progress. </returns>
    Public Function IsFinished() As Boolean Implements IScheduleLog.IsFinished
        Return mEndTime <> Date.MaxValue
    End Function

    ''' <summary>
    ''' Sets the end time to the given date / time. This will throw an exception
    ''' if the log has already finished.
    ''' </summary>
    ''' <param name="time">The time to which the end time of this log should be
    ''' set.</param>
    ''' <exception cref="ScheduleFinishedException">If this log has already been
    ''' 'finished' according to the rules of <see cref="IsFinished"/></exception>
    Private Sub SetEndTime(ByVal time As Date)
        If IsFinished() Then Throw New ScheduleFinishedException(My.Resources.ScheduleLogger_ThisLogIsAlreadyStopped)
        mEndTime = time
        mLastPulse = time
    End Sub

    ''' <summary>
    ''' Marks this log as complete
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">If this log has already been
    ''' finished or is a historical log, rather than an open one.</exception>
    Public Sub Complete() Implements IScheduleLog.Complete
        SetEndTime(Date.Now)
        LogEvent(New ScheduleLogEntry(ScheduleLogEventType.ScheduleCompleted))
    End Sub

    ''' <summary>
    ''' Marks this log as terminated with the given reason.
    ''' </summary>
    ''' <param name="reason">The user-presentable reason why this log was
    '''  terminated.</param>
    ''' <exception cref="ScheduleFinishedException">If this log has already been
    ''' finished or is a historical log, rather than an open one.</exception>
    Public Sub Terminate(ByVal reason As String) Implements IScheduleLog.Terminate
        Terminate(reason, Nothing)
    End Sub

    ''' <summary>
    ''' Marks this log as terminated.
    ''' </summary>
    ''' <param name="reason">The user-readable reason why this log was 
    ''' terminated.</param>
    ''' <param name="ex">The exception which caused this log to terminate,
    ''' null if no exception is in the frame for it.</param>
    ''' <exception cref="ScheduleFinishedException">If this log has already been
    ''' finished or is a historical log, rather than an open one.</exception>
    Public Sub Terminate(ByVal reason As String, ByVal ex As Exception) _
     Implements IScheduleLog.Terminate
        SetEndTime(Date.Now)
        Dim trace As String = Nothing
        If ex IsNot Nothing Then trace = ex.ToString()
        LogEvent(New ScheduleLogEntry(ScheduleLogEventType.ScheduleTerminated, reason))
    End Sub

    ''' <summary>
    ''' Pulses this log indicating that it is still running from the scheduler's
    ''' point of view - this just updates the latest heartbeat time for this log
    ''' so that observers can see that the schedule is still executing.
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">If this log has already been
    ''' marked as finished.</exception>
    Public Sub Pulse() Implements IScheduleLog.Pulse
        If IsFinished() Then Throw New ScheduleFinishedException(
         My.Resources.ScheduleLogger_TheScheduleInstance0HasAlreadyFinished, mInstance)
        mLastPulse = Date.Now
        Try
            gSv.SchedulerPulseLog(mScheduleLogId, SchedulerName)
        Catch
            ' Ignore any errors which might occur while attempting to pulse the log
            ' There's very little we can do about them anyway...
        End Try
    End Sub

End Class
