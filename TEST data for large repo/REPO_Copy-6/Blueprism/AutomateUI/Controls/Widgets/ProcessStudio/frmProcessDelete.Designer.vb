<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmProcessDelete
    Inherits AutomateUI.frmWizard

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProcessDelete))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtDeleteReason = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.pbWarning = New System.Windows.Forms.PictureBox()
        Me.lblWarning = New System.Windows.Forms.Label()
        CType(Me.pbWarning, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'objBluebar
        '
        Me.objBluebar.Title = "Delete a process"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtDeleteReason
        '
        Me.txtDeleteReason.AcceptsReturn = True
        resources.ApplyResources(Me.txtDeleteReason, "txtDeleteReason")
        Me.txtDeleteReason.Name = "txtDeleteReason"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'pbWarning
        '
        Me.pbWarning.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16
        resources.ApplyResources(Me.pbWarning, "pbWarning")
        Me.pbWarning.Name = "pbWarning"
        Me.pbWarning.TabStop = False
        '
        'lblWarning
        '
        resources.ApplyResources(Me.lblWarning, "lblWarning")
        Me.lblWarning.Name = "lblWarning"
        '
        'frmProcessDelete
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblWarning)
        Me.Controls.Add(Me.pbWarning)
        Me.Controls.Add(Me.txtDeleteReason)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label2)
        Me.DefaultSingleStageNextText = Global.AutomateUI.My.Resources.frmProcessDelete_Delete '"Delete"
        Me.Name = "frmProcessDelete"
        Me.Title = "Delete a process"
        Me.Controls.SetChildIndex(Me.Label2, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.Label1, 0)
        Me.Controls.SetChildIndex(Me.txtDeleteReason, 0)
        Me.Controls.SetChildIndex(Me.pbWarning, 0)
        Me.Controls.SetChildIndex(Me.lblWarning, 0)
        CType(Me.pbWarning, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Private WithEvents txtDeleteReason As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents pbWarning As System.Windows.Forms.PictureBox
    Friend WithEvents lblWarning As System.Windows.Forms.Label

End Class
