Imports System.Threading
Imports System.Data.SqlClient
Imports System.DirectoryServices.AccountManagement
Imports System.DirectoryServices.ActiveDirectory
Imports BluePrism.Core.ActiveDirectory.UserQuery
Imports System.Security.Principal
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Common.Security
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Caching
Imports BluePrism.Utilities.Functional
Imports BluePrism.Data
Imports LocaleTools
Imports Microsoft.IdentityModel.Tokens
Imports System.IdentityModel.Tokens.Jwt
Imports BluePrism.Core.WindowsSecurity
Imports BluePrism.Core.ActiveDirectory
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Core.ActiveDirectory.DirectoryServices
Imports Autofac
Imports BluePrism.AutomateAppCore.My.Resources
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer
    Private Const UserOperationBatchSize As Integer = 10
    Private Shared ReadOnly ReloginTokenLifetime As TimeSpan = TimeSpan.FromMinutes(10)
    Private mValidateReloginToken As Func(Of IDatabaseConnection, ReloginTokenRequest, Guid) = Function(con, tokenRequest) ValidateReloginToken(con, tokenRequest)
    Private mGetActiveAuthServerServiceAccountUser As Func(Of IDatabaseConnection, String, User) = Function(con, authServerClientId) GetActiveAuthServerServiceAccountUser(con, authServerClientId)
    ''' <summary>
    ''' The type of user supported - either login or system.
    ''' Used when retrieving all user names.
    ''' </summary>
    Private Enum UserType As Integer
        Login = 1
        System = 2
        All = Login Or System
    End Enum

    Private ReadOnly mTryGetUserId As Func(Of IDatabaseConnection, String, Guid) =
        Function(con, username) TryGetUserID(con, username)

    Private ReadOnly mGetUserByAuthenticationServerUserId As Func(Of IDatabaseConnection, Guid, User) =
        Function(con, authenticationServerUserId) GetUserByAuthenticationServerId(con, authenticationServerUserId)

    Private ReadOnly mUpdateRetireAuthenticationServerUser As Action(Of IDatabaseConnection, DateTimeOffset, Boolean, Guid) =
       Sub(con, synchronizationDate, retired, id) UpdateRetireAuthenticationServerUser(con, synchronizationDate, retired, id)

    Private ReadOnly mGetUserById As Func(Of IDatabaseConnection, Guid, User) =
        Function(con, userId) GetUser(con, userId, Nothing)

    Private ReadOnly mGetUserByClientId As Func(Of IDatabaseConnection, String, User) =
        Function(con, clientId) GetUserByAuthenticationServerClientId(con, clientId)

    Private ReadOnly mUpdateMappedUser As Action(Of IDatabaseConnection, User) =
        Sub(con, user) UpdateUserDetailsForMappedUser(con, user)


    Private Function CreateDatabaseCache(Of TValue)(name As String, eventHandler As OnRefreshRequiredDelegate) _
        As Func(Of IRefreshCache(Of String, TValue))

        Return _
            Function()
                Dim cache =
                    mDependencyResolver.Resolve(Of ICacheFactory)().
                        GetInMemoryCacheWithDatabaseRefresh(Of TValue)(
                            name,
                            mDBConnectionSetting.GetConnectionString(),
                            5000)

                AddHandler cache.OnRefreshRequired, eventHandler

                Return cache
            End Function

    End Function

    Private ReadOnly mEffectiveGroupPermissionsCache As Lazy(Of IRefreshCache(Of String, IGroupPermissions)) =
        New Lazy(Of IRefreshCache(Of String, IGroupPermissions))(
            CreateDatabaseCache(Of IGroupPermissions)(
                "EffectiveGroupPermissions",
                Sub(s, e)
                End Sub))

    Private ReadOnly mActualGroupPermissionsCache As Lazy(Of IRefreshCache(Of String, IGroupPermissions)) =
        New Lazy(Of IRefreshCache(Of String, IGroupPermissions))(
            CreateDatabaseCache(Of IGroupPermissions)(
                "ActualGroupPermissions",
                AddressOf RefreshGroupPermissionsCache))

    Private ReadOnly mProcessGroupsCache As Lazy(Of IRefreshCache(Of String, List(Of Guid))) =
        New Lazy(Of IRefreshCache(Of String, List(Of Guid)))(
            CreateDatabaseCache(Of List(Of Guid))(
                "ProcessGroups",
                AddressOf RefreshProcessGroupsCache))

    Private ReadOnly mResourceGroupsCache As Lazy(Of IRefreshCache(Of String, List(Of Guid))) =
        New Lazy(Of IRefreshCache(Of String, List(Of Guid)))(
            CreateDatabaseCache(Of List(Of Guid))(
                "ResourceGroups",
                AddressOf RefreshResourceGroupsCache))

    Private Sub RefreshGroupPermissionsCache(cache As Object, e As EventArgs)
        Using connection = GetConnection()
            mCacheDataProvider.GetAllGroupPermissions(connection).
                ForEach(Sub(x) mActualGroupPermissionsCache.Value.SetValue(x.Key, x.Value)).
                Evaluate()
        End Using
    End Sub

    Private Sub RefreshProcessGroupsCache(cache As Object, e As EventArgs)
        Using connection = GetConnection()
            mCacheDataProvider.GetProcessGroups(connection).
                ForEach(Sub(x) mProcessGroupsCache.Value.SetValue(x.Key.ToString(), x.Value)).
                Evaluate()
        End Using
    End Sub

    Private Sub RefreshResourceGroupsCache(cache As Object, e As EventArgs)
        Using connection = GetConnection()
            mCacheDataProvider.GetResourceGroups(connection).
                ForEach(Sub(x) mResourceGroupsCache.Value.SetValue(x.Key.ToString(), x.Value)).
                Evaluate()
        End Using
    End Sub

    Private Sub InvalidateCaches()
        mActualGroupPermissionsCache.Value.Clear()
        mProcessGroupsCache.Value.Clear()
        mResourceGroupsCache.Value.Clear()
        mEffectiveGroupPermissionsCache.Value.Clear()
        mIsMIReportingEnabledCache.Value.Clear()
    End Sub


#Region " Secured Methods "

    ''' <summary>
    ''' Returns the sign-on related configuration settings for this environment.
    ''' </summary>
    ''' <param name="rules">Carries back the password rules</param>
    ''' <param name="options">Carries back the logon options</param>
    <SecuredMethod(Permission.SystemManager.Security.SignOnSettings)>
    Public Sub GetSignonSettings(ByRef rules As PasswordRules, ByRef options As LogonOptions) Implements IServer.GetSignonSettings
        CheckPermissions()
        Using con = GetConnection()
            rules = GetPasswordRules(con)
            options = GetLogonOptions(con)
        End Using
    End Sub

    ''' <summary>
    ''' Updates the sign-on related configuration settings for this environment.
    ''' </summary>
    ''' <param name="rules">Password rules</param>
    ''' <param name="options">Logon options</param>
    <SecuredMethod(Permission.SystemManager.Security.SignOnSettings)>
    Public Sub SetSignonSettings(rules As PasswordRules, options As LogonOptions) Implements IServer.SetSignonSettings
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SetSignonSettings(con, rules, options)
            con.CommitTransaction()
        End Using
    End Sub

    Private Sub SetSignonSettings(con As IDatabaseConnection, rules As PasswordRules, options As LogonOptions)
        UpdatePasswordRules(con, rules)
        SetLogonOptions(con, options)
        Dim auditMessage = String.Format(SignOnSettingsAuditMessage,
                                         rules.ToString(), options.AutoPopulate.GetFriendlyName(),
                                         options.ShowUserList.ToString(),
                                         options.MappedActiveDirectoryAuthenticationEnabled.ToString(),
                                         options.AuthenticationServerAuthenticationEnabled.ToString(),
                                         options.AuthenticationServerUrl,
                                         GetAuthenticationServerCredentialNameFromId(con, options))
        AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySignonSettings, auditMessage)
    End Sub

    ''' <summary>
    ''' Returns the single sign-on related configuration for this environment.
    ''' </summary>
    ''' <param name="domain">Active Directory Domain</param>
    ''' <param name="adminGroup">System Administrator Security Group</param>
    <SecuredMethod(Permission.SystemManager.Security.SignOnSettings)>
    Public Sub GetSignonSettings(ByRef domain As String, ByRef adminGroup As String) Implements IServer.GetSignonSettings
        CheckPermissions()
        Using con = GetConnection()
            domain = CStr(con.ExecuteReturnScalar(New SqlCommand(
             "select ActiveDirectoryProvider from BPASysConfig")))
            adminGroup = SystemRoleSet.Current(Role.DefaultNames.SystemAdministrators).ActiveDirectoryGroup
        End Using
    End Sub

    ''' <summary>
    ''' Updates the single sign-on related configuration for this environment.
    ''' </summary>
    ''' <param name="domain">Active Directory Domain</param>
    ''' <param name="domainChanged">Set to True to indicate that the domain is being
    ''' changed</param>
    ''' <param name="groupSID">System Administrator Security Group SID</param>
    ''' <param name="groupName">System Administrator Security Group name</param>
    ''' <param name="groupPath">System Administrator Security Group path</param>
    <SecuredMethod(Permission.SystemManager.Security.SignOnSettings)>
    Public Sub SetSignonSettings(domain As String, domainChanged As Boolean, groupSID As String,
      groupName As String, groupPath As String) Implements IServer.SetSignonSettings
        CheckPermissions()
        Using con = GetConnection()
            'Update the roles cache with the system admin group ensuring other mapped
            'groups are cleared if a new domain was entered
            Dim roles = SystemRoleSet.SystemCurrent.ModifiableCopy()
            roles.SetSysAdminADGroup(groupSID, domainChanged)
            UpdateRoles(con, roles, True)
            SystemRoleSet.SystemCurrent.Poll()

            con.BeginTransaction()

            SetSingleAuthActiveDirectoryDomain(con, domain)

            AuditRecordSysConfigEvent(con, SysConfEventCode.ModifySingleSignOn,
              String.Format(My.Resources.clsServer_NewActiveDirectoryConfigurationIsDomain01Role234,
                          domain, LTools.GetC(Role.DefaultNames.SystemAdministrators, "roleperms", "role"),
                          groupSID, groupName, groupPath))
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Validates the current Active Directory security group/BP role mappings.
    ''' </summary>
    ''' <param name="groups">The groups to check, retrieved from the database if not
    ''' passed</param>
    ''' <param name="reason">Carries back a reason in the event that the groups
    ''' are not valid.</param>
    ''' <returns>True if the method succeeded, otherwise False</returns>
    <SecuredMethod(Permission.SystemManager.Security.UserRoles)>
    Public Function ValidateActiveDirectoryGroups(groups As ICollection(Of String), ByRef reason As String) As Boolean Implements IServer.ValidateActiveDirectoryGroups
        CheckPermissions()
        Using con = GetConnection()
            If groups Is Nothing Then groups = GetActiveDirectoryGroups(con)
            Return clsActiveDirectory.ValidateADGroups(GetActiveDirectoryDomain(con), groups, reason)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function GetDistinguishedNameOfCurrentForest() As String Implements IServer.GetDistinguishedNameOfCurrentForest
        CheckPermissions()
        Return LdapLibrary.GetDistinguishedNameOfCurrentForestRootDomain()
    End Function

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function FindActiveDirectoryUsers(searchRoot As String, filterType As UserFilterType, filter As String, queryPageOptions As QueryPageOptions, credentials As DirectorySearcherCredentials) As PaginatedUserQueryResult Implements IServer.FindActiveDirectoryUsers
        CheckPermissions()

        Dim queryResults = DependencyResolver.FromScope(
            Sub(builder) builder.Register(Function(x) Me).As(Of IServerPrivate)().ExternallyOwned(),
            Function(query As IActiveDirectoryUserQuery)
                Dim queryOptions = New ActiveDirectoryUserQueryOptions(searchRoot, filterType, filter, queryPageOptions, credentials)
                Return query.Run(queryOptions)
            End Function
            )

        Return queryResults

    End Function

    ''' <summary>
    ''' Synchronise users from Active Directory to the Blue Prism database - i.e.
    ''' users who would be authorised to use Blue Prism according to their membership
    ''' in Domain Groups, but haven't logged in to Blue Prism yet, get records
    ''' created in the Blue Prism database.
    ''' </summary>
    ''' <returns>A summary of what's been done, to be displayed to the user, or
    ''' Nothing if no response is necessary.</returns>
    ''' <throws>An Exception if something went badly wrong.</throws>
    ''' <remarks>There are certain things you can't do until a user account is
    ''' created/mirrored. An example is the setting of user alerts.
    '''
    ''' It is not reasonable to expect a user to log in before their settings
    ''' can be changed. Eg Bob takes over from Alice watching user alerts,
    ''' but Bob isn't in today to log in. In this scenario, administrator
    ''' wants to add Bob to appropriate Blue Prism group, visit System Manager
    ''' and configure his alerts profile. This synchronisation feature is designed
    ''' to pull down list of users in the Blue Prism group(s).
    ''' </remarks>
    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function RefreshADUserList() As RefreshADUserList Implements IServer.RefreshADUserList
        CheckPermissions()
        If mLoggedInMode <> AuthMode.ActiveDirectory Then Return Nothing
        Dim returnData As New Auth.RefreshADUserList
        Using con = GetConnection()
            con.BeginTransaction()
            Dim dom As String = GetActiveDirectoryDomain()
            ' No domain? Then what are we even doing here?
            If dom = "" Then Return Nothing

            Using searcher As New ADGroupSearcher()
                Dim newUsers As New HashSet(Of Guid)
                Dim activeUsers As New HashSet(Of Guid)
                Dim unassignedRoles As New HashSet(Of String)
                Dim groupErrors As String = ""

                ' Loop through each role, find out the associated AD Group Name and mirror
                ' all (active) users in those groups.
                For Each role As Role In SystemRoleSet.Current
                    ' Get the SSO group for this role
                    Dim ssoGroup As String = role.ActiveDirectoryGroup
                    ' If not there, move onto the next one.
                    If ssoGroup = "" Then unassignedRoles.Add(role.Name) : Continue For

                    Dim group As GroupPrincipal = searcher.GetGroup(ssoGroup)
                    ' Ignore if group is not found - should this error at this stage?
                    If group Is Nothing Then Continue For
                    Try
                        For Each user In clsActiveDirectory.GetValidGroupMembers(group)
                            If clsActiveDirectory.UserIsEnabled(user) Then
                                Dim userId As Guid
                                If MirrorUser(con, user.UserPrincipalName, userId) Then _
                                 newUsers.Add(userId)
                                activeUsers.Add(userId)
                            End If
                        Next
                    Catch ex As ActiveDirectoryConfigException
                        groupErrors &= ex.Message & vbCrLf
                    End Try
                Next

                ' Get all the users, activate or deactivate them as appropriate, then
                ' categorise them according to the action performed on them
                Dim deactivatedUsers As New List(Of User)
                Dim activatedUsers As New List(Of User)
                Dim addedUsers As New List(Of User)
                For Each u As User In GetUsers(con, Nothing, False).Where(Function(us) Not us.IsHidden)
                    If newUsers.Contains(u.Id) Then
                        addedUsers.Add(u)
                    ElseIf activeUsers.Contains(u.Id) Then
                        If ActivateUser(con, u.Id) Then activatedUsers.Add(u)
                    Else
                        If DeactivateUser(con, u.Id) Then deactivatedUsers.Add(u)
                    End If
                Next

                returnData.AddedUsers = addedUsers
                returnData.ActivatedUsers = activatedUsers
                returnData.DeactivatedUsers = deactivatedUsers
                returnData.RolesNotMapped = GetActiveDirectoryGroupsEmpty(con)
                returnData.GroupErrors = groupErrors

                con.CommitTransaction()
                Return returnData
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Checks that the given user password and confirmation both match and satisfy
    ''' the configured requirements.
    ''' </summary>
    ''' <param name="password">The password</param>
    ''' <param name="confirmation">Confirmation of the password</param>
    ''' <returns>True if the password satisfies the requirements, otherwise False
    ''' </returns>
    <SecuredMethod()>
    Public Function IsValidPassword(password As SafeString,
                                    confirmation As SafeString) _
                                As Boolean Implements IServer.IsValidPassword
        CheckPermissions()
        Using con = GetConnection()
            Dim rules = GetPasswordRules(con)
            rules.CheckPasswordRules(password, confirmation)
            Return True
        End Using
    End Function

    ''' <summary>
    ''' Creates a new user on the database.
    ''' </summary>
    ''' <param name="user">The populated user object with the data to create the user
    ''' record with on the database.</param>
    ''' <param name="password">The password to set on the database.</param>
    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub CreateNewUser(user As User, password As SafeString) _
    Implements IServer.CreateNewUser
        CheckPermissions()
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        If user.AuthType = AuthMode.External OrElse user.AuthType = AuthMode.ActiveDirectory _
            Then Throw New InvalidOperationException($"This method cannot be used with auth type {NameOf(user.AuthType)}")

        Using con = GetConnection()
            con.BeginTransaction()
            CreateNewUser(con, user, password)
            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function CreateNewAuthenticationServerUserWithUniqueName(username As String, authServerId As Guid) As String Implements IServer.CreateNewAuthenticationServerUserWithUniqueName
        CheckPermissions()
        Using connection = GetConnection()
            Return CreateNewAuthenticationServerUserWithUniqueName(connection, username, authServerId)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function CreateNewServiceAccount(clientName As String, clientId As String, hasBluePrismApiScope As Boolean) As String Implements IServer.CreateNewServiceAccount
        CheckPermissions()
        Using connection = GetConnection()
            Return CreateNewServiceAccount(connection, clientName, clientId, hasBluePrismApiScope)
        End Using
    End Function

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdateServiceAccount(clientId As String, clientName As String, hasBluePrismApiScope As Boolean, synchronizationDate As DateTimeOffset) Implements IServer.UpdateServiceAccount
        CheckPermissions()
        Using connection = GetConnection()
            UpdateServiceAccount(connection, clientId, clientName, hasBluePrismApiScope, synchronizationDate)
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub CreateNewMappedActiveDirectoryUser(user As User) Implements IServer.CreateNewMappedActiveDirectoryUser
        CheckPermissions()
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        If String.IsNullOrEmpty(user.ExternalId) Then Throw New InvalidArgumentException("Active Directory user must have a SID")
        If user.AuthType <> AuthMode.MappedActiveDirectory Then Throw New InvalidArgumentException("Auth type must be 'Mapped Active Directory'")

        CreateMappedActiveDirectoryUser(user)
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub CreateActiveDirectoryMapping(user As User) Implements IServer.CreateActiveDirectoryMapping
        CheckPermissions()
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        If String.IsNullOrEmpty(user.ExternalId) Then Throw New InvalidArgumentException("Active Directory user must have a SID")
        If user.AuthType <> AuthMode.ActiveDirectory Then Throw New InvalidArgumentException("Auth type must be 'Active Directory'")

        Using con = GetConnection()
            con.BeginTransaction()
            CreateActiveDirectoryUserMapping(con, user)
            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function CreateNewMappedActiveDirectoryUsers(users As List(Of User), batchSize As Integer) As Integer _
            Implements IServer.CreateNewMappedActiveDirectoryUsers
        CheckPermissions()
        If users Is Nothing Then Throw New ArgumentNullException(NameOf(users))
        For Each user As User In users
            If String.IsNullOrEmpty(user.ExternalId) Then
                Throw New InvalidArgumentException($"Active Directory user {user} must have a SID")
            End If
            If user.AuthType <> AuthMode.MappedActiveDirectory Then
                Throw New InvalidArgumentException($"User {user} Auth type must be 'Mapped Active Directory'")
            End If
        Next
        Return CreateMappedActiveDirectoryUsers(users, batchSize)
    End Function

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function GetSidForActiveDirectoryUser(user As User) As String Implements IServer.GetSidForActiveDirectoryUser
        CheckPermissions()
        If user.AuthType <> AuthMode.MappedActiveDirectory Then Throw New InvalidOperationException("Can only retrieve SID for Mapped Active Directory Users")

        Using con = GetConnection()
            Return GetActiveDirectoryUserSid(con, user.Id)
        End Using
    End Function

    Private Sub CreateMappedActiveDirectoryUser(user As User)
        Using con = GetConnection()
            con.BeginTransaction()
            CreateNewUser(con, user, Nothing)
            CreateActiveDirectoryUserMapping(con, user)
            con.CommitTransaction()
        End Using
    End Sub

    Private Function CreateMappedActiveDirectoryUsers(users As List(Of User), batchSize As Integer) As Integer

        Dim numUsersInserted As Integer = 0
        Const SqlTimeoutErrorCode As Integer = -2

        While numUsersInserted < users.Count

            Dim numUsersToAdd As Integer = users.Count - numUsersInserted
            If numUsersToAdd > batchSize Then
                numUsersToAdd = batchSize
            End If

            Try
                Using con = GetConnection()
                    con.BeginTransaction()
                    For i As Integer = 0 To numUsersToAdd - 1
                        Dim user As User = users.Item(numUsersInserted + i)
                        CreateNewUser(con, user, Nothing)
                        CreateActiveDirectoryUserMapping(con, user)
                    Next
                    con.CommitTransaction()
                    numUsersInserted += numUsersToAdd

                End Using
            Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DeadlockVictim _
                                            OrElse sqle.Number = SqlTimeoutErrorCode
                Return numUsersInserted
            End Try
        End While

        Return numUsersInserted
    End Function

    Private Sub ConvertToAuthenticationServerUser(connection As IDatabaseConnection, bluePrismUser As IUser, authenticationServerUserId As Guid, authenticationServerUserName As String)

        If bluePrismUser.AuthType = AuthMode.AuthenticationServer Then
            Throw New InvalidArgumentException("User already an Authentication Server user")
        End If

        connection.BeginTransaction()
        Try
            Dim userId = mTryGetUserId(connection, authenticationServerUserName)
            Dim userNameIsAlreadyUnique = userId = Guid.Empty OrElse userId = bluePrismUser.Id
            Dim userName = If(userNameIsAlreadyUnique, authenticationServerUserName, mUniqueUserNameGenerator.GenerateUsername(authenticationServerUserName))

            SetAuthenticationServerUserValues(connection, bluePrismUser, authenticationServerUserId, userName, authenticationServerUserName)
            DeletePasswordRecordForUser(connection, bluePrismUser)
            RemovePasswordDataOnUserRecord(connection, bluePrismUser)

            connection.CommitTransaction()

        Catch nameAlreadyExistsException As NameAlreadyExistsException
            connection.RollbackTransaction()
            ConvertToAuthenticationServerUser(connection, bluePrismUser, authenticationServerUserId, authenticationServerUserName)
        Catch ex As Exception
            connection.RollbackTransaction()
            Throw
        End Try

    End Sub

    ''' <summary>
    ''' Updates the basic data found on the given user.
    ''' </summary>
    ''' <param name="user">The user to update</param>
    ''' <param name="password">The password to set on the user. Null indicates that
    ''' the password should not be changed.</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="user"/> is null.
    ''' </exception>
    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdateUser(user As User, password As SafeString) _
    Implements IServer.UpdateUser
        CheckPermissions()
        Using con = GetConnection()
            UpdateUser(con, user, password)
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdateExternalUser(user As User) Implements IServer.UpdateExternalUser
        CheckPermissions()
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        Using con = GetConnection()
            con.BeginTransaction()

            UpdateUserDetailsForMappedUser(con, user)
            UpdateExternalIdentityMapping(con, user)

            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdateAuthenticationServerUser(user As User) Implements IServer.UpdateAuthenticationServerUser
        CheckPermissions()
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        Using con = GetConnection()
            con.BeginTransaction()

            UpdateUserDetailsForMappedUser(con, user)

            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdateAdSSOUserToMappedAdUser(user As User) Implements IServer.UpdateAdSSOUserToMappedAdUser
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            UpdateAdUserAuthTypeToMapped(con, user)

            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdateMappedActiveDirectoryUser(user As User) Implements IServer.UpdateMappedActiveDirectoryUser
        CheckPermissions()
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        If user.AuthType <> AuthMode.MappedActiveDirectory Then Throw New ArgumentException("User should be of type: Mapped Active Directory")

        Using con = GetConnection()
            con.BeginTransaction()

            UpdateUserDetailsForMappedUser(con, user)

            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.SignOnSettings, Permission.SystemManager.Security.Users)>
    Public Function ConvertDatabaseFromAdSsoToMappedAd(nativeAdminUser As NativeAdminUserModel, ByRef conversionResultMessage As String) _
        As Boolean Implements IServer.ConvertDatabaseFromAdSsoToMappedAd

        Dim users As List(Of User)

        CheckPermissions()

        Try
            AuditStartDatabaseConversion()
            users = GetAdSsoUsersToConvert(conversionResultMessage)
            If users Is Nothing Then
                WriteSsoConversionFailureToAuditLog(conversionResultMessage, String.Empty)
                Return False
            End If
        Catch ex As Exception
            conversionResultMessage = ConversionProcessingException
            WriteSsoConversionFailureToAuditLog(String.Concat(Conversion_Exception_ProcessingUsers, ex.Message), String.Empty)
            Return False
        End Try

        Return ConvertDatabaseFromAdSsoToMappedAd(nativeAdminUser, users, conversionResultMessage)

    End Function
    Private Function GetAdSsoUsersToConvert(ByRef conversionResultMessage As String) As List(Of User)

        RefreshADUserList()
        Dim users = GetAllUsers(False).Where(Function(x) Not String.IsNullOrWhiteSpace(x.Name)).ToList()
        clsActiveDirectory.GetUsersSidsFromActiveDirectory(users)
        Dim invalidUserCount = users.Where(Function(u) Not u.Deleted AndAlso String.IsNullOrEmpty(u.ExternalId)).Count()
        If invalidUserCount = 0 Then
            GetUserRolesFromActiveDirectory(users)
            Return users
        End If
        conversionResultMessage = String.Format(ActiveDirectoryConversionHasMissingSids, invalidUserCount)
        Return Nothing

    End Function
    Private Function ConvertDatabaseFromAdSsoToMappedAd(nativeAdminUser As NativeAdminUserModel, users As List(Of User), ByRef message As String) _
        As Boolean

        Dim dbException As Exception
        Using con = GetConnection()

            ' get the logon details outside of the transaction otherwise a deadlock will occur
            Dim rules As PasswordRules = Nothing
            Dim logonOptions As LogonOptions = Nothing
            GetSignonSettings(rules, logonOptions)

            con.BeginTransaction()

            Try
                UpdateUsersToMappedActiveDirectory(con, users)
                SetSingleAuthActiveDirectoryDomain(con, String.Empty)
                EnableMappedActiveDirectoryAuthentication(con, rules, logonOptions)
                ClearMappedActiveDirectoryGroupsFromRoles(con)
                CreateNewUser(con, nativeAdminUser.User, nativeAdminUser.Password)
                AuditRecordSysConfigEvent(con,
                                          SysConfEventCode.CompletedConvertingToMultiAuthDatabase,
                                          String.Empty)

                con.CommitTransaction()

                Return True
            Catch ex As Exception
                dbException = ex
                Try
                    con.RollbackTransaction()
                    message = String.Format(
                        ConvertToMappedSSO_DatabaseHasBeenRestored0_Template,
                        ex.Message)
                Catch rollbackException As Exception
                    message = ConvertToMappedSSO_UnrecoverableDatabaseError
                End Try
            End Try
        End Using

        WriteSsoConversionFailureToAuditLog(String.Empty, dbException.Message)

        Return False

    End Function
    Private Sub WriteSsoConversionFailureToAuditLog(errorMessage As String, exceptionMessage As String)
        Try
            Using auditCon = GetConnection()
                AuditRecordSysConfigEvent(auditCon,
                                          SysConfEventCode.AbortedConvertingToMultiAuthDatabase,
                                          errorMessage,
                                          exceptionMessage)
            End Using
        Catch ex As Exception
            ' ignore any database exceptions here to allow conversion to complete
        End Try
    End Sub
    Private Sub GetUserRolesFromActiveDirectory(users As IEnumerable(Of User))
        Using searcher As New ADGroupSearcher()

            For Each role As Role In SystemRoleSet.Current
                Dim ssoGroup As String = role.ActiveDirectoryGroup
                If String.IsNullOrEmpty(ssoGroup) Then Continue For

                Dim group As GroupPrincipal = searcher.GetGroup(ssoGroup)
                If group Is Nothing Then Continue For

                Try
                    Dim groupUsers = clsActiveDirectory.GetValidGroupMembers(group)

                    For Each user In groupUsers
                        Dim roleUser = users.SingleOrDefault(Function(x) x.Name = user.UserPrincipalName)
                        If roleUser Is Nothing Then Continue For
                        roleUser.Roles.Add(role)
                    Next
                Catch ex As ActiveDirectoryConfigException
                    'Possible group errors but it's fine for us to ignore for a conversion
                End Try
            Next
        End Using
    End Sub

    Private Sub EnableMappedActiveDirectoryAuthentication(ByVal con As IDatabaseConnection,
                                                          rules As PasswordRules,
                                                          logonOptions As LogonOptions)
        logonOptions.MappedActiveDirectoryAuthenticationEnabled = True
        SetSignonSettings(con, rules, logonOptions)
    End Sub

    Private Sub UpdateUsersToMappedActiveDirectory(ByVal con As IDatabaseConnection,
                                                   users As IEnumerable(Of User))

        Dim isDeleted = Function(u As User) String.IsNullOrEmpty(u.ExternalId) AndAlso u.Deleted

        Dim deletedUsers = users.Where(isDeleted)
        Dim usersToAdd = users.Where(Function(u As User) Not isDeleted(u))

        AddMappedActiveDirectoryUsers(con, usersToAdd)
        AddDeletedMappedActiveDirectoryUsers(con, deletedUsers)

    End Sub
    Private Sub AddMappedActiveDirectoryUsers(ByVal con As IDatabaseConnection,
                                              users As IEnumerable(Of User))
        For Each user In users
            CreateActiveDirectoryUserMapping(con, user)

            AssignRolesToMappedActiveDirectoryUser(con, user)

            UpdateAdUserAuthTypeToMapped(con, user)
            AuditRecordActiveDirectoryUserEvent(UserEventCode.UserConvertedToMultiAuthAd,
                                                con, user.Id,
                                                user.ExternalId, user.Name,
                                                String.Empty)
        Next
    End Sub
    Private Sub AddDeletedMappedActiveDirectoryUsers(ByVal con As IDatabaseConnection,
                                                     users As IEnumerable(Of User))
        For Each user In users
            AssignRolesToMappedActiveDirectoryUser(con, user)

            UpdateAdUserAuthTypeToMapped(con, user)
            AuditRecordActiveDirectoryUserEvent(UserEventCode.UserConvertedToDeletedMultiAuthAd,
                                                con, user.Id,
                                                user.ExternalId, user.Name,
                                                String.Empty)
        Next
    End Sub
    Private Sub AssignRolesToMappedActiveDirectoryUser(ByVal con As IDatabaseConnection,
                                                       user As User)
        AssignRoles(con, user.Id, user.Roles)
        AuditRecordUserEvent(con, UserEventCode.UserMappedToMultiAuthAdRoles,
                             user.Id, user.Roles.ToString(),
                             String.Empty, Nothing, Nothing)
    End Sub
    Private Sub SetSingleAuthActiveDirectoryDomain(ByVal con As IDatabaseConnection, ByVal domain As String)
        Dim cmd As New SqlCommand("update BPASysconfig set ActiveDirectoryProvider=@Domain")
        cmd.Parameters.AddWithValue("@Domain", domain)
        con.Execute(cmd)
    End Sub

    Private Sub ClearMappedActiveDirectoryGroupsFromRoles(ByVal con As IDatabaseConnection)

        Dim roleSet As RoleSet = GetRoles()
        For Each role In roleSet
            role.ActiveDirectoryGroup = Nothing
        Next
        UpdateRoles(con, roleSet, False)

    End Sub

    ''' <summary>
    ''' Flags a user as deleted, note that this does not actually delete the user, as
    ''' then it causes problems with audit and logging.
    ''' </summary>
    ''' <param name="userId">The guid of the user to delete</param>
    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub DeleteUser(userId As Guid) Implements IServer.DeleteUser
        CheckPermissions()

        Using con = GetConnection()
            con.BeginTransaction()
            DeleteUser(con, userId)
            con.CommitTransaction()
        End Using
    End Sub

    Private Sub DeleteUser(con As IDatabaseConnection, userId As Guid)
        'Check if the user is logged on anywhere
        Using cmd = New SqlCommand(
                  " select MachineName from BPAAliveResources" &
                  " where userid = @id" &
                  " and lastupdated > dateadd(minute, -2, getutcdate());")
            With cmd.Parameters
                .AddWithValue("@id", userId.ToString())
            End With
            Dim dt = con.ExecuteReturnDataTable(cmd)

            If dt.Rows.Count > 0 Then
                Dim resources As New List(Of String)
                For Each r As DataRow In dt.Rows
                    resources.Add(CStr(r("MachineName")))
                Next
                Throw New BluePrismException(String.Format(
                    My.Resources.clsServer_ThisUserIsCurrentlyLoggedInTo0OnTheFollowingResources1TheUserMustLogOutBeforeSH,
                    ApplicationProperties.ApplicationName, CollectionUtil.Join(resources, ", ")))
            End If
        End Using

        'Check we're not trying to delete the last active user
        Using cmd = New SqlCommand("select count(UserName) from BPAUser " &
                              "where username is not null and IsDeleted = 0")
            If CInt(con.ExecuteReturnScalar(cmd)) = 1 Then Throw New BluePrismException(
                My.Resources.clsServer_YouCannotDeleteTheLastUser)
        End Using


        'Check we're not trying to delete the last system administrator  user
        Using cmd = New SqlCommand("select count(a.userid) from BPAUserRoleAssignment a " &
                                "inner join BPAUserRole r on a.userroleid = r.id " &
                                "inner join BPAUser u on a.userid = u.userid " &
                              "where u.userid <> @userid and r.name = @adminRole and u.isdeleted = 0")
            cmd.Parameters.AddWithValue("@userid", userId)
            cmd.Parameters.AddWithValue("@adminRole", Role.DefaultNames.SystemAdministrators)
            If CInt(con.ExecuteReturnScalar(cmd)) = 0 Then Throw New BluePrismException(
                My.Resources.clsServer_YouCannotDeleteTheLastUserAssignedToThe0Role,
                Role.DefaultNames.SystemAdministrators)
        End Using

        ' Check that a non system admin user is not trying to delete a system admin user.
        Dim userToDelete = GetUser(con, userId, Nothing)
        If userToDelete.IsSystemAdmin AndAlso Not mLoggedInUser.IsSystemAdmin Then
            Throw New BluePrismException(My.Resources.clsServer_YouNeedThe0RoleToDeleteThisUser,
                                         Role.DefaultNames.SystemAdministrators)
        End If

        'Check that the user doesn't hold any process locks, otherwise delete it

        Using cmd = New SqlCommand(" if exists (select 1 from BPAProcessLock where userid = @id)" &
                              "   select 1;" &
                              " else begin" &
                              "   update BPAUser set isdeleted = 1 where userid = @id " &
                              "   select 0" &
                              " end;")

            cmd.Parameters.AddWithValue("@id", userId)
            If IfNull(con.ExecuteReturnScalar(cmd), 0) = 1 Then
                Throw New BluePrismException(
                    My.Resources.clsServer_ThisUserCurrentlyHasALockOnAProcessCannotDeleteThisUser)
            End If

            AuditRecordUserEvent(con, UserEventCode.DeleteUser, userId, "", "",
                                 Nothing, Nothing)

        End Using

        Using cmd = New SqlCommand("delete from BPAGroupUser where memberid=@id")
            cmd.Parameters.AddWithValue("@id", userId)
            con.Execute(cmd)
        End Using

        DeleteGroupExpandedStatesByUser(userId)

        If userToDelete.AuthType = AuthMode.External Then
            DeleteExternalIdentityFromUser(con, userToDelete)
            DeleteReloginTokensForUser(userToDelete.Id)
        ElseIf userToDelete.AuthType = AuthMode.MappedActiveDirectory Then
            DeleteMappedActiveDirectoryIDFromUser(con, userToDelete)
        End If

    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub RetireAuthenticationServerUser(authServerId As Guid, synchronizationDate As DateTimeOffset) Implements IServer.RetireAuthenticationServerUser
        CheckPermissions()
        Using connection = GetConnection()
            ToggleRetireAuthenticationServerUser(connection, authServerId, synchronizationDate, True)
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UnretireAuthenticationServerUser(authServerId As Guid, synchronizationDate As DateTimeOffset) Implements IServer.UnretireAuthenticationServerUser
        CheckPermissions()
        Using connection = GetConnection()
            ToggleRetireAuthenticationServerUser(connection, authServerId, synchronizationDate, False)
        End Using
    End Sub

    Private Sub ToggleRetireAuthenticationServerUser(connection As IDatabaseConnection, authServerId As Guid, synchronizationDate As DateTimeOffset, retired As Boolean)
        connection.BeginTransaction()
        Try
            Dim user = mGetUserByAuthenticationServerUserId(connection, authServerId)

            If user.DeletedLastSynchronizationDate > synchronizationDate Then
                Throw New SynchronizationOutOfSequenceException()
            End If

            mUpdateRetireAuthenticationServerUser(connection, synchronizationDate, retired, user.Id)

            Dim eventCode = If(retired, UserEventCode.DeleteUser, UserEventCode.RestoreUser)
            AuditRecordUserEvent(connection, eventCode, user.Id, "", "", Nothing, Nothing)

            connection.CommitTransaction()
        Catch ex As Exception
            connection.RollbackTransaction()
            Throw
        End Try
    End Sub

    Private Shared Sub UpdateRetireAuthenticationServerUser(connection As IDatabaseConnection, synchronizationDate As DateTimeOffset, retired As Boolean, id As Guid)
        Using cmd = New SqlCommand("update BPAUser " &
                                   "set isdeleted = @retired, deletedLastSynchronizationDate = @deletedLastSynchronizationDate " &
                                   "where userid = @userid")
            cmd.Parameters.AddWithValue("@userid", id)
            cmd.Parameters.AddWithValue("@retired", retired)
            cmd.Parameters.AddWithValue("@deletedLastSynchronizationDate", synchronizationDate)
            connection.ExecuteReturnScalar(cmd)
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub DeleteServiceAccount(clientId As String, synchronizationDate As DateTimeOffset) Implements IServer.DeleteServiceAccount
        CheckPermissions()
        Using connection = GetConnection()
            DeleteAuthenticationServerServiceAccount(connection, clientId, synchronizationDate)
        End Using
    End Sub

    Private Sub DeleteAuthenticationServerServiceAccount(connection As IDatabaseConnection, clientId As String, synchronizationDate As DateTimeOffset)
        connection.BeginTransaction()
        Try
            Dim user = GetUserByAuthenticationServerClientId(connection, clientId)

            Using cmd = New SqlCommand("update BPAUser " &
                                       "set isdeleted = 1, deletedLastSynchronizationDate = @deletedLastSynchronizationDate " &
                                       "where userid = @userid")
                cmd.Parameters.AddWithValue("@userid", user.Id)
                cmd.Parameters.AddWithValue("@deletedLastSynchronizationDate", synchronizationDate)
                connection.ExecuteReturnScalar(cmd)

                AuditRecordUserEvent(connection, UserEventCode.DeleteUser, user.Id, "", "",
                                     Nothing, Nothing)
            End Using

            connection.CommitTransaction()
        Catch ex As Exception
            connection.RollbackTransaction()
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Resets the number of login attempts for the specified user
    ''' </summary>
    ''' <param name="userId">The ID of the user to reset</param>
    ''' <returns>True if the user had their login attempts reset; False if it was
    ''' already zero (or the user ID did not exist on the database)</returns>
    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function UnlockUser(userId As Guid) As Boolean Implements IServer.UnlockUser
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            If Not ResetLoginAttempts(con, userId) Then Return False

            AuditRecordUserEvent(con, UserEventCode.UserUnlocked, userId, "", "", Nothing, Nothing)
            con.CommitTransaction()
            Return True
        End Using
    End Function

    ''' <summary>
    ''' Updates the password of the given user.
    ''' </summary>
    ''' <param name="userName">The users name</param>
    ''' <param name="currentPassword">The users existing password</param>
    ''' <param name="newPassword">The users new password</param>
    ''' <param name="confirmation">A confirmation of the users new password</param>
    ''' <exception cref="InvalidPasswordException">If the given password is the same
    ''' as the previous password</exception>
    <UnsecuredMethod()>
    Public Sub UpdatePassword(userName As String, currentPassword As SafeString, newPassword As SafeString, confirmation As SafeString) Implements IServer.UpdatePassword

        Using con = GetConnection()
            con.BeginTransaction()

            Dim rules = GetPasswordRules(con)
            rules.CheckPasswordRules(newPassword, confirmation)

            Dim result As LoginResult = LoginAttempt(con, userName, currentPassword)
            If Not result.IsPasswordUpdateAllowed Then
                Throw New InvalidPasswordException(My.Resources.clsServer_TheCurrentPasswordGivenWhenUpdatingThePasswordWasInvalid)
            End If

            'If hashed password matches previously hashed password then do not update
            If VerifyPassword(con, newPassword, result.User) Then
                Throw New InvalidPasswordException(
                    String.Format(My.Resources.clsServer_TheNewPasswordForUser0CannotMatchTheCurrentPassword, result.User.Name))
            End If

            Dim noRepeats As Boolean, noRepeatsDays As Boolean
            Dim numberOfRepeats As Integer, numberOfDays As Integer
            GetPasswordUpdateRules(con, noRepeats, numberOfRepeats, noRepeatsDays, numberOfDays)

            'If the hashed password matches previous hashed passwords then do not update
            If noRepeats Then
                Using cmd As New SqlCommand("select top(@number) type,hash,salt,lastuseddate from BPAPassword where userid = @userid and active = 0 order by lastuseddate desc")
                    With cmd.Parameters
                        .AddWithValue("@userid", result.UserID)
                        .AddWithValue("@number", numberOfRepeats)
                    End With
                    Using reader = con.ExecuteReturnDataReader(cmd)
                        Dim prov As New ReaderDataProvider(reader)
                        While reader.Read()
                            If VerifyPassword(newPassword, prov) Then
                                Dim sReason As String = LTools.Format(My.Resources.clsServer_TheNewPasswordForUser0CannotMatchCOUNTPluralOneThePreviousPasswordOtherAnyOfThe, "NAME", result.User.Name, "COUNT", numberOfRepeats)
                                Throw New InvalidPasswordException(sReason)
                            End If
                        End While
                    End Using
                End Using
            End If

            If noRepeatsDays Then
                Using cmd As New SqlCommand("select type,hash,salt,lastuseddate from BPAPassword where userid = @userid and active = 0 and lastuseddate > dateadd(day,-(@number+1),getutcdate())")
                    With cmd.Parameters
                        .AddWithValue("@userid", result.UserID)
                        .AddWithValue("@number", numberOfDays)
                    End With
                    Using reader = con.ExecuteReturnDataReader(cmd)
                        Dim prov As New ReaderDataProvider(reader)
                        While reader.Read()
                            If VerifyPassword(newPassword, prov) Then
                                Dim sReason As String = LTools.Format(My.Resources.clsServer_TheNewPasswordForUser0CannotMatchCOUNTPluralOneAnyOfThePreviousPasswordsUsedWit, "NAME", result.User.Name, "COUNT", numberOfDays)
                                Throw New InvalidPasswordException(sReason)
                            End If
                        End While
                    End Using
                End Using
            End If

            'Update the current password
            UpdatePassword(con, result.User, newPassword)

            ' Update password expiry
            UpdatePasswordExpiryDate(con, result.User)

            UpdatePasswordMaintainance(con, result.User, noRepeats, numberOfRepeats, noRepeatsDays, numberOfDays)

            AuditRecordUserEvent(con, UserEventCode.ResetPassword, result.UserID, "", "", Nothing, Nothing)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Gets the password rules
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="noRepeats">Whether a number of previously used passwords cannot be repeated</param>
    ''' <param name="numberOfRepeats">The number of previous passwords which must not be repeated</param>
    ''' <param name="noRepeatsDays">Whether a password used in a past number of days cannot be repeated</param>
    ''' <param name="numberOfDays">The number of days in which the password must not have been used</param>
    Private Shared Sub GetPasswordUpdateRules(con As IDatabaseConnection, ByRef noRepeats As Boolean, ByRef numberOfRepeats As Integer, ByRef noRepeatsDays As Boolean, ByRef numberOfDays As Integer)
        Using cmd As New SqlCommand("select norepeats,norepeatsdays,numberofrepeats,numberofdays from BPAPasswordRules")
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                If reader.Read Then
                    noRepeats = prov.GetValue("norepeats", False)
                    noRepeatsDays = prov.GetValue("norepeatsdays", False)
                    numberOfRepeats = prov.GetInt("numberofrepeats")
                    numberOfDays = prov.GetInt("numberofdays")
                End If
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Cleans up old passwords for the given user dependent on the provided password rules
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="user">The user whose password needs maintaining</param>
    Private Shared Sub UpdatePasswordMaintainance(con As IDatabaseConnection, user As User)
        Dim noRepeats As Boolean, noRepeatsDays As Boolean
        Dim numberOfRepeats As Integer, numberOfDays As Integer
        GetPasswordUpdateRules(con, noRepeats, numberOfRepeats, noRepeatsDays, numberOfDays)
        UpdatePasswordMaintainance(con, user, noRepeats, numberOfRepeats, noRepeatsDays, numberOfDays)
    End Sub

    ''' <summary>
    ''' Cleans up old passwords for the given user dependent on the provided password rules
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="user">The user whose password needs maintaining</param>
    ''' <param name="noRepeats">Whether a number of previously used passwords cannot be repeated</param>
    ''' <param name="numberOfRepeats">The number of previous passwords which must not be repeated</param>
    ''' <param name="noRepeatsDays">Whether a password used in a past number of days cannot be repeated</param>
    ''' <param name="numberOfDays">The number of days in which the password must not have been used</param>
    Private Shared Sub UpdatePasswordMaintainance(con As IDatabaseConnection, user As IUser, noRepeats As Boolean, numberOfRepeats As Integer, noRepeatsDays As Boolean, numberOfDays As Integer)
        If noRepeats OrElse noRepeatsDays Then

            Dim queryNoRepeats As String = Nothing
            Dim queryNoRepeatsDays As String = Nothing
            If noRepeats Then
                queryNoRepeats = "select top(@numberOfRepeats) id from BPAPassword where userid = @userid and active = 0 order by lastuseddate desc"
            End If
            If noRepeatsDays Then
                queryNoRepeatsDays = "select id from BPAPassword where userid = @userid and active = 0 and lastuseddate > dateadd(day,-(@numberOfDays+1),getutcdate())"
            End If

            Dim selectIDs As String = Nothing
            If noRepeats AndAlso noRepeatsDays Then
                selectIDs = String.Format("select id from ({0}) as rep union ({1})", queryNoRepeats, queryNoRepeatsDays)
            ElseIf noRepeats Then
                selectIDs = queryNoRepeats
            ElseIf noRepeatsDays Then
                selectIDs = queryNoRepeatsDays
            End If

            Using deletecmd As New SqlCommand(String.Format("delete p from BPAPassword p where p.userid=@userid and active = 0 and p.id Not in ({0})", selectIDs))
                With deletecmd.Parameters
                    .AddWithValue("@userid", user.Id)
                    If noRepeats Then .AddWithValue("@numberOfRepeats", numberOfRepeats)
                    If noRepeatsDays Then .AddWithValue("@numberOfDays", numberOfDays)
                End With
                con.Execute(deletecmd)
            End Using
        Else
            Using deletecmd As New SqlCommand(String.Format("delete from BPAPassword where userid=@userid and active = 0"))
                With deletecmd.Parameters
                    .AddWithValue("@userid", user.Id)
                End With
                con.Execute(deletecmd)
            End Using
        End If
    End Sub

    ''' <summary>
    ''' Updates given user's password expiry date to be 4 weeks further on than
    ''' the current expiry date or today if the user has no current expiry date.
    ''' </summary>
    ''' <param name="userName">The users name</param>
    ''' <param name="password">The users existing password</param>
    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Sub UpdatePasswordExpiryDate(userName As String, password As SafeString) Implements IServer.UpdatePasswordExpiryDate

        Using con = GetConnection()
            Dim result = LoginAttempt(con, userName, password)
            If result.IsPasswordUpdateAllowed Then
                CheckPermissions(Nothing, result.User)
                UpdatePasswordExpiryDate(con, result.User)
            End If
        End Using

    End Sub

    ''' <summary>
    '''  Updates the logged in user's password expiry date to be 4 weeks further on
    ''' than the current expiry date or today if the user has no current expiry date.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    Private Sub UpdatePasswordExpiryDate(con As IDatabaseConnection, user As IUser)
        Using cmd As New SqlCommand(
             " update BPAUser" &
             " set passwordexpirydate =" &
             "   dateadd(week, isnull(passworddurationweeks, 4), getdate())" &
             " where userid = @UserID")
            cmd.Parameters.AddWithValue("@UserID", user.Id)
            con.Execute(cmd)
        End Using

    End Sub

    Private Sub RemovePasswordDataOnUserRecord(con As IDatabaseConnection, user As IUser)
        Using cmd As New SqlCommand(
             " update BPAUser" &
             " set validtodate = null" &
             ", PasswordDurationWeeks = null" &
             ", passwordexpirydate = null" &
             " where userid = @UserID")
            cmd.Parameters.AddWithValue("@UserID", user.Id)
            con.Execute(cmd)
        End Using
    End Sub

    Private Sub DeletePasswordRecordForUser(con As IDatabaseConnection, user As IUser)
        Dim cmd As New SqlCommand(
            " delete from BPAPassword where userid = @bpuserid")
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
        End With
        con.Execute(cmd)
    End Sub

    Private Sub DeleteUserGroupRecords(con As IDatabaseConnection, user As IUser)
        Dim cmd As New SqlCommand(
            " delete from BPAGroupUser where memberid = @bpuserid")
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
        End With
        con.Execute(cmd)
    End Sub

    Private Sub DeleteUserGroupPrefRecords(con As IDatabaseConnection, user As IUser)
        Dim cmd As New SqlCommand(
            " delete from BPAGroupUserPref where UserId = @bpuserid")
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Updates the roles in the system to the given roleset.
    ''' After completing this method, any roles with temporary IDs in the given role
    ''' set will have their IDs set to the values assigned to them by the database;
    ''' this is also the returned value, meaning that the new IDs can be retrieved
    ''' over a BP Server connection.
    ''' </summary>
    ''' <param name="rs">The set of roles to update the system roles to.</param>
    ''' <returns>The roleset after updating, with any temporary IDs replaced by the
    ''' IDs assigned to the roles by the database.</returns>
    <SecuredMethod(Permission.SystemManager.Security.UserRoles)>
    Public Function UpdateRoles(rs As RoleSet) As RoleSet Implements IServer.UpdateRoles
        CheckPermissions()
        Using con = GetConnection()
            UpdateRoles(con, rs, True)
            Return rs
        End Using
    End Function

    ''' <summary>
    ''' Gets the user objects representing users who are assigned to a specified
    ''' role (excluding deleted users)
    ''' </summary>
    ''' <param name="roleId">The ID of the role for which the assigned users are
    ''' required.</param>
    ''' <returns>A collection of users <em>with no role information stored</em> which
    ''' represents the users assigned to the specified role.</returns>
    <SecuredMethod()>
    Public Function GetActiveUsersInRole(roleId As Integer) As ICollection(Of User) _
     Implements IServer.GetActiveUsersInRole
        CheckPermissions()
        Using con = GetConnection()
            Return GetActiveUsersInRole(con, roleId)
        End Using
    End Function

    ''' <summary>
    ''' Counts the number of users who have a particular role assigned to them on
    ''' the database (excluding deleted users)
    ''' </summary>
    ''' <param name="r">The role to count the number of users assigned with it.
    ''' </param>
    ''' <returns>The number of users found on the database with the specified role.
    ''' </returns>
    <SecuredMethod(Permission.SystemManager.Security.UserRoles)>
    Public Function CountActiveUsersWithRole(r As Role) As Integer Implements IServer.CountActiveUsersWithRole
        CheckPermissions()
        Using con = GetConnection()
            Return CountActiveUsersWithRole(con, r)
        End Using
    End Function

    Private Function GetKeepAliveTimeStampTable(useAutomateCAliveTable As Boolean) As String
        Dim tableNameToUse = "BPAAliveResources"
        If useAutomateCAliveTable Then tableNameToUse = "BPAAliveAutomateC"
        Return tableNameToUse
    End Function

    ''' <summary>
    ''' Removes any keep alive timestamps associated with the current user
    ''' on the local machine.
    ''' </summary>
    <SecuredMethod()>
    Public Sub ClearKeepAliveTimeStamp(Optional useAutomateCAliveTable As Boolean = False) Implements IServer.ClearKeepAliveTimeStamp
        CheckPermissions()
        ' If we have no logged in machine, then there's nothing to clear
        If mLoggedInMachine = "" Then Return

        Dim tableNameToUse = GetKeepAliveTimeStampTable(useAutomateCAliveTable)

        Using con = GetConnection()
            'either update an existing row or create a new one
            Dim updatecmd As New SqlCommand($"delete from {tableNameToUse} where userid=@UserID and MachineName = @MachineName")
            With updatecmd.Parameters
                .AddWithValue("@UserID", GetLoggedInUserId())
                .AddWithValue("@MachineName", mLoggedInMachine)
            End With

            con.ExecuteReturnRecordsAffected(updatecmd)
        End Using
    End Sub


    ''' <summary>
    ''' Places a timestamp in the BPAAliveResources table together
    ''' with the guid of the currently logged in user (if there is one).
    ''' </summary>
    <SecuredMethod()>
    Public Sub SetKeepAliveTimeStamp(Optional useAutomateCAliveTable As Boolean = False) Implements IServer.SetKeepAliveTimeStamp
        CheckPermissions()
        Using con = GetConnection()
            ' Either update an existing row or create a new one
            ' This does an update/insert concurrently, accurately and without errors
            ' https://samsaffron.com/blog/archive/2007/04/04/14.aspx

            Dim tableNameToUse = GetKeepAliveTimeStampTable(useAutomateCAliveTable)

            Dim mergecmd As New SqlCommand(
                 $" update {tableNameToUse} with (serializable)" &
                 " set userid = @UserID, lastupdated = getutcdate() where MachineName = @MachineName and userid = @UserID;" &
                 " if @@rowcount = 0" &
                 " begin" &
                 $"   insert {tableNameToUse} (MachineName, userid, LastUpdated) values (@MachineName, @UserID, getutcdate());" &
                 " end;")
            With mergecmd.Parameters
                .AddWithValue("@UserID", GetLoggedInUserId())
                .AddWithValue("@MachineName", mLoggedInMachine)
            End With
            con.BeginTransaction()
            con.Execute(mergecmd)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Updates the login date/timestamp for this user, and returns both the current
    ''' and previous login dates/times.
    ''' </summary>
    ''' <param name="currLogin">The current login date/time</param>
    ''' <param name="prevLogin">The previous login date/time</param>
    <SecuredMethod()>
    Public Sub UpdateLoginTimestamp(ByRef currLogin As Date, ByRef prevLogin As Date) Implements IServer.UpdateLoginTimestamp
        CheckPermissions()
        Using con = GetConnection()
            'Get previous login date/timestamp & replace with current
            Dim cmd As New SqlCommand(
                "update BPAUser" &
                " set lastsignedin = getutcdate()" &
                " output" &
                "  deleted.lastsignedin as 'Previous'," &
                "  inserted.lastsignedin as 'Current'" &
                " where userid = @id;")
            With cmd.Parameters
                .AddWithValue("@id", GetLoggedInUserId())
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                reader.Read()
                currLogin = prov.GetValue("Current", Date.MinValue)
                prevLogin = prov.GetValue("Previous", Date.MinValue)
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Reads the preference of the currently logged-in user from the BPAUser table
    ''' as to whether they like to use edit summaries.
    ''' </summary>
    ''' <param name="preference">A boolean to contain the user's preference.</param>
    <SecuredMethod()>
    Public Sub GetUserEditSummariesPreference(ByRef preference As Boolean) Implements IServer.GetUserEditSummariesPreference
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("SELECT UseEditSummaries FROM BPAUser where userID = @UserID")
            With cmd.Parameters
                .AddWithValue("@UserID", GetLoggedInUserId())
            End With

            preference = CBool(con.ExecuteReturnScalar(cmd))
        End Using
    End Sub

    ''' <summary>
    ''' Sets the preference of the currently logged-in user to the
    ''' BPAUser table as to whether they like to use edit summaries.
    ''' </summary>
    ''' <param name="preference">A boolean containing the user's preference.</param>
    <SecuredMethod()>
    Public Sub SetUserEditSummariesPreference(ByVal preference As Boolean) Implements IServer.SetUserEditSummariesPreference
        CheckPermissions()
        Using con = GetConnection()
            Dim i As Integer = 0
            If preference = True Then i = 1
            Dim cmd As New SqlCommand("UPDATE BPAUser SET UseEditSummaries = @UseEditSummaries where userID = @UserID")
            With cmd.Parameters
                .AddWithValue("@UseEditSummaries", i.ToString)
                .AddWithValue("@UserID", GetLoggedInUserId())
            End With
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Gets the user from the database with the given ID.
    ''' </summary>
    ''' <param name="userId">The ID of the required user.</param>
    ''' <returns>The User object corresponding to the given ID.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="userId"/> is
    ''' empty - ie. <see cref="Guid.Empty"/>.</exception>
    ''' <exception cref="NoSuchElementException">If no user with the given user ID
    ''' was found on the database.</exception>
    <SecuredMethod()>
    Public Function GetUser(ByVal userId As Guid) As User Implements IServer.GetUser
        CheckPermissions()
        Using con = GetConnection()
            Return GetUser(con, userId, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Gets the user from the database with the given username.
    ''' </summary>
    ''' <param name="userName">The username of the required user.</param>
    ''' <returns>The User object corresponding to the given username.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="userName"/> is
    ''' null or empty.</exception>
    ''' <exception cref="NoSuchElementException">If no user with the given username
    ''' was found on the database.</exception>
    <SecuredMethod()>
    Public Function GetUser(ByVal username As String) As User Implements IServer.GetUser
        CheckPermissions()
        If username = "" Then Throw New ArgumentNullException(NameOf(username))
        Using con = GetConnection()
            Return GetUser(con, Nothing, username)
        End Using
    End Function

    ''' <summary>
    ''' Gets all the users from the database
    ''' </summary>
    ''' <param name="populateRoles">True to populate the roles inside the user; false
    ''' to leave that information blank. Populating the roles is potentially a long
    ''' operation, especially for AD users, and is not necessary for users in all
    ''' contexts.</param>
    ''' <returns>A collection of all users from the database</returns>
    <SecuredMethod()>
    Public Function GetAllUsers(Optional populateRoles As Boolean = True) As ICollection(Of User) Implements IServer.GetAllUsers
        CheckPermissions()
        Using con = GetConnection()
            Return GetUsers(con, Nothing, populateRoles)
        End Using
    End Function

    ''' <summary>
    ''' Gets all the user names currently stored in the database whether they are
    ''' login users or system users.
    ''' </summary>
    ''' <returns>Gets names of all users held on the system, with system user names
    ''' being surrounded by square brackets.</returns>
    <SecuredMethod()>
    Public Function GetAllUserNames() As ICollection(Of String) Implements IServer.GetAllUserNames
        CheckPermissions()
        Return GetUserNames(UserType.All)
    End Function

    ''' <summary>
    ''' Gets all the names of login users currently stored in the database.
    ''' </summary>
    ''' <returns>A collection of all login user names held on the system.</returns>
    <SecuredMethod()>
    Public Function GetLoginUserNames() As ICollection(Of String) Implements IServer.GetLoginUserNames
        CheckPermissions()
        Return GetUserNames(UserType.Login)
    End Function

    ''' <summary>
    ''' Gets all the names of system users currently stored in the database, with
    ''' each name surrounded by square brackets.
    ''' </summary>
    ''' <returns>The system user names in the database.</returns>
    <SecuredMethod()>
    Public Function GetSystemUserNames() As ICollection(Of String) Implements IServer.GetSystemUserNames
        CheckPermissions()
        Return GetUserNames(UserType.System)
    End Function

    ''' <summary>
    ''' Inserts an authorisation token into the database for the passed user.
    ''' </summary>
    ''' <returns>Returns the token on success.</returns>
    <SecuredMethod()>
    Public Function RegisterAuthorisationToken() As clsAuthToken Implements IServer.RegisterAuthorisationToken
        CheckPermissions()
        Using con = GetConnection()
            Return RegisterAuthorisationToken(con, mLoggedInUser, Guid.Empty)
        End Using
    End Function

    <SecuredMethod()>
    Public Function RegisterAuthorisationTokenWithExpiryTime(validityDuration As Integer) As clsAuthToken Implements IServer.RegisterAuthorisationTokenWithExpiryTime
        CheckPermissions()
        Using con = GetConnection()
            Return RegisterAuthorisationToken(con, mLoggedInUser, Guid.Empty, validityDuration:=validityDuration)
        End Using
    End Function

    ''' <summary>
    ''' Inserts an authorisation token into the database for the passed user.
    ''' </summary>
    ''' <returns>Returns the token on success.</returns>
    <UnsecuredMethod()>
    Public Function RegisterAuthorisationToken(username As String, password As SafeString, processid As Guid, authMode As AuthMode) As clsAuthToken Implements IServer.RegisterAuthorisationToken
        Using con = GetConnection()
            Dim owningUser = ValidateCredentials(username, password, authMode)
            If owningUser Is Nothing Then Return Nothing
            Return RegisterAuthorisationToken(con, owningUser, processid)
        End Using
    End Function

    <SecuredMethod()>
    Public Function RegisterAuthorisationToken(processId As Guid) As clsAuthToken Implements IServer.RegisterAuthorisationToken
        CheckPermissions()
        Using con = GetConnection()
            Return RegisterAuthorisationToken(con, mLoggedInUser, processId)
        End Using
    End Function

    <SecuredMethod()>
    Public Function ValidateWebServiceRequest(username As String, password As SafeString, processId As Guid) As IUser Implements IServer.ValidateWebServiceRequest
        CheckPermissions()
        Using con = GetConnection()
            Dim user As User
            Try
                user = GetUser(con, Guid.Empty, username)
            Catch ex As Exception
                Return Nothing
            End Try

            Dim authenticationMode = user.AuthType
            Dim authenticatedUser = ValidateCredentials(username, password, authenticationMode)
            If authenticatedUser IsNot Nothing Then
                authenticatedUser.AuthorisationToken = RegisterAuthorisationToken(con, authenticatedUser, processId, webService:=True)
            End If
            Return authenticatedUser
        End Using
    End Function



    Private Function RegisterAuthorisationToken(con As IDatabaseConnection, owningUser As IUser, processId As Guid, Optional validityDuration As Integer = clsAuthToken.DefaultTokenExpiryInterval, Optional webService As Boolean = False) As clsAuthToken
        Dim flatRoles = CollectionUtil.Join(owningUser.Roles.Select(Function(r) r.Id), ",")
        Dim cmd As New SqlCommand(
             " declare @Expiry datetime
               set @Expiry = DateAdd(ss, @ValidityDuration, GETUTCDATE())
               insert into BPAInternalAuth (UserID, Roles, LoggedInMode, Token, Expiry, ProcessId, IsWebService)
               values (@UserID, @Roles, @LoggedInMode, @TokenValue, @Expiry, @ProcessId, @IsWebService)
               select @Expiry")

        Dim tokenValue = Guid.NewGuid  'our token values are just random
        With cmd.Parameters
            .AddWithValue("@UserID", owningUser.Id)
            .AddWithValue("@Roles", flatRoles)
            .AddWithValue("@LoggedInMode", mLoggedInMode)
            .AddWithValue("@TokenValue", tokenValue)
            .AddWithValue("@ValidityDuration", validityDuration)
            .AddWithValue("@ProcessId", IIf(processId = Guid.Empty, DBNull.Value, processId))
            .AddWithValue("@IsWebService", webService)
        End With

        Dim expiryTime = CType(con.ExecuteReturnScalar(cmd), DateTime)
        Return New clsAuthToken(owningUser.Id, tokenValue, expiryTime, processId)
    End Function

    ''' <summary>
    ''' Returns the group permission restricted states for the passed list of groups
    ''' </summary>
    ''' <param name="groupIDs">List of group to check</param>
    ''' <returns>The restricted states by group id</returns>
    <SecuredMethod()>
    Public Function GetGroupPermissionStates(groupIDs As List(Of Guid)) _
        As Dictionary(Of Guid, PermissionState) Implements IServer.GetGroupPermissionStates
        CheckPermissions()

        Dim result As New Dictionary(Of Guid, PermissionState)

        Dim groupIdStr = groupIDs.Aggregate("",
                                                Function(str, guid)
                                                    If String.IsNullOrEmpty(str) Then Return String.Format("'{0}'", guid)
                                                    Return String.Format("{0}, '{1}'", str, guid)
                                                End Function)

        Using con = GetConnection()
            Dim cmd As New SqlCommand(String.Format("
                select id, isrestricted from BPAGroup
                where id in ({0})", groupIdStr))

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim id As Guid = prov.GetGuid("id")
                    Dim restriction = PermissionState.UnRestricted
                    Dim isrestricted = prov.GetValue("isrestricted", False)
                    If isrestricted Then
                        restriction = PermissionState.Restricted
                    End If
                    result.Add(id, restriction)
                End While
            End Using
        End Using
        Return result
    End Function

    ''' <summary>
    ''' Gets the group level permissions for the passed group, including whether
    ''' or not group level restrictions apply.
    ''' </summary>
    ''' <param name="groupID">The ID of the group</param>
    ''' <returns>The actual set of permissions for the given group</returns>
    <SecuredMethod()>
    Public Function GetActualGroupPermissions(groupID As Guid) As IGroupPermissions _
        Implements IServer.GetActualGroupPermissions
        CheckPermissions()

        Using con = GetConnection()
            Return GetActualGroupPermissions(con, groupID)
        End Using
    End Function

    ''' <summary>
    ''' Gets the group level permissions for the passed group, including whether
    ''' or not group level restrictions apply.
    ''' </summary>
    ''' <param name="connection">The database connection</param>
    ''' <param name="groupID">The ID of the group</param>
    ''' <returns>The actual set of permissions for the given group</returns>
    Private Function GetActualGroupPermissions(connection As IDatabaseConnection, groupID As Guid) As IGroupPermissions
        Return mActualGroupPermissionsCache.Value.GetValue(
            groupID.ToString(),
            Function() _
                GetActualGroupPermissionsForGroup(connection, groupID))
    End Function

    Private Function GetActualGroupPermissionsForGroup(connection As IDatabaseConnection, groupID As Guid) As GroupPermissions
        Dim groupPermissions As New GroupPermissions(groupID, PermissionState.Unknown)
        With groupPermissions

            ' Determine whether or not restrictions apply to this group
            Using command As New SqlCommand(
                " select isrestricted from BPAGroup " &
                " where id=@group")

                command.Parameters.AddWithValue("@group", groupID)
                If (CBool(connection.ExecuteReturnScalar(command))) Then
                    .State = PermissionState.Restricted
                Else
                    .State = PermissionState.UnRestricted
                End If
            End Using

            ' If restrictions appy, then get available permissions for each role
            If .State = PermissionState.Restricted Then
                Using command As New SqlCommand(
                    " select g.userroleid,g.permid,r.name as name" &
                    " from BPAGroupUserRolePerm g " &
                    "   join BPAUserRole r on r.id = g.userroleid" &
                    " where g.groupid = @group order by g.userroleid")

                    command.Parameters.AddWithValue("@group", groupID)
                    Using reader = connection.ExecuteReturnDataReader(command)
                        Dim provider As New ReaderDataProvider(reader)
                        While reader.Read()
                            Dim roleID = provider.GetInt("userroleid")
                            Dim roleName = provider.GetString("name")
                            Dim groupLevelPermissions = .FirstOrDefault(Function(g) g.Id = roleID)
                            If groupLevelPermissions Is Nothing Then
                                groupLevelPermissions = New GroupLevelPermissions(roleID, roleName)
                                .Add(groupLevelPermissions)
                            End If
                            groupLevelPermissions.Add(Permission.GetPermission(provider.GetInt("permid")))
                        End While
                    End Using
                End Using
            End If
        End With
        Return groupPermissions
    End Function

    ''' <summary>
    ''' Gets the set of permissions for of a group. They may not actaully be set on that group,
    ''' but may be inherited from a parent further up the tree.
    ''' </summary>
    ''' <param name="groupID">The ID of the group</param>
    ''' <returns>The effective group permissions</returns>
    <SecuredMethod()>
    Public Function GetEffectiveGroupPermissions(groupID As Guid) As IGroupPermissions _
        Implements IServer.GetEffectiveGroupPermissions

        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveGroupPermissions(con, groupID)
        End Using
    End Function

    ''' <summary>
    ''' Find out if a given id is used to represent a pool. If it is,
    ''' return the id of the group that contains it.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="id">identitiy of object suspected of being a pool</param>
    ''' <param name="containingGroup">output param representing the containing group</param>
    ''' <returns></returns>
    Private Function IsPool(con As IDatabaseConnection, id As Guid, ByRef containingGroup As Guid) As Boolean
        Using cmd As New SqlCommand("select groupid from BPVGroupedResources where Id = @Id")
            cmd.Parameters.AddWithValue("Id", id)
            containingGroup = IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
            Return containingGroup <> Guid.Empty
        End Using
    End Function

    ''' <summary>
    ''' Gets the set of permissions for of a group. They may not actually be set on that group,
    ''' but may be inherited from a parent further up the tree.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="groupID">The ID of the group</param>
    ''' <returns>The effective group permissions</returns>
    Private Function GetEffectiveGroupPermissions(con As IDatabaseConnection, groupID As Guid) As IGroupPermissions

        Return mEffectiveGroupPermissionsCache.Value.GetValue(
            groupID.ToString(),
            Function()
                ' Check this isn't a pool impersonating a group
                Dim containingGroupId As Guid = Guid.Empty
                If IsPool(con, groupID, containingGroupId) Then
                    groupID = containingGroupId
                End If

                ' Quick win - are there no restricted groups further up the tree
                ' if not, return the permissions of the given group
                Dim inheritedAncestorGroupId = GetRestrictedAncestorGroupId(con, groupID)
                If inheritedAncestorGroupId = Guid.Empty Then
                    Return GetActualGroupPermissions(con, groupID)

                Else
                    Dim ancestorPerms = GetActualGroupPermissions(con, inheritedAncestorGroupId)

                    ' create a new permissions object for the current group
                    ' using the permissions of the ancestor group
                    Dim m As New GroupPermissions(groupID,
                                                  PermissionState.Unknown) _
                            With {.InheritedAncestorID = inheritedAncestorGroupId}
                    m.Merge(ancestorPerms)
                    m.State = PermissionState.RestrictedByInheritance
                    Return m
                End If
            End Function)

    End Function

    ''' <summary>
    ''' Walks the tree branch from a given group to the first group which
    ''' has restrited permissions.
    ''' </summary>
    ''' <param name="con">database connection</param>
    ''' <param name="groupID">Id of group to find restricted ancestor of</param>
    ''' <returns>The Id of the restricted ancestor group</returns>
    Private Function GetRestrictedAncestorGroupId(con As IDatabaseConnection, groupID As Guid) As Guid
        Using cmd As New SqlCommand(
            "With Groups As (
                 Select gp.isrestricted, gp.id from BPAGroup gp where gp.id=@groupid
                 union all
                 Select gp.isrestricted, gg.groupid from BPAGroupGroup gg
                    inner Join groups b on b.id=gg.memberid
                    inner Join BPAGroup gp on gp.id=gg.groupid)
            Select top 1 id from groups where isrestricted=1 and id <> @groupID")

            cmd.Parameters.AddWithValue("groupId", groupID)

            Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)

        End Using

    End Function

    ''' <summary>
    ''' Update the group level permissions for the passed group, including whether
    ''' or not group level restrictions apply for the group.
    ''' </summary>
    ''' <param name="groupId">The Id of the group that the permissions relate to</param>
    ''' <param name="groupPermissions">New set of permissions for this group</param>
    <SecuredMethod()>
    Public Sub SetActualGroupPermissions(groupId As Guid, groupPermissions As IGroupPermissions) _
        Implements IServer.SetActualGroupPermissions
        Using connection = GetConnection()

            Dim treeType As GroupTreeType
            Dim targetGroup = GetGroup(connection, groupId, True, treeType)
            Dim requiredPermission = treeType.GetTreeDefinition().AccessRightsPermission.Name

            ' Check role-based permissions
            CheckPermissions({requiredPermission})

            ' Also check group-based permissions
            If Not GetEffectiveGroupPermissions(connection, groupId).HasPermission(mLoggedInUser, requiredPermission) Then
                Throw New PermissionException(My.Resources.clsServer_YouDoNotHavePermissionToChangeAccessRightsForThisGroup)
            End If

            If targetGroup Is Nothing Then
                Throw New BluePrismException(My.Resources.clsServer_ThisGroupNoLongerExists)
            ElseIf targetGroup.ContainsHiddenMembers Then
                Throw New BluePrismException(My.Resources.clsServer_YouCannotChangeTheAccessRightsForThisGroupItContainsHiddenItemsYouDoNotHaveAcce)
            End If

            connection.BeginTransaction()

            ' Remove any permissions for this group, and also any subgroups
            Dim affectedgroups = targetGroup.Search(
                               Function(m) m.IsGroup).Select(Function(g) g.IdAsGuid)

            Dim groupIds = affectedgroups.Aggregate("",
                               Function(str, guid)
                                   If String.IsNullOrEmpty(str) Then _
                                        Return String.Format("'{0}'", guid)
                                   Return String.Format("{0}, '{1}'", str, guid)
                               End Function)

            Dim existing = GetActualGroupPermissions(connection, targetGroup.IdAsGuid)

            Using command As New SqlCommand(String.Format(
                " delete from BPAGroupUserRolePerm" &
                " where groupid in ({0})" &
                " update BPAGroup set isrestricted=0" &
                " where id in ({0})", groupIds))
                connection.Execute(command)
            End Using

            ' Set the group level restriction flag accordingly
            Using command As New SqlCommand(
                " update BPAGroup set isrestricted=@restricted" &
                " where id=@group")

                ' perms can be nothing if we are clearing the permissions.
                Dim restricted = False
                If groupPermissions IsNot Nothing Then _
                restricted = (groupPermissions.State = PermissionState.Restricted)

                command.Parameters.AddWithValue("@group", targetGroup.IdAsGuid)
                command.Parameters.AddWithValue("@restricted", restricted)
                connection.Execute(command)
            End Using

            Dim sbAudit As New StringBuilder
            sbAudit.AppendFormat(My.Resources.clsServer_GroupType0GroupName1,
                                      TreeDefinitionAttribute.GetLocalizedFriendlyName(treeType.GetTreeDefinition.SingularName),
                                      targetGroup.Path)

            ' If the group is restricted then there may be permissions to apply to
            ' each role. Note that permissions can be 'taken away' at group level
            ' so it is possible that the permissions set is empty
            ' If perms is nothing we are clearing the restrictions.
            If groupPermissions IsNot Nothing AndAlso
               groupPermissions.State = PermissionState.Restricted Then

                Dim sb As StringBuilder = Nothing
                ' Take a copy of the list as the cache may refresh this reference during update
                ' on a direct connection.
                For Each userRole In groupPermissions.ToList()

                    Dim existingGroupLevelPermission = existing.GetGroupLevelPermission(userRole.Id)

                    Dim rolesAdded = False
                    For Each permission In userRole.Permissions.ToList()
                        If sb Is Nothing Then
                            sb = New StringBuilder(
                            "insert into BPAGroupUserRolePerm (groupid, userroleid, permid)")
                        Else
                            sb.Append(" union all")
                        End If
                        sb.AppendFormat(" select @group, {0}, {1}", userRole.Id, permission.Id)
                        If existingGroupLevelPermission Is Nothing OrElse
                            Not existingGroupLevelPermission.Permissions.Contains(permission) Then
                            If Not rolesAdded Then
                                sbAudit.AppendFormat(My.Resources.clsServer_Role0, LTools.GetC(userRole.Name, "roleperms", "role"))
                                rolesAdded = True
                            End If
                            sbAudit.AppendFormat(My.Resources.clsServer_0WasSet, LTools.GetC(permission.Name, "roleperms", "perm"))
                        Else
                            existingGroupLevelPermission.Remove(permission)
                        End If
                    Next
                    If Not rolesAdded Then
                        existing.Remove(userRole)
                    End If
                Next

                For Each userRole In existing.ToList()
                    Dim rolesRemoved = False
                    For Each permission In userRole.Permissions
                        If Not rolesRemoved Then
                            sbAudit.AppendFormat(My.Resources.clsServer_Role0, LTools.GetC(userRole.Name, "roleperms", "role"))
                            rolesRemoved = True
                        End If
                        sbAudit.AppendFormat(My.Resources.clsServer_0WasRemoved, LTools.GetC(permission.Name, "roleperms", "perm"))
                    Next
                Next

                ' Insert role permissions if required
                If sb IsNot Nothing Then
                    Using command As New SqlCommand(sb.ToString())
                        command.Parameters.AddWithValue("@group", targetGroup.IdAsGuid)
                        connection.Execute(command)
                    End Using
                End If
            End If

            AuditRecordSysConfigEvent(connection, SysConfEventCode.ModifyGroupPermissions,
                                      sbAudit.ToString)
            connection.CommitTransaction()
        End Using

        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Returns the set of permissions that are available to members of the passed
    ''' tree type (e.g. Object, Processes, Resources etc.)
    ''' </summary>
    ''' <param name="treeType">The tree type</param>
    ''' <returns>The set of available permissions</returns>
    <SecuredMethod()>
    Public Function GetGroupAvailablePermissions(treeType As GroupTreeType) As ICollection(Of GroupTreePermission) _
        Implements IServer.GetGroupAvailablePermissions
        CheckPermissions()
        Using con = GetConnection()
            Return GetGroupAvailablePermissions(con, treeType)
        End Using
    End Function

    ''' <summary>
    ''' Returns the effective (group based) permissions for the passed group member,
    ''' considering all groups within the tree that contain it.
    ''' </summary>
    ''' <param name="member">The group member</param>
    ''' <returns>The effective permissions</returns>
    <SecuredMethod()>
    Public Function GetEffectiveGroupPermissionsForMember(member As IGroupMember) As ICollection(Of IGroupPermissions) _
        Implements IServer.GetEffectiveGroupPermissionsForMember
        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveGroupPermissionsForMember(con, member)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function HasUserGotCreatePermissionOnDefaultGroup(treeTypes As List(Of GroupTreeType)) As Dictionary(Of GroupTreeType, Boolean) _
        Implements IServer.HasUserGotCreatePermissionOnDefaultGroup
        CheckPermissions()
        Using connection = GetConnection()
            Return HasUserGotCreatePermissionOnDefaultGroup(connection, treeTypes)
        End Using
    End Function


    Private Function HasUserGotCreatePermissionOnDefaultGroup(connection As IDatabaseConnection, treeTypes As List(Of GroupTreeType)) As Dictionary(Of GroupTreeType, Boolean)

        Return treeTypes.
        ToDictionary(Of GroupTreeType, Boolean)(
            (Function(x) x),
            Function(x) Not HasDefaultGroup(connection, x) OrElse GetEffectiveGroupPermissions(connection, GetDefaultGroupId(connection, x)).HasPermission(mLoggedInUser, x.GetTreeDefinition().CreateItemPermission))

    End Function


    ''' <summary>
    ''' Returns the effective (group based) permissions for the passed group member,
    ''' considering all groups within the tree that contain it.
    ''' </summary>
    ''' <param name="con">database connection</param>
    ''' <param name="member">The group member</param>
    ''' <returns></returns>
    Private Function GetEffectiveGroupPermissionsForMember(con As IDatabaseConnection, member As IGroupMember) As ICollection(Of IGroupPermissions)
        Dim perms As New List(Of IGroupPermissions)()
        For Each id In GetIdsOfGroupsContaining(con, DirectCast(member, GroupMember))
            perms.Add(GetEffectiveGroupPermissions(con, id))
        Next
        Return perms
    End Function


    ''' <summary>
    ''' Gets the combined set of permissions that a groupmember has in a light-weight
    ''' container.
    ''' </summary>
    ''' <param name="groupMember">Identity of member</param>
    ''' <returns></returns>
    <SecuredMethod()>
    Public Function GetEffectiveMemberPermissions(groupMember As IGroupMember) As IMemberPermissions _
        Implements IServer.GetEffectiveMemberPermissions
        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveMemberPermissions(con, groupMember)
        End Using
    End Function

    ''' <summary>
    ''' Gets the combined set of permissions that a groupmember has in a light-weight
    ''' </summary>
    ''' <param name="con">database connection</param>
    ''' <param name="groupMember">the group to get the permissions for</param>
    ''' <returns></returns>
    Private Function GetEffectiveMemberPermissions(con As IDatabaseConnection, groupMember As IGroupMember) As IMemberPermissions
        Return GetEffectiveMemberPermissions(con, groupMember, mLoggedInUser)
    End Function


    Private Function GetEffectiveMemberPermissions(con As IDatabaseConnection, groupMember As IGroupMember, user As IUser) As IMemberPermissions
        If groupMember.IsGroup() Then
            Dim grpPerms = GetEffectiveGroupPermissions(con, groupMember.IdAsGuid)
            Return New MemberPermissions(grpPerms)

        ElseIf groupMember.MemberType = GroupMemberType.Object _
            OrElse groupMember.MemberType = GroupMemberType.Process Then
            Return GetEffectiveMemberPermissionsForProcess(con, groupMember.IdAsGuid)

        ElseIf groupMember.MemberType = GroupMemberType.Resource Then
            Return GetEffectiveMemberPermissionsForResource(con, groupMember.IdAsGuid)

        Else
            ' not an object type we care about
            Return New MemberPermissions(New GroupPermissions(PermissionState.Unknown))
        End If
    End Function


    ''' <summary>
    ''' Gets the combined set of permissions for a given process or object
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the process</param>
    ''' <returns>The current permissions for that process</returns>
    <SecuredMethod()>
    Public Function GetEffectiveGroupPermissionsForProcess(id As Guid) As IGroupPermissions _
            Implements IServer.GetEffectiveGroupPermissionsForProcess
        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveGroupPermissionsForProcess(con, id)
        End Using
    End Function


    Private Function GetGroupsForProcess(connection As IDatabaseConnection, id As Guid) As List(Of Guid)
        Dim groups As New List(Of Guid)
        Using command As New SqlCommand("select groupid from BPVGroupedProcessesObjects where id = @processid")
            command.Parameters.AddWithValue("@processid", id.ToString)
            Using dataReader As IDataReader = connection.ExecuteReturnDataReader(command)
                Dim readerDataProvider As New ReaderDataProvider(dataReader)
                While dataReader.Read()
                    groups.Add(readerDataProvider.GetValue("groupid", Guid.Empty))
                End While
            End Using
        End Using
        Return groups
    End Function

    Private Function GetGroupsForResource(connection As IDatabaseConnection, id As Guid) As List(Of Guid)
        Dim groups As New List(Of Guid)
        Using command As New SqlCommand("select groupid from BPAGroupResource where memberid = @resourceid")
            command.Parameters.AddWithValue("@resourceid", id.ToString)
            Using dataReader As IDataReader = connection.ExecuteReturnDataReader(command)
                Dim readerDataProvider As New ReaderDataProvider(dataReader)
                While dataReader.Read()
                    groups.Add(readerDataProvider.GetValue("groupid", Guid.Empty))
                End While
            End Using
        End Using
        Return groups
    End Function


    ''' <summary>
    ''' Gets the combined set of permissions for a given process or object
    ''' </summary>
    ''' <param name="con">Connection to database</param>
    ''' <param name="id">id of the process</param>
    ''' <returns>The current permissions for that process</returns>
    Private Function GetEffectiveGroupPermissionsForProcess(con As IDatabaseConnection, id As Guid) As GroupPermissions
        Dim groups = mProcessGroupsCache.Value.GetValue(
            id.ToString(),
            Function() GetGroupsForProcess(con, id))

        ' Get the merge user permissions across these groups.
        Dim perms As New GroupPermissions(PermissionState.Unknown)

        If groups.Count > 0 Then
            For Each group As Guid In groups
                If group <> Guid.Empty Then
                    ' Get the effective permissions incase nodes further up the tree have
                    ' restrictions.
                    perms.Merge(GetEffectiveGroupPermissions(con, group))
                End If
            Next
        Else
            ' We can assume unrestricted instead of unknown here because we know processes
            ' should have permissions.
            perms.State = PermissionState.UnRestricted
        End If

        Return perms

    End Function

    ''' <summary>
    ''' Gets the combined set of permissions for a given resource
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the resource</param>
    ''' <returns>The current permissions for that resource</returns>
    <SecuredMethod()>
    Public Function GetEffectiveMemberPermissionsForResource(id As Guid) As IMemberPermissions _
            Implements IServer.GetEffectiveMemberPermissionsForResource
        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveMemberPermissionsForResource(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Gets the combined set of permissions for a given resource
    ''' </summary>
    ''' <param name="con">Connection to database</param>
    ''' <param name="id">id of the resource</param>
    ''' <returns>The current permissions for that resource</returns>
    Private Function GetEffectiveMemberPermissionsForResource(con As IDatabaseConnection, id As Guid) As IMemberPermissions
        Dim perms = GetEffectiveGroupPermissionsForResource(con, id)
        Return New MemberPermissions(perms)
    End Function


    ''' <summary>
    ''' Gets the combined set of permissions for a given process or object
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the process</param>
    ''' <returns>The current permissions for that process</returns>
    <SecuredMethod()>
    Public Function GetEffectiveGroupPermissionsForResource(id As Guid) As IGroupPermissions _
            Implements IServer.GetEffectiveGroupPermissionsForResource
        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveGroupPermissionsForResource(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Gets the combined set of permissions for a given process or object
    ''' </summary>
    ''' <param name="con">Connection to database</param>
    ''' <param name="id">id of the process</param>
    ''' <returns>The current permissions for that process</returns>
    Private Function GetEffectiveGroupPermissionsForResource(con As IDatabaseConnection, id As Guid) As IGroupPermissions
        Dim groups = mResourceGroupsCache.Value.GetValue(
            id.ToString(),
            Function() GetGroupsForResource(con, id))

        ' Get the merge user permissions across these groups.
        Dim perms As New GroupPermissions(PermissionState.Unknown)

        If groups.Count > 0 Then
            For Each group As Guid In groups
                If group <> Guid.Empty Then
                    ' Get the effective permissions incase nodes further up the tree have
                    ' restrictions.
                    perms.Merge(GetEffectiveGroupPermissions(con, group))
                End If
            Next
        Else
            ' We can assume unrestricted instead of unknown here because we know processes
            ' should have permissions.
            perms.State = PermissionState.UnRestricted
        End If

        Return perms

    End Function
    '  condition As Predicate(Of MemberPermissions)) As Boolean _
    <SecuredMethod()>
    Public Function TestMemberCanAccessProcesses(processes As List(Of Guid)) As Boolean _
        Implements IServer.TestMemberCanAccessProcesses
        CheckPermissions()

        Dim condition As Predicate(Of IMemberPermissions) = Function(x) Not x.IsRestricted OrElse x.HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedExecuteProcess)
        Using con = GetConnection()
            Return processes.Select(Function(y) GetEffectiveMemberPermissionsForProcess(con, y)).All(Function(x) condition(DirectCast(x, IMemberPermissions)))
        End Using
    End Function

    <SecuredMethod()>
    Public Function TestMemberCanAccessObjects(processes As List(Of Guid)) As Boolean _
        Implements IServer.TestMemberCanAccessObjects
        CheckPermissions()

        Dim condition As Predicate(Of IMemberPermissions) = Function(x) Not x.IsRestricted OrElse x.HasPermission(mLoggedInUser, Permission.ObjectStudio.ImpliedExecuteBusinessObject)
        Using con = GetConnection()
            Return processes.Select(Function(y) GetEffectiveMemberPermissionsForProcess(con, y)).All(Function(x) condition(DirectCast(x, IMemberPermissions)))
        End Using
    End Function

    ''' <summary>
    ''' process of object.
    ''' container.
    ''' </summary>
    ''' <param name="id">Identity of the process</param>
    ''' <returns>The current permissions for that process</returns>
    <SecuredMethod()>
    Public Function GetEffectiveMemberPermissionsForProcess(id As Guid) As IMemberPermissions _
    Implements IServer.GetEffectiveMemberPermissionsForProcess
        CheckPermissions()
        Using con = GetConnection()
            Return GetEffectiveMemberPermissionsForProcess(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Gets the combined set of permissions for the current user for a given process
    ''' or object
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Private Function GetEffectiveMemberPermissionsForProcess(con As IDatabaseConnection, id As Guid) As IMemberPermissions Implements IServerPrivate.GetEffectiveMemberPermissionsForProcess
        Return GetEffectiveMemberPermissionsForProcess(con, id, mLoggedInUser)
    End Function

    ''' <summary>
    ''' Gets the combined set of permissions for the provided user for a given process
    ''' or object
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Private Function GetEffectiveMemberPermissionsForProcess(con As IDatabaseConnection, id As Guid, user As IUser) As IMemberPermissions
        Dim perms = GetEffectiveGroupPermissionsForProcess(con, id)
        Return New MemberPermissions(perms)
    End Function

    <SecuredMethod()>
    Public Function UserHasAccessToSession(sessionId As Guid) As Boolean Implements IServer.UserHasAccessToSession
        CheckPermissions()
        Using con = GetConnection()
            Dim matchingSessions = GetActualSessions(con, sessionId, Nothing)
            If matchingSessions.Count <> 1 Then Throw New InvalidArgumentException(
                My.Resources.clsServer_NoMatchingSessionFoundWithSessionID0)
            Dim session = matchingSessions(0)

            Dim processPermissions = GetEffectiveMemberPermissionsForProcess(con, session.ProcessID)
            If processPermissions.HasAnyPermissions(mLoggedInUser) Then
                Dim resourcePermissions = GetEffectiveMemberPermissionsForResource(con, session.ResourceID)
                If resourcePermissions.HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource) Then Return True
            End If
        End Using

        Return False
    End Function

    <SecuredMethod(Permission.SystemManager.Security.Users)>
    Public Function GetLoggedInUsersAndMachines() As List(Of (id As Guid, userName As String, machineName As String, transient As Boolean)) Implements IServer.GetLoggedInUsersAndMachines
        CheckPermissions()

        Dim userIds As New List(Of (Guid, String, String, Boolean))

        Using con = GetConnection()
            Using cmd = New SqlCommand(
                "select ar.MachineName, bu.userid, bu.username, ar.LastUpdated
                from BPAAliveResources as ar
                inner join BPAUser as bu on bu.userid = ar.UserID
                where bu.username is not null
                and (datediff(second, ar.LastUpdated, getutcdate()) <= 120)
                ;")

                Dim dt = con.ExecuteReturnDataTable(cmd)
                userIds.AddRange(dt.AsEnumerable().Select(Function(r) (New Guid(r("UserId").ToString()), r("username").ToString(), r("MachineName").ToString(), False)))
            End Using

            Using cmd = New SqlCommand(
                "select arc.MachineName, arc.UserID, bu.username
                from BPAAliveAutomateC as arc
                inner join BPAUser as bu on bu.userid = arc.UserID
                where (datediff(second, arc.LastUpdated, getutcdate()) <= 120)
                ;")

                Dim dt = con.ExecuteReturnDataTable(cmd)
                userIds.AddRange(dt.AsEnumerable().Select(Function(r) (New Guid(r("UserId").ToString()), r("username").ToString(), r("MachineName").ToString(), True)))
            End Using

        End Using
        Return userIds
    End Function

    <SecuredMethod()>
    Public Function GetRunningSessions() As ICollection(Of clsProcessSession) Implements IServer.GetRunningSessions
        CheckPermissions()

        Using con = GetConnection()
            Using cmd = New SqlCommand("select sessionid from BPASession where statusid in (1,7,8)")
                Dim dt = con.ExecuteReturnDataTable(cmd)
                Dim runningSessions As New List(Of clsProcessSession)
                If dt.Rows.Count > 0 Then
                    For Each r As DataRow In dt.Rows
                        Dim sessionId = New Guid(r(0).ToString())
                        runningSessions.Add(GetActualSessions(sessionId)(0))
                    Next
                End If
                Return runningSessions
            End Using
        End Using

    End Function

    <SecuredMethod()>
    Public Function RefreshReloginToken(reloginTokenRequest As ReloginTokenRequest) As SafeString _
        Implements IServer.RefreshReloginToken
        CheckPermissions()
        Using con = GetConnection()
            Dim reloginTokenUserId = mValidateReloginToken(con, reloginTokenRequest)
            If reloginTokenUserId = Guid.Empty OrElse reloginTokenUserId <> GetLoggedInUserId() Then
                Throw New NoValidTokenFoundException()
            End If

            Return GenerateReloginTokenForUser(con, reloginTokenUserId, reloginTokenRequest.MachineName, reloginTokenRequest.ProcessId)
        End Using
    End Function

    <SecuredMethod()>
    Public Sub DeleteReloginToken(machineName As String,
                                        processId As Integer) _
        Implements IServer.DeleteReloginToken
        CheckPermissions()
        Dim userId = GetLoggedInUserId()
        Using con = GetConnection()
            DeleteReloginToken(con, userId, machineName, processId)
        End Using
    End Sub
#End Region

#Region " Unsecured Methods "

    ''' <summary>
    ''' Return the logon options for this environment.
    ''' </summary>
    ''' <returns>The logon options</returns>
    <UnsecuredMethod()>
    Public Function GetLogonOptions(ByRef userList As ICollection(Of User)) As LogonOptions Implements IServer.GetLogonOptions
        Using con = GetConnection()
            Dim options = GetLogonOptions(con)
            If Not options.SingleSignon Then _
                userList = GetUsers(con, Nothing, False)
            Return options
        End Using
    End Function

    ''' <summary>
    ''' Gets the authentication domain to be used with single sign-on
    ''' </summary>
    ''' <returns>The name of the domain stored in the database</returns>
    <UnsecuredMethod()>
    Public Function GetActiveDirectoryDomain() As String Implements IServer.GetActiveDirectoryDomain
        Using con = GetConnection()
            Return GetActiveDirectoryDomain(con)
        End Using
    End Function

    <UnsecuredMethod()>
    Public Function DatabaseType() As DatabaseType Implements IServer.DatabaseType
        Using con = GetConnection()
            Return If(IsSingleSignOn(con), DatabaseType.SingleSignOn, DatabaseType.NativeAndExternal)
        End Using
    End Function

    ''' <summary>
    ''' Log in. This is the Windows Authentication version.
    ''' </summary>
    ''' <param name="machine">The name of the machine the login is from</param>
    ''' <param name="locale">The client locale</param>
    ''' <param name="userID">If login is successful, this contains the ID of the
    ''' logged-in user.</param>
    ''' <returns>A LoginResult describing the result</returns>
    ''' <remarks>Clients should not call this directly - they should always use the
    ''' corresponding method in clsUser, which allows some things to be cached
    ''' locally on the client.</remarks>
    <UnsecuredMethod()>
    Public Function Login(machine As String, locale As String, ByRef userID As Guid) As LoginResult Implements IServer.Login

        If (String.IsNullOrEmpty(machine)) Then Return _
            New LoginResult(LoginResultCode.ComputerNameNotSet)

        'Check if already logged in
        If GetLoggedIn() Then Return New LoginResult(LoginResultCode.Already)

        Using con = GetConnection()

            Try
                'Check database is configured for single sign-on
                If Not IsSingleSignOn(con) Then Return New LoginResult(LoginResultCode.TypeMismatch)

                'Check we have a valid Windows user
                Dim windowsIdentity = GetCurrentIdentity()
                If Not windowsIdentity.IsValidAccountType Then Return New LoginResult(LoginResultCode.BadAccount)
                If Not windowsIdentity.IsAuthenticated Then Return New LoginResult(LoginResultCode.NotAuthenticated)

                'Check domain is accessible (throws exception if not)
                Dim ctx As New DirectoryContext(DirectoryContextType.Domain, GetActiveDirectoryDomain())
                Dim dom = Domain.GetDomain(ctx)

                'Check that the Windows user is a member of this domain (or trusted domain)
                Dim upn = clsActiveDirectory.GetUserPrincipalName(windowsIdentity.Sid)
                If String.IsNullOrEmpty(upn) Then Return New LoginResult(LoginResultCode.MissingUPN)

                'Check user has at least one BP role
                Dim roles = clsActiveDirectory.GetRoles(windowsIdentity)
                If roles.Count = 0 Then Return New LoginResult(LoginResultCode.NoGroups)

                'We have an authenticated user, so ensure they're mirrored in the database
                If Not MirrorUser(con, upn, userID) Then
                    'User already exists so ensure they are activated
                    ActivateUser(con, userID)
                End If

            Catch ex As Exception
                Throw New UnknownLoginException(ex)
            End Try

            'Successfully logged in.
            mLoggedInMachine = machine
            mLoggedInMode = AuthMode.ActiveDirectory
            mLoggedInUser = GetUser(con, userID, Nothing)
            mLoggedInUserLocale = locale
            RecordLoginAuditEvent(con, GetLoggedInUserName(), machine, AuthMode.ActiveDirectory)

            Return New LoginResult(LoginResultCode.Success, DirectCast(mLoggedInUser, User))

        End Using
    End Function

    <UnsecuredMethod()>
    Public Function CurrentWindowsUserIsMappedToABluePrismUser() As Boolean Implements IServer.CurrentWindowsUserIsMappedToABluePrismUser
        Using con = GetConnection()
            If IsSingleSignOn(con) Then Return False

            Dim windowsIdentity = mGetClientIdentity()
            If windowsIdentity Is Nothing Then Return False

            Dim userId = GetMappedActiveDirectoryUserId(con, windowsIdentity.Sid.ToString())
            If userId = Guid.Empty Then Return False

            Try
                Dim user = GetActiveUser(con, userId, False)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Using
    End Function

    <UnsecuredMethod()>
    Public Function LoginWithMappedActiveDirectoryUser(machine As String, locale As String) As LoginResult Implements IServer.LoginWithMappedActiveDirectoryUser
        Using con = GetConnection()
            If IsSingleSignOn(con) Then Return New LoginResult(LoginResultCode.TypeMismatch)
            If GetLoggedIn() Then Return New LoginResult(LoginResultCode.Already)
            Try
                Dim result As LoginResult = LoginWithMappedActiveDirectoryUser(con)
                HandleLoginResult(result, AuthMode.MappedActiveDirectory, machine, locale, con)
                Return result
            Catch ex As Exception
                Throw New UnknownLoginException(ex)
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Log in. The is the Blue Prism Authentication version.
    ''' </summary>
    ''' <param name="username">The username</param>
    ''' <param name="password">The password</param>
    ''' <param name="machine">The name of the machine the login is from</param>
    ''' <param name="locale">The client locale</param>
    ''' <returns>A LoginResult describing the result</returns>
    ''' <remarks>Clients should not call this directly - they should always use the
    ''' corresponding method in clsUser, which allows some things to be cached
    ''' locally on the client.</remarks>
    <UnsecuredMethod()>
    Public Function Login(
     username As String, password As SafeString, machine As String, locale As String) _
     As LoginResult Implements IServer.Login

        If (String.IsNullOrEmpty(machine)) Then Return _
            New LoginResult(LoginResultCode.ComputerNameNotSet)

        Using con = GetConnection()
            If password.IsEmpty Then Return New LoginResult(LoginResultCode.MissingPassword)
            If IsSingleSignOn(con) Then Return New LoginResult(LoginResultCode.TypeMismatch)
            If GetLoggedIn() Then Return New LoginResult(LoginResultCode.Already)
            Try
                Dim result As LoginResult = LoginAttempt(con, username, password)
                HandleLoginResult(result, AuthMode.Native, machine, locale, con)
                Return result
            Catch ex As Exception
                Throw New UnknownLoginException(ex)
            End Try
        End Using
    End Function

    <UnsecuredMethod()>
    Public Function Login(machine As String, locale As String, accessToken As String, Optional reloginTokenRequest As ReloginTokenRequest = Nothing) As LoginResultWithReloginToken Implements IServer.Login

        If String.IsNullOrEmpty(machine) Then Return _
            New LoginResultWithReloginToken(LoginResultCode.ComputerNameNotSet)

        Using con = GetConnection()
            Dim logonOptions = GetLogonOptions(con)
            If logonOptions.SingleSignon Then Return New LoginResultWithReloginToken(LoginResultCode.TypeMismatch)
            If GetLoggedIn() Then Return New LoginResultWithReloginToken(LoginResultCode.Already)
            Try

                Dim authenticationServerLoginResult = LoginAttemptWithAuthenticationServerToken(con, accessToken, logonOptions.AuthenticationServerUrl)
                Dim result = authenticationServerLoginResult.Result
                HandleLoginResultForAuthenticationServer(authenticationServerLoginResult, machine, locale, con)

                If result.IsSuccess AndAlso reloginTokenRequest IsNot Nothing AndAlso result.User.AuthType <> AuthMode.AuthenticationServerServiceAccount Then
                    Dim reloginToken = GenerateReloginTokenForUser(con, GetLoggedInUserId(), machine, reloginTokenRequest.ProcessId)
                    Return New LoginResultWithReloginToken(result, reloginToken)
                Else
                    Return New LoginResultWithReloginToken(result, Nothing)
                End If

            Catch ex As Exception
                Throw New UnknownLoginException(ex)
            End Try
        End Using
    End Function

    <UnsecuredMethod()>
    Public Function LoginWithReloginToken(locale As String,
                                          reloginTokenRequest As ReloginTokenRequest) _
                                          As LoginResultWithReloginToken Implements IServer.LoginWithReloginToken
        Using connection = GetConnection()
            Dim result = LoginWithReloginToken(connection, reloginTokenRequest)
            HandleLoginResultForReloginToken(result.LoginResult, AuthMode.AuthenticationServer, reloginTokenRequest.MachineName, locale, connection)
            Return result
        End Using
    End Function

    ''' <summary>
    ''' Validate user credentials. Used only by the listener for validating
    ''' credentials passed across the network.
    ''' </summary>
    ''' <param name="username">The user name</param>
    ''' <param name="password">The password</param>
    ''' <returns>The validated user object, or Nothing if the credentials
    ''' were not valid.</returns>
    <UnsecuredMethod()>
    Public Function ValidateCredentials(username As String, password As SafeString, authenticationMode As AuthMode) As IUser Implements IServer.ValidateCredentials
        If authenticationMode = AuthMode.Unspecified Then authenticationMode = AuthMode.Native

        Using connection = GetConnection()
            If IsSingleSignOn(connection) Then
                Try
                    Return ValidateSSOCredential(connection, username, password)
                Catch
                    Return Nothing
                End Try
            End If

            Try
                If authenticationMode = AuthMode.Native Then
                    Dim result = LoginAttempt(connection, username, password)
                    If result.IsSuccess Then
                        Return GetUser(connection, result.UserID, Nothing)
                    Else
                        Return Nothing
                    End If
                ElseIf authenticationMode = AuthMode.MappedActiveDirectory Then
                    Return ValidateMappedActiveDirectoryUserCredential(connection, username, password)
                Else
                    Return Nothing
                End If

            Catch ex As Exception
                Throw New UnknownLoginException(ex)
            End Try

        End Using
    End Function


    ''' <summary>
    ''' Gets the username for a given user id, or an empty string if the user could
    ''' not be found or is the Scheduler user.
    ''' </summary>
    ''' <param name="userId">the user id</param>
    ''' <returns>The user name or a blank string if the user could not be found or
    ''' the function failed for any other reason</returns>
    <UnsecuredMethod()>
    Public Function GetUserName(ByVal userId As Guid) As String Implements IServer.GetUserName
        Using con = GetConnection()
            Return GetUserName(con, userId)
        End Using
    End Function

    ''' <summary>
    ''' Try to get the ID of a user, given their name.
    ''' </summary>
    ''' <param name="userName">The name of the user</param>
    ''' <returns>The users ID (GUID) or Guid.Empty if the user does
    ''' not exist or the operation failed for any other reason.
    ''' </returns>
    <UnsecuredMethod()>
    Public Function TryGetUserID(userName As String) As Guid Implements IServer.TryGetUserID
        Using con = GetConnection()
            Return TryGetUserID(con, userName)
        End Using
    End Function

    ''' <summary>
    ''' Try to get the user attributes from BPAUser table
    ''' </summary>
    ''' <param name="userId">The (GUID) id of user </param>
    ''' <returns>AuthMode</returns>
    <UnsecuredMethod()>
    Public Function TryGetUserAttrib(userId As Guid) As AuthMode Implements IServer.TryGetUserAttrib
        Using con = GetConnection()
            Return TryGetUserAttrib(con, userId)
        End Using
    End Function

    ''' <summary>
    ''' Gets the system roles applicable to the configuration of the system and adds them to
    ''' a roleset object, ignoring all roles which require features that are currently
    ''' disabled.
    ''' </summary>
    ''' <returns>A roleset containing the roles defined in the current environment.
    ''' </returns>
    <UnsecuredMethod()>
    Public Function GetRoles() As RoleSet Implements IServer.GetRoles
        Using con = GetConnection()
            Return GetRoles(con)
        End Using
    End Function

    ''' <summary>
    ''' Log out.
    ''' </summary>
    <UnsecuredMethod()>
    Public Sub Logout() Implements IServer.Logout
        If Not GetLoggedIn() Then Return
        Using con = GetConnection()
            RecordLogoutAuditEvent(con, GetLoggedInUserName(), mLoggedInMachine)
            mLoggedInUser = Nothing
            mLoggedInMachine = Nothing
            mLoggedInMode = AuthMode.Unspecified
        End Using
    End Sub

    ''' <summary>
    ''' Log in as an Anonymous Resource.
    ''' </summary>
    ''' <returns>A LoginResult describing the result</returns>
    <UnsecuredMethod()>
    Public Function LoginAsAnonResource(machineName As String) _
    As LoginResult Implements IServer.LoginAsAnonResource
        Try
            If (String.IsNullOrEmpty(machineName)) Then Return _
                New LoginResult(LoginResultCode.ComputerNameNotSet)

            Using con = GetConnection()

                If Not GetSystemPref(con, "allow.anon.resource", True) Then _
                    Return New LoginResult(LoginResultCode.AnonymousDisabled)

                Const anonName As String = "Anonymous Resource"
                Dim id = GetSystemUserId(con, anonName)

                mLoggedInMode = AuthMode.Anonymous
                mLoggedInMachine = machineName

                Dim result As New LoginResult(LoginResultCode.Success,
                                              GetUser(con, id, anonName))
                mLoggedInUser = result.User

                Return result
            End Using
        Catch ex As Exception
            Throw New UnknownLoginException(ex)
        End Try

    End Function

    ''' <summary>
    ''' Used to allow the system user to login.
    ''' </summary>
    ''' <param name="systemUser">The system username</param>
    Friend Function LoginAsSystem(systemUser As String) As IUser
        Using con = GetConnection()
            Dim id = GetSystemUserId(con, systemUser)
            mLoggedInMode = AuthMode.System
            mLoggedInMachine = Environment.MachineName
            mLoggedInUser = GetUser(con, id, systemUser)
            Return mLoggedInUser
        End Using
    End Function

#End Region

#Region " Private Methods "
    Private Sub HandleLoginResult(result As LoginResult,
                                  authMode As AuthMode,
                                  machine As String,
                                  locale As String,
                                  con As IDatabaseConnection)
        If result.IsSuccess Then
            SetLoggedInSessionDetails(result.User, machine, authMode, locale)
            RecordLoginAuditEvent(con, GetLoggedInUserName(), machine, authMode)
        Else
            mLoggedInUser = Nothing
        End If
    End Sub

    Private Sub HandleLoginResultForReloginToken(result As LoginResult,
                                  authMode As AuthMode,
                                  machine As String,
                                  locale As String,
                                  con As IDatabaseConnection)
        If result.IsSuccess Then
            SetLoggedInSessionDetails(result.User, machine, authMode, locale)
            RecordLoginWithReloginTokenAuditEvent(con, GetLoggedInUserName(), machine, authMode)
        Else
            mLoggedInUser = Nothing
        End If
    End Sub

    Private Sub HandleLoginResultForAuthenticationServer(result As AuthenticationServerLoginResult,
                                  machine As String,
                                  locale As String,
                                  con As IDatabaseConnection)

        If result.Result.IsSuccess Then
            SetLoggedInSessionDetails(result.Result.User, machine, result.Result.User.AuthType, locale)
            RecordLoginViaAuthenticationServerAuditEvent(con, GetLoggedInUserName(), machine, result.Issuer, result.AuthenticationTime)
        Else
            mLoggedInUser = Nothing
        End If
    End Sub

    Private Sub SetLoggedInSessionDetails(user As IUser, machine As String, loggedInMode As AuthMode, locale As String)
        mLoggedInUser = user
        mLoggedInMachine = machine
        mLoggedInMode = loggedInMode
        mLoggedInUserLocale = locale
    End Sub

    ''' <summary>
    ''' Gets the password rules
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>A PasswordRules class representing the password rules</returns>
    Private Function GetPasswordRules(con As IDatabaseConnection) As PasswordRules
        Using cmd As New SqlCommand("select " &
                                    "pwd.*, " &
                                    "cfg.maxloginattempts, " &
                                    "cfg.passwordexpirywarninginterval " &
                                    "from BPAPasswordRules pwd, BPASysConfig cfg")
            Using dr = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(dr)
                dr.Read()
                Dim attempts = prov.GetInt("maxloginattempts")
                Return New PasswordRules() With {
                    .UseUpperCase = prov.GetValue("uppercase", False),
                    .UseLowerCase = prov.GetValue("lowercase", False),
                    .UseDigits = prov.GetValue("digits", False),
                    .UseSpecialCharacters = prov.GetValue("special", False),
                    .UseBrackets = prov.GetValue("brackets", False),
                    .PasswordLength = prov.GetInt("length"),
                    .AdditionalCharacters = prov.GetString("additional"),
                    .noRepeats = prov.GetValue("norepeats", False),
                    .noRepeatsDays = prov.GetValue("norepeatsdays", False),
                    .numberOfRepeats = prov.GetInt("numberofrepeats"),
                    .numberOfDays = prov.GetInt("numberofdays"),
                    .MaxAttempts = If(attempts > 0, New Integer?(attempts), Nothing),
                    .PasswordExpiryInterval = prov.GetInt("passwordexpirywarninginterval")}
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Updates the password Rules
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="details">The PasswordRules class representing the password
    ''' rules to update</param>
    Private Sub UpdatePasswordRules(con As IDatabaseConnection, details As PasswordRules)
        If details.PasswordExpiryInterval < 0 OrElse details.PasswordExpiryInterval > 255 Then _
          Throw New ArgumentOutOfRangeException("days",
              details.PasswordExpiryInterval,
              My.Resources.clsServer_TheNumberOfDaysMustBeBetween0And255)

        Using cmd As New SqlCommand(
            "update BPAPasswordRules set " &
            "uppercase=@uppercase," &
            "lowercase=@lowercase," &
            "digits=@digits," &
            "special=@special," &
            "brackets=@brackets," &
            "length=@length," &
            "additional=@additional," &
            "norepeats=@norepeats," &
            "norepeatsdays=@norepeatsdays," &
            "numberofrepeats=@numberofrepeats," &
            "numberofdays=@numberofdays")
            With cmd.Parameters
                .AddWithValue("@uppercase", details.UseUpperCase)
                .AddWithValue("@lowercase", details.UseLowerCase)
                .AddWithValue("@digits", details.UseDigits)
                .AddWithValue("@special", details.UseSpecialCharacters)
                .AddWithValue("@brackets", details.UseBrackets)
                .AddWithValue("@length", details.PasswordLength)
                .AddWithValue("@additional", details.AdditionalCharacters)
                .AddWithValue("@norepeats", details.noRepeats)
                .AddWithValue("@norepeatsdays", details.noRepeatsDays)
                .AddWithValue("@numberofrepeats", details.numberOfRepeats)
                .AddWithValue("@numberofdays", details.numberOfDays)
            End With
            con.Execute(cmd)
        End Using
        Using cmd As New SqlCommand(
            "update BPASysConfig set " &
            "maxloginattempts=@Attempts, " &
            "passwordexpirywarninginterval=@Expiry")
            With cmd.Parameters
                .AddWithValue("@Attempts", IIf(details.MaxAttempts.HasValue, details.MaxAttempts, DBNull.Value))
                .AddWithValue("@Expiry", details.PasswordExpiryInterval)
            End With
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Gets the log on options e.g whether single sign-on is enabled, how the
    ''' username should be populated etc.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>A LogonOptions class for the current environment.</returns>
    Private Function GetLogonOptions(con As IDatabaseConnection) As LogonOptions
        Dim cmd As New SqlCommand("select " &
                                  "activedirectoryprovider, " &
                                  "populateusernameusing, " &
                                  "showusernamesonlogin, " &
                                  "authenticationgatewayurl, " &
                                  "enablemappedactivedirectoryauth," &
                                  "enableexternalauth, " &
                                  "authenticationserverurl, " &
                                  "enableauthenticationserverauth, " &
                                  "authenticationserverapicredential " &
                                  "from BPASysConfig")

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            reader.Read()
            Return New LogonOptions() With {
                .AutoPopulate = prov.GetValue("populateusernameusing", AutoPopulateMode.None),
                .ShowUserList = prov.GetValue("showusernamesonlogin", False),
                .SingleSignon = Not String.IsNullOrEmpty(prov.GetString("activedirectoryprovider")),
                .AuthenticationGatewayUrl = prov.GetString("authenticationgatewayurl"),
                .ExternalAuthenticationEnabled = prov.GetValue("enableexternalauth", False),
                .MappedActiveDirectoryAuthenticationEnabled = prov.GetValue("enablemappedactivedirectoryauth", False),
                .AuthenticationServerUrl = prov.GetString("authenticationserverurl"),
                .AuthenticationServerAuthenticationEnabled = prov.GetBool("enableauthenticationserverauth"),
                .AuthenticationServerApiCredentialId = prov.GetValue(Of Guid?)("authenticationserverapicredential", Nothing)}
        End Using
    End Function

    ''' <summary>
    ''' Updates the log on options.
    ''' <param name="con">The database connection</param>
    ''' <param name="options">The logon options to update</param>
    ''' </summary>
    Private Sub SetLogonOptions(con As IDatabaseConnection, options As LogonOptions)

        ValidateAuthServerAuthSettings(con, options.AuthenticationServerAuthenticationEnabled, options.AuthenticationServerUrl)
        ValidateMappedActiveDirectoryAuthSettings(con, options.MappedActiveDirectoryAuthenticationEnabled)

        Dim cmd As New SqlCommand("update BPASysconfig set " &
                                  "populateusernameusing=@Mode, " &
                                  "showusernamesonlogin=@ShowUsers, " &
                                  "enablemappedactivedirectoryauth=@EnableMappedActiveDirectoryAuth, " &
                                  "enableexternalauth=@EnableExternalAuth, " &
                                  "authenticationgatewayurl=@AuthenticationGatewayUrl, " &
                                  "authenticationserverurl=@AuthenticationServerUrl, " &
                                  "enableauthenticationserverauth=@EnableAuthenticationServerAuthentication, " &
                                  "authenticationserverapicredential=@AuthenticationServerApiCredentialId")

        With cmd.Parameters
            .AddWithValue("@Mode", CInt(options.AutoPopulate))
            .AddWithValue("@ShowUsers", options.ShowUserList)
            .AddWithValue("@EnableMappedActiveDirectoryAuth", options.MappedActiveDirectoryAuthenticationEnabled)
            .AddWithValue("@EnableExternalAuth", options.ExternalAuthenticationEnabled)
            .AddWithValue("@EnableAuthenticationServerAuthentication", options.AuthenticationServerAuthenticationEnabled)
            If options.AuthenticationServerApiCredentialId.HasValue AndAlso options.AuthenticationServerApiCredentialId = Guid.Empty Then
                .AddWithValue("@AuthenticationServerApiCredentialId", DBNull.Value)
            Else
                .AddWithValue("@AuthenticationServerApiCredentialId", options.AuthenticationServerApiCredentialId.GetValueOrDefault)
            End If
            Dim trimmedAuthGatewayUrl = options.AuthenticationGatewayUrl?.TrimEnd(CChar("/"))
            Dim trimmedAuthServerUrl = options.AuthenticationServerUrl?.TrimEnd(CChar("/"))
            .AddWithValue("@AuthenticationGatewayUrl", IIf(String.IsNullOrEmpty(trimmedAuthGatewayUrl), DBNull.Value, trimmedAuthGatewayUrl))
            .AddWithValue("@AuthenticationServerUrl", IIf(String.IsNullOrEmpty(trimmedAuthServerUrl), DBNull.Value, trimmedAuthServerUrl))
        End With
        con.Execute(cmd)
    End Sub

    Private Function GetAuthenticationServerCredentialNameFromId(con As IDatabaseConnection, options As LogonOptions) As String
        Dim credentialId = options.AuthenticationServerApiCredentialId
        If credentialId Is Nothing OrElse credentialId = Guid.Empty Then Return String.Empty

        Dim credentialName = String.Empty
        Using cmd As New SqlCommand("select name from BPACredentials where id = @id")
            With cmd.Parameters
                .AddWithValue("@id", credentialId)
            End With
            credentialName = CType(con.ExecuteReturnScalar(cmd), String)
        End Using
        If String.IsNullOrEmpty(credentialName) Then
            Throw New AuditOperationFailedException(AuditOperationFailedException_CouldNotFindAuthenticationServerCredentialName)
        End If
        Return credentialName

    End Function

    Private Sub ValidateAuthServerAuthSettings(connection As IDatabaseConnection, enableAuthenticationServerAuthentication As Boolean, authServerUrl As String)
        If String.IsNullOrWhiteSpace(authServerUrl) Then
            If enableAuthenticationServerAuthentication Then Throw New BlankUrlException(AuthMode.AuthenticationServer.ToString())
        Else
            If Not authServerUrl.IsValidAndWellFormedAbsoluteUrl() Then Throw New InvalidUrlException(AuthMode.AuthenticationServer.ToString())
            If New Uri(authServerUrl).Scheme <> Uri.UriSchemeHttps Then Throw New InvalidProtocolException(AuthMode.AuthenticationServer.ToString())
        End If

        If Not enableAuthenticationServerAuthentication Then
            If DoesActiveUserExist(connection, AuthMode.AuthenticationServer) Then Throw New CannotDisableAuthTypeException(AuthMode.AuthenticationServer)
            If DoesActiveUserExist(connection, AuthMode.AuthenticationServerServiceAccount) Then Throw New CannotDisableAuthTypeException(AuthMode.AuthenticationServerServiceAccount)
        End If
    End Sub

    Private Sub ValidateMappedActiveDirectoryAuthSettings(connection As IDatabaseConnection, enableMappedActiveDirectoryAuth As Boolean)
        If Not enableMappedActiveDirectoryAuth AndAlso DoesActiveUserExist(connection, AuthMode.MappedActiveDirectory) Then _
            Throw New CannotDisableAuthTypeException(AuthMode.MappedActiveDirectory)
    End Sub

    Private Function DoesActiveUserExist(connection As IDatabaseConnection, authMode As AuthMode) As Boolean
        Dim command As New SqlCommand(
         " select count(userid)" &
         " from BPAUser" &
         " where isdeleted = 0 and authtype = @authtype"
        )
        With command.Parameters
            .AddWithValue("@authtype", authMode)
        End With

        Return CInt(connection.ExecuteReturnScalar(command)) > 0

    End Function

    ''' <summary>
    ''' Checks if this connection is a single-sign-on connection by testing whether
    ''' an active directory provider is set within the system config table
    ''' </summary>
    ''' <param name="con">The connection to test for single sign on over.</param>
    ''' <returns>True if this clsServer instance is connected to a single sign on
    ''' connection; False otherwise.</returns>
    ''' <exception cref="Exception">If any exceptions occur while attempting to
    ''' ascertain if this connection is single sign on.</exception>
    Private Function IsSingleSignOn(con As IDatabaseConnection) As Boolean
        Return (IfNull(GetActiveDirectoryDomain(con), "") <> "")
    End Function

    ''' <summary>
    ''' Gets the authentication domain to be used with single sign-on
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <returns>The name of the domain stored in the database</returns>
    Private Function GetActiveDirectoryDomain(con As IDatabaseConnection) As String
        Return CStr(con.ExecuteReturnScalar(New SqlCommand(
             "select ActiveDirectoryProvider from BPASysConfig")))
    End Function

    ''' <summary>
    ''' Gets a collection of AD groups are associated with at least one Blue Prism
    ''' user role.
    ''' </summary>
    ''' <returns>A collection containing the AD groups which are associated with user
    ''' roles on the database.</returns>
    Private Function GetActiveDirectoryGroups(con As IDatabaseConnection) As ICollection(Of String)

        Dim cmd As New SqlCommand("select ssogroup from BPAUserRole")

        Dim groups As New clsSet(Of String)
        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                If Not reader.IsDBNull(0) Then groups.Add(reader.GetString(0))
            End While
        End Using
        Return groups

    End Function

    ''' <summary>
    ''' Returns true if there are any null or blank ssogroup's
    ''' </summary>
    ''' <returns>True if any null or blank</returns>
    Private Function GetActiveDirectoryGroupsEmpty(con As IDatabaseConnection) As List(Of String)

        Dim rolesNotMapped = New List(Of String)

        Dim cmd As New SqlCommand("select name from BPAUserRole
                                          where ssogroup is null or ssogroup = ''")

        Using reader = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
            While reader.Read()
                rolesNotMapped.Add(reader.GetString(0))
            End While
        End Using

        Return rolesNotMapped
    End Function

    Private Sub CheckForDuplicateUser(connection As IDatabaseConnection, currentUser As IUser)
        CheckForDuplicateUser(connection, currentUser.Id, currentUser.Name)
    End Sub

    Private Sub CheckForDuplicateUser(connection As IDatabaseConnection, currentUserId As Guid, currentUserName As String)

        Dim cmd As New SqlCommand(
         " select cast(isdeleted as int)" &
         " from BPAUser" &
         " where username = @name and userid <> @id"
        )
        With cmd.Parameters
            .AddWithValue("@name", currentUserName)
            .AddWithValue("@id", currentUserId)
        End With

        Dim deletedFlag As Integer = IfNull(connection.ExecuteReturnScalar(cmd), -1)
        If deletedFlag = 0 Then
            Throw New NameAlreadyExistsException(
             My.Resources.clsServer_TheName0HasAlreadyBeenUsedPleaseSelectAnother, currentUserName)
        ElseIf deletedFlag = 1 Then
            Throw New NameAlreadyExistsException(
             My.Resources.clsServer_TheName0HasAlreadyBeenUsedByANowDeletedUserItCannotBeReUsedPleaseSelectAnother, currentUserName)
        End If

    End Sub

    Private Sub CheckForDuplicateExternalIdentity(con As IDatabaseConnection, user As User)
        Dim collationDependantMatches = New List(Of String)
        Dim cmd As New SqlCommand(
         " select uei.externalid" &
         " from BPAExternalProvider ep " &
         " inner join BPAUserExternalIdentity uei ON ep.id = uei.externalproviderid" &
         " where uei.externalid = @externalid AND ep.name = @idprovidername and bpuserid<> @bpuserid"
        )

        With cmd.Parameters
            .AddWithValue("@externalid", user.ExternalId)
            .AddWithValue("@idprovidername", user.IdentityProviderName)
            .AddWithValue("@bpuserid", user.Id)
        End With

        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                collationDependantMatches.Add(reader.GetString(0))
            End While
        End Using

        If collationDependantMatches.Count = 0 Then Return

        If collationDependantMatches.Contains(user.ExternalId, StringComparer.Ordinal) Then
            Throw New ExternalIdentityAlreadyInUseException(
                My.Resources.clsServer_TheExternalIdentity0HasAlreadyBeenUsedForIdProvider1,
                user.ExternalId, user.IdentityProviderName)
        End If

    End Sub

    Private Sub CheckForUserAlreadyMappedToAuthenticationServerUserId(con As IDatabaseConnection, user As IUser, authenticationServerUserId As Guid?)
        Dim cmd As New SqlCommand(
         " if exists(select u.userid" &
         " from BPAUser u " &
         " where u.authenticationServerUserId = @authenticationServerUserId and userid <> @bpuserid)" &
         " select cast(1 as bit) else select cast(0 as bit)"
        )

        With cmd.Parameters
            .AddWithValue("@authenticationServerUserId", authenticationServerUserId)
            .AddWithValue("@bpuserid", user.Id)
        End With

        Dim hasMatch = con.ExecuteReturnScalar(Of Boolean)(cmd)

        If hasMatch Then
            Throw New AuthenticationServerUserIdAlreadyInUseException()
        End If

    End Sub

    Private Sub CheckForUserAlreadyMappedToAuthenticationServerClientId(con As IDatabaseConnection, user As IUser, authenticationServerClientId As String)
        Dim cmd As New SqlCommand(
         " if exists(select u.userid" &
         " from BPAUser u " &
         " where u.authenticationServerClientId = @authenticationServerClientId and userid <> @bpuserid)" &
         " select cast(1 as bit) else select cast(0 as bit)"
        )

        With cmd.Parameters
            .AddWithValue("@authenticationServerClientId", authenticationServerClientId)
            .AddWithValue("@bpuserid", user.Id)
        End With

        Dim hasMatch = con.ExecuteReturnScalar(Of Boolean)(cmd)

        If hasMatch Then
            Throw New AuthenticationServerClientIdAlreadyInUseException()
        End If

    End Sub

    Private Sub CheckForDuplicateMappedActiveDirectoryUser(con As IDatabaseConnection, user As IUser)
        Dim cmd As New SqlCommand(
         " select count(bpuserid) from BPAMappedActiveDirectoryUser where sid = @sid And bpuserid <> @userid "
        )

        With cmd.Parameters
            .AddWithValue("@sid", user.ExternalId)
            .AddWithValue("@userid", user.Id)
        End With

        Dim existingUsers = CInt(con.ExecuteReturnScalar(cmd))

        If existingUsers <> 0 Then
            Throw New ActiveDirectoryMappingAlreadyInUseException(My.Resources.clsServer_TheSID0HasAlreadyBeenMappedToAnotherUser, user.ExternalId)
        End If
    End Sub

    Private Function GetAllExternalUserIdentities(con As IDatabaseConnection) As IList(Of String)
        Dim externalIds As IList(Of String) = New List(Of String)

        Dim cmd = New SqlCommand(
            "select externalid from BPAUserExternalIdentity"
            )

        Using reader = CType(con.ExecuteReturnDataReader(cmd), SqlDataReader)
            While reader.Read()
                externalIds.Add(CStr(reader("externalid")))
            End While
        End Using

        Return externalIds
    End Function

    Private Function CreateNewAuthenticationServerUserWithUniqueName(connection As IDatabaseConnection, username As String, authServerId As Guid) As String
        connection.BeginTransaction()
        Try
            Dim userId = mTryGetUserId(connection, username)
            Dim newUserName = If(userId = Guid.Empty, username, mUniqueUserNameGenerator.GenerateUsername(username))
            Dim user = New User(Guid.NewGuid(), newUserName, authServerId, username)
            mCreateNewUser(connection, user)
            connection.CommitTransaction()
            Return user.Name
        Catch nameAlreadyExistsException As NameAlreadyExistsException
            connection.RollbackTransaction()
            Return CreateNewAuthenticationServerUserWithUniqueName(connection, username, authServerId)
        Catch ex As Exception
            connection.RollbackTransaction()
            Throw
        End Try

    End Function

    Private Function CreateNewServiceAccount(connection As IDatabaseConnection, clientName As String, clientId As String, hasBluePrismApiScope As Boolean) As String
        connection.BeginTransaction()

        Try
            Dim userId = mTryGetUserId(connection, clientName)
            Dim newClientName = If(userId = Guid.Empty, clientName, mUniqueUserNameGenerator.GenerateUsername(clientName))
            Dim user = New User(Guid.NewGuid(), newClientName, clientId, hasBluePrismApiScope, clientName)

            mCreateNewUser(connection, user)
            connection.CommitTransaction()
            Return user.Name
        Catch nameAlreadyExistsException As NameAlreadyExistsException
            connection.RollbackTransaction()
            Return CreateNewServiceAccount(connection, clientName, clientId, hasBluePrismApiScope)
        Catch ex As Exception
            connection.RollbackTransaction()
            Throw
        End Try

    End Function

    Private Sub UpdateServiceAccount(connection As IDatabaseConnection, clientId As String, clientName As String, hasBluePrismApiScope As Boolean, synchronizationDate As DateTimeOffset)
        connection.BeginTransaction()

        Try
            Dim user = mGetUserByClientId(connection, clientId)

            If user.UpdatedLastSynchronizationDate > synchronizationDate Then
                Throw New SynchronizationOutOfSequenceException()
            End If

            If clientName <> user.AuthServerName Then
                Dim existingUserIdWithClientName = mTryGetUserId(connection, clientName)
                user.Name = If(existingUserIdWithClientName = Guid.Empty, clientName, mUniqueUserNameGenerator.GenerateUsername(clientName))
                user.AuthServerName = clientName
            End If

            user.HasBluePrismApiScope = hasBluePrismApiScope
            user.UpdatedLastSynchronizationDate = synchronizationDate

            mUpdateMappedUser(connection, user)
            connection.CommitTransaction()

        Catch nameAlreadyExistsException As NameAlreadyExistsException
            connection.RollbackTransaction()
            UpdateServiceAccount(connection, clientId, clientName, hasBluePrismApiScope, synchronizationDate)
        Catch ex As Exception
            connection.RollbackTransaction()
            Throw
        End Try

    End Sub

    Private Sub CreateNewUser(connection As IDatabaseConnection, user As User, password As SafeString)

        CheckForDuplicateUser(connection, user)

        Select Case user.AuthType
            Case AuthMode.External
                InsertNewExternalUser(connection, user)
            Case AuthMode.MappedActiveDirectory
                InsertNewMappedActiveDirectoryUser(connection, user)
            Case AuthMode.AuthenticationServer
                InsertNewAuthenticationServerUser(connection, user)
            Case AuthMode.AuthenticationServerServiceAccount
                InsertNewAuthenticationServerServiceAccount(connection, user)
            Case Else
                InsertNewNativeOrSsoUser(connection, user, password)
        End Select

        AssignRoles(connection, user.Id, user.Roles)

        AuditRecordUserEvent(connection, UserEventCode.CreateUser, user.Id, Nothing, Nothing, Nothing, Nothing)
        If user.AuthenticationServerUserId.HasValue Then
            AuditRecordUserMappedToAuthenticationServerUserEvent(connection, user.Id, user.AuthenticationServerUserId.Value)
        End If

        If Not String.IsNullOrEmpty(user.AuthenticationServerClientId) Then
            AuditRecordUserMappedToAuthenticationServerServiceAccountEvent(connection, user.Id, user.AuthenticationServerClientId)
        End If

    End Sub

    Private Sub InsertNewNativeOrSsoUser(ByVal con As IDatabaseConnection, ByVal u As User,
                              ByVal password As SafeString)

        Dim cmd As New SqlCommand(
            " insert into BPAUser" &
            "   (userid," &
            "    username," &
            "    validfromdate," &
            "    validtodate," &
            "    PasswordDurationWeeks," &
            "    passwordexpirydate," &
            "    authtype)" &
            " values (@id,@name,@validfrom,@validto,@passwordDurationWeeks,@passwordexpiry,@authtype)"
            )
        With cmd.Parameters
            .AddWithValue("@id", u.Id)
            .AddWithValue("@name", u.Name)
            .AddWithValue("@validfrom", u.Created)
            .AddWithValue("@validto", u.Expiry)
            .AddWithValue("@passwordDurationWeeks", u.PasswordDurationWeeks)
            .AddWithValue("@passwordexpiry", u.PasswordExpiry)
            .AddWithValue("@authtype", u.AuthType)
        End With

        con.Execute(cmd)

        If password IsNot Nothing Then
            UpdatePassword(con, u, password)
        End If
    End Sub

    Private Sub InsertNewExternalUser(con As IDatabaseConnection, user As User)

        Dim cmd As New SqlCommand(
            " insert into BPAUser" &
            "   (userid," &
            "    username," &
            "    validfromdate," &
            "    authtype)" &
            " values (@id,@name,@validfrom,@authtype)"
            )
        With cmd.Parameters
            .AddWithValue("@id", user.Id)
            .AddWithValue("@name", user.Name)
            .AddWithValue("@validfrom", user.Created)
            .AddWithValue("@authtype", AuthMode.External)
        End With

        con.Execute(cmd)
    End Sub

    Private Sub InsertNewMappedActiveDirectoryUser(con As IDatabaseConnection, user As User)

        Dim cmd As New SqlCommand(
            " insert into BPAUser" &
            "   (userid," &
            "    username," &
            "    validfromdate," &
            "    authtype)" &
            " values (@id,@name,@validfrom,@authtype)"
            )
        With cmd.Parameters
            .AddWithValue("@id", user.Id)
            .AddWithValue("@name", user.Name)
            .AddWithValue("@validfrom", user.Created)
            .AddWithValue("@authtype", AuthMode.MappedActiveDirectory)
        End With

        con.Execute(cmd)
    End Sub

    Private Sub InsertNewAuthenticationServerUser(con As IDatabaseConnection, user As User)

        CheckForUserAlreadyMappedToAuthenticationServerUserId(con, user, user.AuthenticationServerUserId)

        Dim cmd As New SqlCommand(
           " insert into BPAUser" &
           "   (userid," &
           "    username," &
           "    validfromdate," &
           "    authtype," &
           "    authenticationServerUserId," &
           "    authServerName)" &
           " values (@id,@name,@validfrom,@authtype,@authenticationServerUserId,@authServerName)"
           )
        With cmd.Parameters
            .AddWithValue("@id", user.Id)
            .AddWithValue("@name", user.Name)
            .AddWithValue("@validfrom", user.Created)
            .AddWithValue("@authtype", AuthMode.AuthenticationServer)
            .AddWithValue("@authenticationServerUserId", user.AuthenticationServerUserId)
            .AddWithValue("@authServerName", user.AuthServerName)
        End With

        con.Execute(cmd)
    End Sub

    Private Sub InsertNewAuthenticationServerServiceAccount(con As IDatabaseConnection, user As User)

        CheckForUserAlreadyMappedToAuthenticationServerClientId(con, user, user.AuthenticationServerClientId)

        Dim cmd As New SqlCommand(
            " insert into BPAUser" &
            "   (userid," &
            "    username," &
            "    validfromdate," &
            "    authtype," &
            "    authenticationServerClientId," &
            "    hasBluePrismApiScope," &
            "    authServerName)" &
            " values (@id,@name,@validfrom,@authtype,@authenticationServerClientId,@hasBluePrismApiScope,@authServerName)"
            )
        With cmd.Parameters
            .AddWithValue("@id", user.Id)
            .AddWithValue("@name", user.Name)
            .AddWithValue("@validfrom", user.Created)
            .AddWithValue("@authtype", AuthMode.AuthenticationServerServiceAccount)
            .AddWithValue("@authenticationServerClientId", user.AuthenticationServerClientId)
            .AddWithValue("@hasBluePrismApiScope", user.HasBluePrismApiScope)
            .AddWithValue("@authServerName", user.AuthServerName)
        End With

        con.Execute(cmd)
    End Sub

    Private Function GetExternalProviderId(con As IDatabaseConnection, identityProviderName As String) As Integer?

        Dim cmd As New SqlCommand("select id from BPAExternalProvider where name = @identityProviderName")
        cmd.Parameters.AddWithValue("@identityProviderName", identityProviderName)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read Then Return CInt(reader.GetInt32(0))
            Return Nothing
        End Using
    End Function

    Private Function GetExternalProviderTypeId(con As IDatabaseConnection, identityProviderType As String) As Integer?
        Dim cmd As New SqlCommand("select id from BPAExternalProviderType where name = @identityProviderType")
        cmd.Parameters.AddWithValue("@identityProviderType", identityProviderType)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read Then Return CInt(reader.GetInt32(0))
            Return Nothing
        End Using
    End Function

    Private Function InsertNewExternalProviderType(con As IDatabaseConnection, identityProviderType As String) As Integer
        Using cmd As New SqlCommand("insert into BPAExternalProviderType(name) values (@identityProviderType);" &
                                    " select scope_identity();")
            cmd.Parameters.AddWithValue("@identityProviderType", identityProviderType)

            Return CInt(con.ExecuteReturnScalar(cmd))
        End Using
    End Function

    Private Function InsertNewExternalProviderName(con As IDatabaseConnection, identityProviderName As String, identityProviderTypeId As Integer) As Integer
        Using cmd As New SqlCommand("insert into BPAExternalProvider(name, externalprovidertypeid) values (@identityProviderName, @externalProviderTypeId);" &
                                    " select scope_identity();")
            cmd.Parameters.AddWithValue("@identityProviderName", identityProviderName)
            cmd.Parameters.AddWithValue("@externalProviderTypeId", identityProviderTypeId)
            Return CInt(con.ExecuteReturnScalar(cmd))
        End Using
    End Function

    Private Sub CreateActiveDirectoryUserMapping(ByVal con As IDatabaseConnection, ByVal user As User)

        CheckForDuplicateMappedActiveDirectoryUser(con, user)
        Dim cmd As New SqlCommand(
            " insert into BPAMappedActiveDirectoryUser (bpuserid, sid)" &
            " values (@bpuserid, @sid)"
            )
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
            .AddWithValue("@sid", user.ExternalId)
        End With
        con.Execute(cmd)

        AuditRecordActiveDirectoryUserEvent(
            UserEventCode.UserMappedToActiveDirectoryUser,
            con, user.Id, user.ExternalId, user.Name, Nothing)
    End Sub

    Private Sub MapUserToExternalIdentity(ByVal con As IDatabaseConnection, ByVal user As User)

        CheckForDuplicateExternalIdentity(con, user)
        Dim providerTypeId = GetExternalProviderTypeId(con, user.IdentityProviderType)
        If providerTypeId Is Nothing Then
            providerTypeId = InsertNewExternalProviderType(con, user.IdentityProviderType)
        End If

        Dim providerid = GetExternalProviderId(con, user.IdentityProviderName)
        If providerid Is Nothing Then
            providerid = InsertNewExternalProviderName(con, user.IdentityProviderName, providerTypeId.Value)
        End If

        Dim cmd As New SqlCommand(
         " insert into BPAUserExternalIdentity" &
         "   (bpuserid," &
         "    externalproviderid," &
         "    externalid)" &
         " values (@bpuserid,@externalproviderid,@externalid)"
        )
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
            .AddWithValue("@externalproviderid", providerTypeId)
            .AddWithValue("@externalid", user.ExternalId)
        End With
        con.Execute(cmd)

        Dim externalIdentityMapping =
                New ExternalIdentityMapping(user.ExternalId, user.IdentityProviderName, user.IdentityProviderType)

        AuditRecordUserEvent(
            con, UserEventCode.UserMappedToExternalIdentity,
            user.Id, Nothing, Nothing,
            externalIdentityMapping, Nothing)
    End Sub

    Private Sub DeleteExternalIdentityFromUser(ByVal con As IDatabaseConnection,
                                               ByVal user As IUser)
        Dim identityToDelete = GetExternalIdForUser(con, user.Id)
        Dim cmd As New SqlCommand(
            " delete from BPAUserExternalIdentity where bpuserid = @bpuserid")
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
        End With
        con.Execute(cmd)

        AuditRecordUserEvent(con, UserEventCode.DeleteExternalIdentity,
                             user.Id, Nothing, Nothing,
                             Nothing, identityToDelete)
    End Sub

    Private Function GetMappedActiveDirectorySID(ByVal con As IDatabaseConnection, ByVal userId As Guid) As String
        Dim cmd As New SqlCommand("select sid from BPAMappedActiveDirectoryUser where bpuserid = @userId")
        cmd.Parameters.AddWithValue("@userId", userId)
        Return IfNull(con.ExecuteReturnScalar(cmd), String.Empty)
    End Function

    Private Sub DeleteMappedActiveDirectoryIDFromUser(ByVal con As IDatabaseConnection,
                                              ByVal user As IUser)
        Dim identityToDelete = GetMappedActiveDirectorySID(con, user.Id)
        Dim cmd As New SqlCommand(
            " delete from BPAMappedActiveDirectoryUser where bpuserid = @bpuserid")
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
        End With
        con.Execute(cmd)

        AuditRecordActiveDirectoryUserEvent(
            UserEventCode.DeleteMappedActiveDirectoryUser,
            con, user.Id, identityToDelete.ToString(), user.Name, Nothing)
    End Sub

    ''' <summary>
    ''' Updates the basic data found on the given user.
    ''' </summary>
    ''' <param name="con">The connection over which to update the user data.</param>
    ''' <param name="user">The user to update</param>
    ''' <param name="password">The password to set on the user. Nothing indicates that
    ''' the password should not be changed.</param>
    ''' <exception cref="NameAlreadyExistsException">If the name of the user is
    ''' already in use in the database.</exception>
    ''' <exception cref="ArgumentNullException">If <paramref name="user"/> is null.
    ''' </exception>
    Private Sub UpdateUser(ByVal con As IDatabaseConnection, ByVal user As User,
                           ByVal password As SafeString)
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))

        CheckForDuplicateUser(con, user)

        ' Get the user as it currently stands so we can compare the values later
        ' and see what's changed for the audit record
        Dim old As User = GetUser(con, user.Id, Nothing)

        If password IsNot Nothing Then
            UpdatePassword(con, user, password)
            UpdatePasswordMaintainance(con, user)
        End If

        Dim cmd As New SqlCommand(
         " update BPAUser set " &
         "   username = @name," &
         "   validtodate = @expiry," &
         "   passwordexpirydate = @passwordexpiry," &
         "   passworddurationweeks = @passwordduration," &
         "   alerteventtypes=@alerteventtypes," &
         "   alertnotificationtypes=@alertnotificationtypes" &
         " where userid = @id")

        With cmd.Parameters
            .AddWithValue("@name", user.Name)
            .AddWithValue("@expiry", user.Expiry)
            .AddWithValue("@passwordexpiry", user.PasswordExpiry)
            .AddWithValue("@passwordduration", user.PasswordDurationWeeks)
            .AddWithValue("@alerteventtypes", user.SubscribedAlerts)
            .AddWithValue("@alertnotificationtypes", user.AlertNotifications)
            .AddWithValue("@id", user.Id)
        End With

        con.Execute(cmd)

        AssignRoles(con, user.Id, user.Roles)

        ' Figure out what's changed and write up the audit trail
        If old IsNot Nothing Then
            If user.Name <> old.Name Then AuditRecordUserEvent(
             UserEventCode.RenameUser, user.Id, Nothing, old.Name)

            If password IsNot Nothing Then AuditRecordUserEvent(
             UserEventCode.ResetPassword, user.Id, Nothing)

            If user.Expiry <> old.Expiry Then AuditRecordUserEvent(
             UserEventCode.UserExpiry, user.Id, String.Format(
             "{0:d} -> {1:d}", old.Expiry, user.Expiry))

            If user.PasswordExpiry <> old.PasswordExpiry Then AuditRecordUserEvent(
             UserEventCode.PasswordExpiry, user.Id, String.Format(
             "{0:d} -> {1:d}", old.PasswordExpiry, user.PasswordExpiry))

            If Not user.Roles.Equals(old.Roles) Then AuditRecordUserEvent(
             UserEventCode.ChangeRoles, user.Id, String.Format(
             "{0} -> {1}", old.Roles, user.Roles))

        End If

    End Sub

    Private Sub UpdateUserDetailsForMappedUser(ByVal con As IDatabaseConnection, ByVal user As User)

        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))

        CheckForDuplicateUser(con, user)

        Dim preSaveUser As User = GetUser(con, user.Id, Nothing)

        Dim cmd As New SqlCommand(
               " update BPAUser set " &
               " username = @name, " &
               " hasBluePrismApiScope = @hasBluePrismApiScope," &
               " updatedLastSynchronizationDate = @updatedLastSynchronizationDate," &
               " authServerName = @authServerName" &
               " where userid = @id")
        With cmd.Parameters
            .AddWithValue("@id", user.Id)
            .AddWithValue("@name", user.Name)
            .AddWithValue("@hasBluePrismApiScope", user.HasBluePrismApiScope)
            .AddWithValue("@updatedLastSynchronizationDate", IIf(user.UpdatedLastSynchronizationDate.HasValue, user.UpdatedLastSynchronizationDate, DBNull.Value))
            .AddWithValue("@authServerName", IIf(user.AuthServerName IsNot Nothing, user.AuthServerName, DBNull.Value))
        End With

        con.Execute(cmd)

        AssignRoles(con, user.Id, user.Roles)

        If preSaveUser IsNot Nothing Then
            If user.Name <> preSaveUser.Name Then
                AuditRecordUserEvent(con, UserEventCode.RenameUser, user.Id, Nothing,
                                         preSaveUser.Name, Nothing, Nothing)
            End If

            If Not user.Roles.Equals(preSaveUser.Roles) Then AuditRecordUserEvent(con,
                    UserEventCode.ChangeRoles, user.Id,
                    String.Format("{0} -> {1}", preSaveUser.Roles, user.Roles),
                    Nothing, Nothing, Nothing)

            If user.HasBluePrismApiScope <> preSaveUser.HasBluePrismApiScope Then
                AuditRecordUserHasScopeFlagChangedEvent(con, user.Id, user.HasBluePrismApiScope, preSaveUser.HasBluePrismApiScope)
            End If

        End If

    End Sub

    Private Sub UpdateExternalIdentityMapping(con As IDatabaseConnection, user As User)

        CheckForDuplicateExternalIdentity(con, user)

        Dim preSaveExternalIdentityMapping = GetExternalIdForUser(con, user.Id)
        Dim postSaveExternalIdentityMapping =
                New ExternalIdentityMapping(user.ExternalId, user.IdentityProviderName, user.IdentityProviderType)

        If postSaveExternalIdentityMapping.Equals(preSaveExternalIdentityMapping) Then Return

        Dim cmd As New SqlCommand(
            " update BPAUserExternalIdentity set " &
            " externalid = @externalid " &
            " where bpuserid = @bpuserid"
            )
        With cmd.Parameters
            .AddWithValue("@bpuserid", user.Id)
            .AddWithValue("@externalid", user.ExternalId)
        End With
        con.Execute(cmd)

        AuditRecordUserEvent(con,
                UserEventCode.ChangeExternalIdentity, user.Id, Nothing,
                Nothing, postSaveExternalIdentityMapping, preSaveExternalIdentityMapping)
    End Sub

    Private Sub UpdateUserAuthType(con As IDatabaseConnection, user As IUser, authType As AuthMode)
        Using cmd As New SqlCommand(
            " update BPAUser" &
            " set authtype = @AuthTypeID" &
            " where userid = @UserID")
            cmd.Parameters.AddWithValue("@AuthTypeID", authType)
            cmd.Parameters.AddWithValue("@UserID", user.Id)
            con.Execute(cmd)
        End Using

        AuditRecordUserEvent(con, UserEventCode.UserAuthTypeChanged, user.Id, Nothing, Nothing, Nothing, Nothing, authType)
    End Sub

    Private Sub UpdateAdUserAuthTypeToMapped(con As IDatabaseConnection, user As User)
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))

        Dim cmd As New SqlCommand(
            " update BPAUser set " &
            " authtype = @authtype " &
            " where userid = @id")
        With cmd.Parameters
            .AddWithValue("@authtype", AuthMode.MappedActiveDirectory)
            .AddWithValue("@id", user.Id)
        End With

        con.Execute(cmd)
    End Sub

    Private Sub SetAuthenticationServerUserValues(connection As IDatabaseConnection, user As IUser, authenticationServerUserId As Guid, userName As String, authenticationServerUserName As String)

        CheckForDuplicateUser(connection, user.Id, userName)
        CheckForUserAlreadyMappedToAuthenticationServerUserId(connection, user, authenticationServerUserId)

        Using cmd As New SqlCommand(
            " update BPAUser" &
            " set authenticationServerUserId = @authenticationServerUserId," &
            " authType = @authType," &
            " username = @username," &
            " authServerName = @authServerName" &
            " where userid = @userID")
            cmd.Parameters.AddWithValue("@authenticationServerUserId", authenticationServerUserId)
            cmd.Parameters.AddWithValue("@userID", user.Id)
            cmd.Parameters.AddWithValue("@authType", AuthMode.AuthenticationServer)
            cmd.Parameters.AddWithValue("@username", userName)
            cmd.Parameters.AddWithValue("@authServerName", authenticationServerUserName)
            connection.Execute(cmd)
        End Using

        AuditRecordUserMappedToAuthenticationServerUserEvent(connection, user.Id, authenticationServerUserId)
        AuditRecordUserEvent(connection, UserEventCode.UserAuthTypeChanged, user.Id, Nothing, Nothing, Nothing, Nothing, AuthMode.AuthenticationServer)
        If user.Name <> userName Then
            AuditRecordUserEvent(connection, UserEventCode.RenameUser, user.Id, Nothing, user.Name, Nothing, Nothing)
        End If
    End Sub

    ''' <summary>
    ''' Updates the users password. This does not verify the current password.
    ''' </summary>
    ''' <param name="con">The DB Connection to use to access the database</param>
    ''' <param name="user">The user to update</param>
    ''' <param name="password">The password to set on the user. Nothing indicates that
    ''' the password should not be changed.</param>
    Private Shared Sub UpdatePassword(con As IDatabaseConnection, user As IUser, password As SafeString)

        Using cmd As New SqlCommand(
            " update BPAPassword set lastuseddate = getutcdate() where lastuseddate is null and userid = @userid;" &
            " update BPAPassword set active = 0 where userid = @userid;" &
            " insert into BPAPassword (active, type, userid, salt, hash, lastuseddate) values (1, 1, @userid, @salt, @hash, null);")

            Dim hasher = New PBKDF2PasswordHasher()
            Dim salt As String = Nothing
            Dim hash = hasher.GenerateHash(password, salt)
            With cmd.Parameters
                .AddWithValue("@userid", user.Id)
                .AddWithValue("@hash", hash)
                .AddWithValue("@salt", salt)
            End With

            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Creates an entry in the database corresponding to the current user, if one
    ''' doesn't exist, or verifies the existence of one. A successful return from
    ''' this function indicates that the user is valid in the domain used for Blue
    ''' Prism authentication.
    ''' </summary>
    ''' <param name="con">The DB Connection to use to access the database</param>
    ''' <param name="upn">The user's principal name</param>
    ''' <param name="userID">On return, contains the ID of the user.</param>
    ''' <returns>True if a new user account was created.</returns>
    Private Function MirrorUser(con As IDatabaseConnection, ByVal upn As String,
                                ByRef userID As Guid) As Boolean
        Try
            userID = GetUserID(con, upn)

            'user already exists
            Return False

        Catch ex As NoSuchElementException

            'If the user doesn't exist create one
            userID = Guid.NewGuid()
            CreateNewUser(con, New User(AuthMode.ActiveDirectory, userID, upn), Nothing)

            Return True
        End Try
    End Function

    ''' <summary>
    ''' Removes all the users locks on any processes and sets the users isdeleted
    ''' value to 1
    ''' </summary>
    ''' <param name="userid">The id of the user to deactivate</param>
    ''' <param name="con">The database connection</param>
    ''' <returns>Whether the user record was updated</returns>
    Private Function DeactivateUser(con As IDatabaseConnection, userid As Guid) As Boolean
        Dim deletecmd As New SqlCommand(
            " delete from BPAProcessLock where userid = @userid;")
        deletecmd.Parameters.AddWithValue("@userid", userid)
        con.Execute(deletecmd)

        Dim cmd As New SqlCommand(
            " update BPAUser set isdeleted = 1 where userid = @userid" &
            " and isdeleted = 0")
        cmd.Parameters.AddWithValue("@userid", userid)
        Dim updated = con.ExecuteReturnRecordsAffected(cmd) > 0
        Return updated
    End Function

    ''' <summary>
    ''' Sets the users isdeleted value to 0
    ''' </summary>
    ''' <param name="con">The database connection to use</param>
    ''' <param name="userid">The id of the user to deactivate</param>
    ''' <returns>Whether the user record was updated</returns>
    Private Function ActivateUser(con As IDatabaseConnection, userid As Guid) As Boolean
        Dim cmd As New SqlCommand(
                    " update BPAUser set isdeleted = 0 where userid = @userid" &
                    " and isdeleted = 1")
        cmd.Parameters.AddWithValue("@userid", userid)
        Return (con.ExecuteReturnRecordsAffected(cmd) > 0)
    End Function

    ''' <summary>
    ''' Resets the number of login attempts for the specified user
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="userId">The ID of the user to reset</param>
    ''' <returns>True if the user had their login attempts reset; False if it was
    ''' already zero (or the user ID did not exist on the database)</returns>
    Private Shared Function ResetLoginAttempts(con As IDatabaseConnection, userID As Guid) As Boolean
        Dim cmd As New SqlCommand("update BPAUser set loginattempts = 0 " &
                                  "where userid = @id and loginattempts > 0")
        cmd.Parameters.AddWithValue("@id", userID)
        If con.ExecuteReturnRecordsAffected(cmd) = 0 Then Return False
        Return True
    End Function

    ''' <summary>
    ''' Updates the roles in the system to the given roleset.
    ''' After completing this method, any roles with temporary IDs in the given role
    ''' set will have their IDs set to the values assigned to them by the database.
    ''' </summary>
    ''' <param name="con">The connection over which to update the roles.</param>
    ''' <param name="rs">The set of roles to update the system roles to.</param>
    ''' <param name="useTransaction">If true, commit the changes to the roles.</param>
    Private Sub UpdateRoles(con As IDatabaseConnection, rs As RoleSet, useTransaction As Boolean)

        ' First, get the changes so we can record them in the audit record
        Dim currRoles As RoleSet = GetRoles(con)
        Dim changes As String = currRoles.GetChangeReport(rs)

        ' I want the getting the current roles to be outside the transaction so
        ' we aren't promoting a lock from shared to exclusive (deadlocks-a-go-go)
        If useTransaction Then con.BeginTransaction()
        Dim cmd As New SqlCommand()
        Dim report As New StringBuilder()

        ' First we get the IDs for all roles, so we can delete any which aren't there
        Dim roleIds As New List(Of Integer)
        For Each r As Role In rs : roleIds.Add(r.Id) : Next
        Dim deletedCount As Integer = UpdateMultipleIds(con, cmd, roleIds, Nothing,
         "delete from BPAUserRole where id not in (")

        If deletedCount > 0 Then _
         report.Append(String.Format(My.Resources.clsServer_Deleted0RoleS, deletedCount))

        ' We update any which do
        cmd.CommandText =
         " update BPAUserRole set" &
         "   name = @name," &
         "   ssogroup = @group" &
         " where id = @id"

        With cmd.Parameters
            .Add("@id", SqlDbType.Int)
            .Add("@name", SqlDbType.NVarChar, 255)
            .Add("@group", SqlDbType.NVarChar, 255)
        End With

        For Each r As Role In rs
            If r.HasTemporaryId Then Continue For
            With cmd.Parameters
                .Item("@id").Value = r.Id
                .Item("@name").Value = r.Name
                .Item("@group").Value = r.ActiveDirectoryGroup
            End With
            con.Execute(cmd)
        Next

        ' We now insert the new roles, saving the ID mapping for use in the future
        Dim idMap As New Dictionary(Of Integer, Integer)
        cmd.CommandText =
         " insert into BPAUserRole (name, ssogroup) values (@name, @group);" &
         " select scope_identity();"
        cmd.Parameters.Remove(cmd.Parameters("@id"))
        For Each r As Role In rs
            If Not r.HasTemporaryId Then Continue For
            With cmd.Parameters
                .Item("@name").Value = r.Name
                .Item("@group").Value = r.ActiveDirectoryGroup
            End With
            ' Update any new roles which may have been copied
            ' from this one during the same session
            Dim newID = CInt(con.ExecuteReturnScalar(cmd))
            For Each copy In rs.Where(Function(c) c.CopiedFromRoleID = r.Id)
                copy.CopiedFromRoleID = newID
            Next
            r.Id = newID
        Next

        ' We now re-add all of the assignments to the permissions for the roles
        Dim sb As New StringBuilder(255)
        cmd.Parameters.Clear()

        For Each r As Role In rs
            Dim deletedPerms As IEnumerable(Of Integer)
            Dim insertedPerms As IEnumerable(Of Integer)
            If currRoles.Contains(r.Id) Then
                deletedPerms = From p In currRoles(r.Id).Permissions
                               Where Not r.Permissions.Contains(p) Select p.Id
                insertedPerms = From p In r.Permissions
                                Where Not currRoles(r.Id).Permissions.Contains(p) Select p.Id
            Else
                deletedPerms = GetEmpty.IEnumerable(Of Integer)
                insertedPerms = From p In r.Permissions Select p.Id
            End If

            ' Delete any permissions no longer assigned to this role
            UpdateMultipleIDs(con, deletedPerms.ToList(),
                String.Format("delete from BPAUserRolePerm where userroleid={0} and permid in (", r.Id))

            For Each p In insertedPerms

                ' Before we add our select, we need to either set the initial
                ' statement or add a 'union all' to sit between our select statements
                If sb.Length = 0 _
                 Then sb.Append(" insert into BPAUserRolePerm (userroleid, permid)") _
                 Else sb.Append(" union all")

                ' I could jump through hoops to make this parameterized, but it
                ' seems pointless when we know they're integers anyway, and it can't
                ' be cached due to being entirely dynamic
                sb.AppendFormat(" select {0}, {1}", r.Id, p)
            Next
        Next

        ' If we copied any roles from another, also copy associated
        ' any group level permissions
        For Each r In rs.Where(Function(c) c.CopiedFromRoleID > 0)
            sb.AppendFormat("; insert into BPAGroupUserRolePerm (groupid, userroleid, permid)
                select groupid, {0}, permid from BPAGroupUserRolePerm where userroleid={1}",
                            r.Id, r.CopiedFromRoleID)
        Next

        cmd.CommandText = sb.ToString()
        If Not String.IsNullOrEmpty(cmd.CommandText) Then con.Execute(cmd)

        AuditRecordSysConfigEvent(con, SysConfEventCode.ModifyRoles, changes)
        ' If this is running on the server, we need to ensure that the roles are
        ' updated with the new values. This lazily resets it, meaning that the roles
        ' will be reloaded next time the system roles are requested (see bug 8778)
        If IsServer() Then SystemRoleSet.Reset()
        ' Also, we want to make sure any other systems know to update their cached
        ' role data (bug 9838)
        IncrementDataVersion(con, DataNames.Roles)

        If (useTransaction) Then con.CommitTransaction()
    End Sub

    ''' <summary>
    ''' Gets the system roles applicable to the configuration of the system and adds them to
    ''' a roleset object, ignoring all roles which require features that are currently
    ''' disabled.
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the roles</param>
    ''' <returns>A roleset containing the roles defined in the current environment.
    ''' </returns>
    Private Function GetRoles(con As IDatabaseConnection) As RoleSet
        Dim cmd As New SqlCommand()
        cmd.CommandText =
         " select r.id, r.name, r.ssogroup, r.requiredFeature from BPAUserRole r;" &
         " select r.name as rolename, p.name as permname" &
         "   from BPAUserRole r" &
         "     join BPAUserRolePerm rp on rp.userroleid = r.id" &
         "     join BPAPerm p on rp.permid = p.id"

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderMultiDataProvider(reader)
            Dim roleSet As New RoleSet()

            ' First load the role IDs/names
            roleSet.LoadRoles(prov)

            If Not prov.NextResult() Then Throw New OperationFailedException(
             My.Resources.clsServer_NoResultsForBPAUserRolePerm)

            ' Then assign all the perms to the roles
            roleSet.LoadAssignments(prov)

            Return roleSet

        End Using

    End Function

    ''' <summary>
    ''' Gets the user objects representing users who are assigned to a specified
    ''' role (excluding deleted users)
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the users</param>
    ''' <param name="roleId">The ID of the role for which the assigned users are
    ''' required.</param>
    ''' <returns>A collection of users <em>with no role information stored</em> which
    ''' represents the users assigned to the specified role.</returns>
    Private Function GetActiveUsersInRole(con As IDatabaseConnection, roleId As Integer) As ICollection(Of User)
        Dim cmd As New SqlCommand(
            " select a.userid " &
            " from BPAUserRoleAssignment a " &
            "   join BPAUser u on a.userid = u.userid" &
            " where a.userroleid = @roleid" &
            " and u.systemusername is null" &
            "   and u.isdeleted = 0" &
            "   and (u.authtype <> 8 or u.hasBluePrismApiScope = 1)"
        )
        cmd.Parameters.AddWithValue("@roleid", roleId)
        Dim ids As New HashSet(Of Guid)
        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                ids.Add(reader.GetGuid(0))
            End While
        End Using
        Return GetUsers(con, ids, False)
    End Function

    ''' <summary>
    ''' Counts the number of users who have a particular role assigned to them on
    ''' the database (excluding deleted users)
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="r">The role to count the number of users assigned with it.
    ''' </param>
    ''' <returns>The number of users found on the database with the specified role.
    ''' </returns>
    Private Function CountActiveUsersWithRole(con As IDatabaseConnection, r As Role) As Integer
        Using cmd As New SqlCommand(
            " select count(*) " &
            " from BPAUserRoleAssignment a " &
            "   join BPAUser u on a.userid = u.userid" &
            " where a.userroleid = @roleid" &
            "   and u.isdeleted = 0" &
            "   and (u.authtype <> 8 or u.hasBluePrismApiScope = 1)"
            )

            cmd.Parameters.AddWithValue("@roleid", r.Id)
            Return CInt(con.ExecuteReturnScalar(cmd))
        End Using
    End Function

    ''' <summary>
    ''' Gets the current Windows Identity
    ''' </summary>
    Private Function GetCurrentIdentity() As IWindowsIdentity
        Dim clientIdentity = GetClientIdentity()
        If clientIdentity IsNot Nothing Then Return clientIdentity

        ' Will reach this for direct database connections, or any client server connection mode
        ' where Windows Auth is not used to validate the client's identity.
        Return New WindowsIdentityWrapper(WindowsIdentity.GetCurrent())
    End Function

    Private Function GetClientIdentity() As IWindowsIdentity
        Dim callerId = Thread.CurrentPrincipal.Identity
        If String.IsNullOrEmpty(callerId.Name) Then Return Nothing
        Dim windowsIdentity = CType(callerId, WindowsIdentity)
        Return New WindowsIdentityWrapper(windowsIdentity)
    End Function

    Private mGetClientIdentity As Func(Of IWindowsIdentity) = Function() GetClientIdentity()

    Private Function AttemptLogin(con As IDatabaseConnection,
                                         username As String,
                                         password As SafeString) As User

        Dim user = GetActiveUser(con, username)

        If user.AuthType <> AuthMode.Native Then Throw New IncorrectLoginException()

        If user.IsLocked Then Throw New LimitReachedException(
            My.Resources.clsServer_TheMaximumNumberOfLoginAttemptsHasBeenExceeded)

        Dim hashType As HashTypes
        If Not VerifyPassword(con, password, user, hashType) Then _
            IncrementLoginAttempts(con, user.Id) : Throw New IncorrectLoginException()

        If hashType = HashTypes.Legacy Then
            UpdatePassword(con, user, password)
            UpdatePasswordMaintainance(con, user)
        End If

        ResetLoginAttempts(con, user.Id)

        If user.Expired Then Throw New ExpiredException(
         My.Resources.clsServer_ThisAccountHasExpiredPleaseContactYourSystemAdministrator)

        PopulateRolesIntoUser(con, user)

        Return user

    End Function

    Private Function AttemptLoginWithAuthenticationServerToken(con As IDatabaseConnection,
                                                               accessToken As String,
                                                               authenticationSeverUrl As String) _
                                                           As (User As User,
                                                               Issuer As String,
                                                               AuthenticationTime As DateTimeOffset?)

        Dim claims = mAccessTokenValidator.Validate(accessToken, authenticationSeverUrl)
        Dim user As User
        Dim issuer = mAccessTokenClaimsParser.GetIssuer(claims)
        Dim authenticationTime = mAccessTokenClaimsParser.GetAuthenticationTime(claims)
        Dim id = mAccessTokenClaimsParser.GetId(claims)


        If id.HasValue Then
            user = GetActiveAuthServerUser(con, id.Value)
        Else
            Dim clientId = mAccessTokenClaimsParser.GetClientId(claims)
            user = mGetActiveAuthServerServiceAccountUser(con, clientId)
        End If

        PopulateRolesIntoUser(con, user)

        Return (user, issuer, authenticationTime)
    End Function

    Private Function LoginWithReloginToken(con As IDatabaseConnection,
                                           reloginTokenRequest As ReloginTokenRequest) _
                                        As LoginResultWithReloginToken
        Dim reloginTokenUserId = mValidateReloginToken(con, reloginTokenRequest)
        If reloginTokenUserId = Guid.Empty Then
            Return New LoginResultWithReloginToken(LoginResultCode.InvalidReloginToken)
        End If

        Dim machineName = reloginTokenRequest.MachineName
        Dim processId = reloginTokenRequest.ProcessId

        Dim user = GetActiveUser(con, reloginTokenUserId)
        If user.AuthType <> AuthMode.AuthenticationServer Then
            Return New LoginResultWithReloginToken(LoginResultCode.TypeMismatch)
        End If

        Dim newReloginToken = GenerateReloginTokenForUser(con, reloginTokenUserId, machineName, processId)
        PopulateRolesIntoUser(con, user)

        Dim loginResult = New LoginResult(LoginResultCode.Success, user)
        Return New LoginResultWithReloginToken(loginResult, newReloginToken)

    End Function

    Private Function ValidateReloginToken(con As IDatabaseConnection, reloginTokenRequest As ReloginTokenRequest) As Guid
        Dim hashedToken As String
        Dim salt As String
        Dim userId As Guid
        Using cmd As New SqlCommand(
            " select bpuserid, token, salt from BPAUserExternalReloginToken
                                    where processid = @processid and
                                          tokenexpiry > getutcdate() and
                                          machinename = @machinename"
            )
            cmd.Parameters.AddWithValue("@machinename", reloginTokenRequest.MachineName)
            cmd.Parameters.AddWithValue("@processid", reloginTokenRequest.ProcessId)

            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)
                    hashedToken = prov.GetValue("token", String.Empty)
                    salt = prov.GetValue("salt", String.Empty)
                    userId = prov.GetValue("bpuserid", Guid.Empty)

                    Dim hasher = New PBKDF2PasswordHasher()
                    Dim hashVerified = hasher.VerifyPassword(reloginTokenRequest.PreviousReloginToken, hashedToken, salt)

                    If hashVerified Then Return userId
                End While

                Return Guid.Empty
            End Using
        End Using
    End Function

    Private Sub DeleteReloginToken(con As IDatabaseConnection, userId As Guid, machineName As String, processId As Integer)
        Using cmd As New SqlCommand("delete from BPAUserExternalReloginToken
                                    where machinename = @machinename and
                                          processid = @processid and
                                          bpuserid = @bpuserid")
            cmd.Parameters.AddWithValue("@machinename", machineName)
            cmd.Parameters.AddWithValue("@processid", processId)
            cmd.Parameters.AddWithValue("@bpuserid", userId)
            con.Execute(cmd)
        End Using
    End Sub

    Private Sub DeleteReloginTokensForUser(userId As Guid)
        Using connection = GetConnection()
            Using cmd As New SqlCommand("delete from BPAUserExternalReloginToken where bpuserid = @bpuserid")
                cmd.Parameters.AddWithValue("@bpuserid", userId)
                connection.Execute(cmd)
            End Using
        End Using
    End Sub

    Private Function GenerateReloginTokenForUser(con As IDatabaseConnection, userId As Guid, machineName As String, processId As Integer) As SafeString
        DeleteExpiredReloginTokens(con)

        con.BeginTransaction()
        DeleteReloginToken(con, userId, machineName, processId)

        Dim reloginToken = Guid.NewGuid()
        Dim safeReloginToken = New SafeString(reloginToken.ToString())
        Dim salt As String = Nothing
        Dim hasher = New PBKDF2PasswordHasher()
        Dim hash = hasher.GenerateHash(safeReloginToken, salt)

        Using cmd As New SqlCommand("insert into BPAUserExternalReloginToken (bpuserid, machinename, processid, token, salt, tokenexpiry)
        values (@bpuserid, @machinename, @processid, @token, @salt, dateadd(minute, @tokenLifetime, getutcdate()))")
            cmd.Parameters.AddWithValue("@bpuserid", userId)
            cmd.Parameters.AddWithValue("@machinename", machineName)
            cmd.Parameters.AddWithValue("@processid", processId)
            cmd.Parameters.AddWithValue("@token", hash)
            cmd.Parameters.AddWithValue("@salt", salt)
            cmd.Parameters.AddWithValue("@tokenLifetime", ReloginTokenLifetime.Minutes)
            con.Execute(cmd)
        End Using
        con.CommitTransaction()
        Return safeReloginToken
    End Function

    Private Sub DeleteExpiredReloginTokens(con As IDatabaseConnection)
        Using cmd As New SqlCommand("delete from BPAUserExternalReloginToken where tokenexpiry < getutcdate()")
            con.Execute(cmd)
        End Using
    End Sub


    ''' <summary>
    ''' Verifies that the password for a given user is correct, that is to say it
    ''' matches the hash for the user in the database
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="password">The password to verify</param>
    ''' <param name="user">The user to verify hashes against</param>
    ''' <param name="hashType">Passes back the type of hash used</param>
    Private Shared Function VerifyPassword(con As IDatabaseConnection, password As SafeString,
                                    user As IUser, Optional ByRef hashType As HashTypes = HashTypes.Legacy) As Boolean

        If password.IsEmpty Then Return False

        Using cmd As New SqlCommand(
            " select type, salt, hash from BPAPassword where active = 1 and userid = @userid")

            With cmd.Parameters
                .AddWithValue("@userid", user.Id)
            End With
            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then Throw _
                    New InvalidModeException(My.Resources.clsServer_CouldNotFindActivePasswordForUser0, user.Name)

                Dim prov As New ReaderDataProvider(reader)

                hashType = prov.GetValue("type", HashTypes.Legacy)

                Return VerifyPassword(password, prov)
            End Using
        End Using

    End Function

    ''' <summary>
    ''' Maps Hashtypes to those stored in the DB
    ''' </summary>
    Private Enum HashTypes
        Legacy = 0
        PBKDF2 = 1
    End Enum

    ''' <summary>
    ''' Verifies that the password for a type, salt and hash retrieved from the given
    ''' ReaderDataProvider is correct.
    ''' </summary>
    ''' <param name="password">The password to verify</param>
    ''' <param name="prov">The data provider to get the type, salt and hash</param>
    Private Shared Function VerifyPassword(password As SafeString, prov As ReaderDataProvider) As Boolean

        Dim salt = prov.GetString("salt")
        Dim hash = prov.GetString("hash")
        Dim type = prov.GetValue("type", HashTypes.Legacy)

        Dim hasher As IPasswordHasher
        Select Case type
            Case HashTypes.Legacy
                hasher = New LegacyPasswordHasher
            Case HashTypes.PBKDF2
                hasher = New PBKDF2PasswordHasher
            Case Else
                Throw New InvalidModeException(My.Resources.clsServer_PasswordHashIsNotOfAValidType)
        End Select

        Return hasher.VerifyPassword(password, hash, salt)
    End Function

    ''' <summary>
    ''' Gets the user roles from the database for the given user and sets them into
    ''' the user object.
    ''' </summary>
    ''' <param name="con">The DB Connection to use to access the database</param>
    ''' <param name="user">The user object whose roles should be populated from the
    ''' values in the database. This should be populated with, at least, the user ID
    ''' for the required user.</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="user"/> is null.
    ''' </exception>
    ''' <exception cref="ArgumentException">If the <see cref="User.Id">ID</see> of
    ''' the given user object is empty.</exception>
    Private Shared Sub PopulateRolesIntoUser(ByVal con As IDatabaseConnection, ByVal user As User)
        If user Is Nothing Then Throw New ArgumentNullException(NameOf(user))
        If user.Id = Guid.Empty Then Throw New ArgumentException(
         My.Resources.clsServer_TheUserObjectToSetTheRolesInHasNoIDSet)

        Dim cmd As New SqlCommand(
         " select r.name" &
         " from BPAUser u " &
         "   join BPAUserRoleAssignment ura on u.userid = ura.userid" &
         "   join BPAUserRole r on ura.userroleid = r.id" &
         " where u.userid = @id")
        cmd.Parameters.AddWithValue("@id", user.Id)

        Using reader = con.ExecuteReturnDataReader(cmd)
            user.Roles.Clear()
            While reader.Read()
                user.Roles.Add(reader.GetString(0))
            End While
        End Using
    End Sub

    ''' <summary>
    ''' Check user credentials and increment login attempts on wrong password if
    ''' necessary. This does not set mLoggedIn and other user specific member
    ''' variables so can be used by ValidateCredentials as well.
    ''' </summary>
    ''' <param name="con">The database connection to use</param>
    ''' <param name="username">The username to attempt login with</param>
    ''' <param name="Password">The password to attempt login with</param>
    Private Function LoginAttempt(con As IDatabaseConnection, username As String,
      password As SafeString) As LoginResult
        Try
            Dim user = AttemptLogin(con, username, password)
            If user.PasswordExpired Then
                Return New LoginResult(LoginResultCode.PasswordExpired, user)
            Else
                Return New LoginResult(LoginResultCode.Success, user)
            End If

        Catch ile As IncorrectLoginException
            Return New LoginResult(LoginResultCode.BadCredentials)
        Catch uue As UserNotFoundException
            Return New LoginResult(LoginResultCode.BadCredentials)
        Catch de As DeletedException
            Return New LoginResult(LoginResultCode.Deleted)
        Catch lre As LimitReachedException
            Return New LoginResult(LoginResultCode.AttemptsExceeded)
        Catch ee As ExpiredException
            Return New LoginResult(LoginResultCode.AccountExpired)
        End Try
    End Function


    Private Function LoginAttemptWithAuthenticationServerToken(con As IDatabaseConnection, accessToken As String, authenticationServerUrl As String) As AuthenticationServerLoginResult
        Try
            Dim result = AttemptLoginWithAuthenticationServerToken(con, accessToken, authenticationServerUrl)
            Return AuthenticationServerLoginResult.Success(result.User, result.Issuer, result.AuthenticationTime)
        Catch tve As SecurityTokenValidationException
            Return AuthenticationServerLoginResult.Failed(LoginResultCode.InvalidAccessToken)
        Catch de As DeletedException
            Return AuthenticationServerLoginResult.Failed(LoginResultCode.Deleted)
        Catch nsee As NoSuchElementException
            Return AuthenticationServerLoginResult.Failed(LoginResultCode.UnableToFindUser)
        Catch saunf As ServiceAccountUserNotFoundException
            Return AuthenticationServerLoginResult.Failed(LoginResultCode.UnableToFindServiceAccountUser)
        Catch unf As UserNotFoundException
            Return AuthenticationServerLoginResult.Failed(LoginResultCode.UnableToFindUser)
        End Try
    End Function

    Private Function LoginWithMappedActiveDirectoryUser(con As IDatabaseConnection) As LoginResult
        Dim windowsIdentity = mGetClientIdentity()
        If windowsIdentity Is Nothing Then Return New LoginResult(LoginResultCode.UnableToValidateClientIdentity)
        Return LoginWithMappedActiveDirectoryUser(con, windowsIdentity)
    End Function

    Private Function LoginWithMappedActiveDirectoryUser(con As IDatabaseConnection, windowsIdentity As IWindowsIdentity) As LoginResult

        If windowsIdentity Is Nothing Then Throw New ArgumentNullException(NameOf(windowsIdentity))
        If Not windowsIdentity.IsAuthenticated Then Return New LoginResult(LoginResultCode.NotAuthenticated)

        Try
            Dim sid = windowsIdentity.Sid.ToString()
            Dim userId = GetMappedActiveDirectoryUserId(con, sid)
            If userId = Guid.Empty Then Return New LoginResult(LoginResultCode.NoMappedActiveDirectoryUser)

            Dim user = GetActiveUser(con, userId, False)
            PopulateRolesIntoUser(con, user)

            Return New LoginResult(LoginResultCode.Success, user)

        Catch ex As DeletedException
            Return New LoginResult(LoginResultCode.Deleted)
        End Try
    End Function

    Private Function GetAlreadyMappedActiveDirectoryUsers(activeDirectoryUsers As IEnumerable(Of ISearchResult)) As HashSet(Of String) Implements IServerPrivate.GetAlreadyMappedActiveDirectoryUsers
        Using connection = GetConnection()
            Using command = New SqlCommand("usp_getmappedadusers")

                command.CommandType = CommandType.StoredProcedure

                Const securityIdentifierColumnName = "securityidentifier"
                Dim tableValuedParameter = New DataTable()
                tableValuedParameter.Columns.Add(New DataColumn(securityIdentifierColumnName, GetType(String)))

                activeDirectoryUsers.
                    ToList().
                    ForEach(Sub(user)
                                Dim newRow = tableValuedParameter.NewRow()
                                newRow(securityIdentifierColumnName) = user.Sid
                                tableValuedParameter.Rows.Add(newRow)
                            End Sub)

                command.Parameters.AddWithValue("@tvpActiveDirectoryUsers", tableValuedParameter)

                Using reader = connection.ExecuteReturnDataReader(command)
                    Dim alreadyMappedUsers = New HashSet(Of String)
                    Dim provider As New ReaderDataProvider(reader)
                    While reader.Read()
                        alreadyMappedUsers.Add(provider.GetString(securityIdentifierColumnName))
                    End While
                    Return alreadyMappedUsers
                End Using
            End Using
        End Using

    End Function

    Private Function GetMappedActiveDirectoryUserId(con As IDatabaseConnection, securityIdentifier As String) As Guid
        Using cmd As New SqlCommand("select bpuserid from BPAMappedActiveDirectoryUser where sid = @securityidentifier")
            cmd.Parameters.AddWithValue("@securityidentifier", securityIdentifier)
            Using dataReader = con.ExecuteReturnDataReader(cmd)

                If Not dataReader.Read() Then Return Guid.Empty

                Dim provider As New ReaderDataProvider(dataReader)
                Return provider.GetGuid("bpuserid")
            End Using
        End Using

    End Function

    Private Function GetActiveDirectoryUserSid(con As IDatabaseConnection, userId As Guid) As String
        Using cmd As New SqlCommand("select sid from BPAMappedActiveDirectoryUser where bpuserid = @bpuserid")
            cmd.Parameters.AddWithValue("@bpuserid", userId)
            Using dataReader = con.ExecuteReturnDataReader(cmd)

                If Not dataReader.Read() Then Return String.Empty

                Dim provider As New ReaderDataProvider(dataReader)
                Return provider.GetString("sid")
            End Using
        End Using
    End Function


    ''' <summary>
    ''' Validates the passed credentials against the current BP Active Directory
    ''' environment, returning the ID of the BP user.
    ''' </summary>
    ''' <param name="connection">The database connection</param>
    ''' <param name="username">The user name</param>
    ''' <param name="password">The password</param>
    ''' <returns>The validated user object, or Nothing if the credentials
    ''' were not valid.</returns>
    Private Function ValidateSSOCredential(connection As IDatabaseConnection, username As String, password As SafeString) As User

        Dim userPrincipalName = clsActiveDirectory.GetUserPrincipalName(username, GetActiveDirectoryDomain())

        Using authenticatedUser = GetAuthenticatedWindowsUser(userPrincipalName, password)

            If authenticatedUser Is Nothing OrElse Not authenticatedUser.IsValidAccountType Then Return Nothing

            Dim roles = clsActiveDirectory.GetRoles(authenticatedUser)
            If roles.Count = 0 Then Return Nothing

            Dim userID As Guid = Guid.Empty
            If Not MirrorUser(connection, userPrincipalName, userID) Then
                ActivateUser(connection, userID)
            End If

            'Get the user object without role information (as we've already got
            'it, and GetUsers() uses the current windows identity rather than
            'the impersonated identity used here.)
            Dim user = GetUsers(connection, {userID}, False).FirstOrDefault()
            For Each r In roles
                user.Roles.Add(r)
            Next

            Return user
        End Using
    End Function

    Private Function ValidateMappedActiveDirectoryUserCredential(connection As IDatabaseConnection, username As String,
                                                                 password As SafeString) As IUser
        Using authenticatedUser = GetAuthenticatedWindowsUser(username, password)
            If authenticatedUser Is Nothing Then Return Nothing

            Dim userId = TryGetUserID(connection, username)
            If userId = Guid.Empty Then Return Nothing

            Dim sid = GetMappedActiveDirectorySID(connection, userId)
            If authenticatedUser.Sid.ToString <> sid Then Return Nothing

            Dim loginResult = LoginWithMappedActiveDirectoryUser(connection, authenticatedUser)
            Return If(loginResult.IsSuccess, loginResult.User, Nothing)
        End Using
    End Function

    Private Function GetAuthenticatedWindowsUser(userPrincipalName As String, password As SafeString) As IWindowsIdentity
        Const LOGON32_PROVIDER_DEFAULT = 0
        Const LOGON32_LOGON_NETWORK = 3

        Dim userIsAuthenticated As Boolean
        Dim userToken As IntPtr

        userToken = LogonUser(userPrincipalName, Nothing, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT)
        If userToken = IntPtr.Zero Then Return Nothing

        Dim windowsIdentity = New WindowsIdentityWrapper(New WindowsIdentity(userToken))

        Using impersonationContext = windowsIdentity.Impersonate()
            userIsAuthenticated = windowsIdentity.IsAuthenticated()
            impersonationContext.Undo()
        End Using

        Return If(userIsAuthenticated, windowsIdentity, Nothing)

    End Function

    ''' <summary>
    ''' Increments the the number of login attempts for the specified user
    ''' </summary>
    ''' <param name="userid">The ID of the user to increment</param>
    Private Shared Sub IncrementLoginAttempts(
     ByVal con As IDatabaseConnection, ByVal userid As Guid)
        Dim cmd As New SqlCommand(
         "update BPAUser set loginattempts = loginattempts + 1 where userid = @id")
        cmd.Parameters.AddWithValue("@id", userid)
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Gets the user from the database with the given ID or name (or both).
    ''' </summary>
    ''' <param name="con">The connection to use to access the database.</param>
    ''' <param name="userId">The ID of the required user, or <see cref="Guid.Empty"/>
    ''' to get a user with a username only.</param>
    ''' <param name="userName">The username of the required user, or null to get a
    ''' user with a user ID only.</param>
    ''' <returns>The User object corresponding to the given ID or username.</returns>
    ''' <exception cref="ArgumentException">If neither the <paramref name="userId"/>
    ''' or <paramref name="userName"/> values are provided.</exception>
    ''' <exception cref="NoSuchElementException">If no user with the given
    ''' ID/username was found on the database.</exception>
    Private Function GetUser(ByVal con As IDatabaseConnection,
     ByVal userId As Guid, ByVal userName As String) As User

        If userId = Guid.Empty Then
            If userName = "" Then _
             Throw New ArgumentException(My.Resources.clsServer_GetUserRequiresEitherIDOrUsername)

            ' We need to get the user ID for the given user name
            Dim cmd As New SqlCommand(
             " select u.userid from BPAUser u where u.username = @name"
            )
            cmd.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = userName
            userId = IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
        End If

        Dim u As User = Nothing
        If userId <> Guid.Empty Then
            u = CollectionUtil.First(GetUsers(con, New Guid() {userId}))
        End If

        If u Is Nothing Then Throw New NoSuchElementException(
          My.Resources.clsServer_NoUserFoundWithID0Name1,
          IIf(userId = Guid.Empty, My.Resources.clsServer_Unspecified, userId),
          IIf(userName = "", My.Resources.clsServer_Unspecified, userName)
         )

        Return u

    End Function

    Private Function GetActiveUser(con As IDatabaseConnection, userName As String) As User

        Dim userId As Guid
        Using cmd As New SqlCommand(
            " select top 1 u.userid from BPAUser u where u.username = @name order by isdeleted desc"
            )
            cmd.Parameters.AddWithValue("@name", userName)
            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then
                    Throw New UserNotFoundException()
                End If

                Dim prov As New ReaderDataProvider(reader)
                userId = prov.GetValue("userid", Guid.Empty)
            End Using
        End Using

        Return GetActiveUser(con, userId)

    End Function

    Private Function GetActiveAuthServerServiceAccountUser(con As IDatabaseConnection, authServerClientId As String) As User

        Dim userId As Guid
        Dim cmd = mDatabaseCommandFactory(
            " select u.userid from BPAUser u where u.authenticationServerClientId = @authServerClientId and u.authtype = @authType")

        cmd.AddParameter("@authServerClientId", authServerClientId)
        cmd.AddParameter("@authType", AuthMode.AuthenticationServerServiceAccount)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then
                Throw New ServiceAccountUserNotFoundException()
            End If

            Dim prov As New ReaderDataProvider(reader)
            userId = prov.GetValue("userid", Guid.Empty)
        End Using

        Return GetActiveUser(con, userId)

    End Function


    Private Function GetActiveAuthServerUser(con As IDatabaseConnection, authServerUserId As Guid) As User

        Dim userId As Guid
        Using cmd As New SqlCommand(
            " select u.userid from BPAUser u where u.authenticationServerUserId = @authServerUserId"
            )
            cmd.Parameters.AddWithValue("@authServerUserId", authServerUserId)
            Using reader = con.ExecuteReturnDataReader(cmd)
                If Not reader.Read() Then
                    Throw New UserNotFoundException()
                End If

                Dim prov As New ReaderDataProvider(reader)
                userId = prov.GetValue("userid", Guid.Empty)
            End Using
        End Using

        Return GetActiveUser(con, userId)

    End Function

    Private Function GetActiveUser(con As IDatabaseConnection, userId As Guid) As User
        Return GetActiveUser(con, userId, True)
    End Function

    Private Function GetActiveUser(con As IDatabaseConnection, userId As Guid, populateRolesIntoUser As Boolean) As User
        Dim user As User = Nothing
        If userId <> Guid.Empty Then
            user = GetUsers(con, New Guid() {userId}, populateRolesIntoUser).FirstOrDefault()
        End If

        If user Is Nothing Then Throw New UserNotFoundException()
        If user.Deleted Then Throw New DeletedException(user.Name)

        Return user
    End Function

    ''' <summary>
    ''' Gets all of the users from the database with the given IDs.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="userIds">A collection containing all of the IDs of the required
    ''' users. If null is passed, then this method will return all users.</param>
    ''' <returns>The collection of users whose IDs were found in the given collection
    ''' or all users if the given collection was null.</returns>
    Private Function GetUsers(ByVal con As IDatabaseConnection,
     ByVal userIds As ICollection(Of Guid)) As ICollection(Of User)
        Return GetUsers(con, userIds, True)
    End Function

    ''' <summary>
    ''' Gets all of the users from the database with the given IDs, optionally
    ''' omitting the roles information from the user.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="userIds">A collection containing all of the IDs of the required
    ''' users. If null is passed, then this method will return all users.</param>
    ''' <param name="populateRoles">True to populate the roles inside the user; false
    ''' to leave that information blank. Populating the roles is potentially a long
    ''' operation, especially for AD users, and is not necessary for users in all
    ''' contexts.</param>
    ''' <returns>The collection of users whose IDs were found in the given collection
    ''' or all users if the given collection was null.</returns>
    Private Function GetUsers(
     con As IDatabaseConnection,
     userIds As IEnumerable(Of Guid),
     populateRoles As Boolean) As ICollection(Of User)

        Dim users As New List(Of User)

        ' If an empty collection was provided, return a similarly empty collection
        If userIds IsNot Nothing AndAlso userIds.Count = 0 Then Return users

        Dim cmd As New SqlCommand(
         " select" &
         "   u.userid," &
         "   u.username," &
         "   u.loginattempts," &
         "   u.isdeleted," &
         "   u.validfromdate as created," &
         "   u.validtodate as expiry," &
         "   u.passwordexpirydate as passwordexpiry," &
         "   u.passworddurationweeks," &
         "   u.alerteventtypes," &
         "   u.alertnotificationtypes," &
         "   u.lastsignedin," &
         "   u.authtype," &
         "   u.authenticationServerUserId," &
         "   u.authenticationServerClientId," &
         "   u.hasBluePrismApiScope," &
         "   u.deletedLastSynchronizationDate," &
         "   u.updatedLastSynchronizationDate," &
         "   u.authServerName," &
         "   c.passwordexpirywarninginterval," &
         "   case when u.loginattempts >= c.maxloginattempts then 1 else 0 end as locked" &
         " from BPAUser u cross join BPASysConfig c"
        )

        ' If we have a set of user IDs to retrieve, build up the constraint into a
        ' buffer and append it to the command
        If userIds IsNot Nothing Then
            Dim sb As New StringBuilder(" where u.userid in (")
            Dim index As Integer = 0
            For Each id As Guid In userIds
                If index <> 0 Then sb.Append(","c)
                index += 1
                sb.Append("@id").Append(index)
                cmd.Parameters.AddWithValue("@id" & index, id)
            Next
            sb.Append(")")
            cmd.CommandText &= sb.ToString()
        End If

        Dim loggedInMode = mLoggedInMode

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                users.Add(New User(prov))
            End While
        End Using

        For Each u As User In users
            If u.AuthType = AuthMode.External Then
                PopulateExternalIdentityAndProviderIntoUser(con, u)
            ElseIf u.AuthType = AuthMode.MappedActiveDirectory Then
                PopulateSidIntoUser(con, u)
            End If
        Next

        ' Now get the roles (if requested) - this could probably be improved by
        ' writing the SQL such that it services an arbitrary number of users rather
        ' than doing multiple round trips as it currently does
        If populateRoles Then
            Dim sso As Boolean = IsSingleSignOn(con)
            For Each u As User In users
                If sso AndAlso loggedInMode = AuthMode.ActiveDirectory AndAlso u.AuthType = AuthMode.ActiveDirectory Then
                    GetADuserRolesInto(u)
                Else
                    PopulateRolesIntoUser(con, u)
                End If
            Next
        End If

        Return users

    End Function

    Private Sub PopulateExternalIdentityAndProviderIntoUser(con As IDatabaseConnection, user As User)
        Dim externalCredentials = GetExternalIdForUser(con, user.Id)
        user.ExternalId = externalCredentials.ExternalId
        user.IdentityProviderName = externalCredentials.IdentityProviderName
        user.IdentityProviderType = externalCredentials.IdentityProviderType
    End Sub

    Private Sub PopulateSidIntoUser(con As IDatabaseConnection, user As User)
        user.ExternalId = GetActiveDirectoryUserSid(con, user.Id)
    End Sub

    Private Function GetExternalIdForUser(con As IDatabaseConnection, userId As Guid) As ExternalIdentityMapping
        If userId = Guid.Empty Then
            Throw New ArgumentNullException("UserId can not be empty")
        End If

        Dim externalId As String = String.Empty
        Dim idProviderName As String = String.Empty
        Dim idProviderType As String = String.Empty
        Dim cmd As New SqlCommand(
            " select " &
            " uei.externalid, " &
            " ep.name As externalprovidername, " &
            " ept.name AS externalprovidertypename " &
            " from BPAExternalProviderType ept " &
            " inner join BPAExternalProvider ep ON ept.id = ep.externalprovidertypeid " &
            " inner join BPAUserExternalIdentity uei ON ep.id = uei.externalproviderid " &
            " where uei.bpuserid = @userId")

        cmd.Parameters.AddWithValue("@userId", userId.ToString())
        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Return New ExternalIdentityMapping(String.Empty, String.Empty, String.Empty)

            Dim prov As New ReaderDataProvider(reader)
            externalId = prov.GetValue("externalid", String.Empty)
            idProviderName = prov.GetValue("externalprovidername", String.Empty)
            idProviderType = prov.GetValue("externalprovidertypename", String.Empty)
        End Using

        Return New ExternalIdentityMapping(externalId, idProviderName, idProviderType)
    End Function

    ''' <summary>
    ''' Get roles for an Active Directory user by looking at their current group
    ''' membership.
    ''' </summary>
    ''' <param name="u">The user in question.</param>
    Private Sub GetADuserRolesInto(u As User)

        If u.AuthType = AuthMode.System Then Throw New ArgumentException(My.Resources.clsServer_CannotGetRolesForASystemUser)
        If mLoggedInMode <> AuthMode.ActiveDirectory Then Return

        u.Roles.Clear()
        For Each r In clsActiveDirectory.GetRoles(GetCurrentIdentity())
            u.Roles.Add(r)
        Next

    End Sub

    ''' <summary>
    ''' Assigns the given set of roles to a user
    ''' </summary>
    ''' <param name="con">The connection over which the roles should be set</param>
    ''' <param name="userId">The ID of the user whose role assignments should be set
    ''' </param>
    ''' <param name="roles">The set of roles to assign to the user. Note that an
    ''' empty roleset is valid and will remove all roles from a user</param>
    ''' <exception cref="ArgumentNullException">If the given role set is null.
    ''' </exception>
    Private Sub AssignRoles(
     ByVal con As IDatabaseConnection, ByVal userId As Guid, ByVal roles As RoleSet)
        If roles Is Nothing Then Throw New ArgumentNullException(NameOf(roles))

        Dim cmd As New SqlCommand()
        cmd.Parameters.AddWithValue("@userid", userId)

        Dim sb As New StringBuilder(255)

        If mLoggedInUser IsNot Nothing AndAlso Not mLoggedInUser.IsSystemAdmin Then
            Dim userToAssignRolesTo = GetUser(con, userId, String.Empty)
            ' Check if adding or removing the System Administrator role from the user.
            If userToAssignRolesTo.IsSystemAdmin <> roles.Any(Function(r) r.SystemAdmin) Then
                Throw New OperationFailedException(
                    My.Resources.clsServer_TheLoggedInUserMustBeASystemAdministratorInOrderToAddOrRemoveThe0RoleFromAUser, Role.DefaultNames.SystemAdministrators)
            End If
        End If

        ' If we have no roles for this user, just delete any that are there
        If roles.Count = 0 Then
            sb.Append("delete from BPAUserRoleAssignment where userid = @userid;")

        Else
            ' Otherwise, we insert any which are in our roleset but not the database,
            ' and delete any which are in the database and not the roleset

            ' First we set up a temp '#roles' table to hold the roleset's IDs
            sb.Append(
             " if object_id('tempdb..#roles') is not null drop table #roles;" &
             " create table #roles (id int not null primary key);" &
             " insert into #roles"
            )
            Dim num As Integer = 0
            For Each r As Role In roles
                If num > 0 Then sb.Append(" union all")
                num += 1
                sb.AppendFormat(" select @roleId{0}", num)
                cmd.Parameters.AddWithValue("@roleId" & num, r.Id)
            Next
            sb.Append(";")

            ' Then insert any which are in #roles, but not BPAUserRoleAssignment
            sb.Append(
             " insert into BPAUserRoleAssignment" &
             "   select @userid, r.id" &
             "     from #roles r" &
             "       left join BPAUserRoleAssignment ura " &
             "         on ura.userid = @userid and r.id = ura.userroleid" &
             "     where ura.userroleid is null;"
            )

            ' And delete any which are in BPAUserRoleAssignment but not in #roles
            sb.Append(
             " delete ura" &
             "   from BPAUserRoleAssignment ura" &
             "     left join #roles r on ura.userroleid = r.id" &
             "   where ura.userid = @userid and r.id is null;"
            )

        End If

        cmd.CommandText = sb.ToString()
        con.Execute(cmd)

    End Sub

    ''' <summary>
    ''' Gets the user names as the given type. Note that the type can be gained
    ''' by "Or"ing together 2 different user types.
    ''' </summary>
    ''' <param name="type">The type value indicating which types of users are
    ''' required.</param>
    ''' <returns>A collection of the user names of the specified type from the
    ''' database.</returns>
    Private Function GetUserNames(ByVal type As UserType) As ICollection(Of String)

        Using con = GetConnection()
            Dim sb As New StringBuilder(
             "select isnull(u.username, '['+u.systemusername+']') as username" &
             "  from BPAUser u where 1=1"
            )
            ' If we don't want login users, filter them out
            If (type And UserType.Login) = 0 Then
                sb.Append(" and u.username is null")
            End If
            ' If we don't want system users, filter them out
            If (type And UserType.System) = 0 Then
                sb.Append(" and u.username is not null")
            End If
            sb.Append(" order by username")

            Dim cmd As New SqlCommand(sb.ToString())
            Dim names As ICollection(Of String) = New List(Of String)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    names.Add(reader.GetString(0))
                End While
            End Using
            Return names

        End Using

    End Function

    ''' <summary>
    ''' Gets the required username over a specified connection.
    ''' </summary>
    ''' <param name="con">The connection over which to get the username</param>
    ''' <param name="id">The user ID for which the username is required.</param>
    ''' <returns>The username corresponding to the given ID or an empty string if
    ''' no such user was found, or any other errors occur</returns>
    Private Function GetUserName(ByVal con As IDatabaseConnection, ByVal id As Guid) _
     As String
        Try
            Dim cmd As New SqlCommand(
             "select username from BPAUser where userid = @id")
            cmd.Parameters.AddWithValue("@id", id)
            Return IfNull(con.ExecuteReturnScalar(cmd), "")
        Catch
            Return ""
        End Try

    End Function

    Private Function GetUserByAuthenticationServerId(con As IDatabaseConnection, authenticationServerUserId As Guid) As User
        Dim userId As Guid
        Dim cmd As New SqlCommand(
            "select userid from BPAUser where authenticationServerUserId = @authenticationServerUserId")
        cmd.Parameters.AddWithValue("@authenticationServerUserId", authenticationServerUserId)
        userId = IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)

        Dim user As User = Nothing
        If userId <> Guid.Empty Then
            user = GetUsers(con, New Guid() {userId}, False).FirstOrDefault()
        End If

        If user Is Nothing Then Throw New AuthenticationServerUserNotFoundException()

        Return user
    End Function

    Private Function GetUserByAuthenticationServerClientId(con As IDatabaseConnection, authenticationServerClientId As String) _
        As User

        Dim userId As Guid
        Dim cmd As New SqlCommand(
            "select userid from BPAUser where authenticationServerClientId = @authenticationServerClientId")
        cmd.Parameters.AddWithValue("@authenticationServerClientId", authenticationServerClientId)
        userId = IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)

        Dim user As User = Nothing
        If userId <> Guid.Empty Then
            user = GetUsers(con, New Guid() {userId}).FirstOrDefault()
        End If

        If user Is Nothing Then Throw New AuthenticationServerUserNotFoundException()

        Return user
    End Function

    ''' <summary>
    ''' Try to get the ID of a user, given their name.
    ''' </summary>
    ''' <param name="userName">The name of the user</param>
    ''' <returns>The users ID (GUID) or Guid.Empty if the user does
    ''' not exist or the operation failed for any other reason.
    ''' </returns>
    Private Function TryGetUserID(con As IDatabaseConnection, userName As String) As Guid
        ' The username can be Nothing if the user is the anonymous resource or the
        ' scheduler, so just return an empty GUID to prevent sql errors
        If userName = "" Then Return Guid.Empty
        Try
            Return GetUserID(con, userName)
        Catch
            Return Guid.Empty
        End Try
    End Function

    Private Function TryGetUserAttrib(con As IDatabaseConnection, userId As Guid) As AuthMode
        Try
            Return GetUserAttrib(con, userId)
        Catch
            Return AuthMode.Unspecified
        End Try
    End Function

    ''' <summary>
    ''' Gets the ID of the user with the given name.
    ''' </summary>
    ''' <param name="con">The connection over which the user's ID should be retrieved.
    ''' </param>
    ''' <param name="username">The username of the user required.</param>
    ''' <returns>The ID of the given user.</returns>
    ''' <exception cref="NoSuchElementException">If no user with the specified name
    ''' was found on the database.</exception>
    ''' <exception cref="ArgumentNullException">If the user name provided is blank
    ''' </exception>
    Private Function GetUserID(ByVal con As IDatabaseConnection, ByVal username As String) As Guid
        If username = "" Then _
            Throw New ArgumentNullException(NameOf(username))
        Dim cmd As New SqlCommand("select userid from BPAUser where username = @UserName")
        cmd.Parameters.AddWithValue("@UserName", username)
        Dim idObj As Object = con.ExecuteReturnScalar(cmd)
        If idObj Is Nothing Then
            Throw New NoSuchElementException(My.Resources.clsServer_TheUsername0WasNotFound, username)
        End If
        Return DirectCast(idObj, Guid)
    End Function


    Private Function GetUserAttrib(ByVal con As IDatabaseConnection, userId As Guid) As AuthMode
        If userId = Guid.Empty Then
            Throw New ArgumentException("Invalid userId", NameOf(userId))
        End If

        Using cmd As New SqlCommand("select authtype from BPAUser where userId = @userId")
            cmd.Parameters.AddWithValue("@userId", userId)
            Dim idObj As Object = con.ExecuteReturnScalar(cmd)
            If idObj Is Nothing Then
                Throw New NoSuchElementException($"The userid {userId} not found")
            End If
            Return DirectCast(idObj, AuthMode)
        End Using

    End Function

    ''' <summary>
    ''' Validates a supplied authorisation token. On validation, the token is
    ''' immediately expired - tokens are single-use only.
    ''' </summary>
    ''' <param name="reasonInvalid">When the token is invalid (False returned),
    ''' carries back an explanation as to why the token is not valid (e.g. expired,
    ''' or belongs to different user).</param>
    ''' <returns>Returns a user object if the token is valid, or Nothing if it is not
    ''' valid and should be rejected.</returns>
    <UnsecuredMethod()>
    Public Function ValidateAuthorisationToken(authToken As clsAuthToken, ByRef reasonInvalid As String) As User Implements IServer.ValidateAuthorisationToken
        Using connection = GetConnection()
            Return ValidateAuthorisationToken(connection, authToken, Guid.Empty, Nothing, reasonInvalid)
        End Using
    End Function

    Private Function ValidateAuthorisationToken(connection As IDatabaseConnection,
                                                authToken As clsAuthToken, procId As Guid,
                                                ByRef webService As Boolean, ByRef reasonInvalid As String) As User

        Try

            Dim roles As New RoleSet()

            Dim userId = authToken.OwningUserID
            Dim loggedInMode As AuthMode
            Using cmd As New SqlCommand("select UserID, Roles, LoggedInMode, IsWebService, " &
                 " case when Expiry > GETUTCDATE() then 0 else 1 end as IsExpired " &
                 " from BPAInternalAuth where Token = @TokenValue " &
                 "   and (processId = @processId or @processId is null)")

                With cmd.Parameters
                    .AddWithValue("@TokenValue", authToken.Token)
                    .AddWithValue("@processId", IIf(procId = Guid.Empty, DBNull.Value, procId))
                End With

                Using reader = connection.ExecuteReturnDataReader(cmd)
                    If Not reader.Read() Then
                        reasonInvalid = My.Resources.clsServer_NoMatchingTokenFoundInDatabase
                        Return Nothing
                    End If

                    Dim userIDFromDB As Guid = CType(reader("UserID"), Guid)
                    If userIDFromDB <> userId Then
                        reasonInvalid = My.Resources.clsServer_TheTokenBelongsToAnotherUser
                        Return Nothing
                    End If

                    Dim isExpiredFromDB As Boolean = CType(reader("IsExpired"), Boolean)
                    If isExpiredFromDB Then
                        reasonInvalid = My.Resources.clsServer_TheTokenHasExpired
                        Return Nothing
                    End If

                    Dim flatRoles = CStr(reader("Roles"))
                    If Not String.IsNullOrEmpty(flatRoles) Then
                        For Each rid In flatRoles.Split(","c)
                            roles.Add(SystemRoleSet.Current(CInt(rid)))
                        Next
                    End If

                    loggedInMode = CType(reader("LoggedInMode"), AuthMode)

                    webService = CBool(reader("IsWebService"))
                End Using
            End Using

            'We're accepting the token, so expire it straight away.
            Using cmd As New SqlCommand(
                " delete BPAInternalAuth " &
                " where Token = @TokenValue")
                With cmd.Parameters
                    .AddWithValue("@TokenValue", authToken.Token)
                End With
                connection.Execute(cmd)
            End Using

            reasonInvalid = Nothing
            Return New User(loggedInMode, userId, GetUserName(userId), roles)

        Catch ex As Exception
            Throw ex
        Finally
            'Perform maintenance on the BPAInternalAuth database.
            Using randomGen = New CryptoRandom()
                If randomGen.Next(1, 200) = 100 Then
                    Using command As New SqlCommand("delete from BPAInternalAuth where Expiry < GETUTCDATE()")
                        connection.Execute(command)
                    End Using
                End If
            End Using
        End Try
    End Function

    ''' <summary>
    ''' Gets the system user ID corresponding to the given name.
    ''' </summary>
    ''' <param name="sysUserName">The 'systemusername' value for the required system
    ''' user.</param>
    ''' <returns>The ID of the required system user or Guid.Empty if the system user
    ''' name was not found.</returns>
    Private Function GetSystemUserId(con As IDatabaseConnection, sysUserName As String) As Guid
        Dim cmd As New SqlCommand(
             "select userid from BPAUser where systemusername=@sysname"
            )
        cmd.Parameters.AddWithValue("@sysname", sysUserName)
        Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    End Function

    ''' <summary>
    ''' Get the set of permissions that are available for configuring at group
    ''' level for the passed tree type.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="treeType">The group tree type</param>
    ''' <returns>The set of available permissions</returns>
    Private Function GetGroupAvailablePermissions(con As IDatabaseConnection, treeType As GroupTreeType) As ICollection(Of GroupTreePermission)

        Dim perms = New List(Of GroupTreePermission)
        ' Get any permissions that are associated with the group tree type
        Dim cmd As New SqlCommand("
                select permid, groupLevelPerm from BPATreePerm where treeid=@treeid")
        cmd.Parameters.AddWithValue("@treeid", treeType)

        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()

                perms.Add(New GroupTreePermission(Permission.GetPermission(prov.GetInt("permid")),
                                                   prov.GetValue("groupLevelPerm", GroupPermissionLevel.Member)))
            End While
        End Using
        Return perms
    End Function

    ''' <summary>
    ''' Checks that the give process/object may be accessed by the current user
    ''' </summary>
    ''' <param name="connection">The db connection to use</param>
    ''' <param name="processId">The processId</param>
    ''' <param name="action">For the exception a string representing the action the user
    ''' is trying to perform.</param>
    ''' <param name="processPermissions">The permission required if this is a process</param>
    ''' <param name="objectPermissions">The permission required if this is an object</param>
    Private Sub CheckProcessOrObjectPermissions(
    connection As IDatabaseConnection, processId As Guid, action As String,
    processPermissions As String(), objectPermissions As String())

        Dim permissions As String(), typeName As String
        Select Case GetProcessType(connection, processId)
            Case DiagramType.Process
                typeName = "process definition"
                permissions = processPermissions
            Case DiagramType.Object
                typeName = "object definition"
                permissions = objectPermissions
            Case Else
                Throw New InvalidArgumentException(My.Resources.clsServer_InvalidProcessType)
        End Select

        Dim member = GetEffectiveMemberPermissionsForProcess(processId)
        If member.IsRestricted AndAlso Not member.HasPermission(mLoggedInUser, permissions) Then
            Throw New PermissionException(
                My.Resources.clsServer_YouDoNotHavePermissionTo0This1, action, typeName)
        End If
    End Sub

#End Region

#Region " Internal Methods "

    ''' <summary>
    ''' Initialises the permissions in the current environment.
    ''' </summary>
    <UnsecuredMethod>
    Public Function GetPermissionData() As PermissionData Implements IServer.GetPermissionData
        Using con = GetConnection()
            Dim cmd As New SqlCommand()

            ' First we get all the permissions
            ' Then we get all the permission groups
            ' Then we get all of the assignments of perms to perm groups
            cmd.CommandText = " select p.id, p.name, p.requiredFeature from BPAPerm p;" &
                                " select pg.id, pg.name, pgm.permid, pg.requiredFeature" &
                                " from BPAPermGroup pg" &
                                "   join BPAPermGroupMember pgm on pgm.permgroupid = pg.id;"

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderMultiDataProvider(reader)

                ' Permissions first...
                Dim perms As IDictionary(Of Integer, Permission) =
                 Permission.Load(prov)

                ' Then permission groups
                If Not prov.NextResult() Then Throw New OperationFailedException(
                 My.Resources.clsServer_NoResultsForBPAPermGroupBPAPermGroupMember)

                Dim groups As IDictionary(Of Integer, PermissionGroup) =
                 PermissionGroup.Load(prov, perms)

                Return New PermissionData(perms, groups)

            End Using

        End Using
    End Function

#End Region

End Class
