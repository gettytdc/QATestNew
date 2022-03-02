
Imports System.Runtime.Serialization
Imports BluePrism.Images
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Auth
Imports System.Windows.Forms
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' Represents group nodes with the tree structure
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", IsReference:=True)>
    <KnownType("GetAllKnownTypes")>
    Public Class Group : Inherits GroupMember : Implements IGroup

#Region " Class-scope Declarations "


        ''' <summary>
        ''' Gets the known types which are in use in this class.
        ''' </summary>
        ''' <returns>An enumerable of known types used in this group and its contents
        ''' </returns>
        Public Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
            Return GroupTree.GetAllKnownTypes()
        End Function


        ''' <summary>
        ''' Class used to lock and release a group member for renaming.
        ''' </summary>
        Private Class GroupMemberRenameLock : Implements IDisposable
            Private mGroup As Group
            Private mMember As IGroupMember
            Public Sub New(gp As Group, mem As IGroupMember)
                If gp Is Nothing Then Throw New ArgumentNullException(NameOf(gp))
                If mem Is Nothing Then Throw New ArgumentNullException(NameOf(mem))
                If Not gp.mContents.Remove(mem) Then Throw New NoSuchElementException(
                 "The group '{0}' does not contain the member: {1}",
                 gp.Name, mem.Name)
                mGroup = gp
                mMember = mem
            End Sub
            Sub Dispose() Implements IDisposable.Dispose
                mGroup.Add(mMember)
            End Sub
        End Class

#End Region

#Region " Published Events "

        ''' <summary>
        ''' Event fired when the contents of a group is updated.
        ''' </summary>
        <NonSerialized>
        Public Event GroupContentsUpdated As EventHandler

#End Region

#Region " Member Variables "

        'The collection of nodes belonging to this group
        <DataMember(Name:="con")>
        Private mContents As New clsSortedSet(Of IGroupMember)

        <DataMember(Name:="chi", EmitDefaultValue:=False)>
        Private mContainsHiddenItems As Boolean

        <DataMember(Name:="mid", EmitDefaultValue:=False)>
        Private mIsDefault As Boolean

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new empty group with no defined tree type
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

        ''' <summary>
        ''' Creates an empty group except for the 'IsDefault' property
        ''' </summary>
        ''' <param name="isDefault">If this group is the default group in the tree</param>
        Public Sub New(isDefault As Boolean)
            Me.New(NullDataProvider.Instance)
            mIsDefault = isDefault
        End Sub

        ''' <summary>
        ''' Creates a new group with provided data.
        ''' </summary>
        ''' <param name="prov">The provider of the data which should be used to
        ''' populate this group</param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
            mIsDefault = prov.GetBool("isdefault")
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the raw (ie. unfiltered) group that is backing this group. For
        ''' unfiltered (ie. raw) groups, this property will return the instance
        ''' itself.
        ''' </summary>
        Private ReadOnly Property RawGroup As Group Implements IGroup.RawGroup
            Get
                Return Me
            End Get
        End Property

        ''' <summary>
        ''' Gets the image key for a group
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                If Me.Permissions.State = PermissionState.Restricted OrElse
                    Me.Permissions.State = PermissionState.RestrictedByInheritance Then
                    Return ImageLists.Keys.Component.ClosedGroup
                Else
                    Return ImageLists.Keys.Component.ClosedGlobalGroup
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets the 'expanded' image key for a group
        ''' </summary>
        Public Overrides ReadOnly Property ImageKeyExpanded As String
            Get
                If Me.Permissions.State = PermissionState.Restricted OrElse
                    Me.Permissions.State = PermissionState.RestrictedByInheritance Then
                    Return ImageLists.Keys.Component.OpenGroup
                Else
                    Return ImageLists.Keys.Component.OpenGlobalGroup
                End If
            End Get
        End Property

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Group
            End Get
        End Property

        ''' <summary>
        ''' The type of tree that this group is part of, or
        ''' <see cref="GroupTreeType.None"/> if this group is not part of a tree.
        ''' </summary>
        Public ReadOnly Property TreeType() As GroupTreeType _
         Implements IGroup.TreeType
            Get
                Dim t = Me.Tree
                Return If(t Is Nothing, GroupTreeType.None, t.TreeType)
            End Get
        End Property

        ''' <summary>
        ''' The linking table between nodes of this type and groups. In this case,
        ''' the table is <c>BPAGroupGroup</c>.
        ''' </summary>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupGroup"
            End Get
        End Property

        ''' <summary>
        ''' The number of members directly held by this group
        ''' </summary>
        Public ReadOnly Property Count As Integer Implements IGroup.Count
            Get
                Return mContents.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this collection is readonly or not. It is not.
        ''' </summary>
        Private ReadOnly Property IsReadOnly As Boolean Implements IGroup.IsReadOnly
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether or not this node is a group
        ''' </summary>
        Public Overrides ReadOnly Property IsGroup As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property IsMember As Boolean Implements IGroupMember.IsMember
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets the supported member types by this group or, more precisely, in the
        ''' tree that this group resides in. If this group does not reside within a
        ''' tree, this will return an empty collection.
        ''' </summary>
        Public ReadOnly Property SupportedTypes As ICollection(Of GroupMemberType) _
         Implements IGroup.SupportedTypes
            Get
                Dim defn As TreeDefinitionAttribute = TreeType.GetTreeDefinition()
                If defn Is Nothing _
                 Then Return GetEmpty.ICollection(Of GroupMemberType)()

                Return defn.SupportedMemberTypes
            End Get
        End Property

        ''' <summary>
        ''' Is this group the default group for the tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsDefault As Boolean _
            Implements IGroup.IsDefault
            Get
                Return mIsDefault
            End Get
        End Property

        ''' <summary>
        ''' Does this group contain hidden members the user cannot access.
        ''' </summary>
        ''' <returns></returns>
        Public Property ContainsHiddenMembers As Boolean _
            Implements IGroup.ContainsHiddenMembers
            Get
                Return mContainsHiddenItems
            End Get
            Set(value As Boolean)
                mContainsHiddenItems = value
            End Set
        End Property

        Public Property SortFieldName As String Implements IGroup.SortFieldName

        Public Property SortOrder As SortOrder Implements IGroup.SortOrder

        <DataMember(Name:="exp")>
        Public Property Expanded As Boolean Implements IGroup.Expanded

#End Region

#Region " Methods "

        ''' <summary>
        ''' Gets a filtered view of this group member, using the given filtering
        ''' group tree to provide the filtering context for this group member.
        ''' </summary>
        ''' <param name="filtree">The filtering tree which the filtered view of the
        ''' member should form part of</param>
        ''' <returns>A clone of this group member which operates within the context
        ''' of the filtered tree: <paramref name="filtree"/>.</returns>
        Public Overrides Function GetFilteredView(filtree As IFilteringGroupTree) _
         As IGroupMember
            Return New FilteringGroup(filtree, Me)
        End Function

        ''' <summary>
        ''' Checks if this group contains the given group member.
        ''' </summary>
        ''' <param name="mem">The member to check this group for</param>
        ''' <returns>True if this group contains the given member, false otherwise.
        ''' </returns>
        Public Function Contains(mem As IGroupMember) As Boolean _
         Implements IGroup.Contains
            Return mContents.Contains(mem)
        End Function


        ''' <summary>
        ''' Checks if this group contains any of the given group members.
        ''' </summary>
        ''' <param name="mems">The members to check this group for</param>
        ''' <returns>True if this group contains the any of the given members, false
        ''' otherwise. Note that passing an empty collection will return false.
        ''' </returns>
        Public Function ContainsAny(mems As IEnumerable(Of GroupMember)) As Boolean
            For Each mem As GroupMember In mems
                If Contains(mem) Then Return True
            Next
            Return False
        End Function

        ''' <summary>
        ''' Gets the member from this group of the given type and name, or null if
        ''' there is no such member in this group.
        ''' </summary>
        ''' <param name="type">The type of member required</param>
        ''' <param name="name">The name of the member required</param>
        ''' <returns>The group member with the specified type and name, or null if
        ''' no such member exists in this group</returns>
        Private Function GetMember(type As GroupMemberType, name As String) As GroupMember
            Return GetMemberFrom(Me.OfType(Of GroupMember), type, name)
        End Function

        Private Shared Function GetMemberFrom(members As IEnumerable(Of GroupMember), type As GroupMemberType, name As String) As GroupMember
            Return members.FirstOrDefault(Function(mem) mem.MemberType = type AndAlso mem.Name = name)
        End Function


        ''' <summary>
        ''' Moves a group member from this group into another group.
        ''' </summary>
        ''' <param name="mem">The member to move from this group</param>
        ''' <param name="target">The target group to which the member should be moved
        ''' </param>
        ''' <param name="isCopy">Indicates that the member is a copy</param>
        ''' <returns>The member, after moving to the target group, or null if the
        ''' member was not moved for any reason (eg. member already exists in
        ''' target group).</returns>
        ''' <exception cref="ArgumentNullException">If any of <paramref name="mem"/>
        ''' or <paramref name="target"/> is null.</exception>
        ''' <exception cref="NoSuchElementException">If the given member is not a
        ''' member of this group.</exception>
        Friend Function MoveMember(mem As GroupMember, target As Group, isCopy As Boolean) _
         As GroupMember
            If mem Is Nothing Then Throw New ArgumentNullException(NameOf(mem))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            ' If we don't own it, don't do anything
            If Not Contains(mem) Then Throw New NoSuchElementException(
                "This group '{0}' does not contain the member '{1}'", Name, mem.Name)

            ' Nothing to do if we're moving to ourselves
            If target.Equals(Me) Then Return mem

            ' Can't move default group to anywhere but root node.
            Dim group = TryCast(mem, IGroup)
            If group IsNot Nothing AndAlso group.IsDefault AndAlso Not target.IsRoot Then
                Return Nothing
            End If

            ' If it already exists, we want to raise an error. We can't reliably do
            ' the right thing here (eg. for a process it seems reasonably to remove
            ' the old entry; for a group, not so much).
            Dim existing = target.GetMember(mem.MemberType, mem.Name)
            If existing IsNot Nothing Then Throw New AlreadyExistsException(
             My.Resources.Group_TheGroup0AlreadyHasA1WithTheName2,
             target.Name, mem.MemberType.GetLocalizedFriendlyName().ToLower(), mem.Name)

            ' Otherwise, notwithstanding any further errors, we're good to go...

            ' First handle the store-bound entries
            Dim temp As IGroupMember = mem
            Store.MoveTo(Me, target, temp, isCopy)
            mem = CType(temp, GroupMember)

            ' Assuming that's been handled on the store, we just update our own data
            mContents.Remove(mem)

            If Not target.mContents.Add(mem) Then Return Nothing

            ' And set the owner correctly on the member
            mem.Owner = target

            ' If we've moved into the root, we want to ensure we're not present in
            ' any other group
            If target.IsRoot Then mem.RemoveFromAllGroups()

            Return mem

        End Function

        ''' <summary>
        '''  Can this group be deleted
        ''' </summary>
        ''' <returns></returns>
        Public Function CanDeleteGroup(ByRef reason As String) As Boolean _
            Implements IGroup.CanDeleteGroup

            reason = String.Empty

            If IsDefault Then
                reason = My.Resources.CannotDeleteDefaultGroup
                Return False
            End If

            If IsInRoot() Then
                If Me.Any() Then
                    reason = My.Resources.RemoveAllItems
                    Return False
                End If
            End If

            If ContainsHiddenMembers Then
                reason = My.Resources.HiddenItemsInGroup
                Return False
            End If

            ' Must have manage/edit access rights on this group to delete it
            Dim accessRightsOrEditPermission = Permissions.HasPermission(User.Current, Tree.TreeType.GetTreeDefinition().AccessRightsPermission) OrElse
                                  Permissions.HasPermission(User.Current, Tree.TreeType.GetTreeDefinition().EditPermission)

            If Permissions.IsRestricted AndAlso Not accessRightsOrEditPermission Then
                reason = My.Resources.NoPermToDelete
                Return False
            End If

            ' Must have manage access rights on all direct subgroups.
            If Any(Function(x) x.IsGroup AndAlso x.Permissions.IsRestricted AndAlso Not accessRightsOrEditPermission) Then
                reason = My.Resources.NoPermDeleteSubgroups
                Return False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Can access rights be changed for this group
        ''' </summary>
        ''' <param name="reason">If the rights cannot be changed, this returns the reason why</param>
        ''' <returns>True if the rights can be changed, otherwise False</returns>
        Public Function CanChangeAccessRights(ByRef reason As String) As Boolean Implements IGroup.CanChangeAccessRights
            reason = String.Empty

            If ContainsHiddenMembers Then
                reason = My.Resources.CannotModifyAccessRights
                Return False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Copies the given group member from this group to the target group.
        ''' </summary>
        ''' <param name="mem">The member to copy</param>
        ''' <param name="target">The group to copy the member to</param>
        ''' <returns>The copy of the member after being added to the target group
        ''' or null if the copy was not performed (typically if attempting to copy
        ''' to the same group that the member is already within).</returns>
        ''' <exception cref="ArgumentNullException">If any of <paramref name="mem"/>
        ''' or <paramref name="target"/> is null.</exception>
        ''' <exception cref="InvalidArgumentException">If this group or the target
        ''' group is the <see cref="IsRoot">root</see> of the tree.</exception>
        ''' <exception cref="NoSuchElementException">If the given member is not a
        ''' member of this group.</exception>
        Friend Function CopyMember(mem As GroupMember, target As Group) As GroupMember
            ' Sanity checks
            If mem Is Nothing Then Throw New ArgumentNullException(NameOf(mem))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            If IsRoot OrElse target.IsRoot Then Throw New InvalidArgumentException(
                My.Resources.Group_CannotCopyGroupMembersFromOrToTheRootGroup)
            If Not Contains(mem) Then Throw New NoSuchElementException(
                My.Resources.Group_ThisGroup0DoesNotContainTheMember1, Name, mem.Name)

            ' Nothing to do if we're copying to ourselves
            If target.Equals(Me) Then Return Nothing

            ' Create a copy of the member to copy
            Dim clone As GroupMember = DirectCast(mem.CloneOrphaned(), GroupMember)

            ' Put it in the root group to create it (client-side only - it ensures
            ' that the clone has a tree/store etc, and it has no direct entry in the
            ' linking table on the database so it's a nice base to work from)
            Dim rootGp As Group = DirectCast(RootGroup(), Group)
            rootGp.mContents.Add(clone)
            clone.Owner = rootGp

            ' And then move it to the target group.
            ' This does the actual updating of the store with the data, including
            ' the adding of the new entries and the assignment to the group.
            Return rootGp.MoveMember(clone, target, True)

        End Function

        ''' <summary>
        ''' Gets the group member directly in this group with the given name.
        ''' </summary>
        ''' <param name="name">The name of the member required</param>
        ''' <returns>The group member associated with the given name, or null if no
        ''' such member exists in this group</returns>
        Public Function GetMember(name As String) As GroupMember
            For Each mem As GroupMember In Me
                If mem.Name = name Then Return mem
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Raises the <see cref="GroupContentsUpdated"/> event.
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        Protected Overridable Sub OnGroupContentsUpdated(e As EventArgs)
            RaiseEvent GroupContentsUpdated(Me, e)
        End Sub

        ''' <summary>
        ''' Adds the given member to this group
        ''' </summary>
        ''' <param name="member">The member to add to this group</param>
        Public Sub Add(member As IGroupMember) Implements IGroup.Add
            AddRange(GetSingleton.ICollection(member))
        End Sub

        ''' <summary>
        ''' Adds the given collection of base nodes to this group
        ''' </summary>
        ''' <param name="members">The member nodes to add to this group</param>
        Public Sub AddRange(members As IEnumerable(Of IGroupMember)) _
         Implements IGroup.AddRange
            If members Is Nothing Then Throw New ArgumentNullException(
NameOf(members))
            If members.Contains(Nothing) Then Throw New ArgumentNullException(
NameOf(members), "Null group member found in collection")

            Store.AddTo(Me, members)
            mContents.Union(members)
            For Each mem As GroupMember In members : mem.Owner = Me : Next

        End Sub

        ''' <summary>
        ''' Removes the given member from this group, moving it into the root of the
        ''' tree if it is no longer a member of any group.
        ''' </summary>
        ''' <param name="mem">The member to remove</param>
        Public Overloads Function Remove(mem As IGroupMember) As Boolean _
         Implements ICollection(Of IGroupMember).Remove
            Return RemoveAll(GetSingleton.ICollection(mem))
        End Function

        ''' <summary>
        ''' Removes the given members from this group, moving any which are no longer
        ''' assigned to any group into the root of the tree.
        ''' </summary>
        ''' <param name="members">The members to remove from this group (and only
        ''' this group - ie. no recursive search is performed)</param>
        ''' <remarks>As suggested, this is a 'Remove from Group' operation, not a
        ''' 'Delete' operation - see <seealso cref="Delete"/> and its overloads.
        ''' </remarks>
        Public Function RemoveAll(members As IEnumerable(Of IGroupMember)) As Boolean _
         Implements IGroup.RemoveAll
            RemoveAll(members, True)
        End Function

        ''' <summary>
        ''' Removes the given members from this group, optionally moving them to the
        ''' root of the tree if they are no longer members of any group.
        ''' </summary>
        ''' <param name="members">The members to remove for this group</param>
        ''' <param name="moveToRoot">True to check each member and see if they are
        ''' no longer in any group, moving it to the root of the tree in this group's
        ''' data model if that is the case. False to skip that operation and just
        ''' remove each member; effectively a 'Delete'.</param>
        Private Function RemoveAll(
         members As IEnumerable(Of IGroupMember), moveToRoot As Boolean) As Boolean
            Dim t As IGroupTree = Tree
            Store.RemoveFrom(Me, members)
            Dim preCount As Integer = Count
            Dim anyRemoved As Boolean = False
            For Each mem As GroupMember In members
                anyRemoved = mContents.Remove(mem) OrElse anyRemoved
                mem.Owner = Nothing
                ' If there are no more owners of this group member, move it into the
                ' root group and update its owner reference
                If moveToRoot AndAlso t.Root.FindAllOwnersOf(mem).Count = 0 Then
                    t.Root.Add(mem)
                End If
            Next
            Return anyRemoved

        End Function

        ''' <summary>
        ''' Locks a group member owned by this group for rename, ensuring and returns
        ''' a object which releases the member when it is disposed of.
        ''' Under the bonnet, this ensures that the sorted set maintained by this
        ''' group copes with the rename of the member, which is an essential part of
        ''' the sort order of group members. So, while the member is being renamed,
        ''' it takes it out of the sorted set, and once it is complete, it re-enters
        ''' it back in with the new name.
        ''' </summary>
        ''' <param name="mem">The group member, who is a member of this group, that
        ''' should be locked for rename.</param>
        ''' <returns>A disposable object which will release the member after it has
        ''' been renamed.</returns>
        ''' <exception cref="NoSuchElementException">If <paramref name="mem"/> is not
        ''' a member of this group.</exception>
        ''' <remarks>While the member is locked for rename, it will be unavailable
        ''' via this group. In effect, it will no longer be a member of this group
        ''' for the period while it is locked.</remarks>
        Friend Function LockForRename(mem As IGroupMember) As IDisposable
            Return New GroupMemberRenameLock(Me, mem)
        End Function

        ''' <summary>
        ''' Clears all members of this group, moving them to the root of the tree if
        ''' they are no longer members of any group
        ''' </summary>
        Private Sub Clear() Implements IGroup.Clear
            RemoveAll(New List(Of IGroupMember)(mContents), True)
        End Sub

        ''' <summary>
        ''' Creates a new group within this group with the given name
        ''' </summary>
        ''' <param name="newGroupName">The new name for the group</param>
        ''' <returns>The newly created group, associated with this group and its
        ''' owning tree and after creation within the <see cref="Store"/> set within
        ''' them.</returns>
        ''' <exception cref="AlreadyExistsException">If a group with the given name
        ''' already exists within this group.</exception>
        Public Function CreateGroup(newGroupName As String) As IGroup _
         Implements IGroup.CreateGroup
            If IsGroupNameRootName(newGroupName) Then Throw New AlreadyExistsException(
                    My.Resources.Group_TheDefaultRootFolderIsCalled0YouCannotCreateAGroupWithTheSameName,newGroupName)

            Dim groupMember = GetMember(MemberType, newGroupName)
            If groupMember IsNot Nothing Then Throw New AlreadyExistsException(
                My.Resources.Group_TheGroup0AlreadyHasA1WithTheName2WhichYouMayNotHavePermissionToSee,
                Name, MemberType.GetLocalizedFriendlyName().ToLower(), newGroupName)
            
            Dim gp As New Group() With {.Name = newGroupName, .Owner = Me}
            Store.Update(gp)
            Add(gp)
            Return gp
        End Function

        ''' <inheritdoc/>
        Public Sub UpdateGroupName(newGroupName As String) Implements IGroup.UpdateGroupName
            ValidateGroupMembersForRename(newGroupName)
            Dim currentGroup = Me.RawGroup
            Name = newGroupName
            Store.Update(Me)
        End Sub

        Private Sub ValidateGroupMembersForRename(newGroupName As String)
            Dim potentialMatches = DirectCast(Me.Owner, Group)?.OfType(Of GroupMember).Except({Me})
            If potentialMatches?.Count > 0 Then
                If IsGroupNameRootName(newGroupName) Then Throw New AlreadyExistsException(
                    My.Resources.Group_TheDefaultRootFolderIsCalled0YouCannotCreateAGroupWithTheSameName, newGroupName)

                Dim groupMember = GetMemberFrom(potentialMatches, Me.MemberType, newGroupName)
                If groupMember IsNot Nothing Then Throw New AlreadyExistsException(
                    My.Resources.Group_TheGroup0AlreadyHasA1WithTheName2WhichYouMayNotHavePermissionToSee,
                    Owner.Name, MemberType.GetLocalizedFriendlyName().ToLower(), newGroupName)
            End If
        End Sub

        ''' <inheritdoc/>
        Public Sub UpdateGroupName(newGroupName As String,oldGroupName As String) Implements IGroup.UpdateGroupName
            ValidateGroupMembersForRename(newGroupName)
            Dim current = Store.GetGroup(CType(Me.Id,Guid))
            If current.Name <> oldGroupName And newGroupName <> current.Name
                Throw New GroupRenameException($"Group not correct",current.Name)
            End If
            Name = newGroupName
            Store.Update(Me)
        End Sub

        ''' <summary>
        ''' Gets an orphaned clone of this group - ie. a clone of the group's ID,
        ''' name and contents, but with no owner. It's a deep clone in the sense that
        ''' all contents of the group is also cloned and associated with the newly
        ''' cloned group.
        ''' </summary>
        ''' <remarks><para>
        ''' Note that because the cloned group is orphaned, it will not be
        ''' associated with any <see cref="Tree"/>, and thus will have no
        ''' <see cref="RootGroup"/> or <see cref="Store"/> available to it. It will
        ''' need to be appended to another tree in order to inherit those things from
        ''' the tree that it becomes a part of.
        ''' </para><para>
        ''' Also note that the cloned group is not written to the store - ie. the
        ''' store is entirely unaware of it, and will remain so until it is added to
        ''' the tree. As such, any groups within its structure, including the top
        ''' group itself, will have no ID assigned to it.
        ''' </para></remarks>
        Public Overrides Function CloneOrphaned() As IGroupMember
            Dim gm As Group = DirectCast(MyBase.CloneOrphaned(), Group)
            gm.mContents = New clsSortedSet(Of IGroupMember)()
            gm.Id = Nothing
            For Each mem As GroupMember In mContents
                Dim clone = mem.CloneOrphaned()
                clone.Owner = gm
                gm.mContents.Add(clone)
            Next
            Return gm
        End Function

        ''' <summary>
        ''' Deletes this group and its descendant tree, disassociating it from the
        ''' owning tree and releasing all the members within it and its descendants.
        ''' </summary>
        Public Overrides Sub Delete()
            ' Save the path and raw tree before we excise this group from the model
            Dim oldPath = FullPath
            Dim t = Me.RawTree
            ' Delete the group from the backing store

            Dim reason As String = String.Empty
            If Not CanDeleteGroup(reason) Then
                Throw New BluePrismException(reason)
            End If

            ' Move any items in this group into this group's parent before deleting.
            If Owner IsNot Nothing Then
                Dim children = mContents.ToList()
                For Each mem In children
                    MoveMember(mem.RawMember, RawOwner, False)
                Next
            End If

            ' Delete this group from store and model.
            Store.DeleteGroup(Me)
            RawOwner.Delete(Me)

        End Sub

        ''' <summary>
        ''' Deletes the given group member from this group. Note that this has no
        ''' effect on the linked data, only on the group membership data.
        ''' Until the linked data is removed, the store will show the item in the
        ''' root of this tree, but that will not be reflected by this data model.
        ''' </summary>
        ''' <param name="mem">The member to be deleted from this group and the tree.
        ''' </param>
        ''' <seealso cref="Remove"/>
        Public Overloads Sub Delete(mem As IGroupMember)
            DeleteAll(GetSingleton.ICollection(mem))
        End Sub

        ''' <summary>
        ''' Deletes the given group members from this group. Note that this has no
        ''' effect on the linked data, only on the group membership data.
        ''' Until the linked data is removed, the store will show the item in the
        ''' root of this tree, but that will not be reflected by this data model.
        ''' </summary>
        ''' <param name="mems">The members to be deleted from this group and the tree.
        ''' </param>
        ''' <seealso cref="RemoveAll"/>
        Public Sub DeleteAll(mems As ICollection(Of IGroupMember))
            RemoveAll(mems, False)
        End Sub

        ''' <summary>
        ''' Gets an enumerator over the contents of this group
        ''' </summary>
        ''' <returns>An enumerator over the contents of this group</returns>
        Public Function GetEnumerator() As IEnumerator(Of IGroupMember) _
         Implements IEnumerable(Of IGroupMember).GetEnumerator
            Dim query = mContents.Select(Function(m) m).Sort(Me.SortFieldName, Me.SortOrder)
            Return query.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the contents of this group
        ''' </summary>
        ''' <returns>An enumerator over the contents of this group</returns>
        Private Function GetUntypedEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Copies this collection of group members into an array
        ''' </summary>
        ''' <param name="array">The array to copy into </param>
        ''' <param name="arrayIndex">The index within the array to start copying.
        ''' </param>
        Private Overloads Sub CopyTo(array() As IGroupMember, arrayIndex As Integer) _
         Implements ICollection(Of IGroupMember).CopyTo
            mContents.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Function to check if the rootGroupName is the same as the root node
        ''' </summary>
        ''' <param name="newGroupName">Name to check</param>
        ''' <returns>Return is newGroupName is the root</returns>
        Private Function IsGroupNameRootName(newGroupName As String) As Boolean
            Return TreeType.ToString().Equals(newGroupName,StringComparison.InvariantCultureIgnoreCase)
        End Function

        ''' <summary>
        ''' Validate that the state of the group is correct against the server.
        ''' Throw state is invald.  
        ''' Note: This can be expanded if required.
        ''' </summary>
        Public Sub ValidateGroupState() Implements IGroup.ValidateGroupState
            If Not CType(Me.Id, Guid) = Guid.Empty Then
                Dim current = Store.GetGroup(CType(Me.Id, Guid))
                If current.Name <> Me.Name Then
                    Throw New GroupRenameException($"Group in an invalid state", current.Name)
                End If
            End If
        End Sub
#End Region

    End Class
End Namespace
