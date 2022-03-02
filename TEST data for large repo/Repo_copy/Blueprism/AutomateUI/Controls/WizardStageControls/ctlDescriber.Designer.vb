<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDescriber

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDescriber))
        Me.lblPrompt = New System.Windows.Forms.Label()
        Me.txtDescription = New AutomateControls.ActivatingTextBox()
        Me.SuspendLayout()
        '
        'lblPrompt
        '
        resources.ApplyResources(Me.lblPrompt, "lblPrompt")
        Me.lblPrompt.Name = "lblPrompt"
        '
        'txtDescription
        '
        Me.txtDescription.AcceptsReturn = True
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        '
        'ctlDescriber
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblPrompt)
        Me.Controls.Add(Me.txtDescription)
        Me.Name = "ctlDescriber"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents txtDescription As AutomateControls.ActivatingTextBox
    Private WithEvents lblPrompt As System.Windows.Forms.Label

End Class
