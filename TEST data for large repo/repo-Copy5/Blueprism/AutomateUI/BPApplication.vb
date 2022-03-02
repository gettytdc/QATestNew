Imports System.Threading
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Ipc
Imports AutomateUI.Classes
Imports AutomateUI.Logging
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.ClientServerConnection
Imports BluePrism.AutomateAppCore.Config
Imports BluePrism.AutomateAppCore.Logging
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Common.Security
Imports BluePrism.StartUp
Imports System.Security.Principal
Imports NLog
Imports BluePrism.Common.Security.Exceptions
Imports BluePrism.Core.CommandLineParameters
Imports BluePrism.DigitalWorker
Imports BluePrism.Server.Domain.Models

Public Module BPApplication
    ''' <summary>
    ''' A global reference to the main application form.
    ''' </summary>
    Public gMainForm As frmApplication

    Private Log As Logger = LogManager.GetCurrentClassLogger()

    Private mDependencyResolver As IDependencyResolver
    Private ReadOnly ResourceRunnerFactory As IResourceRunnerFactory = New ResourceRunnerFactory()
    Private ReadOnly UserMessage As IUserMessage = New UserMessageWrapper()
    Private ReadOnly ServerFactory As IServerFactory = New ServerFactoryWrapper()
    Private ReadOnly UserLogin As IUserLogin = New UserLogin()
    Private ReadOnly ConfigOptions As IOptions = Options.Instance
    Private ReadOnly IsUserLoggedIn As Func(Of Boolean) = Function() User.LoggedIn
    Private ReadOnly IsUserSubscribedToProcessAlertsPermission As Func(Of Boolean) = Function() User.Current.HasPermission("Subscribe to Process Alerts")
    Private ReadOnly CurrentUserName As Func(Of String) = Function() User.Current.Name

    ''' <summary>
    ''' Global property which indicates whether this instance of automate is running
    ''' interactively - ie. is a client, rather than a resource - or not.
    ''' </summary>
    Public ReadOnly Property IsInteractive() As Boolean
        Get
            Return (gMainForm IsNot Nothing)
        End Get
    End Property

    Enum ParameterIndex
        username = 1
        userpassword = 2
        resource = 3
        DBserver = 4
        DBname = 5
        DBusername = 6
        DBpassword = 7
        Port = 8
        alerts = 9
        bpserveraddress = 10
        bpserverport = 11
        dbconname = 12
        dbwait = 13
        bpserversecure = 14
        bpservercallback = 15
        agport = 16
        sslcert = 17
        connectionmode = 18
        wslocationprefix = 19
        digitalworkername = 20
        import = 21
        wcfperformance = 22
    End Enum

    Public Function Start(ByVal args() As String) As Integer

        AutomateNLogConfig.Configure()
        AutomateNLogConfig.SetStartUpAppProperties()

        LaunchImportListener(args)

        ' Initialise dependency injection container
        ContainerInitialiser.SetUpContainer()
        mDependencyResolver = DependencyResolver.GetScopedResolver()
        RegexTimeout.SetDefaultRegexTimeout()

        gAuditingEnabled = True

        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf HandleCrash
        Application.EnableVisualStyles()



        'Parse command line...
        Dim iPort As Integer = ResourceMachine.DefaultPort
        Dim sslcert As String = Nothing
        Dim bInvisible = False
        'Priority level required, 0=normal, +ve=higher, -ve=lower, range -1 - +3
        Dim iPriority As Integer = 0
        Dim bSingleSignon As Boolean = False
        Dim bAvailabilityGroup As Boolean = False
        Dim bResourcePC = False
        Dim bLoginAgent = False
        Dim bPublic = False
        Dim bLocal = False
        Dim bShowDBConfig = False
        Dim bAlerts = False
        Dim sTargetResource = ""
        Dim sUserName = ""
        Dim sPassword = New SafeString()
        Dim sDBServer = ""
        Dim sDBName = ""
        Dim sDBUsername = ""
        Dim sDBPassword = New SafeString()
        'Time to wait (keep retrying) for a database connection, in seconds, or -1 for
        'normal behaviour.
        Dim dbWaitTime As Integer = -1
        Dim sBPServerAddress As String = Nothing, sBPServerPort As String = Nothing
        Dim sSecure As String = Nothing
        Dim sConnectionMode As String = Nothing
        Dim callbackPort As Integer = Nothing
        Dim sDBConName As String = Nothing
        Dim agPort As Integer = 1433
        Dim multiSubnetFailover As Boolean = False
        Dim wslocationprefix = ""
        Dim enableHTTP As Boolean = True
        Dim digitalWorkerNameValue As DigitalWorkerName = Nothing
        Dim isDigitalWorker = False
        Dim fileToImport As String = ""

        'Processing mode during reading of command-line, see ParameterIndex
        Dim iMode As Integer = 0

        'Load options
        Dim bUserOpts = args.Contains("/useropts")
        Try
            ConfigOptions.Init(ConfigLocator.Instance(bUserOpts))
        Catch certificateException As CertificateException
            If certificateException.CertificateErrorCode = CertificateErrorCode.NotFound Then
                UserMessage.ShowError(certificateException, My.Resources.ErrorCertificateCannotBeFound)
                Throw
            Else
                DisplayErrorToUser(certificateException)
            End If
        Catch ex As Exception
            DisplayErrorToUser(ex)
        End Try
        'We need to do the following in this order as we need to record the current thread culture before changing it to the last used one
        ConfigOptions.SetSystemLocale()
        ConfigOptions.SetLastUsedLocale()

        Dim NumArrayElements As Integer = args.GetLength(0)
        If Not NumArrayElements = 0 Then

            For iCount = 0 To NumArrayElements - 1
                Dim rawParameterValue = args(iCount)
                Select Case iMode
                    Case 0
                        Select Case rawParameterValue
                            Case "/nosplash"
                                'Do nothing
                                Exit Select
                            Case "/help", "/?"
                                UserMessage.Show(My.Resources.ToAccessTheProductHelp)
                                Return 0
                            Case "/showdbconfig"
                                bShowDBConfig = True
                            Case "/p:below"
                                iPriority = -1
                            Case "/p:above"
                                iPriority = 1
                            Case "/p:high"
                                iPriority = 2
                            Case "/p:realtime"
                                iPriority = 3
                            Case "/resourcepc"
                                bResourcePC = True
                            Case "/pseudolocalization"
                                ctlLogin.PseudoLocalization = True
                            Case "/loginagent"
                                bLoginAgent = True
                            Case "/resource"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.resource, "/resource") Then Return 1
                            Case "/public"
                                bPublic = True
                            Case "/nohttp"
                                enableHTTP = False
                            Case "/local"
                                bLocal = True
                            Case "/invisible"
                                bInvisible = True
                            Case "/user"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.username, "/user") Then Return 1
                            Case "/sso"
                                bSingleSignon = True
                            Case "/ag"
                                bAvailabilityGroup = True
                            Case "/multisubnetfailover"
                                multiSubnetFailover = True
                            Case "/setbpserver"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.bpserveraddress, "/setbpserver") Then Return 1
                            Case "/bpserversecure"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.bpserversecure, "/bpserversecure") Then Return 1
                            Case "/connectionmode"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.connectionmode, "/connectionmode") Then Return 1
                            Case "/bpservercallback"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.bpservercallback, "/bpservercallback") Then Return 1
                            Case "/setdbserver"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.DBserver, "/setdbserver") Then Return 1
                            Case "/setdbname"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.DBname, "/setdbname") Then Return 1
                            Case "/setdbusername"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.DBusername, "/setdbusername") Then Return 1
                            Case "/setdbpassword"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.DBpassword, "/setdbpassword") Then Return 1
                            Case "/dbconname"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.dbconname, "/dbconname") Then Return 1
                            Case "/useropts"
                                bUserOpts = True
                            Case "/dbwait"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.dbwait, "/dbwait") Then Return 1
                            Case "/port"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.Port, "/port") Then Return 1
                            Case "/sslcert"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.sslcert, "/sslcert") Then Return 1
                            Case "/agport"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.agport, "/agport") Then Return 1
                            Case "/alerts"
                                bAlerts = True
                            Case "/wslocationprefix"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.wslocationprefix, "/wslocationprefix") Then Return 1
                            Case "/digitalworker"
                                isDigitalWorker = True
                                Dim toggle As New DigitalWorkerFeatureToggle
                                If Not toggle.FeatureEnabled Then Return 1
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.digitalworkername, "/digitalworker") Then Return 1
                            Case "/import"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.import, "/import") Then Return 1
                            Case "/wcfperformance"
                                If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.wcfperformance, "/wcfperformance") Then Return 1
                            Case Else
                                UserMessage.Show(String.Format(My.Resources.InvalidCommandLineArgument0, rawParameterValue))
                                Return 1
                        End Select
                    Case ParameterIndex.dbwait
                        If Not Integer.TryParse(rawParameterValue, dbWaitTime) Then
                            UserMessage.Show(My.Resources.InvalidWaitTimeSpecified)
                            Return 1
                        End If
                        iMode = 0
                    Case ParameterIndex.username
                        sUserName = rawParameterValue
                        If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.userpassword, "/user") Then Return 1
                    Case ParameterIndex.userpassword
                        sPassword = New SafeString(rawParameterValue)
                        iMode = 0
                    Case ParameterIndex.resource
                        sTargetResource = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.DBserver
                        sDBServer = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.DBname
                        sDBName = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.DBusername
                        sDBUsername = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.DBpassword
                        sDBPassword = New SafeString(rawParameterValue)
                        iMode = 0
                    Case ParameterIndex.Port
                        Try
                            iPort = CInt(rawParameterValue)
                        Catch e As Exception
                            UserMessage.Show(String.Format(My.Resources.InvalidPortNumber0, rawParameterValue))
                            Return 1
                        End Try
                        iMode = 0
                    Case ParameterIndex.sslcert
                        ' Ensure any non-ASCII characters are removed
                        sslcert = rawParameterValue.StripNonASCII()
                        If sslcert <> rawParameterValue Then Log.Warn(
                            "Non-ASCII characters detected in the SSL certificate thumbprint have been removed.")
                        iMode = 0
                    Case ParameterIndex.agport
                        Try
                            agPort = CInt(rawParameterValue)
                        Catch e As Exception
                            UserMessage.Show(String.Format(My.Resources.InvalidPortNumber0, rawParameterValue))
                            Return 1
                        End Try
                        iMode = 0
                    Case ParameterIndex.bpserveraddress
                        sBPServerAddress = rawParameterValue
                        If Not AdvanceModeIfSafe(args, iCount, iMode, ParameterIndex.bpserverport, "/setbpserver") Then Return 1
                    Case ParameterIndex.bpserverport
                        sBPServerPort = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.bpserversecure
                        sSecure = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.connectionmode
                        sConnectionMode = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.bpservercallback
                        If Not Integer.TryParse(rawParameterValue, callbackPort) Then
                            UserMessage.Show(String.Format(My.Resources.InvalidCallbackPort0, rawParameterValue))
                            Return 1
                        End If
                        iMode = 0
                    Case ParameterIndex.dbconname
                        sDBConName = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.wslocationprefix
                        wslocationprefix = rawParameterValue
                        iMode = 0
                    Case ParameterIndex.digitalworkername
                        If Not DigitalWorkerName.IsValid(rawParameterValue) Then
                            UserMessage.Show(My.Resources.StartupParams_DigitalWorker_ValidationWarning)
                            Return 1
                        End If
                        digitalWorkerNameValue = New DigitalWorkerName(rawParameterValue)
                        iMode = 0
                    Case ParameterIndex.import
                        fileToImport = rawParameterValue
                        'If we are the listener then don't use the import client
                        If Not ListenerExists Then
                            Return If(StartRemoteImport(fileToImport), 0, 1)
                        End If

                        iMode = 0
                    Case ParameterIndex.wcfperformance
                        Try
                            Dim performanceTestingParameter = New WcfPerformanceTestingParameter(rawParameterValue)
                            ConfigOptions.WcfPerformanceLogMinutes = performanceTestingParameter.PerformanceTestDurationMinutes
                            iMode = 0
                        Catch ex As Exception
                            UserMessage.Show(String.Format(BluePrism.Core.Properties.Resource.WCFPerformance_Invalid, rawParameterValue))
                            Return 1
                        End Try
                End Select
            Next
        End If

        AutomateNLogConfig.SetAppProperties(bResourcePC, bLoginAgent, iPort)
        Log = LogManager.GetCurrentClassLogger()

        If iMode <> 0 Then
            UserMessage.Show(My.Resources.InvalidCommandLineArguments)
            Return 1
        End If

        If bResourcePC AndAlso (Not bPublic) AndAlso (sUserName.Length = 0) AndAlso (Not bSingleSignon) Then
            UserMessage.Show(My.Resources.WhenStartingWithResourcepcButNotPublicYouMustSpecifyAUserWithUserNamePwdOrUseSso)
            Return 1
        End If

        If (bLocal Or bPublic) And Not bResourcePC Then
            UserMessage.Show(My.Resources.UsingLocalOrPublicMakesNoSenseUnlessUsingResourcepc)
            Return 1
        End If

        If sUserName = "" Xor sPassword.IsEmpty Then
            UserMessage.Show(My.Resources.AUsernameOrAPasswordWasSuppliedWithoutTheCorrespondingPasswordOrUsernamePleaseT)
            Return 1
        End If

        'database parameters may not be supplied with any other kind of parameter:
        If sDBServer & sDBName & sDBUsername <> "" AndAlso bResourcePC Then
            UserMessage.Show(My.Resources.TheParametersSetdbserverSetdbnameSetdbusernameandSetdbpasswordMayNotBeUsedToget)
            Return 1
        End If

        'The correct set of database parameters must be supplied...
        If (sDBServer <> "" AndAlso sDBName = "") OrElse (sDBUsername <> "" AndAlso sDBPassword.IsEmpty) Then
            UserMessage.Show(My.Resources.TheSuppliedDatabaseConnectionDetailsAreIncomplete)
            Return 1
        End If

        'Can't do database and bp server settings at the same time...
        If sDBServer <> "" AndAlso sBPServerAddress IsNot Nothing Then
            UserMessage.Show(My.Resources.CanTSetNormalDatabaseAndBluePrismServerConnectionDetailsAtTheSameTime)
            Return 1
        End If

        If bAvailabilityGroup AndAlso sDBServer = "" Then
            UserMessage.Show(My.Resources.SettingUpAnAvailabilityIGroupConnectionRequiresADatabaseServerListenerToBeSet)
            Return 1
        End If

        If bAvailabilityGroup AndAlso sBPServerAddress IsNot Nothing Then
            UserMessage.Show(My.Resources.BluePrismServerAndAvailabilityGroupsCannotBeMixedInTheSameConnection)
            Return 1
        End If

        'Check whether installing localdb is required
        If LocalDatabaseInstaller.InstallRequested() Then
            Try
                Using welcomeWizard As New WelcomeWizard
                    Dim localDb As New LocalDatabaseInstaller()
                    welcomeWizard.LocalDB = localDb
                    welcomeWizard.UpgradeOnly = localDb.UpgradeOnly
                    welcomeWizard.ShowInTaskbar = False

                    If Not localDb.InstanceExists OrElse
                       Not localDb.DatabaseConfigured OrElse
                       Not localDb.DatabaseExists OrElse
                       Not localDb.DatabaseValid OrElse
                       localDb.DatabaseNeedsUpgrade Then

                        If welcomeWizard.ShowDialog() = DialogResult.Abort Then
                            Return 1
                        End If
                    End If

                End Using
            Catch ex As Exception
                UserMessage.ShowError(ex, My.Resources.ErrorConfiguringBluePrismLocalDB)
                Return 1
            End Try

        End If

        Dim sErr2 As String = Nothing

        ' Database connection choosing - if specified at all, there are 3
        ' possibilities:
        ' 1) DB server passed - creating / changing direct DB connection
        ' 2) BP server passed - creating / changing BP Server connection
        ' 3) None of the above, but 'connection name' passed - specifying existing
        '    connection to use for this session.

        If sDBServer <> "" Then
            'Set database settings if passed on command-line...
            If sDBConName Is Nothing Then sDBConName = sDBName
            ' If we have no user name, blank out the password too - it means we're using winauth...
            If sDBUsername = "" Then sDBPassword = New SafeString()

            Try
                If bAvailabilityGroup Then
                    ConfigOptions.UpdateDatabaseSettings(sDBConName, sDBServer, sDBName,
                          sDBUsername, sDBPassword, (sDBUsername = ""), agPort, multiSubnetFailover)

                Else
                    ConfigOptions.UpdateDatabaseSettings(sDBConName, sDBServer, sDBName,
                          sDBUsername, sDBPassword, (sDBUsername = ""))
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.FailedToSaveDatabaseSettings0, ex.Message))
                Return 1
            End Try

            Return 0

        ElseIf sBPServerAddress IsNot Nothing Then

            'Set Blue Prism Server connection settings if passed on command-line...
            If sDBConName Is Nothing Then sDBConName = sBPServerAddress
            Dim port As Integer
            If Not Integer.TryParse(sBPServerPort, port) Then
                UserMessage.Show(My.Resources.InvalidPort)
                Return 1
            End If

            'Set the default connection mode
            Dim connMode As ServerConnection.Mode =
                ServerConnection.Mode.WCFSOAPMessageWindows

            'If connection mode was passed in as an arg, then try parsing
            If sConnectionMode IsNot Nothing Then
                If Not [Enum].TryParse(sConnectionMode, connMode) Then
                    UserMessage.Show(My.Resources.InvalidConnectionModeSetting)
                    Return 1
                End If
            ElseIf sSecure IsNot Nothing Then
                'If no connection mode was passed in, check whether the deprecated
                'bpserversecure switch was used. If so, choose the appropriate
                '.NET Remoting mode.
                Dim secure As Boolean
                If Not Boolean.TryParse(sSecure, secure) Then
                    UserMessage.Show(My.Resources.InvalidSecuritySetting)
                    Return 1
                End If

                connMode = If(secure, ServerConnection.Mode.DotNetRemotingSecure,
                              ServerConnection.Mode.DotNetRemotingInsecure)
            End If

            Try
                ConfigOptions.UpdateDatabaseSettings(sDBConName, sBPServerAddress, port, connMode, callbackPort)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.FailedToSaveBluePrismServerSettings0, ex.Message))
                Return 1
            End Try

            Return 0

        ElseIf sDBConName IsNot Nothing Then

            ' Find the connection specified in the options connections
            Dim found As Boolean = False
            For Each conn As clsDBConnectionSetting In ConfigOptions.Connections
                If conn.ConnectionName.Equals(
                 sDBConName, StringComparison.CurrentCultureIgnoreCase) Then

                    ' If we need a password, ensure that the user provided one
                    If conn.RequiresPasswordSpecifying AndAlso sDBPassword.IsEmpty Then
                        Try
                            Log.Error(
                             "Password not specified for connection '{0}'",
                             sDBConName)
                        Catch
                        End Try
                        UserMessage.Show(String.Format(
                         My.Resources.ThePasswordMustBeSpecifiedForTheConnection0UsingSetdbpasswordPassword, sDBConName))
                        Return 1
                    End If

                    ' Set the password given if the user provided one (even if
                    ' we don't *need* one - this allows the user to override)
                    If Not sDBPassword.IsEmpty Then conn.DBUserPassword = sDBPassword

                    ' And set the connection to use in the options.
                    ' Note - we don't save this information. Ideally, this should
                    ' only apply for this session, but the client saves clsOptions
                    ' all over the place, so it may get saved at some point.
                    ' However, for resource PCs it should apply for this session only
                    ' even if, in the client, we can't make that guarantee
                    ConfigOptions.DbConnectionSetting = conn

                    found = True
                    Exit For

                End If
            Next

            ' Specified connection wasn't found - give the user a hint at where
            ' this can be resolved.
            If Not found Then
                Log.Error(
                "Specified connection name '{0}' not found", sDBConName)

                UserMessage.Show(String.Format(
                 My.Resources.TheConnection0DoesNotExist1PleaseCheckTheConnectionsByRunningAutomateShowdbconf,
                 sDBConName, vbCrLf))

                Return 1
            End If

        End If

        'Set our process priority if requested on command-line...
        If iPriority <> 0 Then
            Dim p As Process = Process.GetCurrentProcess()
            Select Case iPriority
                Case 1 : p.PriorityClass = ProcessPriorityClass.AboveNormal
                Case 2 : p.PriorityClass = ProcessPriorityClass.High
                Case 3 : p.PriorityClass = ProcessPriorityClass.RealTime
                Case -1 : p.PriorityClass = ProcessPriorityClass.BelowNormal
            End Select
        End If

        Dim connectionSetting As clsDBConnectionSetting = ConfigOptions.DbConnectionSetting

        'Wait for a database connection if /dbwait was specified
        If dbWaitTime <> -1 Then
            If connectionSetting.IsComplete Then
                ServerFactory.ClientInit(connectionSetting)
                Try
                    ServerFactory.ValidateCurrentConnection()
                Catch ex As Exception
                    Dim started As DateTime = DateTime.Now
                    While (DateTime.Now - started).TotalSeconds < dbWaitTime AndAlso Not ServerFactory.ServerAvailable
                        Thread.Sleep(5000)
                        ServerFactory.ClientInit(connectionSetting)
                    End While
                End Try
            End If
        Else
            If isDigitalWorker OrElse bResourcePC OrElse bAlerts Then

                If Not connectionSetting.IsComplete Then
                    Console.WriteLine(My.Resources.DatabaseConnectionStringNotComplete)
                    Return 1
                End If
                ServerFactory.ClientInit(connectionSetting)

                'Check database is valid...
                Try
                    ServerFactory.ValidateCurrentConnection()
                Catch ex As Exception
                    UserMessage.ShowExceptionMessage(ex)
                    Return 1
                End Try
            End If
        End If


        'Handle authentication
        Dim authed = False
        Dim loggedInUser As IUser = Nothing
        If bResourcePC OrElse bAlerts Then
            Dim databaseType = gSv.DatabaseType()
            Dim locale = Thread.CurrentThread.CurrentUICulture.CompareInfo.Name
            Dim machineName = ResourceMachine.GetName(iPort)

            If bSingleSignon Then
                Try
                    Dim result As LoginResult = If(databaseType = DatabaseType.SingleSignOn, UserLogin.Login(machineName, locale), UserLogin.LoginWithMappedActiveDirectoryUser(machineName, locale))

                    If Not result.IsSuccess Then
                        UserMessage.Show(String.Format(
                                         My.Resources.SingleSignOnAuthenticationFailed0,
                                         result.Description))
                        Return 1
                    End If
                    loggedInUser = result.User
                    sUserName = loggedInUser.Name
                Catch ex As UnknownLoginException
                    UserMessage.Show(String.Format(
                                     My.Resources.SingleSignOnAuthenticationFailed0,
                                         ex.Message))
                    Return 1
                End Try

                authed = True

            ElseIf Not (sUserName = "" AndAlso sPassword.IsEmpty) Then 'Equivalent to sUserName <> "" OrElse Not sPassword.IsEmpty
                If databaseType = DatabaseType.SingleSignOn Then
                    UserMessage.Show(My.Resources.UsernameAndPasswordWereSpecifiedButDatabaseRequresSingleSignOn)
                    Return 1
                End If
                Try
                    Dim result As LoginResult = UserLogin.Login(ResourceMachine.GetName(iPort), sUserName, sPassword, locale)
                    If Not result.IsSuccess Then
                        UserMessage.Show(String.Format(My.Resources.LoginFailed0,
                                                       result.Description))
                        Return 1
                    End If
                    loggedInUser = result.User
                Catch ex As UnknownLoginException
                    UserMessage.Show(String.Format(My.Resources.LoginFailed0,
                                                   ex.Message))
                    Return 1
                End Try
                authed = True
            End If
        End If

        'Perform any general APC initialisation here - we are about to either start the
        'main application, resource PC or alerts client......
        clsAppCore.InitAPC(isDigitalWorker)
        clsAPC.SetProcessDebugHook(New clsAutomateProcessDebugHook())

        Try
            'Do whatever we're supposed to be doing...
            If isDigitalWorker OrElse bResourcePC Then
                If isDigitalWorker OrElse (bPublic AndAlso Not authed) Then
                    Try
                        Dim machineName = If(isDigitalWorker, digitalWorkerNameValue.FullName, ResourceMachine.GetName(iPort))
                        Dim result = UserLogin.LoginAsAnonResource(machineName)
                        If Not result.IsSuccess Then
                            UserMessage.Show(String.Format(My.Resources.AuthenticationFailed0,
                                                           result.Description))
                            Return 1
                        End If
                        loggedInUser = result.User
                    Catch ex As UnknownLoginException
                        UserMessage.Show(String.Format(My.Resources.AuthenticationFailed0,
                                                       ex.Message))
                        Return 1
                    End Try
                End If

                If gSv.IsServer Then
                    SaveEnvironmentData(iPort, ConfigOptions.GetCertificateExpiryDateTime, connectionSetting.Port, gSv.GetServerFullyQualifiedDomainName)
                Else
                    SaveEnvironmentData(iPort, ConfigOptions.GetCertificateExpiryDateTime)
                End If

                If Not If(loggedInUser?.HasPermission(Permission.Resources.AuthenticateAsResource), False) Then
                    UserMessage.ShowPermissionMessage()
                    Return 1
                End If

                'If we haven't already logged in (e.g. /public) then we need to
                'refresh the license details from the database...
                If Not UserLogin.LoggedIn Then clsLicenseQueries.RefreshLicense()

                Dim startupOptions As IResourceRunnerStartUpOptions
                If isDigitalWorker Then
                    startupOptions = New DigitalWorkerStartUpOptions With {
                        .Name = digitalWorkerNameValue
                    }
                    DigitalWorkerContextStore.Current = New DigitalWorkerContext(TryCast(startupOptions, DigitalWorkerStartUpOptions))
                Else
                    startupOptions = New Resources.ResourcePCStartUpOptions() With {
                        .IsPublic = bPublic,
                        .Port = iPort,
                        .SSLCertHash = sslcert,
                        .WebServiceAddressPrefix = wslocationprefix,
                        .Username = sUserName,
                        .IsLocal = bLocal,
                        .IsLoginAgent = bLoginAgent,
                        .HTTPEnabled = enableHTTP}
                End If

                Using components = ResourceRunnerFactory.Create(startupOptions)
                    If Not bInvisible Then components.View.ShowForm()
                    Dim runner = components.Runner
                    runner.Init()
                    Using New clsAlertEngine(sUserName, ResourceMachine.GetName(iPort))
                        While runner.IsRunning
                            Application.DoEvents()
                            If runner.SessionsRunning() Then
                                System.Threading.Thread.Sleep(100)               'free up CPU when not running
                            Else
                                System.Threading.Thread.Sleep(10)                'not burn too much CPU when running
                            End If
                        End While
                    End Using
                End Using

                Return 0

            ElseIf bAlerts Then
                If Not IsUserLoggedIn() Then
                    UserMessage.Show(My.Resources.UserInformationMustBeSpecifiedToStartTheAlertsMonitor)
                    Return 1
                End If
                If Not IsUserSubscribedToProcessAlertsPermission() Then
                    UserMessage.ShowError(My.Resources.TheSpecifiedUser0DoesNotHavePermissionToReceiveProcessAlerts, CurrentUserName())
                    Return 1
                End If
                If sTargetResource = "" Then
                    sTargetResource = ResourceMachine.GetName()
                End If
                Using f As New frmAlertMonitor(sTargetResource)
                    f.ShowInTaskbar = False
                    f.ShowDialog()
                End Using

            ElseIf bShowDBConfig Then

                Using proc As New Process()
                    With proc.StartInfo
                        .FileName = ApplicationProperties.AutomateConfigPath
                        .CreateNoWindow = True
                    End With
                    proc.Start()
                    proc.WaitForExit()
                End Using
                Return 0
            Else
                Dim mainForm As frmApplication
                If Not String.IsNullOrEmpty(fileToImport) Then
                    FilesToImport.FilesInBatch = +1
                    FilesToImport.FileQueue.Enqueue(fileToImport)
                End If
                mainForm = New frmApplication()
                Using mainForm
                    gMainForm = mainForm
                    Try
                        mainForm.ShowDialog()
                    Catch ex As Exception
                        ex.HelpLink = "helpFaultReporting.htm"
                        UserMessage.Show(String.Format(My.Resources.x0HasEncounteredAnUnknownErrorWeApologiseForThisInconveniencePleaseHelpUsToResol, ApplicationProperties.ApplicationName), ex)
                    Finally
                        mainForm.ShutDownProcessEngine(False)
                    End Try
                    gMainForm = Nothing
                End Using

                If frmApplication.KeepProcessAlertsRunning Then
                    Dim objAlertForm As New frmAlertMonitor(ResourceMachine.GetName())
                    objAlertForm.ShowInTaskbar = False
                    objAlertForm.ShowDialog()
                    objAlertForm.Dispose()
                End If
            End If

        Finally
            ConfigOptions.DisposeConfig()
            ServerFactory.Close()
        End Try

        Return 0

    End Function

    Private Sub DisplayErrorToUser(ex As Exception)
        UserMessage.ShowError(ex,
                     My.Resources.ErrorLoadingConfigurationOptions01BluePrismWillContinueUsingDefaultSettings, ex.Message, vbCrLf)
    End Sub

    Private Sub SaveEnvironmentData(iPort As Integer, certExpiryDateTime As Date?, Optional applicationServerPortNumber As Integer = 0, Optional applicationServerFullyQualifiedDomainName As String = Nothing)
        Try
            gSv.SaveEnvironmentData(New EnvironmentData(EnvironmentType.Resource,
                                                        clsUtility.GetFQDN(),
                                                        iPort,
                                                        Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString,
                                                        Nothing,
                                                        Nothing,
                                                        certExpiryDateTime), applicationServerPortNumber, applicationServerFullyQualifiedDomainName)
        Catch
            'Do nothing, we have probably failed to connect and that will be handled elsewhere
        End Try
    End Sub

    'Checks if the next element in the cmdargs array a) exists and b) is not a parameter
    'and if so we advance iMode to the supplied value NextMode.
    'Returns true on success.
    Private Function AdvanceModeIfSafe(ByRef CmdArgs() As String, ByVal CurrentIndex As Integer,
      ByRef iMode As Integer, ByVal NextMode As Integer,
      ByVal CurrentParameterName As String) As Boolean

        Dim MaxIndex As Integer = CmdArgs.GetLength(0)

        If CurrentIndex < (MaxIndex - 1) AndAlso Not CmdArgs(CurrentIndex + 1).Mid(1, 1) = "/" Then
            iMode = NextMode
            Return True
        Else
            UserMessage.Show(String.Format(My.Resources.BadCommandLineArgumentsArgumentMissingForParameter0, CurrentParameterName))
            Return False
        End If
    End Function

    ''' <summary>
    ''' Reports an unhandled exception occurring in this application domain by
    ''' logging the error.
    ''' </summary>
    ''' <param name="sender">The source of the unhandled exception event</param>
    ''' <param name="e">The args detailing the unhandled exception</param>
    Private Sub HandleCrash(
     ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
        Log.Fatal(
            "An unhandled exception occurred in Blue Prism:{0}{0}{1}",
            vbCrLf, e.ExceptionObject)
    End Sub

    Public Property ListenerExists As Boolean
    Private Sub LaunchImportListener(args() As String)
        Try
            If args.Any(Function(x) String.Compare(x, "/resourcepc", StringComparison.InvariantCultureIgnoreCase) = 0) Then Exit Sub
            Dim sid = WindowsIdentity.GetCurrent().User
            ChannelServices.RegisterChannel(
                New IpcServerChannel($"automate-IC-server-File-Importer-{sid}"),
                True)
            RemotingConfiguration.RegisterWellKnownServiceType(GetType(ImportListener), "Import", WellKnownObjectMode.Singleton)
            ListenerExists = True
        Catch ex As RemotingException
            ListenerExists = False
            'No Catch as is we cannot create a listener then it must already exist and we will connect later
        End Try
    End Sub

    Private Function StartRemoteImport(filePath As String) As Boolean

        Dim settings As New Hashtable()
        Dim clientExists As Boolean = False
        settings("portName") = "automate-IC-client"
        settings("authorizedGroup") = "Everyone"
        ChannelServices.RegisterChannel(New IpcClientChannel(settings, Nothing), True)
        Dim sid = WindowsIdentity.GetCurrent().User
        Dim listener As ImportListener = DirectCast(
                    Activator.GetObject(
                        GetType(ImportListener),
                        $"ipc://automate-IC-server-File-Importer-{sid}/Import"),
                    ImportListener)


        Try
            listener.ImportProcess(filePath)
            clientExists = True

        Catch ex As Exception
            If ex.Message = "User not logged in." OrElse ex.Message = "Insufficient Permissions" Then
                clientExists = True
            End If
        End Try

        ChannelServices.UnregisterChannel(ChannelServices.GetChannel("automate-IC-client"))
        Return clientExists

    End Function

End Module
