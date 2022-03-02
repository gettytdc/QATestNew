<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStagedWizard

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagedWizard))
        Me.panContents = New System.Windows.Forms.Panel()
        Me.SuspendLayout()
        '
        'panContents
        '
        resources.ApplyResources(Me.panContents, "panContents")
        Me.panContents.Name = "panContents"
        '
        'frmStagedWizard
        '
        resources.ApplyResources(Me, My.Resources.frmStagedWizard_This)
        Me.Controls.Add(Me.panContents)
        Me.Name = "frmStagedWizard"
        Me.Controls.SetChildIndex(Me.panContents, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.ResumeLayout(False)

    End Sub
    Protected WithEvents panContents As System.Windows.Forms.Panel

End Class
