<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlDataPipelineSettings
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDataPipelineSettings))
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.gbMonitoring = New System.Windows.Forms.GroupBox()
        Me.numFrequency = New AutomateControls.StyledNumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.publishedDashboardSettings = New System.Windows.Forms.GroupBox()
        Me.dgvPublishedDashboards = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colId = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colInterval = New AutomateControls.DataGridViews.DataGridViewNumericUpDownColumn()
        Me.cbSendPublishedDashboardToDataGateway = New System.Windows.Forms.CheckBox()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.gbWorkQueueAnalysis = New System.Windows.Forms.GroupBox()
        Me.cbSendWorkQueueAnalysisToDataGateway = New System.Windows.Forms.CheckBox()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.DBAccess = New System.Windows.Forms.GroupBox()
        Me.radSQLAuth = New AutomateControls.StyledRadioButton()
        Me.radIntegratedSecurity = New AutomateControls.StyledRadioButton()
        Me.lblWindowsAuthWarning = New System.Windows.Forms.Label()
        Me.sqlServerDatagatewaysPort = New AutomateControls.StyledNumericUpDown()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cboDGCredentials = New System.Windows.Forms.ComboBox()
        Me.gbSessionLogs = New System.Windows.Forms.GroupBox()
        Me.cbEmitToDataGateways = New System.Windows.Forms.CheckBox()
        Me.cbWriteToDatabase = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.gbMonitoring.SuspendLayout
        CType(Me.numFrequency,System.ComponentModel.ISupportInitialize).BeginInit
        Me.publishedDashboardSettings.SuspendLayout
        CType(Me.dgvPublishedDashboards,System.ComponentModel.ISupportInitialize).BeginInit
        Me.TableLayoutPanel1.SuspendLayout
        Me.gbWorkQueueAnalysis.SuspendLayout
        Me.DBAccess.SuspendLayout
        CType(Me.sqlServerDatagatewaysPort,System.ComponentModel.ISupportInitialize).BeginInit
        Me.gbSessionLogs.SuspendLayout
        Me.SuspendLayout
        '
        'gbMonitoring
        '
        resources.ApplyResources(Me.gbMonitoring, "gbMonitoring")
        Me.gbMonitoring.Controls.Add(Me.numFrequency)
        Me.gbMonitoring.Controls.Add(Me.Label2)
        Me.gbMonitoring.Name = "gbMonitoring"
        Me.gbMonitoring.TabStop = false
        '
        'numFrequency
        '
        resources.ApplyResources(Me.numFrequency, "numFrequency")
        Me.numFrequency.Maximum = New Decimal(New Integer() {3600, 0, 0, 0})
        Me.numFrequency.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.numFrequency.Name = "numFrequency"
        Me.numFrequency.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'publishedDashboardSettings
        '
        resources.ApplyResources(Me.publishedDashboardSettings, "publishedDashboardSettings")
        Me.publishedDashboardSettings.Controls.Add(Me.dgvPublishedDashboards)
        Me.publishedDashboardSettings.Controls.Add(Me.cbSendPublishedDashboardToDataGateway)
        Me.publishedDashboardSettings.Name = "publishedDashboardSettings"
        Me.publishedDashboardSettings.TabStop = false
        '
        'dgvPublishedDashboards
        '
        Me.dgvPublishedDashboards.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvPublishedDashboards.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvPublishedDashboards.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colId, Me.colName, Me.colInterval})
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvPublishedDashboards.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.dgvPublishedDashboards, "dgvPublishedDashboards")
        Me.dgvPublishedDashboards.Name = "dgvPublishedDashboards"
        '
        'colId
        '
        resources.ApplyResources(Me.colId, "colId")
        Me.colId.Name = "colId"
        Me.colId.ReadOnly = true
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = true
        '
        'colInterval
        '
        Me.colInterval.FillWeight = 70!
        resources.ApplyResources(Me.colInterval, "colInterval")
        Me.colInterval.Maximum = New Decimal(New Integer() {1440, 0, 0, 0})
        Me.colInterval.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        Me.colInterval.Name = "colInterval"
        Me.colInterval.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colInterval.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'cbSendPublishedDashboardToDataGateway
        '
        resources.ApplyResources(Me.cbSendPublishedDashboardToDataGateway, "cbSendPublishedDashboardToDataGateway")
        Me.cbSendPublishedDashboardToDataGateway.Name = "cbSendPublishedDashboardToDataGateway"
        Me.cbSendPublishedDashboardToDataGateway.UseVisualStyleBackColor = true
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.publishedDashboardSettings, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.gbMonitoring, 0, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.gbWorkQueueAnalysis, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.btnApply, 0, 5)
        Me.TableLayoutPanel1.Controls.Add(Me.DBAccess, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.gbSessionLogs, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'gbWorkQueueAnalysis
        '
        Me.gbWorkQueueAnalysis.Controls.Add(Me.cbSendWorkQueueAnalysisToDataGateway)
        resources.ApplyResources(Me.gbWorkQueueAnalysis, "gbWorkQueueAnalysis")
        Me.gbWorkQueueAnalysis.Name = "gbWorkQueueAnalysis"
        Me.gbWorkQueueAnalysis.TabStop = false
        '
        'cbSendWorkQueueAnalysisToDataGateway
        '
        resources.ApplyResources(Me.cbSendWorkQueueAnalysisToDataGateway, "cbSendWorkQueueAnalysisToDataGateway")
        Me.cbSendWorkQueueAnalysisToDataGateway.Name = "cbSendWorkQueueAnalysisToDataGateway"
        Me.cbSendWorkQueueAnalysisToDataGateway.UseVisualStyleBackColor = true
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.BackColor = System.Drawing.Color.White
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = false
        '
        'DBAccess
        '
        Me.DBAccess.Controls.Add(Me.radSQLAuth)
        Me.DBAccess.Controls.Add(Me.radIntegratedSecurity)
        Me.DBAccess.Controls.Add(Me.lblWindowsAuthWarning)
        Me.DBAccess.Controls.Add(Me.sqlServerDatagatewaysPort)
        Me.DBAccess.Controls.Add(Me.Label3)
        Me.DBAccess.Controls.Add(Me.cboDGCredentials)
        resources.ApplyResources(Me.DBAccess, "DBAccess")
        Me.DBAccess.Name = "DBAccess"
        Me.DBAccess.TabStop = false
        '
        'radSQLAuth
        '
        resources.ApplyResources(Me.radSQLAuth, "radSQLAuth")
        Me.radSQLAuth.ButtonHeight = 21
        Me.radSQLAuth.Checked = true
        Me.radSQLAuth.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.radSQLAuth.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.radSQLAuth.FocusDiameter = 16
        Me.radSQLAuth.FocusThickness = 3
        Me.radSQLAuth.FocusYLocation = 9
        Me.radSQLAuth.ForceFocus = true
        Me.radSQLAuth.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.radSQLAuth.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        Me.radSQLAuth.MouseLeaveColor = System.Drawing.Color.White
        Me.radSQLAuth.Name = "radSQLAuth"
        Me.radSQLAuth.RadioButtonDiameter = 12
        Me.radSQLAuth.RadioButtonThickness = 2
        Me.radSQLAuth.RadioYLocation = 7
        Me.radSQLAuth.StringYLocation = 1
        Me.radSQLAuth.TabStop = true
        Me.radSQLAuth.TextColor = System.Drawing.Color.Black
        Me.radSQLAuth.UseVisualStyleBackColor = true
        '
        'radIntegratedSecurity
        '
        resources.ApplyResources(Me.radIntegratedSecurity, "radIntegratedSecurity")
        Me.radIntegratedSecurity.BackColor = System.Drawing.SystemColors.Control
        Me.radIntegratedSecurity.ButtonHeight = 21
        Me.radIntegratedSecurity.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.radIntegratedSecurity.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.radIntegratedSecurity.FocusDiameter = 16
        Me.radIntegratedSecurity.FocusThickness = 3
        Me.radIntegratedSecurity.FocusYLocation = 9
        Me.radIntegratedSecurity.ForceFocus = true
        Me.radIntegratedSecurity.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.radIntegratedSecurity.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        Me.radIntegratedSecurity.MouseLeaveColor = System.Drawing.Color.White
        Me.radIntegratedSecurity.Name = "radIntegratedSecurity"
        Me.radIntegratedSecurity.RadioButtonDiameter = 12
        Me.radIntegratedSecurity.RadioButtonThickness = 2
        Me.radIntegratedSecurity.RadioYLocation = 7
        Me.radIntegratedSecurity.StringYLocation = 1
        Me.radIntegratedSecurity.TextColor = System.Drawing.Color.Black
        Me.radIntegratedSecurity.UseVisualStyleBackColor = false
        '
        'lblWindowsAuthWarning
        '
        resources.ApplyResources(Me.lblWindowsAuthWarning, "lblWindowsAuthWarning")
        Me.lblWindowsAuthWarning.ForeColor = System.Drawing.Color.FromArgb(CType(CType(203,Byte),Integer), CType(CType(98,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.lblWindowsAuthWarning.Name = "lblWindowsAuthWarning"
        '
        'sqlServerDatagatewaysPort
        '
        resources.ApplyResources(Me.sqlServerDatagatewaysPort, "sqlServerDatagatewaysPort")
        Me.sqlServerDatagatewaysPort.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
        Me.sqlServerDatagatewaysPort.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.sqlServerDatagatewaysPort.Name = "sqlServerDatagatewaysPort"
        Me.sqlServerDatagatewaysPort.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'cboDGCredentials
        '
        Me.cboDGCredentials.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDGCredentials.FormattingEnabled = true
        resources.ApplyResources(Me.cboDGCredentials, "cboDGCredentials")
        Me.cboDGCredentials.Name = "cboDGCredentials"
        '
        'gbSessionLogs
        '
        resources.ApplyResources(Me.gbSessionLogs, "gbSessionLogs")
        Me.gbSessionLogs.Controls.Add(Me.cbEmitToDataGateways)
        Me.gbSessionLogs.Controls.Add(Me.cbWriteToDatabase)
        Me.gbSessionLogs.Controls.Add(Me.Label1)
        Me.gbSessionLogs.Name = "gbSessionLogs"
        Me.gbSessionLogs.TabStop = false
        '
        'cbEmitToDataGateways
        '
        resources.ApplyResources(Me.cbEmitToDataGateways, "cbEmitToDataGateways")
        Me.cbEmitToDataGateways.Name = "cbEmitToDataGateways"
        Me.cbEmitToDataGateways.UseVisualStyleBackColor = true
        '
        'cbWriteToDatabase
        '
        resources.ApplyResources(Me.cbWriteToDatabase, "cbWriteToDatabase")
        Me.cbWriteToDatabase.Name = "cbWriteToDatabase"
        Me.cbWriteToDatabase.UseVisualStyleBackColor = true
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.FillWeight = 50!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        '
        'ctlDataPipelineSettings
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlDataPipelineSettings"
        Me.gbMonitoring.ResumeLayout(false)
        Me.gbMonitoring.PerformLayout
        CType(Me.numFrequency,System.ComponentModel.ISupportInitialize).EndInit
        Me.publishedDashboardSettings.ResumeLayout(false)
        Me.publishedDashboardSettings.PerformLayout
        CType(Me.dgvPublishedDashboards,System.ComponentModel.ISupportInitialize).EndInit
        Me.TableLayoutPanel1.ResumeLayout(false)
        Me.TableLayoutPanel1.PerformLayout
        Me.gbWorkQueueAnalysis.ResumeLayout(false)
        Me.gbWorkQueueAnalysis.PerformLayout
        Me.DBAccess.ResumeLayout(false)
        Me.DBAccess.PerformLayout
        CType(Me.sqlServerDatagatewaysPort,System.ComponentModel.ISupportInitialize).EndInit
        Me.gbSessionLogs.ResumeLayout(false)
        Me.gbSessionLogs.PerformLayout
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents gbMonitoring As GroupBox
    Friend WithEvents numFrequency As AutomateControls.StyledNumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents publishedDashboardSettings As GroupBox
    Friend WithEvents dgvPublishedDashboards As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents cbSendPublishedDashboardToDataGateway As CheckBox
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents colId As DataGridViewTextBoxColumn
    Friend WithEvents colName As DataGridViewTextBoxColumn
    Friend WithEvents colInterval As AutomateControls.DataGridViews.DataGridViewNumericUpDownColumn
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents gbSessionLogs As GroupBox
    Friend WithEvents Label1 As Label
    Friend WithEvents cbWriteToDatabase As CheckBox
    Friend WithEvents cbEmitToDataGateways As CheckBox
    Friend WithEvents gbWorkQueueAnalysis As GroupBox
    Friend WithEvents cbSendWorkQueueAnalysisToDataGateway As CheckBox
    Friend WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents DBAccess As GroupBox
    Friend WithEvents cboDGCredentials As ComboBox
    Friend WithEvents sqlServerDatagatewaysPort As AutomateControls.StyledNumericUpDown
    Friend WithEvents Label3 As Label
    Friend WithEvents lblWindowsAuthWarning As Label
    Friend WithEvents radIntegratedSecurity As AutomateControls.StyledRadioButton
    Friend WithEvents radSQLAuth As AutomateControls.StyledRadioButton
End Class
