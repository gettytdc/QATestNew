Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Core.Extensions.ResourceAttributeExtensions
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models

''' <summary>
''' This class is used internally to represent a connection to a Resource PC.
''' 
''' It maintains a clsTalker object to handle communication, a queue of commands
''' to be sent asynchronously to the Resource PC, and a queue of user messages
''' relating to the Resource PC, that should be displayed to the Control Room
''' operator.
''' 
''' Information from the multiple instances of this connection object is
''' aggregated by the owning ctlResourceView, and presented to, for example,
''' ctlControlRoom, in a combined view.
''' 
''' The class maintains a continuously looping thread which handles all
''' processing. It will automatically try to open and keep open a connection to
''' its Resource PC.
''' </summary>
'''
Namespace Resources
    Public Class PersistentConnection
        Inherits ResourceConnectionBase
        Implements IResourceConnection


        Public ReadOnly Property IsAvailable(connectedOnly As Boolean) As Boolean
            Get

                ' We don't return 'unavailable' resources (eg. local on remote machines)
                If IsUnavailable Then Return False

                ' If it's not connected and we only want connected machines, skip it too
                If connectedOnly AndAlso Not IsConnected Then Return False
                If ConnectionState <> ResourceConnectionState.Connected Then Return False
                If ProcessesRunning > 0 Then Return False
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Delay (number of connection attempts) before another entry added to event log.
        ''' </summary>
        Private mEventLogDelay As Integer


        ''' <summary>
        ''' The clsResourceConnectionManager that owns this object.
        ''' </summary>
        Private mManager As PersistentConnectionManager

        ''' <summary>
        ''' The thread that handles communcation with the resource PC.
        ''' </summary>
        Private mCommsThread As Thread

        Private mcolSendQueue As Collection

        Private mMessageQueue As Collection
        ''' <summary>
        ''' The size in bytes which the resource comms thread will reserve for stack space. A value of 0 mean use the default.
        ''' See us-3126 for the reason for this.
        ''' </summary>
        Private mThreadStackSize As Integer

        Private mPingTimeoutSeconds As Integer

        ''' <summary>
        ''' Set to true when this connection should terminate. Causes the communication
        ''' thread to exit.
        ''' </summary>
        Private mTerminate As Boolean

        'Constructor...
        Public Sub New(gID As Guid, sName As String, dbStatus As ResourceMachine.ResourceDBStatus,
                        attrs As ResourceAttribute, userID As Guid, parent As PersistentConnectionManager,
                       threadStackSize As Integer, pingTimeoutSeconds As Integer, disableConnections As Boolean)

            mManager = parent
            mResourceMachine = New ResourceMachine(Me, dbStatus, sName, gID, attrs)
            mThreadStackSize = threadStackSize
            NextConnectTime = Date.UtcNow
            mPingTimeoutSeconds = pingTimeoutSeconds
            mTerminate = False

            UpdateConnectionState(disableConnections)

            mcolSendQueue = New Collection()
            mMessageQueue = New Collection()
            mTalker = New clsTalker(mPingTimeoutSeconds)

            'check and 
            CheckAndReviveConnection(disableConnections)
        End Sub
        Public Sub ResourceConnection(disableConnections As Boolean)

            UpdateConnectionState(disableConnections)
            CheckAndReviveConnection(disableConnections)
        End Sub

        Private Sub UpdateConnectionState(disableConnections As Boolean)
            If MyBase.mResourceMachine.DBStatus = Global.BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus.Ready AndAlso Not mResourceMachine.Attributes.IsRetired() Then

                ' If the connection is to a machine running in local mode, and that
                ' machine is not the machine running this connection manager, it's
                ' unavailable;
                ' Likewise, if the user doesn't have permissions to access to the
                ' machine on this connection, it's unavailable.
                Dim machine = AutomateAppCore.Resources.ResourceMachine.GetName()
                If (MyBase.mResourceMachine.Local AndAlso
                 mResourceMachine.Name <> machine AndAlso
                 Not mResourceMachine.Name.StartsWith(machine & ":")) Then
                    ConnectionState = ResourceConnectionState.Unavailable
                Else
                    If disableConnections Then
                        ConnectionState = ResourceConnectionState.Hidden
                    Else
                        ConnectionState = ResourceConnectionState.Connecting
                    End If
                End If

                If Not ShouldReviveConnection(MyBase.mResourceMachine, mManager) Then
                    ConnectionState = ResourceConnectionState.InUse
                End If
            Else
                ConnectionState = ResourceConnectionState.Offline
            End If
        End Sub

        Private Sub CheckAndReviveConnection(disableConnections As Boolean)
            If ShouldReviveConnection(mResourceMachine, mManager) AndAlso Not disableConnections Then
                ReviveConnection()
            End If
        End Sub

        Private Function ShouldReviveConnection(resourceMachine As IResourceMachine, connectionManager As PersistentConnectionManager) As Boolean
            Return Not resourceMachine.Attributes.IsRetired() AndAlso (Not resourceMachine.Attributes.IsPrivate() OrElse resourceMachine.UserID = Guid.Empty OrElse connectionManager.ConnectionUser.Id = resourceMachine.UserID)
        End Function

        Public ReadOnly Property HasUserMessage As Boolean
            Get
                Return mMessageQueue.Count > 0
            End Get
        End Property

        ''' <summary>
        ''' Renews the connection if this connection was terminated or has not
        ''' yet been started (eg in the case of the resource being retired when
        ''' constructor was called; otherwise does nothing.
        ''' </summary>
        Public Sub ReviveConnection()
            SyncLock Me

                If (Not mCommsThread Is Nothing) Then
                    If mCommsThread.IsAlive Then
                        'healthy thread - do nothing
                        Exit Sub
                    Else
                        mCommsThread.Abort()
                    End If
                End If

                mTerminate = False

                If mThreadStackSize = 0 Then
                    mCommsThread = New Thread(AddressOf CommsThread)
                Else
                    mCommsThread = New Thread(AddressOf CommsThread, mThreadStackSize)
                End If

                mCommsThread.Start()

            End SyncLock
        End Sub

        ''' <summary>
        ''' Adds a send queue entry to the send queue
        ''' </summary>
        ''' <param name="entry">The entry to add to the queue</param>
        ''' <exception cref="NotConnectedException">If this resource connection is not in
        ''' a <see cref="ResourceConnectionState.Connected">connected</see> state.
        ''' </exception>
        Protected Overrides Sub AddToSendQueue(entry As SendQueueEntry)
            EnsureConnected()
            SyncLock mcolSendQueue
                mcolSendQueue.Add(entry)
            End SyncLock
            mWaitHandle.Set()
        End Sub

        Protected Overrides Sub QueueMsgQueueEntry(queueEntry As MsgQueueEntry)
            Monitor.Enter(mMessageQueue)
            Try
                mMessageQueue.Add(queueEntry)
                'We will report a new user message as a status change,
                'to ensure that Control Room picks it up in a timely
                'fashion...
                StatusChange = StatusChange Or ResourceStatusChange.UserMessageWaiting
            Finally
                Monitor.Exit(mMessageQueue)
            End Try
        End Sub

        ''' <summary>
        ''' Gets the next user message waiting on this connection.
        ''' </summary>
        ''' <returns>The message, or an empty string if there isn't one.</returns>
        Public Function GetNextUserMessage() As String
            SyncLock (mMessageQueue)
                If mMessageQueue.Count = 0 Then
                    StatusChange = StatusChange And (Not ResourceStatusChange.UserMessageWaiting)
                    Return ""
                End If
                Dim qi As MsgQueueEntry = CType(mMessageQueue(1), MsgQueueEntry)
                mMessageQueue.Remove(1)
                If mMessageQueue.Count = 0 Then StatusChange = StatusChange And (Not ResourceStatusChange.UserMessageWaiting)
                Return qi.MsMessage
            End SyncLock
        End Function


        ''' <summary>
        ''' Check if the thread associated with this connection has terminated.
        ''' </summary>
        Public ReadOnly Property Terminated() As Boolean
            Get
                Return (mCommsThread Is Nothing OrElse Not mCommsThread.IsAlive)
            End Get
        End Property


        Public Overrides Sub Terminate() Implements IResourceConnection.Terminate
            mTerminate = True
        End Sub

        ''' <summary>
        ''' Awaits a connection becoming available on this connection, blocking the
        ''' current thread until one enters a 'valid' state.
        ''' This treats a connection in a state of
        ''' <see cref="ResourceConnectionState.InUse"/> as a valid connection if
        ''' <paramref name="inuseIsValid"/> is true; otherwise, it will be treated as
        ''' invalid.
        ''' This will wait for a time of <paramref name="timeoutMillis"/> milliseconds 
        ''' for a connection to enter a final state. If the timeout expires before
        ''' that occurs, this will return False indicating the connection is not valid
        ''' at the time of returning.
        ''' </summary>
        ''' <param name="timeoutMillis">The timeout to wait for a connection to become
        ''' valid if it is still connecting, or <see cref="Timeout.Infinite"/> to wait
        ''' indefinitely</param>
        ''' <param name="inuseIsValid">True to treat
        ''' <see cref="ResourceConnectionState.InUse"/> as a valid connection state,
        ''' causing this method to return true if the state is entered; False to treat it
        ''' as an invalid state, causing this method to return false if the state is
        ''' entered.</param>
        ''' <param name="state">The last state found before this method returned. This
        ''' will normally be a more fine-grained state than 'valid' or 'not valid' so
        ''' that calling methods can handle the states in a more specific way.</param>
        ''' <returns>True if the underlying connection was valid ie. 
        ''' <see cref="ResourceConnectionState.Connected"/>; False if it was invalid -
        ''' ie. the thread is not alive or the connection state is
        ''' <see cref="ResourceConnectionState.[Error]"/>,
        ''' <see cref="ResourceConnectionState.Offline"/>, 
        ''' <see cref="ResourceConnectionState.Unavailable"/> or 
        ''' <see cref="ResourceConnectionState.InUse"/> -or- if the timeout is reached
        ''' before the connection state reaches a final state (ie. it is still 
        ''' <see cref="ResourceConnectionState.Connecting"/> when the timeout expires).
        ''' </returns>
        Public Overrides Function AwaitValidConnection(
         ByVal timeoutMillis As Integer,
         ByVal inuseIsValid As Boolean,
         ByRef state As ResourceConnectionState) As Boolean Implements IResourceConnection.AwaitValidConnection

            Dim timedOut As Boolean = False ' Timeout flag

            ' Loop while the thread is still active - check the state on
            ' each iteration to see if we've reached a 'final' status, either
            ' valid (Connected & possibly InUse) or invalid (Error, Offline,
            ' Unavailable & possibly InUse). If we're in a transition state
            ' (ie. Connecting) wait for at least the timeout period and then
            ' return 'Invalid' to indicate that no valid state was reached.
            While mCommsThread IsNot Nothing AndAlso mCommsThread.IsAlive

                ' Set the state ready to return
                state = ConnectionState

                Select Case ConnectionState

                    Case ResourceConnectionState.Connected
                        Log.Debug("{0} - Awaiting valid connection successful - Connected", mResourceMachine.Name)
                        Return True

                    Case ResourceConnectionState.Error,
                     ResourceConnectionState.Offline,
                     ResourceConnectionState.Unavailable
                        Log.Debug("{0} - Awaiting valid connection failed - {1}", mResourceMachine.Name, ConnectionState)
                        Return False

                    Case ResourceConnectionState.InUse
                        Log.Debug("{0} - Awaiting valid connection - In Use Valid ({1})", mResourceMachine.Name, inuseIsValid)
                        Return inuseIsValid

                    Case ResourceConnectionState.Connecting
                        ' We timed out on the last iteration through the loop and
                        ' no valid connection has been reached, thus return invalid
                        Log.Debug("{0} - Awaiting valid connection - Connecting", mResourceMachine.Name)

                        If timedOut Then
                            Log.Debug("{0} - Connecting timed out", mResourceMachine.Name)
                            Return False
                        End If

                        Dim startTime As Date = Date.Now

                        ' Wait for the timeout number of millis.
                        timedOut = Not mManager.RateLimiter.WaitOne(timeoutMillis, False)
                        Log.Debug("{0} - Awaiting update - timed out: {1}", mResourceMachine.Name, timedOut)

                        ' Instantly send another signal to the rate limiter to
                        ' ensure that the comms thread isn't held up by our
                        ' interference
                        Log.Debug("Signaling rate limiter")
                        mManager.RateLimiter.Set()

                        ' Effect a 'yield' to allow the comms thread to continue
                        Thread.Sleep(1)

                        ' If we've not timed out, and we do have a timeout value,
                        ' reduce it by the amount of time we've waited ready for
                        ' the next iteration.
                        If Not timedOut AndAlso timeoutMillis >= 0 Then
                            Dim millis As Integer = CInt((Date.Now - startTime).TotalMilliseconds)
                            timeoutMillis -= millis
                            If timeoutMillis <= 0 Then timedOut = True
                            Log.Debug("{0} - Remaining update time {1}ms", mResourceMachine.Name, timeoutMillis)

                        End If

                        ' After the comms thread has done its ting, loop round again
                        ' and re-check the status

                End Select
            End While
            Log.Debug("{0} - Awaiting update - comms check inactive", mResourceMachine.Name)

            ' Thread is either null or 'not alive'
            Return False

        End Function

        Private ReadOnly mWaitHandle As New AutoResetEvent(False)

        ''' <summary>
        ''' The communications thread - created when this object is created. Continues
        ''' running until mbTerminate is set to True.
        ''' </summary>
        Private Sub CommsThread()
            Try
                Do
                    Log.Debug("{0} - Beginning update. Current state: {1}", mResourceMachine.Name, ConnectionState)
                    If Not ShouldReviveConnection(mResourceMachine, mManager) Then
                        ConnectionState = ResourceConnectionState.InUse
                    Else
                        ProcessResourceByConnectionState()
                    End If

                    SkipIfRecentlyCommunicated = True
                    If Not mTerminate Then mWaitHandle.WaitOne(1000)
                Loop Until mTerminate
                Log.Debug("{0} - Exiting connection monitoring loop", mResourceMachine.Name)
            Catch ex As Exception
                Log.Warn(ex, "{0} - Error during connection monitoring", mResourceMachine.Name)
            Finally
                If ConnectionState = ResourceConnectionState.Offline OrElse ConnectionState = ResourceConnectionState.Connecting Then
                    StatusChange = StatusChange Or ResourceStatusChange.OfflineChange
                Else
                    StatusChange = StatusChange Or ResourceStatusChange.OnlineChange
                End If
                ConnectionState = ResourceConnectionState.Offline
            End Try
            Log.Debug("{0} - Closing connection", mResourceMachine.Name)
            mTalker.Close()
        End Sub

        Private Sub ProcessResourceByConnectionState()
            Dim sErr As String = Nothing
            Select Case ConnectionState
                Case ResourceConnectionState.Connecting
                    Log.Debug("{0} - Waiting for rate limiter", mResourceMachine.Name)
                    'Wait if necessary to prevent too many outgoing connections at once.
                    mManager.RateLimiter.WaitOne()
                    If mTerminate Then Return
                    'Decide what machine name to connect to. Normally it's tbe Resource
                    'name, but if the Resource is a pool then we need to connect to the
                    'controller.
                    Dim name As String = Nothing
                    If (mResourceMachine.Attributes And ResourceAttribute.Pool) = 0 Then
                        name = mResourceMachine.Name
                        Log.Debug("{0} - Connecting directly to resource", name)
                    Else
                        Try
                            gSv.GetPoolControllerName(mResourceMachine.Id, name)
                            Log.Debug("{0} - Connecting to pool controller", name)
                        Catch ex As Exception
                            Log.Debug("{0} - Unable to get pool controller, setting offline and delaying connection for 20 cycles", name)
                            ConnectionState = ResourceConnectionState.Offline
                            ConnectLater(ConnectIntervals.ControllerNotAvailable)
                            StatusChange = StatusChange Or ResourceStatusChange.OfflineChange
                        End Try
                    End If

                    If name IsNot Nothing Then
                        If MyBase.mResourceMachine.DBStatus = Global.BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus.Ready _
                                        AndAlso (
                                            Not MyBase.mResourceMachine.Local _
                                            OrElse MyBase.mResourceMachine.Name = AutomateAppCore.Resources.ResourceMachine.GetName()
                                            ) Then
                            'Let the talker know that the resource will attempt to
                            'reconnect if the connection fails
                            MyBase.mTalker.WillAttemptReconnectOnFail = True
                            Log.Debug("{0} - Will attempt reconnect on fail", name)
                        End If

                        Log.Debug("{0} - Attempting connection", name)
                        If mTalker.Connect(name, sErr) Then
                            Log.Debug("{0} - Connected", name)
                            'Authenticate, if there is a user logged in. When used from a Resource
                            'PC, for example, there is no logged in user!
                            If mManager.UserId <> Nothing AndAlso mTalker.Authenticate() Then
                                Log.Debug("{0} - Authenticated", name)
                                'If we're a controller connecting to a pool
                                'member, we need to tell it so it doesn't
                                'try and proxy stuff back to us!
                                If mManager.Mode = IResourceConnectionManager.Modes.Controller AndAlso
                                   Not mTalker.Say("controller " & mManager.PoolId.ToString(), "OK") Then
                                    SetConnectionError(name, mTalker.GetReply)
                                End If
                                If mManager.QueryCapabilities AndAlso
                                    Not mTalker.GetCaps(mResourceMachine.CapabilitiesFriendly) Then
                                    Log.Warn("{0} - Error getting capabilities, setting to error state and delaying connection for 50 cycles", name)
                                    ConnectionState = ResourceConnectionState.Error
                                    ConnectLater(ConnectIntervals.CapabilitiesError)
                                Else
                                    If mManager.MonitorSessionVariables Then
                                        Log.Debug("{0} - Monitoring session variables", name)
                                        mTalker.Say("varupdates on", "SET", 10000)
                                    End If
                                    ConnectionState = ResourceConnectionState.Connected
                                    Log.Debug("{0} - Connected", name)
                                End If
                            Else
                                mTalker.Close()
                                Log.Debug("{0} - Resource in use, delaying connection", name)
                                ConnectionState = ResourceConnectionState.InUse
                                ConnectLater(ConnectIntervals.InUse)
                            End If
                            mResourceMachine.LastError = ""
                            mEventLogDelay = 0
                            StatusChange = StatusChange Or ResourceStatusChange.OnlineChange

                        Else
                            SetConnectionError(name, sErr)
                        End If
                    End If

                Case ResourceConnectionState.Error
                    'If the resource says error, but the database says it should
                    'be online, wait a while then try and connect again.
                    If MyBase.mResourceMachine.DBStatus = Global.BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus.Ready Then
                        Log.Debug("{0} - DB indicates resource ready", MyBase.mResourceMachine.Name)
                        TryConnectingIfReady(ResourceStatusChange.OfflineChange)
                    End If

                Case ResourceConnectionState.InUse
                    TryConnectingIfReady(ResourceStatusChange.OnlineChange)

                Case ResourceConnectionState.Connected
                    If MyBase.mResourceMachine.DBStatus = Global.BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus.Offline Then
                        ' Database says it is offline, so switch to offline status
                        ConnectionState = ResourceConnectionState.Offline
                        ConnectLater(ConnectIntervals.DBStatusOffline)
                        StatusChange = StatusChange Or ResourceStatusChange.OnlineChange
                    ElseIf Not MyBase.mTalker.CheckConnection(SkipIfRecentlyCommunicated) Then
                        Log.Trace("Communication with resource timed out {resource}", New With {ResourceId})
                        ConnectionState = ResourceConnectionState.Offline
                        ConnectLater(ConnectIntervals.ConnectionCheckFailed)
                        StatusChange = StatusChange Or ResourceStatusChange.OnlineChange

                        ' The program thinks the connection is closed, let's make sure.
                        MyBase.mTalker.Close()

                    ElseIf MyBase.mTalker IsNot Nothing AndAlso MyBase.mResourceMachine.HasAttribute(ResourceAttribute.Pool) AndAlso
                                Not MyBase.mTalker?.IsResourcePCStillController(MyBase.mResourceMachine) Then
                        TryConnectingIfReady(ResourceStatusChange.OnlineChange)
                    Else
                        'Process any input...
                        MyBase.mTalker.ProcessInput()
                        If MyBase.mTalker.CheckStatusChanged() Then
                            Log.Debug("{0} - Resource PC status changed", MyBase.mResourceMachine.Name)
                            StatusChange = StatusChange Or ResourceStatusChange.EnvironmentChange
                        End If
                        ProcessesRunning = MyBase.mTalker.ProcessesRunning
                        ProcessesPending = MyBase.mTalker.ProcessesPending

                        'Send any commands from the queue...
                        Dim qi As SendQueueEntry
                        Do

                            'Grab the next entry from the
                            'queue, if there is one. Remove
                            'it at the same time.
                            Monitor.Enter(mcolSendQueue)
                            Try
                                If mcolSendQueue.Count > 0 Then
                                    qi = CType(mcolSendQueue.Item(1), SendQueueEntry)
                                    mcolSendQueue.Remove(1)
                                Else
                                    qi = Nothing
                                End If
                            Finally
                                Monitor.Exit(mcolSendQueue)
                            End Try
                            If qi Is Nothing Then Exit Do

                            Dim controlPermissionValidation = gSv.GetControllingUserPermissionSetting()
                            LogQueueMessage(qi)
                            'Process the entry we just removed
                            'from the queue.
                            Select Case qi.EntryType
                                Case SendQueueType.StartSession
                                    If MyBase.mTalker.Say("startp " & qi.InputXml, "PARAMETERS SET") Then

                                        Dim startCmd As String
                                        If controlPermissionValidation Then
                                            Dim token = gSv.RegisterAuthorisationToken(qi.ProcessId)
                                            startCmd = $"startas {token} {qi.SessionId}"
                                        Else
                                            startCmd = $"start {qi.SessionId}"
                                        End If

                                        If MyBase.mTalker.Say(startCmd, "STARTED") Then
                                            MyBase.OnSessionStart(New SessionStartEventArgs(sessid:= qi.SessionId))
                                        Else
                                            Dim reply As String = MyBase.mTalker.GetReply()
                                            Dim errmsg = If(reply = "",
                                                My.Resources.clsResourceConnection_NoReplyFromResourcePC,
                                                String.Format(My.Resources.clsResourceConnection_ReplyWas0, reply))
                                            QueueUserMessage(String.Format(My.Resources.clsResourceConnection_FailedToStartSessionOn01, MyBase.mResourceMachine.Name, errmsg), 4116)
                                            MyBase.OnSessionStart(New SessionStartEventArgs(sessId:= qi.SessionId, errmsg:= errmsg))
                                        End If
                                    Else
                                        Dim reply As String = MyBase.mTalker.GetReply()
                                        Dim errmsg = If(reply = "",
                                            My.Resources.clsResourceConnection_NoReplyFromResourcePC,
                                            String.Format(My.Resources.clsResourceConnection_ReplyWas0, reply))
                                        QueueUserMessage(String.Format(My.Resources.clsResourceConnection_FailedToSetStartParametersOn01, MyBase.mResourceMachine.Name, errmsg), 4126)
                                        MyBase.OnSessionStart(New SessionStartEventArgs(sessid:= qi.SessionId, errmsg:= errmsg))
                                    End If

                                Case SendQueueType.VarUpdates
                                    MyBase.mTalker.Say("varupdates " & CStr(IIf(qi.Flag, "on", "off")), "SET", 10000)

                                Case SendQueueType.CreateSession
                                    Dim createCmd As String
                                    If controlPermissionValidation Then
                                        Dim token = gSv.RegisterAuthorisationToken(qi.ProcessId)
                                        createCmd = $"createas {token} {qi.ProcessId} {qi.QueueIdent} {qi.SessionId}"
                                    Else
                                        createCmd = $"create {qi.ProcessId} {qi.QueueIdent} {qi.SessionId}"
                                    End If

                                    If MyBase.mTalker.Say(createCmd, "SESSION CREATED") Then

                                        Dim newId As Guid = New Guid(
                                             MyBase.mTalker.GetReply().Substring("SESSION CREATED : ".Length))

                                        ' Fire a successful session created event
                                        MyBase.OnSessionCreate(New SessionCreateEventArgs(
                                                               resourceId:=ResourceId, processId:=qi.ProcessId, sessionId:=qi.SessionId, schedSessId:=qi.ScheduledSessionId) With {.Tag = qi.Tag})
                                    Else
                                        Dim reply As String = MyBase.mTalker.GetReply()
                                        Dim userMessage As String
                                        Dim state As SessionCreateState
                                        Dim data As IDictionary(Of Guid, RunnerStatus) = Nothing
                                        Select Case reply
                                            Case "UNAVAILABLE"
                                                userMessage = String.Format(My.Resources.clsResourceConnection_0IsTooBusyToRunThatProcess, MyBase.mResourceMachine.Name)
                                                state = SessionCreateState.CreateFailedTooBusy
                                                ' Try and get some extra diagnostics - ie
                                                ' indicate the current state of the resource
                                                Try
                                                    data = MyBase.mTalker.GetSessions()

                                                Catch ex As Exception
                                                    ' Just ignore it - it's diag data only,
                                                    ' and the important error has been 
                                                    ' caught already.
                                                End Try

                                            Case Nothing, String.Empty
                                                userMessage = String.Format(My.Resources.clsResourceConnection_FailedToCreateSessionOn0NoReplyFromResourcePC, MyBase.mResourceMachine.Name)
                                                state = SessionCreateState.CreateFailedNoError
                                            Case Else
                                                userMessage = String.Format(My.Resources.clsResourceConnection_FailedToCreateSessionOn01, MyBase.mResourceMachine.Name, MyBase.mTalker.GetReply())
                                                state = SessionCreateState.CreateFailedGenericError
                                        End Select
                                        QueueUserMessage(userMessage)
                                        ' Fire the event with a different state and error msg
                                        ' indicating that the session could not be created
                                        MyBase.OnSessionCreate(New SessionCreateEventArgs(
                                                                createState:=state,
                                                                resourceId:=MyBase.mResourceMachine.Id,
                                                                processId:=qi.ProcessId,
                                                                sessionId:=qi.SessionId,
                                                                schedSessId:=qi.ScheduledSessionId,
                                                                errMsg:=userMessage,
                                                                data:=data) With {.Tag = qi.Tag})
                                    End If

                                Case SendQueueType.StopSession
                                    Dim serverName = Environment.MachineName
                                    Try
                                        serverName = Net.Dns.GetHostEntry("").HostName
                                    Catch
                                    End Try
                                    If Not MyBase.mTalker.Say("stop " & qi.SessionId.ToString & " " & mManager.UserId.ToString & " name " & serverName, "STOPPING") Then
                                        Dim sReply As String = MyBase.mTalker.GetReply()
                                        Dim suffix As String
                                        Select Case sReply
                                            Case Nothing, String.Empty
                                                suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                                            Case Else
                                                suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                                        End Select
                                        QueueUserMessage(String.Format(My.Resources.clsResourceConnection_FailedToStopSessionOn01, MyBase.mResourceMachine.Name, suffix))
                                    End If

                                Case SendQueueType.DeleteSession

                                    Dim deleteCmd As String
                                    If controlPermissionValidation Then
                                        Dim token = gSv.RegisterAuthorisationToken(qi.ProcessId)
                                        deleteCmd = $"deleteas {token} {qi.SessionId}"
                                    Else
                                        deleteCmd = $"delete {qi.SessionId}"
                                    End If

                                    If MyBase.mTalker.Say(deleteCmd, "SESSION DELETED") Then
                                        ' Fire a successful session delete event
                                        MyBase.OnSessionDelete(New SessionDeleteEventArgs(qi.SessionId))
                                    Else
                                        Dim sReply As String = MyBase.mTalker.GetReply()
                                        Dim Suffix As String
                                        Select Case sReply
                                            Case Nothing, String.Empty
                                                Suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                                            Case Else
                                                Suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                                        End Select
                                        QueueUserMessage(String.Format(My.Resources.clsResourceConnection_FailedToDeleteSessionOn01, MyBase.mResourceMachine.Name, Suffix))
                                    End If

                                Case SendQueueType.SetSessionVariable
                                    Dim sessionVars As New List(Of String)
                                    For Each var In qi.SessionVariables
                                        sessionVars.Add(var.ToEscapedString(False))
                                    Next
                                    If Not MyBase.mTalker.Say("setvar " & qi.SessionVariables.First.SessionID.ToString & " " & String.Join(",", sessionVars), "SET") Then
                                        Dim sReply As String = MyBase.mTalker.GetReply()
                                        Dim Suffix As String
                                        Select Case sReply
                                            Case Nothing, String.Empty
                                                Suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                                            Case Else
                                                Suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                                        End Select
                                        QueueUserMessage(String.Format(My.Resources.clsResourceConnection_FailedToSetSessionVariableOn01, MyBase.mResourceMachine.Name, Suffix))
                                    End If
                            End Select
                        Loop
                        mcolSendQueue = New Collection

                    End If
                Case ResourceConnectionState.Offline
                    'If the resource is offline, but the database
                    'says it should be online, wait a while
                    'then try and connect again.
                    If MyBase.mResourceMachine.DBStatus = Global.BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus.Ready AndAlso
                                (Not MyBase.mResourceMachine.Local OrElse MyBase.mResourceMachine.Name = AutomateAppCore.Resources.ResourceMachine.GetName()) Then
                        Log.Debug("{0} - DB indicates resource ready", MyBase.mResourceMachine.Name)
                        TryConnectingIfReady(ResourceStatusChange.None)
                    End If

                Case ResourceConnectionState.Unavailable
                    'Currently the only reason we can be unavailable is if the
                    'resource machine is local and we're elsewhere, so if the
                    'resource machine is no longer local, we can try and connect.
                    If mResourceMachine.Local Then
                        Log.Debug("{0} - Resource machine local - ready to reconnect", mResourceMachine.Name)
                        ConnectionState = ResourceConnectionState.Connecting
                        StatusChange = StatusChange Or ResourceStatusChange.OnlineChange
                    End If
            End Select
        End Sub

        ''' <summary>
        ''' Switch to Connecting state if it is time to connect
        ''' </summary>
        Private Sub TryConnectingIfReady(status As ResourceStatusChange)
            If NextConnectTime <= Date.UtcNow Then
                ConnectionState = ResourceConnectionState.Connecting
                StatusChange = StatusChange Or status
            End If
        End Sub

        Public Overrides Function RefreshResource() As Task(Of Boolean)
            Return Task.FromResult(False)
        End Function

    End Class

End Namespace
