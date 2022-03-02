<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlLogList
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlLogList))
        Me.filterFlow = New System.Windows.Forms.FlowLayoutPanel()
        Me.ctxMenuGridBody = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuViewSelected = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuSearchSelected = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSearchAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxMenuColHeader = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.tip = New System.Windows.Forms.ToolTip(Me.components)
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.lnkSearchAll = New AutomateControls.BulletedLinkLabel()
        Me.lnkSearchSelected = New AutomateControls.BulletedLinkLabel()
        Me.lnkViewLog = New AutomateControls.BulletedLinkLabel()
        Me.lnkResetFilters = New AutomateControls.BulletedLinkLabel()
        Me.gridLogs = New AutomateControls.DataGridViews.ColWidthInhibitingDataGridView()
        Me.colStart = New AutomateControls.DataGridViews.DateColumn()
        Me.colEnd = New AutomateControls.DataGridViews.DateColumn()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.RowsPerPage = New AutomateUI.ctlRowsPerPage()
        Me.colSessionNo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colProcess = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colSource = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTarget = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colWinUser = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ctxMenuGridBody.SuspendLayout
        Me.FlowLayoutPanel1.SuspendLayout
        CType(Me.gridLogs,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'filterFlow
        '
        resources.ApplyResources(Me.filterFlow, "filterFlow")
        Me.filterFlow.Name = "filterFlow"
        '
        'ctxMenuGridBody
        '
        Me.ctxMenuGridBody.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuViewSelected, Me.ToolStripSeparator1, Me.mnuSearchSelected, Me.mnuSearchAll})
        Me.ctxMenuGridBody.Name = "ctxMenuGridBody"
        resources.ApplyResources(Me.ctxMenuGridBody, "ctxMenuGridBody")
        '
        'mnuViewSelected
        '
        Me.mnuViewSelected.Name = "mnuViewSelected"
        resources.ApplyResources(Me.mnuViewSelected, "mnuViewSelected")
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'mnuSearchSelected
        '
        Me.mnuSearchSelected.Name = "mnuSearchSelected"
        resources.ApplyResources(Me.mnuSearchSelected, "mnuSearchSelected")
        '
        'mnuSearchAll
        '
        Me.mnuSearchAll.Name = "mnuSearchAll"
        resources.ApplyResources(Me.mnuSearchAll, "mnuSearchAll")
        '
        'ctxMenuColHeader
        '
        Me.ctxMenuColHeader.Name = "ctxMenuColHeader"
        resources.ApplyResources(Me.ctxMenuColHeader, "ctxMenuColHeader")
        '
        'FlowLayoutPanel1
        '
        resources.ApplyResources(Me.FlowLayoutPanel1, "FlowLayoutPanel1")
        Me.FlowLayoutPanel1.Controls.Add(Me.lnkSearchAll)
        Me.FlowLayoutPanel1.Controls.Add(Me.lnkSearchSelected)
        Me.FlowLayoutPanel1.Controls.Add(Me.lnkViewLog)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        '
        'lnkSearchAll
        '
        resources.ApplyResources(Me.lnkSearchAll, "lnkSearchAll")
        Me.lnkSearchAll.LinkColor = System.Drawing.Color.Black
        Me.lnkSearchAll.Name = "lnkSearchAll"
        Me.lnkSearchAll.TabStop = true
        Me.tip.SetToolTip(Me.lnkSearchAll, resources.GetString("lnkSearchAll.ToolTip"))
        '
        'lnkSearchSelected
        '
        resources.ApplyResources(Me.lnkSearchSelected, "lnkSearchSelected")
        Me.lnkSearchSelected.LinkColor = System.Drawing.Color.Black
        Me.lnkSearchSelected.Name = "lnkSearchSelected"
        Me.lnkSearchSelected.TabStop = true
        Me.tip.SetToolTip(Me.lnkSearchSelected, resources.GetString("lnkSearchSelected.ToolTip"))
        '
        'lnkViewLog
        '
        resources.ApplyResources(Me.lnkViewLog, "lnkViewLog")
        Me.lnkViewLog.LinkColor = System.Drawing.Color.Black
        Me.lnkViewLog.Name = "lnkViewLog"
        Me.lnkViewLog.TabStop = true
        Me.tip.SetToolTip(Me.lnkViewLog, resources.GetString("lnkViewLog.ToolTip"))
        '
        'lnkResetFilters
        '
        resources.ApplyResources(Me.lnkResetFilters, "lnkResetFilters")
        Me.lnkResetFilters.LinkColor = System.Drawing.Color.Black
        Me.lnkResetFilters.Name = "lnkResetFilters"
        Me.lnkResetFilters.TabStop = true
        Me.tip.SetToolTip(Me.lnkResetFilters, resources.GetString("lnkResetFilters.ToolTip"))
        '
        'gridLogs
        '
        Me.gridLogs.AllowUserToAddRows = false
        Me.gridLogs.AllowUserToDeleteRows = false
        Me.gridLogs.AllowUserToResizeRows = false
        resources.ApplyResources(Me.gridLogs, "gridLogs")
        Me.gridLogs.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridLogs.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colSessionNo, Me.colStart, Me.colEnd, Me.colProcess, Me.colStatus, Me.colSource, Me.colTarget, Me.colWinUser})
        Me.gridLogs.ContextMenuStrip = Me.ctxMenuGridBody
        Me.gridLogs.GridColor = System.Drawing.SystemColors.ControlLight
        Me.gridLogs.Name = "gridLogs"
        Me.gridLogs.ReadOnly = true
        Me.gridLogs.RowHeadersVisible = false
        Me.gridLogs.RowTemplate.Height = 18
        Me.gridLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colStart
        '
        Me.colStart.DataPropertyName = "startdatetime"
        Me.colStart.DateFormat = Nothing
        resources.ApplyResources(Me.colStart, "colStart")
        Me.colStart.Name = "colStart"
        Me.colStart.ReadOnly = true
        '
        'colEnd
        '
        Me.colEnd.DataPropertyName = "enddatetime"
        Me.colEnd.DateFormat = Nothing
        resources.ApplyResources(Me.colEnd, "colEnd")
        Me.colEnd.Name = "colEnd"
        Me.colEnd.ReadOnly = true
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.DataPropertyName = "sessionnumber"
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = true
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn2.DataPropertyName = "processname"
        Me.DataGridViewTextBoxColumn2.FillWeight = 200!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = true
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.DataPropertyName = "statustext"
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = true
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn4.DataPropertyName = "starterresourcename"
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.ReadOnly = true
        '
        'DataGridViewTextBoxColumn5
        '
        Me.DataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn5.DataPropertyName = "runningresourcename"
        resources.ApplyResources(Me.DataGridViewTextBoxColumn5, "DataGridViewTextBoxColumn5")
        Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
        Me.DataGridViewTextBoxColumn5.ReadOnly = true
        '
        'DataGridViewTextBoxColumn6
        '
        Me.DataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn6.DataPropertyName = "runningosusername"
        Me.DataGridViewTextBoxColumn6.FillWeight = 50!
        resources.ApplyResources(Me.DataGridViewTextBoxColumn6, "DataGridViewTextBoxColumn6")
        Me.DataGridViewTextBoxColumn6.Name = "DataGridViewTextBoxColumn6"
        Me.DataGridViewTextBoxColumn6.ReadOnly = true
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
        'colSessionNo
        '
        Me.colSessionNo.DataPropertyName = "sessionnumber"
        resources.ApplyResources(Me.colSessionNo, "colSessionNo")
        Me.colSessionNo.Name = "colSessionNo"
        Me.colSessionNo.ReadOnly = true
        '
        'colProcess
        '
        Me.colProcess.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colProcess.DataPropertyName = "processname"
        Me.colProcess.FillWeight = 200!
        resources.ApplyResources(Me.colProcess, "colProcess")
        Me.colProcess.Name = "colProcess"
        Me.colProcess.ReadOnly = true
        '
        'colStatus
        '
        Me.colStatus.DataPropertyName = "statustext"
        resources.ApplyResources(Me.colStatus, "colStatus")
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = true
        '
        'colSource
        '
        Me.colSource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colSource.DataPropertyName = "starterresourcename"
        resources.ApplyResources(Me.colSource, "colSource")
        Me.colSource.Name = "colSource"
        Me.colSource.ReadOnly = true
        '
        'colTarget
        '
        Me.colTarget.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colTarget.DataPropertyName = "runningresourcename"
        resources.ApplyResources(Me.colTarget, "colTarget")
        Me.colTarget.Name = "colTarget"
        Me.colTarget.ReadOnly = true
        '
        'colWinUser
        '
        Me.colWinUser.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colWinUser.DataPropertyName = "runningosusername"
        Me.colWinUser.FillWeight = 50!
        resources.ApplyResources(Me.colWinUser, "colWinUser")
        Me.colWinUser.Name = "colWinUser"
        Me.colWinUser.ReadOnly = true
        '
        'ctlLogList
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.RowsPerPage)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.lnkResetFilters)
        Me.Controls.Add(Me.filterFlow)
        Me.Controls.Add(Me.gridLogs)
        Me.Name = "ctlLogList"
        resources.ApplyResources(Me, "$this")
        Me.ctxMenuGridBody.ResumeLayout(false)
        Me.FlowLayoutPanel1.ResumeLayout(false)
        Me.FlowLayoutPanel1.PerformLayout
        CType(Me.gridLogs,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents filterFlow As System.Windows.Forms.FlowLayoutPanel
    Private WithEvents gridLogs As AutomateControls.DataGridViews.ColWidthInhibitingDataGridView
    Friend WithEvents lnkViewLog As AutomateControls.BulletedLinkLabel
    Friend WithEvents colSessionNo As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStart As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colEnd As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colProcess As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStatus As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colSource As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTarget As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colWinUser As System.Windows.Forms.DataGridViewTextBoxColumn
    Private WithEvents ctxMenuGridBody As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuViewSelected As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSearchSelected As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSearchAll As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents ctxMenuColHeader As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents tip As System.Windows.Forms.ToolTip
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents lnkResetFilters As AutomateControls.BulletedLinkLabel
    Friend WithEvents lnkSearchAll As AutomateControls.BulletedLinkLabel
    Private WithEvents lnkSearchSelected As AutomateControls.BulletedLinkLabel
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents RowsPerPage As ctlRowsPerPage
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn6 As DataGridViewTextBoxColumn
End Class
