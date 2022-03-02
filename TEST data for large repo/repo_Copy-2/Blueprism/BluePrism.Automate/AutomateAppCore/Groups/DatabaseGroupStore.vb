Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' A group store using the database as its backing
    ''' </summary>
    Public Class DatabaseGroupStore : Implements IGroupStore

        ' The server instance to use for this group store
        Private mServer As IServer

        ' Permissions business object used when moving groups
        Private mPermissions As GroupPermissionLogic

        Public Property Server As IServer Implements IGroupStore.Server
            Get
                Return mServer
            End Get
            Set(value As IServer)
                mServer = value
                mPermissions = New GroupPermissionLogic()
            End Set
        End Property

        ''' <summary>
        ''' Creates a new database group store using the given server instance
        ''' </summary>
        ''' <param name="sv">The server instance to connect to the database with.
        ''' </param>
        Public Sub New(sv As IServer)
            If sv Is Nothing Then Throw New ArgumentNullException(NameOf(sv))
            Server = sv
        End Sub

        ''' <summary>
        ''' Gets the group tree of the given type from the backing store, with no
        ''' filtering or modifying.
        ''' </summary>
        ''' <param name="tp">The type of tree to retrieve</param>
        ''' <param name="fullReload">True to skip any cached tree and to retrieve it
        ''' from the backing store</param>
        ''' <returns>The tree of the given type from the database</returns>
        Protected Overridable Function GetRawTree(
         tp As GroupTreeType, fullReload As Boolean, ca As TreeAttributes) As GroupTree
            Return gSv.GetTree(tp, ca)
        End Function


        ''' <summary>
        ''' Gets the tree of the given type, filtered as specified, from the backing
        ''' store.
        ''' </summary>
        ''' <param name="tp">The type denoting which tree to retrieve</param>
        ''' <param name="filter">A filter to run on the tree contents before
        ''' returning it.</param>
        ''' <param name="groupFilter">prediate for filtering groups</param>
        ''' <param name="poolsAsGroups">show pools as groups or members</param>
        ''' <param name="fullReload">use cached version or rebuild structure</param>
        ''' <returns>The group tree of the given type from the database.</returns>
        ''' <remarks>This is not guaranteed to be an instance of
        ''' <see cref="GroupTree"/>, so it should not be treated as such.</remarks>
        Public Function GetTree(tp As GroupTreeType, filter As Predicate(Of IGroupMember),
                         groupFilter As Predicate(Of IGroup), fullReload As Boolean,
                         poolsAsGroups As Boolean, getRetired As Boolean) As IGroupTree _
         Implements IGroupStore.GetTree
            Dim t As GroupTree = Nothing

            Dim combinedAttributes As New TreeAttributes()

            If Not getRetired Then
                If tp = GroupTreeType.Processes OrElse tp = GroupTreeType.Objects Then
                    combinedAttributes.RequiredProcessAttributes = ProcessAttributes.None
                    combinedAttributes.UnacceptableProcessAttributes = ProcessAttributes.Retired
                ElseIf tp = GroupTreeType.Resources Then
                    combinedAttributes.RequiredResourceAttributes = ResourceAttribute.None
                    combinedAttributes.UnacceptableResourceAttributes = ResourceAttribute.Retired
                End If
            End If

            t = GetRawTree(tp, fullReload, combinedAttributes)

            t.Store = Me
            Return New FilteringGroupTree(t, filter, groupFilter, poolsAsGroups)
        End Function

        ''' <summary>
        ''' Refreshes the given group tree, getting the latest data from the backing
        ''' store.
        ''' </summary>
        ''' <param name="tree">The tree to refresh</param>
        Public Overridable Sub Refresh(tree As IGroupTree) _
         Implements IGroupStore.Refresh
            If tree Is Nothing Then Throw New ArgumentNullException(NameOf(tree))

            Dim rawTree As GroupTree = tree.RawTree
            Dim newTree As GroupTree = Nothing

            newTree = GetRawTree(tree.TreeType, True, tree.CombinedAttributes)

            ' Update from the new tree (assuming that they are not the same object
            If rawTree IsNot newTree Then rawTree.UpdateDataFrom(newTree)

        End Sub

        ''' <summary>
        ''' Updates the given group's metadata (ie. not its contents), modifying its
        ''' ID and name as appropriate.
        ''' </summary>
        ''' <param name="gp">The group to update</param>
        ''' <exception cref="ArgumentNullException">If <paramref name="gp"/> is null.
        ''' </exception>
        Public Overridable Sub Update(gp As IGroup) Implements IGroupStore.Update
            If gp Is Nothing Then Throw New ArgumentNullException(NameOf(gp))
            Dim rawGroup = gp.RawGroup
            If rawGroup IsNot Nothing Then
                Dim newGp As Group = gSv.UpdateGroup(rawGroup)
                If newGp IsNot rawGroup Then rawGroup.SetTo(newGp)
            End If
            gp.Permissions = gSv.GetEffectiveMemberPermissions(gp)
        End Sub

        ''' <summary>
        ''' Adds a group member to a group within the database.
        ''' </summary>
        ''' <param name="gp">The group to which the member should be added.</param>
        ''' <param name="mem">The member to be added to the group</param>
        ''' <exception cref="AlreadyExistsException">If the group member is already
        ''' a member of the group</exception>
        Public Sub AddTo(gp As IGroup, mem As IGroupMember) _
         Implements IGroupStore.AddTo
            AddTo(gp, GetSingleton.ICollection(mem))
        End Sub

        ''' <summary>
        ''' Adds group members to a group within the database.
        ''' </summary>
        ''' <param name="gp">The group to which the member should be added.</param>
        ''' <param name="mems">The members to be added to the group</param>
        ''' <exception cref="AlreadyExistsException">If any group member is already
        ''' a member of the group</exception>
        Public Overridable Sub AddTo(
         gp As IGroup, mems As IEnumerable(Of IGroupMember)) _
         Implements IGroupStore.AddTo
            ' You don't add to the root, you just take away from everywhere else
            ' Ensure we're dealing with the raw members for the call to the server
            If Not gp.IsRoot Then gSv.AddToGroup(
                gp.TreeType, gp.IdAsGuid, mems.RawMembers().ToList())
            For Each m In mems
                m.Permissions = gSv.GetEffectiveMemberPermissions(m.RawMember)
            Next
        End Sub

        ''' <summary>
        ''' Removes a group member from a group on the backing database.
        ''' </summary>
        ''' <param name="gp">The group from which the member should be removed.
        ''' </param>
        ''' <param name="mem">The member to remove.</param>
        Public Sub RemoveFrom(gp As IGroup, mem As IGroupMember) _
         Implements IGroupStore.RemoveFrom
            RemoveFrom(gp, GetSingleton.ICollection(mem))
        End Sub

        ''' <summary>
        ''' Removes group members from a group on the backing database.
        ''' </summary>
        ''' <param name="gp">The group from which the member should be removed.
        ''' </param>
        ''' <param name="mems">The members to remove.</param>
        Public Overridable Sub RemoveFrom(
         gp As IGroup, mems As IEnumerable(Of IGroupMember)) _
         Implements IGroupStore.RemoveFrom
            ' You don't remove from the root - you just add somewhere else.
            If gp.IsRoot Then Return
            gSv.RemoveFromGroup(gp.IdAsGuid, mems.RawMembers().ToList())
        End Sub

        ''' <summary>
        ''' Deletes the given group, ensuring that all contents of it and its
        ''' subgroups is also deleted.
        ''' </summary>
        ''' <param name="gp">The group to be deleted</param>
        ''' <exception cref="InvalidArgumentException">If the given group is the
        ''' <see cref="IGroup.IsRoot">root</see> of a tree.</exception>
        Public Overridable Sub DeleteGroup(gp As IGroup) _
         Implements IGroupStore.DeleteGroup
            If gp.IsRoot Then Throw New InvalidArgumentException(
                "Cannot delete the root of the tree")
            gSv.RemoveGroup(gp.RawGroup)
        End Sub

        ''' <summary>
        ''' Moves a group member from one group to another.
        ''' </summary>
        ''' <param name="fromGp">The group to move it from</param>
        ''' <param name="toGp">The group to move it to</param>
        ''' <param name="mem">The members to move</param>
        ''' <param name="isCopy">Indicates that the member is a copy</param>
        Public Sub MoveTo(
         fromGp As IGroup, toGp As IGroup, ByRef mem As IGroupMember, isCopy As Boolean) _
         Implements IGroupStore.MoveTo
            mem = mPermissions.MoveGroupEntry(fromGp, toGp, mem.RawMember, isCopy)
        End Sub

        ''' <summary>
        ''' Gets the permissions for a given group
        ''' </summary>
        ''' <param name="gp">The group to get permissions for</param>
        ''' <returns>An indication of the level of restricted permissions on the group
        ''' </returns>
        Public Function GetPermissionsState(gp As IGroup) As PermissionState _
            Implements IGroupStore.GetPermissionsState
            Return gSv.GetEffectiveGroupPermissions(gp.IdAsGuid).State
        End Function

        ''' <summary>
        ''' Get the current state of the group from the data store
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        Public Function GetGroup(id As Guid) As IGroup Implements IGroupStore.GetGroup
            Return gSv.GetGroup(id)
        End Function

        ''' <summary>
        ''' Disposes of this store
        ''' </summary>
        Public Overridable Sub Dispose() Implements IGroupStore.Dispose
            ' Nothing to do, as yet.
            mPermissions = Nothing
            mServer = Nothing
        End Sub

    End Class

End Namespace
