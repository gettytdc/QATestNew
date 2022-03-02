Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OAuth2AuthenticationPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(OAuth2AuthenticationPanel))
            Me.txtAuthorisationURI = New AutomateControls.Textboxes.StyledTextBox()
            Me.txtScope = New AutomateControls.Textboxes.StyledTextBox()
            Me.lblURI = New System.Windows.Forms.Label()
        Me.lblScope = New System.Windows.Forms.Label()
        Me.ctlCredentialPanel = New AutomateUI.Controls.Widgets.SystemManager.WebApi.Authentication.AuthenticationCredentialPanel()
        Me.SuspendLayout
        '
        'txtAuthorisationURI
        '
        resources.ApplyResources(Me.txtAuthorisationURI, "txtAuthorisationURI")
        Me.txtAuthorisationURI.Name = "txtAuthorisationURI"
        '
        'txtScope
        '
        resources.ApplyResources(Me.txtScope, "txtScope")
        Me.txtScope.Name = "txtScope"
        '
        'lblURI
        '
        resources.ApplyResources(Me.lblURI, "lblURI")
        Me.lblURI.Name = "lblURI"
        '
        'lblScope
        '
        resources.ApplyResources(Me.lblScope, "lblScope")
        Me.lblScope.Name = "lblScope"
        '
        'ctlCredentialPanel
        '
        resources.ApplyResources(Me.ctlCredentialPanel, "ctlCredentialPanel")
        Me.ctlCredentialPanel.DefaultParameterName = Nothing
        Me.ctlCredentialPanel.Name = "ctlCredentialPanel"
        '
        'OAuth2AuthenticationPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblScope)
        Me.Controls.Add(Me.lblURI)
        Me.Controls.Add(Me.txtScope)
        Me.Controls.Add(Me.txtAuthorisationURI)
        Me.Controls.Add(Me.ctlCredentialPanel)
        Me.Name = "OAuth2AuthenticationPanel"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

        Friend WithEvents ctlCredentialPanel As AuthenticationCredentialPanel
        Friend WithEvents txtAuthorisationURI As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents txtScope As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents lblURI As Label
        Friend WithEvents lblScope As Label
    End Class
End namespace
