Imports AutomateControls
Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' Combo box which displays a hierarchical tree of groups, allowing the user to
''' select one of them
''' </summary>
Public Class GroupComboBox : Inherits GroupBasedComboBox

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the selected group member in this combo box.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedGroup As IGroup
        Get
            Return TryCast(SelectedMember, IGroup)
        End Get
        Set(value As IGroup)
            SelectedMember = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Adds an entry representing the given group. Note that any group members which
    ''' are not actually groups are ignored by this method (which neatly means that
    ''' the calling code doesn't need to filter them out first).
    ''' </summary>
    ''' <param name="mem">The member to add an entry for</param>
    ''' <param name="indent">The indent at which to add the entry</param>
    Protected Overrides Sub AddEntry(mem As IGroupMember, indent As Integer)
        ' Get the member as a group
        Dim gp As IGroup = TryCast(mem, IGroup)

        ' We're only interested in groups...
        If gp Is Nothing Then Return

        ' Add an item representing the group
        Items.Add(New ComboBoxItem(gp.Name, gp) With {.Indent = indent})

        ' Indent 1 further
        indent += DefaultIndent

        ' And add entries for each of its children
        For Each child As IGroupMember In gp : AddEntry(child, indent) : Next

    End Sub

#End Region

End Class
