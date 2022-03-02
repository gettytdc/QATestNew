<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlPackageOverview
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPackageOverview))
        Me.lblPackageOverview = New System.Windows.Forms.Label()
        Me.lvPackages = New AutomateControls.FlickerFreeListView()
        Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colCreated = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colCreatedBy = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colLastRelease = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.panHeader = New System.Windows.Forms.Panel()
        Me.mMenuButton = New AutomateControls.MenuButton()
        Me.mMenuButtonContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.CreateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ttButtons = New System.Windows.Forms.ToolTip(Me.components)
        Me.panHeader.SuspendLayout()
        Me.mMenuButtonContextMenuStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblPackageOverview
        '
        resources.ApplyResources(Me.lblPackageOverview, "lblPackageOverview")
        Me.lblPackageOverview.ForeColor = System.Drawing.Color.White
        Me.lblPackageOverview.Name = "lblPackageOverview"
        '
        'lvPackages
        '
        resources.ApplyResources(Me.lvPackages, "lvPackages")
        Me.lvPackages.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colCreated, Me.colCreatedBy, Me.colLastRelease})
        Me.lvPackages.FullRowSelect = True
        Me.lvPackages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.lvPackages.HideSelection = False
        Me.lvPackages.MultiSelect = False
        Me.lvPackages.Name = "lvPackages"
        Me.lvPackages.UseCompatibleStateImageBehavior = False
        Me.lvPackages.View = System.Windows.Forms.View.Details
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        '
        'colCreated
        '
        resources.ApplyResources(Me.colCreated, "colCreated")
        '
        'colCreatedBy
        '
        resources.ApplyResources(Me.colCreatedBy, "colCreatedBy")
        '
        'colLastRelease
        '
        resources.ApplyResources(Me.colLastRelease, "colLastRelease")
        '
        'panHeader
        '
        Me.panHeader.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.panHeader.Controls.Add(Me.mMenuButton)
        Me.panHeader.Controls.Add(Me.lblPackageOverview)
        resources.ApplyResources(Me.panHeader, "panHeader")
        Me.panHeader.Name = "panHeader"
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
        Me.mMenuButtonContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CreateToolStripMenuItem})
        Me.mMenuButtonContextMenuStrip.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.mMenuButtonContextMenuStrip, "mMenuButtonContextMenuStrip")
        '
        'CreateToolStripMenuItem
        '
        Me.CreateToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ComponentImages.Item_Add_16x16
        Me.CreateToolStripMenuItem.Name = "CreateToolStripMenuItem"
        resources.ApplyResources(Me.CreateToolStripMenuItem, "CreateToolStripMenuItem")
        '
        'ctlPackageOverview
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.lvPackages)
        Me.Controls.Add(Me.panHeader)
        Me.Name = "ctlPackageOverview"
        Me.panHeader.ResumeLayout(False)
        Me.panHeader.PerformLayout()
        Me.mMenuButtonContextMenuStrip.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCreated As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCreatedBy As System.Windows.Forms.ColumnHeader
    Friend WithEvents colLastRelease As System.Windows.Forms.ColumnHeader
    Private WithEvents lvPackages As AutomateControls.FlickerFreeListView
    Friend WithEvents lblPackageOverview As System.Windows.Forms.Label
    Private WithEvents panHeader As System.Windows.Forms.Panel
    Friend WithEvents ttButtons As System.Windows.Forms.ToolTip
    Friend WithEvents mMenuButton As AutomateControls.MenuButton
    Friend WithEvents mMenuButtonContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents CreateToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
