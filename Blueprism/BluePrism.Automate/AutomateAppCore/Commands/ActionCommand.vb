Imports BluePrism.AutomateAppCore.Auth

Namespace Commands

    ''' <summary>
    ''' The 'action' command
    ''' </summary>
    Public Class ActionCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return "action"
            End Get
        End Property

        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.ActionCommand_EitherActionSessionidActionnameToRunAnActionOnTheSpecifiedSessionOrActionLastAc
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

            Dim suffix As String = command.Substring(7)
            Dim ind As Integer = suffix.IndexOf(" ", StringComparison.Ordinal)
            If ind = -1 Then Return "MISSING ACTION NAME"

            Dim action As String = suffix.Substring(ind + 1)
            Dim session As String = suffix.Substring(0, ind)

            Dim sessId As Guid
            If session.ToLower() = "last" _
                Then sessId = client.LastSessionCreated _
                Else sessId = GetNearestSessionID(session)

            If sessId = Nothing Then Return "INVALID ID"

            Dim runnerRecord As RunnerRecord = listener.FindRunner(sessId)
            If runnerRecord Is Nothing Then Return "MISSING RUNNER"

            If Not runnerRecord.IsIdle Then Return "WRONG STATUS - " & runnerRecord.Status.ToString()

            runnerRecord.mStartupParams = client.StartupParameters
            runnerRecord.mAction = action
            runnerRecord.StartThread()
            Return "STARTED"

        End Function

    End Class
End Namespace