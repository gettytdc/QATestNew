<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmEffectivePermissions
    Inherits AutomateControls.Forms.HelpButtonForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEffectivePermissions))
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.tbar = New AutomateControls.TitleBar()
        Me.dgvRoles = New System.Windows.Forms.DataGridView()
        Me.colRole = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvPermissions = New System.Windows.Forms.DataGridView()
        Me.colChecked = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colPermission = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvGroupInfo = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.Type = New System.Windows.Forms.DataGridViewImageColumn()
        Me.colGroupInfo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.View = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.lblGroupInfo = New System.Windows.Forms.Label()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvRecordPermissions = New System.Windows.Forms.DataGridView()
        Me.colRecordLevelPermChecked = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colRecordLevelPermission = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        CType(Me.dgvRoles, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvPermissions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvGroupInfo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvRecordPermissions, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tbar
        '
        resources.ApplyResources(Me.tbar, "tbar")
        Me.tbar.Name = "tbar"
        '
        'dgvRoles
        '
        Me.dgvRoles.AllowUserToAddRows = False
        Me.dgvRoles.AllowUserToDeleteRows = False
        Me.dgvRoles.AllowUserToResizeColumns = False
        Me.dgvRoles.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvRoles, "dgvRoles")
        Me.dgvRoles.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRoles.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colRole})
        Me.dgvRoles.MultiSelect = False
        Me.dgvRoles.Name = "dgvRoles"
        Me.dgvRoles.RowHeadersVisible = False
        Me.dgvRoles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colRole
        '
        Me.colRole.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colRole, "colRole")
        Me.colRole.Name = "colRole"
        Me.colRole.ReadOnly = True
        Me.colRole.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'dgvPermissions
        '
        Me.dgvPermissions.AllowUserToAddRows = False
        Me.dgvPermissions.AllowUserToDeleteRows = False
        Me.dgvPermissions.AllowUserToResizeColumns = False
        Me.dgvPermissions.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvPermissions, "dgvPermissions")
        Me.dgvPermissions.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvPermissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvPermissions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colChecked, Me.colPermission})
        Me.dgvPermissions.MultiSelect = False
        Me.dgvPermissions.Name = "dgvPermissions"
        Me.dgvPermissions.ReadOnly = True
        Me.dgvPermissions.RowHeadersVisible = False
        Me.dgvPermissions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colChecked
        '
        Me.colChecked.Frozen = True
        resources.ApplyResources(Me.colChecked, "colChecked")
        Me.colChecked.Name = "colChecked"
        Me.colChecked.ReadOnly = True
        '
        'colPermission
        '
        Me.colPermission.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colPermission, "colPermission")
        Me.colPermission.Name = "colPermission"
        Me.colPermission.ReadOnly = True
        Me.colPermission.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'dgvGroupInfo
        '
        resources.ApplyResources(Me.dgvGroupInfo, "dgvGroupInfo")
        Me.dgvGroupInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvGroupInfo.ColumnHeadersVisible = False
        Me.dgvGroupInfo.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Type, Me.colGroupInfo, Me.View})
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvGroupInfo.DefaultCellStyle = DataGridViewCellStyle2
        Me.dgvGroupInfo.Name = "dgvGroupInfo"
        Me.dgvGroupInfo.ReadOnly = True
        '
        'Type
        '
        Me.Type.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.Type, "Type")
        Me.Type.Name = "Type"
        Me.Type.ReadOnly = True
        Me.Type.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Type.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colGroupInfo
        '
        Me.colGroupInfo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colGroupInfo, "colGroupInfo")
        Me.colGroupInfo.Name = "colGroupInfo"
        Me.colGroupInfo.ReadOnly = True
        '
        'View
        '
        Me.View.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.View.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.View, "View")
        Me.View.Name = "View"
        Me.View.ReadOnly = true
        Me.View.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.View.UseColumnTextForLinkValue = True
        '
        'lblGroupInfo
        '
        resources.ApplyResources(Me.lblGroupInfo, "lblGroupInfo")
        Me.lblGroupInfo.Name = "lblGroupInfo"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        Me.DataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        Me.DataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = true
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        '
        'dgvRecordPermissions
        '
        Me.dgvRecordPermissions.AllowUserToAddRows = false
        Me.dgvRecordPermissions.AllowUserToDeleteRows = false
        Me.dgvRecordPermissions.AllowUserToResizeColumns = false
        Me.dgvRecordPermissions.AllowUserToResizeRows = false
        resources.ApplyResources(Me.dgvRecordPermissions, "dgvRecordPermissions")
        Me.dgvRecordPermissions.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvRecordPermissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRecordPermissions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colRecordLevelPermChecked, Me.colRecordLevelPermission})
        Me.dgvRecordPermissions.MultiSelect = false
        Me.dgvRecordPermissions.Name = "dgvRecordPermissions"
        Me.dgvRecordPermissions.ReadOnly = true
        Me.dgvRecordPermissions.RowHeadersVisible = false
        Me.dgvRecordPermissions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colRecordLevelPermChecked
        '
        Me.colRecordLevelPermChecked.Frozen = true
        resources.ApplyResources(Me.colRecordLevelPermChecked, "colRecordLevelPermChecked")
        Me.colRecordLevelPermChecked.Name = "colRecordLevelPermChecked"
        Me.colRecordLevelPermChecked.ReadOnly = true
        '
        'colRecordLevelPermission
        '
        Me.colRecordLevelPermission.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colRecordLevelPermission, "colRecordLevelPermission")
        Me.colRecordLevelPermission.Name = "colRecordLevelPermission"
        Me.colRecordLevelPermission.ReadOnly = true
        Me.colRecordLevelPermission.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'frmEffectivePermissions
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.dgvRecordPermissions)
        Me.Controls.Add(Me.lblGroupInfo)
        Me.Controls.Add(Me.dgvGroupInfo)
        Me.Controls.Add(Me.dgvPermissions)
        Me.Controls.Add(Me.dgvRoles)
        Me.Controls.Add(Me.tbar)
        Me.HelpButton = true
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "frmEffectivePermissions"
        CType(Me.dgvRoles,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.dgvPermissions,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.dgvGroupInfo,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.dgvRecordPermissions,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents tbar As AutomateControls.TitleBar
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents dgvRoles As DataGridView
    Friend WithEvents colRole As DataGridViewTextBoxColumn
    Friend WithEvents dgvPermissions As DataGridView
    Friend WithEvents dgvGroupInfo As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents lblGroupInfo As Label
    Friend WithEvents colChecked As DataGridViewCheckBoxColumn
    Friend WithEvents colPermission As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents Type As DataGridViewImageColumn
    Friend WithEvents colGroupInfo As DataGridViewTextBoxColumn
    Friend WithEvents View As DataGridViewLinkColumn
    Friend WithEvents dgvRecordPermissions As DataGridView
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents colRecordLevelPermChecked As DataGridViewCheckBoxColumn
    Friend WithEvents colRecordLevelPermission As DataGridViewTextBoxColumn
End Class
