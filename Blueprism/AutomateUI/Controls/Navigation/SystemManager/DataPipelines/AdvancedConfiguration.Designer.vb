<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdvancedConfigurationForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AdvancedConfigurationForm))
        Me.btnSave = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.PictureBox()
        Me.BorderPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.imgExclamation = New System.Windows.Forms.PictureBox()
        Me.lblInvalidConfigName = New System.Windows.Forms.Label()
        Me.lblFlowBreak = New System.Windows.Forms.Label()
        Me.txtConfigurationName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.txtConfiguration = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnPaste = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnImport = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label4 = New System.Windows.Forms.Label()
        CType(Me.btnClose, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.BorderPanel.SuspendLayout()
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(10, 512)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(0, 10, 73, 3)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(220, 32)
        Me.btnSave.TabIndex = 5
        Me.btnSave.Text = Global.AutomateUI.My.Resources.Resources.AdvancedDataPipeLineConfig_Save
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCancel.ForeColor = System.Drawing.Color.Black
        Me.btnCancel.Location = New System.Drawing.Point(303, 512)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(0, 10, 0, 3)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(220, 32)
        Me.btnCancel.TabIndex = 6
        Me.btnCancel.Text = Global.AutomateUI.My.Resources.Resources.AdvancedDataPipeLineConfig_Cancel
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.BorderPanel.SetFlowBreak(Me.Label1, True)
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(10, 43)
        Me.Label1.Margin = New System.Windows.Forms.Padding(0, 0, 10, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(339, 25)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = My.Resources.DataGatewaysAdvancedConfiguration_Title
        '
        'btnClose
        '
        Me.btnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BorderPanel.SetFlowBreak(Me.btnClose, True)
        Me.btnClose.Image = Global.AutomateUI.ActivationWizardResources.Close_32x32
        Me.btnClose.Location = New System.Drawing.Point(485, 15)
        Me.btnClose.Margin = New System.Windows.Forms.Padding(475, 15, 15, 0)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(28, 28)
        Me.btnClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.btnClose.TabIndex = 18
        Me.btnClose.TabStop = False
        '
        'BorderPanel
        '
        Me.BorderPanel.BackColor = System.Drawing.Color.White
        Me.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.BorderPanel.Controls.Add(Me.btnClose)
        Me.BorderPanel.Controls.Add(Me.Label1)
        Me.BorderPanel.Controls.Add(Me.Label2)
        Me.BorderPanel.Controls.Add(Me.imgExclamation)
        Me.BorderPanel.Controls.Add(Me.lblInvalidConfigName)
        Me.BorderPanel.Controls.Add(Me.lblFlowBreak)
        Me.BorderPanel.Controls.Add(Me.txtConfigurationName)
        Me.BorderPanel.Controls.Add(Me.Label3)
        Me.BorderPanel.Controls.Add(Me.Panel2)
        Me.BorderPanel.Controls.Add(Me.btnSave)
        Me.BorderPanel.Controls.Add(Me.btnCancel)
        Me.BorderPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BorderPanel.Location = New System.Drawing.Point(0, 0)
        Me.BorderPanel.Margin = New System.Windows.Forms.Padding(0)
        Me.BorderPanel.Name = "BorderPanel"
        Me.BorderPanel.Padding = New System.Windows.Forms.Padding(10, 0, 10, 0)
        Me.BorderPanel.Size = New System.Drawing.Size(535, 565)
        Me.BorderPanel.TabIndex = 0
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(10, 88)
        Me.Label2.Margin = New System.Windows.Forms.Padding(0, 10, 10, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(111, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = My.Resources.DataGatewaysAdvancedConfiguration_ConfigurationName
        '
        'imgExclamation
        '
        Me.imgExclamation.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16_Hot
        Me.imgExclamation.Location = New System.Drawing.Point(137, 88)
        Me.imgExclamation.Margin = New System.Windows.Forms.Padding(5, 10, 5, 0)
        Me.imgExclamation.Name = "imgExclamation"
        Me.imgExclamation.Size = New System.Drawing.Size(16, 16)
        Me.imgExclamation.TabIndex = 19
        Me.imgExclamation.TabStop = False
        Me.imgExclamation.Visible = False
        '
        'lblInvalidConfigName
        '
        Me.lblInvalidConfigName.AutoSize = True
        Me.lblInvalidConfigName.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInvalidConfigName.Location = New System.Drawing.Point(158, 88)
        Me.lblInvalidConfigName.Margin = New System.Windows.Forms.Padding(0, 10, 10, 0)
        Me.lblInvalidConfigName.Name = "lblInvalidConfigName"
        Me.lblInvalidConfigName.Size = New System.Drawing.Size(120, 13)
        Me.lblInvalidConfigName.TabIndex = 21
        Me.lblInvalidConfigName.Text = My.Resources.ctlChooseOutputType_ConfigNameNotUniqueError
        Me.lblInvalidConfigName.Visible = False
        '
        'lblFlowBreak
        '
        Me.lblFlowBreak.AutoSize = True
        Me.BorderPanel.SetFlowBreak(Me.lblFlowBreak, True)
        Me.lblFlowBreak.Location = New System.Drawing.Point(291, 78)
        Me.lblFlowBreak.Name = "lblFlowBreak"
        Me.lblFlowBreak.Size = New System.Drawing.Size(13, 13)
        Me.lblFlowBreak.TabIndex = 22
        Me.lblFlowBreak.Text = "  "
        '
        'txtConfigurationName
        '
        Me.txtConfigurationName.Location = New System.Drawing.Point(13, 107)
        Me.txtConfigurationName.Margin = New System.Windows.Forms.Padding(3, 3, 10, 3)
        Me.txtConfigurationName.Name = "txtConfigurationName"
        Me.txtConfigurationName.Size = New System.Drawing.Size(302, 20)
        Me.txtConfigurationName.TabIndex = 2
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.SystemColors.GrayText
        Me.Label3.Location = New System.Drawing.Point(325, 109)
        Me.Label3.Margin = New System.Windows.Forms.Padding(0, 5, 10, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(63, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = My.Resources.DataGatewaysAdvancedConfiguration_Advanced
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel2.Controls.Add(Me.txtConfiguration)
        Me.Panel2.Controls.Add(Me.btnPaste)
        Me.Panel2.Controls.Add(Me.btnImport)
        Me.Panel2.Controls.Add(Me.Label4)
        Me.BorderPanel.SetFlowBreak(Me.Panel2, True)
        Me.Panel2.Location = New System.Drawing.Point(13, 150)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(3, 20, 3, 10)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Padding = New System.Windows.Forms.Padding(5, 20, 0, 0)
        Me.Panel2.Size = New System.Drawing.Size(508, 342)
        Me.Panel2.TabIndex = 4
        '
        'txtConfiguration
        '
        Me.txtConfiguration.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtConfiguration.Location = New System.Drawing.Point(12, 40)
        Me.txtConfiguration.Margin = New System.Windows.Forms.Padding(3, 3, 3, 20)
        Me.txtConfiguration.Multiline = True
        Me.txtConfiguration.Name = "txtConfiguration"
        Me.txtConfiguration.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtConfiguration.Size = New System.Drawing.Size(486, 237)
        Me.txtConfiguration.TabIndex = 1
        '
        'btnPaste
        '
        Me.btnPaste.BackColor = System.Drawing.Color.White
        Me.btnPaste.ForeColor = System.Drawing.Color.Black
        Me.btnPaste.Location = New System.Drawing.Point(326, 295)
        Me.btnPaste.Margin = New System.Windows.Forms.Padding(10, 3, 10, 3)
        Me.btnPaste.Name = "btnPaste"
        Me.btnPaste.Size = New System.Drawing.Size(172, 32)
        Me.btnPaste.TabIndex = 3
        Me.btnPaste.Text = Global.AutomateUI.My.Resources.Resources.AdvancedDataPipeLineConfig_Paste
        Me.btnPaste.UseVisualStyleBackColor = False
        '
        'btnImport
        '
        Me.btnImport.BackColor = System.Drawing.Color.White
        Me.btnImport.ForeColor = System.Drawing.Color.Black
        Me.btnImport.Location = New System.Drawing.Point(138, 295)
        Me.btnImport.Margin = New System.Windows.Forms.Padding(10, 2, 10, 3)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(168, 32)
        Me.btnImport.TabIndex = 2
        Me.btnImport.Text = Global.AutomateUI.My.Resources.Resources.AdvancedDataPipeLineConfig_Import
        Me.btnImport.UseVisualStyleBackColor = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(9, 20)
        Me.Label4.Margin = New System.Windows.Forms.Padding(20, 10, 10, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(119, 13)
        Me.Label4.TabIndex = 0
        Me.Label4.Text = My.Resources.DataGatewaysAdvancedConfiguration_OutputConfiguration
        '
        'AdvancedConfigurationForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CausesValidation = False
        Me.ClientSize = New System.Drawing.Size(535, 565)
        Me.ControlBox = False
        Me.Controls.Add(Me.BorderPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "AdvancedConfigurationForm"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "AdvancedConfigurationForm"
        CType(Me.btnClose, System.ComponentModel.ISupportInitialize).EndInit()
        Me.BorderPanel.ResumeLayout(False)
        Me.BorderPanel.PerformLayout()
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnSave As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label1 As Label
    Friend WithEvents BorderPanel As FlowLayoutPanel
    Friend WithEvents btnClose As PictureBox
    Friend WithEvents Panel2 As Panel
    Friend WithEvents btnPaste As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnImport As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label4 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents txtConfigurationName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txtConfiguration As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents imgExclamation As PictureBox
    Friend WithEvents lblInvalidConfigName As Label
    Friend WithEvents lblFlowBreak As Label
End Class
