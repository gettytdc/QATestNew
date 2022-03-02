<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlDebugTrackbar
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDebugTrackbar))
        Me.TrackBar = New System.Windows.Forms.TrackBar()
        Me.lblFast = New System.Windows.Forms.Label()
        Me.lblSlow = New System.Windows.Forms.Label()
        Me.lblNormal = New System.Windows.Forms.Label()
        Me.lblDebugSpeed = New System.Windows.Forms.Label()
        CType(Me.TrackBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TrackBar
        '
        resources.ApplyResources(Me.TrackBar, "TrackBar")
        Me.TrackBar.Maximum = 2000
        Me.TrackBar.Name = "TrackBar"
        Me.TrackBar.SmallChange = 200
        Me.TrackBar.TickFrequency = 200
        Me.TrackBar.Value = 1000
        '
        'lblFast
        '
        resources.ApplyResources(Me.lblFast, "lblFast")
        Me.lblFast.Name = "lblFast"
        '
        'lblSlow
        '
        resources.ApplyResources(Me.lblSlow, "lblSlow")
        Me.lblSlow.Name = "lblSlow"
        '
        'lblNormal
        '
        resources.ApplyResources(Me.lblNormal, "lblNormal")
        Me.lblNormal.Name = "lblNormal"
        '
        'lblDebugSpeed
        '
        resources.ApplyResources(Me.lblDebugSpeed, "lblDebugSpeed")
        Me.lblDebugSpeed.Name = "lblDebugSpeed"
        '
        'ctlDebugTrackbar
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.Menu
        Me.Controls.Add(Me.lblDebugSpeed)
        Me.Controls.Add(Me.lblNormal)
        Me.Controls.Add(Me.lblSlow)
        Me.Controls.Add(Me.lblFast)
        Me.Controls.Add(Me.TrackBar)
        Me.Name = "ctlDebugTrackbar"
        CType(Me.TrackBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TrackBar As System.Windows.Forms.TrackBar
    Friend WithEvents lblFast As System.Windows.Forms.Label
    Friend WithEvents lblSlow As System.Windows.Forms.Label
    Friend WithEvents lblNormal As System.Windows.Forms.Label
    Friend WithEvents lblDebugSpeed As System.Windows.Forms.Label

End Class
