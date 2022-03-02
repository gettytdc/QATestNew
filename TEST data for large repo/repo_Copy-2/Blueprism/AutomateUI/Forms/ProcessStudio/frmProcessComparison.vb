Imports AutomateControls
Imports AutomateUI.My.Resources
Imports System.Threading

Imports AutomateControls.Forms

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Collections.CollectionUtil

Imports BluePrism.AutomateAppCore.Groups
Imports System.IO
Imports BluePrism.BPCoreLib

''' Project  : Automate
''' Class    : frmProcessComparison
'''
''' <summary>
''' This form takes in two processes - the second a modification
''' of the first - and displays new stages in green, deleted stages
''' in red and modified stages in yellow.
''' </summary>
Friend Class frmProcessComparison
    Inherits HelpButtonForm
    Implements IPermission, IProcessViewingForm
    Implements IChild

#Region " Class scope declarations "

    ''' <summary>
    ''' Thin wrapper around a <see cref="List(Of String)"/> which adds a couple of
    ''' extra methods to handle string formatting on the fly
    ''' </summary>
    Private Class DiffList : Inherits List(Of String)
        Public Overloads Sub Add(ByVal msg As String, ByVal arg0 As Object)
            Add(String.Format(msg, arg0))
        End Sub
        Public Overloads Sub Add(ByVal msg As String, ByVal arg0 As Object, ByVal arg1 As Object)
            Add(String.Format(msg, arg0, arg1))
        End Sub
        Public Overloads Sub Add(ByVal msg As String, ByVal ParamArray args() As Object)
            Add(String.Format(msg, args))
        End Sub
    End Class

#End Region

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    Friend WithEvents CtlProcessViewer1 As AutomateUI.ctlProcessViewer
    Friend WithEvents CtlProcessViewer2 As AutomateUI.ctlProcessViewer
    Friend WithEvents objBluebar As AutomateControls.TitleBar
    Friend WithEvents Panel2 As AutomateUI.clsPanel
    Friend WithEvents Panel1 As AutomateUI.clsPanel
    Friend WithEvents mnuMain As System.Windows.Forms.MenuStrip
    Friend WithEvents mnuFile As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuFilePrint As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuView As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuZoom As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z400 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z200 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z150 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z100 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z75 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z50 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents z25 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents zSep As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents zDyn As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents chkGrid As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuClose As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents timStatusBarTimer As System.Windows.Forms.Timer
    Friend WithEvents CtlProcessSearch1 As AutomateUI.DiagramSearchToolstrip
    Friend WithEvents CtlProcessSearch2 As AutomateUI.DiagramSearchToolstrip
    Friend WithEvents mnuFirstDiff As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuPrevDiff As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuNextDiff As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuLastDiff As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblBefore As System.Windows.Forms.Label
    Friend WithEvents ToolStripContainer1 As System.Windows.Forms.ToolStripContainer
    Friend WithEvents stsBar As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents btnFirstDifference As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnPreviousDifference As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnLastDifference As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnNextDifference As System.Windows.Forms.ToolStripButton
    Friend WithEvents sep1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents MenuItem1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuExportLeft As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuExportRight As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents stsBarTextArea As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblAfter As System.Windows.Forms.Label

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProcessComparison))
        Me.CtlProcessViewer1 = New AutomateUI.ctlProcessViewer()
        Me.CtlProcessViewer2 = New AutomateUI.ctlProcessViewer()
        Me.lblBefore = New System.Windows.Forms.Label()
        Me.lblAfter = New System.Windows.Forms.Label()
        Me.objBluebar = New AutomateControls.TitleBar()
        Me.Panel2 = New AutomateUI.clsPanel()
        Me.Panel1 = New AutomateUI.clsPanel()
        Me.mnuMain = New System.Windows.Forms.MenuStrip()
        Me.mnuFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFilePrint = New System.Windows.Forms.ToolStripMenuItem()
        Me.sep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuClose = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuExportLeft = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuExportRight = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuView = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuZoom = New System.Windows.Forms.ToolStripMenuItem()
        Me.z400 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z200 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z150 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z100 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z75 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z50 = New System.Windows.Forms.ToolStripMenuItem()
        Me.z25 = New System.Windows.Forms.ToolStripMenuItem()
        Me.zSep = New System.Windows.Forms.ToolStripMenuItem()
        Me.zDyn = New System.Windows.Forms.ToolStripMenuItem()
        Me.chkGrid = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuFirstDiff = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPrevDiff = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuNextDiff = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuLastDiff = New System.Windows.Forms.ToolStripMenuItem()
        Me.timStatusBarTimer = New System.Windows.Forms.Timer(Me.components)
        Me.CtlProcessSearch1 = New AutomateUI.DiagramSearchToolstrip()
        Me.CtlProcessSearch2 = New AutomateUI.DiagramSearchToolstrip()
        Me.ToolStripContainer1 = New System.Windows.Forms.ToolStripContainer()
        Me.stsBar = New System.Windows.Forms.StatusStrip()
        Me.stsBarTextArea = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.btnFirstDifference = New System.Windows.Forms.ToolStripButton()
        Me.btnPreviousDifference = New System.Windows.Forms.ToolStripButton()
        Me.btnNextDifference = New System.Windows.Forms.ToolStripButton()
        Me.btnLastDifference = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.Panel2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.mnuMain.SuspendLayout()
        Me.ToolStripContainer1.BottomToolStripPanel.SuspendLayout()
        Me.ToolStripContainer1.ContentPanel.SuspendLayout()
        Me.ToolStripContainer1.TopToolStripPanel.SuspendLayout()
        Me.ToolStripContainer1.SuspendLayout()
        Me.stsBar.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'CtlProcessViewer1
        '
        resources.ApplyResources(Me.CtlProcessViewer1, "CtlProcessViewer1")
        Me.CtlProcessViewer1.BackColor = System.Drawing.Color.Transparent
        Me.CtlProcessViewer1.ClipboardProcess = Nothing
        Me.CtlProcessViewer1.ClipboardProcessLocation = CType(resources.GetObject("CtlProcessViewer1.ClipboardProcessLocation"), System.Drawing.PointF)
        Me.CtlProcessViewer1.MouseWheelEnabled = False
        Me.CtlProcessViewer1.Name = "CtlProcessViewer1"
        Me.CtlProcessViewer1.OpenedAsDebugSubProcess = False
        Me.CtlProcessViewer1.ShowGridLines = True
        Me.CtlProcessViewer1.SnapToGrid = True
        Me.CtlProcessViewer1.SuppressProcessDisposal = False
        Me.CtlProcessViewer1.ToolDragging = False
        '
        'CtlProcessViewer2
        '
        resources.ApplyResources(Me.CtlProcessViewer2, "CtlProcessViewer2")
        Me.CtlProcessViewer2.BackColor = System.Drawing.Color.Transparent
        Me.CtlProcessViewer2.ClipboardProcess = Nothing
        Me.CtlProcessViewer2.ClipboardProcessLocation = CType(resources.GetObject("CtlProcessViewer2.ClipboardProcessLocation"), System.Drawing.PointF)
        Me.CtlProcessViewer2.MouseWheelEnabled = False
        Me.CtlProcessViewer2.Name = "CtlProcessViewer2"
        Me.CtlProcessViewer2.OpenedAsDebugSubProcess = False
        Me.CtlProcessViewer2.ShowGridLines = True
        Me.CtlProcessViewer2.SnapToGrid = True
        Me.CtlProcessViewer2.SuppressProcessDisposal = False
        Me.CtlProcessViewer2.ToolDragging = False
        '
        'lblBefore
        '
        resources.ApplyResources(Me.lblBefore, "lblBefore")
        Me.lblBefore.Name = "lblBefore"
        '
        'lblAfter
        '
        resources.ApplyResources(Me.lblAfter, "lblAfter")
        Me.lblAfter.Name = "lblAfter"
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        Me.objBluebar.Name = "objBluebar"
        '
        'Panel2
        '
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.BackColor = System.Drawing.SystemColors.Control
        Me.Panel2.BorderColor = System.Drawing.Color.DarkSlateBlue
        Me.Panel2.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.Panel2.BorderWidth = 4
        Me.Panel2.Controls.Add(Me.CtlProcessViewer2)
        Me.Panel2.Name = "Panel2"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.BackColor = System.Drawing.SystemColors.Control
        Me.Panel1.BorderColor = System.Drawing.Color.DarkSlateBlue
        Me.Panel1.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.Panel1.BorderWidth = 4
        Me.Panel1.Controls.Add(Me.CtlProcessViewer1)
        Me.Panel1.Name = "Panel1"
        '
        'mnuMain
        '
        resources.ApplyResources(Me.mnuMain, "mnuMain")
        Me.mnuMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible
        Me.mnuMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFile, Me.mnuView})
        Me.mnuMain.Name = "mnuMain"
        '
        'mnuFile
        '
        Me.mnuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFilePrint, Me.sep1, Me.mnuClose, Me.mnuExportLeft, Me.mnuExportRight})
        Me.mnuFile.Name = "mnuFile"
        resources.ApplyResources(Me.mnuFile, "mnuFile")
        '
        'mnuFilePrint
        '
        Me.mnuFilePrint.Name = "mnuFilePrint"
        resources.ApplyResources(Me.mnuFilePrint, "mnuFilePrint")
        '
        'sep1
        '
        Me.sep1.Name = "sep1"
        resources.ApplyResources(Me.sep1, "sep1")
        '
        'mnuClose
        '
        Me.mnuClose.Name = "mnuClose"
        resources.ApplyResources(Me.mnuClose, "mnuClose")
        '
        'mnuExportLeft
        '
        Me.mnuExportLeft.Name = "mnuExportLeft"
        resources.ApplyResources(Me.mnuExportLeft, "mnuExportLeft")
        '
        'mnuExportRight
        '
        Me.mnuExportRight.Name = "mnuExportRight"
        resources.ApplyResources(Me.mnuExportRight, "mnuExportRight")
        '
        'mnuView
        '
        Me.mnuView.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuZoom, Me.chkGrid, Me.MenuItem1, Me.mnuFirstDiff, Me.mnuPrevDiff, Me.mnuNextDiff, Me.mnuLastDiff})
        Me.mnuView.Name = "mnuView"
        resources.ApplyResources(Me.mnuView, "mnuView")
        '
        'mnuZoom
        '
        Me.mnuZoom.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.z400, Me.z200, Me.z150, Me.z100, Me.z75, Me.z50, Me.z25, Me.zSep, Me.zDyn})
        Me.mnuZoom.Name = "mnuZoom"
        resources.ApplyResources(Me.mnuZoom, "mnuZoom")
        '
        'z400
        '
        Me.z400.Name = "z400"
        resources.ApplyResources(Me.z400, "z400")
        '
        'z200
        '
        Me.z200.Name = "z200"
        resources.ApplyResources(Me.z200, "z200")
        '
        'z150
        '
        Me.z150.Name = "z150"
        resources.ApplyResources(Me.z150, "z150")
        '
        'z100
        '
        Me.z100.Checked = True
        Me.z100.CheckState = System.Windows.Forms.CheckState.Checked
        Me.z100.Name = "z100"
        resources.ApplyResources(Me.z100, "z100")
        '
        'z75
        '
        Me.z75.Name = "z75"
        resources.ApplyResources(Me.z75, "z75")
        '
        'z50
        '
        Me.z50.Name = "z50"
        resources.ApplyResources(Me.z50, "z50")
        '
        'z25
        '
        Me.z25.Name = "z25"
        resources.ApplyResources(Me.z25, "z25")
        '
        'zSep
        '
        Me.zSep.Name = "zSep"
        resources.ApplyResources(Me.zSep, "zSep")
        '
        'zDyn
        '
        Me.zDyn.Checked = True
        Me.zDyn.CheckState = System.Windows.Forms.CheckState.Checked
        Me.zDyn.Name = "zDyn"
        resources.ApplyResources(Me.zDyn, "zDyn")
        '
        'chkGrid
        '
        Me.chkGrid.Checked = True
        Me.chkGrid.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkGrid.Name = "chkGrid"
        resources.ApplyResources(Me.chkGrid, "chkGrid")
        '
        'MenuItem1
        '
        Me.MenuItem1.Name = "MenuItem1"
        resources.ApplyResources(Me.MenuItem1, "MenuItem1")
        '
        'mnuFirstDiff
        '
        Me.mnuFirstDiff.Name = "mnuFirstDiff"
        resources.ApplyResources(Me.mnuFirstDiff, "mnuFirstDiff")
        '
        'mnuPrevDiff
        '
        Me.mnuPrevDiff.Name = "mnuPrevDiff"
        resources.ApplyResources(Me.mnuPrevDiff, "mnuPrevDiff")
        '
        'mnuNextDiff
        '
        Me.mnuNextDiff.Name = "mnuNextDiff"
        resources.ApplyResources(Me.mnuNextDiff, "mnuNextDiff")
        '
        'mnuLastDiff
        '
        Me.mnuLastDiff.Name = "mnuLastDiff"
        resources.ApplyResources(Me.mnuLastDiff, "mnuLastDiff")
        '
        'timStatusBarTimer
        '
        '
        'CtlProcessSearch1
        '
        resources.ApplyResources(Me.CtlProcessSearch1, "CtlProcessSearch1")
        Me.CtlProcessSearch1.BackColor = System.Drawing.SystemColors.Control
        Me.CtlProcessSearch1.ModeIsObjectStudio = False
        Me.CtlProcessSearch1.Name = "CtlProcessSearch1"
        Me.CtlProcessSearch1.TabStop = True
        '
        'CtlProcessSearch2
        '
        resources.ApplyResources(Me.CtlProcessSearch2, "CtlProcessSearch2")
        Me.CtlProcessSearch2.BackColor = System.Drawing.SystemColors.Control
        Me.CtlProcessSearch2.ModeIsObjectStudio = False
        Me.CtlProcessSearch2.Name = "CtlProcessSearch2"
        Me.CtlProcessSearch2.TabStop = True
        '
        'ToolStripContainer1
        '
        '
        'ToolStripContainer1.BottomToolStripPanel
        '
        Me.ToolStripContainer1.BottomToolStripPanel.Controls.Add(Me.stsBar)
        '
        'ToolStripContainer1.ContentPanel
        '
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.objBluebar)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.CtlProcessSearch1)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.Panel2)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.CtlProcessSearch2)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.Panel1)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.lblBefore)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.lblAfter)
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.ToolStrip1)
        resources.ApplyResources(Me.ToolStripContainer1.ContentPanel, "ToolStripContainer1.ContentPanel")
        resources.ApplyResources(Me.ToolStripContainer1, "ToolStripContainer1")
        Me.ToolStripContainer1.Name = "ToolStripContainer1"
        '
        'ToolStripContainer1.TopToolStripPanel
        '
        Me.ToolStripContainer1.TopToolStripPanel.Controls.Add(Me.mnuMain)
        '
        'stsBar
        '
        resources.ApplyResources(Me.stsBar, "stsBar")
        Me.stsBar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.stsBarTextArea})
        Me.stsBar.Name = "stsBar"
        '
        'stsBarTextArea
        '
        Me.stsBarTextArea.Name = "stsBarTextArea"
        resources.ApplyResources(Me.stsBarTextArea, "stsBarTextArea")
        '
        'ToolStrip1
        '
        Me.ToolStrip1.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.btnFirstDifference, Me.btnPreviousDifference, Me.btnNextDifference, Me.btnLastDifference, Me.ToolStripSeparator1})
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.TabStop = True
        '
        'btnFirstDifference
        '
        Me.btnFirstDifference.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnFirstDifference, "btnFirstDifference")
        Me.btnFirstDifference.Name = "btnFirstDifference"
        '
        'btnPreviousDifference
        '
        Me.btnPreviousDifference.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnPreviousDifference, "btnPreviousDifference")
        Me.btnPreviousDifference.Name = "btnPreviousDifference"
        '
        'btnNextDifference
        '
        Me.btnNextDifference.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnNextDifference, "btnNextDifference")
        Me.btnNextDifference.Name = "btnNextDifference"
        '
        'btnLastDifference
        '
        Me.btnLastDifference.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        resources.ApplyResources(Me.btnLastDifference, "btnLastDifference")
        Me.btnLastDifference.Name = "btnLastDifference"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'frmProcessComparison
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.ToolStripContainer1)
        Me.HelpButton = True
        Me.MainMenuStrip = Me.mnuMain
        Me.Name = "frmProcessComparison"
        Me.Panel2.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.mnuMain.ResumeLayout(False)
        Me.mnuMain.PerformLayout()
        Me.ToolStripContainer1.BottomToolStripPanel.ResumeLayout(False)
        Me.ToolStripContainer1.BottomToolStripPanel.PerformLayout()
        Me.ToolStripContainer1.ContentPanel.ResumeLayout(False)
        Me.ToolStripContainer1.ContentPanel.PerformLayout()
        Me.ToolStripContainer1.TopToolStripPanel.ResumeLayout(False)
        Me.ToolStripContainer1.TopToolStripPanel.PerformLayout()
        Me.ToolStripContainer1.ResumeLayout(False)
        Me.ToolStripContainer1.PerformLayout()
        Me.stsBar.ResumeLayout(False)
        Me.stsBar.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Members"

    ''' <summary>
    ''' The IDs of the two processes: the old version and the young version
    ''' respectively. Note that two guids are needed because in principle the
    ''' two processes being compared may not have the same ID - if one process is
    ''' cloned to another then the ID changes.
    ''' </summary>
    Private mProcessIdLeft As Guid, mProcessIdRight As Guid

    ''' <summary>
    ''' The XML of the older of the two processes
    ''' </summary>
    Private mXMLLeft As String

    ''' <summary>
    ''' The XML of the newer of the two processes
    ''' </summary>
    Private mXMLRight As String

    ''' <summary>
    ''' The process object of the older of the two processes
    ''' </summary>
    Private mProcessLeft As clsProcess

    ''' <summary>
    ''' The process object of the older of the two processes
    ''' </summary>
    Private mProcessRight As clsProcess

    ''' <summary>
    ''' The date from which the older process is taken.
    ''' </summary>
    Private mDateLeft As Date

    ''' <summary>
    ''' The date from which the newer process is taken.
    ''' </summary>
    Private mDateRight As Date


    ''' <summary>
    ''' Keeps track of which process the user has selected, to determine
    ''' which process viewer should be sent user input events (keyboard etc).
    ''' </summary>
    Private SelectedProcessViewer As ctlProcessViewer



    ''' <summary>
    ''' This variable is used in timing the duration of messages on the status bar.
    ''' Counts the ticks of the clock.
    ''' </summary>
    Private miNumberOfStatusBarTimerTicks As Integer

    ''' <summary>
    ''' This variable tells us how many ticks of the clock we should wait before
    ''' clearing any messages from the status bar.
    ''' </summary>
    Private miNumberOfTicksToWaitBeforeClearingStatusBarMessage As Integer

    ''' <summary>
    ''' Navigates through the differences between the two processers.
    ''' </summary>
    Private mDifferencesNavigator As DifferencesEnumerator

    ''' <summary>
    ''' What are we comparing?  Process or Object.
    ''' </summary>
    Private mProcessType As DiagramType = DiagramType.Process

#End Region

#Region "Properties"

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return stsBarTextArea.BackColor
        End Get
        Set(value As Color)
            objBluebar.BackColor = value
            stsBarTextArea.BackColor = value
            stsBar.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return stsBarTextArea.ForeColor
        End Get
        Set(value As Color)
            objBluebar.TitleColor = value
            objBluebar.SubtitleColor = value
            stsBarTextArea.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' Used to determine whether the current user has permission
    ''' to view the form.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return If(mProcessType = DiagramType.Process,
                Permission.ByName(Permission.ProcessStudio.ImpliedViewProcess),
                Permission.ByName(Permission.ObjectStudio.ImpliedViewBusinessObject))
        End Get
    End Property

    ''' <summary>
    ''' private member to store public property DefaultStatusBarMessageDuration().
    ''' </summary>
    Private miDefaultStatusBarMessageDuration As Integer = 3000
    ''' <summary>
    ''' The default time (in milliseconds) that messages will be displayed on the
    ''' status bar for. To use a one-off custom time, use the appropriate overload
    ''' of SetStatusBarText().
    '''
    ''' If not set, then this value defaults to 3000 milliseconds. This value must
    ''' be a multiple of 100. If not, it will be rounded up to the next 100.
    ''' </summary>
    ''' <value></value>
    Public Property DefaultStatusBarMessageDuration() As Integer Implements IProcessViewingForm.DefaultStatusBarMessageDuration
        Get
            Return miDefaultStatusBarMessageDuration
        End Get
        Set(ByVal Value As Integer)
            Value = CInt(Math.Ceiling((Value / 100)) * 100)         'round it up to the next multiple of 100
            miDefaultStatusBarMessageDuration = Value
        End Set
    End Property

#End Region

#Region "Constructor"

    Public Shared Function FromHistory(firstID As Integer, secondID As Integer, firstDate As Date, secondDate As Date, processID As Guid) As frmProcessComparison
        'Always use oldest history item as the 'before' version
        Dim leftProcess As clsProcess, leftXML As String, leftDate As Date
        Dim rightProcess As clsProcess, rightXML As String, rightdate As Date

        Dim info = Options.Instance.GetExternalObjectsInfo()

        If firstDate < secondDate Then
            leftXML = gSv.GetProcessHistoryXML(firstID, processID)
            leftProcess = clsProcess.FromXml(info, leftXML, False)
            leftDate = firstDate
            rightXML = gSv.GetProcessHistoryXML(secondID, processID)
            rightProcess = clsProcess.FromXml(info, rightXML, False)
            rightdate = secondDate
        Else
            leftXML = gSv.GetProcessHistoryXML(secondID, processID)
            leftProcess = clsProcess.FromXml(info, leftXML, False)
            leftDate = secondDate
            rightXML = gSv.GetProcessHistoryXML(firstID, processID)
            rightProcess = clsProcess.FromXml(info, rightXML, False)
            rightdate = firstDate
        End If

        Return New frmProcessComparison(leftXML, rightXML, leftProcess, rightProcess, processID, processID, leftDate, rightdate)
    End Function

    Public Shared Function FromGroupMembers(first As ProcessBackedGroupMember, second As ProcessBackedGroupMember) As frmProcessComparison
        If first Is Nothing Then Throw New ArgumentNullException(
             My.Resources.frmProcessComparison_ErrorComparingFirstEntryIsNotACompareableItem)

        If second Is Nothing Then Throw New ArgumentNullException(
             My.Resources.frmProcessComparison_ErrorComparingSecondEntryIsNotACompareableItem)

        'Always use oldest history item as the 'before' version
        Dim leftProcess As clsProcess, leftXML As String, leftDate As Date
        Dim rightProcess As clsProcess, rightXML As String, rightdate As Date

        Dim info = Options.Instance.GetExternalObjectsInfo()

        If first.ModifiedAt < second.ModifiedAt Then
            leftXML = gSv.GetProcessXML(first.IdAsGuid)
            leftProcess = clsProcess.FromXml(info, leftXML, False)
            leftDate = first.ModifiedAt
            rightXML = gSv.GetProcessXML(second.IdAsGuid)
            rightProcess = clsProcess.FromXml(info, rightXML, False)
            rightdate = second.ModifiedAt
        Else
            leftXML = gSv.GetProcessXML(second.IdAsGuid)
            leftProcess = clsProcess.FromXml(info, leftXML, False)
            leftDate = second.ModifiedAt
            rightXML = gSv.GetProcessXML(first.IdAsGuid)
            rightProcess = clsProcess.FromXml(info, rightXML, False)
            rightdate = first.ModifiedAt
        End If

        Return New frmProcessComparison(
         leftXML, rightXML, leftProcess, rightProcess, first.IdAsGuid, second.IdAsGuid, leftDate, rightdate
        )

    End Function

    Public Shared Function Fromfile(first As ProcessBackedGroupMember, file As FileInfo) As frmProcessComparison
        Dim fileXML = IO.File.ReadAllText(file.FullName)
        Dim info = Options.Instance.GetExternalObjectsInfo()
        Dim process = clsProcess.FromXml(info, fileXML, False)

        Dim leftProcess As clsProcess, leftXML As String, leftDate As Date
        Dim rightProcess As clsProcess, rightXML As String, rightdate As Date
        If first.ModifiedAt < process.ModifiedDate Then
            leftXML = gSv.GetProcessXML(first.IdAsGuid)
            leftProcess = clsProcess.FromXml(info, leftXML, False)
            leftDate = first.ModifiedAt

            rightXML = fileXML
            rightProcess = process
            rightdate = process.ModifiedDate
        Else
            leftXML = fileXML
            leftProcess = process
            leftDate = process.ModifiedDate

            rightXML = gSv.GetProcessXML(first.IdAsGuid)
            rightProcess = clsProcess.FromXml(info, rightXML, False)
            rightdate = first.ModifiedAt
        End If

        Return New frmProcessComparison(
         leftXML, rightXML, leftProcess, rightProcess, first.IdAsGuid, process.Id, leftDate, rightdate
        )
    End Function

    Public Shared Function FromBackup(processID As Guid) As frmProcessComparison

        Dim info = Options.Instance.GetExternalObjectsInfo()

        Dim sBackupXML As String = Nothing, sErr As String = Nothing
        Try
            gSv.AutoSaveGetBackupXML(processID, sBackupXML)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.frmProcessComparison_FailedToRetrieveXmlInAutosavedBackupRecord0, ex.Message))
        End Try
        Dim backupProcess = clsProcess.FromXml(info, sBackupXML, False)


        Dim BackupDate As DateTime
        Try
            BackupDate = gSv.AutoSaveGetBackupDateTime(processID)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.frmProcessComparison_FailedToRetrieveBackupDateRecord0, ex.Message))
        End Try

        Dim dModifiedDate As DateTime
        gSv.GetProcessInfo(processID, Nothing, Nothing, Nothing, dModifiedDate)
        Dim originalXML As String = gSv.GetProcessXML(processID)

        Dim originalProcess = clsProcess.FromXml(info, originalXML, False)

        Return New frmProcessComparison(originalXML, sBackupXML, originalProcess, backupProcess, processID, processID, dModifiedDate, BackupDate)
    End Function

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="XMLLeft">XML of the first process</param>
    ''' <param name="XMLRight">XML of the second process</param>
    ''' <param name="procIdLeft">ID of the first process</param>
    ''' <param name="procIdRight">ID of the second process</param>
    ''' <param name="dtLeft">Date of the second process in UTC</param>
    ''' <param name="dtRight">Date of the second process in UTC</param>
    Private Sub New(
     ByVal XMLleft As String, XMLright As String,
     ByVal processLeft As clsProcess, ByVal processRight As clsProcess,
     ByVal procIdLeft As Guid, ByVal procIdRight As Guid,
     ByVal dtLeft As Date, ByVal dtRight As Date)

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        mProcessType = processLeft.ProcessType
        CtlProcessSearch1.ModeIsObjectStudio = (mProcessType = DiagramType.Object)
        CtlProcessSearch2.ModeIsObjectStudio = (mProcessType = DiagramType.Object)

        mXMLLeft = XMLleft
        mXMLRight = XMLright

        mProcessLeft = processLeft
        mProcessRight = processRight

        mProcessIdLeft = procIdLeft
        mProcessIdRight = procIdRight

        mDateLeft = If(dtLeft = Date.MinValue, Date.MinValue, dtLeft.ToLocalTime())
        mDateRight = If(dtRight = Date.MinValue, Date.MinValue, dtRight.ToLocalTime())

    End Sub

#End Region

#Region "Events"

#Region "Loading"

    Private Sub frmProcessComparison_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Byref argument to processviewer informing us of successful startup
        Dim bStartOK As Boolean
        Dim viewMode = ProcessViewMode.CompareProcesses
        If mProcessType = DiagramType.Object Then viewMode = ProcessViewMode.CompareObjects

        Me.CtlProcessViewer1.SetParent(Me)
        Me.CtlProcessViewer1.SetSuperParent(mParent)
        Me.CtlProcessViewer1.Startup(viewMode, mProcessIdLeft, "", "", bStartOK, mProcessLeft)
        mProcessLeft = Me.CtlProcessViewer1.Process

        If Not bStartOK Then
            UserMessage.Show(My.Resources.frmProcessComparison_WarningAnErrorOccurredWhilstOpeningTheFirstProcessItMayNotBeDisplayedProperly)
        End If
        bStartOK = False

        Me.CtlProcessViewer2.SetParent(Me)
        Me.CtlProcessViewer2.SetSuperParent(mParent)
        Me.CtlProcessViewer2.Startup(viewMode, mProcessIdRight, "", "", bStartOK, mProcessRight)
        mProcessRight = Me.CtlProcessViewer2.Process

        Me.CtlProcessSearch1.SetProcessStudioParent(Me)
        Me.CtlProcessSearch2.SetProcessStudioParent(Me)
        Me.CtlProcessSearch1.SetProcessViewingControl(Me.CtlProcessViewer1)
        Me.CtlProcessSearch2.SetProcessViewingControl(Me.CtlProcessViewer2)

        If Not bStartOK Then
            UserMessage.Show(My.Resources.frmProcessComparison_WarningAnErrorOccurredWhilstOpeningTheSecondProcessItMayNotBeDisplayedProperly)
        End If

        'instantiate the differences navigator
        Me.mDifferencesNavigator = New DifferencesEnumerator(Me.mProcessLeft, Me.mProcessRight)

        'find out what the differences are ...
        CompareProcesses()
        Dim DifferenceCount As Integer = Me.mDifferencesNavigator.DifferenceCount
        If DifferenceCount > 0 Then
            Me.SetStatusBarText(String.Format(My.Resources.frmProcessComparison_0DifferenceSFound, DifferenceCount.ToString))
        Else
            Me.SetStatusBarText(My.Resources.frmProcessComparison_NoDifferencesFound)
        End If

        Me.CtlProcessViewer1.Invalidate()
        Me.CtlProcessViewer2.Invalidate()

        Me.SelectedProcessViewer = Me.CtlProcessViewer1
        Me.CtlProcessViewer1.MouseWheelEnabled = True
        Me.CtlProcessViewer2.MouseWheelEnabled = False
        Me.Panel1.BorderStyle = clsPanel.BorderMode.On
        Me.Panel2.BorderStyle = clsPanel.BorderMode.Off

        Me.objBluebar.TitlePosition = New System.Drawing.Point(10, 10)

        'set title
        Dim sProc1Name As String = "'" & CtlProcessViewer1.Process.Name & "'"
        Dim sProc2Name As String = "'" & CtlProcessViewer2.Process.Name & "'"

        Dim sProcType As String
        If mProcessType = DiagramType.Object Then
            Me.Text = My.Resources.frmProcessComparison_BusinessObjectComparison
            sProcType = My.Resources.frmProcessComparison_Object
        Else
            Me.Text = My.Resources.frmProcessComparison_ProcessComparison
            sProcType = My.Resources.frmProcessComparison_Process
        End If

        'format and set subtitle
        sProc1Name = TrimText(sProc1Name, 48)
        sProc2Name = TrimText(sProc2Name, 48)
        Dim sCmp1 = String.Format(frmProcessComparison_Comparing0, sProcType)
        Dim sCmp2 = String.Format(frmProcessComparison_With0, sProcType)
        Dim iCmp1 = i18nDisplayWidth(sCmp1)
        Dim iCmp2 = i18nDisplayWidth(sCmp2)
        While iCmp2 < iCmp1 - 1
            iCmp2 = i18nDisplayWidth(" "c + sCmp2)
            If iCmp2 < iCmp1 - 1 Then
                sCmp2 = " "c + sCmp2
            End If
        End While

        Const padding As Integer = 90
        sProc1Name = sProc1Name.PadRight(padding)
        sProc2Name = sProc2Name.PadRight(padding)

        Me.objBluebar.Title =
            String.Format(frmProcessComparison_Comparing0, sProcType) +
                          sProc1Name +
                          String.Format(frmProcessComparison_AsOf0, mDateLeft) +
                          vbCrLf +
                          sCmp2 +
                          sProc2Name +
                          String.Format(frmProcessComparison_AsOf0, mDateRight)

        AddHandler CtlProcessViewer1.pbview.MouseDown, AddressOf Me.frmProcessComparisonCtl1_MouseDown
        AddHandler CtlProcessViewer1.pbview.MouseUp, AddressOf Me.frmProcessComparisonCtl1_MouseUp

        AddHandler CtlProcessViewer2.pbview.MouseDown, AddressOf Me.frmProcessComparisonCtl2_MouseDown
        AddHandler CtlProcessViewer2.pbview.MouseUp, AddressOf Me.frmProcessComparisonCtl2_MouseUp

        mnuExportLeft.Enabled = CanExportProcess(mProcessIdLeft)
        mnuExportRight.Enabled = CanExportProcess(mProcessIdRight)

        'Move viewers to first difference
        ShowCurrentDifference()
    End Sub

    Private Function CanExportProcess(processId As Guid) As Boolean
        Dim processPermissions = gSv.GetEffectiveMemberPermissionsForProcess(processId)

        Return _
            processPermissions.HasPermission(
                User.Current,
                If(
                    mProcessType = DiagramType.Process,
                    Permission.ProcessStudio.ExportProcess,
                    Permission.ObjectStudio.ExportBusinessObject))
    End Function

    Private Function TrimText(s As String, len As Integer) As String
        If s Is Nothing Then Return String.Empty
        If s.Length >= len Then
            Return s.Substring(0, len - 3) & My.Resources.frmProcessComparison_Ellipsis
        Else
            Return s
        End If
    End Function

    Private Function i18nDisplayWidth(s As String) As Integer
        If s Is Nothing Then Return 0
        Dim dc As Graphics = objBluebar.CreateGraphics()
        Dim flags As TextFormatFlags = TextFormatFlags.NoClipping Or
                                        TextFormatFlags.NoPadding Or
                                        TextFormatFlags.NoPrefix
        'Dim font As Font = New Font(Me.objBluebar.Font, Me.objBluebar.Font.Style)
        Return TextRenderer.MeasureText(dc, s.Substring(0, s.Length), Me.objBluebar.Font, Me.objBluebar.PreferredSize, flags).Width
    End Function
#End Region

#Region "Resize"
    Private Sub frmProcessComparison_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Const HorizontalMargin As Integer = 4
        Const VerticalMargin As Integer = 4
        Const GapBetweenPanels As Integer = 2
        Const ProcessSearchOffset As Integer = 6

        Me.Panel1.Width = (Me.ClientSize.Width - (2 * HorizontalMargin) - GapBetweenPanels) \ 2
        Me.Panel2.Width = Panel1.Width

        Me.Panel1.Left = Me.ClientRectangle.Left + HorizontalMargin
        Me.Panel2.Left = Me.Panel1.Width + Me.Panel1.Left + GapBetweenPanels

        Me.Panel1.Height = Me.CtlProcessSearch1.Top - Me.Panel1.Top - VerticalMargin
        Me.Panel2.Height = Me.Panel1.Height

        Me.lblBefore.Left = Me.Panel1.Left
        Me.lblAfter.Left = Me.Panel2.Left

        Me.CtlProcessViewer1.Width = Me.Panel1.ClientSize.Width - 2 * Me.CtlProcessViewer1.Left
        Me.CtlProcessViewer2.Width = Me.Panel2.ClientSize.Width - 2 * Me.CtlProcessViewer2.Left

        Me.CtlProcessViewer1.Height = Me.Panel1.ClientSize.Height - 2 * Me.CtlProcessViewer1.Top
        Me.CtlProcessViewer2.Height = Me.Panel2.ClientSize.Height - 2 * Me.CtlProcessViewer2.Top

        Me.CtlProcessSearch1.Left = Me.Panel1.Left + ProcessSearchOffset
        Me.CtlProcessSearch2.Left = Me.Panel2.Left + ProcessSearchOffset

        Me.CtlProcessSearch2.BringToFront()
    End Sub
#End Region

#Region "User Input (Keyboard, Mouse etc)"

    ''' <summary>
    ''' Handles mouse scrolling in this form, passing it onto the selected process
    ''' viewer if it does not have focus (and thus has already handled the mousewheel
    ''' event itself)
    ''' </summary>
    Protected Overrides Sub OnMouseWheel(ByVal e As System.Windows.Forms.MouseEventArgs)
        If Not SelectedProcessViewer.ContainsFocus Then SelectedProcessViewer.DoMouseWheel(e)
    End Sub

    Private Sub frmProcessComparisonCtl1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CtlProcessViewer1.MouseUp

        If Not Me.SelectedProcessViewer Is Me.CtlProcessViewer1 Then
            Me.SelectedProcessViewer = Me.CtlProcessViewer1
            Panel2.BorderStyle = clsPanel.BorderMode.Off
            Me.CtlProcessViewer2.MouseWheelEnabled = False
            Panel1.BorderStyle = clsPanel.BorderMode.On
            Me.CtlProcessViewer1.MouseWheelEnabled = True
            Me.Panel1.Invalidate()
            Me.Panel2.Invalidate()
            UpdateMenus()
        Else
            Me.SelectedProcessViewer.DoMouseUp(sender, e)
        End If

    End Sub

    Private Sub frmProcessComparisonCtl2_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CtlProcessViewer2.MouseUp

        If Not Me.SelectedProcessViewer Is Me.CtlProcessViewer2 Then
            Me.SelectedProcessViewer = Me.CtlProcessViewer2
            Panel1.BorderStyle = clsPanel.BorderMode.Off
            Panel2.BorderStyle = clsPanel.BorderMode.On
            Me.CtlProcessViewer1.MouseWheelEnabled = False
            Me.CtlProcessViewer2.MouseWheelEnabled = True
            Me.Panel2.Invalidate()
            Me.Panel1.Invalidate()
            UpdateMenus()
        Else
            Me.SelectedProcessViewer.DoMouseUp(sender, e)
        End If

    End Sub

    Private Sub frmProcessComparisonCtl1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CtlProcessViewer1.MouseDown

        If Me.CtlProcessViewer1 Is Me.SelectedProcessViewer Then
            Me.SelectedProcessViewer.DoMouseDown(sender, e)
        End If
    End Sub

    Private Sub frmProcessComparisonCtl2_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles CtlProcessViewer1.MouseDown

        If Me.CtlProcessViewer2 Is Me.SelectedProcessViewer Then
            Me.SelectedProcessViewer.DoMouseDown(sender, e)
        End If
    End Sub

    'Private Sub frmProcessComparison_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs)

    '    Me.SelectedProcessViewer = sender.parent
    '    Me.SelectedProcessViewer.DoMouseDoubleClick(sender, e)

    'End Sub

#End Region

#Region "menu click events"

#Region "file Menu"

    Private Sub mnuFilePrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFilePrint.Click
        Me.SelectedProcessViewer.PrintPreview()
    End Sub

    Private Sub mnuClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuClose.Click
        MyBase.Close()
    End Sub

#End Region

#Region "View Menu"

#Region "Zoom SubMenu"

    Private Sub z400_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles z400.Click
        Me.SelectedProcessViewer.DoZoom(400)
    End Sub

    Private Sub z200_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles z200.Click
        Me.SelectedProcessViewer.DoZoom(200)
    End Sub

    Private Sub z150_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles z150.Click
        Me.SelectedProcessViewer.DoZoom(150)
    End Sub

    Private Sub z100_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles z100.Click
        Me.SelectedProcessViewer.DoZoom(100)
    End Sub

    Private Sub z75_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles z75.Click
        Me.SelectedProcessViewer.DoZoom(75)
    End Sub

    Private Sub z50_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles z50.Click
        Me.SelectedProcessViewer.DoZoom(50)
    End Sub

    Private Sub z25_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles z25.Click
        Me.SelectedProcessViewer.DoZoom(25)
    End Sub

#End Region

    Private Sub chkGrid_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkGrid.Click
        If chkGrid.Checked = True Then
            chkGrid.Checked = False
        Else
            chkGrid.Checked = True
        End If
        Me.SelectedProcessViewer.ShowGridLines = chkGrid.Checked
        Me.SelectedProcessViewer.InvalidateView()
    End Sub

#Region "Navigation Menu Item Clicks"

    Private Sub mnuFirstDiff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFirstDiff.Click, btnFirstDifference.Click
        Try
            If Me.mDifferencesNavigator.MoveToFirstDifference() Then
                Me.ShowCurrentDifference()
            Else
                ShowInformalMessage(My.Resources.frmProcessComparison_NoDifferencesFound, My.Resources.frmProcessComparison_ThereAreNoDifferencesToShow, ToolTipIcon.Info)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessComparison_InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub mnuNextDiff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuNextDiff.Click, btnNextDifference.Click
        Try
            If Me.mDifferencesNavigator.MoveToNextDifference() Then
                Me.ShowCurrentDifference()
            Else
                ShowInformalMessage(My.Resources.frmProcessComparison_NoDifferencesFound, My.Resources.frmProcessComparison_ThereAreNoFurtherDifferencesToShow, ToolTipIcon.Info)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessComparison_InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub mnuPrevDiff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuPrevDiff.Click, btnPreviousDifference.Click
        Try
            If Me.mDifferencesNavigator.MoveToPrevDifference() Then
                Me.ShowCurrentDifference()
            Else
                ShowInformalMessage(My.Resources.frmProcessComparison_NoDifferencesFound, My.Resources.frmProcessComparison_ThereAreNoFurtherDifferencesToShow, ToolTipIcon.Info)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessComparison_InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub mnulastDiff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuLastDiff.Click, btnLastDifference.Click
        Try
            If Me.mDifferencesNavigator.MoveToLastDifference() Then
                Me.ShowCurrentDifference()
            Else
                ShowInformalMessage(My.Resources.frmProcessComparison_NoDifferencesFound, My.Resources.frmProcessComparison_ThereAreNoDifferencesToShow, ToolTipIcon.Info)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessComparison_InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub ShowInformalMessage(ByVal Title As String, ByVal Prompt As String, ByVal Icon As ToolTipIcon)
        Me.SetStatusBarText(Prompt)
    End Sub

#End Region

#End Region

#End Region

#Region "Timer Events"

    Private Sub timStatusBarTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles timStatusBarTimer.Tick
        If Me.miNumberOfStatusBarTimerTicks >= Me.miNumberOfTicksToWaitBeforeClearingStatusBarMessage Then
            Me.timStatusBarTimer.Enabled = False
            Me.ClearStatusBarText()
            Me.miNumberOfTicksToWaitBeforeClearingStatusBarMessage = 0
        Else
            Me.miNumberOfStatusBarTimerTicks += 1
        End If
    End Sub

#End Region

#Region "References"

    Private Sub SelectDependency1(d As clsProcessDependency) Handles CtlProcessViewer1.SelectDependency
        CtlProcessSearch1.SelectDependency(d)
    End Sub

    Private Sub SelectDependency2(d As clsProcessDependency) Handles CtlProcessViewer2.SelectDependency
        CtlProcessSearch2.SelectDependency(d)
    End Sub

#End Region

#End Region

#Region "Methods"

#Region "Process Comparison"

    ''' <summary>
    ''' Takes the two processes under comparison, identifies changes
    ''' and updates the process objects accordingly.
    ''' </summary>
    Private Sub CompareProcesses()

        'get all stages' guids into an array from process1
        Dim proc1Stages As New Dictionary(Of Guid, clsProcessStage)
        SyncLock mProcessLeft
            For Each stg As clsProcessStage In mProcessLeft.GetStages()
                proc1Stages(stg.Id) = stg
            Next
        End SyncLock

        Dim proc2Stages As New Dictionary(Of Guid, clsProcessStage)
        SyncLock mProcessRight
            For Each stg As clsProcessStage In mProcessRight.GetStages()
                proc2Stages(stg.Id) = stg
            Next
        End SyncLock

        'go through all stages in process2 and make a comparison
        For Each stg2 As clsProcessStage In proc2Stages.Values
            SyncLock stg2
                Dim stg1 As clsProcessStage = Nothing
                If Not proc1Stages.TryGetValue(stg2.Id, stg1) Then
                    ' This stage did not exist in proc1, so mark it as newly
                    ' created and move on
                    stg2.DisplayMode = StageShowMode.Audit_New
                    stg2.EditSummary = My.Resources.frmProcessComparison_ThisStageIsNewlyCreated
                    stg2.FullChangesList = My.Resources.frmProcessComparison_ThisStageIsNewlyCreated

                    'add to the list of differences
                    mDifferencesNavigator.AddDifference(stg2)
                Else

                    Dim diffs As ICollection(Of String) = GetDifferences(stg1, stg2)

                    If diffs.Count > 0 Then
                        ' set the display mode to modified in each process and
                        ' set the edit summary
                        SyncLock mProcessLeft
                            Dim stg As clsProcessStage = mProcessLeft.GetStage(stg2.Id)
                            SyncLock stg
                                stg.DisplayMode = StageShowMode.Audit_Modified
                                stg.EditSummary = First(diffs)
                                stg.FullChangesList = Join(diffs, vbCrLf)
                                mProcessLeft.SetStage(stg2.GetStageID, stg)

                                'add the stage to the list of differences
                                mDifferencesNavigator.AddDifference(stg)
                            End SyncLock
                        End SyncLock

                        SyncLock mProcessRight
                            Dim stg As clsProcessStage = mProcessRight.GetStage(stg2.Id)
                            SyncLock stg
                                stg.DisplayMode = StageShowMode.Audit_Modified
                                stg.EditSummary = First(diffs)
                                stg.FullChangesList = Join(diffs, vbCrLf)
                                mProcessRight.SetStage(stg2.GetStageID, stg)

                                'add the stage to the list of differences
                                mDifferencesNavigator.AddDifference(stg)
                            End SyncLock
                        End SyncLock

                    Else
                        'mark the two stages as unchanged
                        stg2.EditSummary = My.Resources.frmProcessComparison_ThisStageIsUnchanged
                        stg2.FullChangesList = My.Resources.frmProcessComparison_ThisStageIsUnchanged

                        SyncLock mProcessLeft
                            Dim stg As clsProcessStage = mProcessLeft.GetStage(stg2.Id)
                            SyncLock stg
                                stg.EditSummary = My.Resources.frmProcessComparison_ThisStageIsUnchanged
                                stg.FullChangesList = My.Resources.frmProcessComparison_ThisStageIsUnchanged
                                mProcessLeft.SetStage(stg2.GetStageID, stg)
                            End SyncLock
                        End SyncLock

                    End If
                    diffs = Nothing

                    ' remove the relevant stage from the process 1 stages
                    ' first list so that we know it's been processed
                    proc1Stages.Remove(stg2.Id)
                End If
            End SyncLock
        Next

        ' for each stage left in process1, mark as deleted
        For Each id As Guid In proc1Stages.Keys
            Dim stg As clsProcessStage = mProcessLeft.GetStage(id)
            stg.DisplayMode = StageShowMode.Audit_Deleted
            stg.EditSummary = My.Resources.frmProcessComparison_ThisStageHasBeenDeleted
            stg.FullChangesList = My.Resources.frmProcessComparison_ThisStageHasBeenDeleted
            mProcessLeft.SetStage(id, stg)

            'add to list of differences
            mDifferencesNavigator.AddDifference(stg)
        Next

    End Sub

    ''' <summary>
    ''' Determines if the two stages are equal.
    ''' </summary>
    ''' <param name="stg1">The old version of the stage.</param>
    ''' <param name="stg2">The new version of the stage</param>
    ''' <returns>On return, a list of differences found between the stages.
    ''' Note that the list is created by this method - ie. any existing list is
    ''' overwritten by this method.</returns>
    Private Function GetDifferences(
     ByVal stg1 As clsProcessStage,
     ByVal stg2 As clsProcessStage) As IList(Of String)

        Dim diffs As New DiffList()

        ' If it's the same reference object, then obviously there are no differences
        If stg1 Is stg2 Then Return diffs

        ' If the types are different, abort now - everything following relies on the
        ' two stage types matching
        If stg1.StageType <> stg2.StageType Then diffs.Add(
         My.Resources.frmProcessComparison_InternalErrorUnmatchedStageTypesPassedToComparator01,
         stg1.StageType, stg2.StageType) : Return diffs

        If stg1.Name <> stg2.Name Then diffs.Add(My.Resources.frmProcessComparison_StageNameHasChanged)

        If stg1.GetNarrative() <> stg2.GetNarrative() Then _
         diffs.Add(My.Resources.frmProcessComparison_StageDescriptionHasChanged)

        If stg1.GetSubSheetID() <> stg2.GetSubSheetID() Then diffs.Add(
         My.Resources.frmProcessComparison_StageHasMovedPagesOldPageWas0, stg1.SubSheet.Name)

        Select Case stg1.StageType

            Case StageTypes.Start
                If Not ParameterSetsAreEqual(stg1.GetInputs(), stg2.GetInputs()) Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheInputParametersOfThisStageHaveChanged)

            Case StageTypes.End
                If Not ParameterSetsAreEqual(stg1.GetOutputs(), stg2.GetOutputs()) Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheOutputsOfThisStageHaveChanged)

            Case StageTypes.Action
                'We already know stage1 and stage2 are of the same type so cast both
                Dim actionStg1 As clsActionStage = CType(stg1, clsActionStage)
                Dim actionStg2 As clsActionStage = CType(stg2, clsActionStage)

                If actionStg1.ObjectName <> actionStg2.ObjectName Then diffs.Add(
                 My.Resources.frmProcessComparison_BusinessObjectUsedInThisStageHasChangedFrom0To1,
                 actionStg1.ObjectName, actionStg2.ObjectName)

                If actionStg1.ActionName <> actionStg2.ActionName Then diffs.Add(
                 My.Resources.frmProcessComparison_TheActionUsedInThisStageHasChangedFrom0To1,
                 actionStg1.ActionName, actionStg2.ActionName)

                'if we get to this point then we can assume that the action itself is
                'unchanged so now we check the inputs/outputs
                If Not ParameterSetsAreEqual(stg1.GetInputs(), stg2.GetInputs()) Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheInputParametersOfThisStageHaveChanged)

                If Not ParameterSetsAreEqual(stg1.GetOutputs(), stg2.GetOutputs()) Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheOutputsOfThisStageHaveChanged)

            Case StageTypes.Skill
                'We already know stage1 and stage2 are of the same type so cast both
                Dim skillStage1 As clsSkillStage = CType(stg1, clsSkillStage)
                Dim skillStage2 As clsSkillStage = CType(stg2, clsSkillStage)

                If skillStage1.SkillId <> skillStage2.SkillId Then diffs.Add(
                 My.Resources.frmProcessComparison_SkillUsedInThisStageHasChangedFrom0To1,
                 skillStage1.SkillId, skillStage2.SkillId)

                If skillStage1.ActionName <> skillStage2.ActionName Then diffs.Add(
                 My.Resources.frmProcessComparison_TheActionUsedInThisStageHasChangedFrom0To1,
                 skillStage1.ActionName, skillStage2.ActionName)

                'if we get to this point then we can assume that the skill itself is
                'unchanged so now we check the inputs/outputs
                If Not ParameterSetsAreEqual(stg1.GetInputs(), stg2.GetInputs()) Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheInputParametersOfThisStageHaveChanged)

                If Not ParameterSetsAreEqual(stg1.GetOutputs(), stg2.GetOutputs()) Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheOutputsOfThisStageHaveChanged)

            Case StageTypes.Decision
                Dim decStg1 As clsDecisionStage = CType(stg1, clsDecisionStage)
                Dim decStg2 As clsDecisionStage = CType(stg2, clsDecisionStage)

                If decStg1.Expression <> decStg2.Expression Then _
                 diffs.Add(My.Resources.frmProcessComparison_CalculationUsedInThisStageHasChanged)

                If decStg1.OnTrue <> decStg2.OnTrue Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheDestinationLinkForTheYesResultIsNoLongerTheSame)

                If decStg1.OnFalse <> decStg2.OnFalse Then _
                 diffs.Add(My.Resources.frmProcessComparison_TheDestinationLinkForTheNoResultIsNoLongerTheSame)

            Case StageTypes.Calculation
                Dim calcStg1 As clsCalculationStage = CType(stg1, clsCalculationStage)
                Dim calcStg2 As clsCalculationStage = CType(stg2, clsCalculationStage)

                If calcStg1.Expression <> calcStg2.Expression Then _
                 diffs.Add(My.Resources.frmProcessComparison_CalculationUsedInThisStageHasChanged)

                If calcStg1.StoreIn <> calcStg2.StoreIn Then _
                 diffs.Add(My.Resources.frmProcessComparison_DestinationForCalculationResultInThisStageHasChanged)

            Case StageTypes.MultipleCalculation

                Dim multiCalcStg1 As clsMultipleCalculationStage = CType(stg1, clsMultipleCalculationStage)
                Dim multiCalcStg2 As clsMultipleCalculationStage = CType(stg2, clsMultipleCalculationStage)

                If multiCalcStg1.Steps.Count <> multiCalcStg2.Steps.Count Then
                    Dim increasedSteps As String = My.Resources.frmProcessComparison_TheNumberOfStepsOnThisStageHasIncreased
                    Dim decreasedSteps As String = My.Resources.frmProcessComparison_TheNumberOfStepsOnThisStageHasDecreased
                    diffs.Add(CStr(IIf(multiCalcStg2.Steps.Count > multiCalcStg1.Steps.Count, increasedSteps, decreasedSteps)))
                Else
                    For i As Integer = 0 To multiCalcStg1.Steps.Count - 1
                        Dim step1 As clsCalcStep = CType(multiCalcStg1.Steps(i), clsCalcStep)
                        Dim step2 As clsCalcStep = CType(multiCalcStg2.Steps(i), clsCalcStep)

                        If step1.Expression <> step2.Expression Then
                            diffs.Add(My.Resources.frmProcessComparison_CalculationUsedInStep0HasChanged, i + 1)
                        End If

                        If step1.StoreIn <> step2.StoreIn Then
                            diffs.Add(My.Resources.frmProcessComparison_LocationCalculationResultInStep0HasChanged, i + 1)
                        End If
                    Next
                End If

            Case StageTypes.Process, StageTypes.SubSheet
                Dim subprocStg1 As clsSubProcessRefStage = CType(stg1, clsSubProcessRefStage)
                Dim subprocStg2 As clsSubProcessRefStage = CType(stg2, clsSubProcessRefStage)

                If subprocStg1.ReferenceId <> subprocStg2.ReferenceId Then
                    Dim processDiff As String = My.Resources.frmProcessComparison_TheProcessReferencedByThisStageHasChanged
                    Dim pageDiff As String = My.Resources.frmProcessComparison_ThePageReferencedByThisStageHasChanged
                    diffs.Add(CStr(IIf(stg1.StageType = StageTypes.Process, processDiff, pageDiff)))
                    ' No point in checking the inputs/outputs if the target that
                    ' it's linking to is changed
                Else
                    If Not ParameterSetsAreEqual(stg1.GetInputs(), stg2.GetInputs()) Then _
diffs.Add(My.Resources.frmProcessComparison_TheInputParametersOfThisStageHaveChanged)

                    If Not ParameterSetsAreEqual(stg1.GetOutputs(), stg2.GetOutputs()) Then _
diffs.Add(My.Resources.frmProcessComparison_TheOutputsOfThisStageHaveChanged)

                End If

            Case StageTypes.Note, StageTypes.ProcessInfo, StageTypes.LoopEnd,
StageTypes.SubSheetInfo, StageTypes.ChoiceEnd, StageTypes.WaitEnd
                ' This is already covered by the Name and Description comparison

            Case StageTypes.LoopStart
                Dim loopStg1 As clsLoopStartStage = CType(stg1, clsLoopStartStage)
                Dim loopStg2 As clsLoopStartStage = CType(stg2, clsLoopStartStage)
                'compare the guids of the collection stages used because the name may
                'have changed but it may still be the same stage.
                If loopStg1.CollectionStageId <> loopStg2.CollectionStageId Then _
diffs.Add(My.Resources.frmProcessComparison_TheCollectionUsedInThisLoopHasChanged)

            Case StageTypes.Data
                Dim dataStg1 As clsDataStage = CType(stg1, clsDataStage)
                Dim dataStg2 As clsDataStage = CType(stg2, clsDataStage)
                'check data type
                If dataStg1.DataType <> dataStg2.DataType Then _
diffs.Add(My.Resources.frmProcessComparison_ThisDataItemHasChangedDataType)

                'check scope
                If dataStg1.IsPrivate <> dataStg2.IsPrivate Then
                    Dim noLongerHidden As String = My.Resources.frmProcessComparison_ThisDataItemIsNoLongerHiddenFromOtherPagesInTheProcess
                    Dim nowHidden As String = My.Resources.frmProcessComparison_ThisDataItemIsNowHiddenFromOtherPagesInTheProcess
                    diffs.Add(CStr(IIf(dataStg1.IsPrivate, noLongerHidden, nowHidden)))
                End If

                'check whether statistic
                If dataStg1.Exposure <> dataStg2.Exposure Then _
diffs.Add(My.Resources.frmProcessComparison_ThisDataItemsExposureTypeHasChanged)

                'Check initalvalue N.B. In theory, InitialValue could throw an exception if a related Environmental variable is has been deleted.
                'Therefore we wrap a try catch around InitialValue so we can continue the comparison
                Dim dataStage1Value As New clsProcessValue()
                Dim dataStage2Value As New clsProcessValue()
                Try
                    dataStage1Value = dataStg1.InitialValue
                Catch ex As Exception
                End Try

                Try
                    dataStage2Value = dataStg2.InitialValue
                Catch ex As Exception
                End Try

                If Not dataStage1Value.Equals(dataStage2Value) Then _
diffs.Add(My.Resources.frmProcessComparison_InitialValueOfTheDataInThisStageHasChanged)

            Case StageTypes.Collection
                Dim collStg1 As clsCollectionStage = CType(stg1, clsCollectionStage)
                Dim collStg2 As clsCollectionStage = CType(stg2, clsCollectionStage)

                Dim def1 As clsCollectionInfo = collStg1.Definition
                Dim def2 As clsCollectionInfo = collStg2.Definition

                'Compare the field definitions
                If def1 Is Nothing Xor def2 Is Nothing Then

                    Dim noLongerDefined As String = My.Resources.frmProcessComparison_TheFieldsAreNoLongerDefined
                    Dim nowDefined As String = My.Resources.frmProcessComparison_TheFieldsAreNowDefined
                    diffs.Add(CStr(IIf(def1 Is Nothing, nowDefined, noLongerDefined)))
                End If

                If def1 IsNot Nothing AndAlso def2 IsNot Nothing Then
                    If def1.Count = def2.Count Then
                        For i As Integer = 0 To def1.Count - 1
                            If def1(i).Name <> def2(i).Name Then _
      diffs.Add(String.Format(My.Resources.frmProcessComparison_TheNameOfField0HasChanged, (i + 1)))

                            If def1.Item(i).DataType <> def2.Item(i).DataType Then _
      diffs.Add(String.Format(My.Resources.frmProcessComparison_TheDatatypeOfField0HasChanged, (i + 1)))
                        Next
                    Else
                        diffs.Add(My.Resources.frmProcessComparison_TheNumberOfFieldsInThisStageHasChanged)
                    End If
                End If

                'Compare the initial values
                Dim coll1 As clsCollection = collStg1.InitialValue.Collection
                Dim coll2 As clsCollection = collStg2.InitialValue.Collection
                If coll1.Count <> coll2.Count Then
                    diffs.Add(My.Resources.frmProcessComparison_TheNumberOfRowsInTheInitialValueForThisStageHasChanged)
                Else
                    For i As Integer = 0 To coll1.Count - 1
                        Dim Row1 As clsCollectionRow = coll1.Row(i)
                        Dim Row2 As clsCollectionRow = coll2.Row(i)

                        'Compare each field name
                        If Row1.FieldNames.Count <> Row2.FieldNames.Count Then
                            diffs.Add(
      My.Resources.frmProcessComparison_Row0NowContainsADefinitionFor1FieldsWhereasPreviouslyItContainedADefinitionFor2,
      i + 1, Row2.FieldNames.Count, Row1.FieldNames.Count)
                            Exit For
                        Else
                            For Each fld As String In Row1.FieldNames
                                If Not Row2.Contains(fld) Then
                                    diffs.Add(
              My.Resources.frmProcessComparison_TheInitialValuesOnRow0HaveUndergoneANameChange, i + 1)
                                    Exit For
                                ElseIf Not Row1(fld).Equals(Row2(fld)) Then
                                    diffs.Add(
              My.Resources.frmProcessComparison_TheValueOfItem0OnRow1HasChanged,
              fld, i + 1)
                                End If
                            Next fld
                        End If
                    Next i

                End If

            Case StageTypes.Code
                Dim codeStg1 As clsCodeStage = CType(stg1, clsCodeStage)
                Dim codeStg2 As clsCodeStage = CType(stg2, clsCodeStage)

                If codeStg1.CodeText <> codeStg2.CodeText Then _
diffs.Add(My.Resources.frmProcessComparison_CodeTextForThisStageHasChanged)


                If Not ParameterSetsAreEqual(codeStg1.GetInputs(), codeStg2.GetInputs()) Then _
diffs.Add(My.Resources.frmProcessComparison_TheInputParametersOfThisStageHaveChanged)

                If Not ParameterSetsAreEqual(codeStg1.GetOutputs(), codeStg2.GetOutputs()) Then _
diffs.Add(My.Resources.frmProcessComparison_TheOutputsOfThisStageHaveChanged)

            Case StageTypes.ChoiceStart
                Dim choiceStg1 As clsChoiceStartStage = CType(stg1, clsChoiceStartStage)
                Dim choiceStg2 As clsChoiceStartStage = CType(stg2, clsChoiceStartStage)

                If choiceStg1.Choices.Count <> choiceStg2.Choices.Count Then
                    diffs.Add(My.Resources.frmProcessComparison_TheNumberOfChoicesInThisStageHasChanged)
                Else
                    For i As Integer = 0 To choiceStg1.Choices.Count - 1
                        Dim ch1 As clsChoice = choiceStg1.Choices(i)
                        Dim ch2 As clsChoice = choiceStg2.Choices(i)

                        If ch1.Name <> ch2.Name Then diffs.Add(
  My.Resources.frmProcessComparison_TheNameOfChoice0HasChanged, i + 1)

                        If ch1.Expression <> ch2.Expression Then diffs.Add(
  My.Resources.frmProcessComparison_TheExpressionOfChoice0HasChanged, i + 1)

                        If ch1.LinkTo <> ch2.LinkTo Then diffs.Add(
  My.Resources.frmProcessComparison_TheLinkedStageInChoice0HasChanged, i + 1)

                    Next
                End If

            Case StageTypes.Navigate

                Dim navStg1 As clsNavigateStage = CType(stg1, clsNavigateStage)
                Dim navStg2 As clsNavigateStage = CType(stg2, clsNavigateStage)

                If navStg1.Steps.Count <> navStg2.Steps.Count Then
                    Dim increasedSteps As String = My.Resources.frmProcessComparison_TheNumberOfStepsOnThisStageHasIncreased
                    Dim decreasedSteps As String = My.Resources.frmProcessComparison_TheNumberOfStepsOnThisStageHasDecreased
                    diffs.Add(CStr(IIf(navStg2.Steps.Count > navStg1.Steps.Count, increasedSteps, decreasedSteps)))
                Else
                    For i As Integer = 0 To navStg1.Steps.Count - 1
                        Dim step1 As clsNavigateStep = CType(navStg1.Steps(i), clsNavigateStep)
                        Dim step2 As clsNavigateStep = CType(navStg2.Steps(i), clsNavigateStep)

                        'Compare the target element chosen
                        If step1.ElementId <> step2.ElementId Then
                            diffs.Add(My.Resources.frmProcessComparison_TheTargetElementOfStep0HasChanged, i + 1)
                            Continue For
                        End If

                        'If the same then compare the navigational action chosen
                        If step1.Action Is Nothing Xor step2.Action Is Nothing Then
                            Dim addedTo As String = My.Resources.frmProcessComparison_TheActionHasBeenAddedToStep0InThisStage
                            Dim removedFrom As String = My.Resources.frmProcessComparison_TheActionHasBeenRemovedFromStep0InThisStage
                            diffs.Add(String.Format(CStr(IIf(step1.Action Is Nothing, addedTo, removedFrom)), i + 1))
                            Continue For

                        ElseIf step1.Action IsNot Nothing AndAlso step1.ActionId <> step2.ActionId Then
                            diffs.Add(My.Resources.frmProcessComparison_TheActionOfStep0InThisStageHasChanged, i + 1)
                            Continue For

                        End If

                        'If the same then compare the parameters
                        If step1.Parameters.Count = step2.Parameters.Count Then
                            For j As Integer = 0 To step1.Parameters.Count - 1
                                If step1.Parameters(j).Expression <> step2.Parameters(j).Expression Then _
          diffs.Add(My.Resources.frmProcessComparison_TheValueSuppliedToParameter0OfRow1HasChanged, j + 1, i + 1)
                            Next
                        Else
                            diffs.Add(My.Resources.frmProcessComparison_TheNumberOfParametersSuppliedToRow0HasChanged, i + 1)
                        End If

                        'If the same then compare each argument
                        If step1.ArgumentValues.Count <> step2.ArgumentValues.Count Then
                            diffs.Add(My.Resources.frmProcessComparison_TheNumberOfArgumentsToStep0HasChanged, i + 1)
                        Else
                            Dim deletedArgs As New clsSet(Of String)(step1.ArgumentValues.Keys)
                            deletedArgs.Subtract(step2.ArgumentValues.Keys)

                            Dim addedArgs As New clsSet(Of String)(step2.ArgumentValues.Keys)
                            addedArgs.Subtract(step1.ArgumentValues.Keys)

                            ' We already know that they have the same number of args,
                            ' so if we have added args, we must have deleted args too
                            If addedArgs.Count > 0 Then diffs.Add(My.Resources.frmProcessComparison_TheArgumentsToStep0HaveChangedRemoved1Added2,
      i + 1, Join(deletedArgs, My.Resources.frmProcessComparison_Comma), Join(addedArgs, My.Resources.frmProcessComparison_Comma))

                            ' With that done, we only need to report on values which
                            ' exist in both argument lists that have changed
                            For Each key1 As String In step1.ArgumentValues.Keys

                                ' Try and see if this is in step2 first
                                Dim val2 As String = Nothing
                                If Not step2.ArgumentValues.TryGetValue(key1, val2) _
          Then Continue For

                                ' It's in step2, get the value from step1 and test it
                                Dim val1 As String = step1.ArgumentValues(key1)

                                If val1 <> val2 Then diffs.Add(
          My.Resources.frmProcessComparison_ArgumentFor0OnRow1HasChangedValue,
          key1, i + 1)

                            Next
                        End If
                    Next
                End If


            Case StageTypes.Read, StageTypes.Write
                Dim appStg1 As clsAppStage = CType(stg1, clsAppStage)
                Dim appStg2 As clsAppStage = CType(stg2, clsAppStage)

                If appStg1.Steps.Count <> appStg2.Steps.Count Then
                    diffs.Add(My.Resources.frmProcessComparison_TheNumberOfStepsInThisStageHasChanged)
                Else
                    For i As Integer = 0 To appStg1.Steps.Count - 1
                        Dim step1 As clsStep = CType(appStg1.Steps(i), clsStep)
                        Dim step2 As clsStep = CType(appStg2.Steps(i), clsStep)

                        'Compare the target element chosen
                        If step1.ElementId <> step2.ElementId Then
                            diffs.Add(String.Format(My.Resources.frmProcessComparison_TheTargetElementOfStep0HasChanged, (i + 1)))
                        End If

                        'compare the parameters
                        If step1.Parameters.Count = step2.Parameters.Count Then
                            For j As Integer = 0 To step1.Parameters.Count - 1
                                Dim Param1 As clsApplicationElementParameter = step1.Parameters(j)
                                Dim Param2 As clsApplicationElementParameter = step2.Parameters(j)

                                If Param1.Expression <> Param2.Expression Then
                                    diffs.Add(String.Format(My.Resources.frmProcessComparison_TheValueSuppliedToParameter0OfRow1HasChanged, (j + 1), (i + 1)))
                                End If
                            Next j
                        Else
                            diffs.Add(String.Format(My.Resources.frmProcessComparison_TheNumberOfParametersSuppliedToRow0HasChanged2, (i + 1)))
                        End If

                        'Compare the stage chosen, where appropriate
                        If stg1.StageType = StageTypes.Read AndAlso
  CType(step1, clsReadStep).Stage <> CType(step2, clsReadStep).Stage Then
                            diffs.Add(String.Format(My.Resources.frmProcessComparison_TheStageMappingOfStep0HasChanged, (i + 1)))
                        End If

                        'Compare the expression supplied, where appropriate
                        If stg1.StageType = StageTypes.Write AndAlso
  CType(step1, clsWriteStep).Expression <> CType(step2, clsWriteStep).Expression Then
                            diffs.Add(String.Format(My.Resources.frmProcessComparison_TheExpressionUsedInStep0HasChanged, (i + 1)))
                        End If

                    Next i
                End If

            Case StageTypes.WaitStart

                Dim waitStg1 As clsWaitStartStage = CType(stg1, clsWaitStartStage)
                Dim waitStg2 As clsWaitStartStage = CType(stg2, clsWaitStartStage)

                'Compare timeout
                If waitStg1.Timeout <> waitStg2.Timeout Then
                    diffs.Add(My.Resources.frmProcessComparison_TheTimeoutOnThisStageHasChanged)
                End If

                'Compare each step
                If waitStg1.Choices.Count <> waitStg2.Choices.Count Then
                    diffs.Add(My.Resources.frmProcessComparison_TheNumberOfWaitOptionsOnThisStageHasChanged)
                Else
                    For i As Integer = 0 To waitStg1.Choices.Count - 1
                        Dim w1 As clsWaitChoice = CType(waitStg1.Choices(i), clsWaitChoice)
                        Dim w2 As clsWaitChoice = CType(waitStg2.Choices(i), clsWaitChoice)

                        'Compare the target element
                        If w1.ElementID <> w2.ElementID Then
                            diffs.Add(String.Format(My.Resources.frmProcessComparison_TheTargetElementOfRow0HasChanged, (i + 1)))
                            Continue For
                        End If

                        'If the same element then compare parameters
                        If w1.Parameters Is Nothing Xor w2.Parameters Is Nothing Then
                            diffs.Add(String.Format(My.Resources.frmProcessComparison_TheParametersForRow0HaveChanged, (i + 1)))
                            Continue For
                        End If

                        If w1.Parameters IsNot Nothing Then
                            If w1.Parameters.Count <> w2.Parameters.Count Then
                                diffs.Add(String.Format(My.Resources.frmProcessComparison_GetDifferences_TheNumberOfParametersSuppliedToRow0HasChanged, (i + 1)))
                            Else
                                For j As Integer = 0 To w1.Parameters.Count - 1
                                    If w1.Parameters(j).Expression <> w2.Parameters(j).Expression Then
                                        diffs.Add(String.Format(My.Resources.frmProcessComparison_TheValueSuppliedToParameter0OfRow1HasChanged, (j + 1), (i + 1)))
                                    End If
                                Next j
                            End If
                        End If

                        'Compare the condition
                        If w1.Condition Is Nothing Xor w2.Condition Is Nothing Then _
  diffs.Add(String.Format(My.Resources.frmProcessComparison_TheConditionOfRow0HasChanged, (i + 1)))

                        If w1.Condition IsNot Nothing AndAlso w2.Condition IsNot Nothing _
  AndAlso w1.Condition.ID <> w2.Condition.ID Then _
  diffs.Add(String.Format(My.Resources.frmProcessComparison_TheConditionOfRow0HasChanged, (i + 1)))

                        'Check the expected reply
                        If w1.ExpectedReply <> w2.ExpectedReply Then _
  diffs.Add(String.Format(My.Resources.frmProcessComparison_TheExpectedValueOfTheConditionOfRow0HasChanged, (i + 1)))

                    Next i
                End If

                'Check for any renamed elements (used as the link labels)
                Dim oldElements = waitStg1.GetElementsUsed()
                For Each elementID In waitStg2.GetElementsUsed()
                    If oldElements.Contains(elementID) Then
                        If waitStg1.Process.ParentObject Is Nothing AndAlso waitStg2.Process.ParentObject Is Nothing Then
                            Dim e1 = waitStg1.Process.ApplicationDefinition.FindElement(elementID)
                            Dim e2 = waitStg2.Process.ApplicationDefinition.FindElement(elementID)
                            If e1.Name <> e2.Name Then
                                diffs.Add(String.Format(My.Resources.frmProcessComparison_TheElement0HasBeenRenamedTo1, e1.Name, e2.Name))
                            End If
                        End If
                    End If
                Next
        End Select

        Dim linkStg1 As clsLinkableStage = TryCast(stg1, clsLinkableStage)
        Dim linkStg2 As clsLinkableStage = TryCast(stg2, clsLinkableStage)

        If linkStg1 IsNot Nothing AndAlso linkStg2 IsNot Nothing _
         AndAlso linkStg1.OnSuccess <> linkStg2.OnSuccess Then
            stg1.LinkColour = StageLinkMode.DestinationChanged
            stg2.LinkColour = StageLinkMode.DestinationChanged
            diffs.Add(My.Resources.frmProcessComparison_TheDestinationOfThisStageHasChanged)
        End If

        Return diffs

    End Function


    ''' <summary>
    ''' Compares two sets of parameters and determines if they are equal.
    ''' </summary>
    ''' <param name="params1">The first set of parameters.</param>
    ''' <param name="params2">The second set of parameters.</param>
    ''' <returns>Returns true if the sets of parameters are equal.</returns>
    Private Function ParameterSetsAreEqual(
     ByVal params1 As List(Of clsProcessParameter),
     ByVal params2 As List(Of clsProcessParameter)) As Boolean
        If params1 Is Nothing Then Return (params2 Is Nothing)
        If params2 Is Nothing Then Return (params1 Is Nothing)
        If params1.Count <> params2.Count Then Return False

        For Each pm As clsProcessParameter In params1
            If Not ParameterContainedIn(params2, pm) Then Return False
        Next

        Return True
    End Function


    ''' <summary>
    ''' Determines whether an array of parameters contains the given parameter.
    ''' </summary>
    ''' <param name="list">The list to be searched.</param>
    ''' <param name="p">The parameter sought.</param>
    ''' <returns>Returns true if the parameter is found in the array.</returns>
    Private Function ParameterContainedIn(ByVal list As List(Of clsProcessParameter), ByVal p As clsProcessParameter) As Boolean
        For Each pm As clsProcessParameter In list
            If ParametersAreEqual(pm, p) Then Return True
        Next
        Return False
    End Function


    ''' <summary>
    ''' Determines if two process parameters are equal.
    ''' </summary>
    ''' <param name="p">The first parameter</param>
    ''' <param name="q">The second parameter</param>
    ''' <returns>Returns true if the parameters are equal.</returns>
    Private Function ParametersAreEqual(ByVal p As clsProcessParameter, ByVal q As clsProcessParameter) As Boolean
        If Not p.GetDataType = q.GetDataType Then Return False
        If Not p.GetMap = q.GetMap Then Return False
        If Not p.Name = q.Name Then Return False
        Return True
    End Function


#End Region

#Region "Misc UI methods"

    Private mLastLeftHandHighlightedStage As clsProcessStage
    Private mLastLeftHandHighlightedStageDisplayMode As StageShowMode
    Private Sub HighlightLeftStage(ByVal Stage As clsProcessStage)
        If Me.mLastLeftHandHighlightedStage IsNot Nothing Then
            Me.mLastLeftHandHighlightedStage.DisplayMode = Me.mLastLeftHandHighlightedStageDisplayMode
        End If
        If Stage IsNot Nothing Then
            mLastLeftHandHighlightedStageDisplayMode = Stage.DisplayMode
            Stage.DisplayMode = StageShowMode.Search_Highlight
            Me.CtlProcessViewer1.InvalidateView()
        End If
        Me.mLastLeftHandHighlightedStage = Stage
    End Sub

    Private mLastRightHandHighlightedStage As clsProcessStage
    Private mLastRightHandHighlightedStageDisplayMode As StageShowMode
    Private Sub HighlightRightStage(ByVal Stage As clsProcessStage)
        If Me.mLastRightHandHighlightedStage IsNot Nothing Then
            Me.mLastRightHandHighlightedStage.DisplayMode = Me.mLastRightHandHighlightedStageDisplayMode
        End If
        If Stage IsNot Nothing Then
            mLastRightHandHighlightedStageDisplayMode = Stage.DisplayMode
            Stage.DisplayMode = StageShowMode.Search_Highlight
            Me.CtlProcessViewer2.InvalidateView()
        End If
        Me.mLastRightHandHighlightedStage = Stage
    End Sub

#End Region

#Region "Methods For Interfacing With ProcessViewer Control"

    ''' <summary>
    ''' See comment on interface definition.
    ''' </summary>
    ''' <param name="sMessage"></param>
    Public Overloads Sub SetStatusBarText(ByVal sMessage As String) Implements IProcessViewingForm.SetStatusBarText
        Me.SetStatusBarText(sMessage, Me.DefaultStatusBarMessageDuration)
    End Sub

    ''' <summary>
    ''' See comment on interface definition.
    ''' </summary>
    ''' <param name="sMessage"></param>
    Public Overloads Sub SetStatusBarText(ByVal sMessage As String, ByVal iMessageDuration As Integer) Implements IProcessViewingForm.SetStatusBarText
        'set the message on the status bar
        Me.stsBarTextArea.Text = sMessage

        'if imessageduration i=0 then we should not remove the message after any time.
        If Not iMessageDuration = 0 Then
            'first round the messageduration up to the next multiple of 100
            iMessageDuration = CInt(Math.Ceiling(iMessageDuration / 100) * 100)

            'set up the timer so that the message gets removed after the requested time.
            Me.miNumberOfTicksToWaitBeforeClearingStatusBarMessage = iMessageDuration \ 100
            Me.miNumberOfStatusBarTimerTicks = 0
            Me.timStatusBarTimer.Interval = 100
            Me.timStatusBarTimer.Enabled = True
        Else
            Me.timStatusBarTimer.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' See comment on interface definition.
    ''' </summary>
    Public Sub ClearStatusBarText() Implements IProcessViewingForm.ClearStatusBarText
        Me.stsBarTextArea.Text = ""
    End Sub

    ''' <summary>
    ''' Gets the current difference from the difference navigator and makes sure
    ''' it is visible on each process viewer diagram.
    ''' </summary>
    Private Sub ShowCurrentDifference()
        Dim stage As clsProcessStage = Me.mDifferencesNavigator.GetCurrentDifference
        If stage Is Nothing Then Exit Sub

        'References to the left- and right-hand processes and viewing controls
        Dim Process1 As clsProcess = Me.mProcessLeft
        Dim Process2 As clsProcess = Me.mProcessRight
        Dim ProcessViewer1 As ctlProcessViewer = Me.CtlProcessViewer1
        Dim ProcessViewer2 As ctlProcessViewer = Me.CtlProcessViewer2

        'if the stage only exists in the second process we swap these references round
        Dim StageExistsInFirstProcess As Boolean = Not Me.mProcessLeft.GetStage(stage.GetStageID) Is Nothing
        If Not StageExistsInFirstProcess Then
            Process1 = Me.mProcessRight
            Process2 = Me.mProcessLeft
            ProcessViewer1 = Me.CtlProcessViewer2
            ProcessViewer2 = Me.CtlProcessViewer1
        End If

        'Focus stage in first diagram. We know this stage must exist because we
        'swapped them over where necessary to ensure this condition
        Me.HighlightLeftStage(stage)
        ProcessViewer1.ShowStage(stage.GetStageID)

        'Focus same stage in second diagram if poss
        Dim StageExistsInSecondProcess As Boolean = Not Process2.GetStage(stage.GetStageID) Is Nothing
        If StageExistsInSecondProcess Then
            Me.HighlightRightStage(Process2.GetStage(stage.GetStageID))
            ProcessViewer2.ShowStage(stage.GetStageID)
        Else
            'When stage does not exist we focus same sheet in second diagram
            'at same coordinate location (if poss)
            If Not Process2.GetSubSheetName(stage.GetSubSheetID) = "" Then
                Process2.SetActiveSubSheet(stage.GetSubSheetID)
                Process2.SetCameraLocation(Process1.GetCameraLocation)
            End If
        End If

        ProcessViewer1.InvalidateView()
        ProcessViewer2.InvalidateView()
    End Sub

    Public Sub HideSearchControl(ByVal SearchControl As DiagramSearchToolstrip) Implements IProcessViewingForm.HideSearchControl
        'TODO
    End Sub

    ''' <summary>
    ''' Shows the stage with the specified ID in each of
    ''' the two process viewing controls.
    ''' </summary>
    ''' <param name="gStageID">The ID of the stage to be shown.</param>
    Public Sub ShowStage(ByVal gStageID As Guid)
        Dim Stage1 As clsProcessStage = mProcessLeft.GetStage(gStageID)
        If Stage1 IsNot Nothing Then
            Me.CtlProcessViewer1.ShowStage(Stage1)
            Me.HighlightLeftStage(Stage1)
        Else
            UserMessage.ShowFloating(Me.CtlProcessViewer1, ToolTipIcon.Info, My.Resources.frmProcessComparison_CanNotNavigateToStage, My.Resources.frmProcessComparison_TheRequestedStageDoesNotExistOnThisSide, Point.Empty)
        End If
        Dim Stage2 As clsProcessStage = mProcessRight.GetStage(gStageID)
        If Stage2 IsNot Nothing Then
            Me.CtlProcessViewer2.ShowStage(Stage2)
            Me.HighlightRightStage(Stage2)
        Else
            UserMessage.ShowFloating(Me.CtlProcessViewer2, ToolTipIcon.Info, My.Resources.frmProcessComparison_CanNotNavigateToStage, My.Resources.frmProcessComparison_TheRequestedStageDoesNotExistOnThisSide, Point.Empty)
        End If
    End Sub
#End Region

#Region "Menu Updating"

    ''' <summary>
    ''' Updates the form menus to the appropriate settings for the
    ''' selected process viewer.
    ''' </summary>
    Private Sub UpdateMenus()
        ZoomUpdate(SelectedProcessViewer.Process.ZoomPercent)
        SelectedProcessViewer.GetGridInfo(chkGrid.Checked, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Updates the zoom level.
    ''' </summary>
    ''' <param name="percent">the new level</param>
    Public Sub ZoomUpdate(ByVal percent As Integer) Implements IProcessViewingForm.ZoomUpdate
        z400.Checked = (percent = 400)
        z200.Checked = (percent = 200)
        z150.Checked = (percent = 150)
        z100.Checked = (percent = 100)
        z75.Checked = (percent = 75)
        z50.Checked = (percent = 50)
        z25.Checked = (percent = 25)

        zDyn.Text = CStr(percent)
    End Sub

#End Region

#End Region

#Region "Differences Enumerator Class"

    ''' Project  : Automate
    ''' Class    : frmProcessComparison.DifferencesEnumerator
    '''
    ''' <summary>
    ''' Provides a means of navigating a list of differences between the two
    ''' processes. Stages are added to the list, and by calling first, next, prev,
    ''' last, etc differences can be extracted as the Current().
    ''' </summary>
    Private Class DifferencesEnumerator

#Region "Members"

        ''' <summary>
        ''' The differences available.
        ''' </summary>
        Private mDifferencesList As SortedList

        ''' <summary>
        ''' The first of the two processes under comparision
        ''' </summary>
        Private mProcess1 As clsProcess

        ''' <summary>
        ''' The second of the two processes under comparision
        ''' </summary>
        Private mProcess2 As clsProcess

        ''' <summary>
        ''' A list of the subsheets contained in the two processes. The sheets
        ''' here may not necessarily be contained by each process, because some
        ''' of the sheets may be new or deleted.
        '''
        ''' If none of the sheets have been re-ordered between the two processes,
        ''' a logical order will be clear. Otherwise we stick to the order of the
        ''' sheets in the first process, with new sheets from the second process
        ''' being inserted after the first sheet in the second process found to their
        ''' left which also exists in the first process.
        ''' </summary>
        Private mgSheetList As Guid()

        ''' <summary>
        ''' The index of the current difference.
        ''' </summary>
        Private miCurrentIndex As Integer

        ''' <summary>
        ''' The thread used for instanciating the differences list.
        ''' </summary>
        Private mDifferencesThread As Thread

        ''' <summary>
        ''' Dummy object for locking access to the differences list..
        ''' </summary>
        Private mDifferencesListLock As New Object

        ''' <summary>
        ''' A temporary list into which stages are added when the internal list
        ''' is not yet ready. Such stages are dealt with by the thread
        ''' mTempListTidyingThread.
        ''' </summary>
        Private mTempDifferencesList As New List(Of clsProcessStage)

        ''' <summary>
        ''' Dummy object for locking access to mTempDifferencesList
        ''' </summary>
        Private mTempListLock As New Object

        ''' <summary>
        ''' The thread used for tidying up the temporary list of added stages
        ''' after the internal list is finally prepared.
        ''' </summary>
        Private mTempListTidyingThread As Thread

        Public ReadOnly Property DifferenceCount() As Integer
            Get
                If Me.mDifferencesList IsNot Nothing Then
                    Return Me.mDifferencesList.Count
                Else
                    Return 0
                End If
            End Get
        End Property
#End Region

#Region "Constructor"

        Public Sub New(ByVal Process1 As clsProcess, ByVal Process2 As clsProcess)
            Me.mProcess1 = Process1
            Me.mProcess2 = Process2

            'we do internal processing on a separate thread so as not
            'to slow clients down to much. If they request information before
            'the processing is complete then they will have to wait for this
            'thread to complete.
            Me.mDifferencesThread = New Thread(AddressOf PrepareInternalList)
            Me.mDifferencesThread.Start()

            'This thread copies across any differences added before
            'the internal list was ready
            Me.mTempListTidyingThread = New Thread(AddressOf TidyUpTempList)
            Me.mTempListTidyingThread.Start()
        End Sub

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' Prepares the internal list for use by creating the sheetslist and
        ''' instanciating the internal list member.
        ''' </summary>
        Private Sub PrepareInternalList()
            Try
                Monitor.Enter(Me.mDifferencesListLock)
                Me.GetSheetsList()
                Me.mDifferencesList = New SortedList(New StagePositionComparer(Me.mgSheetList, Me.mProcess1, Me.mProcess2))
            Finally
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Sub

        ''' <summary>
        ''' Merges the sheetlists from the two processes to define an order.
        ''' Resulting list is stored in mgSheetList. See doc for that member
        ''' for a description of how the list order is determined.
        ''' </summary>
        Private Sub GetSheetsList()
            'get the subsheets of each process
            Dim objList1 As Generic.List(Of clsProcessSubSheet)
            Dim objList2 As Generic.List(Of clsProcessSubSheet)
            Try
                Monitor.Enter(Me.mProcess1)
                objList1 = Me.mProcess1.SubSheets
            Finally
                Monitor.Exit(Me.mProcess1)
            End Try
            Try
                Monitor.Enter(Me.mProcess2)
                objList2 = Me.mProcess2.SubSheets
            Finally
                Monitor.Exit(Me.mProcess2)
            End Try

            'make two arrays containing the sheet IDs
            Dim list1 As New List(Of Guid)
            Dim list2 As New List(Of Guid)
            For Each objSub As clsProcessSubSheet In objList1
                list1.Add(objSub.ID)
            Next
            For Each objSub As clsProcessSubSheet In objList2
                list2.Add(objSub.ID)
            Next

            'merge the second list into the first
            Dim LastItemFoundExistingInBothLists As Guid
            While list2.Count > 0
                Dim ItemExistsInFirstList As Boolean = list1.IndexOf(list2(0)) > -1
                If Not ItemExistsInFirstList Then
                    If LastItemFoundExistingInBothLists.Equals(Guid.Empty) Then
                        'append to end of list
                        'List1.Insert(0, List2(0))
                        list1.Add(list2(0))
                        LastItemFoundExistingInBothLists = list2(0)
                    Else
                        'insert immediately after the last item found
                        list1.Insert(list1.IndexOf(LastItemFoundExistingInBothLists) + 1, list2(0))
                        LastItemFoundExistingInBothLists = list2(0)
                    End If
                Else
                    'The item does exist in the first list as well.
                    'Remember this item before we remove it. New items in the
                    'second list which don't exist in the first will be inserted
                    'immediately after this one.
                    LastItemFoundExistingInBothLists = list2.Item(0)
                End If
                list2.RemoveAt(0)
            End While

            'store the list in the member mgSheetList
            Me.mgSheetList = list1.ToArray()
        End Sub

        ''' <summary>
        ''' Waits for the thread mDifferencesThread to complete before
        ''' copying all items in the mTempDifferencesList to the
        ''' real list.
        ''' </summary>
        Private Sub TidyUpTempList()
            While Me.mDifferencesThread.IsAlive
                Thread.Sleep(50)
            End While

            Try
                Monitor.Enter(Me.mTempListLock)
                Monitor.Enter(Me.mDifferencesListLock)
                For Each st As clsProcessStage In Me.mTempDifferencesList
                    Try
                        Monitor.Enter(st)
                        If Not Me.mDifferencesList.Contains(st.GetStageID) Then
                            Me.mDifferencesList.Add(st.GetStageID, st)
                        End If
                    Finally
                        Monitor.Exit(st)
                    End Try
                Next
                'Allow garbage collector to free up this memory
                Me.mTempDifferencesList = Nothing

            Finally
                Monitor.Exit(Me.mTempListLock)
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Sub

#End Region



        ''' <summary>
        ''' Adds a difference to the list.
        ''' </summary>
        ''' <param name="Stage">The stage to add, which corresponds to a difference
        ''' between the two processes.</param>
        Public Sub AddDifference(ByVal Stage As clsProcessStage)
            'Here we speed things up a bit. The client may wish to add
            'a difference before we have finished the internal
            'preparation thread. Here we take the stage anyway
            'without delaying the client. The stage sits in an
            'internal temporary list until the real list is
            'ready to accept items.
            'Once the internal processing thread is complete,
            'the items in the temporary list are tidied up by the
            'thread

            Try
                Monitor.Enter(Stage)
                'Lock the templistnow before testing thread state in order
                'to avoid race condition in which threadstate
                'is running when we make the check, but then
                'by the time we have managed to lock the templist
                'the thread has ended and has discarded the reference
                'to the templist.
                Monitor.Enter(Me.mTempListLock)

                If Me.mDifferencesThread.ThreadState = ThreadState.Stopped Then
                    'we can add directly - no problem
                    Try
                        Monitor.Enter(Me.mDifferencesListLock)

                        If Not Me.mDifferencesList.Contains(Stage.GetStageID) Then
                            Me.mDifferencesList.Add(Stage.GetStageID, Stage)
                        End If
                    Finally
                        Monitor.Exit(Me.mDifferencesListLock)
                    End Try
                Else
                    'we have to add to temporary list
                    Me.mTempDifferencesList.Add(Stage)
                End If

            Finally
                Monitor.Exit(Me.mTempListLock)
                Monitor.Exit(Stage)
            End Try

        End Sub

#Region "Public Navigation Methods"

        ''' <summary>
        ''' Moves to the first difference in the list. This difference can then
        ''' be accessed via GetCurrentDifference()
        ''' </summary>
        ''' <returns>Returns true if successfully navigated to the first
        ''' difference; false otherwise.</returns>
        Public Function MoveToFirstDifference() As Boolean
            Try
                Monitor.Enter(Me.mDifferencesListLock)
                If (Not Me.mDifferencesList Is Nothing) AndAlso (Me.mDifferencesList.Count > 0) Then
                    Me.miCurrentIndex = 0
                    Return True
                Else
                    Return False
                End If
            Finally
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Function

        ''' <summary>
        ''' Moves to the next difference in the list. This difference can then
        ''' be accessed via GetCurrentDifference()
        ''' </summary>
        ''' <returns>Returns true if successfully navigated to the next
        ''' difference; false otherwise.</returns>
        Public Function MoveToNextDifference() As Boolean
            Try
                Monitor.Enter(Me.mDifferencesListLock)
                If (Not Me.mDifferencesList Is Nothing) AndAlso (Me.mDifferencesList.Count > 0) AndAlso (Me.miCurrentIndex < Me.mDifferencesList.Count - 1) Then
                    Me.miCurrentIndex += 1
                    Return True
                Else
                    Return False
                End If
            Finally
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Function

        ''' <summary>
        ''' Moves to the previous difference in the list. This difference can then
        ''' be accessed via GetCurrentDifference()
        ''' </summary>
        ''' <returns>Returns true if successfully navigated to the previous
        ''' difference; false otherwise.</returns>
        Public Function MoveToPrevDifference() As Boolean
            Try
                Monitor.Enter(Me.mDifferencesListLock)
                If (Not Me.mDifferencesList Is Nothing) AndAlso (Me.mDifferencesList.Count > 0) AndAlso (Me.miCurrentIndex > 0) Then
                    Me.miCurrentIndex -= 1
                    Return True
                Else
                    Return False
                End If
            Finally
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Function

        ''' <summary>
        ''' Moves to the last difference in the list. This difference can then
        ''' be accessed via GetCurrentDifference()
        ''' </summary>
        ''' <returns>Returns true if successfully navigated to the last
        ''' difference; false otherwise.</returns>
        Public Function MoveToLastDifference() As Boolean
            Try
                Monitor.Enter(Me.mDifferencesListLock)
                If (Not Me.mDifferencesList Is Nothing) AndAlso (Me.mDifferencesList.Count > 0) Then
                    Me.miCurrentIndex = Me.mDifferencesList.Count - 1
                    Return True
                Else
                    Return False
                End If
            Finally
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Function

        ''' <summary>
        ''' Gets the stage which corresponds to the current difference, or nothing
        ''' if there is no current difference.
        ''' </summary>
        ''' <returns>As summary.</returns>
        Public Function GetCurrentDifference() As clsProcessStage
            Try
                Monitor.Enter(Me.mDifferencesListLock)
                If Me.miCurrentIndex > -1 AndAlso Me.miCurrentIndex <= Me.DifferenceCount - 1 Then
                    Dim stage As clsProcessStage = CType(Me.mDifferencesList.GetByIndex(Me.miCurrentIndex), clsProcessStage)
                    Debug.Assert(Not stage Is Nothing)
                    Return stage
                Else
                    Return Nothing
                End If
            Finally
                Monitor.Exit(Me.mDifferencesListLock)
            End Try
        End Function

#End Region

    End Class

#End Region

#Region "Stage Position Comparer"

    ''' Project  : Automate
    ''' Class    : frmProcessComparison.StagePositionComparer
    '''
    ''' <summary>
    ''' Compares two stages from two related process  (eg two versions of the same
    ''' process or a process which was cloned from the other). It regards one stage
    ''' as less than another if the subsheet order is lower than the other. This
    ''' sheet order is defined by the list passed in to the constructor.
    '''
    ''' When stages reside on the same sheet, one stage is regarded as lower
    ''' than another if it is further left than the other. When two stages are
    ''' on the same xcoord the one with the lower y coord is regarded as lower.
    ''' </summary>
    Private Class StagePositionComparer
        Implements IComparer

        ''' <summary>
        ''' The sheetlist we use as reference to compare. The lower the index in this
        ''' list, the lower the score.
        ''' </summary>
        Private mgSheetList As Guid()

        Private mProcess1 As clsProcess
        Private mProcess2 As clsProcess

        Public Sub New(ByVal SheetList As Guid(), ByVal Process1 As clsProcess, ByVal Process2 As clsProcess)
            Me.mgSheetList = SheetList
            Me.mProcess1 = Process1
            Me.mProcess2 = Process2
        End Sub


        ''' <summary>
        ''' Compares two stages in the process, regarding one stage as less than another
        ''' if the subsheet order is lower than the other. When stages reside on the same
        ''' sheet, one stage is regarded as lower than another if it is further left than
        ''' the other. When two stages are on the same xcoord the one with the lower y
        ''' coord is regarded as lower.
        ''' </summary>
        ''' <param name="xStageID">The first clsProcessStage object.</param>
        ''' <param name="yStageID">The second clsProcessStage object.</param>
        ''' <returns>Returns 0 if stages are equal according to the criteria above,
        ''' </returns>
        Public Function Compare(ByVal xStageID As Object, ByVal yStageID As Object) As Integer Implements System.Collections.IComparer.Compare

            'check for silly clients
            Debug.Assert(TypeOf xStageID Is Guid)
            Debug.Assert(TypeOf yStageID Is Guid)

            'the two stages we are comparing
            Dim xStage As clsProcessStage
            Dim yStage As clsProcessStage

            'No need to lock the processes, since neither thread
            'will modify any data. These objects can be treated as
            'immutable.

            xStage = mProcess1.GetStage(CType(xStageID, Guid))
            yStage = mProcess1.GetStage(CType(yStageID, Guid))
            If xStage Is Nothing Then xStage = mProcess2.GetStage(CType(xStageID, Guid))
            If yStage Is Nothing Then yStage = mProcess2.GetStage(CType(yStageID, Guid))

            'this should never happen. Let's check
            Debug.Assert(Not xStage Is Nothing)
            Debug.Assert(Not yStage Is Nothing)

            'find the index of the subsheet of each of the stages
            Dim iXSheetIndex As Integer
            Dim iySheetIndex As Integer
            Try
                Monitor.Enter(xStage)
                Monitor.Enter(yStage)

                iXSheetIndex = Array.IndexOf(Me.mgSheetList, xStage.GetSubSheetID)
                iySheetIndex = Array.IndexOf(Me.mgSheetList, yStage.GetSubSheetID)

                'Note: when either stage is on main page, the index will be -1 because
                'the GetSubSheets method does not include the Main Page. This is ok
                'and does not affect the code below.

                'compare by sheet, x coord, then y coord
                If iXSheetIndex = iySheetIndex Then
                    If xStage.GetDisplayX = yStage.GetDisplayX Then
                        If xStage.GetDisplayY = yStage.GetDisplayY Then
                            'same page and coordinates. Compare by stage ID
                            If CType(xStageID, Guid).Equals(CType(yStageID, Guid)) Then
                                'same stageid, compare process
                                If xStage.Process Is yStage.Process Then
                                    Return 0
                                Else
                                    Return -1
                                End If
                            Else
                                Return CType(xStageID, Guid).CompareTo(CType(yStageID, Guid))
                            End If
                        Else
                            Return xStage.GetDisplayY.CompareTo(yStage.GetDisplayY)
                        End If
                    Else
                        Return xStage.GetDisplayX.CompareTo(yStage.GetDisplayX)
                    End If
                Else
                    Return iXSheetIndex.CompareTo(iySheetIndex)
                End If

            Finally
                Monitor.Exit(xStage)
                Monitor.Exit(yStage)
            End Try

        End Function
    End Class

#End Region


    Private Sub mnuExportLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExportLeft.Click
        Dim WizardType As frmWizard.WizardType
        Dim Action As String
        If CtlProcessViewer1.ModeIsObjectStudio Then
            WizardType = frmWizard.WizardType.BusinessObject
            Action = Permission.ObjectStudio.ExportBusinessObject
        Else
            WizardType = frmWizard.WizardType.Process
            Action = Permission.ProcessStudio.ExportProcess
        End If
        If User.Current.HasPermission(Action) Then
            Dim f As New frmProcessExport(Me.mProcessIdLeft, mXMLLeft, WizardType)
            f.SetEnvironmentColoursFromAncestor(Me)
            f.AuditComments = String.Format(My.Resources.frmProcessComparison_VersionChosenFromHistoryAsOf0, mDateLeft)
            f.ShowInTaskbar = False
            f.ShowDialog()
            f.Dispose()
        Else
            UserMessage.Show(String.Format(My.Resources.frmProcessComparison_YouDoNotHaveSufficientPermissionToPerformTheAction0PleaseContactYour1Administra, Action, ApplicationProperties.ApplicationName))
        End If
    End Sub

    Private Sub mnuExportRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExportRight.Click
        Dim WizardType As frmWizard.WizardType
        Dim Action As String
        If CtlProcessViewer2.ModeIsObjectStudio Then
            WizardType = frmWizard.WizardType.BusinessObject
            Action = Permission.ObjectStudio.ExportBusinessObject
        Else
            WizardType = frmWizard.WizardType.Process
            Action = Permission.ProcessStudio.ExportProcess
        End If
        If User.Current.HasPermission(Action) Then
            Dim f As New frmProcessExport(Me.mProcessIdRight, Me.mXMLRight, WizardType)
            f.SetEnvironmentColoursFromAncestor(Me)
            f.AuditComments = String.Format(My.Resources.frmProcessComparison_VersionChosenFromHistoryAsOf0, mDateRight)
            f.ShowInTaskbar = False
            f.ShowDialog()
            f.Dispose()
        Else
            UserMessage.Show(String.Format(My.Resources.frmProcessComparison_YouDoNotHaveSufficientPermissionToPerformTheAction0PleaseContactYour1Administra, Action, ApplicationProperties.ApplicationName))
        End If
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Private Sub frmProcessComparison_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        mParent.ClosedForm(Me)
    End Sub
End Class
