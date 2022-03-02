<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlAuditProcessLogs
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlAuditProcessLogs))
        Me.logsProc = New AutomateUI.ctlLogList()
        Me.SuspendLayout()
        '
        'logsProc
        '
        resources.ApplyResources(Me.logsProc, "logsProc")
        Me.logsProc.Name = "logsProc"
        Me.logsProc.ProcessType = BluePrism.AutomateProcessCore.Processes.DiagramType.Process
        '
        'ctlAuditProcessLogs
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.logsProc)
        Me.Name = "ctlAuditProcessLogs"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents logsProc As AutomateUI.ctlLogList

End Class
