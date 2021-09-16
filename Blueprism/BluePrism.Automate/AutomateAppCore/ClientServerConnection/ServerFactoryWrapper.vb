Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateAppCore.Auth

Namespace ClientServerConnection
    Public Class ServerFactoryWrapper : Implements IServerFactory
        Public ReadOnly Property ServerManager As ServerManager Implements IServerFactory.ServerManager
            Get
                Dim manager = ServerFactory.ServerManager
                If manager Is Nothing Then
                    Throw New BluePrismException(My.Resources.ServerFactoryWrapper_TheServerManagerCannotBeReferenced)
                End If
                Return manager
            End Get
        End Property

        Public ReadOnly Property ServerAvailable As Boolean Implements IServerFactory.ServerAvailable
            Get
                Return ServerFactory.ServerAvailable()
            End Get
        End Property

        Public Sub ValidateCurrentConnection() Implements IServerFactory.ValidateCurrentConnection
            ServerFactory.CurrentConnectionValid()
        End Sub

        Public Sub Close() Implements IServerFactory.Close
            ServerFactory.Close()
        End Sub

        Public Sub ValidateConnectionSetting(connectionSetting As clsDBConnectionSetting) Implements IServerFactory.ValidateConnectionSetting
            ServerFactory.Validate(connectionSetting)
        End Sub

        Public Function ClientInit(connectionSetting As clsDBConnectionSetting) As ServerManager Implements IServerFactory.ClientInit
            Return ServerFactory.ClientInit(connectionSetting)
        End Function

        Public Function ServerInit(
                                   listenMode As ServerConnection.Mode, 
                                   ByRef connectionSetting As clsDBConnectionSetting,
                                   keys As Dictionary(Of String, clsEncryptionScheme), 
                                   ByRef systemUser As IUser) _
            As ServerManager Implements IServerFactory.ServerInit

            Return ServerFactory.ServerInit(listenMode, connectionSetting, keys, systemUser)
        End Function

        Public Function CheckServerAvailability() As Boolean Implements IServerFactory.CheckServerAvailability
            Return ServerFactory.CheckServerAvailability()
        End Function
    End Class
End NameSpace
