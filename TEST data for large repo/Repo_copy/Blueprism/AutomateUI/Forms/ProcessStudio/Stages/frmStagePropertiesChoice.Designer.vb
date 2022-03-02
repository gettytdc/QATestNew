<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagePropertiesChoice
    Inherits frmProperties

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesChoice))
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.CtlListView1 = New AutomateUI.ctlListView()
        Me.btnAddCriterion = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnRemove = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnMoveDown = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnMoveUp = New AutomateControls.Buttons.StandardStyledButton()
        Me.objDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.CtlListView1)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnAddCriterion)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnRemove)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnMoveDown)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnMoveUp)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.objDataItemTreeView)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        '
        'CtlListView1
        '
        Me.CtlListView1.AllowDrop = True
        resources.ApplyResources(Me.CtlListView1, "CtlListView1")
        Me.CtlListView1.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.CtlListView1.CurrentEditableRow = Nothing
        Me.CtlListView1.FillColumn = Nothing
        Me.CtlListView1.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.CtlListView1.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.CtlListView1.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.CtlListView1.HighlightedRowOutline = System.Drawing.Color.Red
        Me.CtlListView1.LastColumnAutoSize = True
        Me.CtlListView1.MinimumColumnWidth = 0
        Me.CtlListView1.Name = "CtlListView1"
        Me.CtlListView1.Readonly = False
        Me.CtlListView1.RowHeight = 26
        Me.CtlListView1.Rows.Capacity = 0
        Me.CtlListView1.Sortable = False
        '
        'btnAddCriterion
        '
        resources.ApplyResources(Me.btnAddCriterion, "btnAddCriterion")
        Me.btnAddCriterion.Name = "btnAddCriterion"
        Me.btnAddCriterion.UseVisualStyleBackColor = True
        '
        'btnRemove
        '
        resources.ApplyResources(Me.btnRemove, "btnRemove")
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.UseVisualStyleBackColor = True
        '
        'btnMoveDown
        '
        resources.ApplyResources(Me.btnMoveDown, "btnMoveDown")
        Me.btnMoveDown.Name = "btnMoveDown"
        Me.btnMoveDown.UseVisualStyleBackColor = True
        '
        'btnMoveUp
        '
        resources.ApplyResources(Me.btnMoveUp, "btnMoveUp")
        Me.btnMoveUp.Name = "btnMoveUp"
        Me.btnMoveUp.UseVisualStyleBackColor = True
        '
        'objDataItemTreeView
        '
        Me.objDataItemTreeView.CheckBoxes = False
        resources.ApplyResources(Me.objDataItemTreeView, "objDataItemTreeView")
        Me.objDataItemTreeView.IgnoreScope = False
        Me.objDataItemTreeView.Name = "objDataItemTreeView"
        Me.objDataItemTreeView.Stage = Nothing
        Me.objDataItemTreeView.StatisticsMode = False
        '
        'frmStagePropertiesChoice
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesChoice"
        Me.Controls.SetChildIndex(Me.SplitContainer1, 0)
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents objDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents CtlListView1 As AutomateUI.ctlListView
    Friend WithEvents btnMoveDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnMoveUp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRemove As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddCriterion As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
End Class
