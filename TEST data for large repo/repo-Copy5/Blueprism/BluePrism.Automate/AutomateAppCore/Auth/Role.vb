Imports System.Threading
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.clsServerPartialClasses

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Namespace Auth

    ''' <summary>
    ''' Class to represent a user role in this environment.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class Role : Inherits PermHolder

#Region " Published Events "

        ' FIXME: Auth: In .net3.5+, the custom event code and related field can all
        ' be replaced by the following declaration, but in .net2, you can't set an
        ' event to be nonserialized, so this hoop jumping is required.
        '<NonSerialized()> _
        'Public Event NameChanged As NameChangedEventHandler

        ' The event handler for the NameChanged event - ripe for removal - see above
        <NonSerialized()> _
        Private m_NameChanged As NameChangedEventHandler

        ''' <summary>
        ''' Event fired when this role's name is changed
        ''' </summary>
        ''' <remarks>FIXME: This should be removed when we move to .NET4; it can be
        ''' replaced with a simple
        ''' <see cref="NonSerializedAttribute">NonSerialized</see> attribute on the
        ''' <c>NameChanged</c> event declaration, but that is not supported in .NET2
        ''' </remarks>
        Public Custom Event NameChanged As NameChangedEventHandler
            <MethodImpl(MethodImplOptions.Synchronized)> _
            AddHandler(ByVal value As NameChangedEventHandler)
                m_NameChanged = DirectCast( _
                 [Delegate].Combine(m_NameChanged, value), NameChangedEventHandler)
            End AddHandler
            <MethodImpl(MethodImplOptions.Synchronized)> _
            RemoveHandler(ByVal value As NameChangedEventHandler)
                m_NameChanged = DirectCast( _
                 [Delegate].Remove(m_NameChanged, value), NameChangedEventHandler)
            End RemoveHandler
            RaiseEvent(ByVal sender As Object, ByVal e As NameChangedEventArgs)
                Dim handler As NameChangedEventHandler = m_NameChanged
                If handler IsNot Nothing Then handler(sender, e)
            End RaiseEvent
        End Event

#End Region

#Region " Class-scope declarations "

        ' ID to use as a temporary identifier for a new role; replaced with the
        ' actual database ID when the object is saved to the database
        Private Shared mTempId As Integer

        ''' <summary>
        ''' Gets the next temporary ID for a role object. This will always be a
        ''' negative number
        ''' </summary>
        ''' <returns>A negative number, unique in this application domain, to use as
        ''' a temporary ID for a role object.</returns>
        Private Shared Function GetNextTempId() As Integer
            Return Interlocked.Decrement(mTempId)
        End Function

        ' The roles loaded from the database, mapped against their names.
        Private Shared mRoles As IDictionary(Of String, Role)

        ''' <summary>
        ''' Initialises the static roles using the given data provider.
        ''' </summary>
        ''' <param name="prov">The multiple data provider which contains all of the
        ''' required roles. The provider is expected to contain an integer
        ''' <c>"id"</c> value and a string <c>"name"</c> value.</param>
        Friend Shared Sub Init(ByVal prov As IMultipleDataProvider)
            Dim map As New Dictionary(Of String, Role)
            While prov.MoveNext()
                Dim r As New Role(prov)
                map(r.Name) = r
            End While
            mRoles = GetReadOnly.IDictionary(GetSynced.IDictionary(map))
        End Sub

        ''' <summary>
        ''' Clears the permissions from the held database roles and sets them all
        ''' using the data from the given provider.
        ''' </summary>
        ''' <param name="prov">The provider containing the permission assignments to
        ''' the roles. The provider is expected to contain two string fields, namely
        ''' <c>"rolename"</c>: the name of the role to assign to, and
        ''' <c>"permname"</c>: the name of the perm to assign to the role.</param>
        Friend Shared Sub ClearAndAssign(ByVal prov As IMultipleDataProvider)
            For Each r As Role In mRoles.Values : r.Clear() : Next
            While prov.MoveNext()
                mRoles(prov.GetString("rolename")).Add(prov.GetString("permname"))
            End While
        End Sub

#End Region

#Region " Member Variables "

        ' The associated user group in AD, corresponding to this role
        <DataMember>
        Private mActiveDirectoryGroup As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new role using the specified values
        ''' </summary>
        ''' <param name="id">The ID of this role. This should be a positive integer
        ''' matching the role's record in the database, or a negative integer
        ''' representing a temporary ID until the role is saved to the database.
        ''' </param>
        ''' <param name="name">The name of the role</param>
        ''' <param name="ssoGroup">The name of the Active Directory user group which
        ''' corresponds to this Blue Prism user role, or null if this role has no
        ''' corresponding AD user group</param>
        Private Sub New( _
         ByVal id As Integer, ByVal name As String, ByVal ssoGroup As String)
            MyBase.New(id, name, Feature.None)
            mActiveDirectoryGroup = ssoGroup
        End Sub

        ''' <summary>
        ''' Creates a new role using data from a data provider.
        ''' </summary>
        ''' <param name="prov">The data provider which contains the data for the new
        ''' role. The provider is expected to contain: <list>
        ''' <item>an integer <c>"id"</c> value;</item>
        ''' <item>a string <c>"name"</c> value; and</item>
        ''' <item>a string <c>"ssogroup"</c> value (which may be null or empty if the
        ''' role has no SSO group associated with it)</item>
        ''' </list> 
        ''' </param>
        Friend Sub New(ByVal prov As IDataProvider)
            MyBase.New(prov)
            mActiveDirectoryGroup = prov.GetString("ssogroup")
        End Sub

        ''' <summary>
        ''' Creates a new role with a temporary ID and a specified name.
        ''' </summary>
        ''' <param name="name">The name of the role to create.</param>
        Public Sub New(ByVal name As String)
            Me.New(GetNextTempId(), name, Nothing)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the Active Directory user group which corresponds to this
        ''' Blue Prism user role. An empty string indicates that no corresponding
        ''' user group is associated with this role.
        ''' </summary>
        Public Property ActiveDirectoryGroup() As String
            Get
                If mActiveDirectoryGroup Is Nothing Then Return ""
                Return mActiveDirectoryGroup
            End Get
            Set(ByVal value As String)
                mActiveDirectoryGroup = value
            End Set
        End Property

        ''' <summary>
        ''' Checks if this role is the system admin role.
        ''' </summary>
        Public ReadOnly Property SystemAdmin() As Boolean
            Get
                Return (Name = Role.DefaultNames.SystemAdministrators)
            End Get
        End Property


        ''' <summary>
        ''' Checks if this role is the runtime resource role.
        ''' </summary>
        Public ReadOnly Property RuntimeResource() As Boolean
            Get
                Return (Name = Role.DefaultNames.RuntimeResources)
            End Get
        End Property

        ''' <summary>
        ''' Checks if this roles active directory group can be changed
        ''' </summary>
        Public ReadOnly Property CanChangeActiveDirectoryGroup() As Boolean
            Get
                'Currently all roles are allowed to have their ad group changed
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Checks if this roles permissions can be changed.
        ''' </summary>
        Public ReadOnly Property CanChangePermissions() As Boolean
            Get
                Return _
                Not SystemAdmin AndAlso
                Not RuntimeResource
            End Get
        End Property

        ''' <summary>
        ''' Checks if this role can be deleted.
        ''' </summary>
        Public ReadOnly Property CanDelete() As Boolean
            Get
                Return _
                Not SystemAdmin AndAlso
                Not RuntimeResource
            End Get
        End Property

        ''' <summary>
        ''' Checks if this role can be renamed.
        ''' </summary>
        Public ReadOnly Property CanRename() As Boolean
            Get
                Return _
                Not SystemAdmin AndAlso
                Not RuntimeResource
            End Get
        End Property

        ''' <summary>
        ''' Checks if this role has a temporary ID or not. If it has, then it
        ''' suggests that it does not yet exist on the database.
        ''' </summary>
        Friend ReadOnly Property HasTemporaryId() As Boolean
            Get
                Return (Id < 0)
            End Get
        End Property

        ''' <summary>
        ''' The ID of the role this one was copied from. Only relevant for roles
        ''' which have been copied from another in the UI
        ''' </summary>
        <DataMember>
        Public Property CopiedFromRoleID() As Integer

#End Region

#Region " Methods "

        ''' <summary>
        ''' Creates a deep clone of this role and returns it.
        ''' </summary>
        ''' <returns>A full clone of this role which can be modified without
        ''' affecting this role.</returns>
        Public Function CloneRole() As Role
            Return DirectCast(Clone(), Role)
        End Function

        ''' <summary>
        ''' Checks if the given object is equal to this role.
        ''' </summary>
        ''' <param name="obj">The object to test for equality.</param>
        ''' <returns>True if the given object is a non-null role with the same values
        ''' as this role.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim r As Role = TryCast(obj, Role)
            Return EqualsApartFromADGroup(r) _
             AndAlso ActiveDirectoryGroup = r.ActiveDirectoryGroup
        End Function

        ''' <summary>
        ''' Checks if the attributes (other than the assigned Active Directory group)
        ''' of the given role are equal to this role.
        ''' </summary>
        ''' <param name="r">The role to test for equality.</param>
        ''' <returns>True if the given object is a non-null role with the same values
        ''' as this role (ignoring assigned Active Directory group).</returns>
        Public Function EqualsApartFromADGroup(r As Role) As Boolean
            Return MyBase.Equals(r)
        End Function

        ''' <summary>
        ''' Gets a string representation of this role
        ''' </summary>
        ''' <returns>A string representation of this role, detailing its ID, Name,
        ''' SSO Group (if any) and permissions</returns>
        Public Overrides Function ToString() As String
            Return String.Format(My.Resources.Role_RoleId0Name123,
             Id,
             Name,
             IIf(ActiveDirectoryGroup = "", "", " (" & ActiveDirectoryGroup & ")"),
             CollectionUtil.Join(Permissions, ",")
            )
        End Function

        ''' <summary>
        ''' Gets the permission changes which have occurred from this role to another
        ''' role.
        ''' </summary>
        ''' <param name="other">The role to compare this role to and to determine
        ''' what permission changes would need to be applied to this role in order
        ''' to match the given role's permissions.</param>
        ''' <returns>The perm differences going from this role to
        ''' <paramref name="other"/>. Any additions are prefixed with a "+"
        ''' character, any removals are prefixed with a "-" character. Any perms
        ''' which are present in both this role and <paramref name="other"/> are not
        ''' reported.</returns>
        Public Function GetPermissionChanges(other As Role) As String
            Dim sb As New StringBuilder()
            AppendPermissionChanges(other, sb)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Appends the permission changes which have occurred from this role to
        ''' another role.
        ''' </summary>
        ''' <param name="other">The role to compare this role to and to determine
        ''' what permission changes would need to be applied to this role in order
        ''' to match the given role's permissions.</param>
        ''' <param name="sb">The buffer to which the perm differences should be
        ''' appended. On exit of this method, the buffer will be appended with the
        ''' perm differences going from this role to <paramref name="other"/>. Any
        ''' additions are prefixed with a "+" character, any removals are prefixed
        ''' with a "-" character. Any perms which are present in both this role and
        ''' <paramref name="other"/> are not reported.</param>
        Public Sub AppendPermissionChanges(other As Role, sb As StringBuilder)
            ' Flag to indicate whether we should be appending a comma before a perm
            ' that has been added/removed
            Dim first As Boolean = True

            ' Append the 'added' perm names, prefixed with '+'
            Dim added As New HashSet(Of String)
            For Each p As Permission In other
                If Not Me.Contains(p) Then added.Add(p.Name)
            Next
            If added.Count <> 0 Then
                For Each nm As String In added
                    If first Then first = False Else sb.Append(","c)
                    sb.Append("+'").Append(nm).Append("'")
                Next
            End If

            ' Append the 'removed' perm names, prefixed with '-'
            Dim removed As New HashSet(Of String)
            For Each p As Permission In Me
                If Not other.Contains(p) Then removed.Add(p.Name)
            Next
            If removed.Count <> 0 Then
                For Each nm As String In removed
                    If first Then first = False Else sb.Append(","c)
                    sb.Append("-'").Append(nm).Append("'")
                Next
            End If
        End Sub

        Public Function ShouldBeDisplayed(availablePermissions As ICollection(Of GroupTreePermission)) As Boolean
            If SystemAdmin OrElse
               RuntimeResource OrElse
               Permissions.Intersect(availablePermissions.Select(Function(x) x.Perm)).Count = 0 Then

                Return False
            End If

            Return True
        End Function

#End Region

#Region " Default Role Name Constants "

        ''' <summary>
        ''' Inner class to define constants for the default role names
        ''' </summary>
        Public Class DefaultNames
            Public Const AlertSubscribers = "Alert Subscribers"
            Public Const Developers = "Developers"
            Public Const ProcessAdministrators = "Process Administrators"
            Public Const RuntimeResources = "Runtime Resources"
            Public Const ScheduleManagers = "Schedule Managers"
            Public Const SystemAdministrators = "System Administrators"
            Public Const Testers = "Testers"
        End Class
    End Class

#End Region

End Namespace
