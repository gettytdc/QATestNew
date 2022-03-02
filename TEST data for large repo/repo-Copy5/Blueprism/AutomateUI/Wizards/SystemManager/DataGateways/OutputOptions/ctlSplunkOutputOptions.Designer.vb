Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlSplunkOutputOptions
    Inherits UserControl

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
        Me.lblURL = New System.Windows.Forms.Label()
        Me.txtURL = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblToken = New System.Windows.Forms.Label()
        Me.txtToken = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnPaste = New AutomateControls.Buttons.StandardStyledButton()
        Me.imgExclamation = New System.Windows.Forms.PictureBox()
        Me.lblInvalidURL = New System.Windows.Forms.Label()
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblURL
        '
        Me.lblURL.AutoSize = True
        Me.lblURL.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.lblURL.Location = New System.Drawing.Point(7, 8)
        Me.lblURL.Margin = New System.Windows.Forms.Padding(10, 8, 40, 0)
        Me.lblURL.Name = "lblURL"
        Me.lblURL.Size = New System.Drawing.Size(66, 13)
        Me.lblURL.TabIndex = 7
        Me.lblURL.Text = My.Resources.DataGateway_SplunkOuput_URL
        '
        'txtURL
        '
        Me.txtURL.Location = New System.Drawing.Point(10, 28)
        Me.txtURL.Margin = New System.Windows.Forms.Padding(10, 2, 3, 3)
        Me.txtURL.Name = "txtURL"
        Me.txtURL.Size = New System.Drawing.Size(480, 22)
        Me.txtURL.TabIndex = 9
        '
        'lblToken
        '
        Me.lblToken.AutoSize = True
        Me.lblToken.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.lblToken.Location = New System.Drawing.Point(7, 65)
        Me.lblToken.Margin = New System.Windows.Forms.Padding(10, 10, 10, 0)
        Me.lblToken.Name = "lblToken"
        Me.lblToken.Size = New System.Drawing.Size(95, 13)
        Me.lblToken.TabIndex = 8
        Me.lblToken.Text = My.Resources.DataGateway_SplunkOuput_Token
        '
        'txtToken
        '
        Me.txtToken.Location = New System.Drawing.Point(10, 86)
        Me.txtToken.Margin = New System.Windows.Forms.Padding(10, 2, 10, 3)
        Me.txtToken.Name = "txtToken"
        Me.txtToken.Size = New System.Drawing.Size(480, 22)
        Me.txtToken.TabIndex = 10
        '
        'btnPaste
        '
        Me.btnPaste.BackColor = System.Drawing.SystemColors.Control
        Me.btnPaste.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.btnPaste.Location = New System.Drawing.Point(496, 85)
        Me.btnPaste.Margin = New System.Windows.Forms.Padding(3, 0, 3, 3)
        Me.btnPaste.Name = "btnPaste"
        Me.btnPaste.Size = New System.Drawing.Size(190, 24)
        Me.btnPaste.TabIndex = 11
        Me.btnPaste.Text = Global.AutomateUI.My.Resources.Resources.DataGateway_SplunkOuput_Paste
        Me.btnPaste.UseVisualStyleBackColor = False
        '
        'imgExclamation
        '
        Me.imgExclamation.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16_Hot
        Me.imgExclamation.Location = New System.Drawing.Point(496, 29)
        Me.imgExclamation.Name = "imgExclamation"
        Me.imgExclamation.Size = New System.Drawing.Size(16, 16)
        Me.imgExclamation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.imgExclamation.TabIndex = 13
        Me.imgExclamation.TabStop = False
        '
        'lblInvalidURL
        '
        Me.lblInvalidURL.AutoSize = True
        Me.lblInvalidURL.Location = New System.Drawing.Point(518, 31)
        Me.lblInvalidURL.Name = "lblInvalidURL"
        Me.lblInvalidURL.Size = New System.Drawing.Size(64, 13)
        Me.lblInvalidURL.TabIndex = 12
        Me.lblInvalidURL.Text = My.Resources.DataGateway_SplunkOuput_InvalidURL
        '
        'ctlSplunkOutputOptions
        '
        Me.Font = DataPipelineOutputConfigUISettings.StandardFont
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.imgExclamation)
        Me.Controls.Add(Me.lblInvalidURL)
        Me.Controls.Add(Me.lblURL)
        Me.Controls.Add(Me.txtURL)
        Me.Controls.Add(Me.lblToken)
        Me.Controls.Add(Me.txtToken)
        Me.Controls.Add(Me.btnPaste)
        Me.Name = "ctlSplunkOutputOptions"
        Me.Size = New System.Drawing.Size(689, 125)
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblURL As Label
    Friend WithEvents txtURL As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblToken As Label
    Friend WithEvents txtToken As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnPaste As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents imgExclamation As PictureBox
    Friend WithEvents lblInvalidURL As Label
End Class
