Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.DatabaseInstaller

''' <summary>
''' Server manager to hold the direct server connection.
''' This is really just a stub.
''' </summary>
''' <remarks></remarks>
Public NotInheritable Class ServerManagerDirect
    Inherits ServerManager

    Private Shared ReadOnly InstallerFactory As Func(Of IDatabaseConnectionSetting, IInstaller) = Function(s As IDatabaseConnectionSetting) CreateInstaller(s)

    Private Shared Function CreateInstaller(s As IDatabaseConnectionSetting) As IInstaller
        Dim factory = DependencyResolver.Resolve(Of Func(Of ISqlDatabaseConnectionSetting, TimeSpan, String, String, IInstaller))
        Return factory(
            s.CreateSqlSettings(),
            Options.Instance.DatabaseInstallCommandTimeout,
            ApplicationProperties.ApplicationName,
            clsServer.SingleSignOnEventCode)
    End Function

    Public Overrides Sub OpenConnection(setting As clsDBConnectionSetting, keys As Dictionary(Of String, clsEncryptionScheme), ByRef systemUser As IUser)

        Try

            ConnectionDetails = setting

            Dim installer = InstallerFactory(setting)
            installer.CheckIntegrity()

            Dim sv As New clsServer(setting, keys)
            mServer = sv

            If sv IsNot Nothing Then
                StartDataMonitor()
                Permission.Init(sv)
            End If

            If systemUser IsNot Nothing Then
                systemUser = sv.LoginAsSystem(systemUser.Name)
            End If


        Catch ex As Exception
            LastConnectException = ex
        End Try
    End Sub

    Public Overrides Sub CloseConnection()
        Return
    End Sub

End Class


