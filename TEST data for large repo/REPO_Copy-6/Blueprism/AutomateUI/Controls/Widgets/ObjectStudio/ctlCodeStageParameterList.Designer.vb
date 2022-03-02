<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlCodeStageParameterList
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlCodeStageParameterList))
        Me.lstList = New System.Windows.Forms.ListBox()
        Me.lblParams = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lstList
        '
        resources.ApplyResources(Me.lstList, "lstList")
        Me.lstList.FormattingEnabled = True
        Me.lstList.Name = "lstList"
        '
        'lblParams
        '
        resources.ApplyResources(Me.lblParams, "lblParams")
        Me.lblParams.Name = "lblParams"
        '
        'ctlCodeStageParameterList
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.lblParams)
        Me.Controls.Add(Me.lstList)
        Me.Name = "ctlCodeStageParameterList"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lstList As System.Windows.Forms.ListBox
    Private WithEvents lblParams As System.Windows.Forms.Label

End Class
