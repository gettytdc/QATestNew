Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Server.Domain.Models

Namespace Auth
    Public Interface IUser
        ''' <summary>
        ''' True if the user is assigned the System Administrator role.
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsSystemAdmin() As Boolean

        ''' <summary>
        ''' The date/time that the user logged into this session (UTC).
        ''' </summary>
        <DataMember>
        Property SignedInAt As Date

        ''' <summary>
        ''' The date/time that the user logged into their previous session (UTC).
        ''' </summary>
        <DataMember>
        Property LastSignedInAt As Date

        ''' <summary>
        ''' Indicates whether or not this user's account is currently locked.
        ''' </summary>
        <DataMember>
        Property IsLocked As Boolean

        ''' <summary>
        ''' The ID of this user.
        ''' </summary>
        ReadOnly Property Id() As Guid

        ''' <summary>
        ''' The username of this user.
        ''' </summary>
        Property Name() As String

        ReadOnly Property AuthType As AuthMode

        ''' <summary>
        ''' Boolean indicating whether the user is a hidden
        ''' </summary>
        ReadOnly Property IsHidden As Boolean

        ''' <summary>
        ''' The roles currently assigned to this user.
        ''' </summary>
        ReadOnly Property Roles() As RoleSet

        ''' <summary>
        ''' The permissions in effect for this user.
        ''' </summary>
        ReadOnly Property EffectivePermissions() As ICollection(Of Permission)

        ''' <summary>
        ''' The date/time that this user was created.
        ''' </summary>
        Property Created() As Date

        ''' <summary>
        ''' Gets the formatted local date/time that this user was created
        ''' </summary>
        ReadOnly Property CreatedOptionalDisplayDate As String

        ''' <summary>
        ''' The date/time that this user expires
        ''' </summary>
        Property Expiry() As Date

        ''' <summary>
        ''' The date/time that this user's password expires
        ''' </summary>
        Property PasswordExpiry() As Date

        ''' <summary>
        ''' Display string for the expiry date for this user
        ''' </summary>
        ReadOnly Property ExpiryDisplay() As String

        ''' <summary>
        ''' Display value of the password expiry date for this user.
        ''' </summary>
        ReadOnly Property PasswordExpiryDisplay() As String

        ''' <summary>
        ''' Gets whether this user has expired or not
        ''' </summary>
        ReadOnly Property Expired() As Boolean

        ''' <summary>
        ''' Gets whether this user's password has expired or not
        ''' </summary>
        ReadOnly Property PasswordExpired() As Boolean

        ''' <summary>
        ''' Gets the duration of the password's validity for this user
        ''' </summary>
        Property PasswordDurationWeeks() As Integer

        ''' <summary>
        ''' The alerts that this user is subscribed to
        ''' </summary>
        Property SubscribedAlerts() As AlertEventType

        ''' <summary>
        ''' The alert notifications that this user has registered for
        ''' </summary>
        Property AlertNotifications() As AlertNotificationType

        ''' <summary>
        ''' Gets whether this user is marked as deleted or not.
        ''' </summary>
        Property Deleted() As Boolean

        ''' <summary>
        ''' Gets the formatted local date/time that this session started
        ''' </summary>
        ReadOnly Property SignedInAtDisplay As String

        ''' <summary>
        ''' Gets the formatted local date/time that the user last signed in
        ''' </summary>
        ReadOnly Property LastSignedInAtOptionalDisplayDate As String

        ''' <summary>
        ''' Indicates whether or not this user account will expire within the next n
        ''' days (as configured in System Manager)
        ''' </summary>
        ReadOnly Property AccountExpiresSoon() As Boolean

        ''' <summary>
        ''' Indicates whether or not this user's password will expire within the next
        ''' n days (as configured in System Manager)
        ''' </summary>
        ReadOnly Property PasswordExpiresSoon() As Boolean

        ReadOnly Property ExternalId() As String

        ReadOnly Property IdentityProviderName() As String

        ReadOnly Property IdentityProviderType() As String

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
        Function HasPermission(ByVal ParamArray permNames() As String) _
            As Boolean

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
        Function HasPermission(ByVal permNames As ICollection(Of String)) _
            As Boolean

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
        Function HasPermission(ByVal ParamArray perms() As Permission) _
            As Boolean

        ''' <summary>
        ''' Checks if this user has any of the given permissions available to it
        ''' </summary>
        ''' <param name="perms">The permissions to check for.</param>
        ''' <returns>True if any of the required permissions are available to this
        ''' user; False if none of them are. Note that if <paramref name="perms"/> is
        ''' null or empty, that is treated as 'all users have permission' - ie. this
        ''' method will return <c>True</c>.</returns>
        Function HasPermission(ByVal perms As ICollection(Of Permission)) _
            As Boolean

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
        Function HasPermission(ByVal constraint As IPermission) As Boolean

        ''' <summary>
        ''' Checks if this user has all of the named permissions available to it
        ''' </summary>
        ''' <param name="permNames">The names of the permissions to check for.
        ''' </param>
        ''' <returns>True if all of the required permissions are available to this
        ''' user; False if any of them are not.</returns>
        ''' <exception cref="ArgumentException">If no permission names were provided
        ''' in <paramref name="permNames"/>.</exception>
        Function HasAllPermissions(ByVal ParamArray permNames() As String) _
            As Boolean

        ''' <summary>
        ''' Checks if this user has all of the named permissions available to it
        ''' </summary>
        ''' <param name="permNames">The names of the permissions to check for.
        ''' </param>
        ''' <returns>True if all of the required permissions are available to this
        ''' user; False if any of them are not.</returns>
        ''' <exception cref="ArgumentException">If no permission names were provided
        ''' in <paramref name="permNames"/>.</exception>
        Function HasAllPermissions(ByVal permNames As ICollection(Of String)) _
            As Boolean

        ''' <summary>
        ''' Checks if this user has permissions required to import a file of this type
        ''' </summary>
        ''' <param name="filePath">The path of the file to check permissions for.
        ''' </param>
        ''' <returns>True if all of the required permissions are available to this
        ''' user; False if any of them are not.</returns>
        ''' <exception cref="ArgumentException">If no filepath were provided
        ''' in <paramref name="filePath"/>.</exception>
        Function HasPermissionToImportFile(filePath As String) As Boolean

        ''' <summary>
        ''' Checks if this user is an alert subscriber or not - ie. that they have
        ''' the <c>"Subscribe to Process Alerts"</c> permission and they are
        ''' registered as a subscriber to any actual alerts.
        ''' </summary>
        ''' <returns>True if this user is permitted to subscribe to process alerts
        ''' and they are actually subscribed to some.</returns>
        Function IsAlertSubscriber() As Boolean

        ''' <summary>
        ''' Check if this user has the given role
        ''' </summary>
        ''' <param name="role">The role to check</param>
        ''' <returns>True of the user has this role</returns>
        Function HasRole(role As Role) As Boolean

        ''' <summary>
        ''' Check if this user has the given role
        ''' </summary>
        ''' <param name="role">The role to check</param>
        ''' <returns>True of the user has this role</returns>
        Function HasRole(role As Integer) As Boolean

        ''' <summary>
        ''' Provides access to the users authorisation token
        ''' </summary>
        ''' <returns></returns>
        Property AuthorisationToken As clsAuthToken

        ReadOnly Property AuthenticationServerUserId As Guid?

        ReadOnly Property AuthenticationServerClientId As String

        Property DeletedLastSynchronizationDate As DateTimeOffset?

        Property UpdatedLastSynchronizationDate As DateTimeOffset?

        Function CanSeeTree(treeType As GroupTreeType) As Boolean

        ReadOnly Property IsAuthenticationServerUserOrServiceAccount() As Boolean

        ReadOnly Property HasBluePrismApiScope() As Boolean

        ReadOnly Property AuthServerName() As String
    End Interface
End Namespace
