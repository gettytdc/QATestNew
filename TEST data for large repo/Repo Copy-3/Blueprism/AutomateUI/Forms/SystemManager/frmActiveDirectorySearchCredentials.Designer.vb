<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmActiveDirectorySearchCredentials
    Inherits AutomateControls.Forms.AutomateForm

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
        Me.tBar = New AutomateControls.TitleBar()
        Me.lblUserCredentialsDesc = New System.Windows.Forms.Label()
        Me.rdoUseDefaultCredentials = New AutomateControls.StyledRadioButton()
        Me.rdoInputCustomCredentials = New AutomateControls.StyledRadioButton()
        Me.pnlCredentialType = New System.Windows.Forms.Panel()
        Me.spPassword = New AutomateControls.SecurePasswordTextBox()
        Me.tbUsername = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblUsername = New System.Windows.Forms.Label()
        Me.btnUseCredentials = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.pnlCredentialType.SuspendLayout
        Me.SuspendLayout
        '
        'tBar
        '
        Me.tBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.tBar.Font = New System.Drawing.Font("Segoe UI", 12!)
        Me.tBar.Location = New System.Drawing.Point(0, 0)
        Me.tBar.Name = "tBar"
        Me.tBar.Size = New System.Drawing.Size(482, 70)
        Me.tBar.TabIndex = 0
        Me.tBar.Title = "Choose Credentials"
        Me.tBar.WrapTitle = false
        '
        'lblUserCredentialsDesc
        '
        Me.lblUserCredentialsDesc.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.lblUserCredentialsDesc.AutoSize = true
        Me.lblUserCredentialsDesc.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.lblUserCredentialsDesc.Location = New System.Drawing.Point(11, 10)
        Me.lblUserCredentialsDesc.Name = "lblUserCredentialsDesc"
        Me.lblUserCredentialsDesc.Size = New System.Drawing.Size(401, 17)
        Me.lblUserCredentialsDesc.TabIndex = 1
        Me.lblUserCredentialsDesc.Text = "Choose which credentials you would like to use for this search:"
        '
        'rdoUseDefaultCredentials
        '
        Me.rdoUseDefaultCredentials.AutoSize = true
        Me.rdoUseDefaultCredentials.ButtonHeight = 21
        Me.rdoUseDefaultCredentials.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.rdoUseDefaultCredentials.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.rdoUseDefaultCredentials.FocusDiameter = 16
        Me.rdoUseDefaultCredentials.FocusThickness = 3
        Me.rdoUseDefaultCredentials.FocusYLocation = 9
        Me.rdoUseDefaultCredentials.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.rdoUseDefaultCredentials.ForceFocus = true
        Me.rdoUseDefaultCredentials.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.rdoUseDefaultCredentials.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        Me.rdoUseDefaultCredentials.Location = New System.Drawing.Point(14, 33)
        Me.rdoUseDefaultCredentials.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoUseDefaultCredentials.Name = "rdoUseDefaultCredentials"
        Me.rdoUseDefaultCredentials.RadioButtonDiameter = 12
        Me.rdoUseDefaultCredentials.RadioButtonThickness = 2
        Me.rdoUseDefaultCredentials.RadioYLocation = 7
        Me.rdoUseDefaultCredentials.Size = New System.Drawing.Size(174, 21)
        Me.rdoUseDefaultCredentials.StringYLocation = 1
        Me.rdoUseDefaultCredentials.TabIndex = 2
        Me.rdoUseDefaultCredentials.TabStop = true
        Me.rdoUseDefaultCredentials.Text = "Use default credentials"
        Me.rdoUseDefaultCredentials.TextColor = System.Drawing.Color.Black
        Me.rdoUseDefaultCredentials.UseVisualStyleBackColor = true
        '
        'rdoInputCustomCredentials
        '
        Me.rdoInputCustomCredentials.AutoSize = true
        Me.rdoInputCustomCredentials.ButtonHeight = 21
        Me.rdoInputCustomCredentials.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer), CType(CType(212,Byte),Integer))
        Me.rdoInputCustomCredentials.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255,Byte),Integer), CType(CType(195,Byte),Integer), CType(CType(0,Byte),Integer))
        Me.rdoInputCustomCredentials.FocusDiameter = 16
        Me.rdoInputCustomCredentials.FocusThickness = 3
        Me.rdoInputCustomCredentials.FocusYLocation = 9
        Me.rdoInputCustomCredentials.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.rdoInputCustomCredentials.ForceFocus = true
        Me.rdoInputCustomCredentials.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67,Byte),Integer), CType(CType(74,Byte),Integer), CType(CType(79,Byte),Integer))
        Me.rdoInputCustomCredentials.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184,Byte),Integer), CType(CType(201,Byte),Integer), CType(CType(216,Byte),Integer))
        Me.rdoInputCustomCredentials.Location = New System.Drawing.Point(14, 56)
        Me.rdoInputCustomCredentials.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoInputCustomCredentials.Name = "rdoInputCustomCredentials"
        Me.rdoInputCustomCredentials.RadioButtonDiameter = 12
        Me.rdoInputCustomCredentials.RadioButtonThickness = 2
        Me.rdoInputCustomCredentials.RadioYLocation = 7
        Me.rdoInputCustomCredentials.Size = New System.Drawing.Size(182, 21)
        Me.rdoInputCustomCredentials.StringYLocation = 1
        Me.rdoInputCustomCredentials.TabIndex = 3
        Me.rdoInputCustomCredentials.TabStop = true
        Me.rdoInputCustomCredentials.Text = "Input custom credentials"
        Me.rdoInputCustomCredentials.TextColor = System.Drawing.Color.Black
        Me.rdoInputCustomCredentials.UseVisualStyleBackColor = true
        '
        'pnlCredentialType
        '
        Me.pnlCredentialType.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.pnlCredentialType.Controls.Add(Me.spPassword)
        Me.pnlCredentialType.Controls.Add(Me.lblUserCredentialsDesc)
        Me.pnlCredentialType.Controls.Add(Me.tbUsername)
        Me.pnlCredentialType.Controls.Add(Me.lblUsername)
        Me.pnlCredentialType.Controls.Add(Me.btnUseCredentials)
        Me.pnlCredentialType.Controls.Add(Me.rdoUseDefaultCredentials)
        Me.pnlCredentialType.Controls.Add(Me.lblPassword)
        Me.pnlCredentialType.Controls.Add(Me.rdoInputCustomCredentials)
        Me.pnlCredentialType.Location = New System.Drawing.Point(0, 70)
        Me.pnlCredentialType.Name = "pnlCredentialType"
        Me.pnlCredentialType.Size = New System.Drawing.Size(482, 189)
        Me.pnlCredentialType.TabIndex = 4
        '
        'spPassword
        '
        Me.spPassword.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.spPassword.BorderColor = System.Drawing.Color.Empty
        Me.spPassword.Enabled = false
        Me.spPassword.ImeMode = System.Windows.Forms.ImeMode.Disable
        Me.spPassword.Location = New System.Drawing.Point(34, 134)
        Me.spPassword.Name = "spPassword"
        Me.spPassword.Size = New System.Drawing.Size(436, 22)
        Me.spPassword.TabIndex = 8
        '
        'tbUsername
        '
        Me.tbUsername.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.tbUsername.BorderColor = System.Drawing.Color.Empty
        Me.tbUsername.Enabled = false
        Me.tbUsername.Location = New System.Drawing.Point(34, 95)
        Me.tbUsername.Name = "tbUsername"
        Me.tbUsername.Size = New System.Drawing.Size(436, 22)
        Me.tbUsername.TabIndex = 6
        '
        'lblUsername
        '
        Me.lblUsername.AutoSize = true
        Me.lblUsername.Location = New System.Drawing.Point(31, 79)
        Me.lblUsername.Name = "lblUsername"
        Me.lblUsername.Size = New System.Drawing.Size(73, 17)
        Me.lblUsername.TabIndex = 5
        Me.lblUsername.Text = "Username"
        '
        'btnUseCredentials
        '
        Me.btnUseCredentials.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.btnUseCredentials.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnUseCredentials.Location = New System.Drawing.Point(315, 166)
        Me.btnUseCredentials.Name = "btnUseCredentials"
        Me.btnUseCredentials.Size = New System.Drawing.Size(155, 23)
        Me.btnUseCredentials.TabIndex = 9
        Me.btnUseCredentials.Text = "Use credentials"
        Me.btnUseCredentials.UseVisualStyleBackColor = false
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = true
        Me.lblPassword.Location = New System.Drawing.Point(31, 118)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(69, 17)
        Me.lblPassword.TabIndex = 7
        Me.lblPassword.Text = "Password"
        '
        'frmActiveDirectorySearchCredentials
        '
        Me.ClientSize = New System.Drawing.Size(482, 271)
        Me.Controls.Add(Me.pnlCredentialType)
        Me.Controls.Add(Me.tBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.MinimumSize = New System.Drawing.Size(500, 318)
        Me.Name = "frmActiveDirectorySearchCredentials"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.pnlCredentialType.ResumeLayout(false)
        Me.pnlCredentialType.PerformLayout
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents tBar As AutomateControls.TitleBar
    Friend WithEvents lblUserCredentialsDesc As Label
    Friend WithEvents rdoUseDefaultCredentials As AutomateControls.StyledRadioButton
    Friend WithEvents rdoInputCustomCredentials As AutomateControls.StyledRadioButton
    Friend WithEvents pnlCredentialType As Panel
    Friend WithEvents btnUseCredentials As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents spPassword As AutomateControls.SecurePasswordTextBox
    Friend WithEvents tbUsername As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblUsername As Label
    Friend WithEvents lblPassword As Label
End Class
