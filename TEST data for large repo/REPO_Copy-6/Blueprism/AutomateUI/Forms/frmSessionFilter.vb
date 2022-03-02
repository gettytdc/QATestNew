Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports System.Globalization.CultureInfo

Friend Class frmSessionFilter
    Implements IPermission
    Implements IHelp

    ''' <summary>
    ''' Gets help file string for this class.
    ''' </summary>
    ''' <returns>Help file.</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "HELPMISSING"
    End Function

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property

    ''' <summary>
    ''' Filter operators
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum [Operator]
        EqualTo
        NotEqualTo
        GreaterThan
        LessThan
        GreaterThanOrEqualTo
        LessThanOrEqualTo
        [Like]
    End Enum

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()


        'Create columns for the data grid view.
        Dim oColumn As DataGridViewColumn

        'Visible
        oColumn = New DataGridViewColumn
        oColumn.CellTemplate = New DataGridViewCheckBoxCell()
        oColumn.CellTemplate.ValueType = Type.GetType("System.Boolean")
        oColumn.CellTemplate.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        oColumn.Name = "Visible"
        oColumn.HeaderText = My.Resources.frmSessionFilter_Visible
        oColumn.DataPropertyName = oColumn.Name
        DataGridView1.Columns.Add(oColumn)

        'Column Name
        oColumn = New DataGridViewColumn
        oColumn.CellTemplate = New DataGridViewTextBoxCell()
        oColumn.CellTemplate.ValueType = Type.GetType("System.String")
        oColumn.Name = "Column"
        oColumn.HeaderText = My.Resources.frmSessionFilter_Column
        oColumn.DataPropertyName = oColumn.Name
        oColumn.ReadOnly = True
        DataGridView1.Columns.Add(oColumn)

        'Operator
        Dim comboBoxCol As New DataGridViewComboBoxColumn
        comboBoxCol.CellTemplate = New DataGridViewComboBoxCell
        oColumn.CellTemplate.ValueType = Type.GetType("System.String")
        comboBoxCol.Name = "Operator"
        comboBoxCol.HeaderText = My.Resources.frmSessionFilter_Operator
        comboBoxCol.DataPropertyName = comboBoxCol.Name
        comboBoxCol.Items.Add("")
        For Each o As String In System.Enum.GetNames(GetType([Operator]))
            comboBoxCol.Items.Add(clsUtility.GetUnCamelled(o))
        Next
        DataGridView1.Columns.Add(comboBoxCol)

        'Value
        oColumn = New DataGridViewColumn
        oColumn.CellTemplate = New DataGridViewTextBoxCell()
        oColumn.CellTemplate.ValueType = Type.GetType("System.String")
        oColumn.Name = "Value"
        oColumn.HeaderText = My.Resources.frmSessionFilter_Value
        oColumn.DataPropertyName = oColumn.Name
        oColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        DataGridView1.Columns.Add(oColumn)

        'Data Type (hidden)
        oColumn = New DataGridViewColumn
        oColumn.CellTemplate = New DataGridViewTextBoxCell()
        oColumn.CellTemplate.ValueType = Type.GetType("System.String")
        oColumn.Name = "Type"
        oColumn.HeaderText = My.Resources.frmSessionFilter_Type
        oColumn.Visible = False
        DataGridView1.Columns.Add(oColumn)

    End Sub

    ''' <summary>
    ''' Populates the data grid view using a data table.
    ''' </summary>
    ''' <param name="dtSessions">The data table</param>
    ''' <remarks></remarks>
    Public Sub SetData(ByVal dtSessions As DataTable)

        DataGridView1.Rows.Clear()
        For Each c As DataColumn In dtSessions.Columns
            If c.ColumnName.StartsWith("!") Then
                'Ignore system columns.
            Else
                'Add a new row to the data grid view: Visible, Column, Operator, Value, Data Type.
                DataGridView1.Rows.Add(New Object() {False, c.ColumnName, Nothing, Nothing, c.DataType.ToString})
                'Change the Value cell to a calendar control if necessary.
                If c.DataType.Equals(Type.GetType("System.DateTime")) Then
                    DataGridView1.Item(3, DataGridView1.RowCount - 1) = New DataGridViewCalendarCell
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' Populates the data grid view using an array of objects.
    ''' </summary>
    ''' <param name="aDataGridViewValues">The arrayt of objects</param>
    ''' <remarks></remarks>
    Public Sub SetData(ByVal aDataGridViewValues As Object(,))

        Dim aRowValues As Object()
        Dim bCalendarCell As Boolean

        For r As Integer = 0 To aDataGridViewValues.GetLength(0) - 1
            ReDim aRowValues(aDataGridViewValues.GetLength(1) - 1)
            For c As Integer = 0 To aDataGridViewValues.GetLength(1) - 1
                aRowValues(c) = aDataGridViewValues(r, c)
                If aRowValues(c) IsNot Nothing AndAlso aRowValues(c).GetType.Equals(Type.GetType("System.DateTime")) Then
                    bCalendarCell = True
                End If
            Next
            DataGridView1.Rows.Add(aRowValues)
            If bCalendarCell Then
                DataGridView1.Item(3, r) = New DataGridViewCalendarCell
                DataGridView1.Item(3, r).Value = aRowValues(3)
            End If
            bCalendarCell = False
        Next

    End Sub

    ''' <summary>
    ''' Sets the Visible column value to True in each row where the
    ''' Column value matches the given names.
    ''' </summary>
    ''' <param name="aColumnNames">The column names</param>
    ''' <remarks></remarks>
    Public Sub SetVisible(ByVal aColumnNames As Generic.List(Of String))

        For Each sColumnName As String In aColumnNames
            For r As Integer = 0 To DataGridView1.RowCount - 1
                If CStr(DataGridView1.Rows(r).Cells("Column").Value) = sColumnName Then
                    DataGridView1.Rows(r).Cells("Visible").Value = True
                    Exit For
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' Sets the Operator and Value column values in the row where the
    ''' Column value matches the given name.
    ''' </summary>
    ''' <param name="sColumn">The column name</param>
    ''' <param name="iOperator">The operator</param>
    ''' <param name="oValue">The value</param>
    ''' <remarks></remarks>
    Public Sub SetFilter(ByVal sColumn As String, ByVal iOperator As [Operator], ByVal oValue As Object)

        For r As Integer = 0 To DataGridView1.RowCount - 1
            If CStr(DataGridView1.Rows(r).Cells("Column").Value) = sColumn Then
                DataGridView1.Rows(r).Cells("Operator").Value = clsUtility.GetUnCamelled(iOperator.ToString)
                DataGridView1.Rows(r).Cells("Value").Value = oValue
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' Returns a string representation of rows where a filter has been specified.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetFilters() As Generic.List(Of String())

        Dim aFilters As New Generic.List(Of String())
        Dim oValue As Object
        Dim sColumn, sOperator, sValue, sType As String
        Dim bVisible As Boolean
        Dim iOperator As [Operator]

        For r As Integer = 0 To DataGridView1.RowCount - 1

            oValue = DataGridView1.Rows(r).Cells("Visible").Value
            If oValue Is Nothing Then
                bVisible = False
            Else
                bVisible = CBool(oValue)
            End If
            oValue = DataGridView1.Rows(r).Cells("Column").Value
            If oValue Is Nothing Then
                sColumn = ""
            Else
                sColumn = CStr(oValue)
            End If
            oValue = DataGridView1.Rows(r).Cells("Operator").Value
            If oValue Is Nothing Then
                sOperator = ""
            Else
                sOperator = CStr(oValue)
            End If
            oValue = DataGridView1.Rows(r).Cells("Value").Value
            If oValue Is Nothing Then
                sValue = ""
            Else
                sValue = CStr(oValue)
            End If
            oValue = DataGridView1.Rows(r).Cells("Type").Value
            If oValue Is Nothing Then
                sType = ""
            Else
                sType = CStr(oValue)
            End If

            If bVisible And sColumn.Trim <> "" And sOperator <> "" And sValue.Trim <> "" Then
                Select Case sType
                    Case "System.String"
                        sValue = "'" & sValue.Replace("'", "''") & "'"
                    Case "System.DateTime"
                        sValue = "#" & Format(DateTime.Parse(sValue), CurrentCulture.DateTimeFormat().FullDateTimePattern) & "#"
                End Select

                iOperator = CType(System.Enum.Parse(GetType([Operator]), clsUtility.GetCamelled(sOperator)), [Operator])

                'Translate the operator name to the relevant symbol.
                Select Case iOperator
                    Case [Operator].EqualTo
                        sOperator = "="
                    Case [Operator].NotEqualTo
                        sOperator = "<>"
                    Case [Operator].GreaterThan
                        sOperator = ">"
                    Case [Operator].GreaterThanOrEqualTo
                        sOperator = ">="
                    Case [Operator].LessThan
                        sOperator = "<"
                    Case [Operator].LessThanOrEqualTo
                        sOperator = "<="
                    Case [Operator].Like
                        sOperator = "LIKE"
                End Select

                aFilters.Add(New String() {sColumn, sOperator, sValue})

            End If

        Next

        Return aFilters

    End Function

    ''' <summary>
    ''' Returns a list of column names that have been marked as visible.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetVisibleColumnNames() As Generic.List(Of String)

        Dim oValue As Object
        Dim sColumn As String
        Dim bVisible As Boolean
        Dim aColumns As New Generic.List(Of String)

        For r As Integer = 0 To DataGridView1.RowCount - 1

            oValue = DataGridView1.Rows(r).Cells("Visible").Value
            If TypeOf oValue Is DBNull Then
                bVisible = False
            Else
                bVisible = CBool(oValue)
            End If

            oValue = DataGridView1.Rows(r).Cells("Column").Value
            If TypeOf oValue Is DBNull Then
                sColumn = ""
            Else
                sColumn = CStr(oValue)
            End If

            If bVisible Then
                aColumns.Add(sColumn)
            End If

        Next

        Return aColumns

    End Function

    ''' <summary>
    ''' Validates any filters that have been specified and alerts the user to any
    ''' problems found.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function DataIsValid() As Boolean

        Dim oValue As Object
        Dim sValue, sOperator, sType As String
        Dim iOperator As [Operator]
        Dim bVisible As Boolean

        For r As Integer = 0 To DataGridView1.RowCount - 1

            oValue = DataGridView1.Rows(r).Cells("Visible").Value
            If oValue Is Nothing Then
                bVisible = False
            Else
                bVisible = bVisible Or CBool(oValue)
            End If

            oValue = DataGridView1.Rows(r).Cells("Operator").Value
            If oValue Is Nothing Then
                sOperator = ""
            Else
                sOperator = CStr(oValue)
                iOperator = CType(System.Enum.Parse(GetType([Operator]), clsUtility.GetCamelled(sOperator)), [Operator])
            End If

            oValue = DataGridView1.Rows(r).Cells("Value").Value
            If oValue Is Nothing Then
                sValue = ""
            Else
                sValue = CStr(oValue)
            End If

            oValue = DataGridView1.Rows(r).Cells("Type").Value
            If oValue Is Nothing Then
                sType = ""
            Else
                sType = CStr(oValue)
            End If

            If sType = "System.DateTime" Then
                If sValue.Trim <> "" AndAlso IsDate(sValue) = False Then
                    UserMessage.OK(String.Format(My.Resources.PleaseEnterAValidDateOnRow0, CStr(r + 1)))
                    Return False
                End If
                If sValue.Trim <> "" AndAlso sOperator <> "" AndAlso iOperator = [Operator].Like Then
                    UserMessage.OK(String.Format(My.Resources.PleaseEnterAValidOperatorOnRow0TheLIKEOperatorCannotBeUsedWithADate, CStr(r + 1)))
                    Return False
                End If
            End If
            If Not bVisible Then
                UserMessage.OK(My.Resources.PleaseEnsureAtLeastOneColumnIsAlwaysVisible)
                Return False
            End If
        Next
        Return True

    End Function

    ''' <summary>
    ''' Returns the data in the data grid view as an array of objects.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetData() As Object(,)
        Dim aDataGridViewValues As Object(,)
        ReDim aDataGridViewValues(DataGridView1.RowCount - 1, DataGridView1.ColumnCount - 1)

        For r As Integer = 0 To DataGridView1.RowCount - 1
            For c As Integer = 0 To DataGridView1.ColumnCount - 1
                aDataGridViewValues(r, c) = DataGridView1.Item(c, r).Value
            Next
        Next

        Return aDataGridViewValues

    End Function

#Region "Event Handlers"

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnCancel.Click

        Me.Close()

    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnOK.Click

        If DataIsValid() Then
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End If

    End Sub

    Private Sub lnkCheckAll_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) _
    Handles lnkSelectAll.LinkClicked, lnkSelectNone.LinkClicked
        For r As Integer = 0 To DataGridView1.RowCount - 1
            DataGridView1.Rows(r).Cells("Visible").Value = sender Is lnkSelectAll
        Next
    End Sub

#End Region

#Region "Class DataGridViewCalendarCell"

    Private Class DataGridViewCalendarCell
        Inherits DataGridViewTextBoxCell

        Public Sub New()
            Me.Style.Format = CurrentCulture.DateTimeFormat().ShortDatePattern
        End Sub

        Public Overrides Sub InitializeEditingControl(ByVal rowIndex As Integer, ByVal initialFormattedValue As Object, ByVal dataGridViewCellStyle As DataGridViewCellStyle)

            ' Set the value of the editing control to the current cell value.
            MyBase.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle)

            Dim ctl As DataGridViewCalendarEditingControl = CType(DataGridView.EditingControl, DataGridViewCalendarEditingControl)

            If Me.Value Is Nothing Then
                ctl.Value = Today()
            Else
                ctl.Value = CDate(Me.Value)
            End If

        End Sub

        Public Overrides ReadOnly Property EditType() As Type
            Get
                ' Return the type of the editing contol that DataGridViewCalendarCell uses.
                Return GetType(DataGridViewCalendarEditingControl)
            End Get
        End Property

        Public Overrides ReadOnly Property ValueType() As Type
            Get
                ' Return the type of the value that DataGridViewCalendarCell contains.
                Return GetType(DateTime)
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultNewRowValue() As Object
            Get
                ' Use the current date as the default value.
                Return DateTime.Today
            End Get
        End Property

    End Class

#End Region

#Region "Class DataGridViewCalendarEditingControl"

    Private Class DataGridViewCalendarEditingControl
        Inherits DateTimePicker
        Implements IDataGridViewEditingControl

        Private moDataGridView As DataGridView
        Private mbValueChanged As Boolean = False
        Private miRowIndex As Integer

        Public Sub New()
            Me.Format = DateTimePickerFormat.Custom
            Me.CustomFormat = CurrentCulture.DateTimeFormat().ShortDatePattern
        End Sub

        Public Property EditingControlFormattedValue() As Object _
         Implements IDataGridViewEditingControl.EditingControlFormattedValue

            Get
                Return Me.Value.ToString(Me.CustomFormat)
            End Get

            Set(ByVal value As Object)
                If TypeOf value Is String Then
                    Me.Value = DateTime.Parse(CStr(value))
                End If
            End Set

        End Property

        Public Function GetEditingControlFormattedValue(ByVal context As DataGridViewDataErrorContexts) As Object _
         Implements IDataGridViewEditingControl.GetEditingControlFormattedValue

            Return Me.Value.ToString(Me.CustomFormat)

        End Function

        Public Sub ApplyCellStyleToEditingControl(ByVal dataGridViewCellStyle As DataGridViewCellStyle) _
         Implements IDataGridViewEditingControl.ApplyCellStyleToEditingControl

            Me.Font = dataGridViewCellStyle.Font
            Me.CalendarForeColor = dataGridViewCellStyle.ForeColor
            Me.CalendarMonthBackground = dataGridViewCellStyle.BackColor

        End Sub

        Public Property EditingControlRowIndex() As Integer _
         Implements IDataGridViewEditingControl.EditingControlRowIndex

            Get
                Return miRowIndex
            End Get
            Set(ByVal value As Integer)
                miRowIndex = value
            End Set

        End Property

        Public Function EditingControlWantsInputKey(ByVal key As Keys, ByVal dataGridViewWantsInputKey As Boolean) As Boolean _
         Implements IDataGridViewEditingControl.EditingControlWantsInputKey

            ' Let the DateTimePicker handle the keys listed.
            Select Case key And Keys.KeyCode
                Case Keys.Left, Keys.Up, Keys.Down, Keys.Right,
                 Keys.Home, Keys.End, Keys.PageDown, Keys.PageUp

                    Return True

                Case Else
                    Return Not dataGridViewWantsInputKey
            End Select

        End Function

        Public Sub PrepareEditingControlForEdit(ByVal selectAll As Boolean) _
         Implements IDataGridViewEditingControl.PrepareEditingControlForEdit

            ' No preparation needs to be done.

        End Sub

        Public ReadOnly Property RepositionEditingControlOnValueChange() As Boolean _
         Implements IDataGridViewEditingControl.RepositionEditingControlOnValueChange

            Get
                Return False
            End Get

        End Property

        Public Property EditingControlDataGridView() As DataGridView _
         Implements IDataGridViewEditingControl.EditingControlDataGridView

            Get
                Return moDataGridView
            End Get
            Set(ByVal value As DataGridView)
                moDataGridView = value
            End Set

        End Property

        Public Property EditingControlValueChanged() As Boolean _
         Implements IDataGridViewEditingControl.EditingControlValueChanged

            Get
                Return mbValueChanged
            End Get
            Set(ByVal value As Boolean)
                mbValueChanged = value
            End Set

        End Property

        Public ReadOnly Property EditingControlCursor() As Cursor _
         Implements IDataGridViewEditingControl.EditingPanelCursor

            Get
                Return MyBase.Cursor
            End Get

        End Property

        Protected Overrides Sub OnValueChanged(ByVal eventargs As EventArgs)

            ' Notify the DataGridView that the contents of the cell have changed.
            mbValueChanged = True
            Me.EditingControlDataGridView.NotifyCurrentCellDirty(True)
            MyBase.OnValueChanged(eventargs)

        End Sub

    End Class

#End Region

End Class
