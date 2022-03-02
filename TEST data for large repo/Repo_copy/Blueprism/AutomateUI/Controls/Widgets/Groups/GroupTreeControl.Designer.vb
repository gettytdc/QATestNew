<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class GroupTreeControl
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(GroupTreeControl))
        Me.sepItemGroup = New System.Windows.Forms.ToolStripSeparator()
        Me.tvGroups = New AutomateControls.Trees.FilterableTreeView()
        Me.ctxGroup = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.menuRenameItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuEditItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuCloneItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuDeleteItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuFindRefs = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepCreateRenameDelete = New System.Windows.Forms.ToolStripSeparator()
        Me.menuCreateGroup = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuRenameGroup = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuDeleteGroup = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuRemoveFromGroup = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepGroupAll = New System.Windows.Forms.ToolStripSeparator()
        Me.menuExpandAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuCollapseAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepPermissionsGroup = New System.Windows.Forms.ToolStripSeparator()
        Me.menuPermissions = New System.Windows.Forms.ToolStripMenuItem()
        Me.sepExtraItems = New System.Windows.Forms.ToolStripSeparator()
        Me.timerGroupHover = New System.Windows.Forms.Timer(Me.components)
        Me.dragScroller = New AutomateControls.DragScroller(Me.components)
        Me.txtFilter = New AutomateControls.FilterTextBox()
        Me.ctxGroup.SuspendLayout()
        Me.SuspendLayout()
        '
        'sepItemGroup
        '
        Me.sepItemGroup.Name = "sepItemGroup"
        resources.ApplyResources(Me.sepItemGroup, "sepItemGroup")
        '
        'tvGroups
        '
        Me.tvGroups.AllowDrop = True
        resources.ApplyResources(Me.tvGroups, "tvGroups")
        Me.tvGroups.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.tvGroups.ContextMenuStrip = Me.ctxGroup
        Me.tvGroups.HideSelection = False
        Me.tvGroups.HotTracking = True
        Me.tvGroups.LabelEdit = True
        Me.tvGroups.Name = "tvGroups"
        '
        'ctxGroup
        '
        Me.ctxGroup.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuRenameItem, Me.menuEditItem, Me.menuCloneItem, Me.menuDeleteItem, Me.sepItemGroup, Me.menuFindRefs, Me.sepCreateRenameDelete, Me.menuCreateGroup, Me.menuRenameGroup, Me.menuDeleteGroup, Me.menuRemoveFromGroup, Me.sepGroupAll, Me.menuExpandAll, Me.menuCollapseAll, Me.sepPermissionsGroup, Me.menuPermissions, Me.sepExtraItems})
        Me.ctxGroup.Name = "ctxTree"
        resources.ApplyResources(Me.ctxGroup, "ctxGroup")
        '
        'menuRenameItem
        '
        Me.menuRenameItem.Image = Global.AutomateUI.My.Resources.ToolImages.Rename_16x16
        Me.menuRenameItem.Name = "menuRenameItem"
        resources.ApplyResources(Me.menuRenameItem, "menuRenameItem")
        '
        'menuEditItem
        '
        Me.menuEditItem.Image = Global.AutomateUI.My.Resources.ToolImages.Document_Edit_16x16
        Me.menuEditItem.Name = "menuEditItem"
        resources.ApplyResources(Me.menuEditItem, "menuEditItem")
        '
        'menuCloneItem
        '
        Me.menuCloneItem.Image = Global.AutomateUI.My.Resources.ToolImages.Copy_16x16
        Me.menuCloneItem.Name = "menuCloneItem"
        resources.ApplyResources(Me.menuCloneItem, "menuCloneItem")
        '
        'menuDeleteItem
        '
        Me.menuDeleteItem.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.menuDeleteItem.Name = "menuDeleteItem"
        resources.ApplyResources(Me.menuDeleteItem, "menuDeleteItem")
        '
        'menuFindRefs
        '
        Me.menuFindRefs.Image = Global.AutomateUI.My.Resources.ToolImages.Find_Advanced_16x16
        Me.menuFindRefs.Name = "menuFindRefs"
        resources.ApplyResources(Me.menuFindRefs, "menuFindRefs")
        '
        'sepCreateRenameDelete
        '
        Me.sepCreateRenameDelete.Name = "sepCreateRenameDelete"
        resources.ApplyResources(Me.sepCreateRenameDelete, "sepCreateRenameDelete")
        '
        'menuCreateGroup
        '
        Me.menuCreateGroup.Image = Global.AutomateUI.My.Resources.ToolImages.Folder_New_16x16
        Me.menuCreateGroup.Name = "menuCreateGroup"
        resources.ApplyResources(Me.menuCreateGroup, "menuCreateGroup")
        '
        'menuRenameGroup
        '
        Me.menuRenameGroup.Image = Global.AutomateUI.My.Resources.ToolImages.Folder_Rename_16x16
        Me.menuRenameGroup.Name = "menuRenameGroup"
        resources.ApplyResources(Me.menuRenameGroup, "menuRenameGroup")
        '
        'menuDeleteGroup
        '
        Me.menuDeleteGroup.Image = Global.AutomateUI.My.Resources.ToolImages.Folder_Delete_16x16
        Me.menuDeleteGroup.Name = "menuDeleteGroup"
        resources.ApplyResources(Me.menuDeleteGroup, "menuDeleteGroup")
        '
        'menuRemoveFromGroup
        '
        Me.menuRemoveFromGroup.Image = Global.AutomateUI.My.Resources.ToolImages.Folder_Remove_16x16
        Me.menuRemoveFromGroup.Name = "menuRemoveFromGroup"
        resources.ApplyResources(Me.menuRemoveFromGroup, "menuRemoveFromGroup")
        '
        'sepGroupAll
        '
        Me.sepGroupAll.Name = "sepGroupAll"
        resources.ApplyResources(Me.sepGroupAll, "sepGroupAll")
        '
        'menuExpandAll
        '
        Me.menuExpandAll.Image = Global.AutomateUI.My.Resources.ToolImages.Expand_All_16x16
        Me.menuExpandAll.Name = "menuExpandAll"
        resources.ApplyResources(Me.menuExpandAll, "menuExpandAll")
        '
        'menuCollapseAll
        '
        Me.menuCollapseAll.Image = Global.AutomateUI.My.Resources.ToolImages.Collapse_All_16x16
        Me.menuCollapseAll.Name = "menuCollapseAll"
        resources.ApplyResources(Me.menuCollapseAll, "menuCollapseAll")
        '
        'sepPermissionsGroup
        '
        Me.sepPermissionsGroup.Name = "sepPermissionsGroup"
        resources.ApplyResources(Me.sepPermissionsGroup, "sepPermissionsGroup")
        '
        'menuPermissions
        '
        Me.menuPermissions.Image = Global.AutomateUI.My.Resources.ComponentImages.Folder_Lock_16x16
        Me.menuPermissions.Name = "menuPermissions"
        resources.ApplyResources(Me.menuPermissions, "menuPermissions")
        '
        'sepExtraItems
        '
        Me.sepExtraItems.Name = "sepExtraItems"
        resources.ApplyResources(Me.sepExtraItems, "sepExtraItems")
        '
        'timerGroupHover
        '
        Me.timerGroupHover.Interval = 1000
        '
        'dragScroller
        '
        Me.dragScroller.Control = Me.tvGroups
        '
        'txtFilter
        '
        Me.txtFilter.AcceptsTab = True
        Me.txtFilter.AlwaysShowHandOnFarHover = True
        Me.txtFilter.AlwaysShowHandOnNearHover = True
        resources.ApplyResources(Me.txtFilter, "txtFilter")
        Me.txtFilter.BorderColor = System.Drawing.Color.Empty
        Me.txtFilter.Name = "txtFilter"
        '
        'GroupTreeControl
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtFilter)
        Me.Controls.Add(Me.tvGroups)
        Me.Name = "GroupTreeControl"
        resources.ApplyResources(Me, "$this")
        Me.ctxGroup.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents tvGroups As AutomateControls.Trees.FilterableTreeView
    Private WithEvents ctxGroup As System.Windows.Forms.ContextMenuStrip
    Private WithEvents menuCreateGroup As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuRenameGroup As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuDeleteGroup As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuRemoveFromGroup As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuRenameItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents sepPermissionsGroup As System.Windows.Forms.ToolStripSeparator
    Private WithEvents sepGroupAll As System.Windows.Forms.ToolStripSeparator
    Private WithEvents menuExpandAll As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuCollapseAll As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuDeleteItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuFindRefs As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents sepCreateRenameDelete As System.Windows.Forms.ToolStripSeparator
    Private WithEvents menuCloneItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuPermissions As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents timerGroupHover As System.Windows.Forms.Timer
    Private WithEvents dragScroller As AutomateControls.DragScroller
    Private WithEvents sepItemGroup As ToolStripSeparator
    Friend WithEvents sepExtraItems As ToolStripSeparator
    Friend WithEvents menuEditItem As ToolStripMenuItem
    Private WithEvents txtFilter As AutomateControls.FilterTextBox
End Class
