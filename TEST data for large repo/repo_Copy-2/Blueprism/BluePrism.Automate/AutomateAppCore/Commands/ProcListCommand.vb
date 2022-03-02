Namespace Commands

    ''' <summary>
    ''' The 'proclist' command
    ''' </summary>
    Public Class ProcListCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "proclist"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.ProcListCommand_ProvidesAListOfProcessesWhichAreAvailableToBeRunOutputIsInTheFormOfOneProcessPe
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Authed
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String
            Dim user = Client.AuthenticatedUser
            Dim currentProcessMemberPermissions As IMemberPermissions
            Dim processId As Guid
            Dim processIdIsValid As Boolean

            Dim sb As New StringBuilder()
            For Each row As DataRow In gSv.GetAvailableProcesses().Rows
                processIdIsValid = Guid.TryParse(row("ProcessID").ToString(), processId)

                If (processIdIsValid) Then
                    currentProcessMemberPermissions = Server.GetEffectiveMemberPermissionsForProcess(processId)

                    If currentProcessMemberPermissions.HasAnyPermissions(user) Then _
                        sb.Append(processId.ToString()).Append(" - ").Append(row("Name")) _
                        .AppendLine()
                End If
            Next

            Return sb.ToString()
        End Function
    End Class
End Namespace