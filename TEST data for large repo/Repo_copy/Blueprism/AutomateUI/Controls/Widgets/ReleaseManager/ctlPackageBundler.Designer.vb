<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlPackageBundler

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
        Me.components = New System.ComponentModel.Container()
        Dim pnlTable As System.Windows.Forms.TableLayoutPanel
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPackageBundler))
        Me.mInputTree = New AutomateUI.ctlComponentTree()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.mOutputTree = New AutomateUI.ctlComponentTree()
        pnlTable = New System.Windows.Forms.TableLayoutPanel()
        pnlTable.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTable
        '
        resources.ApplyResources(pnlTable, "pnlTable")
        pnlTable.Controls.Add(Me.mInputTree, 0, 1)
        pnlTable.Controls.Add(Me.Label1, 0, 0)
        pnlTable.Controls.Add(Me.mOutputTree, 1, 1)
        pnlTable.Name = "pnlTable"
        '
        'mInputTree
        '
        Me.mInputTree.AllowDrop = True
        resources.ApplyResources(Me.mInputTree, "mInputTree")
        Me.mInputTree.Name = "mInputTree"
        Me.mInputTree.ShowNodeToolTips = True
        Me.mInputTree.Sorted = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        pnlTable.SetColumnSpan(Me.Label1, 2)
        Me.Label1.Name = "Label1"
        '
        'mOutputTree
        '
        Me.mOutputTree.AllowDrop = True
        resources.ApplyResources(Me.mOutputTree, "mOutputTree")
        Me.mOutputTree.Name = "mOutputTree"
        Me.mOutputTree.ShowNodeToolTips = True
        Me.mOutputTree.Sorted = True
        '
        'ctlPackageBundler
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(pnlTable)
        Me.Name = "ctlPackageBundler"
        pnlTable.ResumeLayout(False)
        pnlTable.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents mInputTree As AutomateUI.ctlComponentTree
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents mOutputTree As AutomateUI.ctlComponentTree

End Class
