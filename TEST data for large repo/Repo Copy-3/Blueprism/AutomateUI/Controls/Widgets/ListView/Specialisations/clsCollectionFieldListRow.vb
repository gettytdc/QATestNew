
Imports BluePrism.AutomateProcessCore

''' <summary>
''' Row representing a collection field, ready made with the appropriate
''' columns.
''' </summary>
Friend Class clsCollectionFieldListRow
    Inherits clsListRow

    ''' <summary>
    ''' The field info object representing the 'current' information.
    ''' </summary>
    Private mFieldInfo As clsCollectionFieldInfo

    ''' <summary>
    ''' The field info object representing the 'base' information - this is
    ''' the actual field on the collection definition that this row is
    ''' modelling. It is 'committed' to with information from the mFieldInfo
    ''' collection field when the row's <see cref="EndEdit">editing is ended
    ''' </see>.
    ''' </summary>
    ''' <remarks>The primary reason for separating the field into two
    ''' duplicates is to allow for the 'field modified' event, fired when the
    ''' collection field definition is modified, to take in all changes made
    ''' to a field at once rather than trawling through the entire collection
    ''' structure for each disparate piece of data.</remarks>
    Private mBaseFieldInfo As clsCollectionFieldInfo

    ''' <summary>
    ''' The field represented by this list row in its current (committed)
    ''' state.
    ''' <strong>Note: </strong> this object represents the actual field defn
    ''' object that is being modelled by this list row, not a local copy.
    ''' </summary>
    Public ReadOnly Property CollectionField() As clsCollectionFieldInfo
        Get
            Return mBaseFieldInfo
        End Get
    End Property

    ''' <summary>
    ''' Creates a listrow which represents the given collection field info
    ''' object.
    ''' </summary>
    ''' <param name="fldInfo">The field info to be handled by this row.</param>
    Public Sub New(ByVal lv As ctlListView, ByVal fldInfo As clsCollectionFieldInfo)
        MyBase.New(lv)
        mBaseFieldInfo = fldInfo
        mFieldInfo = DirectCast(fldInfo.Clone(), clsCollectionFieldInfo)

        If string.IsNullOrEmpty(mFieldInfo.DisplayName) Then mFieldInfo.DisplayName = mFieldInfo.Name

        Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, mFieldInfo.DisplayName)))

        Dim item As New clsListItem(Me, New clsProcessValue(DataType.text, _
         clsProcessDataTypes.GetFriendlyName(mFieldInfo.DataType)))
        item.AvailableValues = clsListItem.GetDataTypeAvailableValues()
        Items.Add(item)

        item = New clsListItem(Me, New clsProcessValue(DataType.text, mFieldInfo.Description))
        Items.Add(item)

        ' We're kinda bending clsListItem into supporting clsCollectionInfo's by
        ' wrapping one into a listitem. It's all geared towards clsProcessValue,
        ' but this should kinda work...
        Items.Add(New CollectionInfoListItem(Me, mFieldInfo))

    End Sub

    ''' <summary>
    ''' Creates an edit control for the given list item.
    ''' </summary>
    ''' <param name="item">The item for which an edit control is required.
    ''' </param>
    ''' <returns>A control used as a view for the given list item.</returns>
    Protected Overrides Function CreateEditControl(ByVal item As clsListItem) As Control
        If TypeOf item Is CollectionInfoListItem Then
            Return New ctlCollectionDefinition()
        Else
            Return MyBase.CreateEditControl(item)
        End If
    End Function

    ''' <summary>
    ''' A list item designed for viewing the child field definitions of a
    ''' specified collection field.
    ''' </summary>
    Private Class CollectionInfoListItem : Inherits clsListItem

        ''' <summary>
        ''' The preferred size of this list item
        ''' </summary>
        Private Shared ReadOnly PreferredSize As New Size(110, ctlListView.DefaultRowHeight)

        ''' <summary>
        ''' The definition whose child definition is being represented by this
        ''' list item.
        ''' </summary>
        Private mDefinition As clsCollectionFieldInfo

        ''' <summary>
        ''' Creates a new list item within the given row, representing the given
        ''' collection field definition.
        ''' </summary>
        ''' <param name="parent">The list row within which this item is held.
        ''' </param>
        ''' <param name="defn">The field definition whose children are being
        ''' represented by this item.</param>
        Public Sub New(ByVal parent As clsListRow, ByVal defn As clsCollectionFieldInfo)
            MyBase.New(parent)
            mDefinition = defn
        End Sub

        ''' <summary>
        ''' The text displayed for this ite.m
        ''' This will be empty if the field being modelled is not a collection
        ''' field, "{undefined}" if it is a collection with no defined
        ''' children fields, and a count of the fields if it is a collection
        ''' with a defined structure.
        ''' </summary>
        Protected Overrides ReadOnly Property Text() As String
            Get
                If mDefinition.DataType <> DataType.collection Then Return ""
                Return clsCollectionInfo.GetInfoLabel(mDefinition.Children)
            End Get
        End Property

        ''' <summary>
        ''' Gets the preferred size of this list item.
        ''' </summary>
        ''' <param name="proposedSize">The proposed size of the list item.
        ''' </param>
        ''' <returns>The preferred size.</returns>
        Public Overrides Function GetPreferredSize(ByVal proposedSize As Size) As Size
            Return PreferredSize
        End Function

    End Class

    ''' <summary>
    ''' Ensures that the data is saved in the internally held field info.
    ''' <strong>Note: </strong> this no longer updates the collection definition
    ''' automatically, since it needs more context - ie. it needs the definition
    ''' manager, so a parent control managing this control must capture the
    ''' <see cref="EditingEnded"/> event and tell this field to update using the
    ''' manager that the parent control is updating.
    ''' </summary>
    ''' <param name="editRow">The editable list row that contains the controls
    ''' used to edit this row.</param>
    Public Overrides Sub EndEdit(ByVal editRow As ctlEditableListRow)

        mFieldInfo.Name = editRow.Items(0).NestedControl.Text
        RemoveHandler editRow.Items(0).NestedControl.Validating, AddressOf ValidateFieldName

        Dim cb As clsDataTypesComboBox = CType(editRow.Items(1).NestedControl, clsDataTypesComboBox)
        mFieldInfo.DataType = cb.ChosenDataType
        RemoveHandler cb.SelectedIndexChanged, AddressOf HandleDataTypeChanged

        mFieldInfo.Description = editRow.Items(2).NestedControl.Text

        ' Update the actual field with the new additions.
        ' SetFrom(CollectionFieldInfo) allows all changes modification events
        ' to be propogated in a single event rather than separate events for
        ' each of the property changes.

        Try
            mBaseFieldInfo.SetFrom(mFieldInfo)

        Catch ex As Exception
            ' The validation should handle most of this stuff (better) - but if you click
            ' on a different row within the listview, that validation is skipped
            ' (due to the textbox effectively being disposed rather than tabbed
            ' away from), so this catches that eventuality.
            UserMessage.Show(
             My.Resources.clsCollectionFieldListRow_AnErrorOccurredWhileTryingToSaveTheCollectionField & ex.Message, ex)

            ' Return the row to its original state - in the field
            mFieldInfo.Name = mBaseFieldInfo.Name
            mFieldInfo.Description = mBaseFieldInfo.Description
            mFieldInfo.DataType = mBaseFieldInfo.DataType
            ' ... and in the UI
            editRow.Items(0).NestedControl.Text = mFieldInfo.Name
            DirectCast(editRow.Items(1).NestedControl, clsDataTypesComboBox).ChosenDataType = mFieldInfo.DataType
            editRow.Items(2).NestedControl.Text = mFieldInfo.Description

            ' We don't need to re-add any event handlers - the EndEdit() is
            ' continuing, but with the row set back to its original state.

        End Try

        MyBase.EndEdit(editRow)
    End Sub

    ''' <summary>
    ''' Instructs the row to populate the supplied edit row with its
    ''' data, ready for editing.
    ''' </summary>
    ''' <param name="editRow">The edit row to be used when editing this row.
    ''' </param>
    Public Overrides Sub BeginEdit(ByVal editRow As ctlEditableListRow)

        Dim fieldNameControl As ctlProcessText = DirectCast(editRow.Items(0).NestedControl, ctlProcessText)
        AddHandler fieldNameControl.Validating, AddressOf ValidateFieldName
        fieldNameControl.Value = New clsProcessValue(DataType.text, mFieldInfo.Name)
        fieldNameControl.Multiline = False

        Dim typeControl As clsDataTypesComboBox = DirectCast(editRow.Items(1).NestedControl, clsDataTypesComboBox)
        typeControl.ChosenDataType = mFieldInfo.DataType
        AddHandler typeControl.SelectedIndexChanged, AddressOf HandleDataTypeChanged

        Dim fieldDescriptionControl As ctlProcessText = DirectCast(editRow.Items(2).NestedControl, ctlProcessText)
        fieldDescriptionControl.Value = New clsProcessValue(DataType.text, mFieldInfo.Description)
        fieldDescriptionControl.Multiline = False

        ' Set the collection definition into the collection defn control
        Dim ctl As ctlCollectionDefinition =
         DirectCast(editRow.Items(3).NestedControl, ctlCollectionDefinition)
        If mFieldInfo.DataType = DataType.collection Then
            ctl.Definition = mFieldInfo.Children
        Else
            ctl.Definition = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Handler for the data type being changed in the editable list row
    ''' representing this collection field list row.
    ''' This ensures that the collection definition item is updated with
    ''' the new data type so that it can correctly display itself.
    ''' </summary>
    Private Sub HandleDataTypeChanged(ByVal sender As Object, ByVal e As EventArgs)

        Dim editRow As ctlEditableListRow = Owner.EditRowControl

        Dim dt As DataType =
         DirectCast(editRow.Items(1).NestedControl, clsDataTypesComboBox).ChosenDataType

        Dim ctl As ctlCollectionDefinition =
         DirectCast(editRow.Items(3).NestedControl, ctlCollectionDefinition)

        If dt = DataType.collection Then
            ctl.Definition = mFieldInfo.Children
        Else
            ctl.Definition = Nothing
        End If

    End Sub

    ''' <summary>
    ''' Validates the field name for the collection.
    ''' </summary>
    Private Sub ValidateFieldName(ByVal sender As Object, ByVal e As CancelEventArgs)
        Dim ctl As Control = DirectCast(sender, Control)

        ' Check that we have a valid field name
        If Not clsCollectionInfo.IsValidFieldName(ctl.Text) Then
            UserMessage.Show(
             My.Resources.clsCollectionFieldListRow_CollectionFieldNamesCannotContainAnyOfTheCharactersOr)
            e.Cancel = True

            ' Valid name - Check that it's not a duplicate - we can only do this if the field is
            ' on a collection definition
        ElseIf mBaseFieldInfo.Parent IsNot Nothing AndAlso
         mBaseFieldInfo.Name <> ctl.Text AndAlso mBaseFieldInfo.Parent.Contains(ctl.Text) Then
            UserMessage.Show(String.Format(
             My.Resources.clsCollectionFieldListRow_TheCollectionAlreadyHasAFieldNamed0, ctl.Text))
            e.Cancel = True

        End If

    End Sub

    ''' <summary>
    ''' Creates a picker control for the given list item.
    ''' This just creates an instance of <see cref="clsDataTypesComboBox"/>
    ''' with no suppressed data types.
    ''' </summary>
    ''' <param name="Item">The item for which a picker control is required.
    ''' </param>
    ''' <returns>A combo box from which the appropriate data type can be 
    ''' selected.</returns>
    Protected Overrides Function CreatePickerControl(ByVal Item As clsListItem) As Control
        Return New clsDataTypesComboBox()
    End Function

End Class
