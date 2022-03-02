

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmProcessMI

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProcessMI))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.SplitContainer1 = New AutomateControls.GrippableSplitContainer()
        Me.SplitContainer2 = New AutomateControls.GrippableSplitContainer()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.moResourceView = New AutomateUI.ctlResourceView()
        Me.lblFrom = New System.Windows.Forms.Label()
        Me.ctlFromDate = New AutomateUI.ctlProcessDateTime()
        Me.ctlToDate = New AutomateUI.ctlProcessDateTime()
        Me.lblTo = New System.Windows.Forms.Label()
        Me.chkDebugOnly = New System.Windows.Forms.CheckBox()
        Me.lblFound = New System.Windows.Forms.Label()
        Me.btnSearch = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblResourceToSearch = New System.Windows.Forms.Label()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.moListView = New AutomateUI.clsAutomateListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(),System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(),System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(),System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(),System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(),System.Windows.Forms.ColumnHeader)
        Me.lblSelected = New System.Windows.Forms.Label()
        Me.btnCheckNone = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCheckAll = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbTemplateSwitcher = New System.Windows.Forms.ComboBox()
        Me.btnDefault = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnDelete = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnSave = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnExport = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.moTreeView = New AutomateUI.clsProcessStageTreeView()
        Me.btnClear = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnClose = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label8 = New System.Windows.Forms.Label()
        Me.moProgressBar = New System.Windows.Forms.ProgressBar()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.btnAnalyse = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.objBluebar = New AutomateControls.TitleBar()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        CType(Me.SplitContainer1,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SplitContainer1.Panel1.SuspendLayout
        Me.SplitContainer1.Panel2.SuspendLayout
        Me.SplitContainer1.SuspendLayout
        CType(Me.SplitContainer2,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SplitContainer2.Panel1.SuspendLayout
        Me.SplitContainer2.Panel2.SuspendLayout
        Me.SplitContainer2.SuspendLayout
        Me.TableLayoutPanel3.SuspendLayout
        Me.TableLayoutPanel2.SuspendLayout
        Me.SuspendLayout
        '
        'SplitContainer1
        '
        Me.SplitContainer1.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.GripVisible = false
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.SplitContainer1.Panel1.Controls.Add(Me.SplitContainer2)
        resources.ApplyResources(Me.SplitContainer1.Panel1, "SplitContainer1.Panel1")
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.SplitContainer1.Panel2.Controls.Add(Me.cmbTemplateSwitcher)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnDefault)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnDelete)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnSave)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnExport)
        Me.SplitContainer1.Panel2.Controls.Add(Me.moTreeView)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnClear)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnClose)
        Me.SplitContainer1.Panel2.Controls.Add(Me.Label8)
        Me.SplitContainer1.Panel2.Controls.Add(Me.moProgressBar)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnCancel)
        Me.SplitContainer1.Panel2.Controls.Add(Me.lblMessage)
        Me.SplitContainer1.Panel2.Controls.Add(Me.btnAnalyse)
        resources.ApplyResources(Me.SplitContainer1.Panel2, "SplitContainer1.Panel2")
        Me.SplitContainer1.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.SplitContainer1.TabStop = false
        '
        'SplitContainer2
        '
        resources.ApplyResources(Me.SplitContainer2, "SplitContainer2")
        Me.SplitContainer2.GripVisible = false
        Me.SplitContainer2.Name = "SplitContainer2"
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.TableLayoutPanel3)
        Me.SplitContainer2.Panel1.Controls.Add(Me.lblResourceToSearch)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.TableLayoutPanel2)
        Me.SplitContainer2.Panel2.Controls.Add(Me.Label1)
        Me.SplitContainer2.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.SplitContainer2.TabStop = false
        '
        'TableLayoutPanel3
        '
        resources.ApplyResources(Me.TableLayoutPanel3, "TableLayoutPanel3")
        Me.TableLayoutPanel3.Controls.Add(Me.moResourceView, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.lblFrom, 0, 1)
        Me.TableLayoutPanel3.Controls.Add(Me.ctlFromDate, 1, 1)
        Me.TableLayoutPanel3.Controls.Add(Me.ctlToDate, 1, 2)
        Me.TableLayoutPanel3.Controls.Add(Me.lblTo, 0, 2)
        Me.TableLayoutPanel3.Controls.Add(Me.chkDebugOnly, 0, 3)
        Me.TableLayoutPanel3.Controls.Add(Me.lblFound, 2, 3)
        Me.TableLayoutPanel3.Controls.Add(Me.btnSearch, 3, 3)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        '
        'moResourceView
        '
        Me.TableLayoutPanel3.SetColumnSpan(Me.moResourceView, 4)
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.moResourceView.Comparer = TreeListViewItemCollectionComparer1
        resources.ApplyResources(Me.moResourceView, "moResourceView")
        Me.moResourceView.FocusedItem = Nothing
        Me.moResourceView.HideSelection = false
        Me.moResourceView.MultiLevelSelect = true
        Me.moResourceView.Name = "moResourceView"
        Me.moResourceView.ShowEmptyGroups = false
        Me.moResourceView.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Resources
        Me.moResourceView.UseCompatibleStateImageBehavior = false
        '
        'lblFrom
        '
        resources.ApplyResources(Me.lblFrom, "lblFrom")
        Me.lblFrom.Name = "lblFrom"
        '
        'ctlFromDate
        '
        Me.TableLayoutPanel3.SetColumnSpan(Me.ctlFromDate, 3)
        Me.ctlFromDate.DateButtonVisible = true
        resources.ApplyResources(Me.ctlFromDate, "ctlFromDate")
        Me.ctlFromDate.MaxValue = Nothing
        Me.ctlFromDate.MinValue = Nothing
        Me.ctlFromDate.Name = "ctlFromDate"
        Me.ctlFromDate.ReadOnly = false
        Me.ctlFromDate.Tag = "204,20"
        Me.ctlFromDate.TimeButtonVisible = true
        '
        'ctlToDate
        '
        Me.TableLayoutPanel3.SetColumnSpan(Me.ctlToDate, 3)
        Me.ctlToDate.DateButtonVisible = true
        resources.ApplyResources(Me.ctlToDate, "ctlToDate")
        Me.ctlToDate.MaxValue = Nothing
        Me.ctlToDate.MinValue = Nothing
        Me.ctlToDate.Name = "ctlToDate"
        Me.ctlToDate.ReadOnly = false
        Me.ctlToDate.Tag = "204,20"
        Me.ctlToDate.TimeButtonVisible = true
        '
        'lblTo
        '
        resources.ApplyResources(Me.lblTo, "lblTo")
        Me.lblTo.Name = "lblTo"
        '
        'chkDebugOnly
        '
        resources.ApplyResources(Me.chkDebugOnly, "chkDebugOnly")
        Me.TableLayoutPanel3.SetColumnSpan(Me.chkDebugOnly, 2)
        Me.chkDebugOnly.Name = "chkDebugOnly"
        Me.chkDebugOnly.UseVisualStyleBackColor = true
        '
        'lblFound
        '
        resources.ApplyResources(Me.lblFound, "lblFound")
        Me.lblFound.Name = "lblFound"
        '
        'btnSearch
        '
        resources.ApplyResources(Me.btnSearch, "btnSearch")
        Me.btnSearch.BackColor = System.Drawing.SystemColors.Control
        Me.btnSearch.Name = "btnSearch"
        Me.btnSearch.UseVisualStyleBackColor = false
        '
        'lblResourceToSearch
        '
        resources.ApplyResources(Me.lblResourceToSearch, "lblResourceToSearch")
        Me.lblResourceToSearch.Name = "lblResourceToSearch"
        '
        'TableLayoutPanel2
        '
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.moListView, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.lblSelected, 0, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.btnCheckNone, 2, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.btnCheckAll, 1, 1)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'moListView
        '
        Me.moListView.CheckBoxes = true
        Me.moListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5})
        Me.TableLayoutPanel2.SetColumnSpan(Me.moListView, 3)
        resources.ApplyResources(Me.moListView, "moListView")
        Me.moListView.FullRowSelect = true
        Me.moListView.HideSelection = false
        Me.moListView.Name = "moListView"
        Me.moListView.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.moListView.UseCompatibleStateImageBehavior = false
        Me.moListView.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'ColumnHeader2
        '
        resources.ApplyResources(Me.ColumnHeader2, "ColumnHeader2")
        '
        'ColumnHeader3
        '
        resources.ApplyResources(Me.ColumnHeader3, "ColumnHeader3")
        '
        'ColumnHeader4
        '
        resources.ApplyResources(Me.ColumnHeader4, "ColumnHeader4")
        '
        'ColumnHeader5
        '
        resources.ApplyResources(Me.ColumnHeader5, "ColumnHeader5")
        '
        'lblSelected
        '
        resources.ApplyResources(Me.lblSelected, "lblSelected")
        Me.lblSelected.Name = "lblSelected"
        '
        'btnCheckNone
        '
        Me.btnCheckNone.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.btnCheckNone, "btnCheckNone")
        Me.btnCheckNone.Name = "btnCheckNone"
        Me.btnCheckNone.UseVisualStyleBackColor = false
        '
        'btnCheckAll
        '
        Me.btnCheckAll.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.btnCheckAll, "btnCheckAll")
        Me.btnCheckAll.Name = "btnCheckAll"
        Me.btnCheckAll.UseVisualStyleBackColor = false
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'cmbTemplateSwitcher
        '
        resources.ApplyResources(Me.cmbTemplateSwitcher, "cmbTemplateSwitcher")
        Me.cmbTemplateSwitcher.FormattingEnabled = true
        Me.cmbTemplateSwitcher.Name = "cmbTemplateSwitcher"
        '
        'btnDefault
        '
        resources.ApplyResources(Me.btnDefault, "btnDefault")
        Me.btnDefault.Image = Global.AutomateUI.My.Resources.ToolImages.Wizard_16x16
        Me.btnDefault.Name = "btnDefault"
        Me.btnDefault.UseVisualStyleBackColor = false
        '
        'btnDelete
        '
        resources.ApplyResources(Me.btnDelete, "btnDelete")
        Me.btnDelete.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.UseVisualStyleBackColor = false
        '
        'btnSave
        '
        resources.ApplyResources(Me.btnSave, "btnSave")
        Me.btnSave.Image = Global.AutomateUI.My.Resources.ToolImages.Save_16x16
        Me.btnSave.Name = "btnSave"
        Me.btnSave.UseVisualStyleBackColor = false
        '
        'btnExport
        '
        resources.ApplyResources(Me.btnExport, "btnExport")
        Me.btnExport.BackColor = System.Drawing.SystemColors.Control
        Me.btnExport.Name = "btnExport"
        Me.btnExport.UseVisualStyleBackColor = false
        '
        'moTreeView
        '
        resources.ApplyResources(Me.moTreeView, "moTreeView")
        Me.moTreeView.CheckBoxes = true
        Me.moTreeView.Name = "moTreeView"
        Me.moTreeView.UseToolTips = false
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.BackColor = System.Drawing.SystemColors.Control
        Me.btnClear.Name = "btnClear"
        Me.btnClear.UseVisualStyleBackColor = false
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.BackColor = System.Drawing.SystemColors.Control
        Me.btnClose.Name = "btnClose"
        Me.btnClose.UseVisualStyleBackColor = false
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.Name = "Label8"
        '
        'moProgressBar
        '
        resources.ApplyResources(Me.moProgressBar, "moProgressBar")
        Me.moProgressBar.Name = "moProgressBar"
        Me.moProgressBar.Step = 1
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.BackColor = System.Drawing.SystemColors.Control
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = false
        '
        'lblMessage
        '
        resources.ApplyResources(Me.lblMessage, "lblMessage")
        Me.lblMessage.Name = "lblMessage"
        '
        'btnAnalyse
        '
        resources.ApplyResources(Me.btnAnalyse, "btnAnalyse")
        Me.btnAnalyse.BackColor = System.Drawing.SystemColors.Control
        Me.btnAnalyse.Name = "btnAnalyse"
        Me.btnAnalyse.UseVisualStyleBackColor = false
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        Me.objBluebar.Name = "objBluebar"
        '
        'SaveFileDialog1
        '
        resources.ApplyResources(Me.SaveFileDialog1, "SaveFileDialog1")
        Me.SaveFileDialog1.InitialDirectory = "c:\"
        '
        'frmProcessMI
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.objBluebar)
        Me.HelpButton = true
        Me.Name = "frmProcessMI"
        Me.SplitContainer1.Panel1.ResumeLayout(false)
        Me.SplitContainer1.Panel2.ResumeLayout(false)
        Me.SplitContainer1.Panel2.PerformLayout
        CType(Me.SplitContainer1,System.ComponentModel.ISupportInitialize).EndInit
        Me.SplitContainer1.ResumeLayout(false)
        Me.SplitContainer2.Panel1.ResumeLayout(false)
        Me.SplitContainer2.Panel1.PerformLayout
        Me.SplitContainer2.Panel2.ResumeLayout(false)
        Me.SplitContainer2.Panel2.PerformLayout
        CType(Me.SplitContainer2,System.ComponentModel.ISupportInitialize).EndInit
        Me.SplitContainer2.ResumeLayout(false)
        Me.TableLayoutPanel3.ResumeLayout(false)
        Me.TableLayoutPanel3.PerformLayout
        Me.TableLayoutPanel2.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents SplitContainer1 As AutomateControls.GrippableSplitContainer
    Friend WithEvents btnExport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents moTreeView As AutomateUI.clsProcessStageTreeView
    Friend WithEvents btnClear As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnClose As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents moProgressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents btnAnalyse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents lblFound As System.Windows.Forms.Label
    Friend WithEvents moListView As AutomateUI.clsAutomateListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents btnCheckAll As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCheckNone As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents objBluebar As AutomateControls.TitleBar
    Friend WithEvents ctlToDate As AutomateUI.ctlProcessDateTime
    Friend WithEvents btnSearch As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ctlFromDate As AutomateUI.ctlProcessDateTime
    Friend WithEvents lblTo As System.Windows.Forms.Label
    Friend WithEvents lblResourceToSearch As System.Windows.Forms.Label
    Friend WithEvents lblSelected As System.Windows.Forms.Label
    Friend WithEvents lblFrom As System.Windows.Forms.Label
    Friend WithEvents moResourceView As AutomateUI.ctlResourceView
    Friend WithEvents chkDebugOnly As System.Windows.Forms.CheckBox
    Friend WithEvents cmbTemplateSwitcher As System.Windows.Forms.ComboBox
    Friend WithEvents btnDefault As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDelete As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnSave As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SplitContainer2 As AutomateControls.GrippableSplitContainer
    Friend WithEvents TableLayoutPanel3 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
