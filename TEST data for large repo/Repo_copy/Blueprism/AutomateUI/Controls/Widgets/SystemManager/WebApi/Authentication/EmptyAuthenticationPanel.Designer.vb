Namespace Controls.Widgets.SystemManager.WebApi.Authentication
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class EmptyAuthenticationPanel
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
        Me.ttDescription = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout
        '
        'EmptyAuthenticationPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Name = "EmptyAuthenticationPanel"
        Me.Size = New System.Drawing.Size(319, 44)
        Me.ResumeLayout(false)

End Sub
        Friend WithEvents ttDescription As ToolTip
    End Class
End NameSpace