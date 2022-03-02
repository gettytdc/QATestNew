Imports System.Reflection
Imports System.IO
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Common.Security
Imports BluePrism.AutomateAppCore.Config
Imports System.Threading
Imports BluePrism.Common.Security.Exceptions
Imports BluePrism.Core.Encryption
Imports BluePrism.Core.Utility
Imports BluePrism.ClientServerResources.Core.Config
Imports BluePrism.ClientServerResources.Core.Enums
Imports System.Security.Cryptography.X509Certificates

''' Project  : AutomateAppCore
''' Class    : Options
''' <summary>
''' A class that wraps all Automate configuration options. The class is a singleton so the instance
''' should be accessed through Options.Instance. Init() should be called ONCE at startup, after which
''' all methods can be used as required.
''' </summary>
Public Class Options : Implements IOptions

    Public Shared ReadOnly Property Instance As IOptions
        Get
            If mInstance Is Nothing Then
                mInstance = New Options()
            End If
            Return mInstance
        End Get
    End Property
    Private Shared mInstance As IOptions

    ''' <summary>
    ''' Event fired when the options have been loaded / reloaded
    ''' </summary>
    ''' <remarks>Care should be taken with this event - since it is static, any
    ''' listeners will be registered and a reference will be kept to them until
    ''' either they stop listening for the event or the application is closed,
    ''' meaning that the object will not be eligible for garbage collection until
    ''' that point.
    ''' Thus, the listener should be removed from the event explicitly when no
    ''' longer needed. At the time of writing, only ctlLogin listens for this
    ''' event, and stops listening when it is Disposed.</remarks>
    Public Event Load() Implements IOptions.Load

    ''' <summary>
    ''' An arbitrary default port to use for a new server
    ''' </summary>
    Public ReadOnly Property DefaultServerPort As Integer Implements IOptions.DefaultServerPort
        Get
            Return 8199
        End Get
    End Property

    ' The static machine configuration exposed by this options class.
    Private ReadOnly Property MachineConfig As MachineConfig
        Get
            If mMachineConfig IsNot Nothing Then Return mMachineConfig
            Throw New InvalidOperationException(My.Resources.OptionsNotInitalised)
        End Get
    End Property
    Private mMachineConfig As MachineConfig


    ' The static user configuration exposed by this options class.
    Private ReadOnly Property UserConfig As UserConfig
        Get
            If mUserConfig IsNot Nothing Then Return mUserConfig
            Throw New InvalidOperationException(My.Resources.OptionsNotInitalised)
        End Get
    End Property
    Private mUserConfig As UserConfig

    ''' <summary>
    ''' Class constructor - ensures that the config objects are there
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' The connection setting used in the application.
    ''' </summary>
    Public Property DbConnectionSetting() As clsDBConnectionSetting Implements IOptions.DbConnectionSetting
        Get
            ' Get the current 'selected connection'
            Dim setting As clsDBConnectionSetting =
             MachineConfig.GetConnection(UserConfig.SelectedConnectionName)

            ' If there's one set, return it
            If setting IsNot Nothing Then Return setting

            ' Check if there is a current connection defined in the machine config
            setting = MachineConfig.MachineWideConnection

            ' If we don't find one there, we must do a bit more work to get the setting
            If setting Is Nothing Then

                ' So see if there are any connections defined.
                ' If so, arbitrarily use the first in the list as the current setting
                If Connections.Count > 0 Then
                    setting = Connections(0)

                Else
                    ' Otherwise, we must create a 'default' connection setting in the
                    ' machine config and use that.
                    setting = New clsDBConnectionSetting(My.Resources.clsOptions_DefaultConnection)
                    AddConnection(setting)

                End If
            End If

            ' Wherever we got it from, ensure that the selected connection is
            ' set in the user config
            UserConfig.SelectedConnectionName = setting.ConnectionName

            ' And return it
            Return setting
        End Get

        Set(ByVal value As clsDBConnectionSetting)
            Dim name As String = Nothing
            If value IsNot Nothing Then name = value.ConnectionName
            UserConfig.SelectedConnectionName = name
        End Set

    End Property

    ''' <summary>
    ''' Gets or sets the current connection name in these options.
    ''' </summary>
    Public Property CurrentConnectionName() As String Implements IOptions.CurrentConnectionName
        Get
            Return UserConfig.SelectedConnectionName
        End Get
        Set(ByVal value As String)
            UserConfig.SelectedConnectionName = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the system locale name in these options.
    ''' Should only be set during startup to what the system Locale was set to (non persistent)
    ''' </summary>
    Public Property SystemLocale() As String Implements IOptions.SystemLocale

    Public Property SystemLocaleFormat() As String Implements IOptions.SystemLocaleFormat

    Public Property LastUsedLocale() As String Implements IOptions.LastUsedLocale

    Public Property LastUsedLocaleFormat() As String Implements IOptions.LastUsedLocaleFormat

    Public Property WcfPerformanceLogMinutes As Integer? Implements IOptions.WcfPerformanceLogMinutes

    ''' <summary>
    ''' Gets whether to use the Culture Aware Calendar control
    ''' This is only required if System and Current locale do not match (Return CurrentLocale != SystemLocale),
    ''' but can be overridden centrally here if desired
    ''' </summary>
    Public ReadOnly Property UseCultureCalendar() As Boolean Implements IOptions.UseCultureCalendar
        Get
            Return True 'Always use CultureCalendar
        End Get
    End Property

    Public ReadOnly Property ResourceCallbackConfig As ConnectionConfig Implements IOptions.ResourceCallbackConfig
        Get
            Return gSv.GetCallbackConnectionConfig()
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the current locale name in these options.
    ''' </summary>
    Public Property CurrentLocale() As String Implements IOptions.CurrentLocale
        Get
            Return UserConfig.SelectedLocale
        End Get
        Set(ByVal value As String)
            UserConfig.SelectedLocale = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the default SQL Command timeout to use in database operations
    ''' </summary>
    Public Property SqlCommandTimeout() As Integer Implements IOptions.SqlCommandTimeout
        Get
            Return MachineConfig.SqlCommandTimeout
        End Get
        Set(ByVal value As Integer)
            MachineConfig.SqlCommandTimeout = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the default SQL Command timeout to use in long database operations
    ''' </summary>
    Public Property SqlCommandTimeoutLong() As Integer Implements IOptions.SqlCommandTimeoutLong
        Get
            Return MachineConfig.SqlCommandTimeoutLong
        End Get
        Set(ByVal value As Integer)
            MachineConfig.SqlCommandTimeoutLong = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the default SQL Command timeout to use in logging database operations
    ''' </summary>
    Public Property SqlCommandTimeoutLog() As Integer Implements IOptions.SqlCommandTimeoutLog
        Get
            Return MachineConfig.SqlCommandTimeoutLog
        End Get
        Set(ByVal value As Integer)
            MachineConfig.SqlCommandTimeoutLog = value
        End Set
    End Property


    ''' <summary>
    ''' Gets or sets the default command timeout to use for SQL Commands during installation of the
    ''' blueprism database. By default 15 minutes.
    ''' </summary>
    Public Property DatabaseInstallCommandTimeout() As TimeSpan Implements IOptions.DatabaseInstallCommandTimeout
        Get
            Return TimeSpan.FromMinutes(MachineConfig.DatabaseInstallCommandTimeout)
        End Get
        Set(ByVal value As TimeSpan)
            MachineConfig.DatabaseInstallCommandTimeout = CInt(value.TotalMinutes)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the default timeout used when sending commands to a data pipeline process
    ''' </summary>
    Public Property DataPipelineProcessSendTimeout() As Integer Implements IOptions.DataPipelineProcessSendTimeout
        Get
            Return MachineConfig.DataPipelineProcessCommandSendTimeout
        End Get
        Set(ByVal value As Integer)
            MachineConfig.DataPipelineProcessCommandSendTimeout = value
        End Set
    End Property

    ''' <summary>
    ''' True to compress process XML in the database. This flag is set up by reading
    ''' the database when a user logs in.
    ''' </summary>
    Public Property CompressProcessXML() As Boolean Implements IOptions.CompressProcessXml
        Get
            Return MachineConfig.CompressProcessXML
        End Get
        Set(ByVal value As Boolean)
            MachineConfig.CompressProcessXML = value
        End Set
    End Property

    ''' <summary>
    ''' A global option applying to all users; determines whether an edit summary
    ''' will be compulsory when saving a process in process studio. This flag is set
    ''' up by reading the database when a user logs in.
    ''' </summary>
    ''' <remarks>For some reason, it is also read from and written to the config
    ''' file within this class!?</remarks>
    Public Property EditSummariesAreCompulsory() As Boolean Implements IOptions.EditSummariesAreCompulsory
        Get
            Return MachineConfig.EditSummariesAreCompulsory
        End Get
        Set(ByVal value As Boolean)
            MachineConfig.EditSummariesAreCompulsory = value
        End Set
    End Property

    ''' <summary>
    ''' The path used for archiving
    ''' </summary>
    Public Property ArchivePath() As String Implements IOptions.ArchivePath
        Get
            Return MachineConfig.ArchivePath
        End Get
        Set(ByVal value As String)
            MachineConfig.ArchivePath = value
        End Set
    End Property

    ''' <summary>
    ''' The last user Blue Prism Native Authentication user that logged in. If the last
    ''' user who logged in was a mapped AD user or an external auth user, this will be blank.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If there is no selected
    ''' connection (which would be used as the connection to register this user on)
    ''' </exception>
    ''' <exception cref="Exception">If any errors occur while attempting to save the
    ''' configuration.</exception>
    Public Property LastNativeUser() As String Implements IOptions.LastNativeUser
        Get
            Return UserConfig.LastUserName
        End Get
        Set(ByVal value As String)
            If value = UserConfig.LastUserName Then Return
            UserConfig.LastUserName = value
        End Set
    End Property

    Public Property CurrentServerConfigName As String Implements IOptions.CurrentServerConfigName

    ''' <summary>
    ''' True if the process engine (resource PC) should be started locally when
    ''' a user logs in interactively.
    ''' </summary>
    Public Property StartProcessEngine() As Boolean Implements IOptions.StartProcessEngine
        Get
            Return MachineConfig.StartProcessEngine
        End Get
        Set(ByVal value As Boolean)
            MachineConfig.StartProcessEngine = value
        End Set
    End Property

    ''' <summary>
    ''' List of defined server instances.
    ''' </summary>
    Public ReadOnly Property Servers() As ICollection(Of MachineConfig.ServerConfig) Implements IOptions.Servers
        Get
            Return MachineConfig.Servers
        End Get
    End Property

    ''' <summary>
    ''' Stores current certificate thumbprint
    ''' </summary>
    ''' <returns></returns>
    Public Property Thumbprint() As String Implements IOptions.Thumbprint
        Get
            Return mMachineConfig.Thumbprint
        End Get
        Set(value As String)
            mMachineConfig.Thumbprint = value
        End Set
    End Property

    ''' <summary>
    ''' Stores current encryption method
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectedConfigEncryptionMethod As MachineConfig.ConfigEncryptionMethod Implements IOptions.SelectedConfigEncryptionMethod
        Get
            Return mMachineConfig.SelectedConfigEncryptionScheme
        End Get
        Set(value As MachineConfig.ConfigEncryptionMethod)
            mMachineConfig.SelectedConfigEncryptionScheme = value
        End Set
    End Property

    ''' <summary>
    ''' Attempts to get the server config with the given name from this local config
    ''' object.
    ''' </summary>
    ''' <param name="name">The name of the required server config</param>
    ''' <returns>The server config with the given name or null if no such config was
    ''' found in this config object.</returns>
    Public Function GetServerConfig(ByVal name As String) As MachineConfig.ServerConfig Implements IOptions.GetServerConfig
        Return MachineConfig.GetServerConfig(name)
    End Function

    ''' <summary>
    ''' Creates a new server configuration with a default (unique) name.
    ''' </summary>
    ''' <returns>A new server config with a name unique amongst current server config
    ''' names.</returns>
    Public Function NewServerConfig() As MachineConfig.ServerConfig Implements IOptions.NewServerConfig
        Return MachineConfig.NewServerConfig()
    End Function

    ''' <summary>
    ''' Last-used branding override title, or Nothing for not overridden.
    ''' </summary>
    Public Property LastBrandingTitle() As String Implements IOptions.LastBrandingTitle
        Get
            Return UserConfig.LastBrandingTitle
        End Get
        Set(ByVal value As String)
            UserConfig.LastBrandingTitle = value
        End Set
    End Property

    ''' <summary>
    ''' Last-used branding override icon, or Nothing for not overridden.
    ''' </summary>
    Public Property LastBrandingIcon() As String Implements IOptions.LastBrandingIcon
        Get
            Return UserConfig.LastBrandingIcon
        End Get
        Set(ByVal value As String)
            UserConfig.LastBrandingIcon = value
        End Set
    End Property


    ''' <summary>
    ''' Last-used branding override large logo, or Nothing for not overridden.
    ''' </summary>
    Public Property LastBrandingLargeLogo() As String Implements IOptions.LastBrandingLargeLogo
        Get
            Return UserConfig.LastBrandingLargeLogo
        End Get
        Set(ByVal value As String)
            UserConfig.LastBrandingLargeLogo = value
        End Set
    End Property

    ''' <summary>
    ''' The maximum number of undo levels to hold when editing a process / object.
    ''' </summary>
    Public Property MaxUndoLevels() As Integer Implements IOptions.MaxUndoLevels
        Get
            Return MachineConfig.MaxUndoLevels
        End Get
        Set(ByVal value As Integer)
            MachineConfig.MaxUndoLevels = value
        End Set
    End Property

    ''' <summary>
    ''' True if the 'unbranded' flag is set.
    ''' </summary>
    Public ReadOnly Property Unbranded As Boolean Implements IOptions.Unbranded
        Get
            Return mUnbranded
        End Get
    End Property
    Private mUnbranded As Boolean = False

    ''' <summary>
    ''' Checks if the current process has privileges enough to save the config
    ''' file to its current configured location.
    ''' Note that if the file is not writable for other reasons (eg. if it is locked
    ''' by another process), this property will not detect that - it just checks the
    ''' process owner's current privileges.
    ''' </summary>
    Public Function HasWritePrivileges() As Boolean Implements IOptions.HasWritePrivileges
        Return MachineConfig.HasWritePrivileges()
    End Function

    ''' <summary>
    ''' The current set of connection settings registered in this class.
    ''' </summary>
    Public ReadOnly Property Connections() As IList(Of clsDBConnectionSetting) Implements IOptions.Connections
        Get
            Return MachineConfig.Connections
        End Get
    End Property

    ''' <summary>
    ''' Reloads the config file, both machine-wide and user-specific.
    ''' </summary>
    ''' <exception cref="Exception">If any errors occur while attempting to reload
    ''' the config file</exception>
    Public Sub Reload() Implements IOptions.Reload
        MachineConfig.Load()
        UserConfig.Load()
        RaiseEvent Load()
    End Sub

    ''' <summary>
    ''' Initialises the options, loading the machine configuration.
    ''' </summary>
    Public Sub Init(location As IConfigLocator) Implements IOptions.Init
        Try
            mUserConfig = New UserConfig(location)
            mUserConfig.TryLoad(False)
            mMachineConfig = New MachineConfig(location)
            mMachineConfig.TryLoad(True)

            Dim ubpath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ubpath = Path.Combine(ubpath, "unbranded")
            If File.Exists(ubpath) Then
                mUnbranded = True
            End If
            'need to get the certificate exception to bubble up.
        Catch cex As CertificateException
            Throw
            ' If there's no config file (or owning directory) there, it will be
            ' created when the config is saved... just ignore any errors right now.
        Catch fnfe As FileNotFoundException : MachineConfig.Reset()
        Catch dnfe As DirectoryNotFoundException : MachineConfig.Reset()

            ' If it fails for any other reason, we need to inform the caller,
            ' so it can deal with it as necessary.
        Catch : MachineConfig.Reset() : Throw

        End Try

        RaiseEvent Load()

    End Sub

    ''' <summary>
    ''' Save the current configuration to the config file, raising exceptions on any
    ''' errors
    ''' </summary>
    ''' <exception cref="UnauthorizedAccessException">If the writing of the config
    ''' could not be performed due to auth constraints.</exception>
    ''' <exception cref="Server.Domain.Models.ConfigSaveException">If the config writing was delegated to
    ''' automateconfig and the process returned a non-zero response code.</exception>
    ''' <exception cref="Exception">If any other errors occur while attempting to
    ''' save the config file.</exception>
    Public Sub Save() Implements IOptions.Save
        MachineConfig.Save()
        UserConfig.Save()
    End Sub

    Public Sub UpdateRabbitMqConfiguration(name As String, hostUrl As String, username As String, password As String) Implements IOptions.UpdateRabbitMqConfiguration

        Dim dbConnection = MachineConfig.GetConnection(name)

        If dbConnection Is Nothing Then
            Throw New ArgumentException(String.Format(My.Resources.CouldNotFindServerConnection0, name))
        End If

        Dim rabbitMqConfiguration = New RabbitMqConfiguration(hostUrl, username, password)
        dbConnection.RabbitMqConfiguration = rabbitMqConfiguration
    End Sub


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
    Public Sub UpdateDatabaseSettings(name As String, server As String, dbname As String,
        username As String, password As SafeString, winauth As Boolean) Implements IOptions.UpdateDatabaseSettings

        MachineConfig.UpdateDatabaseSettings(name, server, dbname, username, password, winauth)
        ' Set this database as the 'last database'
        UserConfig.SelectedConnectionName = name

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
    Public Sub UpdateDatabaseSettings(name As String, server As String, dbname As String,
        username As String, password As SafeString, winauth As Boolean, port As Integer,
        multiSubnetFailover As Boolean) Implements IOptions.UpdateDatabaseSettings

        MachineConfig.UpdateDatabaseSettings(name, server, dbname, username, password, winauth,
                                             port, multiSubnetFailover)
        ' Set this database as the 'last database'
        UserConfig.SelectedConnectionName = name

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
    ''' <param name="connectionMode">The connection mode for the server.
    ''' </param>
    ''' <param name="callbackPort">The callback port for the connection to a BP
    ''' server - zero or negative values have no effect in this method.</param>
    Public Sub UpdateDatabaseSettings(name As String, address As String, port As Integer,
        connectionMode As ServerConnection.Mode, callbackPort As Integer) Implements IOptions.UpdateDatabaseSettings

        MachineConfig.UpdateDatabaseSettings(name, address, port, connectionMode, callbackPort)
        UserConfig.SelectedConnectionName = name

    End Sub

    ''' <summary>
    ''' Updates the server settings for the given server name or creates a new server
    ''' settings with the given name.
    ''' </summary>
    ''' <param name="name">The name of the server</param>
    ''' <param name="connName">The connection name to use</param>
    ''' <param name="port">The port on which to listen</param>
    ''' <param name="connectionMode">The mode used for Blue Prism Server connections</param>
    ''' <param name="scheme">The encryption scheme to add</param>
    ''' <returns>The server config object updated by this method</returns>
    Public Function UpdateServerSettings(name As String, connName As String, port As Integer,
     connectionMode As ServerConnection.Mode, scheme As clsEncryptionScheme, ordered As Boolean) As MachineConfig.ServerConfig Implements IOptions.UpdateServerSettings
        MachineConfig.UpdateServerSettings(name, connName, port, connectionMode, scheme, ordered)
        Return MachineConfig.GetServerConfig(name)
    End Function

    Public Sub UpdateASCRServerSettings(serverName As String, callbackProtocol As Integer, hostName As String, port As Integer,
                                        mode As Integer, certname As String, clientCertName As String,
                                        serverStore As String, clientStore As String) Implements IOptions.UpdateASCRServerSettings
        Dim config = New ConnectionConfig With {
            .callbackProtocol = CType(callbackProtocol, CallbackConnectionProtocol),
            .hostName = hostName,
            .port = port,
            .mode = CType(mode, InstructionalConnectionModes),
            .CertificateName = certname,
            .ClientCertificateName = clientCertName
        }
        If config.Mode = InstructionalConnectionModes.Certificate Then                        
            config.ServerStore = DirectCast([Enum].Parse(GetType(StoreName), serverStore), StoreName)
            config.ClientStore = DirectCast([Enum].Parse(GetType(StoreName), clientStore), StoreName)
        End If
        MachineConfig.UpdateASCRServerSettings(serverName, config)
    End Sub

    ''' <summary>
    ''' Confirm that the given password matches the current database password.
    ''' </summary>
    ''' <param name="sPwd">The password to compare.</param>
    ''' <returns>True if it matches.</returns>
    Public Function ConfirmDatabasePassword(ByVal sPwd As SafeString) As Boolean Implements IOptions.ConfirmDatabasePassword
        Return DbConnectionSetting.ConfirmDBPassword(sPwd)
    End Function

    ''' <summary>
    ''' Get the number of COM Business Objects currently installed.
    ''' </summary>
    ''' <returns>The number of objects.</returns>
    Public Function GetInstalledComBusinessObjectsCount() As Integer Implements IOptions.GetInstalledComBusinessObjectsCount
        Return MachineConfig.GetNumObjects()
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
    Public Function GetExternalObjectsInfo(reload As Boolean) As IGroupObjectDetails Implements IOptions.GetExternalObjectsInfo
        Return MachineConfig.GetExternalObjectsInfo(reload)
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
    Public Function GetExternalObjectsInfo() As IGroupObjectDetails Implements IOptions.GetExternalObjectsInfo
        Return MachineConfig.GetExternalObjectsInfo(False)
    End Function

    ''' <summary>
    ''' Get an list of the currently installed COM Business Objects.
    ''' </summary>
    ''' <returns>A List containing the string representations of the objects.
    ''' The List will never be nothing.</returns>
    Public Function GetObjects() As List(Of String) Implements IOptions.GetObjects
        Return MachineConfig.GetObjects()
    End Function

    ''' <summary>
    ''' Clears the connections collection
    ''' </summary>
    Public Sub ClearAllConnections() Implements IOptions.ClearAllConnections
        MachineConfig.ClearAllConnections()
    End Sub

    ''' <summary>
    ''' Adds a DB Connection name to the collection.
    ''' </summary>
    ''' <param name="mDBConnection">The connectionsetting</param>
    Public Sub AddConnection(ByVal mDBConnection As clsDBConnectionSetting) Implements IOptions.AddConnection
        MachineConfig.AddConnection(mDBConnection)
    End Sub

    ''' <summary>
    ''' Add an installed COM Business Object.
    ''' </summary>
    ''' <param name="sName">The name</param>
    Public Sub AddObject(ByVal sName As String) Implements IOptions.AddObject
        MachineConfig.AddObject(sName)
    End Sub

    ''' <summary>
    ''' Removes an installed COM Business Object.
    ''' </summary>
    ''' <param name="sName">The name</param>
    Public Sub RemoveObject(ByVal sName As String) Implements IOptions.RemoveObject
        MachineConfig.RemoveObject(sName)
    End Sub

    ''' <summary>
    ''' Disposes of the inner config held by this options class. This should be
    ''' treated as rendering the options unusable and thus should only be performed
    ''' when the application is exiting.
    ''' </summary>
    Public Sub DisposeConfig() Implements IOptions.DisposeConfig
        mMachineConfig?.Dispose()
    End Sub

    ''' <summary>
    ''' Sets Culture for the UI to last locale used by user, or if not yet initialized, to the
    ''' Installer's locale.
    ''' </summary>
    Public Sub SetLastUsedLocale() Implements IOptions.SetLastUsedLocale
        If LastUsedLocale Is Nothing Then
            LastUsedLocale = GetInstallerLocale()
            LastUsedLocaleFormat = LastUsedLocale
        End If

        '<last-locale> in User.config
        If CurrentLocale Is Nothing Then
            CurrentLocale = LastUsedLocale
        End If

        If CurrentLocale IsNot Nothing Then
            Dim currentLocaleFormat = CurrentLocale
            If CurrentLocale = SystemLocale Then currentLocaleFormat = SystemLocaleFormat

            Thread.CurrentThread.CurrentUICulture = New Globalization.CultureInfo(CurrentLocale)
            Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo(currentLocaleFormat)
        End If
    End Sub

    Public Sub SetSystemLocale() Implements IOptions.SetSystemLocale
        If SystemLocale Is Nothing Then
            SystemLocale = Thread.CurrentThread.CurrentUICulture.Name
            SystemLocaleFormat = Thread.CurrentThread.CurrentCulture.Name

            If Thread.CurrentThread.CurrentUICulture.Parent.Name = "fr" Then
                SystemLocale = "fr-FR"
            End If
            If CultureHelper.IsLatinAmericanSpanish() Then
                SystemLocale = CultureHelper.LatinAmericanSpanish
            End If
        End If
    End Sub

    ''' <summary>
    ''' Checks if the system is configured to use custom certificates.  If it is, then return the expiry date
    ''' else it will return null
    ''' </summary>
    ''' <returns>certificate expiry date or null, depending on the current configuration</returns>
    Public Function GetCertificateExpiryDateTime() As Date? Implements IOptions.GetCertificateExpiryDateTime
        Dim expires As Date? = Nothing
        If SelectedConfigEncryptionMethod = MachineConfig.ConfigEncryptionMethod.OwnCertificate Then
            Dim certificateServices = New CertificateServices()
            expires = certificateServices.CertificateExpiryDateTime(Thumbprint)
        End If
        Return expires
    End Function

    Private Function GetInstallerLocale() As String
        Try
            Dim regKey = If(OpenAutomateKey(Microsoft.Win32.RegistryView.Registry64),
                            OpenAutomateKey(Microsoft.Win32.RegistryView.Registry32))

            If regKey IsNot Nothing Then
                Dim InstallLocale = CStr(regKey.GetValue("InstallLocale"))
                If InstallLocale IsNot Nothing Then
                    Dim locale = New Globalization.CultureInfo(InstallLocale)
                    If locale.Parent.Name = "fr" Then locale = New Globalization.CultureInfo("fr-FR")
                    If CultureHelper.IsLatinAmericanSpanish(locale.Name) OrElse CultureHelper.IsLatinAmericanSpanish() Then
                        locale = New Globalization.CultureInfo(CultureHelper.LatinAmericanSpanish)
                    End If
                    Return locale.Name
                End If
            End If

        Catch
            'Fall through
        End Try

        Return Thread.CurrentThread.CurrentUICulture.Name
    End Function

    Private Function OpenAutomateKey(view As Microsoft.Win32.RegistryView) As Microsoft.Win32.RegistryKey
        Dim base = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, view)
        Return base.OpenSubKey("SOFTWARE\Blue Prism Limited\Automate")
    End Function
End Class
