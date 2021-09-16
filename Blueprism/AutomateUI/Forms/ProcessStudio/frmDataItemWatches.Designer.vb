<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDataItemWatches
    Inherits frmForm

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDataItemWatches))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.lblAllItems = New System.Windows.Forms.Label()
        Me.ctlDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.lblWatchedItems = New System.Windows.Forms.Label()
        Me.ctlDataItemWatches = New AutomateUI.ctlDataWatchTree()
        Me.btnClose = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.objBluebar = New AutomateControls.TitleBar()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.lblAllItems)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ctlDataItemTreeView)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.lblWatchedItems)
        Me.SplitContainer1.Panel2.Controls.Add(Me.ctlDataItemWatches)
        '
        'lblAllItems
        '
        resources.ApplyResources(Me.lblAllItems, "lblAllItems")
        Me.lblAllItems.BackColor = System.Drawing.SystemColors.Control
        Me.lblAllItems.Name = "lblAllItems"
        '
        'ctlDataItemTreeView
        '
        resources.ApplyResources(Me.ctlDataItemTreeView, "ctlDataItemTreeView")
        Me.ctlDataItemTreeView.BackColor = System.Drawing.SystemColors.Control
        Me.ctlDataItemTreeView.CheckBoxes = False
        Me.ctlDataItemTreeView.IgnoreScope = True
        Me.ctlDataItemTreeView.Name = "ctlDataItemTreeView"
        Me.ctlDataItemTreeView.Stage = Nothing
        Me.ctlDataItemTreeView.StatisticsMode = False
        '
        'lblWatchedItems
        '
        resources.ApplyResources(Me.lblWatchedItems, "lblWatchedItems")
        Me.lblWatchedItems.BackColor = System.Drawing.SystemColors.Control
        Me.lblWatchedItems.Name = "lblWatchedItems"
        '
        'ctlDataItemWatches
        '
        Me.ctlDataItemWatches.AllowDrop = True
        resources.ApplyResources(Me.ctlDataItemWatches, "ctlDataItemWatches")
        Me.ctlDataItemWatches.BackColor = System.Drawing.SystemColors.Control
        Me.ctlDataItemWatches.Name = "ctlDataItemWatches"
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.BackColor = System.Drawing.SystemColors.Control
        Me.btnClose.Name = "btnClose"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.BackColor = System.Drawing.SystemColors.Control
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.UseVisualStyleBackColor = False
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        Me.objBluebar.Name = "objBluebar"
        '
        'frmDataItemWatches
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.objBluebar)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmDataItemWatches"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Protected WithEvents objBluebar As AutomateControls.TitleBar
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents lblWatchedItems As System.Windows.Forms.Label
    Friend WithEvents ctlDataItemWatches As AutomateUI.ctlDataWatchTree
    Friend WithEvents lblAllItems As System.Windows.Forms.Label
    Friend WithEvents btnClose As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
End Class
