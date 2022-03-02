Imports BluePrism.Config

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSystemSingleSignon
    Inherits System.Windows.Forms.UserControl

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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemSingleSignon))
		Me.btnCommitChanges = New AutomateControls.Buttons.StandardStyledButton()
		Me.mADSettings = New BluePrism.Config.ActiveDirectorySettings()
		Me.lblRestartReminder = New System.Windows.Forms.Label()
		Me.GroupBox2 = New System.Windows.Forms.GroupBox()
		Me.ADConversionGroupBox = New System.Windows.Forms.GroupBox()
		Me.pbWarningIcon = New System.Windows.Forms.PictureBox()
		Me.btnConvert = New AutomateControls.Buttons.StandardStyledButton()
		Me.lblADConversionText = New System.Windows.Forms.Label()
		Me.lblADConversionTextPart2 = New System.Windows.Forms.Label()
		Me.GroupBox2.SuspendLayout()
		Me.ADConversionGroupBox.SuspendLayout()
		CType(Me.pbWarningIcon, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'btnCommitChanges
		'
		resources.ApplyResources(Me.btnCommitChanges, "btnCommitChanges")
		Me.btnCommitChanges.BackColor = System.Drawing.SystemColors.ButtonFace
		Me.btnCommitChanges.Name = "btnCommitChanges"
		Me.btnCommitChanges.UseVisualStyleBackColor = False
		'
		'mADSettings
		'
		resources.ApplyResources(Me.mADSettings, "mADSettings")
		Me.mADSettings.BackColor = System.Drawing.Color.Transparent
		Me.mADSettings.ForeColor = System.Drawing.Color.Black
		Me.mADSettings.IsNewConnection = False
		Me.mADSettings.Name = "mADSettings"
		'
		'lblRestartReminder
		'
		resources.ApplyResources(Me.lblRestartReminder, "lblRestartReminder")
		Me.lblRestartReminder.Name = "lblRestartReminder"
		'
		'GroupBox2
		'
		resources.ApplyResources(Me.GroupBox2, "GroupBox2")
		Me.GroupBox2.Controls.Add(Me.lblRestartReminder)
		Me.GroupBox2.Controls.Add(Me.mADSettings)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.TabStop = False
		'
		'ADConversionGroupBox
		'
		resources.ApplyResources(Me.ADConversionGroupBox, "ADConversionGroupBox")
		Me.ADConversionGroupBox.Controls.Add(Me.pbWarningIcon)
		Me.ADConversionGroupBox.Controls.Add(Me.btnConvert)
		Me.ADConversionGroupBox.Controls.Add(Me.lblADConversionText)
		Me.ADConversionGroupBox.Controls.Add(Me.lblADConversionTextPart2)
		Me.ADConversionGroupBox.Name = "ADConversionGroupBox"
		Me.ADConversionGroupBox.TabStop = False
		'
		'pbWarningIcon
		'
		resources.ApplyResources(Me.pbWarningIcon, "pbWarningIcon")
		Me.pbWarningIcon.Name = "pbWarningIcon"
		Me.pbWarningIcon.TabStop = False
		'
		'btnConvert
		'
		resources.ApplyResources(Me.btnConvert, "btnConvert")
		Me.btnConvert.Name = "btnConvert"
		Me.btnConvert.UseVisualStyleBackColor = True
		'
		'lblADConversionText
		'
		resources.ApplyResources(Me.lblADConversionText, "lblADConversionText")
		Me.lblADConversionText.Name = "lblADConversionText"
		'
		'lblADConversionTextPart2
		'
		resources.ApplyResources(Me.lblADConversionTextPart2, "lblADConversionTextPart2")
		Me.lblADConversionTextPart2.Name = "lblADConversionTextPart2"
		'
		'ctlSystemSingleSignon
		'
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
		Me.Controls.Add(Me.GroupBox2)
		Me.Controls.Add(Me.btnCommitChanges)
		Me.Controls.Add(Me.ADConversionGroupBox)
		Me.Name = "ctlSystemSingleSignon"
		resources.ApplyResources(Me, "$this")
		Me.GroupBox2.ResumeLayout(False)
		Me.ADConversionGroupBox.ResumeLayout(False)
		CType(Me.pbWarningIcon, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub
	Private WithEvents btnCommitChanges As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents mADSettings As ActiveDirectorySettings
    Friend WithEvents lblRestartReminder As System.Windows.Forms.Label
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents ADConversionGroupBox As GroupBox
    Private WithEvents btnConvert As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblADConversionText As Label
    Private WithEvents pbWarningIcon As PictureBox
	Friend WithEvents lblADConversionTextPart2 As Label
End Class
