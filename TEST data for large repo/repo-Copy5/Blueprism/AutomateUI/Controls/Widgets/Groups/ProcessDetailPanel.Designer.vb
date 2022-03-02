<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ProcessDetailPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ProcessDetailPanel))
        Me.panTitle = New System.Windows.Forms.Panel()
        Me.lblSubtitle = New System.Windows.Forms.Label()
        Me.mMenuButton = New AutomateControls.MenuButton()
        Me.mMenuButtonContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FindReferencesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RefreshToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeleteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.pbIcon = New System.Windows.Forms.PictureBox()
        Me.lblProcessName = New System.Windows.Forms.Label()
        Me.ProcessDescriptionTextBox = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblHistory = New System.Windows.Forms.Label()
        Me.mHistoryList = New AutomateUI.ProcessHistoryListView()
        Me.panTitle.SuspendLayout
        Me.mMenuButtonContextMenuStrip.SuspendLayout
        CType(Me.pbIcon,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'panTitle
        '
        Me.panTitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(0,Byte),Integer), CType(CType(114,Byte),Integer), CType(CType(198,Byte),Integer))
        Me.panTitle.Controls.Add(Me.lblSubtitle)
        Me.panTitle.Controls.Add(Me.mMenuButton)
        resources.ApplyResources(Me.panTitle, "panTitle")
        Me.panTitle.Name = "panTitle"
        '
        'lblSubtitle
        '
        resources.ApplyResources(Me.lblSubtitle, "lblSubtitle")
        Me.lblSubtitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(0,Byte),Integer), CType(CType(114,Byte),Integer), CType(CType(198,Byte),Integer))
        Me.lblSubtitle.ForeColor = System.Drawing.Color.White
        Me.lblSubtitle.Name = "lblSubtitle"
        '
        'mMenuButton
        '
        resources.ApplyResources(Me.mMenuButton, "mMenuButton")
        Me.mMenuButton.ContextMenuStrip = Me.mMenuButtonContextMenuStrip
        Me.mMenuButton.Name = "mMenuButton"
        '
        'mMenuButtonContextMenuStrip
        '
        Me.mMenuButtonContextMenuStrip.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.mMenuButtonContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ViewToolStripMenuItem, Me.OpenToolStripMenuItem, Me.FindReferencesToolStripMenuItem, Me.RefreshToolStripMenuItem, Me.DeleteToolStripMenuItem})
        Me.mMenuButtonContextMenuStrip.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mMenuButtonContextMenuStrip, "mMenuButtonContextMenuStrip")
        '
        'ViewToolStripMenuItem
        '
        resources.ApplyResources(Me.ViewToolStripMenuItem, "ViewToolStripMenuItem")
        Me.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem"
        '
        'OpenToolStripMenuItem
        '
        Me.OpenToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Document_Edit_16x16
        Me.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem"
        resources.ApplyResources(Me.OpenToolStripMenuItem, "OpenToolStripMenuItem")
        '
        'FindReferencesToolStripMenuItem
        '
        Me.FindReferencesToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Site_Map2_16x16
        Me.FindReferencesToolStripMenuItem.Name = "FindReferencesToolStripMenuItem"
        resources.ApplyResources(Me.FindReferencesToolStripMenuItem, "FindReferencesToolStripMenuItem")
        '
        'RefreshToolStripMenuItem
        '
        Me.RefreshToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Refresh_16x16
        Me.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem"
        resources.ApplyResources(Me.RefreshToolStripMenuItem, "RefreshToolStripMenuItem")
        '
        'DeleteToolStripMenuItem
        '
        Me.DeleteToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem"
        resources.ApplyResources(Me.DeleteToolStripMenuItem, "DeleteToolStripMenuItem")
        '
        'pbIcon
        '
        resources.ApplyResources(Me.pbIcon, "pbIcon")
        Me.pbIcon.Name = "pbIcon"
        Me.pbIcon.TabStop = false
        '
        'lblProcessName
        '
        resources.ApplyResources(Me.lblProcessName, "lblProcessName")
        Me.lblProcessName.Name = "lblProcessName"
        '
        'ProcessDescriptionTextBox
        '
        resources.ApplyResources(Me.ProcessDescriptionTextBox, "ProcessDescriptionTextBox")
        Me.ProcessDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window
        Me.ProcessDescriptionTextBox.BorderColor = System.Drawing.Color.Empty
        Me.ProcessDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ProcessDescriptionTextBox.Name = "ProcessDescriptionTextBox"
        Me.ProcessDescriptionTextBox.ReadOnly = true
        '
        'lblHistory
        '
        resources.ApplyResources(Me.lblHistory, "lblHistory")
        Me.lblHistory.Name = "lblHistory"
        '
        'mHistoryList
        '
        resources.ApplyResources(Me.mHistoryList, "mHistoryList")
        Me.mHistoryList.CanViewDefinition = true
        Me.mHistoryList.FillColumn = 3
        Me.mHistoryList.Mode = Nothing
        Me.mHistoryList.Name = "mHistoryList"
        '
        'ProcessDetailPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.mHistoryList)
        Me.Controls.Add(Me.lblHistory)
        Me.Controls.Add(Me.ProcessDescriptionTextBox)
        Me.Controls.Add(Me.lblProcessName)
        Me.Controls.Add(Me.pbIcon)
        Me.Controls.Add(Me.panTitle)
        resources.ApplyResources(Me, "$this")
        Me.Name = "ProcessDetailPanel"
        Me.panTitle.ResumeLayout(false)
        Me.panTitle.PerformLayout
        Me.mMenuButtonContextMenuStrip.ResumeLayout(false)
        CType(Me.pbIcon,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents panTitle As System.Windows.Forms.Panel
    Private WithEvents lblSubtitle As System.Windows.Forms.Label
    Friend WithEvents pbIcon As System.Windows.Forms.PictureBox
    Friend WithEvents lblProcessName As System.Windows.Forms.Label
    Friend WithEvents ProcessDescriptionTextBox As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblHistory As System.Windows.Forms.Label
    Friend WithEvents mMenuButton As AutomateControls.MenuButton
    Friend WithEvents mMenuButtonContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents OpenToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RefreshToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeleteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FindReferencesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mHistoryList As AutomateUI.ProcessHistoryListView
    Friend WithEvents ViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
