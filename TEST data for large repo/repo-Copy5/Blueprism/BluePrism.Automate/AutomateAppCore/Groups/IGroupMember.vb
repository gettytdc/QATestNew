Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Data
Imports BluePrism.Images
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth

Namespace Groups

    Public Interface IGroupMember : Inherits IComparable

        ''' <summary>
        ''' The ID of the member which this entry in the group represents
        ''' </summary>
        ''' <remarks>This is typically a GUID, String or Integer; if it ever extends
        ''' to anything else, it should be ensured that the type is Serializable.
        ''' </remarks>
        Property Id As Object

        ''' <summary>
        ''' The owner of this group member
        ''' </summary>
        Property Owner As IGroup


        ''' <summary>
        ''' The effective permissions for this member.
        ''' </summary>
        ''' <returns></returns>
        Property Permissions As IMemberPermissions

        ''' <summary>
        ''' The name of the member which this entry in the group represents.
        ''' Note that this can never be null.
        ''' </summary>
        Property Name As String

        ''' <summary>
        ''' Gets the raw member associated with this group member. If this member is
        ''' operating within a filter context, this will be the member instance in
        ''' the raw tree with no filtering applied.
        ''' </summary>
        ReadOnly Property RawMember As GroupMember

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        ReadOnly Property MemberType As GroupMemberType

        ''' <summary>
        ''' Indicates whether or not this group member is a group
        ''' </summary>
        ReadOnly Property IsGroup As Boolean

        ReadOnly Property IsPool As Boolean

        ReadOnly Property IsMember As Boolean

        ''' <summary>
        ''' Indicates if this group member represents the root of a tree or not. Note
        ''' that a disassociated group (ie. a group with no owner) <em>is not</em> a
        ''' root, according to the rules of this property; a root of a tree is the
        ''' group that is returned when <see cref="GroupTree.Root"/> is called, and
        ''' is a specific type, not a group in a specific state.
        ''' </summary>
        ''' <returns>False by default, True if this group is a tree root.</returns>
        ReadOnly Property IsRoot As Boolean

        ''' <summary>
        ''' Gets whether this group member represents a retired member or not.
        ''' By default, no member is retired, but subclasses may vary.
        ''' </summary>
        ReadOnly Property IsRetired As Boolean

        ''' <summary>
        ''' Gets whether this group member represents a locked member or not.
        ''' By default, no member is locked.
        ''' </summary>
        ReadOnly Property IsLocked As Boolean

        ''' <summary>
        ''' Gets whether this group member is in the backing model or not
        ''' </summary>
        ReadOnly Property IsInModel As Boolean

        ''' <summary>
        ''' Gets a dependency with which references to this group member can be
        ''' searched for, or null if this group member cannot be searched for
        ''' dependencies.
        ''' </summary>
        ReadOnly Property Dependency As AutomateProcessCore.clsProcessDependency

        ''' <summary>
        ''' The image key to use for this group member in <see cref="IItemHeader">
        ''' Item Header</see> usage; by default this is just the ImageKey of this
        ''' member, but it may be overridden by subclasses.
        ''' </summary>
        ''' <remarks>Note that, while the <see cref="ImageKey"/> and
        ''' <see cref="ImageKeyExpanded"/> keys must reference the components list
        ''' defined in <see cref="ImageLists.Components_16x16"/> or
        ''' <see cref="ImageLists.Components_32x32"/>, this image key is dependent on
        ''' the image list set within the context of the item info being displayed.
        ''' </remarks>
        ReadOnly Property ImageKey As String

        ''' <summary>
        ''' Gets the image key for this group member if it is expanded. Only really
        ''' applies to groups at the moment, but this may have some semantic meaning
        ''' for other elements in the future (models within objects, for instance).
        ''' </summary>
        ReadOnly Property ImageKeyExpanded As String

        ''' <summary>
        ''' Gets the tree that this group member forms a part of
        ''' </summary>
        ReadOnly Property Tree As IGroupTree

        ''' <summary>
        ''' Resets the locked state of this group member. Does nothing if this member
        ''' has no 'locked' concept or if it is not currently locked.
        ''' </summary>
        Sub ResetLock()

        ''' <summary>
        ''' Appends data to this group member from the given provider.
        ''' This is used in situations where a group member has several rows returned
        ''' for a single member - eg. where a member has a one-to-many relationship
        ''' with a different set of data. The situation which caused this to be
        ''' invented was for a <see cref="UserGroupMember"/> which has a list of
        ''' associated user roles defined in a different table.
        ''' </summary>
        ''' <param name="prov">The provider from which to append the data.</param>
        Sub AppendFrom(prov As IDataProvider)

        ''' <summary>
        ''' Checks if this member is part of a specific tree. This will check the
        ''' source tree that this group member resides in to see if it is the same
        ''' source tree as the given argument.
        ''' </summary>
        ''' <param name="gt">The tree to check to see if this member resides in it.
        ''' </param>
        ''' <returns>True if this member is in the given tree, false otherwise. Note
        ''' that if this member was in the given tree and has been removed, thus
        ''' being orphaned from any tree, this will return false.</returns>
        ''' <remarks>Note that it is enough for the trees to be of the same type for
        ''' them to be considered equal - ie. if this member is in a tree of the
        ''' same <see cref="IGroupTree.TreeType">tree type</see> as
        ''' <paramref name="gt"/>, this will return true</remarks>
        Function IsInTree(gt As IGroupTree) As Boolean

        ''' <summary>
        ''' Creates an orphaned deep clone of this group member
        ''' </summary>
        ''' <returns>A clone of this group member with no owner. Note that if this
        ''' member is associated with other members (eg. it's a group), those
        ''' <em>will</em> be deep cloned in the returned member.</returns>
        ''' <remarks>Note that any associated data (ie. data referenced via the
        ''' <see cref="Data"/> property) is not deep cloned by this implementation.
        ''' If necessary, this task should be performed by subclasses which know if
        ''' the data is value/immutable/cloneable etc.</remarks>
        Function CloneOrphaned() As IGroupMember

        ''' <summary>
        ''' Gets a filtered view of this group member, using the given filtering
        ''' group tree to provide the filtering context for this group member.
        ''' </summary>
        ''' <param name="tree">The filtering tree which the filtered view of the
        ''' member should form part of</param>
        ''' <returns>A clone of this group member which operates within the context
        ''' of the filtered tree: <paramref name="tree"/>.</returns>
        Function GetFilteredView(tree As IFilteringGroupTree) As IGroupMember

        ''' <summary>
        ''' Removes this group member from its owning group. It will effectively be
        ''' deleted as a group entry at this point.
        ''' </summary>
        Sub Remove()

        ''' <summary>
        ''' Moves this group member into the given group.
        ''' </summary>
        ''' <param name="targetGroup">The group to move this member to</param>
        ''' <returns>The member, after moving to the target group, or null if the
        ''' member was not moved for any reason (eg. member already exists in
        ''' target group).</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="targetGroup"/>
        ''' is null.</exception>
        Function MoveTo(targetGroup As IGroup) As IGroupMember

        ''' <summary>
        ''' Copies this group member into the given group
        ''' </summary>
        ''' <param name="targetGroup">The group into which this member should be
        ''' copied.</param>
        ''' <returns>The copy of the member after being added to the target group
        ''' or null if the copy was not performed (typically if attempting to copy
        ''' to the same group that the member is already within).</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="targetGroup"/>
        ''' is null.</exception>
        Function CopyTo(targetGroup As IGroup) As IGroupMember

        ''' <summary>
        ''' Deletes this group member from the model.
        ''' </summary>
        ''' <remarks>
        ''' This removes it from all groups thereby moving it into the root of the
        ''' tree, then deletes it from the root of the tree. Note that, except for
        ''' groups, this will not affect the actual object that the group member
        ''' represents (eg. the process, queue, resource, etc) so, unless that object
        ''' is also deleted, the next time the tree is loaded from the database, a
        ''' group member will be reinstated to represent it.
        ''' In the case of a group, that *is* actually deleted from the backing store
        ''' as well as the model, so it will not be reinstated when the tree loads
        ''' itself from the database again.
        ''' </remarks>
        Sub Delete()

        ''' <summary>
        ''' Checks if the given user has the permission to view this group
        ''' </summary>
        ''' <param name="user">the user in question</param>
        ''' <returns>true if the user has permission</returns>
        Function HasViewPermission(user As IUser) As Boolean

        ''' <summary>
        ''' Checks if this member can be removed from it's group by the passed user.
        ''' </summary>
        ''' <param name="user">The user intending to remove it</param>
        ''' <returns>True if the operation is allowed, otherwise False</returns>
        Function CanBeRemovedFromGroup(user As IUser) As Boolean

        ''' <summary>
        ''' Extracts a localised name, only if the type is correct.  Otherwise the name is simply returned
        ''' </summary>
        ''' <param name="localiser">localisation function</param>
        ''' <returns>Display string</returns>
        Function GetLocalisedName(localiser As Func(Of IGroupMember, String)) As String

        ''' <summary>
        ''' Clear any local cached data used by the group object.
        ''' </summary>
        Sub ClearLocalGroupCache()

    End Interface

End Namespace
