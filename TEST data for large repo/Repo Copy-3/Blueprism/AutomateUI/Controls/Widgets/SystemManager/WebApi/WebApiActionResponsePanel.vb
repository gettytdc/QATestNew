Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models
Imports Newtonsoft.Json.Linq

Namespace Controls.Widgets.SystemManager.WebApi

    ''' <summary>
    ''' Creates a new action response panel
    ''' </summary>
    Friend Class WebApiActionResponsePanel
        Implements IGuidanceProvider, IRequiresValidation

        Private Const BaseJsonPath As String = "$."

        Public Event CodeChanged As CustomCodeChangedEventHandler

        Private mPreviousValues As ResponseDataGridRowValues
        Private mResponse As WebApiActionResponseDetails
        Private WithEvents mCustomCodeControl As CustomCodeControl

        Public Sub New()

            InitializeComponent()

            colDataType.DataSource = clsProcessDataTypes.GetAll().
                Select(Function(d) New DataTypeComboBoxItem(d.Value)).
                ToList()
            colDataType.DisplayMember = NameOf(DataTypeComboBoxItem.Title)
            colDataType.ValueMember = NameOf(DataTypeComboBoxItem.Type)

            colMethodType.DataSource = System.Enum.GetValues(GetType(OutputMethodType)).
                Cast(Of OutputMethodType).
                Select(Function(pt) New MethodTypeComboBoxItem(pt)).
                ToList()
            colMethodType.DisplayMember = NameOf(MethodTypeComboBoxItem.Title)
            colMethodType.ValueMember = NameOf(MethodTypeComboBoxItem.Type)
        End Sub

        ''' <summary>
        ''' Gets the guidance text for this panel.
        ''' </summary>
        <Browsable(False),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property GuidanceText As String _
         Implements IGuidanceProvider.GuidanceText
            Get
                Return WebApi_Resources.GuidanceActionResponsePanel
            End Get
        End Property

        ''' <summary>
        ''' Gets or set the response for this panel
        ''' </summary>
        ''' <returns></returns>
        Public Property Response As WebApiActionResponseDetails
            Get
                Return mResponse
            End Get
            Set(value As WebApiActionResponseDetails)
                mResponse = value

                gridCustomOutputParameters.Rows.Clear()
                If value Is Nothing Then Return

                For Each param In value.CustomOutputParameters.OfType(Of ResponseOutputParameter)
                    Dim index = gridCustomOutputParameters.Rows.Add(
                        Nothing,
                        param.Name,
                        param.Description,
                        param.DataType,
                        param.Type,
                        param.Path)

                    If (param.Type <> OutputMethodType.JsonPath) Then
                        gridCustomOutputParameters.Rows(index).Cells(colJsonPath.Index).ReadOnly = True
                    End If
                    gridCustomOutputParameters.Rows(index).Tag = param.Id
                Next

                Dim methodDefinition = GetMethodDefinition(value)

                mCustomCodeControl = New CustomCodeControl(value.Action.Api.CommonCode, methodDefinition) With {
                    .Dock = DockStyle.Fill
                }
                With ResponseParameterSplitPanel.Panel2.Controls
                    .Clear()
                    .Add(mCustomCodeControl)
                End With
            End Set
        End Property

        ''' <summary>
        ''' Get a method definition to edit in the custom code control, based on the action
        ''' details and the custom code parameters currently in the grid
        ''' </summary>
        Private Function GetMethodDefinition(response As WebApiActionResponseDetails) As MethodDefinition

            Dim codeParameters = gridCustomOutputParameters.
                                    Rows.
                                    OfType(Of DataGridViewRow).
                                    Select(Function(r) ValuesFrom(r)).
                                    Where(Function(r) r.MethodType = OutputMethodType.CustomCode).
                                    Select(Function(r) New CustomCodeOutputParameter(r.ParamName, r.Description, r.Type)).
                                    OfType(Of CustomCodeOutputParameter).ToList()


            If Not codeParameters.Any() Then
                Dim paramName = OutputParameterMethod.GetResponseContentParameterName(Enumerable.Empty(Of IParameter))

                Return New MethodDefinition("NewCode", "NewFileName", If(response.Code, String.Empty),
                    New List(Of MethodParameterDefinition) From {
                        New MethodParameterDefinition(paramName, DataType.text, isOutput:=False)
                    })
            End If

            Dim configBuilder = New WebApiConfigurationBuilder()
            Dim config = configBuilder.
                            WithParameters(response.Action.Api.CommonParameters).
                            WithAction(mResponse.Action.Name, mResponse.Action.Request.Method,
                                   mResponse.Action.Request.UrlPath,
                                   outputParameterConfiguration:=New OutputParameterConfiguration(codeParameters, response.Code)).
                            WithCommonCode(response.Action.Api.CommonCode).
                            Build()

            Dim methodDefinitionHelper = New OutputParameterMethod()
            Return methodDefinitionHelper.CreateMethods(config).FirstOrDefault()
        End Function


        Private Function ValuesFrom(row As DataGridViewRow) As ResponseDataGridRowValues
            Return New ResponseDataGridRowValues(
                row.Index,
                If(row.GetStringValue(colParameter), String.Empty),
                If(row.GetStringValue(colDescription), String.Empty),
                CType(row.Cells().Item(colDataType.Index).Value, DataType),
                CType(row.Cells().Item(colMethodType.Index).Value, OutputMethodType),
                If(row.GetStringValue(colJsonPath), String.Empty)
            )
        End Function


        ''' <inheritdoc/>
        Friend Sub ValidateAllRows() Implements IRequiresValidation.Validate
            Try
                For Each row In gridCustomOutputParameters.Rows.OfType(Of DataGridViewRow)()
                    ValidateRow(ValuesFrom(row))
                Next
            Catch ex As Exception
                Dim message = String.Format(WebApi_Resources.ErrorValidatingWebApiCustomOutputParameters_Template, ex.Message)
                Throw New InvalidStateException(message)
            End Try
        End Sub

        Private Sub ValidateRow(rowValues As ResponseDataGridRowValues)
            If RowIsEmpty(rowValues) Then Return

            ValidateParameterName(rowValues.ParamName, rowValues.Index)
            ValidateDataType(rowValues.Type)
            ValidateMethodType(rowValues.MethodType)
            ValidatePath(rowValues.JsonPath, rowValues.MethodType)
        End Sub

        Private Sub ValidateParameterName(value As String, rowIndex As Integer)
            If String.IsNullOrEmpty(value) Then
                Throw New InvalidArgumentException(WebApi_Resources.ErrorEmptyParameter)
            End If

            Dim methodParameterName = CodeCompiler.GetIdentifier(value)

            Dim matchingRow = gridCustomOutputParameters.
                                    Rows.
                                    OfType(Of DataGridViewRow)().
                                    Where(Function(row) row.Index <> rowIndex).
                                    FirstOrDefault(Function(row)
                                                       Dim paramName = If(row.GetStringValue(colParameter.Index), String.Empty)
                                                       Return methodParameterName.
                                                                    Equals(CodeCompiler.GetIdentifier(paramName),
                                                                            StringComparison.OrdinalIgnoreCase)
                                                   End Function)

            If matchingRow IsNot Nothing Then
                gridCustomOutputParameters.CurrentCell = gridCustomOutputParameters(colParameter.Index, gridCustomOutputParameters.CurrentCell.RowIndex)
                Throw New InvalidOperationException(String.Format(WebApi_Resources.ErrorDuplicateParameter_Template, matchingRow.Index + 1))
            End If
        End Sub

        Private Sub ValidateMethodType(type As OutputMethodType)
            If type = OutputMethodType.None Then
                gridCustomOutputParameters.CurrentCell = gridCustomOutputParameters(colMethodType.Index, gridCustomOutputParameters.CurrentCell.RowIndex)
                Throw New InvalidOperationException(WebApi_Resources.ErrorInvalidParameterType)
            End If
        End Sub

        Private Sub ValidatePath(path As String, methodType As OutputMethodType)
            If methodType <> OutputMethodType.JsonPath Then Return

            If String.IsNullOrEmpty(path) Then Throw New InvalidArgumentException(WebApi_Resources.ErrorInvalidPath)

            Dim testToken = JToken.Parse("{}")
            Try
                testToken.SelectToken(path)
            Catch ex As InvalidOperationException
                gridCustomOutputParameters.CurrentCell = gridCustomOutputParameters(colJsonPath.Index, gridCustomOutputParameters.CurrentCell.RowIndex)
                Throw New InvalidOperationException(WebApi_Resources.ErrorInvalidPath)
            End Try

        End Sub

        Private Sub ValidateDataType(value As DataType)
            If value = DataType.unknown Then
                gridCustomOutputParameters.CurrentCell = gridCustomOutputParameters(colDataType.Index, gridCustomOutputParameters.CurrentCell.RowIndex)
                Throw New InvalidOperationException(WebApi_Resources.ErrorInvalidDataType)
            End If
        End Sub

        Private Sub HandleRowValidating(sender As Object, e As DataGridViewCellCancelEventArgs) _
            Handles gridCustomOutputParameters.RowValidating

            Dim row = gridCustomOutputParameters.Rows.Item(e.RowIndex)
            Dim rowValues = ValuesFrom(row)

            If RowIsEmpty(rowValues) OrElse rowValues.Equals(mPreviousValues) Then Return

            Try
                ValidateRow(rowValues)

                mCustomCodeControl.UpdateMethodDefinition(GetMethodDefinition(Response))
            Catch ex As Exception
                Dim message = String.Format(WebApi_Resources.ErrorValidatingWebApiCustomOutputParameters_Template, ex.Message)
                UserMessage.Err(message)
                e.Cancel = True
            End Try
        End Sub


        Private Sub GetPreviousValues(sender As Object, e As DataGridViewCellEventArgs) Handles gridCustomOutputParameters.RowEnter
            Dim rowValues = ValuesFrom(gridCustomOutputParameters.Rows.Item(e.RowIndex))

            If RowIsValid(rowValues) Then mPreviousValues = rowValues
        End Sub

        Private Function RowIsValid(values As ResponseDataGridRowValues) As Boolean

            If String.IsNullOrWhiteSpace(values.ParamName) OrElse
                    values.MethodType = OutputMethodType.None OrElse
                    values.Type = DataType.unknown Then
                Return False
            End If

            If values.MethodType = OutputMethodType.JsonPath AndAlso
                (values.JsonPath.StartsWith(BaseJsonPath) = False OrElse values.JsonPath = BaseJsonPath) Then
                Return False
            End If

            Return True
        End Function

        Private Function RowIsEmpty(values As ResponseDataGridRowValues) As Boolean

            Return String.IsNullOrEmpty(values.ParamName) AndAlso
                values.Type = DataType.unknown AndAlso
                values.MethodType = OutputMethodType.None AndAlso
                (String.IsNullOrEmpty(values.JsonPath) Or values.JsonPath = BaseJsonPath)
        End Function

        Private Function RowIsEmpty(row As DataGridViewRow) As Boolean
            Return RowIsEmpty(ValuesFrom(row))
        End Function

        Private Sub HandleDataGridViewValidated(sender As Object, e As EventArgs) _
            Handles gridCustomOutputParameters.Validated

            UpdateCollection()
        End Sub

        Private Sub UpdateCollection()
            Response.CustomOutputParameters.Clear()

            For Each row As DataGridViewRow In gridCustomOutputParameters.Rows
                Dim rowValues = ValuesFrom(row)

                If RowIsEmpty(rowValues) Then Continue For

                If rowValues.MethodType = OutputMethodType.JsonPath Then
                    Response.CustomOutputParameters.Add(
                        New JsonPathOutputParameter(CType(row.Tag, Integer),
                            rowValues.ParamName, rowValues.Description, rowValues.JsonPath, rowValues.Type))

                ElseIf rowValues.MethodType = OutputMethodType.CustomCode Then
                    Response.CustomOutputParameters.Add(
                        New CustomCodeOutputParameter(CType(row.Tag, Integer),
                            rowValues.ParamName, rowValues.Description, rowValues.Type))
                End If
            Next
        End Sub


        Private Sub CellFormatting(sender As Object,
            e As DataGridViewCellFormattingEventArgs) Handles gridCustomOutputParameters.CellFormatting

            If e.ColumnIndex = colDelete.Index Then
                e.Value = ToolImages.Bin_16x16
                gridCustomOutputParameters.Rows(e.RowIndex).Cells(e.ColumnIndex).ToolTipText = WebApi_Resources.RemoveParameter
            End If
        End Sub

        Private Sub gridCustomOutputParameters_CellContentClick(sender As Object,
            e As DataGridViewCellEventArgs) Handles gridCustomOutputParameters.CellContentClick

            If e.RowIndex = -1 OrElse e.ColumnIndex = -1 Then Return

            Dim shouldDelete = e.ColumnIndex = colDelete.Index AndAlso
                               e.RowIndex < gridCustomOutputParameters.Rows.Count

            If shouldDelete AndAlso Not RowIsEmpty(gridCustomOutputParameters.Rows(e.RowIndex)) Then
                Dim rowValues = ValuesFrom(gridCustomOutputParameters.Rows.Item(e.RowIndex))
                gridCustomOutputParameters.Rows.RemoveAt(e.RowIndex)

                If (rowValues.MethodType = OutputMethodType.CustomCode) Then
                    mCustomCodeControl.UpdateMethodDefinition(GetMethodDefinition(Response))
                End If
            End If
        End Sub

        Private Sub gridCustomOutputParameters_DefaultValuesNeeded(sender As Object,
            e As DataGridViewRowEventArgs) Handles gridCustomOutputParameters.DefaultValuesNeeded

            e.Row.SetValues(Nothing, String.Empty, String.Empty, DataType.unknown, OutputMethodType.None, String.Empty)
            e.Row.Tag = 0
        End Sub

        Private Sub gridCustomOutputParameters_CurrentCellDirtyStateChanged(sender As Object,
            e As EventArgs) Handles gridCustomOutputParameters.CurrentCellDirtyStateChanged

            If (gridCustomOutputParameters.IsCurrentCellDirty) Then
                gridCustomOutputParameters.CommitEdit(DataGridViewDataErrorContexts.Commit)
            End If
        End Sub

        Private Sub gridCustomOutputParameters_CellValueChanged(sender As Object,
            e As DataGridViewCellEventArgs) Handles gridCustomOutputParameters.CellValueChanged

            If e.RowIndex = -1 Then Return

            If e.ColumnIndex = colMethodType.Index Then
                Dim paramType = CType(gridCustomOutputParameters.Rows(e.RowIndex).
                    Cells(colMethodType.Index).Value, OutputMethodType)

                With gridCustomOutputParameters.Rows(e.RowIndex).Cells(colJsonPath.Index)
                    .ReadOnly = paramType <> OutputMethodType.JsonPath
                    .Value = If(paramType = OutputMethodType.JsonPath, BaseJsonPath, String.Empty)
                End With
            End If
        End Sub

        Private Sub HandleCodeChanged(sender As Object, e As CustomCodeControlEventArgs) Handles mCustomCodeControl.CodeChanged
            Response.Code = e.CodeContent
        End Sub
    End Class

End Namespace


