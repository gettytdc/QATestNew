Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Namespace Auth

    ''' <summary>
    ''' Class to represent a set of roles.
    ''' At least one exists for each environment, representing the current set of
    ''' roles defined within the environment. When editing the roles, a copy of that
    ''' role set is used and finally applied at which point it becomes the current
    ''' set of roles defined in the environment.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class RoleSet : Implements ICollection(Of Role)

#Region " Published Events "

        ''' <summary>
        ''' Function to check if both event handlers (add/remove) are attached
        ''' </summary>
        ''' <returns></returns>
        Public Function HandlersDefined() As Boolean
            Return RoleAddedEvent IsNot Nothing AndAlso RoleRemovedEvent IsNot Nothing
        End Function


        ''' <summary>
        ''' Event fired when a role is added to this roleset.
        ''' </summary>
        ''' <param name="sender">The sender of the event; the roleset which has had a
        ''' role added to it</param>
        ''' <param name="e">The event args detailing the event</param>
        Public Event RoleAdded(ByVal sender As Object, ByVal e As RoleEventArgs)

        ''' <summary>
        ''' Event fired when a role is removed from this roleset.
        ''' </summary>
        ''' <param name="sender">The sender of the event; the roleset which has had a
        ''' role removed from it</param>
        ''' <param name="e">The event args detailing the event</param>
        Public Event RoleRemoved(ByVal sender As Object, ByVal e As RoleEventArgs)

        ''' <summary>
        ''' Event fired when a role is renamed by this roleset.
        ''' </summary>
        ''' <param name="sender">The sender of the event; the roleset which has
        ''' renamed a role</param>
        ''' <param name="e">The event args detailing the event</param>
        Public Event RoleRenamed(ByVal sender As Object, ByVal e As RoleEventArgs)

#End Region

#Region " Member Variables "

        ' A map of the roles held in this set, keyed on their names
        <DataMember>
        Private mRoles As IDictionary(Of String, Role)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new, empty RoleSet.
        ''' </summary>
        Public Sub New()
            Me.New(Nothing)
        End Sub

        ''' <summary>
        ''' Copy constructor - creates a new RoleSet containing a copy of the roles
        ''' in the given roleset.
        ''' </summary>
        ''' <param name="rs">The set of roles from which to draw the roles to
        ''' populate this set. The roles set in this set are actually clones of the
        ''' originals found in this parameter.</param>
        Protected Sub New(ByVal rs As RoleSet)
            If rs IsNot Nothing Then
                For Each r As Role In rs
                    RoleMap(r.Name) = r.CloneRole()
                Next
            End If
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The map of roles held in this set, keyed on the role's name. Note that if
        ''' the role name changes, this map should be updated to ensure that it is
        ''' still held correctly.
        ''' </summary>
        Protected ReadOnly Property RoleMap() As IDictionary(Of String, Role)
            Get
                If mRoles Is Nothing Then mRoles = CreateMap()
                Return mRoles
            End Get
        End Property

        ''' <summary>
        ''' The roles in this set, in arbitrary order.
        ''' </summary>
        Public ReadOnly Property Roles() As ICollection(Of Role)
            Get
                Return RoleMap.Values
            End Get
        End Property

        ''' <summary>
        ''' The permissions in effect for this user.
        ''' </summary>
        Public ReadOnly Property EffectivePermissions() As IBPSet(Of Permission)
            Get
                If CollectionUtil.IsNullOrEmpty(mRoles) Then _
                 Return GetEmpty.IBPSet(Of Permission)()

                Dim perms As New clsSet(Of Permission)
                For Each r As Role In Roles
                    perms.Union(r.Permissions)
                Next
                Return GetReadOnly.ISet(perms)
            End Get
        End Property

        ''' <summary>
        ''' Validates the ActiveDirectory groups in this role set, using
        ''' <see cref="clsServer.ValidateActiveDirectoryGroups"/>
        ''' </summary>
        Public Sub ValidateADGroups()
            Dim reason As String = Nothing
            Dim groups As New List(Of String)
            For Each r As Role In Roles
                If r.ActiveDirectoryGroup <> "" _
                 Then groups.Add(r.ActiveDirectoryGroup)
            Next
            ' If we have no AD groups, then we have nothing to validate.
            If groups.Count = 0 Then Return

            If Not gSv.ValidateActiveDirectoryGroups(groups, reason) Then Throw New ActiveDirectoryConfigException(reason)

        End Sub

        ''' <summary>
        ''' Gets the role with the given name from this roleset, or null if no such
        ''' role exists.
        ''' </summary>
        ''' <param name="name">The <see cref="Role.Name">Role Name</see> of the
        ''' required role.</param>
        Default Public Property Item(ByVal name As String) As Role
            Get
                Dim r As Role = Nothing
                If RoleMap.TryGetValue(name, r) Then Return r
                Return Nothing
            End Get
            Set(ByVal value As Role)
                RoleMap(name) = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the role with the given ID from this roleset, or null if no such
        ''' role exists.
        ''' </summary>
        ''' <param name="id">The <see cref="Role.Id">Role Id</see> of the required
        ''' role.</param>
        Default Public ReadOnly Property Item(ByVal id As Integer) As Role
            Get
                For Each r As Role In Roles
                    If r.Id = id Then Return r
                Next
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Checks if this roleset contains the given role or not.
        ''' </summary>
        ''' <param name="r">The role to test for in this set.</param>
        ''' <returns>True if this role set contains the given role, according to the
        ''' test in <see cref="Role.Equals"/></returns>
        Public Function Contains(ByVal r As Role) As Boolean _
         Implements ICollection(Of Role).Contains
            Return (r IsNot Nothing AndAlso Roles.Contains(r))
        End Function

        ''' <summary>
        ''' Checks if this roleset contains a role with the given ID
        ''' </summary>
        ''' <param name="id">The ID to check</param>
        ''' <returns>True if this roleset contains a role with the given ID; False
        ''' otherwise.</returns>
        Public Function Contains(ByVal id As Integer) As Boolean
            Return (Me(id) IsNot Nothing)
        End Function

        ''' <summary>
        ''' Checks if this roleset contains a role with the given name
        ''' </summary>
        ''' <param name="name">The name of the role to check</param>
        ''' <returns>True if this roleset contains a role with the given name; False
        ''' otherwise</returns>
        Public Function Contains(ByVal name As String) As Boolean
            Return (Me(name) IsNot Nothing)
        End Function

        ''' <summary>
        ''' Checks if this roleset is read only or not. A readonly role set cannot
        ''' be modified in any way.
        ''' </summary>
        Public Overridable ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of Role).IsReadOnly
            Get
                Return False
            End Get
        End Property

#End Region

#Region " Read Methods "

        ''' <summary>
        ''' Provides a deep clone of this role set - note that this method always
        ''' returns an instance of <see cref="RoleSet"/>, even if this object is an
        ''' instance of a subclass (eg. <see cref="SystemRoleSet"/>) - this is to
        ''' allow a cloned readonly role set to be modifiable.
        ''' </summary>
        ''' <returns>A full modifiable deep clone of this role set.</returns>
        Public Overridable Function Clone() As RoleSet
            Dim copy As RoleSet = DirectCast(MemberwiseClone(), RoleSet)
            If mRoles IsNot Nothing Then
                copy.mRoles = Nothing
                For Each r As Role In mRoles.Values
                    copy.RoleMap(r.Name) = r.CloneRole()
                Next
            End If
            Return copy
        End Function

        ''' <summary>
        ''' Provides a RoleSet with clones of this roleset's roles added to it.
        ''' Note that this will always return a concrete instance of
        ''' <see cref="RoleSet"/>, regardless of whether this object is a subclass
        ''' of RoleSet or not.
        ''' </summary>
        ''' <returns>A RoleSet instance containing the same </returns>
        Protected Function Copy() As RoleSet
            Dim rs As New RoleSet()
            If mRoles IsNot Nothing Then
                rs.mRoles = Nothing
                For Each r As Role In mRoles.Values
                    rs.RoleMap(r.Name) = r.CloneRole()
                Next
            End If
            Return rs
        End Function

        ''' <summary>
        ''' Copies the roles in this set into the given array, starting at the
        ''' specified index.
        ''' </summary>
        ''' <param name="array">The array into which this set's roles should be
        ''' copied.</param>
        ''' <param name="arrayIndex">The index in the array at which the copying of
        ''' the roles should begin.</param>
        Public Sub CopyTo(ByVal array() As Role, ByVal arrayIndex As Integer) _
         Implements ICollection(Of Role).CopyTo
            RoleMap.Values.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets a count of the number of roles in this set.
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of Role).Count
            Get
                Return RoleMap.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets an enumerator over the roles in this set
        ''' </summary>
        ''' <returns>An enumerator over this set's roles</returns>
        Private Function GetEnumerator() As IEnumerator(Of Role) _
         Implements IEnumerable(Of Role).GetEnumerator
            Return RoleMap.Values.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the roles in this set
        ''' </summary>
        ''' <returns>An enumerator over this set's roles</returns>
        Private Function GetNonGenericEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

#End Region

#Region " Modify Methods "

        ''' <summary>
        ''' Adds the given role to this role set if a role with its name doesn't
        ''' already exist.
        ''' </summary>
        ''' <param name="r">The role to add</param>
        ''' <returns>True if the role was added to the roleset; False if the given
        ''' role was null, or a role with the given role's name already exists in
        ''' this roleset and, as a result, it was not added.</returns>
        Public Overridable Function Add(ByVal r As Role) As Boolean
            If r Is Nothing Then Return False
            If RoleMap.ContainsKey(r.Name) Then Return False
            RoleMap(r.Name) = r
            OnRoleAdded(New RoleEventArgs(r))
            Return True
        End Function

        ''' <summary>
        ''' Adds the role with the given name to this role set if a role with its
        ''' name doesn't already exist.
        ''' </summary>
        ''' <param name="roleName">The name of the role to add</param>
        ''' <returns>True if a role with the specified name was found in the
        ''' <see cref="SystemRoleSet.Current">Current SystemRoleSet</see> and it was
        ''' then added to the roleset; False if the given role name was not found, or
        ''' a role with the given role's name already exists in this roleset and, as
        ''' a result, it was not added.</returns>
        Public Overridable Function Add(ByVal roleName As String) As Boolean
            Dim rs = SystemRoleSet.Current
            If rs Is Nothing Then Return False
            Dim r = rs(roleName)
            If r Is Nothing Then Return False
            Return Add(r)
        End Function

        ''' <summary>
        ''' Adds all the given roles to this role set.
        ''' </summary>
        ''' <param name="roles">The roles to add</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="roles"/> is
        ''' null.</exception>
        Public Overridable Sub AddAll(roles As IEnumerable(Of Role))
            If roles Is Nothing Then Throw New ArgumentNullException(NameOf(roles))
            For Each r In roles : Add(r) : Next
        End Sub

        ''' <summary>
        ''' Adds the given role to this roleset
        ''' </summary>
        ''' <param name="item">The role to add</param>
        Private Sub ICollAdd(ByVal item As Role) Implements ICollection(Of Role).Add
            Add(item)
        End Sub

        ''' <summary>
        ''' Removes the given role from this roleset, if it matches the one held in
        ''' this roleset with its name.
        ''' </summary>
        ''' <param name="r">The role to remove</param>
        ''' <returns>True if the role matched the one held in this roleset under the
        ''' role's name and it was thus removed; False if either no role with the
        ''' given role's name exists in this set, or the role in this set with that
        ''' name does not match the given role, according to the
        ''' <see cref="Role.Equals"/> method.</returns>
        Public Overridable Function Remove(ByVal r As Role) As Boolean _
         Implements ICollection(Of Role).Remove
            If r Is Nothing Then Return False
            Dim actualRole As Role = Nothing
            If Not RoleMap.TryGetValue(r.Name, actualRole) Then Return False
            If Not r.Equals(actualRole) Then Return False
            If RoleMap.Remove(r.Name) Then
                OnRoleRemoved(New RoleEventArgs(r))
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Removes the role with the given name from this roleset.
        ''' </summary>
        ''' <param name="roleName">The name of the role to remove</param>
        ''' <returns>True if a role with the specified name was found in this roleset
        ''' and therefore removed; False if such a role was not found and therefore
        ''' it was not removed.</returns>
        Public Overridable Function Remove(ByVal roleName As String) As Boolean
            If roleName Is Nothing Then Return False
            Dim r As Role = Nothing
            If RoleMap.TryGetValue(roleName, r) Then Return Remove(r)
            Return False
        End Function

        ''' <summary>
        ''' Renames one of the roles in this role set.
        ''' </summary>
        ''' <param name="oldName">The old name of the role to be removed</param>
        ''' <param name="newName">The new name of the role</param>
        ''' <exception cref="EmptyException">If <paramref name="newName"/> is null or
        ''' empty.</exception>
        ''' <exception cref="AlreadyExistsException">If a role with the name given in
        ''' <paramref name="newName"/> is already present in this set.</exception>
        ''' <exception cref="NoSuchElementException">If no role with the name given
        ''' in <paramref name="oldName"/> exists in this set.</exception>
        Public Overridable Sub Rename(ByVal oldName As String, ByVal newName As String)
            ' Same name - nothing to do
            If oldName = newName Then Return
            If newName = "" Then Throw New EmptyException(
             My.Resources.RoleSet_TheNameOfARoleCannotBeEmpty)

            ' If a role with the intended name already exists, we fail
            If RoleMap.TryGetValue(newName, Nothing) Then _
             Throw New AlreadyExistsException(
              My.Resources.RoleSet_ARoleWithTheName0AlreadyExistsInThisRoleSet, newName)

            ' If a role with the supposed old name is not here, we fail
            Dim r As Role = Nothing
            If Not RoleMap.TryGetValue(oldName, r) Then _
             Throw New NoSuchElementException(
              My.Resources.RoleSet_NoRoleWithTheName0WasFoundInThisRoleSet, oldName)

            ' Work on the role map directly to avoid firing Removed/Added events
            RoleMap.Remove(oldName)
            r.Name = newName
            RoleMap(newName) = r
            OnRoleRenamed(New RoleEventArgs(r))

        End Sub

        ''' <summary>
        ''' Creates a new role with a unique name, adds it to this role set and
        ''' returns it.
        ''' </summary>
        ''' <returns>The newly created role with the unique name</returns>
        Public Overridable Function NewRole() As Role
            Dim namePattern As String = My.Resources.RoleSet_NewRole0
            Dim name As String = Nothing, num As Integer = 0
            Do
                num += 1
                name = String.Format(namePattern, num)
            Loop While RoleMap.ContainsKey(name)
            Dim r As New Role(name)
            RoleMap(name) = r
            OnRoleAdded(New RoleEventArgs(r))
            Return r
        End Function

        ''' <summary>
        ''' Clears all of the roles in this set.
        ''' </summary>
        Public Overridable Sub Clear() Implements ICollection(Of Role).Clear
            ' Create a list of the roles so we can fire the appropriate events
            Dim clearedRoles As New List(Of Role)(Roles)
            RoleMap.Clear()
            For Each r As Role In clearedRoles
                OnRoleRemoved(New RoleEventArgs(r))
            Next
        End Sub

        ''' <summary>
        ''' Sets the System Administrator's AD group, optionally clearing out all the
        ''' others.
        ''' </summary>
        ''' <param name="adminGroup">The System Admin group</param>
        ''' <param name="resetOthers">Set to True to clear down all other group
        ''' mappings</param>
        Public Overridable Sub SetSysAdminADGroup(adminGroup As String, resetOthers As Boolean)
            For Each r In Roles
                If r.SystemAdmin Then
                    r.ActiveDirectoryGroup = adminGroup
                ElseIf resetOthers Then
                    r.ActiveDirectoryGroup = ""
                End If
            Next
        End Sub

#End Region

#Region " Load/Save Methods "

        ''' <summary>
        ''' Loads the roles from the given data provider into this role set.
        ''' </summary>
        ''' <param name="prov">The provider with the data to load the roles from.
        ''' </param>
        Friend Sub LoadRoles(ByVal prov As IMultipleDataProvider)
            While prov.MoveNext()
                Dim r As New Role(prov)
                RoleMap(r.Name) = r
            End While
        End Sub

        ''' <summary>
        ''' Loads the assignments for this role set from the given data provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data for this roleset. This is
        ''' expected to hold the string value <c>"rolename"</c> and the string value
        ''' <c>"permname"</c>, representing the role and permission respectively.
        ''' </param>
        ''' <exception cref="KeyNotFoundException">If any of the rolenames in the
        ''' provider did not match up to the roles which exist in this set.
        ''' </exception>
        Friend Sub LoadAssignments(ByVal prov As IMultipleDataProvider)
            For Each r As Role In Roles : r.Clear() : Next
            While prov.MoveNext()
                RoleMap(prov.GetString("rolename")).Add(prov.GetString("permname"))
            End While
        End Sub

#End Region

#Region " Other Methods "

        ''' <summary>
        ''' Creates the map to be used for this role set.
        ''' </summary>
        ''' <returns>An empty <see cref="Dictionary(Of String,Role)"/> to be used
        ''' to hold the roles in this role set.</returns>
        Protected Overridable Function CreateMap() As IDictionary(Of String, Role)
            Return New Dictionary(Of String, Role)
        End Function

        ''' <summary>
        ''' Raises a <see cref="RoleAdded"/> event.
        ''' </summary>
        ''' <param name="e">The args detailing the added event</param>
        Protected Overridable Sub OnRoleAdded(ByVal e As RoleEventArgs)
            RaiseEvent RoleAdded(Me, e)
        End Sub

        ''' <summary>
        ''' Raises a <see cref="RoleRemoved"/> event.
        ''' </summary>
        ''' <param name="e">The args detailing the added event</param>
        Protected Overridable Sub OnRoleRemoved(ByVal e As RoleEventArgs)
            RaiseEvent RoleRemoved(Me, e)
        End Sub

        ''' <summary>
        ''' Raises a <see cref="RoleRenamed"/> event.
        ''' </summary>
        ''' <param name="e">The args detailing the renamed event.</param>
        Protected Overridable Sub OnRoleRenamed(ByVal e As RoleEventArgs)
            RaiseEvent RoleRenamed(Me, e)
        End Sub

        ''' <summary>
        ''' Compares this roleset to the given roleset, enumerating the changes into
        ''' a report which indicates how this roleset has been modified to match the
        ''' given roleset.
        ''' </summary>
        ''' <param name="rs">The roleset to compare against</param>
        ''' <returns>A string detailing the changes that have been made to this
        ''' roleset to reach the given roleset or "No Change" if the roleset is the
        ''' same as this roleset.</returns>
        Public Function GetChangeReport(ByVal rs As RoleSet) As String

            Dim changed As New Dictionary(Of Role, String)
            Dim deleted As New List(Of Role)
            For Each r As Role In Me
                If Not rs.Contains(r.Id) Then deleted.Add(r) : Continue For
                ' So it's there, check for changes
                Dim other As Role = rs(r.Id)
                ' If they are the same, no changes - carry on
                If r.Equals(other) Then Continue For
                ' Otherwise, figure out what's changed - it can't be the ID, so...
                Dim changeDesc As New StringBuilder()
                If r.Name <> other.Name Then
                    changeDesc.AppendFormat(My.Resources.RoleSet_Name01, r.Name, other.Name)
                End If
                If r.ActiveDirectoryGroup <> other.ActiveDirectoryGroup Then
                    If changeDesc.Length > 0 Then changeDesc.Append(";"c)
                    changeDesc.AppendFormat(My.Resources.RoleSet_ADGroup01,
                     r.ActiveDirectoryGroup, other.ActiveDirectoryGroup)
                End If
                If Not CollectionUtil.AreEquivalent(r, other) Then
                    If changeDesc.Length > 0 Then changeDesc.Append(";"c)
                    changeDesc.Append(My.Resources.RoleSet_Permissions)
                    r.AppendPermissionChanges(other, changeDesc)
                End If
                changed(r) = changeDesc.ToString()
            Next

            Dim added As New List(Of Role)
            For Each r As Role In rs
                If Not Me.Contains(r.Id) Then added.Add(r)
            Next

            If changed.Count = 0 AndAlso deleted.Count = 0 AndAlso added.Count = 0 Then
                Return My.Resources.RoleSet_NoChange
            End If

            Dim sb As New StringBuilder()
            If deleted.Count > 0 Then
                sb.Append(My.Resources.RoleSet_Deleted)
                For Each r As Role In deleted
                    sb.Append("'"c).Append(r.Name).Append("'; ")
                Next
            End If
            If added.Count > 0 Then
                sb.Append(My.Resources.RoleSet_Added)
                For Each r As Role In added
                    sb.Append("'"c).Append(r.Name).Append("'; ")
                Next
            End If
            If changed.Count > 0 Then
                sb.Append(My.Resources.RoleSet_Modified)
                For Each r As Role In changed.Keys
                    sb.AppendFormat("'{0}':[{1}]; ", r.Name, changed(r))
                Next
            End If

            ' Remove the last 2 characters (should be a "; ")
            sb.Remove(sb.Length - 2, 2)
            Return sb.ToString()

        End Function

        ''' <summary>
        ''' Checks if this roleset is equal to the given object. It is equal if the
        ''' object is a non-null RoleSet which contains the same roles as this set.
        ''' </summary>
        ''' <param name="obj">The object to test for equality against.</param>
        ''' <returns>True if the given object is a roleset containing the same roles
        ''' as this role set.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim rs As RoleSet = TryCast(obj, RoleSet)
            If rs Is Nothing Then Return False
            Return CollectionUtil.AreEquivalent(mRoles, rs.mRoles)
        End Function

        ''' <summary>
        ''' Gets an integer hash of this role set - actually just the sum of the
        ''' role Ids in this set.
        ''' </summary>
        ''' <returns>An integer hash of this role set</returns>
        Public Overrides Function GetHashCode() As Integer
            Return Roles.Aggregate(0, Function(accum, r) accum + r.Id)
        End Function

        ''' <summary>
        ''' Gets a string representation of this roleset; that is a comma-separated
        ''' list of the names of the roles in this set, in alpha order.
        ''' </summary>
        ''' <returns>The names of the roles in this role set.</returns>
        Public Overrides Function ToString() As String
            Return "{" &
                String.Join(",", RoleMap.Keys.OrderBy(Function(s) s.ToLower())) &
            "}"
        End Function

#End Region

    End Class

End Namespace
