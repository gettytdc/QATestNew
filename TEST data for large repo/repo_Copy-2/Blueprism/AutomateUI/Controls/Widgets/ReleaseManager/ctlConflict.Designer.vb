<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlConflict

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlConflict))
        Me.mFlowPanel = New AutomateControls.FullWidthFlowLayoutPanel()
        Me.lblComponentName = New System.Windows.Forms.Label()
        Me.lblConflictText = New System.Windows.Forms.Label()
        Me.mFlowPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'mFlowPanel
        '
        resources.ApplyResources(Me.mFlowPanel, "mFlowPanel")
        Me.mFlowPanel.Controls.Add(Me.lblComponentName)
        Me.mFlowPanel.Controls.Add(Me.lblConflictText)
        Me.mFlowPanel.Name = "mFlowPanel"
        '
        'lblComponentName
        '
        resources.ApplyResources(Me.lblComponentName, "lblComponentName")
        Me.lblComponentName.Name = "lblComponentName"
        '
        'lblConflictText
        '
        resources.ApplyResources(Me.lblConflictText, "lblConflictText")
        Me.lblConflictText.Name = "lblConflictText"
        '
        'ctlConflict
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mFlowPanel)
        Me.Name = "ctlConflict"
        Me.mFlowPanel.ResumeLayout(False)
        Me.mFlowPanel.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents mFlowPanel As AutomateControls.FullWidthFlowLayoutPanel
    Private WithEvents lblComponentName As System.Windows.Forms.Label
    Private WithEvents lblConflictText As System.Windows.Forms.Label

End Class
