Namespace Commands

    ''' <summary>
    ''' The 'availability' command
    ''' </summary>
    Public Class AvailabilityCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "availability"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.AvailabilityCommand_ReportsTheCurrentAvailabilityOfThisResourcePCForRunningMoreProcessesTheResponse & vbCrLf
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.AuthedOrLocal
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String
            Return "AVAILABILITY:" & Listener.Availability.ToString()
        End Function
    End Class
End Namespace