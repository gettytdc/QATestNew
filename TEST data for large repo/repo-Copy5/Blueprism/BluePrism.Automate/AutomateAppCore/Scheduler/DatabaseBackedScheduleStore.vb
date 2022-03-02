Imports System.Timers
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar
Imports BluePrism.Scheduling.Triggers
Imports NLog

''' Project  : AutomateAppCore
''' Class    : clsDatabaseBackedScheduleStore
''' <summary>
''' Schedule store which uses the database through the global clsServer instance
''' as its storage mechanism for reading and updating scheduler data.
''' Note that this class will only cope with schedules which are instances of
''' TaskBasedScheduler containing tasks which are clsSessionRunnerTask instances.
''' They are the only types which are supported by the database.
''' 
''' Note that this doesn't get passed across the client server link.
''' 
''' </summary>
<Serializable()>
Public Class DatabaseBackedScheduleStore
    Implements IScheduleStore

#Region " Class Scope Members "

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    ' The default interval that the store should wait to auto-update - in millis
    Private Const DefaultStoreListenInterval As Integer = 10000

    ' The weak reference to the shared inert store - create it with nothing at
    ' first. It is populated in the InertStore property getter
    Private Shared mInertStoreRef As New WeakReference(Nothing)

    ''' <summary>
    ''' Gets a shared instance of an inert database backed schedule store, which
    ''' auto-updates any cached data at regular intervals (currently every 10s)
    ''' </summary>
    Public Shared ReadOnly Property InertStore() As DatabaseBackedScheduleStore
        Get
            Dim store As DatabaseBackedScheduleStore =
             DirectCast(mInertStoreRef.Target, DatabaseBackedScheduleStore)

            ' If we haven't got a store yet (not set in the ref, or it has
            ' been garbage collected), create it and store it in the weakref
            If store Is Nothing Then
                store = New DatabaseBackedScheduleStore(New InertScheduler(), gSv)
                store.LoadDataAndListenForChanges(10000)
                mInertStoreRef.Target = store
            End If

            Return store
        End Get
    End Property

    Public Shared Sub DisposeInertStore()
        Dim store = DirectCast(mInertStoreRef.Target, DatabaseBackedScheduleStore)
        store?.Dispose()
        mInertStoreRef.Target = Nothing
    End Sub

#End Region

#Region " Events "

    ''' <summary>
    ''' Event fired when the calendars held by this store have been
    ''' updated and may have changed the timing data
    ''' </summary>
    Public Event CalendarsUpdated() Implements IScheduleCalendarStore.CalendarsUpdated

    ''' <summary>
    ''' Event fired when the backing store used by this object has been
    ''' updated. The store makes no distinction between pure data
    ''' changes and timing changes - just a blanket 'store updated'.
    ''' </summary>
    Public Event SchedulesUpdated() Implements IScheduleStore.SchedulesUpdated

#End Region

#Region " Member Vars "

    Private ReadOnly _server As IServer

    ' The timer used to listen for changes to the scheduler data on the database
    Private WithEvents _timer As Timer

    ' The current version of the scheduler data
    Private _versionNo As Long

    ' The scheduler which owns this store
    Private _owner As IScheduler

    ' The public holidays stored in a schema
    Private _schema As PublicHolidaySchema

    ' The calendars keyed on their IDs
    Private _calendars As IDictionary(Of Integer, ScheduleCalendar)

    ' The schedules keyed on their IDs
    Private _schedules As IDictionary(Of Integer, SessionRunnerSchedule)

    ' Flag to indicate that this object has been disposed of
    Private _disposed As Boolean = False

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new database-backed schedule store.
    ''' </summary>
    Public Sub New(server As IServer)
        Me.New(New InertScheduler(), server)
    End Sub

    ''' <summary>
    ''' Creates a new database-backed schedule store serving the given scheduler
    ''' </summary>
    ''' <param name="scheduler">The scheduler that this store is operating on
    ''' behalf of or null if it is not yet assigned to a scheduler.</param>
    Public Sub New(ByVal scheduler As IScheduler, server As IServer)
        _schedules = GetSynced.IDictionary(
         New Dictionary(Of Integer, SessionRunnerSchedule))
        ' Call the property to ensure that the scheduler's store is set to this
        Me.Owner = scheduler
        _server = server
    End Sub

#End Region

#Region " Finalization / Disposing "

    ''' <summary>
    ''' Finalizes this class, ensuring that it is disposed of.
    ''' </summary>
    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

    ''' <summary>
    ''' Disposes of this database backed schedule store
    ''' </summary>
    ''' <param name="explicit">True to explicitly dispose of this object, False to
    ''' only dispose of unmanaged references</param>
    Protected Overridable Sub Dispose(explicit As Boolean)
        If Not _disposed Then

            StopListeningForChanges()

            If explicit Then
                _schedules?.Clear()
                _schedules = Nothing
                _calendars?.Clear()
                _calendars = Nothing
            End If

            _disposed = True
        End If
    End Sub

    ''' <summary>
    ''' Disposes of this store
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region

#Region " Listening for changes / Refreshing data "

    ''' <summary>
    ''' Orders this store to load schedule data and to then listen for changes 
    ''' to the data on the database, checking every 
    ''' <paramref name="intervalMillis"/> milliseconds to see if the data has 
    ''' changed in the background.
    ''' </summary>
    ''' <param name="intervalMillis">The number of milliseconds between checks to
    ''' see if the data has changed on the database.</param>
    ''' <remarks>Note that if this store is already listening for changes, this
    ''' will simply change the interval at which it checks to the new given
    ''' value.</remarks>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub LoadDataAndListenForChanges(intervalMillis As Integer)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        ' Clean up any previous timer
        StopListeningForChanges()

        RefreshAll(True)

        _timer = New Timer(intervalMillis)
        _timer.Start()
    End Sub

    ''' <summary>
    ''' Orders this store to stop listening for changes to the scheduler data
    ''' on the database, meaning that any changes made directly to this store
    ''' will still fire events, but changes made separately from this store
    ''' will not.
    ''' </summary>
    ''' <remarks>This will still work if this object has been disposed, though really
    ''' the timer should not be running after that point anyway</remarks>
    Public Sub StopListeningForChanges()
        _timer?.Dispose()
        _timer = Nothing
    End Sub

    ''' <summary>
    ''' Handles the 'change listener' timer event.
    ''' Ignored if this store has been disposed
    ''' </summary>
    Private Sub HandleTimerElapsed(ByVal source As Object, ByVal e As ElapsedEventArgs) _
     Handles _timer.Elapsed
        ' If we're part way through disposing, best we quit now or it gets weird
        If _disposed Then Return
        RefreshAll(True)
    End Sub

    ''' <summary>
    ''' Refreshes all the data held in this store if any of the schedule data has
    ''' been updated by another process / thread, ie. schedule and calendar
    ''' data. Note that, on exit from this method, each of the schedules that
    ''' were retrieved have their 'data changed' flag set to 'False', ie. they
    ''' do <em>not</em> have any changed data.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Private Sub RefreshAll(fireEvent As Boolean)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim tempVerNo As Long = _versionNo ' Don't overwrite the memvar just yet
        Try
            If _server.HasSchedulerDataUpdated(tempVerNo) Then
                ' Ensure we have up to date calendar information
                GetAllCalendars(True)
                _schedules.Clear()

                ' Get and cache the schedules into our local collection.
                For Each sched As SessionRunnerSchedule In
                 _server.SchedulerGetActiveSchedules(_versionNo) ' overwrites the memvar
                    sched.Owner = _owner
                    sched.ResetChanged()
                    _schedules(sched.Id) = sched
                Next

                If fireEvent Then RaiseEvent SchedulesUpdated()

            End If

        Catch ex As Exception
            Log.Warn(ex, "Failed attempting to refresh scheduler data - " &
             "using old values (version {0})", tempVerNo)

        End Try

    End Sub


#End Region

#Region " Calendars & Public Holidays "

    ''' <summary>
    ''' Gets the public holiday schema from the database
    ''' </summary>
    ''' <returns>The public holiday schema as is it is currently held on the
    ''' database</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetSchema() As PublicHolidaySchema Implements IScheduleCalendarStore.GetSchema
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Return GetSchema(False)
    End Function

    ''' <summary>
    ''' Gets the schema for this store - either the cached one, or a fresh one
    ''' from the database if there is no cached one, or if forced to do so by the
    ''' <paramref name="refresh"/> parameter
    ''' </summary>
    ''' <param name="refresh">true to force the schema to be reloaded from
    ''' the database; false to use the cached one if it exists</param>
    ''' <returns>The public holiday schema used within this store.</returns>
    ''' <remarks>Note that each time the schema is loaded from the database, the
    ''' calendars are refreshed too. As such, a separate calendar refresh is 
    ''' unncessary.</remarks>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Private Function GetSchema(ByVal refresh As Boolean) As PublicHolidaySchema
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        If _schema Is Nothing OrElse refresh Then
            _schema = _server.GetPublicHolidaySchema()
            GetAllCalendars(True)
        End If
        Return _schema
    End Function

    ''' <summary>
    ''' Gets the dictionary of calendars held in this store, keyed on their ID.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Protected ReadOnly Property Calendars() As IDictionary(Of Integer, ScheduleCalendar)
        Get
            If _disposed Then Throw New ObjectDisposedException("Store",
            "The schedule/calendar store has been disposed")
            ' Load schema and calendars if they're not already there
            If _schema Is Nothing Then GetSchema()
            If _calendars Is Nothing Then GetAllCalendars()
            Return _calendars
        End Get
    End Property

    ''' <summary>
    ''' Gets all the calendars from this store.
    ''' Note that if the calendars have been modified outside of this instance of
    ''' the store, they may be out of date, since the calendars are initialised
    ''' on instantiation of this store, and updated only through the methods 
    ''' called via this object.
    ''' </summary>
    ''' <returns>The calendars represented within this store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Overridable Function GetAllCalendars() As ICollection(Of ScheduleCalendar) _
     Implements IScheduleCalendarStore.GetAllCalendars
        Return GetAllCalendars(False)
    End Function

    ''' <summary>
    ''' Gets all the calendars from this store, forcing a reload from the
    ''' database if there are none held in this store, or if instructed to do so
    ''' by the <paramref name="refresh"/> parameter.
    ''' </summary>
    ''' <param name="refresh">true to force a refresh of the calendar data from 
    ''' the database; false to use the cached data if it is already there.
    ''' </param>
    ''' <returns>The calendars represented within this store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetAllCalendars(ByVal refresh As Boolean) As ICollection(Of ScheduleCalendar)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        If _calendars Is Nothing OrElse refresh Then
            _calendars = GetSynced.IDictionary(_server.GetAllCalendars)
            RaiseEvent CalendarsUpdated()
        End If

        Return _calendars.Values

    End Function

    ''' <summary>
    ''' Adds the given calendar to this store.
    ''' </summary>
    ''' <param name="cal">The calendar to add to this store.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub CreateCalendar(ByVal cal As ScheduleCalendar)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        cal.Id = _server.CreateCalendar(cal)
        Calendars(cal.Id) = cal
        RaiseEvent CalendarsUpdated()
    End Sub

    ''' <summary>
    ''' Deletes the given calendar from this store.
    ''' </summary>
    ''' <param name="cal">The calendar to delete from this store</param>
    ''' <exception cref="InvalidOperationException">If the given calendar is 
    ''' being used in another record, eg. a trigger.</exception>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub DeleteCalendar(ByVal cal As ScheduleCalendar) _
     Implements IScheduleCalendarStore.DeleteCalendar
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        _server.DeleteCalendar(cal)
        Calendars.Remove(cal.Id)
        RaiseEvent CalendarsUpdated()
    End Sub


    ''' <summary>
    ''' Updates the given calendar. This will effectively remove and re-add any
    ''' data on the calendar ensuring that the calendar in the store has the same
    ''' state as that which is passed into the method.
    ''' </summary>
    ''' <param name="cal"></param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub UpdateCalendar(ByVal cal As ScheduleCalendar)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        _server.UpdateCalendar(cal)
        RaiseEvent CalendarsUpdated()
    End Sub

    ''' <summary>
    ''' Saves the given calendar to the database, either creating it or updating
    ''' it as necessary.
    ''' </summary>
    ''' <param name="cal">The calendar to save.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub SaveCalendar(ByVal cal As ScheduleCalendar) _
     Implements IScheduleCalendarStore.SaveCalendar
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        If cal.Id > 0 Then UpdateCalendar(cal) Else CreateCalendar(cal)
    End Sub

    ''' <summary>
    ''' Gets the calendar with the given ID
    ''' </summary>
    ''' <param name="id">The ID for which the calendar is required.</param>
    ''' <returns>The calendar represented by the specified ID.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Overridable Function GetCalendar(ByVal id As Integer) As ScheduleCalendar _
     Implements IScheduleCalendarStore.GetCalendar
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Return Calendars(id)
    End Function

    ''' <summary>
    ''' Gets the calendar from this store with the given name, or Nothing if
    ''' the given name did not represent a calendar in this store.
    ''' </summary>
    ''' <param name="name">The name of the calendar required</param>
    ''' <returns>The calendar held within this store with the given name or
    ''' Nothing if the name was not found.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Overridable Function GetCalendar(ByVal name As String) As ScheduleCalendar _
     Implements IScheduleCalendarStore.GetCalendar
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        For Each cal As ScheduleCalendar In Calendars.Values
            If name = cal.Name Then Return cal
        Next
        Return Nothing

    End Function

#End Region

#Region " Schedules "

    ''' <summary>
    ''' The scheduler which owns this store and thus any components which are 
    ''' returned from this store.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">When setting the owner, if this
    ''' store has been disposed.</exception>
    Public Property Owner() As IScheduler Implements IScheduleStore.Owner
        Get
            Return _owner
        End Get
        Set(ByVal value As IScheduler)
            If _disposed Then Throw New ObjectDisposedException("Store",
            "The schedule/calendar store has been disposed")
            _owner = value
            ' Check that we're not already set as the store in the scheduler
            ' to ensure there are no infinite loops entered into
            If value IsNot Nothing AndAlso Not Object.ReferenceEquals(Me, value.Store) Then
                value.Store = Me
            End If
        End Set
    End Property

    ''' <summary>
    ''' Saves the given schedule to this store.
    ''' Once saved, it is set such that it has no 'changed data'.
    ''' </summary>
    ''' <param name="schedule">The schedule to save.</param>
    ''' <exception cref="Server.Domain.Models.NameAlreadyExistsException">If either the name of the
    ''' schedule is already in use on the database, or if 2 tasks on the schedule
    ''' have the same name.</exception>
    ''' <remarks>
    ''' This will either create or update the schedule depending on whether it
    ''' is currently saved to the database or not.
    ''' </remarks>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub SaveSchedule(ByVal schedule As ISchedule) Implements IScheduleStore.SaveSchedule
        SaveSchedule(TryCast(schedule, SessionRunnerSchedule))
    End Sub

    ''' <summary>
    ''' Saves the given schedule to this store.
    ''' Once the schedule has been saved successfully, it will have the ID
    ''' from its database representation set in it, and its owner will be set
    ''' to the current scheduler (ie. the owner of this store).
    ''' </summary>
    ''' <param name="sched">The schedule to save.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub SaveSchedule(ByVal sched As SessionRunnerSchedule)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Dim newSched As SessionRunnerSchedule
        If sched.Id = 0 Then ' Not yet on the database
            newSched = _server.SchedulerCreateSchedule(sched)
            ' Ensure we update with the newly inserted schedule's ID
            ' The tasks are updated later
            sched.Id = newSched.Id
        Else
            newSched = _server.SchedulerUpdateSchedule(sched)
        End If

        ' Update the task IDs with the stuff we get back from the server (bug 8076)
        sched.SyncTaskIds(newSched)

        sched.Owner = Me.Owner
        sched.ResetChanged()
        _schedules(sched.Id) = sched
        RaiseEvent SchedulesUpdated()
    End Sub

    ''' <summary>
    ''' Triggers the given schedule - ie. marks it to run now.
    ''' </summary>
    ''' <param name="schedule">The schedule to trigger</param>
    ''' <returns>The exact date at which the schedule has been marked to execute.
    ''' Note that it may not run at this exact time, but this will be the time of
    ''' the new schedule instance created.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function TriggerSchedule(schedule As ISchedule) As Date Implements IScheduleStore.TriggerSchedule
        Dim triggerTimeInUtc = Now.ToUniversalTime()
        Return TriggerSchedule(schedule, triggerTimeInUtc)
    End Function

    Public Function TriggerSchedule(schedule As ISchedule, [when] As Date) As Date Implements IScheduleStore.TriggerSchedule
        If _disposed Then Throw New ObjectDisposedException("Store", "The schedule/calendar store has been disposed")

        Dim sessionSchedule As SessionRunnerSchedule = CType(schedule, SessionRunnerSchedule)

        Dim trigger As New Triggers.OnceTrigger([when]) With {
            .TimeZoneId = TimeZoneInfo.FindSystemTimeZoneById("UTC").Id
        }
        sessionSchedule.AddTrigger(trigger)
        sessionSchedule.AddAuditEvent(New ScheduleAuditEvent(
         ScheduleEventCode.ScheduleManuallyTriggered, sessionSchedule.Id, 0, Nothing, Nothing))

        SaveSchedule(sessionSchedule)

        Return trigger.Start
    End Function

    ''' <summary>
    ''' Deletes the given schedule from this store.
    ''' </summary>
    ''' <param name="schedule">The schedule to delete from this store.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub DeleteSchedule(ByVal schedule As ISchedule) Implements IScheduleStore.DeleteSchedule
        DeleteSchedule(TryCast(schedule, SessionRunnerSchedule))
    End Sub

    Public Function StopRunningSchedule(ByVal schedule As ISchedule) As Date
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Dim sched = CType(schedule, SessionRunnerSchedule)

        Dim trigger As New Triggers.StopTrigger(Now) With {
            .TimeZoneId = TimeZoneInfo.Local.Id
        }
        sched.AddTrigger(trigger)
        sched.AddAuditEvent(New ScheduleAuditEvent(
         ScheduleEventCode.ScheduleStopped, sched.Id, 0, Nothing, Nothing))

        SaveSchedule(sched)

        Return trigger.Start
    End Function

    ''' <summary>
    ''' Deletes the given schedule from this store if it has previously been
    ''' saved in this store.
    ''' Any null arguments or schedules which have not been saved to this store
    ''' are ignored.
    ''' </summary>
    ''' <param name="sched">The schedule to delete</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub DeleteSchedule(ByVal sched As SessionRunnerSchedule)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        ' No point in deleting it if we've never saved it...
        If sched IsNot Nothing AndAlso sched.Id <> 0 Then
            _schedules.Remove(sched.Id)
            _server.SchedulerDeleteSchedule(sched)
            RaiseEvent SchedulesUpdated()
        End If
    End Sub

    ''' <summary>
    ''' Retires the given schedule in this store.
    ''' </summary>
    ''' <param name="schedule">The schedule to retire in this store.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub RetireSchedule(ByVal schedule As ISchedule) Implements IScheduleStore.RetireSchedule
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Dim sched As SessionRunnerSchedule = CType(schedule, SessionRunnerSchedule)
        _schedules.Remove(sched.Id)
        _server.SchedulerRetireSchedule(sched)
        RaiseEvent SchedulesUpdated()
    End Sub

    ''' <summary>
    ''' Unretires the given schedule in this store.
    ''' </summary>
    ''' <param name="schedule">The schedule to unretire in this store.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub UnretireSchedule(ByVal schedule As ISchedule) Implements IScheduleStore.UnretireSchedule
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Dim sched As SessionRunnerSchedule = CType(schedule, SessionRunnerSchedule)
        _schedules.Add(sched.Id, sched)
        _server.SchedulerUnretireSchedule(sched)
        RaiseEvent SchedulesUpdated()
    End Sub

    ''' <summary>
    ''' Refreshes the given schedule with the latest version from the database,
    ''' or removes it if it has been deleted from the database.
    ''' If the schedule is updated from the database, its 'changed data' flag
    ''' is set to indicate 'no data changed'
    ''' </summary>
    ''' <param name="sched">The schedule to refresh.</param>
    ''' <returns>The given schedule's latest version on the database, or Nothing
    ''' if the given schedule has been deleted from the database.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function Refresh(ByVal sched As SessionRunnerSchedule) _
     As SessionRunnerSchedule
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Dim schedule As SessionRunnerSchedule = sched
        If _server.SchedulerRefreshIfChanged(schedule) Then
            ' Data has changed...
            Try
                If schedule Is Nothing Then
                    ' If the schedule is null, that means it's been deleted from
                    ' the database.
                    _schedules.Remove(sched.Id)
                    Return Nothing
                Else
                    ' Otherwise it's been changed by someone else - update our
                    ' cached version
                    _schedules(sched.Id) = schedule
                    schedule.Owner = _owner
                    schedule.ResetChanged()
                    Return schedule
                End If
            Finally
                ' Deleted or updated, end result is the same.
                RaiseEvent SchedulesUpdated()
            End Try
        End If
        Return sched
    End Function


    ''' <summary>
    ''' Gets the schedule with the given name from the database.
    ''' </summary>
    ''' <param name="name">The name for the schedule required.</param>
    ''' <returns>A schedule which is represented by the given name, or
    ''' Nothing if the schedule does not exist on the database.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetSchedule(ByVal name As String) As ISchedule _
     Implements IScheduleStore.GetSchedule
        Return GetSessionRunnerSchedule(name)
    End Function


    ''' <summary>
    ''' Gets the session runner schedule with the given name from the database.
    ''' </summary>
    ''' <param name="name">The name for the schedule required.</param>
    ''' <returns>A schedule which is represented by the given name, or Nothing if
    ''' the schedule does not exist on the database.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetSessionRunnerSchedule(ByVal name As String) As SessionRunnerSchedule

        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        For Each sched As SessionRunnerSchedule In _schedules.Values
            If sched.Name = name Then
                Return Refresh(sched)
            End If
        Next
        ' Not found, see if we can get it direct from the database.
        Dim schedule = TryCast(_server.SchedulerGetSchedule(name), SessionRunnerSchedule)
        If schedule IsNot Nothing Then
            schedule.Owner = Me.Owner
            schedule.ResetChanged()
            _schedules(schedule.Id) = schedule
            RaiseEvent SchedulesUpdated()
        End If

        Return schedule

    End Function

    ''' <summary>
    ''' Gets the schedule with the given ID from this store.
    ''' </summary>
    ''' <param name="id">The ID of the schedule required by this store.</param>
    ''' <returns>The schedule corresponding to the given ID or null if no such
    ''' schedule could be found in this store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetSchedule(ByVal id As Integer) As ISchedule
        Return GetSessionRunnerSchedule(id)
    End Function

    ''' <summary>
    ''' Gets the session runner schedule with the given ID from this store.
    ''' </summary>
    ''' <param name="id">The ID of the schedule required by this store.</param>
    ''' <returns>The schedule corresponding to the given ID or null if no such
    ''' schedule could be found in this store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetSessionRunnerSchedule(ByVal id As Integer) As SessionRunnerSchedule

        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        If _schedules.ContainsKey(id) Then
            Return Refresh(_schedules(id))
        Else
            Dim sched = TryCast(_server.SchedulerGetSchedule(id), SessionRunnerSchedule)
            If sched IsNot Nothing Then
                sched.Owner = Me.Owner
                sched.ResetChanged()
                _schedules(id) = sched
                RaiseEvent SchedulesUpdated()
            End If
            Return sched
        End If

    End Function

    ''' <summary>
    ''' Gets the active schedules from this store.
    ''' </summary>
    ''' <returns>A collection of active schedules for this store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetActiveSchedules() As ICollection(Of ISchedule) _
     Implements IScheduleStore.GetActiveSchedules

        Return New clsCovariantCollection(
         Of ISchedule, SessionRunnerSchedule)(GetActiveSessionRunnerSchedules())

    End Function

    ''' <summary>
    ''' Gets the active schedules from this store as a collection of specific
    ''' session runner schedules.
    ''' </summary>
    ''' <returns>The collection of clsSessionRunnerSchedules which are currently
    ''' active within this store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetActiveSessionRunnerSchedules() As ICollection(Of SessionRunnerSchedule)

        ' Refresh the data, inhibiting any events being fired.
        RefreshAll(False)

        ' Shallow clone so that any changes within the store don't break
        ' anything that wishes to enumerate the schedules.
        Return New List(Of SessionRunnerSchedule)(_schedules.Values)

    End Function

    ''' <summary>
    ''' Gets the retired schedules from this store.
    ''' </summary>
    ''' <returns>A collection of retired schedules from the database.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetRetiredSchedules() As ICollection(Of ISchedule)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim retired As ICollection(Of SessionRunnerSchedule) =
         _server.SchedulerGetRetiredSchedules()

        For Each sched As SessionRunnerSchedule In retired
            sched.Owner = Me.Owner
            sched.ResetChanged()
        Next
        Return New clsCovariantCollection(Of ISchedule, SessionRunnerSchedule)(retired)

    End Function

    ''' <summary>
    ''' Checks if the given task can be deleted from its owner schedule.
    ''' This may not be allowed if the task is referred to from elsewhere,
    ''' typically from schedule log entries.
    ''' A null task always returns false.
    ''' </summary>
    ''' <param name="task">The task to check to see if it can be deleted from
    ''' its schedule or not.</param>
    ''' <returns>True if the task is non-null, and is not referenced by any
    ''' table on the database which would cause the deletion to fail.
    ''' </returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function CanDeleteTask(ByVal task As ScheduledTask) As Boolean
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        If task Is Nothing Then Return False
        If task.HasTemporaryId() Then Return True
        Return _server.SchedulerCanDeleteTask(task.Id)

    End Function

#End Region

#Region " Logs and Logging "


    ''' <summary>
    ''' Creates a schedule log for the given trigger instance.
    ''' This will create the necessary database record, and initialise an open
    ''' session logger for SessionRunner schedules. The returned log will be
    ''' unstarted, but ready to log the progress of the schedule.
    ''' </summary>
    ''' <param name="instance">The instance which is causing the schedule to
    ''' execute.</param>
    ''' <returns>A schedule log ready to accept log events from the executing
    ''' session runner schedule.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function CreateLog(ByVal instance As ITriggerInstance) As IScheduleLog _
     Implements IScheduleStore.CreateLog

        Return CreateLog(instance, TriggerActivationReason.Execute)

    End Function

    ''' <summary>
    ''' Creates a schedule log for the given trigger instance.
    ''' This will create the necessary database record, and initialise an open
    ''' session logger for SessionRunner schedules. The returned log will be
    ''' unstarted, but ready to log the progress of the schedule.
    ''' </summary>
    ''' <param name="inst">The instance which is causing the schedule to
    ''' execute.</param>
    ''' <param name="reason">The reason for the log being created.</param>
    ''' <returns>A schedule log ready to accept logging events.</returns>
    ''' <exception cref="AlreadyActivatedException">If an entry was found on the
    ''' database for the specified schedule at the specified instance time.
    ''' </exception>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function CreateLog(
     ByVal inst As ITriggerInstance, ByVal reason As TriggerActivationReason) As IScheduleLog
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim sched As SessionRunnerSchedule =
         DirectCast(inst.Trigger.Schedule, SessionRunnerSchedule)

        Dim logId As Integer = _server.SchedulerCreateLog(sched.Id, inst.When, reason, Owner.Name)

        Return New ScheduleLogger(logId, inst)

    End Function

    ''' <summary>
    ''' Gets the historical schedule log for the given instant.
    ''' Note that the log returned by this method will <em>not</em> allow the
    ''' Start() or Stop() methods to be called - it should be considered
    ''' read-only by any calling code.
    ''' </summary>
    ''' <param name="instant">The instant for which a log is required.</param>
    ''' <returns>A read-only historical log representing the given schedule for
    ''' the given instant.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetLog(ByVal schedule As ISchedule, ByVal instant As Date) As IScheduleLog _
     Implements IScheduleStore.GetLog
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim log As HistoricalScheduleLog =
         _server.SchedulerGetLog(CType(schedule, SessionRunnerSchedule).Id, instant)

        If log IsNot Nothing Then log.Schedule = schedule

        Return log

    End Function

    ''' <summary>
    ''' Utility method to get the required logs as a collection of their concrete
    ''' type rather than any interface they happen to implement.
    ''' Using this and the clsCovariantCollections, the same data can be
    ''' represented however it needs to be.
    ''' </summary>
    ''' <param name="schedule">The schedule for which logs are required.</param>
    ''' <param name="after">The date after which the logs are required.</param>
    ''' <param name="before">The date before which the logs are required.</param>
    ''' <returns>A set of historical logs for the given schedule's executions
    ''' between the specified dates.</returns>
    ''' <remarks>Note that this only gets 'executed' instances - ie. misfires are
    ''' not included in the returned collection.</remarks>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetSpecificLogs(schedule As ISchedule, after As Date, before As Date) As IBPSet(Of HistoricalScheduleLog)
        If _disposed Then Throw New ObjectDisposedException("Store", "The schedule/calendar store has been disposed")
        Dim logs As New clsSortedSet(Of HistoricalScheduleLog)
        Dim scheduleId = CType(schedule, SessionRunnerSchedule).Id

        ' need to widen the date time query parameters to allow for time zone variances
        Dim scheduleLogs = _server.SchedulerGetLogs(scheduleId, after.AddDays(-1), before.AddDays(1), TriggerActivationReason.Execute)
        For Each log As HistoricalScheduleLog In scheduleLogs
            log.Schedule = schedule
            log.InstanceTime = TimeZoneInfo.ConvertTimeFromUtc(log.StartTime, TimeZoneInfo.Local)
            If IsInDateRange(after, before, log.InstanceTime) Then
                logs.Add(log)
            End If
        Next
        Return logs
    End Function

    ''' <summary>
    ''' Gets the historical logs for the given schedule between the given dates
    ''' / times (exclusive).
    ''' </summary>
    ''' <param name="schedule">The schedule for which the logs are required.
    ''' </param>
    ''' <param name="after">The date after which the logs are required.</param>
    ''' <param name="before">The date before which the logs are required.</param>
    ''' <returns>A set of schedule logs.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetLogs(
     ByVal schedule As ISchedule,
     ByVal after As Date,
     ByVal before As Date) As IBPSet(Of IScheduleLog) Implements IScheduleStore.GetLogs
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Return New clsCovariantSet(Of IScheduleLog, HistoricalScheduleLog)(
         GetSpecificLogs(schedule, after, before))

    End Function

    ''' <summary>
    ''' Gets a report for all the given schedules in between the 2 specified
    ''' dates.
    ''' </summary>
    ''' <param name="schedules">The schedules for which the report is required.
    ''' </param>
    ''' <param name="after">The date after which the schedules executions are
    ''' required.</param>
    ''' <param name="before">The date before which the schedules executions are
    ''' required.</param>
    ''' <returns>A collection of IScheduleInstance objects which provide the
    ''' report for the given schedules' executions between the specified dates,
    ''' in ascending instance date order
    ''' </returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetReport(schedules As ICollection(Of SessionRunnerSchedule), after As Date, before As Date) As ICollection(Of IScheduleInstance)
        If _disposed Then Throw New ObjectDisposedException("Store", "The schedule/calendar store has been disposed")

        Dim allLogs As New clsSortedSet(Of HistoricalScheduleLog)

        For Each schedule As SessionRunnerSchedule In schedules
            Dim logs = GetSpecificLogs(schedule, after, before)
            allLogs.Union(logs)
        Next

        Return New clsCovariantSet(Of IScheduleInstance, HistoricalScheduleLog)(allLogs)
    End Function

    ''' <summary>
    ''' Gets the timetable for the given schedules in between the given dates.
    ''' </summary>
    ''' <param name="schedules">The schedules for which the timetable is
    ''' required.</param>
    ''' <param name="after">The date after which the timetable is required.
    ''' </param>
    ''' <param name="before">The date before which the timetable is required.
    ''' </param>
    ''' <returns>A collection of schedule instances which occur between the
    ''' given dates, in ascending instance date order.</returns>
    ''' <remarks>Note that the instances returned from this method are assumed
    ''' to have not run, thus they will each have a status of 'pending' and no
    ''' start date and end date.</remarks>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetTimetable(schedules As ICollection(Of SessionRunnerSchedule), after As Date, before As Date) As ICollection(Of IScheduleInstance)
        If _disposed Then Throw New ObjectDisposedException("Store", "The schedule/calendar store has been disposed")
        Dim tts As New clsSortedSet(Of IScheduleInstance)

        For Each schedule As SessionRunnerSchedule In schedules
            Dim afterDate = after
            Dim beforeDate = before
            Dim primaryMetaData = schedule.Triggers?.PrimaryMetaData
            Dim scheduleTimeZoneId = primaryMetaData?.TimeZoneId
            Dim scheduleUtcOffset = primaryMetaData?.UtcOffset

            If scheduleUtcOffset IsNot Nothing Then
                afterDate = after - (TimeZoneInfo.Local.BaseUtcOffset - scheduleUtcOffset.Value)
                beforeDate = before - (TimeZoneInfo.Local.BaseUtcOffset - scheduleUtcOffset.Value)
            ElseIf scheduleTimeZoneId <> Nothing AndAlso scheduleTimeZoneId <> TimeZoneInfo.Local.Id Then
                afterDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(after, TimeZoneInfo.Local.Id, scheduleTimeZoneId)
                beforeDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(before, TimeZoneInfo.Local.Id, scheduleTimeZoneId)
            End If

            Dim triggerGroupInstances = schedule.Triggers.GetInstances(afterDate, beforeDate)

            For Each instance In triggerGroupInstances
                Dim clientInstanceTime? = GenerateClientInstanceTime(instance)

                If clientInstanceTime Is Nothing Then Continue For

                If instance.Mode = TriggerMode.Fire Then
                    tts.Add(New ScheduleTimetableInstance(instance, clientInstanceTime.Value))
                End If
            Next
        Next
        Return tts
    End Function

    Private Function GenerateClientInstanceTime(instance As ITriggerInstance) As Date?
        Dim primaryMetaData = instance.Trigger?.PrimaryMetaData
        Dim triggerTimeZoneId = primaryMetaData?.TimeZoneId
        Dim triggerUtcOffset = primaryMetaData?.UtcOffset

        If triggerUtcOffset IsNot Nothing Then
            Return instance.When - (triggerUtcOffset.Value - TimeZoneInfo.Local.BaseUtcOffset)
        End If

        If Not instance.IsTimeValid() Then
            Return Nothing
        End If

        If triggerTimeZoneId <> Nothing AndAlso triggerTimeZoneId <> TimeZoneInfo.Local.Id Then
            Return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instance.When, triggerTimeZoneId, TimeZoneInfo.Local.Id)
        End If

        Return instance.When
    End Function

    Private Function IsInDateRange(ByVal afterDate As Date, ByVal beforeDate As Date, ByVal instanceDate As Date) As Boolean
        If instanceDate >= afterDate AndAlso instanceDate < beforeDate Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Gets the list entries for the given list.
    ''' Note that the list must be fully populated when calling this method or
    ''' it won't actually work.
    ''' </summary>
    ''' <param name="list">The schedule list for which the schedule instance
    ''' entries are required</param>
    ''' <returns>A collection of schedule instances which were found within the
    ''' constraints defined in the given list.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetListEntries(list As ScheduleList) _
     As ICollection(Of IScheduleInstance)
        Return GetListEntries(list, list.GetStartDate(), list.GetEndDate())
    End Function

    ''' <summary>
    ''' Gets the list entries corresponding to the given list, except with the
    ''' date range overridden to the provided individual values rather than the
    ''' dates specified in the list.
    ''' </summary>
    ''' <param name="list">The list providing the details of the entries provided
    ''' (eg. type, schedules).</param>
    ''' <param name="startDate">The start date for which entries from the list
    ''' should be provided. This value will override any date set in the given
    ''' list itself.</param>
    ''' <param name="endDate">The end date for which entries from the list should
    ''' be provided. This value will override any date set in the given list 
    ''' itself.</param>
    ''' <returns>The collection of schedule instance objects which correspond to
    ''' the schedules and type of entry specified in the list, and which occur
    ''' between the two specified dates.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetListEntries(
     list As ScheduleList, startDate As Date, endDate As Date) _
     As ICollection(Of IScheduleInstance)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        '' Dock 1 second off the start date - it's inclusive whereas 'after'
        '' is exclusive. This makes sure they're talking about the same thing
        If startDate <> Date.MinValue Then startDate = startDate.AddSeconds(-1)

        If list.ListType = ScheduleListType.Report Then
            Return GetReport(list.Schedules, startDate, endDate)
        Else
            Return GetTimetable(list.Schedules, startDate, endDate)
        End If

    End Function

    ''' <summary>
    ''' Gets all the currently held lists of the given type from this store.
    ''' </summary>
    ''' <param name="type">The type of lists required.</param>
    ''' <returns>A collection of schedule lists of the specified type from this
    ''' store.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetAllLists(ByVal type As ScheduleListType) _
     As ICollection(Of ScheduleList)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim lists As ICollection(Of ScheduleList) = _server.SchedulerGetScheduleLists(type)
        For Each entry As ScheduleList In lists
            entry.Store = Me
            entry.ResetChanged()
        Next
        Return lists

    End Function

    ''' <summary>
    ''' Gets the schedule list for the given id.
    ''' </summary>
    ''' <param name="id">The id of the schedulelist to get.
    ''' </param>
    ''' <returns>A fully populated schedule list</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetScheduleList(ByVal id As Integer) As ScheduleList
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim list As ScheduleList = _server.SchedulerGetScheduleList(id)
        If list Is Nothing Then Return Nothing
        list.Store = Me
        list.ResetChanged()
        Return list

    End Function

    ''' <summary>
    ''' Gets the full schedule list corresponding to the given name and type
    ''' or null if no such list exists.
    ''' </summary>
    ''' <param name="name">The name of the list required.</param>
    ''' <param name="type">The type of the list required.</param>
    ''' <returns>The fully populated schedule list corresponding to the given
    ''' name and type, or null if no such list exists.</returns>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Function GetScheduleList(ByVal name As String, ByVal type As ScheduleListType) _
     As ScheduleList
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")

        Dim list As ScheduleList = _server.SchedulerGetScheduleList(name, type)
        If list Is Nothing Then Return Nothing
        list.Store = Me
        list.ResetChanged()
        Return list

    End Function

    ''' <summary>
    ''' Saves a schedule list
    ''' </summary>
    ''' <param name="list">The schedule list to save</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub SaveScheduleList(ByVal list As ScheduleList)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        If list.ID = 0 Then ' Not yet on the database
            list.ID = _server.SchedulerCreateScheduleList(list)
        Else
            _server.SchedulerUpdateScheduleList(list)
        End If
        list.ResetChanged()
    End Sub

    ''' <summary>
    ''' Deletes a schedule list
    ''' </summary>
    ''' <param name="list">The schedule list to Delete</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub DeleteScheduleList(ByVal list As ScheduleList)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        _server.SchedulerDeleteScheduleList(list.ID)
    End Sub

#End Region

#Region " Alerts "

    ''' <summary>
    ''' Creates an alert of the specified type for the given schedule.
    ''' </summary>
    ''' <param name="type">The type of alert to create</param>
    ''' <param name="sched">The schedule that the alert refers to.</param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub CreateScheduleAlert(
     ByVal type As AlertEventType, ByVal sched As SessionRunnerSchedule)
        CreateAlert(type, sched.Id, 0)
    End Sub

    ''' <summary>
    ''' Creates an alert of the specified type for the given task.
    ''' </summary>
    ''' <param name="type">The type of alert to create</param>
    ''' <param name="task">The task that the alert refers to. Note that if the
    ''' task is not on the database (ie. has a temporary ID), it will be ignored.
    ''' </param>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Public Sub CreateTaskAlert(
     ByVal type As AlertEventType, ByVal task As ScheduledTask)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        If Not task.HasTemporaryId() Then CreateAlert(type, task.Owner.Id, task.Id)
    End Sub

    ''' <summary>
    ''' Creates an alert of the specified type for the given schedule and task
    ''' IDs.
    ''' </summary>
    ''' <param name="type">The type of alert event to create</param>
    ''' <param name="schedId">The ID of the affected schedule. This should never
    ''' be zero - even if the alert is task-based, its owner schedule should be
    ''' referenced.</param>
    ''' <param name="taskId">The ID of the affected task, zero if not applicable.
    ''' </param>
    ''' <exception cref="ArgumentException">If the given schedule ID is zero.
    ''' </exception>
    ''' <exception cref="ObjectDisposedException">If this store has been disposed.
    ''' </exception>
    Private Sub CreateAlert(
     ByVal type As AlertEventType, ByVal schedId As Integer, ByVal taskId As Integer)
        If _disposed Then Throw New ObjectDisposedException("Store",
         "The schedule/calendar store has been disposed")
        Try
            _server.CreateScheduleAlert(type, schedId, taskId)

        Catch ae As ArgumentException
            ' Rethrow any argument exceptions - these are programming errors
            Throw

        Catch ex As Exception
            ' Not sure what to do here - we could chain the exception to the caller,
            ' but I'm not convinced that alerts are important enough to kill a
            ' schedule over, so this near-silently ignores it, printing out the
            ' the exception to the debug log.
            Debug.Print(ex.ToString())

        End Try

    End Sub

    '''<summary>
    ''' The server time zone ID from the TimeZoneInfo library.
    ''' </summary>
    ''' <returns>
    ''' The string ID of the server's time zone.
    ''' </returns>
    Private Function GetServerTimeZone() As TimeZoneInfo Implements IScheduleStore.GetServerTimeZone
        Return TimeZoneInfo.FindSystemTimeZoneById(_server.GetServerTimeZoneId())
    End Function

#End Region

End Class
