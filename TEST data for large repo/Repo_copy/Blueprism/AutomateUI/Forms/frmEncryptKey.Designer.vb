<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmEncryptKey
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEncryptKey))
        Me.txtKey = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblKey = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblMethod = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.chkShowKey = New System.Windows.Forms.CheckBox()
        Me.llGenerateKey = New System.Windows.Forms.LinkLabel()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.rbAppServer = New AutomateControls.StyledRadioButton()
        Me.rbDatabase = New AutomateControls.StyledRadioButton()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.chkAvailable = New System.Windows.Forms.CheckBox()
        Me.tBar = New AutomateControls.TitleBar()
        Me.lblRetiredScheme = New System.Windows.Forms.Label()
        Me.cmbAlgorithm = New AutomateControls.StyledComboBox()
        Me.SuspendLayout
        '
        'txtKey
        '
        resources.ApplyResources(Me.txtKey, "txtKey")
        Me.txtKey.Name = "txtKey"
        '
        'lblKey
        '
        resources.ApplyResources(Me.lblKey, "lblKey")
        Me.lblKey.Name = "lblKey"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'lblMethod
        '
        resources.ApplyResources(Me.lblMethod, "lblMethod")
        Me.lblMethod.Name = "lblMethod"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        '
        'chkShowKey
        '
        resources.ApplyResources(Me.chkShowKey, "chkShowKey")
        Me.chkShowKey.Name = "chkShowKey"
        Me.chkShowKey.UseVisualStyleBackColor = true
        '
        'llGenerateKey
        '
        resources.ApplyResources(Me.llGenerateKey, "llGenerateKey")
        Me.llGenerateKey.Name = "llGenerateKey"
        Me.llGenerateKey.TabStop = true
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = true
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'rbAppServer
        '
        Me.rbAppServer.ButtonHeight = 21
        Me.rbAppServer.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rbAppServer.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rbAppServer.FocusDiameter = 16
        Me.rbAppServer.FocusThickness = 3
        Me.rbAppServer.FocusYLocation = 9
        Me.rbAppServer.ForceFocus = True
        Me.rbAppServer.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rbAppServer.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        resources.ApplyResources(Me.rbAppServer, "rbAppServer")
        Me.rbAppServer.MouseLeaveColor = System.Drawing.Color.White
        Me.rbAppServer.Name = "rbAppServer"
        Me.rbAppServer.RadioButtonDiameter = 12
        Me.rbAppServer.RadioButtonThickness = 2
        Me.rbAppServer.RadioYLocation = 7
        Me.rbAppServer.StringYLocation = 1
        Me.rbAppServer.TabStop = True
        Me.rbAppServer.TextColor = System.Drawing.Color.Black
        Me.rbAppServer.UseVisualStyleBackColor = True
        '
        'rbDatabase
        '
        Me.rbDatabase.ButtonHeight = 21
        Me.rbDatabase.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rbDatabase.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rbDatabase.FocusDiameter = 16
        Me.rbDatabase.FocusThickness = 3
        Me.rbDatabase.FocusYLocation = 9
        Me.rbDatabase.ForceFocus = True
        Me.rbDatabase.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rbDatabase.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        resources.ApplyResources(Me.rbDatabase, "rbDatabase")
        Me.rbDatabase.MouseLeaveColor = System.Drawing.Color.White
        Me.rbDatabase.Name = "rbDatabase"
        Me.rbDatabase.RadioButtonDiameter = 12
        Me.rbDatabase.RadioButtonThickness = 2
        Me.rbDatabase.RadioYLocation = 7
        Me.rbDatabase.StringYLocation = 1
        Me.rbDatabase.TabStop = True
        Me.rbDatabase.TextColor = System.Drawing.Color.Black
        Me.rbDatabase.UseVisualStyleBackColor = True
        '
        'lblMessage
        '
        resources.ApplyResources(Me.lblMessage, "lblMessage")
        Me.lblMessage.Name = "lblMessage"
        '
        'chkAvailable
        '
        resources.ApplyResources(Me.chkAvailable, "chkAvailable")
        Me.chkAvailable.Name = "chkAvailable"
        Me.chkAvailable.UseVisualStyleBackColor = true
        '
        'tBar
        '
        resources.ApplyResources(Me.tBar, "tBar")
        Me.tBar.Name = "tBar"
        '
        'lblRetiredScheme
        '
        resources.ApplyResources(Me.lblRetiredScheme, "lblRetiredScheme")
        Me.lblRetiredScheme.ForeColor = System.Drawing.Color.Red
        Me.lblRetiredScheme.Name = "lblRetiredScheme"
        '
        'cmbAlgorithm
        '
        Me.cmbAlgorithm.Checkable = false
        Me.cmbAlgorithm.FormattingEnabled = true
        resources.ApplyResources(Me.cmbAlgorithm, "cmbAlgorithm")
        Me.cmbAlgorithm.Name = "cmbAlgorithm"
        '
        'frmEncryptKey
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblRetiredScheme)
        Me.Controls.Add(Me.tBar)
        Me.Controls.Add(Me.chkAvailable)
        Me.Controls.Add(Me.rbDatabase)
        Me.Controls.Add(Me.rbAppServer)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.llGenerateKey)
        Me.Controls.Add(Me.chkShowKey)
        Me.Controls.Add(Me.cmbAlgorithm)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.lblMethod)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lblKey)
        Me.Controls.Add(Me.txtKey)
        Me.Controls.Add(Me.lblMessage)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = false
        Me.MinimizeBox = false
        Me.Name = "frmEncryptKey"
        Me.ShowInTaskbar = false
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents txtKey As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblKey As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblMethod As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents chkShowKey As System.Windows.Forms.CheckBox
    Friend WithEvents llGenerateKey As System.Windows.Forms.LinkLabel
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents rbAppServer As AutomateControls.StyledRadioButton
    Friend WithEvents rbDatabase As AutomateControls.StyledRadioButton
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents chkAvailable As System.Windows.Forms.CheckBox
    Friend WithEvents tBar As AutomateControls.TitleBar
    Friend WithEvents lblRetiredScheme As Label
    Friend WithEvents cmbAlgorithm As AutomateControls.StyledComboBox
End Class
