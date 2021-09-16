<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CtlUsersSummary(Of T As Class)
	Inherits System.Windows.Forms.UserControl


	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
        Me.lblChosenUsers = New System.Windows.Forms.Label()
        Me.lblUserCount = New System.Windows.Forms.Label()
        Me.lblRoles = New System.Windows.Forms.Label()
        Me.gridUsers = New System.Windows.Forms.DataGridView()
        Me.gridRoles = New System.Windows.Forms.DataGridView()
        Me.colRole = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDelete = New System.Windows.Forms.DataGridViewImageColumn()
        CType(Me.gridUsers,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.gridRoles,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'lblChosenUsers
        '
        Me.lblChosenUsers.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.lblChosenUsers.AutoSize = true
        Me.lblChosenUsers.Location = New System.Drawing.Point(13, 15)
        Me.lblChosenUsers.Name = "lblChosenUsers"
        Me.lblChosenUsers.Size = New System.Drawing.Size(381, 17)
        Me.lblChosenUsers.TabIndex = 0
        Me.lblChosenUsers.Text = "You have selected to add the following users to Blue Prism:"
        '
        'lblUserCount
        '
        Me.lblUserCount.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.lblUserCount.AutoSize = true
        Me.lblUserCount.Location = New System.Drawing.Point(439, 238)
        Me.lblUserCount.Name = "lblUserCount"
        Me.lblUserCount.Size = New System.Drawing.Size(167, 17)
        Me.lblUserCount.TabIndex = 2
        Me.lblUserCount.Text = "Total number of users: xx"
        '
        'lblRoles
        '
        Me.lblRoles.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.lblRoles.AutoSize = true
        Me.lblRoles.Location = New System.Drawing.Point(13, 260)
        Me.lblRoles.Name = "lblRoles"
        Me.lblRoles.Size = New System.Drawing.Size(416, 17)
        Me.lblRoles.TabIndex = 3
        Me.lblRoles.Text = "You have selected the following Blue Prism roles for these users:"
        '
        'gridUsers
        '
        Me.gridUsers.AllowUserToAddRows = false
        Me.gridUsers.AllowUserToResizeColumns = false
        Me.gridUsers.AllowUserToResizeRows = false
        Me.gridUsers.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.gridUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.gridUsers.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.gridUsers.BackgroundColor = System.Drawing.SystemColors.ControlLightLight
        Me.gridUsers.CausesValidation = false
        Me.gridUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridUsers.ColumnHeadersVisible = false
        Me.gridUsers.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colDelete})
        Me.gridUsers.GridColor = System.Drawing.SystemColors.ActiveBorder
        Me.gridUsers.Location = New System.Drawing.Point(16, 36)
        Me.gridUsers.MultiSelect = false
        Me.gridUsers.Name = "gridUsers"
        Me.gridUsers.RowHeadersVisible = false
        Me.gridUsers.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.gridUsers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.gridUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.gridUsers.ShowEditingIcon = false
        Me.gridUsers.Size = New System.Drawing.Size(548, 193)
        Me.gridUsers.TabIndex = 19
        '
        'gridRoles
        '
        Me.gridRoles.AllowUserToAddRows = false
        Me.gridRoles.AllowUserToResizeRows = false
        Me.gridRoles.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.gridRoles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.gridRoles.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.gridRoles.BackgroundColor = System.Drawing.SystemColors.ControlLightLight
        Me.gridRoles.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.gridRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridRoles.ColumnHeadersVisible = false
        Me.gridRoles.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colRole})
        Me.gridRoles.GridColor = System.Drawing.SystemColors.ControlLightLight
        Me.gridRoles.Location = New System.Drawing.Point(16, 284)
        Me.gridRoles.Name = "gridRoles"
        Me.gridRoles.ReadOnly = true
        Me.gridRoles.RowHeadersVisible = false
        Me.gridRoles.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.gridRoles.Size = New System.Drawing.Size(548, 113)
        Me.gridRoles.TabIndex = 20
        '
        'colRole
        '
        Me.colRole.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colRole.FillWeight = 175!
        Me.colRole.HeaderText = ""
        Me.colRole.Name = "colRole"
        Me.colRole.ReadOnly = true
        Me.colRole.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn1.FillWeight = 175!
        Me.DataGridViewTextBoxColumn1.HeaderText = ""
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        Me.DataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn2.FillWeight = 175!
        Me.DataGridViewTextBoxColumn2.HeaderText = ""
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        Me.DataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'colDelete
        '
        Me.colDelete.FillWeight = 25!
        Me.colDelete.HeaderText = " "
        Me.colDelete.Name = "colDelete"
        Me.colDelete.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.colDelete.Width = 5
        '
        'ctlUsersSummary
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblChosenUsers)
        Me.Controls.Add(Me.lblUserCount)
        Me.Controls.Add(Me.lblRoles)
        Me.Controls.Add(Me.gridRoles)
        Me.Controls.Add(Me.gridUsers)
        Me.Name = "ctlUsersSummary"
        Me.Size = New System.Drawing.Size(577, 413)
        CType(Me.gridUsers,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.gridRoles,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

	Friend WithEvents lblChosenUsers As Label
	Friend WithEvents lblUserCount As Label
	Friend WithEvents lblRoles As Label
	Private WithEvents gridRoles As DataGridView
	Friend WithEvents colRole As DataGridViewTextBoxColumn
	Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
	Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
	Private WithEvents gridUsers As DataGridView
    Friend WithEvents colDelete As DataGridViewImageColumn
End Class
