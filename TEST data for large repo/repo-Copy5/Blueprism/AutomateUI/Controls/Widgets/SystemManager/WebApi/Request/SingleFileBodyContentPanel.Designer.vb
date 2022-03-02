Namespace Controls.Widgets.SystemManager.WebApi.Request

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class SingleFileBodyContentPanel
        Inherits System.Windows.Forms.UserControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SingleFileBodyContentPanel))
        Me.lblFileParameterName = New System.Windows.Forms.Label()
            Me.txtFileParameterName = New AutomateControls.Textboxes.StyledTextBox()
            Me.SuspendLayout
        '
        'lblFileParameterName
        '
        resources.ApplyResources(Me.lblFileParameterName, "lblFileParameterName")
        Me.lblFileParameterName.Name = "lblFileParameterName"
        '
        'txtFileParameterName
        '
        resources.ApplyResources(Me.txtFileParameterName, "txtFileParameterName")
        Me.txtFileParameterName.Name = "txtFileParameterName"
        '
        'SingleFileBodyContentPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblFileParameterName)
        Me.Controls.Add(Me.txtFileParameterName)
        Me.Name = "SingleFileBodyContentPanel"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
        Friend WithEvents lblFileParameterName As Label
        Friend WithEvents txtFileParameterName As AutomateControls.Textboxes.StyledTextBox
    End Class

End Namespace