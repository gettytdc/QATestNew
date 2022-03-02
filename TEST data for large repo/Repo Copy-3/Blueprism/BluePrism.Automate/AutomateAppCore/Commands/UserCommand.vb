Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Namespace Commands

    ''' <summary>
    ''' The 'user' command
    ''' </summary>
    Public Class UserCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "user"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.UserCommand_StartUsernamePasswordBasedAuthenticationUseEitherUserUseridToSetUserIdOrUserNam
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Any
            End Get
        End Property

        Private _findUserPrincipalName As Func(Of String, String) = Function(userName As String) clsActiveDirectory.GetUserPrincipalName(username, Server.GetActiveDirectoryDomain())

        Protected Overrides Function Exec(command As String) As String

            Dim userId As Guid
            Dim name = String.Empty

            Client.RequestedAuthenticationMode = AuthMode.Unspecified

            If command.StartsWith("user name ") Then
                Dim username = command.Substring(10)

                If Server.DatabaseType() = DatabaseType.SingleSignOn Then
                    Try
                        name = _findUserPrincipalName(userName)
                    Catch ex As Exception
                        ' Catch the exception but allow the user to continue and 
                        ' enter the password before we inform them
                    End Try
                Else
                    userId = Server.TryGetUserID(username)
                    name = Server.GetUserName(userId)
                End If
            ElseIf command.StartsWith("user upn ") Then
                If Server.DatabaseType() = DatabaseType.NativeAndExternal Then
                    Dim userPrincipalName = command.Substring(9)
                    name = userPrincipalName
                    Client.RequestedAuthenticationMode = AuthMode.MappedActiveDirectory                
                End If
            Else
                userId = GetNearestUserID(command.Mid(6))
                name = Server.GetUserName(userId)
            End If
                        
            Client.RequestedUserName = name
            Client.UserRequested = True

            ' At this point the user name may not be valid, but we return the same message
            ' and only inform of failed authentication after the password has also been entered
            Return "USER SET"

        End Function

    End Class
End Namespace
