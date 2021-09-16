Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Namespace Commands

    ''' <summary>
    ''' The 'varupdates' command
    ''' </summary>
    Public Class VarUpdatesCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer,
                       memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "varupdates"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.VarUpdatesCommand_EnableOrDisableSessionVariableUpdatesSpecifyEitherOnOrOffTheResponseIsSETOrAnEr
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

            If command.Length <= Name.Length Then Return "MISSING PARAMETER"

            Dim enabled As Boolean
            Select Case command.Substring(Name.Length + 1)
                Case "on" : enabled = True
                Case "off" : enabled = False
                Case Else
                    Throw New InvalidOperationException("Specify either 'on' or 'off'")
            End Select
            Client.SendSessionVariableUpdates = True

            'Send 'updates' for all known session variables at this point, so the
            'client has a complete picture to start with - otherwise getting
            'updates is kind of pointless!
            Dim sb As StringBuilder = New StringBuilder()
            SyncLock Listener.Runners.Lock
                For Each rr In Listener.Runners
                    Dim session As clsSession = rr.Session
                    If session Is Nothing Then Continue For
                    Dim allvars As Dictionary(Of String, clsProcessValue) =
                            session.GetAllVars()
                    For Each vname As String In allvars.Keys
                        Dim sv As New clsSessionVariable(vname, allvars(vname))
                        sb.AppendFormat(">>VAR {0} {1}", session.ID, sv).AppendLine()
                    Next
                Next

                sb.Append("SET")
                Return sb.ToString()
            End SyncLock

        End Function

    End Class
End Namespace