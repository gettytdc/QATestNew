<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSchedulerOverview
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
        Dim lblTitle As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSchedulerOverview))
        Me.mWeekView = New AutomateControls.Diary.WeekView()
        lblTitle = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblTitle
        '
        resources.ApplyResources(lblTitle, "lblTitle")
        lblTitle.Name = "lblTitle"
        '
        'mWeekView
        '
        resources.ApplyResources(Me.mWeekView, "mWeekView")
        Me.mWeekView.LeftPanelWidth = 40
        Me.mWeekView.Name = "mWeekView"
        '
        'ctlSchedulerOverview
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mWeekView)
        Me.Controls.Add(lblTitle)
        Me.Name = "ctlSchedulerOverview"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents mWeekView As AutomateControls.Diary.WeekView

End Class
