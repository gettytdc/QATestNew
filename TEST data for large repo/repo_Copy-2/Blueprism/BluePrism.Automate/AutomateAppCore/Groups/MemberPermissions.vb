Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Container for group permissions return from server
''' </summary>
<DataContract(Name:="mp", [Namespace]:="bp"), Serializable>
<KnownType(GetType(GroupPermissions))>
Public Class MemberPermissions
    Implements IMemberPermissions

#Region "class data"
    ''' <summary>
    ''' List of permission Id's that the current user has 
    ''' on this group member
    ''' </summary>
    <DataMember(EmitDefaultValue:=False, IsRequired:=False, Name:="gps")>
    Private mGroupPermissions As IGroupPermissions = Nothing

#End Region

#Region "Constructor"

    ''' <summary>
    ''' Create a member permission object based on group permissions and 
    ''' a user's roles
    ''' </summary>
    ''' <param name="grpPerms">The permissions for the containing group</param>
    Public Sub New(grpPerms As IGroupPermissions)
        If grpPerms IsNot Nothing Then
            Me.State = grpPerms.State
        Else
            Me.State = PermissionState.Unknown
        End If
        mGroupPermissions = grpPerms
    End Sub


#End Region

#Region "Public Properties"

    ''' <summary>
    ''' Property to show the restriction placed on the current group
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="s")>
    Public Property State As PermissionState Implements IMemberPermissions.State

    ''' <summary>
    ''' Returns true if there are no permissions
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsEmpty As Boolean Implements IMemberPermissions.IsEmpty
        Get
            Return mGroupPermissions Is Nothing OrElse mGroupPermissions.Count = 0
        End Get
    End Property

    ''' <summary>
    ''' Returns true if the member is directly restricted or restricted by inheritance
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsRestricted As Boolean Implements IMemberPermissions.IsRestricted
        Get
            Return State = PermissionState.Restricted OrElse State = PermissionState.RestrictedByInheritance
        End Get
    End Property
#End Region

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group member
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The names of the permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, ParamArray perms() As String) As Boolean _
        Implements IMemberPermissions.HasPermission
        Return HasPermission(u, DirectCast(perms, ICollection(Of String)))
    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group member
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The names of the permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, perms As ICollection(Of String)) As Boolean _
        Implements IMemberPermissions.HasPermission
        Return HasPermission(u, Permission.ByName(perms))
    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group member
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, ParamArray perms() As Permission) As Boolean _
        Implements IMemberPermissions.HasPermission
        Return HasPermission(u, DirectCast(perms, ICollection(Of Permission)))
    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group member
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Overridable Function HasPermission(u As IUser, perms As ICollection(Of Permission)) As Boolean _
        Implements IMemberPermissions.HasPermission

        If u Is Nothing Then Return False
        ' Remove any empty permission entries
        perms = perms.Where(Function(p) p IsNot Nothing).ToList()

        ' If no permission passed, or user is System Administrator
        ' then assume user has access
        If CollectionUtil.IsNullOrEmpty(perms) OrElse
           u.IsSystemAdmin OrElse u.AuthType = AuthMode.System Then Return True

        ' If user's role does not allow this permission then
        ' don't need to check for any group level permissions
        If Not u.HasPermission(perms) Then Return False

        ' If user has Runtime Resources role, and that role contains one of the
        ' permissions being checked, then don't need to descend to group level
        ' (as Runtime Resources role cannot be restricted at group level)
        Dim resourceRole = Role.DefaultNames.RuntimeResources
        If u.Roles.Contains(resourceRole) AndAlso
            SystemRoleSet.Current(resourceRole).Permissions.Intersect(perms).Count > 1 Then Return True

        ' If the group is unrestricted then allow access
        If Not Me.IsRestricted Then Return True

        Dim memberPerms = GetMemberPermissions(u, mGroupPermissions)

        ' Otherwise check if we hold any of the permission IDs
        Return CollectionUtil.ContainsAny(Of Integer)(
            memberPerms, perms.Select(Of Integer)(Function(p) p.Id))

    End Function

    Private Function GetMemberPermissions(u As IUser, g As IGroupPermissions) As HashSet(Of Integer)

        ' Filter the roles we have by the roles for the user passed in.
        Dim memberPermissions = New HashSet(Of Integer)

        If mGroupPermissions IsNot Nothing Then
            For Each groupPermission As GroupLevelPermissions In mGroupPermissions
                ' Add the permissions, if the user has the role.
                If u.Roles.ToList().Exists(Function(x) x.Id = groupPermission.Id) Then
                    For Each p As Permission In groupPermission
                        If Not memberPermissions.Contains(p.Id) Then
                            memberPermissions.Add(p.Id)
                        End If
                    Next

                End If
            Next
        End If
        Return memberPermissions
    End Function

    ''' <summary>
    ''' Checks if the user has access to one or more permissions on this group member
    ''' </summary>
    ''' <param name="user">the user to test against these permissions</param>
    ''' <returns>True if the user has at least one of the permissions, or is in System Admin role. False otherwise</returns>
    Public Function HasAnyPermissions(user As IUser) As Boolean _
        Implements IMemberPermissions.HasAnyPermissions
        If user.IsSystemAdmin OrElse user.AuthType = AuthMode.System Then Return True
        If State = PermissionState.UnRestricted Then Return True
        If (user.Roles.Contains(Role.DefaultNames.RuntimeResources)) Then Return True

        Return GetMemberPermissions(user, mGroupPermissions).Count > 0
    End Function

End Class
