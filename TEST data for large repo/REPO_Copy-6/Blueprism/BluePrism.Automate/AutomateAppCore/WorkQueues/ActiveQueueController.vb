Imports System.Collections.Concurrent
Imports System.Threading
Imports NLog

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Logging
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.AutomateAppCore.Sessions

''' <summary>
''' Class to represent a controller of an active queue. This provides a place where
''' the queue can be monitored and provides control methods to start, stop and report
''' on the sessions being worked on behalf of an active queue.
''' </summary>
''' <seealso cref="ActiveQueueManager"/>
Public Class ActiveQueueController : Implements IDisposable

#Region " Class-scope Declarations "

    Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()

    ''' <summary>
    ''' Placeholder class to represent a session which is being created / started.
    ''' </summary>
    Private Class SessionPlaceholder
        ' The machine on which the session is created
        Public Property Machine As ResourceMachine
        ' The ID of the process run in the session
        Public Property ProcessId As Guid
        ' The session ID, once known
        Public Property Id As Guid
        ' The attempt number for this session creation
        Public Property Attempt As Integer
    End Class

    ''' <summary>
    ''' Maximum number of session attempts allowed for a single Start() call.
    ''' </summary>
    Private Const MaxStartSessionAttempts As Integer = 3

    ''' <summary>
    ''' Class for use in a 'Using' block which acquires a lock on an active queue,
    ''' and releases it when exiting the block (ie. when Disposed).
    ''' Note that if a lock cannot be acquired, this will not block until such time
    ''' that it can - it will immediately fail with an
    ''' <see cref="ActiveQueueLockFailedException"/> when attempting to create the
    ''' lock object.
    ''' </summary>
    Private Class ActiveQueueControlLock : Implements IDisposable

        ' The controller holding this lock
        Private mCont As ActiveQueueController
        ' The lock token
        Private mToken As Guid

        ''' <summary>
        ''' Creates a new lock on the queue with the given identity.
        ''' </summary>
        ''' <param name="cont">The controller creating the lock</param>
        ''' <exception cref="ActiveQueueLockFailedException">If the lock could not
        ''' be acquired on the required queue.</exception>
        Public Sub New(cont As ActiveQueueController)
            mCont = cont
            ' Get the token from the database
            mToken = gSv.AcquireActiveQueueLock(cont.Queue.Ident)
        End Sub

        ''' <summary>
        ''' Disposes of this object, releasing the acquired active queue lock
        ''' </summary>
        Private Sub Dispose() Implements IDisposable.Dispose
            gSv.ReleaseActiveQueueLock(mCont.Queue.Ident, mToken)
        End Sub

    End Class

#End Region

#Region " Events "

    ''' <summary>
    ''' Event fired when an active session has been started by this controller.
    ''' </summary>
    Public Event ActiveSessionStarted As ActiveQueueSessionEventHandler

    ''' <summary>
    ''' Event raised when an attempt to start an active session failed for any
    ''' reason.
    ''' </summary>
    Public Event ActiveSessionStartFailed As ActiveQueueSessionEventHandler

    ''' <summary>
    ''' Event fired when an active session has been ended by this controller.
    ''' </summary>
    Public Event ActiveSessionEnded As ActiveQueueSessionEventHandler

    ''' <summary>
    ''' Event fired when an error occurs while attempting to start/stop sessions
    ''' within the background thread maintained by an active queue controller.
    ''' </summary>
    Public Event ActiveQueueError As ActiveQueueErrorEventHandler

#End Region

#Region " Member Variables "

    ' The queue that this object is controlling
    Private mQueue As clsWorkQueue

    ' The thread which actually aims towards the target for this controller
    Private mAimingThread As Thread

    ' Stop request for the aiming thread
    Private mAimingThreadStop As Boolean

    ' The target, set by the user (ie. not changed by external events) or null if
    ' the user set target is being or has been processed
    Private mUserSetTarget As Integer?

    ' The amount that the target should change by - this is set when a session ends
    ' which was not stop-requested by this controller
    Private mTargetDecrementBy As Integer

    ' The owner of this queue controller
    Private mOwner As ActiveQueueManager

    ' The manager used to connect to the resources
    Private WithEvents mConnManager As IResourceConnectionManager

    ' Flag indicating if this controller is disposed
    Private mDisposed As Boolean

    ' Map of sessions awaiting be created, keyed on the tag used to create them
    Private mAwaitingCreate As New ConcurrentDictionary(Of Guid, SessionPlaceholder)

    ' Map of created sessions awaiting being started, keyed on session ID
    Private mAwaitingStart As New ConcurrentDictionary(Of Guid, SessionPlaceholder)

    ' A set (kinda) of sessions which have been stop-requested and are awaiting end
    Private mAwaitingEnd As New ConcurrentDictionary(Of Guid, Byte)

    ' Lock used to control access to the target sessions of a queue
    Private ReadOnly mTargetLock As New Object()

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new active queue controller.
    ''' </summary>
    ''' <param name="mgr">The active queue manager which owns this controller</param>
    ''' <param name="q">The queue to use in this controller.</param>
    Friend Sub New(mgr As ActiveQueueManager, q As clsWorkQueue)
        Manager = mgr
        mQueue = q
        mAimingThread = New Thread(AddressOf ProcessTargetChangeThread) With {
            .Name = "Active Queue Target Processor for queue: " & q.Ident,
            .IsBackground = True
        }
        mAimingThread.Start()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The owner of this active queue controller
    ''' </summary>
    ''' <exception cref="ArgumentNullException">If setting with a null value.
    ''' </exception>
    Friend Property Manager As ActiveQueueManager
        Get
            Return mOwner
        End Get
        Private Set(value As ActiveQueueManager)
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value),
                "Cannot set a null queue manager into a queue controller")
            mOwner = value
            mConnManager = value.ConnectionManager
        End Set
    End Property

    ''' <summary>
    ''' Gets the queue associated with this controller
    ''' </summary>
    ''' <exception cref="ArgumentNullException">If setting with a null value.
    ''' </exception>
    Public Property Queue As clsWorkQueue
        Friend Set(value As clsWorkQueue)
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value),
                "Cannot set a null queue into a queue controller")
            mQueue = value
        End Set
        Get
            Return mQueue
        End Get
    End Property

    ''' <summary>
    ''' The resource group configured in the work queue
    ''' </summary>
    Private ReadOnly Property ResourceGroup As IGroup
        Get
            Return mQueue.ResourceGroup
        End Get
    End Property

    ''' <summary>
    ''' The ID of the process used to serve the work queue
    ''' </summary>
    Private ReadOnly Property ProcessId As Guid
        Get
            Return mQueue.ProcessId
        End Get
    End Property

    ''' <summary>
    ''' Gets the resource machines representing the resources in the group assigned
    ''' to the active queue.
    ''' </summary>
    Public ReadOnly Property Resources As ICollection(Of ResourceMachine)
        Get
            Dim groupedResourceNames = CollectionUtil.Convert(
                ResourceGroup.FlattenedContents(Of clsSortedSet(Of IGroupMember))(False), Function(r) r.Name)

            Dim machines As New List(Of ResourceMachine)

            For Each mach As ResourceMachine In mConnManager.GetResources().Values
                If groupedResourceNames.Contains(mach.Name) Then machines.Add(mach)
            Next

            Return machines
        End Get
    End Property

    Public Function ActiveResourcesForQueue(resources As ICollection(Of IResourceMachine)) As ICollection(Of IResourceMachine)
        Dim groupedResourceNames = CollectionUtil.Convert(
            ResourceGroup.FlattenedContents(Of clsSortedSet(Of IGroupMember))(False), Function(r) r.Name)

        Return resources.Where(Function(x) groupedResourceNames.Contains(x.Name)).ToList()

    End Function

    Public Sub StartMonitoring()
        mAimingThread = New Thread(AddressOf ProcessTargetChangeThread) With {
           .Name = "Active Queue Target Processor for queue: " & mQueue.Ident,
           .IsBackground = True
        }
        mAimingThread.Start()
    End Sub

    ''' <summary>
    ''' Gets whether this controller is disposed
    ''' </summary>
    Public ReadOnly Property IsDisposed As Boolean
        Get
            Return mDisposed
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of sessions awaiting create or start in this controller
    ''' </summary>
    Public ReadOnly Property AwaitingSessionCount As Integer
        Get
            Return (mAwaitingCreate.Count + mAwaitingStart.Count)
        End Get
    End Property

    ''' <summary>
    ''' Processes the aiming for target, waking up if the target changes by virtue
    ''' of a <see cref="Monitor.Pulse">Pulse</see> on the target lock.
    ''' </summary>
    Private Sub ProcessTargetChangeThread()
        SyncLock mTargetLock
            While Not mAimingThreadStop AndAlso Not mDisposed
                If mUserSetTarget.HasValue Then
                    Try
                        AimForTargetSessions()
                    Catch aqlf As ActiveQueueLockFailedException
                        ' If the archive lock failed, wait 1 and try again
                        Monitor.Wait(mTargetLock, TimeSpan.FromSeconds(1))
                        Continue While
                    Catch ex As Exception
                        Try
                            OnActiveQueueError(
                                New ActiveQueueErrorEventArgs(Queue.Ident, ex))
                        Catch exAgain As Exception
                            ' We have to swallow any errors that occur in handlers
                            ' to ensure that we don't break the thread that is
                            ' actually handling the queues, because it would never
                            ' get restarted again.
                            ' Just ensure that developers know that something bad
                            ' is occurring.
                            Debug.Fail(exAgain.ToString())
                        End Try
                    End Try

                ElseIf mTargetDecrementBy <> 0 Then
                    Dim targ As Integer =
                        Math.Max(0, Queue.TargetSessionCount - mTargetDecrementBy)

                    Queue.TargetSessionCount = targ
                    gSv.SetTargetSessionCount(Queue.Ident, targ)

                    mTargetDecrementBy = 0

                End If
                Monitor.Wait(mTargetLock, TimeSpan.FromSeconds(30))
            End While
        End SyncLock
    End Sub

    ''' <summary>
    ''' Compares the current state with the target number of sessions and
    ''' asynchronously sends create/start/stop-requests to the resources to try and
    ''' reach that target. All commands are asynchronous in one form or another
    ''' (a different thread for create/start requests, setting a flag for stop
    ''' requests) and control is returned after all requests have been sent.
    ''' If another thread is currently aiming, this will have no effect.
    ''' </summary>
    ''' <exception cref="ActiveQueueLockFailedException">If this method failed to
    ''' acquire a controller lock on the active queue, implying that some other
    ''' routine (or machine) is controlling the queue at the moment. The details are
    ''' in the exception message.</exception>
    ''' <remarks>This is called from within a background thread maintained by this
    ''' controller and assumes that it has a sync lock on <see cref="mTargetLock"/>.
    ''' Basically, only call this from within the loop in
    ''' <see cref="ProcessTargetChangeThread"/> called within the
    ''' <see cref="mAimingThread"/> thread. If it must be called elsewhere, ensure
    ''' that a lock on mTargetLock is acquired before entering it.</remarks>
    Private Sub AimForTargetSessions()
        Debug.Print(
            "mConnManager.AimForTargetSessions [Queue:{0}; Target:{1}]",
            Queue.Name,
            TargetSessions
        )

        ' We need to be locked into this active queue in order to do this processing
        Using New ActiveQueueControlLock(Me)

            Debug.Assert(mUserSetTarget.HasValue)

            ' Take the user-set target and reset it to null
            Dim target As Integer = mUserSetTarget.Value
            mUserSetTarget = Nothing

            ' Tests the process's run mode is valid
            CheckRequiredRunMode(ProcessId)

            ' Get the latest data from the database
            Queue.UpdateActiveData()

            ' If the database has the appropriate value for the target, don't
            ' bother updating it, otherwise update both object and database
            If Queue.TargetSessionCount <> target Then
                ' set the new value into the queue
                Queue.TargetSessionCount = target
                ' ... and the database ...
                gSv.SetTargetSessionCount(Queue.Ident, target)

                ' If the target is already set correctly on the queue, we still
                ' want to go through the motions of checking the sessions to
                ' ensure we have the right count at the end of this method
            End If

            Dim currActiveCount As Integer = TotalKnownActiveSessionCount

            ' Ignore the stopping sessions... they're on borrowed time and are
            ' already "targeted to be stopped"
            currActiveCount -= Queue.StoppingSessions.Count

            ' If that's exactly what we've got (or, will have after the stopping
            ' sessions have stopped), there's nothing else for us to do
            If target = currActiveCount Then Return

            ' Get the running sessions and check the target against them
            Dim running = Queue.RunningSessions
            If target > currActiveCount Then
                ' If more sessions have been requested, start them
                For i As Integer = currActiveCount To target - 1
                    ' Start asynchronously, invoke a session started event when
                    ' the session has started
                    StartActiveSessionAsync()
                Next

            ElseIf target < currActiveCount Then
                ' If less sessions have been targeted, request to stop some
                StopActiveSessionsAsync(currActiveCount - target, target)

            End If

        End Using

    End Sub

    ''' <summary>
    ''' Gets or sets the target sessions to aim for in this controller.
    ''' </summary>
    Public Property TargetSessions As Integer
        Get
            If mUserSetTarget.HasValue Then Return mUserSetTarget.Value
            Return Queue.TargetSessionCount
        End Get
        Private Set(value As Integer)
            SyncLock mTargetLock
                ' If the required value is different to what we already have
                If TargetSessions <> value Then
                    mUserSetTarget = value
                    Monitor.PulseAll(mTargetLock)
                End If
            End SyncLock
        End Set
    End Property

    ''' <summary>
    ''' Gets the number of sessions, known about by this controller, which are either
    ''' running (including those for which a stop has been requested), pending or
    ''' awaiting creation within this controller. It cannot know about those which
    ''' are awaiting creation in other controllers.
    ''' </summary>
    Public ReadOnly Property TotalKnownActiveSessionCount As Integer
        Get
            ' Get the session IDs created but awaiting start
            Dim sessionIds = New clsSet(Of Guid)(
                                mAwaitingStart.Values.Select(Function(ph) ph.Id))
            ' add all the the active session IDs (may overlap)
            sessionIds.Union(Queue.ActiveSessionIds)

            ' the known active session count is all of those plus the ones that
            ' are awaiting being created
            Return (sessionIds.Count + mAwaitingCreate.Count)
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Sets the target sessions for this active queue controller and begins the work
    ''' of reaching that number, either by creating new sessions or by stopping
    ''' existing ones.
    ''' </summary>
    ''' <param name="targetSess">The target number of sessions to aim for.</param>
    Public Sub AimFor(targetSess As Integer)
        TargetSessions = targetSess
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveSessionStarted"/> event
    ''' </summary>
    Protected Overridable Sub OnActiveSessionStarted(e As ActiveQueueSessionEventArgs)
        RaiseEvent ActiveSessionStarted(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveSessionEnded"/> event
    ''' </summary>
    Protected Overridable Sub OnActiveSessionEnded(e As ActiveQueueSessionEventArgs)
        RaiseEvent ActiveSessionEnded(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveSessionStartFailed"/> event.
    ''' </summary>
    Protected Overridable Sub OnActiveSessionStartFailed(e As ActiveQueueSessionEventArgs)
        RaiseEvent ActiveSessionStartFailed(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveQueueError"/> event
    ''' </summary>
    Protected Overridable Sub OnActiveQueueError(e As ActiveQueueErrorEventArgs)
        Log.Error(e.Exception, "Error occurred in queue controller for: {0}", LogOutputHelper.Sanitize(Queue.Name))
        RaiseEvent ActiveQueueError(Me, e)
    End Sub

    ''' <summary>
    ''' Checks if this controller is currently awaiting a session create / start on a
    ''' specific resource machine.
    ''' </summary>
    ''' <param name="rm">The resource machine to check for awaiting sessions.</param>
    ''' <returns>True if there is a session awaiting creation / starting for the
    ''' given resource machine within this controller.</returns>
    ''' <remarks>This assumes reference equality in the resource machines held in
    ''' this controller and given to this method.</remarks>
    Private Function IsAwaitingSessionOn(rm As ResourceMachine) As Boolean
        For Each ph As SessionPlaceholder In mAwaitingCreate.Values
            If ph.Machine Is rm Then Return True
        Next
        For Each ph As SessionPlaceholder In mAwaitingStart.Values
            If ph.Machine Is rm Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Checks if a resource machine currently has a running, pending or creating
    ''' session for the queue controlled by this controller.
    ''' </summary>
    ''' <param name="rm">The resource machine to check for an active session on.
    ''' </param>
    ''' <returns>True if the given resource has a session awaiting create, awaiting
    ''' start or running (including those in the process of stopping).</returns>
    Private Function IsActiveSessionOn(rm As ResourceMachine) As Boolean
        For Each sess As clsProcessSession In Queue.RunningSessions
            If sess.ResourceName = rm.Name Then Return True
        Next
        Return IsAwaitingSessionOn(rm)
    End Function

    ''' <summary>
    ''' Starts a session on an arbitrary resource in a specified group
    ''' </summary>
    ''' <exception cref="ResourceUnavailableException">If no resources in the given
    ''' group were available to start a session in</exception>
    Private Sub StartActiveSessionAsync()
        StartActiveSessionAsync(1, Nothing)
    End Sub

    ''' <summary>
    ''' Starts a session on an arbitrary resource in a specified group
    ''' </summary>
    ''' <param name="attemptNo">The attempt number for this session starting</param>
    ''' <param name="avoidIfPoss">The resource machine to avoid, if possible. This is
    ''' set if a session create/start fails on a resource; if set, this method will
    ''' <em>prefer</em> a different resource from those available - if there are no
    ''' other resources available, and the one to avoid is available, this method
    ''' will attempt to use it anyway.</param>
    ''' <exception cref="ResourceUnavailableException">If no resources in the given
    ''' group were available to start a session in</exception>
    Private Sub StartActiveSessionAsync(
     attemptNo As Integer, avoidIfPoss As ResourceMachine)
        ' Get the resources in 'busy-ness' order with the ones currently doing the
        ' least work at the head of the collection
        Dim connected =
            From r In Resources
            Where r.IsConnected AndAlso Not IsActiveSessionOn(r)
            Select r
            Order By r.ProcessesPending + r.ProcessesRunning Ascending

        ' Try getting the first resource machine that is not the one we are trying to avoid
        Dim rm = connected.FirstOrDefault(Function(m) m IsNot avoidIfPoss)

        ' If that's the only one there, we'll use it, but grudgingly
        If rm Is Nothing Then rm = connected.FirstOrDefault()

        ' If we have no resources left to try, we can't start any sessions
        If rm Is Nothing Then Throw New ResourceUnavailableException()

        Dim awaiter As New SessionPlaceholder() With {
            .Machine = rm,
            .ProcessId = mQueue.ProcessId,
            .Attempt = attemptNo
        }
        Dim key As Guid = Guid.NewGuid
        mAwaitingCreate(key) = awaiter

        mConnManager.SendCreateSession({New CreateSessionData(rm.Id, mQueue.ProcessId, mQueue.Ident, key, Nothing)})
    End Sub

    ''' <summary>
    ''' Requests a stop on a single active session operating on behalf of the queue
    ''' being controlled by this controller.
    ''' </summary>
    Private Sub StopActiveSessionAsync()
        StopActiveSessionsAsync(1)
    End Sub

    ''' <summary>
    ''' Requests a stop on a number of active sessions operating on behalf of
    ''' the queue being controlled by this controller.
    ''' </summary>
    ''' <param name="num">The number of sessions to request a stop on</param>
    Private Sub StopActiveSessionsAsync(num As Integer, Optional target As Integer? = Nothing)
        Dim sessions As ICollection(Of clsProcessSession) = Queue.ContinuingSessions

        ' Some sessions may have already finished, therefore we should aim to match target sessions
        If (target IsNot Nothing) Then
            If num > sessions.Count Then
                If sessions.Count - target.Value >= 0 Then
                    num = sessions.Count - target.Value
                Else
                    num = 0
                End If
            End If
        End If

        If num < 0 OrElse num > sessions.Count Then Throw New ArgumentOutOfRangeException(
            NameOf(num), String.Format("value must be between 0 and {0}; value was {1}", sessions.Count, num))

        ' Request a stop on the oldest sessions first
        Dim i As Integer = 0
        For Each sess In sessions.OrderBy(Function(s) s.SessionStart)
            ' If we've reached our limit, exit now
            If i >= num Then Exit For
            ' Save the session ID into the 'awaiting end' "set"
            mAwaitingEnd(sess.SessionID) = 0
            ' And request a stop - if it fails for any reason, remove it from the set
            If Not gSv.RequestStopSession(sess.SessionNum) Then _
             mAwaitingEnd.TryRemove(sess.SessionID, Nothing)

            i += 1
        Next

        ' At this point, 'i' should equal the number of sessions requested to be
        ' stopped. If it's less, then we ran out of sessions to stop.
        ' FIXME: What do we do about that? Anything we can do? An exception?
        ' It shouldn't happen due to that check in the first line, but clearly it's
        ' programmatically possibly (race conditions / different PCs etc).
        Debug.Assert(i = num)

    End Sub

    ''' <summary>
    ''' Handles a resource status change coming from the resource connection manager
    ''' </summary>
    Private Sub HandleResourceStatusChanged(
     sender As Object, e As ResourcesChangedEventArgs) _
     Handles mConnManager.ResourceStatusChanged
        Debug.Print(
            "mConnManager.HandleResourceStatusChanged [Queue:{0}; Changes:{1}]",
            Queue.Name,
            String.Join(";", e.Changes.ToList().Select(Function(p) p.Key & ":" & p.Value.ToString()))
        )
        If e.OverallChange.HasFlag(ResourceStatusChange.EnvironmentChange) Then
            Dim resourceNames = CollectionUtil.Convert(
                ResourceGroup.FlattenedContents(Of clsSortedSet(Of IGroupMember))(False), Function(r) r.Name)
            For Each pair In e.Changes
                If resourceNames.Contains(pair.Key) Then
                    UpdateActiveQueueData()
                    Exit For
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' Handles the session create event from the resource connection manager. Note
    ''' that this is fired if the session create failed as well as if it succeeded.
    ''' </summary>
    Private Sub HandleSessionCreate(sender As Object, e As SessionCreateEventArgs) _
     Handles mConnManager.SessionCreate
        Debug.Print(
            "mConnManager.HandleSessionCreate [Queue:{0}; Session:{1}; State:{2}]",
            Queue.Name,
            e.SessionId,
            e.State
        )

        Dim tag As Guid
        If Not Guid.TryParse(e.Tag?.ToString(), tag) Then Return
        ' If it's not got a Guid tag, it can't be one of ours - not interested
        If tag = Guid.Empty Then Return

        ' Get the placeholder for that tag - if we don't have it in our 'awaiting
        ' create' map, then we're not interested in it
        Dim holder As SessionPlaceholder = Nothing
        If Not mAwaitingCreate.TryRemove(tag, holder) Then Return

        ' If the session was created, move the session to the awaiting start map
        ' and request that the session be started
        If e.State = SessionCreateState.Created Then
            Try
                holder.Id = e.SessionId
                mAwaitingStart(e.SessionId) = holder
                mConnManager.SendStartSession({New StartSessionData(holder.Machine.Id, ProcessId, holder.Id, Nothing, tag, Nothing, 0)})
                Return
            Catch ex As Exception
                Log.Error("An error occured attemping to start the session: {0}", ex.Message)
            End Try
        End If

        ' Otherwise, there was some kind of error creating the session, report it
        Dim resName As String = mConnManager.GetResource(e.ResourceId).Name
        Log.Error("Attempt {0}: An error occurred trying to create an active session on {1}: {2}",
         holder.Attempt, resName, LogOutputHelper.Sanitize(e.ErrorMessage))

        ' If we've hit our max attempts, cancel the session create
        If holder.Attempt = MaxStartSessionAttempts Then
            FailSessionCreate(e)
            Return
        End If

        Try
            ' Try to start a session again, avoiding the failed resource, and
            ' incrementing the attempt count
            StartActiveSessionAsync(holder.Attempt + 1, holder.Machine)

        Catch rue As ResourceUnavailableException
            ' If there are no more resources available, there's little we can do
            ' but fail the session starting entirely.
            ' The only real error is the one that's already logged, so there's no
            ' point logging any more; just raise the active session create fail with
            ' the original session create failure and move on.
            FailSessionCreate(e)

        End Try

    End Sub

    ''' <summary>
    ''' Handles a session being started in the wrapped resource connection manager.
    ''' This picks it out of the 'awaiting start' session map, and invokes the
    ''' callback registered against the session start request.
    ''' </summary>
    Private Sub HandleSessionStart(sender As Object, e As SessionStartEventArgs) _
     Handles mConnManager.SessionStart
        Debug.Print(
            "mConnManager.HandleSessionStart [Queue:{0}; Session:{1}; Success:{2}]",
            Queue.Name,
            e.SessionId,
            e.Success
        )
        Dim holder As SessionPlaceholder = Nothing

        ' If it's not a session we're monitoring the starting of, not interested
        If Not mAwaitingStart.TryRemove(e.SessionId, holder) Then Return

        ' Successful starts generate an ActiveSessionStarted event - no other
        ' activity is required
        If e.Success Then
            OnActiveSessionStarted(New ActiveQueueSessionEventArgs(
             Queue.Ident, e.SessionId))
            Return
        End If

        ' Otherise the session start failed for some reason, log an error in the
        ' Blue Prism event log
        Log.Error("Session '{0}' failed to start at resource {1} on attempt {2}: {3}",
            holder.Id, holder.Machine.Name, holder.Attempt, LogOutputHelper.Sanitize(e.ErrorMessage))

        ' Try to delete the dangling session
        Try
            mConnManager.SendDeleteSession({New DeleteSessionData(holder.Machine.Id, ProcessId, holder.Id, Nothing, 0)})

        Catch nce As NotConnectedException
            ' If the resource is no longer connected, there's not much we can do
            ' Log a warning and carry on.
            Log.Warn(
                "Couldn't delete dangling session '{0}' on resource '{1}' - not connected",
                holder.Id, holder.Machine.Name)

        Catch ex As Exception
            ' Any other errors should be treated as fatal for the session we
            ' are attempting to create. Log the error in the event log
            Log.Error(ex,
             "Error while deleting unstarted session '{0}' from resource '{1}'",
             holder.Id, holder.Machine.Name)

            ' And raise the active session start fail event
            FailSessionStart(e)

            ' We don't want to try creating another session at this stage, so
            ' just exit without doing so
            Return

        End Try

        ' If we've reached our maximum number of attempts, we mark this session as
        ' not startable using the last session start fail art and exit
        If holder.Attempt = MaxStartSessionAttempts Then
            FailSessionStart(e)
            Return ' ... no more lives
        End If

        Try
            ' Try to start a session again, avoiding the failed resource, and
            ' incrementing the attempt count
            StartActiveSessionAsync(holder.Attempt + 1, holder.Machine)

        Catch rue As ResourceUnavailableException
            ' If there are no more resources available, there's little we can do
            ' but fail the session starting entirely.
            ' The only real error is the one that's already logged, so there's no
            ' point logging any more; just raise the active session start fail with
            ' the original session start failure and move on.
            FailSessionStart(e)

        End Try

    End Sub

    ''' <summary>
    ''' Deals with a session which failed to create, as detailed in the given args
    ''' </summary>
    ''' <param name="e">The event args detailing the session which failed to be
    ''' created
    ''' </param>
    Private Sub FailSessionCreate(e As SessionCreateEventArgs)
        TargetSessions -= 1
        Dim machName As String = mConnManager.GetResource(e.ResourceId).Name
        OnActiveSessionStartFailed(
            New ActiveQueueSessionEventArgs(Queue.Ident, Nothing, machName))
    End Sub

    ''' <summary>
    ''' Deals with a session which failed to start, as detailed in the given args
    ''' </summary>
    ''' <param name="e">The event args detailing the session which failed to start
    ''' </param>
    Private Sub FailSessionStart(e As SessionStartEventArgs)
        ' We can't start it, reduce the number of target sessions such that we don't
        ' keep trying to create them
        TargetSessions -= 1
        Dim resName As String = gSv.GetSessionResourceName(e.SessionId)
        OnActiveSessionStartFailed(
            New ActiveQueueSessionEventArgs(Queue.Ident, e.SessionId, resName))
    End Sub

    ''' <summary>
    ''' Handles a session end being reported in the resource connection manager,
    ''' ensuring that our queue has the latest active data and delegating to the
    ''' <see cref="ActiveSessionEnded"/> event if it pertains to this controller.
    ''' </summary>
    Private Sub HandleSessionEnd(sender As Object, e As SessionEndEventArgs) _
     Handles mConnManager.SessionEnd
        Debug.Print(
            "mConnManager.HandleSessionEnd [Queue:{0}; Session:{1}; Status:{2}]",
            Queue.Name,
            e.SessionId,
            e.UserMessage
        )
        ' If our queue recognises this session
        If Queue.IsActiveSessionOwner(e.SessionId) Then
            ' Update the queue's active data
            UpdateActiveQueueData()

            ' If this session *isn't* one that we requested a stop on, decrement
            ' the target session count.
            ' FIXME: This kinda breaks with 2 separate clients messing with the
            ' active queue targets
            If Not mAwaitingEnd.TryRemove(e.SessionId, 0) Then
                SyncLock mTargetLock
                    mTargetDecrementBy += 1
                    Monitor.PulseAll(mTargetLock)
                End SyncLock
            End If

            ' Raise an event to let interested parties know that the state of the
            ' controller has changed
            OnActiveSessionEnded(
             New ActiveQueueSessionEventArgs(Queue.Ident, e.SessionId))

        End If
    End Sub

    ''' <summary>
    ''' Updates the active queue data in this controller, replacing the queue with
    ''' a new instance as necessary.
    ''' </summary>
    Public Sub UpdateActiveQueueData()
        Queue.UpdateActiveData()
    End Sub

    ''' <summary>
    ''' Updates the statistics on the given queue from the data in the database
    ''' </summary>
    Public Sub UpdateStats(q As clsWorkQueue)
        Queue.UpdateStats(q)
    End Sub

    ''' <summary>
    ''' Resets the target session count for the queue represented by this controller
    ''' </summary>
    Friend Sub ResetTargetSessionCount()
        mQueue.ResetTargetSessionCount()
    End Sub

    ''' <summary>
    ''' Checks if this controller is for the queue with the given identity.
    ''' </summary>
    ''' <param name="ident">The identity of the queue to check this controller for.
    ''' </param>
    ''' <returns>True if the queue represented by this controller has the given
    ''' identity; False otherwise.</returns>
    Public Function IsForQueue(ident As Integer) As Boolean
        Return (mQueue.Ident = ident)
    End Function

    ''' <summary>
    ''' Disposes of this robot manager.
    ''' </summary>
    ''' <param name="explicitly">True to indicate that this method was called by an
    ''' explicit <see cref="Dispose"/> call; False to indicate that it was called by
    ''' a finalizer on the object.</param>
    Protected Overridable Sub Dispose(explicitly As Boolean)
        If Not mDisposed Then
            If explicitly Then
                Dim t As Thread = mAimingThread
                If t IsNot Nothing Then
                    If t.IsAlive Then
                        mAimingThreadStop = True
                        SyncLock mTargetLock
                            Monitor.PulseAll(mTargetLock)
                        End SyncLock
                        t.Join(5000)
                        ' We gave it fair time to exit cleanly. Just leave it, it's
                        ' not worth it - it's a background thread anyway, and worst
                        ' case, it will just sit there doing nothing since it can
                        ' never receive a Pulse() once this controller is disposed
                    End If
                    mAimingThread = Nothing
                End If
            End If
            mConnManager = Nothing
        End If
        mDisposed = True
    End Sub

    ''' <summary>
    ''' Explicitly disposes of the resources used by this robot manager. Note that
    ''' this <em>does not</em> dispose of the backing connection manager; rather, it
    ''' disassociates this controller from it and stops listening to events from it.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Check that the required run mode for the process can be retrieved without error before
    ''' sending the create session message to the queue, done here for validation and
    ''' to display error message if it fails i.e. if object dependency does not exist.
    ''' </summary>
    ''' <param name="processId"></param>
    Private Sub CheckRequiredRunMode(processId As Guid)
        Dim extRunModes As New Dictionary(Of String, BusinessObjectRunMode)
        Dim externalObjectDetails = CType(Options.Instance.GetExternalObjectsInfo(), clsGroupObjectDetails)
        Dim comGroup As New clsGroupObjectDetails(externalObjectDetails.Permissions)
        For Each comObj In externalObjectDetails.Children
            If TypeOf (comObj) Is clsCOMObjectDetails Then _
                    comGroup.Children.Add(comObj)
        Next
        Using objRefs As New clsGroupBusinessObject(comGroup, Nothing, Nothing)
            extRunModes = objRefs.GetNonVBORunModes()
        End Using
        gSv.GetEffectiveRunMode(processId, extRunModes)
    End Sub

#End Region

End Class
