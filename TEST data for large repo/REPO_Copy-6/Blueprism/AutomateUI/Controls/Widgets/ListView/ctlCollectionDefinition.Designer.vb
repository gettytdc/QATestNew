<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlCollectionDefinition
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlCollectionDefinition))
        Me.lnkDefinition = New System.Windows.Forms.LinkLabel()
        Me.SuspendLayout()
        '
        'lnkDefinition
        '
        resources.ApplyResources(Me.lnkDefinition, "lnkDefinition")
        Me.lnkDefinition.Name = "lnkDefinition"
        Me.lnkDefinition.TabStop = True
        '
        'ctlCollectionDefinition
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lnkDefinition)
        Me.Name = "ctlCollectionDefinition"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lnkDefinition As System.Windows.Forms.LinkLabel

End Class
