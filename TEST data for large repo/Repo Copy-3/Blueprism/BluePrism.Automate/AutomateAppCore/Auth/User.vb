Imports System.Runtime.Remoting.Messaging
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Threading

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Common.Security
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Skills

Namespace Auth

    ''' <summary>
    ''' Class to represent a user in Blue Prism
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class User : Inherits clsDataMonitor : Implements ILogicalThreadAffinative
        Implements IUser

#Region " Class-scope declarations "

        ' The auth mode currently set in this environment
        Private Shared sAuthMode As AuthMode

        ' The current logged in user; null if no-one is logged in
        Private Shared sCurrent As User

        Private Shared mDatabaseType As DatabaseType

        Public Delegate Sub ExternalUserReloginEventHandler()

        ''' <summary>
        ''' The current logged in user; this will only occur on the client-side of a
        ''' BP Server connection.
        ''' </summary>
        ''' <exception cref="InvalidStateException">If an attempt is made to set the
        ''' current user from server-side code.</exception>
        Public Shared Property Current() As User
            Get
                ' Getting User.Current is not allowed when running on the server
                If clsServer.RunningOnServer _
                 Then Throw New InvalidStateException(My.Resources.User_CannotGetCurrentUserWithinServerExecutedCode) _
                 Else Return sCurrent
            End Get
            Friend Set(ByVal value As User)
                If clsServer.RunningOnServer Then Throw New InvalidStateException(
                 My.Resources.User_CannotSetTheCurrentUserWithinServerExecutedCode)
                sCurrent = value
            End Set
        End Property

        ''' <summary>
        ''' The current logged in user ID, or Guid.Empty if there is no user logged
        ''' into this instance of the software.
        ''' </summary>
        Public Shared ReadOnly Property CurrentId() As Guid
            Get
                Dim u As User = Current
                If u Is Nothing Then Return Guid.Empty Else Return u.Id
            End Get
        End Property

        ''' <summary>
        ''' The current logged in user's name or an empty string if there is no user
        ''' logged into this instance of the software
        ''' </summary>
        Public Shared ReadOnly Property CurrentName() As String
            Get
                Dim u As User = Current
                If u Is Nothing Then Return "" Else Return u.Name
            End Get
        End Property

        ''' <summary>
        ''' Flag indicating if a user is logged in or not.
        ''' </summary>
        Public Shared ReadOnly Property LoggedIn() As Boolean
            Get
                Return (Current IsNot Nothing)
            End Get
        End Property

        ''' <summary>
        ''' Gets the user with the given ID.
        ''' </summary>
        ''' <param name="userId">The ID of the required user</param>
        ''' <returns>The user object corresponding to the given ID</returns>
        Public Shared Function GetUser(ByVal userId As Guid) As User
            Return gSv.GetUser(userId)
        End Function

        ''' <summary>
        ''' Gets the user with the given name.
        ''' </summary>
        ''' <param name="userName">The name of the required user</param>
        ''' <returns>The user object corresponding to the given name</returns>
        Public Shared Function GetUser(ByVal userName As String) As User
            Return gSv.GetUser(userName)
        End Function

        Public Shared Function IsLoggedInto(databaseType As DatabaseType) As Boolean
            Return databaseType = mDatabaseType
        End Function


        ''' <summary>
        ''' Gets the default expiry date for a new user.
        ''' </summary>
        Private Shared ReadOnly Property DefaultExpiry() As Date
            Get
                Return Date.UtcNow.Date.AddYears(1)
            End Get
        End Property

        ''' <summary>
        ''' Gets the default password expiry date for a new user.
        ''' </summary>
        Private Shared ReadOnly Property DefaultPasswordExpiry() As Date
            Get
                Return Date.UtcNow.Date.AddMonths(1)
            End Get
        End Property

        ''' <summary>
        ''' Checks username rules
        ''' </summary>
        ''' <param name="username">The username to check</param>
        ''' <param name="reason">An reason when the username rules check fails</param>
        ''' <returns>True if all username checks succeed, Otherwise False</returns>
        Public Shared Function IsValidUsername(username As String, ByRef reason As String, Optional isActiveDirectoryUser As Boolean = False) As Boolean
            If username = "" Then
                reason = My.Resources.User_UsernameCannotBeBlank
                Return False
            End If
            Dim maxLength = If(isActiveDirectoryUser, MaxActiveDirectoryUserNameLength, MaxNativeAndExternalUserNameLength)
            If username.Length > maxLength Then
                reason = String.Format(My.Resources.User_UserNameIsTooLongMaximumAllowableLengthIs0, maxLength.ToString)
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Checks external Id rules
        ''' </summary>
        ''' <param name="externalId">The external Id to check</param>
        ''' <param name="reason">An reason when the external Id rules check fails</param>
        ''' <returns>True if all external Id checks succeed, Otherwise False</returns>
        Public Shared Function IsValidExternalId(externalId As String, ByRef reason As String) As Boolean

            If String.IsNullOrWhiteSpace(externalId) Then
                reason = My.Resources.UserSettingsNameAndExternalId_ExternalIdCannotBeBlank
                Return False
            End If

            If externalId.Length > MaxExternalIdLength Then
                reason = String.Format(My.Resources.UserSettingsNameAndExternalId_ExternalIdIsTooLongMaximumAllowableLengthIs0,
                                       MaxExternalIdLength.ToString)
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' The default number of weeks that a password will remain valid.
        ''' </summary>
        Public Const DefaultPasswordDurationWeeks As Integer = 4


        Private Const MaxNativeAndExternalUserNameLength As Integer = 20

        Private Const MaxActiveDirectoryUserNameLength As Integer = 128

        ''' <summary>
        ''' The maximum external Id length (applies to external authentication only)
        ''' </summary>
        Private Const MaxExternalIdLength As Integer = 254

#Region " Old clsUser holdovers "

        ''' <summary>
        ''' These are saved only for the purposes of the ReLogin method.
        ''' </summary>
        Private Shared mLastLoginUsername As String = Nothing
        <NonSerialized>
        Private Shared mLastLoginSecurePassword As SafeString = Nothing
        Private Shared mLastSetMachineName As String = Nothing
        Private Shared mLastUserLocale As String = Nothing
        <NonSerialized>
        Private Shared ReadOnly ReloginTokenUpdater As New Timer(AddressOf UpdateReloginTokenThread)
        <NonSerialized>
        Private Shared ReadOnly ReloginTokenLockObject As New Object()
        <NonSerialized>
        Private Shared mReloginToken As SafeString = Nothing
        Private Shared Property ReloginToken As SafeString
            Get
                SyncLock ReloginTokenLockObject
                    Return mReloginToken
                End SyncLock
            End Get
            Set
                SyncLock ReloginTokenLockObject
                    mReloginToken = Value
                End SyncLock
            End Set
        End Property
        ''' <summary>
        ''' Sets the current user as being logged out, optionally clearing the
        ''' keepalive timestamp on the server.
        ''' </summary>
        Private Shared Sub SetLoggedOut(ByVal attemptingRelogin As Boolean)

            If Not LoggedIn Then Return ' Already logged out.

            If Not attemptingRelogin Then
                Try
                    gSv.ClearKeepAliveTimeStamp()
                Catch ex As Exception
                    'Don't allow an exception here to prevent logout. This has no benefit an
                    'causes serious problems, such as bug #8297. In the case of an awol BP
                    'Server or network connection, gSv would be null, for example.
                    Debug.Assert(False, "Exception clearing timestamp in SetLoggedOut() - " & ex.Message)
                End Try
            End If

            'Stop timer events that require a logged in user to update
            DatabaseBackedScheduleStore.DisposeInertStore()

            If sCurrent.AuthType = AuthMode.AuthenticationServer AndAlso Not attemptingRelogin Then
                ' we are performing a hard clear, otherewise we keep these external 
                ' login details because we are going to try and login back in using them
                StopReloginTokenUpdate()
                Dim processId = Process.GetCurrentProcess().Id
                Try
                    gSv.DeleteReloginToken(mLastSetMachineName, processId)
                Catch ex As Exception
                    Debug.Assert(False, "Exception clearing relogin token in SetLoggedOut() - " & ex.Message)
                End Try
                ClearReloginToken()
            End If

            sCurrent = Nothing
            sAuthMode = AuthMode.Unspecified
            mDatabaseType = DatabaseType.None
            mLastLoginUsername = Nothing
            mLastLoginSecurePassword = Nothing
            mLastSetMachineName = Nothing
            mLastUserLocale = Nothing
        End Sub

        ''' <summary>
        ''' Set a user to be logged in.
        ''' 
        ''' This caches information (e.g. roles) about the user locally.
        ''' </summary>
        ''' <param name="userid">The guid of the user of the logged in user.</param>
        ''' <param name="authType">The AuthMode in use.</param>
        ''' <param name="server">The server to use instead of the standard gSv. Supplying
        ''' this implies a 'ReLogin' situation.</param>
        Private Shared Sub SetLoggedIn(ByVal userid As Guid, ByVal authType As AuthMode, Optional ByVal server As IServer = Nothing, Optional automateC As Boolean = False)

            Dim relogin As Boolean = True
            If server Is Nothing Then
                server = gSv
                relogin = False
            End If

            sAuthMode = authType
            mDatabaseType = server.DatabaseType()

            Try
                If Not relogin Then
                    ' Load the permissions and roles
                    SystemRoleSet.Reset(True)
                End If
                sCurrent = server.GetUser(userid)
                server.UpdateLoginTimestamp(sCurrent.SignedInAt, sCurrent.LastSignedInAt)

                Dim configOptions = Options.Instance

                Try
                    configOptions.EditSummariesAreCompulsory = server.GetEnforceEditSummariesSetting()
                Catch ex As Exception
                    Debug.Assert(False, "There was an error whilst reading the system preference 'Use edit summaries' from the database - " & ex.Message)
                End Try

                Try
                    configOptions.CompressProcessXml = server.GetCompressProcessXMLSetting()
                Catch ex As Exception
                    Debug.Assert(False, "There was an error whilst reading the system preference 'Compress Process XML' from the database - " & ex.Message)
                End Try

                Try
                    server.SetKeepAliveTimeStamp(automateC)
                Catch ex As Exception
                    Debug.Assert(False, "There was an error updating the database with a" &
                     " timestamp. As a consequence, other resources may not" &
                     " continue to interact with this resource or display" &
                     " accurate information about this resource. The error" &
                     " message was:" & vbCrLf & vbCrLf & ex.Message)
                End Try

                If Not relogin Then
                    'Get the current license...
                    clsLicenseQueries.RefreshLicense()
                End If
            Catch ex As Exception
                Debug.Assert(False, "Error while trying to read user details from the database: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Log in. This is the Windows Authentication version.
        ''' </summary>
        ''' <param name="machine">The name of the machine the login is from</param>
        ''' <param name="locale">The client locale</param>
        ''' <param name="server">The server to use instead of the standard gSv. Supplying
        ''' this implies a 'ReLogin' situation.</param>
        ''' <returns>A LoginResult describing the result</returns>
        Public Shared Function Login(machine As String, locale As String, Optional ByVal server As IServer = Nothing, Optional automateC As Boolean = False) As LoginResult
            Dim useserver As IServer = server
            If useserver Is Nothing Then useserver = gSv

            Dim userID As Guid
            Dim result As LoginResult = useserver.Login(machine, locale, userID)
            If result.IsSuccess Then
                SetLoggedIn(userID, AuthMode.ActiveDirectory, server, automateC)
                mLastSetMachineName = machine
                mLastUserLocale = locale
            End If
            Return result
        End Function

        ''' <summary>
        ''' Log in as an anonymous resource
        ''' </summary>
        ''' <param name="server">The server to use instead of the standard gSv. Supplying
        ''' this implies a 'ReLogin' situation.</param>
        ''' <returns>A LoginResult describing the result</returns>
        Public Shared Function LoginAsAnonResource(machine As String, Optional ByVal server As IServer = Nothing) As LoginResult
            Dim useserver As IServer = server
            If useserver Is Nothing Then useserver = gSv
            Dim result As LoginResult = useserver.LoginAsAnonResource(machine)
            If result.IsSuccess Then
                SetLoggedIn(result.UserID, AuthMode.Anonymous, server)
                mLastSetMachineName = machine
                mLastUserLocale = Threading.Thread.CurrentThread.CurrentUICulture.CompareInfo.Name
            End If
            Return result
        End Function

        ''' <summary>
        ''' Log in. This is the Blue Prism Authentication version.
        ''' </summary>
        ''' <param name="machine">The name of the machine the login is from</param>
        ''' <param name="username">The username</param>
        ''' <param name="password">The password</param>
        ''' <param name="locale">The client locale</param>
        ''' <param name="server">The server to use instead of the standard gSv. Supplying
        ''' this implies a 'ReLogin' situation.</param>
        ''' <returns>A LoginResult describing the result</returns>
        Public Shared Function Login(machine As String,
         ByVal username As String, ByVal password As SafeString, locale As String,
         Optional ByVal server As IServer = Nothing, Optional automateC As Boolean = False) As LoginResult
            Dim useserver As IServer = server
            If useserver Is Nothing Then useserver = gSv
            Dim result As LoginResult = useserver.Login(username, password, machine, locale)
            If result.IsSuccess Then
                SetLoggedIn(result.UserID, AuthMode.Native, server, automateC)
                mLastLoginUsername = username
                mLastLoginSecurePassword = password
                mLastSetMachineName = machine
                mLastUserLocale = locale
            End If
            Return result
        End Function

        Public Shared Function LoginWithMappedActiveDirectoryUser(machine As String, locale As String,
                                     Optional server As IServer = Nothing, Optional automateC As Boolean = False) As LoginResult
            Dim useServer = server
            If useServer Is Nothing Then useServer = gSv
            Dim result As LoginResult = useServer.LoginWithMappedActiveDirectoryUser(machine, locale)
            If result.IsSuccess Then
                SetLoggedIn(result.UserID, AuthMode.MappedActiveDirectory, server, automateC)
                mLastSetMachineName = machine
                mLastUserLocale = locale
            End If
            Return result
        End Function

        Public Shared Function LoginWithAccessToken(machine As String, accessToken As String, locale As String,
                                     Optional server As IServer = Nothing) As LoginResult
            Dim useServer = server
            If useServer Is Nothing Then useServer = gSv
            Dim processId = Process.GetCurrentProcess().Id
            Dim tokenRequest = New ReloginTokenRequest(machine, processId, Nothing)
            Dim result As LoginResultWithReloginToken = useServer.Login(machine, locale, accessToken, tokenRequest)
            Dim loginResult = result.LoginResult
            If loginResult.IsSuccess Then
                SetLoggedIn(loginResult.UserID, AuthMode.AuthenticationServer, server)
                mLastSetMachineName = machine
                mLastUserLocale = locale
                SaveReloginToken(result.ReloginToken)
                StartReloginTokenUpdate()
            End If
            Return result.LoginResult
        End Function

        Private Shared Function LoginWithReloginToken(machine As String, locale As String, Optional server As IServer = Nothing) As LoginResult
            Dim useServer = server
            If useServer Is Nothing Then useServer = gSv
            Dim processId = Process.GetCurrentProcess().Id
            Dim tokenRequest = New ReloginTokenRequest(machine, processId, ReloginToken)
            Dim result As LoginResultWithReloginToken = useServer.LoginWithReloginToken(locale, tokenRequest)
            Dim loginResult = result.LoginResult
            If loginResult.IsSuccess Then
                SetLoggedIn(loginResult.UserID, AuthMode.AuthenticationServer, server)
                mLastSetMachineName = machine
                mLastUserLocale = locale
                SaveReloginToken(result.ReloginToken)
                StopReloginTokenUpdate()
                StartReloginTokenUpdate()
            End If
            Return loginResult
        End Function

        ''' <summary>
        ''' Log out the currently logged-in user.
        ''' </summary>
        Public Shared Sub Logout()
            SetLoggedOut(False)
            SystemRoleSet.Reset(False)
            gSv.Logout()
            clsLicenseQueries.UnloadLicense()
        End Sub

        ''' <summary>
        ''' True if the user is assigned the System Administrator role.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsSystemAdmin() As Boolean Implements IUser.IsSystemAdmin
            Get
                Return Roles.Any(Function(r) r.SystemAdmin)
            End Get
        End Property


        ''' <summary>
        ''' Consider the current login cancelled, and repeat it if necessary. This is
        ''' used when the underlying clsServer connection has to be refreshed. It
        ''' should only be called in that scenario.
        ''' </summary>
        Friend Shared Sub ReLogin(ByVal server As IServer)

            Dim u As User = sCurrent
            Dim mode As AuthMode = sAuthMode

            ' Get the user and password and machinename before calling SetLoggedOut
            ' which will clear them
            Dim lastUser = mLastLoginUsername
            Dim lastPassword = mLastLoginSecurePassword
            Dim lastMachine = mLastSetMachineName
            Dim lastLocale = mLastUserLocale
            Dim lastDatabaseType = mDatabaseType

            ' If we think we're logged in, we're not!
            ' Don't clear the keepalive flag - we're not actually logged in with
            ' the new clsServer instance so it would fail if we tried to.
            If u IsNot Nothing Then SetLoggedOut(True)

            Try
                Select Case mode
                    Case AuthMode.ActiveDirectory
                        Login(lastMachine, lastLocale, server)
                    Case AuthMode.Native
                        If lastUser IsNot Nothing AndAlso lastPassword IsNot Nothing Then
                            Login(lastMachine, lastUser, lastPassword, lastLocale, server)
                        End If
                    Case AuthMode.Anonymous
                        LoginAsAnonResource(lastMachine, server)
                    Case AuthMode.MappedActiveDirectory
                        LoginWithMappedActiveDirectoryUser(lastMachine, lastLocale, server)
                    Case AuthMode.AuthenticationServer
                        If ReloginToken IsNot Nothing Then
                            LoginWithReloginToken(lastMachine, lastLocale, server)
                        End If
                End Select
            Catch ex As Exception
                ' On any errors, ensure we restore the data we need for further
                ' relogins when the connection is fully re-established
                sCurrent = u
                sAuthMode = mode
                mDatabaseType = lastDatabaseType
                mLastLoginUsername = lastUser
                mLastLoginSecurePassword = lastPassword
                mLastSetMachineName = lastMachine
                mLastUserLocale = lastLocale
                ' Ignore UnknownLoginExceptions... I'm not sure why, but that was
                ' the previous behaviour and I'm loath to alter it for a patch
                If TypeOf ex IsNot UnknownLoginException Then Throw
            End Try

        End Sub

#End Region

#End Region

#Region " Member Variables "

        ' The authmode used by the user
        <DataMember>
        Private mAuthMode As AuthMode

        ' The user id
        <DataMember>
        Private mId As Guid

        ' The name of the user on the database
        <DataMember>
        Private mName As String

        ' The roles assigned to the user
        <DataMember>
        Private WithEvents mRoles As RoleSet

        ' The date that this user was created
        <DataMember>
        Private mCreated As Date

        ' The expiry date for the user
        <DataMember>
        Private mExpiry As Date

        ' The expiry date for their password
        <DataMember>
        Private mPasswordExpiry As Date

        ' Flag indicating if this user is deleted or not
        <DataMember>
        Private mDeleted As Boolean

        ' The number of weeks until a password expires
        <DataMember>
        Private mPasswordDurationWeeks As Integer

        ' The types of notification that the user has registered
        <DataMember>
        Private mNotifications As AlertNotificationType

        ' The alerts that the user is subscribed to
        <DataMember>
        Private mSubscribedAlerts As AlertEventType

        ' The number of days prior to account/password expiry that
        ' the a warning is issued
        <DataMember>
        Private mExpiryWarningInterval As TimeSpan

        ' The external user provider Id
        <DataMember>
        Private mExternalId As String

        <DataMember>
        Private mIdentityProviderName As String

        <DataMember>
        Private mIdentityProviderType As String

        <DataMember>
        Private mAuthenticationServerUserId As Guid?

        <DataMember>
        Private mAuthenticationServerClientId As String

        <DataMember>
        Private mDeletedLastSynchronizationDate As DateTimeOffset?

        <DataMember>
        Private mUpdatedLastSynchronizationDate As DateTimeOffset?

        <DataMember>
        Private mHasBluePrismApiScope As Boolean

        <DataMember>
        Private mAuthServerName As String
#End Region

#Region " Auto-properties "

        ''' <summary>
        ''' The date/time that the user logged into this session (UTC).
        ''' </summary>
        <DataMember>
        Public Property SignedInAt As Date Implements IUser.SignedInAt

        ''' <summary>
        ''' The date/time that the user logged into their previous session (UTC).
        ''' </summary>
        <DataMember>
        Public Property LastSignedInAt As Date Implements IUser.LastSignedInAt

        ''' <summary>
        ''' Indicates whether or not this user's account is currently locked.
        ''' </summary>
        <DataMember>
        Public Property IsLocked As Boolean Implements IUser.IsLocked


        <DataMember>
        Public Property AuthorisationToken As clsAuthToken Implements IUser.AuthorisationToken


#End Region

#Region " Constructors "


        Private Sub New(authMode As AuthMode, userid As Guid, name As String, roles As RoleSet,
                        created As Date, expiry As Date, passwordExpiry As Date,
                        subscribed As AlertEventType, notif As AlertNotificationType,
                        lastSignedIn As Date, isDeleted As Boolean, expiryWarning As Integer,
                        locked As Boolean, externalId As String, identityProviderName As String,
                        identityProviderType As String, authenticationServerUserId As Guid?,
                        authenticationServerClientId As String, hasBluePrismApiScope As Boolean,
                        deletedLastSynchronizationDate As DateTimeOffset?,
                        updatedLastSynchronizationDate As DateTimeOffset?,
                        authServerName As String
                        )
            mId = userid
            mName = name
            mAuthMode = authMode
            mRoles = roles
            mCreated = created
            mExpiry = expiry
            mPasswordExpiry = passwordExpiry
            mSubscribedAlerts = subscribed
            mNotifications = notif
            LastSignedInAt = lastSignedIn
            mDeleted = isDeleted
            mExpiryWarningInterval = TimeSpan.FromDays(expiryWarning)
            IsLocked = locked
            mPasswordDurationWeeks = DefaultPasswordDurationWeeks
            mExternalId = externalId
            mIdentityProviderName = identityProviderName
            mIdentityProviderType = identityProviderType
            mAuthenticationServerUserId = authenticationServerUserId
            mAuthenticationServerClientId = authenticationServerClientId
            mHasBluePrismApiScope = hasBluePrismApiScope
            mDeletedLastSynchronizationDate = deletedLastSynchronizationDate
            mUpdatedLastSynchronizationDate = updatedLastSynchronizationDate
            mAuthServerName = authServerName
        End Sub

        Public Sub New(authMode As AuthMode, userId As Guid, ByVal name As String, roles As RoleSet)
            Me.New(authMode, userId, name, roles,
             Date.UtcNow, DefaultExpiry, DefaultPasswordExpiry,
             AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
             0, False, String.Empty, String.Empty, String.Empty, Nothing, String.Empty, False, Nothing, Nothing, Nothing)
        End Sub

        Public Sub New(authMode As AuthMode, userId As Guid, name As String)
            Me.New(authMode, userId, name, Nothing,
             Date.UtcNow, DefaultExpiry, DefaultPasswordExpiry,
             AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
             0, False, String.Empty, String.Empty, String.Empty, Nothing, String.Empty, False, Nothing, Nothing, Nothing)
        End Sub

        Public Sub New(userId As Guid, name As String, externalId As String, identityProviderName As String, identityProviderType As String, roles As RoleSet)
            Me.New(AuthMode.External, userId, name, roles,
                   Date.UtcNow, Date.MaxValue, Date.MaxValue,
                   AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
                   0, False, externalId, identityProviderName, identityProviderType, Nothing, String.Empty, False, Nothing, Nothing, Nothing)
        End Sub

        Public Sub New(userId As Guid, name As String, authenticationServerUserId As Guid, authServerName As String, Optional deletedLastSynchronizationDate As DateTimeOffset? = Nothing)
            Me.New(AuthMode.AuthenticationServer, userId, name, Nothing,
                   Date.UtcNow, DefaultExpiry, DefaultPasswordExpiry,
                   AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
                   0, False, String.Empty, String.Empty, String.Empty, authenticationServerUserId, String.Empty, False, deletedLastSynchronizationDate, Nothing, authServerName)
        End Sub

        Public Sub New(userId As Guid, authenticationServerClientName As String, authenticationServerClientId As String, hasBluePrismApiScope As Boolean, authServerName As String)
            Me.New(AuthMode.AuthenticationServerServiceAccount, userId, authenticationServerClientName, Nothing,
                   Date.UtcNow, DefaultExpiry, DefaultPasswordExpiry,
                   AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
                   0, False, String.Empty, String.Empty, String.Empty, Nothing, authenticationServerClientId, hasBluePrismApiScope, Nothing, Nothing, authServerName)
        End Sub

        Public Shared Function CreateEmptyMappedActiveDirectoryUser(userId As Guid) As User
            Return New User(AuthMode.MappedActiveDirectory, userId, String.Empty, Nothing,
                   Date.UtcNow, Date.MaxValue, Date.MaxValue,
                   AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
                   0, False, String.Empty, String.Empty, String.Empty, Nothing, Nothing, False, Nothing, Nothing, Nothing)
        End Function

        Public Shared Function CreateEmptyExternalUser(userId As Guid) As User
            Return New User(AuthMode.External, userId, String.Empty, Nothing,
                   Date.UtcNow, Date.MaxValue, Date.MaxValue,
                   AlertEventType.None, AlertNotificationType.None, DateTime.MinValue, False,
                   0, False, String.Empty, String.Empty, String.Empty, Nothing, Nothing, False, Nothing, Nothing, Nothing)
        End Function

        Friend Sub New(prov As IDataProvider)
            Me.New(
                prov.GetValue("authtype", AuthMode.Unspecified),
                prov.GetValue("userid", Guid.Empty),
                prov.GetString("username"),
                Nothing,
                prov.GetValue("created", Date.UtcNow),
                prov.GetValue("expiry", BPUtil.DateMinValueUtc),
                prov.GetValue("passwordexpiry", BPUtil.DateMinValueUtc),
                prov.GetValue("alerteventtypes", AlertEventType.None),
                prov.GetValue("alertnotificationtypes", AlertNotificationType.None),
                prov.GetValue("lastsignedin", Date.MinValue),
                prov.GetValue("isdeleted", False),
                prov.GetValue("passwordexpirywarninginterval", 0),
                prov.GetValue("locked", False),
                String.Empty,
                String.Empty,
                String.Empty,
                prov.GetValue("authenticationServerUserId", CType(Nothing, Guid?)),
                prov.GetString("authenticationServerClientId"),
                prov.GetBool("hasBluePrismApiScope"),
                prov.GetValue("deletedLastSynchronizationDate", CType(Nothing, DateTimeOffset?)),
                prov.GetValue("updatedLastSynchronizationDate", CType(Nothing, DateTimeOffset?)),
                 prov.GetValue("authServerName", CStr(Nothing)))

            mPasswordDurationWeeks =
             prov.GetValue("passworddurationweeks", DefaultPasswordDurationWeeks)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The ID of this user.
        ''' </summary>
        Public ReadOnly Property Id() As Guid Implements IUser.Id
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The username of this user.
        ''' </summary>
        Public Property Name() As String Implements IUser.Name
            Get
                Return mName
            End Get
            Set(ByVal value As String)
                ChangeData("Name", mName, value)
            End Set
        End Property

        Public ReadOnly Property AuthType As AuthMode Implements IUser.AuthType
            Get
                Return mAuthMode
            End Get
        End Property

        ''' <summary>
        ''' Boolean indicating whether the user is a hidden
        ''' </summary>
        Public ReadOnly Property IsHidden As Boolean Implements IUser.IsHidden
            Get
                Return mName Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' The roles currently assigned to this user.
        ''' </summary>
        Public ReadOnly Property Roles() As RoleSet Implements IUser.Roles
            Get
                If mRoles Is Nothing Then mRoles = New RoleSet()
                Return mRoles
            End Get
        End Property

        ''' <summary>
        ''' The permissions in effect for this user.
        ''' </summary>
        Public ReadOnly Property EffectivePermissions() As ICollection(Of Permission) Implements IUser.EffectivePermissions
            Get
                Return Roles.EffectivePermissions
            End Get
        End Property

        ''' <summary>
        ''' The date/time that this user was created.
        ''' </summary>
        Public Property Created() As Date Implements IUser.Created
            Get
                Return mCreated
            End Get
            Set(ByVal value As Date)
                ChangeData("Created", mCreated, value.Date)
            End Set
        End Property

        ''' <summary>
        ''' Gets the formatted local date/time that this user was created
        ''' </summary>
        Public ReadOnly Property CreatedOptionalDisplayDate As String Implements IUser.CreatedOptionalDisplayDate
            Get
                Dim createdTime = BPUtil.ConvertAndFormatUtcDateTime(Created)
                Return If(IsAuthenticationServerUserOrServiceAccount() OrElse createdTime = String.Empty,
                          My.Resources.UserDisplayDateIsNotApplicable,
                          createdTime)
            End Get
        End Property

        ''' <summary>
        ''' The date/time that this user expires
        ''' </summary>
        Public Property Expiry() As Date Implements IUser.Expiry
            Get
                Return mExpiry
            End Get
            Set(ByVal value As Date)
                ChangeData("Expiry", mExpiry, value.Date)
            End Set
        End Property

        ''' <summary>
        ''' The date/time that this user's password expires
        ''' </summary>
        Public Property PasswordExpiry() As Date Implements IUser.PasswordExpiry
            Get
                Return mPasswordExpiry
            End Get
            Set(ByVal value As Date)
                ChangeData("PasswordExpiry", mPasswordExpiry, value.Date)
            End Set
        End Property

        ''' <summary>
        ''' Display string for the expiry date for this user
        ''' </summary>
        Public ReadOnly Property ExpiryDisplay() As String Implements IUser.ExpiryDisplay
            Get
                Return Expiry.ToString("d")
            End Get
        End Property

        Public ReadOnly Property ExpiryOptionalDisplayDate() As String
            Get
                Return If(AccountNeverExpires(),
                          My.Resources.UserDisplayDateIsNotApplicable,
                          ExpiryDisplay())
            End Get
        End Property

        ''' <summary>
        ''' Display value of the password expiry date for this user.
        ''' </summary>
        Public ReadOnly Property PasswordExpiryDisplay() As String Implements IUser.PasswordExpiryDisplay
            Get
                Return PasswordExpiry.ToString("d")
            End Get
        End Property

        Public ReadOnly Property PasswordExpiryOptionalDisplayDate() As String
            Get
                Return If(AccountNeverExpires(),
                          My.Resources.UserDisplayDateIsNotApplicable,
                          PasswordExpiryDisplay())
            End Get
        End Property

        ''' <summary>
        ''' Account never expires for these authentication types.
        ''' </summary>
        Public ReadOnly Property AccountNeverExpires() As Boolean
            Get
                Return mAuthMode = AuthMode.External OrElse
                       mAuthMode = AuthMode.ActiveDirectory OrElse
                       mAuthMode = AuthMode.MappedActiveDirectory OrElse
                       IsAuthenticationServerUserOrServiceAccount()

            End Get
        End Property

        Public ReadOnly Property IsAuthenticationServerUserOrServiceAccount() As Boolean Implements IUser.IsAuthenticationServerUserOrServiceAccount
            Get
                Return mAuthMode = AuthMode.AuthenticationServer OrElse
                       mAuthMode = AuthMode.AuthenticationServerServiceAccount
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this user has expired or not
        ''' </summary>
        Public ReadOnly Property Expired() As Boolean Implements IUser.Expired
            Get
                Return mExpiry < Date.UtcNow
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this user's password has expired or not
        ''' </summary>
        Public ReadOnly Property PasswordExpired() As Boolean Implements IUser.PasswordExpired
            Get
                Return mPasswordExpiry < Date.UtcNow
            End Get
        End Property

        ''' <summary>
        ''' Gets the duration of the password's validity for this user
        ''' </summary>
        Public Property PasswordDurationWeeks() As Integer Implements IUser.PasswordDurationWeeks
            Get
                Return mPasswordDurationWeeks
            End Get
            Set(ByVal value As Integer)
                ChangeData("PasswordDurationWeeks", mPasswordDurationWeeks, value)
            End Set
        End Property

        ''' <summary>
        ''' The alerts that this user is subscribed to
        ''' </summary>
        Public Property SubscribedAlerts() As AlertEventType Implements IUser.SubscribedAlerts
            Get
                Return mSubscribedAlerts
            End Get
            Set(ByVal value As AlertEventType)
                ChangeData("SubscribedAlerts", mSubscribedAlerts, value)
            End Set
        End Property

        ''' <summary>
        ''' The alert notifications that this user has registered for
        ''' </summary>
        Public Property AlertNotifications() As AlertNotificationType Implements IUser.AlertNotifications
            Get
                Return mNotifications
            End Get
            Set(ByVal value As AlertNotificationType)
                ChangeData("AlertNotifications", mNotifications, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets whether this user is marked as deleted or not.
        ''' </summary>
        Public Property Deleted() As Boolean Implements IUser.Deleted
            Get
                Return mDeleted
            End Get
            Set(ByVal value As Boolean)
                ChangeData("Delete", mDeleted, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the formatted local date/time that this session started
        ''' </summary>
        Public ReadOnly Property SignedInAtDisplay As String Implements IUser.SignedInAtDisplay
            Get
                Return BPUtil.ConvertAndFormatUtcDateTime(SignedInAt)
            End Get
        End Property

        ''' <summary>
        ''' Gets the formatted local date/time that the user last signed in
        ''' </summary>
        Public ReadOnly Property LastSignedInAtOptionalDisplayDate As String Implements IUser.LastSignedInAtOptionalDisplayDate
            Get
                Dim lastSignedInTime = BPUtil.ConvertAndFormatUtcDateTime(LastSignedInAt)
                Return If(mAuthMode = AuthMode.AuthenticationServerServiceAccount OrElse lastSignedInTime = String.Empty,
                          My.Resources.UserDisplayDateIsNotApplicable,
                          lastSignedInTime)
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether or not this user account will expire within the next n
        ''' days (as configured in System Manager)
        ''' </summary>
        Public ReadOnly Property AccountExpiresSoon() As Boolean Implements IUser.AccountExpiresSoon
            Get
                If (AccountNeverExpires()) Then
                    Return False
                End If

                Return (mExpiryWarningInterval <> TimeSpan.Zero AndAlso
                        Expiry - mExpiryWarningInterval <= Today)
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether or not this user's password will expire within the next
        ''' n days (as configured in System Manager)
        ''' </summary>
        Public ReadOnly Property PasswordExpiresSoon() As Boolean Implements IUser.PasswordExpiresSoon
            Get
                If (AccountNeverExpires()) Then
                    Return False
                End If

                Return (mExpiryWarningInterval <> TimeSpan.Zero AndAlso
                        PasswordExpiry - mExpiryWarningInterval <= Today)
            End Get
        End Property

        Public Property ExternalId() As String Implements IUser.ExternalId
            Get
                Return mExternalId
            End Get
            Set(ByVal value As String)
                ChangeData("ExternalId", mExternalId, value)
            End Set
        End Property

        Public Property IdentityProviderName As String Implements IUser.IdentityProviderName
            Get
                Return mIdentityProviderName
            End Get
            Set(ByVal value As String)
                ChangeData("IdentityProviderName", mIdentityProviderName, value)
            End Set
        End Property

        Public Property IdentityProviderType As String Implements IUser.IdentityProviderType
            Get
                Return mIdentityProviderType
            End Get
            Set(ByVal value As String)
                ChangeData("IdentityProviderType", mIdentityProviderType, value)
            End Set
        End Property

        Public ReadOnly Property AuthenticationServerUserId As Guid? Implements IUser.AuthenticationServerUserId
            Get
                Return mAuthenticationServerUserId
            End Get
        End Property

        Public ReadOnly Property AuthenticationServerClientId As String Implements IUser.AuthenticationServerClientId
            Get
                Return mAuthenticationServerClientId
            End Get
        End Property

        Public Property DeletedLastSynchronizationDate() As DateTimeOffset? Implements IUser.DeletedLastSynchronizationDate
            Get
                Return mDeletedLastSynchronizationDate
            End Get
            Set
                ChangeData("DeletedLastSynchronizationDate", mDeletedLastSynchronizationDate, Value)
            End Set
        End Property

        Public Property UpdatedLastSynchronizationDate() As DateTimeOffset? Implements IUser.UpdatedLastSynchronizationDate
            Get
                Return mUpdatedLastSynchronizationDate
            End Get
            Set
                ChangeData("UpdatedLastSynchronizationDate", mUpdatedLastSynchronizationDate, Value)
            End Set
        End Property

        Public Property HasBluePrismApiScope As Boolean Implements IUser.HasBluePrismApiScope
            Get
                Return mHasBluePrismApiScope
            End Get
            Set(value As Boolean)
                mHasBluePrismApiScope = value
            End Set

        End Property

        Public Property AuthServerName As String Implements IUser.AuthServerName
            Get
                Return mAuthServerName
            End Get
            Set(value As String)
                mAuthServerName = value
            End Set

        End Property

#End Region

#Region " Permission Queries "

        ''' <summary>
        ''' Checks if this user has any of the named permissions available to it
        ''' </summary>
        ''' <param name="permNames">The names of the permissions to check for.
        ''' </param>
        ''' <returns>True if any of the required permissions are available to this
        ''' user; False if none of them are. Note that if
        ''' <paramref name="permNames"/> is null or empty, that is treated as 'all
        ''' users have permission' - ie. this method will return <c>True</c>.
        ''' </returns>
        Public Function HasPermission(ByVal ParamArray permNames() As String) _
         As Boolean Implements IUser.HasPermission
            Return HasPermission(DirectCast(permNames, ICollection(Of String)))
        End Function

        ''' <summary>
        ''' Checks if this user has any of the named permissions available to it
        ''' </summary>
        ''' <param name="permNames">The names of the permissions to check for.
        ''' </param>
        ''' <returns>True if any of the required permissions are available to this
        ''' user; False if none of them are. Note that if
        ''' <paramref name="permNames"/> is null or empty, that is treated as 'all
        ''' users have permission' - ie. this method will return <c>True</c>.
        ''' </returns>
        Public Function HasPermission(ByVal permNames As ICollection(Of String)) _
         As Boolean Implements IUser.HasPermission
            Return HasPermission(Permission.ByName(permNames))
        End Function

        ''' <summary>
        ''' Checks if this user has any of the given permissions available to it
        ''' </summary>
        ''' <param name="perms">The permissions to check for.</param>
        ''' <returns>True if any of the required permissions are available to this
        ''' user; False if none of them are. Note that if <paramref name="perms"/> is
        ''' null or empty, that is treated as 'all users have permission' - ie. this
        ''' method will return <c>True</c>.</returns>
        ''' <exception cref="ArgumentException">If no permission names were provided
        ''' in <paramref name="perms"/>.</exception>
        Public Function HasPermission(ByVal ParamArray perms() As Permission) _
         As Boolean Implements IUser.HasPermission
            Return HasPermission(DirectCast(perms, ICollection(Of Permission)))
        End Function

        ''' <summary>
        ''' Checks if this user has any of the given permissions available to it
        ''' </summary>
        ''' <param name="perms">The permissions to check for.</param>
        ''' <returns>True if any of the required permissions are available to this
        ''' user; False if none of them are. Note that if <paramref name="perms"/> is
        ''' null or empty, that is treated as 'all users have permission' - ie. this
        ''' method will return <c>True</c>.</returns>
        Public Function HasPermission(ByVal perms As ICollection(Of Permission)) _
         As Boolean Implements IUser.HasPermission
            ' If the perms param is empty, that means all users are permitted
            If CollectionUtil.IsNullOrEmpty(perms) Then Return True
            ' Otherwise the user's perms must contain any of the given perms
            Return CollectionUtil.ContainsAny(EffectivePermissions, perms)
        End Function

        ''' <summary>
        ''' Checks if this user has permission to the given constraint. If a null
        ''' constraint is given, this is treated as 'no constraint', ie. the user has
        ''' permission because there are no constraints on them.
        ''' </summary>
        ''' <param name="constraint">The constraint which determines the required
        ''' permissions for a user.</param>
        ''' <returns>True if the constraint (or lack thereof) implies that all users
        ''' are allowed, or if this user has any of the required permissions that the
        ''' constraint dictates must be present.</returns>
        Public Function HasPermission(ByVal constraint As IPermission) As Boolean Implements IUser.HasPermission
            Return (constraint Is Nothing _
             OrElse HasPermission(constraint.RequiredPermissions))
        End Function

        ''' <summary>
        ''' Checks if this user has all of the named permissions available to it
        ''' </summary>
        ''' <param name="permNames">The names of the permissions to check for.
        ''' </param>
        ''' <returns>True if all of the required permissions are available to this
        ''' user; False if any of them are not.</returns>
        ''' <exception cref="ArgumentException">If no permission names were provided
        ''' in <paramref name="permNames"/>.</exception>
        Public Function HasAllPermissions(ByVal ParamArray permNames() As String) _
         As Boolean Implements IUser.HasAllPermissions
            Return HasAllPermissions(DirectCast(permNames, ICollection(Of String)))
        End Function

        ''' <summary>
        ''' Checks if this user has all of the named permissions available to it
        ''' </summary>
        ''' <param name="permNames">The names of the permissions to check for.
        ''' </param>
        ''' <returns>True if all of the required permissions are available to this
        ''' user; False if any of them are not.</returns>
        ''' <exception cref="ArgumentException">If no permission names were provided
        ''' in <paramref name="permNames"/>.</exception>
        Public Function HasAllPermissions(ByVal permNames As ICollection(Of String)) _
         As Boolean Implements IUser.HasAllPermissions
            If CollectionUtil.IsNullOrEmpty(permNames) Then _
             Throw New ArgumentException(My.Resources.User_NoPermissionsProvidedToCheck)
            Return CollectionUtil.ContainsAll(
             EffectivePermissions, Permission.ByName(permNames))
        End Function

        Public Function HasPermissionToImportFile(filePath As String) As Boolean Implements IUser.HasPermissionToImportFile
            If String.IsNullOrEmpty(filePath) Then  Throw New ArgumentException(My.Resources.User_NoPermissionsProvidedToCheck)

            Dim fileExtension = Path.GetExtension(filePath).Replace(".", "").Trim()

            Dim requiredPermission = String.Empty

            Select Case True
                Case fileExtension.Equals(clsProcess.ObjectFileExtension, StringComparison.InvariantCultureIgnoreCase)
                    requiredPermission = Permission.ObjectStudio.ImportBusinessObject
                Case fileExtension.Equals(clsProcess.ProcessFileExtension, StringComparison.InvariantCultureIgnoreCase)
                    requiredPermission = Permission.ProcessStudio.ImportProcess
                Case fileExtension.Equals(clsRelease.FileExtension, StringComparison.InvariantCultureIgnoreCase)
                    requiredPermission = Permission.ReleaseManager.ImportRelease
                Case fileExtension.Equals(Skill.FileExtension, StringComparison.InvariantCultureIgnoreCase)
                    requiredPermission = Permission.Skills.ImportSkill
            End Select

            Return Not String.IsNullOrEmpty(requiredPermission) AndAlso HasPermission(requiredPermission)
        End Function

        ''' <summary>
        ''' Check if this user has the given role
        ''' </summary>
        ''' <param name="role">The role to check</param>
        ''' <returns>True of the user has this role</returns>
        Public Function HasRole(role As Role) As Boolean Implements IUser.HasRole
            Return mRoles.Contains(role)
        End Function

        ''' <summary>
        ''' Check if this user has the given role
        ''' </summary>
        ''' <param name="role">The role to check</param>
        ''' <returns>True of the user has this role</returns>
        Public Function HasRole(role As Integer) As Boolean Implements IUser.HasRole
            Return mRoles.Contains(role)
        End Function

        Public Function CanSeeTree(treeType As GroupTreeType) As Boolean Implements IUser.CanSeeTree
            Select Case treeType
                Case GroupTreeType.None,
                    GroupTreeType.Queues,
                    GroupTreeType.Tiles,
                    GroupTreeType.Users
                    Return True
                Case GroupTreeType.Processes
                    Return Me.HasPermission(Permission.ProcessStudio.AllProcessPermissionsAllowingTreeView)
                Case GroupTreeType.Objects
                    Return Me.HasPermission(Permission.ObjectStudio.AllObjectPermissionsAllowingTreeView)
                Case GroupTreeType.Resources
                    Return Me.HasPermission(Permission.Resources.ImpliedViewResource)
                Case Else
                    Return False
            End Select

        End Function

        Private Shared Sub SaveReloginToken(newReloginToken As SafeString)
            ClearReloginToken()
            ReloginToken = newReloginToken
        End Sub

        Private Shared Sub ClearReloginToken()
            ReloginToken?.Dispose()
            ReloginToken = Nothing
        End Sub

        Private Shared Sub StartReloginTokenUpdate()
            ReloginTokenUpdater.Change(TimeSpan.FromMinutes(2.5), TimeSpan.FromMinutes(2.5))
        End Sub

        Private Shared Sub StopReloginTokenUpdate()
            ReloginTokenUpdater.Change(Timeout.Infinite, Timeout.Infinite)
        End Sub

        Private Shared Sub UpdateReloginTokenThread(state As Object)
            Try
                Dim processId = Process.GetCurrentProcess().Id
                Dim tokenRequest = New ReloginTokenRequest(mLastSetMachineName, processId, ReloginToken)
                ReloginToken = gSv.RefreshReloginToken(tokenRequest)
            Catch ex As Exception
                StopReloginTokenUpdate()
            End Try
        End Sub
#End Region

#Region " Other Methods "

        ''' <summary>
        ''' Checks if this user is an alert subscriber or not - ie. that they have
        ''' the <c>"Subscribe to Process Alerts"</c> permission and they are
        ''' registered as a subscriber to any actual alerts.
        ''' </summary>
        ''' <returns>True if this user is permitted to subscribe to process alerts
        ''' and they are actually subscribed to some.</returns>
        Public Function IsAlertSubscriber() As Boolean Implements IUser.IsAlertSubscriber
            Return HasPermission("Subscribe to Process Alerts") _
             AndAlso mSubscribedAlerts <> AlertEventType.None _
             AndAlso mNotifications <> AlertNotificationType.None
        End Function

        ''' <summary>
        ''' Handles roles being added or removed from this user.
        ''' Note that it doesn't handle renames - the role ID should remain the same
        ''' so it has no direct effect on the user's privileges.
        ''' </summary>
        Private Sub HandleRolesetChanged(
         ByVal sender As Object, ByVal e As RoleEventArgs) _
            Handles mRoles.RoleAdded, mRoles.RoleRemoved

            ' We can't really pass the 'oldValue'/'newValue' stuff (well, we could
            ' but it's fiddly and all we need to know is that 'the roles changed')
            MarkDataChanged("Roles", Nothing, mRoles)
        End Sub

        ''' <summary>
        ''' Re-establish the links between roles object and it's event handler.
        ''' Added as this seems to get lost over WCF.
        ''' </summary>
        ''' <param name="context"></param>
        <OnDeserialized()>
        Private Sub OnDeserialized(ByVal context As StreamingContext)
            If mRoles IsNot Nothing AndAlso Not mRoles.HandlersDefined Then
                AddHandler mRoles.RoleAdded, AddressOf Me.HandleRolesetChanged
                AddHandler mRoles.RoleRemoved, AddressOf Me.HandleRolesetChanged
            End If
        End Sub

        Public Shared Event OnCheckIfUserCanLogout As UserEventHandler
        Public Shared Function CurrentUserCanLogout(ByRef logoutDeniedMessage As String) As Boolean
            Dim args = New UserEventArgs(Current) With {.LogoutDenied = False, .logoutDeniedMessage = String.Empty}
            RaiseEvent OnCheckIfUserCanLogout(Nothing, args)
            logoutDeniedMessage = args.LogoutDeniedMessage
            Return Not args.LogoutDenied
        End Function


#End Region

    End Class

End Namespace
