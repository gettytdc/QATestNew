<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlPackageTree
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPackageTree))
        Me.tvPackages = New AutomateControls.Trees.FlickerFreeTreeView()
        Me.SuspendLayout()
        '
        'tvPackages
        '
        Me.tvPackages.BorderStyle = System.Windows.Forms.BorderStyle.None
        resources.ApplyResources(Me.tvPackages, "tvPackages")
        Me.tvPackages.FullRowSelect = True
        Me.tvPackages.HideSelection = False
        Me.tvPackages.Name = "tvPackages"
        '
        'ctlPackageTree
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.tvPackages)
        Me.Name = "ctlPackageTree"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents tvPackages As AutomateControls.Trees.FlickerFreeTreeView

End Class
