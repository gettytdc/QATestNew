Namespace Commands

    ''' <summary>
    ''' The 'ping' command
    ''' </summary>
    Public Class PingCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "ping"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.PingCommand_ReturnsTheMessagePongUsedForDiagnostics
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

        Protected Overrides Function Exec(command As String) As String
            ' The send status updates property is set when the
            ' version command is received, thus we know that the
            ' communication is via an interactive client or scheduler
            If Client.SendStatusUpdates Then
                Client.IsExpectingPing = True
            End If
            Return "pong"
        End Function

    End Class
End Namespace