Imports BluePrism.BPCoreLib

Namespace Commands
    Public Class StartAsCommand
        Inherits StartCommand

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer,
                       memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub
        Public Overrides ReadOnly Property Name As String = "startas"

        Public Overrides ReadOnly Property Help As String
            Get
                Return My.Resources.StartAsCommand_UsesTokenAuthenticationToEitherStartASessionRunningStartasTokenSessionidOrStart
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode = CommandAuthenticationMode.Authed

        Protected Overrides Function Exec(command As String) As String

            Try
                Dim token As New clsAuthToken(command.Mid(9, clsAuthToken.TokenLength))

                Dim session As String = command.Mid(10 + clsAuthToken.TokenLength)
                Dim sessId As Guid
                If session.ToLower() = "last" _
                Then sessId = Client.LastSessionCreated _
                Else sessId = GetNearestSessionID(session)
                If sessId = Nothing Then Return "INVALID ID"

                Return StartSession(token, sessId)

            Catch ex As Exception
                Return ex.Message & vbCrLf
            End Try

        End Function
    End Class
End Namespace
