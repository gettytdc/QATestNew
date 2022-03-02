Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

Namespace Commands

    ''' <summary>
    ''' The 'password' command
    ''' </summary>
    Public Class PasswordCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "password"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.PasswordCommand_UsePasswordPwdToCompleteAuthenticationAfterAUserCommandSeeTheDocumentationForUs
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Any
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            If Not Client.UserRequested Then Return "WRONG AUTH SEQUENCE"

            Dim password = command.Substring(9)
            Client.AuthenticatedUser = Server.ValidateCredentials(Client.RequestedUserName, New SafeString(password), Client.RequestedAuthenticationMode)
            ' Make absolutely certain that blank usernames are not validated as system 
            ' users have blank usernames
            If Not Client.UserSet OrElse Client.RequestedUserName = String.Empty Then _
                Return "AUTHENTICATION FAILED"

            ' Check if the requested user is allowed access to the Resource PC. If
            ' the listener is logged in with a different user, access is not allowed
            If Not Listener.ResourceOptions.IsPublic AndAlso
              Listener.UserId <> Nothing AndAlso Client.UserId <> Listener.UserId Then
                Client.AuthenticatedUser = Nothing
                Return "RESTRICTED : " & Listener.UserName
            End If

            If Not CheckUserHasPermissionOnResource(
              Client.AuthenticatedUser, Listener.ResourceId, Permission.Resources.ImpliedViewResource) Then
                Client.AuthenticatedUser = Nothing
                Return "USER DOES NOT HAVE ACCESS TO THIS RESOURCE"
            End If

            Client.RequestedUserName = String.Empty
            Client.RequestedAuthenticationMode = AuthMode.Unspecified
            Return "USER AUTHENTICATED"

        End Function

    End Class
End NameSpace
