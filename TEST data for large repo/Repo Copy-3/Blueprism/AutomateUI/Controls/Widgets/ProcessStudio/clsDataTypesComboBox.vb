Imports AutomateControls
Imports BluePrism.AutomateProcessCore

Public Class clsDataTypesComboBox
    Inherits clsEnumeratedValueCombobox(Of DataType)

    Public Sub New()
        CreateComboBox()
        Init()
    End Sub

    ''' <summary>
    ''' Creates the inner combobox, setting its style, position, etc.
    ''' </summary>
    Private Sub CreateComboBox()
        Me.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        Me.Dock = DockStyle.Fill
        Me.BackColor = SystemColors.ControlLightLight
        Me.DisabledItemColour = Color.LightGray
        Me.DropDownBackColor = SystemColors.ControlLightLight
        Me.DropDownStyle = ComboBoxStyle.DropDownList
        Me.MaxDropDownItems = 16
    End Sub

    ''' <summary>
    ''' Repopulates the list with all available data types.
    ''' </summary>
    ''' <remarks>Use the <see cref="m:Init(DataType,Boolean)"/> method to
    ''' suppress specific data types if desired.</remarks>
    Public Sub Init()
        Init(DataType.unknown)
    End Sub

    ''' <summary>
    ''' Repopulates the list with all available data types except those specified.
    ''' </summary>
    ''' <param name="suppressed">The data types, logically or'ed together which
    ''' should be disabled in the combo box.</param>
    Public Sub Init(ByVal suppressed As DataType)
        Init(suppressed, True)
    End Sub

    ''' <summary>
    ''' Repopulates the list using all data types but the ones specified for
    ''' exclusion (ie suppression).
    ''' </summary>
    ''' <param name="suppressed">Data types to exclude from the UI. Use a logical
    ''' <c>Or</c> operation to specify more than one.</param>
    ''' <param name="displaySuppressedItems">True to display the suppressed items
    ''' in the combo box as disabled items, rather than hiding them completely.
    ''' False will remove them from the combo box.</param>
    Public Sub Init(ByVal suppressed As DataType, ByVal displaySuppressedItems As Boolean)
        ' Ensure that the items are empty
        Items.Clear()
        ' Go through all enumerated data types
        For Each dt As DataType In [Enum].GetValues(GetType(DataType))

            ' We just ignore unknown data types apparently, regardless of 'suppressed'
            If dt = DataType.unknown Then Continue For

            ' Check if this data type is suppressed
            Dim isSuppressed As Boolean = ((dt And suppressed) <> 0)

            ' If it is, and we shouldn't be displaying such items, skip it
            If isSuppressed AndAlso Not displaySuppressedItems Then Continue For

            ' If we're here, it's either unsurpressed or we're showing suppressed items
            ' Create the item
            Items.Add(New ComboBoxItem(clsProcessDataTypes.GetFriendlyName(dt), _
             DataItemColour.GetDataItemColor(dt), dt, Not isSuppressed))

        Next

    End Sub

    ''' <summary>
    ''' Gets or sets the selected data type in the user interface.
    ''' </summary>
    ''' <value>Must be one of the values available in the UI. Call
    ''' Repopulate with the appropriate suppression to make sure
    ''' that your desired data type is present. If not found then
    ''' no change is made.</value>
    ''' <returns>Gets the value as chosen in the UI, or Datatype.Unknown
    ''' if none selected.</returns>
    Public Property ChosenDataType() As DataType
        Get
            Return ChosenValue
        End Get
        Set(ByVal value As DataType)
            ChosenValue = value
        End Set
    End Property

End Class
