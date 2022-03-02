Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib

Public Class GroupPermissionLogic

    ''' <summary>
    ''' This enum is used to trigger the display of a specific message in the UI
    ''' </summary>
    Public Enum MessageID
        Warn_Message_Inherit_Perms
        Warn_Message_Member_Inherit_Perms
        Warn_Message_Overwrite_Parent
        Warn_Message_Lose_Perms
        Warn_Message_Member_Lose_Perms
        Warn_Message_Overwrite_Ancestor
        Warn_Copy_Unrestricted_to_Restricted
        Warn_Copy_Restricted_to_Unrestricted
        Warn_Copy_Restricted_Diff_Ancestor
        Inform_Insufficient_Permissions_Move
        Inform_Insufficient_Permissions_Restricted_Target
        Inform_Insufficient_Permissions_Restricted_Source
        Inform_Insufficient_Permissions_Restricted_Move
        Inform_Insufficient_Permissions_Same_Target_And_Source
    End Enum

    ''' <summary>
    ''' Validate the move from one part of the tree to another.
    ''' Raise appropriate warnings
    ''' </summary>
    ''' <param name="movingMember">The member that is being moved</param>
    ''' <param name="fromGroup">The group from which the member is being moved</param>
    ''' <param name="newParentGroup">The new parent of member that is being moved</param>
    ''' <param name="isCopy">Is this a move or a copy</param>
    ''' <param name="showMessage">Delegate to raise confirmation messages</param>
    ''' <returns></returns>
    Public Function ValidateMoveMember(movingMember As IGroupMember,
                                       fromGroup As IGroupMember,
                                       newParentGroup As IGroupMember, isCopy As Boolean,
                                       showMessage As Func(Of MessageID, Boolean, String(), Boolean),
                                       currentUser As Auth.IUser
                                       ) As Boolean
        Debug.Print($"CHECKING moving member:-{movingMember.Name} from:-{fromGroup.Name}, new parent:-{ If(newParentGroup Is Nothing, "Root", newParentGroup.Name)}")

        Dim treeDefinition = movingMember.Tree.TreeType.GetTreeDefinition()
        Dim editPermission = treeDefinition.EditPermission
        Dim manageGroupPermission = treeDefinition.AccessRightsPermission

        Dim typeName = movingMember.MemberType.GetFriendlyName()
        If movingMember.IsGroup AndAlso Not currentUser.HasPermission(editPermission) Then
            showMessage(MessageID.Inform_Insufficient_Permissions_Move, True, New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})
            Return False
        End If

        'If new parent is nothing it assumes the new parent is the root node
        If newParentGroup Is Nothing Then
            Return True
        End If

        Dim fromGroupPerms = gSv.GetEffectiveGroupPermissions(fromGroup.IdAsGuid)
        Dim newParentGroupPerms = gSv.GetEffectiveGroupPermissions(newParentGroup.IdAsGuid)

        ' If source and target groups are unrestricted then it's ok.
        If Not fromGroupPerms.IsRestricted AndAlso Not newParentGroupPerms.IsRestricted Then
            Return True
        End If


        ' If both groups are root they cannot inherit permissions and the InheritedAncesterID will be empty on both
        Dim bothGroupsAreRootGroups = fromGroupPerms.InheritedAncestorID = Guid.Empty AndAlso newParentGroupPerms.InheritedAncestorID = Guid.Empty

        'If the groups can inherit permissions and have either inherited from the same group
        'or if either of the groups is the ancestor of the other
        Dim permissionsInheritedFromSameSource = Not (bothGroupsAreRootGroups) AndAlso
                                                 (fromGroupPerms.InheritedAncestorID = newParentGroupPerms.InheritedAncestorID OrElse
                                                 fromGroupPerms.InheritedAncestorID = CType(newParentGroup.Id, Guid) OrElse
                                                 newParentGroupPerms.InheritedAncestorID = CType(fromGroup.Id, Guid))

        ' Check if user has permission to move the source group.
        Dim permissionToMoveFromSource = (Not fromGroupPerms.IsRestricted) OrElse
                                            (
                                                 (permissionsInheritedFromSameSource OrElse fromGroupPerms.HasPermission(currentUser, manageGroupPermission)) AndAlso
                                                 fromGroupPerms.HasPermission(currentUser, editPermission)
                                             )

        ' Check if user has permission to move group into the target group.
        Dim permissionToMoveIntoTarget = (Not newParentGroupPerms.IsRestricted) OrElse
                                            (
                                                 (permissionsInheritedFromSameSource OrElse newParentGroupPerms.HasPermission(currentUser, manageGroupPermission)) AndAlso
                                                 newParentGroupPerms.HasPermission(currentUser, editPermission)
                                             )

        If Not permissionToMoveFromSource AndAlso Not permissionToMoveIntoTarget AndAlso Not permissionsInheritedFromSameSource Then
            showMessage(MessageID.Inform_Insufficient_Permissions_Restricted_Move, True, New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})
            Return False
        ElseIf Not permissionToMoveFromSource AndAlso Not permissionToMoveIntoTarget AndAlso permissionsInheritedFromSameSource Then
            showMessage(MessageID.Inform_Insufficient_Permissions_Same_Target_And_Source, True, New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})
            Return False
        ElseIf Not permissionToMoveFromSource Then
            showMessage(MessageID.Inform_Insufficient_Permissions_Restricted_Source, True, New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})
            Return False
        ElseIf Not permissionToMoveIntoTarget Then
            showMessage(MessageID.Inform_Insufficient_Permissions_Restricted_Target, True, New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})
            Return False
        End If


        ' Get the id of the new group we are potentially inheriting permissions from
        Dim newAncestorID As Guid = newParentGroup.IdAsGuid
        If newParentGroupPerms.State = PermissionState.RestrictedByInheritance Then
            newAncestorID = newParentGroupPerms.InheritedAncestorID
        End If


        ' First case covers unrestricted node being moved to under another unrestricted node
        ' Either top or mid tree
        If fromGroupPerms.State = PermissionState.UnRestricted AndAlso
            newParentGroupPerms.State = PermissionState.UnRestricted Then
            Return True
        End If

        ' Is it moving to a sub group with the same restricted ancestor
        If fromGroupPerms.State <> PermissionState.UnRestricted AndAlso
            newParentGroupPerms.State = PermissionState.RestrictedByInheritance AndAlso
            (fromGroup.IdAsGuid = newParentGroupPerms.InheritedAncestorID OrElse
            fromGroupPerms.InheritedAncestorID = newParentGroupPerms.InheritedAncestorID) Then
            Return True
        End If


        ' If this is a copy and the folder we are moving is restricted and the target is unrestricted
        ' show a confirmation dialog to say we are broadening access.
        If isCopy AndAlso
             (fromGroupPerms.State = PermissionState.Restricted OrElse
             fromGroupPerms.State = PermissionState.RestrictedByInheritance) AndAlso
             (newParentGroup.IsRoot OrElse
                    newParentGroupPerms.State = PermissionState.UnRestricted) Then

            Return showMessage(MessageID.Warn_Copy_Restricted_to_Unrestricted, False, New String() {fromGroup.Name, newParentGroup.Name})
        End If

        ' Moving a directly restricted folder to an unrestricted parent.
        If movingMember.IsGroup AndAlso fromGroupPerms.State = PermissionState.Restricted AndAlso
                (newParentGroup.IsRoot OrElse
                    newParentGroupPerms.State = PermissionState.UnRestricted) AndAlso
                    movingMember.RawMember.MemberType <> GroupMemberType.Pool Then
            Return True
        End If

        ' Moving an unrestricted group under a parent that isn't unrestricted
        If fromGroupPerms.State = PermissionState.UnRestricted AndAlso
                newParentGroupPerms.State <> PermissionState.UnRestricted Then
            If isCopy Then
                Return showMessage(MessageID.Warn_Copy_Unrestricted_to_Restricted, False,
                                          New String() {fromGroup.Name, newParentGroup.Name})
            End If

            If movingMember.IsGroup AndAlso Not movingMember.IsPool Then
                Return showMessage(MessageID.Warn_Message_Inherit_Perms, False,
                               New String() {fromGroup.Name, newParentGroup.Name})
            Else
                Return showMessage(MessageID.Warn_Message_Member_Inherit_Perms, False,
                               New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})
            End If

        End If

        ' If copying display a message if source and target are differenly restricted.
        If isCopy AndAlso
           ((fromGroupPerms.State = PermissionState.Restricted AndAlso newParentGroupPerms.State = PermissionState.Restricted) OrElse
             (fromGroupPerms.State <> PermissionState.UnRestricted AndAlso
              newParentGroupPerms.State <> PermissionState.UnRestricted AndAlso
              fromGroupPerms.InheritedAncestorID <> newParentGroupPerms.InheritedAncestorID)) Then
            Return showMessage(MessageID.Warn_Copy_Restricted_Diff_Ancestor, False, New String() {fromGroup.Name, newParentGroup.Name})
        End If

        ' Moving a restricted group under another restricted group with a different ancestor
        If fromGroupPerms.State = PermissionState.Restricted AndAlso
                newParentGroupPerms.State <> PermissionState.UnRestricted Then
            ' clearPermsOnMovingGroup = True
            Return showMessage(MessageID.Warn_Message_Overwrite_Parent, False, New String() {fromGroup.Name, newParentGroup.Name})
        End If

        'Move restricted member to unrestricted folder
        If Not movingMember.IsGroup AndAlso
            fromGroupPerms.State <> PermissionState.UnRestricted AndAlso
            newParentGroupPerms.State = PermissionState.UnRestricted Then

            Return showMessage(MessageID.Warn_Message_Member_Lose_Perms, False, New String() {TreeDefinitionAttribute.GetLocalizedFriendlyName(typeName, True)})

        End If


        ' Move indirectly restricted folder to unrestricted parent
        If fromGroupPerms.State = PermissionState.RestrictedByInheritance AndAlso
                newParentGroupPerms.State = PermissionState.UnRestricted Then
            If movingMember.IsGroup Then
                Return showMessage(MessageID.Warn_Message_Lose_Perms, False, New String() {fromGroup.Name, newParentGroup.Name})
            Else
                Return showMessage(MessageID.Warn_Message_Member_Lose_Perms, False, New String() {fromGroup.Name, newParentGroup.Name})
            End If
        End If

        ' Move a indirectly restricted group to an already restricted parent where 
        ' the ancestor passing down the permissions is different.
        If fromGroupPerms.State = PermissionState.RestrictedByInheritance AndAlso
                newParentGroupPerms.State <> PermissionState.UnRestricted AndAlso
                fromGroupPerms.InheritedAncestorID <> newAncestorID Then
            Dim ancestorName As String = gSv.GetGroup(newAncestorID)?.Name
            Return showMessage(MessageID.Warn_Message_Overwrite_Ancestor, False, New String() {fromGroup.Name, ancestorName})
        End If



        Return True
    End Function

    ''' <summary>
    ''' Move a group and all of it's descendants from its current location to 
    ''' under a new parent. Copy actually does a clone and move, so will also
    ''' go through this code.
    ''' </summary>
    ''' <param name="fromGroup">The group being moved</param>
    ''' <param name="newParentGroup">The new parent of the group being moved</param>
    ''' <param name="movingEntry">The item being moved</param>
    ''' <param name="isCopy">Indicates that the member is a copy</param>
    ''' <returns>Moved group member after it has been updated on the server.</returns>
    Public Function MoveGroupEntry(fromGroup As IGroup,
                              newParentGroup As IGroup,
                              movingEntry As GroupMember, isCopy As Boolean) As IGroupMember
        If movingEntry.IsGroup Then

            ' Perform additional logic steps for moving a group.
            Dim movingGroupPerms = gSv.GetEffectiveGroupPermissions(movingEntry.IdAsGuid)
            Dim newParentGroupPerms = gSv.GetEffectiveGroupPermissions(newParentGroup.IdAsGuid)

            ' In the case where a node is restricted and we are moving it to be a sub node of another restricted 
            ' group, we need to clear the restriction.
            If movingGroupPerms.State = PermissionState.Restricted AndAlso
                    newParentGroupPerms.State <> PermissionState.UnRestricted AndAlso
                    movingEntry.MemberType <> GroupMemberType.Pool Then
                gSv.SetActualGroupPermissions(movingEntry.IdAsGuid, Nothing)
            End If

        End If

        movingEntry = gSv.MoveGroupEntry(fromGroup.IdAsGuid, newParentGroup.IdAsGuid, movingEntry, isCopy)

        ' Refresh the permissions on the moved entry.
        movingEntry.Permissions = gSv.GetEffectiveMemberPermissions(movingEntry)

        Return movingEntry

    End Function

End Class
