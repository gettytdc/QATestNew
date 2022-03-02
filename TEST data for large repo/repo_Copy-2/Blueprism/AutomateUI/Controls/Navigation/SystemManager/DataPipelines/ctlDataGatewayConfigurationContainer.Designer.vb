<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDataGatewayConfigurationContainer
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
        Me.contentPanel = New System.Windows.Forms.Panel()
        Me.SuspendLayout
        '
        'contentPanel
        '
        Me.contentPanel.AutoScroll = true
        Me.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.contentPanel.Location = New System.Drawing.Point(0, 0)
        Me.contentPanel.Name = "contentPanel"
        Me.contentPanel.Size = New System.Drawing.Size(1183, 719)
        Me.contentPanel.TabIndex = 0
        '
        'ctlDataGatewayConfigurationContainer
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.contentPanel)
        Me.Name = "ctlDataGatewayConfigurationContainer"
        Me.Size = New System.Drawing.Size(1183, 719)
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents contentPanel As Panel
End Class
