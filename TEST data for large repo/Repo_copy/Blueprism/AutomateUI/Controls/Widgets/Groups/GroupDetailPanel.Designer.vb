<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class GroupDetailPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(GroupDetailPanel))
        Me.lblHeader = New System.Windows.Forms.Label()
        Me.gvContents = New AutomateUI.ProcessGroupContentsDataGridView()
        Me.mMenuButton = New AutomateControls.MenuButton()
        Me.mMenuButtonContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.CreateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RefreshToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.gvContents, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mMenuButtonContextMenuStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblHeader
        '
        resources.ApplyResources(Me.lblHeader, "lblHeader")
        Me.lblHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.lblHeader.ForeColor = System.Drawing.Color.White
        Me.lblHeader.Name = "lblHeader"
        '
        'gvContents
        '
        resources.ApplyResources(Me.gvContents, "gvContents")
        Me.gvContents.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.gvContents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gvContents.GridColor = System.Drawing.SystemColors.ControlLight
        Me.gvContents.Name = "gvContents"
        Me.gvContents.ReadOnly = True
        Me.gvContents.StandardTab = True
        '
        'mMenuButton
        '
        resources.ApplyResources(Me.mMenuButton, "mMenuButton")
        Me.mMenuButton.ContextMenuStrip = Me.mMenuButtonContextMenuStrip
        Me.mMenuButton.Name = "mMenuButton"
        '
        'mMenuButtonContextMenuStrip
        '
        Me.mMenuButtonContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CreateToolStripMenuItem, Me.RefreshToolStripMenuItem})
        Me.mMenuButtonContextMenuStrip.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mMenuButtonContextMenuStrip, "mMenuButtonContextMenuStrip")
        '
        'CreateToolStripMenuItem
        '
        Me.CreateToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Document_New_16x16
        Me.CreateToolStripMenuItem.Name = "CreateToolStripMenuItem"
        resources.ApplyResources(Me.CreateToolStripMenuItem, "CreateToolStripMenuItem")
        '
        'RefreshToolStripMenuItem
        '
        Me.RefreshToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Refresh_16x16
        Me.RefreshToolStripMenuItem.Name = "RefreshToolStripMenuItem"
        resources.ApplyResources(Me.RefreshToolStripMenuItem, "RefreshToolStripMenuItem")
        '
        'GroupDetailPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mMenuButton)
        Me.Controls.Add(Me.gvContents)
        Me.Controls.Add(Me.lblHeader)
        Me.Name = "GroupDetailPanel"
        CType(Me.gvContents, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mMenuButtonContextMenuStrip.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents lblHeader As System.Windows.Forms.Label
    Private WithEvents gvContents As AutomateUI.ProcessGroupContentsDataGridView
    Friend WithEvents mMenuButton As AutomateControls.MenuButton
    Friend WithEvents mMenuButtonContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents CreateToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RefreshToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
