<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlSystemReports
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemReports))
        Me.dgReports = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnGenerateReport = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.gpReporting = New System.Windows.Forms.GroupBox()
        Me.MIUnlockFlowLayoutPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblLockedHintText = New System.Windows.Forms.Label()
        Me.btnUnlockMI = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.rbAuto = New AutomateControls.StyledRadioButton()
        Me.dtpRefreshAt = New System.Windows.Forms.DateTimePicker()
        Me.rbManual = New AutomateControls.StyledRadioButton()
        Me.DailyStatisticsFlowLayoutPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.numDaysToKeep = New AutomateControls.StyledNumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.numMonthsToKeep = New AutomateControls.StyledNumericUpDown()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lblLastRefreshed = New System.Windows.Forms.Label()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.cbEnabled = New System.Windows.Forms.CheckBox()
        Me.gpReports = New System.Windows.Forms.GroupBox()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        CType(Me.dgReports,System.ComponentModel.ISupportInitialize).BeginInit
        Me.gpReporting.SuspendLayout
        Me.MIUnlockFlowLayoutPanel.SuspendLayout
        Me.FlowLayoutPanel1.SuspendLayout
        Me.DailyStatisticsFlowLayoutPanel.SuspendLayout
        CType(Me.numDaysToKeep,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.numMonthsToKeep,System.ComponentModel.ISupportInitialize).BeginInit
        Me.gpReports.SuspendLayout
        Me.SuspendLayout
        '
        'dgReports
        '
        Me.dgReports.AllowUserToAddRows = false
        Me.dgReports.AllowUserToDeleteRows = false
        Me.dgReports.AllowUserToResizeRows = false
        resources.ApplyResources(Me.dgReports, "dgReports")
        Me.dgReports.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgReports.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.dgReports.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.dgReports.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.dgReports.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2})
        Me.dgReports.GridColor = System.Drawing.SystemColors.Control
        Me.dgReports.MultiSelect = false
        Me.dgReports.Name = "dgReports"
        Me.dgReports.ReadOnly = true
        Me.dgReports.RowHeadersVisible = false
        Me.dgReports.RowTemplate.Height = 18
        Me.dgReports.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgReports.ShowCellToolTips = false
        Me.dgReports.StandardTab = true
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        Me.DataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'btnGenerateReport
        '
        resources.ApplyResources(Me.btnGenerateReport, "btnGenerateReport")
        Me.btnGenerateReport.BackColor = System.Drawing.Color.White
        Me.btnGenerateReport.Name = "btnGenerateReport"
        Me.btnGenerateReport.UseVisualStyleBackColor = False
        '
        'gpReporting
        '
        resources.ApplyResources(Me.gpReporting, "gpReporting")
        Me.gpReporting.Controls.Add(Me.btnApply)
        Me.gpReporting.Controls.Add(Me.FlowLayoutPanel1)
        Me.gpReporting.Controls.Add(Me.DailyStatisticsFlowLayoutPanel)
        Me.gpReporting.Controls.Add(Me.lblLastRefreshed)
        Me.gpReporting.Controls.Add(Me.cbEnabled)
        Me.gpReporting.Controls.Add(Me.MIUnlockFlowLayoutPanel)
        Me.gpReporting.Name = "gpReporting"
        Me.gpReporting.TabStop = False
        '
        'MIUnlockFlowLayoutPanel
        '
        resources.ApplyResources(Me.MIUnlockFlowLayoutPanel, "MIUnlockFlowLayoutPanel")
        Me.MIUnlockFlowLayoutPanel.Controls.Add(Me.lblLockedHintText)
        Me.MIUnlockFlowLayoutPanel.Controls.Add(Me.btnUnlockMI)
        Me.MIUnlockFlowLayoutPanel.Name = "MIUnlockFlowLayoutPanel"
        '
        'lblLockedHintText
        '
        resources.ApplyResources(Me.lblLockedHintText, "lblLockedHintText")
        Me.lblLockedHintText.Name = "lblLockedHintText"
        '
        'btnUnlockMI
        '
        resources.ApplyResources(Me.btnUnlockMI, "btnUnlockMI")
        Me.btnUnlockMI.BackColor = System.Drawing.Color.White
        Me.btnUnlockMI.Name = "btnUnlockMI"
        Me.btnUnlockMI.UseVisualStyleBackColor = False
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.rbAuto)
        Me.FlowLayoutPanel1.Controls.Add(Me.dtpRefreshAt)
        Me.FlowLayoutPanel1.Controls.Add(Me.rbManual)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'rbAuto
        '
        resources.ApplyResources(Me.rbAuto, "rbAuto")
        Me.rbAuto.ButtonHeight = 21
        Me.rbAuto.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rbAuto.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rbAuto.FocusDiameter = 16
        Me.rbAuto.FocusThickness = 3
        Me.rbAuto.FocusYLocation = 9
        Me.rbAuto.ForceFocus = True
        Me.rbAuto.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rbAuto.HoverColor = System.Drawing.Color.FromArgb(CType(CType(233, Byte), Integer), CType(CType(245, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.rbAuto.MouseLeaveColor = System.Drawing.Color.White
        Me.rbAuto.Name = "rbAuto"
        Me.rbAuto.RadioButtonDiameter = 12
        Me.rbAuto.RadioButtonThickness = 2
        Me.rbAuto.RadioYLocation = 7
        Me.rbAuto.StringYLocation = 1
        Me.rbAuto.TabStop = True
        Me.rbAuto.TextColor = System.Drawing.Color.Black
        Me.rbAuto.UseVisualStyleBackColor = True
        '
        'dtpRefreshAt
        '
        resources.ApplyResources(Me.dtpRefreshAt, "dtpRefreshAt")
        Me.FlowLayoutPanel1.SetFlowBreak(Me.dtpRefreshAt, True)
        Me.dtpRefreshAt.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpRefreshAt.Name = "dtpRefreshAt"
        Me.dtpRefreshAt.ShowUpDown = True
        '
        'rbManual
        '
        resources.ApplyResources(Me.rbManual, "rbManual")
        Me.rbManual.ButtonHeight = 21
        Me.rbManual.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.FlowLayoutPanel1.SetFlowBreak(Me.rbManual, True)
        Me.rbManual.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rbManual.FocusDiameter = 16
        Me.rbManual.FocusThickness = 3
        Me.rbManual.FocusYLocation = 9
        Me.rbManual.ForceFocus = True
        Me.rbManual.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rbManual.HoverColor = System.Drawing.Color.FromArgb(CType(CType(233, Byte), Integer), CType(CType(245, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.rbManual.MouseLeaveColor = System.Drawing.Color.White
        Me.rbManual.Name = "rbManual"
        Me.rbManual.RadioButtonDiameter = 12
        Me.rbManual.RadioButtonThickness = 2
        Me.rbManual.RadioYLocation = 7
        Me.rbManual.StringYLocation = 1
        Me.rbManual.TabStop = True
        Me.rbManual.TextColor = System.Drawing.Color.Black
        Me.rbManual.UseVisualStyleBackColor = True
        '
        'DailyStatisticsFlowLayoutPanel
        '
        resources.ApplyResources(Me.DailyStatisticsFlowLayoutPanel, "DailyStatisticsFlowLayoutPanel")
        Me.DailyStatisticsFlowLayoutPanel.Controls.Add(Me.Label1)
        Me.DailyStatisticsFlowLayoutPanel.Controls.Add(Me.numDaysToKeep)
        Me.DailyStatisticsFlowLayoutPanel.Controls.Add(Me.Label2)
        Me.DailyStatisticsFlowLayoutPanel.Controls.Add(Me.numMonthsToKeep)
        Me.DailyStatisticsFlowLayoutPanel.Controls.Add(Me.Label4)
        Me.DailyStatisticsFlowLayoutPanel.Name = "DailyStatisticsFlowLayoutPanel"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'numDaysToKeep
        '
        resources.ApplyResources(Me.numDaysToKeep, "numDaysToKeep")
        Me.numDaysToKeep.Maximum = New Decimal(New Integer() {90, 0, 0, 0})
        Me.numDaysToKeep.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numDaysToKeep.Name = "numDaysToKeep"
        Me.numDaysToKeep.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'numMonthsToKeep
        '
        resources.ApplyResources(Me.numMonthsToKeep, "numMonthsToKeep")
        Me.numMonthsToKeep.Maximum = New Decimal(New Integer() {36, 0, 0, 0})
        Me.numMonthsToKeep.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numMonthsToKeep.Name = "numMonthsToKeep"
        Me.numMonthsToKeep.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'lblLastRefreshed
        '
        resources.ApplyResources(Me.lblLastRefreshed, "lblLastRefreshed")
        Me.lblLastRefreshed.Name = "lblLastRefreshed"
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.BackColor = System.Drawing.Color.White
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = false
        '
        'cbEnabled
        '
        resources.ApplyResources(Me.cbEnabled, "cbEnabled")
        Me.cbEnabled.Name = "cbEnabled"
        Me.cbEnabled.UseVisualStyleBackColor = true
        '
        'gpReports
        '
        resources.ApplyResources(Me.gpReports, "gpReports")
        Me.gpReports.Controls.Add(Me.dgReports)
        Me.gpReports.Controls.Add(Me.btnGenerateReport)
        Me.gpReports.Name = "gpReports"
        Me.gpReports.TabStop = false
        '
        'ctlSystemReports
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.gpReports)
        Me.Controls.Add(Me.gpReporting)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlSystemReports"
        CType(Me.dgReports,System.ComponentModel.ISupportInitialize).EndInit
        Me.gpReporting.ResumeLayout(false)
        Me.gpReporting.PerformLayout
        Me.MIUnlockFlowLayoutPanel.ResumeLayout(false)
        Me.MIUnlockFlowLayoutPanel.PerformLayout
        Me.FlowLayoutPanel1.ResumeLayout(false)
        Me.FlowLayoutPanel1.PerformLayout
        Me.DailyStatisticsFlowLayoutPanel.ResumeLayout(false)
        Me.DailyStatisticsFlowLayoutPanel.PerformLayout
        CType(Me.numDaysToKeep,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.numMonthsToKeep,System.ComponentModel.ISupportInitialize).EndInit
        Me.gpReports.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents dgReports As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnGenerateReport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents gpReporting As System.Windows.Forms.GroupBox
    Friend WithEvents gpReports As System.Windows.Forms.GroupBox
    Friend WithEvents lblLastRefreshed As System.Windows.Forms.Label
    Friend WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cbEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents numMonthsToKeep As AutomateControls.StyledNumericUpDown
    Friend WithEvents numDaysToKeep As AutomateControls.StyledNumericUpDown
    Friend WithEvents dtpRefreshAt As DateTimePicker
    Friend WithEvents BackgroundWorker1 As BackgroundWorker
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents DailyStatisticsFlowLayoutPanel As FlowLayoutPanel
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents rbAuto As AutomateControls.StyledRadioButton
    Friend WithEvents rbManual As AutomateControls.StyledRadioButton
    Friend WithEvents lblLockedHintText As Label
    Friend WithEvents MIUnlockFlowLayoutPanel As FlowLayoutPanel
    Friend WithEvents btnUnlockMI As AutomateControls.Buttons.StandardStyledButton
End Class
