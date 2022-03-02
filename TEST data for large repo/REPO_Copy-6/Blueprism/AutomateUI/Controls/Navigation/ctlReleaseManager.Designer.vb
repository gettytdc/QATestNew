<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlReleaseManager
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlReleaseManager))
        Me.splitMain = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.mPackageTree = New AutomateUI.ctlPackageTree()
        Me.mPackageManagerContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ctxNewPackage = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxModifyPackage = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxDeletePackage = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ctxNewRelease = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxImportRelease = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxVerifyRelease = New System.Windows.Forms.ToolStripMenuItem()
        Me.pnlTitle = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.mPackageManagerContextMenu.SuspendLayout()
        Me.pnlTitle.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitMain
        '
        Me.splitMain.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitMain.GripVisible = False
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.mPackageTree)
        Me.splitMain.Panel1.Controls.Add(Me.pnlTitle)
        Me.splitMain.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.splitMain.TabStop = False
        '
        'mPackageTree
        '
        Me.mPackageTree.ContextMenuStrip = Me.mPackageManagerContextMenu
        resources.ApplyResources(Me.mPackageTree, "mPackageTree")
        Me.mPackageTree.Name = "mPackageTree"
        '
        'mPackageManagerContextMenu
        '
        Me.mPackageManagerContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ctxNewPackage, Me.ctxModifyPackage, Me.ctxDeletePackage, Me.ToolStripMenuItem1, Me.ctxNewRelease, Me.ctxImportRelease, Me.ctxVerifyRelease})
        Me.mPackageManagerContextMenu.Name = "mPackageManagerContextMenu"
        resources.ApplyResources(Me.mPackageManagerContextMenu, "mPackageManagerContextMenu")
        '
        'ctxNewPackage
        '
        Me.ctxNewPackage.Name = "ctxNewPackage"
        resources.ApplyResources(Me.ctxNewPackage, "ctxNewPackage")
        '
        'ctxModifyPackage
        '
        Me.ctxModifyPackage.Name = "ctxModifyPackage"
        resources.ApplyResources(Me.ctxModifyPackage, "ctxModifyPackage")
        '
        'ctxDeletePackage
        '
        Me.ctxDeletePackage.Name = "ctxDeletePackage"
        resources.ApplyResources(Me.ctxDeletePackage, "ctxDeletePackage")
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'ctxNewRelease
        '
        Me.ctxNewRelease.Name = "ctxNewRelease"
        resources.ApplyResources(Me.ctxNewRelease, "ctxNewRelease")
        '
        'ctxImportRelease
        '
        Me.ctxImportRelease.Name = "ctxImportRelease"
        resources.ApplyResources(Me.ctxImportRelease, "ctxImportRelease")
        '
        'ctxVerifyRelease
        '
        Me.ctxVerifyRelease.Name = "ctxVerifyRelease"
        resources.ApplyResources(Me.ctxVerifyRelease, "ctxVerifyRelease")
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
        'ctlReleaseManager
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitMain)
        Me.Name = "ctlReleaseManager"
        Me.splitMain.Panel1.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.mPackageManagerContextMenu.ResumeLayout(False)
        Me.pnlTitle.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents splitMain As AutomateControls.SplitContainers.HighlightingSplitContainer
    Public WithEvents mPackageTree As AutomateUI.ctlPackageTree
    Friend WithEvents mPackageManagerContextMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ctxVerifyRelease As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxImportRelease As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxDeletePackage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxNewPackage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ctxModifyPackage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ctxNewRelease As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents pnlTitle As System.Windows.Forms.Panel

End Class
