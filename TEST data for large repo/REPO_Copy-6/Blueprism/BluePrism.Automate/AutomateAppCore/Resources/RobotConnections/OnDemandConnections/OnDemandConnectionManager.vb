Imports System.Collections.Concurrent
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Timers
Imports Autofac
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.ClientServerResources.Core.Data
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.ClientServerResources.Core.Interfaces
Imports BluePrism.Core.Resources
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports NLog

Namespace Resources

    Public Class OnDemandConnectionManager : Implements IUserAuthResourceConnectionManager

#Region " Class-scope Declarations "

        Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

        ''' <summary>
        ''' The database polling interval to refresh the view. This is in units of
        ''' whatever the timer interval is.
        ''' </summary>
        Private Const ciDbPollInterval As Integer = 30000 \ mRefreshTimerInterval

        ''' <summary>
        ''' The database polling interval check for recently ended sessions, to refresh the view. This is in units of
        ''' whatever the timer interval is.
        ''' </summary>
        Private Const ciSessionEndDbPollInterval As Integer = 2000 \ mRefreshTimerInterval

        ''' <summary>
        ''' Interval for the mRefreshTimer
        ''' </summary>
        Private Const mRefreshTimerInterval As Integer = 250

        ''' <summary>
        ''' Interval for the mRateLimiterTimer
        ''' </summary>
        Private Const mRateLimiterTimerInterval As Integer = 250

#End Region

#Region " Published Events "

        ''' <summary>
        ''' Event raised when status of a connection changes.
        ''' </summary>
        Public Event ResourceStatusChanged As ResourcesChangedEventHandler Implements IResourceConnectionManager.ResourceStatusChanged

        ''' <summary>
        ''' Event raised when a session has been created
        ''' </summary>
        Public Event SessionCreate As SessionCreateEventHandler Implements IResourceConnectionManager.SessionCreate

        ''' <summary>
        ''' Event raised when a session has been started
        ''' </summary>
        Public Event SessionStart As SessionStartEventHandler Implements IResourceConnectionManager.SessionStart

        Public Event SessionStop As SessionStopEventHandler Implements IResourceConnectionManager.SessionStop

        ''' <summary>
        ''' Event raised when a session has been Deleted
        ''' </summary>
        Public Event SessionDelete As SessionDeleteEventHandler Implements IResourceConnectionManager.SessionDelete

        ''' <summary>
        ''' Event raised when a session has been ended
        ''' </summary>
        Public Event SessionEnd As SessionEndEventHandler Implements IResourceConnectionManager.SessionEnd

        ''' <summary>
        ''' Event raised when any session variables have been updated
        ''' </summary>
        Public Event SessionVariablesUpdated As SessionVariableUpdatedHandler Implements IResourceConnectionManager.SessionVariablesUpdated


        Public Event ShowUserMessage As EventHandler(Of String) Implements IResourceConnectionManager.ShowUserMessage



#End Region

#Region " Member Variables "

        Private ReadOnly mSessionVariables As New ConcurrentDictionary(Of String, clsSessionVariable)

        Private mMonitorSessionVariables As Boolean = False
        Private mMode As IResourceConnectionManager.Modes
        Private mPoolID As Guid
        Private mLock As Integer
        Private mLastCheck As Date = Date.MinValue
        Private mCheckSet As New HashSet(Of Guid)

        ''' <summary>
        ''' The end time of the last session we sent a "Session End" event for.
        ''' </summary>
        Private mLastSessionEndDate As Date = Date.UtcNow

        ''' <summary>
        ''' When in Controller mode, this is the ID of the controller.
        ''' </summary>
        Private mControllerID As Guid

        ''' <summary>
        ''' Timer which causes us to poll the database every now and again
        ''' </summary>
        Private WithEvents mRefreshTimer As ITimer

        ''' <summary>
        ''' Timer used to schedule signaling of the rate limiter
        ''' </summary>
        Private WithEvents mRateLimiterTimer As ITimer

        ''' <summary>
        ''' Countdown until next poll of database to detect newly added
        ''' resources etc.
        ''' </summary>
        Private miPollDBCount As Integer

        ''' <summary>
        ''' Countdown until next poll of database to detect recently ended sessions
        ''' </summary>
        Private miEndedSessionPollDBCount As Integer

        ''' <summary>
        ''' A collection of clsResourceConnection objects, representing our
        ''' connections to resource PCs. Connections are made when the
        ''' control is loaded, and remain open until it is closed.
        ''' The dictionary is keyed on the resource ID of the resource machine which
        ''' is connected to in the resource connection.
        ''' </summary>
        Private ReadOnly mResourceConnections As ConcurrentDictionary(Of Guid, OnDemandConnection)
        ''' <summary>
        ''' A dictionary to allow quick look up for the IResourceMachine via a given name
        ''' </summary>
        Private ReadOnly mResourceNameMap As ConcurrentDictionary(Of String, IResourceMachine)

        Private mRateLimiter As New AutoResetEvent(True)
        Private mUser As IUser
        Private ReadOnly mThreadStackSize As Integer = 0
        Private ReadOnly mPingTimeoutSeconds As Integer = 0
        ' Flag indicating if this connection manager is disposed
        Private mIsDisposed As Boolean
        Private ReadOnly mResourceRefreshes As List(Of (Id As Guid, Time As Date)) = New List(Of (Guid, Date))()
        Private ReadOnly mCancellationTokenSource As CancellationTokenSource = New CancellationTokenSource()
        Private ReadOnly mStartupDelay As Integer
        Private ReadOnly mResourceInitializationPause As Integer = 100
        Private ReadOnly mConnectionStatistics As New ResourceConnectionStatistics()
        Private ReadOnly mResourceSessionEvents As IInstructionalHostController

        Private Const MaxResourceRefreshesPerBlockPeriod As Integer = 5
        Private Const RefreshBlockPeriodInSeconds As Integer = 60
        Private ReadOnly mConnectionPingCooldown As Integer
        Private ReadOnly mProcessResourceInputSleepTime As Integer
        Private ReadOnly mRetrySleepTimerInSeconds As Integer
#End Region

#Region " Properties "
        Public ReadOnly Property QueryCapabilities As Boolean = False Implements IResourceConnectionManager.QueryCapabilities

        ''' <summary>
        ''' A Dictionary of all session variables known to the Resource Connection
        ''' Manager. The key for the dictionary is "XXX.YYY" where XXX is the Guid of
        ''' the session, and YYY is the variable name.
        ''' This will only contain information when the MonitorSessionVariables property
        ''' is True.
        ''' </summary>
        Public ReadOnly Property SessionVariables() _
     As IDictionary(Of String, clsSessionVariable) Implements IResourceConnectionManager.SessionVariables
            Get
                Return New Dictionary(Of String, clsSessionVariable)(mSessionVariables)
            End Get
        End Property

        ''' <summary>
        ''' Determines whether or not this instance should monitor the session variables
        ''' of processes running on all connected Resources.
        ''' </summary>
        Public Property MonitorSessionVariables() As Boolean Implements IResourceConnectionManager.MonitorSessionVariables
            Get
                Return mMonitorSessionVariables
            End Get
            Set(ByVal value As Boolean)
                mMonitorSessionVariables = value
                mSessionVariables.Clear()
                For Each r As OnDemandConnection In mResourceConnections.Values
                    r.SendVarUpdates(value)
                Next
            End Set
        End Property

        ''' <summary>
        ''' The current mode.
        ''' </summary>
        Public ReadOnly Property Mode() As IResourceConnectionManager.Modes Implements IResourceConnectionManager.Mode
            Get
                Return mMode
            End Get
        End Property

        ''' <summary>
        ''' When in Controller mode, this is the ID of the pool.
        ''' </summary>
        Friend ReadOnly Property PoolId() As Guid Implements IResourceConnectionManager.PoolId
            Get
                Return mPoolID
            End Get
        End Property

        ''' <summary>
        ''' Used to slow down the rate of clients making outbound connections to allow
        ''' for XP SP2 stupidity.
        ''' </summary>
        Friend ReadOnly Property RateLimiter() As AutoResetEvent Implements IResourceConnectionManager.RateLimiter
            Get
                Return mRateLimiter
            End Get
        End Property

        ''' <summary>
        ''' The User to use for this connection manager.
        ''' 
        ''' This is almost always going to be the logged in user, but may need to be
        ''' a specific (eg. system) user in some circumstances (eg. scheduler engine).
        ''' 
        ''' Note that setting this to anything other than Nothing will
        ''' result in all contained resource connections using this user rather than the
        ''' currently logged in user.
        ''' </summary>  
        Friend Property ConnectionUser() As IUser Implements IResourceConnectionManager.ConnectionUser
            Get
                If mUser Is Nothing Then Return User.Current
                Return mUser
            End Get
            Set
                mUser = Value
            End Set
        End Property

        ''' <summary>
        ''' The User ID associated with this connection manager (or Guid.Empty if there
        ''' is no user context - e.g. resourcePC)
        ''' </summary>
        Friend ReadOnly Property UserId() As Guid Implements IResourceConnectionManager.UserId
            Get
                If ConnectionUser IsNot Nothing Then Return ConnectionUser.Id
                Return Guid.Empty
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this resource connection manager is disposed.
        ''' </summary>
        Public ReadOnly Property IsDisposed As Boolean Implements IResourceConnectionManager.IsDisposed
            Get
                Return mIsDisposed
            End Get
        End Property

        Public ReadOnly Property MaxRefreshInterval As TimeSpan Implements IResourceConnectionManager.MaxRefreshInterval
            Get
                Return New TimeSpan(0, 0, 15)
            End Get
        End Property

        Public ReadOnly Property ServerName As String Implements IResourceConnectionManager.ServerName

        Public ReadOnly Property UsingAppServerConnection As Boolean Implements IResourceConnectionManager.UsingAppServerConnection
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property ResourceLimit As Integer = Integer.MaxValue

#End Region

#Region " Constructors "
        Public Sub New()
            Me.New(IResourceConnectionManager.Modes.Normal, Nothing, Nothing, Nothing, New TimerWrapper(New Timers.Timer()), New TimerWrapper(New Timers.Timer()))
        End Sub

        Public Sub New(mode As IResourceConnectionManager.Modes,
                   user As IUser,
                   poolid As Guid,
                   controllerid As Guid, refreshTimer As ITimer, rateLimiterTimer As ITimer)

            mMode = mode
            mUser = user
            mPoolID = poolid
            mControllerID = controllerid
            mResourceConnections = New ConcurrentDictionary(Of Guid, OnDemandConnection)
            mResourceNameMap = New ConcurrentDictionary(Of String, IResourceMachine)()
            ServerName = String.Empty
            mRefreshTimer = refreshTimer
            mRateLimiterTimer = rateLimiterTimer

            Try
                mThreadStackSize = gSv.GetPref(PreferenceNames.ResourceConnection.ThreadStackSize, 0)
            Catch ex As Exception
                Log.Warn(ex, "Unable to get " & PreferenceNames.ResourceConnection.ThreadStackSize & " pref from server")
            End Try

            Try
                mStartupDelay = gSv.GetPref(PreferenceNames.ResourceConnection.ConnectionDelay, 10)
            Catch ex As Exception
                Log.Warn(ex, "Unable to get " & PreferenceNames.ResourceConnection.ConnectionDelay & " pref from server")
            End Try

            mResourceInitializationPause = gSv.GetPref(PreferenceNames.ResourceConnection.InitializationPause, 100)

            mPingTimeoutSeconds = gSv.GetPref(PreferenceNames.ResourceConnection.PingTimeOutSeconds, 30)
            QueryCapabilities = gSv.GetPref(PreferenceNames.ResourceConnection.QueryCapabilities, False)
            ResourceLimit = gSv.GetIntPref(SystemSettings.RuntimeResourceLimit)
            mConnectionPingCooldown = gSv.GetPref(ResourceConnection.ConnectionPingTime, 5)
            mProcessResourceInputSleepTime = gSv.GetPref(ResourceConnection.ProcessResourceInputSleepTime, 100)
            mRetrySleepTimerInSeconds = gSv.GetPref(ResourceConnection.RetrySleepTimerInSeconds, 5)
            Dim callbackSendTimeout = gSv.GetPref(ResourceConnection.CallbackKeepAliveTimeoutMS, 10000)
            miPollDBCount = ciDbPollInterval

            Log.Debug("Constructing OnDemandConnectionManager {manager}", New With {mode, user?.Name, poolid, controllerid, mStartupDelay, QueryCapabilities, miPollDBCount})
            Init()

            mResourceSessionEvents =
            If(gSv.GetCallbackConfigProtocol() = CallbackConnectionProtocol.Grpc,
                DependencyResolver.ResolveKeyed(Of IInstructionalHostController)(CallbackConnectionProtocol.Grpc,
                                                                                 New TypedParameter(GetType(ClientServerResources.Core.Config.ConnectionConfig),
                                                                                 Options.Instance.ResourceCallbackConfig)),
                DependencyResolver.ResolveKeyed(Of IInstructionalHostController)(CallbackConnectionProtocol.Wcf,
                                                                                 New TypedParameter(GetType(ClientServerResources.Core.Config.ConnectionConfig),
                                                                                 Options.Instance.ResourceCallbackConfig),
                                                                                 New TypedParameter(GetType(Integer), callbackSendTimeout)))

        End Sub

        Private Async Sub Init()

            Await Task.Delay(mStartupDelay * 1000)

            If mCancellationTokenSource?.Token.IsCancellationRequested OrElse
            IsDisposed Then Exit Sub

            mRefreshTimer.Interval = mRefreshTimerInterval
            mRefreshTimer.AutoReset = False
            mRefreshTimer.Start()

            GetLatestDBResourceInfo()

            Dim taskFactory = New TaskFactory(mCancellationTokenSource.Token)

            Await taskFactory.StartNew(
                Async Sub()
                    Log.Debug("Starting RefreshBlockCleanupTask")
                    While Not mCancellationTokenSource.Token.IsCancellationRequested
                        SyncLock mResourceRefreshes
                            Dim removeBefore = Date.UtcNow.AddSeconds(-(RefreshBlockPeriodInSeconds * 2))
                            mResourceRefreshes.RemoveAll(Function(x) x.Time < removeBefore)
                        End SyncLock
                        Await Task.Delay(5000)
                    End While
                End Sub,
                TaskCreationOptions.LongRunning)

            Await taskFactory.StartNew(
                Async Sub()
                    While Not mCancellationTokenSource.Token.IsCancellationRequested
                        mRateLimiter.Set()
                        Await Task.Delay(250)
                    End While
                End Sub,
        TaskCreationOptions.LongRunning)
        End Sub

#End Region

#Region " Dispose / Finalize "

        ''' <summary>
        ''' Disposes of this resource connection manager.
        ''' This ensures that any resource connections are terminated, and
        ''' that the timer picking up resource changes is stopped.
        ''' </summary>
        Public Sub Dispose() Implements System.IDisposable.Dispose
            mCancellationTokenSource.Cancel()
            Dispose(True)
            GC.SuppressFinalize(Me)
            mResourceSessionEvents.Dispose()
        End Sub

        ''' <summary>
        ''' Disposes of this connection manager.
        ''' </summary>
        ''' <param name="freeManaged">True to dispose of managed objects too, False to
        ''' only dispose of unmanaged objects    of which (at the time of going to press)
        ''' there are none.</param>
        Public Sub Dispose(ByVal freeManaged As Boolean)
            If mIsDisposed Then Return
            If freeManaged Then ' We don't actually have any unmanaged resources.
                Try
                    If mResourceConnections IsNot Nothing Then

                        If mRateLimiterTimer IsNot Nothing Then
                            mRateLimiterTimer.Stop()
                            mRateLimiterTimer.Dispose()
                            mRateLimiterTimer = Nothing
                        End If
                        If mRefreshTimer IsNot Nothing Then
                            mRefreshTimer.Stop()
                            mRefreshTimer.Dispose()
                            ' This is a WithEvents var so this will disassociate any
                            ' events from the timer
                            mRefreshTimer = Nothing
                        End If

                        'Terminate all connections...
                        For Each c In mResourceConnections.Values
                            RemoveEventHandlers(c)
                            c.Terminate()
                        Next

                        'Wait for all connection threads to terminate...
                        Dim done As Boolean
                        Do
                            mRateLimiter?.Set()
                            done = True
                            For Each c As OnDemandConnection In mResourceConnections.Values
                                If Not c.Terminated Then done = False : Exit For
                            Next
                        Loop Until done
                        mRateLimiter?.Dispose()
                        mRateLimiter = Nothing

                    End If
                Catch ' Dispose() should never throw an exception...

                End Try
            End If
            mIsDisposed = True
        End Sub

        ''' <summary>
        ''' Ensures that this manager is disposed of when it is garbage collected.
        ''' </summary>
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub

#End Region

#Region " GetResourceXXX() Methods "
        ''' <summary>
        ''' Gets a list of all available (even if not actually connected) resources
        ''' </summary>
        ''' <returns>A collection of IResourceMachine instances containing the
        ''' available resources.</returns>
        Public Function GetResources() As Dictionary(Of Guid, IResourceMachine) Implements IResourceConnectionManager.GetResources
            Return GetResources(False)
        End Function

        ''' <summary>
        ''' Gets a list of resources, filtering by connection status if desired.
        ''' Resources that are 'unavailable' are never returned.
        ''' </summary>
        ''' <param name="connectedOnly">Set to True to only get resources which have an
        ''' active connection.</param>
        ''' <returns>A List of IResourceMachine instances containing the requested
        ''' resources.</returns>
        Public Function GetResources(ByVal connectedOnly As Boolean) _
     As Dictionary(Of Guid, IResourceMachine) Implements IResourceConnectionManager.GetResources
            Return mResourceConnections.
            Where(Function(x) (Not x.Value.IsUnavailable) AndAlso (x.Value.IsConnected OrElse Not connectedOnly)).
            ToDictionary(Function(x) x.Key, Function(y) y.Value.ResourceMachine)
        End Function

        Public Function GetActiveResources(connectedOnly As Boolean) _
        As List(Of IResourceMachine) Implements IResourceConnectionManager.GetActiveResources

            Return mResourceConnections.Values.
            Where(Function(x) x.IsAvailable(connectedOnly)).
            Select(Function(x) x.ResourceMachine).
            ToList()
        End Function

        Public Function OnlineResourcesCount(attribs As ResourceAttribute) As Integer
            Return mResourceConnections.Values.
            Where(Function(x) x.ResourceMachine.DBStatus = ResourceMachine.ResourceDBStatus.Ready AndAlso x.ResourceMachine.Attributes = attribs).
            Count()
        End Function

        ''' <summary>
        ''' Get the Resource responsible for controlling the one with the specified name.
        ''' This applies when the named Resource is a member of a Pool. In these
        ''' circumstances, there will be no IResourceMachine record of that Resource,
        ''' since we do not communicate with it directly, but instead communicate with
        ''' the Pool's controller, which is what this method finds.
        ''' </summary>
        ''' <param name="name">The name of the Resource sought.</param>
        ''' <returns>The controlling IResourceMachine, or Nothing if not found.
        ''' </returns>
        Public Function GetControllingResource(ByVal name As String) As IResourceMachine Implements IResourceConnectionManager.GetControllingResource
            For Each c As OnDemandConnection In mResourceConnections.Values
                If c.ResourceMachine.HasPoolMember(name) Then Return c.ResourceMachine
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the resource with the specified ID, if one exists.
        ''' </summary>
        ''' <param name="resourceId">The ID of the resource sought.</param>
        ''' <returns>Returns the first resource found with the specified ID, if
        ''' one exists; otherwise returns nothing.</returns>
        Public Function GetResource(ByVal resourceId As Guid) As IResourceMachine Implements IResourceConnectionManager.GetResource
            Try
                Return mResourceConnections(resourceId).ResourceMachine
            Catch ex As KeyNotFoundException
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the resource with the specified name, if one exists.
        ''' </summary>
        ''' <param name="name">The name of the resource sought.</param>
        ''' <returns>Returns the first resource found with the specified name, if
        ''' one exists; otherwise returns Nothing.</returns>
        Public Function GetResource(ByVal name As String) As IResourceMachine Implements IResourceConnectionManager.GetResource
            Dim mach As IResourceMachine = Nothing
            mResourceNameMap.TryGetValue(name, mach)
            Return mach
        End Function

#End Region

#Region " Session Management (eg send start, create, delete messages) "

        Public Sub SendStartSession(sessions As IEnumerable(Of StartSessionData)) Implements IResourceConnectionManager.SendStartSession

            For Each session In sessions
                Try

                    Log.Debug("Send StartSession {info}", New With {session.SessionId, session.ProcessId, session.ResourceId})
                    Dim connection = mResourceConnections(session.ResourceId)
                    connection.SendStartSession(session)
                    RefreshResourceConnection(connection)

                Catch ex As KeyNotFoundException
                    Throw New NoSuchResourceException("No resource found with ID {0}", session.ResourceId)
                End Try
            Next
        End Sub



        ''' <summary>
        ''' Request session variables from the resource.
        ''' </summary>
        ''' <param name="resourceId">resource id of the robot</param>
        ''' <param name="sessionId"></param>
        Public Sub SendSendGetVariable(resourceId As Guid, sessionId As Guid, processId As Guid, ByRef err As String) Implements IResourceConnectionManager.SendGetSessionVariables
            Try
                Log.Debug("Send SendGetVariable {info}", New With {sessionId, resourceId, processId})
                Dim connection = mResourceConnections(resourceId)
                connection.SendGetVariable(sessionId, err)
                RefreshResourceConnection(connection)
            Catch ex As KeyNotFoundException
                Throw New NoSuchResourceException("No resource found with ID {0}", resourceId)
            End Try
        End Sub


        Public Function SendCreateSession(sessionData As IEnumerable(Of CreateSessionData)) As Guid() Implements IResourceConnectionManager.SendCreateSession
            Dim sessionIds As New List(Of Guid)
            For Each s As CreateSessionData In sessionData
                Try
                    Log.Debug("Send Create Session {info}", New With {s.ResourceId, s.ProcessId, s.QueueIdentifier})
                    Dim connection = mResourceConnections(s.ResourceId)
                    sessionIds.Add(connection.SendCreateSession(s))
                    RefreshResourceConnection(connection)
                Catch ex As KeyNotFoundException
                    Throw New NoSuchResourceException("No resource found with ID {0}", s.ResourceId)
                End Try
            Next
            Return sessionIds.ToArray()
        End Function


        ''' <summary>
        ''' Tell the a Resource to stop a Session. This returns immediately
        ''' and the communication is done asynchronously via an existing
        ''' connection. Therefore, only immediately detectable errors
        ''' will result in a failure result from this method.
        ''' </summary>
        Public Sub SendStopSession(sessions As IEnumerable(Of StopSessionData)) Implements IResourceConnectionManager.SendStopSession
            For Each session In sessions
                Try
                    Log.Debug("Send StopSession {info}", New With {session.ResourceId, session.SessionId})
                    Dim connection = mResourceConnections(session.ResourceId)
                    connection.SendStopSession(session)
                    RefreshResourceConnection(connection)
                Catch ex As KeyNotFoundException
                    Throw New NoSuchResourceException("No resource found with ID {0}", session.ResourceId)
                End Try
            Next
        End Sub

        Public Sub SendDeleteSession(sessions As IEnumerable(Of DeleteSessionData)) Implements IResourceConnectionManager.SendDeleteSession
            For Each session In sessions
                Try
                    Log.Debug("Send Delete Session {info}", New With {session.ResourceId, session.SessionId, session.ProcessId})
                    Dim connection = mResourceConnections(session.ResourceId)
                    connection.SendDeleteSession(session)
                    RefreshResourceConnection(connection)
                Catch ex As KeyNotFoundException
                    Throw New NoSuchResourceException("No resource found with ID {0}", session.ResourceId)
                End Try
            Next
        End Sub


        ''' <summary>
        ''' Tell the given resource to set the value of a session variable.
        ''' </summary>
        ''' <param name="vars">The collection of session variable to set.</param>
        Public Sub SendSetSessionVariable(resourceID As Guid, vars As Queue(Of clsSessionVariable)) Implements IResourceConnectionManager.SendSetSessionVariable
            Try
                Log.Debug("Send Set Session Variables {info}", New With {resourceID, vars})
                Dim connection = mResourceConnections(resourceID)
                connection.SendSetSessionVariable(vars)
                RefreshResourceConnection(connection)
            Catch ex As KeyNotFoundException
                Throw New NoSuchResourceException("No resource found with ID {0}", resourceID)
            End Try
        End Sub

        Public Sub ToggleShowSessionVariables(monitorSessionVars As Boolean) Implements IResourceConnectionManager.ToggleShowSessionVariables
            MonitorSessionVariables = monitorSessionVars
        End Sub

#End Region

#Region " Event Handlers "

        ''' <summary>
        ''' Handles the timer event - i.e. called periodically to poll the database,
        ''' and also to check if any Resource PC statuses have changed and if so to
        ''' update the display.
        ''' 
        ''' If a status change is detected, the ResourceStatusChanged event is raised.
        ''' </summary>
        Private Sub Tick(ByVal sender As Object, ByVal e As ElapsedEventArgs) Handles mRefreshTimer.Elapsed

            Try
                'Every so often, poll the database in case
                '  (a) a new resource has been added, or
                '  (b) a resource has changed its status in the database
                'Note that this polling is required for only these two
                'occurrences, so we can get away with doing it less frequently.
                'Other status changes are detected later in this method
                If miPollDBCount > 0 Then
                    miPollDBCount = miPollDBCount - 1
                Else
                    'Poll count has reached zero, so reset to maximum
                    miPollDBCount = ciDbPollInterval
                    GetLatestDBResourceInfo()
                End If


                ' Poll the database for any sessions ending recently. Session end status changes are handled
                ' separately to other status changes, as session end status changes can't be send over the resource connections (as the connection is closed before the session ends in most cases)
                If miEndedSessionPollDBCount > 0 Then
                    miEndedSessionPollDBCount = miEndedSessionPollDBCount - 1
                Else
                    'Poll count has reached zero, so reset to maximum
                    miEndedSessionPollDBCount = ciSessionEndDbPollInterval
                    CheckForRecentlyEndedSessions()
                End If

                'Check for status changes reported by any of our connections,
                'and update the list if necessary.
                Dim accum As ResourceStatusChange = ResourceStatusChange.None
                Dim map As New Dictionary(Of String, ResourceStatusChange)
                If mResourceConnections Is Nothing Then Exit Sub
                For Each con As OnDemandConnection In mResourceConnections.Values
                    If con.PopStatusHasChanged() Then
                        SyncLock mResourceRefreshes
                            mResourceRefreshes.Add((con.ResourceId, DateTime.UtcNow))

                            Dim countRefreshesSince = DateTime.UtcNow.AddSeconds(-RefreshBlockPeriodInSeconds)
                            ' ReSharper disable once VBReplaceWithSingleCallToCount - Count cannot be used in this way on a List

                            'user messages can't be ignored, so don't skip.
                            If mResourceRefreshes.
                               Where(Function(x) x.Id = con.ResourceId AndAlso (x.Time >= countRefreshesSince)).
                               Count() > MaxResourceRefreshesPerBlockPeriod Then Continue For
                        End SyncLock

                        Dim changeType = con.StatusChange
                        map(con.ResourceMachine.Name) = changeType
                        accum = accum Or changeType
                    End If
                Next

                'Send a status change event if something changed in the environment...
                If map.Any() Then
                    OnResourceStatusChanged(New ResourcesChangedEventArgs(accum, map))
                End If
            Catch ex As Exception
                Log.Warn(ex, "Error in ResourceConnectionManager loop")
            Finally
                mRefreshTimer.Start()
            End Try

        End Sub

        ''' <summary>
        ''' Handles a session created event coming from a resource connection. This is
        ''' chained to a <see cref="SessionCreate"/> event fired from this manager.
        ''' </summary>
        ''' <remarks>Note that this does not listen for all sessions, only those
        ''' requested within this resource connection manager.</remarks>
        Private Sub HandleSessionCreate(sender As Object, e As SessionCreateEventArgs)
            OnSessionCreate(e)
        End Sub

        Private Sub HandleResourceChanged(sender As Object, e As ResourcesChangedEventArgs)
            OnResourceStatusChanged(e)
        End Sub

        ''' <summary>
        ''' Handles a session deleted event coming from a resource connection. This is
        ''' chained to a <see cref="SessionDelete"/> event fired from this manager.
        ''' </summary>
        ''' <remarks>Note that this does not listen for all sessions, only those
        ''' requested within this resource connection manager.</remarks>
        Private Sub HandleSessionDelete(sender As Object, e As SessionDeleteEventArgs)
            OnSessionDelete(e)
        End Sub


        ''' <summary>
        ''' Handles a session started event coming from a resource connection. This is
        ''' chained to a <see cref="SessionCreate"/> event fired from this manager.
        ''' </summary>
        ''' <remarks>Note that this does not listen for all sessions, only those
        ''' requested within this resource connection manager.</remarks>
        Private Sub HandleSessionStart(sender As Object, e As SessionStartEventArgs)
            OnSessionStart(e)
        End Sub

        Private Sub HandleSessionStop(sender As Object, e As SessionStopEventArgs)
            OnSessionStop(e)
        End Sub

        ''' <summary>
        ''' Called when a session variable changes on any of the connected Resource PCs.
        ''' Can be called at any time on any thread!
        ''' </summary>
        ''' <param name="sv">Details of the changed session variable.</param>
        Private Sub HandleSessionVarChanged(ByVal sv As clsSessionVariable)
            If Not mMonitorSessionVariables Then Return
            Dim key = sv.SessionID.ToString() & "." & sv.Name
            mSessionVariables(key) = sv
            Try                
                Dim objectString = ObjectEncoder.ObjectToBase64String(sv)
                OnSessionVariablesUpdated(New SessionVariableUpdatedEventArgs(objectString))
            Catch ex As Exception
                Log.Error(ex, "Failed to encode session variable change " + sv.Name)
            End Try
        End Sub

        Private Sub HandleSessionManagerVarUpdated(sender As Object, e As SessionVariableUpdatedEventArgs)
            OnSessionVariablesUpdated(e)
        End Sub

        Private Sub HandleRetryRequired(sender As Object, e As SignalResourceConnectionRetryRequiredEventArgs)
            'We want to retry RefreshResourceConnection, sleep so that all connections have been exited.
            Task.Run(Sub()
                         Try
                             Thread.Sleep(TimeSpan.FromSeconds(mRetrySleepTimerInSeconds))
                             RefreshResourceConnection(e.ResourceId)
                         Catch ex As Exception
                             Log.Debug("Failed to Refresh resource pc {connection}", New With {e.ResourceId})
                         End Try
                     End Sub)
        End Sub


        ''' <summary>
        ''' Called when a session ends on any of the connected Resource PCs.
        ''' Can be called at any time on any thread!
        ''' </summary>
        Private Sub HandleSessionEnd(sender As Object, e As SessionEndEventArgs)
            RemoveSessionVariableInformation(e.SessionId)
            OnSessionEnd(e)
        End Sub

        ''' <summary>
        ''' Handles the session end event for multiple sessions at the same time.
        ''' Cleans up session variable info for each session, but only fires the SessionEnd event once.
        ''' </summary>
        ''' <param name="sessionIds"></param>
        Private Sub BatchSessionEnd(sessionIds As IEnumerable(Of Guid))

            For Each sessionId In sessionIds
                RemoveSessionVariableInformation(sessionId)
            Next

            OnSessionEnd(New SessionEndEventArgs(sessionIds.First(), ""))
        End Sub

        Private Sub RemoveSessionVariableInformation(sessionId As Guid)
            ' When a session finishes, we remove any session variable information we have
            ' for it, because they no longer exist.
            If mMonitorSessionVariables Then
                Dim keysToRemove As New List(Of String)
                For Each key As String In mSessionVariables.Keys
                    Dim variable As clsSessionVariable = Nothing
                    If mSessionVariables.TryGetValue(key, variable) And
                   variable.SessionID = sessionId Then
                        keysToRemove.Add(key)
                    End If
                Next
                For Each key As String In keysToRemove
                    Dim value As clsSessionVariable = Nothing
                    mSessionVariables.TryRemove(key, value)
                Next
                If keysToRemove.Count > 0 Then OnSessionVariablesUpdated(New SessionVariableUpdatedEventArgs(""))
            End If
        End Sub
#End Region

#Region " Event Invokers "

        ''' <summary>
        ''' Raises the <see cref="SessionVariablesUpdated"/> event
        ''' </summary>
        Protected Overridable Sub OnSessionVariablesUpdated(e As SessionVariableUpdatedEventArgs)
            Try
                mResourceSessionEvents.SessionVariablesUpdated(New SessionVariablesUpdatedData(e.JSONData, e.ErrorMessage))
                RaiseEvent SessionVariablesUpdated(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling SessionVariablesUpdated")
            End Try

        End Sub

        ''' <summary>
        ''' Raises the <see cref="ResourceStatusChanged"/> event
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        ''' <remarks>This squashes any errors which occur in the listeners to ensure that
        ''' the manager is not knocked over by bady behaved handlers</remarks>
        Protected Overridable Sub OnResourceStatusChanged(e As ResourcesChangedEventArgs)
            Try
                Log.Trace($"Received Session Created event from app server")
                mResourceSessionEvents.ResourceStatusChanged(New ResourcesChangedData(e.OverallChange, e.Changes))
                RaiseEvent ResourceStatusChanged(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.ResourceStatusChanged")
            End Try
        End Sub

        ''' <summary>
        ''' Fires the <see cref="SessionCreate"/> event using the given values.
        ''' This is actually called by clsResourceConnection instances when they detect
        ''' a session has been created.
        ''' </summary>
        ''' <param name="e">The events detailing the session create event.</param>
        Protected Overridable Sub OnSessionCreate(ByVal e As SessionCreateEventArgs)
            Try
                Log.Debug($"Received Session Created event from app server ({e.SessionId}")

                If Not e.FromScheduler Then
                    mResourceSessionEvents.SessionCreated(New SessionCreatedData(e.State, e.ResourceId, e.ProcessId, e.SessionId,
                                                                        e.ScheduledSessionId,
                                                                        e.ErrorMessage,
                                                                        e.Data?.ToDictionary(Function(x) x.Key.ToString(),
                                                                                             Function(y) y.Value),
                                                                        e.UserId, e.Tag))
                End If
                RaiseEvent SessionCreate(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.SessionCreated")
                ' Ensure that badly behaved listeners don't break this manager
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        ''' <summary>
        ''' Fires the <see cref="SessionStart"/> event using the given values.
        ''' </summary>
        ''' <param name="e">The events detailing the session start event.</param>
        Protected Overridable Sub OnSessionStart(e As SessionStartEventArgs)
            Try
                Log.Debug($"Received Session Started Event from app server ({e.SessionId}")

                If Not e.FromScheduler Then
                    mResourceSessionEvents.SessionStart(New SessionStartedData(e.SessionId, e.ErrorMessage, e.UserId, e.UserMessage, e.ScheduledSessionId))
                End If
                RaiseEvent SessionStart(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.SessionStart")
                ' Ensure that badly behaved listeners don't break this manager
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub OnSessionStop(e As SessionStopEventArgs)
            Try
                Log.Debug($"Received Session Stop Event from app server ({e.SessionId}")

                If Not e.FromScheduler Then
                    mResourceSessionEvents.SessionStop(New SessionStopData(e.SessionId, e.ErrorMessage, e.ScheduledSessionId))
                End If
                RaiseEvent SessionStop(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.SessionStop")
                ' Ensure that badly behaved listeners don't break this manager
                Debug.Fail(ex.ToString())
            End Try
        End Sub


        ''' <summary>
        ''' Fires the <see cref="SessionDelete"/> event using the given values.
        ''' </summary>
        ''' <param name="e">The events detailing the session delete event.</param>
        Protected Overridable Sub OnSessionDelete(ByVal e As SessionDeleteEventArgs)
            Try
                Log.Debug($"Received Session Deleted Event from app server ({e.SessionId}")

                If Not e.FromScheduler Then
                    mResourceSessionEvents.SessionDelete(New SessionDeletedData(e.SessionId, e.ErrorMessage, e.UserId, e.ScheduledSessionId))
                End If
                RaiseEvent SessionDelete(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.SessionDelete")
                ' Ensure that badly behaved listeners don't break this manager
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        ''' <summary>
        ''' Fires the <see cref="SessionEnd"/> event using the given values.
        ''' </summary>
        ''' <param name="e">The events detailing the session end event.</param>
        Protected Overridable Sub OnSessionEnd(e As SessionEndEventArgs)
            Try
                Log.Debug($"Received Session End Event from app server ({e.SessionId}")
                mResourceSessionEvents.SessionEnd(New SessionEndData(e.SessionId, e.ErrorMessage))
                RaiseEvent SessionEnd(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.SessionStart")
                ' Ensure that badly behaved listeners don't break this manager
                Debug.Fail(ex.ToString())
            End Try
        End Sub

#End Region

#Region " Other Methods "

        ''' <summary>
        ''' Adds the given resource connection to this manager, ensuring that it is in
        ''' the map maintained by the manager, and that it has all necessary events wired
        ''' up to the handlers in this class.
        ''' </summary>
        ''' <param name="rc">The resource connection to add to this manager.</param>
        Private Sub AddConnection(rc As OnDemandConnection)
            Log.Debug("Adding connection {connection}", New With {rc.ResourceName, rc.ResourceId})
            mResourceConnections(rc.ResourceId) = rc
            mResourceNameMap(rc.ResourceMachine.Name) = rc.ResourceMachine
            AddEventHandlers(rc)
        End Sub

        ''' <summary>
        ''' Terminates and removes from this manager the connection to the resource with
        ''' the given ID.
        ''' </summary>
        ''' <param name="resourceId">The ID of the resource whose connection should be
        ''' terminated and removed</param>
        Private Sub TerminateConnectionForResource(resourceId As Guid)
            Log.Debug("Attempting to terminate Connection to Resource {resource}", New With {resourceId})
            mConnectionStatistics.Remove(resourceId)
            Dim rc As OnDemandConnection = Nothing
            If mResourceConnections.TryRemove(resourceId, rc) Then
                RemoveEventHandlers(rc)
                rc.Terminate()
                mResourceNameMap.TryRemove(rc.ResourceMachine.Name, Nothing)
                Log.Debug("Terminated connection to {resource}", New With {resourceId})
            End If
        End Sub


        Private Sub CheckForRecentlyEndedSessions()
            Dim sessions As List(Of Guid) = New List(Of Guid)
            Try
                Using reader As IDataReader = gSv.GetSessionsEndedAfter(mLastSessionEndDate).CreateDataReader()
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        Dim sessionId As Guid = prov.GetValue("sessionid", Guid.Empty)
                        Dim enddate As Date = prov.GetValue("enddatetime", Date.MinValue)

                        If enddate > mLastSessionEndDate Then
                            mLastSessionEndDate = enddate
                        End If
                        sessions.Add(sessionId)
                    End While
                End Using

            Catch ex As Exception
                Log.Warn(ex, "Error loading recently ended sessions from database")
                Return
            End Try

            If sessions.Any() Then
                BatchSessionEnd(sessions)
            End If

        End Sub


        ''' <summary>
        ''' Update the database status of any resource connections we have,
        ''' and add any new ones that have appeared...
        ''' </summary>
        Public Sub GetLatestDBResourceInfo() Implements IResourceConnectionManager.GetLatestDBResourceInfo
            GetLatestDBResourceInfo(ResourceAttribute.None)
        End Sub

        Public Sub GetLatestDBResourceInfo(excludedResources As ResourceAttribute) Implements IResourceConnectionManager.GetLatestDBResourceInfo
            Log.Debug($"Updating resource info from database excludedResources={excludedResources}")
            Dim resources As ICollection(Of ResourceInfo)
            Try
                resources = gSv.GetResourceInfo(ResourceAttribute.None, excludedResources Or ResourceAttribute.Retired)
            Catch ex As Exception
                Log.Warn(ex, "Error loading resource info from database")
                Return 'Can't update when we cannot get resources
            End Try

            CheckLatestDBResourceInfo(resources)        'update the resource information
            RefreshResources(resources)
            ClearStatisticsForResourceStatus(resources, ResourceDBStatus.Offline)
        End Sub

        Private Sub CheckLatestDBResourceInfo(resources As ICollection(Of ResourceInfo))

            Log.Debug($"CheckLatestDBResourceInfo Checking {resources.Count} resources")
            Dim poolmachines As New Dictionary(Of Guid, List(Of IResourceMachine))

            If resources IsNot Nothing Then
                ' Check to see if any resources have been deleted
                For Each resourceConnection As KeyValuePair(Of Guid, OnDemandConnection) In mResourceConnections
                    If Not resources.Where(Function(resource) resource.ID = resourceConnection.Key).Any() Then
                        ' The resource has been deleted, therefore remove it from mResourceConnections
                        TerminateConnectionForResource(resourceConnection.Key)
                    End If
                Next

                For Each r As ResourceInfo In resources
                    Dim inPool As Boolean = r.Pool <> Nothing

                    'Decide if we want to connect to this Resource or not. This depends on
                    'our mode, and what kind of resource it is.
                    Dim usethisone As Boolean

                    If inPool Then
                        Log.Debug("Is pool resource. {}", New With {r.ID, mMode})
                        If mMode = IResourceConnectionManager.Modes.Controller Then
                            usethisone = r.Pool.Equals(mPoolID) And Not r.ID.Equals(mControllerID)
                        ElseIf mMode = IResourceConnectionManager.Modes.PoolMember Then
                            usethisone = r.ID.Equals(mControllerID)
                        Else
                            usethisone = False
                        End If
                    Else
                        usethisone = (mMode = IResourceConnectionManager.Modes.Normal)
                    End If

                    ' If this is hidden to us, let's not display it
                    If Not usethisone Then
                        TerminateConnectionForResource(r.ID)
                    End If

                    Dim mach As IResourceMachine = Nothing
                    If mResourceConnections.ContainsKey(r.ID) Then
                        mach = mResourceConnections(r.ID).ResourceMachine
                    Else
                        If usethisone Then
                            ' Create a connection to a newly added resource...
                            If Not r.Attributes.HasAnyFlag(ResourceAttribute.Debug Or ResourceAttribute.Retired) Then
                                AddConnection(New OnDemandConnection(r.ID, r.Name, r.Status, r.Attributes, r.UserID, Me, mThreadStackSize, mPingTimeoutSeconds, mConnectionPingCooldown, mProcessResourceInputSleepTime))
                                mach = mResourceConnections(r.ID).ResourceMachine
                            Else
                                Log.Trace("Ignoring resource {Resource}.  Resource is Debug or Retired", New With {r.Name, r.ID})
                            End If
                        Else
                            mach = New ResourceMachine(Nothing, r.Name, r.ID, r.Attributes)
                            If Not poolmachines.ContainsKey(r.Pool) Then
                                Log.Debug("Creating new pool machine collection {}", New With {r.Pool})
                                poolmachines.Add(r.Pool, New List(Of IResourceMachine))
                            End If
                            Log.Debug("Added pool machine {} ", New With {Key .PoolId = r.Pool, .MachineName = mach.Name, .MachineId = mach.Id})
                            poolmachines(r.Pool).Add(mach)
                        End If
                    End If

                    If mach IsNot Nothing Then
                        Log.Trace("Updating ResourceMachine {Status}", New With {Key .ResourceId = r.ID, r.Status, r.Attributes, r.DisplayStatus, r.Information, inPool})
                        'Update resource information from server
                        mach.DBStatus = r.Status
                        mach.Attributes = r.Attributes
                        mach.DisplayStatus = r.DisplayStatus
                        mach.Info = r.Information
                        mach.InfoColour = Color.FromArgb(r.InfoColour)
                        mach.IsInPool = inPool
                    End If
                Next
            End If

            ' Update Child details of all the resource machines (Only Pools have children)
            For Each resourceConnection As KeyValuePair(Of Guid, OnDemandConnection) In mResourceConnections
                If poolmachines.ContainsKey(resourceConnection.Key) Then
                    resourceConnection.Value.ResourceMachine.ChildResources = poolmachines(resourceConnection.Key)
                Else
                    resourceConnection.Value.ResourceMachine.ChildResources = Nothing
                End If
            Next
        End Sub

        Public Sub RefreshResources(resources As ICollection(Of ResourceInfo))
            If Interlocked.CompareExchange(mLock, 1, 0) = 0 Then
                Try
                    Dim statistics = mConnectionStatistics.GetAll()
                    For Each resource In resources
                        Dim result = New ResourceConnectionStatistic()
                        resource.LastConnectionStatistics = If(statistics?.TryGetValue(resource.ID, result),
                                                               result, Nothing)
                    Next

                    Dim onlineResources = New HashSet(Of Guid)(resources.Where(Function(s) s.Status <> ResourceDBStatus.Offline _
                                                                                           AndAlso s.LastConnectionStatistics IsNot Nothing _
                                                                                           AndAlso s.LastConnectionStatistics.ConnectionSuccess) _
                                                                  .Select(Function(y) y.ID))

                    mCheckSet = New HashSet(Of Guid)(mCheckSet.Where(Function(f) onlineResources.Contains(f)))

                    Dim resourcestoCheck = resources.Where(Function(s)
                                                               Return Not s.Attributes.HasFlag(ResourceAttribute.DefaultInstance) AndAlso
                                                           Not mCheckSet.Contains(s.ID) AndAlso
                                                           s.Status <> ResourceDBStatus.Offline
                                                           End Function).Select(Function(t) t.ID)
                    mLastCheck = Date.UtcNow
                    If resourcestoCheck.Any() Then
                        ConnectToResources(resourcestoCheck)
                    End If

                Finally
                    mLock = 0       'reset the lock....
                End Try
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format(My.Resources.OnDemandConnectionManagerToString, mResourceSessionEvents.ToString())
        End Function

        Public Sub EnsureResourceCapNotExceeded(name As String)
            If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))

            Dim count = OnlineResourcesCount(ResourceAttribute.None)
            Dim resmachine As IResourceMachine = Nothing
            ' determine if the given resource is already connected
            Dim reconnecting =
            mResourceNameMap.TryGetValue(name, resmachine) AndAlso
            resmachine.ConnectionState = ResourceConnectionState.Sleep AndAlso
            resmachine.DBStatus = ResourceMachine.ResourceDBStatus.Ready

            ' if we've reached our cap. throw
            If Not reconnecting AndAlso ResourceLimit <= count Then
                Throw New ResourceLimitExceededException(My.Resources.clsServerResources_maxappserverrobotconnections, count)
            End If
        End Sub

        Private Sub ConnectToResources(resourceIds As IEnumerable(Of Guid))
            Log.Debug("Refreshing {count} resources", New With {resourceIds.Count})
            For Each id As Guid In resourceIds
                Dim connection As OnDemandConnection = Nothing
                Dim connectionFound As Boolean

                connectionFound = mResourceConnections.TryGetValue(id, connection)
                If connectionFound AndAlso Not connection.Processing Then
                    RefreshResourceConnection(connection)
                    mCheckSet.Add(id)

                    ' Starting all the connections at the same time causes untold contention
                    ' Leaving this small gap spreads out the connections
                    Thread.Sleep(mResourceInitializationPause)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Refresh resource connection.
        ''' </summary>
        ''' <param name="resourceId"></param>
        Public Sub RefreshResourceConnection(resourceId As Guid) Implements IResourceConnectionManager.RefreshResourceConnection
            RefreshResourceConnection(mResourceConnections(resourceId))
        End Sub

        Public Function CheckInstructionalClientStatus() As Boolean Implements IResourceConnectionManager.CheckInstructionalClientStatus
            ' not relevent for server
            Return True
        End Function

        ''' <summary>
        ''' Signal a given connection on a thread.
        ''' </summary>
        ''' <param name="connection"></param>
        ''' <returns></returns>
        Private Function RefreshResourceConnection(connection As OnDemandConnection) As Task(Of boolean)
            Return connection.RefreshResource()
        End Function


        Private Sub SignalResourceComplete(sender As Object, e As EventArgs)
            Dim connection = CType(sender, OnDemandConnection)
            Log.Debug("Signal Resource complete {resource}", New With {connection.ResourceId, connection.ResourceName})
        End Sub

        Private Sub HandleErrors(sender As Object, e As EventArgs)
            Dim connection = CType(sender, OnDemandConnection)
            Log.Debug("An Error occurred communicating with resource {connection}", New With {connection})
        End Sub

        Private Sub HandleConnected(sender As Object, e As EventArgs)
            Dim connection = CType(sender, OnDemandConnection)
            Log.Debug("Resource connected: {resource}", New With {connection.ResourceId, connection.ResourceName})
        End Sub

        Private Sub HandlePingFailed(sender As Object, e As EventArgs)
            Dim connection = CType(sender, OnDemandConnection)
            Log.Debug("Failed PING communicated with resource {resource}", New With {connection.ResourceName})
            mCheckSet.Remove(connection.ResourceId)
        End Sub

        Private Sub HandlePingCompleted(sender As Object, e As EventArgs)
            Dim connection = CType(sender, OnDemandConnection)
            Log.Trace("Successfully PING communicated with resource {resource}", New With {connection.ResourceName})
        End Sub

        Private Sub HandleNewStatistic(sender As Object, e As NewConnectionStatisticEventArgs)
            Dim statistic = New ResourceConnectionStatistic(e.Success, e.Ping, e.Time)
            mConnectionStatistics.Update(e.ResourceId, statistic)
        End Sub

        Private Sub ClearStatisticsForResourceStatus(resources As IEnumerable(Of ResourceInfo), statusRemoved As ResourceMachine.ResourceDBStatus)
            For Each resource In From filteredResources In resources Where filteredResources.Status = statusRemoved
                mConnectionStatistics.Remove(resource.ID)
            Next
        End Sub

        ''' <summary>
        ''' Adds the event handlers maintained by this manager to the given resource
        ''' connection
        ''' </summary>
        ''' <param name="rc">The resource connection to remove the event handlers from.
        ''' </param>
        Private Sub AddEventHandlers(rc As OnDemandConnection)
            AddHandler rc.SessionVariableChanged, AddressOf HandleSessionVarChanged
            AddHandler rc.SessionEnd, AddressOf HandleSessionEnd
            AddHandler rc.SessionCreate, AddressOf HandleSessionCreate
            AddHandler rc.SessionDelete, AddressOf HandleSessionDelete
            AddHandler rc.SessionStart, AddressOf HandleSessionStart
            AddHandler rc.SessionStop, AddressOf HandleSessionStop
            AddHandler rc.SignalResourceComplete, AddressOf SignalResourceComplete
            AddHandler rc.Errors, AddressOf HandleErrors
            AddHandler rc.Connected, AddressOf HandleConnected
            AddHandler rc.PingCompleted, AddressOf HandlePingCompleted
            AddHandler rc.PingFailed, AddressOf HandlePingFailed
            AddHandler rc.Statistic, AddressOf HandleNewStatistic
            AddHandler rc.ResourceStatusChanged, AddressOf HandleResourceChanged
            AddHandler rc.SessionManagerVariableUpdated, AddressOf HandleSessionManagerVarUpdated
            AddHandler rc.RetryRequired, AddressOf HandleRetryRequired

        End Sub

        ''' <summary>
        ''' Removes the event handlers maintained by this manager from the given resource
        ''' connection
        ''' </summary>
        ''' <param name="rc">The resource connection to remove the event handlers from.
        ''' </param>
        Private Sub RemoveEventHandlers(rc As OnDemandConnection)
            RemoveHandler rc.SessionVariableChanged, AddressOf HandleSessionVarChanged
            RemoveHandler rc.SessionEnd, AddressOf HandleSessionEnd
            RemoveHandler rc.SessionCreate, AddressOf HandleSessionCreate
            RemoveHandler rc.SessionDelete, AddressOf HandleSessionDelete
            RemoveHandler rc.SessionStart, AddressOf HandleSessionStart
            RemoveHandler rc.SessionStop, AddressOf HandleSessionStop
            RemoveHandler rc.SignalResourceComplete, AddressOf SignalResourceComplete
            RemoveHandler rc.Errors, AddressOf HandleErrors
            RemoveHandler rc.Connected, AddressOf HandleConnected
            RemoveHandler rc.PingCompleted, AddressOf HandlePingCompleted
            RemoveHandler rc.PingFailed, AddressOf HandlePingFailed
            RemoveHandler rc.Statistic, AddressOf HandleNewStatistic
            RemoveHandler rc.ResourceStatusChanged, AddressOf HandleResourceChanged
            RemoveHandler rc.RetryRequired, AddressOf HandleRetryRequired
        End Sub

        ''' <summary>
        ''' Get the next waiting user message. The act of getting the
        ''' message removes it from the queue.
        ''' </summary>
        ''' <returns>The next waiting message, or an empty string if
        ''' there isn't one.</returns>
        Private Function GetNextUserMessage() As String
            Dim c As OnDemandConnection
            Dim sMsg As String
            For Each c In mResourceConnections.Values
                sMsg = c.GetNextUserMessage()
                If sMsg <> String.Empty Then Return sMsg
            Next
            Return String.Empty
        End Function

        ''' <summary>
        ''' Tries to get the next user message if one is there. If it is there, it is
        ''' set into the out variable and True is returned. Otherwise, the out variable
        ''' is set to an empty string and False is returned.
        ''' </summary>
        ''' <param name="msg">On exit, the next user message or an empty string if no
        ''' user message was found.</param>
        ''' <returns>True if a user message was found and populated into
        ''' <paramref name="msg"/>; False if none was found and an empty string was
        ''' populated instead.</returns>
        Public Function TryGetNextUserMessage(ByRef msg As String) As Boolean Implements IResourceConnectionManager.TryGetNextUserMessage
            msg = GetNextUserMessage()
            Return msg <> String.Empty
        End Function


        Public Function GetAllResourceConnectionStatistics() As IDictionary(Of Guid, ResourceConnectionStatistic) Implements IUserAuthResourceConnectionManager.GetAllResourceConnectionStatistics
            Return mConnectionStatistics.GetAll()
        End Function


        Public Sub SendGetSessionVariablesAsUser(token As clsAuthToken, resourceId As Guid, sessionID As Guid) Implements IUserAuthResourceConnectionManager.SendGetSessionVariablesAsUser
            Try
                Dim connection = mResourceConnections(resourceId)
                Log.Debug("Send Get Session Variables by user {info}", New With {resourceId, sessionID})
                connection.SendGetSessionVar(sessionID, token)
                RefreshResourceConnection(connection)
            Catch ex As Exception
                Log.Error("Exception while sending get session variable command as user {ex}", New With {ex.Message})
                Return
            End Try
        End Sub
#End Region

    End Class

End Namespace
