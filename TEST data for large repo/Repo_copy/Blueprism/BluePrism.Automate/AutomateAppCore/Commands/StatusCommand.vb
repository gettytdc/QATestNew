Namespace Commands

    ''' <summary>
    ''' The 'status' command
    ''' </summary>
    Public Class StatusCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "status"
            End Get
        End Property

        Public Overrides ReadOnly Property ValidRunStates As IEnumerable(Of ResourcePcRunState)
            Get
                Return AllRunStates
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.StatusCommand_RetrievesAListOfPendingAndRunningSessionsOnTheResourcePCOutputIsInTheFormOfOneS
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.AuthedOrLocal
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String
            Dim user = Client.AuthenticatedUser
            Dim currentProcessMemberPermissions As IMemberPermissions
            Dim sb As New StringBuilder()
            Dim runningCount As Int32 = 0
            Dim IsLocal As Boolean = user Is Nothing

            sb.AppendLine("RESOURCE UNIT")

            For Each runner In Listener.Runners
                currentProcessMemberPermissions = Server.GetEffectiveMemberPermissionsForProcess(runner.ProcessId)

                If IsLocal OrElse currentProcessMemberPermissions.HasAnyPermissions(user) Then
                    sb.Append(" - ").AppendLine(runner.StatusText)
                Else
                    sb.Append(" - ").Append(runner.Status.ToString()) _
                        .Append(" ").Append("[Restricted]").AppendLine()
                End If

                runningCount += 1
            Next

            sb.Append("Total running: ").Append(runningCount).AppendLine()

            Return sb.ToString()
        End Function

    End Class
End Namespace