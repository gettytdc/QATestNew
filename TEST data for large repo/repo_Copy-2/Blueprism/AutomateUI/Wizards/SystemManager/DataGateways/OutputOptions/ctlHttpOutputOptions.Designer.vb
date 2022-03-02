Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlHttpOutputOptions
    Inherits UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                RemoveHandler cmbHttpMethod.SelectedIndexChanged, AddressOf cmbHttpMethod_SelectedIndexChanged
                RemoveHandler cmbCredential.SelectedIndexChanged, AddressOf cmbCredential_SelectedIndexChanged
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
        Me.txtUrl = New AutomateControls.Textboxes.StyledTextBox()
        Me.cmbCredential = New System.Windows.Forms.ComboBox()
        Me.cmbHttpMethod = New System.Windows.Forms.ComboBox()
        Me.lblCredential = New System.Windows.Forms.Label()
        Me.lblHttpMethod = New System.Windows.Forms.Label()
        Me.lblUrl = New System.Windows.Forms.Label()
        Me.lblUrlValidation = New System.Windows.Forms.Label()
        Me.imgExclamation = New System.Windows.Forms.PictureBox()
        CType(Me.imgExclamation,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'txtUrl
        '
        Me.txtUrl.Location = New System.Drawing.Point(10, 28)
        Me.txtUrl.Name = "txtUrl"
        Me.txtUrl.Size = New System.Drawing.Size(480, 22)
        Me.txtUrl.TabIndex = 5
        '
        'cmbCredential
        '
        Me.cmbCredential.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCredential.FormattingEnabled = true
        Me.cmbCredential.Location = New System.Drawing.Point(10, 149)
        Me.cmbCredential.Name = "cmbCredential"
        Me.cmbCredential.Size = New System.Drawing.Size(240, 21)
        Me.cmbCredential.Sorted = true
        Me.cmbCredential.TabIndex = 4
        '
        'cmbHttpMethod
        '
        Me.cmbHttpMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbHttpMethod.FormattingEnabled = true
        Me.cmbHttpMethod.Location = New System.Drawing.Point(10, 89)
        Me.cmbHttpMethod.Name = "cmbHttpMethod"
        Me.cmbHttpMethod.Size = New System.Drawing.Size(240, 21)
        Me.cmbHttpMethod.TabIndex = 3
        '
        'lblCredential
        '
        Me.lblCredential.AutoSize = true
        Me.lblCredential.Location = New System.Drawing.Point(7, 127)
        Me.lblCredential.Name = "lblCredential"
        Me.lblCredential.Size = New System.Drawing.Size(60, 13)
        Me.lblCredential.TabIndex = 2
        Me.lblCredential.Text = Global.AutomateUI.My.Resources.Resources.ctlHttpOutputOptions_lblCredential
        '
        'lblHttpMethod
        '
        Me.lblHttpMethod.AutoSize = true
        Me.lblHttpMethod.Location = New System.Drawing.Point(7, 68)
        Me.lblHttpMethod.Name = "lblHttpMethod"
        Me.lblHttpMethod.Size = New System.Drawing.Size(75, 13)
        Me.lblHttpMethod.TabIndex = 1
        Me.lblHttpMethod.Text = Global.AutomateUI.My.Resources.Resources.ctlHttpOutputOptions_lblHttpMethod
        '
        'lblUrl
        '
        Me.lblUrl.AutoSize = true
        Me.lblUrl.Location = New System.Drawing.Point(7, 8)
        Me.lblUrl.Name = "lblUrl"
        Me.lblUrl.Size = New System.Drawing.Size(27, 13)
        Me.lblUrl.TabIndex = 0
        Me.lblUrl.Text = Global.AutomateUI.My.Resources.Resources.ctlHttpOutputOptions_lblUrl
        '
        'lblUrlValidation
        '
        Me.lblUrlValidation.AutoSize = true
        Me.lblUrlValidation.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblUrlValidation.Location = New System.Drawing.Point(518, 32)
        Me.lblUrlValidation.Name = "lblUrlValidation"
        Me.lblUrlValidation.Size = New System.Drawing.Size(64, 13)
        Me.lblUrlValidation.TabIndex = 6
        Me.lblUrlValidation.Text = Global.AutomateUI.My.Resources.Resources.ctlHttpOutputOptions_invalidUrl
        '
        'imgExclamation
        '
        Me.imgExclamation.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16_Hot
        Me.imgExclamation.Location = New System.Drawing.Point(496, 30)
        Me.imgExclamation.Name = "imgExclamation"
        Me.imgExclamation.Size = New System.Drawing.Size(16, 16)
        Me.imgExclamation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.imgExclamation.TabIndex = 14
        Me.imgExclamation.TabStop = false
        '
        'ctlHttpOutputOptions
        '
        Me.Font =  DataPipelineOutputConfigUISettings.StandardFont
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.imgExclamation)
        Me.Controls.Add(Me.lblUrlValidation)
        Me.Controls.Add(Me.cmbCredential)
        Me.Controls.Add(Me.lblCredential)
        Me.Controls.Add(Me.lblHttpMethod)
        Me.Controls.Add(Me.cmbHttpMethod)
        Me.Controls.Add(Me.lblUrl)
        Me.Controls.Add(Me.txtUrl)
        Me.Name = "ctlHttpOutputOptions"
        Me.Size = New System.Drawing.Size(733, 179)
        CType(Me.imgExclamation,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents txtUrl As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents cmbCredential As ComboBox
    Friend WithEvents cmbHttpMethod As ComboBox
    Friend WithEvents lblCredential As Label
    Friend WithEvents lblHttpMethod As Label
    Friend WithEvents lblUrl As Label
    Friend WithEvents lblUrlValidation As Label
    Friend WithEvents imgExclamation As PictureBox
End Class
