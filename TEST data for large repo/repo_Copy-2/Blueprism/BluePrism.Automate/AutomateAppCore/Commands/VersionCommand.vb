Namespace Commands

    ''' <summary>
    ''' The 'version' command
    ''' </summary>
    Public Class VersionCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "version"
            End Get
        End Property


        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.VersionCommand_GetVersionInformationTheResponseIsTheInternalProtocolVersionNumber
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

        Protected Overrides Function Exec(command As String) As String

            'Enable status updates to the client as a side effect of this command...
            Client.SendStatusUpdates = True
            Listener.NotifyStatus()
            Return "AUTOMATE RESOURCE PC / LISTENER " & clsListener.ListenerVersion
        End Function

    End Class
End Namespace