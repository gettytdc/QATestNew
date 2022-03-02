Imports AutomateControls.Wizard
Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlChooseOutputType
    Inherits WizardPanel

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                If mOutputOptions IsNot Nothing Then
                    RemoveHandler mOutputOptions.OptionsValidChanged, AddressOf HandleOptionsValidChanged
                    RemoveHandler mOutputOptions.OptionsValidChanged, AddressOf HandleOptionsValidChanged
                End If
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
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblConfigName = New System.Windows.Forms.Label()
        Me.lblType = New System.Windows.Forms.Label()
        Me.cmbOutputTypes = New System.Windows.Forms.ComboBox()
        Me.pnlOutputSettings = New System.Windows.Forms.Panel()
        Me.imgExclamation = New System.Windows.Forms.PictureBox()
        Me.lblInvalidConfigName = New System.Windows.Forms.Label()
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtName
        '
        Me.txtName.Location = New System.Drawing.Point(21, 35)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(480, 22)
        Me.txtName.TabIndex = 0
        '
        'lblConfigName
        '
        Me.lblConfigName.AutoSize = True
        Me.lblConfigName.Location = New System.Drawing.Point(18, 15)
        Me.lblConfigName.Name = "lblConfigName"
        Me.lblConfigName.Size = New System.Drawing.Size(111, 13)
        Me.lblConfigName.TabIndex = 1
        Me.lblConfigName.Text = Global.AutomateUI.My.Resources.Resources.ctlChooseOutputType_configName
        '
        'lblType
        '
        Me.lblType.AutoSize = True
        Me.lblType.Location = New System.Drawing.Point(18, 67)
        Me.lblType.Name = "lblType"
        Me.lblType.Size = New System.Drawing.Size(70, 13)
        Me.lblType.TabIndex = 2
        Me.lblType.Text = Global.AutomateUI.My.Resources.Resources.ctlChooseOutputType_outputType
        '
        'cmbOutputTypes
        '
        Me.cmbOutputTypes.FormattingEnabled = True
        Me.cmbOutputTypes.Location = New System.Drawing.Point(21, 88)
        Me.cmbOutputTypes.Name = "cmbOutputTypes"
        Me.cmbOutputTypes.Size = New System.Drawing.Size(240, 21)
        Me.cmbOutputTypes.TabIndex = 3
        '
        'pnlOutputSettings
        '
        Me.pnlOutputSettings.AutoSize = True
        Me.pnlOutputSettings.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.pnlOutputSettings.Location = New System.Drawing.Point(21, 120)
        Me.pnlOutputSettings.Name = "pnlOutputSettings"
        Me.pnlOutputSettings.Size = New System.Drawing.Size(733, 37)
        Me.pnlOutputSettings.TabIndex = 4
        '
        'imgExclamation
        '
        Me.imgExclamation.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16_Hot
        Me.imgExclamation.Location = New System.Drawing.Point(510, 38)
        Me.imgExclamation.Name = "imgExclamation"
        Me.imgExclamation.Size = New System.Drawing.Size(16, 16)
        Me.imgExclamation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.imgExclamation.TabIndex = 14
        Me.imgExclamation.TabStop = False
        Me.imgExclamation.Visible = False
        '
        'lblInvalidConfigName
        '
        Me.lblInvalidConfigName.AutoSize = True
        Me.lblInvalidConfigName.Location = New System.Drawing.Point(532, 38)
        Me.lblInvalidConfigName.Name = "lblInvalidConfigName"
        Me.lblInvalidConfigName.Size = New System.Drawing.Size(120, 13)
        Me.lblInvalidConfigName.TabIndex = 15
        Me.lblInvalidConfigName.Text = My.Resources.ctlChooseOutputType_ConfigNameNotUniqueError
        Me.lblInvalidConfigName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblInvalidConfigName.Visible = False
        '
        'ctlChooseOutputType
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblInvalidConfigName)
        Me.Controls.Add(Me.imgExclamation)
        Me.Controls.Add(Me.pnlOutputSettings)
        Me.Controls.Add(Me.cmbOutputTypes)
        Me.Controls.Add(Me.lblType)
        Me.Controls.Add(Me.lblConfigName)
        Me.Controls.Add(Me.txtName)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.Name = "ctlChooseOutputType"
        Me.Size = New System.Drawing.Size(785, 523)
        Me.Title = Global.AutomateUI.My.Resources.Resources.ctlChooseOutputType_title
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblConfigName As Label
    Friend WithEvents lblType As Label
    Friend WithEvents cmbOutputTypes As ComboBox
    Friend WithEvents pnlOutputSettings As Panel
    Friend WithEvents imgExclamation As PictureBox
    Friend WithEvents lblInvalidConfigName As Label
End Class
