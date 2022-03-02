<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagePropertiesCode
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesCode))
        Me.mDataItemTree = New AutomateUI.ctlDataItemTreeView()
        Me.mParamsControl = New AutomateUI.ctlInputsOutputsConditions()
        Me.mSplitPanel = New AutomateControls.GrippableSplitContainer()
        Me.mSwitcher = New AutomateControls.SwitchPanel()
        Me.tpDataItemTree = New System.Windows.Forms.TabPage()
        Me.tpCodeParams = New System.Windows.Forms.TabPage()
        Me.mParamList = New AutomateUI.ctlCodeStageParameterList()
        CType(Me.mSplitPanel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitPanel.Panel1.SuspendLayout()
        Me.mSplitPanel.Panel2.SuspendLayout()
        Me.mSplitPanel.SuspendLayout()
        Me.mSwitcher.SuspendLayout()
        Me.tpDataItemTree.SuspendLayout()
        Me.tpCodeParams.SuspendLayout()
        Me.SuspendLayout()
        '
        'mDataItemTree
        '
        Me.mDataItemTree.CheckBoxes = False
        resources.ApplyResources(Me.mDataItemTree, "mDataItemTree")
        Me.mDataItemTree.IgnoreScope = False
        Me.mDataItemTree.Name = "mDataItemTree"
        Me.mDataItemTree.Stage = Nothing
        Me.mDataItemTree.StatisticsMode = False
        '
        'mParamsControl
        '
        resources.ApplyResources(Me.mParamsControl, "mParamsControl")
        Me.mParamsControl.CodeVisible = True
        Me.mParamsControl.Name = "mParamsControl"
        '
        'mSplitPanel
        '
        resources.ApplyResources(Me.mSplitPanel, "mSplitPanel")
        Me.mSplitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.mSplitPanel.Name = "mSplitPanel"
        '
        'mSplitPanel.Panel1
        '
        Me.mSplitPanel.Panel1.Controls.Add(Me.mParamsControl)
        '
        'mSplitPanel.Panel2
        '
        Me.mSplitPanel.Panel2.Controls.Add(Me.mSwitcher)
        Me.mSplitPanel.TabStop = False
        '
        'mSwitcher
        '
        resources.ApplyResources(Me.mSwitcher, "mSwitcher")
        Me.mSwitcher.Controls.Add(Me.tpDataItemTree)
        Me.mSwitcher.Controls.Add(Me.tpCodeParams)
        Me.mSwitcher.Name = "mSwitcher"
        Me.mSwitcher.SelectedIndex = 0
        '
        'tpDataItemTree
        '
        Me.tpDataItemTree.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpDataItemTree.Controls.Add(Me.mDataItemTree)
        resources.ApplyResources(Me.tpDataItemTree, "tpDataItemTree")
        Me.tpDataItemTree.Name = "tpDataItemTree"
        '
        'tpCodeParams
        '
        Me.tpCodeParams.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpCodeParams.Controls.Add(Me.mParamList)
        resources.ApplyResources(Me.tpCodeParams, "tpCodeParams")
        Me.tpCodeParams.Name = "tpCodeParams"
        '
        'mParamList
        '
        Me.mParamList.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.mParamList, "mParamList")
        Me.mParamList.Name = "mParamList"
        '
        'frmStagePropertiesCode
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mSplitPanel)
        Me.Name = "frmStagePropertiesCode"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.mSplitPanel, 0)
        Me.mSplitPanel.Panel1.ResumeLayout(False)
        Me.mSplitPanel.Panel2.ResumeLayout(False)
        CType(Me.mSplitPanel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplitPanel.ResumeLayout(False)
        Me.mSwitcher.ResumeLayout(False)
        Me.tpDataItemTree.ResumeLayout(False)
        Me.tpCodeParams.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents mDataItemTree As AutomateUI.ctlDataItemTreeView
    Friend WithEvents mParamsControl As AutomateUI.ctlInputsOutputsConditions
    Private WithEvents mSplitPanel As AutomateControls.GrippableSplitContainer
    Private WithEvents tpDataItemTree As System.Windows.Forms.TabPage
    Friend WithEvents tpCodeParams As System.Windows.Forms.TabPage
    Private WithEvents mParamList As AutomateUI.ctlCodeStageParameterList
    Private WithEvents mSwitcher As AutomateControls.SwitchPanel
End Class
