Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Namespace Commands

    ''' <summary>
    ''' The 'create' command
    ''' </summary>
    Public Class CreateCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return "create"
            End Get
        End Property

        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.CreateCommand_EitherCreateProcidToCreatePendingSessionWithGivenProcessIdCreateProcidQueueiden
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property


        ''' <summary>
        ''' Validates that the given user has permission to perform this action on the give resource.
        ''' </summary>
        ''' <param name="authUser">the requesting user</param>
        ''' <param name="resourceId">the identity of the resource</param>
        ''' <returns></returns>
        Public Overrides Function CheckPermissions(authUser As IUser, resourceId As Guid) As (String, Boolean)
            If CheckUserHasPermissionOnResource(authUser, resourceId, Permission.Resources.ControlResource) Then
                Return ("", True)
            Else
                Return ("USER DOES NOT HAVE CONTROL RESOURCE PERMISSION FOR THIS RESOURCE", False)
            End If
        End Function
        Protected Overrides Function Exec(command As String) As String
            Dim output As String
            Try

                'Reset the last session created, so there isn't one if this fails.
                Client.LastSessionCreated = Guid.Empty
                'A user must be set before you can create a session...
                If Not Client.UserSet Then
                    output = "USER NOT SET" & vbCrLf
                    Return output
                End If
                'Get the process id, and make sure it is valid...
                Dim procId As Guid
                Dim procName As String
                Dim queueIdent = 0
                Dim sessionId As Guid

                If command.StartsWith("create name ") Then
                    procName = command.Substring(12)
                    procId = gSv.GetProcessIDByName(procName, True)
                    If procId.Equals(Guid.Empty) Then
                        output = "NO SUCH PROCESS" & vbCrLf
                        Return output
                    End If
                Else
                    'create {qi.ProcessId} {qi.QueueIdent} | create {procID} {queueIdent} {sessionId}
                    Dim ids() As String = command.Mid(8).Split(" "c)
                    If ids.Count = 0 Then Return "INVALID ID"

                    procId = GetNearestProcID(ids(0))
                    If procId = Nothing Then Return "INVALID ID"

                    If ids.Count = 2 Then
                        If Not Integer.TryParse(ids(1), queueIdent) Then
                            Return "INVALID QUEUE IDENTITY"
                        End If
                    ElseIf ids.Count > 2 Then
                        If Not Integer.TryParse(ids(1), queueIdent) Then
                            Return "INVALID QUEUE IDENTITY"
                        End If

                        If Not Guid.TryParse(ids(2), sessionID) Then
                            Return "INVALID SESSION IDENTITY"
                        End If
                    End If

                End If

                'Ensure we have a valid session guid
                If sessionId = Guid.Empty Then
                    sessionId = Guid.NewGuid
                End If

                Return CreateSession(Nothing, procId, queueIdent, sessionId)

            Catch ex As Exception
                output = ex.Message
                output &= vbCrLf
                Return output
            End Try

        End Function

        Protected Overridable Function CreateSession(token As clsAuthToken, procID As Guid, queueIdent As Integer, sessionId As Guid) As String
            Dim output As String
            'check for retired processes
            Dim attr As ProcessAttributes
            Try
                attr = gSv.GetProcessAttributes(procID)
            Catch ex As Exception
                output = "DATABASE ERROR" & " - " & ex.Message
                output &= vbCrLf
                Return output
            End Try
            If attr.HasFlag(ProcessAttributes.Retired) Then
                output = "CANNOT RUN RETIRED PROCESSES"
                output &= vbCrLf
                Return output
            End If

            'If the listener is a controller of a pool, we'll try and run the
            'process elsewhere first
            Dim sessionCreatedByPool = False
            If Listener.IsController Then
                Dim res As IEnumerable(Of IResourceMachine) =
                        Listener.ResourceConnections.GetResources(True).Values.OrderBy(
                            Function(m) m.ProcessesPending + m.ProcessesRunning)
                For Each m As ResourceMachine In res
                    Dim errcode As String = Nothing
                    Dim createCmd As String
                    If token IsNot Nothing Then
                        createCmd = $"createas {token.ToString} {procID} {queueIdent} {sessionId}"
                    Else
                        createCmd = $"create {procID} {queueIdent} {sessionId}"
                    End If
                    Dim reply As String = clsTalker.PoolControllerProxyCommand(m, Client.AuthenticatedUser, Listener.PoolId, createCmd, "SESSION CREATED", 30000, Nothing, False, errcode)
                    If reply IsNot Nothing Then
                        sessionCreatedByPool = True
                        Exit For
                    End If
                Next
            End If

            If Not sessionCreatedByPool Then
                Try
                    'Note that we will pass our own resource ID for both
                    'the starter and runner PC, since we don't have a
                    'resource ID for the PC that initiated the process.
                    'The process is not technically initiated by a resource
                    'PC. The user information determines who started the
                    'process.
                    Dim runnerRequest = New RunnerRequest() With {
                            .SessionId = sessionId,
                            .ProcessId = procID,
                            .StarterUserId = Client.UserId,
                            .StarterResourceId = Listener.ResourceId,
                            .RunningResourceId = Listener.ResourceId,
                            .QueueIdent = queueIdent,
                            .AutoInstance = False,
                            .AuthorisationToken = token}

                    Listener.CreateRunner(runnerRequest)
                Catch ex As Exception
                    output = ex.Message
                    output &= vbCrLf
                    Return output
                End Try
            End If

            Listener.AddNotification("CREATED " & sessionId.ToString())
            Listener.NotifyStatus()
            output = "SESSION CREATED : " & sessionId.ToString()
            output &= vbCrLf
            Client.LastSessionCreated = sessionId
            Return output
        End Function

    End Class
End NameSpace
