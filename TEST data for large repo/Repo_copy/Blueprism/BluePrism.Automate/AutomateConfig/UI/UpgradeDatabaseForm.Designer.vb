<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UpgradeDatabaseForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UpgradeDatabaseForm))
        Me.ctlBlueBar = New AutomateControls.TitleBar()
        Me.lblRequiredValue = New System.Windows.Forms.Label()
        Me.lblCurrentValue = New System.Windows.Forms.Label()
        Me.lblPasswordPrompt = New System.Windows.Forms.Label()
        Me.lblRequiredVersion = New System.Windows.Forms.Label()
        Me.lblCurrentVersion = New System.Windows.Forms.Label()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnUpgrade = New AutomateControls.Buttons.StandardStyledButton()
        Me.mBackgroundWorker = New System.ComponentModel.BackgroundWorker()
        Me.lblWarning = New System.Windows.Forms.Label()
        Me.txtPassword = New AutomateControls.SecurePasswordTextBox()
        Me.mSaveFileDialog = New System.Windows.Forms.SaveFileDialog()
        Me.btnGenerateScript = New AutomateControls.Buttons.StandardStyledButton()
        Me.SessionMigrationGroupBox = New System.Windows.Forms.GroupBox()
        Me.MigrationDescription = New System.Windows.Forms.Label()
        Me.NoMigrationDescription = New System.Windows.Forms.Label()
        Me.MigrationWarningLabel = New System.Windows.Forms.Label()
        Me.MigrationRadio = New System.Windows.Forms.RadioButton()
        Me.NoMigrationRadio = New System.Windows.Forms.RadioButton()
        Me.SessionMigrationGroupBox.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlBlueBar
        '
        resources.ApplyResources(Me.ctlBlueBar, "ctlBlueBar")
        Me.ctlBlueBar.Name = "ctlBlueBar"
        '
        'lblRequiredValue
        '
        resources.ApplyResources(Me.lblRequiredValue, "lblRequiredValue")
        Me.lblRequiredValue.Name = "lblRequiredValue"
        '
        'lblCurrentValue
        '
        resources.ApplyResources(Me.lblCurrentValue, "lblCurrentValue")
        Me.lblCurrentValue.Name = "lblCurrentValue"
        '
        'lblPasswordPrompt
        '
        resources.ApplyResources(Me.lblPasswordPrompt, "lblPasswordPrompt")
        Me.lblPasswordPrompt.Name = "lblPasswordPrompt"
        '
        'lblRequiredVersion
        '
        resources.ApplyResources(Me.lblRequiredVersion, "lblRequiredVersion")
        Me.lblRequiredVersion.Name = "lblRequiredVersion"
        '
        'lblCurrentVersion
        '
        resources.ApplyResources(Me.lblCurrentVersion, "lblCurrentVersion")
        Me.lblCurrentVersion.Name = "lblCurrentVersion"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'btnUpgrade
        '
        resources.ApplyResources(Me.btnUpgrade, "btnUpgrade")
        Me.btnUpgrade.Name = "btnUpgrade"
        '
        'mBackgroundWorker
        '
        Me.mBackgroundWorker.WorkerReportsProgress = True
        '
        'lblWarning
        '
        resources.ApplyResources(Me.lblWarning, "lblWarning")
        Me.lblWarning.ForeColor = System.Drawing.Color.Red
        Me.lblWarning.Name = "lblWarning"
        '
        'txtPassword
        '
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.Name = "txtPassword"
        '
        'mSaveFileDialog
        '
        Me.mSaveFileDialog.DefaultExt = "sql"
        '
        'btnGenerateScript
        '
        resources.ApplyResources(Me.btnGenerateScript, "btnGenerateScript")
        Me.btnGenerateScript.Name = "btnGenerateScript"
        '
        'SessionMigrationGroupBox
        '
        resources.ApplyResources(Me.SessionMigrationGroupBox, "SessionMigrationGroupBox")
        Me.SessionMigrationGroupBox.Controls.Add(Me.MigrationDescription)
        Me.SessionMigrationGroupBox.Controls.Add(Me.NoMigrationDescription)
        Me.SessionMigrationGroupBox.Controls.Add(Me.MigrationWarningLabel)
        Me.SessionMigrationGroupBox.Controls.Add(Me.MigrationRadio)
        Me.SessionMigrationGroupBox.Controls.Add(Me.NoMigrationRadio)
        Me.SessionMigrationGroupBox.Name = "SessionMigrationGroupBox"
        Me.SessionMigrationGroupBox.TabStop = False
        '
        'MigrationDescription
        '
        resources.ApplyResources(Me.MigrationDescription, "MigrationDescription")
        Me.MigrationDescription.Name = "MigrationDescription"
        '
        'NoMigrationDescription
        '
        resources.ApplyResources(Me.NoMigrationDescription, "NoMigrationDescription")
        Me.NoMigrationDescription.Name = "NoMigrationDescription"
        '
        'MigrationWarningLabel
        '
        resources.ApplyResources(Me.MigrationWarningLabel, "MigrationWarningLabel")
        Me.MigrationWarningLabel.ForeColor = System.Drawing.Color.Red
        Me.MigrationWarningLabel.Name = "MigrationWarningLabel"
        '
        'MigrationRadio
        '
        resources.ApplyResources(Me.MigrationRadio, "MigrationRadio")
        Me.MigrationRadio.Name = "MigrationRadio"
        Me.MigrationRadio.UseVisualStyleBackColor = True
        '
        'NoMigrationRadio
        '
        resources.ApplyResources(Me.NoMigrationRadio, "NoMigrationRadio")
        Me.NoMigrationRadio.Checked = True
        Me.NoMigrationRadio.Name = "NoMigrationRadio"
        Me.NoMigrationRadio.TabStop = True
        Me.NoMigrationRadio.UseVisualStyleBackColor = True
        '
        'UpgradeDatabaseForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnGenerateScript)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnUpgrade)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.lblWarning)
        Me.Controls.Add(Me.ctlBlueBar)
        Me.Controls.Add(Me.lblRequiredValue)
        Me.Controls.Add(Me.lblCurrentValue)
        Me.Controls.Add(Me.lblPasswordPrompt)
        Me.Controls.Add(Me.lblRequiredVersion)
        Me.Controls.Add(Me.lblCurrentVersion)
        Me.Controls.Add(Me.SessionMigrationGroupBox)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "UpgradeDatabaseForm"
        Me.SessionMigrationGroupBox.ResumeLayout(False)
        Me.SessionMigrationGroupBox.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlBlueBar As AutomateControls.TitleBar
    Friend WithEvents lblRequiredValue As System.Windows.Forms.Label
    Friend WithEvents lblCurrentValue As System.Windows.Forms.Label
    Friend WithEvents lblPasswordPrompt As System.Windows.Forms.Label
    Friend WithEvents lblRequiredVersion As System.Windows.Forms.Label
    Friend WithEvents lblCurrentVersion As System.Windows.Forms.Label
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnUpgrade As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents mBackgroundWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents lblWarning As System.Windows.Forms.Label
    Friend WithEvents txtPassword As AutomateControls.SecurePasswordTextBox
    Friend WithEvents mSaveFileDialog As SaveFileDialog
    Friend WithEvents btnGenerateScript As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents SessionMigrationGroupBox As GroupBox
    Friend WithEvents MigrationWarningLabel As Label
    Friend WithEvents MigrationRadio As RadioButton
    Friend WithEvents NoMigrationRadio As RadioButton
    Friend WithEvents MigrationDescription As Label
    Friend WithEvents NoMigrationDescription As Label
End Class
