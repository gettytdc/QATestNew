<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDashboard
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                components?.Dispose()
                tileView?.Dispose()
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDashboard))
        Me.mDashboardMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuNewPersonalDashboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuNewGlobalDashboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuNewPublishedDashboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepEditDashboard = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuEditDashboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSetHomePage = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepDeleteDashboard = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuCopyAsPersonal = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCopyAsGlobal = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCopyAsPublished = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDeleteDashboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepExpand = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuExpandAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCollapseAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.ilTreeImages = New System.Windows.Forms.ImageList(Me.components)
        Me.tRefresh = New System.Windows.Forms.Timer(Me.components)
        Me.mSplitter = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.lblHeader = New System.Windows.Forms.Label()
        Me.mTreeTab = New System.Windows.Forms.TabControl()
        Me.mViewTab = New System.Windows.Forms.TabPage()
        Me.mViewTree = New System.Windows.Forms.TreeView()
        Me.mTileTab = New System.Windows.Forms.TabPage()
        Me.TilesGroupTreeControl = New AutomateUI.GroupTreeControl()
        Me.txtDashboardTitle = New AutomateControls.Textboxes.StyledTextBox()
        Me.ElementHost1 = New System.Windows.Forms.Integration.ElementHost()
        Me.tileView = New AutomateUI.ctlTileView()
        Me.pnlArea = New System.Windows.Forms.Panel()
        Me.mMenuButton = New AutomateControls.MenuButton()
        Me.mMenuButtonContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.EditToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SaveToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RefreshToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CloseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.lblAreaTitle = New System.Windows.Forms.Label()
        Me.mDashboardMenu.SuspendLayout()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitter.Panel1.SuspendLayout()
        Me.mSplitter.Panel2.SuspendLayout()
        Me.mSplitter.SuspendLayout()
        Me.mTreeTab.SuspendLayout()
        Me.mViewTab.SuspendLayout()
        Me.mTileTab.SuspendLayout()
        Me.pnlArea.SuspendLayout()
        Me.mMenuButtonContextMenuStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'mDashboardMenu
        '
        Me.mDashboardMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuNewPersonalDashboard, Me.mnuNewGlobalDashboard, Me.mnuNewPublishedDashboard, Me.sepEditDashboard, Me.mnuEditDashboard, Me.mnuSetHomePage, Me.sepDeleteDashboard, Me.mnuCopyAsPersonal, Me.mnuCopyAsGlobal, Me.mnuCopyAsPublished, Me.mnuDeleteDashboard, Me.sepExpand, Me.mnuExpandAll, Me.mnuCollapseAll})
        Me.mDashboardMenu.Name = "mViewMenu"
        resources.ApplyResources(Me.mDashboardMenu, "mDashboardMenu")
        '
        'mnuNewPersonalDashboard
        '
        Me.mnuNewPersonalDashboard.Name = "mnuNewPersonalDashboard"
        resources.ApplyResources(Me.mnuNewPersonalDashboard, "mnuNewPersonalDashboard")
        '
        'mnuNewGlobalDashboard
        '
        Me.mnuNewGlobalDashboard.Name = "mnuNewGlobalDashboard"
        resources.ApplyResources(Me.mnuNewGlobalDashboard, "mnuNewGlobalDashboard")
        '
        'mnuNewPublishedDashboard
        '
        Me.mnuNewPublishedDashboard.Name = "mnuNewPublishedDashboard"
        resources.ApplyResources(Me.mnuNewPublishedDashboard, "mnuNewPublishedDashboard")
        '
        'sepEditDashboard
        '
        Me.sepEditDashboard.Name = "sepEditDashboard"
        resources.ApplyResources(Me.sepEditDashboard, "sepEditDashboard")
        '
        'mnuEditDashboard
        '
        Me.mnuEditDashboard.Name = "mnuEditDashboard"
        resources.ApplyResources(Me.mnuEditDashboard, "mnuEditDashboard")
        '
        'mnuSetHomePage
        '
        Me.mnuSetHomePage.Name = "mnuSetHomePage"
        resources.ApplyResources(Me.mnuSetHomePage, "mnuSetHomePage")
        '
        'sepDeleteDashboard
        '
        Me.sepDeleteDashboard.Name = "sepDeleteDashboard"
        resources.ApplyResources(Me.sepDeleteDashboard, "sepDeleteDashboard")
        '
        'mnuCopyAsPersonal
        '
        Me.mnuCopyAsPersonal.Name = "mnuCopyAsPersonal"
        resources.ApplyResources(Me.mnuCopyAsPersonal, "mnuCopyAsPersonal")
        '
        'mnuCopyAsGlobal
        '
        Me.mnuCopyAsGlobal.Name = "mnuCopyAsGlobal"
        resources.ApplyResources(Me.mnuCopyAsGlobal, "mnuCopyAsGlobal")
        '
        'mnuCopyAsPublished
        '
        Me.mnuCopyAsPublished.Name = "mnuCopyAsPublished"
        resources.ApplyResources(Me.mnuCopyAsPublished, "mnuCopyAsPublished")
        '
        'mnuDeleteDashboard
        '
        Me.mnuDeleteDashboard.Name = "mnuDeleteDashboard"
        resources.ApplyResources(Me.mnuDeleteDashboard, "mnuDeleteDashboard")
        '
        'sepExpand
        '
        Me.sepExpand.Name = "sepExpand"
        resources.ApplyResources(Me.sepExpand, "sepExpand")
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
        'ilTreeImages
        '
        Me.ilTreeImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.ilTreeImages, "ilTreeImages")
        Me.ilTreeImages.TransparentColor = System.Drawing.Color.Transparent
        '
        'tRefresh
        '
        Me.tRefresh.Interval = 60000
        '
        'mSplitter
        '
        resources.ApplyResources(Me.mSplitter, "mSplitter")
        Me.mSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.mSplitter.GripVisible = False
        Me.mSplitter.Name = "mSplitter"
        '
        'mSplitter.Panel1
        '
        Me.mSplitter.Panel1.Controls.Add(Me.lblHeader)
        Me.mSplitter.Panel1.Controls.Add(Me.mTreeTab)
        '
        'mSplitter.Panel2
        '
        Me.mSplitter.Panel2.Controls.Add(Me.txtDashboardTitle)
        Me.mSplitter.Panel2.Controls.Add(Me.ElementHost1)
        Me.mSplitter.Panel2.Controls.Add(Me.pnlArea)
        Me.mSplitter.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.mSplitter.TabStop = False
        '
        'lblHeader
        '
        Me.lblHeader.BackColor = System.Drawing.Color.DimGray
        resources.ApplyResources(Me.lblHeader, "lblHeader")
        Me.lblHeader.ForeColor = System.Drawing.Color.White
        Me.lblHeader.Name = "lblHeader"
        '
        'mTreeTab
        '
        resources.ApplyResources(Me.mTreeTab, "mTreeTab")
        Me.mTreeTab.Controls.Add(Me.mViewTab)
        Me.mTreeTab.Controls.Add(Me.mTileTab)
        Me.mTreeTab.Name = "mTreeTab"
        Me.mTreeTab.SelectedIndex = 0
        '
        'mViewTab
        '
        Me.mViewTab.Controls.Add(Me.mViewTree)
        resources.ApplyResources(Me.mViewTab, "mViewTab")
        Me.mViewTab.Name = "mViewTab"
        Me.mViewTab.UseVisualStyleBackColor = True
        '
        'mViewTree
        '
        Me.mViewTree.ContextMenuStrip = Me.mDashboardMenu
        resources.ApplyResources(Me.mViewTree, "mViewTree")
        Me.mViewTree.ImageList = Me.ilTreeImages
        Me.mViewTree.Name = "mViewTree"
        '
        'mTileTab
        '
        Me.mTileTab.Controls.Add(Me.TilesGroupTreeControl)
        resources.ApplyResources(Me.mTileTab, "mTileTab")
        Me.mTileTab.Name = "mTileTab"
        Me.mTileTab.UseVisualStyleBackColor = True
        '
        'gtcTiles
        '
        resources.ApplyResources(Me.TilesGroupTreeControl, "gtcTiles")
        Me.TilesGroupTreeControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.TilesGroupTreeControl.ItemCloneEnabled = True
        Me.TilesGroupTreeControl.ItemCreateEnabled = True
        Me.TilesGroupTreeControl.ItemDeleteEnabled = True
        Me.TilesGroupTreeControl.ItemEditEnabled = True
        Me.TilesGroupTreeControl.Name = "gtcTiles"
        '
        'txtDashboardTitle
        '
        resources.ApplyResources(Me.txtDashboardTitle, "txtDashboardTitle")
        Me.txtDashboardTitle.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtDashboardTitle.Name = "txtDashboardTitle"
        '
        'ElementHost1
        '
        resources.ApplyResources(Me.ElementHost1, "ElementHost1")
        Me.ElementHost1.Name = "ElementHost1"
        Me.ElementHost1.Child = Me.tileView
        '
        'pnlArea
        '
        Me.pnlArea.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.pnlArea.Controls.Add(Me.mMenuButton)
        Me.pnlArea.Controls.Add(Me.lblAreaTitle)
        resources.ApplyResources(Me.pnlArea, "pnlArea")
        Me.pnlArea.ForeColor = System.Drawing.Color.White
        Me.pnlArea.Name = "pnlArea"
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
        Me.mMenuButtonContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.EditToolStripMenuItem, Me.SaveToolStripMenuItem, Me.RefreshToolStripMenuItem, Me.CloseToolStripMenuItem})
        Me.mMenuButtonContextMenuStrip.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mMenuButtonContextMenuStrip, "mMenuButtonContextMenuStrip")
        '
        'EditToolStripMenuItem
        '
        Me.EditToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Document_Edit_16x16
        Me.EditToolStripMenuItem.Name = "EditToolStripMenuItem"
        resources.ApplyResources(Me.EditToolStripMenuItem, "EditToolStripMenuItem")
        '
        'SaveToolStripMenuItem
        '
        Me.SaveToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Save_16x16
        Me.SaveToolStripMenuItem.Name = "SaveToolStripMenuItem"
        resources.ApplyResources(Me.SaveToolStripMenuItem, "SaveToolStripMenuItem")
        '
        'RefreshToolStripMenuItem
        '
        Me.RefreshToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Refresh_16x16
        Me.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem"
        resources.ApplyResources(Me.RefreshToolStripMenuItem, "RefreshToolStripMenuItem")
        '
        'CloseToolStripMenuItem
        '
        Me.CloseToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.CloseToolStripMenuItem.Name = "CloseToolStripMenuItem"
        resources.ApplyResources(Me.CloseToolStripMenuItem, "CloseToolStripMenuItem")
        '
        'lblAreaTitle
        '
        resources.ApplyResources(Me.lblAreaTitle, "lblAreaTitle")
        Me.lblAreaTitle.Name = "lblAreaTitle"
        '
        'ctlDashboard
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.mSplitter)
        Me.DoubleBuffered = True
        Me.Name = "ctlDashboard"
        resources.ApplyResources(Me, "$this")
        Me.mDashboardMenu.ResumeLayout(False)
        Me.mSplitter.Panel1.ResumeLayout(False)
        Me.mSplitter.Panel2.ResumeLayout(False)
        Me.mSplitter.Panel2.PerformLayout()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplitter.ResumeLayout(False)
        Me.mTreeTab.ResumeLayout(False)
        Me.mViewTab.ResumeLayout(False)
        Me.mTileTab.ResumeLayout(False)
        Me.pnlArea.ResumeLayout(False)
        Me.pnlArea.PerformLayout()
        Me.mMenuButtonContextMenuStrip.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents mSplitter As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents pnlArea As System.Windows.Forms.Panel
    Friend WithEvents mTreeTab As System.Windows.Forms.TabControl
    Friend WithEvents mViewTab As System.Windows.Forms.TabPage
    Friend WithEvents mViewTree As System.Windows.Forms.TreeView
    Friend WithEvents mTileTab As System.Windows.Forms.TabPage
    Friend WithEvents mDashboardMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ElementHost1 As System.Windows.Forms.Integration.ElementHost
    Friend WithEvents ilTreeImages As System.Windows.Forms.ImageList
    Friend WithEvents mnuNewPersonalDashboard As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents sepDeleteDashboard As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuDeleteDashboard As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblHeader As System.Windows.Forms.Label
    Friend WithEvents mnuEditDashboard As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCopyAsPersonal As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents sepEditDashboard As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuSetHomePage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tRefresh As System.Windows.Forms.Timer
    Friend WithEvents txtDashboardTitle As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblAreaTitle As System.Windows.Forms.Label
    Private WithEvents TilesGroupTreeControl As AutomateUI.GroupTreeControl
    Friend tileView As AutomateUI.ctlTileView
    Friend WithEvents mnuNewGlobalDashboard As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuNewPublishedDashboard As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents sepExpand As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuExpandAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCollapseAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCopyAsGlobal As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCopyAsPublished As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mMenuButton As AutomateControls.MenuButton
    Friend WithEvents mMenuButtonContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents SaveToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CloseToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RefreshToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EditToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
