<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ActiveQueuePanel
    Inherits System.Windows.Forms.UserControl

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ActiveQueuePanel))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.splitPane = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.gridActiveQueues = New AutomateControls.DataGridViews.RowBasedDataGridView()
        Me.mTimer = New System.Windows.Forms.Timer(Me.components)
        Me.bwQueueSubscriber = New System.ComponentModel.BackgroundWorker()
        Me.bwRefresher = New System.ComponentModel.BackgroundWorker()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn9 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.gridContents = New AutomateUI.ctlWorkQueueContents()
        Me.colIcon = New AutomateControls.DataGridViews.ImageListColumn()
        Me.colName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTargetResources = New AutomateControls.DataGridViews.DataGridViewNumericUpDownColumn()
        Me.colActiveSessions = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colAvailable = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTimeRemaining = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTimeElapsedRemaining = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colEta = New AutomateControls.DataGridViews.DateColumn()
        Me.colWorked = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPending = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colReferred = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotal = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitPane.Panel1.SuspendLayout()
        Me.splitPane.Panel2.SuspendLayout()
        Me.splitPane.SuspendLayout()
        CType(Me.gridActiveQueues, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'splitPane
        '
        Me.splitPane.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        resources.ApplyResources(Me.splitPane, "splitPane")
        Me.splitPane.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitPane.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.splitPane.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.splitPane.GripVisible = False
        Me.splitPane.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.splitPane.MouseLeaveColor = System.Drawing.Color.White
        Me.splitPane.Name = "splitPane"
        '
        'splitPane.Panel1
        '
        Me.splitPane.Panel1.Controls.Add(Me.gridActiveQueues)
        '
        'splitPane.Panel2
        '
        Me.splitPane.Panel2.Controls.Add(Me.gridContents)
        Me.splitPane.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.splitPane.TabStop = False
        Me.splitPane.TextColor = System.Drawing.Color.Black
        '
        'gridActiveQueues
        '
        Me.gridActiveQueues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.gridActiveQueues.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colIcon, Me.colName, Me.colStatus, Me.colTargetResources, Me.colActiveSessions, Me.colAvailable, Me.colTimeRemaining, Me.colTimeElapsedRemaining, Me.colEta, Me.colWorked, Me.colPending, Me.colReferred, Me.colTotal})
        resources.ApplyResources(Me.gridActiveQueues, "gridActiveQueues")
        Me.gridActiveQueues.Name = "gridActiveQueues"
        '
        'mTimer
        '
        Me.mTimer.Enabled = True
        Me.mTimer.Interval = 5000
        '
        'bwQueueSubscriber
        '
        '
        'bwRefresher
        '
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.DataGridViewTextBoxColumn4, "DataGridViewTextBoxColumn4")
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.ReadOnly = True
        '
        'DataGridViewTextBoxColumn5
        '
        Me.DataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.DataGridViewTextBoxColumn5, "DataGridViewTextBoxColumn5")
        Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
        Me.DataGridViewTextBoxColumn5.ReadOnly = True
        '
        'DataGridViewTextBoxColumn6
        '
        Me.DataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.DataGridViewTextBoxColumn6, "DataGridViewTextBoxColumn6")
        Me.DataGridViewTextBoxColumn6.Name = "DataGridViewTextBoxColumn6"
        Me.DataGridViewTextBoxColumn6.ReadOnly = True
        '
        'DataGridViewTextBoxColumn7
        '
        Me.DataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.DataGridViewTextBoxColumn7, "DataGridViewTextBoxColumn7")
        Me.DataGridViewTextBoxColumn7.Name = "DataGridViewTextBoxColumn7"
        Me.DataGridViewTextBoxColumn7.ReadOnly = True
        '
        'DataGridViewTextBoxColumn8
        '
        Me.DataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.DataGridViewTextBoxColumn8, "DataGridViewTextBoxColumn8")
        Me.DataGridViewTextBoxColumn8.Name = "DataGridViewTextBoxColumn8"
        Me.DataGridViewTextBoxColumn8.ReadOnly = True
        '
        'DataGridViewTextBoxColumn9
        '
        Me.DataGridViewTextBoxColumn9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.DataGridViewTextBoxColumn9, "DataGridViewTextBoxColumn9")
        Me.DataGridViewTextBoxColumn9.Name = "DataGridViewTextBoxColumn9"
        Me.DataGridViewTextBoxColumn9.ReadOnly = True
        '
        'DataGridViewTextBoxColumn10
        '
        Me.DataGridViewTextBoxColumn10.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.DataGridViewTextBoxColumn10, "DataGridViewTextBoxColumn10")
        Me.DataGridViewTextBoxColumn10.Name = "DataGridViewTextBoxColumn10"
        Me.DataGridViewTextBoxColumn10.ReadOnly = True
        '
        'gridContents
        '
        Me.gridContents.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.gridContents.Cursor = System.Windows.Forms.Cursors.Default
        resources.ApplyResources(Me.gridContents, "gridContents")
        Me.gridContents.Name = "gridContents"
        '
        'colIcon
        '
        Me.colIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colIcon, "colIcon")
        Me.colIcon.Name = "colIcon"
        Me.colIcon.ReadOnly = True
        Me.colIcon.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        '
        'colName
        '
        Me.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colName, "colName")
        Me.colName.Name = "colName"
        Me.colName.ReadOnly = True
        '
        'colStatus
        '
        Me.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colStatus, "colStatus")
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = True
        '
        'colTargetResources
        '
        Me.colTargetResources.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        Me.colTargetResources.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.colTargetResources, "colTargetResources")
        Me.colTargetResources.Name = "colTargetResources"
        Me.colTargetResources.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colActiveSessions
        '
        Me.colActiveSessions.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colActiveSessions, "colActiveSessions")
        Me.colActiveSessions.Name = "colActiveSessions"
        Me.colActiveSessions.ReadOnly = True
        '
        'colAvailable
        '
        Me.colAvailable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colAvailable, "colAvailable")
        Me.colAvailable.Name = "colAvailable"
        Me.colAvailable.ReadOnly = True
        '
        'colTimeRemaining
        '
        Me.colTimeRemaining.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colTimeRemaining, "colTimeRemaining")
        Me.colTimeRemaining.Name = "colTimeRemaining"
        Me.colTimeRemaining.ReadOnly = True
        '
        'colTimeElapsedRemaining
        '
        Me.colTimeElapsedRemaining.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        resources.ApplyResources(Me.colTimeElapsedRemaining, "colTimeElapsedRemaining")
        Me.colTimeElapsedRemaining.Name = "colTimeElapsedRemaining"
        Me.colTimeElapsedRemaining.ReadOnly = True
        '
        'colEta
        '
        Me.colEta.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        Me.colEta.DateFormat = Nothing
        resources.ApplyResources(Me.colEta, "colEta")
        Me.colEta.Name = "colEta"
        Me.colEta.ReadOnly = True
        Me.colEta.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'colWorked
        '
        Me.colWorked.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colWorked, "colWorked")
        Me.colWorked.Name = "colWorked"
        Me.colWorked.ReadOnly = True
        '
        'colPending
        '
        Me.colPending.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colPending, "colPending")
        Me.colPending.Name = "colPending"
        Me.colPending.ReadOnly = True
        '
        'colReferred
        '
        Me.colReferred.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colReferred, "colReferred")
        Me.colReferred.Name = "colReferred"
        Me.colReferred.ReadOnly = True
        '
        'colTotal
        '
        Me.colTotal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colTotal, "colTotal")
        Me.colTotal.Name = "colTotal"
        Me.colTotal.ReadOnly = True
        '
        'ActiveQueuePanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.splitPane)
        Me.Name = "ActiveQueuePanel"
        resources.ApplyResources(Me, "$this")
        Me.splitPane.Panel1.ResumeLayout(False)
        Me.splitPane.Panel2.ResumeLayout(False)
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitPane.ResumeLayout(False)
        CType(Me.gridActiveQueues, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents splitPane As AutomateControls.SplitContainers.HighlightingSplitContainer
    Private WithEvents gridActiveQueues As AutomateControls.DataGridViews.RowBasedDataGridView
    Private WithEvents gridContents As AutomateUI.ctlWorkQueueContents
    Private WithEvents mTimer As System.Windows.Forms.Timer
    Private WithEvents bwQueueSubscriber As System.ComponentModel.BackgroundWorker
    Private WithEvents bwRefresher As System.ComponentModel.BackgroundWorker
    Friend WithEvents DataGridViewTextBoxColumn1 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn6 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn7 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn8 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn9 As DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn10 As DataGridViewTextBoxColumn
    Friend WithEvents colIcon As AutomateControls.DataGridViews.ImageListColumn
    Friend WithEvents colName As DataGridViewTextBoxColumn
    Friend WithEvents colStatus As DataGridViewTextBoxColumn
    Friend WithEvents colTargetResources As AutomateControls.DataGridViews.DataGridViewNumericUpDownColumn
    Friend WithEvents colActiveSessions As DataGridViewTextBoxColumn
    Friend WithEvents colAvailable As DataGridViewTextBoxColumn
    Friend WithEvents colTimeRemaining As DataGridViewTextBoxColumn
    Friend WithEvents colTimeElapsedRemaining As DataGridViewTextBoxColumn
    Friend WithEvents colEta As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colWorked As DataGridViewTextBoxColumn
    Friend WithEvents colPending As DataGridViewTextBoxColumn
    Friend WithEvents colReferred As DataGridViewTextBoxColumn
    Friend WithEvents colTotal As DataGridViewTextBoxColumn
End Class
