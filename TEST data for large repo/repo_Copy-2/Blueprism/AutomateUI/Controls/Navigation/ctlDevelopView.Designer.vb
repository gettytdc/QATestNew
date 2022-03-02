<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDevelopView
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDevelopView))
        Me.splitPanel = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.gtGroups = New AutomateUI.GroupTreeControl()
        Me.pnlTitle = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        CType(Me.splitPanel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitPanel.Panel1.SuspendLayout()
        Me.splitPanel.SuspendLayout()
        Me.pnlTitle.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitPanel
        '
        Me.splitPanel.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.splitPanel, "splitPanel")
        Me.splitPanel.GripVisible = False
        Me.splitPanel.Name = "splitPanel"
        '
        'splitPanel.Panel1
        '
        Me.splitPanel.Panel1.Controls.Add(Me.gtGroups)
        Me.splitPanel.Panel1.Controls.Add(Me.pnlTitle)
        Me.splitPanel.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.splitPanel.TabStop = False
        '
        'gtGroups
        '
        resources.ApplyResources(Me.gtGroups, "gtGroups")
        Me.gtGroups.ItemCreateEnabled = True
        Me.gtGroups.ItemDeleteEnabled = True
        Me.gtGroups.SaveExpandedGroups = True
        Me.gtGroups.Name = "gtGroups"
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
        'ctlDevelopView
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitPanel)
        Me.Name = "ctlDevelopView"
        Me.splitPanel.Panel1.ResumeLayout(False)
        CType(Me.splitPanel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitPanel.ResumeLayout(False)
        Me.pnlTitle.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents splitPanel As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents pnlTitle As System.Windows.Forms.Panel
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Private WithEvents gtGroups As AutomateUI.GroupTreeControl

End Class
