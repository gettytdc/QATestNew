Imports AutomateControls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLogViewer
    Inherits Forms.HelpButtonForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLogViewer))
        Me.MenuItem1 = New System.Windows.Forms.MenuItem()
        Me.mnuRefresh = New System.Windows.Forms.MenuItem()
        Me.mnuListModeSeparator = New System.Windows.Forms.MenuItem()
        Me.mnuGridMode = New System.Windows.Forms.MenuItem()
        Me.mnuListMode = New System.Windows.Forms.MenuItem()
        Me.mnuColumnsSeparator = New System.Windows.Forms.MenuItem()
        Me.mnuShowAll = New System.Windows.Forms.MenuItem()
        Me.mnuColumns = New System.Windows.Forms.MenuItem()
        Me.mnuClose = New System.Windows.Forms.MenuItem()
        Me.mnuFile = New System.Windows.Forms.MenuItem()
        Me.mnuExportPage = New System.Windows.Forms.MenuItem()
        Me.mnuExportLog = New System.Windows.Forms.MenuItem()
        Me.sep2 = New System.Windows.Forms.MenuItem()
        Me.mnuMain = New System.Windows.Forms.MainMenu(Me.components)
        Me.MenuItem3 = New System.Windows.Forms.MenuItem()
        Me.lnkToggleSearch = New AutomateControls.BulletedLinkLabel()
        Me.panPageControl = New System.Windows.Forms.Panel()
        Me.btnFirstPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnPrevPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblViewType = New System.Windows.Forms.Label()
        Me.txtCurrentPage = New AutomateControls.Textboxes.StyledTextBox()
        Me.cmbViewType = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblRowsPerPage = New System.Windows.Forms.Label()
        Me.cmbRowsPerPage = New System.Windows.Forms.ComboBox()
        Me.btnLastPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtTotalPages = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnNextPage = New AutomateControls.Buttons.StandardStyledButton()
        Me.titleBar = New AutomateControls.TitleBar()
        Me.splitPane = New System.Windows.Forms.SplitContainer()
        Me.panSearchConstraints = New System.Windows.Forms.Panel()
        Me.btnFindAll = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnFindNext = New AutomateControls.Buttons.StandardStyledButton()
        Me.chklSearchColumns = New System.Windows.Forms.CheckedListBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmbFind = New System.Windows.Forms.ComboBox()
        Me.lFind = New System.Windows.Forms.Label()
        Me.chkCase = New System.Windows.Forms.CheckBox()
        Me.chkWildcards = New System.Windows.Forms.CheckBox()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.btnColour = New AutomateUI.clsColorButton()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.chkWholeWord = New System.Windows.Forms.CheckBox()
        Me.panFindControl = New System.Windows.Forms.Panel()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.progressFind = New System.Windows.Forms.ProgressBar()
        Me.btnClear = New AutomateControls.Buttons.StandardStyledButton()
        Me.gridLog = New System.Windows.Forms.DataGridView()
        Me.lblNoLogs = New System.Windows.Forms.Label()
        Me.panPageControl.SuspendLayout()
        CType(Me.splitPane, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitPane.Panel1.SuspendLayout()
        Me.splitPane.Panel2.SuspendLayout()
        Me.splitPane.SuspendLayout()
        Me.panSearchConstraints.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.panFindControl.SuspendLayout()
        CType(Me.gridLog, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'MenuItem1
        '
        Me.MenuItem1.Index = 1
        Me.MenuItem1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuRefresh, Me.mnuListModeSeparator, Me.mnuGridMode, Me.mnuListMode, Me.mnuColumnsSeparator, Me.mnuShowAll, Me.mnuColumns})
        resources.ApplyResources(Me.MenuItem1, "MenuItem1")
        '
        'mnuRefresh
        '
        Me.mnuRefresh.Index = 0
        resources.ApplyResources(Me.mnuRefresh, "mnuRefresh")
        '
        'mnuListModeSeparator
        '
        Me.mnuListModeSeparator.Index = 1
        resources.ApplyResources(Me.mnuListModeSeparator, "mnuListModeSeparator")
        '
        'mnuGridMode
        '
        Me.mnuGridMode.Checked = True
        Me.mnuGridMode.Index = 2
        resources.ApplyResources(Me.mnuGridMode, "mnuGridMode")
        '
        'mnuListMode
        '
        Me.mnuListMode.Index = 3
        resources.ApplyResources(Me.mnuListMode, "mnuListMode")
        '
        'mnuColumnsSeparator
        '
        Me.mnuColumnsSeparator.Index = 4
        resources.ApplyResources(Me.mnuColumnsSeparator, "mnuColumnsSeparator")
        '
        'mnuShowAll
        '
        Me.mnuShowAll.Index = 5
        resources.ApplyResources(Me.mnuShowAll, "mnuShowAll")
        '
        'mnuColumns
        '
        Me.mnuColumns.Index = 6
        resources.ApplyResources(Me.mnuColumns, "mnuColumns")
        '
        'mnuClose
        '
        Me.mnuClose.Index = 3
        resources.ApplyResources(Me.mnuClose, "mnuClose")
        '
        'mnuFile
        '
        Me.mnuFile.Index = 0
        Me.mnuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuExportPage, Me.mnuExportLog, Me.sep2, Me.mnuClose})
        resources.ApplyResources(Me.mnuFile, "mnuFile")
        '
        'mnuExportPage
        '
        Me.mnuExportPage.Index = 0
        resources.ApplyResources(Me.mnuExportPage, "mnuExportPage")
        '
        'mnuExportLog
        '
        Me.mnuExportLog.Index = 1
        resources.ApplyResources(Me.mnuExportLog, "mnuExportLog")
        '
        'sep2
        '
        Me.sep2.Index = 2
        resources.ApplyResources(Me.sep2, "sep2")
        '
        'mnuMain
        '
        Me.mnuMain.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile, Me.MenuItem1})
        '
        'MenuItem3
        '
        Me.MenuItem3.Index = -1
        resources.ApplyResources(Me.MenuItem3, "MenuItem3")
        '
        'lnkToggleSearch
        '
        resources.ApplyResources(Me.lnkToggleSearch, "lnkToggleSearch")
        Me.lnkToggleSearch.Image = Global.AutomateUI.My.Resources.ToolImages.Search_16x16
        Me.lnkToggleSearch.LinkColor = System.Drawing.Color.Black
        Me.lnkToggleSearch.Name = "lnkToggleSearch"
        Me.lnkToggleSearch.TabStop = True
        '
        'panPageControl
        '
        Me.panPageControl.Controls.Add(Me.btnFirstPage)
        Me.panPageControl.Controls.Add(Me.btnPrevPage)
        Me.panPageControl.Controls.Add(Me.lblViewType)
        Me.panPageControl.Controls.Add(Me.txtCurrentPage)
        Me.panPageControl.Controls.Add(Me.cmbViewType)
        Me.panPageControl.Controls.Add(Me.Label1)
        Me.panPageControl.Controls.Add(Me.lblRowsPerPage)
        Me.panPageControl.Controls.Add(Me.cmbRowsPerPage)
        Me.panPageControl.Controls.Add(Me.btnLastPage)
        Me.panPageControl.Controls.Add(Me.txtTotalPages)
        Me.panPageControl.Controls.Add(Me.btnNextPage)
        resources.ApplyResources(Me.panPageControl, "panPageControl")
        Me.panPageControl.Name = "panPageControl"
        '
        'btnFirstPage
        '
        resources.ApplyResources(Me.btnFirstPage, "btnFirstPage")
        Me.btnFirstPage.Name = "btnFirstPage"
        '
        'btnPrevPage
        '
        resources.ApplyResources(Me.btnPrevPage, "btnPrevPage")
        Me.btnPrevPage.Name = "btnPrevPage"
        '
        'lblViewType
        '
        resources.ApplyResources(Me.lblViewType, "lblViewType")
        Me.lblViewType.Name = "lblViewType"
        '
        'txtCurrentPage
        '
        Me.txtCurrentPage.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.txtCurrentPage, "txtCurrentPage")
        Me.txtCurrentPage.Name = "txtCurrentPage"
        '
        'cmbViewType
        '
        resources.ApplyResources(Me.cmbViewType, "cmbViewType")
        Me.cmbViewType.DisplayMember = "Text"
        Me.cmbViewType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbViewType.FormattingEnabled = True
        Me.cmbViewType.Name = "cmbViewType"
        Me.cmbViewType.ValueMember = "Tag"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'lblRowsPerPage
        '
        resources.ApplyResources(Me.lblRowsPerPage, "lblRowsPerPage")
        Me.lblRowsPerPage.Name = "lblRowsPerPage"
        '
        'cmbRowsPerPage
        '
        resources.ApplyResources(Me.cmbRowsPerPage, "cmbRowsPerPage")
        Me.cmbRowsPerPage.FormattingEnabled = True
        Me.cmbRowsPerPage.Items.AddRange(New Object() {resources.GetString("cmbRowsPerPage.Items"), resources.GetString("cmbRowsPerPage.Items1"), resources.GetString("cmbRowsPerPage.Items2"), resources.GetString("cmbRowsPerPage.Items3"), resources.GetString("cmbRowsPerPage.Items4"), resources.GetString("cmbRowsPerPage.Items5"), resources.GetString("cmbRowsPerPage.Items6"), resources.GetString("cmbRowsPerPage.Items7"), resources.GetString("cmbRowsPerPage.Items8"), resources.GetString("cmbRowsPerPage.Items9"), resources.GetString("cmbRowsPerPage.Items10"), resources.GetString("cmbRowsPerPage.Items11"), resources.GetString("cmbRowsPerPage.Items12")})
        Me.cmbRowsPerPage.Name = "cmbRowsPerPage"
        '
        'btnLastPage
        '
        resources.ApplyResources(Me.btnLastPage, "btnLastPage")
        Me.btnLastPage.Name = "btnLastPage"
        '
        'txtTotalPages
        '
        Me.txtTotalPages.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.txtTotalPages, "txtTotalPages")
        Me.txtTotalPages.Name = "txtTotalPages"
        Me.txtTotalPages.ReadOnly = True
        '
        'btnNextPage
        '
        resources.ApplyResources(Me.btnNextPage, "btnNextPage")
        Me.btnNextPage.Name = "btnNextPage"
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        Me.titleBar.TabStop = False
        '
        'splitPane
        '
        resources.ApplyResources(Me.splitPane, "splitPane")
        Me.splitPane.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitPane.Name = "splitPane"
        '
        'splitPane.Panel1
        '
        Me.splitPane.Panel1.Controls.Add(Me.panSearchConstraints)
        Me.splitPane.Panel1.Controls.Add(Me.panFindControl)
        '
        'splitPane.Panel2
        '
        Me.splitPane.Panel2.Controls.Add(Me.gridLog)
        Me.splitPane.Panel2.Controls.Add(Me.lblNoLogs)
        '
        'panSearchConstraints
        '
        Me.panSearchConstraints.Controls.Add(Me.btnFindAll)
        Me.panSearchConstraints.Controls.Add(Me.btnFindNext)
        Me.panSearchConstraints.Controls.Add(Me.chklSearchColumns)
        Me.panSearchConstraints.Controls.Add(Me.Label3)
        Me.panSearchConstraints.Controls.Add(Me.cmbFind)
        Me.panSearchConstraints.Controls.Add(Me.lFind)
        Me.panSearchConstraints.Controls.Add(Me.chkCase)
        Me.panSearchConstraints.Controls.Add(Me.chkWildcards)
        Me.panSearchConstraints.Controls.Add(Me.ToolStrip1)
        Me.panSearchConstraints.Controls.Add(Me.Label2)
        Me.panSearchConstraints.Controls.Add(Me.chkWholeWord)
        resources.ApplyResources(Me.panSearchConstraints, "panSearchConstraints")
        Me.panSearchConstraints.Name = "panSearchConstraints"
        '
        'btnFindAll
        '
        resources.ApplyResources(Me.btnFindAll, "btnFindAll")
        Me.btnFindAll.Name = "btnFindAll"
        '
        'btnFindNext
        '
        resources.ApplyResources(Me.btnFindNext, "btnFindNext")
        Me.btnFindNext.Name = "btnFindNext"
        '
        'chklSearchColumns
        '
        resources.ApplyResources(Me.chklSearchColumns, "chklSearchColumns")
        Me.chklSearchColumns.FormattingEnabled = True
        Me.chklSearchColumns.Name = "chklSearchColumns"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'cmbFind
        '
        resources.ApplyResources(Me.cmbFind, "cmbFind")
        Me.cmbFind.Name = "cmbFind"
        '
        'lFind
        '
        resources.ApplyResources(Me.lFind, "lFind")
        Me.lFind.Name = "lFind"
        '
        'chkCase
        '
        resources.ApplyResources(Me.chkCase, "chkCase")
        Me.chkCase.Name = "chkCase"
        '
        'chkWildcards
        '
        resources.ApplyResources(Me.chkWildcards, "chkWildcards")
        Me.chkWildcards.Name = "chkWildcards"
        '
        'ToolStrip1
        '
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnColour})
        Me.ToolStrip1.Name = "ToolStrip1"
        '
        'btnColour
        '
        Me.btnColour.CurrentColor = System.Drawing.Color.FromArgb(CType(CType(204, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(204, Byte), Integer))
        Me.btnColour.CustomColors = Nothing
        Me.btnColour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnColour, "btnColour")
        Me.btnColour.Name = "btnColour"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'chkWholeWord
        '
        resources.ApplyResources(Me.chkWholeWord, "chkWholeWord")
        Me.chkWholeWord.Name = "chkWholeWord"
        '
        'panFindControl
        '
        Me.panFindControl.Controls.Add(Me.btnCancel)
        Me.panFindControl.Controls.Add(Me.progressFind)
        Me.panFindControl.Controls.Add(Me.btnClear)
        resources.ApplyResources(Me.panFindControl, "panFindControl")
        Me.panFindControl.Name = "panFindControl"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        '
        'progressFind
        '
        resources.ApplyResources(Me.progressFind, "progressFind")
        Me.progressFind.Name = "progressFind"
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.Name = "btnClear"
        '
        'gridLog
        '
        Me.gridLog.AllowUserToAddRows = False
        Me.gridLog.AllowUserToDeleteRows = False
        Me.gridLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridLog.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells
        Me.gridLog.BackgroundColor = System.Drawing.Color.White
        Me.gridLog.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.gridLog.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText
        Me.gridLog.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.[Single]
        Me.gridLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        resources.ApplyResources(Me.gridLog, "gridLog")
        Me.gridLog.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.gridLog.GridColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(164, Byte), Integer), CType(CType(193, Byte), Integer))
        Me.gridLog.Name = "gridLog"
        Me.gridLog.ReadOnly = True
        Me.gridLog.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.gridLog.RowHeadersVisible = False
        Me.gridLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'lblNoLogs
        '
        resources.ApplyResources(Me.lblNoLogs, "lblNoLogs")
        Me.lblNoLogs.Name = "lblNoLogs"
        '
        'frmLogViewer
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lnkToggleSearch)
        Me.Controls.Add(Me.panPageControl)
        Me.Controls.Add(Me.titleBar)
        Me.Controls.Add(Me.splitPane)
        Me.HelpButton = True
        Me.Menu = Me.mnuMain
        Me.Name = "frmLogViewer"
        Me.panPageControl.ResumeLayout(false)
        Me.panPageControl.PerformLayout
        Me.splitPane.Panel1.ResumeLayout(false)
        Me.splitPane.Panel2.ResumeLayout(false)
        Me.splitPane.Panel2.PerformLayout
        CType(Me.splitPane,System.ComponentModel.ISupportInitialize).EndInit
        Me.splitPane.ResumeLayout(false)
        Me.panSearchConstraints.ResumeLayout(false)
        Me.panSearchConstraints.PerformLayout
        Me.ToolStrip1.ResumeLayout(false)
        Me.ToolStrip1.PerformLayout
        Me.panFindControl.ResumeLayout(false)
        CType(Me.gridLog,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents MenuItem1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuRefresh As System.Windows.Forms.MenuItem
    Friend WithEvents mnuClose As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuExportPage As System.Windows.Forms.MenuItem
    Friend WithEvents mnuExportLog As System.Windows.Forms.MenuItem
    Friend WithEvents sep2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuMain As System.Windows.Forms.MainMenu
    Friend WithEvents cmbRowsPerPage As System.Windows.Forms.ComboBox
    Friend WithEvents btnFirstPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnLastPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnPrevPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents btnColour As AutomateUI.clsColorButton
    Friend WithEvents btnClear As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnFindAll As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents chkWildcards As System.Windows.Forms.CheckBox
    Friend WithEvents chkWholeWord As System.Windows.Forms.CheckBox
    Friend WithEvents chkCase As System.Windows.Forms.CheckBox
    Friend WithEvents lFind As System.Windows.Forms.Label
    Friend WithEvents cmbFind As System.Windows.Forms.ComboBox
    Friend WithEvents txtTotalPages As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents btnNextPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnFindNext As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents gridLog As System.Windows.Forms.DataGridView
    Friend WithEvents lblRowsPerPage As System.Windows.Forms.Label
    Friend WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents txtCurrentPage As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents mnuColumns As System.Windows.Forms.MenuItem
    Friend WithEvents MenuItem3 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuShowAll As System.Windows.Forms.MenuItem
    Friend WithEvents mnuListModeSeparator As System.Windows.Forms.MenuItem
    Friend WithEvents mnuListMode As System.Windows.Forms.MenuItem
    Friend WithEvents mnuGridMode As System.Windows.Forms.MenuItem
    Friend WithEvents lblNoLogs As System.Windows.Forms.Label
    Friend WithEvents cmbViewType As System.Windows.Forms.ComboBox
    Friend WithEvents lblViewType As System.Windows.Forms.Label
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents progressFind As System.Windows.Forms.ProgressBar
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents splitPane As System.Windows.Forms.SplitContainer
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents chklSearchColumns As System.Windows.Forms.CheckedListBox
    Friend WithEvents mnuColumnsSeparator As System.Windows.Forms.MenuItem
    Private WithEvents panSearchConstraints As System.Windows.Forms.Panel
    Friend WithEvents panFindControl As System.Windows.Forms.Panel
    Private WithEvents panPageControl As System.Windows.Forms.Panel
    Private WithEvents lnkToggleSearch As AutomateControls.BulletedLinkLabel

End Class
