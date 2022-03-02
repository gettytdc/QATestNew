<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlScheduleReport
    Inherits ctlScheduleListPanel

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout
        '
        'ctlScheduleReport
        '
        Me.AbsoluteDate = New Date(2017, 6, 30, 0, 0, 0, 0)
        Me.Name = "ctlScheduleReport"
        Me.Controls.SetChildIndex(Me.lblDaysDistance, 0)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents ToolTip1 As ToolTip
End Class
