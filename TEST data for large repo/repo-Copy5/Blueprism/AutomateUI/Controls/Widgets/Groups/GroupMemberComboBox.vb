Imports AutomateControls
Imports BluePrism.AutomateAppCore.Groups
Imports Internationalisation

''' <summary>
''' A combo box allowing group members to be selected from a tree displaying a group
''' tree.
''' </summary>
Public Class GroupMemberComboBox : Inherits GroupBasedComboBox

#Region " Properties "

    ''' <summary>
    ''' Shadow of the DisabledItemColour to hide this property from editors and
    ''' designers.
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property DisabledItemColour As Color
        Get
            Return MyBase.DisabledItemColour
        End Get
        Set(value As Color)
            MyBase.DisabledItemColour = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Adds an entry representing the given group member
    ''' </summary>
    ''' <param name="mem">The member to add an entry for</param>
    ''' <param name="indent">The indent at which to add the entry</param>
    Protected Overrides Sub AddEntry(mem As IGroupMember, indent As Integer)
        Dim gp As IGroup = TryCast(mem, IGroup)
        If gp IsNot Nothing Then
            Dim title As String = CStr(IIf(mem.Name.Equals("Default") And gp.IsDefault, My.Resources.GroupMemberComboBox_AddEntry_Default, mem.Name))
            Items.Add(New ComboBoxItem(title, mem) With {
                .Style = FontStyle.Bold,
                .Selectable = False,
                .Indent = indent,
                .Checkable = False
            })
            indent += DefaultIndent
            For Each child As IGroupMember In DirectCast(mem, IGroup)
                AddEntry(child, indent)
            Next
        Else
            Dim comboBoxItemText = mem.Name

            If TreeType = GroupTreeType.Users Then
                comboBoxItemText = CheckToLocaliseSystemUserName(mem.Name)
            End If

            Items.Add(New ComboBoxItem(comboBoxItemText, mem) With {
                .Indent = indent,
                .Checkable = True,
                .Checked = True,
                .Enabled = HasPermission(mem)
            })
        End If
    End Sub

    Private Function CheckToLocaliseSystemUserName(userName As String) As String
        If userName = "[Scheduler]" Then Return $"[{ResMan.GetString("ctlControlRoom_tv_scheduler")}]"
        If userName = "[Anonymous Resource]" Then Return $"[{My.Resources.AnonymousResource}]"
        Return userName
    End Function

    ''' <summary>
    ''' Sub classes can override this to disable members when there is no permission to use them
    ''' </summary>
    ''' <param name="mem">The member to check permissions for</param>
    Protected Overridable Function HasPermission(mem As IGroupMember) As Boolean
        Return True
    End Function


#End Region

End Class
