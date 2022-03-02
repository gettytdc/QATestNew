Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateAppCore.ClientServerConnection

Public Module app

    Private ReadOnly ServerFactory As IServerFactory = New ServerFactoryWrapper()

    ''' <summary>
    ''' A global reference to a clsServer instance that represents the currently open
    ''' connection to a Blue Prism Server (either a shared database, or a real server).
    ''' Set to Nothing when there is no connection. In general, the code knows whether
    ''' there is supposed to be a connection open or not, so there is no need to check
    ''' this, just use it.
    ''' </summary>
    Public ReadOnly Property gSv() As IServer
        Get
            Dim server = ServerFactory.ServerManager.Server
            If server Is Nothing Then
                Throw New BluePrismException(My.Resources.gSv_TheServerInstanceCannotBeReferenced)
            End If
            Return server
        End Get
    End Property

    ''' <summary>
    ''' True when auditing is enabled.
    ''' </summary>
    Public gAuditingEnabled As Boolean = False

End Module
