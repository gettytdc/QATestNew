Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Namespace Commands

    ''' <summary>
    ''' The 'proxyfor' command
    ''' </summary>
    Public Class ProxyForCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return "proxyfor"
            End Get
        End Property

        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.ProxyForCommand_SetsTheUseridThatThePoolMemberWillRunTheSessionAs
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            If Not Client.IsControllerConnection Then
                Return "NOT A POOL MEMBER"
            End If
            Dim userId = GetNearestUserID(command.Mid(10))
            If userId = Nothing Then Return "INVALID ID"
            Dim user = New User(AuthMode.System, userId, Nothing)
            Client.AuthenticatedUser = user
            Return "USER SET : " & user.Name

        End Function

    End Class
End NameSpace
