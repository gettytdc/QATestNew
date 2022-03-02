Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib

Namespace Groups

    ''' <summary>
    ''' The store providing a mechanism from which group information can be retrieved
    ''' and updated.
    ''' </summary>
    Public Interface IGroupStore : Inherits IDisposable

        ''' <summary>
        ''' Gets the tree of the given type, filtered as specified, from the backing
        ''' store.
        ''' </summary>
        ''' <param name="tp">The type denoting which tree to retrieve</param>
        ''' <param name="filter">A filter to run on the tree contents before
        ''' returning it.</param>
        ''' <param name="groupFilter">Filter for groups specifically.</param>
        ''' <param name="fullReload">Determines whether to skip any cached tree and 
        ''' retrieve it from the backing store</param>
        ''' <param name="poolsAsGroups">Show pools as groups or as resources</param>
        ''' <returns>The group tree of the given type from the database.</returns>
        ''' <remarks>This is not guaranteed to be an instance of
        ''' <see cref="GroupTree"/>, so it should not be treated as such.</remarks>
        Function GetTree(tp As GroupTreeType, filter As Predicate(Of IGroupMember),
                         groupFilter As Predicate(Of IGroup), fullReload As Boolean,
                         poolsAsGroups As Boolean, getRetired As Boolean) As IGroupTree

        ''' <summary>
        ''' Refreshes the given group tree, getting the latest data from the backing
        ''' store.
        ''' </summary>
        ''' <param name="tree">The tree to refresh</param>
        Sub Refresh(tree As IGroupTree)

        ''' <summary>
        ''' Updates the given group's metadata (ie. not its contents), modifying its
        ''' ID and name as appropriate.
        ''' </summary>
        ''' <param name="gp">The group to update</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="gp"/> is null.
        ''' </exception>
        Sub Update(gp As IGroup)

        ''' <summary>
        ''' Adds a group member to a group within the database.
        ''' </summary>
        ''' <param name="gp">The group to which the member should be added.</param>
        ''' <param name="mem">The member to be added to the group</param>
        Sub AddTo(gp As IGroup, mem As IGroupMember)

        ''' <summary>
        ''' Adds group members to a group within the database.
        ''' </summary>
        ''' <param name="gp">The group to which the member should be added.</param>
        ''' <param name="mems">The members to be added to the group</param>
        Sub AddTo(gp As IGroup, mems As IEnumerable(Of IGroupMember))

        ''' <summary>
        ''' Removes a group member from a group on the backing database.
        ''' </summary>
        ''' <param name="gp">The group from which the member should be removed.
        ''' </param>
        ''' <param name="mem">The member to remove.</param>
        Sub RemoveFrom(gp As IGroup, mem As IGroupMember)

        ''' <summary>
        ''' Removes group members from a group on the backing database.
        ''' </summary>
        ''' <param name="gp">The group from which the member should be removed.
        ''' </param>
        ''' <param name="mems">The members to remove.</param>
        Sub RemoveFrom(gp As IGroup, mems As IEnumerable(Of IGroupMember))

        ''' <summary>
        ''' Moves a group member from one group to another.
        ''' </summary>
        ''' <param name="fromGp">The group to move it from</param>
        ''' <param name="toGp">The group to move it to</param>
        ''' <param name="mem">The members to move</param>
        ''' <param name="isCopy">Indicates that the member is a copy</param>
        Sub MoveTo(fromGp As IGroup, toGp As IGroup, ByRef mem As IGroupMember, isCopy As Boolean)

        ''' <summary>
        ''' Deletes the given group, ensuring that all contents of it and its
        ''' subgroups is also deleted.
        ''' </summary>
        ''' <param name="gp">The group to be deleted</param>
        Sub DeleteGroup(gp As IGroup)

        ''' <summary>
        ''' Gets the permissions for the given group
        ''' </summary>
        ''' <param name="gp">The given group</param>
        ''' <returns></returns>
        Function GetPermissionsState(gp As IGroup) As PermissionState

        ''' <summary>
        ''' Get the current state of the group from the data store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Function GetGroup(id As Guid) As IGroup

        Property Server As IServer

    End Interface

End Namespace
