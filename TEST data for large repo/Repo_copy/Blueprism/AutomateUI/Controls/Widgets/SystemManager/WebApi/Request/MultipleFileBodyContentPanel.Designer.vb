Namespace Controls.Widgets.SystemManager.WebApi.Request

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class MultipleFileBodyContentPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MultipleFileBodyContentPanel))
            Me.txtFileCollectionParamName = New AutomateControls.Textboxes.StyledTextBox()
            Me.lblFileCollectionParameterName = New System.Windows.Forms.Label()
        Me.SuspendLayout
        '
        'txtFileCollectionParamName
        '
        resources.ApplyResources(Me.txtFileCollectionParamName, "txtFileCollectionParamName")
        Me.txtFileCollectionParamName.Name = "txtFileCollectionParamName"
        '
        'lblFileCollectionParameterName
        '
        resources.ApplyResources(Me.lblFileCollectionParameterName, "lblFileCollectionParameterName")
        Me.lblFileCollectionParameterName.Name = "lblFileCollectionParameterName"
        '
        'MultipleFileBodyContentPanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblFileCollectionParameterName)
        Me.Controls.Add(Me.txtFileCollectionParamName)
        Me.Name = "MultipleFileBodyContentPanel"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

        Friend WithEvents txtFileCollectionParamName As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents lblFileCollectionParameterName As Label
    End Class

End Namespace