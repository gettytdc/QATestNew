''' -----------------------------------------------------------------------------
''' Project  : Automate
''' Class    : clsComboBoxWithTags
''' 
''' -----------------------------------------------------------------------------
''' <summary>
''' An enhancement of the existing combobox included in the framework.
''' Allows the items to carry a tag.
''' </summary>
''' -----------------------------------------------------------------------------
Public Class clsComboBoxWithTags
    Inherits ComboBox


    ''' #######################################################
    ''' WARNING:
    ''' 
    ''' It was not possible to override the items()
    ''' property of the combobox class so you have to be careful
    ''' not to write 
    ''' 
    ''' dim bob as new clsComboBoxWithTags
    ''' bob.items.add("stuff")
    ''' 
    ''' but to write
    ''' 
    ''' dim bob as new clsComboBoxWithTags
    ''' dim bill as new clsComboBoxItem
    ''' bill.text = "stuff"
    ''' bill.comboboxitemtag = objOfYourChoosing
    ''' bob.additem(bill)
    ''' 
    ''' instead.
    ''' ###################################################



#Region "Members"

    ''' <summary>
    ''' Private member for referencing tags against items in the combobox.
    ''' </summary>
    Private mhtTags As Hashtable

#End Region

#Region "Constructor"

    Public Sub New()
        mhtTags = New Hashtable
    End Sub

#End Region


#Region "Public Methods"

    ''' <summary>
    ''' Retrieves a tag from the item having the supplied text.
    ''' </summary>
    ''' <param name="ItemText">The text of the item whose tag is sought.</param>
    ''' <returns>Returns the tag sought.</returns>
    Public Function GetTagForItemWithText(ByVal ItemText As String) As Object
        Return mhtTags(ItemText)
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Returns the tag behind the selected object
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------------
    Public Function SelectedItemTag() As Object
        Dim selecteditemname As String = CStr(Me.SelectedItem)
        If Not selecteditemname Is Nothing Then
            Return GetTagForItemWithText(selecteditemname)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Adds an item to the combobox, and manages the tag. This method must
    ''' be used instead of accessing the .items property.
    ''' </summary>
    ''' <param name="ComboBoxItem">The item to be added.</param>
    ''' <returns>Returns the index of the combobox that the item will appear as.</returns>
    Public Function AddItem(ByVal ComboBoxItem As clsComboBoxItem) As Integer
        'disallow duplicate text
        For Each s As String In Me.Items
            If s = ComboBoxItem.Text Then Return -1
        Next

        'if not duplicate then return index
        Me.mhtTags.Add(ComboBoxItem.Text, ComboBoxItem.ComboboxItemTag)
        Return Me.Items.Add(ComboBoxItem.Text)
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Allows you to add an item and tag in one hit.
    ''' </summary>
    ''' <param name="name">the name of the item</param>
    ''' <param name="Tag">the tag object</param>
    ''' <returns>Returns the index of the combobox that the item will appear as.</returns>
    ''' -----------------------------------------------------------------------------
    Public Function AddItem(ByVal name As String, ByVal Tag As Object) As Integer
        Dim item As New clsComboBoxItem
        item.Text = name
        item.ComboboxItemTag = Tag
        Return Me.AddItem(item)
    End Function

#End Region

    Protected Overrides Sub RefreshItem(ByVal index As Integer)

    End Sub

    Protected Overrides Sub SetItemsCore(ByVal items As System.Collections.IList)

    End Sub
End Class
