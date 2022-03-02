<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WebApiManager
    Inherits System.Windows.Forms.UserControl


    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiManager))
        Me.splitMain = New AutomateControls.GrippableSplitContainer()
        Me.panButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnAddAction = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDeleteAction = New AutomateControls.Buttons.StandardStyledButton()
        Me.tvServices = New AutomateControls.Trees.TreeViewAndFilter()
        Me.ctxActions = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.menuAddAction = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuDeleteAction = New System.Windows.Forms.ToolStripMenuItem()
        Me.splitDetail = New AutomateControls.FlickerFreeSplitContainer()
        Me.lblGuidance = New System.Windows.Forms.Label()
        CType(Me.splitMain,System.ComponentModel.ISupportInitialize).BeginInit
        Me.splitMain.Panel1.SuspendLayout
        Me.splitMain.Panel2.SuspendLayout
        Me.splitMain.SuspendLayout
        Me.panButtons.SuspendLayout
        Me.ctxActions.SuspendLayout
        CType(Me.splitDetail,System.ComponentModel.ISupportInitialize).BeginInit
        Me.splitDetail.Panel1.SuspendLayout
        Me.splitDetail.SuspendLayout
        Me.SuspendLayout
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        resources.ApplyResources(Me.splitMain.Panel1, "splitMain.Panel1")
        Me.splitMain.Panel1.Controls.Add(Me.panButtons)
        Me.splitMain.Panel1.Controls.Add(Me.tvServices)
        '
        'splitMain.Panel2
        '
        resources.ApplyResources(Me.splitMain.Panel2, "splitMain.Panel2")
        Me.splitMain.Panel2.Controls.Add(Me.splitDetail)
        Me.splitMain.TabStop = false
        '
        'panButtons
        '
        resources.ApplyResources(Me.panButtons, "panButtons")
        Me.panButtons.Controls.Add(Me.btnAddAction)
        Me.panButtons.Controls.Add(Me.btnDeleteAction)
        Me.panButtons.Name = "panButtons"
        '
        'btnAddAction
        '
        resources.ApplyResources(Me.btnAddAction, "btnAddAction")
        Me.btnAddAction.Name = "btnAddAction"
        '
        'btnDeleteAction
        '
        resources.ApplyResources(Me.btnDeleteAction, "btnDeleteAction")
        Me.btnDeleteAction.Name = "btnDeleteAction"
        '
        'tvServices
        '
        resources.ApplyResources(Me.tvServices, "tvServices")
        Me.tvServices.ContextMenuStrip = Me.ctxActions
        Me.tvServices.Name = "tvServices"
        '
        'ctxActions
        '
        resources.ApplyResources(Me.ctxActions, "ctxActions")
        Me.ctxActions.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.ctxActions.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuAddAction, Me.menuDeleteAction})
        Me.ctxActions.Name = "ctxActions"
        '
        'menuAddAction
        '
        resources.ApplyResources(Me.menuAddAction, "menuAddAction")
        Me.menuAddAction.Name = "menuAddAction"
        '
        'menuDeleteAction
        '
        resources.ApplyResources(Me.menuDeleteAction, "menuDeleteAction")
        Me.menuDeleteAction.Name = "menuDeleteAction"
        '
        'splitDetail
        '
        resources.ApplyResources(Me.splitDetail, "splitDetail")
        Me.splitDetail.Name = "splitDetail"
        '
        'splitDetail.Panel1
        '
        resources.ApplyResources(Me.splitDetail.Panel1, "splitDetail.Panel1")
        Me.splitDetail.Panel1.Controls.Add(Me.lblGuidance)
        '
        'splitDetail.Panel2
        '
        resources.ApplyResources(Me.splitDetail.Panel2, "splitDetail.Panel2")
        '
        'lblGuidance
        '
        resources.ApplyResources(Me.lblGuidance, "lblGuidance")
        Me.lblGuidance.Name = "lblGuidance"
        '
        'WebApiManager
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitMain)
        Me.Name = "WebApiManager"
        Me.splitMain.Panel1.ResumeLayout(false)
        Me.splitMain.Panel2.ResumeLayout(false)
        CType(Me.splitMain,System.ComponentModel.ISupportInitialize).EndInit
        Me.splitMain.ResumeLayout(false)
        Me.panButtons.ResumeLayout(false)
        Me.ctxActions.ResumeLayout(false)
        Me.splitDetail.Panel1.ResumeLayout(false)
        Me.splitDetail.Panel1.PerformLayout
        CType(Me.splitDetail,System.ComponentModel.ISupportInitialize).EndInit
        Me.splitDetail.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents splitMain As AutomateControls.GrippableSplitContainer
    Private WithEvents tvServices As AutomateControls.Trees.TreeViewAndFilter
    Private WithEvents ctxActions As ContextMenuStrip
    Friend WithEvents menuAddAction As ToolStripMenuItem
    Friend WithEvents menuDeleteAction As ToolStripMenuItem
    Private WithEvents panButtons As FlowLayoutPanel
    Private WithEvents btnAddAction As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnDeleteAction As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents lblGuidance As Label
    Friend WithEvents splitDetail As AutomateControls.FlickerFreeSplitContainer
End Class
