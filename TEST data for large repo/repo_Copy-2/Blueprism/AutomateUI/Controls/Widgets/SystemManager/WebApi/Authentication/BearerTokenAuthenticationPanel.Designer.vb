Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class BearerTokenAuthenticationPanel
        Inherits UserControl

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
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
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(BearerTokenAuthenticationPanel))
        Me.ttDescription = New System.Windows.Forms.ToolTip(Me.components)
        Me.ctlCredentialPanel = New AutomateUI.Controls.Widgets.SystemManager.WebApi.Authentication.AuthenticationCredentialPanel()
        Me.SuspendLayout
        '
        'ctlCredentialPanel
        '
        resources.ApplyResources(Me.ctlCredentialPanel, "ctlCredentialPanel")
        Me.ctlCredentialPanel.DefaultParameterName = Nothing
        Me.ctlCredentialPanel.Name = "ctlCredentialPanel"
        '
        'BearerTokenAuthenticationPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.ctlCredentialPanel)
        Me.Name = "BearerTokenAuthenticationPanel"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)

End Sub
        Friend WithEvents ttDescription As ToolTip
        Friend WithEvents ctlCredentialPanel As AuthenticationCredentialPanel
    End Class
End NameSpace