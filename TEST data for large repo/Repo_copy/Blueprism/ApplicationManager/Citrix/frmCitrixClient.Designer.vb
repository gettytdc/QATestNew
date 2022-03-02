<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCitrixClient
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCitrixClient))
        Me.stsStatus = New System.Windows.Forms.StatusStrip()
        Me.lblPosition = New System.Windows.Forms.ToolStripStatusLabel()
        Me.stsStatus.SuspendLayout()
        Me.SuspendLayout()
        '
        'stsStatus
        '
        Me.stsStatus.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblPosition})
        resources.ApplyResources(Me.stsStatus, "stsStatus")
        Me.stsStatus.Name = "stsStatus"
        '
        'lblPosition
        '
        Me.lblPosition.Name = "lblPosition"
        resources.ApplyResources(Me.lblPosition, "lblPosition")
        '
        'frmCitrixClient
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.stsStatus)
        Me.Name = "frmCitrixClient"
        Me.stsStatus.ResumeLayout(False)
        Me.stsStatus.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents stsStatus As System.Windows.Forms.StatusStrip
    Friend WithEvents lblPosition As System.Windows.Forms.ToolStripStatusLabel

End Class
