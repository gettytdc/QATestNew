Imports BluePrism.AutomateAppCore.Auth

Namespace ClientServerConnection
    Public Interface IServerFactory
        ReadOnly Property ServerManager As ServerManager
        ReadOnly Property ServerAvailable As Boolean

        Function ClientInit(connectionSetting As clsDBConnectionSetting) As ServerManager
        Function ServerInit(listenMode As ServerConnection.Mode, ByRef dbConnection As clsDBConnectionSetting, keys As Dictionary(Of String, clsEncryptionScheme), ByRef systemUser As IUser) As ServerManager
        Function CheckServerAvailability() As Boolean
        Sub ValidateCurrentConnection()
        Sub Close()
        Sub ValidateConnectionSetting(connectionSetting As clsDBConnectionSetting)
    End Interface
End NameSpace