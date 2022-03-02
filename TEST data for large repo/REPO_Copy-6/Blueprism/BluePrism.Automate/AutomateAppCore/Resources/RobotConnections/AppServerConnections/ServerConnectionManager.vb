Imports System.Collections.Concurrent
Imports System.Drawing
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.ClientServerResources.Core.Exceptions
Imports BluePrism.ClientServerResources.Core.Interfaces
Imports BluePrism.Core.Compression
Imports BluePrism.Core.Resources
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Utilities.Functional
Imports NLog

Namespace Resources
    Public Class ServerConnectionManager
        Implements IResourceConnectionManager


        Private ReadOnly mLog As Logger = LogManager.GetCurrentClassLogger()
        Private ReadOnly mServer As IServer = Nothing
        Private ReadOnly mPingTimeoutSeconds As Integer = 30
        Private ReadOnly mTokenSource As New CancellationTokenSource()
        Private ReadOnly mCancellationToken As CancellationToken = mTokenSource.Token
        Private ReadOnly mSessionVariables As New ConcurrentDictionary(Of String, clsSessionVariable)
        Private ReadOnly mConnectionPingCooldown As Integer
        Private ReadOnly mProcessResourceInputSleepTime As Integer

        Private mNextRefresh As Date = Date.MinValue
        Private mPrivateResourcesThatAreNotMine As New HashSet(Of Guid)
        Private mResourceMachines As New Dictionary(Of Guid, IResourceMachine)
        Private mResourceMachineNameMap As New Dictionary(Of String, IResourceMachine)

        Private WithEvents mLocalResource As OnDemandConnection = Nothing

        Private WithEvents mInstructionalClientController As IInstructionalClientController

        Public Sub New(server As IServer)
            mServer = server
            RateLimiter = New AutoResetEvent(True)
            ConnectionUser = User.Current
            UserId = User.Current.Id
            ServerName = server.GetServerFullyQualifiedDomainName

            mPingTimeoutSeconds = mServer.GetPref(PreferenceNames.ResourceConnection.PingTimeOutSeconds, 30)
            mConnectionPingCooldown = mServer.GetPref(PreferenceNames.ResourceConnection.ConnectionPingTime, 5)
            mProcessResourceInputSleepTime = mServer.GetPref(PreferenceNames.ResourceConnection.ProcessResourceInputSleepTime, 100)

            ' we should move this out of the constructor. Not all users benefit from server callbacks
            mInstructionalClientController =
                If(gSv.GetCallbackConfigProtocol() = CallbackConnectionProtocol.Grpc,
                DependencyResolver.ResolveKeyed(Of IInstructionalClientController)(CallbackConnectionProtocol.Grpc, New Autofac.TypedParameter(GetType(BluePrism.ClientServerResources.Core.Config.ConnectionConfig), Options.Instance.ResourceCallbackConfig)),
                DependencyResolver.ResolveKeyed(Of IInstructionalClientController)(CallbackConnectionProtocol.Wcf, New Autofac.TypedParameter(GetType(BluePrism.ClientServerResources.Core.Config.ConnectionConfig), Options.Instance.ResourceCallbackConfig)))
            Try
                mInstructionalClientController.TokenTimeoutInSeconds = gSv.GetIntPref(ResourceConnection.ResourceConnectionTokenTimeout)
                mInstructionalClientController.ReconnectIntervalSeconds = gSv.GetPref(ResourceConnection.CallbackReconnectIntervalSeconds, 30)
                mInstructionalClientController.KeepAliveTimeMS = gSv.GetPref(ResourceConnection.CallbackKeepAliveTimeMS, 15000)
                mInstructionalClientController.KeepAliveTimeoutMS = gSv.GetPref(ResourceConnection.CallbackKeepAliveTimeoutMS, 10000)
            Catch ex As NoSuchPreferenceException
                ' Allowing for future addition on these preferences.
            End Try
            mInstructionalClientController.Connect()
            mInstructionalClientController.RegisterClient()

            Task.Run(Sub()
                         While True
                             If mCancellationToken.IsCancellationRequested Then Exit Sub
                             ' Allow locally managed resources to connect.
                             RateLimiter.Set()

                             If mNextRefresh < Date.UtcNow Then
                                 Try
                                     ResourceRefresh()
                                 Catch ex As Exception
                                     mLog.Info(ex, "Unable to get latest resource information")
                                 Finally
                                     mNextRefresh = Date.UtcNow.AddSeconds(10)
                                 End Try
                             End If

                             Thread.Sleep(1000)
                         End While
                     End Sub, mCancellationToken)
        End Sub


        Public Property MonitorSessionVariables As Boolean Implements IResourceConnectionManager.MonitorSessionVariables

        Public ReadOnly Property SessionVariables() _
     As IDictionary(Of String, clsSessionVariable) Implements IResourceConnectionManager.SessionVariables
            Get
                Return New Dictionary(Of String, clsSessionVariable)(mSessionVariables)
            End Get
        End Property

        Public ReadOnly Property UserId As Guid Implements IResourceConnectionManager.UserId
        Public ReadOnly Property PoolId As Guid Implements IResourceConnectionManager.PoolId
        Public ReadOnly Property RateLimiter As AutoResetEvent Implements IResourceConnectionManager.RateLimiter
        Public Property ConnectionUser As IUser = User.Current Implements IResourceConnectionManager.ConnectionUser
        Public ReadOnly Property Mode As IResourceConnectionManager.Modes = IResourceConnectionManager.Modes.Normal Implements IResourceConnectionManager.Mode
        Public ReadOnly Property QueryCapabilities As Boolean = False Implements IResourceConnectionManager.QueryCapabilities
        Public ReadOnly Property ServerName As String Implements IResourceConnectionManager.ServerName
        Public ReadOnly Property UsingAppServerConnection As Boolean Implements IResourceConnectionManager.UsingAppServerConnection
            Get
                Return True
            End Get
        End Property

        Public Event ResourceStatusChanged As ResourcesChangedEventHandler Implements IResourceConnectionManager.ResourceStatusChanged
        Public Event SessionCreate As SessionCreateEventHandler Implements IResourceConnectionManager.SessionCreate
        Public Event SessionDelete As SessionDeleteEventHandler Implements IResourceConnectionManager.SessionDelete
        Public Event SessionEnd As SessionEndEventHandler Implements IResourceConnectionManager.SessionEnd
        Public Event SessionStop As SessionStopEventHandler Implements IResourceConnectionManager.SessionStop
        Public Event SessionStart As SessionStartEventHandler Implements IResourceConnectionManager.SessionStart
        Public Event SessionVariablesUpdated As SessionVariableUpdatedHandler Implements IResourceConnectionManager.SessionVariablesUpdated
        Public Event OnCallbackError As InvalidResponseEventHandler
        Public Event ShowUserMessage As EventHandler(Of String) Implements IResourceConnectionManager.ShowUserMessage

        Private Sub ResourceRefresh()
            Dim excludedResources = ResourceAttribute.Debug Or ResourceAttribute.Retired
            Dim compressedResources = mServer.GetResourceInfoCompressed(ResourceAttribute.None, excludedResources)
            Dim resources = GZipCompression.InflateAndDeserialize(Of ICollection(Of ResourceInfo))(compressedResources)

            UpdateLastConnectionStatistics(resources)
            UpdateLocalResources()
            UpdatePrivateResources(resources)
            CheckLatestDBResourceInfo(resources)

            RaiseEvent ResourceStatusChanged(Me, New ResourcesChangedEventArgs(ResourceStatusChange.EnvironmentChange, Nothing))
        End Sub

        Public Function CheckInstructionalClientStatus() As Boolean Implements IResourceConnectionManager.CheckInstructionalClientStatus

            Try
                mInstructionalClientController.EnsureConnected()
            Catch ex As InvalidInstructionalConnectionException
                Dim failedOperation As New FailedCallbackOperationEventArgs(GetAscrDocsMessage(),
                    My.Resources.CallbackASCRChannel_ChannelErrorTitle)
                RaiseEvent OnCallbackError(failedOperation)
            End Try

        End Function

        Private Sub UpdateLastConnectionStatistics(resourceInfos As ICollection(Of ResourceInfo))
            For Each resourceInfo As ResourceInfo In resourceInfos
                If resourceInfo.ID <> mLocalResource?.ResourceId Then
                    Dim resourceStatistics = resourceInfo.LastConnectionStatistics

                    If resourceStatistics IsNot Nothing Then
                        resourceInfo.LastConnectionStatus = FormatLastConnectionStatus(resourceInfo, resourceStatistics)
                    End If
                End If
            Next
        End Sub

        Private Function FormatLastConnectionStatus(resourceInfo As ResourceInfo, resourceStatistics As ResourceConnectionStatistic) As String
            Dim lastConnectionStatus As String

            If resourceInfo.DisplayStatus = ResourceStatus.Missing _
               OrElse resourceInfo.DisplayStatus = ResourceStatus.Offline _
               OrElse resourceInfo.DisplayStatus = ResourceStatus.LoggedOut _
               OrElse resourceStatistics.ConnectionSuccess Then
                lastConnectionStatus = String.Format(If(resourceStatistics.ConnectionSuccess,
                                                        BluePrism.Core.Properties.Resource.OnDemandResourceConnectionSucceeded,
                                                        BluePrism.Core.Properties.Resource.OnDemandResourceConnectionFailed),
                                                     resourceInfo.Name,
                                                     resourceStatistics.LastConnected.ToLocalTime(),
                                                     resourceStatistics.LastPing)
            Else
                lastConnectionStatus = String.Format(BluePrism.Core.Properties.Resource.OnDemandResourceConnectionCannotConnectToAppServer,
                                                     resourceInfo.Name)
            End If

            Return lastConnectionStatus
        End Function


        Public Sub GetLatestDBResourceInfo() Implements IResourceConnectionManager.GetLatestDBResourceInfo
            ' Don't have to do anything here.
        End Sub

        Public Sub GetLatestDBResourceInfo(excludedResources As ResourceAttribute) Implements IResourceConnectionManager.GetLatestDBResourceInfo
            ' Don't have to do anything here.
        End Sub

        Public Function GetActiveResources(connectedOnly As Boolean) As List(Of IResourceMachine) Implements IResourceConnectionManager.GetActiveResources
            Return mResourceMachines.Values.Where(Function(r) r.DisplayStatus = ResourceStatus.Idle AndAlso r.ProcessesRunning = 0 AndAlso r.ConnectionState = ResourceConnectionState.Server).ToList()
        End Function

        Public Function GetControllingResource(name As String) As IResourceMachine Implements IResourceConnectionManager.GetControllingResource
            Return mResourceMachines.Values.FirstOrDefault(Function(r) r.HasPoolMember(name))
        End Function

        Public Function GetResource(resourceId As Guid) As IResourceMachine Implements IResourceConnectionManager.GetResource
            'other implementations use try/catch return nothing.
            Dim resource As IResourceMachine = Nothing
            mResourceMachines.TryGetValue(resourceId, resource)
            Return resource
        End Function

        Public Function GetResource(name As String) As IResourceMachine Implements IResourceConnectionManager.GetResource
            Dim mach As IResourceMachine = Nothing
            mResourceMachineNameMap.TryGetValue(name, mach)
            Return mach
        End Function

        Public Function GetResources() As Dictionary(Of Guid, IResourceMachine) Implements IResourceConnectionManager.GetResources
            Return GetResources(False)
        End Function

        Public Function GetResources(connectedOnly As Boolean) As Dictionary(Of Guid, IResourceMachine) Implements IResourceConnectionManager.GetResources
            Return mResourceMachines.
            Where(Function(x) x.Value.IsConnected OrElse Not connectedOnly).
            ToDictionary(Function(x) x.Key, Function(y) y.Value)
        End Function

        Public Function SendCreateSession(sessionData As IEnumerable(Of CreateSessionData)) As Guid() Implements IResourceConnectionManager.SendCreateSession
            Dim resultingSessionIds As New List(Of Guid)
            Dim nonLocalSessions = sessionData

            If mLocalResource IsNot Nothing Then
                Dim localResourceId = mLocalResource.ResourceId
                Dim localSessions = sessionData.Where(Function(s) s.ResourceId = localResourceId)

                For Each s In localSessions
                    mLog.Debug("Send: CreateSession {resource}", New With {s.ResourceId, s.ProcessId})
                    resultingSessionIds.Add(mLocalResource.SendCreateSession(s))
                    mLocalResource.RefreshResource()
                Next

                nonLocalSessions = sessionData.Where(Function(s) s.ResourceId <> localResourceId)
            Else
                nonLocalSessions = sessionData
            End If

            If nonLocalSessions.Any() Then
                If mPrivateResourcesThatAreNotMine.Intersect(nonLocalSessions.Select(Function(s) s.ResourceId)).Any() Then _
                    Throw New PermissionException(My.Resources.ConnectionManager_PrivateRobotBelongingToAnotherUser)

                CheckInstructionalClientStatus()
                resultingSessionIds.AddRange(mServer.DoCreateSessionCommand(nonLocalSessions.ToList))
            End If

            Return resultingSessionIds.ToArray()
        End Function

        Public Sub SendDeleteSession(sessions As IEnumerable(Of DeleteSessionData)) Implements IResourceConnectionManager.SendDeleteSession
            Dim remoteSessions = sessions
            If mLocalResource IsNot Nothing Then
                Dim localSessions = sessions.Where(Function(session) session.ResourceId = mLocalResource.ResourceId)
                If localSessions.Any() Then
                    remoteSessions = sessions.Where(Function(session) session.ResourceId <> mLocalResource.ResourceId)
                    localSessions.ForEach(Sub(session)
                        mLog.Debug("Send: StartSession {resource}", New With {session.ResourceId, session.SessionId, session.ProcessId})
                        mLocalResource.SendDeleteSession(session)
                        mLocalResource.RefreshResource()
                    End Sub).Evaluate()
                End If

            End If

            If mPrivateResourcesThatAreNotMine.Intersect(remoteSessions.Select(Function(x) x.ResourceId)).Any() Then _
                Throw New PermissionException(My.Resources.ConnectionManager_PrivateRobotBelongingToAnotherUser)

            CheckInstructionalClientStatus()
            mServer.DoDeleteSessionCommand(remoteSessions.ToList)
        End Sub

        Public Sub SendStartSession(sessions As IEnumerable(Of StartSessionData)) Implements IResourceConnectionManager.SendStartSession
            Dim remoteSessions = sessions
            If mLocalResource IsNot Nothing Then

                Dim localSessions = sessions.Where(Function(session) session.ResourceId = mLocalResource.ResourceId)
                If localSessions.Any() Then
                    remoteSessions = sessions.Where(Function(session) session.ResourceId <> mLocalResource.ResourceId)
                    localSessions.ForEach(Sub(session)
                        mLog.Debug("Send: StartSession {resource}", New With {session.ResourceId, session.SessionId, session.ProcessId, session.InputParametersXML})
                        mLocalResource.SendStartSession(session)
                        mLocalResource.RefreshResource()
                    End Sub).Evaluate()
                End If
            End If

            If mPrivateResourcesThatAreNotMine.Intersect(remoteSessions.Select(Function(x) x.ResourceId)).Any() Then _
                Throw New PermissionException(My.Resources.ConnectionManager_PrivateRobotBelongingToAnotherUser)

            CheckInstructionalClientStatus()
            mServer.DoStartSessionCommand(remoteSessions.ToList)
        End Sub

        Public Sub SendSendGetVariable(resourceId As Guid, sessionID As Guid, processId As Guid, ByRef err As String) Implements IResourceConnectionManager.SendGetSessionVariables
            mLog.Debug("Send: GetGetVariable {resource}", New With {sessionID})

            If resourceId = mLocalResource?.ResourceId Then
                mLocalResource.SendGetVariable(sessionID, err)
                mLocalResource.RefreshResource()
            Else
                CheckInstructionalClientStatus()
                mServer.DoSendGetSessionVariables(resourceId, sessionID, processId)
            End If
        End Sub

        Public Sub SendStopSession(sessions As IEnumerable(Of StopSessionData)) Implements IResourceConnectionManager.SendStopSession
            Dim remoteSessions As IEnumerable(Of StopSessionData)
            If mLocalResource IsNot Nothing Then
                sessions.Where(Function(f) f.ResourceId = mLocalResource.ResourceId).ForEach(Sub(session)
                                                                                                 mLog.Debug("Send: StopSession {info}", New With {session.ResourceId, session.SessionId})
                                                                                                 mLocalResource.SendStopSession(session)
                                                                                                 mLocalResource.RefreshResource()
                                                                                             End Sub).Evaluate()
                remoteSessions = sessions.Where(Function(f) f.ResourceId <> mLocalResource.ResourceId)
            Else
                remoteSessions = sessions
            End If

            If mPrivateResourcesThatAreNotMine.Intersect(remoteSessions.Select(Function(x) x.ResourceId)).Any() Then _
                Throw New PermissionException(My.Resources.ConnectionManager_PrivateRobotBelongingToAnotherUser)

            remoteSessions.ForEach(Sub(session) mLog.Debug("Send: StopSession {info}", New With {session.ResourceId, session.SessionId})).Evaluate()

            CheckInstructionalClientStatus()
            mServer.SendStopSession(sessions.ToList)
        End Sub


        Public Sub SendSetSessionVariable(resourceID As Guid, vars As Queue(Of clsSessionVariable)) Implements IResourceConnectionManager.SendSetSessionVariable
            If resourceID = mLocalResource?.ResourceId Then
                mLocalResource.SendSetSessionVariable(vars)
                mLocalResource.RefreshResource()
            Else
                If mPrivateResourcesThatAreNotMine.Contains(resourceID) Then _
                Throw New PermissionException(My.Resources.ConnectionManager_PrivateRobotBelongingToAnotherUser)

                Dim sessionVars As String = ""
                For Each var In vars
                    sessionVars += var.ToEscapedString(False)
                    If (Not var.Equals(vars.Last)) Then sessionVars += ","
                Next
                CheckInstructionalClientStatus()
                mServer.SendSetSessionVariable(resourceID, vars.First.SessionID, sessionVars)
            End If
        End Sub

        Public Sub ToggleShowSessionVariables(monitorSessionVars As Boolean) Implements IResourceConnectionManager.ToggleShowSessionVariables
            mServer.ToggleShowSessionVariables(monitorSessionVars)
        End Sub

        Public Function TryGetNextUserMessage(ByRef msg As String) As Boolean Implements IResourceConnectionManager.TryGetNextUserMessage
            ' ASCR now displays user messages from events and not a queue
            Return False
        End Function

        Public Property IsDisposed As Boolean Implements IResourceConnectionManager.IsDisposed

        Public ReadOnly Property MaxRefreshInterval As TimeSpan Implements IResourceConnectionManager.MaxRefreshInterval
            Get
                Return New TimeSpan(0, 0, 15)
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            mTokenSource?.Cancel()
            mInstructionalClientController.DeRegister()
            mInstructionalClientController.Dispose()
            mInstructionalClientController = Nothing
            IsDisposed = True
        End Sub

        Private Sub UpdateLocalResources()
            If mLocalResource Is Nothing Then
                Dim localResource = mServer.GetResourceInfo(ResourceAttribute.DefaultInstance, ResourceAttribute.None, Dns.GetHostName()).FirstOrDefault
                If localResource IsNot Nothing Then
                    With localResource
                        mLocalResource = New OnDemandConnection(.ID, .Name, .Status, .Attributes, .UserID, Me, 0, mPingTimeoutSeconds, mConnectionPingCooldown, mProcessResourceInputSleepTime)
                    End With
                End If
            End If
        End Sub
        Public Sub RefreshResourceConnection(resourceId As Guid) Implements IResourceConnectionManager.RefreshResourceConnection
            ResourceRefresh()
            mNextRefresh = Date.UtcNow.AddSeconds(10)
        End Sub

        Private Sub UpdatePrivateResources(resources As IEnumerable(Of ResourceInfo))
            If (resources IsNot Nothing) Then
                Dim myUserId = User.Current.Id
                Dim count = mPrivateResourcesThatAreNotMine.Count
                mPrivateResourcesThatAreNotMine = New HashSet(Of Guid)(resources.Where(Function(x) x.Attributes.HasFlag(ResourceAttribute.Private) AndAlso Not x.Attributes.HasFlag(ResourceAttribute.DefaultInstance) AndAlso
                                                                                              x.UserID <> myUserId).Select(Function(y) y.ID))
                Dim change = mPrivateResourcesThatAreNotMine.Count - count
                mLog.Debug("Updated private resource.  Change = {change}", New With {change})
            End If
        End Sub

        Private Sub CheckLatestDBResourceInfo(resources As ICollection(Of ResourceInfo))
            Dim poolmachines As New Dictionary(Of Guid, List(Of IResourceMachine))
            If resources Is Nothing Then Return

            ' build Lookup table 
            Dim lookup = New HashSet(Of Guid)(resources.Select(Function(x) x.ID))
            Dim selectedRobots = mResourceMachines.Where(Function(x) lookup.Contains(x.Key))
            mResourceMachines = selectedRobots.ToDictionary(Function(x) x.Key, Function(a) a.Value)
            mResourceMachineNameMap = selectedRobots.ToDictionary(Function(a) a.Value.Name, Function(b) b.Value)

            For Each r As ResourceInfo In resources

                Dim inPool As Boolean = r.Pool <> Nothing
                Dim usethisone As Boolean = Not inPool

                ' If this is hidden to us, let's not display it
                If Not usethisone Then
                    mResourceMachines.Remove(r.ID)
                    mResourceMachineNameMap.Remove(r.Name)
                End If

                Dim mach As IResourceMachine = Nothing
                If mResourceMachines.ContainsKey(r.ID) Then
                    mach = mResourceMachines(r.ID)
                Else
                    If usethisone Then
                        ' Create a connection to a newly added resource...
                        If Not r.Attributes.HasAnyFlag(ResourceAttribute.Debug Or ResourceAttribute.Retired) Then

                            mach = New ServerConnectedResourceMachine(ResourceConnectionState.Server, r.Name, r.ID, r.Attributes)
                            mResourceMachines.Add(r.ID, mach)
                            mResourceMachineNameMap.Add(r.Name, mach)
                        Else
                            mLog.Trace("Ignoring resource {Resource}.  Resource is Debug or Retired", New With {r.Name, r.ID})
                        End If
                    Else
                        mLog.Debug("Creating Server Connected Resource Machine {resource}", New With {Key .Name = r.Name, Key .Id = r.ID})
                        mach = New ServerConnectedResourceMachine(ResourceConnectionState.Connecting, r.Name, r.ID, r.Attributes)
                        If Not poolmachines.ContainsKey(r.Pool) Then
                            mLog.Debug("Adding pool {pool}", New With {r.Pool})
                            poolmachines.Add(r.Pool, New List(Of IResourceMachine))
                        End If
                        mLog.Debug("Adding machine to {pool}", New With {Key .PoolId = r.Pool, Key .MachineId = mach.Id})
                        poolmachines(r.Pool).Add(mach)
                    End If
                End If

                If mach IsNot Nothing Then
                    mLog.Trace("Updating ResourceMachine {Status}", New With {Key .ResourceId = r.ID, r.Status, r.Attributes, r.DisplayStatus, r.Information, inPool})
                    'Update resource information from server
                    mach.DBStatus = r.Status
                    mach.Attributes = r.Attributes
                    mach.DisplayStatus = r.DisplayStatus
                    mach.Info = r.Information
                    mach.InfoColour = Color.FromArgb(r.InfoColour)
                    mach.IsInPool = inPool
                    mach.LastError = r.LastConnectionStatus

                    If r.LastConnectionStatistics IsNot Nothing
                        mach.SuccessfullyConnectedToAppServer = r.LastConnectionStatistics.ConnectionSuccess
                    End If

                End If
            Next

            ' Update Child details of all the resource machines (Only Pools have children)
            For Each resourceConnection As KeyValuePair(Of Guid, IResourceMachine) In mResourceMachines
                If poolmachines.ContainsKey(resourceConnection.Key) Then
                    resourceConnection.Value.ChildResources = poolmachines(resourceConnection.Key)
                Else
                    resourceConnection.Value.ChildResources = Nothing
                End If
            Next
        End Sub

        Private Sub OnSessionCreate(sender As Object, e As SessionCreateEventArgs) _
            Handles mLocalResource.SessionCreate, mInstructionalClientController.SessionCreated
            Try
                If Not e.Success Then
                    CallbackUserMessage(e.ErrorMessage)
                End If
                RaiseEvent SessionCreate(Me, e)
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseCreateSessionEvent)
            End Try
        End Sub

        Private Sub OnSessionDelete(sender As Object, e As SessionDeleteEventArgs) _
            Handles mLocalResource.SessionDelete, mInstructionalClientController.SessionDeleted
            Try
                If Not e.Success Then
                    CallbackUserMessage(e.ErrorMessage)
                Else
                    RaiseEvent SessionDelete(Me, e)
                End If
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseDeleteSessionEvent)
            End Try
        End Sub

        Private Sub OnSessionStart(sender As Object, e As SessionStartEventArgs) _
            Handles mLocalResource.SessionStart, mInstructionalClientController.SessionStarted
            Try
                If e.PendingUserMessage Then
                    CallbackUserMessage(e.UserMessage)
                Else
                    RaiseEvent SessionStart(Me, e)
                End If
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseStartSessionEvent)
            End Try
        End Sub

        Private Sub OnSessionStop(sender As Object, e As SessionStopEventArgs) _
            Handles mLocalResource.SessionStop, mInstructionalClientController.SessionStop
            Try
                If Not e.Success Then
                    CallbackUserMessage(e.ErrorMessage)
                Else
                    RaiseEvent SessionStop(Me, e)
                End If
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseStopSessionEvent)
            End Try
        End Sub

        Private Sub OnSessionVariableChanged(sv As clsSessionVariable) Handles mLocalResource.SessionVariableChanged
            Try
                Dim key As String = sv.SessionID.ToString() & "." & sv.Name
                mSessionVariables(key) = sv
                RaiseEvent SessionVariablesUpdated(Me, New SessionVariableUpdatedEventArgs(String.Empty))
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseVarUpdateSessionEvent)
            End Try
        End Sub

        Private Sub OnSessionVariableChanged(sender As Object, e As SessionVariableUpdatedEventArgs) _
            Handles mInstructionalClientController.SessionVariableUpdated
            Try
                If Not e.Success Then
                    CallbackUserMessage(e.ErrorMessage)
                Else
                    Dim sv As clsSessionVariable = ObjectEncoder.Base64StringToObject(Of clsSessionVariable)(e.JSONData)
                    Dim key = sv.SessionID.ToString() & "." & sv.Name
                    mSessionVariables(key) = sv
                    RaiseEvent SessionVariablesUpdated(Me, New SessionVariableUpdatedEventArgs(String.Empty))
                End If
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseVarUpdateSessionEvent)
            End Try
        End Sub

        Private Sub OnSessionEnd(sender As Object, e As SessionEndEventArgs) _
            Handles mLocalResource.SessionEnd, mInstructionalClientController.SessionEnd
            Try
                If MonitorSessionVariables Then
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
                    If keysToRemove.Count > 0 Then RaiseEvent SessionVariablesUpdated(Me, New SessionVariableUpdatedEventArgs(String.Empty))
                End If

                RaiseEvent SessionEnd(Me, e)
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseEndSessionEvent)
            End Try
        End Sub

        Private Sub OnResourceStatusChanged(sender As Object, e As ResourcesChangedEventArgs) _
            Handles mLocalResource.ResourceStatusChanged, mInstructionalClientController.ResourceStatus
            Try
                RaiseEvent ResourceStatusChanged(Me, e)
            Catch ex As Exception
                mLog.Error(ex, My.Resources.ConnectionManager_FailedToRaiseEndSessionEvent)
            End Try
        End Sub

        Private Sub CallbackUserMessage(e As String)
            RaiseEvent ShowUserMessage(Me, e)
        End Sub

        Private Sub ClientController_ErrorReceived(e As FailedCallbackOperationEventArgs) Handles mInstructionalClientController.ErrorReceived
            Dim argsWithDocs as New FailedCallbackOperationEventArgs(
                $"{e.Message}{Environment.NewLine}{GetAscrDocsMessage()}",
                e.Error)
            RaiseEvent OnCallbackError(argsWithDocs)
        End Sub

        Private Function GetAscrDocsMessage() As String
            Dim url As String = String.Format(My.Resources.ASCRDocumentationURL, AutomateControls.HelpLauncher.GetBPVersion(), AutomateControls.HelpLauncher.GetHelpDocumentationCulture())
            Return String.Format(My.Resources.CallbackASCRChannel_ChannelError, url)
        End Function


    End Class

End Namespace
