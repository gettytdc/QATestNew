Namespace Commands

    ''' <summary>
    ''' The 'quit' command
    ''' </summary>
    Public Class QuitCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "quit"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.QuitCommand_TerminateThisSessionTheClientIsDisconnectedTheServerContinuesToRun
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Any
            End Get
        End Property
        Public Overrides ReadOnly Property ValidRunStates As IEnumerable(Of ResourcePcRunState)
            Get
                Return AllRunStates
            End Get
        End Property

        Protected Overrides Function ExecRemoveClients(command As String) _
            As (output As String, clientsToRemove As IReadOnlyCollection(Of IListenerClient))

            Client.Close()
            Client.RemoveReason = "quit"
            Return (String.Empty, {Client})

        End Function

    End Class
End NameSpace