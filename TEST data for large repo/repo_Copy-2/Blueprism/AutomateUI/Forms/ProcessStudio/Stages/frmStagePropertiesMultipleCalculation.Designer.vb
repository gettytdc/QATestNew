<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagePropertiesMultipleCalculation
    Inherits AutomateUI.frmProperties

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesMultipleCalculation))
        Me.mDataItemTreeView = New AutomateUI.ctlDataItemTreeView()
        Me.btnMoveDown = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnMoveUp = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnRemove = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnAddCriterion = New AutomateControls.Buttons.StandardStyledButton()
        Me.mCalculationListView = New AutomateUI.ctlListView()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
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
        'mDataItemTreeView
        '
        Me.mDataItemTreeView.CheckBoxes = False
        resources.ApplyResources(Me.mDataItemTreeView, "mDataItemTreeView")
        Me.mDataItemTreeView.IgnoreScope = False
        Me.mDataItemTreeView.Name = "mDataItemTreeView"
        Me.mDataItemTreeView.Stage = Nothing
        Me.mDataItemTreeView.StatisticsMode = False
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
        'btnRemove
        '
        resources.ApplyResources(Me.btnRemove, "btnRemove")
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.UseVisualStyleBackColor = True
        '
        'btnAddCriterion
        '
        resources.ApplyResources(Me.btnAddCriterion, "btnAddCriterion")
        Me.btnAddCriterion.Name = "btnAddCriterion"
        Me.btnAddCriterion.UseVisualStyleBackColor = True
        '
        'mCalculationListView
        '
        Me.mCalculationListView.AllowDrop = True
        resources.ApplyResources(Me.mCalculationListView, "mCalculationListView")
        Me.mCalculationListView.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.mCalculationListView.CurrentEditableRow = Nothing
        Me.mCalculationListView.FillColumn = Nothing
        Me.mCalculationListView.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.mCalculationListView.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.mCalculationListView.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.mCalculationListView.HighlightedRowOutline = System.Drawing.Color.Red
        Me.mCalculationListView.LastColumnAutoSize = True
        Me.mCalculationListView.MinimumColumnWidth = 0
        Me.mCalculationListView.Name = "mCalculationListView"
        Me.mCalculationListView.Readonly = False
        Me.mCalculationListView.RowHeight = 26
        Me.mCalculationListView.Rows.Capacity = 0
        Me.mCalculationListView.Sortable = False
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnAddCriterion)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnRemove)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnMoveDown)
        Me.SplitContainer1.Panel1.Controls.Add(Me.btnMoveUp)
        Me.SplitContainer1.Panel1.Controls.Add(Me.mCalculationListView)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.mDataItemTreeView)
        '
        'frmStagePropertiesMultipleCalculation
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Name = "frmStagePropertiesMultipleCalculation"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.SplitContainer1, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents mDataItemTreeView As AutomateUI.ctlDataItemTreeView
    Friend WithEvents mCalculationListView As AutomateUI.ctlListView
    Friend WithEvents btnMoveDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnMoveUp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRemove As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddCriterion As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer

End Class
