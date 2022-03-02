Imports System.IO
Imports BluePrism.AutomateAppCore.Config
Imports BluePrism.AutomateAppCore.Events
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Common.Security
Imports BluePrism.DatabaseInstaller
Imports BluePrism.Server.Domain.Models
Imports Microsoft.Win32

Public Class LocalDatabaseInstaller
    Public Event ReportCurrentStep As ReportCurrentStepEventHandler
    Public Event ReportProgress As PercentageProgressEventHandler
    Public Event ChangeProgressBarStyle As EventHandler

    Private Const BluePrismKey As String = "SOFTWARE\Blue Prism Limited\Automate"
    Private Const LocalDbKey As String = "SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL14E.LOCALDB\Setup"
    Private Const LocalDbBinRoot As String = "SQLBinRoot"
    Private Const LocalDbExe As String = "SQLLocalDB.exe"
    Private Const InstanceName As String = "BluePrismLocalDB"
    Private Const LocalDb As String = "LocalDB"
    Private Const ConnectionName As String = "LocalDB Connection"
    Private Const DatabaseName As String = "BluePrism"
    Private Const ProductName As String = "Blue Prism Limited"

    Public Shared Function InstallRequested() As Boolean
        Try
            Using key = LocalMachine(BluePrismKey)
                If key Is Nothing Then Return False
                Return key.GetValueNames().Contains(LocalDb)
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Shared Function LocalMachine(subKey As String) As RegistryKey
        Try
            Dim view = RegistryView.Registry64                                             'x64 or x32 on respective platforms
            Using key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view)
                Dim openKey = key.OpenSubKey(subKey)
                If openKey IsNot Nothing Then Return openKey
            End Using
        Catch
        End Try

        Try
            Dim view = RegistryView.Registry32                                             'wow6432node on x64
            Using key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view)
                Dim openKey = key.OpenSubKey(subKey)
                If openKey IsNot Nothing Then Return openKey
            End Using
        Catch
        End Try

        Return Nothing
    End Function

    Public ReadOnly Property UpgradeOnly As Boolean
        Get
            Return InstanceExists AndAlso DatabaseConfigured AndAlso
                   DatabaseExists AndAlso DatabaseValid AndAlso
                   DatabaseNeedsUpgrade
        End Get
    End Property


    Private ReadOnly Property LocalDBExePath() As String
        Get
            If mLocalDBExePath Is Nothing Then
                Using key = LocalMachine(LocalDbKey)
                    If key Is Nothing Then Return Nothing

                    Dim location = CStr(key.GetValue(LocalDbBinRoot))
                    location = location.Replace("\LocalDB\", "\Tools\")
                    mLocalDBExePath = Path.Combine(location, LocalDbExe)
                End Using
            End If
            Return mLocalDBExePath
        End Get
    End Property
    Private mLocalDBExePath As String

    Public ReadOnly Property InstanceExists As Boolean
        Get
            'A semi lazy function, if it doesn't exist check again
            'If we saw it exists once, then assume it still does.
            If Not mInstanceExists Then
                mInstanceExists = CheckInstanceExists()
            End If
            Return mInstanceExists
        End Get
    End Property
    Private mInstanceExists As Boolean

    Private Function CheckInstanceExists() As Boolean
        Dim fileName = LocalDBExePath()

        If Not IsPathValid(fileName) Then Return False

        Using process As New Process()
            With process.StartInfo
                .UseShellExecute = False
                .CreateNoWindow = True
                .WindowStyle = ProcessWindowStyle.Hidden
                .RedirectStandardOutput = True
                .FileName = fileName
                .Arguments = "info"
            End With
            process.Start()
            Dim output = process.StandardOutput.ReadToEnd()
            process.WaitForExit()
            If output Is Nothing Then Return False
            If output.Contains(InstanceName) Then Return True
            Return False
        End Using
    End Function

    Public Sub Install()
        If LocalDBExePath() IsNot Nothing Then Return

        Dim installerPath As String
        Using key = LocalMachine(BluePrismKey)
            installerPath = CStr(key.GetValue(LocalDb))
        End Using

        If Not IsPathValid(installerPath) Then Throw New InvalidArgumentException(String.Format(My.Resources.PathIsInvalid, installerPath))

        Using process As New Process
            With process.StartInfo
                .UseShellExecute = True
                .CreateNoWindow = True
                .WindowStyle = ProcessWindowStyle.Hidden
                .Verb = "runas"
                .FileName = "msiexec"
                .Arguments = $"/i ""{installerPath}"" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES"
            End With
            process.Start()
            process.WaitForExit()
        End Using
    End Sub

    Public Sub CreateInstance()
        Dim fileName = LocalDBExePath()
        If Not IsPathValid(fileName) Then Throw New InvalidArgumentException(String.Format(My.Resources.PathIsInvalid, fileName))

        ClearUpDatabaseFiles()

        Using process As New Process
            With process.StartInfo
                .UseShellExecute = False
                .CreateNoWindow = True
                .WindowStyle = ProcessWindowStyle.Hidden
                .FileName = fileName
                .Arguments = $"create {InstanceName}"
            End With
            process.Start()
            process.WaitForExit()
        End Using

        mInstanceExists = True
    End Sub

    Private ReadOnly Property DatabaseSetting() As IDatabaseConnectionSetting
        Get
            Return Options.Instance.Connections.FirstOrDefault(
                Function(x) x.ConnectionName = ConnectionName AndAlso
                x.ConnectionType = ConnectionType.Direct AndAlso
                x.DatabaseName = DatabaseName AndAlso
                x.DBServer = $"({LocalDb})\{InstanceName}")
        End Get
    End Property

    Public ReadOnly Property DatabaseConfigured() As Boolean
        Get
            Return DatabaseSetting IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property DataBasePath() As String
        Get
            Dim localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Return Path.Combine(localApplicationData, ProductName, "databases")
        End Get
    End Property
    Public ReadOnly Property DataBasePathExists() As Boolean
        Get
            Return Directory.Exists(DataBasePath)
        End Get
    End Property
    Public ReadOnly Property DatabaseFileExists() As Boolean
        Get
            Return _
                File.Exists(Path.ChangeExtension(Path.Combine(DataBasePath, DatabaseName), ".mdf")) OrElse
                File.Exists(Path.ChangeExtension(Path.Combine(DataBasePath, $"{DatabaseName}_log"), ".ldf"))
        End Get
    End Property
    Public Sub CreateDatabasePath()
        Directory.CreateDirectory(DataBasePath)
    End Sub
    Public Sub ConfigureDatabase()

        CreateDatabasePath()

        Dim connectionSetting As New clsDBConnectionSetting(ConnectionName)
        With connectionSetting
            .DBServer = $"({LocalDb})\{InstanceName}"
            .DatabaseName = DatabaseName
            .WindowsAuth = True
            .DatabaseFilePath = Path.Combine(DataBasePath, Path.ChangeExtension(DatabaseName, ".mdf"))
        End With

        Dim configOptions = Options.Instance
        configOptions.AddConnection(connectionSetting)
        configOptions.Save()
        configOptions.Init(ConfigLocator.Instance)

    End Sub

    Private ReadOnly Property Installer() As IInstaller
        Get
            If mInstaller Is Nothing Then
                Dim factory = DependencyResolver.Resolve(Of Func(Of ISqlDatabaseConnectionSetting, TimeSpan, String, String, IInstaller))
                mInstaller = factory(
                    DatabaseSetting().CreateSqlSettings(),
                    Options.Instance.DatabaseInstallCommandTimeout,
                    ApplicationProperties.ApplicationName,
                    clsServer.SingleSignOnEventCode)
                AddHandler mInstaller.ReportProgress,
                    Sub(sender, e) RaiseEvent ReportProgress(sender, e)
            End If
            Return mInstaller
        End Get
    End Property
    Private mInstaller As IInstaller


    Public ReadOnly Property DatabaseExists() As Boolean
        Get
            If Not mDatabaseExists Then
                mDatabaseExists = Installer.CheckDatabaseExists()
            End If
            Return mDatabaseExists
        End Get
    End Property
    Private mDatabaseExists As Boolean

    Public Sub ClearUpDatabaseFiles()
        If File.Exists(Path.ChangeExtension(Path.Combine(DataBasePath, DatabaseName), ".mdf")) Then
            File.Delete(Path.ChangeExtension(Path.Combine(DataBasePath, DatabaseName), ".mdf"))
        End If
        If File.Exists(Path.ChangeExtension(Path.Combine(DataBasePath, $"{DatabaseName}_log"), ".ldf")) Then
            File.Delete(Path.ChangeExtension(Path.Combine(DataBasePath, $"{DatabaseName}_log"), ".ldf"))
        End If
    End Sub
    Public Sub CreateDatabase(username As String, password As SafeString)
        If Not DataBasePathExists Then
            CreateDatabasePath()
        End If

        Installer.CreateLocalDatabase(username, password)
        Installer.SetDefaultPasswordRules()
        mDatabaseExists = True
    End Sub

    Public ReadOnly Property DatabaseValid As Boolean
        Get
            Try
                Return Installer.GetCurrentDBVersion > 0
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property

    Public ReadOnly Property DatabaseNeedsUpgrade() As Boolean
        Get
            Dim installer = Me.Installer
            Return installer.GetCurrentDBVersion < installer.GetRequiredDBVersion
        End Get
    End Property

    Public Sub UpgradeDatabase()
        Installer.UpgradeDatabase(0)
    End Sub

    Public Sub FullInstall(username As String, password As SafeString)
        If Not Me.InstanceExists Then
            RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.Install))
            Me.Install()
            RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.CreateInstance))
            Me.CreateInstance()
        End If

        If Not Me.DatabaseConfigured Then
            RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.ConfigureDatabase))
            Me.ConfigureDatabase()
        End If

        RaiseEvent ChangeProgressBarStyle(Me, EventArgs.Empty)

        If Not Me.DatabaseExists OrElse Not Me.DatabaseValid Then
            RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.CreateDatabase))
            Me.CreateDatabase(username, password)
        End If

        If Me.DatabaseNeedsUpgrade Then
            RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.UpgradeDatabase))
            Me.UpgradeDatabase()
        End If

        RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.Complete))
    End Sub

    Public Sub Upgrade()
        If Me.DatabaseNeedsUpgrade Then
            RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.UpgradeDatabase))
            Me.UpgradeDatabase()
        End If

        RaiseEvent ReportCurrentStep(Me, New ReportCurrentStepEventArgs(CurrentStepTypes.Complete))
    End Sub

    Private Function IsPathValid(filePath As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(filePath) AndAlso File.Exists(filePath) AndAlso filePath.IndexOfAny(Path.GetInvalidPathChars()) < 0
    End Function

End Class
