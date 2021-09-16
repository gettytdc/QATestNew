Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlDatabaseOutputOptions
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
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.imgExclamation = New System.Windows.Forms.PictureBox()
        Me.lblInvalidTableName = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtServer = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtDatabaseName = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblTableName = New System.Windows.Forms.Label()
        Me.txtTableName = New AutomateControls.Textboxes.StyledTextBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.cboCredential = New System.Windows.Forms.ComboBox()
        Me.radCredential = New AutomateControls.StyledRadioButton()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.radIntegratedSecurity = New AutomateControls.StyledRadioButton()
        Me.Panel2.SuspendLayout()
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.imgExclamation)
        Me.Panel2.Controls.Add(Me.lblInvalidTableName)
        Me.Panel2.Controls.Add(Me.Label7)
        Me.Panel2.Controls.Add(Me.Label1)
        Me.Panel2.Controls.Add(Me.txtServer)
        Me.Panel2.Controls.Add(Me.Label2)
        Me.Panel2.Controls.Add(Me.txtDatabaseName)
        Me.Panel2.Controls.Add(Me.lblTableName)
        Me.Panel2.Controls.Add(Me.txtTableName)
        Me.Panel2.Location = New System.Drawing.Point(0, 0)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(628, 189)
        Me.Panel2.TabIndex = 14
        '
        'imgExclamation
        '
        Me.imgExclamation.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16_Hot
        Me.imgExclamation.Location = New System.Drawing.Point(264, 154)
        Me.imgExclamation.Name = "imgExclamation"
        Me.imgExclamation.Size = New System.Drawing.Size(16, 16)
        Me.imgExclamation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.imgExclamation.TabIndex = 8
        Me.imgExclamation.TabStop = False
        '
        'lblInvalidTableName
        '
        Me.lblInvalidTableName.AutoSize = True
        Me.lblInvalidTableName.Location = New System.Drawing.Point(286, 156)
        Me.lblInvalidTableName.Name = "lblInvalidTableName"
        Me.lblInvalidTableName.Size = New System.Drawing.Size(203, 13)
        Me.lblInvalidTableName.TabIndex = 7
        Me.lblInvalidTableName.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_InvalidTableName
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = DataPipelineOutputConfigUISettings.BoldFont
        Me.Label7.Location = New System.Drawing.Point(7, 8)
        Me.Label7.Margin = New System.Windows.Forms.Padding(3, 0, 3, 8)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(105, 13)
        Me.Label7.TabIndex = 0
        Me.Label7.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_ConnectionDetails
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(11, 27)
        Me.Label1.Margin = New System.Windows.Forms.Padding(15, 5, 50, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_Server
        '
        'txtServer
        '
        Me.txtServer.Location = New System.Drawing.Point(15, 48)
        Me.txtServer.Margin = New System.Windows.Forms.Padding(19, 3, 3, 3)
        Me.txtServer.Name = "txtServer"
        Me.txtServer.Size = New System.Drawing.Size(240, 22)
        Me.txtServer.TabIndex = 2
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 77)
        Me.Label2.Margin = New System.Windows.Forms.Padding(15, 5, 20, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(87, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_DatabaseName
        '
        'txtDatabaseName
        '
        Me.txtDatabaseName.Location = New System.Drawing.Point(14, 98)
        Me.txtDatabaseName.Name = "txtDatabaseName"
        Me.txtDatabaseName.Size = New System.Drawing.Size(240, 22)
        Me.txtDatabaseName.TabIndex = 4
        '
        'lblTableName
        '
        Me.lblTableName.AutoSize = True
        Me.lblTableName.Location = New System.Drawing.Point(12, 128)
        Me.lblTableName.Name = "lblTableName"
        Me.lblTableName.Size = New System.Drawing.Size(65, 13)
        Me.lblTableName.TabIndex = 5
        Me.lblTableName.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_TableName
        '
        'txtTableName
        '
        Me.txtTableName.Location = New System.Drawing.Point(15, 151)
        Me.txtTableName.Name = "txtTableName"
        Me.txtTableName.Size = New System.Drawing.Size(240, 22)
        Me.txtTableName.TabIndex = 6
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.cboCredential)
        Me.Panel1.Controls.Add(Me.radCredential)
        Me.Panel1.Controls.Add(Me.Label6)
        Me.Panel1.Controls.Add(Me.radIntegratedSecurity)
        Me.Panel1.Location = New System.Drawing.Point(0, 195)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(628, 114)
        Me.Panel1.TabIndex = 13
        '
        'cboCredential
        '
        Me.cboCredential.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCredential.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.cboCredential.FormattingEnabled = True
        Me.cboCredential.Location = New System.Drawing.Point(14, 76)
        Me.cboCredential.Name = "cboCredential"
        Me.cboCredential.Size = New System.Drawing.Size(240, 21)
        Me.cboCredential.TabIndex = 3
        '
        'radCredential
        '
        Me.radCredential.AutoSize = True
        Me.radCredential.Location = New System.Drawing.Point(15, 53)
        Me.radCredential.Name = "radCredential"
        Me.radCredential.Size = New System.Drawing.Size(152, 17)
        Me.radCredential.TabIndex = 2
        Me.radCredential.TabStop = True
        Me.radCredential.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_Credential
        Me.radCredential.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = DataPipelineOutputConfigUISettings.BoldFont
        Me.Label6.Location = New System.Drawing.Point(7, 8)
        Me.Label6.Margin = New System.Windows.Forms.Padding(3, 5, 3, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(48, 13)
        Me.Label6.TabIndex = 0
        Me.Label6.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_Security
        '
        'radIntegratedSecurity
        '
        Me.radIntegratedSecurity.AutoSize = True
        Me.radIntegratedSecurity.Checked = True
        Me.radIntegratedSecurity.Location = New System.Drawing.Point(15, 30)
        Me.radIntegratedSecurity.Name = "radIntegratedSecurity"
        Me.radIntegratedSecurity.Size = New System.Drawing.Size(122, 17)
        Me.radIntegratedSecurity.TabIndex = 1
        Me.radIntegratedSecurity.TabStop = True
        Me.radIntegratedSecurity.Text = Global.AutomateUI.My.Resources.Resources.DataGatewayDatabaseOuptut_IntegratedSecurity
        Me.radIntegratedSecurity.UseVisualStyleBackColor = True
        '
        'ctlDatabaseOutputOptions
        '
        Me.Font = DataPipelineOutputConfigUISettings.StandardFont
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.AutoSize = True
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "ctlDatabaseOutputOptions"
        Me.Size = New System.Drawing.Size(631, 312)
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        CType(Me.imgExclamation, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel2 As Panel
    Friend WithEvents Label7 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents txtServer As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtDatabaseName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblTableName As Label
    Friend WithEvents txtTableName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents cboCredential As ComboBox
    Friend WithEvents radCredential As RadioButton
    Friend WithEvents Label6 As Label
    Friend WithEvents radIntegratedSecurity As RadioButton
    Friend WithEvents imgExclamation As PictureBox
    Friend WithEvents lblInvalidTableName As Label
End Class
