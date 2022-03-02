Public Class SecurePasswordEditingControl
    Inherits ctlAutomateSecurePassword
    Implements IDataGridViewEditingControl

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Changes the control's user interface (UI) to be consistent with the specified
    '''  cell style.
    ''' </summary>
    ''' <param name="dataGridViewCellStyle">The DataGridViewCellStyle to use as the
    '''  model for the UI.</param>
    Public Sub ApplyCellStyleToEditingControl( _
                dataGridViewCellStyle As DataGridViewCellStyle) _
                Implements IDataGridViewEditingControl.ApplyCellStyleToEditingControl
        Font = dataGridViewCellStyle.Font
        ForeColor = dataGridViewCellStyle.ForeColor
        BackColor = dataGridViewCellStyle.BackColor
    End Sub

    ''' <summary>
    ''' Determines whether the specified key is a regular input key that the editing
    '''  control should process or a special key that the DataGridView should process.
    ''' </summary>
    ''' <param name="keyData">A Keys that represents the key that was pressed.</param>
    ''' <param name="dataGridViewWantsInputKey">true when the DataGridView wants to 
    ''' process the Keys in keyData; otherwise, false.</param>
    ''' <returns>true if the specified key is a regular input key that should be 
    ''' handled by the editing control; otherwise, false.</returns>
    Public Function EditingControlWantsInputKey(keyData As Keys, _
                    dataGridViewWantsInputKey As Boolean) As Boolean _
                Implements IDataGridViewEditingControl.EditingControlWantsInputKey
        Return Not dataGridViewWantsInputKey
    End Function


    ''' <summary>
    ''' Retrieves the formatted value of the cell.
    ''' </summary>
    ''' <param name="context">A bitwise combination of DataGridViewDataErrorContexts 
    ''' values that specifies the context in which the data is needed.</param>
    ''' <returns>An Object that represents the formatted version of the cell 
    ''' contents.</returns>
    Public Function GetEditingControlFormattedValue( _
                context As DataGridViewDataErrorContexts) As Object _
            Implements IDataGridViewEditingControl.GetEditingControlFormattedValue
        Return Me.Text
    End Function


    ''' <summary>
    ''' Prepares the currently selected cell for editing.
    ''' </summary>
    ''' <param name="selectAll">true to select all of the cell's content; otherwise,
    '''  false.</param>
    Public Sub PrepareEditingControlForEdit(selectAll As Boolean) _
            Implements IDataGridViewEditingControl.PrepareEditingControlForEdit
        SelectionStart = Text.Length
    End Sub

    ''' <summary>
    ''' Gets or sets the DataGridView that contains the cell.
    ''' </summary>
    Public Property EditingControlDataGridView As DataGridView _
        Implements IDataGridViewEditingControl.EditingControlDataGridView

    ''' <summary>
    ''' Gets or sets the formatted value of the cell being modified by the editor.
    ''' </summary>
    Public Property EditingControlFormattedValue As Object _
        Implements IDataGridViewEditingControl.EditingControlFormattedValue
        Get
            Return Me.Text
        End Get
        Set(value As Object)
            Text = value.ToString()
        End Set
    End Property


    ''' <summary>
    ''' Gets or sets the index of the hosting cell's parent row.
    ''' </summary>
    Public Property EditingControlRowIndex As Integer _
        Implements IDataGridViewEditingControl.EditingControlRowIndex

    ''' <summary>
    ''' Gets or sets a value indicating whether the value of the editing control 
    ''' differs from the value of the hosting cell.
    ''' </summary>
    Public Property EditingControlValueChanged As Boolean _
        Implements IDataGridViewEditingControl.EditingControlValueChanged

    ''' <summary>
    ''' Gets the cursor used when the mouse pointer is over the 
    ''' DataGridView.EditingPanel but not over the editing control.
    ''' </summary>
    Public ReadOnly Property EditingPanelCursor As Cursor _
        Implements IDataGridViewEditingControl.EditingPanelCursor
        Get
            Return Cursors.IBeam
        End Get
    End Property


    ''' <summary>
    ''' Gets or sets a value indicating whether the cell contents need to be 
    ''' repositioned whenever the value changes.
    ''' </summary>
    Public ReadOnly Property RepositionEditingControlOnValueChange As Boolean _
        Implements IDataGridViewEditingControl.RepositionEditingControlOnValueChange
        Get
            Return False
        End Get
    End Property

End Class
