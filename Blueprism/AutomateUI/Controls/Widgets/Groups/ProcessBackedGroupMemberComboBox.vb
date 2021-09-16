Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Auth

Public Class ProcessBackedGroupMemberComboBox : Inherits GroupMemberComboBox
    ''' <summary>
    ''' Checks that the current user has permission to execute the given member.
    ''' </summary>
    ''' <param name="mem">The member to check execute permission for</param>
    Protected Overrides Function HasPermission(mem As IGroupMember) As Boolean
        If mem.Tree.TreeType <> GroupTreeType.Processes Then Return False
        Return mem.Permissions.HasPermission(User.Current, Permission.ProcessStudio.ExecuteProcess)
    End Function
End Class
