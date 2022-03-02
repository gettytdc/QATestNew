<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CreateDatabaseForm

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
        Me.components = New System.ComponentModel.Container()
        Dim Label3 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CreateDatabaseForm))
        Dim Label2 As System.Windows.Forms.Label
        Me.lblDatabaseName = New System.Windows.Forms.Label()
        Me.lblConnectionName = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.rdoSingleAuth = New AutomateControls.StyledRadioButton()
        Me.rdoMultiAuth = New AutomateControls.StyledRadioButton()
        Me.chkPurgeExistingDB = New System.Windows.Forms.CheckBox()
        Me.lblRetypePassword = New System.Windows.Forms.Label()
        Me.lblExplanation = New System.Windows.Forms.Label()
        Me.titleBar = New AutomateControls.TitleBar()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtPassword = New AutomateControls.SecurePasswordTextBox()
        Me.mBackgroundWorker = New System.ComponentModel.BackgroundWorker()
        Me.mSaveFileDialog = New System.Windows.Forms.SaveFileDialog()
        Me.btnGenerateScript = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.objActiveDirectorySettings = New BluePrism.Config.ActiveDirectorySettings()
        Label3 = New System.Windows.Forms.Label()
        Label2 = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
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
        'lblDatabaseName
        '
        resources.ApplyResources(Me.lblDatabaseName, "lblDatabaseName")
        Me.lblDatabaseName.Name = "lblDatabaseName"
        '
        'lblConnectionName
        '
        resources.ApplyResources(Me.lblConnectionName, "lblConnectionName")
        Me.lblConnectionName.Name = "lblConnectionName"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Panel1.Controls.Add(Me.Label5)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.rdoSingleAuth)
        Me.Panel1.Controls.Add(Me.rdoMultiAuth)
        Me.Panel1.Controls.Add(Me.objActiveDirectorySettings)
        Me.Panel1.Name = "Panel1"
        '
        'rdoSingleAuth
        '
        Me.rdoSingleAuth.ButtonHeight = 21
        Me.rdoSingleAuth.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoSingleAuth.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoSingleAuth.FocusDiameter = 16
        Me.rdoSingleAuth.FocusThickness = 3
        Me.rdoSingleAuth.FocusYLocation = 9
        resources.ApplyResources(Me.rdoSingleAuth, "rdoSingleAuth")
        Me.rdoSingleAuth.ForceFocus = True
        Me.rdoSingleAuth.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoSingleAuth.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoSingleAuth.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoSingleAuth.Name = "rdoSingleAuth"
        Me.rdoSingleAuth.RadioButtonDiameter = 12
        Me.rdoSingleAuth.RadioButtonThickness = 2
        Me.rdoSingleAuth.RadioYLocation = 7
        Me.rdoSingleAuth.StringYLocation = 6
        Me.rdoSingleAuth.TextColor = System.Drawing.Color.Black
        Me.rdoSingleAuth.UseVisualStyleBackColor = True
        '
        'rdoMultiAuth
        '
        Me.rdoMultiAuth.ButtonHeight = 21
        Me.rdoMultiAuth.Checked = True
        Me.rdoMultiAuth.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoMultiAuth.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoMultiAuth.FocusDiameter = 16
        Me.rdoMultiAuth.FocusThickness = 3
        Me.rdoMultiAuth.FocusYLocation = 9
        resources.ApplyResources(Me.rdoMultiAuth, "rdoMultiAuth")
        Me.rdoMultiAuth.ForceFocus = True
        Me.rdoMultiAuth.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoMultiAuth.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoMultiAuth.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoMultiAuth.Name = "rdoMultiAuth"
        Me.rdoMultiAuth.RadioButtonDiameter = 12
        Me.rdoMultiAuth.RadioButtonThickness = 2
        Me.rdoMultiAuth.RadioYLocation = 7
        Me.rdoMultiAuth.StringYLocation = 6
        Me.rdoMultiAuth.TabStop = True
        Me.rdoMultiAuth.TextColor = System.Drawing.Color.Black
        Me.rdoMultiAuth.UseVisualStyleBackColor = True
        '
        'chkPurgeExistingDB
        '
        resources.ApplyResources(Me.chkPurgeExistingDB, "chkPurgeExistingDB")
        Me.chkPurgeExistingDB.Name = "chkPurgeExistingDB"
        Me.chkPurgeExistingDB.UseVisualStyleBackColor = True
        '
        'lblRetypePassword
        '
        resources.ApplyResources(Me.lblRetypePassword, "lblRetypePassword")
        Me.lblRetypePassword.Name = "lblRetypePassword"
        '
        'lblExplanation
        '
        resources.ApplyResources(Me.lblExplanation, "lblExplanation")
        Me.lblExplanation.Name = "lblExplanation"
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'txtPassword
        '
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.BorderColor = System.Drawing.Color.Empty
        Me.txtPassword.Name = "txtPassword"
        '
        'mBackgroundWorker
        '
        Me.mBackgroundWorker.WorkerReportsProgress = True
        '
        'mSaveFileDialog
        '
        Me.mSaveFileDialog.DefaultExt = "sql"
        '
        'btnGenerateScript
        '
        resources.ApplyResources(Me.btnGenerateScript, "btnGenerateScript")
        Me.btnGenerateScript.Name = "btnGenerateScript"
        Me.btnGenerateScript.UseVisualStyleBackColor = False
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'objActiveDirectorySettings
        '
        resources.ApplyResources(Me.objActiveDirectorySettings, "objActiveDirectorySettings")
        Me.objActiveDirectorySettings.IsNewConnection = True
        Me.objActiveDirectorySettings.Name = "objActiveDirectorySettings"
        '
        'CreateDatabaseForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnGenerateScript)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.titleBar)
        Me.Controls.Add(Me.lblDatabaseName)
        Me.Controls.Add(Me.lblConnectionName)
        Me.Controls.Add(Label3)
        Me.Controls.Add(Label2)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.chkPurgeExistingDB)
        Me.Controls.Add(Me.lblRetypePassword)
        Me.Controls.Add(Me.lblExplanation)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "CreateDatabaseForm"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lblDatabaseName As System.Windows.Forms.Label
    Private WithEvents lblConnectionName As System.Windows.Forms.Label
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents chkPurgeExistingDB As System.Windows.Forms.CheckBox
    Friend WithEvents lblRetypePassword As System.Windows.Forms.Label
    Private WithEvents lblExplanation As System.Windows.Forms.Label
    Private WithEvents titleBar As AutomateControls.TitleBar
    Protected WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents objActiveDirectorySettings As BluePrism.Config.ActiveDirectorySettings
    Friend WithEvents txtPassword As AutomateControls.SecurePasswordTextBox
    Friend WithEvents mBackgroundWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents mSaveFileDialog As SaveFileDialog
    Protected WithEvents btnGenerateScript As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents rdoMultiAuth As AutomateControls.StyledRadioButton
    Friend WithEvents rdoSingleAuth As AutomateControls.StyledRadioButton
    Friend WithEvents Label5 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label1 As Label
End Class
