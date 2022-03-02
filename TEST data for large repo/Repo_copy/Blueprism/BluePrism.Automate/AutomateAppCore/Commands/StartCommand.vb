Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Namespace Commands

    ''' <summary>
    ''' The 'start' command
    ''' </summary>
    Public Class StartCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "start"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.StartCommand_EitherStartSessionidToStartASessionRunningOrStartLastToStartTheLastSessionCreat
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

            Dim session As String = command.Mid(7)
            Dim sessId As Guid
            If session.ToLower() = "last" _
                Then sessId = Client.LastSessionCreated _
                Else sessId = GetNearestSessionID(session)
            If sessId = Nothing Then Return "INVALID ID"

            Return StartSession(Nothing, sessId)

        End Function

        Protected Function StartSession(token As clsAuthToken, sessionId As Guid) As String
            'If the listener is a controller of a pool, we need to check if the
            'session is being handled on another resource, and if so start tell
            'that resource to start it...
            If Listener.IsController Then
                Dim sessionResourceId As Guid = gSv.GetSessionResourceID(sessionId)
                If sessionResourceId <> Listener.ResourceId Then
                    Dim errorCode As String = String.Empty
                    Dim startCmd As String
                    If token IsNot Nothing Then
                        startCmd = $"startas {token} {sessionId}"
                    Else
                        startCmd = $"start {sessionId}"
                    End If
                    Dim resMachine = Listener.ResourceConnections.GetResource(sessionResourceId)
                    Dim startupParams = If(Client.StartupParameters IsNot Nothing, Client.StartupParametersXml, Nothing)
                    Dim reply As String = clsTalker.PoolControllerProxyCommand(resMachine,
                                                                               Client.AuthenticatedUser,
                                                                               Listener.PoolId,
                                                                               startCmd,
                                                                               "STARTED",
                                                                               30000,
                                                                               startupParams,
                                                                               False,
                                                                               errorCode)

                    return If(reply, errorCode)
                End If
            End If

            Dim nr As RunnerRecord = Listener.FindRunner(sessionId)
            If nr Is Nothing Then Return "MISSING RUNNER"

            If Not nr.IsPending Then Return "NOT PENDING"
            nr.mStartupParams = Client.StartupParameters

            Try
                If gSv.GetControllingUserPermissionSetting Then
                    gSv.SetProcessStartParamsAs(token?.ToString(), sessionId, nr.mStartupParams?.ArgumentsToXML(False, Nothing))
                Else
                    gSv.SetProcessStartParams(sessionId, nr.mStartupParams?.ArgumentsToXML(False, Nothing))
                End If

            Catch ex As Exception
                Return "ERROR:" & ex.Message
            End Try
            nr.mAction = Nothing
            nr.StartThread()
            Listener.AddNotification("START {0}", sessionId)
            Return "STARTED"

        End Function

    End Class
End Namespace
