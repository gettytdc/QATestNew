Imports AutomateControls.Wizard

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlChooseWebServiceTimeout
    Inherits WizardPanel

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseWebServiceTimeout))
        Me.txtTimeout = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtTimeout
        '
        resources.ApplyResources(Me.txtTimeout, "txtTimeout")
        Me.txtTimeout.Name = "txtTimeout"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'ctlChooseWebServiceTimeout
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtTimeout)
        Me.Controls.Add(Me.Label1)
        Me.Name = "ctlChooseWebServiceTimeout"
        Me.NavigatePrevious = True
        Me.Title = Global.AutomateUI.My.Resources.ctlChooseWebServiceTimeout_PleaseSetTheTimeoutToUseWhenInteractingWithTheWebService
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtTimeout As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label

End Class
