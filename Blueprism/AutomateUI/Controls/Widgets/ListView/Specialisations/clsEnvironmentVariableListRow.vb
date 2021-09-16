
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore

''' <summary>
''' Row representing an environment variable, ready made with the appropriate columns
''' </summary>
Friend Class clsEnvironmentVariableListRow
    Inherits clsListRow


    ''' <summary>
    ''' The environment variable associated with this list row.
    ''' </summary>
    Public ReadOnly Property EnvironmentVariable() As clsEnvironmentVariable
        Get
            Return mVariable
        End Get
    End Property
    Private mVariable As clsEnvironmentVariable

    ''' <summary>
    ''' Creates a listrow which represents the given collection field info
    ''' object.
    ''' </summary>
    Public Sub New(ByVal lv As ctlListView, ByVal variable As clsEnvironmentVariable)
        MyBase.New(lv)
        mVariable = variable
        Items.Add(New clsListItem(Me, New clsProcessValue(DataType.text, variable.Name)))

        Dim item As New clsListItem(Me, New clsProcessValue(DataType.text, _
         clsProcessDataTypes.GetFriendlyName(variable.DataType)))
        item.AvailableValues = clsListItem.GetDataTypeAvailableValues()
        Items.Add(item)

        item = New clsListItem(Me, New clsProcessValue(DataType.text, variable.Description))
        Items.Add(item)

        item = New clsListItem(Me, variable.Value.Clone())

        Items.Add(item)

    End Sub

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

        mVariable.Name = editRow.Items(0).NestedControl.Text

        'No need to set the data type - it's implicit in the value of the variable

        mVariable.Description = editRow.Items(2).NestedControl.Text

        mVariable.Value = DirectCast(editRow.Items(3).NestedControl, IProcessValue).Value

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
        fieldNameControl.Value = New clsProcessValue(DataType.text, mVariable.Name)
        fieldNameControl.Multiline = False

        Dim typeControl As clsDataTypesComboBox = DirectCast(editRow.Items(1).NestedControl, clsDataTypesComboBox)
        typeControl.ChosenDataType = mVariable.DataType
        AddHandler typeControl.SelectedIndexChanged, AddressOf HandleDataTypeChanged

        Dim fieldDescriptionControl As ctlProcessText = DirectCast(editRow.Items(2).NestedControl, ctlProcessText)
        fieldDescriptionControl.Value = New clsProcessValue(DataType.text, mVariable.Description)
        fieldDescriptionControl.Multiline = False

        Dim fieldValueControl As IProcessValue = DirectCast(editRow.Items(3).NestedControl, IProcessValue)
        fieldValueControl.Value = mVariable.Value

    End Sub

    ''' <summary>
    ''' Handler for the data type being changed in the editable list row
    ''' representing this collection field list row.
    ''' This ensures that the collection definition item is updated with
    ''' the new data type so that it can correctly display itself.
    ''' </summary>
    Private Sub HandleDataTypeChanged(ByVal sender As Object, ByVal e As EventArgs)

        Dim editRow As ctlEditableListRow = Owner.EditRowControl

        Dim dt As DataType = _
         DirectCast(editRow.Items(1).NestedControl, clsDataTypesComboBox).ChosenDataType

        editRow.Items(3).NestedControl = clsProcessValueControl.GetControl(dt)

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
        Dim combo As New clsDataTypesComboBox()
        ' DataType is a [Flags] enum? That makes absolutely no sense.
        combo.Init(DataType.unknown Or DataType.collection, False)
        Return combo
    End Function

End Class
