Imports System.Data.SqlClient
Imports BluePrism.Common.Security
Imports BluePrism.DatabaseInstaller
Imports BluePrism.DatabaseInstaller.Data

' This is here so that other classes in this assembly don't have to import the class.
Public Enum ConnectionType
    None                   'Undefined/invalid
    Direct                 'Direct database connection
    BPServer               'Blue Prism Server
    Availability           'Availability Group
    CustomConnectionString 'User defined sql connection string
End Enum

''' Project  : AutomateAppCore
''' Class    : clsDBConnectionSetting
''' 
''' <summary>
''' Encapsulates the notion of a database connection setting, consisting of several
''' strings (ie name of db server, user name to use etc).
''' </summary>
<Serializable()>
Public Class clsDBConnectionSetting
    Implements IDatabaseConnectionSetting

    ''' <summary>
    ''' Constructor for a direct database connection.
    ''' </summary>
    ''' <param name="name">The name of the connection.</param>
    ''' <param name="server">The database server name</param>
    ''' <param name="dbname">The database name</param>
    ''' <param name="username">The database username, if applicable</param>
    ''' <param name="password">The database password, if applicable</param>
    ''' <param name="winauth">True for Windows Authentication, in which case the
    ''' database username and password are irrelevant. Otherwise, SQL Server
    ''' Authentication is used.</param>
    Public Sub New(ByVal name As String, ByVal server As String, ByVal dbname As String, ByVal username As String, ByVal password As SafeString, ByVal winauth As Boolean)
        mConnectionName = name
        mDBServer = server
        mDBName = dbname
        mDBUserName = username
        mDBUserPassword = password
        mWindowsAuth = winauth
        mConnectionType = ConnectionType.Direct
    End Sub

    ''' <summary>
    ''' Constructor for a Blue Prism Server connection.
    ''' </summary>
    ''' <param name="name">The name of the connection.</param>
    ''' <param name="server">The server name</param>
    ''' <param name="port">The port the server listens on</param>
    ''' <param name="connectionMode">The connection mode for the Blue Prism server
    ''' </param>
    ''' <param name="callbackport">The port for callbacks to the client - the client
    ''' listens on this port. Can (and normally would) be 0 to allow automatic
    ''' selection.</param>
    Public Sub New(ByVal name As String, ByVal server As String,
                   ByVal port As Integer,
                   ByVal connectionMode As ServerConnection.Mode,
                   ByVal callbackport As Integer)
        mConnectionName = name
        mDBServer = server
        mPort = port
        mConnectionMode = connectionMode
        mCallbackPort = callbackport
        mConnectionType = ConnectionType.BPServer
    End Sub

    Public Function CreateSqlSettings() As ISqlDatabaseConnectionSetting Implements IDatabaseConnectionSetting.CreateSqlSettings
        Return New SqlDatabaseConnectionSetting(mDBName, DatabaseFilePath, GetConnectionString(), GetConnectionString(True), IsComplete())
    End Function

    ''' <summary>
    ''' Constructor for an Availability Group connection.
    ''' </summary>
    ''' <param name="name">The name of the connection.</param>
    ''' <param name="server">The database server (group) name</param>
    ''' <param name="dbname">The database name</param>
    ''' <param name="username">The database username, if applicable</param>
    ''' <param name="password">The database password, if applicable</param>
    ''' <param name="winauth">True for Windows Authentication, in which case the
    ''' database username and password are irrelevant. Otherwise, SQL Server
    ''' Authentication is used.</param>
    ''' <param name="port">The port the group listens on</param>
    ''' <param name="multiSubnetFailover">True to use Multi-Subnet Failover.</param>
    Public Sub New(ByVal name As String, ByVal server As String,
                   ByVal dbname As String, ByVal username As String,
                   ByVal password As SafeString, ByVal winauth As Boolean,
                   ByVal port As Integer, ByVal multiSubnetFailover As Boolean)
        mConnectionName = name
        mDBServer = server
        mDBName = dbname
        mDBUserName = username
        mDBUserPassword = password
        mWindowsAuth = winauth
        mConnectionType = ConnectionType.Availability
        mAGPort = port
        mMultiSubnetFailover = multiSubnetFailover
    End Sub

    ''' <summary> Constructor for a SqlConnectionString connection  </summary>
    ''' <param name="name">The name of the connection.</param>
    ''' <param name="customConnectionString">The connection string specified by the user, 
    ''' from which the database name will be derived.</param>
    Public Sub New(name As String, customConnectionString As String)
        mConnectionName = name
        mConnectionType = ConnectionType.CustomConnectionString
        mCustomConnectionString = customConnectionString
    End Sub

    ''' <summary>
    ''' Constructor for an undefined and invalid connection setting. Used when the
    ''' details will be filled in later, as is the case in the connections dialog.
    ''' </summary>
    ''' <param name="name">The name of the connection.</param>
    Public Sub New(ByVal name As String)
        mConnectionName = name
        mConnectionType = ConnectionType.None
        mConnectionMode = ServerConnection.Mode.WCFSOAPMessageWindows
        mAGPort = 1433
    End Sub

    ''' <summary>
    ''' Indicates whether the password is required to be specified for this
    ''' connection or not - ie. whether it must be given by the user.
    ''' </summary>
    Public ReadOnly Property RequiresPasswordSpecifying() As Boolean Implements IDatabaseConnectionSetting.RequiresPasswordSpecifying
        Get
            ' A password is not required if this is a BP server connection, it uses 
            ' windows auth, or if it is a sqlconnectionstring connection (as any password 
            ' required will be handled in the connection string)
            If mConnectionType = ConnectionType.BPServer OrElse
               mConnectionType = ConnectionType.CustomConnectionString OrElse
               mWindowsAuth Then Return False

            ' Otherwise, we need the password specifying only if this isn't the
            ' default connection.
            Return (Me IsNot Options.Instance.DbConnectionSetting)
        End Get
    End Property

    ''' <summary>
    ''' A friendly name given by the user to this connection setting.
    ''' </summary>
    Public Property ConnectionName() As String Implements IDatabaseConnectionSetting.ConnectionName
        Get
            Return mConnectionName
        End Get
        Set(ByVal value As String)
            mConnectionName = value
        End Set
    End Property
    Private mConnectionName As String = ""

    ''' <summary>
    ''' The name of the server. Depending on the connection type, this can be the
    ''' database server, the Blue Prism Server address/hostname, or the Availability
    ''' Group Listener.
    ''' </summary>
    Public Property DBServer() As String Implements IDatabaseConnectionSetting.DBServer
        Get
            Return mDBServer
        End Get
        Set(ByVal value As String)
            mDBServer = value
        End Set
    End Property
    Private mDBServer As String = ""

    Public Property RabbitMqConfiguration As RabbitMqConfiguration

    ''' <summary>
    ''' The name of the database.
    ''' </summary>
    Public Property DatabaseName() As String Implements IDatabaseConnectionSetting.DatabaseName
        Get
            Return mDBName
        End Get
        Set(ByVal value As String)
            mDBName = value
        End Set
    End Property
    Private mDBName As String = ""

    Public Property DatabaseFilePath As String Implements IDatabaseConnectionSetting.DatabaseFilePath

    ''' <summary>
    ''' The name of the SQL user used on the server.  Not relevant if WindowsAuth is
    ''' True.
    ''' </summary>
    Public Property DBUserName() As String Implements IDatabaseConnectionSetting.DBUserName
        Get
            Return mDBUserName
        End Get
        Set(ByVal value As String)
            mDBUserName = value
        End Set
    End Property
    Private mDBUserName As String = ""

    ''' <summary>
    ''' The password of the user in DBUserName. Not relevant if WindowsAuth is True.
    ''' </summary>
    Public Property DBUserPassword() As SafeString Implements IDatabaseConnectionSetting.DBUserPassword
        Get
            Return mDBUserPassword
        End Get
        Set(ByVal value As SafeString)
            mDBUserPassword = value
        End Set
    End Property
    Private mDBUserPassword As New SafeString

    ''' <summary>
    ''' Confirms that the given password matches the password of this connection.
    ''' </summary>
    Public Function ConfirmDBPassword(password As SafeString) As Boolean Implements IDatabaseConnectionSetting.ConfirmDBPassword
        Return mDBUserPassword = password

    End Function

    ''' <summary>
    ''' True if the connection uses Windows Authentication, instead of SQL Server
    ''' Authentication, in which case the DBUserName and DBPassword properties are
    ''' irrelevant.
    ''' </summary>
    Public Property WindowsAuth() As Boolean Implements IDatabaseConnectionSetting.WindowsAuth
        Get
            Return mWindowsAuth
        End Get
        Set(ByVal value As Boolean)
            mWindowsAuth = value
        End Set
    End Property
    Private mWindowsAuth As Boolean = False

    ''' <summary>
    ''' The type of this connection.
    ''' </summary>
    Public Property ConnectionType() As ConnectionType Implements IDatabaseConnectionSetting.ConnectionType
        Get
            Return mConnectionType
        End Get
        Set(ByVal value As ConnectionType)
            mConnectionType = value
        End Set
    End Property
    Private mConnectionType As ConnectionType

    ''' <summary>
    ''' The port used for Blue Prism Server connections. Not relevant for a database
    ''' connection. Setting this to zero effectively sets it to the
    ''' <see cref="Options.DefaultServerPort">default server port</see>.
    ''' </summary>
    Public Property Port() As Integer Implements IDatabaseConnectionSetting.Port
        Get
            ' Zero is invalid, use it to represent the default.
            If mPort = 0 Then Return Options.Instance.DefaultServerPort
            Return mPort
        End Get
        Set(ByVal value As Integer)
            mPort = value
        End Set
    End Property
    Private mPort As Integer

    ''' <summary>
    ''' The callback port used for Blue Prism Server connections. Not relevant for a
    ''' database connection. The client listens on this port. Can (and normally
    ''' would) be 0 to allow automatic selection.
    ''' </summary>
    Public Property CallbackPort() As Integer Implements IDatabaseConnectionSetting.CallbackPort
        Get
            Return mCallbackPort
        End Get
        Set(ByVal value As Integer)
            mCallbackPort = value
        End Set
    End Property
    Private mCallbackPort As Integer


    ''' <summary>
    ''' The connection mode for Blue Prism Server connections. Not relevant for a
    ''' database connection.
    ''' </summary>
    Public Property ConnectionMode() As ServerConnection.Mode Implements IDatabaseConnectionSetting.ConnectionMode
        Get
            Return mConnectionMode
        End Get
        Set(ByVal value As ServerConnection.Mode)
            mConnectionMode = value
        End Set
    End Property
    Private mConnectionMode As ServerConnection.Mode

    ''' <summary>
    ''' The port used for Availability Group connections. Not relevant for other
    ''' connection types.
    ''' </summary>
    Public Property AGPort() As Integer Implements IDatabaseConnectionSetting.AGPort
        Get
            Return mAGPort
        End Get
        Set(ByVal value As Integer)
            mAGPort = value
        End Set
    End Property
    Private mAGPort As Integer

    ''' <summary>
    ''' The Multi-Subnet Failover setting for Availability Group connections. Not
    ''' relevant for other connection types.
    ''' </summary>
    Public Property MultiSubnetFailover() As Boolean Implements IDatabaseConnectionSetting.MultiSubnetFailover
        Get
            Return mMultiSubnetFailover
        End Get
        Set(ByVal value As Boolean)
            mMultiSubnetFailover = value
        End Set
    End Property
    Private mMultiSubnetFailover As Boolean

    ''' <summary>
    ''' The sql connection string to be used when the string is specified by the user 
    ''' for a <see cref="ConnectionType.CustomConnectionString"/> connection type. 
    ''' Note this is not the string generated by the string builder for the other connection types.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
    ''' </summary>
    Public Property CustomConnectionString() As String
        Get
            Return mCustomConnectionString
        End Get
        Set(ByVal value As String)
            mCustomConnectionString = value
        End Set
    End Property
    Private mCustomConnectionString As String

    ''' <summary>
    ''' The extra parameters to append to the connection string used to access the
    ''' database for this connection.
    ''' </summary>
    Public Property ExtraParams As String Implements IDatabaseConnectionSetting.ExtraParams

    ''' <summary>
    ''' Indicates whether or not the client is a Login Agent Resource. This is set
    ''' dynamically when a Login Agent Runtime Resource starts up
    ''' </summary>
    Public Property ClientIsLoginAgent As Boolean = False Implements IDatabaseConnectionSetting.ClientIsLoginAgent

    Public Property MaxTransferWindowSize As Integer?

    Public Property MaxPendingChannels As Integer?

    Public Property Ordered As Boolean? = True

    ''' <summary>
    ''' Tests the validity of the connection setting by attempting a sample read and
    ''' a sample write operation.
    ''' </summary>
    Public Sub Validate() Implements IDatabaseConnectionSetting.Validate
        ServerFactory.Validate(Me)
    End Sub

    ''' <summary>
    ''' Determines if any one of the connection properties is blank.
    ''' </summary>
    ''' <returns>True if none of the properties is a blank string, False otherwise.
    ''' </returns>
    Public Function IsComplete() As Boolean Implements IDatabaseConnectionSetting.IsComplete
        If mConnectionType = ConnectionType.BPServer Then
            If mDBServer Is Nothing OrElse mDBServer.Length = 0 Then Return False
            Return True
        End If
        If mConnectionType = ConnectionType.Availability Then
            If mDBServer Is Nothing OrElse mDBServer.Length = 0 Then Return False
            Return True
        End If
        If mConnectionType = ConnectionType.CustomConnectionString Then
            Return Not String.IsNullOrEmpty(mCustomConnectionString)
        End If
        If mDBServer Is Nothing OrElse mDBServer.Length = 0 Then Return False
        If mDBName Is Nothing OrElse mDBName.Length = 0 Then Return False
        If mWindowsAuth Then Return True
        If mDBUserName Is Nothing OrElse mDBUserName.Length = 0 Then Return False
        If mDBUserPassword Is Nothing OrElse mDBUserPassword.Length = 0 Then Return False
        Return True
    End Function


    ''' <summary>
    ''' Creates a new database connection using the current configuration
    ''' </summary>
    ''' <param name="master">True to target the master database instead of the one 
    ''' specified in the current configuration. This would only be used at database 
    ''' setup time to drop or create the database.</param>
    ''' <returns>A new database connection using the current configuration</returns>
    Public Function CreateSqlConnection(Optional master As Boolean = False) As SqlConnection Implements IDatabaseConnectionSetting.CreateSqlConnection
        Dim connection = New SqlConnection(GetConnectionString(master))
        Return connection
    End Function

    ''' <summary>
    ''' Gets a SQL Connection string, based on the current configuration
    ''' </summary>
    ''' <param name="master">True to target the master database instead of the one
    ''' specified in the current configuration</param>
    ''' <returns>The SQL connection string, or an empty string if certain DB
    ''' properties have not been completed</returns>
    Public Function GetConnectionString(Optional master As Boolean = False) As String Implements IDatabaseConnectionSetting.GetConnectionString

        If mConnectionType = ConnectionType.CustomConnectionString Then _
            Return New SqlConnectionStringBuilder(mCustomConnectionString).ToString()

        If mDBServer Is Nothing Then Return ""
        If mDBName Is Nothing Then Return ""
        If mDBUserName Is Nothing Then Return ""
        If mDBUserPassword Is Nothing Then Return ""

        Dim builder As SqlConnectionStringBuilder

        Try
            ' If we error here, it can only be due to invalid params
            builder = New SqlConnectionStringBuilder(ExtraParams)
        Catch ex As Exception
            Throw New InvalidOperationException(My.Resources.clsDBConnectionSetting_InvalidAdditionalSqlParameters)
        End Try
        builder.Add("database", If(master, "master", mDBName))

        If mConnectionType = ConnectionType.Availability Then
            builder.Add("Server", $"tcp:{mDBServer},{mAGPort}")
            If mMultiSubnetFailover Then builder.Add("MultiSubnetFailover", True)
        Else
            builder.Add("data source", mDBServer)
        End If

        If mWindowsAuth Then builder.Add("Trusted_Connection", "yes")

        If Not mWindowsAuth Then
            builder.UserID = mDBUserName
            builder.Password = mDBUserPassword.AsString()
        End If

        Return builder.ToString()

    End Function

    ''' <summary>
    ''' Performs a clone of the current object instance.
    ''' Currently a shallow clone is all that is required - all the members of
    ''' clsDBConnectionSetting are value types or semantically immutable, so any
    ''' changes on the cloned object would <em>not</em> be reflected on this object.
    ''' If at any point in the future, clsDBConnectionSetting gains any members which
    ''' are mutable, this will need to be altered to return a deep clone.
    ''' </summary>
    ''' <returns>A clone of the object.</returns>
    Public Function Clone() As IDatabaseConnectionSetting Implements IDatabaseConnectionSetting.Clone
        Return DirectCast(MemberwiseClone(), clsDBConnectionSetting)
    End Function

    ''' <summary>
    ''' Gets an integer hash of this connection setting; this is a function based on
    ''' the connection setting's name
    ''' </summary>
    ''' <returns>An integer hash of this connection setting</returns>
    Public Overrides Function GetHashCode() As Integer Implements IDatabaseConnectionSetting.GetHashCode
        Return ConnectionName.GetHashCode()
    End Function

    ''' <summary>
    ''' Checks if this connection setting is equal to the given object.
    ''' </summary>
    ''' <param name="obj">The object to test this connection setting against.</param>
    ''' <returns>True if the given object is a non-null connection setting with
    ''' <em>all</em> the same values as this connection setting.</returns>
    Public Overrides Function Equals(obj As Object) As Boolean Implements IDatabaseConnectionSetting.Equals
        Dim cs = TryCast(obj, clsDBConnectionSetting)
        Return (
            cs IsNot Nothing AndAlso
            Object.Equals(mConnectionName, cs.mConnectionName) AndAlso
            Object.Equals(mDBServer, cs.mDBServer) AndAlso
            Object.Equals(mDBName, cs.mDBName) AndAlso
            Object.Equals(mDBUserName, cs.mDBUserName) AndAlso
            Object.Equals(mDBUserPassword, cs.mDBUserPassword) AndAlso
            Object.Equals(mWindowsAuth, cs.mWindowsAuth) AndAlso
            Object.Equals(mPort, cs.mPort) AndAlso
            Object.Equals(mCallbackPort, cs.mCallbackPort) AndAlso
            Object.Equals(mConnectionMode, cs.mConnectionMode) AndAlso
            Object.Equals(mAGPort, cs.mAGPort) AndAlso
            Object.Equals(mMultiSubnetFailover, cs.mMultiSubnetFailover) AndAlso
            Object.Equals(mConnectionMode, cs.mConnectionMode) AndAlso
            Object.Equals(mCustomConnectionString, cs.mCustomConnectionString) AndAlso
            Object.Equals(Ordered, cs.Ordered) AndAlso
            Object.Equals(MaxTransferWindowSize, cs.MaxTransferWindowSize) AndAlso
            Object.Equals(MaxPendingChannels, cs.MaxPendingChannels) AndAlso
            Object.Equals(RabbitMqConfiguration, cs.RabbitMqConfiguration)
        )
    End Function

    ''' <summary>
    ''' Override ToString() to just return the connection name, which allows us to
    ''' easily use these objects in a combo box.
    ''' </summary>
    ''' <returns>The connection name</returns>
    Public Overrides Function ToString() As String Implements IDatabaseConnectionSetting.ToString
        Return mConnectionName
    End Function
End Class
