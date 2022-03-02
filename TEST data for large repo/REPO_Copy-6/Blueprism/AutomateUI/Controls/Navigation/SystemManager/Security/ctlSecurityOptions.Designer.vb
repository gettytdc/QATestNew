<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlSecurityOptions
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSecurityOptions))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lblContain = New System.Windows.Forms.Label()
        Me.chkUpperCase = New System.Windows.Forms.CheckBox()
        Me.chkLowerCase = New System.Windows.Forms.CheckBox()
        Me.chkDigits = New System.Windows.Forms.CheckBox()
        Me.txtAdditional = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblAdditional = New System.Windows.Forms.Label()
        Me.chkBrackets = New System.Windows.Forms.CheckBox()
        Me.chkSpecial = New System.Windows.Forms.CheckBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.chkNoRepeatsDays = New System.Windows.Forms.CheckBox()
        Me.updnLoginAttempts = New AutomateControls.StyledNumericUpDown()
        Me.chkNoRepeats = New System.Windows.Forms.CheckBox()
        Me.chkUseLoginAttempts = New System.Windows.Forms.CheckBox()
        Me.updnNumberOfDays = New AutomateControls.StyledNumericUpDown()
        Me.chkPasswordMinLength = New System.Windows.Forms.CheckBox()
        Me.lblPrevious = New System.Windows.Forms.Label()
        Me.updnNumberOfRepeats = New AutomateControls.StyledNumericUpDown()
        Me.updnPasswordMinLength = New AutomateControls.StyledNumericUpDown()
        Me.gpLoginOptions = New System.Windows.Forms.GroupBox()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.chkShowUsersOnLogin = New System.Windows.Forms.CheckBox()
        Me.chkSignIn = New System.Windows.Forms.CheckBox()
        Me.cmbSignIn = New System.Windows.Forms.ComboBox()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.chkExpiryWarning = New System.Windows.Forms.CheckBox()
        Me.cmbExpiryWarningInterval = New System.Windows.Forms.ComboBox()
        Me.gbActiveDirectoryAuthentication = New System.Windows.Forms.GroupBox()
        Me.chkEnableActiveDirectoryAuth = New System.Windows.Forms.CheckBox()
        Me.PanelAuthenticationServer = New System.Windows.Forms.Panel()
        Me.gbAuthServer = New System.Windows.Forms.GroupBox()
        Me.lblAuthServerUrl = New System.Windows.Forms.Label()
        Me.cmbSelectCredential = New System.Windows.Forms.ComboBox()
        Me.lblSelectCredential = New System.Windows.Forms.Label()
        Me.txtAuthServerUrl = New System.Windows.Forms.TextBox()
        Me.chkUseAuthServer = New System.Windows.Forms.CheckBox()
        Me.btnApply = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.TableLayoutPanel1.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        CType(Me.updnLoginAttempts, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.updnNumberOfDays, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.updnNumberOfRepeats, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.updnPasswordMinLength, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gpLoginOptions.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.gbActiveDirectoryAuthentication.SuspendLayout()
        Me.PanelAuthenticationServer.SuspendLayout()
        Me.gbAuthServer.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.btnApply, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.GroupBox1, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.gpLoginOptions, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.gbActiveDirectoryAuthentication, 0, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.PanelAuthenticationServer, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.TableLayoutPanel2)
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'TableLayoutPanel2
        '
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.Panel1, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Panel2, 1, 0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.lblContain)
        Me.Panel1.Controls.Add(Me.chkUpperCase)
        Me.Panel1.Controls.Add(Me.chkLowerCase)
        Me.Panel1.Controls.Add(Me.chkDigits)
        Me.Panel1.Controls.Add(Me.txtAdditional)
        Me.Panel1.Controls.Add(Me.lblAdditional)
        Me.Panel1.Controls.Add(Me.chkBrackets)
        Me.Panel1.Controls.Add(Me.chkSpecial)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'lblContain
        '
        resources.ApplyResources(Me.lblContain, "lblContain")
        Me.lblContain.ForeColor = System.Drawing.Color.Black
        Me.lblContain.Name = "lblContain"
        '
        'chkUpperCase
        '
        resources.ApplyResources(Me.chkUpperCase, "chkUpperCase")
        Me.chkUpperCase.ForeColor = System.Drawing.Color.Black
        Me.chkUpperCase.Name = "chkUpperCase"
        '
        'chkLowerCase
        '
        resources.ApplyResources(Me.chkLowerCase, "chkLowerCase")
        Me.chkLowerCase.ForeColor = System.Drawing.Color.Black
        Me.chkLowerCase.Name = "chkLowerCase"
        '
        'chkDigits
        '
        resources.ApplyResources(Me.chkDigits, "chkDigits")
        Me.chkDigits.ForeColor = System.Drawing.Color.Black
        Me.chkDigits.Name = "chkDigits"
        '
        'txtAdditional
        '
        Me.txtAdditional.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtAdditional, "txtAdditional")
        Me.txtAdditional.Name = "txtAdditional"
        '
        'lblAdditional
        '
        resources.ApplyResources(Me.lblAdditional, "lblAdditional")
        Me.lblAdditional.ForeColor = System.Drawing.Color.Black
        Me.lblAdditional.Name = "lblAdditional"
        '
        'chkBrackets
        '
        resources.ApplyResources(Me.chkBrackets, "chkBrackets")
        Me.chkBrackets.ForeColor = System.Drawing.Color.Black
        Me.chkBrackets.Name = "chkBrackets"
        '
        'chkSpecial
        '
        resources.ApplyResources(Me.chkSpecial, "chkSpecial")
        Me.chkSpecial.ForeColor = System.Drawing.Color.Black
        Me.chkSpecial.Name = "chkSpecial"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.chkNoRepeatsDays)
        Me.Panel2.Controls.Add(Me.updnLoginAttempts)
        Me.Panel2.Controls.Add(Me.chkNoRepeats)
        Me.Panel2.Controls.Add(Me.chkUseLoginAttempts)
        Me.Panel2.Controls.Add(Me.updnNumberOfDays)
        Me.Panel2.Controls.Add(Me.chkPasswordMinLength)
        Me.Panel2.Controls.Add(Me.lblPrevious)
        Me.Panel2.Controls.Add(Me.updnNumberOfRepeats)
        Me.Panel2.Controls.Add(Me.updnPasswordMinLength)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'chkNoRepeatsDays
        '
        resources.ApplyResources(Me.chkNoRepeatsDays, "chkNoRepeatsDays")
        Me.chkNoRepeatsDays.Name = "chkNoRepeatsDays"
        Me.chkNoRepeatsDays.UseVisualStyleBackColor = True
        '
        'updnLoginAttempts
        '
        resources.ApplyResources(Me.updnLoginAttempts, "updnLoginAttempts")
        Me.updnLoginAttempts.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnLoginAttempts.Name = "updnLoginAttempts"
        Me.updnLoginAttempts.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'chkNoRepeats
        '
        resources.ApplyResources(Me.chkNoRepeats, "chkNoRepeats")
        Me.chkNoRepeats.Name = "chkNoRepeats"
        Me.chkNoRepeats.UseVisualStyleBackColor = True
        '
        'chkUseLoginAttempts
        '
        Me.chkUseLoginAttempts.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.chkUseLoginAttempts, "chkUseLoginAttempts")
        Me.chkUseLoginAttempts.Name = "chkUseLoginAttempts"
        Me.chkUseLoginAttempts.UseVisualStyleBackColor = False
        '
        'updnNumberOfDays
        '
        resources.ApplyResources(Me.updnNumberOfDays, "updnNumberOfDays")
        Me.updnNumberOfDays.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnNumberOfDays.Name = "updnNumberOfDays"
        Me.updnNumberOfDays.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'chkPasswordMinLength
        '
        resources.ApplyResources(Me.chkPasswordMinLength, "chkPasswordMinLength")
        Me.chkPasswordMinLength.ForeColor = System.Drawing.Color.Black
        Me.chkPasswordMinLength.Name = "chkPasswordMinLength"
        '
        'lblPrevious
        '
        resources.ApplyResources(Me.lblPrevious, "lblPrevious")
        Me.lblPrevious.ForeColor = System.Drawing.Color.Black
        Me.lblPrevious.Name = "lblPrevious"
        '
        'updnNumberOfRepeats
        '
        resources.ApplyResources(Me.updnNumberOfRepeats, "updnNumberOfRepeats")
        Me.updnNumberOfRepeats.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnNumberOfRepeats.Name = "updnNumberOfRepeats"
        Me.updnNumberOfRepeats.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'updnPasswordMinLength
        '
        resources.ApplyResources(Me.updnPasswordMinLength, "updnPasswordMinLength")
        Me.updnPasswordMinLength.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.updnPasswordMinLength.Name = "updnPasswordMinLength"
        Me.updnPasswordMinLength.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'gpLoginOptions
        '
        Me.gpLoginOptions.Controls.Add(Me.TableLayoutPanel3)
        resources.ApplyResources(Me.gpLoginOptions, "gpLoginOptions")
        Me.gpLoginOptions.Name = "gpLoginOptions"
        Me.gpLoginOptions.TabStop = False
        '
        'TableLayoutPanel3
        '
        resources.ApplyResources(Me.TableLayoutPanel3, "TableLayoutPanel3")
        Me.TableLayoutPanel3.Controls.Add(Me.Panel3, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.Panel4, 1, 0)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.chkShowUsersOnLogin)
        Me.Panel3.Controls.Add(Me.chkSignIn)
        Me.Panel3.Controls.Add(Me.cmbSignIn)
        resources.ApplyResources(Me.Panel3, "Panel3")
        Me.Panel3.Name = "Panel3"
        '
        'chkShowUsersOnLogin
        '
        resources.ApplyResources(Me.chkShowUsersOnLogin, "chkShowUsersOnLogin")
        Me.chkShowUsersOnLogin.Name = "chkShowUsersOnLogin"
        '
        'chkSignIn
        '
        resources.ApplyResources(Me.chkSignIn, "chkSignIn")
        Me.chkSignIn.BackColor = System.Drawing.Color.Transparent
        Me.chkSignIn.Name = "chkSignIn"
        Me.chkSignIn.UseVisualStyleBackColor = False
        '
        'cmbSignIn
        '
        resources.ApplyResources(Me.cmbSignIn, "cmbSignIn")
        Me.cmbSignIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSignIn.Items.AddRange(New Object() {resources.GetString("cmbSignIn.Items"), resources.GetString("cmbSignIn.Items1"), resources.GetString("cmbSignIn.Items2")})
        Me.cmbSignIn.Name = "cmbSignIn"
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.chkExpiryWarning)
        Me.Panel4.Controls.Add(Me.cmbExpiryWarningInterval)
        resources.ApplyResources(Me.Panel4, "Panel4")
        Me.Panel4.Name = "Panel4"
        '
        'chkExpiryWarning
        '
        resources.ApplyResources(Me.chkExpiryWarning, "chkExpiryWarning")
        Me.chkExpiryWarning.BackColor = System.Drawing.Color.Transparent
        Me.chkExpiryWarning.Name = "chkExpiryWarning"
        Me.chkExpiryWarning.UseVisualStyleBackColor = False
        '
        'cmbExpiryWarningInterval
        '
        resources.ApplyResources(Me.cmbExpiryWarningInterval, "cmbExpiryWarningInterval")
        Me.cmbExpiryWarningInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbExpiryWarningInterval.Name = "cmbExpiryWarningInterval"
        '
        'gbActiveDirectoryAuthentication
        '
        Me.gbActiveDirectoryAuthentication.Controls.Add(Me.chkEnableActiveDirectoryAuth)
        resources.ApplyResources(Me.gbActiveDirectoryAuthentication, "gbActiveDirectoryAuthentication")
        Me.gbActiveDirectoryAuthentication.Name = "gbActiveDirectoryAuthentication"
        Me.gbActiveDirectoryAuthentication.TabStop = False
        '
        'chkEnableActiveDirectoryAuth
        '
        resources.ApplyResources(Me.chkEnableActiveDirectoryAuth, "chkEnableActiveDirectoryAuth")
        Me.chkEnableActiveDirectoryAuth.Name = "chkEnableActiveDirectoryAuth"
        '
        'PanelAuthenticationServer
        '
        resources.ApplyResources(Me.PanelAuthenticationServer, "PanelAuthenticationServer")
        Me.PanelAuthenticationServer.Controls.Add(Me.gbAuthServer)
        Me.PanelAuthenticationServer.Name = "PanelAuthenticationServer"
        '
        'gbAuthServer
        '
        resources.ApplyResources(Me.gbAuthServer, "gbAuthServer")
        Me.gbAuthServer.Controls.Add(Me.lblAuthServerUrl)
        Me.gbAuthServer.Controls.Add(Me.cmbSelectCredential)
        Me.gbAuthServer.Controls.Add(Me.lblSelectCredential)
        Me.gbAuthServer.Controls.Add(Me.txtAuthServerUrl)
        Me.gbAuthServer.Controls.Add(Me.chkUseAuthServer)
        Me.gbAuthServer.Name = "gbAuthServer"
        Me.gbAuthServer.TabStop = False
        '
        'lblAuthServerUrl
        '
        resources.ApplyResources(Me.lblAuthServerUrl, "lblAuthServerUrl")
        Me.lblAuthServerUrl.Name = "lblAuthServerUrl"
        '
        'cmbSelectCredential
        '
        resources.ApplyResources(Me.cmbSelectCredential, "cmbSelectCredential")
        Me.cmbSelectCredential.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSelectCredential.Name = "cmbSelectCredential"
        '
        'lblSelectCredential
        '
        resources.ApplyResources(Me.lblSelectCredential, "lblSelectCredential")
        Me.lblSelectCredential.Name = "lblSelectCredential"
        '
        'txtAuthServerUrl
        '
        resources.ApplyResources(Me.txtAuthServerUrl, "txtAuthServerUrl")
        Me.txtAuthServerUrl.Name = "txtAuthServerUrl"
        '
        'chkUseAuthServer
        '
        resources.ApplyResources(Me.chkUseAuthServer, "chkUseAuthServer")
        Me.chkUseAuthServer.Name = "chkUseAuthServer"
        Me.chkUseAuthServer.UseVisualStyleBackColor = True
        '
        'btnApply
        '
        resources.ApplyResources(Me.btnApply, "btnApply")
        Me.btnApply.BackColor = System.Drawing.Color.White
        Me.btnApply.Name = "btnApply"
        Me.btnApply.UseVisualStyleBackColor = False
        '
        'ctlSecurityOptions
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlSecurityOptions"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        CType(Me.updnLoginAttempts, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.updnNumberOfDays, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.updnNumberOfRepeats, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.updnPasswordMinLength, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gpLoginOptions.ResumeLayout(False)
        Me.TableLayoutPanel3.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.Panel4.ResumeLayout(False)
        Me.Panel4.PerformLayout()
        Me.gbActiveDirectoryAuthentication.ResumeLayout(False)
        Me.gbActiveDirectoryAuthentication.PerformLayout()
        Me.PanelAuthenticationServer.ResumeLayout(False)
        Me.gbAuthServer.ResumeLayout(False)
        Me.gbAuthServer.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Private WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents Panel1 As System.Windows.Forms.Panel
    Private WithEvents lblContain As System.Windows.Forms.Label
    Private WithEvents chkUpperCase As System.Windows.Forms.CheckBox
    Private WithEvents chkLowerCase As System.Windows.Forms.CheckBox
    Private WithEvents chkDigits As System.Windows.Forms.CheckBox
    Private WithEvents txtAdditional As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents lblAdditional As System.Windows.Forms.Label
    Private WithEvents chkBrackets As System.Windows.Forms.CheckBox
    Private WithEvents chkSpecial As System.Windows.Forms.CheckBox
    Private WithEvents Panel2 As System.Windows.Forms.Panel
    Private WithEvents updnNumberOfDays As AutomateControls.StyledNumericUpDown
    Private WithEvents chkPasswordMinLength As System.Windows.Forms.CheckBox
    Private WithEvents lblPrevious As System.Windows.Forms.Label
    Private WithEvents updnNumberOfRepeats As AutomateControls.StyledNumericUpDown
    Private WithEvents updnPasswordMinLength As AutomateControls.StyledNumericUpDown
    Private WithEvents gpLoginOptions As System.Windows.Forms.GroupBox
    Private WithEvents TableLayoutPanel3 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents Panel3 As System.Windows.Forms.Panel
    Private WithEvents chkShowUsersOnLogin As System.Windows.Forms.CheckBox
    Private WithEvents chkSignIn As System.Windows.Forms.CheckBox
    Private WithEvents cmbSignIn As System.Windows.Forms.ComboBox
    Private WithEvents Panel4 As System.Windows.Forms.Panel
    Private WithEvents chkExpiryWarning As System.Windows.Forms.CheckBox
    Private WithEvents updnLoginAttempts As AutomateControls.StyledNumericUpDown
    Private WithEvents chkUseLoginAttempts As System.Windows.Forms.CheckBox
    Private WithEvents cmbExpiryWarningInterval As System.Windows.Forms.ComboBox
    Friend WithEvents chkNoRepeatsDays As System.Windows.Forms.CheckBox
    Friend WithEvents chkNoRepeats As System.Windows.Forms.CheckBox
    Friend WithEvents gbActiveDirectoryAuthentication As GroupBox
    Private WithEvents chkEnableActiveDirectoryAuth As CheckBox
    Friend WithEvents PanelAuthenticationServer As Panel
    Friend WithEvents lblSelectCredential As Label
    Friend WithEvents cmbSelectCredential As ComboBox
    Friend WithEvents lblAuthServerUrl As Label
    Friend WithEvents txtAuthServerUrl As TextBox
    Friend WithEvents chkUseAuthServer As CheckBox
    Friend WithEvents gbAuthServer As GroupBox
    Private WithEvents btnApply As AutomateControls.Buttons.StandardStyledButton
End Class
