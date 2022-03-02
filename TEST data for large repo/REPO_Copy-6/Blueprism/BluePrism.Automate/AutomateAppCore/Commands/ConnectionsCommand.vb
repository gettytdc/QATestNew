Imports BluePrism.AutomateAppCore.Resources

Namespace Commands

    ''' <summary>
    ''' The 'connections' command
    ''' </summary>
    Public Class ConnectionsCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "connections"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.ConnectionsCommand_GetAListOfActiveConnectionsInvolvingThisResourcePCTheListIsSeparatedIntoTwoSect
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

            Dim sb As New StringBuilder()
            sb.Append("OUTBOUND: ")
            If Listener.ResourceConnections Is Nothing Then
                sb.AppendLine("disabled")
            Else
                Dim c As ICollection(Of IResourceMachine) =
                        Listener.ResourceConnections.GetResources(False).Values
                sb.Append(c.Count).AppendLine()
                For Each rm As IResourceMachine In c
                    sb.AppendFormat(" {0} - {1}", rm.Name, rm.ConnectionState) _
                        .AppendLine()
                Next
            End If
            sb.Append("INBOUND: ").Append(Listener.Clients.Count).AppendLine()
            For Each lc As ListenerClient In Listener.Clients
                sb.AppendFormat(" {0} ({1})", lc.RemoteAddressFriendlyString, lc.UserName) _
                    .AppendLine()
            Next

            Return sb.ToString()

        End Function

    End Class
End Namespace
