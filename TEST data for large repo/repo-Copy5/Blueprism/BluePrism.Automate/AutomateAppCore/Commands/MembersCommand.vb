Imports BluePrism.AutomateAppCore.Resources

Namespace Commands

    ''' <summary>
    ''' The 'members' command
    ''' </summary>
    Public Class MembersCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "members"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.MembersCommand_ListTheMembersOfThePoolForWhichThisResourcePCIsTheControllerExampleResponseMEMB
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.AuthedOrLocal
            End Get
        End Property


        Protected Overrides Function Exec(command As String) As String

            'Dim sOutput As String = "ERROR", sErr As String = Nothing

            If Not Listener.IsController Then Return "NOT A CONTROLLER"

            Dim members As ICollection(Of IResourceMachine)
            SyncLock Listener.ResourceConnections
                members = Listener.ResourceConnections.GetResources(False).Values
            End SyncLock

            Dim sb As New StringBuilder()
            sb.Append("MEMBERS - ").Append(members.Count + 1).AppendLine()
            sb _
                .Append(Listener.ResourceId).Append(",") _
                .Append(Listener.ResourceName).Append(",") _
                .Append("Connected").AppendLine()

            For Each m As ResourceMachine In members
                sb _
                    .Append(m.Id).Append(",") _
                    .Append(m.Name).Append(",") _
                    .Append(m.ConnectionState).AppendLine()
            Next

            Return sb.ToString()

        End Function

    End Class
End Namespace
