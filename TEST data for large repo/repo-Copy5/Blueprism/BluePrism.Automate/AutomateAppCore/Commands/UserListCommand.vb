Imports BluePrism.AutomateAppCore.Auth

Namespace Commands

    ''' <summary>
    ''' The 'userlist' command
    ''' </summary>
    Public Class UserListCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "userlist"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.UserListCommand_ListAllUsersUsersAreDisplayedOnePerLineInTheFollowingFormatXxxxxxxxXxxxXxxxXxxx
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            Dim sb As New StringBuilder()

            For Each user In gSv.GetAllUsers
                If Not user.IsHidden AndAlso Not user.Deleted Then
                    sb.Append(user.Id).Append(" - ").Append(user.Name) _
                        .AppendLine()
                End If
            Next
            Return sb.ToString()
        End Function

    End Class
End Namespace