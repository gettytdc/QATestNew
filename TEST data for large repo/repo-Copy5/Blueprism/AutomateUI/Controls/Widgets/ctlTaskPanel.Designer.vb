<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlTaskPanel

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlTaskPanel))
        Me.mTooltip = New System.Windows.Forms.ToolTip(Me.components)
        Me.mTooltipTimer = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'mTooltipTimer
        '
        Me.mTooltipTimer.Interval = 500
        '
        'ctlTaskPanel
        '
        resources.ApplyResources(Me, My.Resources.ctlTaskPanel_This)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Name = "ctlTaskPanel"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents mTooltip As System.Windows.Forms.ToolTip
    Friend WithEvents mTooltipTimer As System.Windows.Forms.Timer


End Class
