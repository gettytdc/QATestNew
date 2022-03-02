<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlControlRoom
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlControlRoom))
        Me.mScheduleTreeContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ctxMenuNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuRetire = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuFileEditSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.ctxMenuClone = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuCopy = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuPaste = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuEditRunSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.ctxMenuRunNow = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuStop = New System.Windows.Forms.ToolStripMenuItem()
        Me.mSplitter = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.mControlRoomTree = New AutomateControls.Trees.FlickerFreeTreeView()
        Me.pnlTitle = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.mMenuButton = New AutomateControls.MenuButton()
        Me.mMenuButtonContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.RefreshToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.lblAreaTitle = New System.Windows.Forms.Label()
        Me.mSessionTreeContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ctxMenuRenameSessionView = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuDeleteSessionView = New System.Windows.Forms.ToolStripMenuItem()
        Me.mQueueTreeContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuExpandAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCollapseAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToggleConnectionsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mScheduleTreeContextMenu.SuspendLayout()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitter.Panel1.SuspendLayout()
        Me.mSplitter.Panel2.SuspendLayout()
        Me.mSplitter.SuspendLayout()
        Me.pnlTitle.SuspendLayout()
        Me.mMenuButtonContextMenuStrip.SuspendLayout()
        Me.mSessionTreeContextMenu.SuspendLayout()
        Me.mQueueTreeContextMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'mScheduleTreeContextMenu
        '
        Me.mScheduleTreeContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ctxMenuNew, Me.ctxMenuSave, Me.ctxMenuDelete, Me.ctxMenuRetire, Me.ctxMenuFileEditSeparator, Me.ctxMenuClone, Me.ctxMenuCopy, Me.ctxMenuPaste, Me.ctxMenuEditRunSeparator, Me.ctxMenuRunNow, Me.ctxMenuStop})
        Me.mScheduleTreeContextMenu.Name = "mScheduleTreeviewContextMenu"
        resources.ApplyResources(Me.mScheduleTreeContextMenu, "mScheduleTreeContextMenu")
        '
        'ctxMenuNew
        '
        Me.ctxMenuNew.Name = "ctxMenuNew"
        resources.ApplyResources(Me.ctxMenuNew, "ctxMenuNew")
        '
        'ctxMenuSave
        '
        Me.ctxMenuSave.Name = "ctxMenuSave"
        resources.ApplyResources(Me.ctxMenuSave, "ctxMenuSave")
        '
        'ctxMenuDelete
        '
        Me.ctxMenuDelete.Name = "ctxMenuDelete"
        resources.ApplyResources(Me.ctxMenuDelete, "ctxMenuDelete")
        '
        'ctxMenuRetire
        '
        Me.ctxMenuRetire.Name = "ctxMenuRetire"
        resources.ApplyResources(Me.ctxMenuRetire, "ctxMenuRetire")
        '
        'ctxMenuFileEditSeparator
        '
        Me.ctxMenuFileEditSeparator.Name = "ctxMenuFileEditSeparator"
        resources.ApplyResources(Me.ctxMenuFileEditSeparator, "ctxMenuFileEditSeparator")
        '
        'ctxMenuClone
        '
        Me.ctxMenuClone.Name = "ctxMenuClone"
        resources.ApplyResources(Me.ctxMenuClone, "ctxMenuClone")
        '
        'ctxMenuCopy
        '
        Me.ctxMenuCopy.Name = "ctxMenuCopy"
        resources.ApplyResources(Me.ctxMenuCopy, "ctxMenuCopy")
        '
        'ctxMenuPaste
        '
        Me.ctxMenuPaste.Name = "ctxMenuPaste"
        resources.ApplyResources(Me.ctxMenuPaste, "ctxMenuPaste")
        '
        'ctxMenuEditRunSeparator
        '
        Me.ctxMenuEditRunSeparator.Name = "ctxMenuEditRunSeparator"
        resources.ApplyResources(Me.ctxMenuEditRunSeparator, "ctxMenuEditRunSeparator")
        '
        'ctxMenuRunNow
        '
        Me.ctxMenuRunNow.Name = "ctxMenuRunNow"
        resources.ApplyResources(Me.ctxMenuRunNow, "ctxMenuRunNow")
        '
        'ctxMenuStop
        '
        Me.ctxMenuStop.Name = "ctxMenuStop"
        resources.ApplyResources(Me.ctxMenuStop, "ctxMenuStop")
        '
        'mSplitter
        '
        Me.mSplitter.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        resources.ApplyResources(Me.mSplitter, "mSplitter")
        Me.mSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.mSplitter.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.mSplitter.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.mSplitter.GripVisible = False
        Me.mSplitter.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.mSplitter.MouseLeaveColor = System.Drawing.Color.White
        Me.mSplitter.Name = "mSplitter"
        '
        'mSplitter.Panel1
        '
        Me.mSplitter.Panel1.Controls.Add(Me.mControlRoomTree)
        Me.mSplitter.Panel1.Controls.Add(Me.pnlTitle)
        '
        'mSplitter.Panel2
        '
        Me.mSplitter.Panel2.Controls.Add(Me.mMenuButton)
        Me.mSplitter.Panel2.Controls.Add(Me.lblAreaTitle)
        Me.mSplitter.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.mSplitter.TabStop = False
        Me.mSplitter.TextColor = System.Drawing.Color.Black
        '
        'mControlRoomTree
        '
        Me.mControlRoomTree.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.mControlRoomTree.ContextMenuStrip = Me.mScheduleTreeContextMenu
        resources.ApplyResources(Me.mControlRoomTree, "mControlRoomTree")
        Me.mControlRoomTree.HideSelection = False
        Me.mControlRoomTree.LabelEdit = True
        Me.mControlRoomTree.Name = "mControlRoomTree"
        '
        'pnlTitle
        '
        Me.pnlTitle.Controls.Add(Me.lblTitle)
        resources.ApplyResources(Me.pnlTitle, "pnlTitle")
        Me.pnlTitle.Name = "pnlTitle"
        '
        'lblTitle
        '
        Me.lblTitle.BackColor = System.Drawing.Color.DimGray
        resources.ApplyResources(Me.lblTitle, "lblTitle")
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Name = "lblTitle"
        '
        'mMenuButton
        '
        resources.ApplyResources(Me.mMenuButton, "mMenuButton")
        Me.mMenuButton.ContextMenuStrip = Me.mMenuButtonContextMenuStrip
        Me.mMenuButton.FlatAppearance.BorderSize = 0
        Me.mMenuButton.Name = "mMenuButton"
        '
        'mMenuButtonContextMenuStrip
        '
        Me.mMenuButtonContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RefreshToolStripMenuItem, Me.ToggleConnectionsToolStripMenuItem})
        Me.mMenuButtonContextMenuStrip.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mMenuButtonContextMenuStrip, "mMenuButtonContextMenuStrip")
        '
        'RefreshToolStripMenuItem
        '
        Me.RefreshToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Refresh_16x16
        Me.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem"
        resources.ApplyResources(Me.RefreshToolStripMenuItem, "RefreshToolStripMenuItem")
        '
        'lblAreaTitle
        '
        Me.lblAreaTitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        resources.ApplyResources(Me.lblAreaTitle, "lblAreaTitle")
        Me.lblAreaTitle.ForeColor = System.Drawing.Color.White
        Me.lblAreaTitle.Name = "lblAreaTitle"
        '
        'mSessionTreeContextMenu
        '
        Me.mSessionTreeContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ctxMenuRenameSessionView, Me.ctxMenuDeleteSessionView})
        Me.mSessionTreeContextMenu.Name = "mScheduleTreeviewContextMenu"
        resources.ApplyResources(Me.mSessionTreeContextMenu, "mSessionTreeContextMenu")
        '
        'ctxMenuRenameSessionView
        '
        Me.ctxMenuRenameSessionView.Name = "ctxMenuRenameSessionView"
        resources.ApplyResources(Me.ctxMenuRenameSessionView, "ctxMenuRenameSessionView")
        '
        'ctxMenuDeleteSessionView
        '
        Me.ctxMenuDeleteSessionView.Name = "ctxMenuDeleteSessionView"
        resources.ApplyResources(Me.ctxMenuDeleteSessionView, "ctxMenuDeleteSessionView")
        '
        'mQueueTreeContextMenu
        '
        Me.mQueueTreeContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuExpandAll, Me.mnuCollapseAll})
        Me.mQueueTreeContextMenu.Name = "mQueueTreeContextMenu"
        resources.ApplyResources(Me.mQueueTreeContextMenu, "mQueueTreeContextMenu")
        '
        'mnuExpandAll
        '
        Me.mnuExpandAll.Image = Global.AutomateUI.My.Resources.ToolImages.Expand_All_16x16
        Me.mnuExpandAll.Name = "mnuExpandAll"
        resources.ApplyResources(Me.mnuExpandAll, "mnuExpandAll")
        '
        'mnuCollapseAll
        '
        Me.mnuCollapseAll.Image = Global.AutomateUI.My.Resources.ToolImages.Collapse_All_16x16
        Me.mnuCollapseAll.Name = "mnuCollapseAll"
        resources.ApplyResources(Me.mnuCollapseAll, "mnuCollapseAll")
        '
        'ToggleConnectionsToolStripMenuItem
        '
        Me.ToggleConnectionsToolStripMenuItem.Name = "ToggleConnectionsToolStripMenuItem"
        resources.ApplyResources(Me.ToggleConnectionsToolStripMenuItem, "ToggleConnectionsToolStripMenuItem")
        '
        'ctlControlRoom
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.mSplitter)
        Me.DoubleBuffered = True
        Me.Name = "ctlControlRoom"
        resources.ApplyResources(Me, "$this")
        Me.mScheduleTreeContextMenu.ResumeLayout(False)
        Me.mSplitter.Panel1.ResumeLayout(False)
        Me.mSplitter.Panel2.ResumeLayout(False)
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplitter.ResumeLayout(False)
        Me.pnlTitle.ResumeLayout(False)
        Me.mMenuButtonContextMenuStrip.ResumeLayout(False)
        Me.mSessionTreeContextMenu.ResumeLayout(False)
        Me.mQueueTreeContextMenu.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents mSplitter As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents mControlRoomTree As AutomateControls.Trees.FlickerFreeTreeView
    Friend WithEvents ctxMenuNew As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuDelete As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuEditRunSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ctxMenuRunNow As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuSave As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblAreaTitle As System.Windows.Forms.Label
    Friend WithEvents ctxMenuClone As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuCopy As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuPaste As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuFileEditSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ctxMenuRetire As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents pnlTitle As System.Windows.Forms.Panel
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Private WithEvents mSessionTreeContextMenu As System.Windows.Forms.ContextMenuStrip
    Private WithEvents mScheduleTreeContextMenu As System.Windows.Forms.ContextMenuStrip
    Private WithEvents ctxMenuDeleteSessionView As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents ctxMenuRenameSessionView As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mMenuButton As AutomateControls.MenuButton
    Friend WithEvents mMenuButtonContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents RefreshToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mQueueTreeContextMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuExpandAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCollapseAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxMenuStop As ToolStripMenuItem
    Friend WithEvents ToggleConnectionsToolStripMenuItem As ToolStripMenuItem
End Class
