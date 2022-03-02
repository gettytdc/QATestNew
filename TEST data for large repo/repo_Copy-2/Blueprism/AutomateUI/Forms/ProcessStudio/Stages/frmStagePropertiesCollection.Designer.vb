<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagePropertiesCollection
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesCollection))
        Me.lvCollectionFields = New AutomateUI.ctlListView()
        Me.chkPrivate = New System.Windows.Forms.CheckBox()
        Me.btnAddField = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRemoveField = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tabFieldsAndValues = New AutomateControls.DisablingTabControl()
        Me.tbFields = New System.Windows.Forms.TabPage()
        Me.panFieldButtons = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnImport = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnClearFields = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tbInitValue = New System.Windows.Forms.TabPage()
        Me.lvInitialValue = New AutomateUI.ctlListView()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lblRows = New System.Windows.Forms.Label()
        Me.btnAddInitialValueRow = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRemoveInitialValueRow = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tbCurrent = New System.Windows.Forms.TabPage()
        Me.lvCurrentValue = New AutomateUI.ctlListView()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnRemoveCurrentValueRow = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnAddCurrentValueRow = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.chkAlwaysInit = New System.Windows.Forms.CheckBox()
        Me.chkSingleRow = New System.Windows.Forms.CheckBox()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.mBreadcrumbs = New AutomateUI.ctlBreadcrumbTrail()
        Me.mTooltip = New System.Windows.Forms.ToolTip(Me.components)
        Me.tabFieldsAndValues.SuspendLayout()
        Me.tbFields.SuspendLayout()
        Me.panFieldButtons.SuspendLayout()
        Me.tbInitValue.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.tbCurrent.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
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
        'lvCollectionFields
        '
        Me.lvCollectionFields.AllowDrop = True
        resources.ApplyResources(Me.lvCollectionFields, "lvCollectionFields")
        Me.lvCollectionFields.BackColor = System.Drawing.Color.White
        Me.lvCollectionFields.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvCollectionFields.CurrentEditableRow = Nothing
        Me.lvCollectionFields.FillColumn = Nothing
        Me.lvCollectionFields.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvCollectionFields.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lvCollectionFields.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.lvCollectionFields.HighlightedRowOutline = System.Drawing.Color.Red
        Me.lvCollectionFields.LastColumnAutoSize = False
        Me.lvCollectionFields.MinimumColumnWidth = 50
        Me.lvCollectionFields.Name = "lvCollectionFields"
        Me.lvCollectionFields.Readonly = False
        Me.lvCollectionFields.RowHeight = 26
        Me.lvCollectionFields.Rows.Capacity = 0
        Me.lvCollectionFields.Sortable = False
        '
        'chkPrivate
        '
        resources.ApplyResources(Me.chkPrivate, "chkPrivate")
        Me.chkPrivate.ForeColor = System.Drawing.Color.Black
        Me.chkPrivate.Name = "chkPrivate"
        '
        'btnAddField
        '
        resources.ApplyResources(Me.btnAddField, "btnAddField")
        Me.btnAddField.Name = "btnAddField"
        Me.btnAddField.UseVisualStyleBackColor = False
        '
        'btnRemoveField
        '
        resources.ApplyResources(Me.btnRemoveField, "btnRemoveField")
        Me.btnRemoveField.Name = "btnRemoveField"
        Me.btnRemoveField.UseVisualStyleBackColor = False
        '
        'tabFieldsAndValues
        '
        Me.tabFieldsAndValues.Controls.Add(Me.tbFields)
        Me.tabFieldsAndValues.Controls.Add(Me.tbInitValue)
        Me.tabFieldsAndValues.Controls.Add(Me.tbCurrent)
        resources.ApplyResources(Me.tabFieldsAndValues, "tabFieldsAndValues")
        Me.tabFieldsAndValues.DrawBorder = False
        Me.tabFieldsAndValues.Name = "tabFieldsAndValues"
        Me.tabFieldsAndValues.SelectedIndex = 0
        '
        'tbFields
        '
        Me.tbFields.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tbFields.Controls.Add(Me.lvCollectionFields)
        Me.tbFields.Controls.Add(Me.panFieldButtons)
        resources.ApplyResources(Me.tbFields, "tbFields")
        Me.tbFields.Name = "tbFields"
        '
        'panFieldButtons
        '
        Me.panFieldButtons.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.panFieldButtons.Controls.Add(Me.Label1)
        Me.panFieldButtons.Controls.Add(Me.btnImport)
        Me.panFieldButtons.Controls.Add(Me.btnClearFields)
        Me.panFieldButtons.Controls.Add(Me.btnAddField)
        Me.panFieldButtons.Controls.Add(Me.btnRemoveField)
        resources.ApplyResources(Me.panFieldButtons, "panFieldButtons")
        Me.panFieldButtons.Name = "panFieldButtons"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'btnImport
        '
        resources.ApplyResources(Me.btnImport, "btnImport")
        Me.btnImport.Name = "btnImport"
        Me.btnImport.UseVisualStyleBackColor = False
        '
        'btnClearFields
        '
        resources.ApplyResources(Me.btnClearFields, "btnClearFields")
        Me.btnClearFields.Name = "btnClearFields"
        Me.btnClearFields.UseVisualStyleBackColor = False
        '
        'tbInitValue
        '
        Me.tbInitValue.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tbInitValue.Controls.Add(Me.lvInitialValue)
        Me.tbInitValue.Controls.Add(Me.Panel1)
        resources.ApplyResources(Me.tbInitValue, "tbInitValue")
        Me.tbInitValue.Name = "tbInitValue"
        '
        'lvInitialValue
        '
        Me.lvInitialValue.AllowDrop = True
        resources.ApplyResources(Me.lvInitialValue, "lvInitialValue")
        Me.lvInitialValue.BackColor = System.Drawing.Color.White
        Me.lvInitialValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvInitialValue.CurrentEditableRow = Nothing
        Me.lvInitialValue.FillColumn = Nothing
        Me.lvInitialValue.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvInitialValue.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lvInitialValue.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.lvInitialValue.HighlightedRowOutline = System.Drawing.Color.Red
        Me.lvInitialValue.LastColumnAutoSize = False
        Me.lvInitialValue.MinimumColumnWidth = 50
        Me.lvInitialValue.Name = "lvInitialValue"
        Me.lvInitialValue.Readonly = False
        Me.lvInitialValue.RowHeight = 26
        Me.lvInitialValue.Rows.Capacity = 0
        Me.lvInitialValue.Sortable = False
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.lblRows)
        Me.Panel1.Controls.Add(Me.btnAddInitialValueRow)
        Me.Panel1.Controls.Add(Me.btnRemoveInitialValueRow)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'lblRows
        '
        resources.ApplyResources(Me.lblRows, "lblRows")
        Me.lblRows.Name = "lblRows"
        '
        'btnAddInitialValueRow
        '
        resources.ApplyResources(Me.btnAddInitialValueRow, "btnAddInitialValueRow")
        Me.btnAddInitialValueRow.Name = "btnAddInitialValueRow"
        Me.btnAddInitialValueRow.UseVisualStyleBackColor = False
        '
        'btnRemoveInitialValueRow
        '
        resources.ApplyResources(Me.btnRemoveInitialValueRow, "btnRemoveInitialValueRow")
        Me.btnRemoveInitialValueRow.Name = "btnRemoveInitialValueRow"
        Me.btnRemoveInitialValueRow.UseVisualStyleBackColor = False
        '
        'tbCurrent
        '
        Me.tbCurrent.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tbCurrent.Controls.Add(Me.lvCurrentValue)
        Me.tbCurrent.Controls.Add(Me.Panel2)
        resources.ApplyResources(Me.tbCurrent, "tbCurrent")
        Me.tbCurrent.Name = "tbCurrent"
        '
        'lvCurrentValue
        '
        Me.lvCurrentValue.AllowDrop = True
        resources.ApplyResources(Me.lvCurrentValue, "lvCurrentValue")
        Me.lvCurrentValue.BackColor = System.Drawing.Color.White
        Me.lvCurrentValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvCurrentValue.CurrentEditableRow = Nothing
        Me.lvCurrentValue.FillColumn = Nothing
        Me.lvCurrentValue.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvCurrentValue.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lvCurrentValue.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.lvCurrentValue.HighlightedRowOutline = System.Drawing.Color.Red
        Me.lvCurrentValue.LastColumnAutoSize = False
        Me.lvCurrentValue.MinimumColumnWidth = 50
        Me.lvCurrentValue.Name = "lvCurrentValue"
        Me.lvCurrentValue.Readonly = False
        Me.lvCurrentValue.RowHeight = 26
        Me.lvCurrentValue.Rows.Capacity = 0
        Me.lvCurrentValue.Sortable = False
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.Label2)
        Me.Panel2.Controls.Add(Me.btnRemoveCurrentValueRow)
        Me.Panel2.Controls.Add(Me.btnAddCurrentValueRow)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'btnRemoveCurrentValueRow
        '
        resources.ApplyResources(Me.btnRemoveCurrentValueRow, "btnRemoveCurrentValueRow")
        Me.btnRemoveCurrentValueRow.Name = "btnRemoveCurrentValueRow"
        Me.btnRemoveCurrentValueRow.UseVisualStyleBackColor = False
        '
        'btnAddCurrentValueRow
        '
        resources.ApplyResources(Me.btnAddCurrentValueRow, "btnAddCurrentValueRow")
        Me.btnAddCurrentValueRow.Name = "btnAddCurrentValueRow"
        Me.btnAddCurrentValueRow.UseVisualStyleBackColor = False
        '
        'chkAlwaysInit
        '
        resources.ApplyResources(Me.chkAlwaysInit, "chkAlwaysInit")
        Me.chkAlwaysInit.ForeColor = System.Drawing.Color.Black
        Me.chkAlwaysInit.Name = "chkAlwaysInit"
        '
        'chkSingleRow
        '
        resources.ApplyResources(Me.chkSingleRow, "chkSingleRow")
        Me.chkSingleRow.ForeColor = System.Drawing.Color.Black
        Me.chkSingleRow.Name = "chkSingleRow"
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Controls.Add(Me.chkAlwaysInit)
        Me.FlowLayoutPanel1.Controls.Add(Me.chkPrivate)
        Me.FlowLayoutPanel1.Controls.Add(Me.chkSingleRow)
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'mBreadcrumbs
        '
        resources.ApplyResources(Me.mBreadcrumbs, "mBreadcrumbs")
        Me.mBreadcrumbs.Name = "mBreadcrumbs"
        Me.mBreadcrumbs.Wrap = False
        '
        'mTooltip
        '
        Me.mTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        '
        'frmStagePropertiesCollection
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.tabFieldsAndValues)
        Me.Controls.Add(Me.mBreadcrumbs)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Name = "frmStagePropertiesCollection"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.FlowLayoutPanel1, 0)
        Me.Controls.SetChildIndex(Me.mBreadcrumbs, 0)
        Me.Controls.SetChildIndex(Me.tabFieldsAndValues, 0)
        Me.tabFieldsAndValues.ResumeLayout(False)
        Me.tbFields.ResumeLayout(False)
        Me.panFieldButtons.ResumeLayout(False)
        Me.panFieldButtons.PerformLayout()
        Me.tbInitValue.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.tbCurrent.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkPrivate As System.Windows.Forms.CheckBox
    Friend WithEvents tabFieldsAndValues As AutomateControls.DisablingTabControl
    Friend WithEvents tbFields As System.Windows.Forms.TabPage
    Friend WithEvents tbInitValue As System.Windows.Forms.TabPage
    Friend WithEvents tbCurrent As System.Windows.Forms.TabPage
    Friend WithEvents btnRemoveInitialValueRow As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddInitialValueRow As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lvInitialValue As AutomateUI.ctlListView
    Friend WithEvents btnImport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddField As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRemoveField As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnClearFields As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddCurrentValueRow As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRemoveCurrentValueRow As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lvCollectionFields As AutomateUI.ctlListView
    Friend WithEvents chkAlwaysInit As System.Windows.Forms.CheckBox
    Private WithEvents chkSingleRow As System.Windows.Forms.CheckBox
    Private WithEvents FlowLayoutPanel1 As System.Windows.Forms.FlowLayoutPanel
    Private WithEvents mBreadcrumbs As AutomateUI.ctlBreadcrumbTrail
    Friend WithEvents lvCurrentValue As AutomateUI.ctlListView
    Public WithEvents mTooltip As System.Windows.Forms.ToolTip
    Private WithEvents panFieldButtons As System.Windows.Forms.Panel
    Private WithEvents Panel1 As System.Windows.Forms.Panel
    Private WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Label1 As Label
    Friend WithEvents lblRows As Label
    Friend WithEvents Label2 As Label
End Class
