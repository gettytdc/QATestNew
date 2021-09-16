Imports System.Runtime.Serialization
Imports System.Threading
Imports System.Xml
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.ScheduleData
Imports BluePrism.Scheduling.Triggers
Imports BluePrism.Server.Domain.Models
Imports NLog
Imports SessionData = BluePrism.AutomateAppCore.clsServer.SessionData

''' Project  : AutomateAppCore
''' Class    : clsSessionRunnerSchedule
''' <summary>
''' A database-backed Schedule which contains a binary tree of tasks which are
''' executed in sequence the schedule is activated. 
''' </summary>
<Serializable>
<DataContract([Namespace]:="bp", IsReference:=True)>
<KnownType(GetType(clsSet(Of ITrigger)))>
<KnownType(GetType(clsSet(Of ScheduledTask)))>
Public Class SessionRunnerSchedule
    Inherits BaseSchedule
    Implements IEquatable(Of SessionRunnerSchedule)
    Implements ICollection(Of ScheduledTask)

#Region "AuditEvent handling"
    ''' <summary>
    ''' Set of the tasks which have been modified. This is purely an optimisation
    ''' for checking to see if another task modify audit record is required.
    ''' </summary>
    Private mModifiedTasks As IBPSet(Of Integer)

    <NonSerialized>
    Private ReadOnly mLog As Logger = LogManager.GetCurrentClassLogger()

    ''' <summary>
    ''' Adds the given audit event to the list of events currently held in this
    ''' schedule. This will be a bit intelligent in that :-
    ''' <list>
    ''' <item>If the event is a task removal and the task in question is not yet
    ''' on the database, it will seek out the corresponding task creation event
    ''' and remove it and skip the addition of the now redundant delete event.
    ''' </item>
    ''' <item>If the event is a task modification and the task in question is
    ''' not yet on the database, it will skip the event, assuming that the
    ''' create event will cover the modification too.</item>
    ''' <item>The same applies to sessions being added or removed - if the task
    ''' is not yet on the database, it leaves the audit info to the task created
    ''' event.</item>
    ''' <item>If a session is being removed which has a corresponding create
    ''' audit event, then the create is discarded and the remove is skipped.
    ''' </item>
    ''' <item>If a task modification event is added for a task which already
    ''' has a task modification event, it is discarded.</item>
    ''' <item>If a task removal event is added, any other events pertaining to that
    ''' task are discarded.</item>
    ''' </list>
    ''' </summary>
    ''' <param name="evt">The event to add to the maintained list of audit events
    ''' that have occurred to this schedule.</param>
    ''' <remarks></remarks>
    Public Sub AddAuditEvent(evt As ScheduleAuditEvent)
        ' If we're inhibiting audit events, do nothing
        If mInhibitAuditEvents Then Return

        Dim events As IList(Of ScheduleAuditEvent) = GetAuditEvents()
        Select Case evt.Code

            Case ScheduleEventCode.TaskRemoved
                If evt.TaskId < 0 Then
                    ' It's a temp task - find the corresponding create event and
                    ' remove it.
                    For i As Integer = events.Count - 1 To 0 Step -1
                        Dim e As ScheduleAuditEvent = events(i)
                        If e.Code = ScheduleEventCode.TaskAdded AndAlso
                         e.TaskId = evt.TaskId Then
                            events.RemoveAt(i)
                            ' Once create is removed, there's no point in keeping the
                            ' delete task, since it never really existed.
                            Return
                        End If
                    Next
                End If

            Case ScheduleEventCode.TaskModified

                ' It's a temp task - the 'create' event is enough.
                If evt.TaskId < 0 Then
                    Return
                End If

                ' If there's already a taskmodified event for this task, skip it
                If Not GetModifiedTasks().Add(evt.TaskId) Then
                    Return
                End If

            Case ScheduleEventCode.TaskRemoved
                ' Remove any other events to do with the task... kinda pointless really
                For i As Integer = events.Count - 1 To 0 Step -1
                    If events(i).TaskId = evt.TaskId Then
                        events.RemoveAt(i)
                    End If
                Next

            Case ScheduleEventCode.SessionAdded
                ' It's a temp task - the 'create' event is enough.
                If evt.TaskId < 0 Then
                    Return
                End If

            Case ScheduleEventCode.SessionRemoved
                ' It's a temp task - the 'create' event is enough.
                If evt.TaskId < 0 Then
                    Return
                End If

                ' If there's a sessionadded with the same values, just remove
                ' that and skip this one - they amount to the same thing
                Dim sess As ScheduledSession = evt.ScheduledSession
                For i As Integer = events.Count - 1 To 0 Step -1
                    Dim e As ScheduleAuditEvent = events(i)
                    If e.Code = ScheduleEventCode.SessionAdded AndAlso
                     e.ScheduledSession IsNot Nothing Then

                        Dim existing As ScheduledSession = e.ScheduledSession

                        If sess.ResourceId = existing.ResourceId AndAlso
                         sess.ProcessId = existing.ProcessId Then
                            events.RemoveAt(i)
                            Return
                        End If

                    End If
                Next

        End Select
        GetAuditEvents().Add(evt)
    End Sub

    ''' <summary>
    ''' Gets (or creates) the list of audit events held for this schedule.
    ''' </summary>
    ''' <returns>A non-null list of audit events which are being held within this
    ''' schedule.</returns>
    Friend Function GetAuditEvents() As IList(Of ScheduleAuditEvent)
        If mEvents Is Nothing Then
            mEvents = New List(Of ScheduleAuditEvent)
        End If
        Return mEvents
    End Function

    ''' <summary>
    ''' Gets or creates the set of modified tasks held in this schedule.
    ''' </summary>
    ''' <returns>The set of task Ids which represent the tasks which have 'task
    ''' modified' audit records saved within this schedule.</returns>
    Private Function GetModifiedTasks() As IBPSet(Of Integer)
        If mModifiedTasks Is Nothing Then
            mModifiedTasks = New clsSet(Of Integer)
        End If
        Return mModifiedTasks
    End Function

    ''' <summary>
    ''' Gets the list of audit events held in this schedule, resetting the list
    ''' in this schedule as a result.
    ''' </summary>
    ''' <returns>The list of audit events currently held within this schedule.
    ''' </returns>
    Friend Function GetAndResetAuditEvents() As IList(Of ScheduleAuditEvent)
        Dim l As IList(Of ScheduleAuditEvent) = GetAuditEvents()
        mEvents = Nothing
        mModifiedTasks = Nothing
        Return l
    End Function

    ''' <summary>
    ''' Updates any of the task IDs in the audit records within this scheduler,
    ''' replacing any instances of the old ID with the new ID.
    ''' </summary>
    ''' <param name="oldId">The old task ID to update</param>
    ''' <param name="newId">The new task ID to update it with</param>
    Friend Sub UpdateTaskIds(oldId As Integer, newId As Integer)
        For Each e As ScheduleAuditEvent In GetAuditEvents()
            If e.TaskId = oldId Then e.TaskId = newId
        Next
    End Sub

    ''' <summary>
    ''' Updates the task IDs in this schedule from the task IDs in the given schedule
    ''' </summary>
    ''' <param name="fromSched">The schedule from which to transfer the task IDs from
    ''' </param>
    ''' <remarks>This is designed for use for synchronising IDs on a schedule which
    ''' has been updated by database code. As such, the given schedule is expected to
    ''' be the same as this schedule except for task IDs.</remarks>
    ''' <exception cref="ArgumentNullException">If the given schedule is null
    ''' </exception>
    ''' <exception cref="InvalidValueException">If the ID of the given schedule does
    ''' not match the ID of this schedule, or if the tasks don't match.</exception>
    Friend Sub SyncTaskIds(fromSched As SessionRunnerSchedule)
        ' If this is the same schedule (by reference), we can skip all of this
        If fromSched Is Me Then Return

        ' Sanity check the argument
        If fromSched Is Nothing Then Throw New ArgumentNullException(NameOf(fromSched))
        If fromSched.Id <> mId Then Throw New InvalidValueException(
         "Schedule with ID {0} cannot be synced with the schedule with ID {1}",
         mId, fromSched.Id)
        If fromSched.Count <> Count Then Throw New InvalidValueException(
         "Schedule with {0} tasks can't be synced using a schedule with {1} tasks",
         fromSched.Count, Count)

        ' Get the tasks into a map by name
        Dim map As New Dictionary(Of String, ScheduledTask)
        For Each t As ScheduledTask In mTasks : map(t.Name) = t : Next t

        ' Go through the tasks in the schedule, get the one in this schedule with
        ' the same name and update the ID in this schedule
        For Each t As ScheduledTask In fromSched
            Dim curr As ScheduledTask = map(t.Name)
            ' Check the initial task - update it if that's the task we're changing
            If mInitTaskId = curr.Id Then mInitTaskId = t.Id
            curr.Id = t.Id
        Next t

        ' And briefly, clear and replace the task set - changing the ID changes the
        ' hash code of the task, and clsSet works on hash code, so a clear and re-add
        ' should refresh the internal data structure so that it falls in line.
        mTasks.Clear()
        mTasks.Union(map.Values)

    End Sub

    ''' <summary>
    ''' Handles any data changes in either the schedule or the task.
    ''' 
    ''' This is primarily responsible for maintaining the audit records in this
    ''' schedule, and maintaining references to itself - ie. adding itself as a
    ''' data change listener to any tasks which are added, removing itself from
    ''' those which are removed.
    ''' </summary>
    ''' <param name="sender">The source of the event, ie. this schedule if that
    ''' is what has changed data, or the relevant task if that is the source of
    ''' the data change.</param>
    ''' <param name="e">The event arguments detailing the data change.</param>
    Private Sub HandleDataChange(sender As Object, e As DataChangeEventArgs)
        Select Case e.Name

            Case "TaskAdded"
                Dim task = DirectCast(e.NewValue, ScheduledTask)
                AddHandler task.DataChanged, AddressOf HandleDataChange
                If Not mInhibitAuditEvents Then AddAuditEvent(
                    If(mUser Is Nothing,
                        New ScheduleAuditEvent(
                            ScheduleEventCode.TaskAdded,
                            Id,
                            task.Id,
                            Nothing,
                            Nothing),
                        New ScheduleAuditEvent(
                            ScheduleEventCode.TaskAdded,
                            mUser,
                            Id,
                            task.Id,
                            Nothing,
                            Nothing)))

            Case "TaskRemoved"
                Dim task = DirectCast(e.OldValue, ScheduledTask)
                RemoveHandler task.DataChanged, AddressOf HandleDataChange
                If Not mInhibitAuditEvents Then AddAuditEvent(
                    If(mUser Is Nothing,
                    New ScheduleAuditEvent(
                        ScheduleEventCode.TaskRemoved,
                        Id,
                        DirectCast(e.OldValue, ScheduledTask).Id,
                        Nothing,
                        Nothing),
                    New ScheduleAuditEvent(
                        ScheduleEventCode.TaskRemoved,
                        mUser,
                        Id,
                        DirectCast(e.OldValue, ScheduledTask).Id,
                        Nothing,
                        Nothing)))

            Case "SessionAdded"
                If Not mInhibitAuditEvents Then AddAuditEvent(
                    If(mUser Is Nothing,
                    New ScheduleAuditEvent(
                        ScheduleEventCode.SessionAdded,
                        Id,
                        DirectCast(sender, ScheduledTask).Id,
                        DirectCast(e.NewValue, ScheduledSession),
                        Nothing),
                    New ScheduleAuditEvent(
                        ScheduleEventCode.SessionAdded,
                        mUser,
                        Id,
                        DirectCast(sender, ScheduledTask).Id,
                        DirectCast(e.NewValue, ScheduledSession),
                        Nothing)))

            Case "SessionRemoved"
                If Not mInhibitAuditEvents Then AddAuditEvent(
                    If(mUser Is Nothing,
                    New ScheduleAuditEvent(
                        ScheduleEventCode.SessionRemoved,
                        Id,
                        DirectCast(sender, ScheduledTask).Id,
                        DirectCast(e.OldValue, ScheduledSession),
                        Nothing),
                    New ScheduleAuditEvent(
                        ScheduleEventCode.SessionRemoved,
                        mUser,
                        Id,
                        DirectCast(sender, ScheduledTask).Id,
                        DirectCast(e.OldValue, ScheduledSession),
                        Nothing)))
        End Select

    End Sub

    ''' <summary>
    ''' Class to aid in inhibiting audit events; when retrieved and used in a Using
    ''' block, this just ensures that audit events are resumed when the inhibitor
    ''' exits the using block.
    ''' </summary>
    Private Class AuditEventInhibitor : Implements IDisposable

        ' The owning schedule
        Private mOwner As SessionRunnerSchedule

        ''' <summary>
        ''' Creates a new inhibitor for the given schedule
        ''' </summary>
        ''' <param name="sched">The schedule to inhibit audit events on</param>
        Public Sub New(sched As SessionRunnerSchedule)
            mOwner = sched
            sched.mInhibitAuditEvents = True
        End Sub

        ''' <summary>
        ''' Disposes of this inhibitor, releasing the lock on audit events within
        ''' the owning schedule
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dim sched As SessionRunnerSchedule = mOwner
            mOwner = Nothing
            If sched IsNot Nothing Then
                sched.mInhibitAuditEvents = False
                GC.SuppressFinalize(Me)
            End If
        End Sub

        ''' <summary>
        ''' Ensures that the lock on audit events is released in the owning schedule
        ''' if this inhibitor is garbage collected.
        ''' </summary>
        Protected Overrides Sub Finalize()
            Dispose()
        End Sub

    End Class

    ''' <summary>
    ''' Gets an inhibitor on audit event creation within this schedule. On calling
    ''' <see cref="IDisposable.Dispose"/> on the returned object, audit event
    ''' creation will be resumed on this schedule.
    ''' </summary>
    ''' <returns>An IDisposable which inhibits audit events on this schedule.
    ''' </returns>
    Friend Function GetAuditEventInhibitor() As IDisposable
        Return New AuditEventInhibitor(Me)
    End Function

#End Region

#Region "Member variables"

    ''' <summary>
    ''' The list of audit events which have occurred within this schedule.
    ''' </summary>
    <DataMember>
    Private mEvents As IList(Of ScheduleAuditEvent)

    ' The ID of the task within this schedule which acts as the initial
    ' task - ie. is the first one run when this schedule is executed.
    <DataMember>
    Private mInitTaskId As Integer

    ' The set of tasks which form this schedule
    <DataMember>
    Private mTasks As IBPSet(Of ScheduledTask)

    ' The ID of this schedule
    <DataMember>
    Private mId As Integer

    ' The version number of this schedule
    <DataMember>
    Private mVersion As Integer

    ' Lock object used when aborting / resetting the abort status of this schedule
    <DataMember>
    Private mAbortLock As Object = New Object()

    ' Message to both indicate that this schedules execution is being aborted
    ' and provide the reason why the abort was requested.
    <DataMember>
    Private mAbortMessage As String

    ' Flag set to indicate that the schedule is currently executing
    <DataMember>
    Private mExecuting As Boolean

    ' Flag indicating if this schedule is cuirrently retired.
    <DataMember>
    Private mRetired As Boolean

    ' Flag to inhibit the creation of audit events in this schedule
    <DataMember>
    Private mInhibitAuditEvents As Boolean

    <DataMember>
    Private mUser As IUser

#End Region

#Region "SessionRunnerSchedule Properties & methods"

    ''' <summary>
    ''' Gets the resource connection manager owned by the scheduler which owns
    ''' this scheduler, or null if the scheduler has no resource connection
    ''' manager.
    ''' </summary>
    Friend ReadOnly Property ResourceConnectionManager As IResourceConnectionManager
        Get
            Dim scheduler = TryCast(Owner, AutomateScheduler)
            If scheduler Is Nothing Then Return Nothing
            Return scheduler.ResourceConnectionManager
        End Get
    End Property

    ''' <summary>
    ''' The ID of the initial task which is executed by this schedule
    ''' </summary>
    Public Property InitialTaskId As Integer
        Get
            Return mInitTaskId
        End Get
        Set
            If mInitTaskId <> Value Then
                MarkDataChanged("InitialTaskId", mInitTaskId, Value)
                mInitTaskId = Value
            End If
        End Set
    End Property

    ''' <summary>
    ''' The initial task object which is executed by this schedule,
    ''' or null if the initial task cannot be found or is not set.
    ''' </summary>
    Public Property InitialTask As ScheduledTask
        Get
            If mInitTaskId = 0 Then Return Nothing ' Quick check
            Return mTasks.FirstOrDefault(Function(t) t.Id = mInitTaskId)
        End Get

        Set
            If Value Is Nothing Then
                InitialTaskId = 0 ' go through the property to allow ChangeData() calls
            Else
                For Each t As ScheduledTask In mTasks
                    If ReferenceEquals(t, Value) Then
                        InitialTaskId = t.Id ' as above, through the property rather then the mv
                        Return
                    End If
                Next
                Throw New ArgumentException(
                 My.Resources.SessionRunnerSchedule_InitialTaskCannotBeSetToATaskNotAssignedToThisSchedule, NameOf(Value))
            End If
        End Set
    End Property

    ''' <summary>
    ''' The ID of this schedule.
    ''' </summary>
    Public Property Id As Integer
        Get
            Return mId
        End Get
        Set
            If mId <> Value Then
                MarkDataChanged("Id", mId, Value)
                mId = Value
            End If
        End Set
    End Property

    ''' <summary>
    ''' The version number of this schedule. Should only be used by the 
    ''' database handling code to ensure that this instance represents the
    ''' latest version of the schedule on the database.
    ''' <strong>Note: </strong> The setting of this value does not affect the
    ''' 'changed data' flag held within this object.
    ''' </summary>
    Public Property Version As Integer
        Get
            Return mVersion
        End Get
        Set
            mVersion = Value
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether this schedule has expired or not, ie. whether it is
    ''' currently scheduled to run again or not.
    ''' </summary>
    Public ReadOnly Property Expired As Boolean
        Get
            Return Triggers.HasExpired
        End Get
    End Property

    ''' <summary>
    ''' Gets or creates a task with the given ID.
    ''' This will register the task with this schedule so that it doesn't need to 
    ''' be done separately.
    ''' </summary>
    ''' <param name="id">The ID of the required task</param>
    ''' <returns>The task registered with this schedule with the given ID.
    ''' </returns>
    Public Function GetOrCreateTaskWithId(id As Integer) As ScheduledTask
        For Each tsk As ScheduledTask In Me
            If tsk.Id = id Then
                Return tsk
            End If
        Next

        ' Otherwise, create one...
        Dim t As ScheduledTask = NewTask()
        t.Id = id
        Add(t)
        Return t

    End Function

    ''' <summary>
    ''' Gets the next available unique task name
    ''' </summary>
    ''' <returns>The next available unique task name.</returns>
    Private Function GetUniqueTaskName() As String
        Return GetUniqueTaskName(My.Resources.SessionRunnerSchedule_0NewTask)
    End Function

    ''' <summary>
    ''' Gets a unique task name using the given format string.
    ''' In the given string, two placeholders can be used :- <list>
    ''' <item>"{0}" : Schedule Name</item>
    ''' <item>"{1}" : incrementing number - tried without a number first, and
    ''' then incrementing from 1.</item>
    ''' </list>
    ''' If "{1}" is not present within the format string and the formatted
    ''' string without a number is not unique, an incrementing number (starting
    ''' from 1) will be appended to the end in brackets, ie. in the form : 
    ''' " ({1})", until the name is unique within this schedule.
    ''' </summary>
    ''' <param name="formatString">The string to use to format the name of the
    ''' task which is required.</param>
    ''' <returns></returns>
    ''' <exception cref="OverflowException">If all available task names of
    ''' the given format are currently taken.</exception>
    Public Function GetUniqueTaskName(formatString As String) As String

        Dim name As String = Me.Name
        Dim names As New clsSet(Of String)()
        For Each task As ScheduledTask In mTasks
            names.Add(task.Name)
        Next
        Dim taskName As String = String.Format(formatString, name, 0)
        If Not names.Contains(taskName) Then Return taskName
        If Not formatString.Contains("{1}") Then formatString &= " ({1})"

        For i = 1 To Integer.MaxValue
            Dim newTaskName As String = String.Format(formatString, name, i)
            If Not names.Contains(newTaskName) Then Return newTaskName
        Next

        Throw New OverflowException(
         String.Format(My.Resources.SessionRunnerSchedule_ThisScheduleHas0TasksOnItCannotAddAnyMore, Integer.MaxValue))

    End Function

    ''' <summary>
    ''' Creates a new task which is owned by this schedule, and is ready to
    ''' operate for this schedule. The task is given the name 
    ''' "[ScheduleName] - Task [n]" where [ScheduleName] represents the name of
    ''' this schedule and [n] represents one more than the number of tasks
    ''' already held by this schedule.
    ''' </summary>
    ''' <returns>A new task of the type required by this schedule.</returns>
    Public Function NewTask() As ScheduledTask
        Dim task As New ScheduledTask With {
            .Name = GetUniqueTaskName()
        }
        Return task
    End Function

    ''' <summary>
    ''' Checks if this schedule requires a refresh from the database or not,
    ''' by checking it agains the given version number, retrieved itself from
    ''' the database.
    ''' </summary>
    ''' <param name="versionNo">The current version of the schedule that this
    ''' object should check itself against.</param>
    ''' <returns>True if the given version number does not match that held in
    ''' this schedule - false if it does, and thus this schedule represents
    ''' the latest version on the database.</returns>
    Friend Function RequiresRefresh(versionNo As Integer) As Boolean
        Return versionNo <> mVersion
    End Function

    ''' <summary>
    ''' Checks if this schedule is currently flagged as being aborted.
    ''' </summary>
    ''' <param name="reason">If this schedule is being aborted, this will hold
    ''' the reason when this method exits.</param>
    ''' <returns>True if this schedule is being aborted; False otherwise.
    ''' </returns>
    Friend Function IsAborting(ByRef reason As String) As Boolean
        reason = mAbortMessage
        Return reason IsNot Nothing
    End Function

    ''' <summary>
    ''' Resets the abort status of this schedule. ie. ensures that this schedule
    ''' is no longer flagged as in the process of being aborted.
    ''' This also ensures that any thread waiting for such a status is woken up.
    ''' </summary>
    Private Sub ResetAbort()
        SyncLock mAbortLock
            mAbortMessage = Nothing
            Monitor.PulseAll(mAbortLock)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Flag indicating whether this schedule is retired or not.
    ''' </summary>
    Public Property Retired As Boolean
        Get
            Return mRetired
        End Get
        Set
            mRetired = Value
        End Set
    End Property

#End Region

#Region "SessionRunnerSchedule Events"

    ''' <summary>
    ''' Inner class to hold and call the events for use in this schedule
    ''' </summary>
    Public Class EventManager

        ''' <summary>
        ''' Event fired when a task is added to this schedule.
        ''' </summary>
        ''' <param name="args">The arguments detailing the schedule event.</param>
        ''' <param name="affectedTask">The task being added to the schedule.</param>
        Public Event TaskAdded(args As ScheduleEventArgs, affectedTask As ScheduledTask)

        ''' <summary>
        ''' Event fired when a task is removed from this schedule
        ''' </summary>
        ''' <param name="args">The arguments detailing the schedule event.</param>
        ''' <param name="affectedTask">The task being removed from the schedule.
        ''' </param>
        Public Event TaskRemoved(args As ScheduleEventArgs, affectedTask As ScheduledTask)

        ''' <summary>
        ''' Fires the required event.
        ''' </summary>
        ''' <param name="added">True to fire the task added event; False to fire the
        ''' task removed event.</param>
        ''' <param name="args">The arguments detailing the schedule that is affected
        ''' </param>
        ''' <param name="affectedTask">The task which is either being added or
        ''' removed.</param>
        Friend Sub FireTaskEvent(added As Boolean, args As ScheduleEventArgs, affectedTask As ScheduledTask)
            If added Then
                RaiseEvent TaskAdded(args, affectedTask)
            Else
                RaiseEvent TaskRemoved(args, affectedTask)
            End If
        End Sub

    End Class

    ''' <summary>
    ''' The event manager for this schedule.
    ''' This allows 
    ''' </summary>
    <NonSerialized>
    Private mEventManager As EventManager

    ''' <summary>
    ''' The event manager for this schedule. This deals with the task added / removed
    ''' events.
    ''' </summary>
    Public ReadOnly Property Events As EventManager
        Get
            If mEventManager Is Nothing Then mEventManager = New EventManager()
            Return mEventManager
        End Get
    End Property

#End Region

#Region "Constructors"

    ''' <summary>
    ''' Creates a new empty schedule.
    ''' </summary>
    Public Sub New(owner As IScheduler)
        Me.New(owner, Nothing)
    End Sub

    Friend Sub New(owner As IScheduler, scheduleData As ScheduleDatabaseData, Optional user As IUser = Nothing)
        MyBase.New(owner)

        mUser = user
        mAbortMessage = Nothing
        mTasks = NewTaskSet()

        If scheduleData IsNot Nothing Then
            mId = scheduleData.Id
            Name = scheduleData.Name
            Description = scheduleData.Description
            InitialTaskId = scheduleData.InitialTaskId
            mVersion = scheduleData.VersionNumber
            mRetired = scheduleData.Retired

            For Each task In scheduleData.Tasks
                Dim newTask = GetOrCreateTaskWithId(task.Id)
                newTask.Populate(task)
            Next

            For Each taskSession In scheduleData.TaskSessions
                Dim task = GetOrCreateTaskWithId(taskSession.TaskId)
                Dim session = New ScheduledSession(taskSession)
                task.AddSession(session)
            Next

            Dim factory = TriggerFactory.GetInstance()
            For Each newTrigger In From trigger In scheduleData.Triggers Select factory.CreateTrigger(trigger)
                AddTrigger(newTrigger)
            Next
        End If

        ResetChanged()

        AddHandler DataChanged, AddressOf HandleDataChange
    End Sub

    ''' <summary>
    ''' Generates a new set to hold the clsTask instances in.
    ''' This just ensures that the constructor and the Copy() method use a
    ''' consistent concrete class.
    ''' </summary>
    ''' <returns>The set in which tasks can be stored for this schedule.</returns>
    Private Function NewTaskSet() As IBPSet(Of ScheduledTask)
        Return New clsSet(Of ScheduledTask)
    End Function

#End Region

#Region "clsDataMonitor overrides"

    ''' <summary>
    ''' Checks to see if any data has been changed on this object since it was
    ''' retrieved from the database.
    ''' </summary>
    ''' <returns>True if the schedule has changed at all</returns>
    Public Overrides Function HasChanged() As Boolean
        If MyBase.HasChanged() Then Return True
        Return Any(Function(t) t.HasChanged())
    End Function

    ''' <summary>
    ''' Resets the changed data flag within this object. Immediately after
    ''' calling this method, <see cref="HasChanged"/> will return False.
    ''' </summary>
    Public Overrides Sub ResetChanged()
        MyBase.ResetChanged()

        For Each t As ScheduledTask In Me
            t.ResetChanged()
        Next

        GetAndResetAuditEvents()
    End Sub

#End Region

#Region "Object Overrides & IEquatable Implementation"

    ''' <summary>
    ''' Checks if the given schedule is equal to this one.
    ''' This implementation just checks the ID to see if the schedule is equal.
    ''' </summary>
    ''' <param name="other">The schedule to check this schedule against.</param>
    ''' <returns>true if the given schedule is not null and has the same ID and name
    ''' as this one.</returns>
    Public Overloads Function Equals(other As SessionRunnerSchedule) As Boolean _
        Implements IEquatable(Of SessionRunnerSchedule).Equals

        Return other IsNot Nothing AndAlso
               Id = other.Id AndAlso
               Owner Is other.Owner AndAlso
               Equals(Name, other.Name)

    End Function

    ''' <summary>
    ''' Checks if the given object is equal to this schedule.
    ''' An object is considered equal if it is a non-null instance
    ''' of Schedule with the same ID as this instance.
    ''' </summary>
    ''' <param name="obj">The object to check for equality against.
    ''' </param>
    ''' <returns>true if the given object is equal to this one,
    ''' false otherwise.</returns>
    Public Overrides Function Equals(obj As Object) As Boolean
        Return Equals(TryCast(obj, SessionRunnerSchedule))
    End Function

    ''' <summary>
    ''' Gets a hash code for this schedule.
    ''' </summary>
    ''' <returns>A basic hash which is unique to this schedule. This
    ''' implementation relies on the fact that the name is unique to a
    ''' schedule within the context of its scheduler.</returns>
    Public Overloads Overrides Function GetHashCode() As Integer
        Dim ownerHash = 0
        If Owner IsNot Nothing Then ownerHash = Owner.GetHashCode()
        Return Id Xor ownerHash Xor Name.GetHashCode()
    End Function

    ''' <summary>
    ''' Gets a string representation of this schedule.
    ''' </summary>
    ''' <returns>A string representation of this schedule.</returns>
    Public Overloads Overrides Function ToString() As String
        Return Name
    End Function

#End Region

#Region "BaseSchedule overrides and implementations"

    ''' <summary>
    ''' Gets the currently running instances of this schedule on any scheduler
    ''' that the database is aware of.
    ''' </summary>
    ''' <returns>The logs representing the running instances of this schedule.
    ''' </returns>
    Public Overrides Function GetRunningInstances() As ICollection(Of IScheduleLog)
        ' Get the current running logs from the server - set the schedule to be
        ' this object on each log returned and return to the caller.
        Dim logs As ICollection(Of IScheduleLog) = gSv.SchedulerGetRunningLogs(mId)
        For Each log As BasicReadOnlyLog In logs
            log.Schedule = Me
        Next
        Return logs
    End Function

    ''' <summary>
    ''' Executes this schedule.
    ''' </summary>
    Public Overrides Sub Execute(schedLog As IScheduleLog)
        Try
            mExecuting = True

            ' If 'abort' is set before we start executing... might as well just not bother.
            If mAbortMessage IsNot Nothing Then
                ' reset the abort message so this schedule can be run again in the future
                ResetAbort()
                Return
            End If

            Dim log = TryCast(schedLog, ISessionRunnerScheduleLogger)
            If log Is Nothing Then
                Throw New InvalidOperationException("log is not a SessionRunnerScheduleLogger")
            End If
            log.Start()

            Dim task As ScheduledTask = InitialTask
            Dim lastTask As ScheduledTask = Nothing
            Dim lastResult = TaskResult.Success
            ' assume success if no tasks
            While task IsNot Nothing

                If mAbortMessage IsNot Nothing Then
                    log.Terminate(mAbortMessage)
                    ' reset the abort message so this schedule can be run again in the future
                    ResetAbort()
                    Return
                End If

                lastTask = task ' Store the last task so we know which one we finished on

                ' Log the 'started task' entry
                log.LogEvent(New ScheduleLogEntry(ScheduleLogEventType.TaskStarted, task.Id, 0))

                ' Execute the task inside a try...catch - if it throws an exception,
                ' treat it as a 'task failure' and move onto the 'OnFailure' task
                ' as normal. If it finishes cleanly, check the result from the task
                ' before logging the result.
                Try

                    lastResult = task.Execute(log)
                    If lastResult = TaskResult.Success Then
                        log.LogEvent(New ScheduleLogEntry(ScheduleLogEventType.TaskCompleted, task.Id, 0))
                        task = task.OnSuccess
                    Else
                        log.LogEvent(New ScheduleLogEntry(ScheduleLogEventType.TaskTerminated,
                          task.Id, 0, String.Format(My.Resources.SessionRunnerSchedule_Task0FailedWithNoMessage, task.Name)))
                        task = task.OnFailure
                    End If

                Catch sce As ScheduledSessionCreationFailedException

                    ' If sess creation failed, and diagnostic data is available, ensure that
                    ' it is saved to the database.
                    Dim map = TryCast(sce.DiagData, IDictionary(Of Guid, RunnerStatus))

                    ' If we have diag data regarding the session status on the resource in
                    ' question, format it into something meaningful.
                    Dim dataStr As String = Nothing
                    If map IsNot Nothing Then
                        Dim running As New List(Of Guid)
                        Dim pending As New List(Of Guid)
                        ' Separate the sessions into running and pending sessions.
                        ' Ignore any other state.
                        For Each sessId As Guid In map.Keys
                            Select Case map(sessId)
                                Case RunnerStatus.RUNNING : running.Add(sessId)
                                Case RunnerStatus.PENDING : pending.Add(sessId)
                            End Select
                        Next

                        Dim sb As New StringBuilder()
                        If running.Count > 0 Then
                            sb.Append(My.Resources.SessionRunnerSchedule_RunningSessions).Append(vbCrLf)
                            ' Append each sessions details to the buffer
                            For Each sess In From guid In running Select gSv.GetSessionDetails(guid)
                                sb.AppendFormat(My.Resources.SessionRunnerSchedule_0By12,
                                                sess.ProcessName, sess.UserName, vbCrLf)
                            Next
                        End If

                        If pending.Count > 0 Then
                            sb.Append(My.Resources.SessionRunnerSchedule_PendingSessions).Append(vbCrLf)
                            ' Append each sessions details to the buffer
                            For Each sess In From guid In pending Select gSv.GetSessionDetails(guid)
                                sb.AppendFormat(My.Resources.SessionRunnerSchedule_0By12,
                                                sess.ProcessName, sess.UserName, vbCrLf)
                            Next
                        End If

                        dataStr = sb.ToString().Trim()
                        mLog.Error(dataStr)
                    End If

                    Dim terminationReason = String.Format(
                     My.Resources.SessionRunnerSchedule_Task0SessionCreationFailedOnResource12,
                     task.Name, sce.Session.ResourceName, sce.Message)
                    log.LogEvent(New ScheduleLogEntry(ScheduleLogEventType.TaskTerminated,
                     task.Id, 0, terminationReason))
                    mLog.Error(terminationReason)

                    lastResult = TaskResult.Failure
                    task = task.OnFailure

                Catch ex As Exception
                    log.LogEvent(New ScheduleLogEntry(ScheduleLogEventType.TaskTerminated,
                        task.Id, 0,
                        String.Format(My.Resources.SessionRunnerSchedule_Task0ThrewAnException1, task.Name, ex.Message)))

                    lastResult = TaskResult.Failure
                    task = task.OnFailure

                End Try

            End While

            If lastResult = TaskResult.Success Then
                log.Complete()
            Else
                Dim taskName = My.Resources.SessionRunnerSchedule_NoTaskFound
                If lastTask IsNot Nothing Then taskName = lastTask.Name
                log.Terminate(String.Format(My.Resources.SessionRunnerSchedule_TheLastTaskInTheSchedule0Failed, taskName))
            End If

        Finally
            ' Ensure that abort is reset if it was set before,
            ' and that anyone waiting for abort to finish is informed.
            ResetAbort()
            mExecuting = False
        End Try
    End Sub

    ''' <summary>
    ''' Aborts this schedule if it is currently executing. Has no effect otherwise.
    ''' </summary>
    ''' <param name="reason">The reason that this schedule is being aborted.</param>
    Public Overrides Sub Abort(reason As String)
        SyncLock mAbortLock
            If Not mExecuting Then Return ' Not executing - nothing to abort.
            mAbortMessage = reason
            Monitor.Wait(mAbortLock)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Handles a scheduler misfire, indicating to the caller what to do about
    ''' the misfire in this instance.
    ''' </summary>
    ''' <param name="instance">The trigger instance which has misfired.</param>
    ''' <param name="reason">The reason for the misfire.</param>
    ''' <returns>The action which should be taken with regards to this misfire.
    ''' </returns>
    ''' <exception cref="InvalidOperationException">If the misfire reason was
    ''' unrecognised.</exception>
    Public Overrides Function Misfire(instance As ITriggerInstance, reason As TriggerMisfireReason) As TriggerMisfireAction
        Dim store = DirectCast(Owner.Store, DatabaseBackedScheduleStore)

        ' Generally, we abort any misfire here - the exception is if an earlier
        ' instance of the schedule is still running - in that case we want to
        ' suppress it - ensuring that this is logged properly, so that we don't
        ' attempt to execute that schedule again later.
        Try
            Select Case reason
                Case TriggerMisfireReason.ModeWasIndeterminate
                    store.CreateLog(instance, TriggerActivationReason.Indeterminate)
                    Return TriggerMisfireAction.AbortInstance

                Case TriggerMisfireReason.ScheduleAlreadyExecutedDuringResume,
                  TriggerMisfireReason.ScheduleAlreadyExecutedDuringStartup
                    store.CreateLog(instance, TriggerActivationReason.MissedWhileNotRunning)
                    Return TriggerMisfireAction.AbortInstance

                Case TriggerMisfireReason.ModeWasSuppress
                    store.CreateLog(instance, TriggerActivationReason.Suppress)
                    Return TriggerMisfireAction.AbortInstance

                Case TriggerMisfireReason.EarlierScheduleInstanceStillRunning
                    Return TriggerMisfireAction.SuppressInstance

                Case TriggerMisfireReason.ScheduleInstanceAlreadyExecuted
                    Return TriggerMisfireAction.AbortInstance

                Case Else
                    Throw New InvalidOperationException("Unrecognised misfire reason: " & reason.ToString())

            End Select

        Catch aae As AlreadyActivatedException
            ' Ignore - this suggests that another scheduler is active and
            ' has already suppressed this instance - just tell the scheduler to
            ' abort it and carry on.
            Return TriggerMisfireAction.AbortInstance

        End Try
    End Function

    ''' <summary>
    ''' Basic structure to hold an OnSuccess and OnFailure ID for a task.
    ''' </summary>
    Private Structure SuccessFailureBox
        Public OnSuccessId As Integer ' Old ID of the task called on success
        Public OnFailureId As Integer ' Old ID of the task called on failure

        ' Creates a new SuccessFailure box with the given Old IDs
        Public Sub New(succ As Integer, fail As Integer)
            OnSuccessId = succ
            OnFailureId = fail
        End Sub

        ' Creates a new SuccessFailure box using the IDs of the given tasks,
        ' or 0 if the given tasks are null.
        Public Sub New(succ As ScheduledTask, fail As ScheduledTask)
            If succ IsNot Nothing Then OnSuccessId = succ.Id
            If fail IsNot Nothing Then OnFailureId = fail.Id
        End Sub
    End Structure

    ''' <summary>
    ''' Creates a separate copy of this schedule and returns it.
    ''' Note that this avoids using Clone() simply because the semantics are
    ''' inaccurate - the copied schedule has no ID and all its tasks have different
    ''' (temporary) IDs to those in this schedule.
    ''' Events are not cloned, and audit events are not cloned.
    ''' To all intents and purposes, this is a new schedule which just happens to
    ''' contain the same name, description, triggers etc. and has the same number of
    ''' tasks, each with the same name, description and sessions etc.
    ''' </summary>
    ''' <returns>This schedule copied into a new schedule.</returns>
    Public Overrides Function Copy() As ISchedule

        Dim sched = DirectCast(MyBase.Copy(), SessionRunnerSchedule)

        ' The copy is not the same as this schedule - reset the ID so that
        ' the new schedule doesn't overwrite the source schedule
        sched.mId = 0

        ' Tasks. Huh. What are they good for? Nothing. Say it again. etc.
        ' We want to make sure we keep all the dependencies here, but we
        ' need to make sure that we change all the IDs on the copied data
        ' or things could get very messy when it comes to dealing with DB

        ' Therefore, maintain 2 maps - one to resolve the new ID given the
        ' old ID (idMap), and one to hold the onsuccess / onfailure (old
        ' ID) values 

        ' When the tasks have all been copied, resolve any changes in the
        ' map so that all relationships tie up again.
        Dim idMap As New Dictionary(Of Integer, Integer) ' <oldid> : <newid>
        Dim sfMap As New Dictionary(Of Integer, SuccessFailureBox) ' <oldid> : <oldid,oldid>

        ' And a dictionary to hold the new tasks in temporarily so we can
        ' look up the tasks by ID.
        Dim taskMap As New Dictionary(Of Integer, ScheduledTask) ' <newid> : <task>

        ' Add a null task against ID 0 so we can retrieve the OnSuccess's and 
        ' OnFailure's later on without messy ContainsKey() or TryGetValue() calls
        idMap(0) = 0
        taskMap(0) = Nothing

        ' Now add each task into the cloned schedule, ensuring that the
        ' map of old ID to new ID is updated.
        For Each task As ScheduledTask In Me
            Dim clone As ScheduledTask = task.Copy()
            ' Map the new ID against the old one
            idMap(task.Id) = clone.Id
            ' Map the old onsucc and onfail IDs against the old ID
            sfMap(task.Id) = New SuccessFailureBox(task.OnSuccess, task.OnFailure)
            ' Map the task against its new ID.
            taskMap(clone.Id) = clone
            ' Finally, set the owner to this cloned schedule.
            clone.Owner = sched
        Next

        ' All tasks are added, resolve all the references to ensure that
        ' the cloned schedule and its tasks have correct initial task /
        ' on success / on failure assignments.

        ' Initial task is relatively easy - just use the ID map.
        If InitialTask IsNot Nothing Then
            sched.InitialTaskId = idMap(InitialTaskId)
        End If

        For Each task As ScheduledTask In sched

            ' These are still the old tasks, so get the clone using its new ID
            Dim clone As ScheduledTask = taskMap(idMap(task.Id))

            ' The success/failure is mapped against the old task ID.
            Dim box As SuccessFailureBox = sfMap(task.Id)

            ' If not set, these will resolve to zero which was added to the
            ' map with a null value when the map was created.
            clone.OnSuccess = taskMap(idMap(box.OnSuccessId))
            clone.OnFailure = taskMap(idMap(box.OnFailureId))

        Next

        ' We've finished with the map now - remove the '0' element
        taskMap.Remove(0)

        ' And add the tasks to the schedule in a new task set (ie. don't
        ' use the one in there since it's the same as the one in the source)
        sched.mTasks = sched.NewTaskSet()
        sched.mTasks.Union(taskMap.Values)

        ' Set the audit events and related collection to nothing - these are
        ' not recorded against the copied schedule and thus should not be there.
        ' The collections are re-generated when they are next needed.
        sched.mEvents = Nothing
        sched.mModifiedTasks = Nothing

        ' A new event manager to handle events for the copied schedule.
        ' This has the effect of disabling any events which have been 
        ' cloned across from the old schedule.
        sched.mEventManager = New EventManager()

        ' Final thing, the Abort lock needs to be different in the copied schedule
        sched.mAbortLock = New Object()

        Return sched

    End Function

#End Region

#Region "ICollection and related Members"

    ''' <summary>
    ''' Adds the given task to this schedule and sets it as the initial task as
    ''' specified. Note that if the given task is already contained in this
    ''' schedule, it will not be added, and it will <em>not</em> be set as the
    ''' initial task, regardless of the 'makeInitialTask' parameter.
    ''' </summary>
    ''' <param name="task">The task to add to this schedule.</param>
    ''' <param name="makeInitialTask">True to set the newly added task as the
    ''' initial task in this schedule; False to leave the initial task as it is.
    ''' </param>
    ''' <returns>true if the given task was non-null and did not already
    ''' exist within this schedule and was thus added to it; false if the
    ''' given task was either null or already a part of this schedule and
    ''' it was therefore not added.</returns>
    Public Function Add(task As ScheduledTask, makeInitialTask As Boolean) As Boolean
        If task Is Nothing Then
            Return False
        End If

        If mTasks.Add(task) Then
            task.Owner = Me
            If makeInitialTask Then InitialTaskId = task.Id
            MarkDataChanged("TaskAdded", Nothing, task)
            Events.FireTaskEvent(True, New ScheduleEventArgs(Me), task)
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Adds the given task to this schedule if it is not already contained
    ''' within this schedule.
    ''' This will not alter the initial task currently set on this schedule.
    ''' </summary>
    ''' <param name="task">The task to add to this schedule.</param>
    ''' <returns>true if the given task was non-null and did not already
    ''' exist within this schedule and was thus added to it; false if the
    ''' given task was either null or already a part of this schedule and
    ''' it was therefore not added.</returns>
    Public Function Add(task As ScheduledTask) As Boolean
        Return Add(task, False)
    End Function

    ''' <summary>
    ''' <para>Removes the given task from this schedule, if it exists.
    ''' This will ensure that any links within remaining tasks are joined up - 
    ''' ie. tasks which link to the given task after successful or failed
    ''' execution will now link to the tasks specified in the given task.
    ''' </para><para>
    ''' eg. if:<pre>
    ''' Task1.OnSuccess == Task2 and 
    ''' Task2.OnSuccess == Task3 </pre>
    ''' then after Remove(Task2) is called, the state will be:<pre>
    ''' Task1.OnSuccess == Task3</pre>
    ''' The same applies for OnFailure.
    ''' </para>
    ''' <para>
    ''' If the task to be removed is the initial task for this schedule,
    ''' the initial task will be set to the OnSuccess task of the task
    ''' to be removed.
    ''' </para>
    ''' </summary>
    ''' <param name="task">The task to remove from this schedule.</param>
    Public Function Remove(task As ScheduledTask) As Boolean Implements ICollection(Of ScheduledTask).Remove
        If task Is Nothing Then
            Return False
        End If

        If Not mTasks.Remove(task) Then
            Return False
        End If

        ' task was removed... clean up a bit.
        task.Owner = Nothing

        ' Now we need to go through all the other tasks and make sure
        ' that any with the OnSuccess / OnFailure pointing to the
        ' removed task are updated. This will act like a linked list
        ' in that any tasks pointing to the removed task will now 
        ' point to the task that it pointed to
        For Each t As ScheduledTask In mTasks

            If task.Equals(t.OnSuccess) Then
                If t.Equals(task.OnSuccess) Then
                    ' Can't set onsuccess to itself, so set it to stop
                    t.OnSuccess = Nothing
                Else
                    t.OnSuccess = task.OnSuccess
                End If
            End If

            If task.Equals(t.OnFailure) Then
                If t.Equals(task.OnFailure) Then
                    ' As above, so below.
                    t.OnFailure = Nothing
                Else
                    t.OnFailure = task.OnFailure
                End If
            End If

        Next

        ' Point the initial task of this schedule to the OnSuccess
        ' of the task being removed.
        If InitialTaskId = task.Id Then
            Dim t As ScheduledTask = task.OnSuccess
            If t Is Nothing Then
                InitialTaskId = 0
            Else
                InitialTaskId = t.Id
            End If
        End If

        MarkDataChanged("TaskRemoved", task, Nothing)
        Events.FireTaskEvent(False, New ScheduleEventArgs(Me), task)
        Return True
    End Function

    ''' <summary>
    ''' Adds the task to this schedule implementing the ICollection interface.
    ''' </summary>
    ''' <param name="item">The task to add to this schedule</param>
    Private Sub Add1(item As ScheduledTask) Implements ICollection(Of ScheduledTask).Add
        Add(item)
    End Sub

    ''' <summary>
    ''' Clears all tasks on this schedule, ensuring that their owner
    ''' is set to null in the process
    ''' </summary>
    Public Sub Clear() Implements ICollection(Of ScheduledTask).Clear
        ' Do this the long way to ensure that the references are
        ' kept up to date appropriately and that the events are
        ' fired correctly.
        For Each t As ScheduledTask In New List(Of ScheduledTask)(mTasks)
            Remove(t)
        Next
    End Sub

    ''' <summary>
    ''' Checks if this schedule contains the given task.
    ''' </summary>
    ''' <param name="item">The task to check to see if it is on this
    ''' schedule.</param>
    ''' <returns>true if this schedule contains the given task; false
    ''' otherwise.</returns>
    Public Function Contains(item As ScheduledTask) As Boolean Implements ICollection(Of ScheduledTask).Contains
        Return mTasks.Contains(item)
    End Function

    ''' <summary>
    ''' Checks if this schedule contains a task with the given name.
    ''' </summary>
    ''' <param name="taskName">The name of the task to check for.</param>
    ''' <returns>true if this schedule contains the specified task; false otherwise.
    ''' </returns>
    Public Function Contains(taskName As String) As Boolean
        Return Me(taskName) IsNot Nothing
    End Function

    ''' <summary>
    ''' Copies the tasks on this schedule to the given array.
    ''' </summary>
    ''' <param name="array">The array to copy the tasks to.</param>
    ''' <param name="arrayIndex">The index at which the elements should
    ''' be inserted.</param>
    ''' <exception cref="ArgumentNullException">If the given array was
    ''' null.</exception>
    ''' <exception cref="ArgumentOutOfRangeException">If there is not
    ''' enough space on the array to copy the tasks from the given index,
    ''' or if the index was outside the bounds of the array.</exception>
    Public Sub CopyTo(array As ScheduledTask(), arrayIndex As Integer) Implements ICollection(Of ScheduledTask).CopyTo
        mTasks.CopyTo(array, arrayIndex)
    End Sub

    ''' <summary>
    ''' The number of tasks which are held by this schedule.
    ''' </summary>
    Public ReadOnly Property Count As Integer Implements ICollection(Of ScheduledTask).Count
        Get
            Return mTasks.Count
        End Get
    End Property

    ''' <summary>
    ''' Whether this collection is read-only or not. It is not.
    ''' </summary>
    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of ScheduledTask).IsReadOnly
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets the task with the given name, or null if no such task exists.
    ''' </summary>
    ''' <param name="name">The name of the task required.</param>
    ''' <returns>The task associated with the given name, or null, if no such task
    ''' exists in this schedule</returns>
    Default Public ReadOnly Property Item(name As String) As ScheduledTask
        Get
            Return FirstOrDefault(Function(t) t.Name = name)
        End Get
    End Property

#End Region

#Region "IEnumerable + IEnumerable Members"

    ''' <summary>
    ''' Gets the enumerator which passes over the tasks in this schedule.
    ''' </summary>
    ''' <returns>An enumerator over the tasks in this schedule</returns>
    Public Function GetEnumerator() As IEnumerator(Of ScheduledTask) Implements IEnumerable(Of ScheduledTask).GetEnumerator
        Return mTasks.GetEnumerator()
    End Function

    ''' <summary>
    ''' Gets the non-generic enumerator which passes over the tasks in this
    ''' schedule.
    ''' </summary>
    ''' <returns>An enumerator over the tasks in this schedule</returns>
    Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

#End Region

    Public Function IsLoopingSchedule() As Boolean
        Dim loopFound = False
        Dim tasksChecked As New List(Of Integer)
        While Not loopFound AndAlso tasksChecked.Count <> mTasks.Count
            FindLoop(mTasks.First(Function(t) Not tasksChecked.Contains(t.Id)), loopFound, New HashSet(Of Integer), tasksChecked)
        End While
        Return loopFound
    End Function
    Private Sub FindLoop(task As ScheduledTask, ByRef loopFound As Boolean, taskIds As HashSet(Of Integer), ByRef tasksChecked As List(Of Integer))
        If task IsNot Nothing Then
            If Not loopFound Then
                If Not taskIds.Contains(task.Id) Then
                    taskIds.Add(task.Id)
                    FindLoop(task.OnSuccess, loopFound, New HashSet(Of Integer)(taskIds), tasksChecked)
                    FindLoop(task.OnFailure, loopFound, New HashSet(Of Integer)(taskIds), tasksChecked)
                Else
                    loopFound = True
                End If
            End If
            If Not tasksChecked.Contains(task.Id) Then
                tasksChecked.Add(task.Id)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Writes this schedule out to the given XML writer, including sessions and
    ''' resources as specified. Note that if sessions are set to be excluded, then,
    ''' by implication, resources are excluded.
    ''' The IDs of any calendars referenced by this schedule are returned after this
    ''' method is complete, or an empty collection is returned if no calendars are
    ''' referenced.
    ''' </summary>
    ''' <param name="writer">The writer to which this schedule should be
    ''' written.</param>
    ''' <param name="includeSessions">True to include sessions in the XML output.
    ''' This will write the processes (by name) and, if specified, the resources
    ''' to which those processes are bound.</param>
    ''' <param name="includeResources">True to include the resources in the sessions
    ''' that are written to the given writer. If <paramref name="includeSessions"/>
    ''' is False, then this flag is ignored - ie. the resources are part of the
    ''' sessions, so if they are not included, the resources will not be output.
    ''' </param>
    Public Sub ToXml(writer As XmlWriter, includeSessions As Boolean, includeResources As Boolean)

        writer.WriteStartElement("schedule")
        writer.WriteAttributeString("name", Name)
        writer.WriteElementString("description", Description)

        writer.WriteStartElement("triggers")
        For Each md As TriggerMetaData In Triggers.MetaData
            md.ToXml(writer, Owner.Store)
        Next
        writer.WriteEndElement() 'triggers

        writer.WriteStartElement("tasks")
        For Each t As ScheduledTask In Me
            t.ToXml(writer, includeSessions, includeResources)
        Next
        writer.WriteEndElement() 'tasks

        writer.WriteEndElement() 'schedule

    End Sub

End Class
