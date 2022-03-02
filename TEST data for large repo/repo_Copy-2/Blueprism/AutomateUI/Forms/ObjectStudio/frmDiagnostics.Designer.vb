<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Friend Class frmDiagnostics
    Inherits frmForm

    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDiagnostics))
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.SuspendLayout
        '
        'btnOK
        '
        Me.btnOK.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = false
        '
        'btnCancel
        '
        Me.btnCancel.BackColor = System.Drawing.Color.White
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = false
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.BackColor = System.Drawing.Color.White
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.UseVisualStyleBackColor = false
        '
        'frmDiagnostics
        '
        Me.AcceptButton = Me.btnOK
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "frmDiagnostics"
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Private components As IContainer
End Class
