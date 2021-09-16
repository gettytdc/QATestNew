<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorkflowEnvironmentLocks
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorkflowEnvironmentLocks))
        Me.mFilteredLocks = New AutomateControls.FilteredList()
        Me.tstripEnvLocks = New System.Windows.Forms.ToolStrip()
        Me.btnReleaseLocks = New System.Windows.Forms.ToolStripButton()
        Me.btnDeleteLocks = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnViewLogs = New System.Windows.Forms.ToolStripButton()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.RowsPerPage = New AutomateUI.ctlRowsPerPage()
        Me.btnRefresh = New System.Windows.Forms.ToolStripButton()
        Me.tstripEnvLocks.SuspendLayout
        Me.TableLayoutPanel1.SuspendLayout
        Me.SuspendLayout
        '
        'mFilteredLocks
        '
        resources.ApplyResources(Me.mFilteredLocks, "mFilteredLocks")
        Me.mFilteredLocks.Name = "mFilteredLocks"
        '
        'tstripEnvLocks
        '
        Me.tstripEnvLocks.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnReleaseLocks, Me.btnDeleteLocks, Me.ToolStripSeparator2, Me.btnViewLogs, Me.btnRefresh})
        resources.ApplyResources(Me.tstripEnvLocks, "tstripEnvLocks")
        Me.tstripEnvLocks.Name = "tstripEnvLocks"
        Me.tstripEnvLocks.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        '
        'btnReleaseLocks
        '
        Me.btnReleaseLocks.Image = Global.AutomateUI.My.Resources.ToolImages.Unlock_16x16
        resources.ApplyResources(Me.btnReleaseLocks, "btnReleaseLocks")
        Me.btnReleaseLocks.Name = "btnReleaseLocks"
        '
        'btnDeleteLocks
        '
        Me.btnDeleteLocks.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        resources.ApplyResources(Me.btnDeleteLocks, "btnDeleteLocks")
        Me.btnDeleteLocks.Name = "btnDeleteLocks"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        '
        'btnViewLogs
        '
        Me.btnViewLogs.Image = Global.AutomateUI.My.Resources.ToolImages.Script_View_16x16
        resources.ApplyResources(Me.btnViewLogs, "btnViewLogs")
        Me.btnViewLogs.Name = "btnViewLogs"
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.mFilteredLocks, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.RowsPerPage, 0, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'RowsPerPage
        '
        Me.RowsPerPage.CurrentPage = 1
        resources.ApplyResources(Me.RowsPerPage, "RowsPerPage")
        Me.RowsPerPage.MaxRows = 0
        Me.RowsPerPage.Name = "RowsPerPage"
        Me.RowsPerPage.RowsPerPage = 0
        Me.RowsPerPage.TotalRows = -1
        '
        'btnRefresh
        '
        Me.btnRefresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnRefresh, "btnRefresh")
        Me.btnRefresh.Name = "btnRefresh"
        '
        'ctlWorkflowEnvironmentLocks
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.tstripEnvLocks)
        Me.Name = "ctlWorkflowEnvironmentLocks"
        resources.ApplyResources(Me, "$this")
        Me.tstripEnvLocks.ResumeLayout(false)
        Me.tstripEnvLocks.PerformLayout
        Me.TableLayoutPanel1.ResumeLayout(false)
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents mFilteredLocks As AutomateControls.FilteredList
    Private WithEvents tstripEnvLocks As System.Windows.Forms.ToolStrip
    Private WithEvents btnReleaseLocks As System.Windows.Forms.ToolStripButton
    Private WithEvents btnDeleteLocks As System.Windows.Forms.ToolStripButton
    Private WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents btnViewLogs As System.Windows.Forms.ToolStripButton
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents RowsPerPage As ctlRowsPerPage
    Friend WithEvents btnRefresh As ToolStripButton
End Class
