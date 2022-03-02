Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Data
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    ''' <summary>
    ''' Adds the node to the specified group. If the node being added is a group
    ''' and no groupID is passed then a top-level group is created (groupID must
    ''' always be passed for leaf nodes).
    ''' </summary>
    ''' <param name="treeType">The type of tree being added to</param>
    ''' <param name="groupID">The group to add to</param>
    ''' <param name="mem">The node to add</param>
    <SecuredMethod()>
    Public Sub AddToGroup(treeType As GroupTreeType, groupID As Guid, mem As GroupMember) Implements IServer.AddToGroup
        CheckPermissions()
        AddToGroup(treeType, groupID, GetSingleton.ICollection(mem))
    End Sub

    ''' <summary>
    ''' Adds the list of nodes node to the specified group. If the node being added
    ''' is a group and no groupID is passed then a top-level group is created
    ''' (groupID must always be passed for leaf nodes).
    ''' </summary>
    ''' <param name="treeType">The type of tree being added to</param>
    ''' <param name="groupID">The group to add to</param>
    ''' <param name="nodes">The nodes to add</param>
    <SecuredMethod()>
    Public Sub AddToGroup(treeType As GroupTreeType, groupID As Guid, nodes As IEnumerable(Of GroupMember)) Implements IServer.AddToGroup
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()

            ' Check user has access to add items to this group
            Dim treeDef = treeType.GetTreeDefinition()
            If groupID <> Guid.Empty AndAlso treeDef.CreateItemPermission IsNot Nothing AndAlso
              nodes.Where(Function(n) Not n.IsGroup).Count > 0 AndAlso
              Not GetEffectiveGroupPermissions(con, groupID).HasPermission(
               mLoggedInUser, treeDef.CreateItemPermission) Then
                Throw New PermissionException(
                    My.Resources.clsServer_UnauthorizedUserDoesNotHavePermissionToCreate0InThisGroup,
                    TreeDefinitionAttribute.GetLocalizedFriendlyName(treeDef.PluralName))
            End If

            For Each mem As GroupMember In nodes
                AddToGroup(con, groupID, mem)
            Next
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub
    Private Sub AddToGroup(con As IDatabaseConnection, groupId As Guid, mem As GroupMember)
        AddToGroup(con, groupId, mem, False)
    End Sub

    ''' <summary>
    ''' Adds an entry to a group. Note that if the given group member is a group, it
    ''' will add the entire subtree to the given group ID.
    ''' </summary>
    ''' <param name="con">The connection to use to access the database</param>
    ''' <param name="groupId">The ID of the group to add the entry to</param>
    ''' <param name="mem">The group member to be added to the group</param>
    ''' <exception cref="NoSuchElementException">Either the group or the member does
    ''' not exist on the database.</exception>
    ''' <exception cref="SqlException">If any other database errors occur.
    ''' </exception>
    Private Sub AddToGroup(con As IDatabaseConnection, groupId As Guid, mem As GroupMember, removeFromExistingGroup As Boolean)

        ' Adding item to root node when the tree has a default group.
        ' If adding the item to the root node and this tree contains a default group, add it to the default group instead.
        If groupId = Guid.Empty AndAlso Not mem.IsGroup AndAlso mem.Tree IsNot Nothing AndAlso mem.Tree.HasDefaultGroup Then
            If mem.Tree.CanAccessDefaultGroup Then
                groupId = mem.Tree.DefaultGroup.IdAsGuid()
            Else
                Throw New BluePrismException(My.Resources.clsServer_YouDoNotHavePermissionToCreateThisItemInTheDefaultGroup)
            End If
        End If

        ' Adding to root is a no-op
        If groupId = Guid.Empty Then Return

        ' This should never actually happen, but check that if the item being moved is a default group, it isn't going anywhere
        ' except the root.
        Dim groupMoved = TryCast(mem, IGroup)
        If groupMoved IsNot Nothing Then

            If groupMoved.IsDefault AndAlso groupId <> Guid.Empty Then
                Throw New BluePrismException(My.Resources.clsServer_YouCannotMoveTheDefaultGroup)
            End If
        End If

        ' If we're adding a group and it's not yet on the database we need to save it
        ' Note that this will add all of its contents too - see UpdateGroup()
        If mem.IsGroup AndAlso Not mem.IsPersisted Then _
         UpdateGroup(con, DirectCast(mem, Group))

        Dim cmd As New SqlCommand()
        cmd.CommandText = String.Format(
            " if not exists ( " &
            "   select 1 from {0} where {1} = @groupid And {2} = @memberid)" &
            "     begin " &
            If(removeFromExistingGroup, "delete from {0} where {2} = @memberid", String.Empty) &
            "       insert into {0} ({1}, {2}) values (@groupid, @memberid) " &
            "     end",
            ValidateTableName(con, mem.LinkTableName),
            ValidateFieldName(con, mem.LinkTableName, mem.GroupIdColumnName),
            ValidateFieldName(con, mem.LinkTableName, mem.MemberIdColumnName)
        )
        With cmd.Parameters
            .AddWithValue("@groupid", groupId)
            .AddWithValue("@memberid", mem.Id)
        End With

        Try
            con.Execute(cmd)

        Catch sqle As SqlException _
         When sqle.Number = DatabaseErrorCode.ForeignKeyError
            ' Foreign key error suggests that either group or member does not exist
            Throw New NoSuchElementException(sqle,
                My.Resources.clsServer_EitherTheGroupOrTheMemberSpecifiedDoesNotExistAndSoTheMemberCannotBeAddedToTheG)

        End Try

    End Sub

    ''' <summary>
    ''' Removes the node from the specified group. Note that for leaf nodes this 
    ''' does not delete the underlying item (object/process etc.), it merely returns
    ''' it to the top-level of the tree. Where a group is being removed all it's
    ''' descendant subgroups and contents will be removed too.
    ''' </summary>
    ''' <param name="groupID">The group to remove from</param>
    ''' <param name="mem">The node to remove</param>
    <SecuredMethod(True)>
    Public Sub RemoveFromGroup(groupID As Guid, mem As GroupMember) Implements IServer.RemoveFromGroup
        CheckPermissions()
        RemoveFromGroup(groupID, GetSingleton.ICollection(mem))
    End Sub

    ''' <summary>
    ''' Removes the list of nodes from the specified group. Note that for leaf nodes
    ''' this does not delete the underlying items (objects/processes etc.), it merely
    ''' returns them to the top-level of the tree. Where a group is being removed
    ''' all it's descendant subgroups and contents will be removed too.
    ''' </summary>
    ''' <param name="groupID">The group to remove from</param>
    ''' <param name="nodes">The nodes to remove</param>
    <SecuredMethod(True)>
    Public Sub RemoveFromGroup(groupID As Guid, nodes As IEnumerable(Of GroupMember)) Implements IServer.RemoveFromGroup
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            For Each mem As GroupMember In nodes
                RemoveFromGroup(con, groupID, mem, True)
            Next
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Removes an entry from a group
    ''' </summary>
    ''' <param name="con">The connection to use to access the database</param>
    ''' <param name="groupId">The ID of the group to remove the entry from</param>
    ''' <param name="mem">The node which represents the entry to be removed from the
    ''' group</param>
    ''' <param name="deleteGroup">True to delete the group if that is what is being
    ''' removed; false to leave it orphaned in the database.</param>
    ''' <exception cref="SqlException">If any database errors occur.
    ''' </exception>
    Private Sub RemoveFromGroup(con As IDatabaseConnection,
     groupId As Guid, mem As GroupMember, deleteGroup As Boolean)

        ' Removing from root is a no-op
        If groupId = Guid.Empty Then Return


        Dim treeDefinition = mem.Tree.TreeType.GetTreeDefinition()
        Dim effectiveGroupPerms = GetEffectiveGroupPermissions(con, groupId)

        Dim accessRightsOrEditPermission = effectiveGroupPerms.HasPermission(mLoggedInUser, treeDefinition.AccessRightsPermission) OrElse
                              effectiveGroupPerms.HasPermission(mLoggedInUser, treeDefinition.EditPermission)

        If effectiveGroupPerms.IsRestricted AndAlso Not accessRightsOrEditPermission OrElse
            (mem.IsGroup AndAlso Not effectiveGroupPerms.HasPermission(mLoggedInUser, treeDefinition.EditPermission)) Then
            Throw New PermissionException(String.Format(My.Resources.clsServer_UnauthorizedUserDoesNotHavePermissionToRemove0FromThisGroup, mem.Name))
        End If

        Dim cmd As New SqlCommand(String.Format(
            "delete from {0} where {1} = @groupid and {2} = @memberid",
            ValidateTableName(con, mem.LinkTableName),
            ValidateFieldName(con, mem.LinkTableName, mem.GroupIdColumnName),
            ValidateFieldName(con, mem.LinkTableName, mem.MemberIdColumnName)
        ))
        With cmd.Parameters
            .AddWithValue("@groupid", groupId)
            .AddWithValue("@memberid", mem.Id)
        End With
        Dim deleted As Boolean = (con.ExecuteReturnRecordsAffected(cmd) > 0)
        ' If a group has been removed, then it no longer exists
        If deleteGroup AndAlso mem.IsGroup Then RemoveGroup(con, DirectCast(mem, Group))
    End Sub

    ''' <summary>
    ''' Removes an entry from a group when you are moving to another group
    ''' Assumption is made that validation of the move has been made prior to this call
    ''' </summary>
    ''' <param name="con">The connection to use to access the database</param>
    ''' <param name="groupId">The ID of the group to remove the entry from</param>
    ''' <param name="mem">The node which represents the entry to be removed from the
    ''' group</param>
    ''' <exception cref="SqlException">If any database errors occur.
    ''' </exception>
    Private Sub RemoveFromGroupWhenMoving(con As IDatabaseConnection,
     groupId As Guid, mem As GroupMember)

        ' Removing from root is a no-op
        If groupId = Guid.Empty Then Return


        Dim cmd As New SqlCommand(String.Format(
            "delete from {0} where {1} = @groupid and {2} = @memberid",
            ValidateTableName(con, mem.LinkTableName),
            ValidateFieldName(con, mem.LinkTableName, mem.GroupIdColumnName),
            ValidateFieldName(con, mem.LinkTableName, mem.MemberIdColumnName)
        ))
        With cmd.Parameters
            .AddWithValue("@groupid", groupId)
            .AddWithValue("@memberid", mem.Id)
        End With
        Dim deleted As Boolean = (con.ExecuteReturnRecordsAffected(cmd) > 0)
    End Sub
    ''' <summary>
    ''' Removes a group from the database.
    ''' </summary>
    ''' <param name="gp">The group to remove</param>
    <SecuredMethod()>
    Public Sub RemoveGroup(gp As Group) Implements IServer.RemoveGroup
        CheckPermissions()
        Using con = GetConnection()
            Dim effectivePermissions = GetEffectiveGroupPermissions(con, gp.IdAsGuid)
            If Not effectivePermissions.HasPermission(mLoggedInUser, gp.TreeType.GetTreeDefinition().EditPermission) Then
                Throw New PermissionException(My.Resources.clsServer_UnauthorizedUserDoesNotHavePermissionToRemoveThisGroup)
            End If

            con.BeginTransaction()
            RemoveGroup(con, gp)
            AuditRecordGroupEvent(con, GroupTreeEventCode.DeleteGroup, gp, String.Format(My.Resources.clsServer_GroupPath0, gp.FullPath), "")
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub


    ''' <summary>
    ''' Returns true if this tree type has a default group
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <SecuredMethod(True)>
    Public Function HasDefaultGroup(tree As GroupTreeType) As Boolean _
        Implements IServer.HasDefaultGroup
        CheckPermissions()
        Using con = GetConnection()
            Return HasDefaultGroup(con, tree)
        End Using
    End Function

    ''' <summary>
    ''' Returns true if this tree type has a default group
    ''' </summary>
    ''' <param name="con">The connection to use to access the database</param>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    Private Function HasDefaultGroup(con As IDatabaseConnection, tree As GroupTreeType) As Boolean
        Using cmd As New SqlCommand()
            Dim sb As New StringBuilder(
                    " select count(*)" &
                    " from BPATreeDefaultGroup dg" &
                    " where dg.treeid = @treeid "
                )
            cmd.CommandText = sb.ToString()
            cmd.Parameters.AddWithValue("@treeid", tree)

            Return CType(con.ExecuteReturnScalar(cmd), Integer) = 1

        End Using
    End Function

    ''' <summary>
    ''' Returns the ID of the default group of this tree
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    <SecuredMethod(True)>
    Public Function GetDefaultGroupId(tree As GroupTreeType) As Guid _
        Implements IServer.GetDefaultGroupId
        CheckPermissions()
        Using con = GetConnection()
            Return GetDefaultGroupId(con, tree)
        End Using
    End Function

    ''' <summary>
    ''' Returns the ID of the default group of this tree
    ''' </summary>
    ''' <param name="con">The connection to use to access the database</param>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    Private Function GetDefaultGroupId(con As IDatabaseConnection, tree As GroupTreeType) As Guid
        Using cmd As New SqlCommand()
            Dim sb As New StringBuilder(
                    " select groupid" &
                    " from BPATreeDefaultGroup dg" &
                    " where dg.treeid = @treeid "
                )
            cmd.CommandText = sb.ToString()
            cmd.Parameters.AddWithValue("@treeid", tree)
            Return CType(con.ExecuteReturnScalar(cmd), Guid)
        End Using
    End Function

    ''' <summary>
    ''' Removes a group entirely. The group must be empty.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="gp">The group to remove</param>
    ''' <exception cref="ForeignKeyDependencyException">If this group could not be 
    ''' deleted due to being referenced elsewhere in the database.</exception>
    ''' <exception cref="BluePrismException"> If this group could not be deleted due to it
    ''' containing items. </exception>
    Private Sub RemoveGroup(con As IDatabaseConnection, gp As Group)

        ' Don't allow deletion of default groups.
        If gp.IsDefault Then Throw New BluePrismException(My.Resources.clsServer_UnableToDeleteADefaultGroup)

        ' First check if the group is assigned to active
        ' queues
        Using cmd As New SqlCommand()
            Dim sb As New StringBuilder(
                " select q.name" &
                " from BPAWorkQueue q" &
                "   join BPAGroup g on q.resourcegroupid = g.id" &
                " where g.id = @id"
            )

            cmd.Parameters.AddWithValue("@id", gp.Id)
            cmd.CommandText = sb.ToString()
            Dim queueNames As New List(Of String)
            Using reader = con.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    queueNames.Add(reader.GetString(0))
                End While
            End Using
            If queueNames.Count > 0 Then Throw New ForeignKeyDependencyException(
                My.Resources.clsServer_ThisGroupOrADescendantGroupCannotBeDeletedBecauseItIsAssignedToOneOrMoreOfTheWo,
                vbCrLf, CollectionUtil.Join(queueNames, vbCrLf))

        End Using

        ' Check that the group contains no items (including items this user may not be able to see)
        Dim numMembers = 0
        For Each memType As GroupMemberType In TreeDefinitionAttribute.GetSupportedTypesFor(gp.TreeType)
            Dim viewName As String = memType.GetViewName()
            If viewName = "" Then Continue For
            Dim cmd As New SqlCommand()
            cmd.CommandText = " select count(*) from " & viewName & " where groupid = @groupId"
            cmd.Parameters.AddWithValue("@groupId", gp.Id)
            numMembers = numMembers + CType(con.ExecuteReturnScalar(cmd), Integer)
            If numMembers > 0 Then
                Throw New BluePrismException(My.Resources.clsServer_UnableToDeleteGroupItContainsOneOrMoreItems)
            End If
        Next

        ' Delete group
        Using cmd As New SqlCommand()
            cmd.CommandText =
             "delete from BPAGroupGroup where memberid = @groupId;" &
             "delete from BPAGroup where id = @groupId;"
            cmd.Parameters.AddWithValue("@groupId", gp.Id)
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Moves the list of nodes from one group to another.
    ''' </summary>
    ''' <param name="fromGroupID">The group to move nodes from</param>
    ''' <param name="toGroupID">The group to mode nodes to</param>
    ''' <param name="node">The node to move</param>
    ''' <param name="isCopy">Indicates that the member is a copy</param>
    ''' <returns> The moved group once it has been saved to the database.</returns>
    <SecuredMethod(True)>
    Public Function MoveGroupEntry(
     fromGroupID As Guid, toGroupID As Guid, node As GroupMember, isCopy As Boolean) As GroupMember Implements IServer.MoveGroupEntry
        CheckPermissions()
        Using con = GetConnection()
            Dim beforeState = GetEffectiveMemberPermissions(con, node).State
            con.BeginTransaction()
            ValidateGroupMemberExists(con, node)
            ValidateGroupMemberCanMove(con, node, fromGroupID, toGroupID, isCopy)
            If node.IsGroup Then ValidateGroupMoveIsUnique(con, node, toGroupID)
            RemoveFromGroupWhenMoving(con, fromGroupID, node)
            AddToGroup(con, toGroupID, node)
            Dim afterState = GetEffectiveMemberPermissions(con, node).State
            Dim paths = GetPathsToGroups(con, {fromGroupID, toGroupID})
            Dim audit = String.Format(If(isCopy, My.Resources.clsServer_01CopiedFrom2To3, My.Resources.clsServer_01MovedFrom2To3),
                                      TreeDefinitionAttribute.GetLocalizedFriendlyName(node.MemberType.GetFriendlyName), node.Name,
                                      If(fromGroupID = Guid.Empty, My.Resources.clsServer_Root, paths(fromGroupID)),
                                      If(toGroupID = Guid.Empty, My.Resources.clsServer_Root, paths(toGroupID)))
            If afterState <> beforeState Then audit &= String.Format(My.Resources.clsServer_PermissionsChangedFrom0To1,
                                                                     beforeState.GetFriendlyName(), afterState.GetFriendlyName())
            AuditRecordGroupEvent(con, If(isCopy, GroupTreeEventCode.CopyMember, GroupTreeEventCode.MoveMember), node, audit, Nothing)
            con.CommitTransaction()
            InvalidateCaches()
            Return node
        End Using
    End Function

    ''' <summary>
    ''' Test to check if the move is still valid, if
    ''' the <paramref name="node"/> no longer exists within <paramref name="fromGroupID"/> then
    ''' an exception is thrown.
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="node"></param>
    ''' <param name="fromGroupID"></param>
    ''' <param name="toGroupID"></param>
    ''' <exception cref="GroupMoveException"></exception>
    Private Sub ValidateGroupMemberCanMove(con As IDatabaseConnection, node As GroupMember, fromGroupID As Guid, toGroupID As Guid, isCopy As Boolean)
        Dim validGroupIds As New List(Of Guid)
        Using cmd As New SqlCommand()
            cmd.CommandText = $"select groupid from {ValidateTableName(con, node.LinkTableName)} where {ValidateFieldName(con, node.LinkTableName, node.MemberIdColumnName)} = @memberId"
            cmd.Parameters.AddWithValue("@memberId", node.Id)

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim provider As New ReaderDataProvider(reader)
                While reader.Read()
                    validGroupIds.Add(provider.GetGuid("groupid"))
                End While
            End Using

            ' copy fromGuids are always empty
            If isCopy And fromGroupID <> Guid.Empty Then
                Throw New GroupMoveException(My.Resources.clsServer_TheExpectedSourceGroupForTheItemDoesNotMatchItsCurrentSource,
                                             fromGroupID,
                                             toGroupID)
                ' validate the client side fromGroup is contained in a list of all groups with this node
                ' multiple groups containing the node is the case of cloning - ctrl dragging.
            ElseIf Not isCopy AndAlso validGroupIds.Any() AndAlso Not validGroupIds.Contains(fromGroupID) Then
                Throw New GroupMoveException(My.Resources.clsServer_TheExpectedSourceGroupForTheItemDoesNotMatchItsCurrentSource,
                                             If(validGroupIds.Any(), validGroupIds.First(), fromGroupID),
                                             toGroupID)
            End If

            ' if the group already exists in the folder we're moving to
            If validGroupIds.Contains(toGroupID) Then
                Throw New GroupMoveException(My.Resources.clsServer_TheExpectedSourceGroupForTheItemDoesNotMatchItsCurrentSource,
                                            fromGroupID,
                                            toGroupID)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Check to see if the group exists within 
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="node"></param>
    Private Sub ValidateGroupMemberExists(con As IDatabaseConnection, node As GroupMember)

        If TypeOf node IsNot Group OrElse TypeOf node Is ResourcePool Then
            Return
        End If

        Using cmd As New SqlCommand()
            cmd.CommandText = $"select case when exists (select 1 from BPAGroup bg where bg.id = @id) then 1 else 0 end"
            cmd.Parameters.AddWithValue("@id", node.Id)
            Dim result = CType(con.ExecuteReturnScalar(cmd), Boolean)
            If Not result Then
                Throw New GroupDeletedException(String.Format(My.Resources.clsServer_GroupNotFound, node.Id))
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Updates the given group on the database. At this point, all that means is
    ''' that it can rename the group.
    ''' </summary>
    ''' <param name="group">The group to update.</param>
    ''' <returns>The group node after the update. Note that this may change if the
    ''' group has been inserted onto the database (for direct connections, this will
    ''' be the same object; for BP Server connections, it may not be, but the
    ''' returned value will reflect the data on the database)</returns>
    <SecuredMethod(True)>
    Public Function UpdateGroup(group As Group) As Group Implements IServer.UpdateGroup
        CheckPermissions()
        Using con = GetConnection()
            Dim creating = group.IdAsGuid = Guid.Empty
            Dim effectivePermissions = GetEffectiveGroupPermissions(con, If(creating, group.Owner.IdAsGuid, group.IdAsGuid))
            If Not effectivePermissions.HasPermission(mLoggedInUser, group.TreeType.GetTreeDefinition().EditPermission) Then
                Throw New PermissionException(If(creating,
                                              My.Resources.clsServer_UnauthorizedUserDoesNotHavePermissionToCreateThisGroup,
                                              My.Resources.clsServer_UnauthorizedUserDoesNotHavePermissionToUpdateThisGroup))
            End If

            con.BeginTransaction()

            ValidateGroupCreateIsUnique(con, group)
            Dim oldGroupName = ""
            If Not creating Then oldGroupName = GetGroup(con, group.IdAsGuid).Name
            Dim gp As Group = UpdateGroup(con, group)
            If creating Then
                AuditRecordGroupEvent(con, GroupTreeEventCode.CreateGroup, group,
                                      String.Format(My.Resources.clsServer_GroupPath0, gp.FullPath), "")
            Else
                AuditRecordGroupEvent(con, GroupTreeEventCode.RenameGroup, group, "", oldGroupName)
            End If
            con.CommitTransaction()

            Return gp
        End Using
    End Function

    ''' <summary>
    ''' Updates the given group on the database. At this point, all that means is
    ''' that it can rename the group.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="gp">The group to update.</param>
    ''' <returns>The group node after the update. Note that this may change if the
    ''' group has been inserted onto the database (for direct connections, this will
    ''' be the same object; for BP Server connections, it may not be, but the
    ''' returned value will reflect the data on the database)</returns>
    ''' <exception cref="ArgumentNullException">If the given <paramref name="gp"/>
    ''' reference was null</exception>
    Private Function UpdateGroup(
     con As IDatabaseConnection, gp As Group) As Group
        If gp Is Nothing Then Throw New ArgumentNullException(NameOf(gp))
        Using cmd As New SqlCommand()

            ' This is happening somehow. I would really like to know how.
            Debug.Assert(gp.Name <> "", "Empty group name detected")

            ' If it's already on the database...
            If gp.IsPersisted Then
                cmd.CommandText =
                 " update BPAGroup set" &
                 "   name = @name" &
                 " where id = @id"

            Else
                ' Give it a new ID
                gp.Id = Guid.NewGuid()
                ' And insert it
                cmd.CommandText =
                 " insert into BPAGroup (id, treeid, name)" &
                 " values (@id, @treeid, @name)"

            End If
            With cmd.Parameters
                .AddWithValue("@id", gp.Id)
                .AddWithValue("@treeid", gp.TreeType)
                .AddWithValue("@name", gp.Name)
            End With
            con.Execute(cmd)
            For Each mem As GroupMember In gp
                Dim childGp As Group = TryCast(mem, Group)
                Dim rp As ResourcePool = TryCast(mem, ResourcePool)
                If childGp IsNot Nothing AndAlso rp Is Nothing Then UpdateGroup(con, childGp)
                AddToGroup(con, gp.IdAsGuid, mem)
            Next

            InvalidateCaches()

            Return gp
        End Using
    End Function

    Private Sub ValidateGroupCreateIsUnique(con As IDatabaseConnection, newGroup As Group)

        If Not GroupIsUniqueInFolder(con, newGroup, newGroup.Owner.IdAsGuid) Then Throw New GroupCreateException(String.Format(My.Resources.ValidateGroupUpdateError, newGroup.Name), newGroup.Name, newGroup.Owner.IdAsGuid)
    End Sub

    Private Sub ValidateGroupMoveIsUnique(con As IDatabaseConnection, node As GroupMember, target As Guid)
        If Not GroupIsUniqueInFolder(con, node, target) Then Throw New GroupMoveException(My.Resources.ValidateGroupUpdateError, node.IdAsGuid, target)
    End Sub



    Private Function GroupIsUniqueInFolder(con As IDatabaseConnection, node As GroupMember, target As Guid) As Boolean
        ' find all owners of groups that have the same name as the new group
        ' if any of these owners is the newgroup owner then we have tried to make a group that already exists in the directory
        Dim groupOwners As New List(Of Guid)

        Dim commandString = $"select bgg.groupid
	                                  from BPAGroup bg
	                                  left join BPAGroupGroup bgg on bg.id = bgg.memberid	                                  
                                      where bg.treeid = @treeid
                                      and bg.name = @name"

        Using cmd As New SqlCommand(commandString)
            cmd.AddParameter("@name", node.Name)
            cmd.AddParameter("@treeid", node.Tree.TreeType)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim provider As New ReaderDataProvider(reader)
                While reader.Read()
                    groupOwners.Add(provider.GetGuid("groupid"))
                End While
            End Using
        End Using

        Return Not groupOwners.Any(Function(x) x = target)
    End Function

    ''' <summary>
    ''' Gets the group IDs of any groups containing the given basenode.
    ''' </summary>
    ''' <param name="mem">The member for which all containing groups are required.
    ''' </param>
    ''' <returns>A collection of IDs representing the groups which contain the
    ''' element given.</returns>
    <SecuredMethod(True)>
    Public Function GetIdsOfGroupsContaining(
     mem As GroupMember) As ICollection(Of Guid) Implements IServer.GetIdsOfGroupsContaining
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            Dim ids = GetIdsOfGroupsContaining(con, mem)
            con.CommitTransaction()
            Return ids
        End Using
    End Function

    ''' <summary>
    ''' Gets the group IDs of any groups containing the given basenode.
    ''' </summary>
    ''' <param name="con">The connection to use to access the database.</param>
    ''' <param name="mem">The member node for which all containing groups are
    ''' required.</param>
    ''' <returns>A collection of IDs representing the groups which contain the
    ''' element given.</returns>
    Private Function GetIdsOfGroupsContaining(
     con As IDatabaseConnection, mem As GroupMember) As ICollection(Of Guid)
        If mem Is Nothing Then Throw New ArgumentNullException(NameOf(mem))
        Dim cmd As New SqlCommand(String.Format(
         "select {0} from {1} where {2} = @id",
         ValidateFieldName(con, mem.LinkTableName, mem.GroupIdColumnName),
         ValidateTableName(con, mem.LinkTableName),
         ValidateFieldName(con, mem.LinkTableName, mem.MemberIdColumnName)))
        cmd.Parameters.AddWithValue("@id", mem.Id)
        Dim ids As New List(Of Guid)
        Using reader = con.ExecuteReturnDataReader(cmd)
            While reader.Read()
                ids.Add(reader.GetGuid(0))
            End While
        End Using
        Return ids
    End Function

    ''' <summary>
    ''' Gets a group and its subtree from the database.
    ''' </summary>
    ''' <param name="id">The ID of the required group</param>
    ''' <returns>The group, disconnected from any overarching tree, which corresponds
    ''' to the given group ID or null if no such group was found.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="id"/> is
    ''' <see cref="Guid.Empty"/></exception>
    <SecuredMethod(True)>
    Public Function GetGroup(id As Guid) As Group Implements IServer.GetGroup
        CheckPermissions()
        Return GetGroup(id, True)
    End Function

    ''' <summary>
    ''' Gets a group and, optionally, its subtree from the database.
    ''' </summary>
    ''' <param name="id">The ID of the required group</param>
    ''' <param name="recursive">True to descend into the group's subtree and
    ''' retrieve all group members within its subtree; False to only retrieve the
    ''' immediate members of the group</param>
    ''' <returns>The group, disconnected from any overarching tree, which corresponds
    ''' to the given group ID or null if no such group was found.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="id"/> is
    ''' <see cref="Guid.Empty"/></exception>
    <SecuredMethod(True)>
    Public Function GetGroup(id As Guid, recursive As Boolean) As Group Implements IServer.GetGroup
        CheckPermissions()
        Using con = GetConnection()
            Return GetGroup(con, id, recursive, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Gets a group and its subtree from the database.
    ''' </summary>
    ''' <param name="con">The connection from which to draw the data</param>
    ''' <param name="id">The ID of the required group</param>
    ''' <returns>The group, disconnected from any overarching tree, which corresponds
    ''' to the given group ID or null if no such group was found.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="id"/> is
    ''' <see cref="Guid.Empty"/></exception>
    Private Function GetGroup(con As IDatabaseConnection, id As Guid) As Group
        Return GetGroup(con, id, True, Nothing)
    End Function

    ''' <summary>
    ''' Gets a group and, optionally, its subtree from the database.
    ''' </summary>
    ''' <param name="con">The connection from which to draw the data</param>
    ''' <param name="id">The ID of the required group</param>
    ''' <param name="recursive">True to descend into the group's subtree and
    ''' retrieve all group members within its subtree; False to only retrieve the
    ''' immediate members of the group</param>
    ''' <param name="treeType">Returns the tree type that the group belongs to</param>
    ''' <param name="skipPermissionCheck">If True then all members are returned irrespective
    ''' of whether or not they are accessible to the current user. Default is False</param>
    ''' <returns>The group, disconnected from any overarching tree, which corresponds
    ''' to the given group ID or null if no such group was found.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="id"/> is
    ''' <see cref="Guid.Empty"/></exception>
    Private Function GetGroup(con As IDatabaseConnection, id As Guid,
      recursive As Boolean, ByRef treeType As GroupTreeType,
      Optional skipPermissionCheck As Boolean = False) As Group
        If id = Guid.Empty Then Throw New ArgumentNullException(
            "groupId", My.Resources.clsServer_NoGroupIDProvidedToTheGetGroupMethod)

        ' Get the group details itself
        Dim cmd As New SqlCommand(
         " select g.id, g.name, g.treeid," &
         " case when tdg.treeid is not null then 1 else 0 end as isdefault" &
         " from BPAGroup g" &
         " left join BPATreeDefaultGroup tdg on tdg.treeid = g.treeid And tdg.groupid = g.id" &
         " where g.id = @groupId"
        )
        Dim groupIdParam = cmd.Parameters.AddWithValue("@groupId", id)
        Dim root As Group
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            If Not reader.Read() Then Return Nothing
            root = New Group(prov.GetBool("isdefault")) With {
                .Id = prov.GetGuid("id"),
                .Name = prov.GetString("name")
            }
            treeType = prov.GetValue("treeid", GroupTreeType.None)
        End Using

        Dim gpStack As New Stack(Of Group)
        gpStack.Push(root)

        Dim groupHiddenItems As New List(Of Guid)

        While gpStack.Count > 0
            ' Get the next group to deal with from the stack
            Dim gp As Group = gpStack.Pop()

            ' Check user has access to this group
            If Not skipPermissionCheck Then
                gp.Permissions = GetEffectiveMemberPermissions(con, gp)
                If gp.Permissions.IsRestricted AndAlso Not gp.HasViewPermission(mLoggedInUser) Then
                    groupHiddenItems.Add(gp.IdAsGuid)
                    Continue While
                End If
            End If

            ' Hold all the members of this group in a dictionary so that we can
            ' append data to members if we need to
            Dim groupMembers As New Dictionary(Of Object, GroupMember)

            ' Set its ID as the @groupId param
            groupIdParam.Value = gp.IdAsGuid

            ' Go through all supported types in this group's tree and get those
            ' which belong to the current group
            For Each memType As GroupMemberType In
                 TreeDefinitionAttribute.GetSupportedTypesFor(treeType)
                Dim viewName As String = memType.GetViewName()
                If viewName = "" Then Continue For
                cmd.CommandText =
                     " select * from " & viewName & " where groupid = @groupId"
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        Dim mem As GroupMember = Nothing

                        ' Check to see if we have this group member already
                        Dim memId As Object = prov.GetValue(Of Object)("id", Nothing)
                        If groupMembers.TryGetValue(memId, mem) Then
                            ' If we found it, ensure that we append any further
                            ' information from the provider to the member
                            mem.AppendFrom(prov)

                            ' The rest of the work is for a new member (adding to
                            ' groups, recursing through them etc. so move on now
                            Continue While
                        End If

                        ' It's a new member for us; create it from the provider
                        mem = memType.CreateNew(prov)

                        ' Add the member to the group
                        gp.Add(mem)

                        ' If the member is actually a group and we want to descend
                        ' the subtree, push it onto the stack
                        If recursive Then
                            Dim subGroup = TryCast(mem, Group)
                            If subGroup IsNot Nothing Then gpStack.Push(subGroup)
                        End If
                    End While
                End Using
            Next
        End While

        ' Set hidden member flag on any relevant parent groups
        If Not skipPermissionCheck Then
            root.Scan(Of Group)(
            Sub(g) If g.Owner IsNot Nothing Then _
                g.Owner.ContainsHiddenMembers = groupHiddenItems.Contains(g.IdAsGuid()))
        End If

        ' The group is now filled in
        Return root

    End Function

    ''' <summary>
    ''' Gets the tree from the database with the given type.
    ''' </summary>
    ''' <param name="tp">The type detailing which tree to retrieve</param>
    ''' <returns>The tree with the given type from the database. Note that the tree
    ''' will have no <see cref="GroupTree.Store">store</see> assigned to it on return
    ''' from this method.</returns>
    <SecuredMethod(AllowLocalUnsecuredCalls:=True)>
    Public Function GetTree(tp As GroupTreeType, ca As TreeAttributes) As GroupTree Implements IServer.GetTree
        CheckPermissions()

        Using con = GetConnection()

            Return GetTree(con, tp, ca)

        End Using
    End Function


    ''' <summary>
    ''' Gets the tree from the database with the given type.
    ''' </summary>
    ''' <param name="connection">The connection to the database to use</param>
    ''' <param name="treeType">The type detailing which tree to retrieve</param>
    ''' <returns>The tree with the given type from the database. Note that the tree
    ''' will have no <see cref="GroupTree.Store">store</see> assigned to it on return
    ''' from this method.</returns>
    Private Function GetTree(
        connection As IDatabaseConnection,
        treeType As GroupTreeType, combinedAttributes As TreeAttributes) As GroupTree

        ' Get the tree definition - we'll be using this quite a bit
        Dim treeDefinition As TreeDefinitionAttribute = treeType.GetTreeDefinition()
        Dim command As New SqlCommand()

        Dim simpleAttributes = GetSimpleAttributeValues(treeType, combinedAttributes)
        Dim requiredAtt As Integer = simpleAttributes.required
        Dim unacceptableAtt As Integer = simpleAttributes.unacceptable

        ' Get the current version of the tree first
        command.CommandText =
         " select t.versionno from BPADataTracker t where t.dataname = @dataname"
        command.Parameters.AddWithValue("@dataname", treeDefinition.DataName)
        Dim version As Long = IfNull(connection.ExecuteReturnScalar(command), 0L)
        command.Parameters.Clear()

        ' Get the current version and all the top-level groups in the tree
        command.CommandText =
         " select g.id, g.name, " &
         " case when tdg.treeid is not null then 1 else 0 end as isdefault" &
         " from BPAGroup g" &
         " left join BPAGroupGroup gg on gg.memberid = g.id" &
         " left join BPATreeDefaultGroup tdg on tdg.treeid = g.treeid And tdg.groupid = g.id" &
         " where g.treeid = @treeid" &
         "   and gg.groupid is null"
        command.Parameters.AddWithValue("@treeid", treeType)

        If mLoggedInUser IsNot Nothing AndAlso Not mLoggedInUser.CanSeeTree(treeType) Then
            Return New GroupTree(treeType, version, hasDefaultGroup:=False, combinedAttributes:=combinedAttributes)
        End If

        ' read the groups into a temp store
        Dim rootGroups As New List(Of Group)
        Using reader = connection.ExecuteReturnDataReader(command)
            Dim provider As New ReaderDataProvider(reader)
            While reader.Read()
                rootGroups.Add(New Group(provider.GetBool("isdefault")) With {
                    .Id = provider.GetGuid("id"),
                    .Name = provider.GetString("name")
                })
            End While
        End Using

        Dim tree = New GroupTree(treeType, version, rootGroups.Any(Function(x) x.IsDefault), combinedAttributes)
        Dim root = tree.Root

        ' Add the permission information to the group
        rootGroups.ForEach(Sub(x) x.Permissions = GetEffectiveMemberPermissions(connection, x))

        ' Filter the ones we shouldn't see out.
        rootGroups.Where(Function(x) Not x.Permissions.IsRestricted OrElse
                             x.HasViewPermission(mLoggedInUser)).ToList.ForEach(Sub(y) root.Add(y))

        ' We need to build a map of entries to their groups
        Dim map As New clsGeneratorDictionary(Of Guid, Dictionary(Of Object, GroupMemberDictionaryItem))

        ' Store ids of any groups which contain items this user cannot see.
        Dim groupHiddenItems As New List(Of Guid)

        ' Go through each valid type for this tree and load them
        ' For now, we just add each member to a list keyed against the ID of the
        ' group that they are a member of - Guid.Empty for ungrouped members
        For Each memberType As GroupMemberType In treeDefinition.SupportedMemberTypes
            Dim memberAttr = memberType.GetMemberAttribute()
            Dim viewName As String = memberType.GetViewName()
            If viewName = "" Then Continue For
            ' Note the 'select *' - the view dictates the names of the fields being
            ' returned, and the constructor for the member type picks out the data
            ' from the data provider it is passed
            command.CommandText =
                $"select id, groupid, {memberAttr.GetTreeSelect} from {memberAttr.ViewName}
                where treeid = @treeid or treeid is null"

            If (memberType = GroupMemberType.Process OrElse memberType = GroupMemberType.Object OrElse memberType = GroupMemberType.Resource) Then
                If requiredAtt > 0 Then
                    command.Parameters.AddWithValue("@wanted", requiredAtt)
                    command.CommandText += " and (attributes & @wanted != 0)"
                End If

                If unacceptableAtt > 0 Then
                    command.Parameters.AddWithValue("@unwanted", unacceptableAtt)
                    command.CommandText += " and (attributes & @unwanted = 0)"
                End If
            End If

            ' Build a list of all the ids we need to process
            ' This means we can shut the connection before filtering.
            Dim groupidList As New List(Of KeyValuePair(Of Guid, GroupMemberDictionaryItem))
            Using reader = connection.ExecuteReturnDataReader(command)
                Dim dataProvider = New ReaderDataProvider(reader)
                While reader.Read()
                    Dim id = dataProvider.GetValue(Of Object)("id", Nothing)
                    Dim groupId = dataProvider.GetValue(Of Guid)("groupid", Nothing)

                    ' Find out if we have this group member already - if we do,
                    ' 'append from' the provider rather than creating a new member
                    ' from it.
                    Dim member As GroupMemberDictionaryItem = Nothing
                    Dim groupMember = memberType.CreateNew(dataProvider)

                    Dim groupmembers = map(groupId)
                    groupmembers.TryGetValue(id, member)
                    If member Is Nothing OrElse member.GroupMember.MemberType <> memberType Then
                        member = New GroupMemberDictionaryItem(groupMember, groupmembers.Count)
                        groupmembers(id) = member
                    Else
                        member.GroupMember.AppendFrom(dataProvider)
                    End If
                    groupidList.Add(New KeyValuePair(Of Guid, GroupMemberDictionaryItem)(groupId, member))
                End While
            End Using

            ' Loop back through outside the data connection and add permissions. Filter
            ' nodes we're not supposed to see out.
            For Each entry As KeyValuePair(Of Guid, GroupMemberDictionaryItem) In groupidList

                Dim groupmembers = map(entry.Key)
                entry.Value.GroupMember.Permissions = GetEffectiveMemberPermissions(connection,
                                                                                        entry.Value.GroupMember)
                If entry.Value.GroupMember.Permissions.IsRestricted AndAlso
                   Not entry.Value.GroupMember.HasViewPermission(mLoggedInUser) Then
                    groupHiddenItems.Add(entry.Key)
                    groupmembers.Remove(entry.Value.GroupMember.IdAsGuid)
                End If
            Next

            ' get the list of group ids that are expanded for this tree type
            Dim expandedGroupIds = GetExpandedGroups(CShort(treeType))

            ' We now have all the linked entries in the map, assign them to the groups
            ' Basically, scan the tree for all groups; if the group is in the map (ie.
            ' it has contents), add the contents from the map into the group
            root.Scan(Of Group)(
            Sub(g)
                g.ContainsHiddenMembers = groupHiddenItems.Contains(g.IdAsGuid())
                If map.ContainsKey(g.IdAsGuid) Then
                    g.AddRange(
                        map(g.IdAsGuid).Values.
                            OrderBy(Function(x) x.Order).
                            Select(Function(x) x.GroupMember))
                End If
                g.Expanded = expandedGroupIds.Contains(g.IdAsGuid)
            End Sub)
        Next
        Return tree

    End Function

    Private Function GetSimpleAttributeValues(treeType As GroupTreeType, combinedAttributes As TreeAttributes) As (required As Integer, unacceptable As Integer)

        Dim requiredAtt As Integer = 0
        Dim unacceptableAtt As Integer = 0

        If treeType = GroupTreeType.Processes OrElse treeType = GroupTreeType.Objects AndAlso
                (combinedAttributes.RequiredProcessAttributes > ProcessAttributes.None OrElse
                combinedAttributes.UnacceptableProcessAttributes > ProcessAttributes.None) Then
            requiredAtt = CInt(combinedAttributes.RequiredProcessAttributes)
            unacceptableAtt = CInt(combinedAttributes.UnacceptableProcessAttributes)
        ElseIf treeType = GroupTreeType.Resources AndAlso
                (combinedAttributes.RequiredResourceAttributes > ResourceAttribute.None OrElse
                combinedAttributes.UnacceptableResourceAttributes > ResourceAttribute.None) Then
            requiredAtt = CInt(combinedAttributes.RequiredResourceAttributes)
            unacceptableAtt = CInt(combinedAttributes.UnacceptableResourceAttributes)
        End If

        Return (requiredAtt, unacceptableAtt)
    End Function

    ''' <summary>
    ''' Returns the ID and full path (from the root of the tree) of any groups which
    ''' contain the passed member. Groups in the path are separated by a forward
    ''' slash.
    ''' </summary>
    ''' <param name="mem">The member to look for</param>
    ''' <returns>The collection of group IDs and paths</returns>
    <SecuredMethod(True)>
    Public Function GetPathsToMember(mem As GroupMember) As IDictionary(Of Guid, String) Implements IServer.GetPathsToMember
        CheckPermissions()

        Using con = GetConnection()
            Return GetPathsToGroups(con, GetIdsOfGroupsContaining(con, mem))
        End Using

    End Function

    Private Function GetPathsToGroups(con As IDatabaseConnection, groupIds As ICollection(Of Guid)) As IDictionary(Of Guid, String)
        Dim paths As New Dictionary(Of Guid, String)
        Dim cmd As New SqlCommand(
            " with groups as (" &
            "  select gp.name, gp.id, 0 as Level from BPAGroup gp where id=@grpid" &
            "  union all" &
            "  select gp.name, gg.groupid, Level+1 from BPAGroupGroup gg" &
            "  inner join groups b on b.id=gg.memberid" &
            "  inner join BPAGroup gp on gp.id=gg.groupid)" &
            " select name from groups order by Level desc"
        )
        Dim idParam As SqlParameter = cmd.Parameters.Add("@grpid", SqlDbType.UniqueIdentifier)

        For Each id As Guid In groupIds
            If id = Guid.Empty Then Continue For
            Dim path As String = String.Empty
            idParam.Value = id
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    path &= String.Format("/{0}", prov.GetString("name"))
                End While
            End Using
            paths.Add(id, path.TrimStart(Convert.ToChar("/")))
        Next

        Return paths
    End Function

    ''' <summary>
    ''' Returns the id of the destination group. If this or any of the intervening groups are missing they are added.
    ''' Used during import.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="tree">The target group tree id</param>
    ''' <param name="group">The imported group component </param>
    ''' <returns>The id of the destination group</returns>
    Private Function GetOrCreateGroups(con As IDatabaseConnection, tree As GroupTreeType, group As GroupComponent) As Guid

        ' If the group getting imported is a default group,
        ' get the id of the default group for this tree.
        If group.IsDefaultGroup Then
            Return Me.GetDefaultGroupId(tree)
        End If

        Dim path As String = group.Name
        Dim groupID As Guid = Guid.Empty
        For Each grpName As String In path.Split(Convert.ToChar("/"))
            Dim parentID As Guid = groupID
            groupID = GetOrCreateGroup(con, tree, parentID, grpName)
        Next
        Return groupID
    End Function

    ''' <summary>
    ''' Returns the id of the group with the passed name, optionally within the
    ''' passed parent group. It is created if not found.
    ''' </summary>
    ''' <param name="con">Database connection</param>
    ''' <param name="tree">The target group tree id</param>
    ''' <param name="parentID">The parent group id (or guid.empty for root)</param>
    ''' <param name="name">The name of the group</param>
    ''' <returns>The group id</returns>
    Private Function GetOrCreateGroup(con As IDatabaseConnection, tree As GroupTreeType, parentID As Guid, name As String) As Guid
        Dim join As String = CStr(IIf(parentID <> Guid.Empty, "inner", "left"))
        Dim sb As New StringBuilder(String.Format(
            "declare @groupID uniqueidentifier;" &
            " select @groupID = g.id from BPAGroup g" &
            "  {0} join BPAGroupGroup gg on gg.memberid=g.id" &
            "  {0} join BPAGroup pg on pg.id=gg.groupid" &
            "  where g.treeid=@treeID and g.name=@groupName", join))
        If parentID <> Guid.Empty Then
            sb.Append(" and pg.id=@parentID;")
        Else
            sb.Append(" and pg.id is null;")
        End If
        sb.Append(" if @groupID is null" &
            "  begin" &
            "   set @groupID = NEWID();" &
            "   insert into BPAGroup (id, name, treeid) values (@groupID, @groupName, @treeID);")
        If parentID <> Guid.Empty Then
            sb.Append("   insert into BPAGroupGroup (groupid, memberid) values (@parentID, @groupID);")
        End If
        sb.Append("  end" &
                  " select @groupID")

        Dim cmd As New SqlCommand(sb.ToString())
        With cmd.Parameters
            .AddWithValue("@groupName", name)
            .AddWithValue("@treeID", tree)
            .AddWithValue("@parentID", parentID)
        End With
        Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    End Function

    Private Class GroupMemberDictionaryItem
        Public ReadOnly Property GroupMember As GroupMember

        Public ReadOnly Property Order As Integer

        Sub New(groupMember As GroupMember, order As Integer)
            Me.GroupMember = groupMember
            Me.Order = order
        End Sub
    End Class

End Class
