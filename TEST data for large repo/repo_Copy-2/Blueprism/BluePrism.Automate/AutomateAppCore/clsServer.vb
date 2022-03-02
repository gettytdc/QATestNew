Imports System.Collections.Concurrent
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports NLog

Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Caching
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib

Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Core.Configuration
Imports BluePrism.Data

Imports BluePrism.AutomateAppCore.clsServerPartialClasses.EnvironmentVariables

Imports LocaleTools
Imports BluePrism.DataPipeline
Imports Microsoft.Win32
Imports BluePrism.Server.Core
Imports BluePrism.Server.Domain.Models
''' <summary>
''' The error numbers indicating specific errors on the database
''' See : http://msdn.microsoft.com/en-us/library/cc645611.aspx.
''' You can check for this in a Try..Catch as follows :-
''' <code>
''' Try
'''   ' ... DB Code
'''
''' Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.UniqueKeyError
'''   ' Handle unique key error, any other type of error is still thrown
'''
''' End Try
''' </code>
''' </summary>
Friend Enum DatabaseErrorCode As Integer
    ForeignKeyError = 547
    DeadlockVictim = 1205
    GetAppLockFailed = 51000
    WorkQueueItemNotFound = 51001
    WorkQueueItemLocked = 51002
    WorkQueueItemDeferred = 51003
    WorkQueueItemNotActive = 51004
    LockRequestTimeOutPeriodExceeded = 1222
    ' Helpfully, there are two types of 'UNIQUE' failure - one if the UNIQUE object
    ' which caught the error was an index (UniqueIndexError) and one if the UNIQUE
    ' object which caught the error was a constraint (UniqueConstraintError).
    ' Note that the latter is the one thrown for a duplicate primary key.
    UniqueIndexError = 2601
    UniqueConstraintError = 2627
    UpdateConflict = 3960
    DataTooLongError = 8152
End Enum

Public Enum ResourceRegistrationMode
    'Names are Register As / Address As
    MachineMachine = 0
    MachineFQDN = 1
    FQDNFQDN = 2
End Enum

''' Project  : AutomateAppCore
''' Class    : clsServer
'''
''' <summary>
''' Provides the interface for client applications to the Blue Prism Server. This
''' interface is implemented either as a direct connection to a shared database, or
''' as a remote connection to a Blue Prism Server instance. In the latter case, this
''' same functionality is also used by the server to provide the relevant access its
''' clients.
'''
''' Because this class is proxied over a .NET Remoting connection when used in Blue
''' Prism Server mode (note that it inherits MarshalByRefObject), all parameters of
''' public methods must be Serializable.
'''
''' When checking user-related information (i.e. who is logged in, what permissions
''' they have) within this class, the following member variables should be used:
'''
'''      mLoggedIn,
'''      LoggedInUserID
'''      mLoggedInMachine
'''      mLoggedInRoles
'''
''' Use the above in preference to checking clsUser properties, which are provided
''' as a convenience for client applications, and also in preference to passing these
''' values in as parameters to methods, which is less secure. Remember that this
''' class operates as both a normal class within a 'standalone' client, but also as
''' a fully operational server.
'''
''' Nothing in this class should ever refer to gSv. The Release/Build script is
''' checking that this is the case, and will only allow it to be mentioned once, i.e.
''' in this comment!
''' </summary>
Public Class clsServer
    Inherits MarshalByRefObject
    Implements IServer, IServerPrivate

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    Private ReadOnly mDependencyResolver As IDependencyResolver = DependencyResolver.GetScopedResolver()

    Private ReadOnly mAppSettings As IAppSettings = mDependencyResolver.Resolve(Of IAppSettings)()
    Private ReadOnly mCacheDataProvider As ICacheDataProvider =
                         mDependencyResolver.Resolve(Of ICacheDataProvider)()

    Private ReadOnly mUniqueUserNameGenerator As IUniqueUsernameGenerator =
        mDependencyResolver.Resolve(Of IUniqueUsernameGenerator)()

    Private ReadOnly mDataPipelinePublisher As IDataPipelinePublisher =
                        mDependencyResolver.Resolve(Of IDataPipelinePublisher)()

    Private ReadOnly mSqlHelper As ISqlHelper =
        mDependencyResolver.Resolve(Of ISqlHelper)()

    Private ReadOnly mAccessTokenValidator As IAccessTokenValidator = mDependencyResolver.Resolve(Of IAccessTokenValidator)()

    Private ReadOnly mAccessTokenClaimsParser As IAccessTokenClaimsParser = mDependencyResolver.Resolve(Of IAccessTokenClaimsParser)()

    ''' <summary>
    ''' Constants used primarily within clsServer, but which may be useful elsewhere
    ''' </summary>
    Public Class Constants

        ''' <summary>
        ''' The minimum date accepted by SQL Server.
        ''' </summary>
        Public Shared MinSQLDate As New DateTime(1753, 1, 1)

        ''' <summary>
        ''' The minimum date accepted by SQL Server.
        ''' </summary>
        Public Shared MaxSQLDate As New DateTime(9999, 12, 31)

    End Class

    ''' <summary>
    ''' Checks that there is a currently logged in user, and that they have 1 of the
    ''' passed permissions. Calling this method without any permissions simply checks
    ''' that there is a logged in user.
    ''' </summary>
    ''' <exception cref="PermissionException">Thrown if a user is not logged in, or
    ''' does not have the required permissions</exception>
    Protected Sub CheckPermissions(Optional permissions() As String = Nothing,
                                 Optional user As IUser = Nothing)

        ' SecuredMethod attribute on calling method defines access rules
        Dim method = New StackFrame(1).GetMethod()
        'Debug.Print($"Function: {method.Name}")
        Dim attribute = method.GetCustomAttributes(GetType(SecuredMethodAttribute), False).
            OfType(Of SecuredMethodAttribute).
            FirstOrDefault

        Dim context = New ServerPermissionsContext With {
            .AllowAnyLocalCalls = If(attribute?.AllowLocalUnsecuredCalls, False),
            .IsLocal = Not RunningOnServer,
            .User = If(user, mLoggedInUser),
            .Permissions = If(permissions, attribute?.Permissions)
        }

        PermissionValidator.EnsurePermissions(context)
    End Sub

#Region " MonitorWrapper Class "

    ''' <summary>
    ''' Server-side progress monitor wrapper which fires progress changes through
    ''' its assigned monitor (if it has one), ignoring any errors which may
    ''' occur while doing so.
    ''' </summary>
    ''' <remarks>Note that this class <em>does not</em> cause
    ''' <see cref="clsProgressMonitor.ProgressChanged"/> to be raised on itself, it
    ''' delegates directly to the parent progress monitor.</remarks>
    Private Class MonitorWrapper : Inherits clsProgressMonitor

        ' The progress monitor to which progress updates are sent - may be null
        Private mWrapped As clsProgressMonitor

        ''' <summary>
        ''' Creates a new progress monitor wrapper around the given progress monitor
        ''' </summary>
        ''' <param name="wrappedMonitor">The monitor to which progress updates passed
        ''' to this object should be sent onto. Null indicates that progress updates
        ''' should just be ignored.</param>
        Public Sub New(ByVal wrappedMonitor As clsProgressMonitor)
            mWrapped = wrappedMonitor
        End Sub

        ''' <summary>
        ''' Overrides the event firing method of the base class in order to pass any
        ''' progress changes onto the wrapped progress monitor.
        ''' </summary>
        ''' <param name="value">The value to which the progress should be set</param>
        ''' <param name="data">The callback data with any progress information - this
        ''' will be determined by the context of the event.</param>
        ''' <remarks>Note that this override does not call its base implementation,
        ''' meaning that this object does not fire ProgressChanged events directly -
        ''' it only passes them onto the wrapped progress monitor.</remarks>
        Protected Overrides Sub OnProgressChanged(ByVal value As Integer, ByVal data As Object)
            If mWrapped IsNot Nothing Then
                Try
                    mWrapped.FireProgressChange(value, data)
                Catch ex As Exception
                    Debug.WriteLine(ex.ToString())
                End Try
            End If
        End Sub

    End Class

#End Region


    ''' <summary>
    ''' This enum is used to list the possible places where sessions can be logged.
    ''' It's used in a number of places to either combine query results or chose
    ''' the correct table for the query.
    ''' </summary>
    Private Enum SessionLogTables
        BPASessionLog_Unicode
        BPASessionLog_NonUnicode
    End Enum

    ' This is used as a cache by ValidateTableName
    Private mValidTables As New ConcurrentDictionary(Of String, String)

    ''' <summary>
    ''' Validation function to check that a table name is actually a table name and
    ''' not a sql-injection attack.
    ''' </summary>
    ''' <param name="con">The database connetion</param>
    ''' <param name="name">The name of the database table to check</param>
    ''' <returns>The name of the table</returns>
    Friend Function ValidateTableName(con As IDatabaseConnection, name As String) As String

        ' Check the cache, if it's not there, check it and then add it.
        Dim cacheRead As String = ""
        If mValidTables.TryGetValue(name, cacheRead) Then Return cacheRead

        ' It's not in the cache so validate the table and add it to the cache.

        ' Check alpha chars only in table name
        Dim m As Match = Regex.Match(name, "^[a-zA-Z0-9_.]*$")
        If Not m.Success Then
            Throw New BluePrismException("Invalid Table, contains non alpha characters: " & name)
        End If

        ' check the table exists on the database - use a paramter to prevent sql-injection
        Dim validName As String = Nothing
        Using cmd As New SqlCommand("select name from sysobjects where id = object_id(@name)")
            cmd.Parameters.AddWithValue("@name", name)
            validName = IfNull(con.ExecuteReturnScalar(cmd), "")
            If String.IsNullOrEmpty(validName) Then
                Throw New BluePrismException("Invalid Table, not found in database : " & name)
            End If
        End Using

        ' Add to the cache - might possibly fail if another thread has added it, but we can live
        ' with that.
        mValidTables.TryAdd(name, validName)
        Return validName

    End Function

    Private mValidFields As New ConcurrentDictionary(Of String, String)

    ''' <summary>
    ''' Check that a given table and field is valid.
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="table"></param>
    ''' <param name="field"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function ValidateFieldName(con As IDatabaseConnection, table As String, field As String) As String

        Dim key As String = String.Format("{0}|{1}", table, field)
        Dim cacheRead As String = ""
        If mValidFields.TryGetValue(key, cacheRead) Then Return cacheRead

        ' Wasn't in the cache, let's validate the data.
        ValidateTableName(con, table)

        Dim m As Match = Regex.Match(field, "^[a-zA-Z0-9_.]*$")
        If Not m.Success Then
            Throw New BluePrismException("Invalid Field, contains non alpha characters: " & field)
        End If

        Dim validName As String = Nothing
        Using cmd As New SqlCommand("select name from sys.columns where Name = @field " &
                                    " and Object_ID = Object_ID(@table)")
            cmd.Parameters.AddWithValue("@field", field)
            cmd.Parameters.AddWithValue("@table", table)
            validName = IfNull(con.ExecuteReturnScalar(cmd), "")
            If String.IsNullOrEmpty(validName) Then
                Throw New BluePrismException("Invalid field, not found in database : " & field)
            End If
        End Using

        ' Add to the cache if we've passed the checks.
        mValidFields.TryAdd(key, validName)

        Return validName
    End Function

    <UnsecuredMethod>
    Public Function CheckSnapshotIsolationIsEnabledInDB() As Boolean Implements IServer.CheckSnapshotIsolationIsEnabledInDB
        Using con = GetConnection()
            con.BeginTransaction(IsolationLevel.Snapshot)
            Using cmd As New SqlCommand("select max(sessionnumber) from bpaSession")
                Try
                    con.ExecuteReturnScalar(cmd)
                    con.CommitTransaction()
                    Return True
                Catch
                    ' ALLOW_SNAPSHOT_ISOLATION is set to no
                    con.RollbackTransaction()
                End Try
                Return False
            End Using
        End Using
    End Function


    ' Flag indicating if this class exists on a running BP Server instance
    Private Shared mRunningOnServer As Boolean = False

    ''' <summary>
    ''' Gets or sets whether this clsServer class resides within a running server.
    ''' </summary>
    ''' <remarks>
    ''' This defaults to false (indicating that it resides on either a client or a
    ''' non-running server process), and should only ever be changed by the running
    ''' server code (which unfortunately, it's not easy to constrain using the
    ''' language-available mechanisms).
    ''' </remarks>
    Public Shared Property RunningOnServer() As Boolean
        Get
            Return mRunningOnServer
        End Get
        Set(ByVal value As Boolean)
            If value = mRunningOnServer Then Return
            Debug.Print("Running on server changed to: {0}", value)
            mRunningOnServer = value
        End Set
    End Property

    Protected mPermissionValidator As IPermissionValidator

    Friend ReadOnly Property PermissionValidator As IPermissionValidator

        Get
            If mPermissionValidator Is Nothing Then
                mPermissionValidator = New PermissionValidator()
            End If
            Return mPermissionValidator
        End Get
    End Property

    ''' <summary>
    ''' Stores the connection mode of the transport client-server
    ''' </summary>
    ''' <remarks>used to correctly package error handlers</remarks>
    Private Shared mConnectionMode As ServerConnection.Mode
    Public Shared Property ConnectionMode() As ServerConnection.Mode
        Get
            Return mConnectionMode
        End Get
        Set(ByVal value As ServerConnection.Mode)
            mConnectionMode = value
        End Set
    End Property

    ''' <summary>
    ''' The maximum number of parameters to be passed in a sql query
    ''' The max number of RPC arguments is actually 2100, but I guess the SQL
    ''' internals keep some for themselves because 2100 (and a few other close
    ''' values) didn't work - 2000 seems a reasonable limit, should play nice with
    ''' the SqlCommand-ish classes and is a nice round number to boot. Yay for 2000
    ''' </summary>
    Private Const MaxSqlParams As Integer = 2000

    ''' <summary>
    ''' Adds a varchar (as opposed to nvarchar) parameter to a SqlParameterCollection
    ''' </summary>
    ''' <param name="coll">The collection to which the nvarchar param should be added
    ''' </param>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="value">The value of the parameter</param>
    ''' <returns>The SQL parameter which was added to the collection</returns>
    Private Shared Function AddVarChar(ByVal coll As SqlParameterCollection,
     ByVal name As String, ByVal value As String) As SqlParameter
        Dim param As SqlParameter = coll.Add(name, SqlDbType.VarChar)
        If value Is Nothing _
         Then param.Value = DBNull.Value _
         Else param.Value = value

        Return param
    End Function

    ''' <summary>
    ''' Checks if the given object is null, returning <paramref name="ifNullValue"/>
    ''' if it is. Otherwise, it returns the value given cast into the required type.
    ''' </summary>
    ''' <typeparam name="T">The type required.</typeparam>
    ''' <param name="obj">The object which either holds null or the value required.
    ''' </param>
    ''' <param name="ifNullValue">The value to return if the given object was null.
    ''' </param>
    ''' <returns>The given value cast into the specified type, or <paramref
    ''' name="ifNullValue"/> if the given value was null.</returns>
    ''' <remarks>This is primarily written to make ExecuteScalar calls a bit easier
    ''' to write. eg. rather than:
    ''' <code>
    ''' Dim obj as Object = con.ExecuteReturnScalar(cmd)
    ''' If obj Is Nothing Then Return Guid.Empty
    ''' Return DirectCast(obj, Guid)
    ''' </code>
    ''' ... you can now use :
    ''' <code>
    ''' Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    ''' </code>
    ''' but it could be used for the converse - the setting of parameters
    ''' cmd.Parameters.AddWithValue("@name", IfNull(name, "[empty]"))
    ''' </remarks>
    Friend Shared Function IfNull(Of T)(ByVal obj As Object, ByVal ifNullValue As T) As T
        If (obj Is Nothing OrElse IsDBNull(obj)) Then Return ifNullValue
        Return CType(obj, T)
    End Function

    <UnsecuredMethod>
    Public Function GetBluePrismVersion() As String Implements IServer.GetBluePrismVersion
        Return GetBluePrismVersionS()
    End Function

    ''' <summary>
    ''' Get the Blue Prism version in use. There is a static and non-static version,
    ''' to allow comparision across the remoting boundary to ensure client and
    ''' server are running the same versions.
    ''' </summary>
    Friend Shared Function GetBluePrismVersionS() As String
        If mBluePrismVersion Is Nothing Then
            mBluePrismVersion = GetBlueprismVersionLazy()
        End If
        Return mBluePrismVersion
    End Function
    Private Shared mBluePrismVersion As String

    Friend Shared Function GetBlueprismVersionLazy() As String
        Try
            Using baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry64)
                Dim value = GetBluePrismApiVersion(baseKey)
                If value IsNot Nothing Then Return value
            End Using

            Using baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry32)
                Dim value = GetBluePrismApiVersion(baseKey)
                If value IsNot Nothing Then Return value
            End Using
        Catch
            'If the above fails resort to the old method.
        End Try
        Return GetType(clsServer).Assembly.GetName.Version.ToString()
    End Function

    Private Shared Function GetBluePrismApiVersion(baseKey As RegistryKey) As String
        Return CStr(baseKey?.OpenSubKey("Software\Blue Prism Limited\Automate")?.GetValue("APIVersion"))
    End Function

    ''' <summary>
    ''' Ensures that the connection to the database is valid
    ''' </summary>
    ''' <exception cref="UnavailableException">If there is no valid connection from
    ''' the server to the database.</exception>
    ''' <remarks>It's pretty arbitrary what this method actually does - the main
    ''' thing is that it ensures that communication with the database is available;
    ''' the current implementation just gets the UTC date/time from the database.
    ''' </remarks>
    <UnsecuredMethod>
    Public Sub EnsureDatabaseConnection() Implements IServer.EnsureDatabaseConnection
        Try
            Using con = GetConnection()
                Using cmd As New SqlCommand("select getutcdate()")
                    Dim nowish = IfNull(con.ExecuteReturnScalar(cmd), Date.MinValue)
                    Debug.Assert(nowish <> Date.MinValue)
                End Using
            End Using

        Catch ex As Exception
            Throw New UnavailableException(
                My.Resources.clsServer_DatabaseConnectionFromTheServerIsUnavailable0,
                ex.Message)

        End Try
    End Sub


    ''' <summary>
    ''' The database connection settings currently in use.
    ''' </summary>
    Private mDBConnectionSetting As IDatabaseConnectionSetting

    'TODO: Inject this dependency
    'Ideally this would be injected in the constructor to remove the new statement. Currently
    'it has been moved to this variable to allow unit tests to set it and avoid writing to
    'the database.
    Private mDatabaseConnectionFactory As Func(Of IDatabaseConnection) = Function() New clsDBConnection(mDBConnectionSetting)

    Private ReadOnly mDatabaseCommandFactory As Func(Of String, IDbCommand) = mDependencyResolver.Resolve(Of Func(Of String, IDbCommand))

    ''' <summary>
    ''' When running on the server, this stores the database connection setting in
    ''' use so that client activated instances can pick it up.
    ''' </summary>
    Private Shared mServerDBConnectionSetting As IDatabaseConnectionSetting

    ' The currently logged in user; null if not logged in
    Private mLoggedInUser As IUser = Nothing

    ''' <summary>
    ''' The name of the machine currently logged in from, or Nothing if not logged
    ''' in.
    ''' </summary>
    Private mLoggedInMachine As String = Nothing

    ' The mode in which this server is logged on
    Private mLoggedInMode As AuthMode
    Private mLoggedInUserLocale As String

    ''' <summary>
    ''' Flag indicating if this instance of clsServer is currently logged in or not
    ''' </summary>
    Private Function GetLoggedIn() As Boolean
        Return (mLoggedInUser IsNot Nothing)
    End Function

    ''' <summary>
    ''' The ID of the currently logged in user or <see cref="Guid.Empty"/> if this
    ''' instance of clsServer is not currently logged in.
    ''' </summary>
    Private Function GetLoggedInUserId() As Guid
        Dim u = mLoggedInUser
        If u Is Nothing Then Return Guid.Empty Else Return u.Id
    End Function

    ''' <summary>
    ''' The username of the currently logged in user or null if this instance of
    ''' clsServer is not currently logged in.
    ''' </summary>
    Private Function GetLoggedInUserName() As String
        Dim u = mLoggedInUser
        If u Is Nothing Then
            Return Nothing
        End If
        If Not String.IsNullOrEmpty(u.Name) Then
            Return u.Name
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' The roles that the currently logged in user is a member of, or an empty
    ''' roleset if there is no currently logged in user.
    ''' </summary>
    Private Function GetLoggedInUserRoles() As RoleSet
        Dim u = mLoggedInUser
        If u Is Nothing Then Return New RoleSet() Else Return u.Roles
    End Function

    ''' <summary>
    ''' Uses the TimeZoneInfo library to return the time zone ID of the configured time zone on the Blue Prism Server.
    ''' </summary>
    <SecuredMethod(True)>
    Public Function GetServerTimeZoneId() As String Implements IServer.GetServerTimeZoneId
        CheckPermissions()
        Return TimeZoneInfo.Local.Id
    End Function

    <SecuredMethod(True)>
    Public Function GetServerFullyQualifiedDomainName() As String Implements IServer.GetServerFullyQualifiedDomainName
        CheckPermissions()
        Return clsUtility.GetFQDN
    End Function

    ''' <summary>
    ''' True when this instance of clsServer is being accessed remotely. i.e. if you
    ''' are a client, and your instance of clsServer is hosted on a Blue Prism
    ''' Server, this will be True. In all other cases, it will be False.
    ''' </summary>
    <SecuredMethod(True)>
    Public Function IsServer() As Boolean Implements IServer.IsServer
        CheckPermissions()
        Return mIsServer
    End Function
    Private mIsServer As Boolean

    ''' <summary>
    ''' Constructor used only on the server for client-activated instances. It takes
    ''' the database connection setting from the shared member set up by the first
    ''' instance, which the server created itself.
    ''' </summary>
    Public Sub New()
        mDBConnectionSetting = mServerDBConnectionSetting
        mIsServer = True
    End Sub

    ''' <summary>
    ''' Constructor used when creating a local instance for a direct database
    ''' connection. It makes no sense to use this remotely.
    ''' </summary>
    ''' <param name="cons">The clsDBConnectionSetting that specifies the parameters
    ''' for the database connection.</param>
    ''' <param name="keys">The encryption keys to use. This should be
    ''' passed when constructing in a server instance, where the keys
    ''' have been retrieved from the server's config file. In all other
    ''' circumstances, Nothing should be passed.</param>
    Friend Sub New(cons As clsDBConnectionSetting, keys As Dictionary(Of String, clsEncryptionScheme))
        mDBConnectionSetting = cons
        mServerDBConnectionSetting = mDBConnectionSetting
        If keys IsNot Nothing Then serverKeys = keys
        mIsServer = False
    End Sub


    ''' <summary>
    ''' The maximum number of characters we'll allow on a SQL text field before we
    ''' switch to updating the field via a pointer instead of the normal SQL.
    ''' </summary>
    Private Const MaxSQLTextChars As Integer = 4000

    ''' <summary>
    ''' Gets the generic message for alerts of the given alert event type.
    ''' </summary>
    ''' <param name="type">The type for which an alert event message is required.
    ''' </param>
    ''' <returns>The generic message to return for the given alert type.</returns>
    Private Shared Function GetAlertEventTypeMessage(ByVal type As AlertEventType) As String

        Select Case type

            Case AlertEventType.Stage
                Return My.Resources.clsServer_AlertStageHit

            Case AlertEventType.ProcessPending
                Return My.Resources.clsServer_ProcessPending
            Case AlertEventType.ProcessRunning
                Return My.Resources.clsServer_ProcessRunning
            Case AlertEventType.ProcessStopped
                Return My.Resources.clsServer_ProcessStopped
            Case AlertEventType.ProcessComplete
                Return My.Resources.clsServer_ProcessCompleted
            Case AlertEventType.ProcessFailed
                Return My.Resources.clsServer_ProcessTerminated

            Case AlertEventType.ScheduleStarted
                Return My.Resources.clsServer_ScheduleStarted
            Case AlertEventType.ScheduleCompleted
                Return My.Resources.clsServer_ScheduleCompleted
            Case AlertEventType.ScheduleTerminated
                Return My.Resources.clsServer_ScheduleTerminated

            Case AlertEventType.TaskStarted
                Return My.Resources.clsServer_TaskStarted
            Case AlertEventType.TaskCompleted
                Return My.Resources.clsServer_TaskCompleted
            Case AlertEventType.TaskTerminated
                Return My.Resources.clsServer_TaskTerminated

            Case Else
                Return My.Resources.clsServer_UnknownAlertType

        End Select

    End Function

    ''' <summary>
    ''' Gets the currently supported alert notification types
    ''' </summary>
    ''' <returns>A collection of the alert notification types which are currently
    ''' supported by this server.</returns>
    Private Shared Function GetSupportedAlertNotificationTypes() _
     As ICollection(Of AlertNotificationType)
        Return New AlertNotificationType() {
         AlertNotificationType.PopUp,
         AlertNotificationType.MessageBox,
         AlertNotificationType.Taskbar,
         AlertNotificationType.Sound}
    End Function

#Region "General"

    ''' <summary>
    ''' Get a database connection.
    ''' </summary>
    ''' <returns>A new clsDBConnection instance ready to use</returns>
    Private Function GetConnection() As IDatabaseConnection
        Return mDatabaseConnectionFactory()
    End Function

    ''' <summary>
    ''' Gets a raw database connection without the clsDBConnection wrapper
    ''' </summary>
    ''' <returns>An SqlConnection to the currently connected database.</returns>
    Private Function GetRawConnection() As SqlConnection
        Return mDBConnectionSetting.CreateSqlConnection()
    End Function

    ''' <summary>
    ''' Delegate for running some database code.
    ''' </summary>
    ''' <param name="con">The connection on which the database access should be
    ''' performed</param>
    ''' <param name="params">Any parameters which determine how the function should
    ''' go about its business</param>
    ''' <returns>Any appropriate return value, null if no such value is appropriate.
    ''' </returns>
    ''' <remarks>
    ''' This is internal to AutomateAppCore and, in concert with the clsServer
    ''' <see cref="Run"/> method, is here primarily to allow unit tests to execute
    ''' arbitrary SQL without polluting the clsServer namespace.
    ''' </remarks>
    Friend Delegate Function ConnectionRunner(
     ByVal con As IDatabaseConnection, ByVal params() As Object) As Object

    ''' <summary>
    ''' Runs the given connection runner, first passing in the specified parameters
    ''' and returning the resultant return value.
    ''' </summary>
    ''' <param name="runner">The delegate to call which will handle the access to
    ''' the database.</param>
    ''' <param name="params">The parameters which should be passed onto the delegate.
    ''' </param>
    ''' <returns>The return value from the delegate</returns>
    ''' <exception cref="Exception">This method makes no attempt to mask any
    ''' exceptions which may be thrown by the delegate, so it is up to the calling
    ''' method to deal with exceptions appropriately.</exception>
    ''' <remarks>
    ''' This is internal to AutomateAppCore and is here primarily to allow unit
    ''' tests to execute arbitrary SQL without polluting the clsServer namespace.
    ''' </remarks>
    Friend Function Run(ByVal runner As ConnectionRunner, ByVal ParamArray params() As Object) _
     As Object
        Using con = GetConnection()
            Return runner(con, params)
        End Using
    End Function

    ''' <summary>
    ''' The database server that this server is ultimately connecting to.
    ''' </summary>
    Private Property Database() As DatabaseServer
        Get
            Return mDatabase
        End Get
        Set(ByVal value As DatabaseServer)
            mDatabase = value
        End Set
    End Property
    Private mDatabase As DatabaseServer

    ''' <summary>
    ''' The database or server that we are connected to, in a label form
    ''' (ie. human-readable). When connecting to a Blue Prism Server, this will
    ''' reflect the server machine name, otherwise it will return the underlying
    ''' database.
    ''' </summary>
    <UnsecuredMethod()>
    Public Function GetConnectedTo() As String Implements IServer.GetConnectedTo
        If mIsServer Then Return Environment.MachineName
        Select Case Database
            Case DatabaseServer.SqlServer2000 : Return "SQL Server 2000"
            Case DatabaseServer.SqlServer2005 : Return "SQL Server 2005"
            Case DatabaseServer.SqlServer2008 : Return "SQL Server 2008"
            Case DatabaseServer.SqlServer2012 : Return "SQL Server 2012"
            Case DatabaseServer.SqlServer2014 : Return "SQL Server 2014"
            Case DatabaseServer.SqlServer2016 : Return "SQL Server 2016"
            Case DatabaseServer.SqlServer2017 : Return "SQL Server 2017"
            Case DatabaseServer.SqlServer2019 : Return "SQL Server 2019"
            Case Else
                Return $"SQL Server v{Database}"
        End Select
    End Function

    ''' <summary>
    ''' Tests the validity of the current system database connection setting by
    ''' attempting a sample read and a sample write operation.
    ''' </summary>
    <UnsecuredMethod()>
    Public Sub Validate() Implements IServer.Validate


        If Not mDBConnectionSetting.IsComplete Then
            Throw New BluePrismException(My.Resources.clsServer_ADatabaseConnectionSettingIsBlankPleaseEditTheConnectionSettings)
        End If
        If mDBConnectionSetting.ConnectionType = ConnectionType.BPServer Then
            Throw New BluePrismException(My.Resources.clsServer_ConnectionIsNotADirectDatabaseConnection)
        End If

        Using con = GetConnection()
            Me.Database = con.GetDatabaseVersion()

            Dim iTest As Integer
            'Try reading from known database table
            Try
                Using cmd As New SqlCommand("SELECT populateusernameusing FROM BPASysconfig")
                    Using reader = con.ExecuteReturnDataReader(cmd)
                        If reader.Read() Then
                            iTest = CInt(reader("populateusernameusing"))
                        End If
                    End Using
                End Using
            Catch
                Throw New BluePrismException(My.Resources.clsServer_CouldNotReadFromDatabasePleaseCheckYouHavePermissionToReadFromTheDatabaseOrEdit)
            End Try

            'Try writing to known database table
            Try
                Using cmd As New SqlCommand("UPDATE BPASysconfig SET populateusernameusing=@Test")
                    With cmd.Parameters
                        .AddWithValue("@Test", iTest)
                    End With

                    con.Execute(cmd)
                End Using
            Catch
                Throw New BluePrismException(My.Resources.clsServer_CouldNotWriteToDatabasePleaseCheckYouHavePermissionToWriteToTheDatabaseOrEditTh)
            End Try

        End Using
    End Sub



    ''' <summary>
    ''' This does nothing at all. (But calling it will keep the remote object alive
    ''' in a Blue Prism Server scenario!)
    ''' </summary>
    <UnsecuredMethod()>
    Public Sub Nop() Implements IServer.Nop
    End Sub

    ''' <summary>
    ''' Look up a locale string specific to validation in the current locale, falls back to default if specified or key value
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Private Function GetString(ByVal value As String, Optional ByVal fallback As String = "") As String
        Try
            Return If(LTools.GetOrNull(value, "validation", mLoggedInUserLocale), If(String.IsNullOrEmpty(fallback), value, fallback))
        Catch __exception1__ As Exception
            Return If(String.IsNullOrEmpty(fallback), value, fallback)
        End Try
    End Function

    ''' <summary>
    ''' Get info on all possible Process Validation checks.
    ''' </summary>
    ''' <returns>A dictionary of ValidationInfo objects, with the key being the check
    ''' ID.</returns>
    ''' <remarks>Throws an exception if something goes wrong.</remarks>
    <SecuredMethod(True)>
    Public Function GetValidationInfo() As IEnumerable(Of clsValidationInfo) Implements IServer.GetValidationInfo
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT * FROM BPAValCheck")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim info As New List(Of clsValidationInfo)
                While reader.Read()
                    Dim ni As New clsValidationInfo()
                    ni.Enabled = CBool(reader("enabled"))

                    ni.TypeID = CType(reader("typeid"), clsValidationInfo.Types)
                    ni.CatID = CType(reader("catid"), clsValidationInfo.Categories)
                    ni.CheckID = CInt(reader("checkid"))
                    ni.Message = GetString("check_" + CStr(ni.CheckID), CStr(reader("description"))) ' translated on load here
                    info.Add(ni)
                End While
                Return info
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Sets the validation info values for the given collection of objects.
    ''' </summary>
    ''' <param name="checks">The collection of validation info objects containing
    ''' the settings for the validation checks in this environment.</param>
    <SecuredMethod(True)>
    Public Sub SetValidationInfo(ByVal checks As ICollection(Of clsValidationInfo)) Implements IServer.SetValidationInfo
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SetValidationInfo(con, checks)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Sets the validation info values for the given collection of objects.
    ''' </summary>
    ''' <param name="con">The connection over which the validation info data should
    ''' be updated.</param>
    ''' <param name="checks">The collection of validation info objects containing
    ''' the settings for the validation checks in this environment.</param>
    Private Sub SetValidationInfo(ByVal con As IDatabaseConnection,
     ByVal checks As ICollection(Of clsValidationInfo))

        Dim cmd As New SqlCommand(
         "UPDATE BPAValCheck SET typeid=@typeid, enabled=@enabled WHERE checkid=@checkid")
        Dim typeid As SqlParameter, checkid As SqlParameter, enabled As SqlParameter
        With cmd.Parameters
            typeid = .Add("@typeid", SqlDbType.Int)
            enabled = .Add("@enabled", SqlDbType.Bit)
            checkid = .Add("@checkid", SqlDbType.Int)
        End With
        For Each info As clsValidationInfo In checks
            checkid.Value = info.CheckID
            enabled.Value = info.Enabled
            typeid.Value = info.TypeID
            con.Execute(cmd)
        Next

        AuditRecordSysConfigEvent(con,
         SysConfEventCode.ModifyDesignControl, My.Resources.clsServer_AdvancedSettingsChanged)

    End Sub

    ''' <summary>
    ''' Gets the categories defined for the validation rules
    ''' </summary>
    ''' <returns>Gets a map of validation category labels keyed against their
    ''' database IDs.
    ''' </returns>
    <SecuredMethod(Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls)>
    Public Function GetValidationCategories() As IDictionary(Of Integer, String) Implements IServer.GetValidationCategories
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT * FROM BPAValCategory")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim categories As New SortedDictionary(Of Integer, String)
                While reader.Read
                    categories.Add(CInt(reader("catid")), GetString("cat_" & CStr(reader("description")), CStr(reader("description")))) ' translated on load
                End While

                Return categories
            End Using
        Finally
            con.Close()
        End Try
    End Function

    ''' <summary>
    ''' Gets the types defined for the validation rules - effectively the severity
    ''' levels used in the validation rules.
    ''' </summary>
    ''' <returns>A map of validation severity labels keyed against their database IDs
    ''' </returns>
    <SecuredMethod(Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls,
        Permission.ProcessStudio.CreateProcess, Permission.ObjectStudio.CreateBusinessObject,
        Permission.ProcessStudio.EditProcess, Permission.ObjectStudio.EditBusinessObject,
        Permission.ProcessStudio.ViewProcess, Permission.ObjectStudio.ViewBusinessObject)>
    Public Function GetValidationTypes() As IDictionary(Of Integer, String) Implements IServer.GetValidationTypes
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT * FROM BPAValType")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim types As New SortedDictionary(Of Integer, String)
                While reader.Read
                    types.Add(CInt(reader("typeid")), GetString("type_" & CStr(reader("description")), CStr(reader("description")))) ' translated on load
                End While
                Return types
            End Using
        Finally
            con.Close()
        End Try
    End Function

    ''' <summary>
    ''' Gets the supported actions for validation rules
    ''' </summary>
    ''' <returns>A map of action labels keyed against their database IDs</returns>
    <SecuredMethod(Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls,
        Permission.ProcessStudio.CreateProcess, Permission.ObjectStudio.CreateBusinessObject,
        Permission.ProcessStudio.EditProcess, Permission.ObjectStudio.EditBusinessObject,
        Permission.ProcessStudio.ViewProcess, Permission.ObjectStudio.ViewBusinessObject)>
    Public Function GetValidationActions() As IDictionary(Of Integer, String) Implements IServer.GetValidationActions
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT * FROM BPAValAction")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim actions As New SortedDictionary(Of Integer, String)
                While reader.Read
                    actions.Add(CInt(reader("actionid")), GetString("action_" & CStr(reader("description")), CStr(reader("description")))) ' translated on load
                End While

                Return actions
            End Using
        Finally
            con.Close()
        End Try
    End Function

    ''' <summary>
    ''' Gets a map of actions (IDs) keyed against their severity levels for a given
    ''' category (again, by ID).
    ''' </summary>
    ''' <param name="category">The database ID of the category required.</param>
    ''' <returns>A map of action IDs keyed against the support severity level IDs.
    ''' </returns>
    <SecuredMethod(Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls)>
    Public Function GetValidationActionSettings(ByVal category As Integer) As IDictionary(Of Integer, Integer) Implements IServer.GetValidationActionSettings
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT typeid,actionid FROM BPAValActionMap WHERE catid=@catid")
            cmd.Parameters.AddWithValue("@catid", category)
            Dim reader = con.ExecuteReturnDataReader(cmd)
            Dim actions As New SortedDictionary(Of Integer, Integer)
            While reader.Read
                actions.Add(CInt(reader("typeid")), CInt(reader("actionid")))
            End While

            Return actions
        Finally
            con.Close()
        End Try
    End Function

    <SecuredMethod(Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls,
        Permission.ProcessStudio.CreateProcess, Permission.ObjectStudio.CreateBusinessObject,
        Permission.ProcessStudio.EditProcess, Permission.ObjectStudio.EditBusinessObject,
        Permission.ProcessStudio.ViewProcess, Permission.ObjectStudio.ViewBusinessObject)>
    Public Function GetValidationAllActionSettings() As IDictionary(Of Integer, IDictionary(Of Integer, Integer)) Implements IServer.GetValidationAllActionSettings
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT * FROM BPAValActionMap")
            Dim reader = con.ExecuteReturnDataReader(cmd)
            Dim allActions As New Dictionary(Of Integer, IDictionary(Of Integer, Integer))
            Dim actions As IDictionary(Of Integer, Integer) = Nothing
            While reader.Read
                Dim catid As Integer = CInt(reader("catid"))
                If Not allActions.TryGetValue(catid, actions) Then
                    actions = New SortedDictionary(Of Integer, Integer)
                    allActions.Add(catid, actions)
                End If
                actions.Add(CInt(reader("typeid")), CInt(reader("actionid")))

            End While

            Return allActions
        Finally
            con.Close()
        End Try
    End Function

    <SecuredMethod(Permission.SystemManager.Audit.ConfigureDesignControls)>
    Public Sub SetValidationActionSetting(ByVal catid As Integer, ByVal typeid As Integer, ByVal actionid As Integer) Implements IServer.SetValidationActionSetting
        CheckPermissions()
        Dim con = GetConnection()
        Try
            con.BeginTransaction()

            Dim cmd As New SqlCommand("UPDATE BPAValActionMap Set actionid=@actionid WHERE catid=@catid AND typeid=@typeid")
            With cmd.Parameters
                .AddWithValue("@actionid", actionid)
                .AddWithValue("@catid", catid)
                .AddWithValue("@typeid", typeid)
            End With
            con.Execute(cmd)

            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyDesignControl, My.Resources.clsServer_DesignControlActionChanged)

            con.CommitTransaction()
        Catch
            con.RollbackTransaction()
        Finally
            con.Close()
        End Try
    End Sub

    ''' <summary>
    ''' Set the Resource Registration Mode.
    ''' </summary>
    ''' <param name="mode">The new mode.
    ''' </param>
    ''' <exception cref="ArgumentOutOfRangeException">If the mode is invalid.</exception>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetResourceRegistrationMode(ByVal mode As ResourceRegistrationMode) Implements IServer.SetResourceRegistrationMode
        CheckPermissions()
        If mode < 0 OrElse mode > 2 Then Throw New ArgumentOutOfRangeException(
NameOf(mode), mode, My.Resources.clsServer_TheRegistrationModeMustBeBetween0And2)

        Using con = GetConnection()
            Dim cmd As New SqlCommand("UPDATE BPASysconfig set ResourceRegistrationMode = @mode")
            cmd.Parameters.AddWithValue("@mode", CInt(mode))
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyResourceRegistrationMode, String.Format(My.Resources.clsServer_ResourceRegistrationModeChangedTo0, mode.ToString()))
        End Using
    End Sub

    ''' <summary>
    ''' Get the Resource Registration Mode.
    ''' </summary>
    ''' <returns>The current mode.</returns>
    <UnsecuredMethod()>
    Public Function GetResourceRegistrationMode() As ResourceRegistrationMode Implements IServer.GetResourceRegistrationMode
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT ResourceRegistrationMode FROM BPASysconfig")
            Return CType(con.ExecuteReturnScalar(cmd), ResourceRegistrationMode)
        End Using
    End Function

    ''' <summary>
    ''' Get the address and port to use to connect to the given Resource.
    ''' </summary>
    ''' <param name="resourceName">The name of the Resource.</param>
    ''' <param name="hostname">On return, the hostname to connect to.</param>
    ''' <param name="portNo">On return, the port to connect to.</param>
    ''' <param name="ssl">On return, True if ssl is used.</param>
    <SecuredMethod(True)>
    Public Sub GetResourceAddress(ByVal resourceName As String, ByRef hostname As String,
                                     ByRef portNo As Integer, ByRef ssl As Boolean,
                                     ByRef requiresSecure As Boolean) Implements IServer.GetResourceAddress
        CheckPermissions()
        Using con = GetConnection()
            'Determine actual host and port name from the resource name...
            Dim index As Integer = resourceName.IndexOf(":")
            If index = -1 Then
                hostname = resourceName
                portNo = Resources.ResourceMachine.DefaultPort
            Else
                hostname = resourceName.Left(index)
                portNo = CInt(resourceName.Mid(index + 2))
            End If

            Dim mode As ResourceRegistrationMode
            Dim fqdn As Object = DBNull.Value

            'But if we're supposed to connect via FQDN, use that for the hostname instead...
            Using cmd As New SqlCommand("select ResourceRegistrationMode, RequireSecuredResourceConnections from BPASysconfig")
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        mode = CType(prov.GetValue("ResourceRegistrationMode", 0), ResourceRegistrationMode)
                        requiresSecure = prov.GetValue("RequireSecuredResourceConnections", False)
                    End While
                End Using
            End Using

            Using cmd As New SqlCommand("select FQDN, ssl FROM BPAResource where name = @resourcename")
                cmd.Parameters.AddWithValue("@resourcename", resourceName)
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        fqdn = prov.Item("FQDN")
                        ssl = prov.GetInt("ssl", 0) <> 0
                    End While
                End Using
            End Using

            If mode <> ResourceRegistrationMode.MachineMachine Then
                If Convert.IsDBNull(fqdn) Then
                    Throw New InvalidOperationException(String.Format("Resource {0} has no FQDN", resourceName))
                End If
                hostname = CStr(fqdn)
            End If

        End Using
    End Sub

    ''' <summary>
    ''' Gets the system preference in the database enforcing edit summaries
    ''' in process studio.
    ''' </summary>
    ''' <returns>True if successful</returns>
    <SecuredMethod()>
    Public Function GetEnforceEditSummariesSetting() As Boolean Implements IServer.GetEnforceEditSummariesSetting
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT EnforceEditSummaries FROM BPASysConfig")
            Return CBool(con.ExecuteReturnScalar(cmd))
        End Using
    End Function

    ''' <summary>
    ''' Gets the system preference in the database that determines whether process
    ''' XML should be compressed.
    ''' </summary>
    ''' <returns>True if successful</returns>
    <SecuredMethod(True)>
    Public Function GetCompressProcessXMLSetting() As Boolean Implements IServer.GetCompressProcessXMLSetting
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT CompressProcessXML FROM BPASysConfig")
            Return CBool(con.ExecuteReturnScalar(cmd))
        End Using
    End Function

    ''' <summary>
    ''' Gets the system preference for prevention of Resource registrations.
    ''' </summary>
    ''' <returns>The current value</returns>
    <SecuredMethod()>
    Public Function GetPreventResourceRegistrationSetting() As Boolean Implements IServer.GetPreventResourceRegistrationSetting
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT PreventResourceRegistration FROM BPASysConfig")
            Dim value As Integer = CInt(con.ExecuteReturnScalar(cmd))
            Return value <> 0
        End Using
    End Function

    ''' <summary>
    ''' Sets the system preference for prevention of Resource registrations.
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetPreventResourceRegistrationSetting(value As Boolean) Implements IServer.SetPreventResourceRegistrationSetting
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("UPDATE BPASysConfig SET PreventResourceRegistration = @value")
            With cmd.Parameters
                .AddWithValue("@value", IIf(value, 1, 0))
            End With
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyPreventResourceRegistration, String.Format(My.Resources.clsServer_SettingChangedTo0, value.ToString()))
        End Using
    End Sub

    <SecuredMethod()>
    Public Function GetControllingUserPermissionSetting() As Boolean Implements IServer.GetControllingUserPermissionSetting
        CheckPermissions()
        Using connection = GetConnection()
            Return GetControllingUserPermissionSetting(connection)
        End Using
    End Function

    Private Function GetControllingUserPermissionSetting(connection As IDatabaseConnection) As Boolean
        Try
            Return GetSystemPref(connection, "enforce.controlling.permission", True)
        Catch ex As Exception
            Return False
        End Try
    End Function

    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetControllingUserPermissionSetting(value As Boolean) _
        Implements IServer.SetControllingUserPermissionSetting
        CheckPermissions()
        Using connection = GetConnection()
            SetSystemPref(connection, "enforce.controlling.permission", value)
            AuditRecordSysConfigEvent(connection, SysConfEventCode.ModifyControllingUserPermissionSetting,
                                      String.Format(My.Resources.clsServer_SettingChangedTo0, value))
        End Using
    End Sub

    <SecuredMethod()>
    Public Sub AuditStartProcEngineSettingChange(value As Boolean) _
     Implements IServer.AuditStartProcEngineSettingChange
        CheckPermissions()
        Using connection = GetConnection()
            AuditRecordSysConfigEvent(connection,
                                      SysConfEventCode.ModifyStartProcEngine,
                                      String.Format(My.Resources.SettingChangedTo0, value))
        End Using
    End Sub

    <SecuredMethod()>
    Public Sub AuditStartDatabaseConversion() _
     Implements IServer.AuditStartDatabaseConversion
        CheckPermissions()
        Using connection = GetConnection()
            AuditRecordSysConfigEvent(connection, SysConfEventCode.ConvertDatabaseToMultiAuth, Nothing)
        End Using
    End Sub

    ''' <summary>
    ''' Gets the system preference for requirement of secured Resource connections.
    ''' </summary>
    ''' <returns>The current value</returns>
    <SecuredMethod(True)>
    Public Function GetRequireSecuredResourceConnections() As Boolean Implements IServer.GetRequireSecuredResourceConnections
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT RequireSecuredResourceConnections FROM BPASysConfig")
            Dim value As Integer = CInt(con.ExecuteReturnScalar(cmd))
            Return value <> 0
        End Using
    End Function


    ''' <summary>
    ''' Sets the system preference for requirement of secured Resource connections.
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetRequireSecuredResourceConnections(value As Boolean) Implements IServer.SetRequireSecuredResourceConnections
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("UPDATE BPASysConfig SET RequireSecuredResourceConnections = @value")
            With cmd.Parameters
                .AddWithValue("@value", IIf(value, 1, 0))
            End With
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyRequireSecuredResourceConnections, String.Format(My.Resources.clsServer_SettingChangedTo0, value.ToString()))
        End Using
    End Sub

    ''' <summary>
    ''' Gets the system preference for whether anonymous resource pc connections are
    ''' allowed
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Function GetAllowAnonymousResources() As Boolean Implements IServer.GetAllowAnonymousResources
        CheckPermissions()
        Using con = GetConnection()
            Return GetSystemPref(con, "allow.anon.resource", True)
        End Using
    End Function

    ''' <summary>
    ''' Sets the system preference for whether anonymous resource pc connections are
    ''' allowed
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetAllowAnonymousResources(value As Boolean) Implements IServer.SetAllowAnonymousResources
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(con, "allow.anon.resource", value)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyAllowAnonResources, String.Format(My.Resources.clsServer_SettingChangedTo0, value.ToString()))
        End Using
    End Sub


    <SecuredMethod()>
    Public Function GetTesseractEngine() As Integer Implements IServer.GetTesseractEngine
        CheckPermissions()
        Using con = GetConnection()
            Return GetSystemPref(con, ApplicationManager.Tesseract, 3)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetTesseractEngine(value As Integer) Implements IServer.SetTesseractEngine
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(con, ApplicationManager.Tesseract, value)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySystemSetting, String.Format(My.Resources.clsServer_TesseractEngineSettingChangedTo0, value.ToString()))
        End Using
    End Sub

    ''' <summary>
    ''' Sets the system preference for whether pasing of passwords is allowed
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetAllowPasswordPasting(value As Boolean) Implements IServer.SetAllowPasswordPasting
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(con, "allow.password.pasting", value)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySignonSettings,
             If(value, My.Resources.clsServer_PasswordPastingAllowed, My.Resources.clsServer_PasswordPastingDisallowed))
        End Using
    End Sub

    ''' <summary>
    ''' Gets the system preference for whether pasing of passwords is allowed
    ''' </summary>
    <UnsecuredMethod>
    Public Function GetAllowPasswordPasting() As Boolean Implements IServer.GetAllowPasswordPasting
        Using con = GetConnection()
            Return GetSystemPref(con, "allow.password.pasting", True)
        End Using
    End Function

    ''' <summary>
    ''' Sets the system preference for whether resource screenshots are allowed
    ''' </summary>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetAllowResourceScreenshot(value As Boolean) Implements IServer.SetAllowResourceScreenshot
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(con, "allow.resource.screenshot", value)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySystemSetting,
             If(value, My.Resources.clsServer_ExceptionScreenshotsAllowed, My.Resources.clsServer_ExceptionScreenshotsDisallowed))
        End Using
    End Sub

    ''' <summary>
    ''' Gets the system preference for whether resource screenshots are allowed
    ''' </summary>
    <SecuredMethod(True)>
    Public Function GetAllowResourceScreenshot() As Boolean Implements IServer.GetAllowResourceScreenshot
        CheckPermissions()
        Using con = GetConnection()
            Return GetSystemPref(con, "allow.resource.screenshot", False)
        End Using
    End Function

    ''' <summary>
    ''' Sets the system preference in the database enforcing edit summaries
    ''' in process studio.
    ''' </summary>
    ''' <param name="value">Set to true to enforce the use of summaries;
    ''' false otherwise.</param>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetEnforceEditSummariesSetting(ByVal value As Boolean) Implements IServer.SetEnforceEditSummariesSetting
        CheckPermissions()
        Dim i As Integer = 0
        If value = True Then i = 1
        Using con = GetConnection()

            Dim cmd As New SqlCommand("UPDATE BPASysConfig SET EnforceEditSummaries = @EnforceEditSummaries")
            With cmd.Parameters
                .AddWithValue("@EnforceEditSummaries", i)
            End With

            con.Execute(cmd)

            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyForceSummaryOnSave, String.Empty, value.ToString())
        End Using
    End Sub

    ''' <summary>
    ''' Sets the system preference for when stages should be deemed as overdue and
    ''' show as 'warning'
    ''' </summary>
    ''' <param name="seconds">The new default stage warning threshold in seconds
    ''' </param>
    <SecuredMethod(Permission.SystemManager.System.Settings)>
    Public Sub SetStageWarningThreshold(seconds As Integer) _
        Implements IServer.SetStageWarningThreshold
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(Of Integer)(con, PreferenceNames.SystemSettings.DefaultStageWarningThreshold, seconds)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySystemSetting,
             String.Format(My.Resources.clsServer_DefaultStageWarningThresholdSetTo0Seconds, seconds))
        End Using
    End Sub

    ''' <summary>
    ''' Gets the system preference for when stages should be deemed as overdue and
    ''' show as 'warning'
    ''' </summary>
    <SecuredMethod(True)>
    Public Function GetStageWarningThreshold() As Integer _
        Implements IServer.GetStageWarningThreshold
        CheckPermissions()
        Using con = GetConnection()
            Return GetSystemPref(con, PreferenceNames.SystemSettings.DefaultStageWarningThreshold, 300)
        End Using
    End Function

    ''' <summary>
    ''' Gets the system preference for when stages should be deemed as overdue and
    ''' show as 'warning'
    ''' </summary>
    ''' <param name="con">The database connection</param>
    Private Function GetStageWarningThreshold(con As IDatabaseConnection) As Integer
        Return GetSystemPref(con, PreferenceNames.SystemSettings.DefaultStageWarningThreshold, 300)
    End Function


    <UnsecuredMethod>
    Public Function GetCertificateExpThreshold() As Integer Implements IServer.GetCertificateExpThreshold
        Using con = GetConnection()
            Const DefaultCertificateExpThresholdInDays = 7
            Return GetSystemPref(con, PreferenceNames.SystemSettings.CertificateExpThreshold, DefaultCertificateExpThresholdInDays)
        End Using
    End Function


    ''' <inheritdoc />
    <UnsecuredMethod>
    Public Function GetWebConnectionSettings() As WebConnectionSettings _
        Implements IServer.GetWebConnectionSettings

        Using con = GetConnection()
            Dim maxIdleTime As Integer
            Dim connectionLimit As Integer
            Dim connectionLeaseTimeout As Integer?
            Dim cmd As New SqlCommand("select top 1 maxidletime, connectionlimit, connectiontimeout from BPASysWebConnectionSettings")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    maxIdleTime = prov.GetValue("maxidletime", 0)
                    connectionLimit = prov.GetValue("connectionlimit", 0)
                    connectionLeaseTimeout = prov.GetValue(Of Integer?)("connectiontimeout", Nothing)
                End While
            End Using

            Dim settings = GetUriWebConnectionSettings(con)

            Return New WebConnectionSettings(connectionLimit, maxIdleTime, connectionLeaseTimeout, settings)
        End Using
    End Function

    '''<inheritdoc/>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebConnectionSettings)>
    Public Sub UpdateWebConnectionSettings(updatedSettings As WebConnectionSettings) _
        Implements IServer.UpdateWebConnectionSettings

        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            Dim originalSettings = GetWebConnectionSettings()
            UpdateWebConnectionSettings(con, updatedSettings)

            If updatedSettings.ConnectionLimit <> originalSettings.ConnectionLimit Then _
                AuditRecordWebSettingsEvent(con,
                    WebSettingsEventCode.ModifyDefaultSettings,
                    String.Format(My.Resources.clsServer_ConnectionLimitChangedFrom0To1, originalSettings.ConnectionLimit, updatedSettings.ConnectionLimit))

            If updatedSettings.MaxIdleTime <> originalSettings.MaxIdleTime Then _
               AuditRecordWebSettingsEvent(con,
                   WebSettingsEventCode.ModifyDefaultSettings,
                   String.Format(My.Resources.clsServer_MaxIdleTimeChangedFrom0To1, originalSettings.MaxIdleTime, updatedSettings.MaxIdleTime))

            If updatedSettings.ConnectionLeaseTimeout <> originalSettings.ConnectionLeaseTimeout Then _
               AuditRecordWebSettingsEvent(con,
                   WebSettingsEventCode.ModifyDefaultSettings,
                   String.Format(My.Resources.ConnectionLeaseTimeoutChangedFrom0To1, originalSettings.ConnectionLeaseTimeout, updatedSettings.ConnectionLeaseTimeout))

            UpdateUriWebSettings(con, originalSettings.UriSpecificSettings, updatedSettings.UriSpecificSettings)

            con.CommitTransaction()
        End Using
    End Sub

    <UnsecuredMethod()>
    Public Function GetAuthenticationGatewayUrl() As String Implements IServer.GetAuthenticationGatewayUrl
        Using con = GetConnection()
            Return GetLogonOptions(con).AuthenticationGatewayUrl.ToLower()
        End Using
    End Function


    <UnsecuredMethod()>
    Public Function IsAuthenticationServerIntegrationEnabled() As Boolean Implements IServer.IsAuthenticationServerIntegrationEnabled
        Using con = GetConnection()
            Return GetLogonOptions(con).AuthenticationServerAuthenticationEnabled
        End Using
    End Function

    <UnsecuredMethod()>
    Public Function GetAuthenticationServerUrl() As String Implements IServer.GetAuthenticationServerUrl
        Using con = GetConnection()
            Dim authServerUrl = GetLogonOptions(con).AuthenticationServerUrl
            If authServerUrl Is Nothing Then _
                Throw New AuthenticationServerNotConfiguredException("Authentication server has not been configured")
            Return authServerUrl.ToLower()
        End Using
    End Function

    Private Sub UpdateUriWebSettings(con As IDatabaseConnection,
                                     originalUriSettings As IEnumerable(Of UriWebConnectionSettings),
                                     newUriSettings As IEnumerable(Of UriWebConnectionSettings))

        Dim originalUris = originalUriSettings.Select(Function(s) s.BaseUri)
        Dim updatedUris = newUriSettings.Select(Function(s) s.BaseUri)
        Dim settingsToDelete = originalUris.Where(Function(s) Not updatedUris.Contains(s)).ToList()
        Dim settingsToAdd = newUriSettings.Where(Function(s) Not originalUris.Contains(s.BaseUri)).ToList()
        Dim urisToAdd = settingsToAdd.Select(Function(s) s.BaseUri)
        Dim settingsToUpdate = newUriSettings.Where(Function(s) Not urisToAdd.Contains(s.BaseUri)).ToList()

        DeleteUriWebConnectionSettings(con, settingsToDelete)
        AddUriWebConnectionSettings(con, settingsToAdd)
        UpdateUriWebConnectionSettings(con, settingsToUpdate)
    End Sub


    Private Sub UpdateWebConnectionSettings(connection As IDatabaseConnection, settings As WebConnectionSettings)

        Using cmd As New SqlCommand("update BPASysWebConnectionSettings
                                        set maxidletime = @maxidletime,
                                            connectionlimit = @connectionlimit,
                                            connectiontimeout = @connectiontimeout")
            With cmd.Parameters
                .AddWithValue("@maxidletime", settings.MaxIdleTime)
                .AddWithValue("@connectionlimit", settings.ConnectionLimit)
                .AddWithValue("@connectiontimeout", IIf(settings.ConnectionLeaseTimeout IsNot Nothing, settings.ConnectionLeaseTimeout, DBNull.Value))
            End With

            connection.Execute(cmd)
        End Using

    End Sub

    Private Function GetUriWebConnectionSettings(con As IDatabaseConnection) As IEnumerable(Of UriWebConnectionSettings)

        Dim cmd As New SqlCommand("select baseuri, connectionlimit, connectiontimeout, maxidletime
                                        from BPASysWebUrlSettings")

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim provider As New ReaderDataProvider(reader)
            Dim settings = New List(Of UriWebConnectionSettings)

            While reader.Read()
                settings.Add(New UriWebConnectionSettings(
                                    provider.GetString("baseuri"),
                                    provider.GetInt("connectionlimit"),
                                    provider.GetValue(Of Integer?)("connectiontimeout", Nothing),
                                    provider.GetInt("maxidletime")))
            End While
            Return settings
        End Using
    End Function

    Private Sub AddUriWebConnectionSettings(con As IDatabaseConnection, settings As IEnumerable(Of UriWebConnectionSettings))

        For Each item In settings
            Dim cmd As New SqlCommand("insert into BPASysWebUrlSettings (baseuri, connectionlimit, connectiontimeout, maxidletime)
                                            values (@uri, @connectionLimit, @connectionTimeout, @maxIdleTime)")
            cmd.Parameters.Add(New SqlParameter("@uri", item.BaseUri.ToString))
            cmd.Parameters.Add(New SqlParameter("@connectionLimit", item.ConnectionLimit))

            If item.ConnectionLeaseTimeout Is Nothing Then
                cmd.Parameters.Add(New SqlParameter("@connectionTimeout", DBNull.Value))
            Else
                cmd.Parameters.Add(New SqlParameter("@connectionTimeout", item.ConnectionLeaseTimeout))
            End If

            cmd.Parameters.Add(New SqlParameter("@maxIdleTime", item.MaxIdleTime))
            con.Execute(cmd)

            Dim baseURI = String.Format(My.Resources.clsServer_TheSettingsForTheUri0HaveBeenCreated, item.BaseUri)
            AuditRecordWebSettingsEvent(con,
                     WebSettingsEventCode.AddUriConfiguration,
                     $"{baseURI}
                         ConnectionLimit: {item.ConnectionLimit},
                         ConnectionTimeout: {item.ConnectionLeaseTimeout},
                         MadIdleTime { item.MaxIdleTime}")
        Next
    End Sub

    Private Sub UpdateUriWebConnectionSettings(con As IDatabaseConnection, settings As IEnumerable(Of UriWebConnectionSettings))

        Dim originals = GetUriWebConnectionSettings(con).ToList()

        For Each item In settings
            Dim original = originals.FirstOrDefault(Function(o) o.BaseUri = item.BaseUri)

            If original.ConnectionLimit <> item.ConnectionLimit Then
                UpdateUriWebConnectionLimitSetting(con, item, original.ConnectionLimit)
            End If

            If Not Nullable.Equals(original.ConnectionLeaseTimeout, item.ConnectionLeaseTimeout) Then
                UpdateUriWebConnectionTimeoutSetting(con, item, original.ConnectionLeaseTimeout)
            End If

            If original.MaxIdleTime <> item.MaxIdleTime Then
                UpdateUriWebMaxIdleTimeSetting(con, item, original.MaxIdleTime)
            End If
        Next
    End Sub

    Private Sub UpdateUriWebConnectionLimitSetting(con As IDatabaseConnection, setting As UriWebConnectionSettings, originalValue As Integer)
        Dim uri = setting.BaseUri
        Dim limit = setting.ConnectionLimit
        Dim message = String.Format(My.Resources.clsServer_TheConnectionLimitSettingForTheUri0HasBeenChangedFrom1ToLimit, uri, originalValue)
        UpdateUriWebConnectionSetting(con, uri, "connectionlimit", limit, message)
    End Sub

    Private Sub UpdateUriWebConnectionTimeoutSetting(con As IDatabaseConnection, setting As UriWebConnectionSettings, originalValue As Integer?)
        Dim uri = setting.BaseUri
        Dim timeout = setting.ConnectionLeaseTimeout
        Dim message = String.Format(My.Resources.clsServer_TheConnectionLeaseTimeoutSettingForTheUri0HasBeenChangedFrom1To2, uri, originalValue, timeout)
        UpdateUriWebConnectionSetting(con, uri, "connectiontimeout", timeout, message)
    End Sub

    Private Sub UpdateUriWebMaxIdleTimeSetting(con As IDatabaseConnection, setting As UriWebConnectionSettings, originalValue As Integer)
        Dim uri = setting.BaseUri
        Dim idleTime = setting.MaxIdleTime
        Dim message = String.Format(My.Resources.clsServer_TheMaxIdleTimeSettingForTheUri0HasBeenChangedFrom1To2, uri, originalValue, idleTime)
        UpdateUriWebConnectionSetting(con, uri, "maxidletime", idleTime, message)
    End Sub

    Private Sub UpdateUriWebConnectionSetting(con As IDatabaseConnection,
                                              uri As Uri,
                                              fieldName As String,
                                              value As Integer?,
                                              auditMessage As String)
        Dim cmd As New SqlCommand($"update BPASysWebUrlSettings
                                                set {fieldName} = @value
                                                where baseuri = @uri")
        With cmd.Parameters
            .AddWithValue("@uri", uri.ToString())
            .AddWithValue("@fieldName", fieldName)
            If value Is Nothing Then
                .AddWithValue("@value", DBNull.Value)
            Else
                .AddWithValue("@value", value)
            End If
        End With

        con.Execute(cmd)

        AuditRecordWebSettingsEvent(con, WebSettingsEventCode.UpdateUriConfiguration, auditMessage)

    End Sub

    Private Sub DeleteUriWebConnectionSettings(con As IDatabaseConnection, uris As IEnumerable(Of Uri))
        Dim toBeDeleted = GetUriWebConnectionSettings(con).Where(Function(s) uris.Contains(s.BaseUri))

        For Each item In toBeDeleted
            Dim cmd As New SqlCommand("delete
                                        from BPASysWebUrlSettings
                                        where baseuri = @uri")
            cmd.Parameters.Add(New SqlParameter("@uri", item.BaseUri.ToString))
            con.Execute(cmd)

            AuditRecordWebSettingsEvent(con,
                                            WebSettingsEventCode.DeleteUriConfiguration,
                                            String.Format(My.Resources.clsServer_TheSettingsForTheUri0HaveBeenDeleted, item.BaseUri))
        Next

    End Sub

#Region " ToolStrip Positions "

    ''' <summary>
    ''' Gets the named toolstrip position for the current user in the specified mode.
    ''' </summary>
    ''' <param name="name">The name of the toolstrip for which the position is
    ''' required. Null or an empty string indicates that all toolstrips' positions
    ''' should be returned</param>
    ''' <param name="objectStudio">True to return the toolstrip positions saved for
    ''' this user within object studio, False to return the process studio
    ''' equivalents</param>
    ''' <returns>The collection of UI Element positions stored for the logged in user
    ''' in the specified mode</returns>
    Private Function GetToolStripPositions(ByVal name As String, ByVal objectStudio As Boolean) _
     As ICollection(Of clsUIElementPosition)

        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             " select p.name, p.position, p.x, p.y, p.visible" &
             " from BPAToolPosition p" &
             " where p.userid = @userid and p.mode = @mode" &
             " order by p.y, p.x"
            )
            If name <> "" Then cmd.CommandText &= " and p.name = @name"

            With cmd.Parameters
                .AddWithValue("@mode", IIf(objectStudio, "o", "p"))
                .AddWithValue("@userid", GetLoggedInUserId())
                If name <> "" Then .AddWithValue("@name", name)
            End With

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                Dim posns As New List(Of clsUIElementPosition)
                While reader.Read()
                    posns.Add(New clsUIElementPosition(prov))
                End While
                Return posns
            End Using

        End Using

    End Function

    ''' <summary>
    ''' Gets all the toolstrip positions for the current user in the specified mode.
    ''' </summary>
    ''' <param name="objectStudio">True to return the toolstrip positions saved for
    ''' this user within object studio, False to return the process studio
    ''' equivalents</param>
    ''' <returns>The collection of UI Element positions stored for the logged in user
    ''' in the specified mode</returns>
    <SecuredMethod(True)>
    Public Function GetToolStripPositions(ByVal objectStudio As Boolean) _
     As ICollection(Of clsUIElementPosition) Implements IServer.GetToolStripPositions
        CheckPermissions()
        Return GetToolStripPositions(Nothing, objectStudio)
    End Function

    ''' <summary>
    ''' Sets the toolstrip positions for the current user from the given collection.
    ''' </summary>
    ''' <param name="posns">The collection of position details to save</param>
    ''' <param name="objectStudio">True to save the toolstrip positions for object
    ''' studio mode, rather than process studio mode</param>
    <SecuredMethod(True)>
    Public Sub SetToolStripPositions(
     ByVal posns As ICollection(Of clsUIElementPosition), ByVal objectStudio As Boolean) Implements IServer.SetToolStripPositions
        CheckPermissions()

        Using con = GetConnection()
            con.BeginTransaction()

            Dim cmd As New SqlCommand(
             " delete from BPAToolPosition " &
             "   where name = @name and userid = @userid and mode = @mode" & _
 _
             " insert into BPAToolPosition " &
             "        (userid, name, x, y, visible, position, mode) " &
             " values (@userid,@name,@x,@y,@visible,@position,@mode)"
            )

            With cmd.Parameters
                .Add("@userid", SqlDbType.UniqueIdentifier)
                .Add("@name", SqlDbType.NVarChar)
                .Add("@mode", SqlDbType.Char)
                .Add("@position", SqlDbType.Char)
                .Add("@x", SqlDbType.Int)
                .Add("@y", SqlDbType.Int)
                .Add("@visible", SqlDbType.Bit)
            End With

            For Each posn As clsUIElementPosition In posns
                With cmd.Parameters
                    .Item("@userid").Value = GetLoggedInUserId()
                    .Item("@name").Value = posn.Name
                    .Item("@mode").Value = IIf(objectStudio, "o", "p")
                    .Item("@position").Value = posn.DockChar
                    .Item("@x").Value = posn.X
                    .Item("@y").Value = posn.Y
                    .Item("@visible").Value = posn.Visible
                End With
                con.Execute(cmd)
            Next
            con.CommitTransaction()
        End Using

    End Sub

#End Region

    ''' <summary>
    ''' Function to check if versioned data has been updated or not.
    ''' This is handled by the version number held on the data tracking table,
    ''' which must be incremented by any function which updates the data managed
    ''' by the record in question.
    ''' </summary>
    ''' <param name="dataName">The name of the data registered on the version table.
    ''' </param>
    ''' <param name="currentVersion">The current version held in the system. This
    ''' will be updated as a ref parameter if it differs from the latest version
    ''' registered on the database.</param>
    ''' <returns>True if the data has been updated, ie. if the version number on the
    ''' database differs from the one specified; False otherwise.</returns>
    ''' <exception cref="ArgumentException">If the given data name was not found in
    ''' the data version table.</exception>
    <SecuredMethod(True)>
    Public Function HasDataUpdated(ByVal dataName As String, ByRef currentVersion As Long) As Boolean Implements IServer.HasDataUpdated
        CheckPermissions()
        Using con = GetConnection()
            Dim latestVersion As Long = GetDataVersion(con, dataName)
            If currentVersion = latestVersion Then Return False ' Same number = no change
            ' Else set what the current version is and indicate a data update
            currentVersion = latestVersion
            Return True
        End Using
    End Function

    ''' <summary>
    ''' Gets the updated data versions for a given map of data names.
    ''' </summary>
    ''' <param name="currVers">The current versions held against the datanames that
    ''' they are versions of. Only these will be queried in the version query.
    ''' </param>
    ''' <returns>A map of the data names whose versions differ from those passed in,
    ''' along with the new version numbers assigned to those data names.;
    ''' Note that any which still have the same version number registered against
    ''' them on the database are not returned in this method.</returns>
    <SecuredMethod(True)>
    Public Function GetUpdatedDataVersions(
     currVers As IDictionary(Of String, Long)) As IDictionary(Of String, Long) Implements IServer.GetUpdatedDataVersions
        CheckPermissions()
        ' Deal with nothing before we bother getting a connection
        If currVers Is Nothing OrElse currVers.Count = 0 Then _
         Return GetEmpty.IDictionary(Of String, Long)()

        ' Load the data versions specified: For each one check if the caller has the
        ' latest version, storing the current version if they have not.
        Using con = GetConnection()
            Dim vers As New Dictionary(Of String, Long)
            For Each ver As KeyValuePair(Of String, Long) _
             In GetDataVersions(con, currVers.Keys)
                ' If the 'current version' for this data name matches the one on the
                ' database, don't add it to the 'updated versions' map.
                Dim curr As Long = 0
                If currVers.TryGetValue(ver.Key, curr) AndAlso curr = ver.Value _
                 Then Continue For

                ' Either the name wasn't there (?) or the version number has changed;
                ' either way, add it to the 'updated versions' map for return
                vers(ver.Key) = ver.Value
            Next
            Return vers
        End Using
    End Function

    ''' <summary>
    ''' Gets the current data versions for all monitored data in the database.
    ''' </summary>
    ''' <returns>A mapping of version numbers against data names as currently held in
    ''' the database.</returns>
    <UnsecuredMethod()>
    Public Function GetCurrentDataVersions() As IDictionary(Of String, Long) _
     Implements IServer.GetCurrentDataVersions
        Using con = GetConnection()
            Return GetCurrentDataVersions(con)
        End Using
    End Function

    ''' <summary>
    ''' Gets the current data versions for all monitored data in the database.
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the data versions.
    ''' </param>
    ''' <returns>A mapping of version numbers against data names as currently held in
    ''' the database.</returns>
    Private Function GetCurrentDataVersions(con As IDatabaseConnection) _
     As IDictionary(Of String, Long)
        Dim map As New Dictionary(Of String, Long)
        Dim cmd As New SqlCommand("select dataname, versionno from BPADataTracker")
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                map(prov.GetString("dataname")) = prov.GetValue("versionno", 0L)
            End While
        End Using
        Return map
    End Function

    ''' <summary>
    ''' Gets the current version number for the specified versioned data in the
    ''' database.
    ''' </summary>
    ''' <param name="con">The connection from which the version number should be
    ''' retrieved</param>
    ''' <param name="dataName">The name of the data registered on the version table.
    ''' </param>
    ''' <returns>The current version number held on the system for the specified
    ''' data name.</returns>
    ''' <exception cref="NoSuchElementException">If the given data name was not found
    ''' in the data version table.</exception>
    Private Function GetDataVersion(con As IDatabaseConnection, dataName As String) As Long
        Dim map As IDictionary(Of String, Long) = GetDataVersions(con, {dataName})
        If map.Count = 0 Then Throw New NoSuchElementException(
         "The data name '{0}' was not found on the database", dataName)
        Return map(dataName)
    End Function

    ''' <summary>
    ''' Gets the data versions for a collection of data names
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="dataNames">The datanames that </param>
    ''' <returns></returns>
    Private Function GetDataVersions(
     con As IDatabaseConnection, dataNames As ICollection(Of String)) _
     As IDictionary(Of String, Long)
        ' We're not really using SelectMultipleIds the way it was intended here,
        ' but it'll do until a more lambda-based implementation can be used.
        Dim map As New Dictionary(Of String, Long)
        mSqlHelper.SelectMultipleIds(con, dataNames,
         Sub(prov) map(prov.GetString("dataname")) = prov.GetValue("versionno", 0L),
         " select dataname, versionno" &
         " from BPADataTracker" &
         " where dataname in ({multiple-ids})"
        )
        Return map
    End Function

    ''' <summary>
    ''' Increments the version number associated with the given data name.
    ''' </summary>
    ''' <param name="con">The connection over which the version number should be
    ''' incremented.</param>
    ''' <param name="dataName">The name of the data whose version number should be
    ''' incremented</param>
    ''' <returns>The new version number held against the specified name.</returns>
    ''' <remarks>If the data name given does not exist on the database, it is created
    ''' by calling this method.</remarks>
    <SecuredMethod(True)>
    Public Function IncrementDataVersion(
     ByVal con As IDatabaseConnection, ByVal dataName As String) As Long Implements IServerPrivate.IncrementDataVersion
        CheckPermissions()

        Dim cmd As New SqlCommand(
         " if not exists (select 1 from BPADataTracker where dataname=@dname)" &
         "   insert into BPADataTracker (dataname,versionno) values(@dname, 0)" &
         " update BPADataTracker set versionno = versionno + 1 " &
         " where dataname = @dname; " &
         " select versionno from BPADataTracker where dataname = @dname"
        )
        cmd.Parameters.AddWithValue("@dname", dataName)

        Return IfNull(con.ExecuteReturnScalar(cmd), 0L)

    End Function

    ''' <summary>
    ''' Updates multiple IDs on the database, taking into account the maximum number
    ''' of parameters that can be sent over the ADO.NET interface / handled by SQL
    ''' Server.
    ''' </summary>
    ''' <typeparam name="T">The type of the ID to update - this can be inferred from
    ''' the type of the <paramref name="items"/> collection.</typeparam>
    ''' <param name="con">The connection over which the IDs should be updated.
    ''' Note that this method does not implement any transaction handling, so if
    ''' a transaction is required, it must be initiated and completed by the calling
    ''' context.</param>
    ''' <param name="items">The IDs to use in the query</param>
    ''' <param name="query">The start of the query to use for the update.
    ''' This should contain all the INSERT / UPDATE commands and any WHERE clauses
    ''' required up to (and including) "WHERE {id} IN (" - this method will assume
    ''' that the statement is up to that point and begin entering values.
    ''' </param>
    ''' <returns>The total number of records updated by this call.</returns>
    Private Function UpdateMultipleIDs(Of T)(
     ByVal con As IDatabaseConnection,
     ByVal items As ICollection(Of T),
     ByVal query As String) As Integer
        Using cmd As New SqlCommand()
            Return UpdateMultipleIds(Of T)(con, cmd, items, Nothing, query)
        End Using
    End Function

    ''' <summary>
    ''' Updates multiple IDs on the database, taking into account the maximum number
    ''' of parameters that can be sent over the ADO.NET interface / handled by SQL
    ''' Server.
    ''' </summary>
    ''' <typeparam name="T">The type of the ID to update - this can be inferred from
    ''' the type of the <paramref name="items"/> collection.</typeparam>
    ''' <param name="con">The connection over which the IDs should be updated.
    ''' Note that this method does not implement any transaction handling, so if
    ''' a transaction is required, it must be initiated and completed by the calling
    ''' context.</param>
    ''' <param name="cmd">The command containing any initial parameters and which
    ''' should be used to update the database.
    ''' When this method exits normally, the command text and parameters will be set
    ''' back to their value when the method was entered. </param>
    ''' <param name="items">The IDs to use in the query</param>
    ''' <param name="idPrefix">The prefix of the param name to use for the IDs. If
    ''' this is set to null, "id" will be used.</param>
    ''' <param name="query">The start of the query to use for the update.
    ''' This should contain all the INSERT / UPDATE commands and any WHERE clauses
    ''' required up to (and including) "WHERE {id} IN (" - this method will assume
    ''' that the statement is up to that point and begin entering values.
    ''' </param>
    ''' <returns>The total number of records updated by this call.</returns>
    Friend Function UpdateMultipleIds(Of T)(
     ByVal con As IDatabaseConnection,
     ByVal cmd As SqlCommand,
     ByVal items As ICollection(Of T),
     ByVal idPrefix As String,
     ByVal query As String) As Integer

        If items Is Nothing OrElse items.Count = 0 Then Return 0
        If String.IsNullOrEmpty(idPrefix) Then idPrefix = "id"

        Dim i As Integer = 0
        Dim params As SqlParameterCollection = cmd.Parameters

        ' Put the initial parameters into a dictionary, so that we don't
        ' lose them in the Clear() call on each iteration of the maximum
        ' number of IDs sent.
        Dim initParams As New Dictionary(Of String, SqlParameter)
        Dim initText As String = cmd.CommandText
        For Each param As SqlParameter In cmd.Parameters
            initParams(param.ParameterName) = param
        Next
        Dim totalUpdated As Integer = 0
        ' Go through all the items, updating each 'window' of items in turn
        ' until all of the collection has been processed.
        While i < items.Count
            ' The builder into which the query will be built up, starting from
            ' the initial query from the caller (up to "where <id> in (" ).
            Dim sb As New StringBuilder(query)

            For Each id As T In New clsWindowedEnumerable(Of T)(items, i, MaxSqlParams)
                sb.AppendFormat("@{0}{1},", idPrefix, i)
                params.AddWithValue("@" & idPrefix & i, id)
                i += 1
            Next
            sb.Length -= 1
            sb.Append(")")
            cmd.CommandText = sb.ToString()
            totalUpdated += con.ExecuteReturnRecordsAffected(cmd)

            ' Clear the params we've added and restore the initial parameters that
            ' the command had when it entered the method.
            params.Clear()
            For Each param As SqlParameter In initParams.Values
                params.Add(param)
            Next

        End While

        cmd.CommandText = initText
        Return totalUpdated

    End Function

    Private Function GetDatabaseId(connection As IDatabaseConnection) As String
        Try
            Using cmd As New SqlCommand("SELECT drs.database_guid
FROM sys.databases d
JOIN sys.database_recovery_status drs ON d.database_id = drs.database_id WHERE [name]=@database_name")
                With cmd.Parameters
                    .AddWithValue("@database_name", connection.GetDatabaseName())
                End With
                Using dataReader = connection.ExecuteReturnDataReader(cmd)
                    If dataReader.Read Then
                        Dim id = CType(dataReader(0), Guid)
                        If id <> Guid.Empty Then Return id.ToString
                    End If
                End Using
            End Using
        Catch
        End Try

        Throw New BluePrismException(My.Resources.clsServer_DatabaseIdCouldNotBeRetrieved)
    End Function


    Private Function GetEnvironmentId(connection As IDatabaseConnection) As String
        Using cmd As New SqlCommand("SELECT EnvironmentId FROM BPASysConfig")
            Using dataReader = connection.ExecuteReturnDataReader(cmd)
                If dataReader.Read Then
                    Dim id = CType(dataReader(0), Guid)
                    If id <> Guid.Empty Then Return id.ToString
                End If
            End Using
        End Using

        Throw New BluePrismException(My.Resources.clsServer_EnvironmentIdCouldNotBeRetrieved)
    End Function

    Private Function GetServerName(connection As IDatabaseConnection) As String
        Dim cmd As New SqlCommand("SELECT @@SERVERNAME")
        Using dataReader = connection.ExecuteReturnDataReader(cmd)
            If dataReader.Read Then
                Return CType(dataReader(0), String)
            Else
                Throw New BluePrismException(My.Resources.clsServer_ServerNameCouldNotBeRetrieved)
            End If
        End Using
    End Function

    Friend Sub InsertDataTable(connection As IDatabaseConnection, table As DataTable, destinationTableName As String)
        connection.InsertDataTable(table, destinationTableName)
    End Sub

    Private Function GetLocalisedResourceString(resourceName As String) As String
        Return My.Resources.ResourceManager.GetString(resourceName, AuditLocale)
    End Function

#End Region

#Region "Statistics"

    ''' <summary>
    ''' Reads the statistics table from the database.
    ''' </summary>
    ''' <returns>A datatable with fields sessionid, name, datatype, value_text,
    ''' value_number, value_date and value_flag</returns>
    <SecuredMethod(Permission.SystemManager.Audit.Statistics)>
    Public Function GetStatistics() As DataTable Implements IServer.GetStatistics
        CheckPermissions()

        Dim processPermissionCache As New Dictionary(Of Guid, Boolean)
        Dim resourcePermissionCache As New Dictionary(Of Guid, Boolean)

        Using con = GetConnection()
            Using cmd As New SqlCommand("SELECT st.*, Se.processId, Se.runningresourceid FROM BPAStatistics St join BPASession Se on  Se.sessionid = St.sessionid")
                Using data = con.ExecuteReturnDataTable(cmd)
                    Dim rows = data.Rows.Cast(Of DataRow)()
                    Dim allowedRows = rows.Where(Function(x)
                                                     Dim processId = Guid.Parse(x.Item("processId").ToString)
                                                     Dim resourceId = Guid.Parse(x.Item("runningresourceid").ToString)

                                                     If processId = Guid.Empty OrElse resourceId = Guid.Empty Then Return False

                                                     If Not processPermissionCache.ContainsKey(processId) Then
                                                         Dim processPermission = GetEffectiveMemberPermissionsForProcess(processId).HasAnyPermissions(mLoggedInUser)
                                                         processPermissionCache.Add(processId, processPermission)
                                                     End If

                                                     If Not resourcePermissionCache.ContainsKey(resourceId) Then
                                                         Dim resourceResult = GetEffectiveMemberPermissionsForResource(resourceId).HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource)
                                                         resourcePermissionCache.Add(resourceId, resourceResult)
                                                     End If

                                                     Return resourcePermissionCache(resourceId) AndAlso processPermissionCache(processId)
                                                 End Function)
                    If allowedRows.Count > 0 Then
                        Dim filteredData = allowedRows.CopyToDataTable()
                        filteredData.Columns.Remove("processId")
                        filteredData.Columns.Remove("runningresourceid")
                        filteredData.TableName = "filteredData"
                        For Each row As DataRow In filteredData.Rows
                            Dim type As DataType
                            If DataType.TryParse(row("datatype").ToString, type) Then
                                row("datatype") = clsDataTypeInfo.GetLocalizedFriendlyName(type, False)
                            End If
                        Next
                        Return filteredData
                    Else
                        Return Nothing
                    End If
                End Using
            End Using
        End Using
    End Function

#End Region

#Region "Exception Types"

    ''' <summary>
    ''' Add an exception type to the database table
    ''' </summary>
    ''' <param name="exceptionType">The type of the exception to add</param>
    ''' <returns>True if the exception type was added into the table.</returns>
    <SecuredMethod(
        Permission.SystemManager.Processes.ExceptionTypes,
        Permission.SystemManager.BusinessObjects.ExceptionTypes,
        Permission.ProcessStudio.EditProcess,
        Permission.ProcessStudio.CreateProcess,
        Permission.ObjectStudio.EditBusinessObject,
        Permission.ObjectStudio.CreateBusinessObject)>
    Public Function AddExceptionType(exceptionType As String) As Boolean _
        Implements IServer.AddExceptionType

        CheckPermissions()

        Using connection = GetConnection()
            Return AddExceptionType(exceptionType, connection)
        End Using
    End Function

    Private Function AddExceptionType(
        exceptionType As String,
        connection As IDatabaseConnection) As Boolean
        If exceptionType Is Nothing Then Return False
        Dim cmd As New SqlCommand(
            " if not exists (select 1 from BPAExceptionType where type = @type)" &
            " insert into BPAExceptionType (id, type)" &
            " values (newid(), @type);"
        )
        cmd.Parameters.AddWithValue("@type", exceptionType.Trim())

        Return connection.ExecuteReturnRecordsAffected(cmd) = 1
    End Function

    ''' <summary>
    ''' Adds a collection of exception types to the database table (if they do not
    ''' exist there already).
    ''' </summary>
    ''' <param name="exceptionTypes">The collection of exception types to add</param>
    <SecuredMethod(
        Permission.SystemManager.Processes.ExceptionTypes,
        Permission.SystemManager.BusinessObjects.ExceptionTypes,
        Permission.ProcessStudio.EditProcess,
        Permission.ProcessStudio.CreateProcess,
        Permission.ObjectStudio.EditBusinessObject,
        Permission.ObjectStudio.CreateBusinessObject)>
    Public Sub AddExceptionTypes(ByVal exceptionTypes As IEnumerable(Of String)) _
        Implements IServer.AddExceptionTypes

        CheckPermissions()

        Dim cmd As New SqlCommand(
           " if not exists (select 1 from BPAExceptionType where type = @type)" &
           " insert into BPAExceptionType (id, type) values (newid(), @type)"
                                 )
        cmd.Parameters.Add("@type", SqlDbType.NVarChar)

        Using con = GetConnection()
            For Each exceptionType In exceptionTypes
                If exceptionType Is Nothing Then Continue For
                cmd.Parameters("@type").Value = exceptionType.Trim()
                con.Execute(cmd)
            Next
        End Using

    End Sub

    ''' <summary>
    ''' Finds all exception types in existing objects and processes and removes ones
    ''' that are no longer in use.
    ''' </summary>
    ''' <param name="sErr">An error on failure</param>
    ''' <returns>True if successful</returns>
    <SecuredMethod(
        Permission.SystemManager.Processes.ExceptionTypes,
        Permission.SystemManager.BusinessObjects.ExceptionTypes)>
    Public Function DeleteExceptionType(ByVal sExceptionType As String) As Boolean Implements IServer.DeleteExceptionType
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("DELETE FROM BPAExceptionType WHERE type=@Type")
            With cmd.Parameters
                .AddWithValue("@Type", sExceptionType.Trim())
            End With

            con.Execute(cmd)
            Return True
        Catch ex As Exception
            Return False
        Finally
            con.Close()
        End Try
    End Function

    ''' <summary>
    ''' Gets a list of exception types.
    ''' </summary>
    ''' <param name="ExceptionTypes">A list to be populated with exception types (may be null)</param>
    ''' <param name="sErr">An error message on failure</param>
    ''' <returns>True if successful</returns>
    <SecuredMethod(True)>
    Public Function GetExceptionTypes() As List(Of String) Implements IServer.GetExceptionTypes
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT type FROM BPAExceptionType ORDER BY type")
            Dim exceptionTypes As New List(Of String)

            Dim Reader = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
            If Reader.HasRows Then
                While Reader.Read
                    Dim Val As Object = Reader("type")
                    If Not TypeOf Val Is DBNull Then
                        exceptionTypes.Add(CStr(Val))
                    End If
                End While
            End If

            Return exceptionTypes
        End Using
    End Function

#End Region

#Region "Font Data"

    ''' <summary>
    ''' Get a list of all fonts stored in the database.
    ''' </summary>
    ''' <returns>A DataTable with columns 'name' and 'version'.</returns>
    <SecuredMethod(True)>
    Public Function GetFonts() As DataTable Implements IServer.GetFonts
        CheckPermissions()
        Dim con = GetConnection()
        Try
            Dim cmd As New SqlCommand("SELECT name,version FROM BPAFont")
            Return con.ExecuteReturnDataTable(cmd)
        Finally
            con.Close()
        End Try
    End Function

    ''' <summary>
    ''' Gets a collection of the font names on the database
    ''' </summary>
    ''' <returns>The names of all the fonts on the system.</returns>
    <SecuredMethod()>
    Public Function GetFontNames() As ICollection(Of String) Implements IServer.GetFontNames
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select name from BPAFont")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                Dim names As New List(Of String)
                While reader.Read()
                    names.Add(prov.GetString("name"))
                End While
                Return names
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Delete a font from the database.
    ''' </summary>
    ''' <param name="name">The name of the font.</param>
    ''' <returns>True if the font was found on the database and deleted; False if the
    ''' font was not found on the database.</returns>
    <SecuredMethod(Permission.SystemManager.System.Fonts)>
    Public Function DeleteFont(ByVal name As String) As Boolean Implements IServer.DeleteFont
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("DELETE FROM BPAFont WHERE name = @name")
            cmd.Parameters.AddWithValue("@name", name)
            If con.ExecuteReturnRecordsAffected(cmd) > 0 Then
                AuditRecordFontEvent(con, FontEventCode.Delete, name)
                Return True
            End If
            IncrementDataVersion(con, DataNames.Font)
            Return False
        End Using
    End Function

    ''' <summary>
    ''' Get data for the specified font.
    ''' </summary>
    ''' <param name="name">The name of the font.</param>
    ''' <param name="version">On success, contains the font version.</param>
    ''' <returns>The XML font definition, or Nothing if a font with the given name
    ''' does not exist in the database.</returns>
    <SecuredMethod(True)>
    Public Function GetFont(ByVal name As String, ByRef version As String) As String Implements IServer.GetFont
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             "SELECT version,fontdata FROM BPAFont WHERE name = @name")
            cmd.Parameters.AddWithValue("@name", name)
            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then Return Nothing
                version = CStr(reader("version"))
                Return CStr(reader("fontdata"))
            End Using
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetFontOcrPlus(ByVal name As String, ByRef version As String) As String Implements IServer.GetFontOcrPlus
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand(
             "select version,fontdata from BPAFontOcrPlusPlus where name = @name")
            cmd.Parameters.AddWithValue("@name", name)
            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then Return Nothing
                version = CStr(reader("version"))
                Return CStr(reader("fontdata"))
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Checks if the font data has updated from the given version number, updating
    ''' the version number reference if it has.
    ''' </summary>
    ''' <param name="versionNo">The version number to test. On exit, this contains
    ''' the most up to date version number for the fonts in the environment.</param>
    ''' <returns>True if the font data has been updated since the specified version
    ''' number; False if the given version number was the latest version of the
    ''' fonts recorded.</returns>
    <SecuredMethod(True)>
    Public Function HasFontDataUpdated(ByRef versionNo As Long) As Boolean Implements IServer.HasFontDataUpdated
        CheckPermissions()
        Return HasDataUpdated(DataNames.Font, versionNo)
    End Function

    ''' <summary>
    ''' Update (or create) font data.
    ''' </summary>
    ''' <param name="name">The name of the font. If a font doesn't already exist
    ''' with that name, a new record is created.</param>
    ''' <param name="version">The font version.</param>
    ''' <param name="data">The XML font definition.</param>
    <SecuredMethod(Permission.SystemManager.System.Fonts,
        Permission.ObjectStudio.CreateBusinessObject, Permission.ObjectStudio.EditBusinessObject)>
    Public Sub SaveFont(ByVal origName As String,
     ByVal name As String, ByVal version As String, ByVal data As String) Implements IServer.SaveFont
        CheckPermissions()
        Using con = GetConnection()
            SaveFont(con, origName, name, version, data)
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Sub SaveFontOcrPlus(ByVal name As String, ByVal data As String) Implements IServer.SaveFontOcrPlus
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("delete from BPAFontOcrPlusPlus where name = @name")
            cmd.Parameters.AddWithValue("@name", name)
            con.ExecuteReturnRecordsAffected(cmd)
            Dim cmd1 As New SqlCommand("insert into BPAFontOcrPlusPlus (name, Version, fontdata) values (@name,@ver,@data)")
            cmd1.Parameters.AddWithValue("@name", name)
            cmd1.Parameters.AddWithValue("@ver", "1.0")
            cmd1.Parameters.AddWithValue("@data", data)
            con.ExecuteReturnScalar(cmd1)
        End Using
    End Sub

    ''' <summary>
    ''' Update (or create) font data.
    ''' </summary>
    ''' <param name="name">The name of the font. If a font doesn't already exist
    ''' with that name, a new record is created.</param>
    ''' <param name="version">The font version.</param>
    ''' <param name="data">The XML font definition.</param>
    <SecuredMethod(True)>
    Public Sub SaveFont(
     ByVal name As String, ByVal version As String, ByVal data As String) Implements IServer.SaveFont
        CheckPermissions()
        Using con = GetConnection()
            SaveFont(con, name, name, version, data)
        End Using
    End Sub

    ''' <summary>
    ''' Saves the given font data over the specified connection.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="origName">The original name of the font - may be different to
    ''' the <paramref name="name"/> parameter if part of the update is changing the
    ''' font name. This parameter is immaterial if the font doesn't yet exist on
    ''' the database.</param>
    ''' <param name="name">The current name of the font - ie. that which should be
    ''' held in the corresponding record after this method has completed.</param>
    ''' <param name="ver">The version to record against the font.</param>
    ''' <param name="data">The data describing the font.</param>
    Private Sub SaveFont(con As IDatabaseConnection, origName As String, name As String,
                         ver As String, data As String)
        SaveFont(con, origName, name, ver, data, True)
    End Sub

    ''' <summary>
    ''' Saves the given font data over the specified connection.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="origName">The original name of the font - may be different to
    ''' the <paramref name="name"/> parameter if part of the update is changing the
    ''' font name. This parameter is immaterial if the font doesn't yet exist on
    ''' the database.</param>
    ''' <param name="name">The current name of the font - ie. that which should be
    ''' held in the corresponding record after this method has completed.</param>
    ''' <param name="ver">The version to record against the font.</param>
    ''' <param name="data">The data describing the font.</param>
    ''' <param name="incrementVersion">Controls whether version data that is monitored
    ''' to check for updates is incremented following the change. If this change is
    ''' part of a batch, then the version update may be deferred until all changes
    ''' in the batch have been made.</param>
    Private Sub SaveFont(con As IDatabaseConnection, origName As String, name As String,
                         ver As String, data As String, incrementVersion As Boolean)

        Dim cmd As New SqlCommand()
        ' We remove the name updating if the name is the same (ie. if name == origName)
        ' It is a primary key, so we don't want to cascade a load of changes that didn't occur
        ' (and I just don't know whether SQL Server already optimises it out or not)
        ' This will return a scalar value of '1' if inserting a new font, and
        ' a value of '2' if updating an existing one
        If origName = name Then
            cmd.CommandText =
             " if exists (select 1 from BPAFont where name = @name) begin" &
             "   update BPAFont set version=@ver,fontdata=@data where name=@name" &
             "   select 2 end" &
             " else begin" &
             "   insert into BPAFont (name,version,fontdata) " &
             "     values (@name,@ver,@data)" &
             "   select 1 end"
        Else
            ' We're renaming as well as updating - change the name too
            cmd.CommandText =
             " if exists (select 1 from BPAFont where name = @orig) begin" &
             "   update BPAFont set name=@name,version=@ver,fontdata=@data " &
             "     where name=@orig" &
             "   select 2 end" &
             " else begin" &
             "   insert into BPAFont (name,version,fontdata) " &
             "     values (@name,@ver,@data)" &
             "   select 1 end"
            cmd.Parameters.AddWithValue("@orig",
             IIf(origName Is Nothing, DBNull.Value, origName))
        End If

        With cmd.Parameters
            .AddWithValue("@name", name)
            .AddWithValue("@ver", ver)
            .AddWithValue("@data", data)
        End With
        Dim op As Integer = IfNull(con.ExecuteReturnScalar(cmd), 0)
        Select Case op
            Case 1 : AuditRecordFontEvent(con, FontEventCode.Create, name)
            Case 2 : AuditRecordFontEvent(con, FontEventCode.Modify, name, origName)
            Case Else : Debug.Fail("Unrecognised font operation: " & op)
        End Select

        If incrementVersion Then
            IncrementDataVersion(con, DataNames.Font)
        End If

    End Sub

#End Region

#Region "Environment Variables"

    ''' <summary>
    ''' Gets the single environment variable with the given name.
    ''' </summary>
    ''' <param name="name">The name of the required environment variable</param>
    ''' <returns>The environment variable with the given name as found on the
    ''' database, or null if no environment variable with the given name was found.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">If no name was given</exception>
    <SecuredMethod(
        Permission.SystemManager.Processes.ViewEnvironmentVariables,
        Permission.SystemManager.BusinessObjects.ViewEnvironmentVariables,
        Permission.SystemManager.Processes.ConfigureEnvironmentVariables,
        Permission.SystemManager.BusinessObjects.ConfigureEnvironmentVariables)>
    Public Function GetEnvironmentVariable(ByVal name As String) As clsEnvironmentVariable Implements IServer.GetEnvironmentVariable
        CheckPermissions()
        Using con = GetConnection()
            Return GetEnvironmentVariable(con, name)
        End Using
    End Function

    ''' <summary>
    ''' Gets all the environment variables into a dictionary mapped against their
    ''' names.
    ''' </summary>
    ''' <returns>A map of environment variables mapped against their names.
    ''' </returns>
    <SecuredMethod(True)>
    Public Function GetEnvironmentVariables() As ICollection(Of clsEnvironmentVariable) Implements IServer.GetEnvironmentVariables
        CheckPermissions()
        Using con = GetConnection()
            Return GetEnvironmentVariables(con)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetEnvironmentVariablesNames() As List(Of String) Implements IServer.GetEnvironmentVariablesNames
        CheckPermissions()
        Using con = GetConnection()
            Return GetEnvironmentVariablesNames(con)
        End Using
    End Function


    ''' <summary>
    ''' Gets the single environment variable with the given name over the specified
    ''' connection.
    ''' </summary>
    ''' <param name="con">The connection over which the environment variable should
    ''' be retrieved.</param>
    ''' <param name="name">The name of the required environment variable</param>
    ''' <returns>The environment variable with the given name as found on the
    ''' database or null if no such variable was found.</returns>
    ''' <exception cref="ArgumentNullException">If no name was given.</exception>
    Private Function GetEnvironmentVariable(
     ByVal con As IDatabaseConnection, ByVal name As String) As clsEnvironmentVariable

        If name = "" Then Throw New ArgumentNullException(NameOf(name))

        Dim cmd As New SqlCommand(
         "select name,datatype,value,description from BPAEnvironmentVar where name=@name")

        cmd.Parameters.AddWithValue("@name", name)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Return Nothing
            Return New clsEnvironmentVariable(New ReaderDataProvider(reader))
        End Using

    End Function

    ''' <summary>
    ''' Gets all the environment variables into a dictionary mapped against their
    ''' names.
    ''' </summary>
    ''' <param name="con">The connection to use to get the environment variables.
    ''' </param>
    ''' <returns>A map of environment variables mapped against their names.
    ''' </returns>
    Private Function GetEnvironmentVariables(ByVal con As IDatabaseConnection) _
     As ICollection(Of clsEnvironmentVariable)

        Using reader = con.ExecuteReturnDataReader(
         New SqlCommand("select name,datatype,value,description from BPAEnvironmentVar"))
            Dim prov As New ReaderDataProvider(reader)
            Dim vars As New List(Of clsEnvironmentVariable)
            While reader.Read()
                vars.Add(New clsEnvironmentVariable(prov))
            End While
            Return vars
        End Using

    End Function

    Private Function GetEnvironmentVariablesNames(ByVal con As IDatabaseConnection) As List(Of String)
        Using reader = con.ExecuteReturnDataReader(
         New SqlCommand("select name from BPAEnvironmentVar"))
            Dim prov As New ReaderDataProvider(reader)
            Dim vars As New List(Of String)
            While reader.Read()
                vars.Add(prov.GetString("name"))
            End While
            Return vars
        End Using
    End Function

    ''' <summary>
    ''' Updates, or creates, an environment variable.
    ''' </summary>
    ''' <param name="name">The environment variable name</param>
    ''' <param name="datatype">The data type</param>
    ''' <param name="value">The value, in Automate text-encoded format.</param>
    ''' <param name="description">A description of the variable.</param>
    <SecuredMethod(
        Permission.SystemManager.Processes.ConfigureEnvironmentVariables,
        Permission.SystemManager.BusinessObjects.ConfigureEnvironmentVariables)>
    Public Sub UpdateEnvironmentVariable(ByVal name As String, ByVal datatype As DataType,
     ByVal value As String, ByVal description As String) Implements IServer.UpdateEnvironmentVariable
        CheckPermissions()
        Using con = GetConnection()
            UpdateAndAuditEnvironmentVariable(con, name, datatype, value, description)
        End Using
    End Sub

    ''' <summary>
    ''' Updates, or creates, an environment variable.
    ''' </summary>
    ''' <param name="name">The environment variable name</param>
    ''' <returns>True if successful.</returns>
    <SecuredMethod(
        Permission.SystemManager.Processes.ConfigureEnvironmentVariables,
        Permission.SystemManager.BusinessObjects.ConfigureEnvironmentVariables)>
    Public Function DeleteEnvironmentVariable(ByVal name As String) As Boolean Implements IServer.DeleteEnvironmentVariable
        CheckPermissions()

            Using con = GetConnection()
                Dim environmentVariableList As New List(Of clsEnvironmentVariable) From {GetEnvironmentVariable(con, name)}

                If environmentVariableList.FirstOrDefault() Is Nothing Then
                    Return False
                End If

                con.BeginTransaction()
                DeleteEnvironmentVariables(con, environmentVariableList, EnvironmentVariableEventCode.AutomateCDeleted)
                con.CommitTransaction()
            End Using

        Return True
    End Function

    ''' <summary>
    ''' Updates, or creates, an environment variable.
    ''' </summary>
    ''' <param name="con">The connection over which the environment variable should
    ''' be updated</param>
    ''' <param name="name">The environment variable name</param>
    ''' <param name="datatype">The data type</param>
    ''' <param name="value">The value, in Automate text-encoded format.</param>
    ''' <param name="description">A description of the variable.</param>
    Private Sub UpdateAndAuditEnvironmentVariable(ByVal con As IDatabaseConnection,
     ByVal name As String, ByVal datatype As DataType,
     ByVal value As String, ByVal description As String)

        Dim newEnvironmentVariable As New clsEnvironmentVariable(name, New clsProcessValue(datatype, value), description)
        Dim currentEnvironmentVariable As clsEnvironmentVariable = GetEnvironmentVariable(con, newEnvironmentVariable.Name)

        If newEnvironmentVariable.Equals(currentEnvironmentVariable) Then Return

        Dim auditEvent As EnvironmentVariablesAuditEvent
        If currentEnvironmentVariable Is Nothing Then
            Dim createdEnvironmentVariablesAuditEventFactory As New CreatedEnvironmentVariablesAuditEventGenerator(newEnvironmentVariable)
            auditEvent = createdEnvironmentVariablesAuditEventFactory.Generate(EnvironmentVariableEventCode.AutomateCCreated, mLoggedInUser)
        Else
            Dim environmentVariablesAuditEventFactory _
             As New ModifiedEnvironmentVariablesAuditEventGenerator(
              currentEnvironmentVariable, newEnvironmentVariable, mLoggedInUser)
            auditEvent = environmentVariablesAuditEventFactory.Generate(
             EnvironmentVariableEventCode.AutomateCModified, mLoggedInUser)
        End If

        SaveEnvironmentVariable(con, newEnvironmentVariable)
        AuditRecordEnvironmentVariablesChanges(con, auditEvent)

    End Sub

    ''' <summary>
    ''' Updates, or creates, an environment variable.
    ''' </summary>
    ''' <param name="con">The connection over which the environment variable should
    ''' be updated</param>
    ''' <param name="ev">The environment variable to update</param>
    ''' <remarks>This is ultimately used by all Save/Update methods so we have the
    ''' actual mechanism in one place. UpdateXXX() seems like a misnomer to me if it
    ''' may also create the record in question - update implies that it's being
    ''' changed, not created.</remarks>
    Private Sub SaveEnvironmentVariable(
     ByVal con As IDatabaseConnection, ByVal ev As clsEnvironmentVariable)

        Dim cmd As New SqlCommand(
         " if exists (select 1 from BPAEnvironmentVar where name=@name) " &
         "   update BPAEnvironmentVar set " &
         "     datatype=@datatype," &
         "     value=@value," &
         "     description=@description" &
         "   where name=@name" &
         " else" &
         "   insert into BPAEnvironmentVar (name,datatype,value,description) " &
         "     values (@name,@datatype,@value,@description)"
        )

        With cmd.Parameters
            .AddWithValue("@name", ev.Name)
            .AddWithValue("@datatype", ev.Value.EncodedType)
            .AddWithValue("@value", ev.Value.EncodedValue)
            .AddWithValue("@description", ev.Description)
        End With

        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Updates environment variables based on a list of inserted, updated and
    ''' deleted items.
    ''' </summary>
    ''' <param name="inserted">The collection of env vars which are to be inserted
    ''' into the database</param>
    ''' <param name="updated">The collection of env vars which should be updated
    ''' with new data on the database.</param>
    ''' <param name="deleted">The collection of env vars which should deleted from
    ''' the database.</param>
    ''' <exception cref="ArgumentNullException">If any of the collections are null.
    ''' </exception>
    <SecuredMethod(
        Permission.SystemManager.Processes.ConfigureEnvironmentVariables,
        Permission.SystemManager.BusinessObjects.ConfigureEnvironmentVariables)>
    Public Sub UpdateEnvironmentVariables(inserted As ICollection(Of clsEnvironmentVariable),
                                           updated As ICollection(Of clsEnvironmentVariable),
                                           deleted As ICollection(Of clsEnvironmentVariable)) Implements IServer.UpdateEnvironmentVariables
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            UpdateEnvironmentVariables(con, inserted, updated, deleted)
            con.CommitTransaction()
        End Using

    End Sub

    ''' <summary>
    ''' Updates environment variables based on a list of inserted, updated and
    ''' deleted items.
    ''' </summary>
    ''' <param name="con">The connection to the database to use to update the env
    ''' vars.</param>
    ''' <param name="inserted">The collection of env vars which are to be inserted
    ''' into the database</param>
    ''' <param name="updated">The collection of env vars which should be updated
    ''' with new data on the database.</param>
    ''' <param name="deleted">The collection of env vars which should deleted from
    ''' the database.</param>
    ''' <exception cref="ArgumentNullException">If any of the collections are null.
    ''' </exception>
    Private Sub UpdateEnvironmentVariables(con As IDatabaseConnection,
                                            inserted As ICollection(Of clsEnvironmentVariable),
                                            updated As ICollection(Of clsEnvironmentVariable),
                                            deleted As ICollection(Of clsEnvironmentVariable))
        If inserted Is Nothing Then Throw New ArgumentNullException(NameOf(inserted))
        If updated Is Nothing Then Throw New ArgumentNullException(NameOf(updated))
        If deleted Is Nothing Then Throw New ArgumentNullException(NameOf(deleted))

        Dim original = GetEnvironmentVariables()
        If original Is Nothing Then Throw New ArgumentNullException(NameOf(original))

        ' Deal with the deleted vars first (see bug 5660-e)
        DeleteEnvironmentVariables(con, deleted, EnvironmentVariableEventCode.Deleted)

        ' Set up the command + params we're going to be using in the updates/inserts
        Dim cmd As New SqlCommand()
        With cmd.Parameters
            .Add("@name", SqlDbType.NVarChar, 255)
            .Add("@datatype", SqlDbType.NVarChar)
            .Add("@value", SqlDbType.NVarChar)
            .Add("@description", SqlDbType.NVarChar)
            .Add("@newname", SqlDbType.NVarChar)
        End With

        ' Updates next - we need to ensure that any inserts don't clash with
        ' existing names, so they must be done last when all existing vars
        ' have been changed as appropriate.
        cmd.CommandText =
         " update BPAEnvironmentVar set" &
         "   name=@newname," &
         "   datatype=@datatype," &
         "   value=@value," &
         "   description=@description" &
         " where name=@name"

        For Each variable As clsEnvironmentVariable In updated
            With cmd.Parameters
                .Item("@name").Value = variable.OldName
                .Item("@datatype").Value = variable.Value.EncodedType
                .Item("@value").Value = variable.Value.EncodedValue
                .Item("@description").Value = variable.Description
                .Item("@newname").Value = variable.Name
            End With
            con.Execute(cmd)

            Dim oldVariable = original.Where(Function(x) x.Name = variable.OldName).FirstOrDefault

            If oldVariable IsNot Nothing Then
                Dim environmentVariablesAuditEventFactory _
                  As New ModifiedEnvironmentVariablesAuditEventGenerator(oldVariable, variable, mLoggedInUser)
                Dim auditEvent = environmentVariablesAuditEventFactory.Generate(
                 EnvironmentVariableEventCode.Modified, mLoggedInUser)
                If auditEvent IsNot Nothing Then _
                 AuditRecordEnvironmentVariablesChanges(con, auditEvent)
            End If
        Next

        ' Finally, handle the new vars
        cmd.CommandText =
         " insert into BPAEnvironmentVar (name,datatype,value,description)" &
         "   values(@name,@datatype,@value,@description)"

        ' We no longer need to pass the @newname param
        cmd.Parameters.RemoveAt("@newname")
        For Each variable As clsEnvironmentVariable In inserted
            With cmd.Parameters
                .Item("@name").Value = variable.Name
                .Item("@datatype").Value = variable.Value.EncodedType
                .Item("@value").Value = variable.Value.EncodedValue
                .Item("@description").Value = variable.Description
            End With
            con.Execute(cmd)

            Dim createdEnvironmentVariablesAuditEventFactory As New CreatedEnvironmentVariablesAuditEventGenerator(variable)
            Dim auditEvent = createdEnvironmentVariablesAuditEventFactory.Generate(EnvironmentVariableEventCode.Created, mLoggedInUser)
            If auditEvent IsNot Nothing Then AuditRecordEnvironmentVariablesChanges(con, auditEvent)

        Next

    End Sub

    Private Sub DeleteEnvironmentVariables(con As IDatabaseConnection, deleted As ICollection(Of clsEnvironmentVariable),
                                           auditEventCode As EnvironmentVariableEventCode)
        Try
            Dim deletedNames As New List(Of String)
            For Each variable As clsEnvironmentVariable In deleted
                deletedNames.Add(variable.Name)
            Next
            UpdateMultipleIds(con, New SqlCommand(), deletedNames, "name",
             "delete from BPAEnvironmentVar where name in(")
            For Each variable As clsEnvironmentVariable In deleted
                Dim evt As New EnvironmentVariablesAuditEvent(
                 auditEventCode, mLoggedInUser, variable.Name)
                AuditRecordEnvironmentVariablesChanges(con, evt)
            Next
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub UpdateEnableOfflineHelp(enable As Boolean) Implements IServer.UpdateEnableOfflineHelp
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("update BPASysConfig set enableofflinehelp=@enable")
            cmd.Parameters.AddWithValue("@enable", enable)
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyOfflinehelp, String.Empty, enable.ToString())
        End Using
    End Sub

    <SecuredMethod()>
    Private Function OfflineHelpEnabled() As Boolean Implements IServer.OfflineHelpEnabled
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select enableofflinehelp from BPASysConfig")
            Return IfNull(con.ExecuteReturnScalar(cmd), False)
        End Using
    End Function

    Private Sub UpdateOfflineHelpBaseUrl(baseUrl As String) Implements IServer.UpdateOfflineHelpBaseUrl
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("update BPASysConfig set offlinehelpbaseurl=@baseUrl")
            cmd.Parameters.AddWithValue("@baseUrl", baseUrl)
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyOfflineHelpUrl, String.Empty, baseUrl)
        End Using
    End Sub

    Private Function GetOfflineHelpBaseUrl() As String Implements IServer.GetOfflineHelpBaseUrl
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select offlinehelpbaseurl from BPASysConfig")
            Return IfNull(con.ExecuteReturnScalar(cmd), String.Empty)
        End Using
    End Function

    Private Sub UpdateOfflineHelpData(enable As Boolean, baseUrl As String) Implements IServer.UpdateOfflineHelpData
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("update BPASysConfig set enableofflinehelp=@enable, offlinehelpbaseurl=@baseUrl")
            cmd.Parameters.AddWithValue("@enable", enable)
            cmd.Parameters.AddWithValue("@baseUrl", baseUrl)
            con.Execute(cmd)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyOfflineHelpData, String.Empty, enable.ToString(), baseUrl)
        End Using
    End Sub

    <SecuredMethod()>
    Private Sub UpdateHideDigitalExchangeSetting(value As Boolean) Implements IServer.UpdateHideDigitalExchangeSetting
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(con, PreferenceNames.Env.HideDigitalExchangeTab, value)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyHideDigitalExchangeTab, String.Empty, value.ToString())
        End Using
    End Sub

    <SecuredMethod()>
    Private Function GetHideDigitalExchangeSetting(defaultValue As Boolean) As Boolean Implements IServer.GetHideDigitalExchangeSetting
        CheckPermissions()
        Return _GetPref(PreferenceNames.Env.HideDigitalExchangeTab, defaultValue)
    End Function

    <SecuredMethod()>
    Private Sub UpdateEnableBpaEnvironmentDataSetting(value As Boolean) Implements IServer.UpdateEnableBpaEnvironmentDataSetting
        CheckPermissions()
        Using con = GetConnection()
            SetSystemPref(con, PreferenceNames.SystemSettings.EnableBpaEnvironmentData, value)
            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyEnableBpaEnvironmentData, String.Empty, value.ToString())
        End Using
    End Sub

    <SecuredMethod()>
    Private Function GetEnableBpaEnvironmentDataSetting(defaultValue As Boolean) As Boolean Implements IServer.GetEnableBpaEnvironmentDataSetting
        CheckPermissions()
        Return _GetPref(PreferenceNames.SystemSettings.EnableBpaEnvironmentData, defaultValue)
    End Function

    <SecuredMethod()>
    Public Sub SetUserUILayoutPreferences(preferences As String) Implements IServer.SetUserUILayoutPreferences
        CheckPermissions()
        _SetUserPref(PreferenceNames.UI.LayoutPreferences, preferences)
    End Sub

    <SecuredMethod()>
    Public Function GetUserUILayoutPreferences() As String Implements IServer.GetUserUILayoutPreferences
        CheckPermissions()
        Return _GetPref(Of String)(PreferenceNames.UI.LayoutPreferences, Nothing)
    End Function

#End Region

End Class
