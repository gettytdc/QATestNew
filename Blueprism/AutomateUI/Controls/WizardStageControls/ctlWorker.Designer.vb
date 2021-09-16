<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorker

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorker))
        Me.lblWorking = New System.Windows.Forms.Label()
        Me.mProgress = New System.Windows.Forms.ProgressBar()
        Me.lblState = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblWorking
        '
        resources.ApplyResources(Me.lblWorking, "lblWorking")
        Me.lblWorking.Name = "lblWorking"
        '
        'mProgress
        '
        resources.ApplyResources(Me.mProgress, "mProgress")
        Me.mProgress.Name = "mProgress"
        Me.mProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        '
        'lblState
        '
        resources.ApplyResources(Me.lblState, "lblState")
        Me.lblState.Name = "lblState"
        '
        'ctlWorker
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblState)
        Me.Controls.Add(Me.lblWorking)
        Me.Controls.Add(Me.mProgress)
        Me.Name = "ctlWorker"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lblWorking As System.Windows.Forms.Label
    Friend WithEvents mProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents lblState As System.Windows.Forms.Label

End Class
