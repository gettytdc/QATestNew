Namespace Commands

    ''' <summary>
    ''' The 'pool' command
    ''' </summary>
    Public Class PoolCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "pool"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.PoolCommand_DetermineWhichPoolThisResourceIsAMemberControllerOfTheResponseWillEitherBeNotIn
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            If Listener.PoolId = Guid.Empty Then Return "Not in a pool"

            If Listener.IsController _
                Then Return "Controller of " & Listener.PoolId.ToString() _
                Else Return "Member of " & Listener.PoolId.ToString()

        End Function

    End Class
End Namespace