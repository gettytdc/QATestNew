Imports BluePrism.BPCoreLib

Namespace Commands
    Public Class DeleteAsCommand
        Inherits DeleteCommand

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer,
                       memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name As String = "deleteas"

        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.DeleteAsCommand_UsesTokenAuthenticationToDeleteAPendingSessionDeleteasTokenSessionidTheResponse
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode = CommandAuthenticationMode.Authed

        Protected Overrides Function Exec(command As String) As String
            Try

                Dim token = New clsAuthToken(command.Mid(10, clsAuthToken.TokenLength))

                Dim gID As Guid = GetNearestSessionID(command.Mid(11 + clsAuthToken.TokenLength))
                If gID = Nothing Then Return "INVALID ID"

                Return DeleteSession(token, gID)

            Catch ex As Exception
                Return ex.Message & vbCrLf
            End Try
        End Function
    End Class
End Namespace
