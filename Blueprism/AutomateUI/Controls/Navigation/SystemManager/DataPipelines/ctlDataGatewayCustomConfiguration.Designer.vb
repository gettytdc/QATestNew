<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDataGatewayCustomConfiguration
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
        Me.ctlConfigEditor = New AutomateUI.ctlCodeEditor()
        Me.btnEdit = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDelete = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'ctlConfigEditor
        '
        Me.ctlConfigEditor.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.ctlConfigEditor.Code = ""
        Me.ctlConfigEditor.Location = New System.Drawing.Point(15, 79)
        Me.ctlConfigEditor.Name = "ctlConfigEditor"
        Me.ctlConfigEditor.ReadOnly = false
        Me.ctlConfigEditor.Size = New System.Drawing.Size(873, 460)
        Me.ctlConfigEditor.TabIndex = 1
        '
        'btnEdit
        '
        Me.btnEdit.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.btnEdit.Location = New System.Drawing.Point(644, 545)
        Me.btnEdit.Margin = New System.Windows.Forms.Padding(10, 3, 10, 3)
        Me.btnEdit.Name = "btnEdit"
        Me.btnEdit.Size = New System.Drawing.Size(244, 32)
        Me.btnEdit.TabIndex = 19
        Me.btnEdit.Text = My.Resources.ctlDataGatewayCustomConfiguration_btnEdit
        '
        'btnDelete
        '
        Me.btnDelete.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
        Me.btnDelete.Location = New System.Drawing.Point(394, 545)
        Me.btnDelete.Margin = New System.Windows.Forms.Padding(10, 3, 10, 3)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(244, 32)
        Me.btnDelete.TabIndex = 20
        Me.btnDelete.Text = My.Resources.ctlDataGatewayCustomConfiguration_btnDelete
        '
        'Label1
        '
        Me.Label1.AutoSize = true
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        Me.Label1.Location = New System.Drawing.Point(10, 41)
        Me.Label1.Margin = New System.Windows.Forms.Padding(7, 0, 10, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(198, 25)
        Me.Label1.TabIndex = 21
        Me.Label1.Text = My.Resources.ctlDataGatewayCustomConfiguration_lblCustomConfig
        '
        'ctlDataGatewayCustomConfiguration
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnEdit)
        Me.Controls.Add(Me.ctlConfigEditor)
        Me.Name = "ctlDataGatewayCustomConfiguration"
        Me.Size = New System.Drawing.Size(927, 597)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents ctlConfigEditor As ctlCodeEditor
    Friend WithEvents btnEdit As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDelete As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label1 As Label
End Class
