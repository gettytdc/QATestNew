
Imports System.Collections.Concurrent
Imports System.Threading
Imports System.Timers
Imports Timer = System.Timers.Timer

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.WorkQueues
Imports BluePrism.BPCoreLib.Collections
Imports NLog
Imports BluePrism.Utilities.Functional

''' <summary>
''' Class which describes a manager of all the active queues subscribed to in this
''' app domain. Each queue is individually handled by an
''' <see cref="ActiveQueueController"/> instance; this class brings the controllers
''' together and provides an interface to the overall management of the active
''' queues.
''' 
''' It is typically created when a user logs in and remains active until the user
''' logs out. It isn't loaded for resource PCs, only clients.
''' </summary>
Public Class ActiveQueueManager : Implements IDisposable

#Region " Class-scope Declarations "

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    ' The interval at which the active queues' data is updated
    Private ReadOnly TimerInterval As TimeSpan = TimeSpan.FromSeconds(10)

    ' The number of timer ticks to update the statistics on the queues for
    Private Const TickIntervalStatsUpdate As Integer = 6

#End Region

#Region " Events "

    ''' <summary>
    ''' Event raised when an active queue session has been started within the
    ''' scope of this active queue manager
    ''' </summary>
    Public Event ActiveQueueSessionStarted As ActiveQueueSessionEventHandler

    ''' <summary>
    ''' Event raised when an active queue session start failed - either the session
    ''' could not be created, or it was created and could not be started.
    ''' </summary>
    Public Event ActiveQueueSessionStartFailed As ActiveQueueSessionEventHandler

    ''' <summary>
    ''' Event raised when an active queue session has been ended within the scope of
    ''' this active queue manager
    ''' </summary>
    Public Event ActiveQueueSessionEnded As ActiveQueueSessionEventHandler

    ''' <summary>
    ''' Event raised when an error occurs within the scope of this active queue
    ''' manager
    ''' </summary>
    Public Event ActiveQueueError As ActiveQueueErrorEventHandler

#End Region

#Region " Member Variables "

    ' A count of the number of times that the update timer has elapsed
    Private mTickCount As Integer

    ' The resource connection manager used by this active queue manager
    Private mManager As IResourceConnectionManager

    ' The queue controllers keyed on the idents of the queues they are controlling
    Private mControllers As ConcurrentDictionary(Of Integer, ActiveQueueController)

    ' Flag indicating if this manager is disposed
    Private mDisposed As Boolean

    ' The timer to update the active data in the hosted active queue controllers
    Private WithEvents mUpdateTimer As Timer

    ' Flag indicating if the manager is currently updating, either via the timer
    ' ticking or by an external call. 0 when not updating; 1 when updating.
    ' This should only ever be updated using Interlocked.XXX since it has the
    ' potential to be changed on multiple threads
    Private mUpdating As Integer

    ' The group store which can be used by any active queue components
    Private mGroupStore As IGroupStore

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new active queue controller.
    ''' </summary>
    ''' <param name="connMgr">The resource connection manager to use to schedule
    ''' sessions on connected resources</param>
    ''' <param name="grpStore">The group store to use for controllers under this
    ''' manager.</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="connMgr"/> is null
    ''' </exception>
    Public Sub New(connMgr As IResourceConnectionManager, grpStore As IGroupStore)
        If connMgr Is Nothing Then Throw New ArgumentNullException(NameOf(connMgr),
            My.Resources.ActiveQueueManager_CannotCreateAQueueManagerWithoutAResourceConnectionManager)
        If connMgr.IsDisposed Then Throw New ObjectDisposedException(
            My.Resources.ActiveQueueManager_ConnectionManagerIsDisposedItCannotBeUsedInAQueueManager)
        mManager = connMgr
        mGroupStore = grpStore
        mControllers = New ConcurrentDictionary(Of Integer, ActiveQueueController)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The store which can be used by active queue controllers and related classes
    ''' to load groups (eg. the resource groups) from the database.
    ''' If not set, an instance of <see cref="NullGroupStore"/> will be returned.
    ''' </summary>
    Public ReadOnly Property GroupStore As IGroupStore
        Get
            EnsureNotDisposed()
            If mGroupStore Is Nothing Then Return NullGroupStore.Instance
            Return mGroupStore
        End Get
    End Property

    ''' <summary>
    ''' The connection manager assigned to this active queue manager
    ''' </summary>
    Friend ReadOnly Property ConnectionManager As IResourceConnectionManager
        Get
            EnsureNotDisposed()
            Return mManager
        End Get
    End Property

    ''' <summary>
    ''' Gets the controller for the queue with the given identity
    ''' </summary>
    ''' <param name="ident">The identity of the queue for which the controller is
    ''' required.</param>
    ''' <returns>The controller which is looking after the queue with the given
    ''' identity, or null if this manager has no controller looking after such a
    ''' queue.</returns>
    ''' <exception cref="ObjectDisposedException">If this manager is disposed.
    ''' </exception>
    Default ReadOnly Property Controller(ident As Integer) As ActiveQueueController
        Get
            EnsureNotDisposed()
            Dim cont As ActiveQueueController = Nothing
            If mControllers.TryGetValue(ident, cont) Then Return cont
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this controller is disposed
    ''' </summary>
    Public ReadOnly Property IsDisposed As Boolean
        Get
            Return mDisposed
        End Get
    End Property

    ''' <summary>
    ''' Gets a collection of the queues that this manager is looking after
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this manager is disposed.
    ''' </exception>
    Private ReadOnly Property Queues As ICollection(Of clsWorkQueue)
        Get
            EnsureNotDisposed()
            Dim lst As New List(Of clsWorkQueue)
            For Each cont As ActiveQueueController In mControllers.Values
                lst.Add(cont.Queue)
            Next
            Return lst
        End Get
    End Property

#End Region

#Region " Methods "

    Public Function GetActiveResourcesForQueues() As ICollection(Of IResourceMachine)

        Return ConnectionManager.GetActiveResources(False)

    End Function


    ''' <summary>
    ''' Handles the 'update active queue data' timer elapsing by telling all the
    ''' hosted active queue controllers to update their active queue data.
    ''' </summary>
    Private Sub HandleUpdateActiveQueueDataTimerTick(
     sender As Object, e As ElapsedEventArgs) Handles mUpdateTimer.Elapsed
        ' If we're in the middle of an update, do nothing - doesn't count as a real
        ' tick (even as far as not incrementing the tick count)
        If mUpdating = 1 Then Return

        mTickCount += 1
        Dim updateStats As Boolean = (mTickCount Mod TickIntervalStatsUpdate = 0)

        ' Unlikely... but belt and braces...
        If mTickCount = Integer.MaxValue Then mTickCount = 0

        RefreshAll(updateStats)

    End Sub

    ''' <summary>
    ''' Gets or creates a queue controller for the queue with the given identity,
    ''' returning it to the caller.
    ''' </summary>
    ''' <param name="queueIdent">The identity of the queue for which a controller is
    ''' required.</param>
    ''' <returns>The queue controller associated with the queue with the given
    ''' identity.</returns>
    ''' <exception cref="ObjectDisposedException">If this manager is disposed.
    ''' </exception>
    Public Function Subscribe(queueIdent As Integer) As ActiveQueueController
        Return Subscribe(GetSingleton.ICollection(queueIdent)).First()
    End Function

    ''' <summary>
    ''' Gets or creates a queue controller for the queue with the given identity,
    ''' returning it to the caller
    ''' </summary>
    ''' <param name="queueIdents">The identities of the queue for which a controller
    ''' is required.</param>
    ''' <returns>The queue controllers associated with the queues with the given
    ''' identities.</returns>
    ''' <exception cref="ObjectDisposedException">If this manager is disposed.
    ''' </exception>
    Public Function Subscribe(queueIdents As ICollection(Of Integer)) _
     As ICollection(Of ActiveQueueController)
        ' Can't subscribe if we're disposed
        EnsureNotDisposed()

        ' Update the queue data in the controllers
        Dim queues As New Dictionary(Of Integer, clsWorkQueue)
        For Each q As clsWorkQueue In gSv.GetQueueStatsList(Nothing)
            q.GroupStore = mGroupStore
            queues(q.Ident) = q
        Next

        Dim conts As New List(Of ActiveQueueController)

        For Each ident As Integer In queueIdents

            ' Get or create the new controller
            ' If we're creating a new one, we want to reset the target session count;
            ' otherwise we use what's there - sessions could be in the process of
            ' being created on a different thread, so we leave the target as it is
            Dim cont As ActiveQueueController = mControllers.GetOrAdd(ident,
             Function(i)
                 ' Create a controller using the new data
                 Dim c As New ActiveQueueController(Me, queues(ident))
                 ' We want to have the target session count settled before we open
                 ' this queue up to UI elements relying on these controllers.
                 c.ResetTargetSessionCount()
                 AddHandlers(c)
                 Return c
             End Function
            )

            ' Update the queue with the new data (redundant if it's a new controller,
            ' but harmless, so we might as well not bother trying to figure out
            ' whether it is or not)
            cont.Queue = queues(ident)

            conts.Add(cont)
        Next

        Dim targetSessionCountDetails As IList(Of ActiveQueueTargetSessionCount) = conts.Select(Function(x)
                                                                                                    Return New ActiveQueueTargetSessionCount _
                                                                                            With {.QueueId = x.Queue.Ident,
                                                                                             .TargetSessionCount = x.Queue.TargetSessionCount}
                                                                                                End Function).ToList()


        gSv.SetTargetSessionCountForMultipleActiveQueues(targetSessionCountDetails)


        Return conts

    End Function

    ''' <summary>
    ''' Adds the necessary event handlers in this manager to the given controller
    ''' </summary>
    ''' <param name="c">The controller whose event handlers from within this manager
    ''' instance should be set</param>
    Private Sub AddHandlers(c As ActiveQueueController)
        AddHandler c.ActiveSessionStarted, AddressOf HandleSessionStarted
        AddHandler c.ActiveSessionStartFailed, AddressOf HandleSessionStartFailed
        AddHandler c.ActiveSessionEnded, AddressOf HandleSessionEnded
        AddHandler c.ActiveQueueError, AddressOf HandleErrorOccurred
    End Sub

    ''' <summary>
    ''' Removes the event handlers set in this manager for the given controller
    ''' </summary>
    ''' <param name="c">The controller whose event handlers set within this manager
    ''' instance should be removed</param>
    Private Sub RemoveHandlers(c As ActiveQueueController)
        RemoveHandler c.ActiveSessionStarted, AddressOf HandleSessionStarted
        RemoveHandler c.ActiveSessionStartFailed, AddressOf HandleSessionStartFailed
        RemoveHandler c.ActiveSessionEnded, AddressOf HandleSessionEnded
        RemoveHandler c.ActiveQueueError, AddressOf HandleErrorOccurred
    End Sub

    ''' <summary>
    ''' Refreshes all data in the queues represented in the active queue manager,
    ''' including refreshing all the stats data (note - that last part may take some
    ''' time for an environment with large queues).
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this manager is disposed.
    ''' </exception>
    Public Sub RefreshAll()
        RefreshAll(True)
    End Sub

    ''' <summary>
    ''' Refreshes all data in the queues represented in the active queue manager,
    ''' optionally including refreshing all the stats data (note - that last part may
    ''' take some time for an environment with large queues).
    ''' </summary>
    ''' <param name="includeStats">True to update the queue statistics; False to
    ''' only update the active queue data.</param>
    ''' <exception cref="ObjectDisposedException">If this manager is disposed.
    ''' </exception>
    Public Sub RefreshAll(includeStats As Boolean)
        EnsureNotDisposed()

        ' compare mUpdating to 0:
        ' if it is 0, then replace with 1;
        ' return the original value of mUpdating.
        Dim updating = Interlocked.CompareExchange(mUpdating, 1, 0)

        ' If updating was already 1, that means an update is already occuring. Bail.
        If updating = 1 Then Return

        Try
            GroupStore.GetTree(
                GroupTreeType.Resources,
                ResourceGroupMember.Active,
                Nothing,
                True,
                False,
                False)

            For Each cont As ActiveQueueController In mControllers.Values
                cont.UpdateActiveQueueData()
            Next

            If includeStats Then
                Dim queuelist = mControllers.Values.Select(Function(x) x.Queue).ToList()
                Dim latestQueues = gSv.GetQueueStatsList(queuelist)

                For Each cont As ActiveQueueController In mControllers.Values
                    Dim currentQueue = latestQueues.Where(Function(x) x.Ident = cont.Queue.Ident).First()
                    cont.UpdateStats(currentQueue)
                Next
            End If

        Catch ex As Exception
            Log.Error(ex, "An error occurred updating the active queue data")
        Finally
            ' Reset the mUpdating flag to indicate we're not updating any more
            Interlocked.Exchange(mUpdating, 0)
        End Try

    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveQueueSessionStarted"/> event.
    ''' </summary>
    Protected Overridable Sub OnActiveQueueSessionStarted(
     e As ActiveQueueSessionEventArgs)
        RaiseEvent ActiveQueueSessionStarted(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveQueueSessionStartFailed"/> event.
    ''' </summary>
    Protected Overridable Sub OnActiveQueueSessionStartFailed(
     e As ActiveQueueSessionEventArgs)
        RaiseEvent ActiveQueueSessionStartFailed(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveQueueSessionEnded"/> event.
    ''' </summary>
    Protected Overridable Sub OnActiveQueueSessionEnded(
     e As ActiveQueueSessionEventArgs)
        RaiseEvent ActiveQueueSessionEnded(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ActiveQueueError"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnActiveQueueError(e As ActiveQueueErrorEventArgs)
        RaiseEvent ActiveQueueError(Me, e)
    End Sub

    ''' <summary>
    ''' Ensures that this queue manager has not been disposed before performing an
    ''' operation.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this object is disposed.
    ''' </exception>
    Private Sub EnsureNotDisposed()
        If IsDisposed Then Throw New ObjectDisposedException(
            My.Resources.ActiveQueueManager_ThisQueueManagerHasBeenDisposed)
    End Sub

    ''' <summary>
    ''' Disposes of this robot manager.
    ''' </summary>
    ''' <param name="explicitly">True to indicate that this method was called by an
    ''' explicit <see cref="Dispose"/> call; False to indicate that it was called by
    ''' a finalizer on the object.</param>
    Protected Overridable Sub Dispose(explicitly As Boolean)
        If Not mDisposed Then
            If explicitly Then
                If mUpdateTimer IsNot Nothing Then mUpdateTimer.Dispose()
                For Each c As ActiveQueueController In mControllers.Values
                    RemoveHandlers(c)
                    c.Dispose()
                Next
                mControllers.Clear()
            End If
            mUpdateTimer = Nothing
            mControllers = Nothing
            mManager = Nothing
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
    ''' Handles an active session start fired from one of the controllers, passing
    ''' it onto the <see cref="ActiveQueueSessionStarted"/> event in this manager
    ''' </summary>
    Private Sub HandleSessionStarted(sender As Object, e As ActiveQueueSessionEventArgs)
        OnActiveQueueSessionStarted(e)
    End Sub

    ''' <summary>
    ''' Handles an active session start fail fired from one of the controllers,
    ''' passing it onto the <see cref="ActiveQueueSessionStartFailed"/> event in this
    ''' manager.
    ''' </summary>
    Private Sub HandleSessionStartFailed(
     sender As Object, e As ActiveQueueSessionEventArgs)
        OnActiveQueueSessionStartFailed(e)
    End Sub

    ''' <summary>
    ''' Handles an active session end fired from one of the controllers, passing
    ''' it onto the <see cref="ActiveQueueSessionStarted"/> event in this manager
    ''' </summary>
    Private Sub HandleSessionEnded(sender As Object, e As ActiveQueueSessionEventArgs)
        OnActiveQueueSessionEnded(e)
    End Sub

    ''' <summary>
    ''' Handles an error occurring in one of the queue controllers. This passes it
    ''' onto any handlers of the <see cref="ActiveQueueError"/> event.
    ''' </summary>
    Private Sub HandleErrorOccurred(sender As Object, e As ActiveQueueErrorEventArgs)
        OnActiveQueueError(e)
    End Sub

#End Region

    Public Class QueueActiveResources
        Public Property Queue As clsWorkQueue
        Public Property ActiveResources As Integer
    End Class

End Class
