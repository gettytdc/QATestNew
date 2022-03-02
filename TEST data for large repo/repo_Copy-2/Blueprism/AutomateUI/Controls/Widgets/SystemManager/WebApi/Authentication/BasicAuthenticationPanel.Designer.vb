Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class BasicAuthenticationPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(BasicAuthenticationPanel))
        Me.ttDescription = New System.Windows.Forms.ToolTip(Me.components)
        Me.chkPreEmptive = New System.Windows.Forms.CheckBox()
        Me.ctlCredentialPanel = New AutomateUI.Controls.Widgets.SystemManager.WebApi.Authentication.AuthenticationCredentialPanel()
        Me.SuspendLayout
        '
        'chkPreEmptive
        '
        resources.ApplyResources(Me.chkPreEmptive, "chkPreEmptive")
        Me.chkPreEmptive.Name = "chkPreEmptive"
        Me.ttDescription.SetToolTip(Me.chkPreEmptive, resources.GetString("chkPreEmptive.ToolTip"))
        Me.chkPreEmptive.UseVisualStyleBackColor = true
        '
        'ctlCredentialPanel
        '
        resources.ApplyResources(Me.ctlCredentialPanel, "ctlCredentialPanel")
        Me.ctlCredentialPanel.DefaultParameterName = Nothing
        Me.ctlCredentialPanel.Name = "ctlCredentialPanel"
        '
        'BasicAuthenticationPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.ctlCredentialPanel)
        Me.Controls.Add(Me.chkPreEmptive)
        Me.Name = "BasicAuthenticationPanel"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
        Friend WithEvents ttDescription As ToolTip
        Friend WithEvents chkPreEmptive As CheckBox
        Friend WithEvents ctlCredentialPanel As AuthenticationCredentialPanel
    End Class
End NameSpace