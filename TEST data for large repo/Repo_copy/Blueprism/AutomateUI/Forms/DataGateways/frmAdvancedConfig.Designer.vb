<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmAdvancedConfig
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
        Me.btnSave = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ctlConfigEditor = New AutomateUI.ctlCodeEditor()
        Me.Panel = New System.Windows.Forms.Panel()
        Me.btnClose = New System.Windows.Forms.PictureBox()
        Me.Panel.SuspendLayout()
        CType(Me.btnClose, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(720, 515)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(10, 3, 10, 3)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(244, 32)
        Me.btnSave.TabIndex = 18
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(442, 515)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(10, 3, 10, 3)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(244, 32)
        Me.btnCancel.TabIndex = 19
        Me.btnCancel.Text = My.Resources.frmAdvancedConfig_btnCancelText
        Me.btnCancel.UseVisualStyleBackColor = False
        Me.btnCancel.Visible = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(15, 36)
        Me.Label1.Margin = New System.Windows.Forms.Padding(7, 0, 10, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(319, 25)
        Me.Label1.TabIndex = 20
        Me.Label1.Text = My.Resources.frmAdvancedConfig_lblCustomConfig
        '
        'ctlConfigEditor
        '
        Me.ctlConfigEditor.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlConfigEditor.BackgroundColour = System.Drawing.SystemColors.Window
        Me.ctlConfigEditor.Code = ""
        Me.ctlConfigEditor.Location = New System.Drawing.Point(12, 74)
        Me.ctlConfigEditor.Name = "ctlConfigEditor"
        Me.ctlConfigEditor.ReadOnly = True
        Me.ctlConfigEditor.Size = New System.Drawing.Size(952, 433)
        Me.ctlConfigEditor.TabIndex = 0
        '
        'Panel
        '
        Me.Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel.Controls.Add(Me.btnClose)
        Me.Panel.Controls.Add(Me.Label1)
        Me.Panel.Controls.Add(Me.btnCancel)
        Me.Panel.Controls.Add(Me.btnSave)
        Me.Panel.Controls.Add(Me.ctlConfigEditor)
        Me.Panel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel.Location = New System.Drawing.Point(0, 0)
        Me.Panel.Name = "Panel"
        Me.Panel.Size = New System.Drawing.Size(978, 560)
        Me.Panel.TabIndex = 21
        '
        'btnClose
        '
        Me.btnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnClose.Image = Global.AutomateUI.ActivationWizardResources.Close_32x32
        Me.btnClose.Location = New System.Drawing.Point(936, 15)
        Me.btnClose.Margin = New System.Windows.Forms.Padding(475, 15, 15, 0)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(28, 28)
        Me.btnClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.btnClose.TabIndex = 21
        Me.btnClose.TabStop = False
        '
        'frmAdvancedConfig
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(978, 560)
        Me.Controls.Add(Me.Panel)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAdvancedConfig"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = My.Resources.frmAdvancedConfig_lblCustomConfig
        Me.Panel.ResumeLayout(False)
        Me.Panel.PerformLayout()
        CType(Me.btnClose, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ctlConfigEditor As ctlCodeEditor
    Friend WithEvents btnSave As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel As Panel
    Friend WithEvents btnClose As PictureBox
End Class
