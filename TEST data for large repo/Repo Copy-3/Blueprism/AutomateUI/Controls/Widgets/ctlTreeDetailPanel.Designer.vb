<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlTreeDetailPanel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlTreeDetailPanel))
        Me.splitPane = New System.Windows.Forms.SplitContainer()
        Me.treeMain = New System.Windows.Forms.TreeView()
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitPane.Panel1.SuspendLayout()
        Me.splitPane.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitPane
        '
        Me.splitPane.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        resources.ApplyResources(Me.splitPane, "splitPane")
        Me.splitPane.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitPane.Name = "splitPane"
        '
        'splitPane.Panel1
        '
        Me.splitPane.Panel1.Controls.Add(Me.treeMain)
        '
        'treeMain
        '
        resources.ApplyResources(Me.treeMain, "treeMain")
        Me.treeMain.HideSelection = False
        Me.treeMain.Name = "treeMain"
        '
        'ctlTreeDetailPanel
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitPane)
        Me.Name = "ctlTreeDetailPanel"
        Me.splitPane.Panel1.ResumeLayout(False)
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitPane.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents treeMain As System.Windows.Forms.TreeView
    Private WithEvents splitPane As System.Windows.Forms.SplitContainer

End Class
