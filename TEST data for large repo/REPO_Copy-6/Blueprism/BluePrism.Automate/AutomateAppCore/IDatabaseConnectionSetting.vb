Imports System.Data.SqlClient
Imports BluePrism.Common.Security
Imports BluePrism.DatabaseInstaller

Public Interface IDatabaseConnectionSetting
    Property AGPort As Integer
    Property CallbackPort As Integer
    Property ClientIsLoginAgent As Boolean
    Property ConnectionMode As ServerConnection.Mode
    Property ConnectionName As String
    Property ConnectionType As ConnectionType
    Function Clone() As IDatabaseConnectionSetting
    Property DBServer As String
    Property DBUserName As String
    Property DBUserPassword As SafeString
    Property ExtraParams As String
    Property MultiSubnetFailover As Boolean
    Property Port As Integer

    ReadOnly Property RequiresPasswordSpecifying As Boolean
    Property WindowsAuth As Boolean
    Sub Validate()
    Function ConfirmDBPassword(password As SafeString) As Boolean
    Function CreateSqlConnection(Optional master As Boolean = False) As SqlConnection
    Function Equals(obj As Object) As Boolean
    Function GetHashCode() As Integer
    Function ToString() As String
    Function GetConnectionString(Optional master As Boolean = False) As String
    Function IsComplete() As Boolean
    Property DatabaseName As String
    Property DatabaseFilePath As String
    Function CreateSqlSettings() As ISqlDatabaseConnectionSetting
End Interface
