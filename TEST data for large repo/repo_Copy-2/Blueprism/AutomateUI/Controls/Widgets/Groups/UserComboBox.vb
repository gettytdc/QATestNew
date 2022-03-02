Imports BluePrism.AutomateAppCore.Groups
Imports AutomateControls

''' <summary>
''' Minor variation on the GroupMemberComboBox which allows deleted users to be
''' styled differently to active users.
''' </summary>
Public Class UserComboBox : Inherits GroupMemberComboBox

    ''' <summary>
    ''' Creates a new UserComboBox - this can only be for
    ''' <see cref="GroupTreeType.Users">Users</see>
    ''' </summary>
    Public Sub New()
        Me.TreeType = GroupTreeType.Users
    End Sub

    ''' <summary>
    ''' Overrides the adding of an entry to the combo box allowing deleted users to
    ''' be styled differently.
    ''' </summary>
    ''' <param name="mem">The member to add an entry for</param>
    ''' <param name="indent">The indent at which to add the entry</param>
    Protected Overrides Sub AddEntry(mem As IGroupMember, indent As Integer)
        MyBase.AddEntry(mem, indent)
        ' If the last entry added was a user, check if it is deleted and amend
        ' its style if it is.
        Dim u = TryCast(mem, UserGroupMember)
        If u IsNot Nothing AndAlso u.IsDeleted Then
            DirectCast(Items(Items.Count - 1), ComboBoxItem).Style = FontStyle.Italic
        End If

    End Sub

End Class
