Namespace Commands

    ''' <summary>
    ''' The 'busy' command
    ''' </summary>
    Public Class BusyCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "busy"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.BusyCommand_CheckIfThereAreThereAnySessionsPendingRunningRespondsYesOrNo
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
            If Listener.Runners.IsBusy Then Return "yes" Else Return "no"
        End Function

    End Class
End Namespace