<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlSystemSettings
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim Label3 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemSettings))
        Dim Label2 As System.Windows.Forms.Label
        Dim Label1 As System.Windows.Forms.Label
        Me.GroupBoxSysWide = New System.Windows.Forms.GroupBox()
        Me.GroupBoxOfflineHelp = New System.Windows.Forms.GroupBox()
        Me.chkOfflineHelp = New System.Windows.Forms.CheckBox()
        Me.lblOfflineHelpBaseUrl = New System.Windows.Forms.Label()
        Me.txtOfflineHelpBaseUrl = New AutomateControls.Textboxes.StyledTextBox()
        Me.GroupBoxEnvironment = New System.Windows.Forms.GroupBox()
        Me.txtEnvName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblPreview = New System.Windows.Forms.Label()
        Me.cmbForeground = New AutomateControls.ColorComboBox()
        Me.cmbBackground = New AutomateControls.ColorComboBox()
        Me.ApplicationManagerSettingsGroupBox = New System.Windows.Forms.GroupBox()
        Me.TesseractEngineLabel = New System.Windows.Forms.Label()
        Me.TesseractEngineComboBox = New System.Windows.Forms.ComboBox()
        Me.GroupBoxDB = New System.Windows.Forms.GroupBox()
        Me.chkEnableEnvironmentRecording = New System.Windows.Forms.CheckBox()
        Me.chkUnicodeLogging = New System.Windows.Forms.CheckBox()
        Me.GroupBoxResConnectivity = New System.Windows.Forms.GroupBox()
        Me.chkPreventResourceRegistration = New System.Windows.Forms.CheckBox()
        Me.cmbResourceReg = New System.Windows.Forms.ComboBox()
        Me.chkRequireSecuredResource = New System.Windows.Forms.CheckBox()
        Me.chkAllowAnonymousResources = New System.Windows.Forms.CheckBox()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.GroupBoxGeneral = New System.Windows.Forms.GroupBox()
        Me.chkHideDigitalExchange = New System.Windows.Forms.CheckBox()
        Me.numDefaultStageWarning = New AutomateControls.StyledNumericUpDown()
        Me.lblWarningThreshold = New System.Windows.Forms.Label()
        Me.chkExceptionScreenshot = New System.Windows.Forms.CheckBox()
        Me.chkAllowPasswordPasting = New System.Windows.Forms.CheckBox()
        Me.chkBackUp = New System.Windows.Forms.CheckBox()
        Me.chkEnforceSummaries = New System.Windows.Forms.CheckBox()
        Me.cmbBackUp = New System.Windows.Forms.ComboBox()
        Me.GroupBoxLocal = New System.Windows.Forms.GroupBox()
        Me.chkStartProcEngine = New System.Windows.Forms.CheckBox()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.BrowserPluginLegacyPortToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Label3 = New System.Windows.Forms.Label()
        Label2 = New System.Windows.Forms.Label()
        Label1 = New System.Windows.Forms.Label()
        Me.GroupBoxSysWide.SuspendLayout()
        Me.GroupBoxOfflineHelp.SuspendLayout()
        Me.GroupBoxEnvironment.SuspendLayout()
        Me.ApplicationManagerSettingsGroupBox.SuspendLayout()
        Me.GroupBoxDB.SuspendLayout()
        Me.GroupBoxResConnectivity.SuspendLayout()
        Me.GroupBoxGeneral.SuspendLayout()
        CType(Me.numDefaultStageWarning, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBoxLocal.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.Name = "Label3"
        '
        'Label2
        '
        resources.ApplyResources(Label2, "Label2")
        Label2.Name = "Label2"
        '
        'Label1
        '
        resources.ApplyResources(Label1, "Label1")
        Label1.Name = "Label1"
        '
        'GroupBoxSysWide
        '
        Me.GroupBoxSysWide.Controls.Add(Me.GroupBoxOfflineHelp)
        Me.GroupBoxSysWide.Controls.Add(Me.GroupBoxEnvironment)
        Me.GroupBoxSysWide.Controls.Add(Me.ApplicationManagerSettingsGroupBox)
        Me.GroupBoxSysWide.Controls.Add(Me.GroupBoxDB)
        Me.GroupBoxSysWide.Controls.Add(Me.GroupBoxResConnectivity)
        Me.GroupBoxSysWide.Controls.Add(Me.GroupBoxGeneral)
        resources.ApplyResources(Me.GroupBoxSysWide, "GroupBoxSysWide")
        Me.GroupBoxSysWide.Name = "GroupBoxSysWide"
        Me.GroupBoxSysWide.TabStop = False
        '
        'GroupBoxOfflineHelp
        '
        Me.GroupBoxOfflineHelp.Controls.Add(Me.chkOfflineHelp)
        Me.GroupBoxOfflineHelp.Controls.Add(Me.lblOfflineHelpBaseUrl)
        Me.GroupBoxOfflineHelp.Controls.Add(Me.txtOfflineHelpBaseUrl)
        resources.ApplyResources(Me.GroupBoxOfflineHelp, "GroupBoxOfflineHelp")
        Me.GroupBoxOfflineHelp.Name = "GroupBoxOfflineHelp"
        Me.GroupBoxOfflineHelp.TabStop = False
        '
        'chkOfflineHelp
        '
        resources.ApplyResources(Me.chkOfflineHelp, "chkOfflineHelp")
        Me.chkOfflineHelp.ForeColor = System.Drawing.Color.Black
        Me.chkOfflineHelp.Name = "chkOfflineHelp"
        '
        'lblOfflineHelpBaseUrl
        '
        Me.lblOfflineHelpBaseUrl.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.lblOfflineHelpBaseUrl, "lblOfflineHelpBaseUrl")
        Me.lblOfflineHelpBaseUrl.Name = "lblOfflineHelpBaseUrl"
        '
        'txtOfflineHelpBaseUrl
        '
        Me.txtOfflineHelpBaseUrl.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtOfflineHelpBaseUrl, "txtOfflineHelpBaseUrl")
        Me.txtOfflineHelpBaseUrl.Name = "txtOfflineHelpBaseUrl"
        '
        'GroupBoxEnvironment
        '
        Me.GroupBoxEnvironment.Controls.Add(Me.txtEnvName)
        Me.GroupBoxEnvironment.Controls.Add(Me.Label4)
        Me.GroupBoxEnvironment.Controls.Add(Me.btnApply)
        Me.GroupBoxEnvironment.Controls.Add(Me.lblPreview)
        Me.GroupBoxEnvironment.Controls.Add(Me.cmbForeground)
        Me.GroupBoxEnvironment.Controls.Add(Me.cmbBackground)
        Me.GroupBoxEnvironment.Controls.Add(Label3)
        Me.GroupBoxEnvironment.Controls.Add(Label2)
        Me.GroupBoxEnvironment.Controls.Add(Label1)
        resources.ApplyResources(Me.GroupBoxEnvironment, "GroupBoxEnvironment")
        Me.GroupBoxEnvironment.Name = "GroupBoxEnvironment"
        Me.GroupBoxEnvironment.TabStop = False
        '
        'txtEnvName
        '
        Me.txtEnvName.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtEnvName, "txtEnvName")
        Me.txtEnvName.Name = "txtEnvName"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'btnApply
        '
        Me.btnApply.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = False
        '
        'lblPreview
        '
        Me.lblPreview.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.lblPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.lblPreview, "lblPreview")
        Me.lblPreview.ForeColor = System.Drawing.Color.White
        Me.lblPreview.Name = "lblPreview"
        '
        'cmbForeground
        '
        Me.cmbForeground.FormattingEnabled = True
        resources.ApplyResources(Me.cmbForeground, "cmbForeground")
        Me.cmbForeground.Name = "cmbForeground"
        Me.cmbForeground.SelectedColor = System.Drawing.Color.Empty
        '
        'cmbBackground
        '
        Me.cmbBackground.FormattingEnabled = True
        resources.ApplyResources(Me.cmbBackground, "cmbBackground")
        Me.cmbBackground.Name = "cmbBackground"
        Me.cmbBackground.SelectedColor = System.Drawing.Color.Empty
        '
        'ApplicationManagerSettingsGroupBox
        '
        Me.ApplicationManagerSettingsGroupBox.Controls.Add(Me.TesseractEngineLabel)
        Me.ApplicationManagerSettingsGroupBox.Controls.Add(Me.TesseractEngineComboBox)
        resources.ApplyResources(Me.ApplicationManagerSettingsGroupBox, "ApplicationManagerSettingsGroupBox")
        Me.ApplicationManagerSettingsGroupBox.Name = "ApplicationManagerSettingsGroupBox"
        Me.ApplicationManagerSettingsGroupBox.TabStop = False
        '
        'TesseractEngineLabel
        '
        Me.TesseractEngineLabel.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.TesseractEngineLabel, "TesseractEngineLabel")
        Me.TesseractEngineLabel.Name = "TesseractEngineLabel"
        '
        'TesseractEngineComboBox
        '
        resources.ApplyResources(Me.TesseractEngineComboBox, "TesseractEngineComboBox")
        Me.TesseractEngineComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.TesseractEngineComboBox.FormattingEnabled = True
        Me.TesseractEngineComboBox.Items.AddRange(New Object() {resources.GetString("TesseractEngineComboBox.Items"), resources.GetString("TesseractEngineComboBox.Items1"), resources.GetString("TesseractEngineComboBox.Items2"), resources.GetString("TesseractEngineComboBox.Items3")})
        Me.TesseractEngineComboBox.Name = "TesseractEngineComboBox"
        '
        'GroupBoxDB
        '
        Me.GroupBoxDB.Controls.Add(Me.chkEnableEnvironmentRecording)
        Me.GroupBoxDB.Controls.Add(Me.chkUnicodeLogging)
        resources.ApplyResources(Me.GroupBoxDB, "GroupBoxDB")
        Me.GroupBoxDB.Name = "GroupBoxDB"
        Me.GroupBoxDB.TabStop = False
        '
        'chkEnableEnvironmentRecording
        '
        resources.ApplyResources(Me.chkEnableEnvironmentRecording, "chkEnableEnvironmentRecording")
        Me.chkEnableEnvironmentRecording.Name = "chkEnableEnvironmentRecording"
        Me.chkEnableEnvironmentRecording.UseVisualStyleBackColor = True
        '
        'chkUnicodeLogging
        '
        resources.ApplyResources(Me.chkUnicodeLogging, "chkUnicodeLogging")
        Me.chkUnicodeLogging.Name = "chkUnicodeLogging"
        Me.chkUnicodeLogging.UseVisualStyleBackColor = True
        '
        'GroupBoxResConnectivity
        '
        Me.GroupBoxResConnectivity.Controls.Add(Me.chkPreventResourceRegistration)
        Me.GroupBoxResConnectivity.Controls.Add(Me.cmbResourceReg)
        Me.GroupBoxResConnectivity.Controls.Add(Me.chkRequireSecuredResource)
        Me.GroupBoxResConnectivity.Controls.Add(Me.chkAllowAnonymousResources)
        Me.GroupBoxResConnectivity.Controls.Add(Me.Label14)
        resources.ApplyResources(Me.GroupBoxResConnectivity, "GroupBoxResConnectivity")
        Me.GroupBoxResConnectivity.Name = "GroupBoxResConnectivity"
        Me.GroupBoxResConnectivity.TabStop = False
        '
        'chkPreventResourceRegistration
        '
        resources.ApplyResources(Me.chkPreventResourceRegistration, "chkPreventResourceRegistration")
        Me.chkPreventResourceRegistration.Name = "chkPreventResourceRegistration"
        Me.chkPreventResourceRegistration.UseVisualStyleBackColor = True
        '
        'cmbResourceReg
        '
        resources.ApplyResources(Me.cmbResourceReg, "cmbResourceReg")
        Me.cmbResourceReg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbResourceReg.FormattingEnabled = True
        Me.cmbResourceReg.Items.AddRange(New Object() {resources.GetString("cmbResourceReg.Items"), resources.GetString("cmbResourceReg.Items1"), resources.GetString("cmbResourceReg.Items2")})
        Me.cmbResourceReg.Name = "cmbResourceReg"
        '
        'chkRequireSecuredResource
        '
        resources.ApplyResources(Me.chkRequireSecuredResource, "chkRequireSecuredResource")
        Me.chkRequireSecuredResource.Name = "chkRequireSecuredResource"
        Me.chkRequireSecuredResource.UseVisualStyleBackColor = True
        '
        'chkAllowAnonymousResources
        '
        resources.ApplyResources(Me.chkAllowAnonymousResources, "chkAllowAnonymousResources")
        Me.chkAllowAnonymousResources.Name = "chkAllowAnonymousResources"
        Me.chkAllowAnonymousResources.UseVisualStyleBackColor = True
        '
        'Label14
        '
        Me.Label14.ForeColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.Label14, "Label14")
        Me.Label14.Name = "Label14"
        '
        'GroupBoxGeneral
        '
        Me.GroupBoxGeneral.Controls.Add(Me.chkHideDigitalExchange)
        Me.GroupBoxGeneral.Controls.Add(Me.numDefaultStageWarning)
        Me.GroupBoxGeneral.Controls.Add(Me.lblWarningThreshold)
        Me.GroupBoxGeneral.Controls.Add(Me.chkExceptionScreenshot)
        Me.GroupBoxGeneral.Controls.Add(Me.chkAllowPasswordPasting)
        Me.GroupBoxGeneral.Controls.Add(Me.chkBackUp)
        Me.GroupBoxGeneral.Controls.Add(Me.chkEnforceSummaries)
        Me.GroupBoxGeneral.Controls.Add(Me.cmbBackUp)
        resources.ApplyResources(Me.GroupBoxGeneral, "GroupBoxGeneral")
        Me.GroupBoxGeneral.Name = "GroupBoxGeneral"
        Me.GroupBoxGeneral.TabStop = False
        '
        'chkHideDigitalExchange
        '
        resources.ApplyResources(Me.chkHideDigitalExchange, "chkHideDigitalExchange")
        Me.chkHideDigitalExchange.ForeColor = System.Drawing.Color.Black
        Me.chkHideDigitalExchange.Name = "chkHideDigitalExchange"
        '
        'numDefaultStageWarning
        '
        resources.ApplyResources(Me.numDefaultStageWarning, "numDefaultStageWarning")
        Me.numDefaultStageWarning.Maximum = New Decimal(New Integer() {3600, 0, 0, 0})
        Me.numDefaultStageWarning.Name = "numDefaultStageWarning"
        '
        'lblWarningThreshold
        '
        resources.ApplyResources(Me.lblWarningThreshold, "lblWarningThreshold")
        Me.lblWarningThreshold.Name = "lblWarningThreshold"
        '
        'chkExceptionScreenshot
        '
        resources.ApplyResources(Me.chkExceptionScreenshot, "chkExceptionScreenshot")
        Me.chkExceptionScreenshot.ForeColor = System.Drawing.Color.Black
        Me.chkExceptionScreenshot.Name = "chkExceptionScreenshot"
        '
        'chkAllowPasswordPasting
        '
        resources.ApplyResources(Me.chkAllowPasswordPasting, "chkAllowPasswordPasting")
        Me.chkAllowPasswordPasting.ForeColor = System.Drawing.Color.Black
        Me.chkAllowPasswordPasting.Name = "chkAllowPasswordPasting"
        '
        'chkBackUp
        '
        resources.ApplyResources(Me.chkBackUp, "chkBackUp")
        Me.chkBackUp.ForeColor = System.Drawing.Color.Black
        Me.chkBackUp.Name = "chkBackUp"
        '
        'chkEnforceSummaries
        '
        resources.ApplyResources(Me.chkEnforceSummaries, "chkEnforceSummaries")
        Me.chkEnforceSummaries.ForeColor = System.Drawing.Color.Black
        Me.chkEnforceSummaries.Name = "chkEnforceSummaries"
        '
        'cmbBackUp
        '
        resources.ApplyResources(Me.cmbBackUp, "cmbBackUp")
        Me.cmbBackUp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBackUp.Items.AddRange(New Object() {resources.GetString("cmbBackUp.Items"), resources.GetString("cmbBackUp.Items1"), resources.GetString("cmbBackUp.Items2"), resources.GetString("cmbBackUp.Items3"), resources.GetString("cmbBackUp.Items4"), resources.GetString("cmbBackUp.Items5"), resources.GetString("cmbBackUp.Items6"), resources.GetString("cmbBackUp.Items7"), resources.GetString("cmbBackUp.Items8"), resources.GetString("cmbBackUp.Items9"), resources.GetString("cmbBackUp.Items10")})
        Me.cmbBackUp.Name = "cmbBackUp"
        '
        'GroupBoxLocal
        '
        Me.GroupBoxLocal.Controls.Add(Me.chkStartProcEngine)
        resources.ApplyResources(Me.GroupBoxLocal, "GroupBoxLocal")
        Me.GroupBoxLocal.Name = "GroupBoxLocal"
        Me.GroupBoxLocal.TabStop = False
        '
        'chkStartProcEngine
        '
        resources.ApplyResources(Me.chkStartProcEngine, "chkStartProcEngine")
        Me.chkStartProcEngine.ForeColor = System.Drawing.Color.Black
        Me.chkStartProcEngine.Name = "chkStartProcEngine"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.GroupBoxLocal, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.GroupBoxSysWide, 0, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'BrowserPluginLegacyPortToolTip
        '
        Me.BrowserPluginLegacyPortToolTip.AutomaticDelay = 0
        Me.BrowserPluginLegacyPortToolTip.ShowAlways = True
        '
        'ctlSystemSettings
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlSystemSettings"
        Me.GroupBoxSysWide.ResumeLayout(False)
        Me.GroupBoxOfflineHelp.ResumeLayout(False)
        Me.GroupBoxOfflineHelp.PerformLayout
        Me.GroupBoxEnvironment.ResumeLayout(false)
        Me.GroupBoxEnvironment.PerformLayout
        Me.ApplicationManagerSettingsGroupBox.ResumeLayout(false)
        Me.GroupBoxDB.ResumeLayout(false)
        Me.GroupBoxDB.PerformLayout
        Me.GroupBoxResConnectivity.ResumeLayout(false)
        Me.GroupBoxResConnectivity.PerformLayout
        Me.GroupBoxGeneral.ResumeLayout(false)
        Me.GroupBoxGeneral.PerformLayout
        CType(Me.numDefaultStageWarning,System.ComponentModel.ISupportInitialize).EndInit
        Me.GroupBoxLocal.ResumeLayout(false)
        Me.GroupBoxLocal.PerformLayout
        Me.TableLayoutPanel1.ResumeLayout(false)
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents GroupBoxSysWide As GroupBox
    Friend WithEvents GroupBoxEnvironment As GroupBox
    Friend WithEvents txtEnvName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label4 As Label
    Private WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents lblPreview As Label
    Private WithEvents cmbForeground As AutomateControls.ColorComboBox
    Private WithEvents cmbBackground As AutomateControls.ColorComboBox
    Friend WithEvents ApplicationManagerSettingsGroupBox As GroupBox
    Friend WithEvents TesseractEngineLabel As Label
    Friend WithEvents TesseractEngineComboBox As ComboBox
    Friend WithEvents GroupBoxDB As GroupBox
    Friend WithEvents chkUnicodeLogging As CheckBox
    Friend WithEvents GroupBoxResConnectivity As GroupBox
    Friend WithEvents chkPreventResourceRegistration As CheckBox
    Friend WithEvents cmbResourceReg As ComboBox
    Friend WithEvents chkRequireSecuredResource As CheckBox
    Friend WithEvents chkAllowAnonymousResources As CheckBox
    Friend WithEvents Label14 As Label
    Friend WithEvents GroupBoxGeneral As GroupBox
    Friend WithEvents lblWarningThreshold As Label
    Private WithEvents chkExceptionScreenshot As CheckBox
    Private WithEvents chkAllowPasswordPasting As CheckBox
    Private WithEvents chkBackUp As CheckBox
    Private WithEvents chkEnforceSummaries As CheckBox
    Private WithEvents cmbBackUp As ComboBox
    Friend WithEvents GroupBoxLocal As GroupBox
    Private WithEvents chkStartProcEngine As CheckBox
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents GroupBoxOfflineHelp As GroupBox
    Friend WithEvents chkOfflineHelp As CheckBox
    Friend WithEvents lblOfflineHelpBaseUrl As Label
    Friend WithEvents txtOfflineHelpBaseUrl As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents chkHideDigitalExchange As CheckBox
    Friend WithEvents chkEnableEnvironmentRecording As CheckBox
    Friend WithEvents BrowserPluginLegacyPortToolTip As ToolTip
    Friend WithEvents numDefaultStageWarning As AutomateControls.StyledNumericUpDown
End Class
