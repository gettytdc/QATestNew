<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUIAutomationNavigator

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim GroupBox1 As System.Windows.Forms.GroupBox
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUIAutomationNavigator))
        Dim Label3 As System.Windows.Forms.Label
        Me.radControl = New AutomateControls.StyledRadioButton()
        Me.radContent = New AutomateControls.StyledRadioButton()
        Me.radRaw = New AutomateControls.StyledRadioButton()
        Me.filTreeElements = New AutomateControls.Trees.TreeViewAndFilter()
        Me.pgElement = New System.Windows.Forms.PropertyGrid()
        Me.btnRefresh = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.splitAppTree = New AutomateControls.GrippableSplitContainer()
        Me.btnCollapse = New AutomateControls.ArrowButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ttTip = New System.Windows.Forms.ToolTip(Me.components)
        GroupBox1 = New System.Windows.Forms.GroupBox()
        Label3 = New System.Windows.Forms.Label()
        GroupBox1.SuspendLayout
        CType(Me.splitAppTree,System.ComponentModel.ISupportInitialize).BeginInit
        Me.splitAppTree.Panel1.SuspendLayout
        Me.splitAppTree.Panel2.SuspendLayout
        Me.splitAppTree.SuspendLayout
        Me.SuspendLayout
        '
        'GroupBox1
        '
        resources.ApplyResources(GroupBox1, "GroupBox1")
        GroupBox1.Controls.Add(Me.radControl)
        GroupBox1.Controls.Add(Me.radContent)
        GroupBox1.Controls.Add(Me.radRaw)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.TabStop = false
        '
        'radControl
        '
        resources.ApplyResources(Me.radControl, "radControl")
        Me.radControl.Name = "radControl"
        Me.radControl.UseVisualStyleBackColor = true
        '
        'radContent
        '
        resources.ApplyResources(Me.radContent, "radContent")
        Me.radContent.Name = "radContent"
        Me.radContent.UseVisualStyleBackColor = true
        '
        'radRaw
        '
        resources.ApplyResources(Me.radRaw, "radRaw")
        Me.radRaw.Name = "radRaw"
        Me.radRaw.UseVisualStyleBackColor = true
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.Name = "Label3"
        '
        'filTreeElements
        '
        resources.ApplyResources(Me.filTreeElements, "filTreeElements")
        Me.filTreeElements.Name = "filTreeElements"
        '
        'pgElement
        '
        resources.ApplyResources(Me.pgElement, "pgElement")
        Me.pgElement.CommandsVisibleIfAvailable = false
        Me.pgElement.DisabledItemForeColor = System.Drawing.SystemColors.ControlText
        Me.pgElement.LineColor = System.Drawing.SystemColors.ControlDark
        Me.pgElement.Name = "pgElement"
        Me.pgElement.PropertySort = System.Windows.Forms.PropertySort.Alphabetical
        Me.pgElement.ToolbarVisible = false
        '
        'btnRefresh
        '
        resources.ApplyResources(Me.btnRefresh, "btnRefresh")
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.UseVisualStyleBackColor = true
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = true
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = true
        '
        'splitAppTree
        '
        resources.ApplyResources(Me.splitAppTree, "splitAppTree")
        Me.splitAppTree.Name = "splitAppTree"
        '
        'splitAppTree.Panel1
        '
        Me.splitAppTree.Panel1.Controls.Add(Me.btnCollapse)
        Me.splitAppTree.Panel1.Controls.Add(Me.Label1)
        Me.splitAppTree.Panel1.Controls.Add(Me.filTreeElements)
        Me.splitAppTree.Panel1.Controls.Add(GroupBox1)
        '
        'splitAppTree.Panel2
        '
        Me.splitAppTree.Panel2.Controls.Add(Me.Label2)
        Me.splitAppTree.Panel2.Controls.Add(Me.pgElement)
        Me.splitAppTree.TabStop = false
        '
        'btnCollapse
        '
        resources.ApplyResources(Me.btnCollapse, "btnCollapse")
        Me.btnCollapse.Name = "btnCollapse"
        Me.ttTip.SetToolTip(Me.btnCollapse, resources.GetString("btnCollapse.ToolTip"))
        Me.btnCollapse.UseVisualStyleBackColor = true
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'ttTip
        '
        Me.ttTip.AutoPopDelay = 5000
        Me.ttTip.InitialDelay = 50
        Me.ttTip.ReshowDelay = 100
        '
        'frmUIAutomationNavigator
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.splitAppTree)
        Me.Controls.Add(Label3)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnRefresh)
        Me.Name = "frmUIAutomationNavigator"
        GroupBox1.ResumeLayout(false)
        GroupBox1.PerformLayout
        Me.splitAppTree.Panel1.ResumeLayout(false)
        Me.splitAppTree.Panel1.PerformLayout
        Me.splitAppTree.Panel2.ResumeLayout(false)
        Me.splitAppTree.Panel2.PerformLayout
        CType(Me.splitAppTree,System.ComponentModel.ISupportInitialize).EndInit
        Me.splitAppTree.ResumeLayout(false)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents radControl As AutomateControls.StyledRadioButton
    Friend WithEvents radContent As AutomateControls.StyledRadioButton
    Friend WithEvents radRaw As AutomateControls.StyledRadioButton
    Friend WithEvents btnRefresh As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents pgElement As PropertyGrid
    Private WithEvents filTreeElements As AutomateControls.Trees.TreeViewAndFilter
    Private WithEvents Label1 As Label
    Private WithEvents btnCollapse As AutomateControls.ArrowButton
    Private WithEvents splitAppTree As AutomateControls.GrippableSplitContainer
    Private WithEvents ttTip As ToolTip
    Private WithEvents Label2 As Label
End Class
