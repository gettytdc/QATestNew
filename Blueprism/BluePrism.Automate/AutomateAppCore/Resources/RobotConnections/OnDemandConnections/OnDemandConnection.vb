Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Core.Extensions
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models

Namespace Resources
    Public Class OnDemandConnection
        Inherits ResourceConnectionBase
        Implements IResourceConnection

        ''' <summary>
        ''' Event fires when a signal resource operation completes 
        ''' </summary>
        Public Event SignalResourceComplete As EventHandler

        ''' <summary>
        ''' Event raided when an error occures.
        ''' </summary>
        Public Event Errors As EventHandler(Of ErrorEventArgs)

        ''' <summary>
        ''' resource connected 
        ''' </summary>
        Public Event Connected As EventHandler

        ''' <summary>
        ''' resource disconnected
        ''' </summary>
        Public Event Disconnected As EventHandler

        Public Event PingCompleted As EventHandler

        Public Event PingFailed As EventHandler

        Public Event Statistic As EventHandler(Of NewConnectionStatisticEventArgs)

        Public Event RetryRequired As EventHandler(Of SignalResourceConnectionRetryRequiredEventArgs)

        Public ReadOnly Property IsSleeping As Boolean
            Get
                Return (ConnectionState = ResourceConnectionState.Sleep)
            End Get
        End Property

        ''' <summary>
        ''' Gets the resource ID for the resource represented by this connection, or
        ''' <see cref="Guid.Empty"/> if this connection has no resource machine
        ''' associated with it (which really shouldn't happen).
        ''' </summary>




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
        Private ReadOnly mManager As IResourceConnectionManager

        ''' <summary>
        ''' Stops multiple connections to same resource.
        ''' </summary>
        Private mThreadActive As Integer = 0


        Private ReadOnly mSendEntryQueue As Queue(Of SendQueueEntry)

        Private ReadOnly mPingTimeoutSeconds As Integer

        Private ReadOnly mResourceAttribute As ResourceAttribute
        Private mResourceUserId As Guid

        Private ReadOnly Property ConnectionStatusDeterminedEvent As AutoResetEvent

        Private Const ConnectionRetryCount As Integer = 3
        Private Const ConnectionRetryTimeout As Integer = 5000
        Private ReadOnly mPingResourceForInSeconds As Integer
        Private ReadOnly mProcessResourceInputSleepTime As Integer

        Public Sub New(gID As Guid, sName As String, dbStatus As ResourceMachine.ResourceDBStatus,
                        attrs As ResourceAttribute, userID As Guid, parent As IResourceConnectionManager,
                       threadStackSize As Integer, pingTimeoutSeconds As Integer, pingResourceForInSeconds As Integer, processResourceInputSleepTime As Integer)

            Log.Debug("Creating OnDemandConnection {resource}", New With {Key .ResourceId = gID, Key .name = sName, Key .status = dbStatus, Key .Attributes = attrs})

            mManager = parent
            mResourceMachine = New ResourceMachine(Me, dbStatus, sName, gID, attrs)
            NextConnectTime = Date.UtcNow
            mPingTimeoutSeconds = pingTimeoutSeconds
            ConnectionStatusDeterminedEvent = New AutoResetEvent(False)
            mPingResourceForInSeconds = pingResourceForInSeconds
            mProcessResourceInputSleepTime = processResourceInputSleepTime

            If MyBase.mResourceMachine.DBStatus = Global.BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus.Ready _
             AndAlso (attrs And Global.BluePrism.Core.Resources.ResourceAttribute.Retired) = 0 Then

                ' If the connection is to a machine running in local mode, and that
                ' machine is not the machine running this connection manager, it's
                ' unavailable;
                ' Likewise, if the user doesn't have permissions to access to the
                ' machine on this connection, it's unavailable.
                Dim machine = AutomateAppCore.Resources.ResourceMachine.GetName()
                If (MyBase.mResourceMachine.Local AndAlso
                 sName <> machine AndAlso
                 Not sName.StartsWith(machine & ":")) Then
                    ConnectionState = ResourceConnectionState.Unavailable
                Else
                    ConnectionState = ResourceConnectionState.Connecting
                End If
            Else
                ConnectionState = ResourceConnectionState.Offline
            End If

            mSendEntryQueue = New Queue(Of SendQueueEntry)()
            mTalker = Nothing
            mResourceAttribute = attrs
        End Sub



        ''' <summary>
        ''' RefreshResource, instruct the resource to update.
        ''' </summary>
        Public Overrides Function RefreshResource() As Task(Of Boolean) Implements IResourceConnection.RefreshResource
            Return SignalResourceConnection()
        End Function


        Public Overrides Function AwaitValidConnection(timeoutMillis As Integer, inuseIsValid As Boolean, ByRef state As ResourceConnectionState) As Boolean Implements IResourceConnection.AwaitValidConnection

            Dim resourceConnected = False
            Dim t As Task = SignalResourceConnection()

            If Not t.Wait(timeoutMillis) Then
                Log.Trace($"RESOURCE {ResourceName} not connected")
                Return False
            Else
                Log.Trace($"RESOURCE {ResourceName} {If(resourceConnected, "", "not ")} connected")
                Return resourceConnected
            End If

        End Function

        ''' <summary>
        ''' Adds a send queue entry to the send queue
        ''' </summary>
        ''' <param name="entry">The entry to add to the queue</param>
        ''' <exception cref="NotConnectedException">If this resource connection is not in
        ''' a <see cref="ResourceConnectionState.Connected">connected</see> state.
        ''' </exception>
        Protected Overrides Sub AddToSendQueue(entry As SendQueueEntry)
            'Something meaninful is about to happen, so reset the disonnection.
            SyncLock mSendEntryQueue
                mSendEntryQueue.Enqueue(entry)
            End SyncLock
        End Sub

        Private Sub ClearSendQueue()
            Log.Debug("Clearing Send Queue. {connection}", New With {ResourceName})
            SyncLock mSendEntryQueue
                mSendEntryQueue.Clear()
            End SyncLock
        End Sub

        Protected Overrides Sub QueueMsgQueueEntry(queueEntry As MsgQueueEntry)
            ' Event based user-messages are used in ASCR
        End Sub

        ''' <summary>
        ''' Gets the next user message waiting on this connection.
        ''' </summary>
        ''' <returns>The message, or an empty string if there isn't one.</returns>
        Public Function GetNextUserMessage() As String
            Return String.Empty
        End Function

        ''' <summary>
        ''' Check if the thread associated with this connection has terminated.
        ''' </summary>
        Public ReadOnly Property Terminated() As Boolean
            Get
                Return mThreadActive = 0
            End Get
        End Property

        ''' <summary>
        ''' Run the check connection code.
        ''' </summary>
        ''' <returns></returns>
        Private Function CheckConnection(skipIfRecentlyCommunicated As Boolean) As Boolean
            Dim result = mTalker.CheckConnection(skipIfRecentlyCommunicated)

            If result Then
                'Pong came back from the server
                OnPing()
            Else
                'if the check connection fails then raise an event
                Log.Trace("Ping failed to connection {}", New With {ResourceId})
                OnPingFailed()
            End If
            Return result
        End Function

        Private Function PingConnection(skipIfRecentlyCommunicated As Boolean) As Boolean
            Return mTalker.CheckConnection(skipIfRecentlyCommunicated)
        End Function

        Public ReadOnly Property Processing As Boolean
            Get
                Return mThreadActive = 1
            End Get
        End Property

        Private mLockTime As DateTime
        ''' <summary>
        ''' Refactor how the process happens.  this will be called within a thread.
        ''' </summary>
        Private Function SignalResourceConnection(Optional processInput As Boolean = True) As Task(Of Boolean)

            Log.Debug("Signaling ResourceConnection {connection}", New With {ResourceName, ResourceId})
            Dim successfulConnection As Boolean = False
            If Interlocked.CompareExchange(mThreadActive, 1, 0) = 0 Then
                mLockTime = Date.Now
                Return Task.Run(Function()
                                    Dim sw = Stopwatch.StartNew()
                                    Log.Debug("Entering interlocked section {connection}", New With {Thread.CurrentThread.ManagedThreadId, ResourceName})
                                    Log.Debug("mTalker state {connection}", New With {ResourceName, mTalker?.IsConnnected()})
                                    Try

                                        Dim sErr As String = String.Empty
                                        'Connect
                                        Dim name As String = Nothing
                                        GetResourceName(sErr, name)
                                        sErr = AttemptConnection(sErr, name)
                                        Log.Debug($"Attempt Connection complete {name} took {sw.ElapsedMilliseconds} ms {sErr}")
                                        If Not String.IsNullOrEmpty(sErr) Then
                                            'Raise an OnError here with message.
                                            OnError(sErr)
                                            Return successfulConnection
                                        End If

                                        'reset the error flag
                                        LastErrortime = Date.MinValue

                                        'CheckConnection
                                        If CheckConnection(SkipIfRecentlyCommunicated) Then
                                            If processInput Then
                                                ProcessResourceInput(mPingResourceForInSeconds)
                                            End If
                                            successfulConnection = True
                                        End If
                                        Log.Debug($"Closed successful communication With {name} after {sw.ElapsedMilliseconds}ms")
                                    Catch ex As Exception
                                        OnError(ex.Message)
                                    Finally
                                        CleanupSignalResourceConnection()
                                        SignalRetryIfRequired()
                                    End Try
                                    Return successfulConnection
                                End Function)
            End If
            Return Task.FromResult(False)
        End Function

        Private Sub CleanupSignalResourceConnection()
            'make sure that the resource is always disconnected.
            Disconnect()
            OnSignalResourceConnectionComplete()
            Interlocked.Exchange(mThreadActive, 0)
        End Sub

        Private Sub SignalRetryIfRequired()
            'check if any messages have been queued after the disconnect has called.   
            If mSendEntryQueue.Any Then
                Log.Debug("Raising retry required {connection}", New With {ResourceName, mSendEntryQueue.Count})
                RaiseEvent RetryRequired(Me, New SignalResourceConnectionRetryRequiredEventArgs(ResourceId))
            End If
        End Sub

        Private Sub ProcessResourceInput(timeoutInSeconds As Integer)
            Dim startTime = Date.Now
            'Process any input...
            mTalker.ProcessInput()
            If mTalker.CheckStatusChanged() Then
                Log.Trace("{0} - Resource PC status changed", mResourceMachine.Name)
                StatusChange = StatusChange Or ResourceStatusChange.EnvironmentChange
            End If
            ProcessesRunning = mTalker.ProcessesRunning
            ProcessesPending = mTalker.ProcessesPending
            Log.Trace("Connection {1} process queue contains {0} items", mSendEntryQueue.Count, ResourceName)
            'Send any commands from the queue...
            Dim qi As SendQueueEntry
            Dim controlPermissionValidation = gSv.GetControllingUserPermissionSetting()
            Do

                'Grab the next entry from the
                'queue, if there is one. Remove
                'it at the same time.
                Monitor.Enter(mSendEntryQueue)
                Try
                    qi = mSendEntryQueue.DequeueOrDefault()
                Finally
                    Monitor.Exit(mSendEntryQueue)
                End Try

                If qi Is Nothing AndAlso startTime.AddSeconds(timeoutInSeconds) < Date.Now Then
                    Log.Trace("Finished processing mSendEntryQueue {connection}", New With {ResourceName})
                    Exit Do
                End If

                'Process the entry we just removed
                'from the queue.
                If qi IsNot Nothing Then
                    LogQueueMessage(qi)

                    Select Case qi.EntryType
                        Case SendQueueType.StartSession
                            StartSession(qi, controlPermissionValidation)

                        Case SendQueueType.VarUpdates
                            mTalker.Say("varupdates " & CStr(IIf(qi.Flag, "on", "off")), "SET", 10000)

                        Case SendQueueType.CreateSession
                            CreateSession(qi, controlPermissionValidation)

                        Case SendQueueType.StopSession
                            StopSession(qi)

                        Case SendQueueType.DeleteSession
                            DeleteSession(qi, controlPermissionValidation)

                        Case SendQueueType.SetSessionVariable
                            SetSessionVariable(qi)

                        Case SendQueueType.VarVariables
                            GetSessionVariables(qi)
                    End Select
                    'reset while we have something to do
                    startTime = Date.Now

                End If
                Thread.Sleep(mProcessResourceInputSleepTime)
            Loop
        End Sub

        Private Sub DeleteSession(qi As SendQueueEntry, controlPermissionValidation As Boolean)
            Dim deleteCmd As String

            Dim userId = Guid.Empty
            If qi.Token IsNot Nothing Then
                userId = qi.Token.OwningUserID
                deleteCmd = $"deleteas {qi.Token} {qi.SessionId}"
            ElseIf controlPermissionValidation Then
                Dim token = gSv.RegisterAuthorisationToken(qi.ProcessId)
                userId = token.OwningUserID
                deleteCmd = $"deleteas {token} {qi.SessionId}"
            Else
                deleteCmd = $"delete {qi.SessionId}"
            End If

            If mTalker.Say(deleteCmd, "SESSION DELETED") Then
                ' Fire a successful session delete event
                OnSessionDelete(New SessionDeleteEventArgs(qi.SessionId, userId))
            Else
                Dim sReply As String = mTalker.GetReply()
                Dim suffix As String
                Select Case sReply
                    Case Nothing, String.Empty
                        suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                    Case Else
                        suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                End Select

                OnSessionDelete(New SessionDeleteEventArgs(qi.SessionId,
                                                         String.Format(My.Resources.clsResourceConnection_FailedToDeleteSessionOn01, mResourceMachine.Name, suffix),
                                                         userId,
                                                         qi.ScheduledSessionId))

            End If
        End Sub

        Private Sub StopSession(qi As SendQueueEntry)
            Dim serverName = Environment.MachineName
            Try
                serverName = Net.Dns.GetHostEntry("").HostName
            Catch
            End Try
            If Not mTalker.Say("stop " & qi.SessionId.ToString & " " & mManager.UserId.ToString & " name " & serverName, "STOPPING") Then
                Dim sReply As String = mTalker.GetReply()
                Dim suffix As String
                Select Case sReply
                    Case Nothing, String.Empty
                        suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                    Case Else
                        suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                End Select

                OnSessionStop(New SessionStopEventArgs(qi.SessionId,
                                                       String.Format(My.Resources.clsResourceConnection_FailedToStopSessionOn01, mResourceMachine.Name, suffix),
                                                       qi.ScheduledSessionId))
            End If
        End Sub

        Private Sub CreateSession(qi As SendQueueEntry, controlPermissionValidation As Boolean)
            Dim createCmd As String
            If qi.SessionId = Guid.Empty Then qi.SessionId = Guid.NewGuid()
            Dim userId = Guid.Empty
            If qi.Token IsNot Nothing Then
                userId = qi.Token.OwningUserID
                createCmd = $"createas {qi.Token} {qi.ProcessId} {qi.QueueIdent} {qi.SessionId}"
            ElseIf controlPermissionValidation Then
                Dim token = gSv.RegisterAuthorisationToken(qi.ProcessId)
                userId = token.OwningUserID
                createCmd = $"createas {token} {qi.ProcessId} {qi.QueueIdent} {qi.SessionId}"
            Else
                createCmd = $"create {qi.ProcessId} {qi.QueueIdent} {qi.SessionId}"
            End If

            If mTalker.Say(createCmd, "SESSION CREATED") Then
                ' Fire a successful session created event
                OnSessionCreate(New SessionCreateEventArgs(
                    resourceId:=ResourceId,
                    processId:=qi.ProcessId,
                    sessionId:=qi.SessionId,
                    userId:=userId,
                    schedSessId:=qi.ScheduledSessionId) With {.Tag = qi.Tag})
            Else
                Dim reply As String = mTalker.GetReply()
                Dim userMessage As String
                Dim state As SessionCreateState
                Dim data As IDictionary(Of Guid, RunnerStatus) = Nothing
                Select Case reply
                    Case "UNAVAILABLE"
                        userMessage = String.Format(My.Resources.clsResourceConnection_0IsTooBusyToRunThatProcess, mResourceMachine.Name)
                        state = SessionCreateState.CreateFailedTooBusy
                        ' Try and get some extra diagnostics - ie
                        ' indicate the current state of the resource
                        Try
                            data = mTalker.GetSessions()

                        Catch ex As Exception
                            ' Just ignore it - it's diag data only,
                            ' and the important error has been 
                            ' caught already.
                        End Try

                    Case Nothing, String.Empty
                        Log.Debug($"No reply from the resource pc {mResourceMachine.Name}")
                        userMessage = String.Format(My.Resources.clsResourceConnection_FailedToCreateSessionOn0NoReplyFromResourcePC, mResourceMachine.Name)
                        state = SessionCreateState.CreateFailedNoError
                    Case Else
                        userMessage = String.Format(My.Resources.clsResourceConnection_FailedToCreateSessionOn01, mResourceMachine.Name, $"Unexpected response - '{mTalker.GetReply()}'")
                        state = SessionCreateState.CreateFailedGenericError
                End Select

                ' Fire the event with a different state and error msg
                ' indicating that the session could not be created

                OnSessionCreate(New SessionCreateEventArgs(
                    createState:=state,
                    resourceId:=mResourceMachine.Id,
                    processId:=qi.ProcessId,
                    sessionId:=qi.SessionId,
                    schedSessId:=qi.ScheduledSessionId,
                    errMsg:=userMessage,
                    data:=data,
                    userId:=userId) With {.Tag = qi.Tag})
            End If
        End Sub

        Private Sub StartSession(qi As SendQueueEntry, controlPermissionValidation As Boolean)

            If mTalker.Say("startp " & qi.InputXml, "PARAMETERS SET") Then

                Dim startCmd As String
                Dim userId As Guid
                If qi.Token IsNot Nothing Then
                    userId = qi.Token.OwningUserID
                    startCmd = $"startas {qi.Token} {qi.SessionId}"
                ElseIf controlPermissionValidation Then
                    Dim token = gSv.RegisterAuthorisationToken(qi.ProcessId)
                    userId = token.OwningUserID
                    startCmd = $"startas {token} {qi.SessionId}"
                Else
                    startCmd = $"start {qi.SessionId}"
                End If



                If mTalker.Say(startCmd, "STARTED") Then
                    OnSessionStart(New SessionStartEventArgs(sessid:= qi.SessionId, userid:= userId, schedId:= qi.ScheduledSessionId))
                Else
                    Dim reply As String = mTalker.GetReply()
                    Dim errmsg = If(reply = "",
                        My.Resources.clsResourceConnection_NoReplyFromResourcePC,
                        String.Format(My.Resources.clsResourceConnection_ReplyWas0, reply))

                    OnSessionStart(New SessionStartEventArgs(sessid:= qi.SessionId,
                                                             errmsg := errmsg,
                                                             userid:= userId,
                                                             usermsg:= String.Format(My.Resources.clsResourceConnection_FailedToStartSessionOn01, mResourceMachine.Name, errmsg),
                                                             schedId:= qi.ScheduledSessionId))
                End If
            Else
                Dim reply As String = mTalker.GetReply()
                Dim errmsg = If(reply = "",
                    My.Resources.clsResourceConnection_NoReplyFromResourcePC,
                    String.Format(My.Resources.clsResourceConnection_ReplyWas0, reply))

                OnSessionStart(New SessionStartEventArgs(sessid:= qi.SessionId,
                                                         errmsg:= errmsg,
                                                         userid:= If(qi.Token?.OwningUserID, Guid.Empty),
                                                         usermsg:= String.Format(My.Resources.clsResourceConnection_FailedToSetStartParametersOn01, mResourceMachine.Name, errmsg),
                                                         schedId:= qi.ScheduledSessionId
                                                         ))
            End If

        End Sub

        Private Sub SetSessionVariable(qi As SendQueueEntry)
            Dim sessionVars As New List(Of String)
            Dim expectedReply As String = "SET"
            For Each var In qi.SessionVariables
                sessionVars.Add(var.ToEscapedString(False))
            Next

            Dim saySuccess = mTalker.Say("setvar " & qi.SessionVariables.First.SessionID.ToString & " " & String.Join(",", sessionVars), expectedReply)

            If saySuccess Then
                Dim sReply As String = mTalker.GetReply()
                Dim suffix As String

                If sReply <> expectedReply Then
                    Select Case sReply
                        Case Nothing, String.Empty
                            suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                        Case Else
                            suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                    End Select
                    OnSessionManagerVariableUpdated(New SessionVariableUpdatedEventArgs(String.Empty,
                                                                                        String.Format(My.Resources.clsResourceConnection_FailedToSetSessionVariableOn01, mResourceMachine.Name, suffix)))
                Else
                    qi.SessionId = qi.SessionVariables.First.SessionID
                    GetSessionVariables(qi)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Request the current session values from the server using the talker/listener method
        ''' </summary>
        ''' <param name="qi"></param>
        Private Sub GetSessionVariables(qi As SendQueueEntry)
            Dim getVarSuccess As Boolean = mTalker.Say($"vars {qi.SessionId}", "VARIABLES", 2000)

            If getVarSuccess Then
                Dim sReply As String = mTalker.GetReply()
                Dim suffix As String
                Select Case sReply
                    Case Nothing, String.Empty
                        suffix = My.Resources.clsResourceConnection_NoReplyFromResourcePC
                        Log.Debug("No response to command GetSessionVariables {connection}", New With {ResourceName, suffix})
                        QueueUserMessage(String.Format("Failed to GetSessionVariables {0} {1}", mResourceMachine.Name, suffix))
                    Case Else
                        suffix = String.Format(My.Resources.clsResourceConnection_ReplyWas0, sReply)
                        Log.Debug("GetSessionVariables response {suffix}", New With {suffix})

                        Dim separatingString As String() = {"//END//"}
                        Dim sessionVariables = sReply.Replace("VARIABLES", String.Empty) _
                                                     .Split(separatingString, StringSplitOptions.RemoveEmptyEntries)

                        For Each entry As String In sessionVariables
                            Dim sv As clsSessionVariable = clsSessionVariable.Parse(entry)
                            sv.SessionID = qi.SessionId
                            HandleSessionVariableChanged(sv)
                        Next
                End Select
            Else
                Log.Debug("GetSessionVariables: failed to process {connection}", New With {ResourceName})
            End If
        End Sub

        Private Sub GetResourceName(ByRef sErr As String, ByRef name As String)
            name = Nothing

            If (mResourceMachine.Attributes And ResourceAttribute.Pool) = 0 Then
                name = mResourceMachine.Name
                Log.Trace("{0} - Connecting directly to resource", name)
            Else
                gSv.GetPoolControllerName(mResourceMachine.Id, name)

                If String.IsNullOrWhiteSpace(name) Then
                    Log.Trace("{0} - Unable to get pool controller, setting offline and delaying connection for 20 cycles", name)
                    ConnectionState = ResourceConnectionState.Offline
                    ConnectLater(ConnectIntervals.ControllerNotAvailable)
                    StatusChange = StatusChange Or ResourceStatusChange.OfflineChange
                Else
                    Log.Trace("{0} - Connecting to pool controller", name)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Attempt connection to resource
        ''' </summary>
        ''' <param name="sErr"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Private Function AttemptConnection(sErr As String, name As String) As String

            If Connect(name, sErr, ConnectionRetryCount, ConnectionRetryTimeout) Then
                Log.Debug("{0} - Connected", name)
                'Authenticate, if there is a user logged in. When used from a Resource
                'PC, for example, there is no logged in user!
                Log.Debug($"Manager UserId = {mManager?.UserId}")
                If mManager.UserId <> Nothing AndAlso mTalker.Authenticate() Then
                    Log.Trace("{0} - Authenticated", name)
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
                        ConnectionState = ResourceConnectionState.Connected
                        Log.Debug("{0} - Connected", name)
                    End If
                Else
                    Log.Debug("{0} - Resource in use, delaying connection", name)
                    mTalker.Close()
                    ConnectionState = ResourceConnectionState.InUse
                    ConnectLater(ConnectIntervals.InUse)
                End If
                mResourceMachine.LastError = ""
                mEventLogDelay = 0
                StatusChange = StatusChange Or ResourceStatusChange.OnlineChange
            Else
                Log.Debug("{0} - Failed to connect", name)
                SetConnectionError(name, sErr)
                ClearSendQueue()
            End If

            Return sErr
        End Function

        Private Function Connect(name As String, ByRef sErr As String, retryCount As Integer, timeoutInMilliseconds As Integer) As Boolean
            Dim tryCount = 0
            While tryCount < retryCount
                mTalker = New clsTalker(mPingTimeoutSeconds)
                Dim result = mTalker.Connect(name, sErr)
                RaiseEvent Statistic(Me, New NewConnectionStatisticEventArgs(ResourceId, result, mTalker.LastPingTime, Date.UtcNow))
                If result Then
                    OnConnection()
                    Return True
                End If
                tryCount += 1
                Log.Debug("Failed to connect to resource {connection}", New With {name, timeoutInMilliseconds, tryCount})
                mTalker.Dispose()
                Task.Delay(timeoutInMilliseconds)
            End While

            Return False
        End Function

        ''' <summary>
        ''' Wrap the disconnect code.
        ''' </summary>
        ''' <returns></returns>
        Private Function Disconnect() As Boolean
            Try
                If mTalker IsNot Nothing Then
                    ConnectionState = ResourceConnectionState.Sleep
                    mTalker.Close()
                    OnDisconnected()
                End If
            Catch ex As Exception
                OnError(ex.Message)
            End Try
        End Function



        ''' <summary>
        ''' Raise a resource connection complete event
        ''' </summary>
        Protected Overridable Sub OnSignalResourceConnectionComplete()
            Try
                RaiseEvent SignalResourceComplete(Me, New EventArgs)
            Catch ex As Exception
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub OnPingFailed()
            RaiseEvent Statistic(Me, New NewConnectionStatisticEventArgs(ResourceId, False, mTalker.LastPingTime, Date.UtcNow))
            RaiseEvent PingFailed(Me, New EventArgs())
        End Sub

        Protected Overridable Sub OnPing()
            RaiseEvent Statistic(Me, New NewConnectionStatisticEventArgs(ResourceId, True, mTalker.LastPingTime, Date.UtcNow))
            RaiseEvent PingCompleted(Me, New EventArgs())
        End Sub

        ''' <summary>
        ''' Raise an error back to the calling thread
        ''' </summary>
        ''' <param name="message"></param>
        Protected Overridable Sub OnError(message As String)
            Try
                ' Info so as not to break nightlies, but stil ends up in event viewer
                Log.Info($"OnDemandConnection: Handled error received from resource: {ResourceId}, message: {message}")
                LastErrortime = Date.UtcNow
                RaiseEvent Errors(Me, New ErrorEventArgs(message))
                RaiseEvent Statistic(Me, New NewConnectionStatisticEventArgs(ResourceId, False, 0, Date.UtcNow))
            Catch ex As Exception
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub OnConnection()
            Try
                Log.Trace($"Resource {ResourceId} Connected")
                RaiseEvent Connected(Me, New EventArgs())
            Catch ex As Exception
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub OnDisconnected()
            Try
                Log.Trace($"Resource {ResourceId} Disconnected")
                RaiseEvent Disconnected(Me, New EventArgs())
            Catch ex As Exception
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
End Namespace
