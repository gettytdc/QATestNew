<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlScheduleListPanel
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlScheduleListPanel))
        Me.listboxSchedules = New AutomateControls.FlickerFreeListView()
        Me.columnSchedules = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.gridScheduleList = New System.Windows.Forms.DataGridView()
        Me.colStatus = New AutomateControls.DataGridViews.ItemStatusColumn()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStart = New AutomateControls.DataGridViews.DateColumn()
        Me.colEnd = New AutomateControls.DataGridViews.DateColumn()
        Me.colScheduler = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colViewLog = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.colViewSched = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblName = New System.Windows.Forms.Label()
        Me.buttonRefresh = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.dateAbsoluteDate = New System.Windows.Forms.DateTimePicker()
        Me.comboRelativeDate = New System.Windows.Forms.ComboBox()
        Me.txtDesc = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.updnDaysDistance = New AutomateControls.StyledNumericUpDown()
        Me.radioAbsoluteDate = New AutomateControls.StyledRadioButton()
        Me.radioRelativeDate = New AutomateControls.StyledRadioButton()
        Me.lblDaysDistance = New System.Windows.Forms.Label()
        Me.lblShowDays = New System.Windows.Forms.Label()
        Me.txtRelativeDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtAbsoluteDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnExport = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.cultureDateAbsoluteDate = New CustomControls.DatePicker()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        splitPanel = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        CType(splitPanel, System.ComponentModel.ISupportInitialize).BeginInit()
        splitPanel.Panel1.SuspendLayout()
        splitPanel.Panel2.SuspendLayout()
        splitPanel.SuspendLayout()
        CType(Me.gridScheduleList, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.updnDaysDistance, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'splitPanel
        '
        resources.ApplyResources(splitPanel, "splitPanel")
        splitPanel.Cursor = System.Windows.Forms.Cursors.Default
        splitPanel.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        splitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        splitPanel.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        splitPanel.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        splitPanel.GripVisible = False
        splitPanel.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        splitPanel.MouseLeaveColor = System.Drawing.Color.White
        splitPanel.Name = "splitPanel"
        '
        'splitPanel.Panel1
        '
        splitPanel.Panel1.Controls.Add(Me.listboxSchedules)
        '
        'splitPanel.Panel2
        '
        splitPanel.Panel2.Controls.Add(Me.gridScheduleList)
        splitPanel.TabStop = False
        splitPanel.TextColor = System.Drawing.Color.Black
        '
        'listboxSchedules
        '
        Me.listboxSchedules.CheckBoxes = True
        Me.listboxSchedules.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.columnSchedules})
        resources.ApplyResources(Me.listboxSchedules, "listboxSchedules")
        Me.listboxSchedules.HideSelection = False
        Me.listboxSchedules.Name = "listboxSchedules"
        Me.listboxSchedules.UseCompatibleStateImageBehavior = False
        Me.listboxSchedules.View = System.Windows.Forms.View.Details
        '
        'columnSchedules
        '
        resources.ApplyResources(Me.columnSchedules, "columnSchedules")
        '
        'gridScheduleList
        '
        Me.gridScheduleList.AllowUserToAddRows = False
        Me.gridScheduleList.AllowUserToDeleteRows = False
        Me.gridScheduleList.AllowUserToResizeColumns = False
        Me.gridScheduleList.AllowUserToResizeRows = False
        Me.gridScheduleList.BackgroundColor = System.Drawing.Color.White
        Me.gridScheduleList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridScheduleList.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colStatus, Me.colName, Me.colStart, Me.colEnd, Me.colScheduler, Me.colViewLog, Me.colViewSched})
        resources.ApplyResources(Me.gridScheduleList, "gridScheduleList")
        Me.gridScheduleList.Name = "gridScheduleList"
        Me.gridScheduleList.ReadOnly = True
        Me.gridScheduleList.RowHeadersVisible = False
        '
        'colStatus
        '
        Me.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colStatus, "colStatus")
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = True
        Me.colStatus.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.colStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colName
        '
        Me.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colStart
        '
        Me.colStart.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.colStart.DateFormat = Nothing
        resources.ApplyResources(Me.colStart, "colStart")
        Me.colStart.Name = "colStart"
        Me.colStart.ReadOnly = True
        '
        'colEnd
        '
        Me.colEnd.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.colEnd.DateFormat = Nothing
        resources.ApplyResources(Me.colEnd, "colEnd")
        Me.colEnd.Name = "colEnd"
        Me.colEnd.ReadOnly = True
        '
        'colScheduler
        '
        Me.colScheduler.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colScheduler, "colScheduler")
        Me.colScheduler.Name = "colScheduler"
        Me.colScheduler.ReadOnly = True
        '
        'colViewLog
        '
        Me.colViewLog.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colViewLog, "colViewLog")
        Me.colViewLog.Name = "colViewLog"
        Me.colViewLog.ReadOnly = True
        '
        'colViewSched
        '
        Me.colViewSched.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colViewSched, "colViewSched")
        Me.colViewSched.Name = "colViewSched"
        Me.colViewSched.ReadOnly = True
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Name = "Label3"
        '
        'lblName
        '
        resources.ApplyResources(Me.lblName, "lblName")
        Me.lblName.ForeColor = System.Drawing.Color.Black
        Me.lblName.Name = "lblName"
        '
        'buttonRefresh
        '
        resources.ApplyResources(Me.buttonRefresh, "buttonRefresh")
        Me.buttonRefresh.ForeColor = System.Drawing.Color.Black
        Me.buttonRefresh.Name = "buttonRefresh"
        Me.buttonRefresh.UseVisualStyleBackColor = True
        '
        'dateAbsoluteDate
        '
        resources.ApplyResources(Me.dateAbsoluteDate, "dateAbsoluteDate")
        Me.dateAbsoluteDate.Name = "dateAbsoluteDate"
        '
        'comboRelativeDate
        '
        Me.comboRelativeDate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboRelativeDate.FormattingEnabled = True
        Me.comboRelativeDate.Items.AddRange(New Object() {resources.GetString("comboRelativeDate.Items"), resources.GetString("comboRelativeDate.Items1"), resources.GetString("comboRelativeDate.Items2")})
        resources.ApplyResources(Me.comboRelativeDate, "comboRelativeDate")
        Me.comboRelativeDate.Name = "comboRelativeDate"
        '
        'txtDesc
        '
        resources.ApplyResources(Me.txtDesc, "txtDesc")
        Me.txtDesc.Name = "txtDesc"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'updnDaysDistance
        '
        resources.ApplyResources(Me.updnDaysDistance, "updnDaysDistance")
        Me.updnDaysDistance.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.updnDaysDistance.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnDaysDistance.Name = "updnDaysDistance"
        Me.updnDaysDistance.Value = New Decimal(New Integer() {999, 0, 0, 0})
        '
        'radioAbsoluteDate
        '
        Me.radioAbsoluteDate.ButtonHeight = 21
        Me.radioAbsoluteDate.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioAbsoluteDate.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioAbsoluteDate.FocusDiameter = 16
        Me.radioAbsoluteDate.FocusThickness = 3
        Me.radioAbsoluteDate.FocusYLocation = 9
        Me.radioAbsoluteDate.ForceFocus = True
        Me.radioAbsoluteDate.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioAbsoluteDate.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        resources.ApplyResources(Me.radioAbsoluteDate, "radioAbsoluteDate")
        Me.radioAbsoluteDate.MouseLeaveColor = System.Drawing.Color.White
        Me.radioAbsoluteDate.Name = "radioAbsoluteDate"
        Me.radioAbsoluteDate.RadioButtonDiameter = 12
        Me.radioAbsoluteDate.RadioButtonThickness = 2
        Me.radioAbsoluteDate.RadioYLocation = 7
        Me.radioAbsoluteDate.StringYLocation = 1
        Me.radioAbsoluteDate.TabStop = True
        Me.radioAbsoluteDate.TextColor = System.Drawing.Color.Black
        Me.radioAbsoluteDate.UseVisualStyleBackColor = True
        '
        'radioRelativeDate
        '
        Me.radioRelativeDate.ButtonHeight = 21
        Me.radioRelativeDate.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.radioRelativeDate.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.radioRelativeDate.FocusDiameter = 16
        Me.radioRelativeDate.FocusThickness = 3
        Me.radioRelativeDate.FocusYLocation = 9
        Me.radioRelativeDate.ForceFocus = True
        Me.radioRelativeDate.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.radioRelativeDate.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        resources.ApplyResources(Me.radioRelativeDate, "radioRelativeDate")
        Me.radioRelativeDate.MouseLeaveColor = System.Drawing.Color.White
        Me.radioRelativeDate.Name = "radioRelativeDate"
        Me.radioRelativeDate.RadioButtonDiameter = 12
        Me.radioRelativeDate.RadioButtonThickness = 2
        Me.radioRelativeDate.RadioYLocation = 7
        Me.radioRelativeDate.StringYLocation = 1
        Me.radioRelativeDate.TabStop = True
        Me.radioRelativeDate.TextColor = System.Drawing.Color.Black
        Me.radioRelativeDate.UseVisualStyleBackColor = True
        '
        'lblDaysDistance
        '
        resources.ApplyResources(Me.lblDaysDistance, "lblDaysDistance")
        Me.lblDaysDistance.ForeColor = System.Drawing.Color.Black
        Me.lblDaysDistance.Name = "lblDaysDistance"
        '
        'lblShowDays
        '
        resources.ApplyResources(Me.lblShowDays, "lblShowDays")
        Me.lblShowDays.ForeColor = System.Drawing.Color.Black
        Me.lblShowDays.Name = "lblShowDays"
        '
        'txtRelativeDate
        '
        resources.ApplyResources(Me.txtRelativeDate, "txtRelativeDate")
        Me.txtRelativeDate.Name = "txtRelativeDate"
        Me.txtRelativeDate.ReadOnly = True
        '
        'txtAbsoluteDate
        '
        resources.ApplyResources(Me.txtAbsoluteDate, "txtAbsoluteDate")
        Me.txtAbsoluteDate.Name = "txtAbsoluteDate"
        Me.txtAbsoluteDate.ReadOnly = True
        '
        'btnExport
        '
        resources.ApplyResources(Me.btnExport, "btnExport")
        Me.btnExport.ForeColor = System.Drawing.Color.Black
        Me.btnExport.Name = "btnExport"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'cultureDateAbsoluteDate
        '
        resources.ApplyResources(Me.cultureDateAbsoluteDate, "cultureDateAbsoluteDate")
        Me.cultureDateAbsoluteDate.Name = "cultureDateAbsoluteDate"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        '
        'ctlScheduleListPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.cultureDateAbsoluteDate)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.updnDaysDistance)
        Me.Controls.Add(splitPanel)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.lblName)
        Me.Controls.Add(Me.lblShowDays)
        Me.Controls.Add(Me.lblDaysDistance)
        Me.Controls.Add(Me.buttonRefresh)
        Me.Controls.Add(Me.radioRelativeDate)
        Me.Controls.Add(Me.dateAbsoluteDate)
        Me.Controls.Add(Me.radioAbsoluteDate)
        Me.Controls.Add(Me.comboRelativeDate)
        Me.Controls.Add(Me.txtDesc)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.txtRelativeDate)
        Me.Controls.Add(Me.txtAbsoluteDate)
        Me.Name = "ctlScheduleListPanel"
        resources.ApplyResources(Me, "$this")
        splitPanel.Panel1.ResumeLayout(False)
        splitPanel.Panel2.ResumeLayout(False)
        CType(splitPanel, System.ComponentModel.ISupportInitialize).EndInit()
        splitPanel.ResumeLayout(False)
        CType(Me.gridScheduleList, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.updnDaysDistance, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents comboRelativeDate As System.Windows.Forms.ComboBox
    Friend WithEvents updnDaysDistance As AutomateControls.StyledNumericUpDown
    Friend WithEvents radioAbsoluteDate As AutomateControls.StyledRadioButton
    Friend WithEvents radioRelativeDate As AutomateControls.StyledRadioButton
    Friend WithEvents lblShowDays As System.Windows.Forms.Label
    Friend WithEvents listboxSchedules As AutomateControls.FlickerFreeListView
    Friend WithEvents dateAbsoluteDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents buttonRefresh As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblName As System.Windows.Forms.Label
    Private WithEvents txtDesc As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Public WithEvents lblDaysDistance As System.Windows.Forms.Label
    Friend WithEvents gridScheduleList As System.Windows.Forms.DataGridView
    Friend WithEvents columnSchedules As System.Windows.Forms.ColumnHeader
    Friend WithEvents txtRelativeDate As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtAbsoluteDate As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents colStatus As AutomateControls.DataGridViews.ItemStatusColumn
    Friend WithEvents colName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStart As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colEnd As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colScheduler As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colViewLog As System.Windows.Forms.DataGridViewLinkColumn
    Friend WithEvents colViewSched As System.Windows.Forms.DataGridViewLinkColumn
    Private WithEvents btnExport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cultureDateAbsoluteDate As CustomControls.DatePicker
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents splitPanel As AutomateControls.SplitContainers.HighlightingSplitContainer
End Class
