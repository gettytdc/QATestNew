Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.ClientServerResources.Core.Enums

Namespace Commands

    ''' <summary>
    ''' The 'stop' command
    ''' </summary>
    Public Class StopCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "stop"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.StopCommand_UseStopSessionidToStopProcessRunningOptionalExtraParametersUseridResourceidMayB
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

            Dim splitten As String() = Split(command)

            Dim sessId As Guid = GetNearestSessionID(splitten(1))
            If sessId = Nothing Then Return "INVALID ID"

            Dim userId As Guid
            Dim userName As String = ""
            Dim resName As String = ""

            'get the user id and resource id if they are specified
            If splitten.Length > 2 Then
                userId = GetNearestUserID(splitten(2))
                If userId = Nothing Then _
                    Return "INVALID USER ID - " & splitten(2)

                'There shouldn't be really be any reason why this user ID is
                'being passed as a parameter in the first place, because you
                'need to be authenticated to do the stop command. It's here
                'for historical reasons though. Until it's removed, we'll just
                'make sure that *if* an ID is passed, it's the right one!
                If userId <> Client.UserId Then Return "BAD USER ID"

                userName = gSv.GetUserName(userId)

                'if we are given user id then we also expect resource id
                If splitten.Length <= 3 Then Return "MISSING RESOURCE - " &
                                                    "resource must be specified when user id is specified."

                If splitten(3) = "name" Then
                    If splitten.Length <= 4 Then Return "MISSING RESOURCE NAME - " &
                                                        "resource name must be specified."

                    'Remove the 4 preceeding parts plus the 4 spaces which are added when the command is built
                    resName = command.Remove(0, splitten(0).Length + splitten(1).Length + splitten(2).Length + splitten(3).Length + 4)
                Else
                    Dim resId = GetNearestResourceID(splitten(3))
                    If resId = Nothing Then Return "INVALID RESOURCE ID - " & splitten(3)

                    resName = gSv.GetResourceName(resId)
                End If
            End If

            'If the listener is a controller of a pool, we need to check if the
            'session is being handled on another resource, and if so tell that
            'resource to stop it...
            If Listener.IsController Then
                Dim resourceId = gSv.GetSessionResourceID(sessId)
                If resourceId <> Listener.ResourceId Then
                    Dim machine = Listener.ResourceConnections.GetResource(resourceId)
                    Dim errorCode As String = Nothing
                    Dim reply = clsTalker.PoolControllerProxyCommand(
                        machine, Client.AuthenticatedUser, Listener.PoolId, "stop " & sessId.ToString(),
                        "STOPPING", 30000, Nothing, False, errorCode)
                    If reply Is Nothing Then
                        Return errorCode
                    End If
                    Return reply
                End If
            End If


            SyncLock Listener.Runners.Lock
                Dim r As RunnerRecord = Listener.FindRunner(sessId)
                If r Is Nothing Then Return "SESSION NOT FOUND"

                'You can't stop a pending session. Maybe you wanted to delete it?
                If r.IsPending Then _
                    Return "BAD STATUS - Can't stop a pending session"

                r.Status = RunnerStatus.STOPPING
                r.ImmediateStopRequested = True
                If r.Process IsNot Nothing Then r.StopProcess(userName, resName)

                Listener.AddNotification("STOPPING {0}", sessId)
                Return "STOPPING"
            End SyncLock

        End Function

    End Class
End Namespace
