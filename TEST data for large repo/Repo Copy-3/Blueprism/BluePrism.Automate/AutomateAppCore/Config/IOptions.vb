Imports BluePrism.AutomateProcessCore
Imports BluePrism.Common.Security

Namespace Config
    Public Interface IOptions
        Event Load()
        ReadOnly Property DefaultServerPort As Integer
        ReadOnly Property Servers As ICollection(Of MachineConfig.ServerConfig)
        Property Thumbprint() As String
        Property SelectedConfigEncryptionMethod As MachineConfig.ConfigEncryptionMethod
        ReadOnly Property Unbranded As Boolean
        ReadOnly Property Connections As IList(Of clsDBConnectionSetting)
        Property DbConnectionSetting As clsDBConnectionSetting
        Property CurrentConnectionName As String
        Property SystemLocale As String
        Property CurrentServerConfigName As String
        Property SystemLocaleFormat As String
        Property LastUsedLocale As String
        Property LastUsedLocaleFormat As String
        ReadOnly Property UseCultureCalendar As Boolean
        Property CurrentLocale As String
        Property SqlCommandTimeout As Integer
        Property SqlCommandTimeoutLong As Integer
        Property SqlCommandTimeoutLog As Integer
        Property DatabaseInstallCommandTimeout As TimeSpan
        Property DataPipelineProcessSendTimeout As Integer
        Property CompressProcessXml As Boolean
        Property EditSummariesAreCompulsory As Boolean
        Property ArchivePath As String
        Property LastNativeUser As String
        Property StartProcessEngine As Boolean
        Property LastBrandingTitle As String
        Property LastBrandingIcon As String
        Property LastBrandingLargeLogo As String
        Property MaxUndoLevels As Integer
        Property WcfPerformanceLogMinutes As Integer?
        ReadOnly Property ResourceCallbackConfig As ClientServerResources.Core.Config.ConnectionConfig
        Function GetServerConfig(name As String) As MachineConfig.ServerConfig
        Function NewServerConfig() As MachineConfig.ServerConfig
        Function HasWritePrivileges() As Boolean
        Sub Reload()
        Sub Init(location As IConfigLocator)
        Sub Save()
        Sub UpdateRabbitMqConfiguration(name As String, hostUrl As String, username As String, password As String)
        Sub UpdateASCRServerSettings(serverName As String, callbackProtocol As Integer, hostName As String, port As Integer, mode As Integer, certname As String, clientCertName As String, serverStore As String, clientStore As String)
        Sub UpdateDatabaseSettings(name As String, server As String, databaseName As String, username As String, password As SafeString, windowsAuthentication As Boolean)
        Sub UpdateDatabaseSettings(name As String, server As String, databaseName As String, username As String, password As SafeString, windowsAuthentication As Boolean, port As Integer, multiSubnetFailover As Boolean)
        Sub UpdateDatabaseSettings(name As String, address As String, port As Integer, connectionMode As ServerConnection.Mode, callbackPort As Integer)
        Function UpdateServerSettings(name As String, connectionName As String, port As Integer, connectionMode As ServerConnection.Mode, encryptionScheme As clsEncryptionScheme, ordered As Boolean) As MachineConfig.ServerConfig
        Function ConfirmDatabasePassword(password As SafeString) As Boolean
        Function GetInstalledComBusinessObjectsCount() As Integer
        Function GetExternalObjectsInfo(reload As Boolean) As IGroupObjectDetails
        Function GetExternalObjectsInfo() As IGroupObjectDetails
        Function GetObjects() As List(Of String)
        Sub ClearAllConnections()
        Sub AddConnection(databaseConnection As clsDBConnectionSetting)
        Sub AddObject(name As String)
        Sub RemoveObject(name As String)
        Sub SetLastUsedLocale()
        Sub SetSystemLocale()
        Function GetCertificateExpiryDateTime() As Date?
        Sub DisposeConfig()
    End Interface
End Namespace
