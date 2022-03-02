Imports AutomateControls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmIntegrationAssistant
    Inherits Forms.HelpButtonForm

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmIntegrationAssistant))
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.mTitleBar = New AutomateControls.TitleBar()
        Me.pnlBottomStrip = New System.Windows.Forms.Panel()
        Me.ctxMenuRegions = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuNewScreenshot = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuOpenScreenshot = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolTips = New System.Windows.Forms.ToolTip(Me.components)
        Me.timerElementName = New System.Windows.Forms.Timer(Me.components)
        Me.ctxMenuIdentify = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.miSpyElement = New System.Windows.Forms.ToolStripMenuItem()
        Me.miAppNavigator = New System.Windows.Forms.ToolStripMenuItem()
        Me.miUIAutomationNavigator = New System.Windows.Forms.ToolStripMenuItem()
        Me.lblModelOwner = New System.Windows.Forms.Label()
        Me.splitMain = New AutomateControls.GrippableSplitContainer()
        Me.appExplorer = New AutomateUI.ctlApplicationExplorer()
        Me.lblAppExplorer = New System.Windows.Forms.Label()
        Me.panSwitch = New AutomateControls.SwitchPanel()
        Me.tabAppInfo = New System.Windows.Forms.TabPage()
        Me.panAppInfo = New AutomateUI.ctlApplicationInfo()
        Me.tabElemInfo = New System.Windows.Forms.TabPage()
        Me.pnlElementDetails = New System.Windows.Forms.Panel()
        Me.btnIdentify = New AutomateControls.SplitButton()
        Me.lblElemDetails = New System.Windows.Forms.Label()
        Me.cmbDataType = New System.Windows.Forms.ComboBox()
        Me.cmbElemType = New System.Windows.Forms.ComboBox()
        Me.btnRegions = New AutomateControls.SplitButton()
        Me.tcElementDetails = New System.Windows.Forms.TabControl()
        Me.tpAttributes = New System.Windows.Forms.TabPage()
        Me.elemEditor = New AutomateUI.ctlApplicationElementEditor()
        Me.tpNotes = New System.Windows.Forms.TabPage()
        Me.txtNotes = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.btnClearAttributes = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnShowElement = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtElementName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblElementType = New System.Windows.Forms.Label()
        Me.lblElementDataType = New System.Windows.Forms.Label()
        Me.lblElementName = New System.Windows.Forms.Label()
        Me.pnlBottomStrip.SuspendLayout()
        Me.ctxMenuRegions.SuspendLayout()
        Me.ctxMenuIdentify.SuspendLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.panSwitch.SuspendLayout()
        Me.tabAppInfo.SuspendLayout()
        Me.tabElemInfo.SuspendLayout()
        Me.pnlElementDetails.SuspendLayout()
        Me.tcElementDetails.SuspendLayout()
        Me.tpAttributes.SuspendLayout()
        Me.tpNotes.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.Name = "btnApply"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        Me.mTitleBar.Name = "mTitleBar"
        '
        'pnlBottomStrip
        '
        Me.pnlBottomStrip.Controls.Add(Me.btnApply)
        Me.pnlBottomStrip.Controls.Add(Me.btnCancel)
        Me.pnlBottomStrip.Controls.Add(Me.btnOK)
        resources.ApplyResources(Me.pnlBottomStrip, "pnlBottomStrip")
        Me.pnlBottomStrip.Name = "pnlBottomStrip"
        '
        'ctxMenuRegions
        '
        Me.ctxMenuRegions.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuNewScreenshot, Me.mnuOpenScreenshot})
        Me.ctxMenuRegions.Name = "ctxMenuRegions"
        resources.ApplyResources(Me.ctxMenuRegions, "ctxMenuRegions")
        '
        'mnuNewScreenshot
        '
        Me.mnuNewScreenshot.Name = "mnuNewScreenshot"
        resources.ApplyResources(Me.mnuNewScreenshot, "mnuNewScreenshot")
        '
        'mnuOpenScreenshot
        '
        resources.ApplyResources(Me.mnuOpenScreenshot, "mnuOpenScreenshot")
        Me.mnuOpenScreenshot.Name = "mnuOpenScreenshot"
        '
        'timerElementName
        '
        Me.timerElementName.Interval = 500
        '
        'ctxMenuIdentify
        '
        Me.ctxMenuIdentify.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.miSpyElement, Me.miAppNavigator, Me.miUIAutomationNavigator})
        Me.ctxMenuIdentify.Name = "ctxMenuIdentify"
        resources.ApplyResources(Me.ctxMenuIdentify, "ctxMenuIdentify")
        '
        'miSpyElement
        '
        resources.ApplyResources(Me.miSpyElement, "miSpyElement")
        Me.miSpyElement.Name = "miSpyElement"
        '
        'miAppNavigator
        '
        Me.miAppNavigator.Name = "miAppNavigator"
        resources.ApplyResources(Me.miAppNavigator, "miAppNavigator")
        '
        'miUIAutomationNavigator
        '
        Me.miUIAutomationNavigator.Name = "miUIAutomationNavigator"
        resources.ApplyResources(Me.miUIAutomationNavigator, "miUIAutomationNavigator")
        '
        'lblModelOwner
        '
        Me.lblModelOwner.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.lblModelOwner, "lblModelOwner")
        Me.lblModelOwner.Name = "lblModelOwner"
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.appExplorer)
        Me.splitMain.Panel1.Controls.Add(Me.lblAppExplorer)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.panSwitch)
        Me.splitMain.SplitLineColor = System.Drawing.SystemColors.InactiveBorder
        Me.splitMain.TabStop = False
        '
        'appExplorer
        '
        resources.ApplyResources(Me.appExplorer, "appExplorer")
        Me.appExplorer.Name = "appExplorer"
        Me.appExplorer.ReadOnly = False
        '
        'lblAppExplorer
        '
        resources.ApplyResources(Me.lblAppExplorer, "lblAppExplorer")
        Me.lblAppExplorer.BackColor = System.Drawing.Color.Transparent
        Me.lblAppExplorer.Name = "lblAppExplorer"
        '
        'panSwitch
        '
        Me.panSwitch.Controls.Add(Me.tabAppInfo)
        Me.panSwitch.Controls.Add(Me.tabElemInfo)
        Me.panSwitch.DisableArrowKeys = False
        resources.ApplyResources(Me.panSwitch, "panSwitch")
        Me.panSwitch.Name = "panSwitch"
        Me.panSwitch.SelectedIndex = 0
        '
        'tabAppInfo
        '
        Me.tabAppInfo.Controls.Add(Me.panAppInfo)
        resources.ApplyResources(Me.tabAppInfo, "tabAppInfo")
        Me.tabAppInfo.Name = "tabAppInfo"
        '
        'panAppInfo
        '
        Me.panAppInfo.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me.panAppInfo, "panAppInfo")
        Me.panAppInfo.LaunchEnabled = True
        Me.panAppInfo.Name = "panAppInfo"
        Me.panAppInfo.ReadOnly = False
        '
        'tabElemInfo
        '
        Me.tabElemInfo.Controls.Add(Me.pnlElementDetails)
        resources.ApplyResources(Me.tabElemInfo, "tabElemInfo")
        Me.tabElemInfo.Name = "tabElemInfo"
        '
        'pnlElementDetails
        '
        Me.pnlElementDetails.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.pnlElementDetails.Controls.Add(Me.btnIdentify)
        Me.pnlElementDetails.Controls.Add(Me.lblElemDetails)
        Me.pnlElementDetails.Controls.Add(Me.cmbDataType)
        Me.pnlElementDetails.Controls.Add(Me.cmbElemType)
        Me.pnlElementDetails.Controls.Add(Me.btnRegions)
        Me.pnlElementDetails.Controls.Add(Me.tcElementDetails)
        Me.pnlElementDetails.Controls.Add(Me.txtDescription)
        Me.pnlElementDetails.Controls.Add(Me.lblDescription)
        Me.pnlElementDetails.Controls.Add(Me.btnClearAttributes)
        Me.pnlElementDetails.Controls.Add(Me.btnShowElement)
        Me.pnlElementDetails.Controls.Add(Me.txtElementName)
        Me.pnlElementDetails.Controls.Add(Me.lblElementType)
        Me.pnlElementDetails.Controls.Add(Me.lblElementDataType)
        Me.pnlElementDetails.Controls.Add(Me.lblElementName)
        resources.ApplyResources(Me.pnlElementDetails, "pnlElementDetails")
        Me.pnlElementDetails.Name = "pnlElementDetails"
        '
        'btnIdentify
        '
        resources.ApplyResources(Me.btnIdentify, "btnIdentify")
        Me.btnIdentify.Name = "btnIdentify"
        Me.btnIdentify.UseVisualStyleBackColor = True
        '
        'lblElemDetails
        '
        resources.ApplyResources(Me.lblElemDetails, "lblElemDetails")
        Me.lblElemDetails.BackColor = System.Drawing.Color.Transparent
        Me.lblElemDetails.Name = "lblElemDetails"
        '
        'cmbDataType
        '
        resources.ApplyResources(Me.cmbDataType, "cmbDataType")
        Me.cmbDataType.DisplayMember = "FriendlyName"
        Me.cmbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDataType.FormattingEnabled = True
        Me.cmbDataType.Name = "cmbDataType"
        Me.ToolTips.SetToolTip(Me.cmbDataType, resources.GetString("cmbDataType.ToolTip"))
        Me.cmbDataType.ValueMember = "Value"
        '
        'cmbElemType
        '
        resources.ApplyResources(Me.cmbElemType, "cmbElemType")
        Me.cmbElemType.DisplayMember = "Name"
        Me.cmbElemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbElemType.DropDownWidth = 160
        Me.cmbElemType.FormattingEnabled = True
        Me.cmbElemType.Name = "cmbElemType"
        Me.ToolTips.SetToolTip(Me.cmbElemType, resources.GetString("cmbElemType.ToolTip"))
        '
        'btnRegions
        '
        resources.ApplyResources(Me.btnRegions, "btnRegions")
        Me.btnRegions.ContextMenuStrip = Me.ctxMenuRegions
        Me.btnRegions.Name = "btnRegions"
        Me.btnRegions.SplitMenuStrip = Me.ctxMenuRegions
        '
        'tcElementDetails
        '
        resources.ApplyResources(Me.tcElementDetails, "tcElementDetails")
        Me.tcElementDetails.Controls.Add(Me.tpAttributes)
        Me.tcElementDetails.Controls.Add(Me.tpNotes)
        Me.tcElementDetails.Name = "tcElementDetails"
        Me.tcElementDetails.SelectedIndex = 0
        '
        'tpAttributes
        '
        Me.tpAttributes.Controls.Add(Me.elemEditor)
        resources.ApplyResources(Me.tpAttributes, "tpAttributes")
        Me.tpAttributes.Name = "tpAttributes"
        Me.tpAttributes.UseVisualStyleBackColor = True
        '
        'elemEditor
        '
        Me.elemEditor.AllowDrop = True
        Me.elemEditor.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.elemEditor.CurrentEditableRow = Nothing
        resources.ApplyResources(Me.elemEditor, "elemEditor")
        Me.elemEditor.FillColumn = Nothing
        Me.elemEditor.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.elemEditor.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.elemEditor.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182, Byte), Integer), CType(CType(202, Byte), Integer), CType(CType(234, Byte), Integer))
        Me.elemEditor.HighlightedRowOutline = System.Drawing.Color.Red
        Me.elemEditor.LastColumnAutoSize = True
        Me.elemEditor.MinimumColumnWidth = 100
        Me.elemEditor.Name = "elemEditor"
        Me.elemEditor.Readonly = False
        Me.elemEditor.RowHeight = 24
        Me.elemEditor.Rows.Capacity = 0
        Me.elemEditor.Sortable = False
        '
        'tpNotes
        '
        Me.tpNotes.Controls.Add(Me.txtNotes)
        resources.ApplyResources(Me.tpNotes, "tpNotes")
        Me.tpNotes.Name = "tpNotes"
        Me.tpNotes.UseVisualStyleBackColor = True
        '
        'txtNotes
        '
        Me.txtNotes.AcceptsReturn = True
        resources.ApplyResources(Me.txtNotes, "txtNotes")
        Me.txtNotes.Name = "txtNotes"
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        Me.ToolTips.SetToolTip(Me.txtDescription, resources.GetString("txtDescription.ToolTip"))
        '
        'lblDescription
        '
        resources.ApplyResources(Me.lblDescription, "lblDescription")
        Me.lblDescription.Name = "lblDescription"
        '
        'btnClearAttributes
        '
        resources.ApplyResources(Me.btnClearAttributes, "btnClearAttributes")
        Me.btnClearAttributes.Name = "btnClearAttributes"
        '
        'btnShowElement
        '
        resources.ApplyResources(Me.btnShowElement, "btnShowElement")
        Me.btnShowElement.Name = "btnShowElement"
        '
        'txtElementName
        '
        resources.ApplyResources(Me.txtElementName, "txtElementName")
        Me.txtElementName.Name = "txtElementName"
        Me.ToolTips.SetToolTip(Me.txtElementName, resources.GetString("txtElementName.ToolTip"))
        '
        'lblElementType
        '
        resources.ApplyResources(Me.lblElementType, "lblElementType")
        Me.lblElementType.Name = "lblElementType"
        '
        'lblElementDataType
        '
        resources.ApplyResources(Me.lblElementDataType, "lblElementDataType")
        Me.lblElementDataType.Name = "lblElementDataType"
        '
        'lblElementName
        '
        resources.ApplyResources(Me.lblElementName, "lblElementName")
        Me.lblElementName.Name = "lblElementName"
        '
        'frmIntegrationAssistant
        '
        Me.AcceptButton = Me.btnOK
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblModelOwner)
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.mTitleBar)
        Me.Controls.Add(Me.pnlBottomStrip)
        Me.HelpButton = True
        Me.Name = "frmIntegrationAssistant"
        Me.pnlBottomStrip.ResumeLayout(False)
        Me.ctxMenuRegions.ResumeLayout(False)
        Me.ctxMenuIdentify.ResumeLayout(False)
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel1.PerformLayout()
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.panSwitch.ResumeLayout(False)
        Me.tabAppInfo.ResumeLayout(False)
        Me.tabElemInfo.ResumeLayout(False)
        Me.pnlElementDetails.ResumeLayout(False)
        Me.pnlElementDetails.PerformLayout()
        Me.tcElementDetails.ResumeLayout(False)
        Me.tpAttributes.ResumeLayout(False)
        Me.tpNotes.ResumeLayout(False)
        Me.tpNotes.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents timerElementName As System.Windows.Forms.Timer
    Private WithEvents ctxMenuRegions As System.Windows.Forms.ContextMenuStrip
    Private WithEvents mnuOpenScreenshot As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuNewScreenshot As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents mTitleBar As AutomateControls.TitleBar
    Private WithEvents pnlBottomStrip As System.Windows.Forms.Panel
    Private WithEvents lblAppExplorer As System.Windows.Forms.Label
    Private WithEvents appExplorer As AutomateUI.ctlApplicationExplorer
    Private WithEvents ToolTips As System.Windows.Forms.ToolTip
    Private WithEvents splitMain As AutomateControls.GrippableSplitContainer
    Private WithEvents pnlElementDetails As System.Windows.Forms.Panel
    Private WithEvents lblElemDetails As System.Windows.Forms.Label
    Private WithEvents btnRegions As AutomateControls.SplitButton
    Private WithEvents tcElementDetails As System.Windows.Forms.TabControl
    Private WithEvents tpAttributes As System.Windows.Forms.TabPage
    Private WithEvents elemEditor As AutomateUI.ctlApplicationElementEditor
    Private WithEvents tpNotes As System.Windows.Forms.TabPage
    Private WithEvents txtNotes As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblDescription As System.Windows.Forms.Label
    Private WithEvents btnClearAttributes As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnShowElement As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents txtElementName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblElementType As System.Windows.Forms.Label
    Private WithEvents lblElementDataType As System.Windows.Forms.Label
    Private WithEvents lblElementName As System.Windows.Forms.Label
    Private WithEvents tabAppInfo As System.Windows.Forms.TabPage
    Private WithEvents panSwitch As AutomateControls.SwitchPanel
    Private WithEvents tabElemInfo As System.Windows.Forms.TabPage
    Private WithEvents panAppInfo As AutomateUI.ctlApplicationInfo
    Private WithEvents cmbDataType As System.Windows.Forms.ComboBox
    Private WithEvents cmbElemType As System.Windows.Forms.ComboBox
    Private WithEvents btnIdentify As AutomateControls.SplitButton
    Friend WithEvents ctxMenuIdentify As System.Windows.Forms.ContextMenuStrip
    Private WithEvents miSpyElement As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents miAppNavigator As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblModelOwner As System.Windows.Forms.Label
    Private WithEvents miUIAutomationNavigator As ToolStripMenuItem
End Class
