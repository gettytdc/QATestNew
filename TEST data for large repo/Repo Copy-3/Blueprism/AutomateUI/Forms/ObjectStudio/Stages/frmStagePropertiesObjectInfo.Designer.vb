<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Friend Class frmStagePropertiesObjectInfo
    Inherits frmStagePropertiesProcessInfo

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesObjectInfo))
        Me.tcGlobalCode = New System.Windows.Forms.TabPage()
        Me.mEditor = New AutomateUI.ctlCodeEditor()
        Me.btnCheckCode = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tcCodeOptions = New System.Windows.Forms.TabPage()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lstDlls = New AutomateUI.ctlListView()
        Me.lLanguage = New System.Windows.Forms.Label()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.cmbLanguage = New System.Windows.Forms.ComboBox()
        Me.btnAddReference = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRemoveNamespace = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lstNamespaces = New AutomateUI.ctlListView()
        Me.btnAddNamespace = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRemoveRef = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.rdoForeground = New AutomateControls.StyledRadioButton()
        Me.rdoExclusive = New AutomateControls.StyledRadioButton()
        Me.rdoBackground = New AutomateControls.StyledRadioButton()
        Me.lblForeground = New System.Windows.Forms.Label()
        Me.lblBackground = New System.Windows.Forms.Label()
        Me.lblExclusive = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.cbSharedObject = New System.Windows.Forms.CheckBox()
        Me.lblSharedObject = New System.Windows.Forms.Label()
        Me.TabControl1.SuspendLayout
        Me.tcInfo.SuspendLayout
        Me.gpLogging.SuspendLayout
        CType(Me.spinLoggingRetryPeriod,System.ComponentModel.ISupportInitialize).BeginInit
        CType(Me.spinLoggingAttempts,System.ComponentModel.ISupportInitialize).BeginInit
        Me.tcGlobalCode.SuspendLayout
        Me.tcCodeOptions.SuspendLayout
        Me.Panel1.SuspendLayout
        Me.GroupBox1.SuspendLayout
        Me.SuspendLayout
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.tcGlobalCode)
        Me.TabControl1.Controls.Add(Me.tcCodeOptions)
        resources.ApplyResources(Me.TabControl1, "TabControl1")
        Me.TabControl1.Controls.SetChildIndex(Me.tcInfo, 0)
        Me.TabControl1.Controls.SetChildIndex(Me.tcCodeOptions, 0)
        Me.TabControl1.Controls.SetChildIndex(Me.tcGlobalCode, 0)
        '
        'tcInfo
        '
        Me.tcInfo.Controls.Add(Me.lblSharedObject)
        Me.tcInfo.Controls.Add(Me.cbSharedObject)
        Me.tcInfo.Controls.Add(Me.GroupBox1)
        resources.ApplyResources(Me.tcInfo, "tcInfo")
        Me.tcInfo.Controls.SetChildIndex(Me.gpLogging, 0)
        Me.tcInfo.Controls.SetChildIndex(Me.GroupBox1, 0)
        Me.tcInfo.Controls.SetChildIndex(Me.cbSharedObject, 0)
        Me.tcInfo.Controls.SetChildIndex(Me.lblSharedObject, 0)
        '
        'gpLogging
        '
        resources.ApplyResources(Me.gpLogging, "gpLogging")
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'tcGlobalCode
        '
        Me.tcGlobalCode.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tcGlobalCode.Controls.Add(Me.mEditor)
        Me.tcGlobalCode.Controls.Add(Me.btnCheckCode)
        resources.ApplyResources(Me.tcGlobalCode, "tcGlobalCode")
        Me.tcGlobalCode.Name = "tcGlobalCode"
        '
        'mEditor
        '
        resources.ApplyResources(Me.mEditor, "mEditor")
        Me.mEditor.BackgroundColour = System.Drawing.SystemColors.Window
        Me.mEditor.Code = ""
        Me.mEditor.Name = "mEditor"
        Me.mEditor.ReadOnly = false
        '
        'btnCheckCode
        '
        resources.ApplyResources(Me.btnCheckCode, "btnCheckCode")
        Me.btnCheckCode.BackColor = System.Drawing.Color.White
        Me.btnCheckCode.Name = "btnCheckCode"
        Me.btnCheckCode.UseVisualStyleBackColor = false
        '
        'tcCodeOptions
        '
        Me.tcCodeOptions.BackColor = System.Drawing.SystemColors.Control
        Me.tcCodeOptions.Controls.Add(Me.Panel1)
        resources.ApplyResources(Me.tcCodeOptions, "tcCodeOptions")
        Me.tcCodeOptions.Name = "tcCodeOptions"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Panel1.Controls.Add(Me.lstDlls)
        Me.Panel1.Controls.Add(Me.lLanguage)
        Me.Panel1.Controls.Add(Me.btnBrowse)
        Me.Panel1.Controls.Add(Me.cmbLanguage)
        Me.Panel1.Controls.Add(Me.btnAddReference)
        Me.Panel1.Controls.Add(Me.btnRemoveNamespace)
        Me.Panel1.Controls.Add(Me.lstNamespaces)
        Me.Panel1.Controls.Add(Me.btnAddNamespace)
        Me.Panel1.Controls.Add(Me.btnRemoveRef)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'lstDlls
        '
        Me.lstDlls.AllowDrop = true
        resources.ApplyResources(Me.lstDlls, "lstDlls")
        Me.lstDlls.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.lstDlls.CurrentEditableRow = Nothing
        Me.lstDlls.FillColumn = Nothing
        Me.lstDlls.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lstDlls.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lstDlls.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182,Byte),Integer), CType(CType(202,Byte),Integer), CType(CType(234,Byte),Integer))
        Me.lstDlls.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.lstDlls.LastColumnAutoSize = true
        Me.lstDlls.MinimumColumnWidth = 200
        Me.lstDlls.Name = "lstDlls"
        Me.lstDlls.Readonly = false
        Me.lstDlls.RowHeight = 26
        Me.lstDlls.Rows.Capacity = 0
        Me.lstDlls.Sortable = false
        '
        'lLanguage
        '
        resources.ApplyResources(Me.lLanguage, "lLanguage")
        Me.lLanguage.Name = "lLanguage"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.BackColor = System.Drawing.Color.White
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = false
        '
        'cmbLanguage
        '
        resources.ApplyResources(Me.cmbLanguage, "cmbLanguage")
        Me.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLanguage.Name = "cmbLanguage"
        '
        'btnAddReference
        '
        resources.ApplyResources(Me.btnAddReference, "btnAddReference")
        Me.btnAddReference.BackColor = System.Drawing.Color.White
        Me.btnAddReference.Name = "btnAddReference"
        Me.btnAddReference.UseVisualStyleBackColor = false
        '
        'btnRemoveNamespace
        '
        resources.ApplyResources(Me.btnRemoveNamespace, "btnRemoveNamespace")
        Me.btnRemoveNamespace.BackColor = System.Drawing.Color.White
        Me.btnRemoveNamespace.Name = "btnRemoveNamespace"
        Me.btnRemoveNamespace.UseVisualStyleBackColor = false
        '
        'lstNamespaces
        '
        Me.lstNamespaces.AllowDrop = true
        resources.ApplyResources(Me.lstNamespaces, "lstNamespaces")
        Me.lstNamespaces.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.lstNamespaces.CurrentEditableRow = Nothing
        Me.lstNamespaces.FillColumn = Nothing
        Me.lstNamespaces.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lstNamespaces.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lstNamespaces.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182,Byte),Integer), CType(CType(202,Byte),Integer), CType(CType(234,Byte),Integer))
        Me.lstNamespaces.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.lstNamespaces.LastColumnAutoSize = true
        Me.lstNamespaces.MinimumColumnWidth = 200
        Me.lstNamespaces.Name = "lstNamespaces"
        Me.lstNamespaces.Readonly = false
        Me.lstNamespaces.RowHeight = 26
        Me.lstNamespaces.Rows.Capacity = 0
        Me.lstNamespaces.Sortable = false
        '
        'btnAddNamespace
        '
        resources.ApplyResources(Me.btnAddNamespace, "btnAddNamespace")
        Me.btnAddNamespace.BackColor = System.Drawing.Color.White
        Me.btnAddNamespace.Name = "btnAddNamespace"
        Me.btnAddNamespace.UseVisualStyleBackColor = false
        '
        'btnRemoveRef
        '
        resources.ApplyResources(Me.btnRemoveRef, "btnRemoveRef")
        Me.btnRemoveRef.BackColor = System.Drawing.Color.White
        Me.btnRemoveRef.Name = "btnRemoveRef"
        Me.btnRemoveRef.UseVisualStyleBackColor = false
        '
        'rdoForeground
        '
        Me.rdoForeground.ButtonHeight = 21
        Me.rdoForeground.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.rdoForeground.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.rdoForeground.FocusDiameter = 16
        Me.rdoForeground.FocusThickness = 3
        Me.rdoForeground.FocusYLocation = 9
        Me.rdoForeground.ForceFocus = true
        Me.rdoForeground.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.rdoForeground.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        resources.ApplyResources(Me.rdoForeground, "rdoForeground")
        Me.rdoForeground.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoForeground.Name = "rdoForeground"
        Me.rdoForeground.RadioButtonDiameter = 12
        Me.rdoForeground.RadioButtonThickness = 2
        Me.rdoForeground.RadioYLocation = 7
        Me.rdoForeground.StringYLocation = 1
        Me.rdoForeground.TabStop = true
        Me.rdoForeground.TextColor = System.Drawing.Color.Black
        Me.rdoForeground.UseVisualStyleBackColor = true
        '
        'rdoExclusive
        '
        Me.rdoExclusive.ButtonHeight = 21
        Me.rdoExclusive.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.rdoExclusive.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.rdoExclusive.FocusDiameter = 16
        Me.rdoExclusive.FocusThickness = 3
        Me.rdoExclusive.FocusYLocation = 9
        Me.rdoExclusive.ForceFocus = true
        Me.rdoExclusive.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.rdoExclusive.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        resources.ApplyResources(Me.rdoExclusive, "rdoExclusive")
        Me.rdoExclusive.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoExclusive.Name = "rdoExclusive"
        Me.rdoExclusive.RadioButtonDiameter = 12
        Me.rdoExclusive.RadioButtonThickness = 2
        Me.rdoExclusive.RadioYLocation = 7
        Me.rdoExclusive.StringYLocation = 1
        Me.rdoExclusive.TabStop = true
        Me.rdoExclusive.TextColor = System.Drawing.Color.Black
        Me.rdoExclusive.UseVisualStyleBackColor = true
        '
        'rdoBackground
        '
        Me.rdoBackground.ButtonHeight = 21
        Me.rdoBackground.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.rdoBackground.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.rdoBackground.FocusDiameter = 16
        Me.rdoBackground.FocusThickness = 3
        Me.rdoBackground.FocusYLocation = 9
        Me.rdoBackground.ForceFocus = true
        Me.rdoBackground.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.rdoBackground.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        resources.ApplyResources(Me.rdoBackground, "rdoBackground")
        Me.rdoBackground.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoBackground.Name = "rdoBackground"
        Me.rdoBackground.RadioButtonDiameter = 12
        Me.rdoBackground.RadioButtonThickness = 2
        Me.rdoBackground.RadioYLocation = 7
        Me.rdoBackground.StringYLocation = 1
        Me.rdoBackground.TabStop = true
        Me.rdoBackground.TextColor = System.Drawing.Color.Black
        Me.rdoBackground.UseVisualStyleBackColor = true
        '
        'lblForeground
        '
        resources.ApplyResources(Me.lblForeground, "lblForeground")
        Me.lblForeground.Name = "lblForeground"
        '
        'lblBackground
        '
        resources.ApplyResources(Me.lblBackground, "lblBackground")
        Me.lblBackground.Name = "lblBackground"
        '
        'lblExclusive
        '
        resources.ApplyResources(Me.lblExclusive, "lblExclusive")
        Me.lblExclusive.Name = "lblExclusive"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.lblForeground)
        Me.GroupBox1.Controls.Add(Me.lblExclusive)
        Me.GroupBox1.Controls.Add(Me.rdoExclusive)
        Me.GroupBox1.Controls.Add(Me.lblBackground)
        Me.GroupBox1.Controls.Add(Me.rdoBackground)
        Me.GroupBox1.Controls.Add(Me.rdoForeground)
        Me.GroupBox1.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = false
        '
        'cbSharedObject
        '
        Me.cbSharedObject.AutoCheck = false
        resources.ApplyResources(Me.cbSharedObject, "cbSharedObject")
        Me.cbSharedObject.Name = "cbSharedObject"
        Me.cbSharedObject.UseVisualStyleBackColor = true
        '
        'lblSharedObject
        '
        resources.ApplyResources(Me.lblSharedObject, "lblSharedObject")
        Me.lblSharedObject.Name = "lblSharedObject"
        '
        'frmStagePropertiesObjectInfo
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Name = "frmStagePropertiesObjectInfo"
        Me.TabControl1.ResumeLayout(false)
        Me.tcInfo.ResumeLayout(false)
        Me.tcInfo.PerformLayout
        Me.gpLogging.ResumeLayout(false)
        Me.gpLogging.PerformLayout
        CType(Me.spinLoggingRetryPeriod,System.ComponentModel.ISupportInitialize).EndInit
        CType(Me.spinLoggingAttempts,System.ComponentModel.ISupportInitialize).EndInit
        Me.tcGlobalCode.ResumeLayout(false)
        Me.tcCodeOptions.ResumeLayout(false)
        Me.Panel1.ResumeLayout(false)
        Me.Panel1.PerformLayout
        Me.GroupBox1.ResumeLayout(false)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents tcGlobalCode As System.Windows.Forms.TabPage
    Friend WithEvents tcCodeOptions As System.Windows.Forms.TabPage
    Friend WithEvents btnRemoveRef As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddReference As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lstDlls As AutomateUI.ctlListView
    Friend WithEvents btnRemoveNamespace As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAddNamespace As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lstNamespaces As AutomateUI.ctlListView
    Friend WithEvents lLanguage As System.Windows.Forms.Label
    Friend WithEvents cmbLanguage As System.Windows.Forms.ComboBox
    Friend WithEvents rdoForeground As AutomateControls.StyledRadioButton
    Friend WithEvents rdoBackground As AutomateControls.StyledRadioButton
    Friend WithEvents rdoExclusive As AutomateControls.StyledRadioButton
    Friend WithEvents lblForeground As System.Windows.Forms.Label
    Friend WithEvents lblBackground As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents lblExclusive As System.Windows.Forms.Label
    Friend WithEvents btnCheckCode As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents mEditor As AutomateUI.ctlCodeEditor
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents cbSharedObject As System.Windows.Forms.CheckBox
    Friend WithEvents lblSharedObject As System.Windows.Forms.Label
End Class
