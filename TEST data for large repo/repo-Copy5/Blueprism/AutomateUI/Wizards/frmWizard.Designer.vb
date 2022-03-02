Imports AutomateControls

Partial Friend Class frmWizard
    Inherits Forms.TitledHelpButtonForm
    Implements IHelp

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.

    Protected WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents btnNext As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents btnBack As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents HelpProvider1 As System.Windows.Forms.HelpProvider
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmWizard))
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnNext = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnBack = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.HelpProvider1 = New System.Windows.Forms.HelpProvider()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.BackColor = System.Drawing.Color.White
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        Me.btnNext.BackColor = System.Drawing.Color.White
        Me.btnNext.Name = "btnNext"
        Me.btnNext.UseVisualStyleBackColor = False
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        Me.btnBack.BackColor = System.Drawing.Color.White
        Me.btnBack.Name = "btnBack"
        Me.btnBack.UseVisualStyleBackColor = False
        '
        'frmWizard
        '
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnNext)
        Me.Controls.Add(Me.btnBack)
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmWizard"
        Me.ResumeLayout(False)

    End Sub
End Class
