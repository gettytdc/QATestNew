Imports BluePrism.AutomateAppCore.Auth

Namespace Commands

    ''' <summary>
    ''' The 'shutdown' command
    ''' </summary>
    Public Class ShutdownCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "shutdown"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.ShutdownCommand_ShutDownTheResourcePCUseOptionalParameterWaitforsessionsToWaitForAnyCurrentlyRu
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.AuthedOrLocal
            End Get
        End Property

        Public Overrides ReadOnly Property ValidRunStates As IEnumerable(Of ResourcePcRunState)
            Get
                Return AllRunStates
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

            'If the request is for Login Agent shutdown, ensure that this resource IS the Login Agent
            Dim args As String() = Split(command)
            Dim loginAgent As Boolean = args.Contains("loginagent", StringComparer.OrdinalIgnoreCase)
            Dim waitForSessions As Boolean = args.Contains("waitforsessions", StringComparer.OrdinalIgnoreCase)
            If loginAgent AndAlso Not Listener.IsLoginAgent Then
                Return "No."
            End If

            Dim response As String
            If (Listener.RunState = ResourcePcRunState.Running) Then
                Dim reason = String.Format("Shutting down in response to shutdown command from {0}",
                                           Client.RemoteAddressFriendlyString)
                Listener.InitiateShutdown(reason, waitForSessions)
                response = "OK, shutting down..."
            Else
                response = "OK, already shutting down"
                If (Listener.RunState = ResourcePcRunState.WaitingToStop) Then
                    response += " (waiting for running sessions)"
                End If
            End If
            Return response
        End Function

    End Class
End Namespace