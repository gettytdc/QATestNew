<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlAuth
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlAuth))
        Me.ctxRoles = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuDeleteRole = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRenameRole = New System.Windows.Forms.ToolStripMenuItem()
        Me.mmuCloneRole = New System.Windows.Forms.ToolStripMenuItem()
        Me.workSearcher = New System.ComponentModel.BackgroundWorker()
        Me.timerDirSearchTimeout = New System.Windows.Forms.Timer(Me.components)
        Me.ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.splitMain = New AutomateControls.GrippableSplitContainer()
        Me.splitTop = New AutomateControls.GrippableSplitContainer()
        Me.tvRoles = New AutomateControls.Trees.FlickerFreeTreeView()
        Me.lblRolesTreeviewLabel = New System.Windows.Forms.Label()
        Me.panRoleButtons = New System.Windows.Forms.Panel()
        Me.btnNewRole = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnDeleteRole = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tvPerms = New AutomateControls.Trees.FlickerFreeTreeView()
        Me.lblPermissionsTreeviewLabel = New System.Windows.Forms.Label()
        Me.panAdGroup = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.btnGroupLookup = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtGroupPath = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtGroupName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblActiveDirectoryGroup = New System.Windows.Forms.Label()
        Me.lblFromServer = New System.Windows.Forms.Label()
        Me.gridMembers = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.colFullName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colUserPrincipalName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDistinguishedName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colValidFrom = New AutomateControls.DataGridViews.DateColumn()
        Me.colValidTo = New AutomateControls.DataGridViews.DateColumn()
        Me.colPasswordExpiry = New AutomateControls.DataGridViews.DateColumn()
        Me.colLastLoggedIn = New AutomateControls.DataGridViews.DateColumn()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.llManageRoles = New AutomateControls.BulletedLinkLabel()
        Me.ctxRoles.SuspendLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        CType(Me.splitTop, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitTop.Panel1.SuspendLayout()
        Me.splitTop.Panel2.SuspendLayout()
        Me.splitTop.SuspendLayout()
        Me.panRoleButtons.SuspendLayout()
        Me.panAdGroup.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.gridMembers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ctxRoles
        '
        Me.ctxRoles.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuDeleteRole, Me.mnuRenameRole, Me.mmuCloneRole})
        Me.ctxRoles.Name = "ctxRoles"
        resources.ApplyResources(Me.ctxRoles, "ctxRoles")
        '
        'mnuDeleteRole
        '
        Me.mnuDeleteRole.Name = "mnuDeleteRole"
        resources.ApplyResources(Me.mnuDeleteRole, "mnuDeleteRole")
        '
        'mnuRenameRole
        '
        Me.mnuRenameRole.Name = "mnuRenameRole"
        resources.ApplyResources(Me.mnuRenameRole, "mnuRenameRole")
        '
        'mmuCloneRole
        '
        Me.mmuCloneRole.Name = "mmuCloneRole"
        resources.ApplyResources(Me.mmuCloneRole, "mmuCloneRole")
        '
        'workSearcher
        '
        Me.workSearcher.WorkerReportsProgress = True
        Me.workSearcher.WorkerSupportsCancellation = True
        '
        'timerDirSearchTimeout
        '
        Me.timerDirSearchTimeout.Interval = 5000
        '
        'ToolTip
        '
        Me.ToolTip.AutomaticDelay = 150
        '
        'ImageList1
        '
        Me.ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.ImageList1, "ImageList1")
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.splitTop)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.lblFromServer)
        Me.splitMain.Panel2.Controls.Add(Me.gridMembers)
        Me.splitMain.Panel2.Controls.Add(Me.Label1)
        Me.splitMain.TabStop = False
        '
        'splitTop
        '
        resources.ApplyResources(Me.splitTop, "splitTop")
        Me.splitTop.Name = "splitTop"
        '
        'splitTop.Panel1
        '
        Me.splitTop.Panel1.Controls.Add(Me.tvRoles)
        Me.splitTop.Panel1.Controls.Add(Me.lblRolesTreeviewLabel)
        Me.splitTop.Panel1.Controls.Add(Me.panRoleButtons)
        '
        'splitTop.Panel2
        '
        Me.splitTop.Panel2.Controls.Add(Me.tvPerms)
        Me.splitTop.Panel2.Controls.Add(Me.lblPermissionsTreeviewLabel)
        Me.splitTop.Panel2.Controls.Add(Me.panAdGroup)
        Me.splitTop.TabStop = False
        '
        'tvRoles
        '
        Me.tvRoles.CheckBoxes = True
        Me.tvRoles.ContextMenuStrip = Me.ctxRoles
        resources.ApplyResources(Me.tvRoles, "tvRoles")
        Me.tvRoles.DoubleClickEnabled = False
        Me.tvRoles.HideSelection = False
        Me.tvRoles.Name = "tvRoles"
        '
        'lblRolesTreeviewLabel
        '
        resources.ApplyResources(Me.lblRolesTreeviewLabel, "lblRolesTreeviewLabel")
        Me.lblRolesTreeviewLabel.Name = "lblRolesTreeviewLabel"
        '
        'panRoleButtons
        '
        Me.panRoleButtons.Controls.Add(Me.btnNewRole)
        Me.panRoleButtons.Controls.Add(Me.btnDeleteRole)
        resources.ApplyResources(Me.panRoleButtons, "panRoleButtons")
        Me.panRoleButtons.Name = "panRoleButtons"
        '
        'btnNewRole
        '
        resources.ApplyResources(Me.btnNewRole, "btnNewRole")
        Me.btnNewRole.Name = "btnNewRole"
        Me.btnNewRole.UseVisualStyleBackColor = False
        '
        'btnDeleteRole
        '
        resources.ApplyResources(Me.btnDeleteRole, "btnDeleteRole")
        Me.btnDeleteRole.Name = "btnDeleteRole"
        Me.btnDeleteRole.UseVisualStyleBackColor = False
        '
        'tvPerms
        '
        Me.tvPerms.CheckBoxes = True
        resources.ApplyResources(Me.tvPerms, "tvPerms")
        Me.tvPerms.DoubleClickEnabled = False
        Me.tvPerms.HideSelection = False
        Me.tvPerms.Name = "tvPerms"
        Me.tvPerms.StateImageList = Me.ImageList1
        '
        'lblPermissionsTreeviewLabel
        '
        resources.ApplyResources(Me.lblPermissionsTreeviewLabel, "lblPermissionsTreeviewLabel")
        Me.lblPermissionsTreeviewLabel.Name = "lblPermissionsTreeviewLabel"
        '
        'panAdGroup
        '
        Me.panAdGroup.Controls.Add(Me.TableLayoutPanel1)
        Me.panAdGroup.Controls.Add(Me.txtGroupPath)
        Me.panAdGroup.Controls.Add(Me.lblActiveDirectoryGroup)
        resources.ApplyResources(Me.panAdGroup, "panAdGroup")
        Me.panAdGroup.Name = "panAdGroup"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.btnGroupLookup, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.txtGroupName, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'btnGroupLookup
        '
        resources.ApplyResources(Me.btnGroupLookup, "btnGroupLookup")
        Me.btnGroupLookup.Name = "btnGroupLookup"
        Me.btnGroupLookup.UseVisualStyleBackColor = False
        '
        'txtGroupPath
        '
        resources.ApplyResources(Me.txtGroupPath, "txtGroupPath")
        Me.txtGroupPath.BorderColor = System.Drawing.Color.Empty
        Me.txtGroupPath.Name = "txtGroupPath"
        Me.txtGroupPath.ReadOnly = True
        '
        'txtGroupName
        '
        resources.ApplyResources(Me.txtGroupName, "txtGroupName")
        Me.txtGroupName.BackColor = System.Drawing.SystemColors.Control
        Me.txtGroupName.BorderColor = System.Drawing.Color.Empty
        Me.txtGroupName.Name = "txtGroupName"
        Me.txtGroupName.ReadOnly = True
        '
        'lblActiveDirectoryGroup
        '
        resources.ApplyResources(Me.lblActiveDirectoryGroup, "lblActiveDirectoryGroup")
        Me.lblActiveDirectoryGroup.Name = "lblActiveDirectoryGroup"
        '
        'lblFromServer
        '
        resources.ApplyResources(Me.lblFromServer, "lblFromServer")
        Me.lblFromServer.Name = "lblFromServer"
        '
        'gridMembers
        '
        resources.ApplyResources(Me.gridMembers, "gridMembers")
        Me.gridMembers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridMembers.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colFullName, Me.colUserPrincipalName, Me.colDistinguishedName, Me.colValidFrom, Me.colValidTo, Me.colPasswordExpiry, Me.colLastLoggedIn})
        Me.gridMembers.Name = "gridMembers"
        Me.gridMembers.ReadOnly = True
        '
        'colFullName
        '
        Me.colFullName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colFullName, "colFullName")
        Me.colFullName.Name = "colFullName"
        Me.colFullName.ReadOnly = True
        '
        'colUserPrincipalName
        '
        Me.colUserPrincipalName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colUserPrincipalName, "colUserPrincipalName")
        Me.colUserPrincipalName.Name = "colUserPrincipalName"
        Me.colUserPrincipalName.ReadOnly = True
        '
        'colDistinguishedName
        '
        Me.colDistinguishedName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colDistinguishedName, "colDistinguishedName")
        Me.colDistinguishedName.Name = "colDistinguishedName"
        Me.colDistinguishedName.ReadOnly = True
        '
        'colValidFrom
        '
        Me.colValidFrom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.colValidFrom.DateFormat = Nothing
        resources.ApplyResources(Me.colValidFrom, "colValidFrom")
        Me.colValidFrom.Name = "colValidFrom"
        Me.colValidFrom.ReadOnly = True
        Me.colValidFrom.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'colValidTo
        '
        Me.colValidTo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.colValidTo.DateFormat = Nothing
        resources.ApplyResources(Me.colValidTo, "colValidTo")
        Me.colValidTo.Name = "colValidTo"
        Me.colValidTo.ReadOnly = True
        '
        'colPasswordExpiry
        '
        Me.colPasswordExpiry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.colPasswordExpiry.DateFormat = Nothing
        resources.ApplyResources(Me.colPasswordExpiry, "colPasswordExpiry")
        Me.colPasswordExpiry.Name = "colPasswordExpiry"
        Me.colPasswordExpiry.ReadOnly = True
        '
        'colLastLoggedIn
        '
        Me.colLastLoggedIn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.colLastLoggedIn.DateFormat = Nothing
        resources.ApplyResources(Me.colLastLoggedIn, "colLastLoggedIn")
        Me.colLastLoggedIn.Name = "colLastLoggedIn"
        Me.colLastLoggedIn.ReadOnly = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'llManageRoles
        '
        resources.ApplyResources(Me.llManageRoles, "llManageRoles")
        Me.llManageRoles.Image = Global.AutomateUI.My.Resources.AuthImages.Users_16x16
        Me.llManageRoles.Name = "llManageRoles"
        Me.llManageRoles.TabStop = True
        '
        'ctlAuth
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.llManageRoles)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlAuth"
        Me.ctxRoles.ResumeLayout(False)
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.splitTop.Panel1.ResumeLayout(False)
        Me.splitTop.Panel2.ResumeLayout(False)
        CType(Me.splitTop, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitTop.ResumeLayout(False)
        Me.panRoleButtons.ResumeLayout(False)
        Me.panAdGroup.ResumeLayout(False)
        Me.panAdGroup.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        CType(Me.gridMembers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnGroupLookup As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtGroupPath As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblActiveDirectoryGroup As System.Windows.Forms.Label
    Friend WithEvents lblPermissionsTreeviewLabel As System.Windows.Forms.Label
    Friend WithEvents tvPerms As AutomateControls.Trees.FlickerFreeTreeView
    Friend WithEvents btnDeleteRole As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnNewRole As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents tvRoles As AutomateControls.Trees.FlickerFreeTreeView
    Friend WithEvents lblRolesTreeviewLabel As System.Windows.Forms.Label
    Private WithEvents ctxRoles As System.Windows.Forms.ContextMenuStrip
    Private WithEvents mnuDeleteRole As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuRenameRole As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents splitTop As AutomateControls.GrippableSplitContainer
    Private WithEvents workSearcher As System.ComponentModel.BackgroundWorker
    Private WithEvents timerDirSearchTimeout As System.Windows.Forms.Timer
    Private WithEvents panAdGroup As System.Windows.Forms.Panel
    Private WithEvents panRoleButtons As System.Windows.Forms.Panel
    Private WithEvents llManageRoles As AutomateControls.BulletedLinkLabel
    Friend WithEvents ToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents txtGroupName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents splitMain As AutomateControls.GrippableSplitContainer
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents gridMembers As AutomateControls.DataGridViews.RowBasedDataGridView
    Friend WithEvents colFullName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colUserPrincipalName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colDistinguishedName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colValidFrom As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colValidTo As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colPasswordExpiry As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colLastLoggedIn As AutomateControls.DataGridViews.DateColumn
    Private WithEvents lblFromServer As System.Windows.Forms.Label
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Private WithEvents mmuCloneRole As ToolStripMenuItem
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
End Class
