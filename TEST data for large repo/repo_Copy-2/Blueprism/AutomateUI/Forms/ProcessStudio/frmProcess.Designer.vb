Imports AutomateUI.Classes.UserInterface
Imports BluePrism.Images

Partial Friend Class frmProcess


    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private WithEvents MenuItem3 As System.Windows.Forms.MenuItem
    Friend WithEvents ntfyDebug As System.Windows.Forms.NotifyIcon
    Friend WithEvents ntfyContext As System.Windows.Forms.ContextMenu
    Friend WithEvents mnuTaskTrayGo As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTaskTrayStep As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTaskTrayStepOver As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTaskTrayRestart As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTaskTrayStop As System.Windows.Forms.MenuItem
    Friend WithEvents ttTips As System.Windows.Forms.ToolTip
    Friend WithEvents timStatusBarTimer As System.Windows.Forms.Timer
    Friend WithEvents mnuTaskTrayStepOut As System.Windows.Forms.MenuItem
    Friend WithEvents lblFast As System.Windows.Forms.Label
    Friend WithEvents lblSlow As System.Windows.Forms.Label
    Friend WithEvents TrackBar1 As System.Windows.Forms.TrackBar
    Friend WithEvents lblShowDetails As System.Windows.Forms.Label
    Friend WithEvents mProcessViewer As ctlProcessViewer
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TrackBar2 As System.Windows.Forms.TrackBar
    Friend WithEvents Label3 As System.Windows.Forms.Label

    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProcess))
        Me.toolstripCont = New System.Windows.Forms.ToolStripContainer()
        Me.stsBar = New System.Windows.Forms.StatusStrip()
        Me.lblStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.btnLogging = New System.Windows.Forms.ToolStripButton()
        Me.btnShowSysManLogger = New System.Windows.Forms.ToolStripButton()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.mProcessViewer = New AutomateUI.ctlProcessViewer()
        Me.txtLogMessages = New AutomateUI.ctlRichTextBox()
        Me.pnlExpressionRow = New System.Windows.Forms.Panel()
        Me.lStoreIn = New System.Windows.Forms.Label()
        Me.exprEdit = New AutomateUI.ctlExpressionEdit()
        Me.objStoreInEdit = New AutomateUI.ctlStoreInEdit()
        Me.lExpression = New System.Windows.Forms.Label()
        Me.SkillsToolbarPanel = New System.Windows.Forms.Panel()
        Me.toolstripTools = New System.Windows.Forms.ToolStrip()
        Me.btnPointer = New System.Windows.Forms.ToolStripButton()
        Me.btnLink = New System.Windows.Forms.ToolStripButton()
        Me.btnBlock = New System.Windows.Forms.ToolStripButton()
        Me.btnRead = New System.Windows.Forms.ToolStripButton()
        Me.btnWrite = New System.Windows.Forms.ToolStripButton()
        Me.btnNavigate = New System.Windows.Forms.ToolStripButton()
        Me.btnCode = New System.Windows.Forms.ToolStripButton()
        Me.btnWait = New System.Windows.Forms.ToolStripButton()
        Me.btnProcess = New System.Windows.Forms.ToolStripButton()
        Me.btnPage = New System.Windows.Forms.ToolStripButton()
        Me.btnAction = New System.Windows.Forms.ToolStripButton()
        Me.btnDecision = New System.Windows.Forms.ToolStripButton()
        Me.btnChoice = New System.Windows.Forms.ToolStripButton()
        Me.btnCalculation = New System.Windows.Forms.ToolStripButton()
        Me.btnMultipleCalculation = New System.Windows.Forms.ToolStripButton()
        Me.btnData = New System.Windows.Forms.ToolStripButton()
        Me.btnCollection = New System.Windows.Forms.ToolStripButton()
        Me.btnLoop = New System.Windows.Forms.ToolStripButton()
        Me.btnNote = New System.Windows.Forms.ToolStripButton()
        Me.btnAnchor = New System.Windows.Forms.ToolStripButton()
        Me.btnEnd = New System.Windows.Forms.ToolStripButton()
        Me.btnAlert = New System.Windows.Forms.ToolStripButton()
        Me.btnException = New System.Windows.Forms.ToolStripButton()
        Me.btnRecover = New System.Windows.Forms.ToolStripButton()
        Me.btnResume = New System.Windows.Forms.ToolStripButton()
        Me.toolstripFont = New System.Windows.Forms.ToolStrip()
        Me.cmbFont = New System.Windows.Forms.ToolStripComboBox()
        Me.cmbSize = New System.Windows.Forms.ToolStripComboBox()
        Me.btnBold = New System.Windows.Forms.ToolStripButton()
        Me.btnItalic = New System.Windows.Forms.ToolStripButton()
        Me.btnUnderline = New System.Windows.Forms.ToolStripButton()
        Me.btnFontColour = New AutomateUI.clsColorButton()
        Me.mnuMain = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuChkUseSummaries = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFileSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackupNever = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup2 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup3 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup4 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup5 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup10 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup15 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup20 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup25 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBackup30 = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuReports = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuElementUsage = New System.Windows.Forms.ToolStripMenuItem()
        Me.PrintToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPrintPreview = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPrintOnSinglePage = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPrintProcess = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuExport = New System.Windows.Forms.ToolStripMenuItem()
        Me.miExportThisPage = New System.Windows.Forms.ToolStripMenuItem()
        Me.miExportThisProcess = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator13 = New System.Windows.Forms.ToolStripSeparator()
        Me.miExportRelease = New System.Windows.Forms.ToolStripMenuItem()
        Me.miExportAdhocPackage = New System.Windows.Forms.ToolStripMenuItem()
        Me.misepExportRelease = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuFileClose = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEdit = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditUndo = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditRedo = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuEditCut = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditCopy = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditPaste = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditSelectAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectedStagesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EnableLoggingSelected = New System.Windows.Forms.ToolStripMenuItem()
        Me.DisableLoggingSelected = New System.Windows.Forms.ToolStripMenuItem()
        Me.EnableErrorLoggingSelected = New System.Windows.Forms.ToolStripMenuItem()
        Me.AllStagesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EnableLoggingAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.DisableLoggingAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.EnableErrorLoggingAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator12 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuEditProperties = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditDependencies = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEditAdvancedSearch = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator9 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuEditAddPage = New System.Windows.Forms.ToolStripMenuItem()
        Me.ViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ZoomToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.z400 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z200 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z150 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z100 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z75 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z50 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z25 = New System.Windows.Forms.ToolStripMenuItem()
        Me.zSep = New System.Windows.Forms.ToolStripSeparator()
        Me.zDyn = New System.Windows.Forms.ToolStripTextBox()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuTransparency = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTransparencyNone = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTransparencyLow = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTransparencyMedium = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTransparencyHigh = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTransparencyVeryHigh = New System.Windows.Forms.ToolStripMenuItem()
        Me.AlwaysOnTopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFullScreen = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.chkSnap = New System.Windows.Forms.ToolStripMenuItem()
        Me.chkGrid = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem13 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuDynCursor = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolBoxes = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsSavePositions = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsLockPositions = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuShowAllTools = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuHideAllTools = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator5 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuToolsStandard = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsTools = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsDebug = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsSearch = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsFont = New System.Windows.Forms.ToolStripMenuItem()
        Me.SkillToolbarVisiblityToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator8 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuToolsProcessMI = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuToolsDataItemWatch = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTools = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPointer = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuLink = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBlock = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRead = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuWrite = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuNavigate = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCode = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuWait = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuProcess = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPage = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAction = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDecision = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuChoice = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalculation = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuMultipleCalculation = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDataItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCollection = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuLoop = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuNote = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEnd = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAnchor = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAlert = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuException = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRecover = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuResume = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem9 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuApplicationModeller = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebug = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugGo = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugStep = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugStepOut = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugStepOver = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugStop = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugRestart = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem4 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuDebugValidate = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuStartParams = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCalculationZoom = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuClearAllBreakpoints = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFocusDebugStage = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDebugExceptions = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTopic = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem6 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuOpenHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSearch = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem7 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuAPIHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRequest = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem8 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuAbout = New System.Windows.Forms.ToolStripMenuItem()
        Me.toolstripStandard = New System.Windows.Forms.ToolStrip()
        Me.btnSave = New System.Windows.Forms.ToolStripButton()
        Me.btnPrint = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnCut = New System.Windows.Forms.ToolStripButton()
        Me.btnCopy = New System.Windows.Forms.ToolStripButton()
        Me.btnPaste = New System.Windows.Forms.ToolStripButton()
        Me.btnUndo = New System.Windows.Forms.ToolStripSplitButton()
        Me.btnRedo = New System.Windows.Forms.ToolStripSplitButton()
        Me.btnRefresh = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripLabel1 = New System.Windows.Forms.ToolStripLabel()
        Me.mZoomToolstripCombo = New System.Windows.Forms.ToolStripComboBox()
        Me.mZoomInToolstripButton = New System.Windows.Forms.ToolStripButton()
        Me.mZoomOutToolstripButton = New System.Windows.Forms.ToolStripButton()
        Me.toolstripDebug = New System.Windows.Forms.ToolStrip()
        Me.btnMenuDebugGo = New System.Windows.Forms.ToolStripSplitButton()
        Me.btnMenuDebugPause = New System.Windows.Forms.ToolStripButton()
        Me.btnMenuDebugStep = New System.Windows.Forms.ToolStripButton()
        Me.btnMenuDebugStepOver = New System.Windows.Forms.ToolStripButton()
        Me.btnMenuDebugStepOut = New System.Windows.Forms.ToolStripButton()
        Me.btnMenuDebugReset = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator10 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnValidate = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator11 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnMenuDebugLaunchApp = New System.Windows.Forms.ToolStripButton()
        Me.btnMenuApplicationModeller = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnBreakpoint = New System.Windows.Forms.ToolStripButton()
        Me.btnWatch = New System.Windows.Forms.ToolStripButton()
        Me.btnProcessMI = New System.Windows.Forms.ToolStripButton()
        Me.btnMenuFullScreen = New System.Windows.Forms.ToolStripButton()
        Me.btnPanViewDropDown = New System.Windows.Forms.ToolStripSplitButton()
        Me.toolstripSearch = New AutomateUI.DiagramSearchToolstrip()
        Me.ntfyDebug = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ntfyContext = New System.Windows.Forms.ContextMenu()
        Me.mnuTaskTrayGo = New System.Windows.Forms.MenuItem()
        Me.mnuTaskTrayStep = New System.Windows.Forms.MenuItem()
        Me.mnuTaskTrayStepOver = New System.Windows.Forms.MenuItem()
        Me.mnuTaskTrayStepOut = New System.Windows.Forms.MenuItem()
        Me.mnuTaskTrayRestart = New System.Windows.Forms.MenuItem()
        Me.mnuTaskTrayStop = New System.Windows.Forms.MenuItem()
        Me.ttTips = New System.Windows.Forms.ToolTip(Me.components)
        Me.TrackBar1 = New System.Windows.Forms.TrackBar()
        Me.timStatusBarTimer = New System.Windows.Forms.Timer(Me.components)
        Me.lblFast = New System.Windows.Forms.Label()
        Me.lblSlow = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TrackBar2 = New System.Windows.Forms.TrackBar()
        Me.lblShowDetails = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.mnuUndockAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.toolstripCont.BottomToolStripPanel.SuspendLayout()
        Me.toolstripCont.ContentPanel.SuspendLayout()
        Me.toolstripCont.LeftToolStripPanel.SuspendLayout()
        Me.toolstripCont.TopToolStripPanel.SuspendLayout()
        Me.toolstripCont.SuspendLayout()
        Me.stsBar.SuspendLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlExpressionRow.SuspendLayout()
        Me.toolstripTools.SuspendLayout()
        Me.toolstripFont.SuspendLayout()
        Me.mnuMain.SuspendLayout()
        Me.toolstripStandard.SuspendLayout()
        Me.toolstripDebug.SuspendLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TrackBar2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'toolstripCont
        '
        '
        'toolstripCont.BottomToolStripPanel
        '
        Me.toolstripCont.BottomToolStripPanel.Controls.Add(Me.stsBar)
        '
        'toolstripCont.ContentPanel
        '
        Me.toolstripCont.ContentPanel.Controls.Add(Me.splitMain)
        Me.toolstripCont.ContentPanel.Controls.Add(Me.pnlExpressionRow)
        Me.toolstripCont.ContentPanel.Controls.Add(Me.SkillsToolbarPanel)
        resources.ApplyResources(Me.toolstripCont.ContentPanel, "toolstripCont.ContentPanel")
        resources.ApplyResources(Me.toolstripCont, "toolstripCont")
        '
        'toolstripCont.LeftToolStripPanel
        '
        Me.toolstripCont.LeftToolStripPanel.Controls.Add(Me.toolstripTools)
        Me.toolstripCont.Name = "toolstripCont"
        '
        'toolstripCont.TopToolStripPanel
        '
        Me.toolstripCont.TopToolStripPanel.Controls.Add(Me.mnuMain)
        Me.toolstripCont.TopToolStripPanel.Controls.Add(Me.toolstripStandard)
        Me.toolstripCont.TopToolStripPanel.Controls.Add(Me.toolstripDebug)
        Me.toolstripCont.TopToolStripPanel.Controls.Add(Me.toolstripFont)
        Me.toolstripCont.TopToolStripPanel.Controls.Add(Me.toolstripSearch)
        resources.ApplyResources(Me.toolstripCont.TopToolStripPanel, "toolstripCont.TopToolStripPanel")
        '
        'stsBar
        '
        Me.stsBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        resources.ApplyResources(Me.stsBar, "stsBar")
        Me.stsBar.ForeColor = System.Drawing.Color.White
        Me.stsBar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblStatus, Me.btnLogging, Me.btnShowSysManLogger})
        Me.stsBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow
        Me.stsBar.Name = "stsBar"
        Me.stsBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode
        Me.stsBar.ShowItemToolTips = True
        '
        'lblStatus
        '
        resources.ApplyResources(Me.lblStatus, "lblStatus")
        Me.lblStatus.Name = "lblStatus"
        '
        'btnLogging
        '
        Me.btnLogging.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.btnLogging.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnLogging, "btnLogging")
        Me.btnLogging.Name = "btnLogging"
        '
        'btnShowSysManLogger
        '
        Me.btnShowSysManLogger.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.btnShowSysManLogger.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnShowSysManLogger, "btnShowSysManLogger")
        Me.btnShowSysManLogger.Name = "btnShowSysManLogger"
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.mProcessViewer)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.txtLogMessages)
        '
        'mProcessViewer
        '
        Me.mProcessViewer.BackColor = System.Drawing.Color.White
        Me.mProcessViewer.ClipboardProcess = Nothing
        Me.mProcessViewer.ClipboardProcessLocation = CType(resources.GetObject("mProcessViewer.ClipboardProcessLocation"), System.Drawing.PointF)
        resources.ApplyResources(Me.mProcessViewer, "mProcessViewer")
        Me.mProcessViewer.MouseWheelEnabled = True
        Me.mProcessViewer.Name = "mProcessViewer"
        Me.mProcessViewer.OpenedAsDebugSubProcess = False
        Me.mProcessViewer.RunAtNearFullSpeed = False
        Me.mProcessViewer.ShowGridLines = True
        Me.mProcessViewer.SnapToGrid = True
        Me.mProcessViewer.SuppressProcessDisposal = False
        Me.mProcessViewer.ToolDragging = False
        '
        'txtLogMessages
        '
        Me.txtLogMessages.BackColor = System.Drawing.Color.White
        Me.txtLogMessages.DetectUrls = False
        resources.ApplyResources(Me.txtLogMessages, "txtLogMessages")
        Me.txtLogMessages.Name = "txtLogMessages"
        Me.txtLogMessages.ReadOnly = True
        '
        'pnlExpressionRow
        '
        Me.pnlExpressionRow.BackColor = System.Drawing.SystemColors.Control
        Me.pnlExpressionRow.Controls.Add(Me.lStoreIn)
        Me.pnlExpressionRow.Controls.Add(Me.exprEdit)
        Me.pnlExpressionRow.Controls.Add(Me.objStoreInEdit)
        Me.pnlExpressionRow.Controls.Add(Me.lExpression)
        resources.ApplyResources(Me.pnlExpressionRow, "pnlExpressionRow")
        Me.pnlExpressionRow.Name = "pnlExpressionRow"
        '
        'lStoreIn
        '
        resources.ApplyResources(Me.lStoreIn, "lStoreIn")
        Me.lStoreIn.Name = "lStoreIn"
        '
        'exprEdit
        '
        resources.ApplyResources(Me.exprEdit, "exprEdit")
        Me.exprEdit.BackColor = System.Drawing.Color.White
        Me.exprEdit.Border = False
        Me.exprEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.exprEdit.HighlightingEnabled = True
        Me.exprEdit.IsDecision = False
        Me.exprEdit.Name = "exprEdit"
        Me.exprEdit.PasswordChar = Global.Microsoft.VisualBasic.ChrW(0)
        Me.exprEdit.Stage = Nothing
        '
        'objStoreInEdit
        '
        Me.objStoreInEdit.AllowDrop = True
        resources.ApplyResources(Me.objStoreInEdit, "objStoreInEdit")
        Me.objStoreInEdit.AutoCreateDefault = Nothing
        Me.objStoreInEdit.BackColor = System.Drawing.Color.White
        Me.objStoreInEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.objStoreInEdit.Name = "objStoreInEdit"
        Me.objStoreInEdit.PasswordChar = Global.Microsoft.VisualBasic.ChrW(0)
        '
        'lExpression
        '
        resources.ApplyResources(Me.lExpression, "lExpression")
        Me.lExpression.Name = "lExpression"
        '
        'SkillsToolbarPanel
        '
        resources.ApplyResources(Me.SkillsToolbarPanel, "SkillsToolbarPanel")
        Me.SkillsToolbarPanel.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.SkillsToolbarPanel.Name = "SkillsToolbarPanel"
        '
        'toolstripTools
        '
        resources.ApplyResources(Me.toolstripTools, "toolstripTools")
        Me.toolstripTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.toolstripTools.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnPointer, Me.btnLink, Me.btnBlock, Me.btnRead, Me.btnWrite, Me.btnNavigate, Me.btnCode, Me.btnWait, Me.btnProcess, Me.btnPage, Me.btnAction, Me.btnDecision, Me.btnChoice, Me.btnCalculation, Me.btnMultipleCalculation, Me.btnData, Me.btnCollection, Me.btnLoop, Me.btnNote, Me.btnAnchor, Me.btnEnd, Me.btnAlert, Me.btnException, Me.btnRecover, Me.btnResume})
        Me.toolstripTools.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow
        Me.toolstripTools.Name = "toolstripTools"
        '
        'btnPointer
        '
        Me.btnPointer.AutoToolTip = False
        Me.btnPointer.Checked = True
        Me.btnPointer.CheckState = System.Windows.Forms.CheckState.Checked
        resources.ApplyResources(Me.btnPointer, "btnPointer")
        Me.btnPointer.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnPointer.Name = "btnPointer"
        Me.btnPointer.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnLink
        '
        Me.btnLink.AutoToolTip = False
        resources.ApplyResources(Me.btnLink, "btnLink")
        Me.btnLink.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnLink.Name = "btnLink"
        Me.btnLink.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnBlock
        '
        resources.ApplyResources(Me.btnBlock, "btnBlock")
        Me.btnBlock.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnBlock.Name = "btnBlock"
        Me.btnBlock.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnRead
        '
        Me.btnRead.AutoToolTip = False
        resources.ApplyResources(Me.btnRead, "btnRead")
        Me.btnRead.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnRead.Name = "btnRead"
        Me.btnRead.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnWrite
        '
        Me.btnWrite.AutoToolTip = False
        resources.ApplyResources(Me.btnWrite, "btnWrite")
        Me.btnWrite.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnWrite.Name = "btnWrite"
        Me.btnWrite.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnNavigate
        '
        Me.btnNavigate.AutoToolTip = False
        resources.ApplyResources(Me.btnNavigate, "btnNavigate")
        Me.btnNavigate.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnNavigate.Name = "btnNavigate"
        Me.btnNavigate.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnCode
        '
        Me.btnCode.AutoToolTip = False
        resources.ApplyResources(Me.btnCode, "btnCode")
        Me.btnCode.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnCode.Name = "btnCode"
        Me.btnCode.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnWait
        '
        Me.btnWait.AutoToolTip = False
        resources.ApplyResources(Me.btnWait, "btnWait")
        Me.btnWait.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnWait.Name = "btnWait"
        Me.btnWait.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnProcess
        '
        Me.btnProcess.AutoToolTip = False
        resources.ApplyResources(Me.btnProcess, "btnProcess")
        Me.btnProcess.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnProcess.Name = "btnProcess"
        Me.btnProcess.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnPage
        '
        Me.btnPage.AutoToolTip = False
        resources.ApplyResources(Me.btnPage, "btnPage")
        Me.btnPage.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnPage.Name = "btnPage"
        Me.btnPage.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnAction
        '
        Me.btnAction.AutoToolTip = False
        resources.ApplyResources(Me.btnAction, "btnAction")
        Me.btnAction.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnAction.Name = "btnAction"
        Me.btnAction.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnDecision
        '
        Me.btnDecision.AutoToolTip = False
        resources.ApplyResources(Me.btnDecision, "btnDecision")
        Me.btnDecision.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnDecision.Name = "btnDecision"
        Me.btnDecision.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnChoice
        '
        Me.btnChoice.AutoToolTip = False
        resources.ApplyResources(Me.btnChoice, "btnChoice")
        Me.btnChoice.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnChoice.Name = "btnChoice"
        Me.btnChoice.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnCalculation
        '
        Me.btnCalculation.AutoToolTip = False
        resources.ApplyResources(Me.btnCalculation, "btnCalculation")
        Me.btnCalculation.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnCalculation.Name = "btnCalculation"
        Me.btnCalculation.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnMultipleCalculation
        '
        resources.ApplyResources(Me.btnMultipleCalculation, "btnMultipleCalculation")
        Me.btnMultipleCalculation.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnMultipleCalculation.Name = "btnMultipleCalculation"
        Me.btnMultipleCalculation.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnData
        '
        Me.btnData.AutoToolTip = False
        resources.ApplyResources(Me.btnData, "btnData")
        Me.btnData.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnData.Name = "btnData"
        Me.btnData.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnCollection
        '
        Me.btnCollection.AutoToolTip = False
        resources.ApplyResources(Me.btnCollection, "btnCollection")
        Me.btnCollection.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnCollection.Name = "btnCollection"
        Me.btnCollection.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnLoop
        '
        Me.btnLoop.AutoToolTip = False
        resources.ApplyResources(Me.btnLoop, "btnLoop")
        Me.btnLoop.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnLoop.Name = "btnLoop"
        Me.btnLoop.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnNote
        '
        Me.btnNote.AutoToolTip = False
        resources.ApplyResources(Me.btnNote, "btnNote")
        Me.btnNote.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnNote.Name = "btnNote"
        Me.btnNote.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnAnchor
        '
        Me.btnAnchor.AutoToolTip = False
        resources.ApplyResources(Me.btnAnchor, "btnAnchor")
        Me.btnAnchor.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnAnchor.Name = "btnAnchor"
        Me.btnAnchor.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnEnd
        '
        Me.btnEnd.AutoToolTip = False
        resources.ApplyResources(Me.btnEnd, "btnEnd")
        Me.btnEnd.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnEnd.Name = "btnEnd"
        Me.btnEnd.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnAlert
        '
        resources.ApplyResources(Me.btnAlert, "btnAlert")
        Me.btnAlert.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnAlert.Name = "btnAlert"
        Me.btnAlert.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnException
        '
        resources.ApplyResources(Me.btnException, "btnException")
        Me.btnException.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnException.Name = "btnException"
        Me.btnException.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnRecover
        '
        resources.ApplyResources(Me.btnRecover, "btnRecover")
        Me.btnRecover.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnRecover.Name = "btnRecover"
        Me.btnRecover.Padding = New System.Windows.Forms.Padding(1)
        '
        'btnResume
        '
        resources.ApplyResources(Me.btnResume, "btnResume")
        Me.btnResume.Margin = New System.Windows.Forms.Padding(5, 1, 5, 2)
        Me.btnResume.Name = "btnResume"
        Me.btnResume.Padding = New System.Windows.Forms.Padding(1)
        '
        'toolstripFont
        '
        resources.ApplyResources(Me.toolstripFont, "toolstripFont")
        Me.toolstripFont.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.cmbFont, Me.cmbSize, Me.btnBold, Me.btnItalic, Me.btnUnderline, Me.btnFontColour})
        Me.toolstripFont.Name = "toolstripFont"
        '
        'cmbFont
        '
        Me.cmbFont.Name = "cmbFont"
        resources.ApplyResources(Me.cmbFont, "cmbFont")
        '
        'cmbSize
        '
        resources.ApplyResources(Me.cmbSize, "cmbSize")
        Me.cmbSize.DropDownWidth = 75
        Me.cmbSize.Name = "cmbSize"
        '
        'btnBold
        '
        Me.btnBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnBold, "btnBold")
        Me.btnBold.Name = "btnBold"
        '
        'btnItalic
        '
        Me.btnItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnItalic, "btnItalic")
        Me.btnItalic.Name = "btnItalic"
        '
        'btnUnderline
        '
        Me.btnUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnUnderline, "btnUnderline")
        Me.btnUnderline.Name = "btnUnderline"
        '
        'btnFontColour
        '
        Me.btnFontColour.CurrentColor = System.Drawing.Color.Black
        Me.btnFontColour.CustomColors = Nothing
        Me.btnFontColour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnFontColour, "btnFontColour")
        Me.btnFontColour.Name = "btnFontColour"
        '
        'mnuMain
        '
        resources.ApplyResources(Me.mnuMain, "mnuMain")
        Me.mnuMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.mnuEdit, Me.ViewToolStripMenuItem, Me.mnuTools, Me.mnuDebug, Me.HelpToolStripMenuItem})
        Me.mnuMain.Name = "mnuMain"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuChkUseSummaries, Me.mnuFileSave, Me.mnuSaveAs, Me.mnuBackup, Me.mnuReports, Me.PrintToolStripMenuItem, Me.mnuExport, Me.ToolStripMenuItem2, Me.mnuFileClose, Me.mnuExit})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        resources.ApplyResources(Me.FileToolStripMenuItem, "FileToolStripMenuItem")
        '
        'mnuChkUseSummaries
        '
        Me.mnuChkUseSummaries.Name = "mnuChkUseSummaries"
        resources.ApplyResources(Me.mnuChkUseSummaries, "mnuChkUseSummaries")
        '
        'mnuFileSave
        '
        resources.ApplyResources(Me.mnuFileSave, "mnuFileSave")
        Me.mnuFileSave.Name = "mnuFileSave"
        '
        'mnuSaveAs
        '
        resources.ApplyResources(Me.mnuSaveAs, "mnuSaveAs")
        Me.mnuSaveAs.Name = "mnuSaveAs"
        '
        'mnuBackup
        '
        Me.mnuBackup.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuBackupNever, Me.mnuBackup1, Me.mnuBackup2, Me.mnuBackup3, Me.mnuBackup4, Me.mnuBackup5, Me.mnuBackup10, Me.mnuBackup15, Me.mnuBackup20, Me.mnuBackup25, Me.mnuBackup30})
        resources.ApplyResources(Me.mnuBackup, "mnuBackup")
        Me.mnuBackup.Name = "mnuBackup"
        '
        'mnuBackupNever
        '
        Me.mnuBackupNever.Name = "mnuBackupNever"
        resources.ApplyResources(Me.mnuBackupNever, "mnuBackupNever")
        '
        'mnuBackup1
        '
        Me.mnuBackup1.Name = "mnuBackup1"
        resources.ApplyResources(Me.mnuBackup1, "mnuBackup1")
        '
        'mnuBackup2
        '
        Me.mnuBackup2.Name = "mnuBackup2"
        resources.ApplyResources(Me.mnuBackup2, "mnuBackup2")
        '
        'mnuBackup3
        '
        Me.mnuBackup3.Name = "mnuBackup3"
        resources.ApplyResources(Me.mnuBackup3, "mnuBackup3")
        '
        'mnuBackup4
        '
        Me.mnuBackup4.Name = "mnuBackup4"
        resources.ApplyResources(Me.mnuBackup4, "mnuBackup4")
        '
        'mnuBackup5
        '
        Me.mnuBackup5.Name = "mnuBackup5"
        resources.ApplyResources(Me.mnuBackup5, "mnuBackup5")
        '
        'mnuBackup10
        '
        Me.mnuBackup10.Name = "mnuBackup10"
        resources.ApplyResources(Me.mnuBackup10, "mnuBackup10")
        '
        'mnuBackup15
        '
        Me.mnuBackup15.Name = "mnuBackup15"
        resources.ApplyResources(Me.mnuBackup15, "mnuBackup15")
        '
        'mnuBackup20
        '
        Me.mnuBackup20.Name = "mnuBackup20"
        resources.ApplyResources(Me.mnuBackup20, "mnuBackup20")
        '
        'mnuBackup25
        '
        Me.mnuBackup25.Name = "mnuBackup25"
        resources.ApplyResources(Me.mnuBackup25, "mnuBackup25")
        '
        'mnuBackup30
        '
        Me.mnuBackup30.Name = "mnuBackup30"
        resources.ApplyResources(Me.mnuBackup30, "mnuBackup30")
        '
        'mnuReports
        '
        Me.mnuReports.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuElementUsage})
        Me.mnuReports.Name = "mnuReports"
        resources.ApplyResources(Me.mnuReports, "mnuReports")
        '
        'mnuElementUsage
        '
        Me.mnuElementUsage.Name = "mnuElementUsage"
        resources.ApplyResources(Me.mnuElementUsage, "mnuElementUsage")
        '
        'PrintToolStripMenuItem
        '
        Me.PrintToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuPrintPreview, Me.mnuPrintOnSinglePage, Me.mnuPrintProcess})
        resources.ApplyResources(Me.PrintToolStripMenuItem, "PrintToolStripMenuItem")
        Me.PrintToolStripMenuItem.Name = "PrintToolStripMenuItem"
        '
        'mnuPrintPreview
        '
        resources.ApplyResources(Me.mnuPrintPreview, "mnuPrintPreview")
        Me.mnuPrintPreview.Name = "mnuPrintPreview"
        '
        'mnuPrintOnSinglePage
        '
        Me.mnuPrintOnSinglePage.Name = "mnuPrintOnSinglePage"
        resources.ApplyResources(Me.mnuPrintOnSinglePage, "mnuPrintOnSinglePage")
        '
        'mnuPrintProcess
        '
        Me.mnuPrintProcess.Name = "mnuPrintProcess"
        resources.ApplyResources(Me.mnuPrintProcess, "mnuPrintProcess")
        '
        'mnuExport
        '
        Me.mnuExport.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.miExportThisPage, Me.miExportThisProcess, Me.ToolStripSeparator13, Me.miExportRelease})
        Me.mnuExport.Name = "mnuExport"
        resources.ApplyResources(Me.mnuExport, "mnuExport")
        '
        'miExportThisPage
        '
        resources.ApplyResources(Me.miExportThisPage, "miExportThisPage")
        Me.miExportThisPage.Name = "miExportThisPage"
        '
        'miExportThisProcess
        '
        resources.ApplyResources(Me.miExportThisProcess, "miExportThisProcess")
        Me.miExportThisProcess.Name = "miExportThisProcess"
        '
        'ToolStripSeparator13
        '
        Me.ToolStripSeparator13.Name = "ToolStripSeparator13"
        resources.ApplyResources(Me.ToolStripSeparator13, "ToolStripSeparator13")
        '
        'miExportRelease
        '
        Me.miExportRelease.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.miExportAdhocPackage, Me.misepExportRelease})
        Me.miExportRelease.Name = "miExportRelease"
        resources.ApplyResources(Me.miExportRelease, "miExportRelease")
        '
        'miExportAdhocPackage
        '
        Me.miExportAdhocPackage.Name = "miExportAdhocPackage"
        resources.ApplyResources(Me.miExportAdhocPackage, "miExportAdhocPackage")
        '
        'misepExportRelease
        '
        Me.misepExportRelease.Name = "misepExportRelease"
        resources.ApplyResources(Me.misepExportRelease, "misepExportRelease")
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        resources.ApplyResources(Me.ToolStripMenuItem2, "ToolStripMenuItem2")
        '
        'mnuFileClose
        '
        Me.mnuFileClose.Name = "mnuFileClose"
        resources.ApplyResources(Me.mnuFileClose, "mnuFileClose")
        '
        'mnuExit
        '
        Me.mnuExit.Name = "mnuExit"
        resources.ApplyResources(Me.mnuExit, "mnuExit")
        '
        'mnuEdit
        '
        Me.mnuEdit.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuEditUndo, Me.mnuEditRedo, Me.ToolStripMenuItem3, Me.mnuEditCut, Me.mnuEditCopy, Me.mnuEditPaste, Me.mnuEditDelete, Me.mnuEditSelectAll, Me.SelectedStagesToolStripMenuItem, Me.AllStagesToolStripMenuItem, Me.ToolStripSeparator12, Me.mnuEditProperties, Me.mnuEditDependencies, Me.mnuEditAdvancedSearch, Me.ToolStripSeparator9, Me.mnuEditAddPage})
        Me.mnuEdit.Name = "mnuEdit"
        resources.ApplyResources(Me.mnuEdit, "mnuEdit")
        '
        'mnuEditUndo
        '
        resources.ApplyResources(Me.mnuEditUndo, "mnuEditUndo")
        Me.mnuEditUndo.Name = "mnuEditUndo"
        '
        'mnuEditRedo
        '
        resources.ApplyResources(Me.mnuEditRedo, "mnuEditRedo")
        Me.mnuEditRedo.Name = "mnuEditRedo"
        '
        'ToolStripMenuItem3
        '
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        resources.ApplyResources(Me.ToolStripMenuItem3, "ToolStripMenuItem3")
        '
        'mnuEditCut
        '
        resources.ApplyResources(Me.mnuEditCut, "mnuEditCut")
        Me.mnuEditCut.Name = "mnuEditCut"
        '
        'mnuEditCopy
        '
        resources.ApplyResources(Me.mnuEditCopy, "mnuEditCopy")
        Me.mnuEditCopy.Name = "mnuEditCopy"
        '
        'mnuEditPaste
        '
        resources.ApplyResources(Me.mnuEditPaste, "mnuEditPaste")
        Me.mnuEditPaste.Name = "mnuEditPaste"
        '
        'mnuEditDelete
        '
        resources.ApplyResources(Me.mnuEditDelete, "mnuEditDelete")
        Me.mnuEditDelete.Name = "mnuEditDelete"
        '
        'mnuEditSelectAll
        '
        Me.mnuEditSelectAll.Name = "mnuEditSelectAll"
        resources.ApplyResources(Me.mnuEditSelectAll, "mnuEditSelectAll")
        '
        'SelectedStagesToolStripMenuItem
        '
        Me.SelectedStagesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.EnableLoggingSelected, Me.DisableLoggingSelected, Me.EnableErrorLoggingSelected})
        Me.SelectedStagesToolStripMenuItem.Name = "SelectedStagesToolStripMenuItem"
        resources.ApplyResources(Me.SelectedStagesToolStripMenuItem, "SelectedStagesToolStripMenuItem")
        '
        'EnableLoggingSelected
        '
        Me.EnableLoggingSelected.Name = "EnableLoggingSelected"
        resources.ApplyResources(Me.EnableLoggingSelected, "EnableLoggingSelected")
        '
        'DisableLoggingSelected
        '
        Me.DisableLoggingSelected.Name = "DisableLoggingSelected"
        resources.ApplyResources(Me.DisableLoggingSelected, "DisableLoggingSelected")
        '
        'EnableErrorLoggingSelected
        '
        Me.EnableErrorLoggingSelected.Name = "EnableErrorLoggingSelected"
        resources.ApplyResources(Me.EnableErrorLoggingSelected, "EnableErrorLoggingSelected")
        '
        'AllStagesToolStripMenuItem
        '
        Me.AllStagesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.EnableLoggingAll, Me.DisableLoggingAll, Me.EnableErrorLoggingAll})
        Me.AllStagesToolStripMenuItem.Name = "AllStagesToolStripMenuItem"
        resources.ApplyResources(Me.AllStagesToolStripMenuItem, "AllStagesToolStripMenuItem")
        '
        'EnableLoggingAll
        '
        Me.EnableLoggingAll.Name = "EnableLoggingAll"
        resources.ApplyResources(Me.EnableLoggingAll, "EnableLoggingAll")
        '
        'DisableLoggingAll
        '
        Me.DisableLoggingAll.Name = "DisableLoggingAll"
        resources.ApplyResources(Me.DisableLoggingAll, "DisableLoggingAll")
        '
        'EnableErrorLoggingAll
        '
        Me.EnableErrorLoggingAll.Name = "EnableErrorLoggingAll"
        resources.ApplyResources(Me.EnableErrorLoggingAll, "EnableErrorLoggingAll")
        '
        'ToolStripSeparator12
        '
        Me.ToolStripSeparator12.Name = "ToolStripSeparator12"
        resources.ApplyResources(Me.ToolStripSeparator12, "ToolStripSeparator12")
        '
        'mnuEditProperties
        '
        resources.ApplyResources(Me.mnuEditProperties, "mnuEditProperties")
        Me.mnuEditProperties.Name = "mnuEditProperties"
        '
        'mnuEditDependencies
        '
        Me.mnuEditDependencies.Name = "mnuEditDependencies"
        resources.ApplyResources(Me.mnuEditDependencies, "mnuEditDependencies")
        '
        'mnuEditAdvancedSearch
        '
        resources.ApplyResources(Me.mnuEditAdvancedSearch, "mnuEditAdvancedSearch")
        Me.mnuEditAdvancedSearch.Name = "mnuEditAdvancedSearch"
        '
        'ToolStripSeparator9
        '
        Me.ToolStripSeparator9.Name = "ToolStripSeparator9"
        resources.ApplyResources(Me.ToolStripSeparator9, "ToolStripSeparator9")
        '
        'mnuEditAddPage
        '
        resources.ApplyResources(Me.mnuEditAddPage, "mnuEditAddPage")
        Me.mnuEditAddPage.Name = "mnuEditAddPage"
        '
        'ViewToolStripMenuItem
        '
        Me.ViewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZoomToolStripMenuItem, Me.ToolStripSeparator3, Me.mnuTransparency, Me.AlwaysOnTopToolStripMenuItem, Me.mnuFullScreen, Me.ToolStripMenuItem1, Me.chkSnap, Me.chkGrid, Me.ToolStripMenuItem13, Me.mnuDynCursor, Me.mnuToolBoxes})
        Me.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem"
        resources.ApplyResources(Me.ViewToolStripMenuItem, "ViewToolStripMenuItem")
        '
        'ZoomToolStripMenuItem
        '
        Me.ZoomToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.z400, Me.z200, Me.z150, Me.z100, Me.z75, Me.z50, Me.z25, Me.zSep, Me.zDyn})
        resources.ApplyResources(Me.ZoomToolStripMenuItem, "ZoomToolStripMenuItem")
        Me.ZoomToolStripMenuItem.Name = "ZoomToolStripMenuItem"
        '
        'z400
        '
        Me.z400.Name = "z400"
        resources.ApplyResources(Me.z400, "z400")
        Me.z400.Tag = "400"
        '
        'z200
        '
        Me.z200.Name = "z200"
        resources.ApplyResources(Me.z200, "z200")
        Me.z200.Tag = "200"
        '
        'z150
        '
        Me.z150.Name = "z150"
        resources.ApplyResources(Me.z150, "z150")
        Me.z150.Tag = "150"
        '
        'z100
        '
        Me.z100.Name = "z100"
        resources.ApplyResources(Me.z100, "z100")
        Me.z100.Tag = "100"
        '
        'z75
        '
        Me.z75.Name = "z75"
        resources.ApplyResources(Me.z75, "z75")
        Me.z75.Tag = "75"
        '
        'z50
        '
        Me.z50.Name = "z50"
        resources.ApplyResources(Me.z50, "z50")
        Me.z50.Tag = "50"
        '
        'z25
        '
        Me.z25.Name = "z25"
        resources.ApplyResources(Me.z25, "z25")
        Me.z25.Tag = "25"
        '
        'zSep
        '
        Me.zSep.Name = "zSep"
        resources.ApplyResources(Me.zSep, "zSep")
        '
        'zDyn
        '
        Me.zDyn.Name = "zDyn"
        resources.ApplyResources(Me.zDyn, "zDyn")
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        resources.ApplyResources(Me.ToolStripSeparator3, "ToolStripSeparator3")
        '
        'mnuTransparency
        '
        Me.mnuTransparency.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuTransparencyNone, Me.mnuTransparencyLow, Me.mnuTransparencyMedium, Me.mnuTransparencyHigh, Me.mnuTransparencyVeryHigh})
        Me.mnuTransparency.Name = "mnuTransparency"
        resources.ApplyResources(Me.mnuTransparency, "mnuTransparency")
        '
        'mnuTransparencyNone
        '
        Me.mnuTransparencyNone.Name = "mnuTransparencyNone"
        resources.ApplyResources(Me.mnuTransparencyNone, "mnuTransparencyNone")
        Me.mnuTransparencyNone.Tag = "1.0"
        '
        'mnuTransparencyLow
        '
        Me.mnuTransparencyLow.Name = "mnuTransparencyLow"
        resources.ApplyResources(Me.mnuTransparencyLow, "mnuTransparencyLow")
        Me.mnuTransparencyLow.Tag = "0.8"
        '
        'mnuTransparencyMedium
        '
        Me.mnuTransparencyMedium.Name = "mnuTransparencyMedium"
        resources.ApplyResources(Me.mnuTransparencyMedium, "mnuTransparencyMedium")
        Me.mnuTransparencyMedium.Tag = "0.6"
        '
        'mnuTransparencyHigh
        '
        Me.mnuTransparencyHigh.Name = "mnuTransparencyHigh"
        resources.ApplyResources(Me.mnuTransparencyHigh, "mnuTransparencyHigh")
        Me.mnuTransparencyHigh.Tag = "0.4"
        '
        'mnuTransparencyVeryHigh
        '
        Me.mnuTransparencyVeryHigh.Name = "mnuTransparencyVeryHigh"
        resources.ApplyResources(Me.mnuTransparencyVeryHigh, "mnuTransparencyVeryHigh")
        Me.mnuTransparencyVeryHigh.Tag = "0.2"
        '
        'AlwaysOnTopToolStripMenuItem
        '
        Me.AlwaysOnTopToolStripMenuItem.Name = "AlwaysOnTopToolStripMenuItem"
        resources.ApplyResources(Me.AlwaysOnTopToolStripMenuItem, "AlwaysOnTopToolStripMenuItem")
        '
        'mnuFullScreen
        '
        resources.ApplyResources(Me.mnuFullScreen, "mnuFullScreen")
        Me.mnuFullScreen.Name = "mnuFullScreen"
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'chkSnap
        '
        Me.chkSnap.Checked = True
        Me.chkSnap.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSnap.Name = "chkSnap"
        resources.ApplyResources(Me.chkSnap, "chkSnap")
        '
        'chkGrid
        '
        Me.chkGrid.Checked = True
        Me.chkGrid.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkGrid.Name = "chkGrid"
        resources.ApplyResources(Me.chkGrid, "chkGrid")
        '
        'ToolStripMenuItem13
        '
        Me.ToolStripMenuItem13.Name = "ToolStripMenuItem13"
        resources.ApplyResources(Me.ToolStripMenuItem13, "ToolStripMenuItem13")
        '
        'mnuDynCursor
        '
        Me.mnuDynCursor.Checked = True
        Me.mnuDynCursor.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuDynCursor.Name = "mnuDynCursor"
        resources.ApplyResources(Me.mnuDynCursor, "mnuDynCursor")
        '
        'mnuToolBoxes
        '
        Me.mnuToolBoxes.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuToolsSavePositions, Me.mnuToolsLockPositions, Me.mnuShowAllTools, Me.mnuHideAllTools, Me.ToolStripSeparator5, Me.mnuToolsStandard, Me.mnuToolsTools, Me.mnuToolsDebug, Me.mnuToolsSearch, Me.mnuToolsFont, Me.SkillToolbarVisiblityToolStripMenuItem, Me.ToolStripSeparator8, Me.mnuToolsProcessMI, Me.mnuToolsDataItemWatch})
        Me.mnuToolBoxes.Name = "mnuToolBoxes"
        resources.ApplyResources(Me.mnuToolBoxes, "mnuToolBoxes")
        '
        'mnuToolsSavePositions
        '
        Me.mnuToolsSavePositions.Name = "mnuToolsSavePositions"
        resources.ApplyResources(Me.mnuToolsSavePositions, "mnuToolsSavePositions")
        '
        'mnuToolsLockPositions
        '
        Me.mnuToolsLockPositions.Name = "mnuToolsLockPositions"
        resources.ApplyResources(Me.mnuToolsLockPositions, "mnuToolsLockPositions")
        '
        'mnuShowAllTools
        '
        Me.mnuShowAllTools.Name = "mnuShowAllTools"
        resources.ApplyResources(Me.mnuShowAllTools, "mnuShowAllTools")
        '
        'mnuHideAllTools
        '
        Me.mnuHideAllTools.Name = "mnuHideAllTools"
        resources.ApplyResources(Me.mnuHideAllTools, "mnuHideAllTools")
        '
        'ToolStripSeparator5
        '
        Me.ToolStripSeparator5.Name = "ToolStripSeparator5"
        resources.ApplyResources(Me.ToolStripSeparator5, "ToolStripSeparator5")
        '
        'mnuToolsStandard
        '
        Me.mnuToolsStandard.Checked = True
        Me.mnuToolsStandard.CheckOnClick = True
        Me.mnuToolsStandard.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuToolsStandard.Name = "mnuToolsStandard"
        resources.ApplyResources(Me.mnuToolsStandard, "mnuToolsStandard")
        '
        'mnuToolsTools
        '
        Me.mnuToolsTools.Checked = True
        Me.mnuToolsTools.CheckOnClick = True
        Me.mnuToolsTools.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuToolsTools.Name = "mnuToolsTools"
        resources.ApplyResources(Me.mnuToolsTools, "mnuToolsTools")
        '
        'mnuToolsDebug
        '
        Me.mnuToolsDebug.Checked = True
        Me.mnuToolsDebug.CheckOnClick = True
        Me.mnuToolsDebug.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuToolsDebug.Name = "mnuToolsDebug"
        resources.ApplyResources(Me.mnuToolsDebug, "mnuToolsDebug")
        '
        'mnuToolsSearch
        '
        Me.mnuToolsSearch.Checked = True
        Me.mnuToolsSearch.CheckOnClick = True
        Me.mnuToolsSearch.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuToolsSearch.Name = "mnuToolsSearch"
        resources.ApplyResources(Me.mnuToolsSearch, "mnuToolsSearch")
        '
        'mnuToolsFont
        '
        Me.mnuToolsFont.Checked = True
        Me.mnuToolsFont.CheckOnClick = True
        Me.mnuToolsFont.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuToolsFont.Name = "mnuToolsFont"
        resources.ApplyResources(Me.mnuToolsFont, "mnuToolsFont")
        '
        'SkillToolbarVisiblityToolStripMenuItem
        '
        Me.SkillToolbarVisiblityToolStripMenuItem.Checked = True
        Me.SkillToolbarVisiblityToolStripMenuItem.CheckOnClick = True
        Me.SkillToolbarVisiblityToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.SkillToolbarVisiblityToolStripMenuItem.Name = "SkillToolbarVisiblityToolStripMenuItem"
        resources.ApplyResources(Me.SkillToolbarVisiblityToolStripMenuItem, "SkillToolbarVisiblityToolStripMenuItem")
        '
        'ToolStripSeparator8
        '
        Me.ToolStripSeparator8.Name = "ToolStripSeparator8"
        resources.ApplyResources(Me.ToolStripSeparator8, "ToolStripSeparator8")
        '
        'mnuToolsProcessMI
        '
        resources.ApplyResources(Me.mnuToolsProcessMI, "mnuToolsProcessMI")
        Me.mnuToolsProcessMI.Name = "mnuToolsProcessMI"
        '
        'mnuToolsDataItemWatch
        '
        resources.ApplyResources(Me.mnuToolsDataItemWatch, "mnuToolsDataItemWatch")
        Me.mnuToolsDataItemWatch.Name = "mnuToolsDataItemWatch"
        '
        'mnuTools
        '
        Me.mnuTools.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuPointer, Me.mnuLink, Me.mnuBlock, Me.mnuRead, Me.mnuWrite, Me.mnuNavigate, Me.mnuCode, Me.mnuWait, Me.mnuProcess, Me.mnuPage, Me.mnuAction, Me.mnuDecision, Me.mnuChoice, Me.mnuCalculation, Me.mnuMultipleCalculation, Me.mnuDataItem, Me.mnuCollection, Me.mnuLoop, Me.mnuNote, Me.mnuEnd, Me.mnuAnchor, Me.mnuAlert, Me.mnuException, Me.mnuRecover, Me.mnuResume, Me.ToolStripMenuItem9, Me.mnuApplicationModeller})
        Me.mnuTools.Name = "mnuTools"
        resources.ApplyResources(Me.mnuTools, "mnuTools")
        '
        'mnuPointer
        '
        Me.mnuPointer.Checked = True
        Me.mnuPointer.CheckState = System.Windows.Forms.CheckState.Checked
        resources.ApplyResources(Me.mnuPointer, "mnuPointer")
        Me.mnuPointer.Name = "mnuPointer"
        '
        'mnuLink
        '
        resources.ApplyResources(Me.mnuLink, "mnuLink")
        Me.mnuLink.Name = "mnuLink"
        '
        'mnuBlock
        '
        resources.ApplyResources(Me.mnuBlock, "mnuBlock")
        Me.mnuBlock.Name = "mnuBlock"
        '
        'mnuRead
        '
        resources.ApplyResources(Me.mnuRead, "mnuRead")
        Me.mnuRead.Name = "mnuRead"
        '
        'mnuWrite
        '
        resources.ApplyResources(Me.mnuWrite, "mnuWrite")
        Me.mnuWrite.Name = "mnuWrite"
        '
        'mnuNavigate
        '
        resources.ApplyResources(Me.mnuNavigate, "mnuNavigate")
        Me.mnuNavigate.Name = "mnuNavigate"
        '
        'mnuCode
        '
        resources.ApplyResources(Me.mnuCode, "mnuCode")
        Me.mnuCode.Name = "mnuCode"
        '
        'mnuWait
        '
        resources.ApplyResources(Me.mnuWait, "mnuWait")
        Me.mnuWait.Name = "mnuWait"
        '
        'mnuProcess
        '
        resources.ApplyResources(Me.mnuProcess, "mnuProcess")
        Me.mnuProcess.Name = "mnuProcess"
        '
        'mnuPage
        '
        resources.ApplyResources(Me.mnuPage, "mnuPage")
        Me.mnuPage.Name = "mnuPage"
        '
        'mnuAction
        '
        resources.ApplyResources(Me.mnuAction, "mnuAction")
        Me.mnuAction.Name = "mnuAction"
        '
        'mnuDecision
        '
        resources.ApplyResources(Me.mnuDecision, "mnuDecision")
        Me.mnuDecision.Name = "mnuDecision"
        '
        'mnuChoice
        '
        resources.ApplyResources(Me.mnuChoice, "mnuChoice")
        Me.mnuChoice.Name = "mnuChoice"
        '
        'mnuCalculation
        '
        resources.ApplyResources(Me.mnuCalculation, "mnuCalculation")
        Me.mnuCalculation.Name = "mnuCalculation"
        '
        'mnuMultipleCalculation
        '
        resources.ApplyResources(Me.mnuMultipleCalculation, "mnuMultipleCalculation")
        Me.mnuMultipleCalculation.Name = "mnuMultipleCalculation"
        '
        'mnuDataItem
        '
        resources.ApplyResources(Me.mnuDataItem, "mnuDataItem")
        Me.mnuDataItem.Name = "mnuDataItem"
        '
        'mnuCollection
        '
        resources.ApplyResources(Me.mnuCollection, "mnuCollection")
        Me.mnuCollection.Name = "mnuCollection"
        '
        'mnuLoop
        '
        resources.ApplyResources(Me.mnuLoop, "mnuLoop")
        Me.mnuLoop.Name = "mnuLoop"
        '
        'mnuNote
        '
        resources.ApplyResources(Me.mnuNote, "mnuNote")
        Me.mnuNote.Name = "mnuNote"
        '
        'mnuEnd
        '
        resources.ApplyResources(Me.mnuEnd, "mnuEnd")
        Me.mnuEnd.Name = "mnuEnd"
        '
        'mnuAnchor
        '
        resources.ApplyResources(Me.mnuAnchor, "mnuAnchor")
        Me.mnuAnchor.Name = "mnuAnchor"
        '
        'mnuAlert
        '
        resources.ApplyResources(Me.mnuAlert, "mnuAlert")
        Me.mnuAlert.Name = "mnuAlert"
        '
        'mnuException
        '
        resources.ApplyResources(Me.mnuException, "mnuException")
        Me.mnuException.Name = "mnuException"
        '
        'mnuRecover
        '
        resources.ApplyResources(Me.mnuRecover, "mnuRecover")
        Me.mnuRecover.Name = "mnuRecover"
        '
        'mnuResume
        '
        resources.ApplyResources(Me.mnuResume, "mnuResume")
        Me.mnuResume.Name = "mnuResume"
        '
        'ToolStripMenuItem9
        '
        Me.ToolStripMenuItem9.Name = "ToolStripMenuItem9"
        resources.ApplyResources(Me.ToolStripMenuItem9, "ToolStripMenuItem9")
        '
        'mnuApplicationModeller
        '
        resources.ApplyResources(Me.mnuApplicationModeller, "mnuApplicationModeller")
        Me.mnuApplicationModeller.Name = "mnuApplicationModeller"
        '
        'mnuDebug
        '
        Me.mnuDebug.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuDebugGo, Me.mnuDebugStep, Me.mnuDebugStepOut, Me.mnuDebugStepOver, Me.mnuDebugStop, Me.mnuDebugRestart, Me.ToolStripMenuItem4, Me.mnuDebugValidate, Me.mnuStartParams, Me.mnuCalculationZoom, Me.mnuClearAllBreakpoints, Me.mnuFocusDebugStage, Me.mnuDebugExceptions})
        Me.mnuDebug.Name = "mnuDebug"
        resources.ApplyResources(Me.mnuDebug, "mnuDebug")
        '
        'mnuDebugGo
        '
        resources.ApplyResources(Me.mnuDebugGo, "mnuDebugGo")
        Me.mnuDebugGo.Name = "mnuDebugGo"
        '
        'mnuDebugStep
        '
        resources.ApplyResources(Me.mnuDebugStep, "mnuDebugStep")
        Me.mnuDebugStep.Name = "mnuDebugStep"
        '
        'mnuDebugStepOut
        '
        resources.ApplyResources(Me.mnuDebugStepOut, "mnuDebugStepOut")
        Me.mnuDebugStepOut.Name = "mnuDebugStepOut"
        '
        'mnuDebugStepOver
        '
        resources.ApplyResources(Me.mnuDebugStepOver, "mnuDebugStepOver")
        Me.mnuDebugStepOver.Name = "mnuDebugStepOver"
        '
        'mnuDebugStop
        '
        resources.ApplyResources(Me.mnuDebugStop, "mnuDebugStop")
        Me.mnuDebugStop.Name = "mnuDebugStop"
        '
        'mnuDebugRestart
        '
        resources.ApplyResources(Me.mnuDebugRestart, "mnuDebugRestart")
        Me.mnuDebugRestart.Name = "mnuDebugRestart"
        '
        'ToolStripMenuItem4
        '
        Me.ToolStripMenuItem4.Name = "ToolStripMenuItem4"
        resources.ApplyResources(Me.ToolStripMenuItem4, "ToolStripMenuItem4")
        '
        'mnuDebugValidate
        '
        resources.ApplyResources(Me.mnuDebugValidate, "mnuDebugValidate")
        Me.mnuDebugValidate.Name = "mnuDebugValidate"
        '
        'mnuStartParams
        '
        Me.mnuStartParams.Name = "mnuStartParams"
        resources.ApplyResources(Me.mnuStartParams, "mnuStartParams")
        '
        'mnuCalculationZoom
        '
        Me.mnuCalculationZoom.Name = "mnuCalculationZoom"
        resources.ApplyResources(Me.mnuCalculationZoom, "mnuCalculationZoom")
        '
        'mnuClearAllBreakpoints
        '
        Me.mnuClearAllBreakpoints.Name = "mnuClearAllBreakpoints"
        resources.ApplyResources(Me.mnuClearAllBreakpoints, "mnuClearAllBreakpoints")
        '
        'mnuFocusDebugStage
        '
        resources.ApplyResources(Me.mnuFocusDebugStage, "mnuFocusDebugStage")
        Me.mnuFocusDebugStage.Name = "mnuFocusDebugStage"
        '
        'mnuDebugExceptions
        '
        Me.mnuDebugExceptions.CheckOnClick = True
        Me.mnuDebugExceptions.Name = "mnuDebugExceptions"
        resources.ApplyResources(Me.mnuDebugExceptions, "mnuDebugExceptions")
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuTopic, Me.ToolStripMenuItem6, Me.mnuOpenHelp, Me.mnuSearch, Me.ToolStripMenuItem7, Me.mnuAPIHelp, Me.mnuRequest, Me.ToolStripMenuItem8, Me.mnuAbout})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        resources.ApplyResources(Me.HelpToolStripMenuItem, "HelpToolStripMenuItem")
        '
        'mnuTopic
        '
        Me.mnuTopic.Name = "mnuTopic"
        resources.ApplyResources(Me.mnuTopic, "mnuTopic")
        '
        'ToolStripMenuItem6
        '
        Me.ToolStripMenuItem6.Name = "ToolStripMenuItem6"
        resources.ApplyResources(Me.ToolStripMenuItem6, "ToolStripMenuItem6")
        '
        'mnuOpenHelp
        '
        Me.mnuOpenHelp.Name = "mnuOpenHelp"
        resources.ApplyResources(Me.mnuOpenHelp, "mnuOpenHelp")
        '
        'mnuSearch
        '
        Me.mnuSearch.Name = "mnuSearch"
        resources.ApplyResources(Me.mnuSearch, "mnuSearch")
        '
        'ToolStripMenuItem7
        '
        Me.ToolStripMenuItem7.Name = "ToolStripMenuItem7"
        resources.ApplyResources(Me.ToolStripMenuItem7, "ToolStripMenuItem7")
        '
        'mnuAPIHelp
        '
        Me.mnuAPIHelp.Name = "mnuAPIHelp"
        resources.ApplyResources(Me.mnuAPIHelp, "mnuAPIHelp")
        '
        'mnuRequest
        '
        Me.mnuRequest.Name = "mnuRequest"
        resources.ApplyResources(Me.mnuRequest, "mnuRequest")
        '
        'ToolStripMenuItem8
        '
        Me.ToolStripMenuItem8.Name = "ToolStripMenuItem8"
        resources.ApplyResources(Me.ToolStripMenuItem8, "ToolStripMenuItem8")
        '
        'mnuAbout
        '
        Me.mnuAbout.Name = "mnuAbout"
        resources.ApplyResources(Me.mnuAbout, "mnuAbout")
        '
        'toolstripStandard
        '
        Me.toolstripStandard.CanOverflow = False
        resources.ApplyResources(Me.toolstripStandard, "toolstripStandard")
        Me.toolstripStandard.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnSave, Me.btnPrint, Me.ToolStripSeparator2, Me.btnCut, Me.btnCopy, Me.btnPaste, Me.btnUndo, Me.btnRedo, Me.btnRefresh, Me.ToolStripLabel1, Me.mZoomToolstripCombo, Me.mZoomInToolstripButton, Me.mZoomOutToolstripButton})
        Me.toolstripStandard.Name = "toolstripStandard"
        '
        'btnSave
        '
        Me.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnSave, "btnSave")
        Me.btnSave.Name = "btnSave"
        '
        'btnPrint
        '
        Me.btnPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnPrint, "btnPrint")
        Me.btnPrint.Name = "btnPrint"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        '
        'btnCut
        '
        Me.btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnCut, "btnCut")
        Me.btnCut.Name = "btnCut"
        '
        'btnCopy
        '
        Me.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnCopy, "btnCopy")
        Me.btnCopy.Name = "btnCopy"
        '
        'btnPaste
        '
        Me.btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnPaste, "btnPaste")
        Me.btnPaste.Name = "btnPaste"
        '
        'btnUndo
        '
        Me.btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnUndo, "btnUndo")
        Me.btnUndo.Name = "btnUndo"
        '
        'btnRedo
        '
        Me.btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnRedo, "btnRedo")
        Me.btnRedo.Name = "btnRedo"
        '
        'btnRefresh
        '
        Me.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnRefresh, "btnRefresh")
        Me.btnRefresh.Name = "btnRefresh"
        '
        'ToolStripLabel1
        '
        Me.ToolStripLabel1.Name = "ToolStripLabel1"
        resources.ApplyResources(Me.ToolStripLabel1, "ToolStripLabel1")
        '
        'mZoomToolstripCombo
        '
        resources.ApplyResources(Me.mZoomToolstripCombo, "mZoomToolstripCombo")
        Me.mZoomToolstripCombo.DropDownWidth = 100
        Me.mZoomToolstripCombo.Items.AddRange(New Object() {resources.GetString("mZoomToolstripCombo.Items"), resources.GetString("mZoomToolstripCombo.Items1"), resources.GetString("mZoomToolstripCombo.Items2"), resources.GetString("mZoomToolstripCombo.Items3"), resources.GetString("mZoomToolstripCombo.Items4"), resources.GetString("mZoomToolstripCombo.Items5"), resources.GetString("mZoomToolstripCombo.Items6"), resources.GetString("mZoomToolstripCombo.Items7")})
        Me.mZoomToolstripCombo.Name = "mZoomToolstripCombo"
        '
        'mZoomInToolstripButton
        '
        Me.mZoomInToolstripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.mZoomInToolstripButton, "mZoomInToolstripButton")
        Me.mZoomInToolstripButton.Name = "mZoomInToolstripButton"
        '
        'mZoomOutToolstripButton
        '
        Me.mZoomOutToolstripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.mZoomOutToolstripButton, "mZoomOutToolstripButton")
        Me.mZoomOutToolstripButton.Name = "mZoomOutToolstripButton"
        '
        'toolstripDebug
        '
        resources.ApplyResources(Me.toolstripDebug, "toolstripDebug")
        Me.toolstripDebug.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnMenuDebugGo, Me.btnMenuDebugPause, Me.btnMenuDebugStep, Me.btnMenuDebugStepOver, Me.btnMenuDebugStepOut, Me.btnMenuDebugReset, Me.ToolStripSeparator10, Me.btnValidate, Me.ToolStripSeparator11, Me.btnMenuDebugLaunchApp, Me.btnMenuApplicationModeller, Me.ToolStripSeparator1, Me.btnBreakpoint, Me.btnWatch, Me.btnProcessMI, Me.btnMenuFullScreen, Me.btnPanViewDropDown})
        Me.toolstripDebug.Name = "toolstripDebug"
        '
        'btnMenuDebugGo
        '
        Me.btnMenuDebugGo.BackColor = System.Drawing.SystemColors.Menu
        Me.btnMenuDebugGo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuDebugGo, "btnMenuDebugGo")
        Me.btnMenuDebugGo.Name = "btnMenuDebugGo"
        '
        'btnMenuDebugPause
        '
        Me.btnMenuDebugPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuDebugPause, "btnMenuDebugPause")
        Me.btnMenuDebugPause.Name = "btnMenuDebugPause"
        '
        'btnMenuDebugStep
        '
        Me.btnMenuDebugStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuDebugStep, "btnMenuDebugStep")
        Me.btnMenuDebugStep.Name = "btnMenuDebugStep"
        '
        'btnMenuDebugStepOver
        '
        Me.btnMenuDebugStepOver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuDebugStepOver, "btnMenuDebugStepOver")
        Me.btnMenuDebugStepOver.Name = "btnMenuDebugStepOver"
        '
        'btnMenuDebugStepOut
        '
        Me.btnMenuDebugStepOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuDebugStepOut, "btnMenuDebugStepOut")
        Me.btnMenuDebugStepOut.Name = "btnMenuDebugStepOut"
        '
        'btnMenuDebugReset
        '
        Me.btnMenuDebugReset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuDebugReset, "btnMenuDebugReset")
        Me.btnMenuDebugReset.Name = "btnMenuDebugReset"
        '
        'ToolStripSeparator10
        '
        Me.ToolStripSeparator10.Name = "ToolStripSeparator10"
        resources.ApplyResources(Me.ToolStripSeparator10, "ToolStripSeparator10")
        '
        'btnValidate
        '
        resources.ApplyResources(Me.btnValidate, "btnValidate")
        Me.btnValidate.Name = "btnValidate"
        '
        'ToolStripSeparator11
        '
        Me.ToolStripSeparator11.Name = "ToolStripSeparator11"
        resources.ApplyResources(Me.ToolStripSeparator11, "ToolStripSeparator11")
        '
        'btnMenuDebugLaunchApp
        '
        resources.ApplyResources(Me.btnMenuDebugLaunchApp, "btnMenuDebugLaunchApp")
        Me.btnMenuDebugLaunchApp.Name = "btnMenuDebugLaunchApp"
        '
        'btnMenuApplicationModeller
        '
        resources.ApplyResources(Me.btnMenuApplicationModeller, "btnMenuApplicationModeller")
        Me.btnMenuApplicationModeller.Name = "btnMenuApplicationModeller"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'btnBreakpoint
        '
        Me.btnBreakpoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnBreakpoint, "btnBreakpoint")
        Me.btnBreakpoint.Name = "btnBreakpoint"
        '
        'btnWatch
        '
        Me.btnWatch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnWatch, "btnWatch")
        Me.btnWatch.Name = "btnWatch"
        '
        'btnProcessMI
        '
        Me.btnProcessMI.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnProcessMI, "btnProcessMI")
        Me.btnProcessMI.Name = "btnProcessMI"
        '
        'btnMenuFullScreen
        '
        Me.btnMenuFullScreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnMenuFullScreen, "btnMenuFullScreen")
        Me.btnMenuFullScreen.Name = "btnMenuFullScreen"
        '
        'btnPanViewDropDown
        '
        Me.btnPanViewDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnPanViewDropDown, "btnPanViewDropDown")
        Me.btnPanViewDropDown.Name = "btnPanViewDropDown"
        '
        'toolstripSearch
        '
        resources.ApplyResources(Me.toolstripSearch, "toolstripSearch")
        Me.toolstripSearch.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow
        Me.toolstripSearch.ModeIsObjectStudio = False
        Me.toolstripSearch.Name = "toolstripSearch"
        '
        'ntfyDebug
        '
        Me.ntfyDebug.ContextMenu = Me.ntfyContext
        resources.ApplyResources(Me.ntfyDebug, "ntfyDebug")
        '
        'ntfyContext
        '
        Me.ntfyContext.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuTaskTrayGo, Me.mnuTaskTrayStep, Me.mnuTaskTrayStepOver, Me.mnuTaskTrayStepOut, Me.mnuTaskTrayRestart, Me.mnuTaskTrayStop})
        '
        'mnuTaskTrayGo
        '
        Me.mnuTaskTrayGo.Index = 0
        resources.ApplyResources(Me.mnuTaskTrayGo, "mnuTaskTrayGo")
        '
        'mnuTaskTrayStep
        '
        Me.mnuTaskTrayStep.Index = 1
        resources.ApplyResources(Me.mnuTaskTrayStep, "mnuTaskTrayStep")
        '
        'mnuTaskTrayStepOver
        '
        Me.mnuTaskTrayStepOver.Index = 2
        resources.ApplyResources(Me.mnuTaskTrayStepOver, "mnuTaskTrayStepOver")
        '
        'mnuTaskTrayStepOut
        '
        Me.mnuTaskTrayStepOut.Index = 3
        resources.ApplyResources(Me.mnuTaskTrayStepOut, "mnuTaskTrayStepOut")
        '
        'mnuTaskTrayRestart
        '
        Me.mnuTaskTrayRestart.Index = 4
        resources.ApplyResources(Me.mnuTaskTrayRestart, "mnuTaskTrayRestart")
        '
        'mnuTaskTrayStop
        '
        Me.mnuTaskTrayStop.Index = 5
        resources.ApplyResources(Me.mnuTaskTrayStop, "mnuTaskTrayStop")
        '
        'TrackBar1
        '
        resources.ApplyResources(Me.TrackBar1, "TrackBar1")
        Me.TrackBar1.Maximum = 2000
        Me.TrackBar1.Name = "TrackBar1"
        Me.TrackBar1.SmallChange = 200
        Me.TrackBar1.TickFrequency = 200
        Me.ttTips.SetToolTip(Me.TrackBar1, resources.GetString("TrackBar1.ToolTip"))
        '
        'timStatusBarTimer
        '
        '
        'lblFast
        '
        resources.ApplyResources(Me.lblFast, "lblFast")
        Me.lblFast.Name = "lblFast"
        '
        'lblSlow
        '
        resources.ApplyResources(Me.lblSlow, "lblSlow")
        Me.lblSlow.Name = "lblSlow"
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'TrackBar2
        '
        Me.TrackBar2.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.TrackBar2, "TrackBar2")
        Me.TrackBar2.Maximum = 2000
        Me.TrackBar2.Name = "TrackBar2"
        Me.TrackBar2.SmallChange = 200
        Me.TrackBar2.TickFrequency = 200
        '
        'lblShowDetails
        '
        Me.lblShowDetails.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.lblShowDetails, "lblShowDetails")
        Me.lblShowDetails.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.lblShowDetails.Name = "lblShowDetails"
        '
        'Label3
        '
        Me.Label3.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.Label3.Name = "Label3"
        '
        'mnuUndockAll
        '
        Me.mnuUndockAll.Name = "mnuUndockAll"
        resources.ApplyResources(Me.mnuUndockAll, "mnuUndockAll")
        '
        'frmProcess
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.toolstripCont)
        Me.KeyPreview = True
        Me.MainMenuStrip = Me.mnuMain
        Me.Name = "frmProcess"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.toolstripCont.BottomToolStripPanel.ResumeLayout(False)
        Me.toolstripCont.BottomToolStripPanel.PerformLayout()
        Me.toolstripCont.ContentPanel.ResumeLayout(False)
        Me.toolstripCont.ContentPanel.PerformLayout()
        Me.toolstripCont.LeftToolStripPanel.ResumeLayout(False)
        Me.toolstripCont.LeftToolStripPanel.PerformLayout()
        Me.toolstripCont.TopToolStripPanel.ResumeLayout(False)
        Me.toolstripCont.TopToolStripPanel.PerformLayout()
        Me.toolstripCont.ResumeLayout(False)
        Me.toolstripCont.PerformLayout()
        Me.stsBar.ResumeLayout(False)
        Me.stsBar.PerformLayout()
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlExpressionRow.ResumeLayout(False)
        Me.pnlExpressionRow.PerformLayout()
        Me.toolstripTools.ResumeLayout(False)
        Me.toolstripTools.PerformLayout()
        Me.toolstripFont.ResumeLayout(False)
        Me.toolstripFont.PerformLayout()
        Me.mnuMain.ResumeLayout(False)
        Me.mnuMain.PerformLayout()
        Me.toolstripStandard.ResumeLayout(False)
        Me.toolstripStandard.PerformLayout()
        Me.toolstripDebug.ResumeLayout(False)
        Me.toolstripDebug.PerformLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TrackBar2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents mnuMain As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuExport As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuChkUseSummaries As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuFileSave As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuFileClose As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackupNever As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup2 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup3 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup4 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup5 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup10 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup15 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup20 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup25 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBackup30 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSaveAs As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZoomToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z400 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z200 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z100 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z150 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z25 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z50 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z75 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents zSep As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents zDyn As System.Windows.Forms.ToolStripTextBox
    Friend WithEvents mnuDynCursor As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuFullScreen As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents chkSnap As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents chkGrid As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsSavePositions As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolBoxes As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsTools As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsSearch As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsStandard As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuUndockAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsDebug As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsProcessMI As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEdit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditCut As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditCopy As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditPaste As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditDelete As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditProperties As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditUndo As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEditRedo As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebug As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebugStop As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebugStepOver As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebugStepOut As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebugGo As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebugStep As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDebugRestart As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuClearAllBreakpoints As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTopic As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuOpenHelp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAPIHelp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuRequest As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSearch As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAbout As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents toolstripStandard As System.Windows.Forms.ToolStrip
    Friend WithEvents btnSave As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnPrint As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnCut As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnCopy As System.Windows.Forms.ToolStripButton
    Friend WithEvents PrintToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuPrintProcess As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuPrintPreview As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuPrintOnSinglePage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripMenuItem3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripMenuItem4 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuStartParams As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem6 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuCalculationZoom As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuFocusDebugStage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem7 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripMenuItem8 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents btnPaste As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolstripSearch As DiagramSearchToolstrip
    Friend WithEvents mnuEditAdvancedSearch As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents toolstripDebug As System.Windows.Forms.ToolStrip
    Friend WithEvents btnMenuDebugGo As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents btnMenuDebugPause As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMenuDebugStep As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMenuDebugStepOver As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMenuDebugStepOut As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMenuDebugReset As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMenuFullScreen As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolstripCont As System.Windows.Forms.ToolStripContainer
    Friend WithEvents mnuTools As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuPointer As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuLink As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuRead As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuWrite As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuNavigate As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCode As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuWait As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuProcess As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuPage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAction As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDecision As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuChoice As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCalculation As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDataItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCollection As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuLoop As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuNote As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuEnd As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAnchor As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnBreakpoint As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnProcessMI As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMenuDebugLaunchApp As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents stsBar As System.Windows.Forms.StatusStrip
    Friend WithEvents lblStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents btnWatch As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnUndo As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents btnRedo As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents ToolStripSeparator5 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuShowAllTools As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuHideAllTools As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator8 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuToolsDataItemWatch As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnRefresh As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator9 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuEditAddPage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator10 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents pnlExpressionRow As System.Windows.Forms.Panel
    Friend WithEvents lStoreIn As System.Windows.Forms.Label
    Friend WithEvents exprEdit As AutomateUI.ctlExpressionEdit
    Friend WithEvents objStoreInEdit As AutomateUI.ctlStoreInEdit
    Friend WithEvents lExpression As System.Windows.Forms.Label
    Friend WithEvents mnuAlert As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTransparency As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTransparencyNone As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTransparencyLow As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTransparencyMedium As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTransparencyHigh As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTransparencyVeryHigh As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripMenuItem13 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents AlwaysOnTopToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnPanViewDropDown As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents ToolStripSeparator11 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuEditSelectAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator12 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents SelectedStagesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DisableLoggingSelected As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EnableLoggingSelected As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem9 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuApplicationModeller As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnLogging As System.Windows.Forms.ToolStripButton
    Friend WithEvents txtLogMessages As ctlRichTextBox
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents btnShowSysManLogger As System.Windows.Forms.ToolStripButton
    Friend WithEvents mnuDebugExceptions As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuException As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuRecover As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuResume As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuBlock As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuMultipleCalculation As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents btnValidate As System.Windows.Forms.ToolStripButton
    Friend WithEvents mnuDebugValidate As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator13 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents miExportThisPage As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents miExportThisProcess As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents miExportAdhocPackage As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents miExportRelease As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents misepExportRelease As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuReports As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuElementUsage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents toolstripTools As System.Windows.Forms.ToolStrip
    Friend WithEvents btnPointer As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnLink As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnBlock As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnRead As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnWrite As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnNavigate As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnCode As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnWait As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnProcess As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnPage As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnAction As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnDecision As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnChoice As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnCalculation As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnMultipleCalculation As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnData As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnCollection As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnLoop As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnNote As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnAnchor As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnEnd As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnAlert As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnException As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnRecover As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnResume As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolstripFont As System.Windows.Forms.ToolStrip
    Friend WithEvents cmbFont As System.Windows.Forms.ToolStripComboBox
    Friend WithEvents cmbSize As System.Windows.Forms.ToolStripComboBox
    Friend WithEvents btnBold As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnItalic As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnUnderline As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnFontColour As AutomateUI.clsColorButton
    Friend WithEvents btnMenuApplicationModeller As System.Windows.Forms.ToolStripButton
    Friend WithEvents mnuToolsFont As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mZoomToolstripCombo As System.Windows.Forms.ToolStripComboBox
    Friend WithEvents mZoomInToolstripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents mZoomOutToolstripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripLabel1 As System.Windows.Forms.ToolStripLabel
    Friend WithEvents mnuEditDependencies As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AllStagesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EnableErrorLoggingSelected As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EnableLoggingAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DisableLoggingAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EnableErrorLoggingAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuToolsLockPositions As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SkillToolbarVisiblityToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SkillsToolbarPanel As Panel
End Class