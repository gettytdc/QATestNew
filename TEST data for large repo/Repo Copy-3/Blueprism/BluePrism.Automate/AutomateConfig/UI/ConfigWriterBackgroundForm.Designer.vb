<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ConfigWriterBackgroundForm
    Inherits System.Windows.Forms.Form

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim Panel1 As System.Windows.Forms.Panel
        Dim Label1 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ConfigWriterBackgroundForm))
        Me.iconTray = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ctxMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.menuExit = New System.Windows.Forms.ToolStripMenuItem()
        Panel1 = New System.Windows.Forms.Panel()
        Label1 = New System.Windows.Forms.Label()
        Panel1.SuspendLayout()
        Me.ctxMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Panel1.Controls.Add(Label1)
        resources.ApplyResources(Panel1, "Panel1")
        Panel1.Name = "Panel1"
        '
        'Label1
        '
        resources.ApplyResources(Label1, "Label1")
        Label1.Name = "Label1"
        '
        'iconTray
        '
        resources.ApplyResources(Me.iconTray, "iconTray")
        Me.iconTray.ContextMenuStrip = Me.ctxMenu
        '
        'ctxMenu
        '
        Me.ctxMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuExit})
        Me.ctxMenu.Name = "ctxMenu"
        resources.ApplyResources(Me.ctxMenu, "ctxMenu")
        '
        'menuExit
        '
        Me.menuExit.Name = "menuExit"
        resources.ApplyResources(Me.menuExit, "menuExit")
        '
        'ConfigWriterBackgroundForm
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "ConfigWriterBackgroundForm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        Me.ctxMenu.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents iconTray As System.Windows.Forms.NotifyIcon
    Private WithEvents ctxMenu As System.Windows.Forms.ContextMenuStrip
    Private WithEvents menuExit As System.Windows.Forms.ToolStripMenuItem
End Class
