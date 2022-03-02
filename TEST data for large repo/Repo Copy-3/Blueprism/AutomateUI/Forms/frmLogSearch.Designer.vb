Imports AutomateControls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLogSearch
    Inherits Forms.HelpButtonForm

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLogSearch))
        Me.Label4 = New System.Windows.Forms.Label()
        Me.mProgressBar = New System.Windows.Forms.ProgressBar()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnSearch = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblMessage = New System.Windows.Forms.Label()
        Me.cmbSearch = New System.Windows.Forms.ComboBox()
        Me.pbViewLog = New System.Windows.Forms.PictureBox()
        Me.lnkViewLog = New System.Windows.Forms.LinkLabel()
        Me.mContextMenu = New System.Windows.Forms.ContextMenu()
        Me.mnuViewSelectedLogs = New System.Windows.Forms.MenuItem()
        Me.mFinder = New System.ComponentModel.BackgroundWorker()
        Me.ctxMenuFields = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuFieldsStageName = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFieldsProcessName = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFieldsPageName = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFieldsObjectName = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFieldsActionName = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFieldsResult = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFieldsParams = New System.Windows.Forms.ToolStripMenuItem()
        Me.btnFields = New AutomateControls.SplitButton()
        Me.gridSessions = New AutomateControls.DataGridViews.ColWidthInhibitingDataGridView()
        Me.colSessionNo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStart = New AutomateControls.DataGridViews.DateColumn()
        Me.colEnd = New AutomateControls.DataGridViews.DateColumn()
        Me.colProcess = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colSource = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTarget = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colWinUser = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.pbViewLog, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctxMenuFields.SuspendLayout()
        CType(Me.gridSessions, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'mProgressBar
        '
        resources.ApplyResources(Me.mProgressBar, "mProgressBar")
        Me.mProgressBar.Name = "mProgressBar"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnSearch
        '
        resources.ApplyResources(Me.btnSearch, "btnSearch")
        Me.btnSearch.Name = "btnSearch"
        Me.btnSearch.UseVisualStyleBackColor = True
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'lblMessage
        '
        resources.ApplyResources(Me.lblMessage, "lblMessage")
        Me.lblMessage.Name = "lblMessage"
        '
        'cmbSearch
        '
        resources.ApplyResources(Me.cmbSearch, "cmbSearch")
        Me.cmbSearch.FormattingEnabled = True
        Me.cmbSearch.Name = "cmbSearch"
        '
        'pbViewLog
        '
        resources.ApplyResources(Me.pbViewLog, "pbViewLog")
        Me.pbViewLog.Image = Global.AutomateUI.My.Resources.ToolImages.Script_Details_16x16
        Me.pbViewLog.Name = "pbViewLog"
        Me.pbViewLog.TabStop = False
        '
        'lnkViewLog
        '
        resources.ApplyResources(Me.lnkViewLog, "lnkViewLog")
        Me.lnkViewLog.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.lnkViewLog.Name = "lnkViewLog"
        Me.lnkViewLog.TabStop = True
        '
        'mContextMenu
        '
        Me.mContextMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuViewSelectedLogs})
        '
        'mnuViewSelectedLogs
        '
        Me.mnuViewSelectedLogs.Index = 0
        resources.ApplyResources(Me.mnuViewSelectedLogs, "mnuViewSelectedLogs")
        '
        'mFinder
        '
        Me.mFinder.WorkerReportsProgress = True
        Me.mFinder.WorkerSupportsCancellation = True
        '
        'ctxMenuFields
        '
        Me.ctxMenuFields.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFieldsStageName, Me.mnuFieldsProcessName, Me.mnuFieldsPageName, Me.mnuFieldsObjectName, Me.mnuFieldsActionName, Me.mnuFieldsResult, Me.mnuFieldsParams})
        Me.ctxMenuFields.Name = "ctxMenuFields"
        resources.ApplyResources(Me.ctxMenuFields, "ctxMenuFields")
        '
        'mnuFieldsStageName
        '
        Me.mnuFieldsStageName.Checked = True
        Me.mnuFieldsStageName.CheckOnClick = True
        Me.mnuFieldsStageName.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsStageName.Name = "mnuFieldsStageName"
        resources.ApplyResources(Me.mnuFieldsStageName, "mnuFieldsStageName")
        Me.mnuFieldsStageName.Tag = "stagename"
        '
        'mnuFieldsProcessName
        '
        Me.mnuFieldsProcessName.Checked = True
        Me.mnuFieldsProcessName.CheckOnClick = True
        Me.mnuFieldsProcessName.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsProcessName.Name = "mnuFieldsProcessName"
        resources.ApplyResources(Me.mnuFieldsProcessName, "mnuFieldsProcessName")
        Me.mnuFieldsProcessName.Tag = "processname"
        '
        'mnuFieldsPageName
        '
        Me.mnuFieldsPageName.Checked = True
        Me.mnuFieldsPageName.CheckOnClick = True
        Me.mnuFieldsPageName.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsPageName.Name = "mnuFieldsPageName"
        resources.ApplyResources(Me.mnuFieldsPageName, "mnuFieldsPageName")
        Me.mnuFieldsPageName.Tag = "pagename"
        '
        'mnuFieldsObjectName
        '
        Me.mnuFieldsObjectName.Checked = True
        Me.mnuFieldsObjectName.CheckOnClick = True
        Me.mnuFieldsObjectName.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsObjectName.Name = "mnuFieldsObjectName"
        resources.ApplyResources(Me.mnuFieldsObjectName, "mnuFieldsObjectName")
        Me.mnuFieldsObjectName.Tag = "objectname"
        '
        'mnuFieldsActionName
        '
        Me.mnuFieldsActionName.Checked = True
        Me.mnuFieldsActionName.CheckOnClick = True
        Me.mnuFieldsActionName.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsActionName.Name = "mnuFieldsActionName"
        resources.ApplyResources(Me.mnuFieldsActionName, "mnuFieldsActionName")
        Me.mnuFieldsActionName.Tag = "actionname"
        '
        'mnuFieldsResult
        '
        Me.mnuFieldsResult.Checked = True
        Me.mnuFieldsResult.CheckOnClick = True
        Me.mnuFieldsResult.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsResult.Name = "mnuFieldsResult"
        resources.ApplyResources(Me.mnuFieldsResult, "mnuFieldsResult")
        Me.mnuFieldsResult.Tag = "result"
        '
        'mnuFieldsParams
        '
        Me.mnuFieldsParams.Checked = True
        Me.mnuFieldsParams.CheckOnClick = True
        Me.mnuFieldsParams.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuFieldsParams.Name = "mnuFieldsParams"
        resources.ApplyResources(Me.mnuFieldsParams, "mnuFieldsParams")
        Me.mnuFieldsParams.Tag = "attributexml"
        '
        'btnFields
        '
        resources.ApplyResources(Me.btnFields, "btnFields")
        Me.btnFields.ContextMenuStrip = Me.ctxMenuFields
        Me.btnFields.DropDownOnButtonClick = True
        Me.btnFields.Name = "btnFields"
        Me.btnFields.SplitMenuStrip = Me.ctxMenuFields
        Me.btnFields.UseVisualStyleBackColor = True
        '
        'gridSessions
        '
        Me.gridSessions.AllowUserToAddRows = False
        Me.gridSessions.AllowUserToDeleteRows = False
        Me.gridSessions.AllowUserToResizeRows = False
        resources.ApplyResources(Me.gridSessions, "gridSessions")
        Me.gridSessions.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridSessions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridSessions.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colSessionNo, Me.colStart, Me.colEnd, Me.colProcess, Me.colStatus, Me.colSource, Me.colTarget, Me.colWinUser})
        Me.gridSessions.GridColor = System.Drawing.SystemColors.ControlLight
        Me.gridSessions.Name = "gridSessions"
        Me.gridSessions.ReadOnly = True
        Me.gridSessions.RowHeadersVisible = False
        Me.gridSessions.RowTemplate.Height = 18
        Me.gridSessions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colSessionNo
        '
        Me.colSessionNo.DataPropertyName = "sessionnumber"
        resources.ApplyResources(Me.colSessionNo, "colSessionNo")
        Me.colSessionNo.Name = "colSessionNo"
        Me.colSessionNo.ReadOnly = True
        '
        'colStart
        '
        Me.colStart.DataPropertyName = "startdatetime"
        Me.colStart.DateFormat = Nothing
        resources.ApplyResources(Me.colStart, "colStart")
        Me.colStart.Name = "colStart"
        Me.colStart.ReadOnly = True
        '
        'colEnd
        '
        Me.colEnd.DataPropertyName = "enddatetime"
        Me.colEnd.DateFormat = Nothing
        resources.ApplyResources(Me.colEnd, "colEnd")
        Me.colEnd.Name = "colEnd"
        Me.colEnd.ReadOnly = True
        '
        'colProcess
        '
        Me.colProcess.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colProcess.DataPropertyName = "processname"
        Me.colProcess.FillWeight = 200.0!
        resources.ApplyResources(Me.colProcess, "colProcess")
        Me.colProcess.Name = "colProcess"
        Me.colProcess.ReadOnly = True
        '
        'colStatus
        '
        Me.colStatus.DataPropertyName = "statustext"
        resources.ApplyResources(Me.colStatus, "colStatus")
        Me.colStatus.Name = "colStatus"
        Me.colStatus.ReadOnly = True
        '
        'colSource
        '
        Me.colSource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colSource.DataPropertyName = "starterresourcename"
        resources.ApplyResources(Me.colSource, "colSource")
        Me.colSource.Name = "colSource"
        Me.colSource.ReadOnly = True
        '
        'colTarget
        '
        Me.colTarget.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colTarget.DataPropertyName = "runningresourcename"
        resources.ApplyResources(Me.colTarget, "colTarget")
        Me.colTarget.Name = "colTarget"
        Me.colTarget.ReadOnly = True
        '
        'colWinUser
        '
        Me.colWinUser.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colWinUser.DataPropertyName = "runningosusername"
        Me.colWinUser.FillWeight = 50.0!
        resources.ApplyResources(Me.colWinUser, "colWinUser")
        Me.colWinUser.Name = "colWinUser"
        Me.colWinUser.ReadOnly = True
        '
        'frmLogSearch
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.gridSessions)
        Me.Controls.Add(Me.pbViewLog)
        Me.Controls.Add(Me.btnFields)
        Me.Controls.Add(Me.lnkViewLog)
        Me.Controls.Add(Me.cmbSearch)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.mProgressBar)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnSearch)
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmLogSearch"
        CType(Me.pbViewLog, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctxMenuFields.ResumeLayout(False)
        CType(Me.gridSessions, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents mProgressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnSearch As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblMessage As System.Windows.Forms.Label
    Friend WithEvents cmbSearch As System.Windows.Forms.ComboBox
    Friend WithEvents pbViewLog As System.Windows.Forms.PictureBox
    Friend WithEvents lnkViewLog As System.Windows.Forms.LinkLabel
    Private WithEvents mContextMenu As System.Windows.Forms.ContextMenu
    Private WithEvents mnuViewSelectedLogs As System.Windows.Forms.MenuItem
    Friend WithEvents mFinder As System.ComponentModel.BackgroundWorker
    Private WithEvents gridSessions As AutomateControls.DataGridViews.ColWidthInhibitingDataGridView
    Friend WithEvents colSessionNo As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStart As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colEnd As AutomateControls.DataGridViews.DateColumn
    Friend WithEvents colProcess As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colStatus As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colSource As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTarget As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colWinUser As System.Windows.Forms.DataGridViewTextBoxColumn
    Private WithEvents btnFields As AutomateControls.SplitButton
    Private WithEvents ctxMenuFields As System.Windows.Forms.ContextMenuStrip
    Private WithEvents mnuFieldsStageName As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFieldsObjectName As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFieldsProcessName As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFieldsPageName As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFieldsActionName As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFieldsResult As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFieldsParams As System.Windows.Forms.ToolStripMenuItem

End Class
