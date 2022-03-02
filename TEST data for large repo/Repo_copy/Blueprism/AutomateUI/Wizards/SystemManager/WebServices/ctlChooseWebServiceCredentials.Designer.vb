Imports AutomateControls.Wizard

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlChooseWebServiceCredentials
    Inherits WizardPanel

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseWebServiceCredentials))
        Me.chkNeedsUsernameAndPassword = New System.Windows.Forms.CheckBox()
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.txtUsername = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblUsername = New System.Windows.Forms.Label()
        Me.chkNeedsClientSSL = New System.Windows.Forms.CheckBox()
        Me.cmbStoreName = New System.Windows.Forms.ComboBox()
        Me.lblStoreName = New System.Windows.Forms.Label()
        Me.lblStoreLocation = New System.Windows.Forms.Label()
        Me.cmbStoreLocation = New System.Windows.Forms.ComboBox()
        Me.txtSearchLocation = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblSearchLocation = New System.Windows.Forms.Label()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblFindType = New System.Windows.Forms.Label()
        Me.cmbFindType = New System.Windows.Forms.ComboBox()
        Me.dlgCertificate = New System.Windows.Forms.OpenFileDialog()
        Me.lblCertPassword = New System.Windows.Forms.Label()
        Me.btnChange = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtPassword = New AutomateUI.ctlAutomateSecurePassword()
        Me.txtCertPassword = New AutomateUI.ctlAutomateSecurePassword()
        Me.SuspendLayout()
        '
        'chkNeedsUsernameAndPassword
        '
        resources.ApplyResources(Me.chkNeedsUsernameAndPassword, "chkNeedsUsernameAndPassword")
        Me.chkNeedsUsernameAndPassword.Name = "chkNeedsUsernameAndPassword"
        Me.chkNeedsUsernameAndPassword.UseVisualStyleBackColor = True
        '
        'lblPassword
        '
        resources.ApplyResources(Me.lblPassword, "lblPassword")
        Me.lblPassword.Name = "lblPassword"
        '
        'txtUsername
        '
        resources.ApplyResources(Me.txtUsername, "txtUsername")
        Me.txtUsername.Name = "txtUsername"
        '
        'lblUsername
        '
        resources.ApplyResources(Me.lblUsername, "lblUsername")
        Me.lblUsername.Name = "lblUsername"
        '
        'chkNeedsClientSSL
        '
        resources.ApplyResources(Me.chkNeedsClientSSL, "chkNeedsClientSSL")
        Me.chkNeedsClientSSL.Name = "chkNeedsClientSSL"
        Me.chkNeedsClientSSL.UseVisualStyleBackColor = True
        '
        'cmbStoreName
        '
        resources.ApplyResources(Me.cmbStoreName, "cmbStoreName")
        Me.cmbStoreName.FormattingEnabled = True
        Me.cmbStoreName.Name = "cmbStoreName"
        '
        'lblStoreName
        '
        resources.ApplyResources(Me.lblStoreName, "lblStoreName")
        Me.lblStoreName.Name = "lblStoreName"
        '
        'lblStoreLocation
        '
        resources.ApplyResources(Me.lblStoreLocation, "lblStoreLocation")
        Me.lblStoreLocation.Name = "lblStoreLocation"
        '
        'cmbStoreLocation
        '
        resources.ApplyResources(Me.cmbStoreLocation, "cmbStoreLocation")
        Me.cmbStoreLocation.FormattingEnabled = True
        Me.cmbStoreLocation.Name = "cmbStoreLocation"
        '
        'txtSearchLocation
        '
        resources.ApplyResources(Me.txtSearchLocation, "txtSearchLocation")
        Me.txtSearchLocation.Name = "txtSearchLocation"
        '
        'lblSearchLocation
        '
        resources.ApplyResources(Me.lblSearchLocation, "lblSearchLocation")
        Me.lblSearchLocation.Name = "lblSearchLocation"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'lblFindType
        '
        resources.ApplyResources(Me.lblFindType, "lblFindType")
        Me.lblFindType.Name = "lblFindType"
        '
        'cmbFindType
        '
        resources.ApplyResources(Me.cmbFindType, "cmbFindType")
        Me.cmbFindType.DropDownWidth = 200
        Me.cmbFindType.FormattingEnabled = True
        Me.cmbFindType.Name = "cmbFindType"
        '
        'dlgCertificate
        '
        resources.ApplyResources(Me.dlgCertificate, "dlgCertificate")
        '
        'lblCertPassword
        '
        resources.ApplyResources(Me.lblCertPassword, "lblCertPassword")
        Me.lblCertPassword.Name = "lblCertPassword"
        '
        'btnChange
        '
        resources.ApplyResources(Me.btnChange, "btnChange")
        Me.btnChange.Name = "btnChange"
        Me.btnChange.UseVisualStyleBackColor = True
        '
        'txtPassword
        '
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.Name = "txtPassword"
        '
        'txtCertPassword
        '
        resources.ApplyResources(Me.txtCertPassword, "txtCertPassword")
        Me.txtCertPassword.Name = "txtCertPassword"
        '
        'ctlChooseWebServiceCredentials
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtCertPassword)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.btnChange)
        Me.Controls.Add(Me.lblCertPassword)
        Me.Controls.Add(Me.lblFindType)
        Me.Controls.Add(Me.cmbFindType)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.lblSearchLocation)
        Me.Controls.Add(Me.txtSearchLocation)
        Me.Controls.Add(Me.cmbStoreLocation)
        Me.Controls.Add(Me.lblStoreLocation)
        Me.Controls.Add(Me.lblStoreName)
        Me.Controls.Add(Me.cmbStoreName)
        Me.Controls.Add(Me.chkNeedsClientSSL)
        Me.Controls.Add(Me.chkNeedsUsernameAndPassword)
        Me.Controls.Add(Me.lblPassword)
        Me.Controls.Add(Me.txtUsername)
        Me.Controls.Add(Me.lblUsername)
        Me.Name = "ctlChooseWebServiceCredentials"
        Me.NavigateNext = True
        Me.NavigatePrevious = True
        Me.Title = Global.AutomateUI.My.Resources.ctlChooseWebServiceCredentials_PleaseSupplyAuthenticationDetailsForTheWebService
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkNeedsUsernameAndPassword As System.Windows.Forms.CheckBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents txtUsername As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblUsername As System.Windows.Forms.Label
    Friend WithEvents chkNeedsClientSSL As System.Windows.Forms.CheckBox
    Friend WithEvents cmbStoreName As System.Windows.Forms.ComboBox
    Friend WithEvents lblStoreName As System.Windows.Forms.Label
    Friend WithEvents lblStoreLocation As System.Windows.Forms.Label
    Friend WithEvents cmbStoreLocation As System.Windows.Forms.ComboBox
    Friend WithEvents txtSearchLocation As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblSearchLocation As System.Windows.Forms.Label
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblFindType As System.Windows.Forms.Label
    Friend WithEvents cmbFindType As System.Windows.Forms.ComboBox
    Friend WithEvents dlgCertificate As System.Windows.Forms.OpenFileDialog
    Friend WithEvents lblCertPassword As System.Windows.Forms.Label
    Friend WithEvents btnChange As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtPassword As AutomateUI.ctlAutomateSecurePassword
    Friend WithEvents txtCertPassword As AutomateUI.ctlAutomateSecurePassword

End Class
