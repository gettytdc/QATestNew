Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OAuth2WithJwtAuthenticationPanel
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(OAuth2WithJwtAuthenticationPanel))
            Me.txtAlgorithm = New AutomateControls.Textboxes.StyledTextBox()
            Me.txtSubject = New AutomateControls.Textboxes.StyledTextBox()
            Me.lblAlgorithm = New System.Windows.Forms.Label()
        Me.lblSubject = New System.Windows.Forms.Label()
        Me.lblAudience = New System.Windows.Forms.Label()
            Me.txtAudience = New AutomateControls.Textboxes.StyledTextBox()
            Me.lblScope = New System.Windows.Forms.Label()
            Me.txtScope = New AutomateControls.Textboxes.StyledTextBox()
            Me.numExpiry = New AutomateControls.StyledNumericUpDown()
        Me.lblExpiry = New System.Windows.Forms.Label()
        Me.lblUri = New System.Windows.Forms.Label()
            Me.txtAuthorizationUri = New AutomateControls.Textboxes.StyledTextBox()
            Me.ctlToolTip = New System.Windows.Forms.ToolTip(Me.components)
            Me.ctlCredentialPanel = New AutomateUI.Controls.Widgets.SystemManager.WebApi.Authentication.AuthenticationCredentialPanel()
            CType(Me.numExpiry, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'txtAlgorithm
            '
            resources.ApplyResources(Me.txtAlgorithm, "txtAlgorithm")
            Me.txtAlgorithm.Name = "txtAlgorithm"
            '
            'txtSubject
            '
            resources.ApplyResources(Me.txtSubject, "txtSubject")
            Me.txtSubject.Name = "txtSubject"
            Me.ctlToolTip.SetToolTip(Me.txtSubject, resources.GetString("txtSubject.ToolTip"))
            '
            'lblAlgorithm
            '
            resources.ApplyResources(Me.lblAlgorithm, "lblAlgorithm")
            Me.lblAlgorithm.Name = "lblAlgorithm"
            '
            'lblSubject
            '
            resources.ApplyResources(Me.lblSubject, "lblSubject")
            Me.lblSubject.Name = "lblSubject"
            '
            'lblAudience
            '
            resources.ApplyResources(Me.lblAudience, "lblAudience")
            Me.lblAudience.Name = "lblAudience"
            '
            'txtAudience
            '
            resources.ApplyResources(Me.txtAudience, "txtAudience")
            Me.txtAudience.Name = "txtAudience"
            '
            'lblScope
            '
            resources.ApplyResources(Me.lblScope, "lblScope")
            Me.lblScope.Name = "lblScope"
            '
            'txtScope
            '
            resources.ApplyResources(Me.txtScope, "txtScope")
            Me.txtScope.Name = "txtScope"
            '
            'numExpiry
            '
            resources.ApplyResources(Me.numExpiry, "numExpiry")
            Me.numExpiry.Maximum = New Decimal(New Integer() {3600, 0, 0, 0})
            Me.numExpiry.Name = "numExpiry"
            Me.numExpiry.Value = New Decimal(New Integer() {3600, 0, 0, 0})
            '
            'lblExpiry
            '
            resources.ApplyResources(Me.lblExpiry, "lblExpiry")
            Me.lblExpiry.Name = "lblExpiry"
            '
            'lblUri
            '
            resources.ApplyResources(Me.lblUri, "lblUri")
            Me.lblUri.Name = "lblUri"
            '
            'txtAuthorizationUri
            '
            resources.ApplyResources(Me.txtAuthorizationUri, "txtAuthorizationUri")
            Me.txtAuthorizationUri.Name = "txtAuthorizationUri"
            '
            'ctlCredentialPanel
            '
            resources.ApplyResources(Me.ctlCredentialPanel, "ctlCredentialPanel")
            Me.ctlCredentialPanel.DefaultParameterName = Nothing
            Me.ctlCredentialPanel.Name = "ctlCredentialPanel"
            '
            'OAuth2WithJwtAuthenticationPanel
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
            Me.Controls.Add(Me.lblUri)
            Me.Controls.Add(Me.txtAuthorizationUri)
            Me.Controls.Add(Me.lblExpiry)
            Me.Controls.Add(Me.numExpiry)
            Me.Controls.Add(Me.lblScope)
            Me.Controls.Add(Me.txtScope)
            Me.Controls.Add(Me.lblAudience)
            Me.Controls.Add(Me.txtAudience)
            Me.Controls.Add(Me.lblSubject)
            Me.Controls.Add(Me.lblAlgorithm)
            Me.Controls.Add(Me.txtSubject)
            Me.Controls.Add(Me.txtAlgorithm)
            Me.Controls.Add(Me.ctlCredentialPanel)
            Me.Name = "OAuth2WithJwtAuthenticationPanel"
            CType(Me.numExpiry, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents ctlCredentialPanel As AuthenticationCredentialPanel
        Friend WithEvents txtAlgorithm As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents txtSubject As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents lblAlgorithm As Label
        Friend WithEvents lblSubject As Label
        Friend WithEvents lblAudience As Label
        Friend WithEvents txtAudience As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents lblScope As Label
        Friend WithEvents txtScope As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents numExpiry As AutomateControls.StyledNumericUpDown
        Friend WithEvents lblExpiry As Label
        Friend WithEvents lblUri As Label
        Friend WithEvents txtAuthorizationUri As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents ctlToolTip As ToolTip
    End Class
End namespace
