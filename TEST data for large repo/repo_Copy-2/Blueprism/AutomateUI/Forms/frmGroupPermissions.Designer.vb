<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGroupPermissions
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmGroupPermissions))
        Me.tbar = New AutomateControls.TitleBar()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlRestrictions = New System.Windows.Forms.Panel()
        Me.btnSelectAll = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDeselectAll = New AutomateControls.Buttons.StandardStyledButton()
        Me.cbxSelectAllGroupPerms = New System.Windows.Forms.CheckBox()
        Me.cbxSelectAllItemPerms = New System.Windows.Forms.CheckBox()
        Me.lblRecordLevelPermissions = New System.Windows.Forms.Label()
        Me.lblGroupLevelPermission = New System.Windows.Forms.Label()
        Me.dgvRecordPermissions = New System.Windows.Forms.DataGridView()
        Me.colRecordLevelPermChecked = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colRecordLevelPermission = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.rbInherited = New AutomateControls.StyledRadioButton()
        Me.dgvGroupPermissions = New System.Windows.Forms.DataGridView()
        Me.colChecked = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colPermission = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dgvRoles = New System.Windows.Forms.DataGridView()
        Me.colRole = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.rbUnrestricted = New AutomateControls.StyledRadioButton()
        Me.rbRestricted = New AutomateControls.StyledRadioButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.dgvUsers = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colUserName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lblUsersInRole = New System.Windows.Forms.Label()
        Me.pnlUsersInRole = New System.Windows.Forms.Panel()
        Me.lblGroupVisibilityWarning = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlOKCancel = New System.Windows.Forms.Panel()
        Me.pnlRestrictions.SuspendLayout()
        CType(Me.dgvRecordPermissions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvGroupPermissions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvRoles, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlUsersInRole.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.pnlOKCancel.SuspendLayout()
        Me.SuspendLayout()
        '
        'tbar
        '
        resources.ApplyResources(Me.tbar, "tbar")
        Me.tbar.Name = "tbar"
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        Me.DataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        Me.DataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'pnlRestrictions
        '
        Me.pnlRestrictions.Controls.Add(Me.btnSelectAll)
        Me.pnlRestrictions.Controls.Add(Me.btnDeselectAll)
        Me.pnlRestrictions.Controls.Add(Me.cbxSelectAllGroupPerms)
        Me.pnlRestrictions.Controls.Add(Me.cbxSelectAllItemPerms)
        Me.pnlRestrictions.Controls.Add(Me.lblRecordLevelPermissions)
        Me.pnlRestrictions.Controls.Add(Me.lblGroupLevelPermission)
        Me.pnlRestrictions.Controls.Add(Me.dgvRecordPermissions)
        Me.pnlRestrictions.Controls.Add(Me.rbInherited)
        Me.pnlRestrictions.Controls.Add(Me.dgvGroupPermissions)
        Me.pnlRestrictions.Controls.Add(Me.dgvRoles)
        Me.pnlRestrictions.Controls.Add(Me.rbUnrestricted)
        Me.pnlRestrictions.Controls.Add(Me.rbRestricted)
        resources.ApplyResources(Me.pnlRestrictions, "pnlRestrictions")
        Me.pnlRestrictions.Name = "pnlRestrictions"
        '
        'btnSelectAll
        '
        resources.ApplyResources(Me.btnSelectAll, "btnSelectAll")
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.UseVisualStyleBackColor = True
        '
        'btnDeselectAll
        '
        resources.ApplyResources(Me.btnDeselectAll, "btnDeselectAll")
        Me.btnDeselectAll.Name = "btnDeselectAll"
        Me.btnDeselectAll.UseVisualStyleBackColor = True
        '
        'cbxSelectAllGroupPerms
        '
        resources.ApplyResources(Me.cbxSelectAllGroupPerms, "cbxSelectAllGroupPerms")
        Me.cbxSelectAllGroupPerms.Name = "cbxSelectAllGroupPerms"
        Me.cbxSelectAllGroupPerms.UseVisualStyleBackColor = True
        '
        'cbxSelectAllItemPerms
        '
        resources.ApplyResources(Me.cbxSelectAllItemPerms, "cbxSelectAllItemPerms")
        Me.cbxSelectAllItemPerms.Name = "cbxSelectAllItemPerms"
        Me.cbxSelectAllItemPerms.UseVisualStyleBackColor = True
        '
        'lblRecordLevelPermissions
        '
        resources.ApplyResources(Me.lblRecordLevelPermissions, "lblRecordLevelPermissions")
        Me.lblRecordLevelPermissions.Name = "lblRecordLevelPermissions"
        '
        'lblGroupLevelPermission
        '
        resources.ApplyResources(Me.lblGroupLevelPermission, "lblGroupLevelPermission")
        Me.lblGroupLevelPermission.Name = "lblGroupLevelPermission"
        '
        'dgvRecordPermissions
        '
        Me.dgvRecordPermissions.AllowUserToAddRows = False
        Me.dgvRecordPermissions.AllowUserToDeleteRows = False
        Me.dgvRecordPermissions.AllowUserToResizeColumns = False
        Me.dgvRecordPermissions.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvRecordPermissions, "dgvRecordPermissions")
        Me.dgvRecordPermissions.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvRecordPermissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRecordPermissions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colRecordLevelPermChecked, Me.colRecordLevelPermission})
        Me.dgvRecordPermissions.MultiSelect = False
        Me.dgvRecordPermissions.Name = "dgvRecordPermissions"
        Me.dgvRecordPermissions.RowHeadersVisible = False
        Me.dgvRecordPermissions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvRecordPermissions.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvRecordPermissions.RowsDefaultCellStyle = DataGridViewCellStyle1
        '
        'colRecordLevelPermChecked
        '
        Me.colRecordLevelPermChecked.Frozen = True
        resources.ApplyResources(Me.colRecordLevelPermChecked, "colRecordLevelPermChecked")
        Me.colRecordLevelPermChecked.Name = "colRecordLevelPermChecked"
        '
        'colRecordLevelPermission
        '
        Me.colRecordLevelPermission.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colRecordLevelPermission, "colRecordLevelPermission")
        Me.colRecordLevelPermission.Name = "colRecordLevelPermission"
        Me.colRecordLevelPermission.ReadOnly = True
        Me.colRecordLevelPermission.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'rbInherited
        '
        resources.ApplyResources(Me.rbInherited, "rbInherited")
        Me.rbInherited.Name = "rbInherited"
        Me.rbInherited.TabStop = True
        Me.rbInherited.UseVisualStyleBackColor = True
        '
        'dgvGroupPermissions
        '
        Me.dgvGroupPermissions.AllowUserToAddRows = False
        Me.dgvGroupPermissions.AllowUserToDeleteRows = False
        Me.dgvGroupPermissions.AllowUserToResizeColumns = False
        Me.dgvGroupPermissions.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvGroupPermissions, "dgvGroupPermissions")
        Me.dgvGroupPermissions.BackgroundColor = System.Drawing.SystemColors.Window
        Me.dgvGroupPermissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvGroupPermissions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colChecked, Me.colPermission})
        Me.dgvGroupPermissions.MultiSelect = False
        Me.dgvGroupPermissions.Name = "dgvGroupPermissions"
        Me.dgvGroupPermissions.RowHeadersVisible = False
        Me.dgvGroupPermissions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvGroupPermissions.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.dgvGroupPermissions.RowsDefaultCellStyle = DataGridViewCellStyle2
        '
        'colChecked
        '
        Me.colChecked.Frozen = True
        resources.ApplyResources(Me.colChecked, "colChecked")
        Me.colChecked.Name = "colChecked"
        '
        'colPermission
        '
        Me.colPermission.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colPermission, "colPermission")
        Me.colPermission.Name = "colPermission"
        Me.colPermission.ReadOnly = True
        Me.colPermission.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
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
        'rbUnrestricted
        '
        resources.ApplyResources(Me.rbUnrestricted, "rbUnrestricted")
        Me.rbUnrestricted.Name = "rbUnrestricted"
        Me.rbUnrestricted.TabStop = True
        Me.rbUnrestricted.UseVisualStyleBackColor = True
        '
        'rbRestricted
        '
        resources.ApplyResources(Me.rbRestricted, "rbRestricted")
        Me.rbRestricted.Name = "rbRestricted"
        Me.rbRestricted.TabStop = True
        Me.rbRestricted.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'dgvUsers
        '
        resources.ApplyResources(Me.dgvUsers, "dgvUsers")
        Me.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvUsers.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colUserName})
        Me.dgvUsers.Name = "dgvUsers"
        Me.dgvUsers.ReadOnly = True
        '
        'colUserName
        '
        Me.colUserName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colUserName, "colUserName")
        Me.colUserName.Name = "colUserName"
        Me.colUserName.ReadOnly = True
        '
        'lblUsersInRole
        '
        resources.ApplyResources(Me.lblUsersInRole, "lblUsersInRole")
        Me.lblUsersInRole.Name = "lblUsersInRole"
        '
        'pnlUsersInRole
        '
        Me.pnlUsersInRole.Controls.Add(Me.lblGroupVisibilityWarning)
        Me.pnlUsersInRole.Controls.Add(Me.lblUsersInRole)
        Me.pnlUsersInRole.Controls.Add(Me.dgvUsers)
        resources.ApplyResources(Me.pnlUsersInRole, "pnlUsersInRole")
        Me.pnlUsersInRole.Name = "pnlUsersInRole"
        '
        'lblGroupVisibilityWarning
        '
        resources.ApplyResources(Me.lblGroupVisibilityWarning, "lblGroupVisibilityWarning")
        Me.lblGroupVisibilityWarning.Name = "lblGroupVisibilityWarning"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.pnlOKCancel, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.pnlUsersInRole, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.pnlRestrictions, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'pnlOKCancel
        '
        Me.pnlOKCancel.Controls.Add(Me.btnOK)
        Me.pnlOKCancel.Controls.Add(Me.btnCancel)
        resources.ApplyResources(Me.pnlOKCancel, "pnlOKCancel")
        Me.pnlOKCancel.Name = "pnlOKCancel"
        '
        'frmGroupPermissions
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.tbar)
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmGroupPermissions"
        Me.pnlRestrictions.ResumeLayout(False)
        Me.pnlRestrictions.PerformLayout()
        CType(Me.dgvRecordPermissions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvGroupPermissions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvRoles, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvUsers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlUsersInRole.ResumeLayout(False)
        Me.pnlUsersInRole.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.pnlOKCancel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tbar As AutomateControls.TitleBar
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents pnlRestrictions As Panel
    Friend WithEvents btnSelectAll As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDeselectAll As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cbxSelectAllGroupPerms As CheckBox
    Friend WithEvents cbxSelectAllItemPerms As CheckBox
    Friend WithEvents lblRecordLevelPermissions As Label
    Friend WithEvents lblGroupLevelPermission As Label
    Friend WithEvents dgvRecordPermissions As DataGridView
    Friend WithEvents colRecordLevelPermChecked As DataGridViewCheckBoxColumn
    Friend WithEvents colRecordLevelPermission As DataGridViewTextBoxColumn
    Friend WithEvents rbInherited As RadioButton
    Friend WithEvents dgvGroupPermissions As DataGridView
    Friend WithEvents colChecked As DataGridViewCheckBoxColumn
    Friend WithEvents colPermission As DataGridViewTextBoxColumn
    Friend WithEvents dgvRoles As DataGridView
    Friend WithEvents colRole As DataGridViewTextBoxColumn
    Friend WithEvents rbUnrestricted As RadioButton
    Friend WithEvents rbRestricted As RadioButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents dgvUsers As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents colUserName As DataGridViewTextBoxColumn
    Friend WithEvents lblUsersInRole As Label
    Friend WithEvents pnlUsersInRole As Panel
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents pnlOKCancel As Panel
    Friend WithEvents lblGroupVisibilityWarning As Label
End Class
