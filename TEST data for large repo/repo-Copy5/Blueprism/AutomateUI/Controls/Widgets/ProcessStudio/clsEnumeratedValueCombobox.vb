

Imports AutomateControls

''' <summary>
''' ComboBox which models a set of enumerated values of a specified type.
''' </summary>
''' <typeparam name="T">The type of values to be enumerated into a combobox
''' </typeparam>
Public Class clsEnumeratedValueCombobox(Of T) : Inherits StyledComboBox

    ''' <summary>
    ''' Creates a new enumerated value combo box.
    ''' </summary>
    Public Sub New()
        DropDownStyle = ComboBoxStyle.DropDownList
        DropDownBackColor = SystemColors.ControlLightLight
        DisabledItemColour = Color.LightGray
    End Sub

    ''' <summary>
    ''' Gets or sets the selected data type in the user interface.
    ''' </summary>
    ''' <value>Must be one of the values available in the UI. If not found then no
    ''' change is made.</value>
    ''' <returns>Gets the value as chosen in the UI, or Datatype.Unknown if none is
    ''' selected.</returns>
    Public Property ChosenValue() As T
        Get
            Dim item As ComboBoxItem = TryCast(SelectedItem, ComboBoxItem)
            If item IsNot Nothing Then Return DirectCast(item.Tag, T)
            Return Nothing
        End Get
        Set(ByVal value As T)
            SelectedItem = FindComboBoxItemByTag(value)
        End Set
    End Property

End Class
