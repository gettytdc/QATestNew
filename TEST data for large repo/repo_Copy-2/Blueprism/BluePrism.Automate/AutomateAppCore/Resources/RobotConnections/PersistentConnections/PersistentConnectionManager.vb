Imports System.Collections.Concurrent
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Timers
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.BPCoreLib
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models
Imports NLog

''' Project  : Automate
''' Class    : clsResourceConnectionManager
''' 
''' <summary>
''' This connection manager opens and maintains a connection to each resource
''' listed in the database, or to specific resources only, depending on how it
''' is configured.
''' 
''' The connection to each Resource PC is handled by an instance of
''' clsResourceConnection, which maintains its own thread to TCP communication
''' with the PC via clsTalker. By this mechanism, the control is able to
''' determine when status changes occur on Resource PCs, such as processes
''' starting, failing, etc.
''' 
''' The class raises an event, StatusChanged, whenever it detects that a
''' significant change in status occurs. Control Room handles this event by
''' updating the environment view. A finer level of granularity is possible with
''' the existing protocol, but is not currently provided at this level.
''' </summary>
'''
Namespace Resources
    Public Class PersistentConnectionManager : Implements IUserAuthResourceConnectionManager

#Region " Class-scope Declarations "

        Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

        ''' <summary>
        ''' The database polling interval to refresh the view. This is in units of
        ''' whatever the timer interval is.
        ''' </summary>
        Private Const ciDbPollInterval As Integer = 30000 \ mRefreshTimerInterval

        ''' <summary>
        ''' Interval for the mRefreshTimer
        ''' </summary>
        Private Const mRefreshTimerInterval As Integer = 250

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

        ''' <summary>
        ''' Event raised when a session has been Deleted
        ''' </summary>
        Public Event SessionDelete As SessionDeleteEventHandler Implements IResourceConnectionManager.SessionDelete

        ''' <summary>
        ''' Event raised when a session has been ended
        ''' </summary>
        Public Event SessionEnd As SessionEndEventHandler Implements IResourceConnectionManager.SessionEnd

        Public Event SessionStop As SessionStopEventHandler Implements IResourceConnectionManager.SessionStop

        ''' <summary>
        ''' Event raised when any session variables have been updated
        ''' </summary>
        Public Event SessionVariablesUpdated As SessionVariableUpdatedHandler Implements IResourceConnectionManager.SessionVariablesUpdated

        ' Used in ASCR and Persistant uses an older method of queuing user messages. May use this event later but not for now
        Public Event ShowUserMessage As EventHandler(Of String) Implements IResourceConnectionManager.ShowUserMessage

#End Region

#Region " Member Variables "

        Private ReadOnly mSessionVariables As New ConcurrentDictionary(Of String, clsSessionVariable)

        Private mMonitorSessionVariables As Boolean = False
        Private mMode As IResourceConnectionManager.Modes
        Private mPoolID As Guid

        ''' <summary>
        ''' When in Controller mode, this is the ID of the controller.
        ''' </summary>
        Private mControllerID As Guid

        ''' <summary>
        ''' Timer which causes us to poll the database every now and again
        ''' </summary>
        Private WithEvents mRefreshTimer As New System.Timers.Timer()

        ''' <summary>
        ''' Timer used to schedule signaling of the rate limiter
        ''' </summary>
        Private WithEvents mRateLimiterTimer As New System.Timers.Timer()

        ''' <summary>
        ''' Countdown until next poll of database to detect newly added
        ''' resources etc.
        ''' </summary>
        Private miPollDBCount As Integer

        ''' <summary>
        ''' A collection of clsResourceConnectionPoll objects, representing our
        ''' connections to resource PCs. Connections are made when the
        ''' control is loaded, and remain open until it is closed.
        ''' The dictionary is keyed on the resource ID of the resource machine which
        ''' is connected to in the resource connection.
        ''' </summary>
        Private ReadOnly mResourceConnections As ConcurrentDictionary(Of Guid, PersistentConnection)

        ''' <summary>
        ''' A dictionary to allow quick look up for the IResourceMachine via a given name
        ''' </summary>
        Private ReadOnly mResourceNameMap As ConcurrentDictionary(Of String, IResourceMachine)


        Private mRateLimiter As New AutoResetEvent(True)
        Private mUser As IUser

        Private mThreadStackSize As Integer = 0
        Private mPingTimeoutSeconds As Integer = 0

        ' Flag indicating if this connection manager is disposed
        Private mIsDisposed As Boolean

        Private ReadOnly mResourceRefreshes As List(Of (Id As Guid, Time As DateTime)) = New List(Of (Guid, DateTime))()

        Private ReadOnly mCancellationTokenSource As CancellationTokenSource = New CancellationTokenSource()
        Private ReadOnly mStartupDelay As Integer
        Private Const MaxResourceRefreshesPerBlockPeriod As Integer = 5
        Private Const RefreshBlockPeriodInSeconds As Integer = 60
        Private mDisableConnections As Boolean

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
                For Each r As PersistentConnection In mResourceConnections.Values
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
                Return New TimeSpan(0, 1, 0)
            End Get
        End Property

        Public ReadOnly Property ServerName As String Implements IResourceConnectionManager.ServerName
        Public ReadOnly Property UsingAppServerConnection As Boolean Implements IResourceConnectionManager.UsingAppServerConnection
            Get
                Return False
            End Get
        End Property

#End Region

#Region " Constructors "
        Public Sub New(mode As IResourceConnectionManager.Modes,
                   user As IUser,
                   poolid As Guid,
                   controllerid As Guid)

            mMode = mode
            mUser = user
            mPoolID = poolid
            mControllerID = controllerid
            mResourceConnections = New ConcurrentDictionary(Of Guid, PersistentConnection)
            mResourceNameMap = New ConcurrentDictionary(Of String, IResourceMachine)
            ServerName = String.Empty

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

            mPingTimeoutSeconds = gSv.GetPref(PreferenceNames.ResourceConnection.PingTimeOutSeconds, 30)
            QueryCapabilities = gSv.GetPref(PreferenceNames.ResourceConnection.QueryCapabilities, False)
            mDisableConnections = Not gSv.IsServer AndAlso gSv.GetPref(ResourceConnection.DisableConnection, False)
            miPollDBCount = ciDbPollInterval
            Init()

        End Sub

        Private Async Sub Init()

            Await Task.Delay(mStartupDelay * 1000)

            If mCancellationTokenSource?.Token.IsCancellationRequested OrElse IsDisposed Then
                Exit Sub
            End If

            mRefreshTimer.Interval = mRefreshTimerInterval
            mRefreshTimer.AutoReset = False
            mRefreshTimer.Start()

            GetLatestDBResourceInfo()

            Dim refreshBlockCleanupTask =
            New TaskFactory(mCancellationTokenSource.Token).
            StartNew(CType(
                Async Sub()
                    While Not mCancellationTokenSource.Token.IsCancellationRequested
                        SyncLock mResourceRefreshes
                            Dim removeBefore = Date.UtcNow.AddSeconds(-(RefreshBlockPeriodInSeconds * 2))
                            mResourceRefreshes.RemoveAll(Function(x) x.Time < removeBefore)
                        End SyncLock
                        Await Task.Delay(5000)
                    End While
                End Sub, Action),
                TaskCreationOptions.LongRunning)

            Dim rateLimiter =
            New TaskFactory(mCancellationTokenSource.Token).
            StartNew(CType(
                Async Sub()
                    While Not mCancellationTokenSource.Token.IsCancellationRequested
                        mRateLimiter.Set()
                        Await Task.Delay(250)
                    End While
                End Sub, Action),
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
                            For Each c As PersistentConnection In mResourceConnections.Values
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
        ''' Get information on the members of the specified pool, in the form of a List
        ''' of clsResourceMachine objects.
        ''' </summary>
        ''' <param name="poolid">The Resource ID of the pool.</param>
        ''' <returns>A List of clsResourceMachinePoll objects describing the members of the
        ''' pool. These will not be 'fully fledged' instances of this class of the kind
        ''' you normally get from the GetResources-style methods. They are temporarily
        ''' constructed to hold the basic information about the resources, as retrieved
        ''' from the controller.</returns>
        ''' <remarks>Because this method needs to communicate with the controller of the
        ''' pool to get the information it needs, care should be taken to call it only
        ''' when necessary.</remarks>
        Public Function GetMemberResources(ByVal poolid As Guid) As ICollection(Of IResourceMachine)

            Dim lst As New List(Of IResourceMachine)
            Try
                Dim conn As PersistentConnection = mResourceConnections(poolid)
                Dim first As Boolean = True
                For Each mi As clsTalker.MemberInfo In conn.GetMembers()
                    Dim rm As New ResourceMachine(mi.ConnectionState, mi.Name, mi.ID, ResourceAttribute.None)
                    rm.IsController = first
                    first = False
                    lst.Add(rm)
                Next
            Catch ex As KeyNotFoundException
                ' Silently ignore a nonexistent resource connection
            End Try
            Return lst

        End Function

        ''' <summary>
        ''' Gets a list of all available (even if not actually connected) resources
        ''' </summary>
        ''' <returns>A collection of clsResourceMachinePoll instances containing the
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
        ''' <returns>A List of clsResourceMachinePoll instances containing the requested
        ''' resources.</returns>
        Public Function GetResources(ByVal connectedOnly As Boolean) _
     As Dictionary(Of Guid, IResourceMachine) Implements IResourceConnectionManager.GetResources
            Dim list As New Dictionary(Of Guid, IResourceMachine)
            For Each c As PersistentConnection In mResourceConnections.Values
                ' We don't return 'unavailable' resources (eg. local on remote machines)
                If c.IsUnavailable Then Continue For
                ' If it's not connected and we only want connected machines, skip it too
                If connectedOnly AndAlso Not c.IsConnected Then Continue For
                ' Otherwise add it to the list
                list.Add(c.ResourceMachine.Id, c.ResourceMachine)
            Next
            Return list
        End Function

        Public Function GetActiveResources(connectedOnly As Boolean) _
        As List(Of IResourceMachine) Implements IResourceConnectionManager.GetActiveResources

            Dim list As New List(Of IResourceMachine)

            For Each c As PersistentConnection In mResourceConnections.Values
                If c.IsAvailable(connectedOnly) Then
                    list.Add(c.ResourceMachine)
                End If
            Next

            Return list
        End Function


        ''' <summary>
        ''' Get the Resource responsible for controlling the one with the specified name.
        ''' This applies when the named Resource is a member of a Pool. In these
        ''' circumstances, there will be no clsResourceMachinePoll record of that Resource,
        ''' since we do not communicate with it directly, but instead communicate with
        ''' the Pool's controller, which is what this method finds.
        ''' </summary>
        ''' <param name="name">The name of the Resource sought.</param>
        ''' <returns>The controlling clsResourceMachinePoll, or Nothing if not found.
        ''' </returns>
        Public Function GetControllingResource(ByVal name As String) As IResourceMachine Implements IResourceConnectionManager.GetControllingResource
            For Each c As PersistentConnection In mResourceConnections.Values
                If c.ResourceMachine.HasPoolMember(name) Then Return c.ResourceMachine
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the resource with the specified ID, if one exists.
        ''' </summary>
        ''' <param name="ResourceID">The ID of the resource sought.</param>
        ''' <returns>Returns the first resource found with the specified ID, if
        ''' one exists; otherwise returns nothing.</returns>
        Public Function GetResource(ByVal ResourceID As Guid) As IResourceMachine Implements IResourceConnectionManager.GetResource
            Try
                Return mResourceConnections(ResourceID).ResourceMachine
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
        Public Function GetResource(name As String) As IResourceMachine Implements IResourceConnectionManager.GetResource
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
                    connection.RefreshResource()
                Catch ex As KeyNotFoundException
                    Throw New NoSuchResourceException("No resource found with ID {0}", session.ResourceId)
                End Try
            Next
        End Sub

        Public Function SendCreateSession(sessionData As IEnumerable(Of CreateSessionData)) As Guid() Implements IResourceConnectionManager.SendCreateSession
            Dim sessionIds As New List(Of Guid)
            For Each s In sessionData
                Try
                    Log.Debug("Send Create Session {info}", New With {s.ResourceId, s.ProcessId, s.QueueIdentifier})
                    Dim connection = mResourceConnections(s.ResourceId)
                    sessionIds.Add(connection.SendCreateSession(s))
                    connection.RefreshResource()
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
                    connection.RefreshResource()
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
                    connection.RefreshResource()
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
                Log.Debug("Send Set Session Variable {info}", New With {resourceID, vars})
                Dim connection = mResourceConnections(resourceID)
                connection.SendSetSessionVariable(vars)
                connection.RefreshResource()
            Catch ex As KeyNotFoundException
                Throw New NoSuchResourceException("No resource found with ID {0}", resourceID)
            End Try
        End Sub

        Public Sub ToggleShowSessionVariables(monitorSessionVars As Boolean) Implements IResourceConnectionManager.ToggleShowSessionVariables
            MonitorSessionVariables = monitorSessionVars
        End Sub


        Public Sub SendGetSessionVariables(resourceId As Guid, sessionID As Guid, processId As Guid, ByRef sErr As String) Implements IResourceConnectionManager.SendGetSessionVariables
            Try
                Log.Debug("Send Get Session Var {info}", New With {resourceId, sessionID, processId})
                Dim connection = mResourceConnections(resourceId)
                connection.SendGetVariable(sessionID, sErr)
                connection.RefreshResource()
            Catch ex As KeyNotFoundException
                Throw New NoSuchResourceException("No resource found with ID {0}", resourceId)
            End Try
        End Sub

        Public Sub SendGetSessionVariablesAsUser(token As clsAuthToken, resourceId As Guid, sessionID As Guid) Implements IUserAuthResourceConnectionManager.SendGetSessionVariablesAsUser
            Dim connection = mResourceConnections(resourceId)
            Log.Debug("Send Get Session Variables by user {info}", New With {resourceId, sessionID})
            connection.SendGetSessionVar(sessionID, token)
            connection.RefreshResource()
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

                'Check for status changes reported by any of our connections,
                'and update the list if necessary.
                Dim accum As ResourceStatusChange = ResourceStatusChange.None
                Dim map As New Dictionary(Of String, ResourceStatusChange)
                If mResourceConnections Is Nothing Then Exit Sub
                For Each con As PersistentConnection In mResourceConnections.Values
                    If con.PopStatusHasChanged() Then
                        SyncLock mResourceRefreshes
                            mResourceRefreshes.Add((con.ResourceId, DateTime.UtcNow))

                            Dim countRefreshesSince = DateTime.UtcNow.AddSeconds(-RefreshBlockPeriodInSeconds)
                            ' ReSharper disable once VBReplaceWithSingleCallToCount - Count cannot be used in this way on a List

                            'user messages can't be ignored, so don't skip.
                            If Not con.HasUserMessage Then
                                If mResourceRefreshes.
                               Where(Function(x) x.Id = con.ResourceId AndAlso (x.Time >= countRefreshesSince)).
                               Count() > MaxResourceRefreshesPerBlockPeriod Then Continue For
                            End If
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

        ''' <summary>
        ''' Called when a session variable changes on any of the connected Resource PCs.
        ''' Can be called at any time on any thread!
        ''' </summary>
        ''' <param name="sv">Details of the changed session variable.</param>
        Private Sub HandleSessionVarChanged(ByVal sv As clsSessionVariable)
            If Not mMonitorSessionVariables Then Return
            Dim key = sv.SessionID.ToString() & "." & sv.Name
            mSessionVariables(key) = sv
            OnSessionVariablesUpdated(New SessionVariableUpdatedEventArgs(String.Empty))
        End Sub

        ''' <summary>
        ''' Called when a session ends on any of the connected Resource PCs.
        ''' Can be called at any time on any thread!
        ''' </summary>
        Private Sub HandleSessionEnd(sender As Object, e As SessionEndEventArgs)
            ' When a session finishes, we remove any session variable information we have
            ' for it, because they no longer exist.
            If mMonitorSessionVariables Then
                Dim keysToRemove As New List(Of String)
                For Each key As String In mSessionVariables.Keys
                    Dim variable As clsSessionVariable = Nothing
                    If mSessionVariables.TryGetValue(key, variable) And
                   variable.SessionID = e.SessionId Then
                        keysToRemove.Add(key)
                    End If
                Next
                For Each key As String In keysToRemove
                    Dim value As clsSessionVariable = Nothing
                    mSessionVariables.TryRemove(key, value)
                Next
                If keysToRemove.Count > 0 Then OnSessionVariablesUpdated(New SessionVariableUpdatedEventArgs(String.Empty))
            End If
            OnSessionEnd(e)
        End Sub

#End Region

#Region " Event Invokers "

        ''' <summary>
        ''' Raises the <see cref="SessionVariablesUpdated"/> event
        ''' </summary>
        Protected Overridable Sub OnSessionVariablesUpdated(e As SessionVariableUpdatedEventArgs)
            RaiseEvent SessionVariablesUpdated(Me, e)
        End Sub

        ''' <summary>
        ''' Raises the <see cref="ResourceStatusChanged"/> event
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        ''' <remarks>This squashes any errors which occur in the listeners to ensure that
        ''' the manager is not knocked over by bady behaved handlers</remarks>
        Protected Overridable Sub OnResourceStatusChanged(e As ResourcesChangedEventArgs)
            Try
                RaiseEvent ResourceStatusChanged(Me, e)
            Catch
            End Try
        End Sub

        ''' <summary>
        ''' Fires the <see cref="SessionCreate"/> event using the given values.
        ''' This is actually called by clsResourceConnectionPole instances when they detect
        ''' a session has been created.
        ''' </summary>
        ''' <param name="e">The events detailing the session create event.</param>
        Protected Overridable Sub OnSessionCreate(ByVal e As SessionCreateEventArgs)
            Try
                RaiseEvent SessionCreate(Me, e)
            Catch ex As Exception
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
                RaiseEvent SessionStart(Me, e)
            Catch ex As Exception
                Log.Warn(ex, "Error while handling ResourceConnectionManager.SessionStart")
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
                RaiseEvent SessionDelete(Me, e)
            Catch ex As Exception
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
        Private Sub AddConnection(rc As PersistentConnection)
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
            Log.Debug("Attempting to terminating Connection to Resource {resource}", New With {resourceId})
            Dim rc As PersistentConnection = Nothing
            If mResourceConnections.TryRemove(resourceId, rc) Then
                RemoveEventHandlers(rc)
                rc.Terminate()
                mResourceNameMap.TryRemove(rc.ResourceMachine.Name, Nothing)
                Log.Debug("Terminated connection to {resource}", New With {resourceId})
            End If
        End Sub

        ''' <summary>
        ''' Update the database status of any resource connections we have,
        ''' and add any new ones that have appeared...
        ''' </summary>
        Public Sub GetLatestDBResourceInfo() Implements IResourceConnectionManager.GetLatestDBResourceInfo

            Log.Debug("Updating resource info from database")
            Dim resources As ICollection(Of ResourceInfo)
            Try
                resources = gSv.GetResourceInfo(ResourceAttribute.None, ResourceAttribute.None)
            Catch ex As Exception
                Log.Warn(ex, "Error loading resource info from database")
                Return 'Can't update when we cannot get resources
            End Try

            CheckLatestDBResourceInfo(resources)
        End Sub

        Public Sub GetLatestDBResourceInfo(excludedResources As ResourceAttribute) Implements IResourceConnectionManager.GetLatestDBResourceInfo
            Log.Debug("Updating resource info from database")
            Dim resources As ICollection(Of ResourceInfo)
            Try
                resources = gSv.GetResourceInfo(ResourceAttribute.None, excludedResources)
            Catch ex As Exception
                Log.Warn(ex, "Error loading resource info from database")
                Return 'Can't update when we cannot get resources
            End Try

            CheckLatestDBResourceInfo(resources)
        End Sub

        Private Sub CheckLatestDBResourceInfo(resources As ICollection(Of ResourceInfo))

            Dim poolmachines As New Dictionary(Of Guid, List(Of IResourceMachine))

            If resources IsNot Nothing Then
                ' Check to see if any resources have been deleted
                For Each resourceConnection As KeyValuePair(Of Guid, PersistentConnection) In mResourceConnections
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
                    If Not usethisone Then TerminateConnectionForResource(r.ID)

                    Dim mach As IResourceMachine = Nothing
                    If mResourceConnections.ContainsKey(r.ID) Then
                        mach = mResourceConnections(r.ID).ResourceMachine
                    Else
                        If usethisone Then
                            ' Create a connection to a newly added resource...
                            If Not r.Attributes.HasAnyFlag(ResourceAttribute.Debug Or ResourceAttribute.Retired) Then
                                AddConnection(New PersistentConnection(r.ID, r.Name, r.Status, r.Attributes, r.UserID, Me, mThreadStackSize, mPingTimeoutSeconds, mDisableConnections))
                                mach = mResourceConnections(r.ID).ResourceMachine
                            Else
                                Log.Debug("Ignoring resource {Resource}.  Resource is Debug or Retired", New With {r.Name, r.ID})
                            End If
                        Else
                            Log.Debug("Creating Resource Machine {resource}", New With {Key .Name = r.Name, Key .Id = r.ID})
                            mach = New ResourceMachine(Nothing, r.Name, r.ID, r.Attributes)
                            If Not poolmachines.ContainsKey(r.Pool) Then
                                Log.Debug("Adding pool {pool}", New With {r.Pool})
                                poolmachines.Add(r.Pool, New List(Of IResourceMachine))
                            End If
                            Log.Debug("Adding machine to {pool}", New With {Key .PoolId = r.Pool, Key .MachineId = mach.Id})
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
            For Each resourceConnection As KeyValuePair(Of Guid, PersistentConnection) In mResourceConnections
                If poolmachines.ContainsKey(resourceConnection.Key) Then
                    resourceConnection.Value.ResourceMachine.ChildResources = poolmachines(resourceConnection.Key)
                Else
                    resourceConnection.Value.ResourceMachine.ChildResources = Nothing
                End If
            Next
        End Sub


        ''' <summary>
        ''' Adds the event handlers maintained by this manager to the given resource
        ''' connection
        ''' </summary>
        ''' <param name="rc">The resource connection to remove the event handlers from.
        ''' </param>
        Private Sub AddEventHandlers(rc As PersistentConnection)
            AddHandler rc.SessionVariableChanged, AddressOf HandleSessionVarChanged
            AddHandler rc.SessionEnd, AddressOf HandleSessionEnd
            AddHandler rc.SessionCreate, AddressOf HandleSessionCreate
            AddHandler rc.SessionDelete, AddressOf HandleSessionDelete
            AddHandler rc.SessionStart, AddressOf HandleSessionStart
        End Sub

        ''' <summary>
        ''' Removes the event handlers maintained by this manager from the given resource
        ''' connection
        ''' </summary>
        ''' <param name="rc">The resource connection to remove the event handlers from.
        ''' </param>
        Private Sub RemoveEventHandlers(rc As PersistentConnection)
            RemoveHandler rc.SessionVariableChanged, AddressOf HandleSessionVarChanged
            RemoveHandler rc.SessionEnd, AddressOf HandleSessionEnd
            RemoveHandler rc.SessionCreate, AddressOf HandleSessionCreate
            RemoveHandler rc.SessionDelete, AddressOf HandleSessionDelete
            RemoveHandler rc.SessionStart, AddressOf HandleSessionStart
        End Sub

        ''' <summary>
        ''' Get the next waiting user message. The act of getting the
        ''' message removes it from the queue.
        ''' </summary>
        ''' <returns>The next waiting message, or an empty string if
        ''' there isn't one.</returns>
        Private Function GetNextUserMessage() As String
            Dim c As PersistentConnection
            Dim sMsg As String
            For Each c In mResourceConnections.Values
                sMsg = c.GetNextUserMessage()
                If sMsg <> "" Then Return sMsg
            Next
            Return ""
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
        Private Function IResourceConnectionManager_TryGetNextUserMessage(ByRef msg As String) As Boolean Implements IResourceConnectionManager.TryGetNextUserMessage
            msg = GetNextUserMessage()
            Return (msg <> "")
        End Function

        Public Sub RefreshResourceConnection(resourceId As Guid) Implements IResourceConnectionManager.RefreshResourceConnection
            'Other resource managers use this method but not this one.
        End Sub

        Public Function CheckInstructionalClientStatus() As Boolean Implements IResourceConnectionManager.CheckInstructionalClientStatus
            Return True
        End Function

        'This method is not to be used when using this connection mode as we do not wish to report back stats on these connections.
        Public Function GetAllResourceConnectionStatistics() As IDictionary(Of Guid, ResourceConnectionStatistic) Implements IUserAuthResourceConnectionManager.GetAllResourceConnectionStatistics
            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return My.Resources.PersistentConnectionManagerToString
        End Function


        Public Sub ToggleDisableConnection()
            mDisableConnections = Not mDisableConnections
            Try
                For Each r As PersistentConnection In mResourceConnections.Values
                    If mDisableConnections Then r.Terminate()
                    r.ResourceConnection(mDisableConnections)
                Next
            Catch ex As Exception
                Log.Warn(ex, "Error loading resource info from database")
                Return 'Can't update when we cannot get resources
            End Try
        End Sub

#End Region

    End Class

End Namespace
