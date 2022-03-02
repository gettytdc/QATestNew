Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports AutomateC.AuthenticationServerUserMapping
Imports AutomateC.Logging
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.BackgroundJobs
Imports BluePrism.AutomateAppCore.BackgroundJobs.Monitoring
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports BluePrism.AutomateAppCore.Commands
Imports BluePrism.AutomateAppCore.Commands.Documentation
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.CharMatching
Imports BluePrism.Common.Security
Imports BluePrism.Core.Compression
Imports BluePrism.Core.Resources
Imports BluePrism.Core.Xml
Imports BluePrism.DatabaseInstaller
Imports BluePrism.DigitalWorker
Imports BluePrism.Scheduling
Imports BluePrism.Server.Domain.Models
Imports BluePrism.StartUp
Imports FeatureToggle.Core
Imports LocaleTools
Imports NLog
Imports BluePrism.AutomateAppCore.My.Resources

Public Module AutomateC

    Private ReadOnly Log As Logger = LogManager.GetCurrentClassLogger

    Private mServerStatusUpdater As Threading.Timer
    Private mSendServerStatusUpdate As Integer = 1
    Private mAuthenticationServerFeatureEnabled As Boolean

    ''' <summary>
    ''' Enumeration of the argument modes for the parsing of command line arguments.
    ''' The 'default' is "nextswitch" - ie. the parser is looking for a switch as
    ''' the next argument that it is dealing with. Anything else is a particular
    ''' type of parameter which is to be dealt with.
    ''' </summary>
    Enum ArgMode

        ''' <summary>
        ''' The default argument mode - look for a switch as the next argument
        ''' </summary>
        nextswitch = 0

        locale
        run
        username
        userpassword
        resource
        startp
        Installer
        Port
        createdb
        genivbowrapper
        import
        sessionid
        licensefile
        clsid
        publish
        unpublish
        publishws
        unpublishws
        export
        upgradedb
        singlesignonauth
        activedirectorydomain
        activedirectoryadmingroup
        forceid
        filespec
        queuename
        queuekey
        queuerunning
        queuemaxattempts
        queuefilter
        queueencryption
        objectname
        path
        fromdate
        todate
        fromrev
        torev
        age
        connection
        serverport
        webservicename
        webserviceurl
        pool
        timeout
        wsuser
        wspassword
        schedule
        viewschedlist
        viewschedlistdate
        startsched
        delsched
        process
        servername
        secure
        connectionmode
        overwrite
        evname
        evdatatype
        evvalue
        evdesc
        exportpackage
        importrelease
        release
        commandtimeout
        wslog
        force
        configencrypt
        forceconfigencrypt
        listlogins
        maxdbver
        dbconnection
        batchsize
        maxbatches
        exportformat
        allowanonresourcesflag
        enforcecontrollinguserpermissionsflag
        encryptionschemename
        encryptionschemealgorithm
        credentialname
        credentialpassword
        credentialusername
        expirydate
        invalid
        description
        credentialpropertyname
        credentialpropertyvalue
        credentialType
        resourcename
        limitnumber
        limittype
        installlocaldb
        localdbusername
        localdbpassword
        disablewelcome
        mappedactivedirectoryusername
        mappedactivedirectorysid
        setactivedirectoryauth
        ordered
        rabbitmqconnection
        rabbitmqurl
        rabbitmqusername
        rabbitmqpassword
        mapauthenticationserverusersInputCsvPath
        mapauthenticationserverusersOutputCsvPath
        ascrservername
        ascrconntype
        ascrhostname
        ascrport
        ascrmode
        ascrcertname
        ascrclientcertname
        ascrservercertstore
        ascrclientcertstore
        getblueprismtemplateforusermapping

    End Enum

    Enum LimitType
        Minutes
        Hours
        Days
        Months
    End Enum

    ''' <summary>
    ''' Exception to throw if any of the options on the command line fail for
    ''' any reason. The message is crucial and should be able to be output to
    ''' the user without any further information and still be meaningful.
    ''' This is primarily here for 'expected' errors - usually validation errors
    ''' on the client's input.
    ''' </summary>
    <Serializable>
    Private Class CommandLineFailureException : Inherits Exception

        ''' <summary>
        ''' Create a new exception with the given message.
        ''' </summary>
        ''' <param name="message">The message for the exception.</param>
        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Create a new exception with the given formatted message.
        ''' </summary>
        ''' <param name="message">The message format string, as defined by the
        ''' String.Format() method.</param>
        ''' <param name="args">The arguments to use in the formatted message
        ''' </param>
        Public Sub New(ByVal message As String, ByVal ParamArray args() As Object)
            MyBase.New(String.Format(message, args))
        End Sub

    End Class

    ''' <summary>
    ''' Helper method to set the action into the given byref parameter,
    ''' ensuring that it is the only action specified.
    ''' If an action is already specified an error message is displayed
    ''' and the program is exited with an error response of 1
    ''' </summary>
    ''' <param name="value">The value to set as the action</param>
    ''' <param name="into">The variable into which the value should be
    ''' set if it is not already set. If this has any value other than
    ''' null, then an error message will be displayed and the console
    ''' will be exited.</param>
    Private Sub SetAction(ByVal value As String, ByRef into As String)
        If into IsNot Nothing Then
            Console.WriteLine(My.Resources.OnlyOneActionCanBeSpecified)
            Environment.Exit(1)
        End If
        into = value
    End Sub

    ''' <summary>
    ''' Helper method to write a message to the console and return an
    ''' errorlevel value indicating an error.
    ''' </summary>
    ''' <param name="message">The message to write to the console</param>
    ''' <param name="args">The arguments to pass to the WriteLine()
    ''' method for formatted output.</param>
    ''' <returns>An error level indicating an error (1)</returns>
    Private Function Err(message As String, ParamArray args() As Object) As Integer
        Console.WriteLine(message, args)
        Return 1
    End Function


    ''' <summary>
    ''' Helper method to write a message to the console and return an
    ''' errorlevel value indicating an error.
    ''' </summary>
    ''' <param name="message">The message to write to the console</param>
    ''' <returns>An error level indicating an error (1)</returns>
    Private Function Err(message As String) As Integer
        WriteWithNewlines(message)
        Return 1
    End Function

    ''' <summary>
    ''' Helper method to write a message to the console and allow "\n" escapes for new lines
    ''' </summary>
    ''' <param name="message">The message to write to the console</param>
    ''' <param name="args">The arguments to pass to the WriteLine()
    ''' method for formatted output.</param>
    Private Sub WriteWithNewlines(message As String, ParamArray args() As Object)
        Dim replacedMessage = message.Replace("\n", Environment.NewLine)
        Console.WriteLine(replacedMessage, args)
    End Sub

    ''' <summary>
    ''' Helper method to write a message to the console and allow "\n" escapes for new lines
    ''' </summary>
    ''' <param name="message">The message to write to the console</param>
    Private Sub WriteWithNewlines(message As String)
        Dim replacedMessage = message.Replace("\n", Environment.NewLine)
        Console.WriteLine(replacedMessage)
    End Sub


    ''' <summary>
    ''' Helper function to get the schedules specified by the given names or all
    ''' the active schedules if no names were given.
    ''' </summary>
    ''' <param name="scheduleNames">The names of the schedules required.</param>
    ''' <param name="store">The scheduler store which is to be used to retrieve
    ''' the schedules.</param>
    ''' <returns>The collection of schedules corresponding to the given names.
    ''' </returns>
    Private Function GetSchedules(
     ByVal scheduleNames As ICollection(Of String),
     ByVal store As DatabaseBackedScheduleStore) _
     As ICollection(Of ISchedule)

        If store Is Nothing Then
            Throw New ArgumentNullException(
             My.Resources.NoScheduleStoreAvailableCannotRetrieveSchedules, My.Resources.Scheduler)
        End If

        ' Treat null array as an empty array
        If scheduleNames Is Nothing Then
            scheduleNames = New String() {}
        End If

        Dim schedules As ICollection(Of ISchedule) = Nothing

        If scheduleNames.Count = 0 Then
            ' Clone into a list, so that the scheduler's schedule collection can
            ' be modified while this collection is being enumerated
            schedules = New List(Of ISchedule)(store.GetActiveSchedules())
        Else
            Dim schedSet As New clsSet(Of ISchedule)
            For Each name As String In scheduleNames
                Dim sched As ISchedule = store.GetSchedule(name)
                If sched Is Nothing Then
                    Console.WriteLine(My.Resources.WarningSchedule0NotFoundSkipping, name)
                Else
                    If Not schedSet.Add(sched) Then
                        Console.WriteLine(My.Resources.WarningSchedule0EnteredMoreThanOnceExtraEntriesIgnored, name)
                    End If
                End If
            Next
            schedules = schedSet
        End If

        ' If we have nowt to go back with, error back to user.
        If schedules.Count = 0 Then
            Throw New CommandLineFailureException(My.Resources.NoSchedulesFoundToProcessNoWorkToDo)
        End If

        Return schedules

    End Function

    ''' <summary>
    ''' Helper method to check that the user is both logged in and has sufficient
    ''' permissions to perform the given action.
    ''' If either is not the case an exception is thrown.
    ''' </summary>
    ''' <param name="permissionActions">The actions to check against the current
    ''' user's permissions for. If the user has <em>any</em> of the specified
    ''' permissions, they will be allowed through - if they have none of them
    ''' a <see cref="CommandLineFailureException"/> will be thrown.</param>
    ''' <exception cref="CommandLineFailureException">If the user is not logged
    ''' in, or has insufficient privileges to perform any of the required actions.
    ''' </exception>
    Private Sub CheckLoggedInAndUserRole(ByVal ParamArray permissionActions() As String)

        'Check username/password correctly supplied:
        If Not User.LoggedIn Then Throw New CommandLineFailureException(
         My.Resources.ErrorYouMustSupplyValidLoginDetailsForThatAction)

        ' If a permission action was supplied, check it on the current user.
        If permissionActions IsNot Nothing AndAlso
         Not User.Current.HasPermission(permissionActions) Then
            If permissionActions.Length = 1 Then
                Throw New CommandLineFailureException(
                 My.Resources.ErrorPermissionDeniedTheUserRoleDoesNotHaveThe0Permission,
                 permissionActions(0))
            End If
            Dim sb As New StringBuilder(
             My.Resources.ErrorPermissionDeniedPermissionsRequired)
            CollectionUtil.JoinInto(permissionActions, ", ", sb)
            Throw New CommandLineFailureException(sb.ToString())

        End If

    End Sub

    Private Sub CheckUserHasPermissionForProcess(processId As Guid, permission As String)

        Dim processPermissions = gSv.GetEffectiveMemberPermissionsForProcess(processId)

        Dim hasPermission = processPermissions.HasPermission(User.Current, permission)

        If Not hasPermission Then
            Throw New CommandLineFailureException(
                My.Resources.ErrorPermissionDeniedTheUserRoleDoesNotHaveThe0PermissionForThisItem,
                permission)
        End If
    End Sub

    ''' <summary>
    ''' Helper function to link two stages in a Process, throwing an Exception
    ''' if something goes wrong.
    ''' </summary>
    ''' <param name="linkfrom">The stage to link from</param>
    ''' <param name="linkto">The stage to link to</param>
    ''' <param name="proc">The process containing the two stages</param>
    Private Sub LinkStages(ByVal linkfrom As clsProcessStage, ByVal linkto As clsProcessStage, ByVal proc As clsProcess)
        Dim sErr As String = Nothing
        If Not proc.CreateLink(CType(linkfrom, clsLinkableStage), linkto, sErr) Then
            Throw New InvalidOperationException(String.Format(My.Resources.ERRORFailedToLinkStage0To12, linkfrom.GetName(), linkto.GetName(), sErr))
        End If
    End Sub


    ''' <summary>
    ''' Entry point for this module
    ''' </summary>
    ''' <param name="args">The command line arguments detailing what this module
    ''' should be doing.</param>
    ''' <returns>An integer exit code - 0 indicates success; 1 indicates failure.
    ''' </returns>
    <STAThread>
    Public Function main(ByVal args() As String) As Integer
        Try
            ContainerInitialiser.SetUpContainer()
            SetDefaultRegexTimeout()
            AutomateCNLogConfig.Configure()
            Return Run(args)
        Finally
            ' Ensure the automateconfig program is exited if appropriate
            Options.Instance.DisposeConfig()
            ClearAutomateCAlive()
        End Try
    End Function


    ''' <summary>
    ''' Runs this AutomateC module with the given arguments.
    ''' </summary>
    ''' <param name="args">The command line arguments detailing what this module
    ''' should be doing.</param>
    ''' <returns>An integer exit code - 0 indicates success; 1 indicates failure.
    ''' </returns>
    Public Function Run(ByVal args() As String) As Integer

        'Use the below debugger.launch to attach process for debugging when executing from command prompt
        'Debugger.Launch

        gAuditingEnabled = True

        'TODO: This needs to be done by DI when that is in the solution
        Dim documentationProvider As IDocumentationProvider = New DocumentationProvider(New CommandFactory(Nothing, Nothing, Nothing, Nothing))

        'Name of a process supplied on the command line, be it for running/publishing/etc
        Dim sRunProcess As String
        'Username and password combination for authorising running of processes, etc
        Dim sLocale As String
        Dim sUserName As String, sPassword As SafeString
        Dim sTargetResource As String = Nothing
        Dim sStartParms As String, sPool As String
        Dim sModifyDBPwd As New SafeString()
        Dim bCreateDBDrop As Boolean
        Dim maxDBVer As Integer = 0
        Dim sIVBOToWrap As String = ""
        Dim sImportFile As String = ""
        Dim sExportProcess As String = ""
        Dim sObjectCLSID As String = ""
        Dim forcingId As Boolean = False
        Dim overwrite As Boolean = False
        Dim delete As Boolean = False
        Dim debug_opt As Boolean = False
        Dim sForceID As String = ""
        Dim iPort As Integer
        Dim singleSignon As Boolean = False
        Dim bClearExported As Boolean = False
        Dim sPath As String = Nothing
        Dim timeout As Integer = -1
        Dim wsuser As String = Nothing
        Dim wspassword As SafeString = Nothing

        Dim secureServer As String = Nothing
        Dim connMode As String = Nothing
        Dim ordered As Boolean = True
        Dim orderedChangeRequested As Boolean = False

        Dim bUserOpts As Boolean = False

        'This will contain the action that has been selected by the command line parameters.
        Dim sAction As String = Nothing

        Dim servername As String = Nothing
        Dim sObjectName As String = Nothing
        Dim sWebServiceName As String = ""
        Dim sWebServiceURL As String = ""
        Dim bDocLiteral As Boolean = False
        Dim useLegacyNamespace As Boolean = False
        Dim sSessionID As String = ""
        Dim resourceName = String.Empty
        Dim limitNumber = String.Empty
        Dim limitType = String.Empty
        Dim sLicenseFile As String = ""
        Dim sDomainName As String = ""
        Dim sFilespec As String = ""
        Dim sActiveDirectoryAdminGroup As String = ""
        Dim sConnection As String = Nothing
        Dim cmdTimeout As Integer = -1
        Dim sWSLog As String = Nothing
        Dim evname As String = Nothing
        Dim evdatatype As String = Nothing
        Dim evvalue As String = Nothing
        Dim evdesc As String = Nothing

        Dim rabbitMQConnection As String = Nothing
        Dim rabbitMQHostUrl As String = Nothing
        Dim rabbitMQUserName As String = Nothing
        Dim rabbitMQPassword As String = Nothing

        Dim mapAuthenticationServerUsersInputCsvPath As String = Nothing
        Dim mapAuthenticationServerUsersOutputCsvPath As String = Nothing
        Dim bluePrismUserTemplateOutputCsvPath As String = Nothing

        sRunProcess = ""
        sPool = Nothing
        sStartParms = ""
        sLocale = Threading.Thread.CurrentThread.CurrentUICulture.CompareInfo.Name
        sUserName = ""
        sPassword = New SafeString()
        iPort = ResourceMachine.DefaultPort
        Dim fromDate As Date = Date.MinValue
        Dim toDate As Date = Date.MaxValue
        Dim age As String = Nothing
        Dim fromRev As Integer = 0
        Dim toRev As Integer = 0

        ' Scheduler command variables
        Dim scheduleNames As New List(Of String)
        Dim scheduleListType As ScheduleListType
        Dim scheduleListName As String = Nothing
        Dim scheduleListDate As Date = Now
        Dim scheduleListDays As Integer = Integer.MinValue
        Dim exportFormat As String = "txt"

        ' Queue command variable 
        ' /createqueue <keyfield> <running> <maxattempts> {/queuename <queue>}
        ' /exportqueue <file> {/queuename <queue>} [/queuefilter <filter>] [/clearexported]
        ' /queueclearworked {/queuename <queue>}
        ' /deletequeue {/queuename <queue>}
        ' /setencrypt <encryption-name> {/queuename <queue>}
        ' /unsetencrypt {/queuename <queue>}
        Dim queueName As String = ""
        Dim queueKey As String = ""
        Dim queueRunning As Boolean = False
        Dim queueMaxAttempts As Integer = 0

        Dim queueFilter As String = ""
        Dim queueEncrypter As String = Nothing

        ' /exportpackage {package-name} [/release {release-name}]
        Dim packageToExport As String = Nothing
        Dim releaseName As String = Nothing
        Dim forceRefresh As Boolean = False
        Dim configEncryptParam As String = Nothing
        Dim forceConfigEncryptParam As Boolean = False
        Dim listloginsParam As Boolean = False
        Dim connectionName As String = Nothing
        Dim batchSize As Integer = -1
        Dim maxBatches As Integer = -1
        Dim allowAnonResources As Boolean = False
        Dim enforceControllingUserPermissions As Boolean = True
        Dim encSchemeName As String = ""
        Dim encSchemeAlgorithm = EncryptionAlgorithm.None

        ' /createcredential <credname> <username> <password> [/description <string> /expirydate <date> /invalid <flag> /credentialtype <string>]
        ' /updatecredential <credname> [/username <username> /password <password> /description <string>  /expirydate <date> /invalid <flag> /credentialtype <string>]
        ' /setcredentialproperty <credname> <propertyname> <propertyvalue>
        Dim credentialName As String = String.Empty
        Dim credentialPassword As New SafeString
        Dim credentialUsername As String = String.Empty
        Dim expiryDate As Date = Date.MinValue
        Dim isInvalid As Boolean = False
        Dim isInvalidSet As Boolean = False
        Dim description As String = String.Empty
        Dim credentialTypeName As String = String.Empty
        Dim credentialPropertyName = ""
        Dim credentialPropertyValue As New SafeString

        Dim sErr As String = Nothing

        Dim sLocalDbUserName = String.Empty
        Dim sLocalDbPassword As SafeString = Nothing

        Dim adUserName As String = ""
        Dim adSid As String = ""
        Dim adEnableAuth As Boolean

        'ASCR configuration command variables
        Dim ascrServerName As String = String.Empty
        Dim ascrConnType As Integer = -1
        Dim ascrHostName As String = String.Empty
        Dim ascrPort As Integer = -1
        Dim ascrMode As Integer = -1
        Dim ascrCertname As String = String.Empty
        Dim ascrClientCertName As String = String.Empty
        Dim ascrServerStore As String = String.Empty
        Dim ascrClientStore As String = String.Empty
        'Processing mode during reading of command-line, see ArgMode
        Dim mode As ArgMode = ArgMode.nextswitch

        mAuthenticationServerFeatureEnabled = New DefaultToDisabledOnErrorDecorator(New AuthenticationServerToggle()).FeatureEnabled

        For iCount As Integer = 0 To args.Length - 1
            Dim s As String = args(iCount)
            Select Case mode
                Case ArgMode.nextswitch
                    Select Case s
                        Case "/locale"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.locale, "/locale") Then Return 1

                        Case "/help", "/?"
                            SetAction("help", sAction)

                        Case "/dbconname"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.dbconnection, "/dbconname") Then Return 1

                        Case "/usagetree"
                            SetAction("usagetree", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode,
                             ArgMode.process, "/usagetree") Then Return 1

                        Case "/getdbscript"
                            SetAction("getdbscript", sAction)
                        Case "/getdbversion"
                            SetAction("getdbversion", sAction)

                        Case "/report"
                            SetAction("report", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.filespec, "/report") Then Return 1

                        Case "/rolereport"
                            SetAction("rolereport", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.filespec, "/rolereport") Then Return 1

                        Case "/elementusage"
                            SetAction("elementusage", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.filespec, "/elementusage") Then Return 1

                        Case "/listprocesses"
                            SetAction("listprocesses", sAction)

                        Case "/license"
                            SetAction("license", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.licensefile, "/license") Then Return 1

                        Case "/removelicense"
                            SetAction("removelicense", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.licensefile, "/license") Then Return 1

                        Case "/connectionmode"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.connectionmode, "/connectionmode") Then Return 1

                        Case "/ordered"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ordered, "/ordered") Then Return 1

                        Case "/secure"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.secure, "/secure") Then Return 1

                        Case "/serverconfig"
                            SetAction("serverconfig", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.servername, "/serverconfig") Then Return 1

                        Case "/rabbitmqconfig"
                            Dim toggle As New DigitalWorkerFeatureToggle()
                            If Not toggle.FeatureEnabled Then Return 1

                            SetAction("rabbitmqconfig", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.rabbitmqconnection, "/rabbitmqconfig") Then Return 1

                        Case "/ascrconfig"
                            SetAction("ascrconfig", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrservername, "/ascrconfig") Then Return 1

                        Case "/encryptionscheme"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.encryptionschemename, "/encryptionscheme") Then Return 1

                        Case "/regobject"
                            SetAction("regobject", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.clsid, "/regobject") Then Return 1

                        Case "/regwebservice"
                            SetAction("regwebservice", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.webservicename, "/regwebservice") Then Return 1

                        Case "/unregwebservice"
                            SetAction("unregwebservice", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.webservicename, "/unregwebservice") Then Return 1

                        Case "/getlog"
                            SetAction("getlog", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.sessionid, "/getlog") Then Return 1

                        Case "/getauditlog"
                            SetAction("getauditlog", sAction)

                        Case "/getbod"
                            SetAction("getbod", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.objectname, "/getbod") Then Return 1

                        Case "/objectname"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.objectname, "/objectname") Then Return 1

                        Case "/getauthtoken"
                            SetAction("getauthtoken", sAction)

                        Case "/status"
                            SetAction("status", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.sessionid, "/status") Then Return 1

                        Case "/createqueue"
                            SetAction("createqueue", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.queuekey, "/createqueue") Then Return 1

                        Case "/exportqueue"
                            SetAction("exportqueue", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.filespec, "/exportqueue") Then Return 1

                        Case "/deletequeue"
                            SetAction("deletequeue", sAction)

                        Case "/queuename"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.queuename, "/queuename") Then Return 1

                        Case "/queuefilter"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.queuefilter, "/queuefilter") Then Return 1

                        Case "/setencrypt"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.queueencryption, "/setencrypt") Then Return 1
                            SetAction("setencrypt", sAction)

                        Case "/resetencrypt"
                            ' /resetencrypt is just a /setencrypt with queueEncrypter set to null
                            SetAction("setencrypt", sAction)

                        Case "/clearexported"
                            bClearExported = True

                        Case "/queueclearworked"
                            SetAction("queueclearworked", sAction)

                        Case "/poolcreate"
                            SetAction("poolcreate", sAction)

                        Case "/pooldelete"
                            SetAction("pooldelete", sAction)

                        Case "/pooladd"
                            SetAction("pooladd", sAction)

                        Case "/poolremove"
                            SetAction("poolremove", sAction)

                        Case "/createdb"
                            SetAction("createdb", sAction)
                            bCreateDBDrop = True
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.createdb, "/createdb") Then Return 1

                        Case "/configuredb"
                            SetAction("configuredb", sAction)
                            bCreateDBDrop = True
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.createdb, "/configuredb") Then Return 1

                        Case "/annotatedb"
                            SetAction("annotatedb", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.createdb, "/annotatedb") Then Return 1

                        Case "/getdbdocs"
                            SetAction("getdbdocs", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.createdb, "/getdbdocs") Then Return 1

                        Case "/genivbowrapper"
                            SetAction("genivbowrapper", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.genivbowrapper, "/genivbowrapper") Then Return 1

                        Case "/getresprotdocs"
                            SetAction("getresprotdocs", sAction)

                        Case "/getvalidationdocs"
                            SetAction("getvalidationdocs", sAction)

                        Case "/getresprothtmldocs"
                            SetAction("getresprothtmldocs", sAction)

                        Case "/upgradedb"
                            SetAction("upgradedb", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.upgradedb, "/upgradedb") Then Return 1

                        Case "/setaddomain"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.activedirectorydomain, "/setaddomain") Then Return 1

                        Case "/setadadmingroup"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.activedirectoryadmingroup, "/setadadmingroup") Then Return 1

                        Case "/replacedb"
                            SetAction("createdb", sAction)
                            bCreateDBDrop = False
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.createdb, "/replacedb") Then Return 1

                        Case "/maxdbver"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.maxdbver, "/maxdbver") Then Return 1

                        Case "/unexpire"
                            SetAction("unexpire", sAction)

                        Case "/useropts"
                            bUserOpts = True

                        Case "/import"
                            SetAction("import", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.import, "/import") Then Return 1

                        Case "/importrelease"
                            SetAction("importrelease", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.importrelease, "/importrelease") Then Return 1

                        Case "/export"
                            SetAction("export", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.export, "/export") Then Return 1

                        Case "/exportpackage"
                            SetAction("exportpackage", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.exportpackage, "/exportpackage") Then Return 1

                        Case "/release"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.release, "/release") Then Return 1

                        Case "/archive"
                            SetAction("archive", sAction)

                        Case "/restorearchive"
                            SetAction("restorearchive", sAction)

                        Case "/from"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.fromdate, "/from") Then Return 1

                        Case "/to"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.todate, "/to") Then Return 1

                        Case "/age"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.age, "/age") Then Return 1

                        Case "/fromrev"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.fromrev, "/fromrev") Then Return 1

                        Case "/torev"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.torev, "/torev") Then Return 1

                        Case "/resource"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.resource, "/resource") Then Return 1

                        Case "/pool"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.pool, "/pool") Then Return 1

                        Case "/run"
                            SetAction("run", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.run, "/run") Then Return 1

                        Case "/wslog"
                            SetAction("wslog", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.wslog, "/wslog") Then Return 1

                        Case "/process"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.process, "/process") Then Return 1

                        Case "/timeout"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.timeout, "/timeout") Then Return 1

                        Case "/publish"
                            SetAction("publish", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.publish, "/publish") Then Return 1

                        Case "/unpublish"
                            SetAction("unpublish", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.unpublish, "/unpublish") Then Return 1

                        Case "/publishws"
                            SetAction("publishws", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.publishws, "/publishws") Then Return 1

                        Case "/forcedoclitencoding"
                            bDocLiteral = True

                        Case "/useGlobalNamespace"
                            useLegacyNamespace = True

                        Case "/unpublishws"
                            SetAction("unpublishws", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.unpublishws, "/unpublishws") Then Return 1

                        Case "/user"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.username, "/user") Then Return 1

                        Case "/wsauth"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.wsuser, "/wsauth") Then Return 1

                        Case "/sso"
                            singleSignon = True

                        Case "/startp"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.startp, "/startp") Then Return 1

                        Case "/requeststop"
                            SetAction("requeststop", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.sessionid, "/requeststop") Then Return 1

                        Case "/forceid"
                            forcingId = True
                            ' Get the ID being forced if it exists... otherwise, just reset the
                            ' mode to default. The flag will indicate that it should use the ID in
                            ' the imported file.
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.forceid, "/forceid", False) Then
                                mode = ArgMode.nextswitch
                            End If

                        Case "/overwrite"
                            overwrite = True

                        Case "/delete"
                            delete = True

                        Case "/debug"
                            debug_opt = True

                        Case "/setev"
                            SetAction("setev", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.evname, "/setev") Then Return 1

                        Case "/deleteev"
                            SetAction("deleteev", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.evname, "/deleteev") Then Return 1

                        Case "/setcommandtimeout"
                            SetAction("setcommandtimeout", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.commandtimeout, "/setcommandtimeout") Then Return 1

                        Case "/setcommandtimeoutlong"
                            SetAction("setcommandtimeoutlong", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.commandtimeout, "/setcommandtimeoutlong") Then Return 1

                        Case "/setcommandtimeoutlog"
                            SetAction("setcommandtimeoutlog", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.commandtimeout, "/setcommandtimeoutlog") Then Return 1

                        Case "/port"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.Port, "/port") Then Return 1

                        Case "/setarchivepath"
                            SetAction("setarchivepath", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.path, "/setarchivepath") Then Return 1


                        Case "/fontimport"
                            SetAction("fontimport", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.path, "/fontimport") Then Return 1

                        Case "/refreshdependencies"
                            SetAction("refreshdependencies", sAction)
                            AdvanceModeIfSafe(args, iCount, mode, ArgMode.force, "/refreshdependencies", False)

                        Case "/configencrypt"
                            SetAction("configencrypt", sAction)
                            AdvanceModeIfSafe(args, iCount, mode, ArgMode.configencrypt, "/configencrypt", False)

                        Case "/forceconfigencrypt"
                            forceConfigEncryptParam = True
                            AdvanceModeIfSafe(args, iCount, mode, ArgMode.forceconfigencrypt, "/forceconfigencrypt", False)

                        Case "/loginsession"
                            SetAction("loginsession", sAction)

                        Case "/listlogins"
                            listloginsParam = True
                            AdvanceModeIfSafe(args, iCount, mode, ArgMode.listlogins, "/listlogins", False)

                        Case "/schedule"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.schedule, "/schedule") Then Return 1

                        Case "/viewschedtimetable"
                            SetAction("viewschedlist", sAction)
                            scheduleListType = scheduleListType.Timetable
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.viewschedlist, "/viewschedtimetable") Then Return 1

                        Case "/viewschedreport"
                            SetAction("viewschedlist", sAction)
                            scheduleListType = scheduleListType.Report
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.viewschedlist, "/viewschedreport") Then Return 1

                        Case "/startschedule"
                            SetAction("startschedule", sAction)

                        Case "/format"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.exportformat, "/format") Then Return 1

                        Case "/deleteschedule"
                            SetAction("deleteschedule", sAction)

                        Case "/reencryptdata"
                            SetAction("reencryptdata", sAction)

                        Case "/batchsize"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.batchsize, "/batchsize") Then Return 1

                        Case "/maxbatches"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.maxbatches, "/maxbatches") Then Return 1

                        Case "/setallowanonresources"
                            SetAction("setallowanonresources", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.allowanonresourcesflag, "/setallowanonresources") Then Return 1

                        Case "/enforcecontrollinguserpermissions"
                            SetAction("enforcecontrollinguserpermissions", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.enforcecontrollinguserpermissionsflag, "/enforcecontrollinguserpermissions") Then Return 1

                        Case "/createcredential"
                            SetAction("createcredential", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.credentialname, "/createcredential") Then Return 1

                        Case "/updatecredential"
                            SetAction("updatecredential", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.credentialname, "/updatecredential") Then Return 1

                        Case "/setcredentialproperty"
                            SetAction("setcredentialproperty", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.credentialname, "/setcredentialproperty") Then Return 1

                        Case "/username"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.credentialusername, "/username") Then Return 1

                        Case "/password"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.credentialpassword, "/password") Then Return 1

                        Case "/expirydate"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.expirydate, "/expirydate") Then Return 1

                        Case "/invalid"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.invalid, "/invalid") Then Return 1

                        Case "/description"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.description, "/description") Then Return 1

                        Case "/credentialtype"
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.credentialType, "/credentialtype") Then Return 1

                        Case "/resourcestatus"
                            SetAction("resourcestatus", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.resourcename, "/resourecestatus") Then Return 1

                        Case "/installlocaldb"
                            SetAction("installlocaldb", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.localdbusername, "/installlocaldb") Then Return 1

                        Case "/disablewelcome"
                            SetAction("disablewelcome", sAction)

                        Case "/createmappedactivedirectorysysadmin"
                            SetAction("createmappedactivedirectorysysadmin", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.mappedactivedirectoryusername, "/createmappedactivedirectorysysadmin") Then Return 1

                        Case "/setactivedirectoryauth"
                            SetAction("setactivedirectoryauth", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.setactivedirectoryauth, "/setactivedirectoryauth") Then Return 1

                        Case "/mapauthenticationserverusers"
                            SetAction("mapauthenticationserverusers", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.mapauthenticationserverusersInputCsvPath, "/mapauthenticationserverusers") Then Return 1

                        Case "/getblueprismtemplateforusermapping"
                            SetAction("getblueprismtemplateforusermapping", sAction)
                            If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.getblueprismtemplateforusermapping, "/getblueprismtemplateforusermapping") Then Return 1

                        Case Else
                            Console.WriteLine(My.Resources.InvalidCommandLineArgument0, s)
                            Return 1
                    End Select

                Case ArgMode.locale
                    sLocale = s
                    Try
                        Threading.Thread.CurrentThread.CurrentUICulture = New Globalization.CultureInfo(sLocale)
                        Threading.Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo(sLocale)
                    Catch ex As System.Globalization.CultureNotFoundException
                        Console.WriteLine(My.Resources.NotAValidCultureName)
                        Return 1
                    Catch ex2 As IndexOutOfRangeException
                        Console.WriteLine(My.Resources.LocaleMissing)
                        Return 1
                    End Try


                    mode = ArgMode.nextswitch

                Case ArgMode.commandtimeout
                    cmdTimeout = Integer.Parse(s)
                    mode = ArgMode.nextswitch

                Case ArgMode.maxdbver
                    If Not Integer.TryParse(s, maxDBVer) Then
                        Console.WriteLine(My.Resources.InvalidMaximumDatabaseVersionSpecified)
                        Return 1
                    End If
                    mode = ArgMode.nextswitch

                Case ArgMode.evname
                    evname = s
                    If sAction = "setev" Then
                        If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.evdatatype, "/setev") Then Return 1
                    Else
                        mode = ArgMode.nextswitch
                    End If

                Case ArgMode.evdatatype
                    evdatatype = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.evvalue, "/setev") Then Return 1
                Case ArgMode.evvalue
                    evvalue = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.evdesc, "/setev") Then Return 1
                Case ArgMode.evdesc
                    evdesc = s
                    mode = ArgMode.nextswitch

                Case ArgMode.schedule

                    ' An arbitrary number of schedules could be provided here.
                    ' Keep eating them up until there are no more... at that
                    ' point reset the mode to look again for actions.
                    scheduleNames.Add(s)

                    ' Set mode back to default if no more schedules to process
                    ' Make sure no error messages are output if 'not safe'
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.schedule, "/schedule", False) Then mode = ArgMode.nextswitch
                    ' Otherwise, stay in 'schedule' mode to pick up the next one

                Case ArgMode.viewschedlist
                    ' /viewschedtimetable {name} | {number-of-days} {date}
                    ' -or-
                    ' /viewschedreport {name} | {number-of-days} {date}

                    ' First, check if we have an argument following this one
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.viewschedlistdate, "/" & sAction, False) Then
                        ' No, then assume this is a {name}
                        scheduleListName = s
                        mode = ArgMode.nextswitch
                    Else
                        ' Yes, assume this is a {number-of-days} with {date} to follow
                        If Not Integer.TryParse(s, scheduleListDays) OrElse scheduleListDays < 0 Then
                            Return Err(My.Resources.WithViewsched0NoOfDaysDateTheNumberOfDaysMustBeZeroOrAPositiveNumber,
                             If(scheduleListType = scheduleListType.Report, "report", "timetable"))
                        End If
                    End If

                Case ArgMode.viewschedlistdate
                    Try
                        scheduleListDate = Date.ParseExact(s, "yyyyMMdd", Nothing)
                    Catch fe As FormatException
                        Return Err(My.Resources.InvalidDateSpecified0ItMustBeInTheFormatYyyyMMdd, s)
                    End Try
                    mode = ArgMode.nextswitch

                Case ArgMode.exportformat
                    If s.Equals("txt", StringComparison.InvariantCultureIgnoreCase) Then
                        exportFormat = "txt"
                    ElseIf s.Equals("csv", StringComparison.InvariantCultureIgnoreCase) Then
                        exportFormat = "csv"
                    Else
                        Return Err(My.Resources.TheFormatSwitchOnlySupportsCsvOrTxt)
                    End If
                    mode = ArgMode.nextswitch

                Case ArgMode.timeout
                    If Not Integer.TryParse(s, timeout) Then
                        Console.WriteLine(My.Resources.InvalidTimeoutSpecified)
                        Return 1
                    End If
                    mode = ArgMode.nextswitch
                Case ArgMode.run, ArgMode.publish, ArgMode.unpublish, ArgMode.unpublishws, ArgMode.process
                    sRunProcess = s
                    mode = ArgMode.nextswitch
                Case ArgMode.publishws
                    sRunProcess = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.webservicename, "/publishws", False) Then mode = ArgMode.nextswitch
                Case ArgMode.wslog
                    sWSLog = s
                    mode = ArgMode.nextswitch
                Case ArgMode.createdb, ArgMode.upgradedb
                    sModifyDBPwd = New SafeString(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.genivbowrapper
                    sIVBOToWrap = s
                    mode = ArgMode.nextswitch
                Case ArgMode.objectname
                    sObjectName = s
                    mode = ArgMode.nextswitch
                Case ArgMode.webservicename
                    sWebServiceName = s
                    If sAction = "regwebservice" Then
                        If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.webserviceurl, "/regwebservice") Then Return 1
                    Else
                        mode = ArgMode.nextswitch
                    End If
                Case ArgMode.webserviceurl
                    sWebServiceURL = s
                    mode = ArgMode.nextswitch
                Case ArgMode.queuename
                    queueName = s
                    mode = ArgMode.nextswitch
                Case ArgMode.queuekey
                    queueKey = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.queuerunning, "/createqueue") Then Return 1
                Case ArgMode.queuerunning
                    queueRunning = Boolean.Parse(s)
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.queuemaxattempts, "/createqueue") Then Return 1
                Case ArgMode.queuemaxattempts
                    queueMaxAttempts = Integer.Parse(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.queuefilter
                    queueFilter = s
                    mode = ArgMode.nextswitch
                Case ArgMode.queueencryption
                    queueEncrypter = s
                    mode = ArgMode.nextswitch
                Case ArgMode.path
                    sPath = s
                    mode = ArgMode.nextswitch
                Case ArgMode.todate
                    Try
                        toDate = Date.ParseExact(s, "yyyyMMdd", Nothing)
                    Catch ex As Exception
                        Return Err(My.Resources.InvalidDateSpecifiedForTodate)
                    End Try
                    mode = ArgMode.nextswitch
                Case ArgMode.fromdate
                    Try
                        fromDate = Date.ParseExact(s, "yyyyMMdd", Nothing)
                    Catch ex As Exception
                        Return Err(My.Resources.InvalidDateSpecifiedForFromdate)
                    End Try
                    mode = ArgMode.nextswitch
                Case ArgMode.fromrev
                    Try
                        fromRev = Integer.Parse(s)
                    Catch ex As Exception
                        Return Err(My.Resources.InvalidRevisionSpecifiedForFromrev)
                    End Try
                    If fromRev < 10 AndAlso fromRev <> 0 Then
                        Return Err(My.Resources.ValueForFromrevMustBe10OrMoreOr0ToCreate)
                    End If
                    mode = ArgMode.nextswitch
                Case ArgMode.torev
                    Try
                        toRev = Integer.Parse(s)
                    Catch ex As Exception
                        Return Err(My.Resources.InvalidRevisionSpecifiedForTorev)
                    End Try
                    If toRev < 11 AndAlso toRev <> 0 Then
                        Return Err(My.Resources.ValueForTorevMustBe11OrMoreOr0ForCurrent)
                    End If
                    mode = ArgMode.nextswitch
                Case ArgMode.age
                    age = s
                    mode = ArgMode.nextswitch
                Case ArgMode.connectionmode
                    connMode = s
                    mode = ArgMode.nextswitch
                Case ArgMode.ordered
                    ordered = Boolean.Parse(s)
                    orderedChangeRequested = True
                    mode = ArgMode.nextswitch
                Case ArgMode.secure
                    secureServer = s
                    mode = ArgMode.nextswitch
                Case ArgMode.activedirectoryadmingroup
                    sActiveDirectoryAdminGroup = s
                    mode = ArgMode.nextswitch
                Case ArgMode.activedirectorydomain
                    sDomainName = s
                    mode = ArgMode.nextswitch
                Case ArgMode.sessionid
                    sSessionID = s
                    mode = ArgMode.nextswitch
                Case ArgMode.resourcename
                    resourceName = s
                    If String.IsNullOrWhiteSpace(resourceName) Then Return Err("Must provide a resource name or All")
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.limitnumber, "/resourcestatus", False) Then mode = ArgMode.nextswitch
                Case ArgMode.limitnumber
                    limitNumber = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.limittype, "/resourcestatus") Then Return 1
                Case ArgMode.limittype
                    limitType = s
                    mode = ArgMode.nextswitch
                Case ArgMode.licensefile
                    sLicenseFile = s
                    mode = ArgMode.nextswitch
                Case ArgMode.filespec
                    sFilespec = s
                    mode = ArgMode.nextswitch
                Case ArgMode.import, ArgMode.importrelease
                    sImportFile = s
                    mode = ArgMode.nextswitch
                Case ArgMode.export
                    sExportProcess = s
                    mode = ArgMode.nextswitch
                Case ArgMode.exportpackage
                    packageToExport = s
                    mode = ArgMode.nextswitch
                Case ArgMode.release
                    releaseName = s
                    mode = ArgMode.nextswitch
                Case ArgMode.username
                    sUserName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.userpassword, "/user") Then Return 1
                Case ArgMode.userpassword
                    sPassword = New SafeString(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.wsuser
                    wsuser = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.wspassword, "/wsauth") Then Return 1
                Case ArgMode.wspassword
                    wspassword = New SafeString(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.resource
                    sTargetResource = s
                    mode = ArgMode.nextswitch
                Case ArgMode.pool
                    sPool = s
                    mode = ArgMode.nextswitch
                Case ArgMode.startp
                    sStartParms = s
                    mode = ArgMode.nextswitch
                Case ArgMode.forceid
                    sForceID = s
                    mode = ArgMode.nextswitch
                Case ArgMode.servername
                    servername = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.connection, "/serverconfig") Then Return 1
                Case ArgMode.connection
                    sConnection = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.serverport, "/serverconfig") Then Return 1
                Case ArgMode.rabbitmqconnection
                    rabbitMQConnection = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.rabbitmqurl, "/rabbitmqconfig") Then Return 1
                Case ArgMode.rabbitmqurl
                    rabbitMQHostUrl = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.rabbitmqusername, "/rabbitmqconfig") Then Return 1
                Case ArgMode.rabbitmqusername
                    rabbitMQUserName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.rabbitmqpassword, "/rabbitmqconfig") Then Return 1
                Case ArgMode.rabbitmqpassword
                    rabbitMQPassword = s
                    mode = ArgMode.nextswitch
                Case ArgMode.ascrservername
                    ascrServerName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrconntype, "/ascrconfig") Then Return 1
                Case ArgMode.ascrconntype
                    ascrConnType = CInt(s)
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrhostname, "/ascrconfig") Then Return 1
                Case ArgMode.ascrhostname
                    ascrHostName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrport, "/ascrconfig") Then Return 1
                Case ArgMode.ascrport
                    ascrPort = CInt(s)
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrmode, "/ascrconfig") Then Return 1
                Case ArgMode.ascrmode
                    ascrMode = CInt(s)
                    If ascrMode.Equals(1) Then
                        If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrcertname, "/ascrconfig") Then Return 1
                    Else
                        mode = ArgMode.nextswitch
                    End If
                Case ArgMode.ascrcertname
                    ascrCertname = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrclientcertname, "/ascrconfig") Then Return 1
                Case ArgMode.ascrclientcertname
                    ascrClientCertName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrservercertstore, "/ascrconfig") Then Return 1
                Case ArgMode.ascrservercertstore
                    ascrServerStore = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.ascrclientcertstore, "/ascrconfig") Then Return 1
                Case ArgMode.ascrclientcertstore
                    ascrClientStore = s
                    mode = ArgMode.nextswitch
                Case ArgMode.Port, ArgMode.serverport
                    Try
                        iPort = CInt(s)
                    Catch e As Exception
                        Console.WriteLine(My.Resources.InvalidPortNumber0, s)
                        Return 1
                    End Try
                    mode = ArgMode.nextswitch
                Case ArgMode.clsid
                    sObjectCLSID = s
                    mode = ArgMode.nextswitch
                Case ArgMode.force
                    If s.ToLower(CultureInfo.InvariantCulture) <> "force" Then
                        Console.WriteLine(My.Resources.InvalidOption0, s)
                        Return 1
                    End If
                    forceRefresh = True
                    mode = ArgMode.nextswitch

                Case ArgMode.configencrypt
                    configEncryptParam = s
                    mode = ArgMode.nextswitch

                Case ArgMode.forceconfigencrypt
                    mode = ArgMode.nextswitch

                Case ArgMode.listlogins
                    mode = ArgMode.nextswitch

                Case ArgMode.dbconnection
                    connectionName = s
                    mode = ArgMode.nextswitch
                Case ArgMode.batchsize
                    Try
                        batchSize = CInt(s)
                        If batchSize < 1 Then Throw New InvalidArgumentException()
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.InvalidBatchSize0MustBeAPositiveInteger, s)
                        Return 1
                    End Try
                    mode = ArgMode.nextswitch
                Case ArgMode.maxbatches
                    Try
                        maxBatches = CInt(s)
                        If maxBatches < 1 Then Throw New InvalidArgumentException()
                    Catch ex As InvalidArgumentException
                        Console.WriteLine(My.Resources.InvalidNumberOfBatches0MustBeAPositiveInteger, s)
                        Return 1
                    End Try
                    mode = ArgMode.nextswitch
                Case ArgMode.allowanonresourcesflag
                    allowAnonResources = Boolean.Parse(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.enforcecontrollinguserpermissionsflag
                    enforceControllingUserPermissions = Boolean.Parse(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.encryptionschemename
                    encSchemeName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.encryptionschemealgorithm, "/serverconfig", False) Then _
                        mode = ArgMode.nextswitch
                Case ArgMode.encryptionschemealgorithm
                    If Not [Enum].TryParse(s, encSchemeAlgorithm) OrElse
                     Not [Enum].IsDefined(GetType(EncryptionAlgorithm), encSchemeAlgorithm) OrElse
                     encSchemeAlgorithm = EncryptionAlgorithm.None OrElse
                     Not clsFIPSCompliance.CheckForFIPSCompliance(encSchemeAlgorithm) Then
                        Console.WriteLine(My.Resources.InvalidEncryptionMethod0SeeHelpForValidValues, s)
                        Return 1
                    End If
                    mode = ArgMode.nextswitch
                Case ArgMode.credentialname
                    credentialName = s
                    Select Case sAction
                        Case "updatecredential"
                            mode = ArgMode.nextswitch
                        Case "createcredential"
                            'Username is also required to create a new credential
                            If Not AdvanceModeIfSafe(
                                args, iCount, mode, ArgMode.credentialusername, "username") Then Return 1
                        Case "setcredentialproperty"
                            If Not AdvanceModeIfSafe(
                                args, iCount, mode, ArgMode.credentialpropertyname, "propertyname") Then Return 1
                    End Select
                Case ArgMode.credentialusername
                    credentialUsername = s
                    Select Case sAction
                        Case "updatecredential"
                            mode = ArgMode.nextswitch
                        Case "createcredential"
                            'Password is also required to create a new credential
                            If Not AdvanceModeIfSafe(
                                args, iCount, mode, ArgMode.credentialpassword, "password") Then Return 1
                    End Select
                Case ArgMode.credentialpassword
                    credentialPassword = New SafeString(s)

                    mode = ArgMode.nextswitch

                Case ArgMode.expirydate
                    Try
                        expiryDate = Date.ParseExact(s, "yyyyMMdd", Nothing)
                    Catch ex As Exception
                        Return Err(My.Resources.InvalidDateSpecifiedForExpirydate)
                    End Try
                    mode = ArgMode.nextswitch
                Case ArgMode.invalid
                    Try
                        isInvalid = Boolean.Parse(s)
                    Catch ex As Exception
                        Return Err(My.Resources.InvalidValueSpecifiedForInvalid)
                    End Try
                    isInvalidSet = True
                    mode = ArgMode.nextswitch
                Case ArgMode.description
                    description = s
                    mode = ArgMode.nextswitch
                Case ArgMode.credentialpropertyname
                    credentialPropertyName = s
                    If Not AdvanceModeIfSafe(
                                args, iCount, mode, ArgMode.credentialpropertyvalue, "propertyvalue") Then Return 1
                Case ArgMode.credentialpropertyvalue
                    credentialPropertyValue = New SafeString(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.credentialType
                    If s = String.Empty Then _
                        Return Err(My.Resources.NoCredentialTypeSpecified)

                    credentialTypeName = s
                    mode = ArgMode.nextswitch
                Case ArgMode.localdbusername
                    sLocalDbUserName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.localdbpassword, "/installlocaldb") Then Return 1
                Case ArgMode.localdbpassword
                    sLocalDbPassword = New SafeString(s)
                    mode = ArgMode.nextswitch
                Case ArgMode.disablewelcome
                    mode = ArgMode.nextswitch
                Case ArgMode.mappedactivedirectoryusername
                    adUserName = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.mappedactivedirectorysid, "/createmappedactivedirectorysysadmin") Then Return 1
                Case ArgMode.mappedactivedirectorysid
                    adSid = s
                    mode = ArgMode.nextswitch
                Case ArgMode.setactivedirectoryauth
                    If Not Boolean.TryParse(s, adEnableAuth) Then
                        Return Err(My.Resources.BooleanMustBeTrueOrFalse)
                    End If
                    mode = ArgMode.nextswitch
                Case ArgMode.mapauthenticationserverusersInputCsvPath
                    mapAuthenticationServerUsersInputCsvPath = s
                    If Not AdvanceModeIfSafe(args, iCount, mode, ArgMode.mapauthenticationserverusersOutputCsvPath, "/mapauthenticationserverusers", False) Then _
                        mode = ArgMode.nextswitch
                Case ArgMode.mapauthenticationserverusersOutputCsvPath
                    mapAuthenticationServerUsersOutputCsvPath = s
                    mode = ArgMode.nextswitch
                Case ArgMode.getblueprismtemplateforusermapping
                    BluePrismUserTemplateOutputCsvPath = s
                    mode = ArgMode.nextswitch

            End Select
        Next

        If mode <> ArgMode.nextswitch Then
            Console.WriteLine(My.Resources.InvalidCommandLineArguments)
            Return 1
        End If

        'Make sure a valid action was specified...
        If sAction Is Nothing Then
            Console.WriteLine(My.Resources.NoActionSpecifiedUseHelpForHelp)
            Return 1

        ElseIf sAction = "help" Then ' Special case - no need to initialise AMI, load the INI file, connect to the database if we're only showing help text.

            'NOTE: The following line is needed, in this exact format, for automated tools to
            'be able to get the BP version number, because we don't have any kind of '/version'
            'option. --updatetraininglicense in qacontrol/Server.py uses this, for example.
            WriteWithNewlines(My.Resources.x0CommandLineInterfaceVersion1, ApplicationProperties.ApplicationName, GetType(AutomateC).Assembly.GetName.Version.ToString)
            WriteWithNewlines(String.Format(My.Resources.CopyrightBluePrismLimited20040, Date.Now.Year))
            WriteWithNewlines(My.Resources.Help_Title)
            WriteWithNewlines(My.Resources.Actions)
            WriteWithNewlines(My.Resources.Help_help)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_locale)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_createdb)
            WriteWithNewlines(My.Resources.Help_replacedb)
            WriteWithNewlines(My.Resources.Help_upgradedb)
            WriteWithNewlines(My.Resources.Help_configuredb)
            WriteWithNewlines(My.Resources.Help_license)
            WriteWithNewlines(My.Resources.Help_removelicense)
            WriteWithNewlines(My.Resources.Help_regobject)
            WriteWithNewlines(My.Resources.Help_regwebservice)
            WriteWithNewlines(My.Resources.Help_unregwebservice)
            ' bug #4529 : /unexpire isn't displayed in the usage output.
            ' WriteWithNewlines("    /unexpire")
            WriteWithNewlines(My.Resources.Help_report)
            WriteWithNewlines(My.Resources.Help_rolereport)
            WriteWithNewlines(My.Resources.Help_elementusage)
            WriteWithNewlines(My.Resources.Help_listprocesses)
            WriteWithNewlines(My.Resources.Help_refreshdependencies)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_import)
            WriteWithNewlines(My.Resources.Help_importrelease)
            WriteWithNewlines(My.Resources.Help_export)
            WriteWithNewlines(My.Resources.Help_exportpackage)
            WriteWithNewlines(My.Resources.Help_publish)
            WriteWithNewlines(My.Resources.Help_unpublish)
            WriteWithNewlines(My.Resources.Help_publishws)
            WriteWithNewlines(My.Resources.Help_unpublishws)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_fontimport)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_createqueue)
            WriteWithNewlines(My.Resources.Help_setencrypt)
            WriteWithNewlines(My.Resources.Help_resetencrypt)
            WriteWithNewlines(My.Resources.Help_exportqueue)
            WriteWithNewlines(My.Resources.Help_queueclearworked)
            WriteWithNewlines(My.Resources.Help_deletequeue)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_poolcreate)
            WriteWithNewlines(My.Resources.Help_pooldelete)
            WriteWithNewlines(My.Resources.Help_pooladd)
            WriteWithNewlines(My.Resources.Help_poolremove)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_startschedule)
            WriteWithNewlines(My.Resources.Help_deleteschedule)
            WriteWithNewlines(My.Resources.Help_viewschedtimetable)
            WriteWithNewlines(My.Resources.Help_viewschedreport)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_setev)
            WriteWithNewlines(My.Resources.Help_deleteev)
            WriteWithNewlines(My.Resources.Help_archive)
            WriteWithNewlines(My.Resources.Help_restorearchive)
            WriteWithNewlines(My.Resources.Help_setarchivepath)
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_run)
            WriteWithNewlines(My.Resources.Help_getauthtoken)
            WriteWithNewlines(My.Resources.Help_status)
            WriteWithNewlines(My.Resources.Help_resourceStatus)
            WriteWithNewlines(My.Resources.Help_getlog)
            WriteWithNewlines(My.Resources.Help_requeststop)
            WriteWithNewlines(My.Resources.Help_wslog)
            WriteWithNewlines(My.Resources.Help_getauditlog)
            WriteWithNewlines(My.Resources.Help_getbod)
            WriteWithNewlines(My.Resources.Help_genivbowrapper)
            WriteWithNewlines(My.Resources.Help_serverconfig)
            WriteWithNewlines(My.Resources.Help_ascrconfig)
            WriteWithNewlines(My.Resources.Help_reencryptdata)
            WriteWithNewlines(My.Resources.Help_configencrypt)
            WriteWithNewlines(My.Resources.Help_createcredential, String.Join(", ", CredentialType.GetAll().Select(Function(c) c.Name)))
            WriteWithNewlines(My.Resources.Help_updatecredential, String.Join(", ", CredentialType.GetAll().Select(Function(c) c.Name)))
            WriteWithNewlines(My.Resources.Help_setcredentialproperty)

            If mAuthenticationServerFeatureEnabled Then
                WriteWithNewlines(My.Resources.Help_mapauthenticationserverusers)
            End If
            
            WriteWithNewlines(My.Resources.Help_getblueprismtemplateforusermapping, "/getblueprismtemplateforusermapping", "outputcsvpath")

            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_Options)
            WriteWithNewlines(My.Resources.HelpOptions_dbconname)
            WriteWithNewlines(My.Resources.HelpOptions_user)
            WriteWithNewlines(My.Resources.HelpOptions_sso)
            WriteWithNewlines(My.Resources.HelpOptions_wsauth)
            WriteWithNewlines(My.Resources.HelpOptions_setaddomain)
            WriteWithNewlines(My.Resources.HelpOptions_setadadmingroup)
            WriteWithNewlines(My.Resources.HelpOptions_maxdbver)
            WriteWithNewlines(My.Resources.HelpOptions_startp)
            WriteWithNewlines(My.Resources.HelpOptions_resource)
            WriteWithNewlines(My.Resources.HelpOptions_port)
            WriteWithNewlines(My.Resources.HelpOptions_forceid)
            WriteWithNewlines(My.Resources.HelpOptions_overwrite)
            WriteWithNewlines(My.Resources.HelpOptions_queuename)
            WriteWithNewlines(My.Resources.HelpOptions_queuefilter)
            WriteWithNewlines(My.Resources.HelpOptions_clearexported)
            WriteWithNewlines(My.Resources.HelpOptions_from)
            WriteWithNewlines(My.Resources.HelpOptions_to)
            WriteWithNewlines(My.Resources.HelpOptions_age)
            WriteWithNewlines(My.Resources.HelpOptions_process)
            WriteWithNewlines(My.Resources.HelpOptions_timeout)
            WriteWithNewlines(My.Resources.HelpOptions_objectname)
            WriteWithNewlines(My.Resources.HelpOptions_connectionmode)
            For Each e As ServerConnection.Mode In [Enum].GetValues(GetType(ServerConnection.Mode))
                WriteWithNewlines("                      {0} = {1}", CInt(e), TypeDescriptor.GetConverter(e).ConvertToString(e))
            Next
            WriteWithNewlines(My.Resources.HelpOptions_encryptionscheme)
            For Each alg As EncryptionAlgorithm In [Enum].GetValues(GetType(EncryptionAlgorithm))
                If Not alg = EncryptionAlgorithm.None AndAlso clsFIPSCompliance.CheckForFIPSCompliance(alg) Then
                    WriteWithNewlines("                                           {0} = {1}", CInt(alg), alg.GetFriendlyName(True))
                End If
            Next
            WriteWithNewlines(My.Resources.HelpOptions_ordered)
            WriteWithNewlines(My.Resources.HelpOptions_secure)
            WriteWithNewlines(My.Resources.HelpOptions_delete)
            WriteWithNewlines(My.Resources.HelpOptions_setallowanonresources)
            WriteWithNewlines(My.Resources.HelpOptions_enforececontrollinguserpermissions)
            WriteWithNewlines(My.Resources.HelpOptions_password)
            WriteWithNewlines(My.Resources.HelpOptions_description)
            WriteWithNewlines(My.Resources.HelpOptions_username)
            WriteWithNewlines(My.Resources.HelpOptions_expirydate)
            WriteWithNewlines(My.Resources.HelpOptions_invalid)
            WriteWithNewlines(My.Resources.HelpOptions_credentialtype, String.Join(", ", CredentialType.GetAll().Select(Function(c) c.Name)))
            WriteWithNewlines("")
            WriteWithNewlines(My.Resources.Help_Notes)

            Return 0

        End If

        'Load options...
        Dim configOptions = Options.Instance
        Try
            configOptions.Init(ConfigLocator.Instance(bUserOpts))
        Catch ex As Exception
            Console.WriteLine(My.Resources.ErrorLoadingConfigurationOptions0, ex.Message)
            Return 1
        End Try

        'Set up APC
        clsAppCore.InitAPC()

        If sActiveDirectoryAdminGroup <> "" Xor sDomainName <> "" Then
            Console.WriteLine(My.Resources.CannotUseJustOneOfSetadadmingroupAndSetaddomainIfYouUseOneThenTheOtherMustAsWell)
            Return 1
        End If
        If sActiveDirectoryAdminGroup <> "" Or sDomainName <> "" Then
            If sAction <> "createdb" And sAction <> "replacedb" Then
                Console.WriteLine(My.Resources.TheSwitchesSetadadmingroupAndSetaddomainAndCanOnlyBeUsedWithCreatedbOrReplacedb)
                Return 1
            End If
        End If


        'Set db connection (if specified)
        If connectionName IsNot Nothing Then
            Dim found As Boolean = False
            For Each conn As clsDBConnectionSetting In configOptions.Connections
                If conn.ConnectionName.Equals(connectionName, StringComparison.CurrentCultureIgnoreCase) Then
                    configOptions.DbConnectionSetting = conn
                    found = True : Exit For
                End If
            Next
            If Not found Then
                Console.WriteLine(My.Resources.SpecifiedConnectionName0NotFound, connectionName)
                Return 1
            End If
        End If

        'Do authentication check
        Dim result As LoginResult
        If Not (sUserName = "" AndAlso sPassword.IsEmpty) Then

            Dim connectionSetting As clsDBConnectionSetting = configOptions.DbConnectionSetting
            If Not connectionSetting.IsComplete Then
                Console.WriteLine(My.Resources.DatabaseConnectionStringNotComplete)
                Return 1
            End If

            ServerFactory.ClientInit(connectionSetting)

            'Check database...
            Try
                ServerFactory.CurrentConnectionValid()
            Catch ex As Exception
                Console.WriteLine(My.Resources.DatabaseError0, ex.Message)
                Return 1
            End Try

            Try
                result = User.Login(ResourceMachine.GetName(), sUserName, sPassword, sLocale, Nothing, True)
                Select Case result.Code
                    Case LoginResultCode.Success
                        'Do nothing
                    Case LoginResultCode.PasswordExpired
                        If sAction = "unexpire" Then
                            Try
                                gSv.UpdatePasswordExpiryDate(sUserName, sPassword)
                                Console.WriteLine(My.Resources.UpdatedExpiryDate)
                                Return 0
                            Catch ex As Exception
                                Return Err(My.Resources.FailedToUpdateExpiryDate0, ex.Message)
                            End Try
                        Else
                            Return Err(My.Resources.LoginFailed0, result.Description)
                        End If
                    Case Else
                        Return Err(My.Resources.LoginFailed0, result.Description)
                End Select
            Catch ex As UnknownLoginException
                Return Err(My.Resources.LoginFailed0, ex)
            End Try
        ElseIf singleSignon Then

            Dim connectionSetting As clsDBConnectionSetting = configOptions.DbConnectionSetting
            If Not connectionSetting.IsComplete Then
                Console.WriteLine(My.Resources.DatabaseConnectionStringNotComplete)
                Return 1
            End If

            ServerFactory.ClientInit(connectionSetting)

            'Check database...
            Try
                ServerFactory.CurrentConnectionValid()
            Catch ex As Exception
                Console.WriteLine(My.Resources.DatabaseError0, ex.Message)
                Return 1
            End Try

            Dim databaseType = gSv.DatabaseType()
            Try
                Dim machine = ResourceMachine.GetName()

                result = If(databaseType = databaseType.SingleSignOn,
                            User.Login(machine, sLocale, Nothing, True),
                            User.LoginWithMappedActiveDirectoryUser(machine, sLocale, Nothing, True))

                If result.Code <> LoginResultCode.Success Then _
                    Return Err(My.Resources.LoginFailed0, result.Description)

            Catch ex As UnknownLoginException
                Return Err(My.Resources.LoginFailed0, ex.Message)
            End Try

        End If

        If mServerStatusUpdater Is Nothing Then
            mServerStatusUpdater = New Threading.Timer(AddressOf UpdateAutomateCAlive, Nothing, 0, 120000)
        End If

        ' If the /age option has been specified, convert this into /from and /to values.
        ' This information will either be used for:
        ' - archiving and restoring archives, or
        ' - deleting worked items from a work queue
        If age IsNot Nothing Then
            If fromDate <> DateTime.MinValue OrElse toDate <> DateTime.MaxValue Then
                Console.WriteLine(My.Resources.ErrorIfYouSpecifyAgeYouCanTAlsoSpecifyToOrFrom)
                Return 1
            End If
            Dim i As Integer
            If Not Integer.TryParse(age.Substring(0, age.Length - 1), i) OrElse i < 1 Then
                Console.WriteLine(My.Resources.ErrorInvalidAgeValueSpecified)
                Return 1
            End If
            'Need to base all the calculated 'to' dates on tomorrow, since the
            'implied time component is midnight - in other words, the 'to' date
            'is not inclusive.
            Dim tom As Date = DateAdd(DateInterval.Day, 1, Date.Today())
            Select Case age.Substring(age.Length - 1, 1)
                Case "y"
                    toDate = DateAdd(DateInterval.Year, -i, tom)
                Case "m"
                    toDate = DateAdd(DateInterval.Month, -i, tom)
                Case "w"
                    toDate = DateAdd(DateInterval.Day, -i * 7, tom)
                Case "d"
                    toDate = DateAdd(DateInterval.Day, -i, tom)
                Case Else
                    Console.WriteLine(My.Resources.ErrorInvalidAgeTypeSpecified)
                    Return 1
            End Select
        End If

        ' Set up the scheduler store that we'll be using later if any of the
        ' schedule actions are specified.
        Dim store As DatabaseBackedScheduleStore = Nothing
        If sAction.Contains("sched") Then
            store = New DatabaseBackedScheduleStore(gSv)
            Dim insched As New InertScheduler(store) ' Don't care about keeping the reference
        End If

        'Perform the requested action...
        Try
            Select Case sAction
                Case "unexpire"
                    'If we got here, it's because the unexpire at login above wasn't triggered,
                    'which is because the user wasn't expired. We just update the expiry now
                    'and exit.
                    Try
                        gSv.UpdatePasswordExpiryDate(sUserName, sPassword)
                        Console.WriteLine(My.Resources.UpdatedExpiryDate)
                        Return 0
                    Catch ex As Exception
                        Return Err(My.Resources.FailedToUpdateExpiryDate0, ex.Message)
                    End Try

                Case "setarchivepath"
                    'Check permission
                    CheckLoggedInAndUserRole(Permission.SystemManager.GroupName)

                    configOptions.ArchivePath = sPath
                    Try
                        configOptions.Save()
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToSetArchivePath0, ex.Message)
                        Return 1
                    End Try
                    Console.WriteLine(My.Resources.ArchivePathSet)
                    Return 0

                Case "license", "removelicense"
                    'Check permission
                    CheckLoggedInAndUserRole(Permission.SystemManager.System.License)
                    Dim narr As String = My.Resources.Added

                    Try
                        If Not File.Exists(sLicenseFile) Then
                            Console.WriteLine(My.Resources.TheLicenseFile0DoesNotExist, sLicenseFile)
                            Return 1
                        End If
                        Dim key As String = File.ReadAllText(sLicenseFile)
                        Dim keys As List(Of KeyInfo)

                        If sAction = "license" Then
                            Dim keyToAdd = New KeyInfo(key, DateTime.UtcNow, User.CurrentId)
                            keys = gSv.AddLicenseKey(keyToAdd, sLicenseFile)
                        Else
                            keys = gSv.RemoveLicenseKey(New KeyInfo(key))
                            narr = My.Resources.Removed
                        End If
                        Licensing.SetLicenseKeys(keys)

                    Catch ex As Exception
                        Return Err(My.Resources.Error0, ex.Message)

                    End Try
                    Console.WriteLine(My.Resources.License0, narr)
                    Return 0

                Case "getauthtoken"
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToGetARemoteAuthenticationToken)
                        Return 1
                    End If

                    Dim authToken = gSv.RegisterAuthorisationToken()
                    Console.WriteLine(authToken.ToString())
                    Return 0

                Case "getbod"
                    'check username/password correctly supplied:
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToGenerateObjectDocumentation)
                        Return 1
                    End If

                    Using refs As New clsGroupBusinessObject(configOptions.GetExternalObjectsInfo())
                        Dim obj As clsBusinessObject = refs.FindObjectReference(sObjectName)
                        If obj Is Nothing Then
                            Console.WriteLine(My.Resources.CouldNotFindBusinessObjectCalled0, sObjectName)
                            Return 1
                        End If
                        Console.Write(obj.GetWikiDocumentation())
                        Return 0
                    End Using

                Case "serverconfig"
                    Try
                        'Set the default connection mode
                        Dim m As ServerConnection.Mode =
                            ServerConnection.Mode.WCFSOAPMessageWindows
                        If connMode <> "" Then
                            'If connection mode was passed in as an arg, then try parsing
                            If Not [Enum].TryParse(connMode, m) Then
                                Console.WriteLine(My.Resources.InvalidValueSpecifiedForConnectionmode)
                                Return 1
                            End If
                        ElseIf secureServer <> "" Then
                            'If no connection mode was passed in, check whether the deprecated
                            'server switch was used. If so, choose the appropriate
                            '.NET Remoting mode.
                            Dim secure As Boolean
                            If Not Boolean.TryParse(secureServer, secure) Then
                                Console.WriteLine(My.Resources.InvalidSecuritySetting)
                                Return 1
                            End If

                            m = If(secure, ServerConnection.Mode.DotNetRemotingSecure,
                                   ServerConnection.Mode.DotNetRemotingInsecure)
                        End If

                        'Create an encryption scheme to store with the server config
                        Dim schemesExist = False
                        Dim sv = configOptions.GetServerConfig(servername)
                        If sv IsNot Nothing Then
                            schemesExist = sv.EncryptionKeys.Count <> 0

                            If Not orderedChangeRequested Then
                                ordered = sv.Ordered IsNot Nothing AndAlso CType(sv.Ordered, Boolean)
                            End If
                        End If

                        Dim sch As clsEncryptionScheme = Nothing
                        If encSchemeName = "" Then
                            'If there are no schemes present and no scheme name override
                            'then create the default encryption scheme
                            If Not schemesExist Then _
                                sch = clsEncryptionScheme.DefaultEncrypter()
                        ElseIf schemesExist Then
                            'If encryption details specified ensure no schemes already setup
                            Return Err(My.Resources.EncryptionSchemesHaveAlreadyBeenDefinedForThisServerConfiguration)
                        Else
                            'Otherwise create a new scheme with the passed name, defaulting
                            'the algorithm if not specified
                            sch = New clsEncryptionScheme() With {
                                .Name = encSchemeName,
                                .Algorithm = If(encSchemeAlgorithm <> EncryptionAlgorithm.None,
                                                encSchemeAlgorithm, clsEncryptionScheme.DefaultEncryptionAlgorithm)}
                            sch.GenerateKey()
                        End If

                        If sch IsNot Nothing AndAlso Not clsFIPSCompliance.CheckForFIPSCompliance(sch.Algorithm) Then
                            Throw New InvalidOperationException(String.Format(My.Resources.InvalidEncryptionMethod0SeeHelpForValidValues, sch.AlgorithmName))
                        End If

                        configOptions.UpdateServerSettings(servername, sConnection, iPort, m, sch, ordered)
                        Console.WriteLine(My.Resources.UpdatedServerConfiguration)
                        Return 0

                    Catch ex As Exception
                        Return Err(My.Resources.UnableToUpdateServerConfiguration0, ex)
                    End Try

                Case "rabbitmqconfig"

                    Try
                        configOptions.UpdateRabbitMqConfiguration(rabbitMQConnection, rabbitMQHostUrl, rabbitMQUserName, rabbitMQPassword)
                        configOptions.Save()

                        Console.WriteLine(My.Resources.UpdatedRabbitMQConfiguration)
                        Return 0
                    Catch ex As Exception
                        Return Err(My.Resources.UnableToUpdateRabbitMQConfiguration0, ex)
                    End Try

                Case "ascrconfig"
                    Try

                        If ascrConnType < 1 OrElse ascrConnType > 2 Then
                            Return Err(My.Resources.InvalidCallbackProtocol)
                        End If

                        If ascrPort < 0 OrElse ascrPort > 99999 Then
                            Return Err(My.Resources.PortOutOfRange)
                        End If

                        If ascrMode < 0 OrElse ascrMode > 3 Then
                            Return Err(My.Resources.InvalidConnectionCallbackSecurityMode)
                        End If

                        If ascrMode = 1 Then
                            If Not [Enum].TryParse(Of StoreName)(ascrServerStore, Nothing)
                                Return Err(My.Resources.ServerStoreNotValid)
                            End If
                           
                            If Not [Enum].TryParse(Of StoreName)(ascrClientStore, Nothing)
                                Return Err(My.Resources.ClientStoreIsNotValid)
                            End If
                        End If

                        configOptions.UpdateASCRServerSettings(ascrServerName, ascrConnType, ascrHostName, ascrPort,
                                                                ascrMode, ascrCertname, ascrClientCertName, ascrServerStore, ascrClientStore)

                        Console.WriteLine(My.Resources.UpdatedASCRConfiguration)
                        Return 0
                    Catch ex As Exception
                        Return Err(My.Resources.UnableToUpdateASCRConfiguration0, ex)
                    End Try

                Case "setcommandtimeout"
                    Try
                        Dim timeoutStr As String =
                         CStr(IIf(cmdTimeout = 0, My.Resources.Infinity, cmdTimeout & My.Resources.S_seconds))

                        If configOptions.SqlCommandTimeout = cmdTimeout Then
                            Console.WriteLine(
                             My.Resources.CommandTimeoutAlreadySetTo0Ignoring,
                             timeoutStr)
                            Return 0
                        End If

                        If cmdTimeout < 0 Then Return Err(
                         My.Resources.YouMustProvideAValidValueForSetcommandtimeoutANumberRepresentingTheNumberOfSeco)

                        configOptions.SqlCommandTimeout = cmdTimeout
                        configOptions.Save()
                        Console.WriteLine(My.Resources.CommandTimeoutSetTo0, timeoutStr)
                        Return 0

                    Catch ex As Exception
                        Return Err(My.Resources.ErrorSavingCommandTimeout0, ex)
                    End Try

                Case "setcommandtimeoutlong"
                    Try
                        Dim timeoutStr As String =
                         CStr(IIf(cmdTimeout = 0, My.Resources.Infinity, cmdTimeout & My.Resources.S_seconds))

                        If configOptions.SqlCommandTimeout = cmdTimeout Then
                            Console.WriteLine(My.Resources.LongCommandTimeoutAlreadySetTo0Ignoring,
                             timeoutStr)
                            Return 0
                        End If

                        If cmdTimeout < 0 Then Return Err(My.Resources.YouMustProvideAValidValueForSetcommandtimeoutlongANumberRepresentingTheNumberOfSeco)

                        configOptions.SqlCommandTimeoutLong = cmdTimeout
                        configOptions.Save()
                        Console.WriteLine(My.Resources.LongCommandTimeoutSetTo0, timeoutStr)
                        Return 0

                    Catch ex As Exception
                        Return Err(My.Resources.ErrorSavingLongCommandTimeout0, ex)
                    End Try

                Case "setcommandtimeoutlog"
                    Try
                        Dim timeoutStr As String =
                         CStr(IIf(cmdTimeout = 0, My.Resources.Infinity, cmdTimeout & My.Resources.S_seconds))

                        If configOptions.SqlCommandTimeout = cmdTimeout Then
                            Console.WriteLine(My.Resources.LogCommandTimeoutAlreadySetTo0Ignoring, timeoutStr)
                            Return 0
                        End If

                        If cmdTimeout < 0 Then Return Err(My.Resources.YouMustProvideAValidValueForSetcommandtimeoutlogANumberRepresentingTheNumberOfSeco)

                        configOptions.SqlCommandTimeoutLog = cmdTimeout
                        configOptions.Save()
                        Console.WriteLine(My.Resources.LogCommandTimeoutSetTo0, timeoutStr)
                        Return 0

                    Catch ex As Exception
                        Return Err(My.Resources.ErrorSavingLogCommandTimeout0, ex)
                    End Try

                Case "rolereport"

                    'check username/password correctly supplied:
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToGenerateAReport)
                        Return 1
                    End If

                    Console.WriteLine(My.Resources.GeneratingReport)

                    Try
                        Dim rr As New clsRoleReporter()
                        Using sw As New StreamWriter(sFilespec)
                            rr.Generate(New List(Of Object), sw)
                        End Using
                        Console.WriteLine(My.Resources.ReportGenerated)
                        Return 0

                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToGenerateReport0, ex.Message)
                        Return 1
                    End Try

                Case "usagetree"
                    Try
                        Dim perms = Permission.ObjectStudio.ImpliedViewBusinessObject.Union(
                            Permission.ProcessStudio.ImpliedViewProcess)
                        CheckLoggedInAndUserRole(perms.ToArray())

                        Dim id As Guid = gSv.GetProcessIDByName(sRunProcess)
                        If id = Nothing Then Throw New InvalidOperationException(My.Resources.NoSuchProcess)
                        Dim sb As New StringBuilder()
                        Dim extRunModes As New Dictionary(Of String, BusinessObjectRunMode)
                        Dim externalObjectDetails = CType(configOptions.GetExternalObjectsInfo(), clsGroupObjectDetails)
                        Dim comGroup As New clsGroupObjectDetails(externalObjectDetails.Permissions)
                        For Each comObj In externalObjectDetails.Children
                            If TypeOf (comObj) Is clsCOMObjectDetails Then _
                                comGroup.Children.Add(comObj)
                        Next
                        Using objRefs As New clsGroupBusinessObject(comGroup, Nothing, Nothing)
                            extRunModes = objRefs.GetNonVBORunModes()
                        End Using
                        gSv.GetEffectiveRunMode(id, extRunModes, sb)
                        Console.WriteLine(sb.ToString())
                        Return 0
                    Catch clfe As CommandLineFailureException
                        Return Err(clfe.Message)

                    Catch ex As Exception
                        Return Err(ex.ToString())

                    End Try

                Case "getdbscript"
                    Try
                        Dim shouldMinify = Not debug_opt
                        Console.Write(CreateInstaller().GenerateInstallerScript(fromRev, toRev, shouldMinify))
                        Return 0
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToGenerateScript0, ex.Message)
                        Return 1
                    End Try

                Case "getdbversion"
                    Console.WriteLine(CreateInstaller().GetRequiredDBVersion().ToString())
                    Return 0

                Case "report"

                    'check username/password correctly supplied:
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToGenerateAReport)
                        Return 1
                    End If

                    Console.WriteLine(My.Resources.GeneratingReport)

                    Try
                        Dim rr As New clsSystemReporter()
                        Using sw As New StreamWriter(sFilespec)
                            rr.Generate(New List(Of Object), sw)
                        End Using
                        Console.WriteLine(My.Resources.ReportGenerated)
                        Return 0

                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToGenerateReport0, ex.Message)
                        Return 1
                    End Try

                Case "listprocesses"
                    'Check permissions...
                    Dim perms = Permission.ObjectStudio.ImpliedViewBusinessObject.Union(
                        Permission.ProcessStudio.ImpliedViewProcess)
                    CheckLoggedInAndUserRole(perms.ToArray())

                    'Output the list (we have to do it twice, because the GetProcesses function won't give
                    'us both kinds of processes at once.
                    Dim dt As DataTable
                    If User.Current.HasPermission(Permission.ProcessStudio.ImpliedViewProcess) Then
                        dt = gSv.GetProcesses(ProcessAttributes.None, ProcessAttributes.Retired, False)
                        For Each dr As DataRow In dt.Rows
                            Console.WriteLine(dr("name"))
                        Next
                    End If

                    If User.Current.HasPermission(Permission.ObjectStudio.ImpliedViewBusinessObject) Then
                        dt = gSv.GetProcesses(ProcessAttributes.None, ProcessAttributes.Retired, True)
                        For Each dr As DataRow In dt.Rows
                            Console.WriteLine(dr("name"))
                        Next
                    End If
                    Return 0

                Case "refreshdependencies"
                    'check username/password correctly supplied:
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToRefreshDependencies)
                        Return 1
                    End If

                    'Force refresh of all dependency data
                    gSv.RebuildDependencies(forceRefresh)
                    Return 0

                Case "configencrypt"
                    CheckLoggedInAndUserRole(Auth.Permission.SystemManager.Security.ManageEncryptionSchemes)

                    Const MethodBuiltIn = "default"
                    Dim thumbprint = configEncryptParam

                    Try
                        If String.IsNullOrWhiteSpace(thumbprint) Then
                            Dim getResult As String = Nothing
                            Try
                                getResult = gSv.GetConfigEncryptMethod()
                            Catch ex As Exception
                                Console.Write(ex.Message)
                            End Try

                            If String.IsNullOrWhiteSpace(getResult) Then
                                Console.WriteLine(My.Resources.ConfigEncryptEncryptionMethodBuiltIn)
                            Else
                                Console.WriteLine(My.Resources.ConfigEncryptEncryptionMethodOwnCertificate & getResult)
                            End If
                            Return 0
                        End If


                        If thumbprint.ToLower(CultureInfo.InvariantCulture) = MethodBuiltIn Then
                            gSv.SetConfigEncryptMethod(String.Empty, False)
                        Else
                            gSv.SetConfigEncryptMethod(thumbprint, forceConfigEncryptParam)
                        End If
                    Catch ex As Exception
                        Console.WriteLine(ex.Message)
                    End Try
                    Return 0

                Case "loginsession"
                    Return LoginSession(listloginsParam)

                Case "elementusage"

                    'check username/password correctly supplied:
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToGenerateElementUsageData)
                        Return 1
                    End If

                    Console.WriteLine(My.Resources.GeneratingUsageData)

                    Try
                        If sRunProcess.Length = 0 Then
                            Console.WriteLine(My.Resources.YouMustSpecifyTheProcessToAnalyse)
                            Return 1
                        End If
                        Dim rr As New clsElementUsageReporter()
                        Dim xargs As New List(Of Object)()
                        xargs.Add(sRunProcess)
                        Using sw As New StreamWriter(sFilespec)
                            rr.Generate(xargs, sw)
                        End Using

                        Console.WriteLine(My.Resources.ReportGenerated)
                        Return 0

                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToGenerateUsageData0, ex.Message)
                        Return 1
                    End Try


                Case "poolcreate"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.Resources.Pools)

                    'Validate input
                    If sPool Is Nothing Then
                        Return Err(My.Resources.ErrorNoPoolNameSpecified)
                    End If

                    Try
                        gSv.CreateResourcePool(sPool)
                    Catch ex As Exception
                        Return Err(My.Resources.FailedToCreatePool0, ex.Message)
                    End Try

                    Console.WriteLine(My.Resources.PoolCreated)
                    Return 0

                Case "pooldelete"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.Resources.Pools)

                    'Validate input
                    If sPool Is Nothing Then
                        Return Err(My.Resources.ErrorNoPoolNameSpecified)
                    End If

                    Try
                        gSv.DeleteResourcePool(sPool)
                    Catch ex As Exception
                        Return Err(My.Resources.FailedToDeletePool0, ex.Message)
                    End Try

                    Console.WriteLine(My.Resources.PoolDeleted)
                    Return 0

                Case "pooladd"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.Resources.Pools)

                    'Validate input
                    If sPool Is Nothing Then
                        Return Err(My.Resources.ErrorNoPoolNameSpecified)
                    End If

                    If sTargetResource Is Nothing Then
                        sTargetResource = ResourceMachine.GetName(iPort)
                    End If
                    Try
                        gSv.AddResourceToPool(sPool, sTargetResource)
                    Catch ex As Exception
                        Return Err(My.Resources.FailedToAddToPool0, ex.Message)
                    End Try

                    Console.WriteLine(My.Resources.ResourceAddedToPool)
                    Return 0

                Case "poolremove"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.Resources.Pools)

                    If sTargetResource Is Nothing Then
                        sTargetResource = ResourceMachine.GetName(iPort)
                    End If
                    Try
                        gSv.RemoveResourceFromPool(sTargetResource)
                    Catch ex As Exception
                        Return Err(My.Resources.FailedToRemoveFromPool0, ex.Message)
                    End Try

                    Console.WriteLine(My.Resources.ResourceRemovedFromPool)
                    Return 0

                Case "setev"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(
                        Permission.SystemManager.Processes.ConfigureEnvironmentVariables,
                        Permission.SystemManager.BusinessObjects.ConfigureEnvironmentVariables
                    )

                    Try
                        gSv.UpdateEnvironmentVariable(evname, clsProcessDataTypes.DataTypeId(evdatatype), evvalue, evdesc)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToSetEnvironmentVariable0, ex.Message)
                        Return 1
                    End Try

                    Console.WriteLine(My.Resources.EnvironmentVariableSet)
                    Return 0

                Case "deleteev"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(
                        Permission.SystemManager.Processes.ConfigureEnvironmentVariables,
                        Permission.SystemManager.BusinessObjects.ConfigureEnvironmentVariables
                    )

                    Try
                        If Not gSv.DeleteEnvironmentVariable(evname) Then
                            Console.WriteLine(My.Resources.FailedToDeleteEnvironmentVariable0, String.Format(My.Resources.NoEnvironmentVariablesExistWithName0, evname))
                            Return 1
                        End If
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToDeleteEnvironmentVariable0, ex.Message)
                    End Try

                    Console.WriteLine(My.Resources.EnvironmentVariableDeleted)
                    Return 0

                Case "startschedule"

                    Try
                        CheckLoggedInAndUserRole(Permission.ControlRoom.GroupName)

                        For Each sched As ISchedule In GetSchedules(scheduleNames, store)
                            Console.WriteLine(My.Resources.Schedule0SetToRunAt1,
                             sched, store.TriggerSchedule(sched))
                        Next

                        Return 0

                    Catch clfe As CommandLineFailureException
                        Return Err(clfe.Message)

                    Catch ex As Exception
                        Return Err(My.Resources.ErrorInAction01, sAction, ex)

                    End Try

                Case "deleteschedule"

                    Try
                        CheckLoggedInAndUserRole(Permission.ControlRoom.GroupName)

                        For Each sched As ISchedule In GetSchedules(scheduleNames, store)
                            store.DeleteSchedule(sched)
                            Console.WriteLine(My.Resources.Schedule0HasBeenDeleted, sched)
                        Next

                        Return 0

                    Catch clfe As CommandLineFailureException
                        Return Err(clfe.Message)

                    Catch ex As Exception
                        Return Err(My.Resources.ErrorInAction01, sAction, ex)

                    End Try

                Case "viewschedlist"

                    Try
                        CheckLoggedInAndUserRole(Permission.ControlRoom.GroupName)
                        Dim outputType As ScheduleLogOutputType
                        Select Case exportFormat
                            Case "txt" : outputType = ScheduleLogOutputType.Readable
                            Case "csv" : outputType = ScheduleLogOutputType.CSV
                            Case Else
                                Return Err(My.Resources.InvalidExportFormat0, exportFormat)
                        End Select

                        Using scheduleWriter As New ScheduleLogWriter(Console.Out, outputType)
                            scheduleWriter.OutputScheduleList(
                            scheduleListName, scheduleListDays, scheduleListDate,
                            store, scheduleListType)
                        End Using
                        Return 0

                    Catch cle As CommandLineFailureException
                        Return Err(cle.Message)

                    Catch swe As ScheduleWriterException
                        Return Err(swe.Message)

                    End Try

                Case "createqueue", "deletequeue"

                    Dim userAction As String = sAction.Split("q"c)(0) ' ie. "create" or "delete"

                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.GroupName)

                    'Validate input
                    If queueName = "" Then Return Err(
                     My.Resources.ErrorNoQueueNameSpecified0MustBeAccompaniedByQueuename,
                     sAction)

                    Try
                        If sAction = "deletequeue" Then
                            gSv.DeleteWorkQueue(queueName, True)
                        Else
                            Dim wq As clsWorkQueue = gSv.CreateWorkQueue(New clsWorkQueue(Nothing, queueName, queueKey, queueMaxAttempts, queueRunning, Nothing), True)
                            If wq.Ident <> 0 Then
                                Console.WriteLine(My.Resources.CreatedQueue0, wq.Id)
                            Else
                                Return Err(String.Format(My.Resources.ErrorTheQueueName0AlreadyExists, queueName), queueName)
                            End If

                        End If
                        Return 0

                    Catch ex As Exception
                        Return Err(My.Resources.Error0, ex)

                    End Try

                Case "wslog"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.Resources.ConfigureResource)

                    If sWSLog <> "on" And sWSLog <> "off" Then 'I18N: "on" and "off" are parameters not intended to be translated?!
                        Console.WriteLine(My.Resources.ErrorSpecifyOnOrOff)
                        Return 1
                    End If

                    If sTargetResource Is Nothing Then
                        sTargetResource = ResourceMachine.GetName(iPort)
                    End If
                    Dim resid As Guid = gSv.GetResourceId(sTargetResource)
                    If resid = Guid.Empty Then
                        Console.WriteLine(My.Resources.ErrorResourceDoesNotExist)
                        Return 1
                    End If

                    Dim diags As Integer = gSv.GetResourceDiagnostics(resid)
                    If sWSLog = "on" Then 'I18N: "on" and "off" are parameters not intended to be translated?!
                        diags = diags Or clsAPC.Diags.LogWebServices
                    Else
                        diags = diags And (Not clsAPC.Diags.LogWebServices)
                    End If

                    Try
                        gSv.SetResourceDiagnostics(resid, diags)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ErrorFailedToSetDiagnostics0, ex.Message)
                        Return 1
                    End Try

                    Console.WriteLine(My.Resources.WebServicesLoggingModeSet)
                    Return 0

                Case "requeststop"
                    CheckLoggedInAndUserRole(Permission.Resources.ControlResource)

                    Dim sessId As Guid, sessNo As Integer
                    If Not Guid.TryParse(sSessionID, sessId) AndAlso
                     Not Integer.TryParse(sSessionID, sessNo) Then Return Err(
                        My.Resources.RequeststopMustBeFollowedByAValidSessionIDNumber)

                    If sessNo = 0 Then sessNo = gSv.GetSessionNumber(sessId)

                    If sessNo <= 0 Then Return Err(
                     My.Resources.CouldNotFindTheSessionWithTheIDNumber0, sSessionID)

                    If gSv.RequestStopSession(sessNo) Then
                        Console.WriteLine(My.Resources.StopRequestedForSession0, sSessionID)
                        Return 0

                    Else
                        ' Find out why it didn't work... get the session ID (if we
                        ' don't already have it)
                        If sessId = Guid.Empty Then
                            sessId = gSv.GetSessionID(sessNo)
                            ' If it's not there, that's why it didn't work
                            If sessId = Guid.Empty Then Return Err(
                             My.Resources.NoSessionWithTheNumber0WasFound, sessNo)
                        End If

                        ' If the session is not running, that's why it didn't work
                        Dim status As BluePrism.Server.Domain.Models.SessionStatus = gSv.GetSessionStatus(sessId)
                        If status <> BluePrism.Server.Domain.Models.SessionStatus.Running Then Return Err(
                         My.Resources.TheSession0IsNotRunningStatus1,
                         sSessionID, status)

                        ' Neither of those? Don't know why it failed...
                        Return Err(
                            My.Resources.TheStopRequestForSession0HadNoEffect, sessNo)

                    End If

                Case "run"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.Resources.ControlResource)

                    'Check licensing restrictions
                    clsLicenseQueries.RefreshLicense()
                    If Not gSv.CanCreateSessions(1) Then Return Err(
                     My.Resources.ErrorCanNotCreateSessionToRunProcess0,
                     Licensing.SessionLimitReachedMessage)

                    'Check Process Exists
                    Dim ProcessID As Guid = gSv.GetProcessIDByName(sRunProcess)
                    If ProcessID = Guid.Empty Then
                        Console.WriteLine(My.Resources.ErrorProcess0DoesNotExist, sRunProcess
                                          )
                        Return 1
                    End If

                    'Check whether process is published
                    Dim attributes As ProcessAttributes
                    Try
                        attributes = gSv.GetProcessAttributes(ProcessID)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ErrorFailedToRetrieveProcessAttributesFromDatabase0, ex.Message)
                        Return 1
                    End Try

                    If (attributes And ProcessAttributes.Published) = 0 Then
                        Console.WriteLine(My.Resources.ErrorCanNotRunProcess0BecauseItHasNotBeenPublished, sRunProcess
                                          )
                        Return 1
                    End If

                    Dim gSessionID As Guid
                    Dim actualResource As String = ""
                    Try

                        'Create a talker object to communicate with our
                        'resource PC.
                        Dim objTalker As New clsTalker(gSv.GetPref(PreferenceNames.ResourceConnection.PingTimeOutSeconds, 30))
                        Try

                            If sTargetResource Is Nothing Then
                                sTargetResource = ResourceMachine.GetName(iPort)
                            ElseIf iPort <> ResourceMachine.DefaultPort Then
                                sTargetResource &= ":" & iPort
                            End If

                            'Get the attributes of the requested target resource...
                            Dim attr As ResourceAttribute
                            Try
                                attr = gSv.GetResourceAttributes(sTargetResource)
                            Catch ex As Exception
                                Console.WriteLine(My.Resources.FailedToGetAttributesOfResource01, sTargetResource, ex.Message)
                                Return 1
                            End Try

                            'Decide what machine name to connect to. Normally it's tbe Resource
                            'name, but if the Resource is a pool then we need to connect to the
                            'controller.
                            Dim name As String = Nothing
                            If (attr And ResourceAttribute.Pool) = 0 Then
                                name = sTargetResource
                            Else
                                Try
                                    gSv.GetPoolControllerName(gSv.GetResourceId(sTargetResource), name)
                                Catch ex As Exception
                                    Console.WriteLine(My.Resources.ThePool0IsOffline, sTargetResource)
                                    Return 1
                                End Try
                            End If

                            If Not objTalker.Connect(name, sErr) Then
                                Console.WriteLine(My.Resources.ErrorCouldNotConnectToResource01, sTargetResource, sErr)
                                Return 1
                            End If
                            If Not objTalker.Authenticate() Then
                                Console.WriteLine(My.Resources.AuthenticationError0, objTalker.GetReply())
                                Return 1
                            End If

                            Dim controllingUser = gSv.GetControllingUserPermissionSetting
                            Dim createCommand As String
                            Dim sessionId = Guid.NewGuid
                            Dim queueId = 0
                            If controllingUser Then
                                Dim token = gSv.RegisterAuthorisationToken(ProcessID)
                                createCommand = $"createas {token} {ProcessID} {queueId} {sessionId}"
                            Else
                                createCommand = $"create {ProcessID} {queueId} {sessionId}"
                            End If

                            'Create session...
                            If Not objTalker.Say(createCommand, "SESSION CREATED") Then
                                Console.WriteLine(ErrorFailedToCreateSession0, objTalker.GetReply())
                                Return 1
                            End If
                            'Get ID of session just created...
                            Dim sReply As String
                            Dim ind As Integer
                            sReply = objTalker.GetReply
                            ind = sReply.IndexOf("SESSION CREATED")
                            sReply = Mid(sReply, ind + 19, 36)
                            sReply = "{" & sReply & "}"
                            gSessionID = New Guid(sReply)
                            'Set startup parameters if given...
                            If sStartParms <> "" Then
                                If Not objTalker.Say("startp " & sStartParms, "PARAMETERS SET") Then
                                    Console.WriteLine(My.Resources.ErrorFailedToSetParameters0, objTalker.GetReply())
                                    Return 1
                                End If
                            End If

                            Dim startCommand As String
                            If controllingUser Then
                                Dim tok = gSv.RegisterAuthorisationToken(ProcessID)
                                startCommand = String.Format("startas {0} {1}", tok, gSessionID)
                            Else
                                startCommand = String.Format("start {0}", gSessionID)
                            End If

                            'Start process...
                            If Not objTalker.Say(startCommand, "STARTED") Then
                                Console.WriteLine(My.Resources.ErrorFailedToStartSession0, objTalker.GetReply())

                                Dim deleteCommand As String
                                If controllingUser Then
                                    Dim tok = gSv.RegisterAuthorisationToken(ProcessID)
                                    deleteCommand = String.Format("deleteas {0} {1}", tok, gSessionID)
                                Else
                                    deleteCommand = String.Format("delete {0}", gSessionID)
                                End If

                                If Not objTalker.Say(deleteCommand, "SESSION DELETED") Then
                                    Console.WriteLine(My.Resources.ErrorFailedToDeleteSession0, objTalker.GetReply())
                                End If
                                Return 1
                            End If
                        Finally
                            objTalker.Close()
                        End Try

                        'Get the actual running resource, since it may be different if
                        'we're running on a pool.
                        Dim aresID As Guid = gSv.GetSessionResourceID(gSessionID)
                        Try
                            actualResource = gSv.GetResourceName(aresID)
                        Catch ex As Exception
                            Throw New InvalidOperationException(String.Format(My.Resources.FailedToGetActualResource0, ex.Message))
                        End Try

                    Catch e As Exception
                        Console.WriteLine(My.Resources.Error0, sErr)
                        Return 1
                    End Try

                    Console.WriteLine(My.Resources.StartedProcess0, sRunProcess)
                    Console.WriteLine(My.Resources.Resource0, sTargetResource)
                    Console.WriteLine(My.Resources.ActualResource0, actualResource)
                    Console.WriteLine(My.Resources.Session0, gSessionID.ToString())
                    Return 0

                Case "publishws", "unpublishws"
                    ' Check logged in
                    CheckLoggedInAndUserRole()

                    'Get Process Id
                    Dim isObject As Boolean
                    Dim processId = GetProcessOrObjectID(sRunProcess, isObject)
                    If processId = Guid.Empty Then Return Err(My.Resources.ProcessObject0DoesNotExist, sRunProcess)

                    ' Check permissions (now we know what type it is)
                    CheckLoggedInAndUserRole(If(isObject,
                                             Permission.SystemManager.BusinessObjects.Exposure,
                                             Permission.SystemManager.Processes.Exposure))

                    Dim newWSDetails As WebServiceDetails = Nothing
                    If sAction = "publishws" Then
                        Dim currentWSDetails As WebServiceDetails = gSv.GetProcessWSDetails(processId)

                        'Determine the published name
                        Dim wsName As String
                        If Not String.IsNullOrEmpty(sWebServiceName) Then
                            'Use the specified name
                            wsName = sWebServiceName
                        ElseIf Not String.IsNullOrEmpty(currentWSDetails.WebServiceName) Then
                            'Use the existing name in database
                            wsName = currentWSDetails.WebServiceName
                        Else
                            'Otherwise use object/process name
                            wsName = sRunProcess
                        End If
                        newWSDetails = New WebServiceDetails(clsProcess.GetSafeName(wsName), bDocLiteral, useLegacyNamespace)
                    End If

                    Try
                        If sAction = "publishws" Then
                            If isObject Then
                                gSv.ExposeObjectAsWebService(processId, newWSDetails)
                            Else
                                gSv.ExposeProcessAsWebService(processId, newWSDetails)
                            End If
                        Else
                            If isObject Then
                                gSv.ConcealObjectWebService(processId)
                            Else
                                gSv.ConcealProcessWebService(processId)
                            End If
                        End If
                    Catch ex As Exception
                        Return Err(My.Resources.Error0, ex.Message)
                    End Try

                    Return 0

                Case "publish", "unpublish"

                    'Check logged in and user has permission
                    CheckLoggedInAndUserRole(Permission.ProcessStudio.ImpliedEditProcess)

                    'Get Process Id
                    Dim isObject As Boolean
                    Dim processId = GetProcessOrObjectID(sRunProcess, isObject)
                    If processId = Guid.Empty Then Return Err(My.Resources.ProcessObject0DoesNotExist, sRunProcess)

                    Try
                        If sAction = "publish" Then
                            gSv.PublishProcess(processId)
                            Console.WriteLine(My.Resources.AddedPublishedAttributeToProcess0, sRunProcess)
                        Else
                            gSv.UnpublishProcess(processId)
                            Console.WriteLine(My.Resources.RemovedPublishedAttributeFromProcess0, sRunProcess)
                        End If
                    Catch ex As Exception
                        Return Err(My.Resources.Error0, ex.Message)
                    End Try

                    Return 0

                Case "export"
                    Try
                        'Check logged in
                        CheckLoggedInAndUserRole()
                        Dim xml As String, id As Guid, processAttributes As ProcessAttributes
                        Dim isObject As Boolean
                        id = GetProcessOrObjectID(sExportProcess, isObject)
                        If id = Guid.Empty Then Err(My.Resources.ProcessObject0DoesNotExist, sExportProcess)

                        'Check user roles
                        CheckLoggedInAndUserRole(If(isObject,
                                                 Permission.ObjectStudio.ExportBusinessObject,
                                                 Permission.ProcessStudio.ExportProcess))

                        CheckUserHasPermissionForProcess(
                            id,
                            If(isObject,
                               Permission.ObjectStudio.ExportBusinessObject,
                               Permission.ProcessStudio.ExportProcess))

                        Try
                            Dim processDetails = gSv.GetProcessXMLAndAssociatedData(id)
                            xml = CStr(IIf(processDetails.Zipped, GZipCompression.Decompress(processDetails.Xml), processDetails.Xml))
                            processAttributes = processDetails.Attributes
                        Catch ex As Exception
                            Console.WriteLine(My.Resources.CouldNotGetProcessDefinition0, ex.Message)
                            Return 1
                        End Try

                        Dim filename As String = String.Format(My.Resources.BPA01Xml,
                         IIf(isObject, My.Resources.xObject, My.Resources.Process), sExportProcess)

                        For Each invalid As Char In Path.GetInvalidFileNameChars()
                            filename = filename.Replace(invalid, "_")
                        Next

                        If Not clsProcess.ExportXMLToFile(xml, id, processAttributes, filename, sErr) Then
                            Console.WriteLine(sErr)
                            Return 1
                        End If
                        If isObject Then
                            gSv.AuditRecordBusinessObjectEvent(BusinessObjectEventCode.ExportBusinessObject, id, My.Resources.ExportedViaCommandLine, Nothing, Nothing)
                        Else
                            gSv.AuditRecordProcessEvent(ProcessEventCode.ExportProcess, id, My.Resources.ExportedViaCommandLine, Nothing, Nothing)
                        End If
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToExportProcess0, ex.Message)
                        Return 1
                    End Try
                    Console.WriteLine(My.Resources.ExportedProcess)
                    Return 0

                Case "import"
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToImport)
                        Return 1
                    End If

                    Try

                        Dim dir As String = Path.GetDirectoryName(sImportFile)
                        If String.IsNullOrEmpty(dir) Then dir = Directory.GetCurrentDirectory()
                        Dim files() As String = Directory.GetFiles(dir, Path.GetFileName(sImportFile))
                        If files.Length = 0 Then
                            Return Err(My.Resources.ErrorNoMatchingFilesFound)
                        End If

                        For Each fileName As String In files

                            Console.WriteLine(My.Resources.Importing0, fileName)

                            ' Load the file into an XML Document.
                            Dim x As New ReadableXmlDocument()
                            x.Load(fileName)

                            Dim id As Guid ' The ID for the imported process.
                            ' If we're forcing the ID, get the requested ID.
                            If forcingId Then
                                If "new".Equals(sForceID, StringComparison.OrdinalIgnoreCase) Then
                                    id = Guid.NewGuid()
                                Else
                                    Try
                                        id = New Guid(sForceID)

                                    Catch ex As Exception
                                        Return Err(My.Resources.ErrorSpecifiedIDIsNotValid0, ex.Message)

                                    End Try
                                End If

                            Else ' Not forcing it - use the 'preferredid' from the document.
                                Try
                                    id = New Guid(x.DocumentElement.GetAttribute("preferredid"))

                                Catch ' No preferredid element, just generate a new one.
                                    id = Guid.NewGuid()

                                End Try
                            End If

                            Dim sXML As String = x.OuterXml
                            Dim p As clsProcess =
                             clsProcess.FromXML(clsGroupObjectDetails.Empty, sXML, False, sErr)
                            If p Is Nothing Then
                                Return Err(My.Resources.ErrorFailedToCreateProcess0, sErr)
                            End If

                            Dim permissionRequired As String
                            Select Case p.ProcessType
                                Case DiagramType.Object
                                    permissionRequired = Permission.ObjectStudio.ImportBusinessObject
                                Case DiagramType.Process
                                    permissionRequired = Permission.ProcessStudio.ImportProcess
                                Case Else
                                    Return Err(My.Resources.ErrorUnableToImportThatTypeOfProcessPermissionRequirementsUnknown)
                            End Select

                            'Check user roles
                            CheckLoggedInAndUserRole(permissionRequired)

                            Try
                                Dim isObject As Boolean = (p.ProcessType = DiagramType.Object)
                                gSv.ImportProcess(id, p.Name, "1.0", p.Description, sXML,
                                 overwrite, isObject, p.GetDependencies(False), fileName)

                            Catch ex As Exception
                                Return Err(My.Resources.ErrorFailedToCreateProcess0, ex.Message)

                            End Try
                        Next

                    Catch ex As Exception
                        Return Err(My.Resources.FailedToImportProcess0, ex.Message)
                    End Try

                    Console.WriteLine(My.Resources.ImportedProcess)
                    Return 0

                Case "importrelease"
                    CheckLoggedInAndUserRole(Permission.ReleaseManager.ImportRelease)

                    If Not Licensing.License.CanUse(LicenseUse.ReleaseManager) Then
                        Return Err(My.Resources.Command0CannotBeUsedWithNhsEdition, sAction)
                    End If

                    Dim dir As String = Path.GetDirectoryName(sImportFile)
                    If String.IsNullOrEmpty(dir) Then dir = Directory.GetCurrentDirectory()
                    Dim files() As String = Directory.GetFiles(dir, Path.GetFileName(sImportFile))
                    If Not files?.Any() Then
                        Return Err(My.Resources.ErrorNoMatchingFilesFound)
                    End If

                    ' Load release objects from file
                    Dim file = files.First()
                    Dim release = clsRelease.Import(New FileInfo(file), Nothing, True)

                    Try
                        Console.WriteLine(My.Resources.ImportingFile0, file)

                        ' Check user has the relevant import permissions
                        Dim missingPerms = release.CheckImportPermissions()
                        If missingPerms.Count > 0 Then
                            Console.WriteLine(
                              My.Resources.YouDoNotHavePermissionToImportThisReleaseRequiredPermissionsAre0,
                              missingPerms)
                            Return 1
                        End If

                        ' Apply the default (non-interactive) resolutions for any conflicts
                        Dim resolutions = New List(Of ConflictResolution)
                        For Each c In release.Conflicts.AllConflicts
                            Dim res = c.Definition.DefaultNonInteractiveResolution
                            If res = ConflictOption.UserChoice.Fail Then Continue For
                            Dim opt = c.Definition.Options.First(Function(o) o.Choice = res)
                            c.Resolution = New ConflictResolution(c, opt)
                            resolutions.Add(c.Resolution)
                        Next

                        ' Ensure that all conflicts can be resolved
                        release.Conflicts.Resolve(resolutions)
                        If Not release.Conflicts.IsResolved Then
                            Dim list = (From c In release.Conflicts.AllConflicts
                                        Where Not c.IsResolved
                                        Select String.Format("{0} '{1}': {2}",
                                                PackageComponentType.GetLocalizedFriendlyName(c.Component.Type),
                                                c.Component.Name,
                                                c.Definition.Text))
                            Console.WriteLine(My.Resources.UnableToResolveTheFollowingConflicts01,
                                    vbCrLf, CollectionUtil.Join(list, vbCrLf))
                            Return 1
                        End If

                        ' Attempt to import the release (as a background server job)
                        Dim notifier As New BackgroundJobNotifier()
                        Dim job = gSv.ImportRelease(release, True, notifier)
                        Dim jobResult = job.Wait(notifier, AddressOf HandleBackgroundJobupdate).Result
                        Select Case jobResult.Status
                            Case JobMonitoringStatus.Success
                                Console.WriteLine(My.Resources.Release0ImportedFromFile1, release.Name, file)
                            Case JobMonitoringStatus.Failure
                                Throw New BackgroundJobException(If(jobResult.Data.Error IsNot Nothing,
                                                                    jobResult.Data.Error.Message,
                                                                    My.Resources.NoErrorInformationAvailable))
                            Case JobMonitoringStatus.Missing
                                Throw New BackgroundJobException(
                                    My.Resources.UnableToGetStatusOfReleaseServerMayHaveStoppedRunning)
                            Case JobMonitoringStatus.Timeout
                                Throw New BackgroundJobException(
                                    My.Resources.NoUpdateReceivedWithinTheExpectedTimeTheImportMayHaveStalled)
                            Case JobMonitoringStatus.MonitoringError
                                Throw New BackgroundJobException(
                                    My.Resources.AnUnexpectedErrorOccuredWhileWaitingForTheImportToComplete0,
                                    If(jobResult.Exception IsNot Nothing,
                                        jobResult.Exception.Message,
                                        My.Resources.NoErrorInformationAvailable))

                        End Select
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToImportRelease0, ex.Message)
                    End Try
                    Return 0

                Case "exportpackage"
                    CheckLoggedInAndUserRole(Permission.ReleaseManager.CreateRelease)

                    If packageToExport = "" Then Throw New CommandLineFailureException(
                     My.Resources.YouMustProvideAPackageNameToExport)

                    Dim pkg As clsPackage = gSv.GetPackage(packageToExport)
                    If pkg Is Nothing Then Throw New CommandLineFailureException(
                     My.Resources.NoPackageWithTheName0WasFound, packageToExport)

                    Dim rel As clsRelease
                    If releaseName Is Nothing Then
                        rel = pkg.CreateRelease()
                    Else
                        rel = pkg.CreateRelease(releaseName)
                    End If

                    Dim filename As String =
                     String.Format("{0}.{1}", rel.Name, clsRelease.FileExtension)

                    If filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
                        For Each c As Char In Path.GetInvalidFileNameChars()
                            filename = filename.Replace(c, "_"c)
                        Next
                    End If

                    gSv.CreateRelease(rel)
                    Dim monitor As New clsProgressMonitor()
                    AddHandler monitor.ProgressChanged, AddressOf ReportProgress

                    rel.Export(New FileInfo(filename), monitor)

                    Console.WriteLine()
                    Console.WriteLine(My.Resources.Release0ExportedToFile1, rel.Name, filename)
                    Return 0

                Case "fontimport"
                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToFontimport)
                        Return 1
                    End If

                    Try
                        Dim dir As String = Path.GetDirectoryName(sPath)
                        If String.IsNullOrEmpty(dir) Then dir = Directory.GetCurrentDirectory()
                        Dim files() As String = Directory.GetFiles(dir, Path.GetFileName(sPath))
                        If files.Length = 0 Then
                            Return Err(My.Resources.ErrorNoMatchingFilesFound)
                        End If

                        For Each f As String In files
                            Dim xml As String = File.ReadAllText(f)
                            Dim name As String = Path.GetFileNameWithoutExtension(f)
                            If name.StartsWith("font_") Then
                                name = name.Substring(5)
                            End If
                            Console.WriteLine(My.Resources.ImportingFont0, name)
                            Dim font As New FontData(xml)
                            gSv.SaveFont(name, font.Version, xml)
                        Next

                    Catch ex As Exception
                        Return Err(My.Resources.FailedToImportFont0, ex.Message)
                    End Try
                    Return 0

                Case "archive"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.GroupName)

                    Try
                        Dim arch As New clsArchiver(configOptions.ArchivePath)
                        Dim proc As String = sRunProcess
                        If proc.Length = 0 Then proc = Nothing
                        If Not arch.CreateArchive(fromDate, toDate, delete, proc, True, sErr) Then
                            Console.WriteLine(My.Resources.ArchivingFailed0, sErr)
                            Return 1
                        End If
                        Console.WriteLine(My.Resources.ArchiveComplete)
                        Return 0
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ArchivingFailed0, ex.Message)
                        Return 1
                    End Try

                Case "restorearchive"

                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.SystemManager.GroupName)

                    Try
                        Dim arch As New clsArchiver(configOptions.ArchivePath)
                        If Not arch.RestoreArchive(fromDate, toDate, sErr) Then
                            Console.WriteLine(My.Resources.RestoreFailed0, sErr)
                            Return 1
                        End If
                        Return 0
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.RestoreFailed0, ex.Message)
                        Return 1
                    End Try
                    Console.WriteLine(My.Resources.RestoreComplete)
                    Return 0


                Case "status"
                    'Check user roles
                    CheckLoggedInAndUserRole(Permission.Resources.ImpliedViewResource)

                    'Check the user's supplied session ID
                    Dim gSessionID As Guid
                    Try
                        If String.Compare(sSessionID, "all", StringComparison.InvariantCultureIgnoreCase) <> 0 Then
                            gSessionID = New Guid(sSessionID)
                        End If
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.SuppliedValue0IsNotAValidSessionIdentifier, sSessionID)
                        Console.WriteLine(My.Resources.TheCorrectFormatIs0, Guid.Empty.ToString.Replace("0", "x"))
                        Return 1
                    End Try

                    If String.Compare(sSessionID, "all", StringComparison.InvariantCultureIgnoreCase) <> 0 Then
                        Dim st As SessionStatus = gSv.GetSessionStatus(gSessionID)
                        If st = SessionStatus.All Then
                            Console.WriteLine(My.Resources.ERRORNoInformationFoundForThatSession)
                            Return 1
                        End If

                        Console.WriteLine(My.Resources.Session0, sSessionID)
                        Console.WriteLine(My.Resources.Status0,
                         IIf(st = SessionStatus.Terminated, My.Resources.Failed, st.ToString()))
                    Else
                        Dim sessions = gSv.GetActualSessions().ToList()
                        sessions.Sort(Function(x, y) x.SessionStart.CompareTo(y.SessionStart))

                        Console.WriteLine(String.Format("Total Sessions: {0}", sessions.Count))
                        Console.WriteLine(String.Format("Resource ID                           Session ID                            Status{0}Start Time           End Time", vbTab))
                        For Each session In sessions
                            Console.WriteLine(String.Format("{0}  {1}  {2}{3}{4}   {5}", session.ResourceName, session.SessionID.ToString, session.StatusText, vbTab, session.SessionStartText, session.SessionEndText))
                        Next
                    End If

                    Return 0

                Case "resourcestatus"

                    'Check user roles
                    CheckLoggedInAndUserRole(Permission.Resources.ImpliedViewResource)

                    Dim sessions As List(Of clsProcessSession)

                    If String.Compare(resourceName, "all", StringComparison.InvariantCultureIgnoreCase) = 0 Then
                        sessions = gSv.GetSessionsForAllResources().ToList()
                    Else
                        sessions = gSv.GetSessionsForResource(resourceName).ToList()
                    End If

                    sessions.Sort(Function(x, y) x.SessionStart.CompareTo(y.SessionStart))

                    Dim gLimitNumber As Integer = -1
                    Dim earliestDate As DateTime
                    'If there is a limit apply it
                    Dim limitTypeName As LimitType = Nothing

                    If Not String.IsNullOrWhiteSpace(limitType) AndAlso Not String.IsNullOrWhiteSpace(limitNumber) AndAlso Integer.TryParse(limitNumber, gLimitNumber) _
                       AndAlso IsValidLimitType(limitType, gLimitNumber, limitTypeName, earliestDate) Then
                        sessions = sessions.Where(Function(x) x.SessionStart > earliestDate).ToList()
                        Console.WriteLine(My.Resources.ResourceStatusShowingSessions, resourceName, limitNumber, limitTypeName.ToString(), earliestDate.ToShortDateString(), earliestDate.ToShortTimeString)
                    Else
                        Console.WriteLine(My.Resources.ShowingAllSessionsFor0, resourceName)
                    End If
                    If sessions.Any Then
                        Console.WriteLine($"Total Sessions: {sessions.Count}")
                        If String.Compare(resourceName, "all", StringComparison.InvariantCultureIgnoreCase) = 0 Then
                            Dim longestNameLength = sessions.OrderByDescending(Function(x) x.ResourceName.Length).First().ResourceName.Length
                            longestNameLength = If(longestNameLength < 9, 9, longestNameLength)
                            Console.WriteLine($"Resource Name{New String(" "c, longestNameLength - 7)}Session ID                            Status         {vbTab}Start Time            End Time")
                            For Each session In sessions
                                If Not session.ProcessPermissions.IsRestricted OrElse session.ProcessPermissions.HasPermission(User.Current, Permission.Resources.ImpliedViewResource) Then
                                    Console.WriteLine($"{session.ResourceName}{New String(" "c, (longestNameLength - session.ResourceName.Length) + 6)}{session.SessionID.ToString}  {session.StatusText.PadRight(15)}  {vbTab}{session.SessionStartText}   {session.SessionEndText}")
                                End If
                            Next
                        Else
                            Console.WriteLine(
                                $"Session ID                            Status         {vbTab}Start Time            End Time")
                            For Each session In sessions
                                If Not session.ProcessPermissions.IsRestricted OrElse session.ProcessPermissions.HasPermission(User.Current, Permission.Resources.ImpliedViewResource) Then
                                    Console.WriteLine($"{session.SessionID.ToString}  {session.StatusText.PadRight(15)}  {vbTab}{session.SessionStartText}   {session.SessionEndText}")
                                End If
                            Next

                        End If
                    Else
                        Console.WriteLine("No sessions found for selected options")
                    End If


                    Return 0
                Case "getauditlog"
                    CheckLoggedInAndUserRole(Permission.SystemManager.Audit.AuditLogs)

                    If toDate = Date.MaxValue Then
                        Console.WriteLine(My.Resources.EitherFromdateAndTodateOrAgeMustBeUsed)
                        Return 1
                    ElseIf fromDate = Date.MinValue Then
                        '/age was used
                        fromDate = toDate
                        toDate = Date.Today + New TimeSpan(1, 0, 0, 0)
                    End If

                    Dim d As DataTable = Nothing
                    Try
                        gSv.GetAuditLogData(fromDate, toDate, d)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToRetrieveAuditLogs0, ex.Message)
                        Return 1
                    End Try

                    Dim dateRange = String.Format(My.Resources.DateRange0To1, fromDate.ToLongDateString, toDate.ToLongDateString)
                    If Not gSv.AuditRecordSysConfigEvent(SysConfEventCode.ExportAuditLog, dateRange) Then

                        Console.WriteLine(My.Resources.FailedToCreateAuditEntry)
                        Return 1
                    End If

                    Console.WriteLine(My.Resources.AuditResults0, d.Rows.Count)
                    For Each r As DataRow In d.Rows
                        Console.WriteLine("{0:u} {1}|{2}", r("Time"), r("Narrative"), r("Comments"))
                    Next
                    Return 0

                Case "getlog"
                    CheckLoggedInAndUserRole(Permission.Resources.ImpliedViewResource.Union(
                                             {Permission.SystemManager.GroupName}).ToArray())

                    'Check the user's supplied session ID
                    Dim gSessionID As Guid
                    Try
                        gSessionID = New Guid(sSessionID)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.SuppliedValue0IsNotAValidSessionIdentifier, sSessionID)
                        Console.WriteLine(My.Resources.TheCorrectFormatIs0, Guid.Empty.ToString.Replace("0", "x"))
                        Return 1
                    End Try

                    Dim iSessionNum As Integer
                    iSessionNum = gSv.GetSessionNumber(gSessionID)
                    If iSessionNum = -1 Then
                        Console.WriteLine(My.Resources.ERRORCouldNotLookUpSession)
                        Return 1
                    End If
                    Dim d As DataTable = Nothing
                    Try
                        Try
                            d = gSv.GetLogs(iSessionNum, 0, Integer.MaxValue)
                        Catch ex As Exception
                            Console.WriteLine(My.Resources.ERRORFailedToFetchLogData0, ex.Message)
                            Return 1
                        End Try
                        Console.WriteLine(My.Resources.LogForSession0, sSessionID
                                          )
                        For Each r As DataRow In d.Rows
                            Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss},{1},{2},{3}",
                             r("resource start"), r("stageid"), r("stagename"), r("result"))
                        Next
                        Return 0
                    Finally
                        d?.Dispose()
                    End Try
                Case "regobject"
                    Dim sFriendlyName As String
                    Try
                        Dim o As clsCOMBusinessObject = New clsCOMBusinessObject(sObjectCLSID, "")
                        sFriendlyName = o.FriendlyName
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ERRORCanTRegisterObject0, ex.Message)
                        Return 1
                    End Try
                    configOptions.AddObject(sObjectCLSID)
                    Try
                        configOptions.Save()
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToRegisterObject0, ex.Message)
                        Return 1
                    End Try
                    Console.WriteLine(My.Resources.RegisteredCOMBusinessObject0, sFriendlyName)
                    Return 0

                Case "regwebservice"
                    CheckLoggedInAndUserRole(
                        Permission.SystemManager.BusinessObjects.WebServicesSoap)

                    Try
                        'Set up the details of the web service according to the
                        'command-line parameters given...
                        Dim details As New clsWebServiceDetails() With {
                            .Id = Guid.NewGuid(),
                            .URL = sWebServiceURL,
                            .ServiceToUse = sWebServiceName,
                            .Username = wsuser,
                            .Secret = If(wsuser Is Nothing, Nothing, wspassword),
                            .timeout = If(timeout = -1, 10000, timeout)
                        }

                        Dim investigator As New clsWSDLProcess()
                        Dim services As Dictionary(Of String, clsWebService)
                        services = investigator.Import(sWebServiceURL, details)
                        If Not services.ContainsKey(sWebServiceName) Then
                            Console.WriteLine(My.Resources.TheWebService0IsNotDefinedByTheWSDLAtTheSpecifiedLocation, sWebServiceName)
                            Console.WriteLine(My.Resources.AvailableServicesAre)
                            For Each svcname As String In services.Keys
                                Console.WriteLine(" " & svcname)
                            Next
                            Return 1
                        End If
                        Dim service As clsWebService = services(sWebServiceName)
                        For Each action As clsWebServiceAction In service.GetActions()
                            details.Actions.Add(action.GetName(), True)
                        Next

                        details.FriendlyName = sWebServiceName
                        If sObjectName IsNot Nothing Then details.FriendlyName = sObjectName
                        Try
                            gSv.SaveWebServiceDefinition(details)
                        Catch ex As Exception
                            Console.WriteLine(My.Resources.FailedToAddAddTheWebServiceToTheDatabase0, ex.Message)
                            Return 1
                        End Try

                    Catch ex As Exception
                        Console.WriteLine(My.Resources.FailedToRegisterWebService0, ex.Message)
                        Return 1
                    End Try

                    Console.WriteLine(My.Resources.RegisteredWebService0, sWebServiceName)
                    Return 0

                Case "unregwebservice"
                    CheckLoggedInAndUserRole(
                        Permission.SystemManager.BusinessObjects.WebServicesSoap)

                    Try
                        gSv.DeleteWebservice(sWebServiceName)

                    Catch ex As Exception
                        Return Err(My.Resources.FailedToDeleteTheWebServiceFromTheDatabase0, ex.Message)

                    End Try

                    Console.WriteLine(My.Resources.UnregisteredWebService0, sWebServiceName)
                    Return 0

                Case "createdb"
                    If Not configOptions.DbConnectionSetting.WindowsAuth Then
                        If Not configOptions.ConfirmDatabasePassword(sModifyDBPwd) Then
                            Console.WriteLine(My.Resources.DatabasePasswordConfirmationFailed)
                            Return 1
                        End If
                    End If
                    Try

                        Dim activeDirectoryOptions As DatabaseActiveDirectorySettings = Nothing

                        If Not String.IsNullOrEmpty(sDomainName) Then
                            activeDirectoryOptions = New DatabaseActiveDirectorySettings(
                                sDomainName,
                                sActiveDirectoryAdminGroup,
                                "",
                                "",
                                Role.DefaultNames.SystemAdministrators)
                        End If

                        CreateInstaller().CreateDatabase(activeDirectoryOptions, bCreateDBDrop, False, maxDBVer)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ErrorCreatingDatabase0, ex.Message)
                        Return 1
                    End Try
                    Return 0

                Case "configuredb"
                    If Not configOptions.DbConnectionSetting.WindowsAuth Then
                        If Not configOptions.ConfirmDatabasePassword(sModifyDBPwd) Then
                            Console.WriteLine(My.Resources.DatabasePasswordConfirmationFailed)
                            Return 1
                        End If
                    End If
                    Try
                        Dim activeDirectoryOptions As DatabaseActiveDirectorySettings = Nothing

                        If Not String.IsNullOrEmpty(sDomainName) Then
                            activeDirectoryOptions = New DatabaseActiveDirectorySettings(
                                sDomainName,
                                sActiveDirectoryAdminGroup,
                                "",
                                "",
                                Role.DefaultNames.SystemAdministrators)
                        End If

                        CreateInstaller().CreateDatabase(activeDirectoryOptions, bCreateDBDrop, True, maxDBVer)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ErrorConfiguringDatabase0, ex.Message)
                        Return 1
                    End Try

                    Return 0

                Case "upgradedb"
                    If configOptions.DbConnectionSetting.ConnectionType = ConnectionType.BPServer Then
                        Console.WriteLine(My.Resources.CannotRemotelyUpgradeTheBluePrismServerDatabase)
                        Return 1
                    End If
                    If Not configOptions.DbConnectionSetting.WindowsAuth Then
                        If Not configOptions.ConfirmDatabasePassword(sModifyDBPwd) Then
                            Console.WriteLine(My.Resources.DatabasePasswordConfirmationFailed)
                            Return 1
                        End If
                    End If
                    Try
                        CreateInstaller().UpgradeDatabase(maxDBVer)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.UpgradeFailed0, ex.Message)
                        Return 1
                    End Try
                    'Rebuild dependency data (if required)
                    ServerFactory.ClientInit(configOptions.DbConnectionSetting)
                    gSv.RebuildDependencies()
                    Return 0

                Case "annotatedb"
                    If configOptions.DbConnectionSetting.ConnectionType = ConnectionType.BPServer Then
                        Console.WriteLine(My.Resources.CannotRemotelyAnnotateTheBluePrismServerDatabase)
                        Return 1
                    End If
                    If Not configOptions.DbConnectionSetting.WindowsAuth Then
                        If Not configOptions.ConfirmDatabasePassword(sModifyDBPwd) Then
                            Console.WriteLine(My.Resources.DatabasePasswordConfirmationFailed)
                            Return 1
                        End If
                    End If
                    Try
                        CreateInstaller().AnnotateDatabase()
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.AnnotationFailed0, ex.Message)
                        Return 1
                    End Try

                    Return 0

                Case "getresprotdocs"
                    Console.Write(documentationProvider.GetCommandWikiDocs())
                    Return 0

                Case "getvalidationdocs"

                    If Not User.LoggedIn Then
                        Console.WriteLine(My.Resources.ErrorYouMustSupplyValidLoginDetailsToDoThat)
                        Return 1
                    End If

                    Dim rules = gSv.GetValidationInfo()
                    Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)
                    Dim ob As New StringBuilder()
                    ob.AppendLine("{| border=1")
                    ob.AppendLine("!" & My.Resources.Code)
                    ob.AppendLine("!" & My.Resources.Category)
                    ob.AppendLine("!" & My.Resources.Type)
                    ob.AppendLine("!" & My.Resources.Message)
                    For Each v As clsValidationInfo In validationInfo.Values
                        ob.AppendLine("|-")
                        ob.AppendLine("| " & v.CheckID.ToString())
                        ob.AppendLine("| " & v.CatID.ToString())
                        ob.AppendLine("| " & v.TypeID.ToString())
                        ob.AppendLine("| " & v.Message)
                    Next
                    ob.AppendLine("|}")
                    Console.Write(ob.ToString())
                    Return 0

                Case "getresprothtmldocs"
                    Console.Write(documentationProvider.GetCommandHTMLDocs())
                    Return 0

                Case "genivbowrapper"

                    CheckLoggedInAndUserRole()

                    Try
                        Dim proc As New clsProcess(Nothing, DiagramType.Object, True)
                        clsAPC.ObjectLoader = DependencyResolver.Resolve(Of IObjectLoader)()

                        Dim obj As clsInternalBusinessObject = Nothing
                        Dim names As New List(Of String)
                        Dim sessionConnectionSettings = gSv.GetWebConnectionSettings()
                        Dim sess As New clsSession(Guid.NewGuid, 0, sessionConnectionSettings)
                        For Each obr As clsInternalBusinessObject In clsAPC.ObjectLoader.CreateAll(proc, sess)
                            names.Add(obr.Name)
                            If obr.Name = sIVBOToWrap Then
                                obj = obr
                            End If
                        Next
                        If obj Is Nothing Then
                            If sIVBOToWrap <> "?" Then
                                Console.WriteLine(My.Resources.NoSuchInternalBusinessObject)
                            End If
                            Console.WriteLine(My.Resources.AvailableInternalBusinessObjects)
                            For Each name As String In names
                                Console.WriteLine("  " & name)
                            Next
                            Return 1
                        End If

                        Dim procname As String = obj.FriendlyName

                        proc.Name = procname
                        proc.GetMainPage.Published = True

                        Dim st1, st2 As clsProcessStage

                        'Link start and end on the init (i.e. main) page...
                        st1 = proc.GetStageByTypeAndSubSheet(StageTypes.Start, Guid.Empty)
                        st2 = proc.GetStageByTypeAndSubSheet(StageTypes.End, Guid.Empty)
                        LinkStages(st1, st2, proc)

                        Dim pi As clsProcessInfoStage = CType(proc.GetStageByTypeAndSubSheet(StageTypes.ProcessInfo, Guid.Empty), clsProcessInfoStage)
                        pi.SetNarrative(obj.Narrative)

                        'Add the cleanup page...
                        Dim sheet As clsProcessSubSheet = proc.AddSubSheet("Clean Up")
                        sheet.Published = True
                        sheet.SheetType = SubsheetType.CleanUp

                        'Link start and end on the cleanup page...
                        sheet.StartStage.OnSuccess = sheet.EndStage.Id

                        For Each act As clsBusinessObjectAction In obj.GetActions()

                            sheet = proc.AddSubSheet(act.GetName())
                            sheet.Published = True
                            sheet.SheetType = SubsheetType.Normal

                            Dim info As clsSubsheetInfoStage = CType(proc.GetStageByTypeAndSubSheet(StageTypes.SubSheetInfo, sheet.ID), clsSubsheetInfoStage)
                            info.SetNarrative(act.GetNarrative())

                            Dim actstart As clsStartStage = CType(proc.GetStageByTypeAndSubSheet(StageTypes.Start, sheet.ID), clsStartStage)
                            Dim actend As clsEndStage = CType(proc.GetStageByTypeAndSubSheet(StageTypes.End, sheet.ID), clsEndStage)

                            st1 = proc.AddStage(StageTypes.Action, My.Resources.DoAction)
                            st1.SetSubSheetID(sheet.ID)
                            st1.SetDisplayX(15)
                            st1.SetDisplayY(0)
                            Dim ast As clsActionStage = CType(st1, clsActionStage)
                            ast.SetResource(sIVBOToWrap, act.GetName())
                            Dim paramY As Integer = -75
                            For Each p As clsProcessParameter In act.GetParameters()
                                st2 = proc.AddStage(StageTypes.Data, p.Name)
                                st2.SetSubSheetID(sheet.ID)
                                st2.SetDisplayX(90)
                                st2.SetDisplayY(paramY)
                                st2.SetDisplayWidth(90)
                                Dim ds As clsDataStage = CType(st2, clsDataStage)
                                ds.SetDataType(p.GetDataType())
                                ds.IsPrivate = True
                                paramY += 45
                                ast.AddParameter(p.Direction, p.GetDataType(), p.Name, p.Narrative, MapType.Stage, p.Name)
                                If p.Direction = ParamDirection.In Then
                                    actstart.AddParameter(ParamDirection.In, p.GetDataType(), p.Name, p.Narrative, MapType.Stage, p.Name)
                                Else
                                    actend.AddParameter(ParamDirection.Out, p.GetDataType(), p.Name, p.Narrative, MapType.Stage, p.Name)
                                End If
                            Next

                            LinkStages(actstart, st1, proc)
                            LinkStages(st1, actend, proc)

                        Next

                        Dim xml As String = proc.GenerateXML(True)
                        Dim filename As String = String.Format(My.Resources.BPAObject0Xml, procname
                                                               )
                        File.WriteAllText(filename, xml)
                        Console.WriteLine(My.Resources.Wrote0, filename)
                        Return 0

                    Catch ex As Exception
                        Console.WriteLine(My.Resources.Failed0, ex.Message)
                        Return 1
                    End Try


                Case "getdbdocs"
                    If configOptions.DbConnectionSetting.ConnectionType = ConnectionType.BPServer Then
                        Console.WriteLine(My.Resources.CannotRemotelyGetDocumentationFromBluePrismServerDatabase)
                        Return 1
                    End If
                    If Not configOptions.DbConnectionSetting.WindowsAuth Then
                        If Not configOptions.ConfirmDatabasePassword(sModifyDBPwd) Then
                            Console.WriteLine(My.Resources.DatabasePasswordConfirmationFailed)
                            Return 1
                        End If
                    End If
                    Try
                        Dim installer = CreateInstaller()
                        installer.AnnotateDatabase()
                        Console.Write(installer.GetDBDocs())
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.AnnotationFailed0, ex.Message)
                        Return 1
                    End Try

                    Return 0

                Case "exportqueue"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)

                    ' If queue is to be cleared after export, check we have permission to do so.
                    If bClearExported Then
                        CheckLoggedInAndUserRole(Permission.ControlRoom.ManageQueuesReadOnly, Permission.ControlRoom.ManageQueuesFullAccess)
                    End If

                    'Validate input
                    If queueFilter = "" Then
                        Console.WriteLine(My.Resources.NoQueueFilterSpecifiedExportingAllQueueItemsWithoutFiltering)
                    End If
                    If queueName = "" Then
                        Console.WriteLine(My.Resources.NoQueueSpecifiedExportqueueMustBeAccompaniedByQueuename)
                        Return 1
                    End If
                    If sFilespec = "" Then
                        Console.WriteLine(My.Resources.NoOutputFileSpecifiedExportqueueMustBeAccompaniedByFilespec)
                        Return 1
                    End If

                    'Resolve requested queue and filter
                    Dim Filter As WorkQueueFilter
                    If queueFilter <> "" Then
                        Try
                            Filter = WorkQueueUIFilter.FromName(queueFilter).ToContentFilter
                        Catch Ex As Exception
                            Console.WriteLine(My.Resources.ErrorCreatingFilterObject0, Ex.Message)
                            Return 1
                        End Try
                    Else
                        'get all content without filtering
                        Filter = New WorkQueueFilter()
                    End If
                    Dim QueueID As Guid
                    Try
                        gSv.WorkQueueGetQueueID(queueName, QueueID)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ErrorResolvingQueueID0, ex.Message)
                        Return 1
                    End Try
                    If QueueID = Guid.Empty Then
                        Console.WriteLine(My.Resources.ErrorNoQueueExistsWithTheName0, queueName)
                        Return 1
                    End If


                    'Now do the work
                    Console.WriteLine(My.Resources.ExportingQueueData)
                    Dim Total As Integer
                    Dim queueItems As ICollection(Of clsWorkQueueItem) = Nothing
                    Try
                        gSv.WorkQueuesGetQueueFilteredContents(QueueID, Filter, Total, queueItems)
                        'Write the report contents
                        Dim sr As StreamWriter = Nothing
                        Try
                            sr = New System.IO.StreamWriter(sFilespec, False)
                            clsWorkQueueItem.ToCsv(queueItems, sr)
                            Dim comments = LTools.Format(My.Resources.DataWrittenToFileFILESPECUsingCommandLineCOUNTPluralOne1ItemOtherItems, "FILESPEC",
                                                         sFilespec, "COUNT", queueItems.Count)
                            gSv.AuditRecordWorkQueueEvent(WorkQueueEventCode.ExportFromQueue, QueueID, "", comments)
                            Console.WriteLine(My.Resources.ReportWritten0, sFilespec)
                        Catch ex As Exception
                            Console.WriteLine(My.Resources.ErrorWhenCreatingFile0, ex.Message)
                            Return 1
                        Finally
                            If sr IsNot Nothing Then
                                sr.Close()
                            End If
                        End Try

                        'Delete the exported rows
                        If bClearExported And queueItems.Any() Then
                            Dim selectedItems As IList(Of clsWorkQueueItem) = queueItems.ToList()
                            Dim Deleted As Integer = gSv.WorkQueueClearWorked(QueueID, selectedItems, My.Resources.ItemsDeletedDueToClearexportedCommandlineOption, True)

                            Console.WriteLine(My.Resources.ExportedItemsDeletedTotal0Items, Deleted.ToString)

                        End If

                        Console.WriteLine(My.Resources.Done)
                        Return 0
                    Catch
                        Console.WriteLine(My.Resources.UnableToFetchContentFromDatabase0, sErr)
                        Return 1
                    End Try

                Case "queueclearworked"
                    'Check user roles for required rights
                    CheckLoggedInAndUserRole(Permission.ControlRoom.ManageQueuesFullAccess)

                    If queueName = "" Then
                        Console.WriteLine(My.Resources.NoQueueSpecifiedQueueclearworkedMustBeAccompaniedByQueuename)
                        Return 1
                    End If

                    Dim QueueID As Guid

                    Try
                        gSv.WorkQueueGetQueueID(queueName, QueueID)
                    Catch ex As Exception
                        Console.WriteLine(My.Resources.ErrorResolvingQueueID0, ex.Message)
                        Return 1
                    End Try
                    If QueueID = Guid.Empty Then
                        Console.WriteLine(My.Resources.ErrorNoQueueExistsWithTheName0, queueName)
                        Return 1
                    End If

                    Console.WriteLine(My.Resources.DeletingWorkedItems)
                    Dim Deleted As Integer
                    If toDate = DateTime.MinValue Then
                        Deleted = gSv.WorkQueueClearAllWorked(QueueID)
                    Else
                        '/age switch has been used
                        Deleted = gSv.WorkQueueClearWorkedByDate(queueName, toDate, False)
                    End If

                    gSv.AuditRecordWorkQueueEvent(WorkQueueEventCode.DeleteProcessFromQueue, QueueID, queueName, String.Format(My.Resources.x0ItemsDeletedDueToQueueclearworkedCommandlineOption, Deleted.ToString))
                    Console.WriteLine(My.Resources.Done0ItemsDeleted, Deleted.ToString)
                    Return 0

                Case "setencrypt"
                    CheckLoggedInAndUserRole(Permission.SystemManager.Workflow.WorkQueueConfiguration)
                    If queueName = "" Then Return Err(
                     My.Resources.YouMustSpecifyAQueueUsingTheQueuenameParameter)

                    Dim encryptID As Integer = 0
                    If queueEncrypter IsNot Nothing Then

                        Try
                            Dim scheme = gSv.GetEncryptionSchemeExcludingKey(queueEncrypter)
                            If Not scheme.FIPSCompliant Then
                                Throw New InvalidOperationException(My.Resources.FailedToSetEncryptionSchemeCannotEncryptDataWithANonFIPSCompliantAlgorithm)
                            End If
                            encryptID = scheme.ID
                        Catch ex As NoSuchEncrypterException
                            Return Err(ex.Message)
                        Catch ex As InvalidOperationException
                            Return Err(ex.Message)
                        End Try
                    End If

                    Dim q As clsWorkQueue = gSv.WorkQueueGetQueue(queueName)
                    If q Is Nothing Then Return Err(
                     My.Resources.NoQueueFoundWithTheName0, queueName)

                    q.EncryptionKeyID = encryptID

                    ' Exit without work if the encryption key the user set is the same
                    ' as that already specified for the queue (not an error, just silly).
                    If Not q.EncryptionKeyChanged Then
                        Console.WriteLine(
                         My.Resources.QueueEncryptionFor0AlreadySetTo1NoWorkDone,
                         queueName,
                         IIf(queueEncrypter Is Nothing, My.Resources.NoEncryption, queueEncrypter))
                        Return 0
                    End If

                    ' Update the queue with the new encrypter
                    gSv.UpdateWorkQueue(q)

                    ' Report success
                    If queueEncrypter Is Nothing Then
                        Console.WriteLine(My.Resources.QueueEncryptionResetFor0, queueName)
                    Else
                        Console.WriteLine(My.Resources.QueueEncryptionFor0SetTo1,
                         queueName, queueEncrypter)
                    End If
                    Return 0

                Case "reencryptdata"
                    CheckLoggedInAndUserRole(Auth.Permission.SystemManager.Security.ManageEncryptionSchemes)
                    'Set defaults for batch parameters
                    If batchSize = -1 Then batchSize = 1000
                    If maxBatches = -1 Then maxBatches = 1

                    'Re-encrypt credentials
                    Console.WriteLine(My.Resources.x0CheckingCredentials, Date.Now.ToLongTimeString)
                    Dim total As Integer = gSv.ReEncryptCredentials()
                    If total = -1 Then
                        Console.WriteLine(My.Resources.x0NoCredentialsRequireReEncrypting, Date.Now.ToLongTimeString)
                    Else
                        Console.WriteLine(My.Resources.x01CredentialsUpdated, Date.Now.ToLongTimeString, total)
                    End If

                    'Re-enrypt work queue items
                    total = 0
                    Console.WriteLine(My.Resources.x0CheckingQueueItems, Date.Now.ToLongTimeString)
                    While maxBatches > 0
                        Dim count As Integer = gSv.ReEncryptQueueItems(batchSize)
                        If count = -1 Then
                            If total = 0 Then Console.WriteLine(My.Resources.x0NoQueueItemsRequireReEncrypting, Date.Now.ToLongTimeString)
                            Exit While
                        End If
                        total += count
                        Console.WriteLine(My.Resources.x01QueueItemsUpdated, Date.Now.ToLongTimeString, total)
                        maxBatches -= 1
                    End While

                    'Re-encrypt resource screenshots
                    Console.WriteLine(My.Resources.x0CheckingResourceScreenCaptures, Date.Now.ToLongTimeString)
                    total = gSv.ReEncryptExceptionScreenshots()
                    If total = -1 Then
                        Console.WriteLine(My.Resources.x0NoScreenCapturesRequireReEncrypting, Date.Now.ToLongTimeString)
                    Else
                        Console.WriteLine(My.Resources.x01ScreenCapturesUpdated, Date.Now.ToLongTimeString, total)
                    End If

                    'Re-encrypt data pipeline configuration files
                    Console.WriteLine(My.Resources.x0CheckingDataPipelineConfigFiles, Date.Now.ToLongTimeString)
                    total = gSv.ReEncryptDataPipelineConfigurationFiles()
                    If total = -1 Then
                        Console.WriteLine(My.Resources.x0NoDataPipelineConfigFilesRequireReEncrypting, Date.Now.ToLongTimeString)
                    Else
                        Console.WriteLine(My.Resources.x01DataPipelineConfigFilesUpdated, Date.Now.ToLongTimeString, total)
                    End If

                Case "setallowanonresources"
                    CheckLoggedInAndUserRole(Permission.SystemManager.System.Settings)

                    gSv.SetAllowAnonymousResources(allowAnonResources)

                    Console.WriteLine(If(allowAnonResources, My.Resources.AnonymousResourcepcLoginsAreNowAllowed, My.Resources.AnonymousResourcepcLoginsAreNowDisallowed))
                    Return 0

                Case "enforcecontrollinguserpermissions"
                    CheckLoggedInAndUserRole(Permission.SystemManager.System.Settings)

                    gSv.SetControllingUserPermissionSetting(enforceControllingUserPermissions)

                    Console.WriteLine(If(enforceControllingUserPermissions, My.Resources.ControllingUserPermissionsIsEnforced, My.Resources.ControllingUserPermissionsIsNotEnforced))
                    Return 0

                Case "createcredential", "updatecredential"
                    'Indicates whether we are updating an existing, or creating a brand new credential
                    Dim isUpdating As Boolean = sAction = "updatecredential"

                    Return CreateOrUpdateCredential(credentialName,
                                                    credentialUsername,
                                                    credentialPassword,
                                                    expiryDate,
                                                    description,
                                                    isInvalid,
                                                    isInvalidSet,
                                                    credentialTypeName,
                                                    isUpdating)
                Case "setcredentialproperty"
                    Return SetCredentialProperty(credentialName, credentialPropertyName, credentialPropertyValue)

                Case "installlocaldb"
                    Return InstallLocalDb(sLocalDbUserName, sLocalDbPassword)

                Case "disablewelcome"
                    Return DisableWelcome()

                Case "createmappedactivedirectorysysadmin"
                    Return CreateMappedActiveDirectoryUser(adUserName, adSid, singleSignon)

                Case "setactivedirectoryauth"
                    Return SetActiveDirectorySignOnSetting(adEnableAuth, singleSignon)

                Case "mapauthenticationserverusers"

                    If Not mAuthenticationServerFeatureEnabled Then
                        Console.WriteLine(My.Resources.ERRORUnableToProcessAction0, sAction)
                        Return 1
                    End If

                    Return MapAuthenticationServerUsers(mapAuthenticationServerUsersInputCsvPath,
                                                        mapAuthenticationServerUsersOutputCsvPath)

                Case "getblueprismtemplateforusermapping"
                    Return CreateBluePrismUserMappingTemplate(bluePrismUserTemplateOutputCsvPath)

                Case Else
                    Console.WriteLine(My.Resources.ERRORUnableToProcessAction0, sAction)
                    Return 1
            End Select

            ' CommandLineFailureExceptions should have a reasonably useful message already
        Catch clfe As CommandLineFailureException
            Return Err(clfe.Message)
        Catch pe As PermissionException
            Return Err(pe.Message)
            ' Any other exception - provide a little bit of context
        Catch ex As Exception
            Return Err(My.Resources.ErrorProcessingAction012, sAction, vbCrLf, ex)

        End Try

    End Function

    Private Function MapAuthenticationServerUsers(mapAuthenticationServerUsersInputCsvPath As String,
                                                  mapAuthenticationServerUsersOutputCsvPath As String) As Integer
        Try
            CheckLoggedInAndUserRole(Permission.SystemManager.AuthenticationServer.MapAuthenticationServerUsers)
            Dim usersToMap = ReadUserMappingInputCsv(mapAuthenticationServerUsersInputCsvPath)

            If Not usersToMap.Any() Then
                Dim noInputRecordsFoundMessage = My.Resources.MapAuthenticationServerUsers_Norecordsfoundinauthenticationserverusermappinginputcsv
                Console.WriteLine(noInputRecordsFoundMessage)
                Log.Warn(noInputRecordsFoundMessage)
                Return 0
            End If

            Dim notifier As New BackgroundJobNotifier()
            Log.Info(My.Resources.MapAuthenticationServerUsers_Startingauthenticationserverusermappingprocess)

            Dim job = gSv.MapAuthenticationServerUsers(usersToMap, notifier)

            Dim jobResult = job.Wait(notifier, AddressOf HandleBackgroundJobupdate).Result

            If TypeOf jobResult.Data.ResultData IsNot MapUsersResult Then
                Console.WriteLine(My.Resources.MapAuthenticationServerUsers_Anerroroccurredwhenprocessingresultfromserver)
                Return 0
            End If

            HandleAuthenticationServerMappingJobResult(jobResult, mapAuthenticationServerUsersOutputCsvPath)

            Return 0
        Catch ex As Exception
            Return Err(My.Resources.MapAuthenticationServerUsers_Errormappingusers0, ex)
        End Try
    End Function
    Private Function CreateBluePrismUserMappingTemplate(bluePrismUserTemplateOutputCsvPath As String) As Integer

        CheckLoggedInAndUserRole(Permission.SystemManager.AuthenticationServer.MapAuthenticationServerUsers)

        Try
            Console.WriteLine(My.Resources.MapBluePrismUsersToTemplate_Starting)
            Log.Info(My.Resources.MapBluePrismUsersToTemplate_Starting)

            Dim passwordRules As PasswordRules = Nothing
            Dim logonOptions As LogonOptions = Nothing
            gSv.GetSignonSettings(passwordRules, logonOptions)

            If logonOptions.SingleSignon Then
                Return Err(My.Resources.CannotCreateBluePrismUserTemplateInSSOEnvironment)
            End If

            If Not WriteAuditLogEntry(SysConfEventCode.StartCreationOfBluePrismUserTemplate, String.Empty)
                Return 1
            End If

            Dim userNames = gSv.GetAllNativeBluePrismUserNames()

            If Not userNames.Any() Then
                WriteAuditLogEntry(SysConfEventCode.CompletedCreationOfBluePrismUserTemplate, My.Resources.MapBluePrismUsersToTemplate_NoUsersFound)
                Console.WriteLine(My.Resources.MapBluePrismUsersToTemplate_NoUsersFound)
                Log.Warn(My.Resources.MapBluePrismUsersToTemplate_NoUsersFound)
                Return 0
            End If

            Dim userMappingRecords = 
                (From name In userNames Select New UserMappingCsvRecord() With { .BluePrismUsername = name }).ToList()

            Dim csv = New CsvUserTemplateFileWriter(New StreamWriterFactory())
            csv.Write(New CsvUserTemplate(bluePrismUserTemplateOutputCsvPath, My.Resources.MapUsersToTemplate_CsvFileHeading, userMappingRecords))

            Dim countOfUsersAddedMessage = String.Format(My.Resources.MapBluePrismUsersToTemplate_CountOfUsersAdded, userMappingRecords.Count)
            WriteAuditLogEntry(SysConfEventCode.CompletedCreationOfBluePrismUserTemplate, countOfUsersAddedMessage)
            Dim templateCompletedMessageWithUserCount = String.Format(My.Resources.MapBluePrismUsersToTemplate_Completed, userMappingRecords.Count)
            Log.Info(templateCompletedMessageWithUserCount)
            Console.WriteLine(templateCompletedMessageWithUserCount)

            Return 0
        Catch ex As Exception
            WriteAuditLogEntry(SysConfEventCode.CreationOfBluePrismUserTemplateFailed, ex.Message)
            Log.Error(ex, My.Resources.MapBluePrismUsersToTemplate_ErrorMappingUsers0)
            Return Err(My.Resources.MapBluePrismUsersToTemplate_ErrorMappingUsers0, ex.Message)
        End Try

    End Function
    Private Function WriteAuditLogEntry(eventCode As SysConfEventCode, comment As String) As Boolean
        If Not gSv.AuditRecordSysConfigEvent(eventCode, comment) Then
            Log.Info(My.Resources.FailedToCreateAuditEntry)
            Console.WriteLine(My.Resources.FailedToCreateAuditEntry)
            Return False
        End If
        Return True
    End Function
    Private Sub HandleAuthenticationServerMappingJobResult(jobResult As BackgroundJobResult, mapAuthenticationServerUsersOutputCsvPath As String)

        Dim result = CType(jobResult.Data.ResultData, MapUsersResult)
        Select Case result.Status
            Case MapUsersStatus.Completed
                Console.WriteLine(My.Resources.MapAuthenticationServerUsers_MappingCompleted)
                Console.WriteLine(String.Format(My.Resources.MapAuthenticationServerUsers_0RecordsSuccessfullyMapped, result.SuccessfullyMappedRecordsCount))

            Case MapUsersStatus.CompletedWithErrors
                Console.WriteLine(My.Resources.MapAuthenticationServerUsers_MappingCompletedWithErrors)
                Console.WriteLine(String.Format(My.Resources.MapAuthenticationServerUsers_0RecordsSuccessfullyMapped, result.SuccessfullyMappedRecordsCount))
                Console.WriteLine(String.Format(My.Resources.MapAuthenticationServerUsers_0RecordsFailedToBemapped, result.RecordsThatFailedToMap?.Count))
            Case MapUsersStatus.Failed
                Console.WriteLine(My.Resources.MapAuthenticationServerUsers_MappingFailed)
                Select Case result.ErrorCode
                    Case MapUsersErrorCode.InvalidActionInSsoEnvironment
                        Console.WriteLine(My.Resources.MapAuthenticationServerUsers_CannotPerformMappingInSingleSignOnEnvironment)
                    Case MapUsersErrorCode.AuthenticationServerCredentialIdNotSet
                        Console.WriteLine(My.Resources.MapAuthenticationServerUsers_AuthenticationServerAPICredentialNotConfigured)
                    Case MapUsersErrorCode.AuthenticationServerUrlNotSet
                        Console.WriteLine(My.Resources.MapAuthenticationServerUsers_AuthenticationServerURLnotconfigured)
                    Case MapUsersErrorCode.MappingNotAvailableWhenAuthenticationServerEnabled
                        Console.WriteLine(My.Resources.MapAuthenticationServerUsers_UserMappingIsNotAvailableWhenAuthenticationServerIsEnabled)
                    Case MapUsersErrorCode.UnexpectedError
                        Console.WriteLine(My.Resources.MapAuthenticationServerUsers_Anunexpectederroroccurredwhenmappingauthenticationserverusers)
                End Select
        End Select

        WriteErrorsToOutputCsv(mapAuthenticationServerUsersOutputCsvPath, result)
    End Sub

    Private Function ReadUserMappingInputCsv(inputCsvPath As String) As List(Of UserMappingRecord)
        Dim userMappingCsvRecords = New List(Of UserMappingRecord)

        Try
            Dim streamReaderFactory = New StreamReaderFactory()
            Dim csvReader = New CsvUserMappingFileReader(streamReaderFactory)
            userMappingCsvRecords = csvReader.Read(inputCsvPath)
        Catch ex As Exception
            Dim errorMessage = String.Format(My.Resources.MapAuthenticationServerUsers_Errorreadingusermappingcsvatpath0, inputCsvPath)
            Console.WriteLine(errorMessage)
            Log.Error(ex, errorMessage)
        End Try

        Return userMappingCsvRecords
    End Function

    Private Sub WriteErrorsToOutputCsv(mapAuthenticationServerUsersOutputCsvPath As String, result As MapUsersResult)
        Try
            Dim errorList = result.RecordsThatFailedToMap.ToList()
            If errorList.Any() Then
                Console.WriteLine(String.Format(My.Resources.MapAuthenticationServerUsers_Writing0errorstooutputcsv1, errorList.Count, mapAuthenticationServerUsersOutputCsvPath))
                Dim streamWriterFactory = New StreamWriterFactory()
                Dim errorLogger As New CsvUserMappingErrorLogger(streamWriterFactory)
                errorLogger.LogErrors(mapAuthenticationServerUsersOutputCsvPath, errorList)
            End If
        Catch ex As Exception
            Dim errorMessage = String.Format(My.Resources.MapAuthenticationServerUsers_Errorwritingmappingerrorstooutputcsvatpath0, mapAuthenticationServerUsersOutputCsvPath)
            Console.WriteLine(errorMessage)
            Log.Error(ex, errorMessage)
        End Try
    End Sub

    Private Function LoginSession(listloginsParam As Boolean) As Integer
        CheckLoggedInAndUserRole(Auth.Permission.SystemManager.Security.Users)
        Try
            Dim loggedInUsers = gSv.GetLoggedInUsersAndMachines()
            If loggedInUsers.Any Then
                Console.WriteLine(My.Resources.AtLeastOneUserLogin)
                If listloginsParam Then
                    Dim listLogins As New List(Of String)
                    For Each item In loggedInUsers
                        listLogins.Add($"{item.machineName}\{item.userName}")
                    Next

                    listLogins.Distinct().ToList().ForEach(Sub(s) Console.WriteLine(s))
                End If
            End If

            Dim runningSessions = gSv.GetRunningSessions()
            If runningSessions.Any Then
                Console.WriteLine(My.Resources.AtLeastOneSessionRunning)
            End If

            If Not loggedInUsers.Any AndAlso Not runningSessions.Any Then
                Console.WriteLine(My.Resources.NoActiveLoginOrSession)
            End If
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return 1
        End Try
        Return 0
    End Function

    Private Function IsValidLimitType(limit As String, limitNumber As Integer, ByRef limitType As LimitType, ByRef earliestDate As DateTime) As Boolean
        limitNumber = limitNumber * -1
        Select Case limit.ToLowerInvariant
            Case "m"
                limitType = LimitType.Minutes
                earliestDate = Date.Now.AddMinutes(limitNumber)
                Return True
            Case "h"
                limitType = LimitType.Hours
                earliestDate = Date.Now.AddHours(limitNumber)
                Return True
            Case "d"
                limitType = LimitType.Days
                earliestDate = Date.Now.AddDays(limitNumber)
                Return True
            Case "mm"
                limitType = LimitType.Months
                earliestDate = Date.Now.AddMonths(limitNumber)
                Return True
        End Select

        Return False
    End Function

    Private Function CreateInstaller() As IInstaller
        Dim factory = DependencyResolver.Resolve(Of Func(Of ISqlDatabaseConnectionSetting, TimeSpan, String, String, IInstaller))
        Dim configOptions = Options.Instance
        Return factory(
            configOptions.DbConnectionSetting.CreateSqlSettings(),
            configOptions.DatabaseInstallCommandTimeout,
            ApplicationProperties.ApplicationName,
            clsServer.SingleSignOnEventCode)
    End Function

    Private Function SetCredentialProperty(name As String, propertyName As String, newValue As SafeString) As Integer
        CheckLoggedInAndUserRole(Permission.SystemManager.Security.Credentials)

        Try
            gSv.RequestSetCredentialProperty(name, propertyName, newValue)
        Catch ex As Exception When TypeOf ex Is PermissionException OrElse
            TypeOf ex Is NoSuchCredentialException

            Return Err(My.Resources.FailedToSetCredentialProperty0, ex.Message)
        End Try

        Console.WriteLine(My.Resources.SuccessfullySetCredentialProperty)
        Return 0
    End Function

    Private Function InstallLocalDb(username As String, password As SafeString) As Integer
        Try
            If LocalDatabaseInstaller.InstallRequested Then
                Dim installer = New LocalDatabaseInstaller()
                installer.FullInstall(username, password)
                Return 0
            Else
                Return 1
            End If
        Catch ex As Exception
            Return Err(My.Resources.LocalDbInstallationError, ex.ToString())
        End Try
    End Function

    Private Function DisableWelcome() As Integer
        Try
            gSv.SetUserPref(PreferenceNames.UI.ShowTourAtStartup, False)
            Return 0
        Catch ex As Exception
            Return Err(My.Resources.DisableWelcomeError, ex.ToString())
        End Try
    End Function

    Private Function CreateOrUpdateCredential(name As String,
                                              username As String,
                                              password As SafeString,
                                              expiry As DateTime,
                                              description As String,
                                              isInvalid As Boolean,
                                              isInvalidSet As Boolean,
                                              credentialTypeName As String,
                                              isUpdate As Boolean) As Integer

        CheckLoggedInAndUserRole(Permission.SystemManager.Security.Credentials)

        'Check if the credential already exists (and whether it should do)
        Dim credentialID As Guid = gSv.GetCredentialID(name)
        Dim credentialExists As Boolean = credentialID <> Guid.Empty
        If credentialExists <> isUpdate Then
            Return Err(
                If(isUpdate,
                   String.Format(My.Resources.CannotFindCredentialWithName0NUseCreatecredentialToCreateANewCredential, name),
                   String.Format(My.Resources.ACredentialWithName0AlreadyExistsNUseUpdatecredentialIfYouWishToOverwriteThisCr, name)))
        End If

        ' Add/update the credential properties
        Dim credential = New clsCredential
        If isUpdate Then credential = gSv.GetCredentialIncludingLogon(credentialID)

        ' Start to add the passed in properties...
        credential.Name = name

        ' If optional parameters are blank/default (i.e. were not passed in),
        ' take care not to overwrite the existing values while updating
        credential.Username =
            If(isUpdate AndAlso String.IsNullOrEmpty(username),
               credential.Username, username)

        credential.Password = If(
            isUpdate AndAlso password.IsEmpty,
            credential.Password, password)

        credential.ExpiryDate = If(
            expiry = Date.MinValue, credential.ExpiryDate, expiry)

        ' isInvalid is optional and we cannot just check for the default
        ' value with a Boolean (as False could have been the value passed
        ' in), thus we check the isInvalidSet flag to see whether to
        ' update it or not
        credential.IsInvalid = If(
            Not isInvalidSet, credential.IsInvalid, isInvalid)

        credential.Description = If(
            description = String.Empty, credential.Description, description)

        If credentialTypeName <> "" Then
            Try
                credential.Type = CredentialType.GetByName(credentialTypeName)
            Catch
                Return Err(My.Resources.InvalidCredentialType0SupportedTypesAre1,
                           credentialTypeName,
                           String.Join(", ", CredentialType.GetAll().Select(Function(c) c.Name)))
            End Try
        End If

        Try
            If isUpdate Then
                gSv.UpdateCredential(credential, credential.Name, Nothing, Not password.IsEmpty)
            Else
                ' When creating a new credential, set the restrictions
                ' (can be accessesed by any role, resource or process)
                credential.Roles.Add(Nothing)
                credential.ProcessIDs.Add(Guid.Empty)
                credential.ResourceIDs.Add(Guid.Empty)
                gSv.CreateCredential(credential)

            End If
        Catch e As Exception
            Return Err(My.Resources.FailedToSaveCredential0, e.Message)
        End Try

        Console.WriteLine(My.Resources.SuccessfullySavedCredential0, credential.Name)
        Return 0
    End Function


    ''' <summary>
    ''' Checks if the next element in the cmdargs array a) exists and b) is not a
    ''' parameter and advances the argument mode if that is the case. This will
    ''' display an error to the user if it is not the case.
    ''' </summary>
    ''' <param name="args">The argument array</param>
    ''' <param name="currInd">The current index within the array</param>
    ''' <param name="mode">The current argument mode of the parser</param>
    ''' <param name="nextMode">The next mode required if an argument exists</param>
    ''' <param name="currParam">The name of the current parmaeter (for use when
    ''' displaying an error message only)</param>
    ''' <returns>True if a non-switch argument was found next in the array, and
    ''' the mode was advanced as a result; False if there was no subsequent
    ''' argument in the array, or there was one and it started with a "/" char -
    ''' ie. it was a switch argument.</returns>
    Private Function AdvanceModeIfSafe(ByRef args() As String, ByVal currInd As Integer,
      ByRef mode As ArgMode, ByVal nextMode As ArgMode,
      ByVal currParam As String) As Boolean
        Return AdvanceModeIfSafe(args, currInd, mode, nextMode, currParam, True)
    End Function

    ''' <summary>
    ''' Checks if the next element in the cmdargs array a) exists and b) is not a
    ''' parameter and advances the argument mode if that is the case.
    ''' </summary>
    ''' <param name="args">The argument array</param>
    ''' <param name="currInd">The current index within the array</param>
    ''' <param name="mode">The current argument mode of the parser</param>
    ''' <param name="nextMode">The next mode required if an argument exists</param>
    ''' <param name="currParam">The name of the current parmaeter (for use when
    ''' displaying an error message only)</param>
    ''' <param name="displayError">True to display an error, false otherwise.
    ''' </param>
    ''' <returns>True if a non-switch argument was found next in the array, and
    ''' the mode was advanced as a result; False if there was no subsequent
    ''' argument in the array, or there was one and it started with a "/" char -
    ''' ie. it was a switch argument.</returns>
    Private Function AdvanceModeIfSafe(ByRef args() As String, ByVal currInd As Integer,
      ByRef mode As ArgMode, ByVal nextMode As ArgMode,
      ByVal currParam As String, ByVal displayError As Boolean) As Boolean

        Try
            ' If the next arg isn't a switch
            If Not args(currInd + 1).StartsWith("/") Then mode = nextMode : Return True
        Catch ' Ignore - that's the sound of an array index out of bounds
        End Try

        If displayError Then Console.WriteLine(
         My.Resources.BadCommandLineArgumentsArgumentMissingForParameter0,
         currParam)

        Return False

    End Function

    ''' <summary>
    ''' Reports progress using the given percentage and message on the current
    ''' console line.
    ''' </summary>
    ''' <param name="perc">The percentage of progress to report.</param>
    ''' <param name="msg">The message to send to the console</param>
    Private Sub ReportProgress(ByVal perc As Integer, ByVal msg As Object)
        Console.CursorLeft = 0
        Console.Write(My.Resources.progress_percentage, perc, msg)
        Threading.Thread.Sleep(10)
        Console.Out.Flush()
    End Sub

    ''' <summary>
    ''' Returns the ID of the passed Process or Object name, along with a flag
    ''' to indicate whether it is an Object or a Process.
    ''' </summary>
    ''' <param name="name">The name of the Process/Object to look for</param>
    ''' <param name="isObject">Returned as True if the name relates to an Object,
    ''' otherwise False</param>
    ''' <returns>The Process/Object ID if one was found, otherwise Guid.Empty
    ''' </returns>
    Private Function GetProcessOrObjectID(name As String, ByRef isObject As Boolean) As Guid

        'Look for matching processes first
        Dim procid = gSv.GetProcessIDByName(name, False)
        If procid <> Guid.Empty Then isObject = False : Return procid

        'Not a process, so look for matching Objects
        procid = gSv.GetProcessIDByName(name, True)
        If procid <> Guid.Empty Then isObject = True : Return procid

        'No match found
        Return Guid.Empty
    End Function

    ''' <summary>
    ''' Report background job progress to console
    ''' </summary>
    ''' <param name="jobdata">The background job data</param>
    Private Sub HandleBackgroundJobupdate(jobdata As BackgroundJobData)
        Console.WriteLine(String.Format(My.Resources.x0Complete1,
            jobdata.PercentComplete, jobdata.Description))
    End Sub

    Private Function CreateMappedActiveDirectoryUser(username As String, sid As String, singleSignon As Boolean) As Integer
        CheckLoggedInAndUserRole(Permission.SystemManager.Security.Users)

        If singleSignon Then
            Return Err(My.Resources.YouMustUseANativeDatabaseToAddMappedActiveDirectoryUsers)
        End If

        Dim passwordRules As PasswordRules = Nothing
        Dim logonOptions As LogonOptions = Nothing
        gSv.GetSignonSettings(passwordRules, logonOptions)

        If Not logonOptions.MappedActiveDirectoryAuthenticationEnabled Then
            Return Err(My.Resources.CannotCreateMappedActiveDirectoryUserWhenActiveDirectoryIsDisabled)
        End If

        Dim user = New User(AuthMode.MappedActiveDirectory, Guid.NewGuid(), username)
        user.ExternalId = sid
        user.Roles.Add(SystemRoleSet.Current(Role.DefaultNames.SystemAdministrators))

        gSv.CreateNewMappedActiveDirectoryUser(user)

        Return 0
    End Function

    Private Function SetActiveDirectorySignOnSetting(activeDirectoryAuthenticationEnabled As Boolean, singleSignon As Boolean) As Integer
        CheckLoggedInAndUserRole(Permission.SystemManager.Security.SignOnSettings)

        If singleSignon Then
            Return Err(My.Resources.YouMustUseANativeDatabaseToAddMappedActiveDirectoryUsers)
        End If

        Dim passwordRules As PasswordRules = Nothing
        Dim logonOptions As LogonOptions = Nothing
        gSv.GetSignonSettings(passwordRules, logonOptions)
        If logonOptions.MappedActiveDirectoryAuthenticationEnabled = activeDirectoryAuthenticationEnabled Then
            Return Err(My.Resources.ActiveDirectoryAuthIsAlreadySetTo0, activeDirectoryAuthenticationEnabled)
        End If

        If Not activeDirectoryAuthenticationEnabled Then
            Dim users = gSv.GetAllUsers(False)
            If users.Any(Function(x) x.AuthType = AuthMode.MappedActiveDirectory AndAlso Not x.Deleted) Then
                Return Err(My.Resources.CannotDistableActiveDirectoryWhenUsersArePresent)
            End If
        End If

        logonOptions.MappedActiveDirectoryAuthenticationEnabled = activeDirectoryAuthenticationEnabled
        gSv.SetSignonSettings(passwordRules, logonOptions)

        Return 0
    End Function

    Private Sub UpdateAutomateCAlive(state As Object)
        Try
            If Not User.LoggedIn Then Return
            If mSendServerStatusUpdate > 0 Then
                gSv.SetKeepAliveTimeStamp(True)
            End If
        Catch
            'silent ignore
        End Try
    End Sub

    Private Sub ClearAutomateCAlive()
        Dim sErr As String = Nothing
        Try
            If Not User.LoggedIn Then Return
            Threading.Interlocked.Decrement(mSendServerStatusUpdate)
            System.Threading.Thread.Sleep(500)
            gSv.ClearKeepAliveTimeStamp(True)
        Catch
            'silent ignore
        End Try
    End Sub

End Module
