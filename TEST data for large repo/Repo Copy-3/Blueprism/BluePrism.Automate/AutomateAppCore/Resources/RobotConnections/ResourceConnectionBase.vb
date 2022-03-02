Imports System.Threading.Tasks
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.Server.Domain.Models
Imports NLog
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.ClientServerResources.Core.Enums

Namespace Resources
    Public MustInherit Class ResourceConnectionBase
        Implements IResourceConnection

        ''' <summary>
        ''' The clsTalker object used for communication with this resource PC.
        ''' </summary>
        Protected WithEvents mTalker As clsTalker

        Protected Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()


        Protected Class ConnectIntervals
            Public Shared ReadOnly ConnectionCheckFailed As TimeSpan = TimeSpan.FromSeconds(5)
            Public Shared ReadOnly ControllerNotAvailable As TimeSpan = TimeSpan.FromSeconds(20)
            Public Shared ReadOnly CapabilitiesError As TimeSpan = TimeSpan.FromSeconds(50)
            Public Shared ReadOnly InUse As TimeSpan = TimeSpan.FromSeconds(50)
            Public Shared ReadOnly DBStatusOffline As TimeSpan = TimeSpan.FromSeconds(5)
            Public Shared ReadOnly ConnectionError As TimeSpan = TimeSpan.FromSeconds(10)
        End Class

        ''' <summary>
        ''' Queue of 'commands' to send to resource PC from the comms thread....
        ''' </summary>
        Protected Enum SendQueueType
            StartSession            'Needs SessionID,InputXML,LoggingXML
            CreateSession           'Needs ProcessID
            StopSession             'Needs SessionID
            DeleteSession           'Needs SessionID
            SetSessionVariable      'Needs SessionVariable
            VarUpdates              'Needs Flag
            VarVariables
        End Enum

        Protected Class SendQueueEntry
            Public Property EntryType As SendQueueType
            Public Property SessionId As Guid
            Public Property InputXml As String
            Public Property ProcessId As Guid
            Public Property QueueIdent As Integer
            Public Property ScheduledSessionId As Integer
            Public Property SessionVariables As Queue(Of clsSessionVariable)
            Public Property Flag As Boolean
            Public Property Tag As Object
            Public Property Token As clsAuthToken
            Public Overrides Function ToString() As String
                Return String.Format(
                 "{0}: Session:{1}; ProcessID:{2}; ScheduledSessionId:{3}",
                 EntryType, SessionId, ProcessId, ScheduledSessionId
                )
            End Function
        End Class


        ''' Project  : Automate
        ''' Class    : clsResourceConnectionManager.clsResourceConnection.clsMsgQueueEntry
        ''' <summary>
        ''' Queue of user messages from the resource PC.
        ''' </summary>
        Protected Class MsgQueueEntry
            Public Property MsMessage As String
            Public Property HelpTopicNumber As Integer = -1
            Public Property HelpPage As String
        End Class

        ''' <summary>
        ''' The state of the connection
        ''' </summary>
        Private mConnectionState As ResourceConnectionState

        ''' <summary>
        ''' Gets the state of this connection.
        ''' </summary>
        ''' <returns>Returns the state of this connection.</returns>

        Public Property ConnectionState() As ResourceConnectionState Implements IResourceConnection.ConnectionState
            Get
                Return mConnectionState
            End Get
            Protected Set(value As ResourceConnectionState)
                mConnectionState = value
            End Set
        End Property

        ''' <summary>
        ''' The number of processes the remote Resource PC is currently running.
        ''' </summary>
        Public Property ProcessesRunning() As Integer Implements IResourceConnection.ProcessesRunning
            Get
                Return mProcessesRunning
            End Get
            Set
                mProcessesRunning = Value
            End Set
        End Property
        Private mProcessesRunning As Integer = 0

        ''' <summary>
        ''' The number of processes the remote Resource PC has pending.
        ''' </summary>
        Public Property ProcessesPending() As Integer Implements IResourceConnection.ProcessesPending
            Get
                Return mProcessesPending
            End Get
            Protected Set(value As Integer)
                mProcessesPending = value
            End Set
        End Property
        Private mProcessesPending As Integer = 0


        ''' <summary>
        ''' Gets the resource ID for the resource represented by this connection, or
        ''' <see cref="Guid.Empty"/> if this connection has no resource machine
        ''' associated with it (which really shouldn't happen).
        ''' </summary>
        Public ReadOnly Property ResourceId As Guid Implements IResourceConnection.ResourceId
            Get
                If mResourceMachine Is Nothing Then Return Guid.Empty
                Return mResourceMachine.Id
            End Get
        End Property



        ''' <summary>
        ''' Current status change level on this resource PC.
        ''' </summary>
        Private mStatusChange As ResourceStatusChange

        ''' <summary>
        ''' Holds a value indicating whether or not mStatusChanged is dirty
        ''' </summary>
        Private mHasStatusChanged As Boolean

        Private mLastErrorTime As Date

        ''' <summary>
        ''' Event raised when notification of a session variable changing is received.
        ''' </summary>
        Public Event SessionVariableChanged(e As clsSessionVariable)

        ''' <summary>
        ''' Event raised when notification of a session ending is received.
        ''' </summary>
        Public Event SessionEnd As SessionEndEventHandler

        ''' <summary>
        ''' Event raised when a session has been deleted
        ''' </summary>
        Public Event SessionDelete As SessionDeleteEventHandler

        ''' <summary>
        ''' Event raised when a session has been created
        ''' </summary>
        Public Event SessionCreate As SessionCreateEventHandler

        Public Event SessionStop As SessionStopEventHandler

        ''' <summary>
        ''' Event raised when a session start has occurred. Note that this is called if
        ''' the start failed for some reason too. The error message in the event args
        ''' gives details about the problem.
        ''' </summary>
        Public Event SessionStart As SessionStartEventHandler

        ' Used in OnDemand/ASCR after a failed Session Variable update
        Public Event SessionManagerVariableUpdated As SessionVariableUpdatedHandler

        Public Event ResourceStatusChanged As ResourcesChangedEventHandler

        ''' <summary>
        ''' Controls whether connection check can be skipped and the connection assumed to be OK if the
        ''' Resource PC has recently been in communication
        ''' </summary>
        Private mSkipIfRecentlyCommunicated As Boolean

        Public Property SkipIfRecentlyCommunicated As Boolean
            Get
                Return mSkipIfRecentlyCommunicated
            End Get
            Protected Set(value As Boolean)
                mSkipIfRecentlyCommunicated = value
            End Set
        End Property


        ''' <summary>
        ''' The time at which the next attempt should be made to establish a network connection
        ''' </summary>
        Private mNextConnectTime As Date = Date.UtcNow

        Public Property NextConnectTime As Date
            Get
                Return mNextConnectTime
            End Get
            Protected Set(value As Date)
                mNextConnectTime = value
            End Set
        End Property

        ''' <summary>
        ''' The resource to which we would like to connect / to which we are
        ''' connected.
        ''' </summary>
        ''' <value>The resource</value>
        Public ReadOnly Property ResourceMachine() As IResourceMachine
            Get
                Return mResourceMachine
            End Get
        End Property

        Protected WithEvents mResourceMachine As ResourceMachine



        Public Property StatusChange As ResourceStatusChange
            Protected Set
                If Not mHasStatusChanged Then mHasStatusChanged = (mStatusChange <> Value)
                mStatusChange = Value
            End Set
            Get
                Return mStatusChange
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this resource connection is
        ''' <see cref="ResourceConnectionState.Connected">connected</see> according
        ''' to its current connection state.
        ''' </summary>
        Public ReadOnly Property IsConnected As Boolean
            Get
                Return (mConnectionState = ResourceConnectionState.Connected)
            End Get
        End Property


        ''' <summary>
        ''' Gets whether this resource connection is
        ''' <see cref="ResourceConnectionState.Unavailable">unavailable</see> according
        ''' to its current connection state.
        ''' </summary>
        Public ReadOnly Property IsUnavailable As Boolean
            Get
                Return (mConnectionState = ResourceConnectionState.Unavailable)
            End Get
        End Property

        Public Property LastErrortime As Date
            Get
                Return mLastErrorTime
            End Get
            Protected Set(value As Date)
                mLastErrorTime = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the resource name of the machine that this connection is on behalf of,
        ''' or an empty string if there is no machine associated with this connection.
        ''' </summary>
        ''' <value></value>
        Public ReadOnly Property ResourceName As String
            Get
                Return If(mResourceMachine Is Nothing, String.Empty, mResourceMachine.Name)
            End Get
        End Property


        ''' <summary>
        ''' Check if the status of the resource PC has changed. The flag is then reset.
        ''' </summary>
        Friend Function PopStatusHasChanged() As Boolean
            Dim result = mHasStatusChanged
            mHasStatusChanged = False
            Return result
        End Function


        ''' <summary>
        ''' Resets any wait times and caching using when updating connection status.
        ''' This is used when the machine's attributes or status has changed to ensure
        ''' that the status is updated promptly.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub EnsureImmediateConnectionStateUpdate()
            mNextConnectTime = Date.UtcNow
            mSkipIfRecentlyCommunicated = False
        End Sub

        Public Function GetMembers() As List(Of clsTalker.MemberInfo)
            Return mTalker.GetMembers()
        End Function


        ''' <summary>
        ''' Ensures that this resource connection is actually connected.
        ''' </summary>
        ''' <exception cref="NotConnectedException">If the resource is not connected.
        ''' </exception>
        Protected Overloads Sub EnsureConnected()
            If Not IsConnected Then Throw New NotConnectedException()
        End Sub


        ''' <summary>
        ''' Send the command to enable or disable session variable updates.
        ''' </summary>
        ''' <param name="value">True to enable, False to disable.</param>
        Public Sub SendVarUpdates(ByVal value As Boolean)
            If IsConnected Then AddToSendQueue(New SendQueueEntry() With {
                .EntryType = SendQueueType.VarUpdates,
                .Flag = value
            })
        End Sub

        Public Sub SendStartSession(session As StartSessionData)
            AddToSendQueue(New SendQueueEntry() With {
                              .EntryType = SendQueueType.StartSession,
                              .SessionId = session.SessionId,
                              .InputXml = session.InputParametersXML,
                              .Tag = session.Tag,
                              .ProcessId = session.ProcessId,
                              .Token = session.AuthorizationToken,
                              .ScheduledSessionId = session.ScheduledSessionId})
        End Sub
        
        Public Function SendCreateSession(sessionData As CreateSessionData) As Guid
            Dim sessionId = Guid.NewGuid
            AddToSendQueue(New SendQueueEntry() With {
                              .EntryType = SendQueueType.CreateSession,
                              .ProcessId = sessionData.ProcessId,
                              .QueueIdent = sessionData.QueueIdentifier,
                              .Tag = sessionData.Tag,
                              .Token = sessionData.AuthorizationToken,
                              .SessionId = sessionId,
                              .ScheduledSessionId = sessionData.ScheduledSessionId
                              })
            Return sessionId
        End Function
        
        ''' <summary>
        ''' Send the relevant commands to stop a session to the Resource on the other end
        ''' of this connection. Executes asynchronously.
        ''' </summary>
        ''' <param name="sessId">The Session ID</param>
        Public Sub SendStopSession(sessionData As StopSessionData)
            AddToSendQueue(New SendQueueEntry With {
                              .EntryType = SendQueueType.StopSession,
                              .SessionId = sessionData.SessionId,
                              .ScheduledSessionId = sessionData.ScheduleSessionId
                              })
        End Sub



        Public Sub SendGetSessionVar(ByVal sessionID As Guid, token As clsAuthToken)
            AddToSendQueue(New SendQueueEntry With {
                               .EntryType = SendQueueType.VarVariables,
                               .SessionId = sessionID,
                               .Token = token
            })
        End Sub

        ''' <summary>
        ''' Send the relevant commands to delete a session to the Resource on the other
        ''' end of this connection. Executes asynchronously.
        ''' </summary>
        ''' <param name="gSessionID">The Session ID</param>
        ''' <param name="sErr">On failure, an error description</param>
        ''' <returns>True if successful, False otherwise</returns>
        Public Sub SendDeleteSession(session As DeleteSessionData)
            AddToSendQueue(New SendQueueEntry With {
                              .EntryType = SendQueueType.DeleteSession,
                              .SessionId = session.SessionId,
                              .ProcessId = session.ProcessId,
                              .Token = session.AuthorizationToken,
                              .ScheduledSessionId = 0
                              })
        End Sub


        ''' <summary>
        ''' Send the relevant command to change the value of a session variable.
        ''' </summary>
        ''' <param name="vars">The session variables to set.</param>
        Public Sub SendSetSessionVariable(vars As Queue(Of clsSessionVariable))
            AddToSendQueue(New SendQueueEntry With {
                .EntryType = SendQueueType.SetSessionVariable,
                .SessionVariables = vars
            })
        End Sub

        ''' <summary>
        ''' Add a message to the queue of messages to be reported back to the user.
        ''' </summary>
        ''' <param name="sText"></param>
        Protected Sub QueueUserMessage(ByVal sText As String, Optional ByVal helpTopicNumber As Integer = -1, Optional ByVal helpPage As String = "")

            QueueMsgQueueEntry(New MsgQueueEntry With {
                .MsMessage = sText,
                .HelpTopicNumber = helpTopicNumber,
                .HelpPage = helpPage
            })

        End Sub

        Public Function SendGetVariable(ByVal sessionID As Guid, ByRef sErr As String) As Boolean
            If sessionID = Guid.Empty Then
                Throw New InvalidOperationException("SendGetVariable sessionID is empty")
            End If

            Try
                AddToSendQueue(New SendQueueEntry With {
                               .EntryType = SendQueueType.VarVariables,
                               .SessionId = sessionID
                               })
                Return True
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try
        End Function

        Protected MustOverride Sub QueueMsgQueueEntry(queueEntry As MsgQueueEntry)


        ''' <summary>
        ''' Finds out if the connected resource has any pending or running sessions.
        ''' </summary>
        ''' <param name="bBusy">If successful, set to True if there are pending or
        ''' running sessions or False otherwise.</param>
        ''' <param name="sErr">Error message in the event of failure.</param>
        ''' <returns>True unless an error occurs.</returns>
        Public Function AskIsBusy(ByRef bBusy As Boolean, ByRef sErr As String) As Boolean
            If mConnectionState <> ResourceConnectionState.Connected Then
                bBusy = False
                Return True
            End If

            If Not mTalker.Say("busy", New String() {"yes", "no"}) Then
                sErr = "Internal communications error - reply """ & mTalker.GetReply & """ came in response to 'AreYouBusy' query"
                Return False
            End If

            bBusy = mTalker.GetReply() = "yes"
            Return True
        End Function


        ''' <summary>
        ''' Gets the sessions registered on the resource connected to by this connection
        ''' </summary>
        ''' <returns>A mapping of runner statuses onto the IDs of the sessions that they
        ''' are running</returns>
        Public Function GetSessions() As IDictionary(Of Guid, RunnerStatus) Implements IResourceConnection.GetSessions
            If mTalker Is Nothing Then Return GetEmpty.IDictionary(Of Guid, RunnerStatus)()
            Return mTalker.GetSessions()
        End Function

        Public MustOverride Function RefreshResource() As Task(Of Boolean) Implements IResourceConnection.RefreshResource

        ''' <summary>
        ''' The method for pushing the message onto the queue will differ depending on the impl
        ''' </summary>
        ''' <param name="entry"></param>
        Protected MustOverride Sub AddToSendQueue(entry As SendQueueEntry)

        Public MustOverride Function AwaitValidConnection(
         ByVal timeoutMillis As Integer,
         ByVal inuseIsValid As Boolean,
         ByRef state As ResourceConnectionState) As Boolean Implements IResourceConnection.AwaitValidConnection


        ''' <summary>
        ''' Terminates this connection. Called by the owner when it is no longer
        ''' required. Note that the process of termination is not necessarily
        ''' instantaneous.
        ''' </summary>
        Public Overridable Sub Terminate() Implements IResourceConnection.Terminate
            'do nothing?
        End Sub

        Protected Sub ConnectLater(interval As TimeSpan)
            mNextConnectTime = Date.UtcNow.Add(interval)
        End Sub

        ''' <summary>
        ''' Sets the correct statuses if the connect failed
        ''' </summary>
        ''' <param name="name">The name of the connection that failed</param>
        ''' <param name="errorMsg">The errormessage when the connection failed</param>
        Protected Sub SetConnectionError(name As String, errorMsg As String)
            Log.Debug("{0} - Connection error: {1}", name, errorMsg)
            mResourceMachine.LastError = errorMsg
            mConnectionState = ResourceConnectionState.Offline
            ConnectLater(ConnectIntervals.ConnectionError)
            mStatusChange = mStatusChange Or ResourceStatusChange.OfflineChange
        End Sub


        ''' <summary>
        ''' Handles a session variable changed event from the underlying talker, chaining
        ''' it onto the <see cref="SessionVariableChanged"/> event in this connection
        ''' after filling in its resource ID.
        ''' </summary>
        Protected Sub HandleSessionVariableChanged(ByVal sv As clsSessionVariable) Handles mTalker.SessionVariableChanged
            'Fill in the Resource ID so we can track where it came from.
            sv.ResourceID = mResourceMachine.Id
            RaiseEvent SessionVariableChanged(sv)
        End Sub

        Protected Sub HandleDbStatusChanged(sender As Object, e As EventArgs) Handles mResourceMachine.DbStatusChanged
            EnsureImmediateConnectionStateUpdate()
        End Sub

        ''' <summary>
        ''' Handles event raised by resource machine - used to update connection
        ''' state promptly in response to changes
        ''' </summary>
        Protected Sub HandleAttributesChanged(sender As Object, e As EventArgs) Handles mResourceMachine.AttributesChanged
            EnsureImmediateConnectionStateUpdate()
        End Sub


        ''' <summary>
        ''' Handles a session started event from the underlying talker, chaining it onto
        ''' the <see cref="SessionStart"/> event in this connection.
        ''' </summary>
        Private Sub HandleSessionStarted(sender As Object, e As SessionStartEventArgs) Handles mTalker.SessionStarted
            OnSessionStart(e)
        End Sub

        ''' <summary>
        ''' Handles a session ended event from the underlying talker, chaining it onto
        ''' the <see cref="SessionEnd"/> event in this connection.
        ''' </summary>
        Private Sub HandleSessionEnded(sender As Object, e As SessionEndEventArgs) Handles mTalker.SessionEnded
            OnSessionEnd(e)
        End Sub


        ''' <summary>
        ''' Raises the <see cref="SessionEnd"/> event from this connection
        ''' </summary>
        Protected Overridable Sub OnSessionEnd(e As SessionEndEventArgs)
            RaiseEvent SessionEnd(Me, e)
        End Sub

        ''' <summary>
        ''' Fires the session-created event using the given values.
        ''' This is actually called by clsResourceConnection instances when they detect
        ''' a session has been created.
        ''' </summary>
        ''' <param name="e">The events detailing the session create event.</param>
        Protected Overridable Sub OnSessionCreate(ByVal e As SessionCreateEventArgs)
            Try
                RaiseEvent SessionCreate(Me, e)
            Catch ex As Exception
                ' Ensure that badly behaved listeners don't break this connection
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        ''' <summary>
        ''' Fires the session-deleted event using the given values.
        ''' This is actually called by clsResourceConnection instances when they detect
        ''' a session has been deleted.
        ''' </summary>
        ''' <param name="e">The events detailing the session delete event.</param>
        Protected Overridable Sub OnSessionDelete(ByVal e As SessionDeleteEventArgs)
            Try
                RaiseEvent SessionDelete(Me, e)
            Catch ex As Exception
                ' Ensure that badly behaved listeners don't break this connection
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub OnSessionStop(ByVal e As SessionStopEventArgs)
            Try
                RaiseEvent SessionStop(Me, e)
            Catch ex As Exception
                ' Ensure that badly behaved listeners don't break this connection
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        ''' <summary>
        ''' Fires the session-started event using the given values.
        ''' This is actually called by clsResourceConnection instances when they detect
        ''' a session has been started.
        ''' </summary>
        ''' <param name="e">The events detailing the session start event.</param>
        Protected Overridable Sub OnSessionStart(e As SessionStartEventArgs)
            Try
                RaiseEvent SessionStart(Me, e)
            Catch ex As Exception
                ' Ensure that badly behaved listeners don't break this connection
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub OnSessionManagerVariableUpdated(e As SessionVariableUpdatedEventArgs)
            Try
                RaiseEvent SessionManagerVariableUpdated(Me, e)
            Catch ex As Exception
                ' Ensure that badly behaved listeners don't break this connection
                Debug.Fail(ex.ToString())
            End Try
        End Sub

        Protected Overridable Sub LogQueueMessage(qi As SendQueueEntry)
            Log.Trace("Send Message {entry}", New With {qi.EntryType, qi.ProcessId, qi.SessionId, qi.ScheduledSessionId, qi.QueueIdent, qi.InputXml})
        End Sub

    End Class

End Namespace
