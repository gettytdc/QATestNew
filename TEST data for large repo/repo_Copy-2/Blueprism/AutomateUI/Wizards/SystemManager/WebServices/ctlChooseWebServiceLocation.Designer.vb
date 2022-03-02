Imports AutomateControls.Wizard

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlChooseWebServiceLocation
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChooseWebServiceLocation))
        Me.pbDownload = New System.Windows.Forms.ProgressBar()
        Me.txtUrl = New AutomateControls.Textboxes.StyledTextBox()
        Me.lUrl = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'pbDownload
        '
        resources.ApplyResources(Me.pbDownload, "pbDownload")
        Me.pbDownload.Name = "pbDownload"
        '
        'txtUrl
        '
        resources.ApplyResources(Me.txtUrl, "txtUrl")
        Me.txtUrl.Name = "txtUrl"
        '
        'lUrl
        '
        resources.ApplyResources(Me.lUrl, "lUrl")
        Me.lUrl.Name = "lUrl"
        '
        'ctlChooseWebServiceLocation
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.pbDownload)
        Me.Controls.Add(Me.txtUrl)
        Me.Controls.Add(Me.lUrl)
        Me.Name = "ctlChooseWebServiceLocation"
        Me.Title = Global.AutomateUI.My.Resources.ctlChooseWebServiceLocation_ChooseAWebServiceToIntegrateIntoBluePrism
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents pbDownload As System.Windows.Forms.ProgressBar
    Friend WithEvents txtUrl As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lUrl As System.Windows.Forms.Label

End Class
