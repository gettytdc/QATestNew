<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemLicense
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemLicense))
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle8 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle9 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.lblAlerts = New System.Windows.Forms.Label()
        Me.lblResources = New System.Windows.Forms.Label()
        Me.lblSessions = New System.Windows.Forms.Label()
        Me.lblProcesses = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.llNewLicense = New AutomateControls.BulletedLinkLabel()
        Me.dgvLicenses = New System.Windows.Forms.DataGridView()
        Me.chkShowExpiredLicenses = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtLicenseType = New System.Windows.Forms.Label()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.DataGridViewTextBoxColumn10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn11 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn13 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn15 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn16 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn17 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn18 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colActivationStatus = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.colOwner = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStartDate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colExpiryDate = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colProcesses = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colSessions = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colResources = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colAlerts = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Standalone = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DecipherIDP = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colRemove = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.GroupBox1.SuspendLayout
        Me.GroupBox2.SuspendLayout
        Me.FlowLayoutPanel1.SuspendLayout
        CType(Me.dgvLicenses,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.lblAlerts)
        Me.GroupBox1.Controls.Add(Me.lblResources)
        Me.GroupBox1.Controls.Add(Me.lblSessions)
        Me.GroupBox1.Controls.Add(Me.lblProcesses)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = false
        '
        'lblAlerts
        '
        resources.ApplyResources(Me.lblAlerts, "lblAlerts")
        Me.lblAlerts.Name = "lblAlerts"
        '
        'lblResources
        '
        resources.ApplyResources(Me.lblResources, "lblResources")
        Me.lblResources.Name = "lblResources"
        '
        'lblSessions
        '
        resources.ApplyResources(Me.lblSessions, "lblSessions")
        Me.lblSessions.Name = "lblSessions"
        '
        'lblProcesses
        '
        resources.ApplyResources(Me.lblProcesses, "lblProcesses")
        Me.lblProcesses.Name = "lblProcesses"
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.FlowLayoutPanel1)
        Me.GroupBox2.Controls.Add(Me.dgvLicenses)
        Me.GroupBox2.Controls.Add(Me.chkShowExpiredLicenses)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = false
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.llNewLicense)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'llNewLicense
        '
        resources.ApplyResources(Me.llNewLicense, "llNewLicense")
        Me.llNewLicense.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer), CType(CType(64,Byte),Integer))
        Me.llNewLicense.Name = "llNewLicense"
        Me.llNewLicense.TabStop = true
        '
        'dgvLicenses
        '
        Me.dgvLicenses.AllowUserToAddRows = false
        Me.dgvLicenses.AllowUserToDeleteRows = false
        Me.dgvLicenses.AllowUserToResizeColumns = false
        Me.dgvLicenses.AllowUserToResizeRows = false
        resources.ApplyResources(Me.dgvLicenses, "dgvLicenses")
        Me.dgvLicenses.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvLicenses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvLicenses.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colStatus, Me.colActivationStatus, Me.colOwner, Me.colStartDate, Me.colExpiryDate, Me.colProcesses, Me.colSessions, Me.colResources, Me.colAlerts, Me.Standalone, Me.DecipherIDP, Me.colRemove})
        DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        DataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvLicenses.DefaultCellStyle = DataGridViewCellStyle5
        Me.dgvLicenses.MultiSelect = false
        Me.dgvLicenses.Name = "dgvLicenses"
        Me.dgvLicenses.ReadOnly = true
        Me.dgvLicenses.RowHeadersVisible = false
        Me.dgvLicenses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'chkShowExpiredLicenses
        '
        resources.ApplyResources(Me.chkShowExpiredLicenses, "chkShowExpiredLicenses")
        Me.chkShowExpiredLicenses.Name = "chkShowExpiredLicenses"
        Me.chkShowExpiredLicenses.UseVisualStyleBackColor = true
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtLicenseType
        '
        resources.ApplyResources(Me.txtLicenseType, "txtLicenseType")
        Me.txtLicenseType.Name = "txtLicenseType"
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.ContextMenuStrip1, "ContextMenuStrip1")
        '
        'DataGridViewTextBoxColumn10
        '
        Me.DataGridViewTextBoxColumn10.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn10.FillWeight = 90!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn10, "DataGridViewTextBoxColumn10")
        Me.DataGridViewTextBoxColumn10.Name = "DataGridViewTextBoxColumn10"
        '
        'DataGridViewTextBoxColumn11
        '
        Me.DataGridViewTextBoxColumn11.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn11, "DataGridViewTextBoxColumn11")
        Me.DataGridViewTextBoxColumn11.Name = "DataGridViewTextBoxColumn11"
        '
        'DataGridViewTextBoxColumn12
        '
        Me.DataGridViewTextBoxColumn12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn12, "DataGridViewTextBoxColumn12")
        Me.DataGridViewTextBoxColumn12.Name = "DataGridViewTextBoxColumn12"
        '
        'DataGridViewTextBoxColumn13
        '
        Me.DataGridViewTextBoxColumn13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn13, "DataGridViewTextBoxColumn13")
        Me.DataGridViewTextBoxColumn13.Name = "DataGridViewTextBoxColumn13"
        '
        'DataGridViewTextBoxColumn14
        '
        Me.DataGridViewTextBoxColumn14.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.DataGridViewTextBoxColumn14.DefaultCellStyle = DataGridViewCellStyle6
        resources.ApplyResources(Me.DataGridViewTextBoxColumn14, "DataGridViewTextBoxColumn14")
        Me.DataGridViewTextBoxColumn14.Name = "DataGridViewTextBoxColumn14"
        Me.DataGridViewTextBoxColumn14.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn15
        '
        Me.DataGridViewTextBoxColumn15.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.DataGridViewTextBoxColumn15.DefaultCellStyle = DataGridViewCellStyle7
        resources.ApplyResources(Me.DataGridViewTextBoxColumn15, "DataGridViewTextBoxColumn15")
        Me.DataGridViewTextBoxColumn15.Name = "DataGridViewTextBoxColumn15"
        Me.DataGridViewTextBoxColumn15.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn16
        '
        Me.DataGridViewTextBoxColumn16.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.DataGridViewTextBoxColumn16.DefaultCellStyle = DataGridViewCellStyle8
        resources.ApplyResources(Me.DataGridViewTextBoxColumn16, "DataGridViewTextBoxColumn16")
        Me.DataGridViewTextBoxColumn16.Name = "DataGridViewTextBoxColumn16"
        Me.DataGridViewTextBoxColumn16.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn17
        '
        Me.DataGridViewTextBoxColumn17.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.DataGridViewTextBoxColumn17.DefaultCellStyle = DataGridViewCellStyle9
        resources.ApplyResources(Me.DataGridViewTextBoxColumn17, "DataGridViewTextBoxColumn17")
        Me.DataGridViewTextBoxColumn17.Name = "DataGridViewTextBoxColumn17"
        Me.DataGridViewTextBoxColumn17.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn18
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn18, "DataGridViewTextBoxColumn18")
        Me.DataGridViewTextBoxColumn18.Name = "DataGridViewTextBoxColumn18"
        '
        'colStatus
        '
        Me.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colStatus.FillWeight = 90!
        resources.ApplyResources(Me.colStatus, "colStatus")
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = true
        '
        'colActivationStatus
        '
        Me.colActivationStatus.ActiveLinkColor = System.Drawing.Color.DodgerBlue
        resources.ApplyResources(Me.colActivationStatus, "colActivationStatus")
        Me.colActivationStatus.LinkColor = System.Drawing.Color.Black
        Me.colActivationStatus.Name = "colActivationStatus"
        Me.colActivationStatus.ReadOnly = true
        '
        'colOwner
        '
        Me.colOwner.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colOwner, "colOwner")
        Me.colOwner.Name = "colOwner"
        Me.colOwner.ReadOnly = true
        '
        'colStartDate
        '
        Me.colStartDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colStartDate, "colStartDate")
        Me.colStartDate.Name = "colStartDate"
        Me.colStartDate.ReadOnly = true
        '
        'colExpiryDate
        '
        Me.colExpiryDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colExpiryDate, "colExpiryDate")
        Me.colExpiryDate.Name = "colExpiryDate"
        Me.colExpiryDate.ReadOnly = true
        '
        'colProcesses
        '
        Me.colProcesses.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colProcesses.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.colProcesses, "colProcesses")
        Me.colProcesses.Name = "colProcesses"
        Me.colProcesses.ReadOnly = true
        Me.colProcesses.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colSessions
        '
        Me.colSessions.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colSessions.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.colSessions, "colSessions")
        Me.colSessions.Name = "colSessions"
        Me.colSessions.ReadOnly = true
        Me.colSessions.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colResources
        '
        Me.colResources.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colResources.DefaultCellStyle = DataGridViewCellStyle3
        resources.ApplyResources(Me.colResources, "colResources")
        Me.colResources.Name = "colResources"
        Me.colResources.ReadOnly = true
        Me.colResources.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colAlerts
        '
        Me.colAlerts.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.colAlerts.DefaultCellStyle = DataGridViewCellStyle4
        resources.ApplyResources(Me.colAlerts, "colAlerts")
        Me.colAlerts.Name = "colAlerts"
        Me.colAlerts.ReadOnly = true
        Me.colAlerts.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'Standalone
        '
        resources.ApplyResources(Me.Standalone, "Standalone")
        Me.Standalone.Name = "Standalone"
        Me.Standalone.ReadOnly = true
        '
        'DecipherIDP
        '
        resources.ApplyResources(Me.DecipherIDP, "DecipherIDP")
        Me.DecipherIDP.Name = "DecipherIDP"
        Me.DecipherIDP.ReadOnly = true
        Me.DecipherIDP.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DecipherIDP.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colRemove
        '
        Me.colRemove.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colRemove, "colRemove")
        Me.colRemove.Name = "colRemove"
        Me.colRemove.ReadOnly = true
        Me.colRemove.Text = "Remove"
        '
        'ctlSystemLicense
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtLicenseType)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "ctlSystemLicense"
        resources.ApplyResources(Me, "$this")
        Me.GroupBox1.ResumeLayout(false)
        Me.GroupBox1.PerformLayout
        Me.GroupBox2.ResumeLayout(false)
        Me.GroupBox2.PerformLayout
        Me.FlowLayoutPanel1.ResumeLayout(false)
        Me.FlowLayoutPanel1.PerformLayout
        CType(Me.dgvLicenses,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents chkShowExpiredLicenses As System.Windows.Forms.CheckBox
    Friend WithEvents dgvLicenses As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn7 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn8 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents lblAlerts As System.Windows.Forms.Label
    Friend WithEvents lblResources As System.Windows.Forms.Label
    Friend WithEvents lblSessions As System.Windows.Forms.Label
    Friend WithEvents lblProcesses As System.Windows.Forms.Label
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents DataGridViewTextBoxColumn9 As DataGridViewTextBoxColumn
    Friend WithEvents Label1 As Label
    Friend WithEvents txtLicenseType As Label
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents llNewLicense As AutomateControls.BulletedLinkLabel
    Friend WithEvents DataGridViewTextBoxColumn10 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn11 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn12 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn13 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn14 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn15 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn16 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn17 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn18 As DataGridViewTextBoxColumn
    Friend WithEvents colStatus As DataGridViewTextBoxColumn
    Friend WithEvents colActivationStatus As DataGridViewLinkColumn
    Friend WithEvents colOwner As DataGridViewTextBoxColumn
    Friend WithEvents colStartDate As DataGridViewTextBoxColumn
    Friend WithEvents colExpiryDate As DataGridViewTextBoxColumn
    Friend WithEvents colProcesses As DataGridViewTextBoxColumn
    Friend WithEvents colSessions As DataGridViewTextBoxColumn
    Friend WithEvents colResources As DataGridViewTextBoxColumn
    Friend WithEvents colAlerts As DataGridViewTextBoxColumn
    Friend WithEvents Standalone As DataGridViewTextBoxColumn
    Friend WithEvents DecipherIDP As DataGridViewTextBoxColumn
    Friend WithEvents colRemove As DataGridViewLinkColumn
End Class
