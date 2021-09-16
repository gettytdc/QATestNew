Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Utilities.Functional

''' <summary>
''' Container for group permissions return from server
''' </summary>
<DataContract([Namespace]:="bp", Name:="gp"), Serializable>
Public Class GroupPermissions
    Implements IGroupPermissions, IList(Of GroupLevelPermissions), IEquatable(Of GroupPermissions)

    ' Don't instantiate this unless necessary to save A LOT of serialization
    <DataMember(EmitDefaultValue:=False, IsRequired:=False, Name:="glp")>
    Private mGroupLevelPermissions As List(Of GroupLevelPermissions)

    Public Sub New()
        Me.State = PermissionState.Unknown
        Me.MemberId = Guid.Empty
    End Sub

    ''' <summary>
    ''' Specific constructor allows input of state and id
    ''' </summary>
    ''' <param name="initalState">initial state</param>
    ''' <param name="memberId">memberid of group member</param>
    Public Sub New(memberId As Guid, initalState As PermissionState)
        MyBase.New()
        Me.State = initalState
        Me.MemberId = memberId
    End Sub

    ''' <summary>
    ''' Specific constructor allows input of state only
    ''' </summary>
    ''' <param name="initalState">initial state</param>
    Public Sub New(initalState As PermissionState)
        MyBase.New()
        Me.State = initalState
        Me.MemberId = Guid.Empty
    End Sub

    ''' <summary>
    ''' The identity of the group
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="m", IsRequired:=False, EmitDefaultValue:=False)>
    Public Property MemberId As Guid Implements IGroupPermissions.MemberId


    ''' <summary>
    ''' Property to show the restriction placed on the current group
    ''' </summary>
    ''' <returns></returns>
    <DataMember(Name:="s")>
    Public Property State As PermissionState Implements IGroupPermissions.State



    ''' <summary>
    ''' The id of the group that this group inherits its permissions from.
    ''' </summary>
    <DataMember(IsRequired:=False, EmitDefaultValue:=False, Name:="i")>
    Public Property InheritedAncestorID As Guid Implements IGroupPermissions.InheritedAncestorID

    ''' <summary>
    ''' Combine the current set of permissions with another.
    ''' </summary>
    ''' <param name="otherPermissions">The permissions object to merge with</param>
    Public Sub Merge(otherPermissions As IGroupPermissions) Implements IGroupPermissions.Merge

        otherPermissions.ForEach(Sub(otherGroupLevelPermission)
                                     Dim matchingGroupLevelPermission = FirstOrDefault(Function(y) y.Id = otherGroupLevelPermission.Id)
                                     If matchingGroupLevelPermission Is Nothing Then
                                         DirectCast(otherGroupLevelPermission.Clone(), GroupLevelPermissions).Tee(AddressOf Add)
                                     Else
                                         matchingGroupLevelPermission.Tee(MergeGroupLevelPermissions(otherGroupLevelPermission))
                                     End If
                                 End Sub).
                                 Evaluate()

        State = GetLeastRestrictiveState(State, otherPermissions.State)
    End Sub

    Private Shared Function GetLeastRestrictiveState(a As PermissionState, b As PermissionState) As PermissionState
        ' Anything is better than unknown - might get rid of this.
        If a = PermissionState.Unknown Then Return b

        ' if either is unrestricted it's unrestricted
        If a = PermissionState.UnRestricted OrElse b = PermissionState.UnRestricted Then Return PermissionState.UnRestricted

        ' if we are here, neither is unrestricted, so if either is directly restricted return that.
        If a = PermissionState.Restricted OrElse b = PermissionState.Restricted Then Return PermissionState.Restricted

        ' only case left.
        Return PermissionState.RestrictedByInheritance
    End Function

    Private Shared Function MergeGroupLevelPermissions(otherPermissions As GroupLevelPermissions) As Action(Of GroupLevelPermissions)

        Return Sub(myPermissions) _
            otherPermissions.
                Except(myPermissions).
                ForEach(AddressOf myPermissions.Add).
                Evaluate()

    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The names of the permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, ParamArray perms() As String) As Boolean Implements IGroupPermissions.HasPermission
        Return HasPermission(u, DirectCast(perms, ICollection(Of String)))
    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The names of the permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, perms As ICollection(Of String)) As Boolean Implements IGroupPermissions.HasPermission
        Return HasPermission(u, Permission.ByName(perms))
    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, ParamArray perms() As Permission) As Boolean Implements IGroupPermissions.HasPermission
        Return HasPermission(u, DirectCast(perms, ICollection(Of Permission)))
    End Function

    ''' <summary>
    ''' Checks if the passed user has access to any of the given permissions on
    ''' this group
    ''' </summary>
    ''' <param name="u">The user to check</param>
    ''' <param name="perms">The permissions to check for</param>
    ''' <returns>True if the user has at least one of the permissions, otherwise
    ''' False</returns>
    Public Function HasPermission(u As IUser, perms As ICollection(Of Permission)) As Boolean Implements IGroupPermissions.HasPermission

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
        If State = PermissionState.UnRestricted Then Return True

        ' Otherwise check if we hold any of the permissions
        For Each i As GroupLevelPermissions In Me
            If u.Roles.ToList().Exists(Function(x) x.Id = i.Id) AndAlso
                CollectionUtil.ContainsAny(Of Permission)(i.Permissions, perms) Then Return True
        Next
        Return False
    End Function


    ''' <summary>
    ''' Returns true if this Group is restricted directly or by inheritance
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsRestricted As Boolean Implements IGroupPermissions.IsRestricted
        Get
            Return State = PermissionState.Restricted OrElse State = PermissionState.RestrictedByInheritance
        End Get
    End Property

    ''' <summary>
    ''' Gets the Group level permission for the given roleid
    ''' </summary>
    ''' <param name="roleid">The Id of the role</param>
    Public Function GetGroupLevelPermission(roleid As Integer) As GroupLevelPermissions Implements IGroupPermissions.GetGroupLevelPermission
        Return Where(Function(x) x.Id = roleid).FirstOrDefault
    End Function

    Public Function IndexOf(item As GroupLevelPermissions) As Integer Implements IList(Of GroupLevelPermissions).IndexOf
        If mGroupLevelPermissions Is Nothing Then Return -1
        Return mGroupLevelPermissions.IndexOf(item)
    End Function

    Public Sub Insert(index As Integer, item As GroupLevelPermissions) Implements IList(Of GroupLevelPermissions).Insert
        If mGroupLevelPermissions Is Nothing Then mGroupLevelPermissions = New List(Of GroupLevelPermissions)
        mGroupLevelPermissions.Insert(index, item)
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList(Of GroupLevelPermissions).RemoveAt
        If mGroupLevelPermissions Is Nothing Then Return
        mGroupLevelPermissions.RemoveAt(index)
    End Sub

    Public Sub Add(item As GroupLevelPermissions) Implements ICollection(Of GroupLevelPermissions).Add
        If mGroupLevelPermissions Is Nothing Then
            mGroupLevelPermissions = New List(Of GroupLevelPermissions)
        End If
        mGroupLevelPermissions.Add(item)
    End Sub

    Public Sub Clear() Implements ICollection(Of GroupLevelPermissions).Clear
        mGroupLevelPermissions = Nothing
    End Sub

    Public Function Contains(item As GroupLevelPermissions) As Boolean Implements ICollection(Of GroupLevelPermissions).Contains
        If mGroupLevelPermissions Is Nothing Then
            Return False
        End If
        Return mGroupLevelPermissions.Contains(item)
    End Function

    Public Sub CopyTo(array() As GroupLevelPermissions, arrayIndex As Integer) Implements ICollection(Of GroupLevelPermissions).CopyTo
        If mGroupLevelPermissions Is Nothing Then
            Return
        End If
        mGroupLevelPermissions.CopyTo(array, arrayIndex)
    End Sub

    Public Function Remove(item As GroupLevelPermissions) As Boolean Implements ICollection(Of GroupLevelPermissions).Remove
        If mGroupLevelPermissions Is Nothing Then
            Return False
        End If
        Return mGroupLevelPermissions.Remove(item)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of GroupLevelPermissions) Implements IEnumerable(Of GroupLevelPermissions).GetEnumerator
        If mGroupLevelPermissions Is Nothing Then
            Dim emptyList = New List(Of GroupLevelPermissions)
            Return emptyList.GetEnumerator()
        End If
        Return mGroupLevelPermissions.GetEnumerator()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        If mGroupLevelPermissions Is Nothing Then
            Dim emptyList = New List(Of GroupLevelPermissions)
            Return emptyList.GetEnumerator()
        End If
        Return mGroupLevelPermissions.GetEnumerator()
    End Function


    Default Public Property Item(index As Integer) As GroupLevelPermissions Implements IList(Of GroupLevelPermissions).Item
        Get
            If mGroupLevelPermissions Is Nothing Then
                Throw New ArgumentOutOfRangeException($"Permissions list does not have item {index}")
            Else
                Return mGroupLevelPermissions(index)
            End If
        End Get
        Set(value As GroupLevelPermissions)
            If mGroupLevelPermissions Is Nothing Then
                Throw New ArgumentOutOfRangeException($"Permissions list does not have item {index}")
            Else
                mGroupLevelPermissions(index) = value
            End If
        End Set
    End Property

    Public ReadOnly Property Count As Integer Implements ICollection(Of GroupLevelPermissions).Count
        Get
            If mGroupLevelPermissions Is Nothing Then Return 0 Else Return mGroupLevelPermissions.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of GroupLevelPermissions).IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public Overloads Function Equals(other As GroupPermissions) As Boolean Implements IEquatable(Of GroupPermissions).Equals
        If other Is Nothing Then Return False
        If ReferenceEquals(Me, other) Then Return True
        Return _
            If(
                mGroupLevelPermissions IsNot Nothing,
                mGroupLevelPermissions.ElementsEqual(other.mGroupLevelPermissions),
                Equals(mGroupLevelPermissions, other.mGroupLevelPermissions)) AndAlso
            MemberId.Equals(other.MemberId) AndAlso
            State = other.State AndAlso
            InheritedAncestorID.Equals(other.InheritedAncestorID)
    End Function

    Public Overloads Overrides Function Equals(obj As Object) As Boolean
        If ReferenceEquals(Nothing, obj) Then Return False
        If ReferenceEquals(Me, obj) Then Return True
        If obj.GetType IsNot Me.GetType Then Return False
        Return Equals(DirectCast(obj, GroupPermissions))
    End Function

    Public Overrides Function GetHashCode() As Integer
        Dim hashCode = 0
        If mGroupLevelPermissions IsNot Nothing Then
            hashCode = CInt((hashCode*397L) Mod Integer.MaxValue) Xor mGroupLevelPermissions.GetHashCode
        End If
        hashCode = CInt((hashCode*397L) Mod Integer.MaxValue) Xor MemberId.GetHashCode
        hashCode = CInt((hashCode*397L) Mod Integer.MaxValue) Xor CInt(State)
        hashCode = CInt((hashCode*397L) Mod Integer.MaxValue) Xor InheritedAncestorID.GetHashCode
        Return hashCode
    End Function

    Public Function Clone() As Object Implements ICloneable.Clone
        Return New GroupPermissions() With
            {
            .InheritedAncestorID = Me.InheritedAncestorID,
            .MemberId = Me.MemberId,
            .mGroupLevelPermissions = Me.mGroupLevelPermissions,
            .State = Me.State
        }
    End Function
End Class
