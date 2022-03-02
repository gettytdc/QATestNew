Imports AutomateControls
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports BluePrism.AutomateProcessCore
Imports ActionParameter = BluePrism.AutomateProcessCore.WebApis.ActionParameter
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Panel used to view and edit parameters within a Web API
''' </summary>
Friend Class WebApiParametersPanel : Implements IGuidanceProvider, IRequiresValidation

    ' The headers being edited in this panel
    Private mParams As WebApiCollection(Of ActionParameter)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ColDataType.DataSource = clsProcessDataTypes.GetAll().Select(Function(d) New DataTypeComboBoxItem(d.Value)).ToList()
        ColDataType.DisplayMember = NameOf(DataTypeComboBoxItem.Title)
        ColDataType.ValueMember = NameOf(DataTypeComboBoxItem.Type)

    End Sub

    ''' <summary>
    ''' Gets the guidance text for this panel.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property GuidanceText As String _
     Implements IGuidanceProvider.GuidanceText
        Get
            Return If(mParams?.ActionSpecific,
                WebApi_Resources.GuidanceActionSpecificParametersPanel,
                WebApi_Resources.GuidanceParametersPanel
            )
        End Get
    End Property

    ''' <summary>
    ''' Gets and sets the parameters used in this panel
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend WriteOnly Property Parameters As WebApiCollection(Of ActionParameter)

        Set(value As WebApiCollection(Of ActionParameter))
            mParams = value
            ClearGrid()
            If value Is Nothing Then Return
            For Each param In value
                Dim rowIndex = gridParams.Rows.Add(
                    Nothing,
                    param.Name,
                    param.Description,
                    param.DataType,
                    param.InitialValue,
                    param.ExposeToProcess)

                AddDataControlToGrid(param, param.DataType, rowIndex)
                gridParams.Rows(rowIndex).Tag = param
            Next
        End Set
    End Property

    ''' <summary>
    ''' Handles the validating of a cell, ensuring that any parameter name is unique
    ''' within the scope of this panel.
    ''' </summary>
    Private Sub HandleCellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs) _
     Handles gridParams.CellValidating

        Dim paramName = If(e.ColumnIndex = colName.Index,
                                If(CStr(e.FormattedValue), ""),
                                gridParams.CurrentRow.GetStringValue(colName))

        Dim type = If(e.ColumnIndex = ColDataType.Index,
                                ColDataType.Items.OfType(Of DataTypeComboBoxItem)().FirstOrDefault(Function(c) c.Title = CStr(e.FormattedValue))?.Type,
                                CType(gridParams.CurrentRow.Cells().Item(ColDataType.Index).Value, DataType))

        If RowIsEmpty(paramName, If(type, DataType.unknown)) Then Return

        Try
            If e.ColumnIndex = colName.Index Then
                ValidateParameterName(paramName, e.RowIndex)
            End If
        Catch ex As Exception
            UserMessage.Err(ex.Message)
            e.Cancel = True
        End Try

    End Sub

    ''' <summary>
    ''' Updates the collection with the data from this panel
    ''' </summary>
    Private Sub UpdateCollection()
        mParams.Clear()
        For Each row As DataGridViewRow In gridParams.Rows

            Dim name = row.GetStringValue(colName)

            Dim comboboxCell = DirectCast(row.Cells(ColDataType.Index), DataGridViewComboBoxCell)
            Dim selectedDataType = DirectCast(If(comboboxCell.Value, DataType.unknown), DataType)

            Try
                ValidateDataType(selectedDataType)
                ValidateParameterName(name)
            Catch
                'Don't admit this parameter to the collection
                Continue For
            End Try

            Dim param = TryCast(row.Tag, ActionParameter)

            ' If no Action Parameter stored in the tag it must mean a new row, so
            ' set the id to be 0
            Dim id = If(param?.Id, 0)
            Dim desc = row.GetStringValue(colDescription)
            Dim exposed = CBool(row.Cells(colExpose.Index).Value)
            Dim processValue = TryCast(row.Cells(colValue.Index).Tag, IProcessValue)

            Dim processValueToSave = If(processValue?.Value.EncodedValue Is Nothing,
                                        New clsProcessValue(""),
                                        processValue.Value)

            mParams.Add(New ActionParameter(
                id,
                name,
                If(desc, String.Empty),
                selectedDataType,
                exposed,
                processValueToSave)
            )
        Next
    End Sub

    ''' <summary>
    ''' Handles the data grid view being validated, ensuring that the underlying
    ''' collection is updated with the modified data
    ''' </summary>
    Private Sub HandleDataGridViewValidated(sender As Object, e As EventArgs) _
     Handles gridParams.Validated

        UpdateCollection()
    End Sub

    ''' <summary>
    ''' Handles adding the bin icon to the first cell in a row.
    ''' </summary>
    Private Sub gridParams_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) _
        Handles gridParams.CellFormatting

        If e.ColumnIndex = colDelete.Index Then
            e.Value = ToolImages.Bin_16x16
            gridParams.Rows(e.RowIndex).Cells(e.ColumnIndex).ToolTipText = WebApi_Resources.RemoveParameter
        End If

    End Sub

    ''' <summary>
    ''' Removes a row when the user clicks on the delete column cell.
    ''' </summary>    
    Private Sub gridParams_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
        Handles gridParams.CellContentClick

        Dim shouldDelete = e.ColumnIndex = colDelete.Index AndAlso
                                gridParams.Rows.Count > 1 AndAlso
                                e.RowIndex <> gridParams.Rows.Count - 1

        If Not shouldDelete Then Return

        Dim oldControl = TryCast(gridParams.Rows(e.RowIndex).Cells(colValue.Index).Tag, Control)
        oldControl?.Dispose()
        gridParams.Rows.RemoveAt(e.RowIndex)

    End Sub

    ''' <summary>
    ''' Handles placing controls which have been added to the data grid to the
    ''' correct position when the Cell Painting event is fired.
    ''' </summary>
    Private Sub gridParams_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) _
        Handles gridParams.CellPainting

        PositionGridSubControls()
    End Sub

    ''' <summary>
    ''' Handles the cell value combo box being changed and updates the clsProcessControl
    ''' if needed.
    ''' </summary>
    Private Sub gridParams_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) _
        Handles gridParams.CellValueChanged

        If gridParams.CurrentRow Is Nothing Then Return

        Dim currentRowIndex = gridParams.CurrentRow.Index
        Dim updatedCombo As DataGridViewComboBoxCell = TryCast(gridParams.CurrentRow.Cells(ColDataType.Index), DataGridViewComboBoxCell)
        If updatedCombo Is Nothing Then Return

        gridParams.Invalidate()

        Dim currentValue = TryCast(gridParams.Rows(currentRowIndex).Cells(colValue.Index).Tag, IProcessValue)?.Value

        Dim selectedDataType = If(TypeOf updatedCombo.Value Is DataTypeComboBoxItem,
                                  DirectCast(updatedCombo.Value, DataTypeComboBoxItem).Type,
                                  DirectCast(updatedCombo.Value, DataType))

        ' We already have the correct control in the cell.
        If currentValue?.DataType = selectedDataType Then Return

        Dim parameter = TryCast(gridParams.CurrentRow.Tag, ActionParameter)
        AddDataControlToGrid(parameter, selectedDataType, currentRowIndex)

        PositionGridSubControls()

    End Sub

    ''' <summary>
    ''' Handles actual data changes in the cells.
    ''' </summary>
    Private Sub gridParams_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) _
        Handles gridParams.CurrentCellDirtyStateChanged

        If Not gridParams.IsCurrentCellDirty Then Return

        gridParams.CommitEdit(DataGridViewDataErrorContexts.Commit)

    End Sub

    ''' <summary>
    ''' Removes any previous rows used in this grid as well as any child components used for 
    ''' editing clsProcessValues.
    ''' </summary>
    Private Sub ClearGrid()

        For Each row As DataGridViewRow In gridParams.Rows
            Dim control = TryCast(row.Cells(colValue.Index).Tag, Control)
            control?.Dispose()
        Next

        gridParams.Controls.Clear()
        gridParams.Rows.Clear()

    End Sub

    ''' <summary>
    ''' Adds a control for editing the clsProcessValue to the grid as a child control.
    ''' </summary>
    ''' <param name="parameter">The ActionParameter being used to populate the control.</param>
    ''' <param name="chosenDataType">The data type in question which will be used to choose the
    ''' control type.</param>
    ''' <param name="rowIndex">The row index for the row we are manipulating.</param>
    Private Sub AddDataControlToGrid(parameter As ActionParameter, chosenDataType As DataType, rowIndex As Integer)

        Dim oldControl = TryCast(gridParams.Rows(rowIndex)?.Cells(colValue.Index).Tag, Control)
        oldControl?.Dispose()

        Dim dataControl = clsProcessValueControl.GetControl(chosenDataType)
        gridParams.Controls.Add(dataControl)
        gridParams.Rows(rowIndex).Cells(colValue.Index).Tag = dataControl

        If chosenDataType = DataType.collection AndAlso TypeOf dataControl Is ctlProcessCollection Then _
            dataControl.Enabled = False

        ' If changing back to a data type that was given when the form was loaded up for this parameter
        ' load the original value back in.
        If parameter?.DataType = chosenDataType Then
            Dim valueControl = DirectCast(dataControl, IProcessValue)
            valueControl.Value = parameter.InitialValue
        End If

    End Sub

    ''' <summary>
    ''' Repositions grid sub controls to their respective cells where they are tagged.
    ''' </summary>
    Private Sub PositionGridSubControls()

        For Each row As DataGridViewRow In gridParams.Rows
            Dim valueControl = If(TryCast(row.Cells(colValue.Index).Tag, Control),
                              clsProcessValueControl.GetControl(DataType.unknown))

            Dim cellRectangle = gridParams.GetCellDisplayRectangle(colValue.Index, row.Index, True)
            valueControl.Location = New Point(cellRectangle.X, cellRectangle.Y)
            valueControl.Size = New Size(cellRectangle.Width - 1, cellRectangle.Height - 1)
            valueControl.Visible = True
        Next

    End Sub

    Private Sub gridParams_DefaultValuesNeeded(sender As Object, e As DataGridViewRowEventArgs) _
        Handles gridParams.DefaultValuesNeeded

        e.Row.SetValues(Nothing, String.Empty, String.Empty, DataType.unknown, String.Empty, False)

    End Sub

    ''' <inheritdoc/>
    Friend Sub ValidateParameters() Implements IRequiresValidation.Validate
        Try
            For Each row In gridParams.Rows.OfType(Of DataGridViewRow)()
                ValidateRow(row)
            Next
        Catch ex As Exception
            Dim message = String.Format(WebApi_Resources.ErrorValidatingWebApiCustomParameters_Template, ex.Message)
            Throw New InvalidStateException(message)
        End Try
    End Sub

    Private Sub ValidateDataType(value As DataType)
        If value = DataType.unknown Then _
            Throw New InvalidArgumentException(WebApi_Resources.ErrorInvalidDataType)
    End Sub


    Private Sub ValidateRow(row As DataGridViewRow)

        If RowIsEmpty(row) Then Return

        ValidateParameterName(row.GetStringValue(colName), row.Index)
        ValidateDataType(DirectCast(row.Cells().Item(ColDataType.Index).Value, DataType))
    End Sub

    Private Sub ValidateParameterName(value As String, rowIndex As Integer)

        ValidateParameterName(value)

        Dim otherRows = gridParams.Rows.OfType(Of DataGridViewRow)().
                                Where(Function(row) row.Index <> rowIndex)

        Dim methodParameterName = CodeCompiler.GetIdentifier(value)

        Dim matchingRow = gridParams.
                                Rows.
                                OfType(Of DataGridViewRow)().
                                Where(Function(row) row.Index <> rowIndex).
                                FirstOrDefault(Function(row)
                                                   Dim paramName = If(row.GetStringValue(colName.Index), String.Empty)
                                                   Return methodParameterName.
                                                                Equals(CodeCompiler.GetIdentifier(paramName),
                                                                        StringComparison.OrdinalIgnoreCase)
                                               End Function)
        If matchingRow IsNot Nothing Then _
            Throw New InvalidOperationException(String.Format(WebApi_Resources.ErrorDuplicateParameter_Template, matchingRow.Index + 1))
    End Sub

    Private Sub ValidateParameterName(value As String)

        If value.Contains(".") Then _
            UserMessage.Err(WebApi_Resources.ErrorInvalidParameterName)

        If value = "" Then _
            Throw New InvalidArgumentException(WebApi_Resources.ErrorEmptyParameter)
    End Sub

    Private Function RowIsEmpty(row As DataGridViewRow) As Boolean
        Return RowIsEmpty(row.GetStringValue(colName),
                          CType(row.Cells().Item(ColDataType.Index).Value, DataType))

    End Function

    Private Function RowIsEmpty(paramName As String, type As DataType) As Boolean
        Return paramName = "" AndAlso
                type = DataType.unknown

    End Function
End Class
