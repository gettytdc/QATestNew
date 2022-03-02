<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmScheduleLogViewer
    Inherits frmForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmScheduleLogViewer))
        Me.gridLogEntries = New System.Windows.Forms.DataGridView()
        Me.columnName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.columnResource = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.columnStart = New AutomateControls.DataGridViews.DateColumn()
        Me.columnEnd = New AutomateControls.DataGridViews.DateColumn()
        Me.columnViewLogs = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.columnTerminationReason = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.mBlueIconBar = New AutomateControls.TitleBar()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportEntireLogToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.CloseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.gridLogEntries,System.ComponentModel.ISupportInitialize).BeginInit
        Me.MenuStrip1.SuspendLayout
        Me.SuspendLayout
        '
        'gridLogEntries
        '
        Me.gridLogEntries.AllowUserToAddRows = false
        Me.gridLogEntries.AllowUserToDeleteRows = false
        Me.gridLogEntries.AllowUserToResizeRows = false
        resources.ApplyResources(Me.gridLogEntries, "gridLogEntries")
        Me.gridLogEntries.BackgroundColor = System.Drawing.Color.White
        Me.gridLogEntries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridLogEntries.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.columnName, Me.columnResource, Me.columnStart, Me.columnEnd, Me.columnViewLogs, Me.columnTerminationReason})
        Me.gridLogEntries.Name = "gridLogEntries"
        Me.gridLogEntries.ReadOnly = true
        Me.gridLogEntries.RowHeadersVisible = false
        '
        'columnName
        '
        resources.ApplyResources(Me.columnName, "columnName")
        Me.columnName.Name = "columnName"
        Me.columnName.ReadOnly = true
        '
        'columnResource
        '
        resources.ApplyResources(Me.columnResource, "columnResource")
        Me.columnResource.Name = "columnResource"
        Me.columnResource.ReadOnly = true
        '
        'columnStart
        '
        Me.columnStart.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.columnStart.DateFormat = Nothing
        resources.ApplyResources(Me.columnStart, "columnStart")
        Me.columnStart.Name = "columnStart"
        Me.columnStart.ReadOnly = true
        Me.columnStart.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'columnEnd
        '
        Me.columnEnd.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.columnEnd.DateFormat = Nothing
        resources.ApplyResources(Me.columnEnd, "columnEnd")
        Me.columnEnd.Name = "columnEnd"
        Me.columnEnd.ReadOnly = true
        Me.columnEnd.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'columnViewLogs
        '
        Me.columnViewLogs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.columnViewLogs, "columnViewLogs")
        Me.columnViewLogs.Name = "columnViewLogs"
        Me.columnViewLogs.ReadOnly = true
        '
        'columnTerminationReason
        '
        Me.columnTerminationReason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.columnTerminationReason, "columnTerminationReason")
        Me.columnTerminationReason.Name = "columnTerminationReason"
        Me.columnTerminationReason.ReadOnly = true
        '
        'mBlueIconBar
        '
        resources.ApplyResources(Me.mBlueIconBar, "mBlueIconBar")
        Me.mBlueIconBar.Name = "mBlueIconBar"
        Me.mBlueIconBar.TabStop = false
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem})
        resources.ApplyResources(Me.MenuStrip1, "MenuStrip1")
        Me.MenuStrip1.Name = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExportEntireLogToolStripMenuItem, Me.ToolStripMenuItem1, Me.CloseToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        resources.ApplyResources(Me.FileToolStripMenuItem, "FileToolStripMenuItem")
        '
        'ExportEntireLogToolStripMenuItem
        '
        Me.ExportEntireLogToolStripMenuItem.Name = "ExportEntireLogToolStripMenuItem"
        resources.ApplyResources(Me.ExportEntireLogToolStripMenuItem, "ExportEntireLogToolStripMenuItem")
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'CloseToolStripMenuItem
        '
        Me.CloseToolStripMenuItem.Name = "CloseToolStripMenuItem"
        resources.ApplyResources(Me.CloseToolStripMenuItem, "CloseToolStripMenuItem")
        '
        'frmScheduleLogViewer
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.mBlueIconBar)
        Me.Controls.Add(Me.gridLogEntries)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmScheduleLogViewer"
        CType(Me.gridLogEntries,System.ComponentModel.ISupportInitialize).EndInit
        Me.MenuStrip1.ResumeLayout(false)
        Me.MenuStrip1.PerformLayout
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents gridLogEntries As System.Windows.Forms.DataGridView
    Friend WithEvents columnName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents columnResource As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents columnStart As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents columnEnd As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents columnViewLogs As System.Windows.Forms.DataGridViewLinkColumn
    Friend WithEvents columnTerminationReason As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents mBlueIconBar As AutomateControls.TitleBar
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportEntireLogToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents CloseToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
