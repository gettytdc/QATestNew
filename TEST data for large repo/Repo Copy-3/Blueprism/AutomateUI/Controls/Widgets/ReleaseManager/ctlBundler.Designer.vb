<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlBundler
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
        Me.components = New System.ComponentModel.Container
        Dim TreeNode1 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("Business Processes")
        Dim TreeNode2 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("Business Processes")
        Dim TreeNode3 As System.Windows.Forms.TreeNode = New System.Windows.Forms.TreeNode("Business Processes")
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer
        Me.mInputTree = New Automate.ctlComponentTree_OLD
        Me.Label1 = New System.Windows.Forms.Label
        Me.mOutputTree = New Automate.ctlComponentTree_OLD
        Me.Label2 = New System.Windows.Forms.Label
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.mInputTree)
        Me.SplitContainer1.Panel1.Controls.Add(Me.Label1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mOutputTree)
        Me.SplitContainer1.Panel2.Controls.Add(Me.Label2)
        Me.SplitContainer1.Size = New System.Drawing.Size(604, 397)
        Me.SplitContainer1.SplitterDistance = 284
        Me.SplitContainer1.TabIndex = 0
        '
        'mInputTree
        '
        Me.mInputTree.AllowDrop = True
        Me.mInputTree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mInputTree.HideSelection = False
        Me.mInputTree.ImageIndex = 0
        Me.mInputTree.Location = New System.Drawing.Point(0, 21)
        Me.mInputTree.Name = "mInputTree"
        TreeNode1.ImageKey = "process"
        TreeNode1.Name = "Business Processes"
        TreeNode1.SelectedImageKey = "process"
        TreeNode1.Text = "Business Processes"
        Me.mInputTree.Nodes.AddRange(New System.Windows.Forms.TreeNode() {TreeNode1})
        Me.mInputTree.SelectedComponent = Nothing
        Me.mInputTree.SelectedImageIndex = 0
        Me.mInputTree.ShowComponents = CType((Automate.ctlComponentTree_OLD.ShowIf.BusinessProcess Or Automate.ctlComponentTree_OLD.ShowIf.BusinessObject), Automate.ctlComponentTree_OLD.ShowIf)
        Me.mInputTree.Size = New System.Drawing.Size(284, 376)
        Me.mInputTree.TabIndex = 2
        '
        'Label1
        '
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label1.Location = New System.Drawing.Point(0, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(284, 21)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Available Components"
        '
        'mOutputTree
        '
        Me.mOutputTree.AllowDrop = True
        Me.mOutputTree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mOutputTree.HideSelection = False
        Me.mOutputTree.ImageIndex = 0
        Me.mOutputTree.Location = New System.Drawing.Point(0, 21)
        Me.mOutputTree.Name = "mOutputTree"
        TreeNode2.ImageKey = "process"
        TreeNode2.Name = "Business Processes"
        TreeNode2.SelectedImageKey = "process"
        TreeNode2.Text = "Business Processes"
        TreeNode3.ImageKey = "process"
        TreeNode3.Name = "Business Processes"
        TreeNode3.SelectedImageKey = "process"
        TreeNode3.Text = "Business Processes"
        Me.mOutputTree.Nodes.AddRange(New System.Windows.Forms.TreeNode() {TreeNode2, TreeNode3})
        Me.mOutputTree.SelectedComponent = Nothing
        Me.mOutputTree.SelectedImageIndex = 0
        Me.mOutputTree.ShowComponents = CType((Automate.ctlComponentTree_OLD.ShowIf.BusinessProcess Or Automate.ctlComponentTree_OLD.ShowIf.BusinessObject), Automate.ctlComponentTree_OLD.ShowIf)
        Me.mOutputTree.Size = New System.Drawing.Size(316, 376)
        Me.mOutputTree.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label2.Location = New System.Drawing.Point(0, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(316, 21)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Package Contents"
        '
        'ctlBundler
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "ctlBundler"
        Me.Size = New System.Drawing.Size(604, 397)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Private WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents Label2 As System.Windows.Forms.Label
    Private WithEvents mInputTree As Automate.ctlComponentTree_OLD
    Private WithEvents mOutputTree As Automate.ctlComponentTree_OLD

End Class
