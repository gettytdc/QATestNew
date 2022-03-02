Imports AutomateControls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCredential
    Inherits Forms.HelpButtonForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCredential))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtUserName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.lblUsername = New System.Windows.Forms.Label()
        Me.tcAccessRights = New System.Windows.Forms.TabControl()
        Me.tpRoles = New System.Windows.Forms.TabPage()
        Me.tblSecurityRolesContainer = New System.Windows.Forms.TableLayoutPanel()
        Me.lstRoles = New System.Windows.Forms.ListView()
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lblSecurityRoleDesc = New System.Windows.Forms.Label()
        Me.tpProcesses = New System.Windows.Forms.TabPage()
        Me.tblProcessesContainer = New System.Windows.Forms.TableLayoutPanel()
        Me.lstProcesses = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lblProcessesDesc = New System.Windows.Forms.Label()
        Me.tpResources = New System.Windows.Forms.TabPage()
        Me.tblResourcesContainer = New System.Windows.Forms.TableLayoutPanel()
        Me.lstResources = New System.Windows.Forms.ListView()
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.lblResourcesDesc = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.dpExpiryDate = New System.Windows.Forms.DateTimePicker()
        Me.cbInvalid = New System.Windows.Forms.CheckBox()
        Me.tcMain = New System.Windows.Forms.TabControl()
        Me.tpCredentials = New System.Windows.Forms.TabPage()
        Me.pwdPasswordConfirm = New AutomateUI.ctlAutomateSecurePassword()
        Me.lblPasswordConfirm = New System.Windows.Forms.Label()
        Me.cultureDpExpiryCheckBox = New System.Windows.Forms.CheckBox()
        Me.cultureDpExpiryDate = New CustomControls.DatePicker()
        Me.lblCredentialTypeDescription = New System.Windows.Forms.Label()
        Me.pwdPassword = New AutomateUI.ctlAutomateSecurePassword()
        Me.dgvProperties = New System.Windows.Forms.DataGridView()
        Me.Action = New System.Windows.Forms.DataGridViewImageColumn()
        Me.propertyName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.propertyValue = New AutomateUI.PasswordColumn()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.tpRights = New System.Windows.Forms.TabPage()
        Me.tblAccessRightsContainer = New System.Windows.Forms.TableLayoutPanel()
        Me.tblTextFlowContainer = New System.Windows.Forms.FlowLayoutPanel()
        Me.lbAccessRightsDescription = New System.Windows.Forms.Label()
        Me.lbLinkHelp = New System.Windows.Forms.LinkLabel()
        Me.DataGridViewImageColumn1 = New System.Windows.Forms.DataGridViewImageColumn()
        Me.objTitleBar = New AutomateControls.TitleBar()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.PasswordColumn1 = New AutomateUI.PasswordColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.lblType = New System.Windows.Forms.Label()
        Me.cmbType = New System.Windows.Forms.ComboBox()
        Me.tcAccessRights.SuspendLayout()
        Me.tpRoles.SuspendLayout()
        Me.tblSecurityRolesContainer.SuspendLayout()
        Me.tpProcesses.SuspendLayout()
        Me.tblProcessesContainer.SuspendLayout()
        Me.tpResources.SuspendLayout()
        Me.tblResourcesContainer.SuspendLayout()
        Me.tcMain.SuspendLayout()
        Me.tpCredentials.SuspendLayout()
        CType(Me.dgvProperties, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tpRights.SuspendLayout()
        Me.tblAccessRightsContainer.SuspendLayout()
        Me.tblTextFlowContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'txtUserName
        '
        resources.ApplyResources(Me.txtUserName, "txtUserName")
        Me.txtUserName.Name = "txtUserName"
        '
        'lblPassword
        '
        resources.ApplyResources(Me.lblPassword, "lblPassword")
        Me.lblPassword.Name = "lblPassword"
        '
        'lblUsername
        '
        resources.ApplyResources(Me.lblUsername, "lblUsername")
        Me.lblUsername.Name = "lblUsername"
        '
        'tcAccessRights
        '
        resources.ApplyResources(Me.tcAccessRights, "tcAccessRights")
        Me.tcAccessRights.Controls.Add(Me.tpRoles)
        Me.tcAccessRights.Controls.Add(Me.tpProcesses)
        Me.tcAccessRights.Controls.Add(Me.tpResources)
        Me.tcAccessRights.Name = "tcAccessRights"
        Me.tcAccessRights.SelectedIndex = 0
        '
        'tpRoles
        '
        Me.tpRoles.Controls.Add(Me.tblSecurityRolesContainer)
        resources.ApplyResources(Me.tpRoles, "tpRoles")
        Me.tpRoles.Name = "tpRoles"
        Me.tpRoles.UseVisualStyleBackColor = True
        '
        'tblSecurityRolesContainer
        '
        resources.ApplyResources(Me.tblSecurityRolesContainer, "tblSecurityRolesContainer")
        Me.tblSecurityRolesContainer.Controls.Add(Me.lstRoles, 0, 1)
        Me.tblSecurityRolesContainer.Controls.Add(Me.lblSecurityRoleDesc, 0, 0)
        Me.tblSecurityRolesContainer.Name = "tblSecurityRolesContainer"
        '
        'lstRoles
        '
        resources.ApplyResources(Me.lstRoles, "lstRoles")
        Me.lstRoles.CheckBoxes = True
        Me.lstRoles.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader4})
        Me.lstRoles.Name = "lstRoles"
        Me.lstRoles.UseCompatibleStateImageBehavior = False
        Me.lstRoles.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader4
        '
        resources.ApplyResources(Me.ColumnHeader4, "ColumnHeader4")
        '
        'lblSecurityRoleDesc
        '
        resources.ApplyResources(Me.lblSecurityRoleDesc, "lblSecurityRoleDesc")
        Me.lblSecurityRoleDesc.Name = "lblSecurityRoleDesc"
        '
        'tpProcesses
        '
        Me.tpProcesses.Controls.Add(Me.tblProcessesContainer)
        resources.ApplyResources(Me.tpProcesses, "tpProcesses")
        Me.tpProcesses.Name = "tpProcesses"
        Me.tpProcesses.UseVisualStyleBackColor = True
        '
        'tblProcessesContainer
        '
        resources.ApplyResources(Me.tblProcessesContainer, "tblProcessesContainer")
        Me.tblProcessesContainer.Controls.Add(Me.lstProcesses, 0, 1)
        Me.tblProcessesContainer.Controls.Add(Me.lblProcessesDesc, 0, 0)
        Me.tblProcessesContainer.Name = "tblProcessesContainer"
        '
        'lstProcesses
        '
        resources.ApplyResources(Me.lstProcesses, "lstProcesses")
        Me.lstProcesses.CheckBoxes = True
        Me.lstProcesses.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.lstProcesses.Name = "lstProcesses"
        Me.lstProcesses.UseCompatibleStateImageBehavior = False
        Me.lstProcesses.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'ColumnHeader2
        '
        resources.ApplyResources(Me.ColumnHeader2, "ColumnHeader2")
        '
        'lblProcessesDesc
        '
        resources.ApplyResources(Me.lblProcessesDesc, "lblProcessesDesc")
        Me.lblProcessesDesc.Name = "lblProcessesDesc"
        '
        'tpResources
        '
        Me.tpResources.Controls.Add(Me.tblResourcesContainer)
        resources.ApplyResources(Me.tpResources, "tpResources")
        Me.tpResources.Name = "tpResources"
        Me.tpResources.UseVisualStyleBackColor = True
        '
        'tblResourcesContainer
        '
        resources.ApplyResources(Me.tblResourcesContainer, "tblResourcesContainer")
        Me.tblResourcesContainer.Controls.Add(Me.lstResources, 0, 1)
        Me.tblResourcesContainer.Controls.Add(Me.lblResourcesDesc, 0, 0)
        Me.tblResourcesContainer.Name = "tblResourcesContainer"
        '
        'lstResources
        '
        resources.ApplyResources(Me.lstResources, "lstResources")
        Me.lstResources.CheckBoxes = True
        Me.lstResources.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader3})
        Me.lstResources.Name = "lstResources"
        Me.lstResources.UseCompatibleStateImageBehavior = False
        Me.lstResources.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader3
        '
        resources.ApplyResources(Me.ColumnHeader3, "ColumnHeader3")
        '
        'lblResourcesDesc
        '
        resources.ApplyResources(Me.lblResourcesDesc, "lblResourcesDesc")
        Me.lblResourcesDesc.Name = "lblResourcesDesc"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'dpExpiryDate
        '
        resources.ApplyResources(Me.dpExpiryDate, "dpExpiryDate")
        Me.dpExpiryDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dpExpiryDate.Name = "dpExpiryDate"
        Me.dpExpiryDate.ShowCheckBox = True
        '
        'cbInvalid
        '
        resources.ApplyResources(Me.cbInvalid, "cbInvalid")
        Me.cbInvalid.Name = "cbInvalid"
        Me.cbInvalid.UseVisualStyleBackColor = True
        '
        'tcMain
        '
        resources.ApplyResources(Me.tcMain, "tcMain")
        Me.tcMain.Controls.Add(Me.tpCredentials)
        Me.tcMain.Controls.Add(Me.tpRights)
        Me.tcMain.Name = "tcMain"
        Me.tcMain.SelectedIndex = 0
        '
        'tpCredentials
        '
        Me.tpCredentials.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpCredentials.Controls.Add(Me.pwdPasswordConfirm)
        Me.tpCredentials.Controls.Add(Me.lblPasswordConfirm)
        Me.tpCredentials.Controls.Add(Me.cultureDpExpiryCheckBox)
        Me.tpCredentials.Controls.Add(Me.cultureDpExpiryDate)
        Me.tpCredentials.Controls.Add(Me.lblCredentialTypeDescription)
        Me.tpCredentials.Controls.Add(Me.pwdPassword)
        Me.tpCredentials.Controls.Add(Me.dgvProperties)
        Me.tpCredentials.Controls.Add(Me.Label5)
        Me.tpCredentials.Controls.Add(Me.txtUserName)
        Me.tpCredentials.Controls.Add(Me.cbInvalid)
        Me.tpCredentials.Controls.Add(Me.lblUsername)
        Me.tpCredentials.Controls.Add(Me.lblPassword)
        Me.tpCredentials.Controls.Add(Me.dpExpiryDate)
        Me.tpCredentials.Controls.Add(Me.Label4)
        resources.ApplyResources(Me.tpCredentials, "tpCredentials")
        Me.tpCredentials.Name = "tpCredentials"
        '
        'pwdPasswordConfirm
        '
        Me.pwdPasswordConfirm.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.pwdPasswordConfirm, "pwdPasswordConfirm")
        Me.pwdPasswordConfirm.Name = "pwdPasswordConfirm"
        '
        'lblPasswordConfirm
        '
        resources.ApplyResources(Me.lblPasswordConfirm, "lblPasswordConfirm")
        Me.lblPasswordConfirm.Name = "lblPasswordConfirm"
        '
        'cultureDpExpiryCheckBox
        '
        resources.ApplyResources(Me.cultureDpExpiryCheckBox, "cultureDpExpiryCheckBox")
        Me.cultureDpExpiryCheckBox.Name = "cultureDpExpiryCheckBox"
        Me.cultureDpExpiryCheckBox.UseVisualStyleBackColor = True
        '
        'cultureDpExpiryDate
        '
        resources.ApplyResources(Me.cultureDpExpiryDate, "cultureDpExpiryDate")
        Me.cultureDpExpiryDate.Name = "cultureDpExpiryDate"
        '
        'lblCredentialTypeDescription
        '
        resources.ApplyResources(Me.lblCredentialTypeDescription, "lblCredentialTypeDescription")
        Me.lblCredentialTypeDescription.Name = "lblCredentialTypeDescription"
        '
        'pwdPassword
        '
        Me.pwdPassword.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.pwdPassword, "pwdPassword")
        Me.pwdPassword.Name = "pwdPassword"
        '
        'dgvProperties
        '
        Me.dgvProperties.AllowUserToResizeRows = False
        resources.ApplyResources(Me.dgvProperties, "dgvProperties")
        Me.dgvProperties.BackgroundColor = System.Drawing.Color.White
        Me.dgvProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvProperties.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Action, Me.propertyName, Me.propertyValue})
        Me.dgvProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
        Me.dgvProperties.Name = "dgvProperties"
        Me.dgvProperties.RowHeadersVisible = False
        Me.dgvProperties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'Action
        '
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.NullValue = Nothing
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.White
        Me.Action.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.Action, "Action")
        Me.Action.Name = "Action"
        Me.Action.ReadOnly = True
        Me.Action.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'propertyName
        '
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black
        Me.propertyName.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.propertyName, "propertyName")
        Me.propertyName.Name = "propertyName"
        '
        'propertyValue
        '
        Me.propertyValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black
        Me.propertyValue.DefaultCellStyle = DataGridViewCellStyle3
        Me.propertyValue.DisplayInterval = 1500
        resources.ApplyResources(Me.propertyValue, "propertyValue")
        Me.propertyValue.Name = "propertyValue"
        Me.propertyValue.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.propertyValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'tpRights
        '
        Me.tpRights.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpRights.Controls.Add(Me.tblAccessRightsContainer)
        resources.ApplyResources(Me.tpRights, "tpRights")
        Me.tpRights.Name = "tpRights"
        '
        'tblAccessRightsContainer
        '
        resources.ApplyResources(Me.tblAccessRightsContainer, "tblAccessRightsContainer")
        Me.tblAccessRightsContainer.Controls.Add(Me.tblTextFlowContainer, 0, 0)
        Me.tblAccessRightsContainer.Controls.Add(Me.tcAccessRights, 0, 1)
        Me.tblAccessRightsContainer.Name = "tblAccessRightsContainer"
        '
        'tblTextFlowContainer
        '
        resources.ApplyResources(Me.tblTextFlowContainer, "tblTextFlowContainer")
        Me.tblTextFlowContainer.Controls.Add(Me.lbAccessRightsDescription)
        Me.tblTextFlowContainer.Controls.Add(Me.lbLinkHelp)
        Me.tblTextFlowContainer.Name = "tblTextFlowContainer"
        '
        'lbAccessRightsDescription
        '
        resources.ApplyResources(Me.lbAccessRightsDescription, "lbAccessRightsDescription")
        Me.lbAccessRightsDescription.Name = "lbAccessRightsDescription"
        '
        'lbLinkHelp
        '
        Me.lbLinkHelp.ActiveLinkColor = System.Drawing.SystemColors.ControlText
        resources.ApplyResources(Me.lbLinkHelp, "lbLinkHelp")
        Me.lbLinkHelp.LinkBehavior = System.Windows.Forms.LinkBehavior.AlwaysUnderline
        Me.lbLinkHelp.LinkColor = System.Drawing.SystemColors.ControlText
        Me.lbLinkHelp.Name = "lbLinkHelp"
        Me.lbLinkHelp.TabStop = True
        '
        'DataGridViewImageColumn1
        '
        resources.ApplyResources(Me.DataGridViewImageColumn1, "DataGridViewImageColumn1")
        Me.DataGridViewImageColumn1.Name = "DataGridViewImageColumn1"
        Me.DataGridViewImageColumn1.ReadOnly = True
        Me.DataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'objTitleBar
        '
        resources.ApplyResources(Me.objTitleBar, "objTitleBar")
        Me.objTitleBar.Name = "objTitleBar"
        Me.objTitleBar.SubtitleFont = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.objTitleBar.SubtitlePosition = New System.Drawing.Point(10, 35)
        Me.objTitleBar.TabStop = False
        Me.objTitleBar.TitleFont = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        '
        'DataGridViewTextBoxColumn1
        '
        DataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black
        Me.DataGridViewTextBoxColumn1.DefaultCellStyle = DataGridViewCellStyle4
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        '
        'PasswordColumn1
        '
        Me.PasswordColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.White
        DataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.Black
        Me.PasswordColumn1.DefaultCellStyle = DataGridViewCellStyle5
        Me.PasswordColumn1.DisplayInterval = 1500
        resources.ApplyResources(Me.PasswordColumn1, "PasswordColumn1")
        Me.PasswordColumn1.Name = "PasswordColumn1"
        Me.PasswordColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.PasswordColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        '
        'lblType
        '
        resources.ApplyResources(Me.lblType, "lblType")
        Me.lblType.BackColor = System.Drawing.Color.FromArgb(CType(CType(13, Byte), Integer), CType(CType(42, Byte), Integer), CType(CType(72, Byte), Integer))
        Me.lblType.ForeColor = System.Drawing.Color.White
        Me.lblType.Name = "lblType"
        '
        'cmbType
        '
        resources.ApplyResources(Me.cmbType, "cmbType")
        Me.cmbType.DisplayMember = "Name"
        Me.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbType.FormattingEnabled = True
        Me.cmbType.Name = "cmbType"
        Me.cmbType.ValueMember = "Tag"
        '
        'frmCredential
        '
        Me.AcceptButton = Me.btnOK
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.cmbType)
        Me.Controls.Add(Me.lblType)
        Me.Controls.Add(Me.tcMain)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.objTitleBar)
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmCredential"
        Me.tcAccessRights.ResumeLayout(False)
        Me.tpRoles.ResumeLayout(False)
        Me.tblSecurityRolesContainer.ResumeLayout(False)
        Me.tpProcesses.ResumeLayout(False)
        Me.tblProcessesContainer.ResumeLayout(False)
        Me.tpResources.ResumeLayout(False)
        Me.tblResourcesContainer.ResumeLayout(False)
        Me.tcMain.ResumeLayout(False)
        Me.tpCredentials.ResumeLayout(False)
        Me.tpCredentials.PerformLayout()
        CType(Me.dgvProperties, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tpRights.ResumeLayout(False)
        Me.tblAccessRightsContainer.ResumeLayout(False)
        Me.tblTextFlowContainer.ResumeLayout(False)
        Me.tblTextFlowContainer.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Protected WithEvents objTitleBar As AutomateControls.TitleBar
    Protected WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtUserName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents lblUsername As System.Windows.Forms.Label
    Friend WithEvents tcAccessRights As System.Windows.Forms.TabControl
    Friend WithEvents tpProcesses As System.Windows.Forms.TabPage
    Friend WithEvents tpResources As System.Windows.Forms.TabPage
    Friend WithEvents tpRoles As System.Windows.Forms.TabPage
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents dpExpiryDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents cbInvalid As System.Windows.Forms.CheckBox
    Friend WithEvents tcMain As System.Windows.Forms.TabControl
    Friend WithEvents tpCredentials As System.Windows.Forms.TabPage
    Friend WithEvents tpRights As System.Windows.Forms.TabPage
    Friend WithEvents lbAccessRightsDescription As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents dgvProperties As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewImageColumn1 As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents pwdPassword As ctlAutomateSecurePassword
    Friend WithEvents Action As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents propertyName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents propertyValue As PasswordColumn
    Friend WithEvents PasswordColumn1 As PasswordColumn
    Friend WithEvents lblType As Label
    Friend WithEvents cmbType As ComboBox
    Friend WithEvents lblCredentialTypeDescription As Label
    Friend WithEvents cultureDpExpiryDate As CustomControls.DatePicker
    Friend WithEvents cultureDpExpiryCheckBox As CheckBox
    Friend WithEvents tblAccessRightsContainer As TableLayoutPanel
    Friend WithEvents tblTextFlowContainer As FlowLayoutPanel
    Friend WithEvents lbLinkHelp As LinkLabel
    Friend WithEvents tblSecurityRolesContainer As TableLayoutPanel
    Friend WithEvents lstRoles As ListView
    Friend WithEvents ColumnHeader4 As ColumnHeader
    Friend WithEvents lblSecurityRoleDesc As Label
    Friend WithEvents tblProcessesContainer As TableLayoutPanel
    Friend WithEvents lstProcesses As ListView
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents lblProcessesDesc As Label
    Friend WithEvents tblResourcesContainer As TableLayoutPanel
    Friend WithEvents lstResources As ListView
    Friend WithEvents ColumnHeader3 As ColumnHeader
    Friend WithEvents lblResourcesDesc As Label
    Friend WithEvents pwdPasswordConfirm As ctlAutomateSecurePassword
    Friend WithEvents lblPasswordConfirm As Label    
End Class
