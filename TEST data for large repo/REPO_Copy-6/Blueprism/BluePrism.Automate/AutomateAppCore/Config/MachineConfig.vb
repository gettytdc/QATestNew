
Imports System.Data.SqlClient
Imports System.IO
Imports System.Reflection
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Ipc
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Xml

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.ClientServerResources.Core.Config
Imports BluePrism.Common.Security
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models

Imports NLog

<Serializable()>
Public Class MachineConfig : Inherits BaseConfig : Implements IDisposable

#Region " Class-scope declarations "

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    'make it align with ConfigEncryptionMethod in BPServer
    Public Enum ConfigEncryptionMethod
        BuiltIn = 0
        BluePrismCreatedCertificate = 2
        OwnCertificate = 1
    End Enum


    ''' <summary>
    ''' The name used for the default server configuration in the current
    ''' installation
    ''' </summary>
    ''' <remarks></remarks>
    Public Const DefaultServerConfigName As String = "Default"

    ''' <summary>
    ''' Enumeration describing the current status of the saving of this configuration
    ''' object's data. It starts as Untried - on a successful save, the method of
    ''' that save is recorded. On an unsuccessful save, the failure type is recorded
    ''' </summary>
    Private Enum SaveStatus
        Untried = 0
        SavedDirect
        SavedViaPipe
        UserCancelled
        Failed
    End Enum

    ''' <summary>
    ''' Encapsulates a set of options for a server configuration. Multiple server
    ''' configurations can exist.
    ''' </summary>
    <Serializable()>
    Public Class ServerConfig

        ' The supported types of server keystore
        Public Enum KeyStoreType
            Embedded
            ExternalFile
        End Enum

        ' The options owner of these server options
        Private mOwner As MachineConfig

        ' Whether to write verbose log entries to the event log for this server
        Private mVerbose As Boolean

        ' Whether to log detailed traffic (e.g. remoting calls) information
        Private mLogTraffic As Boolean

        ''' <summary>
        ''' The name of this server.
        ''' </summary>
        Public Name As String

        ''' <summary>
        ''' The name of the database connection setting to be used by the server.
        ''' </summary>
        Public Connection As String

        ' The port on which the server will listen
        Private mPort As Integer

        ''' <summary>
        ''' The connection mode that is used with the Blue Prism Server.
        ''' </summary>
        Public ConnectionMode As ServerConnection.Mode

        ''' <summary>
        ''' The IP address for the server to bind to (can be used when there are
        ''' multiple NICs on the host machine, for example) or Nothing to not set
        ''' this.
        ''' </summary>
        Public BindTo As String

        ''' <summary>
        ''' The type of keystore in use for this server configuration
        ''' </summary>
        Public KeyStore As KeyStoreType

        ''' <summary>
        ''' The folder containing the keystore files (KeyStoreType.ExternalFiles
        ''' only).
        ''' </summary>
        Public ExternalKeyStoreFolder As String

        ''' <summary>
        ''' The encryption keys. This should only be stored in the
        ''' configuration on a server machine. When client machines use encryption
        ''' management without a secure Blue Prism Server in place, the key should
        ''' instead be stored in the database.
        ''' </summary>
        Public EncryptionKeys As New Dictionary(Of String, clsEncryptionScheme)

        ''' <summary>
        ''' True to log server service status messages to the system event log. This
        ''' can result in a lot of event log messages, but can be useful to diagnose
        ''' problems.
        ''' </summary>
        Public StatusEventLogging As Boolean

        ' Whether the scheduler should be disable on this server instance
        Private mSchedulerDisabled As Boolean

        Public MaxTransferWindowSize As Integer?

        Public MaxPendingChannels As Integer?

        Public Ordered As Boolean? = True

        ''' <summary>
        ''' Creates a new server config object owned by the given local config
        ''' </summary>
        ''' <param name="owner">The owning configuration object.</param>
        Public Sub New(ByVal owner As MachineConfig)
            mOwner = owner
            StatusEventLogging = True
            ConnectionMode = ServerConnection.Mode.WCFSOAPMessageWindows
            DataPipelineProcessCommandListenerPort = 8101
        End Sub

        ''' <summary>
        ''' Indicates whether this ServerConfig is the default configuration
        ''' for the installation.
        ''' </summary>
        Public ReadOnly Property IsDefault As Boolean
            Get
                Return Name = DefaultServerConfigName
            End Get
        End Property

        ''' <summary>
        ''' The port the server should listen on.
        ''' A value of zero effectively sets the port to the
        ''' <see cref="Options.DefaultServerPort"/> value.
        ''' </summary>
        Public Property Port() As Integer
            Get
                If mPort = 0 Then Return Options.Instance.DefaultServerPort
                Return mPort
            End Get
            Set(ByVal value As Integer)
                mPort = value
            End Set
        End Property

        ''' <summary>
        ''' Whether verbose logging is turned on for this server configuration
        ''' </summary>
        Public Property Verbose() As Boolean
            Get
                Return mVerbose
            End Get
            Set(ByVal value As Boolean)
                mVerbose = value
            End Set
        End Property

        ''' <summary>
        ''' Whether traffic logging is turned on for this server configuration
        ''' </summary>
        Public Property LogTraffic() As Boolean
            Get
                Return mLogTraffic
            End Get
            Set(ByVal value As Boolean)
                mLogTraffic = value
            End Set
        End Property

        ''' <summary>
        ''' Whether the scheduler should be disabled for this instance of the server
        ''' </summary>
        Public Property SchedulerDisabled() As Boolean
            Get
                Return mSchedulerDisabled
            End Get
            Set(ByVal value As Boolean)
                mSchedulerDisabled = value
            End Set
        End Property

        ''' <summary>
        ''' Whether this app server instance will start a data pipeline process.
        ''' </summary>
        ''' <returns></returns>
        Public Property DataPipelineProcessEnabled() As Boolean

        ''' <summary>
        ''' Listen for TCP connections on this port, used to send commands to the data pipeline process managed by this app server.
        ''' </summary>
        ''' <returns></returns>
        Public Property DataPipelineProcessCommandListenerPort() As Integer

        ''' <summary>
        ''' Whether this app server instance will start a data gateways process as a specific user.
        ''' </summary>
        ''' <returns></returns>
        Public Property DataGatewaySpecificUser As Boolean

        ''' <summary>
        ''' Specific users domain that the data gateways process will run as.
        ''' </summary>
        ''' <returns></returns>
        Public Property DataGatewayDomain As String

        ''' <summary>
        ''' Specific users username that the data gateways process will run as.
        ''' </summary>
        ''' <returns></returns>
        Public Property DataGatewayUser As String

        ''' <summary>
        ''' Specific users password that the data gateways process will run as.
        ''' </summary>
        ''' <returns></returns>
        Public Property DataGatewayPassword As SafeString

        ''' <summary>
        ''' This is the true reference to ConnectionConfig from Load/Save
        ''' For global usage use <see cref="BluePrism.AutomateAppCore.Config.IOptions.ResourceCallbackConfig"/>
        ''' as this can be trusted to hold values for server generate values i.e. client certificate
        ''' </summary>
        ''' <returns></returns>
        Public Property CallbackConnectionConfig As New ConnectionConfig()

        Public Property DataGatewayLogToConsole As Boolean

        Public Property DataGatewayTraceLogging As Boolean

        Public Property AuthenticationServerBrokerConfig As AuthenticationServerBrokerConfig

        ''' <summary>
        ''' Get the connection settings for this server, as currently configured.
        ''' </summary>
        ''' <returns>A clsDBConnectionSetting, or Nothing if it is not configured.
        ''' </returns>
        Public Function GetServerConnection() As clsDBConnectionSetting
            Return mOwner.GetConnection(Connection)
        End Function

        ''' <summary>
        ''' Returns the name, allowing these items to be used in a combo box.
        ''' </summary>
        ''' <returns>The name of this server configuration.</returns>
        Public Overrides Function ToString() As String
            Return Name
        End Function

    End Class

#End Region

#Region " Member Variables "

    ' The configuration manager - responsible for loading / saving the config
    <NonSerialized()>
    Private mManager As ConfigManager

    ' The save status of this local config object.
    <NonSerialized()>
    Private mStatus As SaveStatus

    ' List of external COM objects we can use as Business Objects.
    Private mObjects As IList(Of String) = New List(Of String)

    ' A list of all the defined database connection settings.
    Private mConnections As New List(Of clsDBConnectionSetting)

    ''' <summary>
    ''' True to compress process XML in the database. This flag is set up by reading
    ''' the database when a user logs in.
    ''' </summary>
    Public CompressProcessXML As Boolean

    ''' <summary>
    ''' A global option applying to all users; determines whether an edit summary
    ''' will be compulsory when saving a process in process studio. This flag is set
    ''' up by reading the database when a user logs in.
    ''' </summary>
    ''' <remarks>For some reason, it is also read from and written to the config
    ''' file within this class!?</remarks>
    Public EditSummariesAreCompulsory As Boolean

    ''' <summary>
    ''' The path used for archiving
    ''' </summary>
    Public ArchivePath As String

    ''' <summary>
    ''' True if the process engine (resource PC) should be started locally when
    ''' a user logs in interactively.
    ''' </summary>
    Public StartProcessEngine As Boolean

    ' The server configurations defined in this local config
    Private mServers As List(Of ServerConfig) = New List(Of ServerConfig)

    ''' <summary>
    ''' The maximum number of undo levels to hold when editing a process / object.
    ''' </summary>
    Public MaxUndoLevels As Integer = 20

    ' The current connection - stored machine-wide. Legacy only - never updated any
    ' more, but here to provide smooth transition to user-based connection storage.
    Private mMachineWideConnection As String

    ''' <summary>
    ''' The default command timeout to use for SQL Commands. By default, 30 seconds,
    ''' but this can be altered if lots of timeouts are being encountered.
    ''' </summary>
    Public SqlCommandTimeout As Integer = 30

    ''' <summary>
    ''' The default command timeout to use for long SQL Commands. By default, 600 seconds,
    ''' but this can be altered if lots of timeouts are being encountered.
    ''' </summary>
    Public SqlCommandTimeoutLong As Integer = 600

    ''' <summary>
    ''' The default command timeout to use for logging SQL Commands. By default, infinite.
    ''' </summary>
    Public SqlCommandTimeoutLog As Integer = 0

    ''' <summary>
    ''' The default command timeout (in minutes) to use for SQL Commands during installation of the
    ''' blueprism database. By default 15 minutes.
    ''' </summary>
    Public DatabaseInstallCommandTimeout As Integer = 15

    ''' <summary>
    ''' The default send timeout used when sending commands to a data pipeline process 
    ''' </summary>
    Public DataPipelineProcessCommandSendTimeout As Integer = 3

    ''' <summary>
    ''' Defines which encryption scheme will be used.
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectedConfigEncryptionScheme As ConfigEncryptionMethod

    ''' <summary>
    ''' Thumbprint for the certificate, this will be obtained from the user certificate store.
    ''' </summary>
    ''' <returns></returns>
    Public Property Thumbprint As String = String.Empty

#End Region

#Region " Constructors "

    Public Sub New(location As IConfigLocator)
        MyBase.New(location)
        SelectedConfigEncryptionScheme = ConfigEncryptionMethod.BuiltIn
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' List of defined server instances. Note that unlike collections, this is
    ''' editable, indeed, aside from loading from the backing file, amending the
    ''' collection returned is the only way to add / remove servers from the config
    ''' object.
    ''' </summary>
    Public ReadOnly Property Servers() As ICollection(Of ServerConfig)
        Get
            Return mServers
        End Get
    End Property

    ''' <summary>
    ''' A read-only view of the connection settings held in this config object
    ''' </summary>
    Public ReadOnly Property Connections() As IList(Of clsDBConnectionSetting)
        Get
            Return GetReadOnly.IList(mConnections)
        End Get
    End Property

    ''' <summary>
    ''' The current connection as specified from the legacy machine-wide location.
    ''' This is loaded from the machine config file and only used if there is no
    ''' current connection specified in the user config file.
    ''' It is never modified - any current connection modifications are performed in
    ''' the user config now, not in here. As such, this may well be null - if the
    ''' connection name specified in the file no longer exists in the configured
    ''' connections.
    ''' </summary>
    Public ReadOnly Property MachineWideConnection() As clsDBConnectionSetting
        Get
            Return GetConnection(mMachineWideConnection)
        End Get
    End Property

    Protected Overrides ReadOnly Property FileName As String
        Get
            Return "Automate.config"
        End Get
    End Property

    Protected Overrides ReadOnly Property ConfigFile As FileInfo
        Get
            Return New FileInfo(Path.Combine(mLocation.MachineConfigDirectory.FullName, FileName))
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Attempts to get the server config with the given name from this local config
    ''' object.
    ''' </summary>
    ''' <param name="name">The name of the required server config</param>
    ''' <returns>The server config with the given name or null if no such config was
    ''' found in this config object.</returns>
    Public Function GetServerConfig(ByVal name As String) As ServerConfig
        For Each cfg As ServerConfig In Servers
            If cfg.Name.Equals(name, StringComparison.OrdinalIgnoreCase) Then Return cfg
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Creates a new server configuration with a default (unique) name.
    ''' </summary>
    ''' <returns>A new server config with a name unique amongst current server config
    ''' names.</returns>
    Public Function NewServerConfig() As MachineConfig.ServerConfig
        Dim fmt As String = My.Resources.MachineConfig_Server0
        Dim i As Integer = 1

        Dim name As String = DefaultServerConfigName
        While GetServerConfig(name) IsNot Nothing
            i += 1
            name = String.Format(fmt, i)
        End While

        Dim cfg As New ServerConfig(Me)
        cfg.Name = name
        Return cfg

    End Function

    ''' <summary>
    ''' Resets the state of this machine config with some basic defaults
    ''' </summary>
    Friend Sub Reset()
        'Clear objects list...
        mObjects = New List(Of String)

        'Clear DB list...
        mConnections.Clear()

        'No server definitions by default.
        mServers = New List(Of ServerConfig)

        'Other defaults...
        EditSummariesAreCompulsory = True
        CompressProcessXML = True
        ArchivePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        StartProcessEngine = True
    End Sub

#Region " Load Methods "


    ''' <summary>
    ''' Loads the config options from the given text reader
    ''' </summary>
    ''' <param name="reader">The text reader from which the config options should be
    ''' read - if null, this will initialise the options to defaults and return.
    ''' </param>
    Protected Overrides Sub Load(ByVal reader As TextReader)
        'Set everything to its default state first. This is what will be used if
        'there is no config file present in the expected location. If we then
        'change something, it will get saved.
        Reset()

        'Attempt to load and parse the config...
        If reader IsNot Nothing Then
            Dim x As New ReadableXmlDocument()

            x.Load(reader)

            'Verify that the root element is as expected...
            If x.DocumentElement.Name <> "config" Then Throw New NoSuchElementException(
             "The config file should have a root element of 'config'")

            'Iterate through the child nodes...
            For Each n As XmlNode In x.DocumentElement

                ' Get the node as an element, if it is one
                Dim e As XmlElement = TryCast(n, XmlElement)
                ' Discard it and continue if it isn't (a comment is written out
                ' to the config file which would fail the cast into an element)
                If e Is Nothing Then Continue For

                ' All of the following need (at least) a 'first child' node - skip this
                ' element if it doesn't have one.
                If e.FirstChild Is Nothing Then Continue For

                Select Case e.Name

                    Case "editsummariesarecompulsory"
                        EditSummariesAreCompulsory = Boolean.Parse(e.FirstChild.Value)

                    Case "startprocessengine"
                        StartProcessEngine = Boolean.Parse(e.FirstChild.Value)

                    Case "archivepath"
                        ArchivePath = e.FirstChild.Value

                    Case "currentconnection"
                        ' Largely for back-compatibility - the user's last chosen
                        ' connection
                        mMachineWideConnection = e.FirstChild.Value

                    Case "sqlcommandtimeout"
                        Integer.TryParse(e.FirstChild.Value, SqlCommandTimeout)

                    Case "sqlcommandtimeoutlong"
                        Integer.TryParse(e.FirstChild.Value, SqlCommandTimeoutLong)

                    Case "sqlcommandtimeoutlog"
                        Integer.TryParse(e.FirstChild.Value, SqlCommandTimeoutLog)

                    Case "databaseinstallcommandtimeout"
                        Integer.TryParse(e.FirstChild.Value, DatabaseInstallCommandTimeout)

                    Case "datapipelinecommandsendtimeout"
                        Integer.TryParse(e.FirstChild.Value, DataPipelineProcessCommandSendTimeout)

                        '
                    Case "thumprint"
                        Thumbprint = e.FirstChild.Value

                        If Not String.IsNullOrEmpty(Thumbprint) Then
                            ValidateCertificate()
                        End If

                    Case "config-encryption-method"
                        Dim val As String = e.FirstChild.Value
                        If Not [Enum].TryParse(val, SelectedConfigEncryptionScheme) Then
                            Throw New InvalidValueException(
                                "Invalid server config encryption mode specification: {0}", val)
                        End If

                    Case "server"
                        Dim sv As New ServerConfig(Me)
                        mServers.Add(sv)
                        For Each se As XmlElement In e.ChildNodes
                            Select Case se.Name

                                Case "name"
                                    sv.Name = se.FirstChild.Value

                                Case "statuseventlogging"
                                    sv.StatusEventLogging = Boolean.Parse(se.FirstChild.Value)

                                Case "connection"
                                    sv.Connection = se.FirstChild.Value

                                Case "verbose"
                                    sv.Verbose = Boolean.Parse(se.FirstChild.Value)

                                Case "logtraffic"
                                    sv.LogTraffic = Boolean.Parse(se.FirstChild.Value)

                                Case "port"
                                    Dim val As String = se.FirstChild.Value
                                    If Not Integer.TryParse(val, sv.Port) Then _
                                     Throw New InvalidValueException(
                                      "Invalid server port specification: {0}", val)

                                Case "secure"
                                    'This setting was historically used to toggle
                                    'between.NET Remoting Secure and Insecure but 
                                    'is now incorporated in connectionmode. Secure 
                                    'may still exist in old config files. If so, 
                                    'then set the appropriate ConnectionMode setting.
                                    Dim val As String = se.FirstChild.Value
                                    Dim secure As Boolean
                                    If Not Boolean.TryParse(val, secure) Then _
                                     Throw New InvalidValueException(
                                      "Invalid server security specification: {0}", val)


                                    sv.ConnectionMode = If(secure, ServerConnection.Mode.DotNetRemotingSecure,
                                                           ServerConnection.Mode.DotNetRemotingInsecure)

                                Case "connectionmode"
                                    Dim val As String = se.FirstChild.Value
                                    If Not [Enum].TryParse(val, sv.ConnectionMode) Then
                                        Throw New InvalidValueException(
                                      "Invalid server connection mode specification: {0}", val)
                                    End If

                                Case "scheduler-disabled"
                                    sv.SchedulerDisabled = True

                                Case "datapipelineprocess-enabled"
                                    sv.DataPipelineProcessEnabled = True

                                Case "datapipelineprocess-port"
                                    sv.DataPipelineProcessCommandListenerPort = Integer.Parse(se.FirstChild.Value)

                                Case "bindto"
                                    sv.BindTo = se.FirstChild.Value

                                Case "keystore"
                                    sv.KeyStore = CType(se.GetAttribute("type"), ServerConfig.KeyStoreType)
                                    If sv.KeyStore = ServerConfig.KeyStoreType.ExternalFile Then
                                        sv.ExternalKeyStoreFolder = se.GetAttribute("folder")
                                    End If

                                    For Each ke As XmlElement In se.ChildNodes
                                        Dim scheme As New clsEncryptionScheme(ke.GetAttribute("name"))
                                        If sv.KeyStore = ServerConfig.KeyStoreType.ExternalFile Then
                                            ReadKeyFile(sv.ExternalKeyStoreFolder, scheme)
                                        Else
                                            scheme.Algorithm = CType(ke.GetAttribute("algorithm"), EncryptionAlgorithm)
                                            If ke.FirstChild IsNot Nothing Then
                                                Try
                                                    If ke.HasAttribute("enc") Then
                                                        'If there is an encrypted password, then
                                                        'convert it from the the xml to a safe string
                                                        For Each c As XmlElement In ke.ChildNodes
                                                            If c.Name = SafeString.XmlElementName Then _
                                                                scheme.Key = SafeString.FromXml(c) : Exit For
                                                        Next
                                                    Else
                                                        'Some passwords will stil be encrypted using the old
                                                        'method. Use the legacy decrypter to get the decrypted
                                                        'password.
                                                        scheme.Key =
                                                            LegacyCipherDecrypter.Instance.Decrypt(ke.InnerText)
                                                    End If
                                                Catch ex As Exception
                                                    Throw New InvalidPasswordException("Password is invalid")
                                                End Try
                                            End If
                                        End If
                                        sv.EncryptionKeys.Add(scheme.Name, scheme)
                                    Next

                                Case "credentialskey"
                                    'Retained for backwards compatility with old "Credentials Key"
                                    Dim scheme As clsEncryptionScheme = New clsEncryptionScheme("Credentials Key") With {
                                        .Algorithm = EncryptionAlgorithm.TripleDES,
                                        .Key = LegacyCipherDecrypter.Instance.Decrypt(se.FirstChild.Value)}
                                    sv.EncryptionKeys.Add(scheme.Name, scheme)
                                    sv.KeyStore = ServerConfig.KeyStoreType.Embedded
                                Case "datapipelineprocess-domain"
                                    If se.FirstChild IsNot Nothing Then
                                        sv.DataGatewayDomain = se.FirstChild.Value
                                    Else
                                        sv.DataGatewayDomain = String.Empty
                                    End If
                                Case "datapipelineprocess-user"
                                    If Not se.FirstChild Is Nothing Then
                                        sv.DataGatewayUser = se.FirstChild.Value
                                    Else
                                        sv.DataGatewayUser = String.Empty
                                    End If
                                Case "datapipelineprocess-password"
                                    If Not se.FirstChild Is Nothing Then
                                        Try
                                            If se.HasAttribute("enc") Then
                                                'If there is an encrypted password, then convert it from the the xml to a safe string. 
                                                Dim password = CType(se.FirstChild, XmlElement)
                                                If password.Name = SafeString.XmlElementName Then
                                                    sv.DataGatewayPassword = SafeString.FromXml(password)
                                                End If
                                            End If
                                        Catch ex As Exception
                                            Throw New InvalidPasswordException("Password is invalid")
                                        End Try
                                    End If
                                Case "datapipelineprocess-specificuser"
                                    sv.DataGatewaySpecificUser = True
                                Case "ordered"
                                    sv.Ordered = Boolean.Parse(se.FirstChild.Value)
                                Case "maxtransferwindowsize"
                                    sv.MaxTransferWindowSize = Integer.Parse(se.FirstChild.Value)
                                Case "maxpendingchannels"
                                    sv.MaxPendingChannels = Integer.Parse(se.FirstChild.Value)
                                Case "authenticationServerBrokerConfig"
                                    sv.AuthenticationServerBrokerConfig = ParseAuthenticationServerBrokerConfig(se)
                                Case ConnectionConfig.ElementName
                                    Try
                                        sv.CallbackConnectionConfig = ConfigLoader.LoadXML(se)
                                    Catch ex As InvalidProgramException
                                        Log.Error(ex)
                                    End Try
                            End Select
                        Next

                    Case "connections"
                        For Each e2 As XmlElement In e.ChildNodes
                            If e2.Name = "connection" Then
                                Dim con As New clsDBConnectionSetting("")
                                Dim t = ConnectionType.Direct
                                For Each e3 As XmlElement In e2.ChildNodes
                                    Select Case e3.Name
                                        Case "port"
                                            If e3.FirstChild IsNot Nothing Then con.Port = Integer.Parse(e3.FirstChild.Value)
                                        Case "agport"
                                            If e3.FirstChild IsNot Nothing Then con.AGPort = Integer.Parse(e3.FirstChild.Value)
                                        Case "callbackport"
                                            If e3.FirstChild IsNot Nothing Then con.CallbackPort = Integer.Parse(e3.FirstChild.Value)
                                        Case "secure"
                                            'This setting was historically used to toggle
                                            'between.NET Remoting Secure and Insecure but 
                                            'is now incorporated in connectionmode. Secure 
                                            'may still exist in old config files. If so, 
                                            'then set the appropriate ConnectionMode setting
                                            If e3.FirstChild IsNot Nothing Then

                                                Dim val = e3.FirstChild.Value
                                                Dim secure As Boolean
                                                If Not Boolean.TryParse(e3.FirstChild.Value, secure) Then _
                                                 Throw New InvalidValueException(
                                                  "Invalid connection mode specification: {0}", val)

                                                If secure Then
                                                    con.ConnectionMode =
                                                        ServerConnection.Mode.DotNetRemotingSecure
                                                Else
                                                    con.ConnectionMode =
                                                        ServerConnection.Mode.DotNetRemotingInsecure
                                                End If
                                            End If

                                        Case "connectionmode"
                                            If e3.FirstChild IsNot Nothing Then
                                                Dim val As String = e3.FirstChild.Value

                                                If Not [Enum].TryParse(val, con.ConnectionMode) Then
                                                    Throw New InvalidValueException(
                                                  "Invalid connection mode specification: {0}", val)
                                                End If
                                            End If

                                        Case "multisubnetfailover"
                                            If e3.FirstChild IsNot Nothing Then con.MultiSubnetFailover = Boolean.Parse(e3.FirstChild.Value)
                                        Case "name"
                                            If e3.FirstChild IsNot Nothing Then con.ConnectionName = e3.FirstChild.Value
                                        Case "server"
                                            If e3.FirstChild IsNot Nothing Then con.DBServer = e3.FirstChild.Value
                                        Case "user"
                                            If e3.FirstChild IsNot Nothing Then con.DBUserName = e3.FirstChild.Value
                                        Case "dbname"
                                            If e3.FirstChild IsNot Nothing Then con.DatabaseName = e3.FirstChild.Value
                                        Case "databasefilepath"
                                            If e3.FirstChild IsNot Nothing Then con.DatabaseFilePath = e3.FirstChild.Value
                                        Case "password"
                                            If e3.FirstChild IsNot Nothing Then
                                                Try
                                                    If e3.HasAttribute("enc") Then
                                                        'If there is an encrypted password, then
                                                        'convert it from the the xml to a safe string
                                                        For Each c As XmlElement In e3.ChildNodes
                                                            If c.Name = SafeString.XmlElementName AndAlso Not String.IsNullOrEmpty(c.OuterXml) Then _
                                                                con.DBUserPassword = SafeString.FromXml(c) : Exit For
                                                        Next
                                                    Else
                                                        'Some passwords will stil be encrypted using the old
                                                        'method. Use the legacy decrypter to get the decrypted
                                                        'password.
                                                        con.DBUserPassword =
                                                            LegacyCipherDecrypter.Instance.Decrypt(e3.InnerText)
                                                    End If
                                                Catch ex As Exception
                                                    Throw New InvalidPasswordException("Password is invalid")
                                                End Try
                                            End If
                                        Case "extraparams"
                                            If e3.FirstChild IsNot Nothing Then con.ExtraParams = e3.FirstChild.Value
                                        Case "windowsauth"
                                            con.WindowsAuth = True
                                        Case "bpserver"
                                            t = ConnectionType.BPServer
                                        Case "availability"
                                            t = ConnectionType.Availability
                                        Case "customconnectionstring"
                                            t = ConnectionType.CustomConnectionString
                                        Case "connectionstring"
                                            If e3.FirstChild IsNot Nothing Then con.CustomConnectionString = e3.FirstChild.Value
                                            Try
                                                Dim builder = New SqlConnectionStringBuilder(con.CustomConnectionString)
                                                con.DatabaseName = If(builder?.InitialCatalog, "")
                                            Catch ex As Exception
                                                ' Ignore this for now, the invalid string will be reported correctly to the user
                                            End Try
                                        Case "ordered"
                                            con.Ordered = Boolean.Parse(e3.FirstChild.Value)
                                        Case "maxtransferwindowsize"
                                            con.MaxTransferWindowSize = Integer.Parse(e3.FirstChild.Value)
                                        Case "maxpendingchannels"
                                            con.MaxPendingChannels = Integer.Parse(e3.FirstChild.Value)

                                        Case "rabbitmqconfiguration"
                                            Dim hostUrl As String = String.Empty
                                            Dim username As String = String.Empty
                                            Dim password As SafeString = Nothing

                                            For Each rabbitMqSetting As XmlElement In e3.ChildNodes
                                                Select Case rabbitMqSetting.Name
                                                    Case "hosturl"
                                                        hostUrl = rabbitMqSetting.FirstChild.Value
                                                    Case "username"
                                                        username = rabbitMqSetting.FirstChild.Value
                                                    Case "password"
                                                        Try
                                                            Dim passwordElement = CType(rabbitMqSetting.FirstChild, XmlElement)
                                                            If passwordElement.Name = SafeString.XmlElementName Then
                                                                password = SafeString.FromXml(passwordElement)
                                                            End If
                                                        Catch ex As Exception
                                                            Log.Warn("RabbitMQ password is invalid.")
                                                        End Try
                                                End Select
                                            Next

                                            Dim configIsComplete = Not String.IsNullOrEmpty(hostUrl) AndAlso Not String.IsNullOrEmpty(username) AndAlso password IsNot Nothing
                                            If configIsComplete Then
                                                con.RabbitMqConfiguration = New RabbitMqConfiguration(hostUrl, username, password)
                                            Else
                                                Log.Warn("RabbitMQ configuration is incomplete.")
                                                con.RabbitMqConfiguration = Nothing
                                            End If

                                    End Select
                                Next
                                con.ConnectionType = t
                                AddConnection(con)
                            End If
                        Next

                    Case "businessobjects"
                        For Each e2 As XmlElement In e.ChildNodes
                            If e2.Name = "object" Then
                                AddObject(e2.FirstChild.Value)
                            End If
                        Next

                End Select

            Next

        End If

    End Sub

    Private Function ParseAuthenticationServerBrokerConfig(node As XmlNode) As AuthenticationServerBrokerConfig

        Try
            Dim brokerAddress = String.Empty
            Dim brokerUsername = String.Empty
            Dim brokerPassword As SafeString = Nothing
            Dim environmentIdentifier = String.Empty

            For Each element As XmlElement In node.ChildNodes
                Select Case element.Name
                    Case "brokerAddress"
                        brokerAddress = element.FirstChild.Value
                    Case "brokerUsername"
                        brokerUsername = element.FirstChild.Value
                    Case "brokerPassword"
                        Try
                            Dim passwordElement = CType(element.FirstChild, XmlElement)
                            If passwordElement.Name = SafeString.XmlElementName Then
                                brokerPassword = SafeString.FromXml(passwordElement)
                            End If
                        Catch ex As Exception
                            Log.Warn("Authentication Server broker password is invalid.")
                        End Try
                    Case "environmentIdentifier"
                        environmentIdentifier = element.FirstChild.Value
                End Select
            Next

            Return New AuthenticationServerBrokerConfig(brokerAddress, brokerUsername, brokerPassword, environmentIdentifier)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Attempts to read the encryption key from a file in the specified folder.
    ''' </summary>
    ''' <param name="folder">The external folder</param>
    ''' <param name="scheme">The encryption scheme</param>
    Private Sub ReadKeyFile(ByVal folder As String, ByVal scheme As clsEncryptionScheme)
        Dim file = Path.Combine(folder, scheme.ExternalFileName)
        Try
            scheme.FromExternalFileFormat(FileIO.FileSystem.ReadAllText(file))
        Catch ex As Exception
            'Ignore any errors here, if the file is not present/not readable then
            'assume that the user cannot edit/use the key
        End Try
    End Sub

#End Region

#Region " Save Methods "

    ''' <summary>
    ''' Save the current configuration to the config file, raising exceptions on any
    ''' errors
    ''' </summary>
    ''' <exception cref="UnauthorizedAccessException">If the writing of the config
    ''' could not be performed due to auth constraints.</exception>
    ''' <exception cref="ConfigSaveException">If the config writing was delegated to
    ''' automateconfig and the process returned a non-zero response code.</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to
    ''' save the config file.</exception>
    Public Overrides Sub Save()

        Dim currProcId As Integer = Process.GetCurrentProcess().Id
        Try
            ' If we have a config manager set, just use that.
            If mManager IsNot Nothing Then mManager.Save(Me) : Return
        Catch e As Exception
            Log.Error(e, "{0}: Error saving local config", currProcId)
            Debug.WriteLine(
             "Error saving local config via config manager: " & e.ToString())
        End Try

        If mStatus = SaveStatus.UserCancelled OrElse mStatus = SaveStatus.Failed Then
            ' The user didn't want to elevate or we couldn't write for some other reason
            ' stop prompting for elevation - it's very annoying
            Return
        End If

        ' So we don't have a (working) manager...

        ' Just save it if we can - if we have write access to the
        ' directory anyway, again, just save. No point in creating a child process
        ' if we don't need to - they're so 'spensive in windows.
        Try
            ' This is where we differ in how we deal with options depending on whether
            ' the current user has write access to the config file or not.
            If mStatus = SaveStatus.SavedDirect OrElse mStatus = SaveStatus.Untried Then
                MyBase.Save()
                Log.Info(
                 "{0}: Saved to file: {1}", currProcId, ConfigFile.FullName)
                mStatus = SaveStatus.SavedDirect
                Return
            End If

        Catch ex As Exception
            Log.Error(ex, "{0}: Error saving local config", currProcId)
        End Try


        ' If we are elevated then we can do no more to try and get access to the output file.
        ' Also, if we're already within automateconfig, there's nothing further to try.
        ' Otherwise, we can try calling automateconfig (thus requesting elevated privileges)
        If Not UacHelper.IsProcessElevated AndAlso
         Not Assembly.GetEntryAssembly().FullName.StartsWith("AutomateConfig") Then

            ' We're not elevated and we don't have write access.
            ' Go ahead and call automateconfig
            Log.Info("Process is not elevated - writing local config via automateconfig")
            Try
                ' get our proc id to identify this process in automateconfig
                Dim procId As Integer = Process.GetCurrentProcess().Id

                Dim proc As Process = Process.Start(
                 ApplicationProperties.AutomateConfigPath, "daemon " & procId)
                ' Wait to see if the process has started...
                Dim exited As Boolean = proc.WaitForExit(500)
                If exited Then
                    Log.Warn(
                     "automateprocess exited with error code: {0}", proc.ExitCode)
                    mStatus = SaveStatus.Failed
                End If

                If Not WaitForNamedPipe(procId, TimeSpan.FromSeconds(5)) Then _
                 Throw New OperationFailedException(
                         My.Resources.MachineConfig_FailedToFindTheAutomateConfigNamedPipe0, procId)

                ' Get the manager from IPC - if any errors occur, the manager
                ' instance is set to null and processing continues - primarily so we
                ' can have a single meaningful message to inform the user in all 'not
                ' saving config' situations.
                ChannelServices.RegisterChannel(
                 New IpcChannel("automateconfig-client-" & procId), True)
                mManager = DirectCast(Activator.GetObject(GetType(ConfigManager),
                 "ipc://automateconfig-server-" & procId & "/ConfigManager"), ConfigManager)

                Const MaxAttempts As Integer = 10
                Dim attempt As Integer = 1
                While True
                    Try
                        ' If we don't have a a manager, or it's alive, exit the retry loop
                        If mManager Is Nothing OrElse mManager.IsAlive Then Exit While
                    Catch re As RemotingException
                        ' Keep going for 'MaxAttempts' tries, then give up
                        Log.Warn(re,
                         "Remoting Exception calling ConfigManager (attempt {0})",
                         attempt)
                        If attempt >= MaxAttempts Then mManager = Nothing : Exit While
                        attempt += 1
                        Thread.Sleep(100)
                    End Try
                End While

            Catch w32e As Win32Exception
                Log.Error(w32e, "PID-{0}: Failed to run automateconfig", currProcId)

                ' The most common cause of this exception is the user cancelling...
                mManager = Nothing
                mStatus = SaveStatus.UserCancelled

            End Try

            ' Crunch point - the first time that the manager is used.
            ' Use it, and, if successful, indicate that we are saving via pipe so
            ' we can skip the 'test the file' check earlier in the save method
            ' the next time the user saves the file
            Try
                If mManager IsNot Nothing Then
                    mManager.Save(Me)
                    Log.Info(
                     "PID-{0}: Saved local config via named pipe", currProcId)
                    mStatus = SaveStatus.SavedViaPipe
                    Return
                End If
            Catch ex As Exception
                Log.Error(ex, "{0}: Error saving local config", currProcId)
            End Try

        End If

        ' If we get here, then we just couldn't write to the file - it must be
        ' because we didn't have the rights to do so and couldn't (or the user
        ' refused to) elevate our privileges.
        mStatus = SaveStatus.Failed
        Log.Warn(
         "PID-{0}: Failed to save local config: Unauthorized", currProcId)

        Throw New UnauthorizedAccessException(
         My.Resources.MachineConfig_TheBluePrismConfigurationFileCouldNotBeSavedDueToNonAdministrativeAccessRightsT)

    End Sub

    ''' <summary>
    ''' Save the current configuration to the given text writer
    ''' </summary>
    ''' <exception cref="Exception">If any errors occur while attempting to build the
    ''' XML from this config, or write to the provided text writer.</exception>
    Protected Overrides Sub Save(ByVal writer As TextWriter)

        Dim encryptor As IObfuscator = GetEncryptor()
        Dim doc As New XmlDocument()
        Dim root As XmlNode = doc.AppendChild(doc.CreateElement("config"))

        BPUtil.AppendTextElement(root,
         "editsummariesarecompulsory", EditSummariesAreCompulsory)
        BPUtil.AppendTextElement(root, "startprocessengine", StartProcessEngine)
        BPUtil.AppendTextElement(root, "currentconnection", mMachineWideConnection)
        BPUtil.AppendTextElement(root, "sqlcommandtimeout", SqlCommandTimeout)
        BPUtil.AppendTextElement(root, "sqlcommandtimeoutlong", SqlCommandTimeoutLong)
        BPUtil.AppendTextElement(root, "sqlcommandtimeoutlog", SqlCommandTimeoutLog)
        BPUtil.AppendTextElement(root, "databaseinstallcommandtimeout", DatabaseInstallCommandTimeout)
        BPUtil.AppendTextElement(root, "datapipelinecommandsendtimeout", DataPipelineProcessCommandSendTimeout)
        BPUtil.AppendTextElement(root, "archivepath", ArchivePath)

        If Not String.IsNullOrWhiteSpace(Thumbprint) Then _
            BPUtil.AppendTextElement(root, "thumprint", Thumbprint)
        BPUtil.AppendTextElement(root, "config-encryption-method", CInt(SelectedConfigEncryptionScheme))

        For Each sv As ServerConfig In mServers

            Dim svEl As XmlNode = root.AppendChild(doc.CreateElement("server"))

            If sv.Name IsNot Nothing Then _
             BPUtil.AppendTextElement(svEl, "name", sv.Name)

            If sv.Connection IsNot Nothing Then _
             BPUtil.AppendTextElement(svEl, "connection", sv.Connection)

            If sv.Port <> 0 Then _
             BPUtil.AppendTextElement(svEl, "port", sv.Port)

            If sv.Ordered.HasValue Then BPUtil.AppendTextElement(svEl, "ordered", sv.Ordered)
            If sv.MaxPendingChannels.HasValue Then BPUtil.AppendTextElement(svEl, "maxpendingchannels", sv.MaxPendingChannels)
            If sv.MaxTransferWindowSize.HasValue Then BPUtil.AppendTextElement(svEl, "maxtransferwindowsize", sv.MaxTransferWindowSize)

            BPUtil.AppendTextElement(svEl, "connectionmode", CInt(sv.ConnectionMode))

            If sv.Verbose Then BPUtil.AppendTextElement(svEl, "verbose", True)

            If sv.LogTraffic Then BPUtil.AppendTextElement(svEl, "logtraffic", True)

            If sv.SchedulerDisabled Then _
             svEl.AppendChild(doc.CreateElement("scheduler-disabled"))

            If sv.DataPipelineProcessEnabled Then _
                svEl.AppendChild(doc.CreateElement("datapipelineprocess-enabled"))

            If sv.DataGatewaySpecificUser Then
                svEl.AppendChild(doc.CreateElement("datapipelineprocess-specificuser"))
                svEl.AppendChild(BPUtil.AppendTextElement(svEl, "datapipelineprocess-domain", sv.DataGatewayDomain))
                svEl.AppendChild(BPUtil.AppendTextElement(svEl, "datapipelineprocess-user", sv.DataGatewayUser))

                Dim password As XmlElement = doc.CreateElement("datapipelineprocess-password")
                svEl.AppendChild(password)
                'Indicate that the password has been encrypted
                password.SetAttribute("enc", "true")
                Dim cipheredPassword As New SafeString(sv.DataGatewayPassword, encryptor)
                password.AppendChild(cipheredPassword.ToXml(doc))
            End If

            If sv.DataPipelineProcessCommandListenerPort <> 0 Then _
                BPUtil.AppendTextElement(svEl, "datapipelineprocess-port", sv.DataPipelineProcessCommandListenerPort)

            If sv.BindTo IsNot Nothing Then _
             BPUtil.AppendTextElement(svEl, "bindto", sv.BindTo)

            Dim ksEl As XmlElement = doc.CreateElement("keystore")
            ksEl.SetAttribute("type", CStr(sv.KeyStore))
            If sv.KeyStore = ServerConfig.KeyStoreType.ExternalFile Then _
             ksEl.SetAttribute("folder", sv.ExternalKeyStoreFolder)

            For Each scheme As clsEncryptionScheme In sv.EncryptionKeys.Values
                Dim keyEl As XmlElement = doc.CreateElement("key")
                keyEl.SetAttribute("name", scheme.Name)
                If sv.KeyStore = ServerConfig.KeyStoreType.Embedded Then
                    keyEl.SetAttribute("algorithm", CStr(scheme.Algorithm))
                    keyEl.SetAttribute("enc", "true")
                    Dim cipheredPassword As New SafeString(scheme.Key, encryptor)
                    keyEl.AppendChild(cipheredPassword.ToXml(doc))
                End If
                ksEl.AppendChild(keyEl)
            Next
            svEl.AppendChild(ksEl)

            If sv.AuthenticationServerBrokerConfig IsNot Nothing Then
                AppendAuthenticationServerBrokerConfig(doc, sv, svEl, encryptor)
            End If

            BPUtil.AppendTextElement(svEl,
             "statuseventlogging", sv.StatusEventLogging)

            If sv.CallbackConnectionConfig IsNot Nothing Then
                Try
                    svEl.AppendChild(ConfigLoader.SaveXML(sv.CallbackConnectionConfig, doc))
                Catch ex As InvalidProgramException
                    Log.Error(ex)
                End Try
            End If
        Next

        Dim connsEl As XmlElement = doc.CreateElement("connections")
        root.AppendChild(connsEl)
        For Each con As clsDBConnectionSetting In mConnections
            Dim connEl As XmlElement = doc.CreateElement("connection")
            connsEl.AppendChild(connEl)

            If con.ConnectionType = ConnectionType.CustomConnectionString Then
                connEl.AppendChild(doc.CreateElement("customconnectionstring"))
                BPUtil.AppendTextElement(connEl, "connectionstring", con.CustomConnectionString)
            ElseIf con.ConnectionType = ConnectionType.BPServer Then
                connEl.AppendChild(doc.CreateElement("bpserver"))
                BPUtil.AppendTextElement(connEl, "port", con.Port)
                BPUtil.AppendTextElement(connEl, "callbackport", con.CallbackPort)
                BPUtil.AppendTextElement(connEl, "connectionmode", CInt(con.ConnectionMode))
                If con.Ordered.HasValue Then BPUtil.AppendTextElement(connEl, "ordered", con.Ordered)
                If con.MaxPendingChannels.HasValue Then BPUtil.AppendTextElement(connEl, "maxpendingchannels", con.MaxPendingChannels)
                If con.MaxTransferWindowSize.HasValue Then BPUtil.AppendTextElement(connEl, "maxtransferwindowsize", con.MaxTransferWindowSize)
            ElseIf con.ConnectionType = ConnectionType.Availability Then
                connEl.AppendChild(doc.CreateElement("availability"))
                BPUtil.AppendTextElement(connEl, "agport", con.AGPort)
                BPUtil.AppendTextElement(connEl, "multisubnetfailover", con.MultiSubnetFailover)
            End If

            BPUtil.AppendTextElement(connEl, "name", con.ConnectionName)

            If con.ConnectionType = ConnectionType.CustomConnectionString Then Continue For

            BPUtil.AppendTextElement(connEl, "server", con.DBServer)

            If con.ConnectionType <> ConnectionType.BPServer Then
                BPUtil.AppendTextElement(connEl, "dbname", con.DatabaseName)
                BPUtil.AppendTextElement(connEl, "databasefilepath", con.DatabaseFilePath)
                If con.WindowsAuth Then
                    connEl.AppendChild(doc.CreateElement("windowsauth"))
                Else
                    BPUtil.AppendTextElement(connEl, "user", con.DBUserName)
                    Dim password As XmlElement = doc.CreateElement("password")
                    connEl.AppendChild(password)
                    'Indicate that the password has been encrypted
                    password.SetAttribute("enc", "true")
                    Dim cipheredPassword As New SafeString(con.DBUserPassword, encryptor)
                    password.AppendChild(cipheredPassword.ToXml(doc))
                End If
            End If
            BPUtil.AppendTextElement(connEl, "extraparams", con.ExtraParams)

            If con.RabbitMqConfiguration IsNot Nothing Then
                Dim rabbitMQConfig As XmlElement = doc.CreateElement("rabbitmqconfiguration")
                connEl.AppendChild(rabbitMQConfig)

                BPUtil.AppendTextElement(rabbitMQConfig, "hosturl", con.RabbitMqConfiguration.HostUrl)
                BPUtil.AppendTextElement(rabbitMQConfig, "username", con.RabbitMqConfiguration.Username)

                Dim password As XmlElement = doc.CreateElement("password")
                rabbitMQConfig.AppendChild(password)
                Dim cipheredPassword As New SafeString(con.RabbitMqConfiguration.Password, encryptor)
                password.AppendChild(cipheredPassword.ToXml(doc))
            End If
        Next

        Dim bos As XmlNode = root.AppendChild(doc.CreateElement("businessobjects"))
        For Each objName As String In mObjects
            BPUtil.AppendTextElement(bos, "object", objName)
        Next

        doc.Save(writer)

        'Write any external key files
        For Each sv As ServerConfig In mServers
            For Each scheme As clsEncryptionScheme In sv.EncryptionKeys.Values
                If sv.KeyStore = ServerConfig.KeyStoreType.ExternalFile Then
                    If scheme.HasValidKey Then _
                     WriteKeyFile(sv.ExternalKeyStoreFolder, scheme)
                End If
            Next
        Next
    End Sub

    Private Sub AppendAuthenticationServerBrokerConfig(doc As XmlDocument, serverConfig As ServerConfig, parentNode As XmlNode, encryptor As IObfuscator)

        Dim authenticationServerBrokerConfigNode = doc.CreateElement("authenticationServerBrokerConfig")
        parentNode.AppendChild(authenticationServerBrokerConfigNode)

        BPUtil.AppendTextElement(authenticationServerBrokerConfigNode, "brokerAddress", serverConfig.AuthenticationServerBrokerConfig.BrokerAddress)

        BPUtil.AppendTextElement(authenticationServerBrokerConfigNode, "brokerUsername", serverConfig.AuthenticationServerBrokerConfig.BrokerUsername)

        Dim brokerPassword = doc.CreateElement("brokerPassword")
        authenticationServerBrokerConfigNode.AppendChild(brokerPassword)
        brokerPassword.SetAttribute("enc", "true")
        Dim cipheredPassword As New SafeString(serverConfig.AuthenticationServerBrokerConfig.BrokerPassword, encryptor)
        brokerPassword.AppendChild(cipheredPassword.ToXml(doc))

        BPUtil.AppendTextElement(authenticationServerBrokerConfigNode, "environmentIdentifier", serverConfig.AuthenticationServerBrokerConfig.EnvironmentIdentifier)
    End Sub

    ''' <summary>
    ''' Factory function to return encryption method based on selected encryption scheme.
    ''' </summary>
    ''' <returns></returns>
    Private Function GetEncryptor() As IObfuscator
        Select Case SelectedConfigEncryptionScheme
            Case ConfigEncryptionMethod.BuiltIn
                Return New EncryptingObfuscator()
            Case Else
                Return New RsaCertificateEncryption(Thumbprint)
        End Select
    End Function

    ''' <summary>
    ''' Validate the cert is accessible and valid.
    ''' </summary>
    Private Sub ValidateCertificate()
        Dim certificateStoreService = New CertificateStoreService
        Using certificate As X509Certificate2 = certificateStoreService.GetCertificate(Thumbprint)
            certificateStoreService.ValidateCert(certificate)
        End Using
    End Sub

    ''' <summary>
    ''' Attempts to write the encryption key associated with the passed scheme to the
    ''' external folder.
    ''' </summary>
    ''' <param name="folder">The external folder</param>
    ''' <param name="scheme">The scheme</param>
    Private Sub WriteKeyFile(ByVal folder As String, ByVal scheme As clsEncryptionScheme)
        Dim file As String = Path.Combine(folder, scheme.ExternalFileName)
        FileIO.FileSystem.WriteAllText(file, scheme.ToExternalFileFormat(), False)
    End Sub

#End Region

    ''' <summary>
    ''' Waits a predefined amount of time for a named pipe to appear in the system.
    ''' </summary>
    ''' <param name="procId">The ID of the process whose named pipe we should be
    ''' checking for.</param>
    ''' <param name="timeout">The amount of time to wait</param>
    ''' <returns>True if </returns>
    Private Function WaitForNamedPipe(
     ByVal procId As Integer, ByVal timeout As TimeSpan) As Boolean

        Dim pipePath As String = "\\.\pipe\automateconfig-server-" & procId

        ' Wait for the named pipe to arrive
        Dim found As Boolean = False
        Dim waiter As Stopwatch = Stopwatch.StartNew()
        While Not found AndAlso waiter.Elapsed < timeout
            Thread.Sleep(100)
            For Each path As String In Directory.GetFiles("\\.\pipe\")
                If path.Contains(pipePath) Then
                    'EventLogger.SafeGetLogger().Info( _
                    ' "Found pipe path '{0}' in path '{1}'", pipePath, path)
                    Return True
                End If
            Next
        End While

        ' If we get here, we haven't found it in time
        Return False

    End Function

    ''' <summary>
    ''' Update database settings as passed from the command-line. The connection
    ''' with the given name is either created, or updated if it already exists.
    ''' This set of settings is also set to be the current one.
    ''' This overload is for a direct connection.
    ''' </summary>
    ''' <param name="name">The connection name</param>
    ''' <param name="server">The database server name</param>
    ''' <param name="dbname">The database name</param>
    ''' <param name="username">The database username, if applicable</param>
    ''' <param name="password">The database password, if applicable</param>
    ''' <param name="winauth">True for Windows Authentication, in which case the
    ''' database username and password are irrelevant. Otherwise, SQL Server
    ''' Authentication is used.</param>
    Public Sub UpdateDatabaseSettings(ByVal name As String, ByVal server As String,
     ByVal dbname As String, ByVal username As String, ByVal password As SafeString,
     ByVal winauth As Boolean)
        'See if we can find an existing connection with this name
        Dim con As clsDBConnectionSetting = GetConnection(name)

        'If not, create a new one...
        If con Is Nothing Then
            con = New clsDBConnectionSetting(
                name, server, dbname, username, password, winauth)
            mConnections.Add(con)

        Else
            con.ConnectionType = ConnectionType.Direct
            con.DBServer = server
            con.DatabaseName = dbname
            con.DBUserName = username
            con.DBUserPassword = password
            con.WindowsAuth = winauth

        End If

        ' Save the changes
        Save()
    End Sub

    ''' <summary>
    ''' Update database settings as passed from the command-line. The connection
    ''' with the given name is either created, or updated if it already exists.
    ''' This set of settings is also set to be the current one.
    ''' This overload is for an Availability Group connection.
    ''' </summary>
    ''' <param name="name">The connection name</param>
    ''' <param name="server">The database server name</param>
    ''' <param name="dbname">The database name</param>
    ''' <param name="username">The database username, if applicable</param>
    ''' <param name="password">The database password, if applicable</param>
    ''' <param name="winauth">True for Windows Authentication, in which case the
    ''' database username and password are irrelevant. Otherwise, SQL Server
    ''' Authentication is used.</param>
    ''' <param name="port">The AG listener port</param>
    ''' <param name="multiSubnetFailover">True for Multi Subnet Failover.</param>
    Public Sub UpdateDatabaseSettings(ByVal name As String, ByVal server As String,
     ByVal dbname As String, ByVal username As String, ByVal password As SafeString,
     ByVal winauth As Boolean, ByVal port As Integer, ByVal multiSubnetFailover As Boolean)
        'See if we can find an existing connection with the same name as the database
        'name...
        Dim con As clsDBConnectionSetting = GetConnection(name)

        'If not, create a new one...
        If con Is Nothing Then
            con = New clsDBConnectionSetting(name, server, dbname, username,
                                             password, winauth,
                                             port, multiSubnetFailover) _
                                         With {.ExtraParams = ""}
            mConnections.Add(con)

        Else
            con.ConnectionType = ConnectionType.Availability
            con.DBServer = server
            con.DatabaseName = dbname
            con.DBUserName = username
            con.DBUserPassword = password
            con.WindowsAuth = winauth
            con.MultiSubnetFailover = multiSubnetFailover
            con.AGPort = port

        End If

        ' Save the changes
        Save()

    End Sub


    ''' <summary>
    ''' Update database settings as passed from the command-line. The connection
    ''' with the given name is either created, or updated if it already exists.
    ''' This set of settings is also set to be the current one.
    ''' This overload is for a Blue Prism Server connection.
    ''' </summary>
    ''' <param name="name">The connection name</param>
    ''' <param name="address">The address of the server</param>
    ''' <param name="port">The port the server runs on</param>
    ''' <param name="connectionMode">The connection mode for the server </param>
    ''' <param name="callbackPort">The callback port for the connection to a BP
    ''' server - zero or negative values have no effect in this method.</param>
    ''' <exception cref="Exception">If any errors occur while </exception>
    Public Sub UpdateDatabaseSettings(
     ByVal name As String, ByVal address As String, ByVal port As Integer,
     ByVal connectionMode As ServerConnection.Mode, ByVal callbackPort As Integer)

        If callbackPort < 0 Then callbackPort = 0

        'See if we can find an existing connection with the same name as the database
        'name...
        Dim con As clsDBConnectionSetting = GetConnection(name)

        'If not, create a new one...
        If con Is Nothing Then
            con = New clsDBConnectionSetting(name, address, port,
                                             connectionMode, callbackPort)
            mConnections.Add(con)
        Else
            con.ConnectionType = ConnectionType.BPServer
            con.DBServer = address
            con.Port = port
            con.ConnectionMode = connectionMode
            ' Don't overwrite the one that's there if we don't have one to overwrite it with
            If callbackPort <> 0 Then con.CallbackPort = callbackPort
        End If

        ' Save the changes
        Save()

    End Sub

    ''' <summary>
    ''' Updates the server settings for the given server name or creates a new server
    ''' settings with the given name.
    ''' </summary>
    ''' <param name="name">The name of the server</param>
    ''' <param name="connName">The connection name to use</param>
    ''' <param name="port">The port on which to listen</param>
    ''' <param name="connectionMode">The mode used for Blue Prism Server connections
    ''' </param>
    ''' <param name="scheme">The encryption scheme to add (Nothing if not adding a
    ''' scheme)</param>
    Public Sub UpdateServerSettings(name As String, connName As String, port As Integer,
     connectionMode As ServerConnection.Mode, scheme As clsEncryptionScheme, ordered As Boolean)

        Dim sv As ServerConfig = GetServerConfig(name)
        If sv IsNot Nothing Then
            sv.Connection = connName
            sv.Port = port
            sv.ConnectionMode = connectionMode
            sv.Ordered = ordered

            If scheme IsNot Nothing Then _
                sv.EncryptionKeys.Add(scheme.Name, scheme)
        Else
            sv = New ServerConfig(Me) With {
                .Name = name,
                .Port = port,
                .ConnectionMode = connectionMode,
                .Connection = connName,
                .StatusEventLogging = True,
                .Ordered = ordered
            }
            sv.EncryptionKeys.Add(scheme.Name, scheme)
            Options.Instance.Servers.Add(sv)

        End If
        Save()

    End Sub

    Public Sub UpdateASCRServerSettings(name As String, config As ConnectionConfig)

        Dim sv As ServerConfig = GetServerConfig(name)
        If sv IsNot Nothing Then
            sv.CallbackConnectionConfig = config
        Else
            Throw New ConfigNotFoundException(String.Format(My.Resources.MachineConfig_FailedToFindTheAutomateConfigNamed0, name))
        End If
        Save()

    End Sub

    ''' <summary>
    ''' Get the number of COM Business Objects currently installed.
    ''' </summary>
    ''' <returns>The number of objects.</returns>
    Public Function GetNumObjects() As Integer
        Return mObjects.Count
    End Function

    ''' <summary>
    ''' Instantiates and returns a clsExternalObjectsInstance, populated with
    ''' relevant information about the current set of available Business Objects.
    ''' This includes:
    '''   1. COM Business Objects that are installed on the local machine
    '''   2. Visual Business Objects from the database, as defined in Object Studio
    '''   3. Web Services from the database, as set up in System Manager
    ''' </summary>
    ''' <returns>A clsExternalObjects instance</returns>
    Public Function GetExternalObjectsInfo(reload As Boolean) As IGroupObjectDetails
        Dim groupStore = DependencyResolver.Resolve(Of IGroupStore)()
        Dim tree = groupStore.GetTree(
            GroupTreeType.Objects, GroupMember.NotRetired, Nothing, reload, False, False)

        Dim objEx As New clsGroupObjectDetails(tree.Root.Permissions)
        Dim sConfig As String
        For Each s As String In mObjects
            Try
                sConfig = gSv.GetResourceConfig(s)
            Catch ex As Exception
                sConfig = Nothing
            End Try
            If sConfig Is Nothing Then sConfig = String.Empty
            objEx.Children.Add(New clsCOMObjectDetails(s, sConfig))
        Next


        For Each gp In tree.Root : DescendChildren(gp, objEx) : Next

        For Each objWebDetail As clsWebServiceDetails In gSv.GetWebServiceDefinitions
            If objWebDetail.Enabled Then
                objEx.Children.Add(objWebDetail)
            End If
        Next

        For Each webItem As WebApi In gSv.GetWebApis
            If webItem.Enabled Then
                objEx.Children.Add(webItem)
            End If
        Next

        For Each skill In gSv.GetDetailsForAllSkills()
            objEx.Children.Add(skill)
        Next

        Return objEx
    End Function

    Private Sub DescendChildren(mem As IGroupMember, parentDets As IGroupObjectDetails)

        Dim proc = TryCast(mem, ProcessBackedGroupMember)
        If proc IsNot Nothing Then
            parentDets.Children.Add(New clsVBODetails() With {
                .ID = proc.IdAsGuid(),
                .FriendlyName = proc.Name
            })
        End If

        Dim gp = TryCast(mem, IGroup)
        If gp IsNot Nothing Then
            Dim gpDets As New clsGroupObjectDetails(gp.Permissions) With {
                .FriendlyName = gp.Name
            }
            For Each child In gp : DescendChildren(child, gpDets) : Next
            parentDets.Children.Add(gpDets)
        End If
    End Sub
    ''' <summary>
    ''' Gets the connection with the given name from this config object.
    ''' </summary>
    ''' <param name="name">The name of the connection required.</param>
    ''' <returns>The connection setting associated with the specified name.</returns>
    Public Function GetConnection(ByVal name As String) As clsDBConnectionSetting
        If name Is Nothing Then Return Nothing
        For Each conn As clsDBConnectionSetting In Me.Connections
            If conn.ConnectionName = name Then Return conn
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Clears the connections collection
    ''' </summary>
    Public Sub ClearAllConnections()
        mConnections.Clear()
    End Sub

    ''' <summary>
    ''' Adds a DB Connection name to the collection.
    ''' </summary>
    ''' <param name="mDBConnection">The connectionsetting</param>
    Public Sub AddConnection(ByVal mDBConnection As clsDBConnectionSetting)

        'Don't add duplicates...
        For Each c As clsDBConnectionSetting In mConnections
            If c.ConnectionName = mDBConnection.ConnectionName Then Exit Sub
        Next
        mConnections.Add(mDBConnection)
    End Sub

    Public Sub AddServer(ByVal server As ServerConfig)
        If Not mServers.Where(Function(s) s.Name = server.Name).Any() Then
            mServers.Add(server)
        End If
    End Sub

    ''' <summary>
    ''' Get a list of the currently installed COM Business Objects.
    ''' </summary>
    ''' <returns>A List containing the string representations of the objects.
    ''' The List will never be nothing.</returns>
    Public Function GetObjects() As List(Of String)
        Dim asObjs As New List(Of String)
        For Each s As String In mObjects
            asObjs.Add(s)
        Next
        Return asObjs
    End Function

    ''' <summary>
    ''' Add an installed COM Business Object.
    ''' </summary>
    ''' <param name="sName">The name</param>
    Public Sub AddObject(ByVal sName As String)

        'Don't add duplicates...
        For Each s As String In mObjects
            If s = sName Then Exit Sub
        Next
        mObjects.Add(sName)
    End Sub

    ''' <summary>
    ''' Removes an installed COM Business Object.
    ''' </summary>
    ''' <param name="sName">The name</param>
    Public Sub RemoveObject(ByVal sName As String)
        mObjects.Remove(sName)
    End Sub

    ''' <summary>
    ''' Disposes of this object.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            If mManager IsNot Nothing Then mManager.Dispose() : mManager = Nothing
        Catch
        End Try
    End Sub
#End Region

End Class
