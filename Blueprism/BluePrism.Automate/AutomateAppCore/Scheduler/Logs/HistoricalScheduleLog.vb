Imports System.Runtime.Serialization
Imports BluePrism.Scheduling
Imports BluePrism.BPCoreLib.Diary
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

''' <summary>
''' A historical log for a session runner schedule.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp", Name:="hsl")>
Public Class HistoricalScheduleLog : Implements _
     IScheduleLog, ICollection(Of ScheduleLogEntry), IScheduleInstance, IDiaryEntry

#Region "Member Variables"

    ' The ID of this log
    <DataMember(Name:="id")>
    Private mId As Integer

    ' The name of the scheduler which created/executed the schedule
    <DataMember(Name:="sn")>
    Private mSchedulerName As String

    ' The reason this log was created
    <DataMember(Name:="re")>
    Private mReason As TriggerActivationReason

    ' the time of the instance for this log
    <DataMember(Name:="it")>
    Private mInstanceTime As Date

    ' The start time for this log
    <DataMember(Name:="st")>
    Private mStartTime As Date

    ' The end time for this log
    <DataMember(Name:="et")>
    Private mEndTime As Date

    ' Only has meaning if EndTime is set - indicates whether this logs an
    ' instance which completed (if True) or terminated (if False)
    <DataMember(Name:="s")>
    Private mSuccess As Boolean

    ' The entries which make up this log
    <DataMember(Name:="en")>
    Private mEntries As List(Of ScheduleLogEntry)

    ' The schedule which this log is for
    <NonSerialized()> Private mSchedule As ISchedule

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new historical log using data from the given provider. This expects
    ''' the provided data to contain:
    ''' <list>
    ''' <item>id: Int32: The ID Of the log.</item>
    ''' <item>schedulername: String: The name of the scheduler which created the log.
    ''' </item>
    ''' <item>firereason: TriggerActivationReason: The cause of the trigger being
    ''' activated.</item>
    ''' <item>instancetime: DateTime: The date/time of the schedule instance, not
    ''' necessarily the actual date/time that it executed, but the date/time that it
    ''' was <em>scheduled</em> to be executed.</item>
    ''' </list>
    ''' </summary>
    ''' <param name="prov">The provider which contains all the data required.</param>
    Public Sub New(prov As IDataProvider)
        Me.New(
            prov.GetInt("id"),
            prov.GetString("schedulername"),
            prov.GetValue("firereason", TriggerActivationReason.Execute),
            prov.GetValue("instancetime", DateTime.MinValue)
        )
    End Sub

    ''' <summary>
    ''' Creates a new historical session runner log - ie. one which is 
    ''' already complete and cannot be started or stopped
    ''' </summary>
    ''' <param name="id">The database ID of the log</param>
    ''' <param name="reason">The reason for the log being created.</param>
    ''' <param name="instanceTime">The time of the instance for this log.
    ''' </param>
    Public Sub New(
     id As Integer,
     schedName As String,
     reason As TriggerActivationReason,
     instanceTime As Date)
        mId = id
        mSchedulerName = schedName
        mInstanceTime = instanceTime
        mReason = reason
        mStartTime = Date.MinValue
        mEndTime = Date.MaxValue
    End Sub

#End Region

#Region "IScheduleLog & IScheduleInstance Properties"

    ''' <summary>
    ''' The status of this log - ie. the current state of the schedule
    ''' instance that it represents.
    ''' </summary>
    Public ReadOnly Property Status() As ItemStatus Implements IScheduleInstance.Status
        Get
            If mStartTime = Date.MinValue Then Return ItemStatus.Pending
            If mEndTime = Date.MaxValue Then Return ItemStatus.Running
            If mSuccess AndAlso mEntries.Any(Function(o) o.EntryType = ScheduleLogEventType.TaskTerminated ) Then Return ItemStatus.PartExceptioned
            If Not mSuccess AndAlso mEntries.Any(Function(o) o.EntryType = ScheduleLogEventType.TaskCompleted) Then Return ItemStatus.PartExceptioned
            If mSuccess Then Return ItemStatus.Completed
            Return ItemStatus.Terminated
        End Get
    End Property

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
    ''' The start time for this log
    ''' </summary>
    Public ReadOnly Property StartTime() As Date _
     Implements IScheduleLog.StartTime, IScheduleInstance.StartTime
        Get
            Return mStartTime
        End Get
    End Property

    ''' <summary>
    ''' The end time for this log
    ''' </summary>
    Public ReadOnly Property EndTime() As Date _
     Implements IScheduleLog.EndTime, IScheduleInstance.EndTime
        Get
            Return mEndTime
        End Get
    End Property

    ''' <summary>
    ''' The time of the trigger instance which initiated this log
    ''' </summary>
    Public Property InstanceTime() As Date _
     Implements IScheduleLog.InstanceTime, IScheduleInstance.InstanceTime, IDiaryEntry.Time
        Get
            Return mInstanceTime
        End Get
        Set(value As Date)
            mInstanceTime = value
        End Set
    End Property

    ''' <summary>
    ''' Checks when this log was last updated - pretty much by definition, this
    ''' is the end date/time of the log, since there can be no updates after that
    ''' </summary>
    Public ReadOnly Property LastUpdated() As Date Implements IScheduleLog.LastUpdated
        Get
            Return mEndTime
        End Get
    End Property

    ''' <summary>
    ''' The schedule that this log represents
    ''' </summary>
    Public Property Schedule() As ISchedule
        Get
            Return mSchedule
        End Get
        Friend Set(ByVal value As ISchedule)
            mSchedule = value
        End Set
    End Property

    ''' <summary>
    ''' Explicit implementation of the interface's Schedule property - needed because
    ''' you can't implement a property with a private setter because the signature
    ''' would be different to that in the interface. Still blech, but better than
    ''' a separate 'setter' method to go alongside the 'getter' property.
    ''' </summary>
    Private ReadOnly Property ExplicitSchedule() As ISchedule _
     Implements IScheduleLog.Schedule, IScheduleInstance.Schedule
        Get
            Return Me.Schedule
        End Get
    End Property

    ''' <summary>
    ''' Throws an exception - this log is historical and cannot be modified
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">When called - this log cannot
    ''' be started or stopped because it represents a log which is already 
    ''' finished.</exception>
    Public Sub Start() Implements IScheduleLog.Start
        Throw New ScheduleFinishedException(My.Resources.HistoricalScheduleLog_ThisLogIsHistoricalItCannotBeStarted)
    End Sub

    ''' <summary>
    ''' Throws an exception - this log is historical and cannot be modified
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">When called - this log cannot
    ''' be started or stopped because it represents a log which is already 
    ''' finished.</exception>
    Public Sub Complete() Implements IScheduleLog.Complete
        Throw New ScheduleFinishedException(My.Resources.HistoricalScheduleLog_ThisLogIsHistoricalItCannotBeCompleted)
    End Sub

    ''' <summary>
    ''' Throws an exception - this log is historical and cannot be modified
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">When called - this log cannot
    ''' be started or stopped because it represents a log which is already 
    ''' finished.</exception>
    Public Sub Terminate(ByVal reason As String) Implements IScheduleLog.Terminate
        Throw New ScheduleFinishedException(My.Resources.HistoricalScheduleLog_ThisLogIsHistoricalItCannotBeTerminated)
    End Sub

    ''' <summary>
    ''' Throws an exception - this log is historical and cannot be modified
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">When called - this log cannot
    ''' be started or stopped because it represents a log which is already 
    ''' finished.</exception>
    Public Sub Terminate(ByVal reason As String, ByVal ex As Exception) _
     Implements IScheduleLog.Terminate
        Throw New ScheduleFinishedException(My.Resources.HistoricalScheduleLog_ThisLogIsHistoricalItCannotBeTerminated)
    End Sub

    ''' <summary>
    ''' Throws an exception - this log is historical and cannot be modified
    ''' </summary>
    ''' <exception cref="ScheduleFinishedException">When called - this log cannot
    ''' be updated because it represents a log which is already finished.</exception>
    Public Sub Pulse() Implements IScheduleLog.Pulse
        Throw New ScheduleFinishedException(My.Resources.HistoricalScheduleLog_ThisLogIsHistoricalItCannotBePulsed)
    End Sub

    ''' <summary>
    ''' Checks if this log has finished - it is a historical log, thus it is
    ''' always finished.
    ''' </summary>
    ''' <returns>True to indicate that the log has finished.</returns>
    Public Function IsFinished() As Boolean Implements IScheduleLog.IsFinished
        Return True
    End Function

#End Region

    ''' <summary>
    ''' The database ID for this historical log, this is set at creation time.
    ''' </summary>
    Public ReadOnly Property Id() As Integer
        Get
            Return mId
        End Get
    End Property

    ''' <summary>
    ''' The name of the scheduler which created and executed the schedule referred to
    ''' by this log, or an empty string if that name is not known.
    ''' </summary>
    Public ReadOnly Property SchedulerName As String _
     Implements IScheduleLog.SchedulerName
        Get
            Return If(mSchedulerName, "")
        End Get
    End Property

    ''' <summary>
    ''' Internal function to create (if necessary) and return the list of log
    ''' entries that this log contains.
    ''' </summary>
    ''' <returns>A non-null list of schedule log entries that is contained within
    ''' this log.</returns>
    Private Function GetEntries() As IList(Of ScheduleLogEntry)
        If mEntries Is Nothing Then mEntries = New List(Of ScheduleLogEntry)
        Return mEntries
    End Function

    ''' <summary>
    ''' The log entries which make up this log
    ''' </summary>
    Private ReadOnly Property Entries() As ICollection(Of ScheduleLogEntry) _
     Implements IScheduleInstance.LogEntries
        Get
            Return Me
        End Get
    End Property

    ''' <summary>
    ''' The entries in this log as a collection compound task log entries.
    ''' This provides a hierarchical view of the log entries represented by this
    ''' historical log.
    ''' Note that no schedule entry is provided in this log - only the task
    ''' entries and any session entries owned by those tasks.
    ''' </summary>
    Public ReadOnly Property CompoundEntries() As ICollection(Of TaskCompoundLogEntry) _
     Implements IScheduleInstance.CompoundLogEntries
        Get
            Dim taskNames As New Dictionary(Of Integer, String)

            Dim completedTasks As New clsSortedSet(Of TaskCompoundLogEntry)
            Dim currentTask As TaskCompoundLogEntry = Nothing

            ' For each log entry 
            For Each ent As ScheduleLogEntry In Me

                Select Case ent.EntryType

                    Case ScheduleLogEventType.TaskStarted
                        currentTask = New TaskCompoundLogEntry()
                        currentTask.StartDate = ent.EntryTime
                        Dim name As String = Nothing
                        If Not taskNames.TryGetValue(ent.TaskId, name) Then
                            name = gSv.SchedulerGetTaskNameFromID(ent.TaskId)
                            taskNames(ent.TaskId) = name
                        End If
                        currentTask.Name = name

                    Case ScheduleLogEventType.TaskTerminated, ScheduleLogEventType.TaskCompleted
                        currentTask.EndDate = ent.EntryTime
                        currentTask.TerminationReason = ent.TerminationReason
                        completedTasks.Add(currentTask)
                        currentTask = Nothing

                    Case ScheduleLogEventType.SessionFailedToStart
                        currentTask.SessionCreationFailedMessages.Add(
                         New KeyValuePair(Of Date, String)(ent.EntryTime, ent.TerminationReason))

                    Case ScheduleLogEventType.SessionStarted
                        Dim sess = gSv.GetSessionDetails(ent.SessionLogNo)
                        With currentTask.Sessions(ent.SessionLogNo)
                            .StartDate = ent.EntryTime
                            .SessionNo = ent.SessionLogNo
                            .Name = sess?.ProcessName
                            .ResourceName = sess?.ResourceName
                        End With

                    Case ScheduleLogEventType.SessionTerminated,
                     ScheduleLogEventType.SessionCompleted
                        With currentTask.Sessions(ent.SessionLogNo)
                            .EndDate = ent.EntryTime
                            .TerminationReason = ent.TerminationReason
                        End With
                End Select
            Next
            ' If we have a dangling task (task was not ended correctly / still running / etc)
            ' ensure it's there in our log.
            If currentTask IsNot Nothing Then completedTasks.Add(currentTask)

            Return completedTasks
        End Get
    End Property

    ''' <summary>
    ''' Adds the given entry to this log
    ''' </summary>
    ''' <param name="entry">The entry to add to the log</param>
    Public Sub Add(ByVal entry As ScheduleLogEntry) _
     Implements ICollection(Of ScheduleLogEntry).Add

        Select Case entry.EntryType

            Case ScheduleLogEventType.ScheduleStarted
                mStartTime = entry.EntryTime

            Case ScheduleLogEventType.ScheduleCompleted
                mEndTime = entry.EntryTime
                mSuccess = True

            Case ScheduleLogEventType.ScheduleTerminated
                mEndTime = entry.EntryTime
                mSuccess = False

        End Select
        GetEntries().Add(entry)

    End Sub

    ''' <summary>
    ''' Clears the entries in this log
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of ScheduleLogEntry).Clear
        GetEntries().Clear()
        mStartTime = DateTime.MinValue
        mEndTime = DateTime.MaxValue
    End Sub

    ''' <summary>
    ''' Checks if this log contains the given entry
    ''' </summary>
    ''' <param name="entry">The entry to check for within this log</param>
    ''' <returns>True if this log contains the given entry;
    ''' false otherwise.</returns>
    Public Function Contains(ByVal entry As ScheduleLogEntry) As Boolean _
     Implements ICollection(Of ScheduleLogEntry).Contains

        Return GetEntries().Contains(entry)

    End Function

    ''' <summary>
    ''' Copies the entries in this log to the given array.
    ''' </summary>
    ''' <param name="array">The array into which this log should be copied.</param>
    ''' <param name="arrayIndex">The index at which the entries should begin to
    ''' be copied.</param>
    Private Sub CopyTo(ByVal array() As ScheduleLogEntry, ByVal arrayIndex As Integer) _
     Implements ICollection(Of ScheduleLogEntry).CopyTo

        GetEntries().CopyTo(array, arrayIndex)

    End Sub

    ''' <summary>
    ''' Gets the number of entries in this log.
    ''' </summary>
    Public ReadOnly Property Count() As Integer Implements ICollection(Of ScheduleLogEntry).Count
        Get
            Return GetEntries().Count
        End Get
    End Property

    ''' <summary>
    ''' Checks if this log is read-only or not.
    ''' </summary>
    Private ReadOnly Property IsReadOnly() As Boolean _
     Implements ICollection(Of ScheduleLogEntry).IsReadOnly
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Removes the given entry from this log
    ''' </summary>
    ''' <param name="entry">The item to remove</param>
    ''' <returns>True if the given entry was found and removed; false if it was
    ''' not found.</returns>
    Public Function Remove(ByVal entry As ScheduleLogEntry) As Boolean _
     Implements ICollection(Of ScheduleLogEntry).Remove

        Return GetEntries().Remove(entry)

    End Function

    ''' <summary>
    ''' Gets an enumerator over the entries in this log.
    ''' </summary>
    ''' <returns>An enumerator over the entries in this log.</returns>
    Public Function GetEnumerator() As IEnumerator(Of ScheduleLogEntry) _
     Implements IEnumerable(Of ScheduleLogEntry).GetEnumerator

        Return GetEntries().GetEnumerator()

    End Function

    ''' <summary>
    ''' Gets an enumerator over the entries in this log.
    ''' </summary>
    ''' <returns>An enumerator over the entries in this log.</returns>
    Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function


    ''' <summary>
    ''' Compares this log to the given schedule instance.
    ''' </summary>
    ''' <param name="other">The other log to compare against.</param>
    ''' <returns>A negative number, zero or a positive number depending on
    ''' whether this instance was scheduled before, at the same time, or after
    ''' the provided instance.</returns>
    Public Function CompareTo(ByVal other As IScheduleInstance) As Integer _
     Implements System.IComparable(Of IScheduleInstance).CompareTo

        Dim db As Double = (Me.StartTime - other.StartTime).TotalSeconds
        If db < 0 Then
            Return -1
        ElseIf db > 0 Then
            Return 1
        End If
        ' Started at exactly the same time - let's go by ID - the higher the
        ' number, the later it ran (by definition, seeing as it's an IDENTITY field)
        Dim histLog As HistoricalScheduleLog = TryCast(other, HistoricalScheduleLog)
        If histLog Is Nothing Then Return 0 Else Return mId - histLog.mId

    End Function

    ''' <summary>
    ''' The title for this diary entry - in this case, the schedule name.
    ''' </summary>
    Public ReadOnly Property Title() As String Implements IDiaryEntry.Title
        Get
            Return Me.Schedule.Name
        End Get
    End Property
End Class
