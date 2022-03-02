Option Strict On

Imports System.IO
Imports System.Drawing.Printing
Imports AutomateControls
Imports BluePrism.Images
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Extensions
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages

Imports Actions = BluePrism.AutomateAppCore.clsValidationInfo.Actions
Imports ResourceDBStatus = BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus
Imports LocaleTools
Imports BluePrism.AutomateProcessCore.ProcessLoading
Imports System.Globalization
Imports BluePrism.Utilities.Functional
Imports System.Linq
Imports BluePrism.Core.Extensions
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models
Imports NLog

''' Project  : Automate
''' Class    : ctlProcessViewer
''' 
''' <summary>
''' Provides a picture box with scrollbars etc suitable for viewing a process.
''' </summary>
Friend Class ctlProcessViewer
    Inherits UserControl

#Region " Windows Form Designer generated code "

    Friend WithEvents mnuTabNew As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabDuplicate As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabCopy As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabCut As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabPaste As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabRename As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabDelete As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabSep3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuTabPublish As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabSep4 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuTabManage As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuTabSep2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuFindReferences As System.Windows.Forms.ToolStripMenuItem

    Private mbSuppressProcessDisposal As Boolean
    ''' <summary>
    ''' When false, the process being displayed will be disposed
    ''' of when this viewer is disposed. Defaults to false.
    ''' </summary>
    Public Property SuppressProcessDisposal() As Boolean
        Get
            Return Me.mbSuppressProcessDisposal
        End Get
        Set(ByVal value As Boolean)
            Me.mbSuppressProcessDisposal = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether the process should run at near full speed.
    ''' </summary>
    Public Property RunAtNearFullSpeed As Boolean

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
            If Not Me.mbSuppressProcessDisposal Then
                If Me.mProcess IsNot Nothing Then
                    Me.mProcess.Dispose()
                End If
            End If

            If Not mRenderer Is Nothing Then mRenderer.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents tcPages As TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents ScrollV As System.Windows.Forms.VScrollBar
    Friend WithEvents ScrollH As System.Windows.Forms.HScrollBar
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents Timer2 As System.Windows.Forms.Timer
    Friend WithEvents mnuTabContext As ContextMenuStrip
    Friend WithEvents cmbTabSelector As AutomateControls.StyledComboBox
    Friend WithEvents pbview As System.Windows.Forms.PictureBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessViewer))
        Me.ScrollV = New System.Windows.Forms.VScrollBar()
        Me.ScrollH = New System.Windows.Forms.HScrollBar()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        Me.mnuTabContext = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuTabNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabDuplicate = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabRename = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabSep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuTabDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabCut = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabCopy = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabPaste = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabSep3 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuTabPublish = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFindReferences = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTabSep4 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuTabManage = New System.Windows.Forms.ToolStripMenuItem()
        Me.pbview = New System.Windows.Forms.PictureBox()
        Me.tcPages = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.cmbTabSelector = New AutomateControls.StyledComboBox()
        Me.mnuTabContext.SuspendLayout()
        CType(Me.pbview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tcPages.SuspendLayout()
        Me.SuspendLayout()
        '
        'ScrollV
        '
        resources.ApplyResources(Me.ScrollV, "ScrollV")
        Me.ScrollV.Name = "ScrollV"
        '
        'ScrollH
        '
        resources.ApplyResources(Me.ScrollH, "ScrollH")
        Me.ScrollH.Name = "ScrollH"
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 1000
        '
        'Timer2
        '
        Me.Timer2.Enabled = True
        Me.Timer2.Interval = 2000
        '
        'mnuTabContext
        '
        Me.mnuTabContext.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuTabNew, Me.mnuTabDuplicate, Me.mnuTabRename, Me.mnuTabSep2, Me.mnuTabDelete, Me.mnuTabCut, Me.mnuTabCopy, Me.mnuTabPaste, Me.mnuTabSep3, Me.mnuTabPublish, Me.mnuFindReferences, Me.mnuTabSep4, Me.mnuTabManage})
        Me.mnuTabContext.Name = "mnuTabContext"
        resources.ApplyResources(Me.mnuTabContext, "mnuTabContext")
        '
        'mnuTabNew
        '
        Me.mnuTabNew.Image = Global.AutomateUI.My.Resources.ToolImages.New_16x16
        Me.mnuTabNew.Name = "mnuTabNew"
        resources.ApplyResources(Me.mnuTabNew, "mnuTabNew")
        '
        'mnuTabDuplicate
        '
        Me.mnuTabDuplicate.Name = "mnuTabDuplicate"
        resources.ApplyResources(Me.mnuTabDuplicate, "mnuTabDuplicate")
        '
        'mnuTabRename
        '
        Me.mnuTabRename.Name = "mnuTabRename"
        resources.ApplyResources(Me.mnuTabRename, "mnuTabRename")
        '
        'mnuTabSep2
        '
        Me.mnuTabSep2.Name = "mnuTabSep2"
        resources.ApplyResources(Me.mnuTabSep2, "mnuTabSep2")
        '
        'mnuTabDelete
        '
        Me.mnuTabDelete.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.mnuTabDelete.Name = "mnuTabDelete"
        resources.ApplyResources(Me.mnuTabDelete, "mnuTabDelete")
        '
        'mnuTabCut
        '
        Me.mnuTabCut.Image = Global.AutomateUI.My.Resources.ToolImages.Cut_16x16
        Me.mnuTabCut.Name = "mnuTabCut"
        resources.ApplyResources(Me.mnuTabCut, "mnuTabCut")
        '
        'mnuTabCopy
        '
        Me.mnuTabCopy.Image = Global.AutomateUI.My.Resources.ToolImages.Copy_16x16
        Me.mnuTabCopy.Name = "mnuTabCopy"
        resources.ApplyResources(Me.mnuTabCopy, "mnuTabCopy")
        '
        'mnuTabPaste
        '
        Me.mnuTabPaste.Image = Global.AutomateUI.My.Resources.ToolImages.Paste_16x16
        Me.mnuTabPaste.Name = "mnuTabPaste"
        resources.ApplyResources(Me.mnuTabPaste, "mnuTabPaste")
        '
        'mnuTabSep3
        '
        Me.mnuTabSep3.Name = "mnuTabSep3"
        resources.ApplyResources(Me.mnuTabSep3, "mnuTabSep3")
        '
        'mnuTabPublish
        '
        Me.mnuTabPublish.Name = "mnuTabPublish"
        resources.ApplyResources(Me.mnuTabPublish, "mnuTabPublish")
        '
        'mnuFindReferences
        '
        Me.mnuFindReferences.Name = "mnuFindReferences"
        resources.ApplyResources(Me.mnuFindReferences, "mnuFindReferences")
        '
        'mnuTabSep4
        '
        Me.mnuTabSep4.Name = "mnuTabSep4"
        resources.ApplyResources(Me.mnuTabSep4, "mnuTabSep4")
        '
        'mnuTabManage
        '
        Me.mnuTabManage.Name = "mnuTabManage"
        resources.ApplyResources(Me.mnuTabManage, "mnuTabManage")
        '
        'pbview
        '
        Me.pbview.AllowDrop = True
        resources.ApplyResources(Me.pbview, "pbview")
        Me.pbview.BackColor = System.Drawing.Color.FromArgb(CType(CType(254, Byte), Integer), CType(CType(252, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.pbview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pbview.Name = "pbview"
        Me.pbview.TabStop = False
        '
        'tcPages
        '
        resources.ApplyResources(Me.tcPages, "tcPages")
        Me.tcPages.ContextMenuStrip = Me.mnuTabContext
        Me.tcPages.Controls.Add(Me.TabPage1)
        Me.tcPages.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed
        Me.tcPages.HotTrack = True
        Me.tcPages.Name = "tcPages"
        Me.tcPages.SelectedIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.TabPage1, "TabPage1")
        Me.TabPage1.Name = "TabPage1"
        '
        'cmbTabSelector
        '
        Me.cmbTabSelector.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbTabSelector.Checkable = False
        Me.cmbTabSelector.DisplayMember = "Value"
        Me.cmbTabSelector.DropDownWidth = 250
        Me.cmbTabSelector.FormattingEnabled = True
        resources.ApplyResources(Me.cmbTabSelector, "cmbTabSelector")
        Me.cmbTabSelector.Name = "cmbTabSelector"
        Me.cmbTabSelector.ValueMember = "Value"
        '
        'ctlProcessViewer
        '
        Me.Controls.Add(Me.ScrollH)
        Me.Controls.Add(Me.ScrollV)
        Me.Controls.Add(Me.pbview)
        Me.Controls.Add(Me.cmbTabSelector)
        Me.Controls.Add(Me.tcPages)
        Me.Name = "ctlProcessViewer"
        resources.ApplyResources(Me, "$this")
        Me.mnuTabContext.ResumeLayout(False)
        CType(Me.pbview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tcPages.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Events "

    ''' <summary>
    ''' Event raised when the process in this viewer is saved to the database.
    ''' </summary>
    Public Event Saved As ProcessEventHandler

    Public Event SelectDependency(d As clsProcessDependency)

    Public Event PropertySaved As EventHandler

#End Region

#Region " Constructors "

    Public Sub New()
        MyBase.New()

        InitializeComponent()
        tcPages.ImageList = ImageLists.Publishing_16x16

        mPropertyWindows = New Dictionary(Of Guid, frmProperties)
    End Sub

#End Region

#Region " Member Variables "

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()
    Private mClosed As Boolean = False
    Private ReadOnly mPropertyWindows As IDictionary(Of Guid, frmProperties)

    Private WithEvents mAutoSaver As clsAutoSaver

    ''' <summary>
    ''' The source object of any link being drawn,
    ''' if any.
    ''' </summary>
    ''' <remarks>This is usually nothing, except when a link
    ''' is being drawn and has not yet been finished.</remarks>
    Public LinkSource As ILinkable


    ''' <summary>
    ''' Private member to store public property ClipBoardProcess()
    ''' </summary>
    Private mClipBoardProcess As clsProcess
    ''' <summary>
    ''' A process that is part-way through being pasted.
    ''' This process has yet to be positioned, and will
    ''' be rendered in a lighter colour to indicate that
    ''' it is not yet part of the main process.
    ''' </summary>
    ''' <remarks>See also clipboardProcessLocation</remarks>
    Public Property ClipboardProcess() As clsProcess
        Get
            Return mClipBoardProcess
        End Get
        Set(ByVal value As clsProcess)
            mClipBoardProcess = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property ClipboardProcessLocation()
    ''' </summary>
    Private mClipboardProcessLocation As PointF
    ''' <summary>
    ''' The location at which the clipboard process should
    ''' be rendered, in world coordinates.
    ''' </summary>
    ''' <value>The first stage in the clipboard process
    ''' will be rendered at this world location; all other
    ''' stages will be rendered relative to this stage's location.</value>
    Public Property ClipboardProcessLocation() As PointF
        Get
            If Me.mSnapToGrid Then
                Return SnapTo(Me.mClipboardProcessLocation)
            Else
                Return mClipboardProcessLocation
            End If
        End Get
        Set(ByVal value As PointF)
            mClipboardProcessLocation = value
        End Set
    End Property




    Private WithEvents mobjPrintDocument As PrintDocument
    Public mobjCalculationZoom As frmCalculationZoom
    Private mnuContextCalculationZoom As ToolStripMenuItem


    Private ReadOnly Property ciGridSize() As Integer
        Get
            Return clsProcess.GridUnitSize
        End Get
    End Property

    ''' <summary>
    ''' Determines if the viewer was opened internally whilst stepping into a
    ''' subprocess/object.
    ''' </summary>
    Public Property OpenedAsDebugSubProcess() As Boolean
        Get
            Return mOpenedAsDebugSubProcess
        End Get
        Set(ByVal value As Boolean)
            mOpenedAsDebugSubProcess = value
        End Set
    End Property
    Private mOpenedAsDebugSubProcess As Boolean

    ''' <summary>
    ''' Position of mouse pointer in world coordinates. Updated by events.
    ''' </summary>
    Private mpMousePos As PointF

    ''' <summary>
    ''' Position where the mouse button was last pressed down, in world coordinates.
    ''' Used to determine behaviour when button goes back up!
    ''' </summary>
    Private mpMouseDownWorldPos As PointF

    ''' <summary>
    ''' The point at which the mouse button was depressed,
    ''' in local coordinates relative to the pbview object.
    ''' </summary>
    Private mpMouseDownLocalPos As Point

    ''' <summary>
    ''' The position of the camera, in world coordinates
    ''' as recorded at the last moment when the mouse
    ''' button was depressed.
    ''' </summary>
    Private mpMouseDownCameraPos As PointF

    ''' <summary>
    '''  Minimum size of a rubber band box - below this it doesn't exist which
    ''' means you can place objects while moving the mouse slightly
    ''' </summary>
    Private msngMinRubberBandSize As Single = 8

    ''' <summary>
    ''' Private member to store public property MouseDragging
    ''' </summary>
    ''' <remarks></remarks>
    Private mbMouseDragging As Boolean
    ''' <summary>
    ''' True if we are between mouse down and mouse up events, and therefore
    ''' have the mouse captured.
    ''' </summary>
    Private Property MouseDragging() As Boolean
        Get
            Return mbMouseDragging
        End Get
        Set(ByVal value As Boolean)
            mbMouseDragging = value
            pbview.Capture = value
        End Set
    End Property

    Private mbMouseDragSelected As Boolean

    ''' <summary>
    ''' When dragging an object, this is the X offset from its centre that we
    ''' originally clicked on.
    ''' </summary>
    Private msngMouseDragXOffset As Single
    ''' <summary>
    ''' When dragging an object, this is the Y offset from its centre that we
    ''' originally clicked on.
    ''' </summary>
    Private msngMouseDragYOffset As Single

    ''' <summary>
    ''' When dragging an object, this is the stage that we originally clicked on (if
    ''' any).
    ''' </summary>
    Private mobjMouseDragStage As clsProcessStage

    Private mbMouseWheelEnabled As Boolean

    ''' <summary>
    ''' 'When dragging an object, this specifies whether we have hold of the
    ''' object itself, or just a corner. Values are as follows:
    '''
    '''         0 - Normal, just the object
    '''         1 - Top left
    '''         2 - Top right
    '''         3 - Bottom right
    '''         4 - Bottom left
    ''' </summary>
    Private miMouseDragCorner As Integer

    ''' <summary>
    ''' The process object being drawn by this control.
    ''' </summary>
    Public ReadOnly Property Process() As clsProcess
        Get
            Return mProcess
        End Get
    End Property
    Private WithEvents mProcess As clsProcess

    ''' <summary>
    ''' The rendering object used for drawing objects and stages on our display.
    ''' </summary>
    Private mRenderer As clsRenderer

    Public ReadOnly Property Renderer() As clsRenderer
        Get
            Return mRenderer
        End Get
    End Property

    ''' <summary>
    ''' Initially false, but true when form has been fully loaded.
    ''' </summary>
    Private mbLoaded As Boolean

    ''' <summary>
    ''' True if the user has already said that changes should be discarded. This can
    ''' happen when PromptToSave is called from the Debug Hook and the user chooses
    ''' Discard, in which case the form will be closed later, but the user should not
    ''' be prompted again.
    ''' </summary>
    Private mChangesDiscarded As Boolean = False

    ''' <summary>
    ''' used for context menu 'goto page' and 'goback'
    ''' </summary>
    Private mobjStageToReturnTo As clsProcessStage
    ''' <summary>
    ''' used for context menu 'goto page' and 'goback'
    ''' </summary>
    Private mbExistsPageRefToReturnTo As Boolean

    Private mbRunningStep As Boolean

    ''' <summary>
    ''' the instance of frmapplication that we were all born from
    ''' </summary>
    Private mSuperParent As frmApplication

    ''' <summary>
    ''' The form in which this control is contained.
    ''' 
    ''' WARNING: This may be one of frmprocess or frmProcessComparison so be careful
    ''' how you cast it (use typeof to check first, or enhance the
    ''' IProcessViewingForm interface).
    ''' </summary>
    Private mobjParentForm As Form

    ''' <summary>
    ''' Used for printing
    ''' </summary>
    Private mrectPrintExtent As Rectangle
    ''' <summary>
    ''' Used for printing
    ''' </summary>
    Private msngPrintLeft As Single
    ''' <summary>
    ''' Used for printing
    ''' </summary>
    Private msngPrintTop As Single

    ''' <summary>
    ''' The tool to display as the current one in the user interface. Determines
    ''' the meaning of mouse clicks.
    ''' </summary>
    Private mTool As StudioTool

    ''' <summary>
    ''' The available tools to the user. See the tooltips for each one for a
    ''' descirption of what they  are for.
    ''' </summary>
    Public Enum StudioTool
        Pointer = 1
        Action = 2
        Decision = 3
        Link = 4
        Anchor = 5
        Data = 6
        Note = 7
        Start = 8
        [End] = 9
        Zoom = 10
        Calculation = 11

        Collection = 12
        [Loop] = 13
        Process = 14
        Page = 15
        Choice = 16
        Wait = 32
        Read = 64
        Write = 128
        Code = 256
        Navigate = 512
        Alert = 1024
        Pan = 2048
        Exception = 4096
        Recover = 8192
        [Resume] = 16384
        Block = 32768
        MultipleCalculation = 65536
        Skill = 131072
    End Enum

    <Serializable>
    Public Class StageDropContainer
        Public Property Tool As StudioTool
        Public Property Context As Object
    End Class

    '''' <summary>
    '''' The mode being used in this control.
    '''' </summary>
    Private mMode As ProcessViewMode = ProcessViewMode.EditProcess

    ''' <summary>
    ''' The ID of the process currently being edited. Used for session logging and 
    ''' for saving to the database.
    ''' </summary>
    Public ReadOnly Property ProcessID() As Guid
        Get
            Return mProcessId
        End Get
    End Property
    Private mProcessId As Guid

    ''' <summary>
    ''' Same as mgProcessID except for new processes it goes here.
    ''' </summary>
    Public mgNewProcessID As Guid

    ''' <summary>
    ''' The name of the currently active process. We have an extra reference here 
    ''' rather than just looking up the process object because when cloning, the 
    ''' two may not match. NB it wasn't me who designed it this way :P
    ''' </summary>
    Private msName As String

    ''' <summary>
    ''' The name of the currently active process. We have an extra reference here 
    ''' rather than just looking up the process object because when cloning, the 
    ''' two may not match. NB it wasn't me who designed it this way :P
    ''' </summary>
    Private msDescription As String

    Private msSaveInGroup As Guid
    ''' <summary>
    ''' Determines whether we should snap stages to a grid in the user interface.
    ''' </summary>
    Private mSnapToGrid As Boolean = True
    ''' <summary>
    ''' Determines whether gridlines should be shown.
    ''' </summary>
    Private mShowGridLines As Boolean = True
    ''' <summary>
    ''' Determines whether the user would like to see a dynamic cursor - ie one which
    ''' changes with the current tool.
    ''' </summary>
    Private mbDynamicCursor As Boolean = True

    ''' <summary>
    ''' XML read in during process comparison
    ''' </summary>
    Private msXML As String

    ''' <summary>
    ''' This value tells us whether or not we should prompt the user to
    ''' provide a comment summarising the changes that they have made when they
    ''' save a process.
    ''' </summary>
    Private mbSummariesEnabled As Boolean


    ''' <summary>
    ''' The greatest width in pixels that tooltips may occupy on screen.
    ''' </summary>
    Private MaxToolTipWidth As Integer = 70

    ''' <summary>
    ''' The cursor currently being displayed to the user.
    ''' </summary>
    Private myCurrentPointer As Cursor = Cursors.Default
    ''' <summary>
    ''' 
    ''' </summary>
    Private mySizingPointer As Cursor


    ''' <summary>
    ''' If run in mode NewProcess then we are interested in whether the process has
    ''' yet been saved in order to know, for example:
    ''' 1. whether we should create a process lock after the first save
    ''' 2. whether it is sensible to prompt the user for an edit summary
    ''' 3. if it's possible to generate an element usage report
    ''' </summary>
    Public ReadOnly Property HasNeverEverBeenSaved() As Boolean
        Get
            Return mThisProcessHasNeverEverBeenSaved
        End Get
    End Property
    Private mThisProcessHasNeverEverBeenSaved As Boolean = False

    ''' <summary>
    ''' The  attributes of the current process as they were at the time of the
    ''' last save. This tells us whether we need to record an audit event
    ''' for the change of attributes when the process is saved.
    ''' </summary>
    Private mProcessAttributesAtLastSave As ProcessAttributes

    ''' <summary>
    ''' A hashtable instantiated in the mouse move handler and used in the 
    ''' subsequent mouse up handler to create an undo point.
    ''' </summary>
    ''' <remarks></remarks>
    Private mhtUndoMovedStages As Hashtable

    ''' <summary>
    ''' A hashtable instantiated in the mouse move handler and used in the 
    ''' subsequent mouse up handler to create an undo point.
    ''' </summary>
    ''' <remarks></remarks>
    Private mhtUndoResizedStages As Hashtable


    ''' <summary>
    ''' A boolean indicating weather a node was moved during the mouse move event.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbNodeMoved As Boolean

    ''' <summary>
    ''' Refers to the frmProcess from which the 'go to object studio' context 
    ''' menu was clicked. Used to create a similar 'return to process/object studio'
    ''' context menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private moProcessStudioReference As frmProcess

    ''' <summary>
    ''' Refers to the subsheet from which the 'go to object studio' context
    ''' menu was clicked. Used to activate the activate the subsheet when the 
    ''' 'return to process studio' context menu item is clicked.
    ''' </summary>
    ''' <remarks></remarks>
    Private msProcessStudioReferenceSubSheetName As String

    ''' <summary>
    ''' Indicates the current user has lost the lock on the object/process
    ''' This could happen if another user edits the process and takes over lock
    ''' </summary>
    ''' <remarks></remarks>
    Private mLostLock As Boolean = False

    Private mParentObjectID As Guid = Guid.Empty

    ''' <summary>
    ''' Format for text on the tab page tabs
    ''' </summary>
    ''' <remarks></remarks>
    Private ReadOnly mFormat As StringFormat = New StringFormat With {
        .FormatFlags = StringFormatFlags.NoWrap,
        .Trimming = StringTrimming.None,
        .Alignment = StringAlignment.Center,
        .LineAlignment = StringAlignment.Center
        }
    ''' <summary>
    ''' Brush used to paint unselected tabs in tab pages
    ''' </summary>
    ''' <remarks></remarks>
    Private ReadOnly mControlBrush As Brush = New SolidBrush(SystemColors.ControlLight)
    ''' <summary>
    ''' Brush used to paint selected tabs in tab pages
    ''' </summary>
    ''' <remarks></remarks>
    Private ReadOnly mBluePrismColorBrush As Brush = New SolidBrush(ColourScheme.BluePrismControls.BluePrismBlue)
    ''' <summary>
    ''' Image used to indicate page is published
    ''' </summary>
    ''' <remarks></remarks>
    Private ReadOnly mPublishedImage As Image = ComponentImages.Structure_16x16
    ''' <summary>
    ''' the currently selected tab
    ''' </summary>
    ''' <remarks></remarks>
    Private mCurrentSelectedTab As Integer


#End Region

#Region "Properties"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property AutoSaver() As clsAutoSaver
        Get
            Return mAutoSaver
        End Get
        Set(ByVal value As clsAutoSaver)
            mAutoSaver = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ParentObject() As KeyValuePair(Of Guid, String)
        Get
            Return New KeyValuePair(Of Guid, String)(mParentObjectID,
             If(mProcess Is Nothing, Nothing, mProcess.ParentObject))
        End Get
        Set(ByVal value As KeyValuePair(Of Guid, String))
            mParentObjectID = value.Key
            If mProcess IsNot Nothing Then mProcess.ParentObject = value.Value
        End Set
    End Property

    ''' <summary>
    ''' Determines if the underlying process can be debugged or not.
    ''' </summary>
    ''' <value>True if debugging is possible/allowed.</value>
    Public ReadOnly Property CanDebug() As Boolean
        Get
            ' If we're comparing or previewing, we cannot debug
            If mMode.HasAnyFlag(
             ProcessViewMode.Compare Or ProcessViewMode.Preview) Then Return False

            If mThisProcessHasNeverEverBeenSaved Then
                'Really, the answer at this point should be false, because you can't
                'debug when the process has never been saved. (See bug #2084, also
                'referenced elsewhere in this file). However, we are going to say that
                'you CAN debug, in order to be able to present the message box saying
                'why you can't, when the user clicks the button. The alternative is to
                'have the debug options greyed out, and they don't know why.
                Return True
            End If

            'By default, we'll say yes...
            Return True
        End Get
    End Property

    ''' <summary>
    ''' The current view mode of this process viewer.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
     DefaultValue(ProcessViewMode.EditProcess), Description(
     "The mode in which the process is being viewed")>
    Public Property ViewMode() As ProcessViewMode
        Get
            Return mMode
        End Get
        Set(ByVal value As ProcessViewMode)
            mMode = value
        End Set
    End Property

    ''' <summary>
    ''' Determines whether stages should be aligned to an imaginary grid. User
    ''' interface feature.
    ''' </summary>
    ''' <value>Value.</value>
    Public Property SnapToGrid() As Boolean
        Get
            Return mSnapToGrid
        End Get
        Set(ByVal Value As Boolean)
            mSnapToGrid = Value
        End Set
    End Property

    ''' <summary>
    ''' Determines whether gridlines should be displayed to the user.
    ''' </summary>
    ''' <value></value>
    Public Property ShowGridLines() As Boolean
        Get
            Return mShowGridLines
        End Get
        Set(ByVal Value As Boolean)
            mShowGridLines = Value
        End Set
    End Property

    ''' <summary>
    ''' Determines whether the mouse wheel should be enabled.
    ''' </summary>
    ''' <value></value>
    Public Property MouseWheelEnabled() As Boolean
        Get
            Return mbMouseWheelEnabled
        End Get
        Set(ByVal Value As Boolean)
            mbMouseWheelEnabled = Value
        End Set
    End Property

    Public ReadOnly Property HasPropertyWindowsOpen As Boolean
        Get
            Return mPropertyWindows.Count > 0
        End Get
    End Property

#End Region

#Region "event code"

#Region "Process Events"

    ''' <summary>
    ''' Updates the edit menu options on the parent form (eg
    ''' cut copy paste, etc).
    ''' </summary>
    Private Sub UpdateParentEditMenuOptions()
        Dim CutCopyDeleteEnabled As Boolean
        Dim PasteEnabled As Boolean = mProcess.CanPaste
        Select Case mProcess.GetSelectionType
            Case clsProcessSelection.SelectionType.None, clsProcessSelection.SelectionType.ChoiceNode
                CutCopyDeleteEnabled = False
            Case Else
                CutCopyDeleteEnabled = True
        End Select

        Dim objParent As frmProcess = TryCast(Me.mobjParentForm, frmProcess)
        If objParent IsNot Nothing Then
            objParent.SetEditMenuOptions(CutCopyDeleteEnabled, PasteEnabled)
        End If
    End Sub

    Private Sub HandleProcessSelectionChanged() Handles mProcess.SelectionChanged
        If TypeOf Me.mobjParentForm Is frmProcess And ModeIsEditable() Then
            UpdateParentEditMenuOptions()

            Dim objStage As clsProcessStage = Nothing
            Dim objSelection As clsProcessSelectionContainer = mProcess.SelectionContainer
            If objSelection.Count = 1 Then
                If objSelection.PrimarySelection.mtType = clsProcessSelection.SelectionType.Stage Then
                    objStage = mProcess.GetStage(objSelection.PrimarySelection.mgStageID)
                End If
            End If
            UpdateExpressionEditRow(objStage)
        End If
    End Sub

    ''' <summary>
    ''' Updates the expression edit row using details from the given stage.
    ''' </summary>
    ''' <param name="objStage">The Stage to use for the update</param>
    ''' <remarks></remarks>
    Private Sub UpdateExpressionEditRow(ByVal objStage As clsProcessStage)
        Dim objParent As frmProcess = TryCast(Me.mobjParentForm, frmProcess)
        If objParent IsNot Nothing Then
            objParent.UpdateExpressionEditRow(objStage)
        End If
    End Sub

    Private Sub HandleProcessUndoRedoStatusChanged(ByVal NewState As clsUndoRedoManager.ManagerStates) Handles mProcess.UndoRedoStatusChanged
        If TypeOf Me.mobjParentForm Is frmProcess Then

            Dim CanUndo As Boolean = (NewState And clsUndoRedoManager.ManagerStates.CanUndo) > 0
            Dim CanRedo As Boolean = (NewState And clsUndoRedoManager.ManagerStates.CanRedo) > 0

            CType(Me.mobjParentForm, frmProcess).SetEditMenuUndoRedoEnabled(CanUndo, CanRedo)
        End If
    End Sub

#End Region

#Region "drag events"

    Private Sub ctlProcessViewer_DragOver(sender As Object, e As DragEventArgs) Handles pbview.DragOver

        If mobjMouseDragStage Is Nothing Then Return

        Dim mousePos As Point = pbview.PointToClient(New Point(e.X, e.Y))
        Dim loc As PointF = mRenderer.CoordinateTranslator.TranslateScreenPointToWorldPoint(mousePos, pbview.ClientSize)
        loc = SnapTo(loc)
        mobjMouseDragStage.SetDisplayX(loc.X)
        mobjMouseDragStage.SetDisplayY(loc.Y)
        InvalidateView()
    End Sub

    Private Sub pbview_DragDrop(sender As Object, e As DragEventArgs) Handles pbview.DragDrop

        If Not e.Data.GetDataPresent(GetType(StageDropContainer)) Then Return

        Dim stageContainer = CType(e.Data.GetData(GetType(StageDropContainer)), StageDropContainer)

        Dim mousePos As Point = pbview.PointToClient(New Point(e.X, e.Y))
        Dim loc As PointF = mRenderer.CoordinateTranslator.TranslateScreenPointToWorldPoint(mousePos, pbview.ClientSize)
        mProcess.SelectAtPosition(loc, False, True)
        loc = SnapTo(loc)

        OnToolPressed(loc, stageContainer.Tool, stageContainer.Context)

        DragFinished()
    End Sub

    Private Sub pbview_DragEnter(sender As Object, e As DragEventArgs) Handles pbview.DragEnter

        If Not e.Data.GetDataPresent(GetType(StageDropContainer)) Then
            e.Effect = DragDropEffects.None
            DragFinished()
            Exit Sub
        End If

        Dim stageContainer = CType(e.Data.GetData(GetType(StageDropContainer)), StageDropContainer)
        If ToolAllowsSelection(stageContainer.Tool) Then
            e.Effect = DragDropEffects.Copy
            Dim stageType As StageTypes = StageFromTool(stageContainer.Tool)
            mobjMouseDragStage = mProcess.CreateStage(stageType)
            mobjMouseDragStage.InitialName = mProcess.GetUniqueStageID(stageType)
            ToolDragging = True
        End If

    End Sub

    Public Property ToolDragging As Boolean

    Private Sub pbview_DragLeave(sender As Object, e As EventArgs) Handles pbview.DragLeave
        DragFinished()
    End Sub

    Private Sub DragFinished()
        mobjMouseDragStage = Nothing
        ToolDragging = False
        InvalidateView()
    End Sub

#End Region

#Region "paint events"

    Private Sub pbView_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pbview.Paint
        If DesignMode Then Exit Sub

        mRenderer.UpdateView(e.Graphics, Me.pbview.ClientRectangle, mProcess, True, True)
        If Me.ClipboardProcess IsNot Nothing Then
            Try
                Dim ClipboardStages As List(Of clsProcessStage) = ClipboardProcess.GetStages()
                If ClipboardStages.Count > 0 Then
                    Dim FirstStage As clsProcessStage = Me.ClipboardProcess.GetStages()(0)
                    If FirstStage IsNot Nothing Then
                        ClipboardProcess.SetActiveSubSheet(mProcess.GetActiveSubSheet)
                        For Each objstage As clsProcessStage In ClipboardProcess.GetStages
                            objstage.SetSubSheetID(mProcess.GetActiveSubSheet)
                        Next
                        mRenderer.CoordinateTranslator.OriginOffset = (New SizeF(Me.SnapTo(Me.ClipboardProcess.GetStages()(0).Location)) - New SizeF(Me.ClipboardProcessLocation))
                        mRenderer.AlphaTransparency = 150
                        mRenderer.UpdateView(e.Graphics, Me.pbview.ClientRectangle, Me.ClipboardProcess, False, False, True)
                        mRenderer.AlphaTransparency = 255
                        mRenderer.CoordinateTranslator.OriginOffset = SizeF.Empty
                    End If
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_PasteOperationCancelledDueToAFatalRenderingError0, ex.Message))
                Me.CancelPaste(False)
            End Try
        End If
    End Sub

    Private Sub TabControlDrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs) Handles tcPages.DrawItem

        mCurrentSelectedTab = e.Index

        Dim tabControl = CType(sender, TabControl)
        Dim textBrush As SolidBrush

        If e.Index = tabControl.SelectedIndex Then
            e.Graphics.FillRectangle(mBluePrismColorBrush, e.Bounds)
            textBrush = New SolidBrush(Color.White)
        Else
            e.Graphics.FillRectangle(mControlBrush, e.Bounds)
            textBrush = New SolidBrush(Color.Black)
        End If
        Dim tabBounds = tabControl.GetTabRect(e.Index)
        Dim textRectangle = tabBounds

        If Not String.IsNullOrEmpty(tabControl.TabPages(e.Index).ImageKey) Then
            Dim imageRectangle = New Rectangle(tabBounds.X + tcPages.Padding.X, tabBounds.Y + ((tabBounds.Height \ 2) - (mPublishedImage.Height \ 2)), mPublishedImage.Width, mPublishedImage.Height)
            e.Graphics.DrawImage(mPublishedImage, imageRectangle)
            textRectangle = New Rectangle(textRectangle.X + tcPages.Padding.X + mPublishedImage.Width, textRectangle.Y, textRectangle.Width - tcPages.Padding.X - mPublishedImage.Width, textRectangle.Height)
        End If
        e.Graphics.DrawString(tabControl.TabPages(e.Index).Text, e.Font, textBrush, textRectangle, mFormat)
    End Sub
#End Region

#Region "scrollbar events"

    Private Sub scrollH_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ScrollH.ValueChanged
        mProcess.SetCameraX(ScrollH.Value)
        InvalidateView()
    End Sub

    Private Sub scrollV_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ScrollV.ValueChanged
        mProcess.SetCameraY(ScrollV.Value)
        InvalidateView()
    End Sub

#End Region

#Region "keyboard events"


    Protected Overrides Function ProcessDialogKey(ByVal keyData As System.Windows.Forms.Keys) As Boolean
        Select Case keyData
            Case Keys.Right
                If ScrollH.Value + 50 < ScrollH.Maximum Then
                    ScrollH.Value = ScrollH.Value + 50
                    Return True 'Indicates that the key was processed
                End If
            Case Keys.Left
                If ScrollH.Value - 50 > ScrollH.Minimum Then
                    ScrollH.Value = ScrollH.Value - 50
                    Return True 'Indicates that the key was processed
                End If
            Case Keys.Up
                If ScrollV.Value - 50 > ScrollV.Minimum Then
                    ScrollV.Value = ScrollV.Value - 50
                    Return True 'Indicates that the key was processed
                End If
            Case Keys.Down
                If ScrollV.Value + 50 < ScrollV.Maximum Then
                    ScrollV.Value = ScrollV.Value + 50
                    Return True 'Indicates that the key was processed
                End If
            Case Keys.Escape
                Select Case True
                    Case TypeOf mobjParentForm Is frmProcess AndAlso CType(mobjParentForm, frmProcess).InFullScreenMode
                        CType(mobjParentForm, frmProcess).ExitFullScreenMode()
                        Return True 'Indicates that the key was processed
                    Case ClipboardProcess IsNot Nothing
                        Me.CancelPaste()
                        Return True 'Indicates that the key was processed
                End Select
        End Select

        'If key not processed then allow base implementation to decide
        Return MyBase.ProcessDialogKey(keyData)
    End Function

#End Region

#Region "Tabs Context Menu items"

    ''' <summary>
    ''' Handler for create new sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabNew_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuTabNew.Click
        CreateNewPage(mProcess.GetSubSheetIndex(CType(tcPages.SelectedTab.Tag, Guid)))
    End Sub

    Public Sub CreateNewPage(Optional ByVal iPosition As Integer = -1)

        'choose where to put our new sheet, snapping it 
        'to the grid if required.
        Dim StartStageXCoord, StartStageYCoord As Single
        Dim EndStageXCoord, EndStageYCoord As Single
        Me.mProcess.GetDefaultStartStageLocationForANewSubSheet(StartStageXCoord, StartStageYCoord)
        Me.mProcess.GetDefaultEndStageLocationForANewSubSheet(EndStageXCoord, EndStageYCoord)
        Me.SnapTo(StartStageXCoord, StartStageYCoord)
        Me.SnapTo(EndStageXCoord, EndStageYCoord)

        'Create new subsheet...
        Dim gSubID As Guid

        Dim sName As String
        Dim iNum As Integer = Me.tcPages.TabCount
        Do
            If ModeIsObjectStudio() Then
                sName = String.Format(My.Resources.ctlProcessViewer_Action0, iNum - 1) 'Exclude Cleanup page from count
            Else
                sName = String.Format(My.Resources.ctlProcessViewer_Page0, iNum)
            End If
            If mProcess.GetSubSheetID(sName).Equals(Guid.Empty) Then Exit Do
            iNum += 1
        Loop

        Dim f As New frmPageName(frmPageName.mode.NewPage)
        f.ShowInTaskbar = False
        f.SetName(sName)
        f.SetProcess(mProcess)
        If f.ShowDialog() = DialogResult.OK Then

            gSubID = mProcess.AddSubSheet(f.GetName, StartStageXCoord, StartStageYCoord, EndStageXCoord, EndStageYCoord, Guid.NewGuid, False, SubsheetType.Normal, iPosition).ID
            Dim sheet As clsProcessSubSheet = mProcess.GetSubSheetByID(gSubID)

            'Set view to new sheet...
            mProcess.SetActiveSubSheet(gSubID)
            CreateNewTab(sheet)

            'Add the latest state to the undo/redo buffer
            mProcess.SaveUndoPosition(clsUndo.ActionType.Create, sheet)

            InvalidateView()
        End If
        f.Dispose()
    End Sub

    ''' <summary>
    ''' Handler for duplicate sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabDuplicate_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuTabDuplicate.Click

        ' Copy current sheet to XML
        Dim sXML As String = mProcess.GeneratePageSelectionXML(CType(tcPages.SelectedTab.Tag, Guid))
        Dim iPosition As Integer = mProcess.GetSubSheetIndex(CType(tcPages.SelectedTab.Tag, Guid))

        ' Generate new page name
        Dim sName As String = String.Format(My.Resources.ctlProcessViewer_0Copy, tcPages.SelectedTab.Text)

        ' Create new subsheet
        Dim f As New frmPageName(frmPageName.mode.NewPage)
        f.SetName(sName)
        f.SetProcess(mProcess)
        f.ShowInTaskbar = False
        If f.ShowDialog() = DialogResult.OK Then
            ' Add sheet to process
            Dim newID As Guid = mProcess.PasteSubSheet(f.GetName, sXML, iPosition)

            'Set view to new sheet...
            Dim sheet As clsProcessSubSheet = mProcess.GetSubSheetByID(newID)
            mProcess.SetActiveSubSheet(sheet.ID)
            CreateNewTab(sheet)

            'Add the latest state to the undo/redo buffer
            mProcess.SaveUndoPosition(clsUndo.ActionType.Create, sheet)

            InvalidateView()
        End If
        f.Dispose()
    End Sub

    ''' <summary>
    ''' Handler for copy sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabCopy_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuTabCopy.Click

        ' Copy current sheet to clipboard
        Clipboard.SetDataObject(mProcess.GeneratePageSelectionXML(CType(tcPages.SelectedTab.Tag, Guid)))

    End Sub

    ''' <summary>
    ''' Handler for cut sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabCut_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuTabCut.Click

        ' Copy current sheet to clipboard & delete
        Clipboard.SetDataObject(mProcess.GeneratePageSelectionXML(CType(tcPages.SelectedTab.Tag, Guid)))
        DeleteSheet(True)

    End Sub

    ''' <summary>
    ''' Handler for paste sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabPaste_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuTabPaste.Click
        Dim sXML As String = CStr(Clipboard.GetDataObject().GetData(DataFormats.StringFormat, True))
        Dim iPosition As Integer = mProcess.GetSubSheetIndex(CType(tcPages.SelectedTab.Tag, Guid))

        ' Add sheet(s) to process
        Dim newIDs As IList(Of Guid) = mProcess.PasteSubSheets(sXML, iPosition)
        For Each newID As Guid In newIDs
            Dim sheet As clsProcessSubSheet = mProcess.GetSubSheetByID(newID)

            'Set view to new sheet...
            mProcess.SetActiveSubSheet(sheet.ID)
            CreateNewTab(sheet)
        Next

        'Add the latest state to the undo/redo buffer
        mProcess.SaveUndoPosition(clsUndo.ActionType.Paste, LTools.Format(My.Resources.ctlProcessViewer_plural_bracket_pages, "COUNT", newIDs.Count), "")
        InvalidateView()

    End Sub

    ''' <summary>
    ''' Handler for rename sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabRename_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTabRename.Click

        Dim t As TabPage
        Dim gID As Guid
        Dim sErr As String = "", sNewName, sOldName As String
        Dim Sheet As clsProcessSubSheet

        t = tcPages.SelectedTab
        Sheet = mProcess.GetSubSheetByID(CType(t.Tag, Guid))

        If Not t Is Nothing Then
            If Not Sheet.IsNormal Then Return ' Can only rename 'normal' sheets
            gID = New Guid(t.Name)
            Dim f As New frmPageName(frmPageName.mode.RenamePage)
            sOldName = mProcess.GetSubSheetName(gID)
            f.SetName(sOldName)
            f.SetProcess(mProcess)
            f.ShowInTaskbar = False
            If f.ShowDialog() = DialogResult.OK Then
                sNewName = f.GetName()
                If mProcess.RenameSubSheet(gID, sNewName, sErr) Then
                    UpdateSheetReferenceNames(sOldName, sNewName, gID)
                Else
                    UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_RenameFailed0, sErr))
                End If
                RenameTabByGuid(gID, sNewName)
                UpdateSheetTabs()
                InvalidateView()
            End If
            f.Dispose()
        End If

        'Add the latest state to the undo/redo buffer
        mProcess.SaveUndoPosition(clsUndo.ActionType.ChangeNameOf, Sheet)

    End Sub

    ''' <summary>
    ''' Handler for delete sheet
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTabDelete.Click

        DeleteSheet(False)

    End Sub

    ''' <summary>
    ''' This is the handler for the clicking of the publication
    ''' context menu item on the tab control.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuTabPublish_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuTabPublish.Click
        Dim t As TabPage
        Dim gID As Guid

        t = tcPages.SelectedTab
        If Not t Is Nothing Then
            gID = CType(t.Tag, Guid)
            Dim sh As clsProcessSubSheet = mProcess.GetSubSheetByID(gID)
            If sh Is Nothing Then
                UserMessage.Show(My.Resources.ctlProcessViewer_InternalErrorFailedToMapTabPageToProcessPageTheOperationCannotBeCompleted)
            Else
                If sh.IsNormal Then
                    sh.Published = Not sh.Published

                    If sh.Published Then
                        DoValidate(DoValidateAction.PublishAction, sh)
                    End If

                    SetTabPageHighlight(t, sh.Published)

                    InvalidateView()
                    'Add the latest state to the undo/redo buffer
                    mProcess.SaveUndoPosition(clsUndo.ActionType.Publish, sh)
                Else
                    UserMessage.Show(My.Resources.ctlProcessViewer_YouCannotChangeThePublicationStatusOfAnInitialiseOrCleanupPage, 2058)

                End If
            End If
        End If

        t.Refresh()

    End Sub

    ''' <summary>
    ''' Handles 'Find References' menu option.
    ''' </summary>
    Private Sub mnuFindReferences_Click(sender As Object, e As EventArgs) Handles mnuFindReferences.Click
        Dim s As clsProcessSubSheet = mProcess.GetSubSheetByID(CType(tcPages.SelectedTab.Tag, Guid))
        If s.SheetType = SubsheetType.MainPage Then
            If ModeIsObjectStudio() Then
                mSuperParent.FindReferences(New clsProcessNameDependency(mProcess.Name), mProcess)
            Else
                mSuperParent.FindReferences(New clsProcessIDDependency(mProcess.Id), mProcess)
            End If
        ElseIf s.SheetType = SubsheetType.Normal Then
            ' If an Object action is unpublished then it can only be referenced by a
            ' Page stage in the current Object, otherwise look for Action stage references
            If ModeIsObjectStudio() AndAlso s.Published Then
                mSuperParent.FindReferences(New clsProcessActionDependency(mProcess.Name, s.Name), mProcess)
            Else
                RaiseEvent SelectDependency(New clsProcessPageDependency(s.ID))
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handler for manage tabs
    ''' </summary>
    Private Sub mnuTabManage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles mnuTabManage.Click

        Dim wiz As frmSubSheetOrder = New frmSubSheetOrder(mProcess, Me)

        wiz.ShowDialog()
        If wiz.DialogResult = DialogResult.OK Then
            Dim subSheets As IList(Of KeyValuePair(Of Guid, String)) = wiz.SubSheets

            ' Handle deletions
            Dim affected As Integer = 0
            Dim oldTabOrder As New List(Of Guid)
            For Each tp As TabPage In tcPages.TabPages
                Dim sheetID As Guid = CType(tp.Tag, Guid)
                Dim bDelete As Boolean = True

                For Each kvp As KeyValuePair(Of Guid, String) In subSheets
                    If kvp.Key = sheetID Then
                        bDelete = False
                        Exit For
                    End If
                Next
                If bDelete Then
                    affected += 1
                    mProcess.DeleteSubSheet(sheetID, Guid.Empty)
                Else
                    oldTabOrder.Add(sheetID)
                End If
            Next
            If affected > 0 Then
                'Add the latest state to the undo/redo buffer
                mProcess.SaveUndoPosition(clsUndo.ActionType.Delete, LTools.Format(My.Resources.ctlProcessViewer_plural_bracket_pages, "COUNT", affected), "")
            End If

            ' Handle renames
            affected = 0
            Dim newTabOrder As New List(Of Guid)
            For Each sheetList As KeyValuePair(Of Guid, String) In subSheets
                newTabOrder.Add(sheetList.Key)
                Dim sheet As clsProcessSubSheet = mProcess.GetSubSheetByID(sheetList.Key)
                If Not sheetList.Value = sheet.Name Then
                    affected += 1
                    Dim oldName As String = sheet.Name
                    mProcess.RenameSubSheet(sheetList.Key, sheetList.Value, Nothing)
                    UpdateSheetReferenceNames(oldName, sheetList.Value, sheetList.Key)
                End If
            Next
            If affected > 0 Then
                'Add the latest state to the undo/redo buffer
                mProcess.SaveUndoPosition(clsUndo.ActionType.ChangeNameOf, LTools.Format(My.Resources.ctlProcessViewer_plural_bracket_pages, "COUNT", affected), "")
            End If

            ' Handle any re-ordering of renaming tabs
            If Not ListsAreEqual(oldTabOrder, newTabOrder) Then
                mProcess.SetSubSheetOrder(newTabOrder)
                'Add the latest state to the undo/redo buffer
                mProcess.SaveUndoPosition(clsUndo.ActionType.ChangePageOrder)
            End If

            wiz.Dispose()
            UpdateSheetTabs()
            InvalidateView()
        End If

    End Sub


    ''' <summary>
    ''' Utility to check if two lists of guids are identical
    ''' </summary>
    ''' <param name="list1">First list</param>
    ''' <param name="list2">Second list</param>
    ''' <returns>True if the list contents are identical</returns>
    Private Function ListsAreEqual(ByVal list1 As List(Of Guid), ByVal list2 As List(Of Guid)) As Boolean
        If list1.Count <> list2.Count Then Return False
        For i As Integer = 0 To list1.Count - 1
            If Not list1(i).Equals(list2(i)) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' Updates the names of any subsheet reference stages
    ''' pointing to a particular subsheet,
    ''' replacing instances of the string supplied in 
    ''' the <paramref name="oldname" /> with the string
    ''' supplied in <paramref name="newName" />.
    ''' </summary>
    ''' <param name="oldName">The string to be replaced in
    ''' the names of subsheet reference</param>
    ''' <param name="newName">The string with which to replace
    ''' the old string.</param>
    ''' <param name="TargetSheetID">The id of the subsheet of interest.
    ''' Only subsheet reference stages pointing to this stage
    ''' will be updated.</param>
    ''' <remarks>Useful after renaming a subsheet.</remarks>
    Public Sub UpdateSheetReferenceNames(ByVal oldName As String, ByVal newName As String, ByVal TargetSheetID As Guid)
        For Each RefStage As clsSubSheetRefStage In mProcess.GetStages(Of clsSubSheetRefStage)()
            If RefStage.ReferenceId.Equals(TargetSheetID) Then
                'Rename any 'SubSheet' stages (references) linked to the subsheet (page).
                RefStage.Name = RefStage.Name.Replace(oldName, newName)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Delete (or cut) the currently selected page
    ''' </summary>
    ''' <param name="bCut">Indicates cut was performed</param>
    Private Sub DeleteSheet(ByVal bCut As Boolean)
        Dim oTab As TabPage
        Dim sText As String
        Dim gSheetID As Guid
        Dim gNewSheetID As Guid
        Dim Sheet As clsProcessSubSheet

        'Get the tab one before the selected tab
        Dim i As Integer = tcPages.SelectedIndex - 1
        If i >= 0 Then
            oTab = tcPages.TabPages(i)
            gNewSheetID = CType(oTab.Tag, Guid)
        End If

        'Get the currently selected tab to delete
        oTab = tcPages.SelectedTab
        If Not oTab Is Nothing Then
            sText = oTab.Text
            gSheetID = CType(oTab.Tag, Guid)
            Sheet = mProcess.GetSubSheetByID(gSheetID)
            If Not Sheet.IsNormal Then Return ' Can only delete 'normal' sheets

            If UserMessage.YesNo(String.Format(My.Resources.ctlProcessViewer_AreYouSureYouWantToDeletePage0, sText)) = MsgBoxResult.Yes Then

                If GetOpenPropertiesWindowsOnSubSheet(Sheet).Any Then
                    If UserMessage.OK(String.Format(My.Resources.ThePageCannotBeDeletedWhilePropertyDialogsAreOpen, sText)) = MsgBoxResult.Ok Then
                        Return
                    End If
                End If

                mProcess.DeleteSubSheet(gSheetID, gNewSheetID)
                Me.SelectTabByGuid(gNewSheetID)
                Me.SelectTabSelectorByGuid(gNewSheetID)
                DeleteTabByGuid(gSheetID)
                InvalidateView()
            End If

            'Add the latest state to the undo/redo buffer
            If bCut Then
                mProcess.SaveUndoPosition(clsUndo.ActionType.Cut, Sheet)
            Else
                mProcess.SaveUndoPosition(clsUndo.ActionType.Delete, Sheet)
            End If
        End If
    End Sub
#End Region

#Region "Context Menu OnClick Events"

    Protected Sub contextProperties_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        LaunchSelectedStageProperties()
        InvalidateView()
    End Sub

    Protected Sub contextReferences_OnClick(sender As Object, e As EventArgs)
        If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then
            Dim selection = mProcess.SelectionContainer.PrimarySelection
            Dim name = mProcess.GetStageName(selection.mgStageID)
            Dim scopeStage = mProcess.GetStages(mProcess.GetActiveSubSheet).FirstOrDefault()
            Dim outOfScope As Boolean
            Dim stage = mProcess.GetDataStage(name, scopeStage, outOfScope)
            If Not outOfScope AndAlso stage IsNot Nothing Then _
                RaiseEvent SelectDependency(New clsProcessDataItemDependency(stage))
        End If
    End Sub

    Protected Sub contextCopy_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        DoCopy()
    End Sub

    Protected Sub contextPaste_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        DoPaste(Me.mpMouseDownWorldPos)
    End Sub

    Protected Sub contextCut_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        DoCut()
    End Sub

    Protected Sub contextDelete_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            DoDeleteSelection()
            ValidateOpenPropertiesWindows()
            InvalidateView()
        Catch ex As Exception
            UserMessage.Err("Failed to delete stages", ex)
        End Try
    End Sub

    Protected Sub contextGoToPage_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim objStage As clsProcessStage = Nothing

            If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then _
             objStage = mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)

            If Not (objStage Is Nothing) Then

                If objStage Is Nothing OrElse objStage.StageType <> StageTypes.SubSheet Then
                    'if either we didn't click on a stage, or the stage isn't a page reference
                    'or the page reference doesn't lead anywhere then quit
                    Exit Sub
                Else
                    Dim objSheetRef As clsSubSheetRefStage = CType(objStage, clsSubSheetRefStage)
                    If mProcess.GetSubSheetStartStage(objSheetRef.ReferenceId).Equals(Guid.Empty) Then
                        Exit Sub
                    End If
                    mProcess.SetActiveSubSheet(objSheetRef.ReferenceId)
                    InvalidateView()

                    mobjStageToReturnTo = objStage
                    mbExistsPageRefToReturnTo = True

                End If
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_FailedToGoToPage0, ex.Message), ex)
        End Try
    End Sub

    Protected Sub contextReturnToPage_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        mProcess.SetActiveSubSheet(mobjStageToReturnTo.GetSubSheetID)
        mProcess.FocusCameraOnStage(pbview.Width, pbview.Height, mobjStageToReturnTo)
        InvalidateView()

        mbExistsPageRefToReturnTo = False

    End Sub

    Protected Sub SelectTabByName(ByVal name As String)

        For Each t As TabPage In Me.tcPages.TabPages
            If t.Text = name Then
                SelectTabByGuid(CType(t.Tag, Guid))
                Exit For
            End If
        Next

    End Sub

    Protected Sub SelectTabSelectorByName(ByVal name As String)

        For Each it As ComboBoxItem In cmbTabSelector.Items
            If it.Text = name Then
                SelectTabSelectorByGuid(CType(it.Tag, Guid))
                Exit For
            End If
        Next

    End Sub

    Protected Function GetSelectedStageVBO() As clsVBO

        Dim oProcessStage As clsProcessStage
        Dim oActionStage As clsActionStage
        Dim sObject As String = ""
        Dim sAction As String = ""
        Dim oBusinessObject As clsBusinessObject
        Dim oVBO As clsVBO = Nothing

        If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then
            oProcessStage = mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)

            If Not oProcessStage Is Nothing AndAlso oProcessStage.StageType = StageTypes.Action Then
                oActionStage = CType(oProcessStage, clsActionStage)
                oActionStage.GetResource(sObject, sAction)
                oBusinessObject = Me.mProcess.GetBusinessObjects().FindObjectReference(sObject)
                If TypeOf oBusinessObject Is clsVBO Then
                    oVBO = CType(oBusinessObject, clsVBO)
                End If
            End If

        End If
        Return oVBO

    End Function

    Protected Sub contextReturnToProcessStudio_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim oProcessForm As frmProcess = CType(Me.ParentForm, frmProcess).ProcessViewer.moProcessStudioReference

        If Not oProcessForm Is Nothing Then
            If msProcessStudioReferenceSubSheetName <> "" Then
                oProcessForm.ProcessViewer.SelectTabByName(msProcessStudioReferenceSubSheetName)
                oProcessForm.ProcessViewer.SelectTabSelectorByName(msProcessStudioReferenceSubSheetName)
            End If
            If oProcessForm.WindowState = FormWindowState.Minimized Then
                oProcessForm.WindowState = FormWindowState.Normal
            End If
            oProcessForm.BringToFront()
            oProcessForm.Focus()
            oProcessForm.ProcessViewer.moProcessStudioReference = Nothing
        End If


    End Sub

    Protected Sub contextReturnToException_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim gStageID As Guid = mProcess.ExceptionSource
        If Not gStageID.Equals(Guid.Empty) Then
            Dim oStage As clsProcessStage = mProcess.GetStage(gStageID)

            mProcess.SetActiveSubSheet(oStage.GetSubSheetID)
            mProcess.FocusCameraOnStage(pbview.Width, pbview.Height, oStage)
            mProcess.SelectStage(mProcess.GetStageIndex(gStageID), False)
            InvalidateView()

        End If
    End Sub

    Private Function GetCorrespondingMode(ByVal m As ProcessViewMode) As ProcessViewMode
        ' If it's an object, mask out the object bit and apply the process bit
        ' If it's a process, mask out the process bit and apply the object bit
        If m.HasFlag(ProcessViewMode.Object) Then
            Return (m And Not ProcessViewMode.Object) Or ProcessViewMode.Process

        ElseIf m.HasFlag(ProcessViewMode.Process) Then
            Return (m And Not ProcessViewMode.Process) Or ProcessViewMode.Object

        Else
            ' Not an object, Not a process, so what is it?
            Debug.Fail($"Invalid process viewer mode: {m.ToString()}")
            Throw New InvalidArgumentException(My.Resources.ctlProcessViewer_InternalErrorBadConfigurationInCtlProcessViewerGetCorrespondingMode)
        End If
    End Function

    Private Function GetModeAdjustedForPermissions(mode As ProcessViewMode, businessObject As clsVBO) As ProcessViewMode

        Select Case mode

            Case ProcessViewMode.AdHocTestObject, ProcessViewMode.DebugObject
                If _
                    Not User.Current.HasPermission(Permission.ObjectStudio.ImpliedExecuteBusinessObject) _
                    OrElse Not gSv.GetEffectiveMemberPermissionsForProcess(businessObject.ProcessID).HasPermission(User.Current, Permission.ObjectStudio.ImpliedExecuteBusinessObject) Then

                    Return GetModeAdjustedForPermissions(ProcessViewMode.EditObject, businessObject)
                End If

            Case ProcessViewMode.CloneObject
                Return GetModeAdjustedForPermissions(ProcessViewMode.EditObject, businessObject)

            Case ProcessViewMode.CompareObjects
                Return GetModeAdjustedForPermissions(ProcessViewMode.PreviewObject, businessObject)

            Case ProcessViewMode.EditObject
                If _
                    Not User.Current.HasPermission(Permission.ObjectStudio.ImpliedEditBusinessObject) _
                    OrElse Not gSv.GetEffectiveMemberPermissionsForProcess(businessObject.ProcessID).HasPermission(User.Current, Permission.ObjectStudio.ImpliedEditBusinessObject) Then

                    Return GetModeAdjustedForPermissions(ProcessViewMode.PreviewObject, businessObject)
                End If

        End Select

        Return mode

    End Function

    Protected Sub contextGoToObjectStudio_OnClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim oVBO As clsVBO
        Dim mode As ProcessViewMode
        Dim oProcessForm As frmProcess
        Dim bCanEdit As Boolean
        Dim bCanView As Boolean
        Dim bCanTest As Boolean
        Dim oActionStage As clsActionStage
        Dim sObject As String = String.Empty
        Dim sAction As String = String.Empty

        oVBO = GetSelectedStageVBO()

        If Not oVBO Is Nothing Then

            With User.Current
                bCanEdit = .HasPermission(Permission.ObjectStudio.ImpliedEditBusinessObject)
                bCanView = .HasPermission(Permission.ObjectStudio.ImpliedViewBusinessObject)
                bCanTest = .HasPermission(Permission.ObjectStudio.ImpliedExecuteBusinessObject)
            End With
            If Me.ModeIsObjectStudio Then
                mode = Me.mMode
            Else
                mode = GetCorrespondingMode(Me.mMode)
            End If

            mode = GetModeAdjustedForPermissions(mode, oVBO)


            If bCanView Or bCanEdit Then

                oActionStage = CType(mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID), clsActionStage)
                oActionStage.GetResource(sObject, sAction)

                'See if there is an instance of the process already open
                oProcessForm = frmProcess.GetInstance(oVBO.ProcessID, mode)
                If oProcessForm IsNot Nothing Then
                    oProcessForm.ProcessViewer.moProcessStudioReference = CType(Me.ParentForm, frmProcess)

                    If oProcessForm.WindowState = FormWindowState.Minimized Then
                        oProcessForm.WindowState = FormWindowState.Normal
                    End If
                    oProcessForm.ProcessViewer.msProcessStudioReferenceSubSheetName = mProcess.GetSubSheetName(oActionStage.GetSubSheetID)
                    oProcessForm.ProcessViewer.SelectTabByName(sAction)
                    oProcessForm.ProcessViewer.SelectTabSelectorByName(sAction)
                    oProcessForm.BringToFront()
                    oProcessForm.Focus()
                Else
                    oProcessForm = New frmProcess(mode, oVBO.ProcessID, "", "")
                    If TypeOf ParentForm Is frmProcess Then
                        oProcessForm.ProcessViewer.moProcessStudioReference = CType(Me.ParentForm, frmProcess)
                    End If

                    If oProcessForm.WindowState = FormWindowState.Minimized Then
                        oProcessForm.WindowState = FormWindowState.Normal
                    End If
                    oProcessForm.ProcessViewer.msProcessStudioReferenceSubSheetName = mProcess.GetSubSheetName(oActionStage.GetSubSheetID)
                    mSuperParent.StartForm(oProcessForm)
                    oProcessForm.ProcessViewer.SelectTabByName(sAction)
                    oProcessForm.ProcessViewer.SelectTabSelectorByName(sAction)
                End If
            Else
                UserMessage.Show(My.Resources.ctlProcessViewer_YouDoNotHavePermissionToEditOrViewThisBusinessObject, 0, "helpUserPermissions.htm")
            End If

        End If

    End Sub

    Protected Sub contextViewProcess_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            'for some daft reason this collection is indexed from 1 instead of 0
            Dim objProcessRef As clsSubProcessRefStage = TryCast(mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID), clsSubProcessRefStage)
            If objProcessRef Is Nothing Then
                UserMessage.Show(My.Resources.ctlProcessViewer_FailedToFindProcessStageOfInterestIsYouProcessStageSelected)
                Exit Sub
            End If

            Dim gProcessID As Guid = objProcessRef.ReferenceId
            If gProcessID = Guid.Empty Then
                UserMessage.Show(My.Resources.ctlProcessViewer_ItAppearsThatYouHaveNotYetSetTheProcessForThisStagePleaseSetAProcessByEditingTh)
                Exit Sub
            Else
                Dim processViewMode = GetProcessViewMode(mMode, gProcessID)
                If mSuperParent IsNot Nothing Then _
                 mSuperParent.StartForm(New frmProcess(processViewMode, gProcessID, "", ""))
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_InternalError0, ex.Message) & vbCrLf & vbCrLf & My.Resources.ctlProcessViewer_PleaseContactBluePrismForTechnicalSupportIfThisProblemPersists, ex)
        End Try
    End Sub

    Private Function GetProcessViewMode(currentViewMode As ProcessViewMode, groupMemberId As Guid) As ProcessViewMode
        Dim currentProcessGroupPermissions = gSv.GetEffectiveGroupPermissionsForProcess(groupMemberId)
        Dim hasPermission = currentProcessGroupPermissions.HasPermission(User.Current, Permission.ProcessStudio.ImpliedEditProcess)

        If hasPermission Then
            If ModeIsObjectStudio() Then
                Return ProcessViewMode.EditProcess
            Else
                Return currentViewMode
            End If
        Else
            Return ProcessViewMode.PreviewProcess
        End If
    End Function

    Protected Sub contextBreakpoint_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
            Exit Sub
        End If
        Dim objStage As clsProcessStage
        objStage = mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)
        If Not objStage Is Nothing Then
            If Not objStage.HasBreakPoint Then
                objStage.BreakPoint = New clsProcessBreakpoint(objStage)
                mProcess.SaveUndoPosition(clsUndo.ActionType.Breakpoint, objStage)
                If objStage.StageType = StageTypes.Data Then
                    objStage.BreakPoint.BreakPointType = clsProcessBreakpoint.BreakEvents.WhenDataValueChanged
                End If
            Else
                objStage.BreakPoint = Nothing
                mProcess.SaveUndoPosition(clsUndo.ActionType.Breakpoint, objStage)
            End If
        End If
        InvalidateView()
    End Sub


    Protected Sub contextBreakpointProperties_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
            Exit Sub
        End If
        Dim objStage As clsProcessStage
        objStage = mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)
        If Not objStage Is Nothing Then
            Select Case objStage.StageType
                Case StageTypes.Data
                    Dim f As New frmBreakpointProperties
                    f.SetEnvironmentColoursFromAncestor(Me)
                    f.SetStage(objStage)
                    f.ProcessViewer = Me
                    f.ParentAppForm = mSuperParent
                    f.ShowInTaskbar = False
                    f.ShowDialog()
                    f.Dispose()
                Case Else
                    Dim f As New frmExpressionChooser
                    f.SetEnvironmentColoursFromAncestor(Me)
                    f.Stage = objStage
                    f.mExpressionBuilder.ProcessViewer = Me
                    f.ParentAppForm = mSuperParent
                    f.mExpressionBuilder.StoreInVisible = False
                    f.mExpressionBuilder.Validator = AddressOf f.mExpressionBuilder.IsValidDecision
                    f.mExpressionBuilder.Tester = AddressOf f.mExpressionBuilder.TestDecision

                    If objStage.BreakPoint IsNot Nothing Then f.Expression = clsExpression.NormalToLocal(objStage.BreakPoint.Condition)
                    f.ShowInTaskbar = False
                    f.ShowDialog()
                    If f.DialogResult = DialogResult.OK Then
                        objStage.BreakPoint.Condition = clsExpression.LocalToNormal(f.Expression)
                        If Not f.Expression.Length = 0 Then
                            objStage.BreakPoint.BreakPointType = objStage.BreakPoint.BreakPointType Or clsProcessBreakpoint.BreakEvents.WhenConditionMet
                        End If
                    End If
                    f.Dispose()
            End Select

        End If
        InvalidateView()
    End Sub

    Protected Sub ContextRunToStage_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If HasPropertyWindowsOpen Then
            UserMessage.OK(My.Resources.frmProcess_DebuggingIsNotAvailableWhilePropertiesDialogsAreOpen)
            Exit Sub
        End If

        Dim stgId As Guid = mProcess.SelectionContainer.PrimarySelection.mgStageID
        mProcess.RunToStageId = stgId
        If stgId <> Guid.Empty Then
            DebugAction(ProcessRunAction.Go)
        End If
        InvalidateView()
    End Sub

    Protected Sub ContextSetNextStage_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If HasPropertyWindowsOpen Then
            UserMessage.OK(My.Resources.frmProcess_DebuggingIsNotAvailableWhilePropertiesDialogsAreOpen)
            Exit Sub
        End If

        If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
            Exit Sub
        End If

        Dim sErr As String = Nothing
        SetupDebugSessionAndLogging()
        If Not mProcess.SetRunStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID, sErr) Then
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_FailedToSetRunStage0, sErr))
        End If
        mProcess.RunState = ProcessRunState.Paused

        If TypeOf mobjParentForm Is frmProcess Then
            CType(mobjParentForm, frmProcess).UpdateDebugButtons()
        End If

        InvalidateView()
    End Sub

    Protected Sub ContextEnableCalculationZoom_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mnuContextCalculationZoom.Checked = Not mnuContextCalculationZoom.Checked
        SetCalcZoomMenuOption(mnuContextCalculationZoom.Checked)
        frmCalculationZoom.Disabled(Me) = Not mnuContextCalculationZoom.Checked
        SetCalculationZoomEnabled(mnuContextCalculationZoom.Checked)
    End Sub

    Protected Sub ContextFullScreen_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If CType(mobjParentForm, frmProcess).InFullScreenMode = True Then
            CType(mobjParentForm, frmProcess).ExitFullScreenMode()
        Else
            CType(mobjParentForm, frmProcess).EnterFullScreenMode()
        End If
    End Sub

    'This menu option can only be clicked when a stage is selected so no
    'need to worry about null object references
    Protected Sub ContextShowAllChanges_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UserMessage.ShowBlueBarMessage(mProcess.GetStage(
            CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID).FullChangesList,
            String.Format(My.Resources.ctlProcessViewer_0DetectedTheFollowingChangesToThisStage, ApplicationProperties.ApplicationName),
            My.Resources.ctlProcessViewer_ViewAListOfAllChangesDetected, CType(mobjParentForm, IEnvironmentColourManager), -1, "", 400, 300)
    End Sub

    Protected Sub ContextSwitch_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim objStage As clsProcessStage
        Dim sErr As String = Nothing

        Dim objSelection As clsProcessSelection = CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection)
        objStage = mProcess.GetStage(objSelection.mgStageID)
        If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Link Then
            Dim objDecision As clsDecisionStage = CType(objStage, clsDecisionStage)
            If Not objDecision.SwitchLink(objSelection.msLinkType, sErr) Then
                UserMessage.Show(sErr)
            Else
                'Add the latest state to the undo/redo buffer
                mProcess.SaveUndoPosition(clsUndo.ActionType.SwitchLink, objDecision)
            End If
            InvalidateView()
        ElseIf objStage.StageType = StageTypes.Decision Then
            Dim objDecision As clsDecisionStage = CType(objStage, clsDecisionStage)
            If Not objDecision.SwitchLink("True", sErr) Then
                UserMessage.Show(sErr)
            Else
                mProcess.SaveUndoPosition(clsUndo.ActionType.SwitchLink, objDecision)
            End If
            InvalidateView()
        End If

    End Sub

    Protected Sub ContextGotoCorrespondingStage_OnClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If TypeOf Me.mobjParentForm Is frmProcessComparison Then
            If mProcess.GetSelectionType = clsProcessSelection.SelectionType.Stage Then
                CType(Me.mobjParentForm, frmProcessComparison).ShowStage(mProcess.SelectionContainer.PrimarySelection.mgStageID)
            End If
        End If
    End Sub


#End Region

#Region "pbview mouse events"

    Private Sub pbView_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbview.DoubleClick

        Try

            'Ignore double-clicking in the view area when the zoom tool is
            'in use - we want to be able to click quickly to zoom in with
            'the left mouse button...
            If GetCurrentTool() = StudioTool.Zoom Then Exit Sub

            If Me.ModeAllowsStageProperties Then
                Dim objStage As clsProcessStage = mProcess.GetStageAtPosition(mpMouseDownWorldPos)
                If objStage IsNot Nothing Then
                    LaunchStageProperties(objStage)
                Else
                    'maybe there is a choice node here
                    Dim Linkable As ILinkable = mProcess.GetLinkableObjectAtPosition(mpMouseDownWorldPos, mProcess.GetActiveSubSheet())
                    Dim choice As clsChoice = TryCast(Linkable, clsChoice)
                    If choice?.OwningStage IsNot Nothing Then
                        LaunchStageProperties(choice.OwningStage)
                    End If
                End If
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_ThereWasAnErrorLaunchingStagePropertiesTheErrorMessageWasExMessage, ex.Message), ex)
        End Try

    End Sub


    Protected Overridable Sub DoMouseDown(ByVal e As MouseEventArgs)
        Try
            'Force the control into focus when we click it. see bug# 2733
            pbview.Focus()

            'determine if we should allow the user to make edits, or to view information
            Dim bIsEditable As Boolean = Me.ModeIsEditable
            Dim bIsPreview As Boolean = Not bIsEditable

            'Convert mouse position to our coordinate system,
            'taking into account view position and zoom factor.
            Me.mpMouseDownWorldPos = Me.GetWorldCoordsFromScreen(New Point(e.X, e.Y))
            Me.mpMouseDownLocalPos = e.Location
            Me.mpMouseDownCameraPos = New PointF(mProcess.GetCameraX, mProcess.GetCameraY)

            'These will default to 0 unless something later in the
            'handler sets them explicitly.
            msngMouseDragXOffset = 0
            msngMouseDragYOffset = 0

            Select Case e.Button
                Case System.Windows.Forms.MouseButtons.Right
                    'Handle the right button by bringing up a contextmenu

                    If GetCurrentTool() = StudioTool.Zoom Then
                        'Don't do anything else for right-button with
                        'zoom tool - we need to zoom out, but that is
                        'handled when the button is released.
                        Me.ContextMenu = Nothing
                        Exit Sub
                    Else
                        'right click should select stages at that point
                        mProcess.SelectAtPosition(Me.mpMouseDownWorldPos, False, True)
                        InvalidateView()

                        'All we need to do is show the context menu so now we are done.
                        Me.ShowContextMenuForCurrentSelection(bIsEditable, bIsPreview)
                        Exit Sub
                    End If

                Case System.Windows.Forms.MouseButtons.Left
                    If Me.mTool = StudioTool.Link Then
                        LinkSource = mProcess.GetLinkableObjectAtPosition(Me.mpMouseDownWorldPos)
                        If LinkSource IsNot Nothing Then Exit Sub
                    End If

                    If Not mProcess.SelectResizeHandles(Me.mpMouseDownWorldPos.X, Me.mpMouseDownWorldPos.Y, 5) Then
                        'Either select what is under the mouse,
                        'or prepare for a rubber band selection, if nothing there
                        Dim bExtend As Boolean = False
                        If Control.ModifierKeys = Keys.Shift OrElse Control.ModifierKeys = Keys.Control Then bExtend = True
                        Dim OverStage As Boolean = (mProcess.GetStageAtPosition(mpMouseDownWorldPos) IsNot Nothing)
                        mProcess.SelectAtPosition(Me.mpMouseDownWorldPos, bExtend, Not bExtend)
                        If Not (OverStage AndAlso bExtend) Then
                            Me.BeginMouseDragging()
                        End If
                    Else
                        Me.BeginMouseDragging()
                    End If

                    'for some reason mousedown was getting rid of sizing pointer.
                    pbview.Cursor = mySizingPointer
            End Select

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_ThereWasAnErrorWhilstHandlingTheMouseEndTheErrorMessageWasExMessage, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Initialises member variables for the start of mouse dragging.
    ''' </summary>
    Protected Overridable Sub BeginMouseDragging()
        pbview.Capture = True
        MouseDragging = True
        mbMouseDragSelected = False
        mobjMouseDragStage = Nothing
        miMouseDragCorner = 0
    End Sub

    Private Sub EndMouseDragging()
        pbview.Capture = False
        MouseDragging = False
        mbMouseDragSelected = False
        mobjMouseDragStage = Nothing
        miMouseDragCorner = 0
        mobjMouseDragStage = Nothing
    End Sub

    Private Sub pbView_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbview.MouseDown
        DoMouseDown(e)
    End Sub

    ''' <summary>
    ''' Shows the context menu to the user, based on the current selection in the
    ''' user interface.
    ''' </summary>
    ''' <param name="bIsEditable">Set to true if the process can be modified by the
    ''' user.</param>
    ''' <param name="bIsPreview">Set to true if the process is displayed as
    ''' a preview only.</param>
    Private Sub ShowContextMenuForCurrentSelection(ByVal bIsEditable As Boolean, ByVal bIsPreview As Boolean)

        'find out which stage we are on, if any
        Dim objStage As clsProcessStage = Nothing
        If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then
            objStage = mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)
        End If

        'Determine if the process info stage is selected,
        'as we want to know this several times while setting
        'up the menu...
        Dim bIsProcInfoStage As Boolean
        bIsProcInfoStage = (Not objStage Is Nothing) AndAlso
         ((objStage.StageType = StageTypes.SubSheetInfo) OrElse (objStage.StageType = StageTypes.ProcessInfo))


        'find out if start stage selected because may want to disable delete option etc
        Dim bIsStart As Boolean
        bIsStart = (Not objStage Is Nothing) AndAlso
         (objStage.StageType = StageTypes.Start)


        'start building context menu ...
        Dim mnuContextMenu As New ContextMenuStrip()
        Dim mm As ToolStripItem


        'allow user to get full changes list when in process comparison mode
        If ModeIsComparison() Then
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ShowFullChangesList, Nothing, New System.EventHandler(AddressOf Me.ContextShowAllChanges_OnClick))
            mm.Enabled = ((mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage) AndAlso objStage.MoreChangesAvailable)

            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ShowCorrespondingStageOnOtherSide, Nothing, New System.EventHandler(AddressOf Me.ContextGotoCorrespondingStage_OnClick))
        End If

        'menu option to go direct to a sub page
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_GoToPage, Nothing, New System.EventHandler(AddressOf Me.contextGoToPage_OnClick))
        If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
            mm.Enabled = False
        ElseIf objStage Is Nothing OrElse objStage.StageType <> StageTypes.SubSheet Then
            mm.Enabled = False
        Else
            Dim objSheetRef As clsSubSheetRefStage = CType(objStage, clsSubSheetRefStage)
            If mProcess.GetSubSheetStartStage(objSheetRef.ReferenceId).Equals(Guid.Empty) Then
                'if the reference doesn't appear to point anywhere then don't bother.
                mm.Enabled = False
            End If
        End If
        'and an option to come back
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ReturnToPageReference, Nothing, New System.EventHandler(AddressOf Me.contextReturnToPage_OnClick))
        If Not mbExistsPageRefToReturnTo Then mm.Enabled = False


        'Add a 'go to object' link, on action stages
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ViewActionInObjectStudio, Nothing, New System.EventHandler(AddressOf Me.contextGoToObjectStudio_OnClick))
        If Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Action Then
            'A single action stage has been selected
            Dim actionStage = GetSelectedStageVBO()
            If actionStage Is Nothing OrElse
                Not gSv.GetEffectiveMemberPermissionsForProcess(actionStage.ProcessID).HasPermission(User.Current, Permission.ObjectStudio.ImpliedViewBusinessObject) Then
                'The action does not relate to a VBO, or the user does not have permission to view the vbo
                mm.Enabled = False
            Else
                mm.Enabled = True
            End If
        Else
            mm.Enabled = False
        End If

        'Option to return to last studio reference, after drilling
        'down on action stage, as above
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ReturnToReferringActionStage, Nothing, New System.EventHandler(AddressOf Me.contextReturnToProcessStudio_OnClick))
        mm.Enabled = moProcessStudioReference IsNot Nothing AndAlso (Not moProcessStudioReference.IsDisposed)

        'Option to return to stage that caused exception
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ReturnToStageThatCausedException, Nothing, New System.EventHandler(AddressOf Me.contextReturnToException_OnClick))
        If Not objStage Is Nothing AndAlso Not objStage.StageType = StageTypes.Recover Then mm.Enabled = False

        'and a 'view process' link
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ViewProcess, Nothing, New System.EventHandler(AddressOf Me.contextViewProcess_OnClick))

        mm.Enabled = False
        If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then
            Dim processRefStage = TryCast(objStage, clsSubProcessRefStage)
            If processRefStage IsNot Nothing AndAlso processRefStage.StageType = StageTypes.Process Then
                mm.Enabled = gSv.GetEffectiveMemberPermissionsForProcess(processRefStage.ReferenceId).HasPermission(User.Current, Permission.ProcessStudio.ImpliedViewProcess)
            End If
        End If

        'and a 'Full Screen' link
        If TypeOf Me.mobjParentForm Is frmProcess Then
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_FullScreen, Nothing, New System.EventHandler(AddressOf Me.ContextFullScreen_OnClick))
            If CType(mobjParentForm, frmProcess).InFullScreenMode = True Then
                CType(mm, ToolStripMenuItem).Checked = True
            End If
        End If

        'adds a separator
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ContextMenuSeparator)

        Dim sErr As String = Nothing
        'add cut,copy,paste, switch
        If bIsEditable Then
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_CuT, Nothing, New System.EventHandler(AddressOf Me.contextCut_OnClick))
            If Not mProcess.CanCut(sErr) Then mm.Enabled = False
        End If
        'copy is always available
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_Copy, Nothing, New System.EventHandler(AddressOf Me.contextCopy_OnClick))
        If Not mProcess.CanCopy(sErr) Then mm.Enabled = False
        If bIsEditable Then
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_Paste, Nothing, New System.EventHandler(AddressOf Me.contextPaste_OnClick))
            If Not mProcess.CanPaste() Then mm.Enabled = False
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_Delete, Nothing, New System.EventHandler(AddressOf Me.contextDelete_OnClick))
            If bIsProcInfoStage Then mm.Enabled = False
            If bIsStart Then mm.Enabled = False
            If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.None Then mm.Enabled = False
        End If

        'always add separator
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ContextMenuSeparator)

        If bIsEditable Then
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_Switch, Nothing, New System.EventHandler(AddressOf Me.ContextSwitch_OnClick))
            Select Case mProcess.GetSelectionType
                Case clsProcessSelection.SelectionType.Link
                    Select Case mProcess.SelectionContainer.PrimarySelection.msLinkType
                        Case "True", "False"
                            mm.Enabled = True
                        Case Else
                            mm.Enabled = False
                    End Select
                Case clsProcessSelection.SelectionType.Stage
                    mm.Enabled = (TypeOf objStage Is clsDecisionStage)
                Case Else
                    mm.Enabled = False
            End Select
            mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ContextMenuSeparator)
        End If

        'debug stuff
        If Not bIsPreview OrElse mMode.HasFlag(ProcessViewMode.AdhocTest) OrElse mMode.HasFlag(ProcessViewMode.Debug) Then

            'add breakpoint option
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_Breakpoint, Nothing, New System.EventHandler(AddressOf Me.contextBreakpoint_OnClick))
            If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Collection Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Block Then
                mm.Enabled = False
            End If
            If bIsProcInfoStage Then mm.Enabled = False
            If Not objStage Is Nothing Then
                CType(mm, ToolStripMenuItem).Checked = objStage.HasBreakPoint
            End If

            'breakpoint condition
            Dim bEnabled As Boolean = mm.Enabled AndAlso CType(mm, ToolStripMenuItem).Checked
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_SetBreakpointCondition, Nothing, New System.EventHandler(AddressOf Me.contextBreakpointProperties_OnClick))
            mm.Enabled = bEnabled

            'run to this stage
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_RunToThisStage, Nothing, New System.EventHandler(AddressOf Me.ContextRunToStage_OnClick))
            If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Data Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Collection Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Block Then
                mm.Enabled = False
            End If
            If bIsProcInfoStage Then mm.Enabled = False

            '"set next stage" option
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_SetNextStage, Nothing, New System.EventHandler(AddressOf Me.ContextSetNextStage_OnClick))
            If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Data Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Collection Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Block Then
                mm.Enabled = False
            End If
            If bIsProcInfoStage Then mm.Enabled = False

            'calc zoom option
            mnuContextCalculationZoom = CType(mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_EnableCalculationZoom, Nothing, New System.EventHandler(AddressOf Me.ContextEnableCalculationZoom_OnClick)), ToolStripMenuItem)
            mnuContextCalculationZoom.Checked = Not frmCalculationZoom.Disabled(Me)
            mm = mnuContextCalculationZoom
            If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Data Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Collection Then
                mm.Enabled = False
            ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Block Then
                mm.Enabled = False
            End If
            If bIsProcInfoStage Then mm.Enabled = False

            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_ContextMenuSeparator)
        End If

        'add properties item
        If bIsProcInfoStage Then mm.Enabled = False
        mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_Properties, Nothing, New System.EventHandler(AddressOf Me.contextProperties_OnClick))
        If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then
            mm.Enabled = False
        ElseIf Not objStage Is Nothing AndAlso objStage.StageType = StageTypes.Anchor Then
            mm.Enabled = False
        End If

        'add find references option (data item & collection stages only)
        If objStage IsNot Nothing AndAlso
            (objStage.StageType = StageTypes.Collection OrElse objStage.StageType = StageTypes.Data) Then
            mm = mnuContextMenu.Items.Add(My.Resources.ctlProcessViewer_FindReferences, Nothing, New System.EventHandler(AddressOf Me.contextReferences_OnClick))
        End If

        'shows context menu to user
        Me.pbview.ContextMenuStrip = mnuContextMenu

    End Sub

    ''' <summary>
    ''' Determines whether edits may be made to the process diagram by observing
    ''' the mode in the member mimode.
    ''' </summary>
    ''' <returns>Returns true if we are in edit, create or clone mode; false
    ''' otherwise.</returns>
    Public Function ModeIsEditable() As Boolean
        Return mMode.HasAnyFlag(ProcessViewMode.Edit Or ProcessViewMode.Clone)
    End Function

    ''' <summary>
    ''' Determines whether the current mode is object studio (the only alternative
    ''' being Process Studio at the time of writing).
    ''' </summary>
    ''' <returns>Returns true if the current mode is any mode of object studio;
    ''' false otherwise.</returns>
    Public Function ModeIsObjectStudio() As Boolean
        Return mMode.HasFlag(ProcessViewMode.Object)
    End Function

    ''' <summary>
    ''' Gets the minimum pages allowed by this viewer in its current mode.
    ''' This is 2 for object studio (Initialise + Clean Up), 1 for process studio
    ''' (Main Page).
    ''' </summary>
    Private ReadOnly Property MinimumPagesAllowed() As Integer
        Get
            Return If(ModeIsObjectStudio(), 2, 1)
        End Get
    End Property

    ''' <summary>
    ''' Determines if the current mode allows for stage properties forms to be seen.
    ''' </summary>
    ''' <returns></returns>
    Private Function ModeAllowsStageProperties() As Boolean
        'Properties are available in every mode, but may be readonly
        'in some circumstances
        Return True
    End Function

    ''' <summary>
    ''' Determines if the current mode is a cloning, be it in object studio
    ''' or process studio.
    ''' </summary>
    ''' <returns>Returns true if we are in clone mode for either
    ''' object studio or process studio; false otherwise.</returns>
    Public Function ModeIsCloning() As Boolean
        Return mMode.HasFlag(ProcessViewMode.Clone)
    End Function

    ''' <summary>
    ''' Determines if the current mode is a comparison mode, be it in object
    ''' studio or process studio.
    ''' </summary>
    ''' <returns>Returns true when in either object studio comparison mode
    ''' or process studio comparison mode.</returns>
    Private Function ModeIsComparison() As Boolean
        Return mMode.HasFlag(ProcessViewMode.Compare)
    End Function

#Region "pbView Mouse Events"

    Private Sub pbView_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbview.MouseMove
        DoMouseMove(e)
    End Sub

    ''' <summary>
    ''' Converts a screen location (relative to pbView) into world coordinates.
    ''' </summary>
    ''' <param name="ScreenLocation">The screen location.</param>
    ''' <returns>As summary.</returns>
    Private Function GetWorldCoordsFromScreen(ByVal ScreenLocation As Point) As PointF
        Dim x As Single = CSng(mProcess.GetCameraX() + (ScreenLocation.X - pbview.ClientRectangle.Width \ 2) / mProcess.Zoom)
        Dim y As Single = CSng(mProcess.GetCameraY() + (ScreenLocation.Y - pbview.ClientRectangle.Height \ 2) / mProcess.Zoom)

        Return New PointF(x, y)
    End Function


    ''' <summary>
    ''' Sets a tooltip for the specified mouse position in screen coords.
    ''' </summary>
    ''' <param name="X">The x coord.</param>
    ''' <param name="Y">The x coord.</param>
    Protected Overridable Sub SetToolTipForMousePos(ByVal X As Integer, ByVal Y As Integer)

        'If we are hovering over something then draw the relevent tooltip
        'objStage = mobjProcess.GetStageAtPosition(msngMousePosX, msngMousePosY)
        Dim WorldPoint As PointF = Me.mRenderer.CoordinateTranslator.TranslateScreenPointToWorldPoint(New Point(X, Y), Me.pbview.ClientSize)
        Dim ItemAtXY As clsPixmap.PixmapItem = Me.mRenderer.GetItemAt(WorldPoint)
        Select Case True
            Case TypeOf ItemAtXY.Item Is clsProcessStage
                Dim objstage As clsProcessStage = CType(ItemAtXY.Item, clsProcessStage)

                'set tooltip if we are over stage
                mRenderer.SetToolTipActive(True)
                mRenderer.SetToolTipIn(False)
                If ModeIsComparison() Then
                    mRenderer.SetToolTipText(objstage.EditSummary)
                Else
                    SetToolTipForStage(objstage)
                End If
                Dim p As New Point(CInt(X), CInt(Y + 22))
                mRenderer.SetToolTipPoint(p)

                InvalidateView()

            Case TypeOf ItemAtXY.Item Is String
                mRenderer.SetToolTipText(CType(ItemAtXY.Item, String))

                Dim p As New Point(CInt(X), CInt(Y + 22))
                mRenderer.SetToolTipPoint(p)
                InvalidateView()

            Case Else
                mRenderer.SetToolTipActive(False)
                mRenderer.SetToolTipIn(False)
        End Select
    End Sub

    Protected Overridable Sub DoMouseMove(ByVal e As MouseEventArgs)
        Try
            If DesignMode Then Exit Sub

            'Convert mouse position to our coordinate system,
            'taking into account view position and zoom factor.
            Me.mpMousePos = Me.GetWorldCoordsFromScreen(New Point(e.X, e.Y))
            Me.ClipboardProcessLocation = mpMousePos
            If Me.ClipboardProcess IsNot Nothing Then
                Me.InvalidateView()
            End If

            'If dragging, and the mouse is outside of the current view,
            'then nudge the view over to allow selection to continue
            Static MouseMovement As clsGeometry.Vector
            Static LastMouseLocation As Point
            MouseMovement.X = MouseMovement.X / 2 + e.X - LastMouseLocation.X
            MouseMovement.Y = MouseMovement.Y / 2 + e.Y - LastMouseLocation.Y
            LastMouseLocation = e.Location
            If Me.MouseDragging Then
                If mobjMouseDragStage IsNot Nothing Then
                    If (Me.mProcess.SelectionContainer IsNot Nothing) AndAlso Me.mProcess.SelectionContainer.Count > 0 Then
                        Dim SelectionBounds As RectangleF = Me.mProcess.SelectionContainer.SelectionBounds(mProcess)
                        If Not mProcess.GetWorldViewPort(Me.pbview.ClientSize).Contains(SelectionBounds) Then
                            mProcess.FocusCameraOnRectangle(SelectionBounds, MouseMovement, Me.pbview.ClientSize)
                        End If
                    End If
                Else
                    mProcess.FocusCameraOnPoint(mpMousePos, pbview.ClientSize)
                    Me.InvalidateView()
                End If
            End If

            SetToolTipForMousePos(e.X, e.Y)

            'If we're dragging something, update its position
            'and make sure the display is updated.
            If MouseDragging Then

                If Not mbMouseDragSelected AndAlso GetCurrentTool() <> StudioTool.Zoom Then

                    mbMouseDragSelected = True
                    'Retrieve selection offset information...
                    If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.None Then
                        mProcess.GetSelectionOffset(mobjMouseDragStage, msngMouseDragXOffset, msngMouseDragYOffset, miMouseDragCorner)
                    End If

                End If

                Dim sngSX As Single = mpMousePos.X - msngMouseDragXOffset
                Dim sngSY As Single = mpMousePos.Y - msngMouseDragYOffset
                SnapTo(sngSX, sngSY)

                'do dragging operation, depending on current tool and
                'previous mouse behaviour
                Dim CurrentTool As StudioTool
                CurrentTool = GetCurrentTool()
                Select Case CurrentTool
                    Case StudioTool.Pan
                        Dim LocalOffset As Size = New Size(e.X - mpMouseDownLocalPos.X, e.Y - mpMouseDownLocalPos.Y)
                        Dim WorldOffset As SizeF = New SizeF(LocalOffset.Width / mProcess.Zoom, LocalOffset.Height / mProcess.Zoom)
                        mProcess.SetCameraX(mpMouseDownCameraPos.X - WorldOffset.Width)
                        mProcess.SetCameraY(mpMouseDownCameraPos.Y - WorldOffset.Height)
                    Case StudioTool.Link
                        'We don't drag the object when we're creating
                        'a link, but we want the view to update so we
                        'can see the link being created
                        InvalidateView()
                    Case StudioTool.Zoom
                        'Don't need to do anything at all for the zoom
                        'tool.
                    Case Else
                        If Not mobjMouseDragStage Is Nothing Then
                            Dim oProcessSelectionContainer As clsProcessSelectionContainer = mProcess.SelectionContainer

                            Select Case miMouseDragCorner
                                Case 0
                                    If Me.ModeIsEditable Then
                                        'Not dragging a corner, so we must be moving
                                        'all the selected stages. First we find out
                                        'how far to move, by comparing the new and
                                        'old position of the stage we "have hold of":
                                        Dim sngXMoved As Single, sngYMoved As Single
                                        sngXMoved = sngSX - mobjMouseDragStage.GetDisplayX()
                                        sngYMoved = sngSY - mobjMouseDragStage.GetDisplayY()

                                        'Now each stage, including the one we are
                                        '"holding", gets moved by the same amount...
                                        Dim aPositionChanges As Single()

                                        'Create an empty hashtable ready to hold any stages that have moved.
                                        If mhtUndoMovedStages Is Nothing Then
                                            mhtUndoMovedStages = New Hashtable
                                        End If
                                        mbNodeMoved = False

                                        For Each oProcessSelection As clsProcessSelection In oProcessSelectionContainer
                                            If oProcessSelection.mtType = clsProcessSelection.SelectionType.Stage Then
                                                'Get stage and remember its old position, along with its node positions
                                                Dim objstage As clsProcessStage = mProcess.GetStage(oProcessSelection.mgStageID)
                                                Dim OldStageLocation As PointF = objstage.Location

                                                'Update its new position
                                                objstage.SetDisplayX(objstage.GetDisplayX() + sngXMoved)
                                                objstage.SetDisplayY(objstage.GetDisplayY() + sngYMoved)

                                                'Update position of choice/wait nodes accordingly
                                                If objstage.StageType = StageTypes.WaitEnd OrElse objstage.StageType = StageTypes.ChoiceEnd _
                                                OrElse objstage.StageType = StageTypes.WaitStart OrElse objstage.StageType = StageTypes.ChoiceStart Then

                                                    'If the any choice node sticks past the choice end then shift it up.
                                                    Dim objChoiceEnd As clsChoiceEndStage
                                                    Dim objChoiceStart As clsChoiceStartStage
                                                    Dim IncrementStep As Integer
                                                    Dim StartIndex As Integer
                                                    Dim EndIndex As Integer

                                                    If objstage.StageType = StageTypes.WaitEnd OrElse objstage.StageType = StageTypes.ChoiceEnd Then
                                                        objChoiceEnd = TryCast(objstage, clsChoiceEndStage)
                                                        objChoiceStart = mProcess.GetChoiceStart(objChoiceEnd)
                                                        IncrementStep = -1
                                                        StartIndex = objChoiceStart.Choices.Count - 1
                                                        EndIndex = 0
                                                    Else
                                                        objChoiceStart = TryCast(objstage, clsChoiceStartStage)
                                                        objChoiceEnd = mProcess.GetChoiceEnd(objChoiceStart)
                                                        IncrementStep = 1
                                                        StartIndex = 0
                                                        EndIndex = objChoiceStart.Choices.Count - 1
                                                    End If

                                                    'If we encounter a problem then we intend to revert
                                                    'changes to positions of stages and nodes
                                                    Dim ProblemExists As Boolean = False
                                                    Dim OldNodePositions(objChoiceStart.Choices.Count - 1) As Single
                                                    For i As Integer = 0 To objChoiceStart.Choices.Count - 1
                                                        OldNodePositions(i) = objChoiceStart.Choices(i).Distance
                                                    Next

                                                    'Update each node, if it is out of range of acceptable bounds
                                                    For i As Integer = StartIndex To EndIndex Step IncrementStep
                                                        Dim AcceptableValues As Single() = objChoiceStart.GetAllowedDistanceRangeForNode(i)
                                                        Dim MinDistance As Single = AcceptableValues(0)
                                                        Dim MaxDistance As Single = AcceptableValues(1)

                                                        If (i = EndIndex) AndAlso MaxDistance < MinDistance Then
                                                            'Last node hitting barrier. Can we bunch up intermediate nodes?
                                                            'Assume not.
                                                            ProblemExists = True
                                                            Dim Index As Integer = EndIndex
                                                            Do
                                                                Dim ChoiceDistance As Single = objChoiceStart.Choices(Index).Distance
                                                                Select Case True
                                                                    Case ChoiceDistance > objChoiceStart.GetAllowedDistanceRangeForNode(Index)(1)
                                                                        objChoiceStart.Choices(Index).Distance -= clsChoice.DisplayWidth
                                                                    Case ChoiceDistance < objChoiceStart.GetAllowedDistanceRangeForNode(Index)(0)
                                                                        objChoiceStart.Choices(Index).Distance += clsChoice.DisplayWidth
                                                                    Case Else
                                                                        ProblemExists = False
                                                                        Exit Do
                                                                End Select

                                                                Index -= IncrementStep
                                                            Loop While Index >= 0 AndAlso Index <= objChoiceStart.Choices.Count - 1
                                                            Exit For
                                                        End If

                                                        Dim c As clsChoice = objChoiceStart.Choices(i)
                                                        If MinDistance <= MaxDistance Then
                                                            c.Distance = Math.Max(Math.Min(c.Distance, MaxDistance), MinDistance)
                                                        Else
                                                            If StartIndex = 0 Then
                                                                c.Distance = MinDistance
                                                            Else
                                                                c.Distance = MaxDistance
                                                            End If
                                                        End If
                                                    Next

                                                    If ProblemExists Then
                                                        objstage.Location = OldStageLocation
                                                        For i As Integer = 0 To objChoiceStart.Choices.Count - 1
                                                            objChoiceStart.Choices(i).Distance = OldNodePositions(i)
                                                        Next
                                                    End If
                                                End If

                                                If mhtUndoMovedStages.ContainsKey(objstage) Then
                                                    'Update the position changes of this stage.
                                                    aPositionChanges = CType(mhtUndoMovedStages(objstage), Single())
                                                    aPositionChanges(0) += sngXMoved
                                                    aPositionChanges(1) += sngYMoved
                                                    mhtUndoMovedStages(objstage) = aPositionChanges
                                                Else
                                                    'Set the position changes of this stage.
                                                    mhtUndoMovedStages.Add(objstage, New Single() {sngXMoved, sngYMoved})
                                                End If

                                            ElseIf oProcessSelection.mtType = clsProcessSelection.SelectionType.ChoiceNode Then
                                                Dim objChoiceStart As clsChoiceStartStage = TryCast(Me.mProcess.GetStage(oProcessSelection.mgStageID), clsChoiceStartStage)
                                                Dim objChoiceEnd As clsChoiceEndStage = Me.mProcess.GetChoiceEnd(objChoiceStart)
                                                Dim choice As clsChoice = objChoiceStart.Choices(oProcessSelection.miChoiceIndex)

                                                Dim src As PointF = objChoiceStart.Location
                                                Dim dest As PointF = objChoiceEnd.Location
                                                Dim intersec As PointF = Me.PerpendicularIntersection(mpMousePos, src, dest)

                                                Dim dx As Single = intersec.X - src.X
                                                Dim dy As Single = intersec.Y - src.Y
                                                Dim distance As Single = CSng(Math.Sqrt(dx * dx + dy * dy))

                                                'Constrain the current choice node to within its neighbours
                                                Dim AcceptableValues As Single() = objChoiceStart.GetAllowedDistanceRangeForNode(oProcessSelection.miChoiceIndex)
                                                Dim MinDistance As Single = AcceptableValues(0)
                                                Dim MaxDistance As Single = AcceptableValues(1)
                                                distance = Math.Max(MinDistance, distance)
                                                distance = Math.Min(MaxDistance, distance)

                                                choice.Distance = distance

                                                mbNodeMoved = True
                                                'only one choice node can ever be selected at a time so we can 
                                                'skip the rest of the selected stages
                                                Exit For
                                            End If
                                        Next
                                        pbview.Cursor = myCurrentPointer


                                    End If

                                Case Else

                                    Dim sngWidthBefore, sngHeightBefore As Single
                                    Dim sngWidthChange, sngHeightChange As Single
                                    Dim aDimensionChanges As Single()

                                    If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then

                                        Dim objstage As clsProcessStage = mProcess.GetStage(CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID)

                                        sngWidthBefore = objstage.GetDisplayWidth
                                        sngHeightBefore = objstage.GetDisplayHeight
                                        Dim iCorner As Integer
                                        mProcess.GetSelectionOffset(objstage, Nothing, Nothing, iCorner)
                                        objstage.SetCorner(sngSX, sngSY, iCorner)
                                        sngWidthChange = objstage.GetDisplayWidth - sngWidthBefore
                                        sngHeightChange = objstage.GetDisplayHeight - sngHeightBefore

                                        If Math.Abs(sngWidthChange) > 0 Or Math.Abs(sngHeightChange) > 0 Then
                                            'Stage size has changed

                                            'Create an empty hashtable ready to hold the stage that has been resized.
                                            If mhtUndoResizedStages Is Nothing Then
                                                mhtUndoResizedStages = New Hashtable
                                            End If

                                            If mhtUndoResizedStages.ContainsKey(objstage) Then
                                                'Update the dimension changes of this stage.
                                                aDimensionChanges = CType(mhtUndoResizedStages(objstage), Single())
                                                aDimensionChanges(0) += sngWidthChange
                                                aDimensionChanges(1) += sngHeightChange
                                                mhtUndoResizedStages(objstage) = aDimensionChanges
                                            Else
                                                'Set the dimension changes of this stage.
                                                mhtUndoResizedStages.Add(objstage, New Single() {sngWidthChange, sngHeightChange})
                                            End If

                                        End If
                                    End If

                                    If Me.ModeIsEditable Then
                                        If (miMouseDragCorner = 1 OrElse miMouseDragCorner = 3) Then
                                            pbview.Cursor = Cursors.SizeNWSE
                                            mySizingPointer = Cursors.SizeNWSE
                                        Else
                                            pbview.Cursor = Cursors.SizeNESW
                                            mySizingPointer = Cursors.SizeNESW
                                        End If
                                    End If
                            End Select
                        End If
                End Select
                InvalidateView()
            Else
                'See if we're over any resize handles before we
                'select anything else...
                Dim CurrentWorldCoords As PointF = Me.GetWorldCoordsFromScreen(New Point(e.X, e.Y))
                If Me.ModeIsEditable AndAlso mProcess.SelectResizeHandles(CurrentWorldCoords.X, CurrentWorldCoords.Y, 5) Then
                    'Retrieve selection offset information...
                    If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.None Then
                        mProcess.GetSelectionOffset(mobjMouseDragStage, msngMouseDragXOffset, msngMouseDragYOffset, miMouseDragCorner)

                        If (miMouseDragCorner = 1 OrElse miMouseDragCorner = 3) Then
                            pbview.Cursor = Cursors.SizeNWSE
                            mySizingPointer = Cursors.SizeNWSE
                        Else
                            pbview.Cursor = Cursors.SizeNESW
                            mySizingPointer = Cursors.SizeNESW
                        End If
                    End If
                Else
                    pbview.Cursor = myCurrentPointer
                    mySizingPointer = Nothing
                End If
                InvalidateView()
            End If


        Catch ex As Exception
            Static MouseMoveErrorShown As Boolean = False
            If Not MouseMoveErrorShown Then
                UserMessage.Show(My.Resources.ctlProcessViewer_UnexpectedErrorDuringMousemoveEventHandlerPleaseReportThisProblemToBluePrism, ex)
                MouseMoveErrorShown = True
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Gets the point on the line between the src and dest points which
    ''' minimises the distance from this line to the specified mouse point.
    ''' </summary>
    ''' <param name="mouse">The mouse point of interest, in world coordinates.</param>
    ''' <param name="src">The first of the two points, lying on the line of interest.</param>
    ''' <param name="dest">The second of the two points, lying on the line of interest.</param>
    ''' <returns>Returns the point on the line which passes through the points
    ''' src and dest, which minimises the distance from that line to the specified
    ''' mouse point.</returns>
    Public Function PerpendicularIntersection(ByVal mouse As PointF, ByVal src As PointF, ByVal dest As PointF) As PointF

        Dim dx As Single = (dest.X - src.X)
        Dim dy As Single = (dest.Y - src.Y)
        Dim u As Single = (((mouse.X - src.X) * dx) + ((mouse.Y - src.Y) * dy)) / ((dx * dx) + (dy * dy))

        If u <= 0 Then
            Return src
        ElseIf u > 1 Then
            Return dest
        Else
            Dim i As PointF
            i.X = src.X + u * dx
            i.Y = src.Y + u * dy


            Dim adx As Single = Math.Abs(dx)
            Dim ady As Single = Math.Abs(dy)
            If adx = ady Then
                '45 degrees
                i.X = Snap(i.X)
                i.Y = Snap(i.Y)
            ElseIf adx > ady Then
                i.X = Snap(i.X)
            Else
                i.Y = Snap(i.Y)
            End If

            Return i
        End If

    End Function

    ''' <summary>
    ''' Generates a tooltip suitable for the specified stage and passes it on
    ''' to the rendering class.
    ''' </summary>
    ''' <param name="stg">The stage to generate a tooltip for.</param>
    Protected Overridable Sub SetToolTipForStage(ByVal stg As clsProcessStage)
        Dim stageType As String = String.Empty
        Dim stageName As String = String.Empty
        Dim txt As String = String.Empty
        Dim tipTxt As String
        Dim sInputs As String = String.Empty
        Dim sOutputs As String = String.Empty
        Dim i As Integer
        Dim objParameter As clsProcessParameter

        stageType = clsStageTypeName.GetLocalizedFriendlyName(stg.StageType.ToString())
        stageName = stg.Name.Left(MaxToolTipWidth - 10)

        Select Case stg.StageType

            Case StageTypes.Start
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Inputs & vbNewLine
                tipTxt &= FormatToolTipText(GetParameterToolTipText(stg))
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.End
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Outputs & vbNewLine
                tipTxt &= FormatToolTipText(GetParameterToolTipText(stg))
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.Decision
                Dim decStg As clsDecisionStage = CType(stg, clsDecisionStage)
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                txt = decStg.Expression.LocalForm
                If txt <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Logic & txt
                End If
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.Calculation
                Dim calcStg As clsCalculationStage = CType(stg, clsCalculationStage)
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                If calcStg.StoreIn <> "" Then _
                 tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Output & calcStg.StoreIn

                If stg.GetNarrative() <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(stg.GetNarrative())
                End If

            Case StageTypes.Action
                Dim objAction As clsActionStage = CType(stg, clsActionStage)
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                Dim sObjectName As String = Nothing
                Dim sActionName As String = Nothing
                objAction.GetResource(sObjectName, sActionName)
                If sObjectName <> "" Then
                    If sObjectName.IndexOf(".cls") > 0 Then
                        sActionName = CStr(IIf(sActionName <> "", objAction.GetActionFriendlyName(sObjectName, sActionName), sActionName))
                        sObjectName = sObjectName.Substring(sObjectName.IndexOf(".cls") + 4)
                    End If
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_ObjectColin & sObjectName
                End If
                If sActionName <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_MethodColin & sActionName
                End If
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.Skill
                Dim skillStage As clsSkillStage = CType(stg, clsSkillStage)
                tipTxt = stageType & ": " & stageName
                If skillStage.ActionName <> "" Then
                    tipTxt &= vbNewLine & String.Format(My.Resources.ctlProcessViewer_ToolTipActionName0, skillStage.ActionName)
                End If
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.Collection
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName

            Case StageTypes.Data
                Dim dataStg As clsDataStage = CType(stg, clsDataStage)
                tipTxt = My.Resources.ctlProcessViewer_DataItem & stageName
                txt = clsProcessDataTypes.GetFriendlyName(dataStg.GetDataType)
                If txt <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Type & txt
                End If
                txt = dataStg.GetShortValue(True)
                If txt <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_InitialValue & txt
                End If
                txt = dataStg.GetShortValue(False)
                If txt <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_CurrentValue & txt
                End If
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.SubSheetInfo
                tipTxt = My.Resources.ctlProcessViewer_ProcessColin & stageName
                tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_PageColin & stg.GetName

            Case StageTypes.SubSheet
                Dim objSheetRef As clsSubSheetRefStage = CType(stg, clsSubSheetRefStage)

                tipTxt = My.Resources.ctlProcessViewer_PageColin & stg.GetName.Left(MaxToolTipWidth - 10)
                txt = mProcess.GetSubSheetName(objSheetRef.ReferenceId)
                If txt <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_ReferenceColin & FormatToolTipText(txt)
                End If

                For i = 0 To stg.GetNumParameters - 1
                    objParameter = stg.GetParameter(i)
                    If objParameter.Direction = ParamDirection.In Then
                        sInputs &= vbNewLine & "  " & objParameter.FriendlyName
                        sInputs &= String.Format(My.Resources.ctlProcessViewer_ParamNameDataType0, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType))
                    Else
                        sOutputs &= vbNewLine & "  " & objParameter.FriendlyName
                        sOutputs &= String.Format(My.Resources.ctlProcessViewer_ParamNameDataType0, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType))
                    End If
                Next
                If sInputs <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Inputs & sInputs
                End If
                If sOutputs <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Outputs & sOutputs
                End If
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.LoopStart
                Dim objLoop As clsLoopStartStage = CType(stg, clsLoopStartStage)
                tipTxt = My.Resources.ctlProcessViewer_Loop & stg.GetName.Left(MaxToolTipWidth - 10)
                txt = objLoop.LoopData
                If txt <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Collection & txt.Mid(1, MaxToolTipWidth)
                End If
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case StageTypes.Process
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName

                For i = 0 To stg.GetNumParameters - 1
                    objParameter = stg.GetParameter(i)
                    If objParameter.Direction = ParamDirection.In Then
                        sInputs &= vbNewLine & "  " & objParameter.FriendlyName
                        sInputs &= String.Format(My.Resources.ctlProcessViewer_ParamNameDataType0, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType))
                    Else
                        sOutputs &= vbNewLine & "  " & objParameter.FriendlyName
                        sOutputs &= String.Format(My.Resources.ctlProcessViewer_ParamNameDataType0, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType))
                    End If
                Next
                If sInputs <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Inputs & FormatToolTipText(sInputs)
                End If
                If sOutputs <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Outputs & FormatToolTipText(sOutputs)
                End If

            Case StageTypes.Code
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName

                For i = 0 To stg.GetNumParameters - 1
                    objParameter = stg.GetParameter(i)
                    If objParameter.Direction = ParamDirection.In Then
                        sInputs &= vbNewLine & "  " & objParameter.FriendlyName
                        sInputs &= String.Format(My.Resources.ctlProcessViewer_ParamNameDataType0, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType))
                    Else
                        sOutputs &= vbNewLine & "  " & objParameter.FriendlyName
                        sOutputs &= String.Format(My.Resources.ctlProcessViewer_ParamNameDataType0, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType))
                    End If
                Next
                If sInputs <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Inputs & sInputs
                End If
                If sOutputs <> "" Then
                    tipTxt &= vbNewLine & My.Resources.ctlProcessViewer_Outputs & sOutputs
                End If

            Case StageTypes.Navigate,
             StageTypes.Read, StageTypes.Write,
             StageTypes.WaitStart, StageTypes.WaitEnd,
             StageTypes.ChoiceStart, StageTypes.ChoiceEnd

                'Tooltips possibly could be enhanced for these stages.

                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

            Case Else
                tipTxt = stageType & My.Resources.ctlProcessViewer_Colin & stageName
                txt = stg.GetNarrative
                If txt <> "" Then
                    tipTxt &= vbNewLine & FormatToolTipText(txt)
                End If

        End Select

        mRenderer.SetToolTipText(tipTxt)
    End Sub



    ''' <summary>
    ''' Little function for tooltip text to cut a string to a maximum length 
    ''' of 3 lines (crlf) each with a maximum of maxtooltipwidth characters.
    ''' If a line has more than maxtooltipwidth characters it will continue it on the
    ''' next line.
    ''' </summary>
    ''' <param name="sOrigText"></param>
    ''' <returns>The shortened string</returns>
    Private Function FormatToolTipText(ByVal sOrigText As String) As String
        Try
            Dim sReturn As String = String.Empty

            If Len(sOrigText) > 0 Then
                Dim iCount As Integer
                Dim iFindPosition As Integer
                Dim iStartPosition As Integer = 1
                Dim scheck1 As String
                Dim scheck2 As String
                Dim bNoReturns As Boolean
                'next find third occurance of iposition, if there is one
                Do
                    iCount = iCount + 1
                    iFindPosition = InStr(iStartPosition, sOrigText, vbCrLf)
                    If iFindPosition = 0 And Len(sOrigText) >= iStartPosition Then
                        If Len(sOrigText) > iStartPosition + MaxToolTipWidth Then
                            iFindPosition = iStartPosition + MaxToolTipWidth
                        Else
                            Return sOrigText
                        End If
                        bNoReturns = True
                    End If
                    If iFindPosition > 0 Then
                        scheck1 = sOrigText.Mid(iStartPosition, iFindPosition - iStartPosition)
                        scheck2 = scheck1.Left(MaxToolTipWidth)
                        sReturn += sOrigText.Mid(iStartPosition, iFindPosition - iStartPosition).Left(MaxToolTipWidth)
                        If iFindPosition <= iStartPosition + MaxToolTipWidth Then
                            sReturn += vbCrLf
                            iStartPosition = iFindPosition + 2
                        ElseIf iFindPosition > iStartPosition + MaxToolTipWidth Then
                            sReturn += vbCrLf
                            iStartPosition = iStartPosition + MaxToolTipWidth
                        Else
                            iStartPosition = iFindPosition + 2
                        End If
                        If bNoReturns Then iStartPosition = iFindPosition
                    End If
                    If iCount = 1 And iFindPosition = 0 Then
                        sReturn = sOrigText.Mid(MaxToolTipWidth)
                    End If
                Loop While (iCount < 3 And iFindPosition > 0)
            End If

            Return sReturn
        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlProcessViewer_InternalErrorCouldNotFormatTooltipText, ex)
        End Try

        Return String.Empty
    End Function


    Private Sub pbView_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pbview.MouseUp
        DoMouseUp(e.Button = System.Windows.Forms.MouseButtons.Right)
    End Sub

    Private Sub DoMouseUp(ByVal bRightButton As Boolean)

        Try
            Dim CurrentTool As StudioTool
            CurrentTool = GetCurrentTool()

            If bRightButton Then
                'We don't need to do anything when the right mouse button
                'goes up, since we dealt with it when it went down...
                'Except for the Zoom tool that is, where we need to
                'zoom out!!
                If CurrentTool = StudioTool.Zoom Then ZoomOut()
                Exit Sub
            End If


            'Release the mouse. Done first, because it must be
            'done no matter what.
            pbview.Capture = False
            Dim bWasDragging As Boolean = MouseDragging
            MouseDragging = False
            Me.miMouseDragCorner = 0

            'If in the middle of a paste operation, commit the paste and exit
            If Not Me.mbMouseDragging Then
                If ClipboardProcess IsNot Nothing Then
                    Me.CommitPaste()
                    Exit Sub
                End If
            End If

            'if we've not been dragging and we're not zooming with zoom tool then
            'select items on process diagram
            If ToolAllowsSelection(mTool) Then
                If Not mbMouseDragSelected AndAlso GetCurrentTool() <> StudioTool.Zoom Then
                    'Retrieve selection offset information...
                    If mProcess.GetSelectionType() <> clsProcessSelection.SelectionType.None Then
                        mProcess.GetSelectionOffset(mobjMouseDragStage, msngMouseDragXOffset, msngMouseDragYOffset, miMouseDragCorner)
                    End If
                End If
            End If

            'do selections for dragging selection rectangles
            If ToolAllowsSelection(mTool) Then
                Dim bExistsValidSelectionRectangle As Boolean = ((Math.Abs(mpMouseDownWorldPos.X - mpMousePos.X) > msngMinRubberBandSize OrElse Math.Abs(mpMouseDownWorldPos.Y - mpMousePos.Y) > msngMinRubberBandSize)) AndAlso (Me.mobjMouseDragStage Is Nothing)
                If bWasDragging AndAlso bExistsValidSelectionRectangle Then
                    Dim bExtend As Boolean = (Control.ModifierKeys = Keys.Shift OrElse Control.ModifierKeys = Keys.Control)
                    mProcess.SelectRegion(mpMouseDownWorldPos.X, mpMouseDownWorldPos.Y, mpMousePos.X, mpMousePos.Y, mProcess.GetActiveSubSheet, bExtend)
                    If TypeOf mobjParentForm Is frmProcess Then
                        CType(mobjParentForm, frmProcess).UpdateFormattingControls()
                    End If
                    InvalidateView()
                    Exit Sub
                End If
            End If

            'do whatever is necessary for pressing mouse using current tool
            If Me.ModeIsEditable Then

                Dim loc = New PointF(
                          mpMousePos.X - msngMouseDragXOffset,
                          mpMousePos.Y - msngMouseDragYOffset)
                loc = SnapTo(loc)

                Me.OnToolPressed(loc, CurrentTool, Nothing)

                'Check for stages that have been moved.
                Dim aPositionChanges, aDimensionChanges As Single()
                Dim oStage As clsProcessStage
                Dim aSaveUndoPositionStages As List(Of clsProcessStage)
                Dim IDE As IDictionaryEnumerator

                If Not mhtUndoMovedStages Is Nothing Then

                    aSaveUndoPositionStages = New List(Of clsProcessStage)
                    IDE = mhtUndoMovedStages.GetEnumerator

                    While IDE.MoveNext
                        oStage = CType(IDE.Key, clsProcessStage)
                        aPositionChanges = CType(IDE.Value, Single())
                        If Not SnapToGrid Or (Math.Abs(aPositionChanges(0)) >= ciGridSize / 2 Or Math.Abs(aPositionChanges(1)) >= ciGridSize / 2) Then
                            'Either snap to grid is off or the stage has moved at least one grid unit.
                            aSaveUndoPositionStages.Add(oStage)
                        End If
                    End While
                    mhtUndoMovedStages.Clear()

                    If aSaveUndoPositionStages.Count > 0 Then
                        'Stages have moved, so create an undo point.
                        mProcess.SaveUndoPosition(clsUndo.ActionType.ChangePositionOf, aSaveUndoPositionStages.ToArray())
                    End If

                End If

                'Check to see if a node has been moved
                If mbNodeMoved Then
                    mProcess.SaveUndoPosition(clsUndo.ActionType.ChangePositionOf, "node", "node")
                End If

                'Check if a stage has been resized.
                If Not mhtUndoResizedStages Is Nothing Then
                    'NB There will only ever be one stage in this hashtable.
                    aSaveUndoPositionStages = New List(Of clsProcessStage)
                    IDE = mhtUndoResizedStages.GetEnumerator

                    While IDE.MoveNext
                        oStage = CType(IDE.Key, clsProcessStage)
                        aDimensionChanges = CType(IDE.Value, Single())
                        If Math.Abs(aDimensionChanges(0)) > 0 Or Math.Abs(aDimensionChanges(1)) > 0 Then
                            aSaveUndoPositionStages.Add(oStage)
                        End If
                    End While
                    mhtUndoResizedStages.Clear()

                    If aSaveUndoPositionStages.Count > 0 Then
                        'A stage has been resized, so create an undo point.
                        mProcess.SaveUndoPosition(clsUndo.ActionType.ChangeSizeOf, aSaveUndoPositionStages.ToArray())
                    End If

                End If

                InvalidateView()

            End If

            If TypeOf mobjParentForm Is frmProcess Then
                CType(mobjParentForm, frmProcess).UpdateFormattingControls()
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_ThereWasAnErrorWhilstHandlingTheMouseEndTheErrorMessageWas0, ex.Message), ex)
        Finally
            LinkSource = Nothing
            EndMouseDragging()
        End Try

    End Sub

    ''' <summary>
    ''' Determines if the supplied tool allows
    ''' selections to be made.
    ''' </summary>
    ''' <param name="tool">The tool to be tested.</param>
    ''' <returns>True if selections can be made,
    ''' false otherwise.</returns>
    Private Function ToolAllowsSelection(ByVal tool As StudioTool) As Boolean
        Return (tool <> StudioTool.Zoom AndAlso
                tool <> StudioTool.Pan AndAlso
                tool <> StudioTool.Block)
    End Function

    Private Function StageFromTool(ByVal Tool As StudioTool) As StageTypes
        Select Case Tool
            Case StudioTool.Anchor
                Return StageTypes.Anchor
            Case StudioTool.Action
                Return StageTypes.Action
            Case StudioTool.Skill
                Return StageTypes.Skill
            Case StudioTool.Process
                Return StageTypes.Process
            Case StudioTool.Page
                Return StageTypes.SubSheet
            Case StudioTool.Note
                Return StageTypes.Note
            Case StudioTool.Calculation
                Return StageTypes.Calculation
            Case StudioTool.Data
                Return StageTypes.Data
            Case StudioTool.Collection
                Return StageTypes.Collection
            Case StudioTool.Loop
                Return StageTypes.LoopStart
            Case StudioTool.Decision
                Return StageTypes.Decision
            Case StudioTool.Start
                Return StageTypes.Start
            Case StudioTool.End
                Return StageTypes.End
            Case StudioTool.Choice
                Return StageTypes.ChoiceStart
            Case StudioTool.Wait
                Return StageTypes.WaitStart
            Case StudioTool.Code
                Return StageTypes.Code
            Case StudioTool.Read
                Return StageTypes.Read
            Case StudioTool.Write
                Return StageTypes.Write
            Case StudioTool.Navigate
                Return StageTypes.Navigate
            Case StudioTool.Alert
                Return StageTypes.Alert
            Case StudioTool.MultipleCalculation
                Return StageTypes.MultipleCalculation
            Case StudioTool.Exception
                Return StageTypes.Exception
            Case StudioTool.Recover
                Return StageTypes.Recover
            Case StudioTool.Resume
                Return StageTypes.Resume
            Case StudioTool.Block
                Return StageTypes.Block
        End Select
    End Function

    ''' <summary>
    ''' Makes the necessary modifications to the process object when the specified
    ''' tool is used at the specified location.
    ''' </summary>
    ''' <param name="loc">The location at which the tool was used, in
    ''' world coordinates.</param>
    ''' <param name="Tool">The tool used.</param>
    Private Sub OnToolPressed(loc As PointF, tool As StudioTool, context As Object)

        Dim stg As clsProcessStage = Nothing
        Dim initialName = mobjMouseDragStage?.InitialName
        Dim stgType As StageTypes = StageFromTool(tool)
        Select Case tool
            Case StudioTool.Zoom
                ZoomIn()
            Case StudioTool.Anchor,
                StudioTool.Action,
                StudioTool.Skill,
                StudioTool.Process,
                StudioTool.Calculation,
                StudioTool.Data,
                StudioTool.Decision,
                StudioTool.End,
                StudioTool.Note,
                StudioTool.Code,
                StudioTool.Read,
                StudioTool.Write,
                StudioTool.Navigate,
                StudioTool.Alert,
                StudioTool.MultipleCalculation,
                StudioTool.Exception,
                StudioTool.Recover,
                StudioTool.Resume

                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    stg = AddStage(stgType, initialName)
                    stg.SetContext(context)
                End If
            Case StudioTool.Page
                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    Dim wiz As New frmSubSheetWizard
                    wiz.SetEnvironmentColoursFromAncestor(Me)
                    wiz.SetProcess(mProcess)
                    wiz.ShowInTaskbar = False

                    Dim iResult As Integer = wiz.ShowDialog(Me)
                    'With the introduction of a third option in frmSubSheetWizard, 
                    'DialogResult.Yes has been used to indicate a new but unreferenced sub-sheet
                    If iResult = DialogResult.OK OrElse iResult = DialogResult.Yes Then

                        Dim gSubID As Guid
                        If wiz.GetExisting() Then
                            gSubID = mProcess.GetSubSheetID(wiz.GetName())
                        Else
                            'choose where to put our new sheet, snapping it 
                            'to the grid if required.
                            Dim StartStageXCoord, StartStageYCoord As Single
                            Dim EndStageXCoord, EndStageYCoord As Single
                            Me.mProcess.GetDefaultStartStageLocationForANewSubSheet(StartStageXCoord, StartStageYCoord)
                            Me.mProcess.GetDefaultEndStageLocationForANewSubSheet(EndStageXCoord, EndStageYCoord)
                            Me.SnapTo(StartStageXCoord, StartStageYCoord)
                            Me.SnapTo(EndStageXCoord, EndStageYCoord)

                            'Create new subsheet...
                            gSubID = mProcess.AddSubSheet(wiz.GetName(), StartStageXCoord, StartStageYCoord, EndStageXCoord, EndStageYCoord, Guid.NewGuid, False).ID

                        End If

                        If iResult = DialogResult.OK Then
                            'Add the reference object itself...
                            Dim objNewSheetRef As clsSubSheetRefStage = CType(AddStage(StageTypes.SubSheet, wiz.GetName()), clsSubSheetRefStage)

                            objNewSheetRef.ReferenceId = gSubID
                            objNewSheetRef.SetDisplayWidth(60)
                            objNewSheetRef.SetDisplayHeight(30)
                            objNewSheetRef.SetSubSheetID(mProcess.GetActiveSubSheet())

                            'Make this reference to trigger save undo position below.
                            stg = objNewSheetRef

                        End If

                    End If
                    wiz.Dispose()
                    UpdateSheetTabs()
                End If
            Case StudioTool.Collection
                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    'If nothing was selected, we clicked at a
                    'blank location, so add the object...
                    stg = AddStage(StageTypes.Collection, initialName)

                    ' Ensure that the stage has a value if we're currently running this process.
                    If mProcess.RunState <> ProcessRunState.Off Then
                        DirectCast(stg, clsCollectionStage).Value =
                         New clsProcessValue(New clsCollection())
                    End If
                End If
            Case StudioTool.Loop
                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    'If nothing was selected, we clicked at a
                    'blank location, so add the object...
                    Dim gGroupID As Guid
                    gGroupID = Guid.NewGuid()
                    stg = AddStage(StageTypes.LoopStart, initialName)
                    CType(stg, clsGroupStage).SetGroupID(gGroupID)
                    stg.SetDisplayX(loc.X)
                    stg.SetDisplayY(loc.Y)
                    stg.SetSubSheetID(mProcess.GetActiveSubSheet())
                    stg = AddStage(StageTypes.LoopEnd, Nothing)
                    stg.SetDisplayX(loc.X)
                    stg.SetDisplayY(loc.Y + 135)
                    stg.SetSubSheetID(mProcess.GetActiveSubSheet())
                    CType(stg, clsGroupStage).SetGroupID(gGroupID)
                    mProcess.SaveUndoPosition(clsUndo.ActionType.Create, stg)
                    stg = Nothing
                End If
            Case StudioTool.Start
                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    If mProcess.HasStartStage() Then
                        UserMessage.Show(My.Resources.ctlProcessViewer_ThisProcessAlreadyHasAStartYouCanTAddAnother)
                    Else
                        'If nothing was selected, we clicked at a
                        'blank location, so add the object...
                        stg = AddStage(StageTypes.Start, initialName)
                    End If
                End If
            Case StudioTool.Link
                Dim DestinationStage As clsProcessStage = mProcess.GetStageAtPosition(Me.mpMousePos)
                If (LinkSource Is Nothing) Then
                    Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_UsingTheLinkTool, My.Resources.ctlProcessViewer_CanNotCreateALinkFromThere, ToolTipIcon.Info)
                    InvalidateView()
                    Exit Sub
                End If
                If (DestinationStage Is Nothing) Then
                    Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_UsingTheLinkTool, My.Resources.ctlProcessViewer_InvalidDestinationForALink, ToolTipIcon.Info)
                    InvalidateView()
                    Exit Sub
                End If
                If LinkSource IsNot DestinationStage Then
                    Dim sErr As String = Nothing
                    If mProcess.CreateLink(LinkSource, DestinationStage, sErr) Then
                        LinkSource = Nothing
                        mProcess.SaveUndoPosition(clsUndo.ActionType.LinkTo, DestinationStage)
                    Else
                        LinkSource = Nothing
                        Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_UsingTheLinkTool, sErr, ToolTipIcon.Info)
                        InvalidateView()
                        Exit Sub
                    End If
                End If
            Case StudioTool.Choice
                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    'If nothing was selected, we clicked at a
                    'blank location, so add the object...
                    Dim gGroupID As Guid = Guid.NewGuid()
                    stg = AddStage(StageTypes.ChoiceStart, initialName)
                    CType(stg, clsGroupStage).SetGroupID(gGroupID)
                    stg.SetDisplayX(loc.X)
                    stg.SetDisplayY(loc.Y)
                    stg.SetSubSheetID(mProcess.GetActiveSubSheet())

                    stg = AddStage(StageTypes.ChoiceEnd, Nothing)
                    stg.SetDisplayX(loc.X)
                    stg.SetDisplayY(loc.Y + 120)
                    stg.SetSubSheetID(mProcess.GetActiveSubSheet())
                    CType(stg, clsGroupStage).SetGroupID(gGroupID)
                    mProcess.SaveUndoPosition(clsUndo.ActionType.Create, stg)
                    stg = Nothing
                End If
            Case StudioTool.Wait
                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    'If nothing was selected, we clicked at a blank location, so add the object...

                    'Add wait end
                    Dim gGroupID As Guid = Guid.NewGuid()
                    stg = AddStage(StageTypes.WaitEnd, Nothing)
                    stg.SetDisplayX(loc.X)
                    stg.SetDisplayY(loc.Y + 120)
                    stg.SetSubSheetID(mProcess.GetActiveSubSheet())
                    CType(stg, clsGroupStage).SetGroupID(gGroupID)

                    'Then add wait start - this leaves wait start as the selected stage
                    stg = AddStage(StageTypes.WaitStart, initialName)
                    CType(stg, clsGroupStage).SetGroupID(gGroupID)
                    stg.SetDisplayX(loc.X)
                    stg.SetDisplayY(loc.Y)
                    stg.SetSubSheetID(mProcess.GetActiveSubSheet())

                    mProcess.SaveUndoPosition(clsUndo.ActionType.Create, stg)
                    stg = Nothing
                End If

            Case StudioTool.Block

                If mProcess.GetSelectionType = clsProcessSelection.SelectionType.None Then
                    'If nothing was selected, we clicked at a blank location, so add the object...
                    stg = AddStage(StageTypes.Block, initialName)

                    Dim down As PointF = SnapTo(mpMouseDownWorldPos)

                    Dim sngWidth As Single = loc.X - down.X
                    Dim sngHeight As Single = loc.Y - down.Y
                    loc.X = down.X
                    loc.Y = down.Y

                    'Correct the rectangle if it was drawn from bottom-right to top-left.
                    'In this case it has negative width/height
                    If sngWidth < 0 Then
                        loc.X += sngWidth
                        sngWidth = -sngWidth
                    End If
                    If sngHeight < 0 Then
                        loc.Y += sngHeight
                        sngHeight = -sngHeight
                    End If

                    stg.SetDisplayWidth(sngWidth)
                    stg.SetDisplayHeight(sngHeight)
                End If
        End Select

        'Settings common to all new stages
        If Not stg Is Nothing Then
            stg.SetDisplayX(loc.X)
            stg.SetDisplayY(loc.Y)
            stg.SetSubSheetID(mProcess.GetActiveSubSheet())

            'end stages need to have their parameters update AFTER the subsheet id has been set.
            Dim objEnd As Stages.clsEndStage = TryCast(stg, Stages.clsEndStage)
            If Not objEnd Is Nothing Then
                objEnd.UpdateParametersToMatchMainEnd()
            End If

            'Add the latest state to the undo/redo buffer
            mProcess.SaveUndoPosition(clsUndo.ActionType.Create, stg)
        End If
    End Sub

    Private Sub pbView_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pbview.MouseLeave

        If DesignMode Then Exit Sub

        Me.ContextMenu = Nothing
        mRenderer.SetToolTipActive(False)
        InvalidateView()
    End Sub

#End Region

#End Region

#Region "tcPages Mouse Events"

    Private Sub tcPages_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles tcPages.MouseDown
        Me.ContextMenu = Nothing
        InvalidateView()
    End Sub

    Private Sub tcPages_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles tcPages.MouseUp
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            ShowTabBarContextMenu(New Point(e.X, e.Y))
        End If
    End Sub

    ''' <summary>
    ''' Shows the context menu for the tab bar.
    ''' </summary>
    ''' <param name="MenuLocation">Mouse </param>
    Protected Overridable Sub ShowTabBarContextMenu(ByVal MenuLocation As Point)
        Dim tab As TabPage = Nothing
        Dim sheet As clsProcessSubSheet = Nothing

        ' Determine if we've right-clicked on a tab
        For i As Integer = 0 To tcPages.TabPages.Count - 1
            Dim r As Rectangle = tcPages.GetTabRect(i)
            If r.Contains(MenuLocation) Then
                tab = tcPages.TabPages(i)
                tcPages.SelectedTab = tab

                sheet = mProcess.GetSubSheetByID(CType(tab.Tag, Guid))
                Exit For
            End If
        Next

        ' New - always available in edit mode
        If Me.ModeIsEditable Then
            mnuTabNew.Enabled = True
        Else
            mnuTabNew.Enabled = False
        End If
        ' Copy - available if normal tab selected
        If tab IsNot Nothing AndAlso sheet.IsNormal Then
            mnuTabCopy.Enabled = True
        Else
            mnuTabCopy.Enabled = False
        End If
        ' Duplicate/Cut/Rename/Delete - available if normal tab selected in edit mode
        If Me.ModeIsEditable AndAlso tab IsNot Nothing AndAlso sheet.IsNormal Then
            mnuTabDuplicate.Enabled = True
            mnuTabCut.Enabled = True
            mnuTabRename.Enabled = True
            mnuTabDelete.Enabled = True
        Else
            mnuTabDuplicate.Enabled = False
            mnuTabCut.Enabled = False
            mnuTabRename.Enabled = False
            mnuTabDelete.Enabled = False
        End If
        ' Paste - available in edit mode and clipboard contains pastable object
        If Me.ModeIsEditable AndAlso mProcess.CanPaste(True) Then
            mnuTabPaste.Enabled = True
        Else
            mnuTabPaste.Enabled = False
        End If
        ' Publish - available if normal tab selected in edit mode (Object studio only)
        If ModeIsObjectStudio() AndAlso tab IsNot Nothing Then
            If Me.ModeIsEditable AndAlso sheet.IsNormal Then
                mnuTabPublish.Enabled = True
            Else
                mnuTabPublish.Enabled = False
            End If
            mnuTabPublish.Checked = sheet.Published
            mnuTabPublish.Visible = True
        Else
            mnuTabPublish.Visible = False
        End If
        ' Find references - available in all modes for Initialise & action tabs (Object Studio)
        ' and for all tabs (Process Studio)
        If tab IsNot Nothing AndAlso (sheet.SheetType = SubsheetType.MainPage OrElse
                                      sheet.SheetType = SubsheetType.Normal) Then
            mnuFindReferences.Visible = True
        Else
            mnuFindReferences.Visible = False
        End If
        'Separator
        If mnuTabPublish.Visible OrElse mnuFindReferences.Visible Then
            mnuTabSep4.Visible = True
        Else
            mnuTabSep4.Visible = False
        End If
        ' Manage tabs - available in edit mode if more than one normal tab
        ' (i.e. more than Main in process studio, more than init&cleanup in object studio
        If Me.ModeIsEditable AndAlso tcPages.TabPages.Count > MinimumPagesAllowed + 1 Then
            mnuTabManage.Enabled = True
        Else
            mnuTabManage.Enabled = False
        End If

        tcPages.ContextMenuStrip.Show(tcPages, MenuLocation)
    End Sub

    Private Sub tcPages_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles tcPages.MouseLeave
        If DesignMode Then Exit Sub
        Me.ContextMenu = Nothing
    End Sub

#End Region

#Region "Mouse Scroll Wheel Events"

    ''' <summary>
    ''' Triggers a mousewheel event. This just allows other controls (eg.
    ''' <see cref="frmProcess"/>, <see cref="frmProcessComparison"/>) to send a
    ''' mousewheel event to this process viewer control if such an event is detected
    ''' while this control doesn't have focus and thus hasn't handled it itself.
    ''' </summary>
    Friend Sub DoMouseWheel(ByVal e As MouseEventArgs)
        OnMouseWheel(e)
    End Sub

    ''' <summary>
    ''' Handles mousewheel events which occur while this control has focus.
    ''' </summary>
    Protected Overrides Sub OnMouseWheel(ByVal e As MouseEventArgs)
        If Not Me.MouseWheelEnabled Then Exit Sub
        MyBase.OnMouseWheel(e)

        'Decide how much we will move
        Dim Increment As Single = CSng(1.4 * ciGridSize)

        ' Shift being pressed? Zoom.
        If (Control.ModifierKeys And Keys.Shift) <> 0 Then
            'we zoom in and out
            If e.Delta < 0 Then
                ZoomOut()
            Else
                ZoomIn()
            End If
            InvalidateView()
            Exit Sub

            ' Ctl being pressed? Left/Right Translation
        ElseIf (Control.ModifierKeys And Keys.Control) <> 0 Then
            'we move left and right
            If e.Delta < 0 Then
                mProcess.SetCameraX(mProcess.GetCameraX - Increment)
                InvalidateView()
            Else
                mProcess.SetCameraX(mProcess.GetCameraX + Increment)
                InvalidateView()
            End If
            Exit Sub

            ' No modifier keys? Up/Down Translation
        Else
            'we move up and down
            If e.Delta < 0 Then
                mProcess.SetCameraY(mProcess.GetCameraY + Increment)
                InvalidateView()
            Else
                mProcess.SetCameraY(mProcess.GetCameraY - Increment)
                InvalidateView()
            End If
        End If
    End Sub

#End Region

#Region "Passed In Mouse Events"

    Public Sub DoMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.X > Me.pbview.Left And e.Y < Me.pbview.Left + Me.pbview.Left + Me.pbview.Width _
          And e.Y > Me.pbview.Top And e.Y < Me.pbview.Top + Me.pbview.Top + Me.pbview.Height Then
            Me.pbView_MouseUp(sender, e)
        End If
    End Sub

    Public Sub DoMouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.X > Me.pbview.Left And e.Y < Me.pbview.Left + Me.pbview.Left + Me.pbview.Width _
         And e.Y > Me.pbview.Top And e.Y < Me.pbview.Top + Me.pbview.Top + Me.pbview.Height Then
            Me.pbView_MouseDown(sender, e)
        End If
    End Sub

    Public Sub DoMouseDoubleClick(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.pbView_DoubleClick(sender, e)
    End Sub

#End Region

    Private Sub UnlockProcessesLinkLabelClicked_Click(ByVal sender As Object, ByVal e As EventArgs)
        If Not Me.mSuperParent Is Nothing Then
            Dim WizardMode As frmWizard.WizardType
            If ModeIsObjectStudio() Then
                WizardMode = frmWizard.WizardType.BusinessObject
            Else
                WizardMode = frmWizard.WizardType.Process
            End If
            mSuperParent.StartForm(New frmSystemUnlock(WizardMode), True)
        End If
    End Sub

#Region "tab page events"

    Private Sub tcPages_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tcPages.SelectedIndexChanged
        Try
            'If the form has not finished loading yet, do nothing. This
            'can happen, since this event will get fired during
            'InitializeComponent().
            If Not mbLoaded Then
                Exit Sub
            End If

            Dim t As TabPage
            Dim gID As Guid
            t = tcPages.SelectedTab
            If Not t Is Nothing Then
                If t.Name = "Main Page" Then

                    gID = Guid.Empty
                    If TypeOf mobjParentForm Is frmProcess Then
                        CType(mobjParentForm, frmProcess).SetPageAsExportable(False)
                    End If
                ElseIf t.Name <> "" Then
                    gID = New Guid(t.Name)
                    If TypeOf mobjParentForm Is frmProcess Then
                        CType(mobjParentForm, frmProcess).SetPageAsExportable(True)
                    End If
                End If
            End If

            Me.SelectTabSelectorByGuid(gID)

            If Not mProcess.GetActiveSubSheet.Equals(gID) Then
                mProcess.SetActiveSubSheet(gID)
                UpdateStartParamsEnabled(gID)
                InvalidateView()
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_InternalErrorAnUnexpectedErrorWasEncounteredTheErrorMessageWas0PleaseContactBlu, ex.Message), ex)
        End Try
    End Sub

#End Region

#Region "timer events"

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'This is the number of stages to run each tick when RunAtNearFullSpeed is true
        Const stagesRunAtFullSpeedDuringTick = 50

        If Not mProcess Is Nothing Then
            If mProcess.RunState = ProcessRunState.Running Then
                If Not mbRunningStep Then
                    Try
                        mbRunningStep = True
                        Timer1.Stop()

                        If RunAtNearFullSpeed Then
                            Dim stages = 0
                            While mProcess.RunState = ProcessRunState.Running AndAlso stages <= stagesRunAtFullSpeedDuringTick
                                If Not DebugAction(ProcessRunAction.RunNextStep, True) Then Exit While
                                stages += 1
                            End While

                            ShowStage(mProcess.GetStage(mProcess.RunStageID), False)
                            InvalidateView()
                        Else
                            DebugAction(ProcessRunAction.RunNextStep, True)
                        End If

                        'The parent process form can end up closing during the debug
                        'action (e.g. if running returns to a parent process). In that
                        'case we need to make sure we don't restart the timer, otherwise
                        'we negate the stopping of it that happens when the form closes.
                        If Not mClosed Then
                            Timer1.Start()
                        End If
                    Catch ee As Exception
                        UserMessage.ShowExceptionMessage(ee)
                        CType(Me.mobjParentForm, frmProcess).RunStateChanged(ProcessRunState.Paused)
                    Finally
                        mbRunningStep = False
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick

        If DesignMode Then Exit Sub

        If mRenderer Is Nothing Then Exit Sub

        If mRenderer.IsToolTipActive Then
            If Not mRenderer.IsToolTipIn Then
                mRenderer.SetToolTipIn(True)
                InvalidateView()
            Else
                If Now.Subtract(Me.mRenderer.TimeLastToolTipWasShown).CompareTo(mRenderer.ToolTipDurations) >= 0 Then
                    Me.mRenderer.SetToolTipActive(False)
                    mRenderer.SetToolTipIn(False)
                    InvalidateView()
                End If
            End If
        End If

    End Sub

#End Region

#Region "Autosave"

    Private Sub OnAutosavePerformed(ByVal e As clsAutoSaver.BackupEventArgs) Handles mAutoSaver.AutosavePerformed
        If Not Me.mobjParentForm Is Nothing Then
            CType(Me.mobjParentForm, IProcessViewingForm).SetStatusBarText(String.Format(My.Resources.ctlProcessViewer_AutosaveBackupMadeAt0NextAutosaveWillOccurAt1, Now.ToShortTimeString, Now.AddMinutes(Me.mAutoSaver.Interval).ToShortTimeString), 0)
        End If
    End Sub

    Private Sub OnAutosaveNotPerformed(ByVal e As Exception) Handles mAutoSaver.AutosaveError
        If InvokeRequired Then
            Invoke(New Action(Of Exception)(AddressOf AutosaveError), e)
        Else
            AutosaveError(e)
        End If
    End Sub

    Private Sub AutosaveError(ByVal e As Exception)
        mAutoSaver.Dispose()
        mAutoSaver = Nothing
        If TypeOf e Is LockUnavailableException Then
            If Not mLostLock Then UserHasLock()
        Else
            UserMessage.Show(My.Resources.ctlProcessViewer_AnErrorHasOccuredWhileTryingToPerformAnAutomatedBackUp & vbCrLf & vbCrLf & e.Message)
        End If
        If Not Me.mobjParentForm Is Nothing Then
            CType(Me.mobjParentForm, IProcessViewingForm).SetStatusBarText(My.Resources.ctlProcessViewer_AutosaveUnavailable, 0)
        End If
    End Sub

    ''' <summary>
    ''' Called by the parent form to update the status bar text when the backup interval
    ''' has been changed.
    ''' </summary>
    Public Sub BackupIntervalChanged()

        Dim sStatusBarText As String

        If mobjParentForm IsNot Nothing AndAlso AutoSaver IsNot Nothing Then
            If AutoSaver.Interval = 0 Then
                sStatusBarText = My.Resources.ctlProcessViewer_AutosaveSwitchedOff
            Else
                If AutoSaver.Interval = 1 Then
                    sStatusBarText = My.Resources.ctlProcessViewer_AutosaveSetToOccurEveryMinute
                Else
                    sStatusBarText = String.Format(My.Resources.ctlProcessViewer_AutosaveSetToOccurEvery0Minutes, AutoSaver.Interval.ToString())
                End If
                sStatusBarText &= String.Format(My.Resources.ctlProcessViewer_TheNextAutosaveWillBeAt0, Now.AddMinutes(AutoSaver.Interval).ToShortTimeString())
            End If
            CType(Me.mobjParentForm, IProcessViewingForm).SetStatusBarText(sStatusBarText)
        End If
    End Sub

#End Region

    Private Sub ctlProcessViewer_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            ShowTabBarContextMenu(New Point(e.X, e.Y))
        End If
    End Sub

    Private Sub ctlProcessViewer_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize, MyBase.SizeChanged
        Me.pbview.Invalidate()
    End Sub

#End Region

#Region "Methods"

#Region "ToolTip Methods"

    Private Function GetParameterToolTipText(ByVal stage As clsProcessStage) As String

        Dim sToolTipText As String = ""
        For i As Integer = 0 To stage.GetNumParameters() - 1
            Dim objParameter As clsProcessParameter = stage.GetParameter(i)

            Dim sValue As String = ""
            Dim xdoc As New Xml.XmlDocument
            Dim Inputs As clsArgumentList = mProcess.GetInputParams
            If Not Inputs Is Nothing Then
                Dim objParam As clsArgument = Inputs.GetArgumentByName(objParameter.Name)
                If Not objParam Is Nothing Then
                    sValue = objParam.Value.FormattedValue
                End If
            End If
            sToolTipText &= String.Format(My.Resources.ctlProcessViewer_TipParamNameDataType012, objParameter.FriendlyName, clsProcessDataTypes.GetFriendlyName(objParameter.GetDataType), sValue) & vbNewLine
        Next

        Return sToolTipText
    End Function

#End Region

#Region "Stage properties"

    ''' <summary>
    ''' Opens a new form allowing the user to modify the properties of the
    ''' supplied stage in the Automate Process. The form opened depends on the
    ''' type of stage.
    ''' 
    ''' A clone of the stage will be created, so the stage object will
    ''' not be modified unless the user commits the changes.
    ''' </summary>
    Public Sub LaunchStageProperties(ByVal fromStage As clsProcessStage)
        ' we do not want to open multiple of the same properties window
        Dim propertyForm As frmProperties = Nothing
        mPropertyWindows.TryGetValue(fromStage.Id, propertyForm)
        If propertyForm IsNot Nothing Then
            propertyForm.WindowState = FormWindowState.Normal
            propertyForm.BringToFront()
            Return
        End If

        Dim stage As clsProcessStage = fromStage.Clone
        Dim Result As DialogResult = DialogResult.Abort

        If stage Is Nothing Then Exit Sub

        'Bug 2753. Parameters should be disabled in object studio
        'cleanup and initialise pages
        Dim disableStartEndParams As Boolean =
         (ModeIsObjectStudio() AndAlso Not mProcess.GetActiveSubSheetRef().IsNormal)

        Try
            Select Case stage.StageType
                Case StageTypes.Decision
                    propertyForm = New frmStagePropertiesDecision
                Case StageTypes.Data
                    propertyForm = New frmStagePropertiesData
                Case StageTypes.Collection
                    propertyForm = New frmStagePropertiesCollection(Me.ModeIsEditable)
                Case StageTypes.LoopStart
                    propertyForm = New frmStagePropertiesLoopStart
                Case StageTypes.LoopEnd, StageTypes.ChoiceEnd, StageTypes.WaitEnd
                    propertyForm = New frmStagePropertiesGroupEnd
                Case StageTypes.Note
                    propertyForm = New frmStagePropertiesNote
                Case StageTypes.Calculation
                    propertyForm = New frmStagePropertiesCalculation
                Case StageTypes.ProcessInfo
                    If ModeIsObjectStudio() Then
                        propertyForm = New frmStagePropertiesObjectInfo(Me.mProcessId)
                    Else
                        propertyForm = New frmStagePropertiesProcessInfo(Me.mProcessId)
                    End If
                Case StageTypes.SubSheetInfo
                    propertyForm = New frmStagePropertiesSubSheetInfo()
                Case StageTypes.Start
                    propertyForm = New frmStagePropertiesStart
                    If disableStartEndParams Then
                        CType(propertyForm, frmStagePropertiesStart).mInputsOutputsConditions.Enabled = False
                    End If
                Case StageTypes.End
                    propertyForm = New frmStagePropertiesEnd
                    If disableStartEndParams Then
                        CType(propertyForm, frmStagePropertiesEnd).mInputsOutputsConditions.Enabled = False
                    End If
                Case StageTypes.Action
                    propertyForm = New frmStagePropertiesAction
                Case StageTypes.Skill
                    propertyForm = New frmStagePropertiesSkill
                Case StageTypes.Process
                    propertyForm = New frmStagePropertiesProcess
                Case StageTypes.SubSheet
                    propertyForm = New frmStagePropertiesSubSheet
                Case StageTypes.Anchor
                    Exit Sub
                Case StageTypes.ChoiceStart
                    Dim ChoiceProp As New frmStagePropertiesChoice
                    ChoiceProp.ChoiceEnd = mProcess.GetChoiceEnd(TryCast(fromStage, clsChoiceStartStage))
                    propertyForm = ChoiceProp
                Case StageTypes.Read
                    propertyForm = New frmStagePropertiesRead
                Case StageTypes.Code
                    propertyForm = New frmStagePropertiesCode
                Case StageTypes.Write
                    propertyForm = New frmStagePropertiesWrite
                Case StageTypes.Navigate
                    propertyForm = New frmStagePropertiesNavigate
                Case StageTypes.WaitStart
                    Dim WaitProp As New frmStagePropertiesWait
                    WaitProp.WaitEnd = TryCast(mProcess.GetChoiceEnd(TryCast(fromStage, clsWaitStartStage)), clsWaitEndStage)
                    propertyForm = WaitProp
                Case StageTypes.Alert
                    propertyForm = New frmStagePropertiesAlert
                Case StageTypes.Exception
                    propertyForm = New frmStagePropertiesException
                Case StageTypes.Recover
                    propertyForm = New frmStagePropertiesRecover
                Case StageTypes.Resume
                    propertyForm = New frmStagePropertiesResume
                Case StageTypes.Block
                    propertyForm = New frmStagePropertiesBlock
                Case StageTypes.MultipleCalculation
                    propertyForm = New frmStagePropertiesMultipleCalculation
                Case Else
                    UserMessage.Show(My.Resources.ctlProcessViewer_ThereIsNoPropertiesFormForThisTypeOfStage)
                    Exit Sub
            End Select

            If ModeIsEditable() Then
                propertyForm.ProcessViewer = Me
            End If

            propertyForm.SetParentForm(mobjParentForm)
            propertyForm.IsEditable = Me.ModeIsEditable
            propertyForm.ProcessStage = stage
            propertyForm.SetEnvironmentColoursFromAncestor(mobjParentForm)

            If Me.ModeIsObjectStudio Then
                propertyForm.PrepareForObjectStudio()
            End If

            propertyForm.DialogResult = If(propertyForm.IsEditable, DialogResult.Abort, DialogResult.Cancel)
            AddHandler propertyForm.FormClosed, AddressOf OnChildPropertiesClosed
            mPropertyWindows.Add(propertyForm.ProcessStage.Id, propertyForm)
            propertyForm.Show()
        Catch ex As Exception
            Dim s As String
            If propertyForm Is Nothing OrElse propertyForm.Text = "" Then
                s = My.Resources.ctlProcessViewer_ThisFormHasEncounteredAnUnknownErrorAndHasBeenForcedToClosePleaseContactBluePri
            Else
                s = String.Format(My.Resources.ctlProcessViewer_TheForm0HasEncounteredAnUnknownErrorAndHasBeenForcedToClosePleaseContactBluePri, propertyForm.Text)
            End If
            UserMessage.Show(s, ex)
        End Try

        'Update expression edit row
        UpdateExpressionEditRow(stage)

        'Update tab control - published flag of a subsheet may have changed
        SetTabPageHighlight(
         tcPages.SelectedTab, mProcess.GetActiveSubSheetRef().Published)
    End Sub

    Private Sub OnChildPropertiesClosed(sender As Object, e As FormClosedEventArgs)
        If TypeOf sender IsNot frmProperties Then
            UserMessage.Err(My.Resources.ctlProcessViewer_ThereWasAnErrorClosingStageProperties,
                            New InvalidArgumentException($"Stage property close handler expected {NameOf(sender)} to be {GetType(frmProperties)}"))
            Exit Sub
        End If

        Dim form = DirectCast(sender, frmProperties)
        mPropertyWindows.Remove(form.ProcessStage.Id)

        Dim stage = form.ProcessStage
        Dim fromStage = Process.GetStage(form.ProcessStage.Id)

        UpdateExpressionEditRow(stage)

        If (form.DialogResult And DialogResult.Abort And DialogResult.Cancel) <> 0 Then Return

        ResetExternalProperties(fromStage, stage)

        mProcess.SetStage(stage.GetStageID, stage)
        If stage.StageType = StageTypes.Start Then
            UpdateStartParamsEnabled(stage.GetSubSheetID())
        ElseIf stage.StageType = StageTypes.ProcessInfo AndAlso
            mProcess.Attributes = ProcessAttributes.Published Then
            DoValidate(DoValidateAction.PublishProcess)
        End If

        'add latest state to undo buffer
        mProcess.SaveUndoPosition(clsUndo.ActionType.ChangePropertiesOf, stage)
        ' manually call to update any clsDataStages that depend on this
        Dim dataStage = TryCast(form.ProcessStage, clsDataStage)
        If dataStage IsNot Nothing Then
            RefreshTreeDataItems(dataStage)
        End If

        'as the dialog is no longer modal, we need to force a refresh of the display at this point. 
        InvalidateView()
        ' raise the event if this process has been launched from a clsDataStage
        RaiseEvent PropertySaved(form, Nothing)
    End Sub

    Private Sub ResetExternalProperties(fromStage As clsProcessStage, toStage As clsProcessStage)
        ' generic settings that will always be maintained as they
        ' cannot be edited from within the properties
        toStage.BreakPoint = fromStage.BreakPoint
        toStage.Location = fromStage.Location
        toStage.Size = fromStage.Size
        toStage.FontFamily = fromStage.FontFamily
        toStage.FontSize = fromStage.FontSize
        toStage.FontSizeUnit = fromStage.FontSizeUnit
        toStage.FontStyle = fromStage.FontStyle

        toStage.Color = fromStage.Color
        toStage.LinkColour = fromStage.LinkColour

        ' Reset links if necessary
        Dim toLinkable = TryCast(toStage, clsLinkableStage)
        Dim fromLinkable = TryCast(fromStage, clsLinkableStage)
        If toLinkable IsNot Nothing AndAlso fromLinkable IsNot Nothing
            toLinkable.OnSuccess = fromLinkable.OnSuccess
        End If

        ' Reset decisionLinks if necessary
        Dim toDecision = TryCast(toStage, clsDecisionStage)
        Dim fromDecision = TryCast(fromStage, clsDecisionStage)
        If toDecision IsNot Nothing AndAlso fromDecision IsNot Nothing
            toDecision.OnTrue = fromDecision.OnTrue
            toDecision.OnFalse = fromDecision.OnFalse
        End If
    End Sub

    Private Sub RefreshTreeDataItems(stage As clsDataStage)
        For Each frm In mPropertyWindows.Values.OfType(Of IDataItemTreeRefresher)
            frm.Repopulate(stage)
        Next
    End Sub

    Private Sub RemoveTreeDataItem(stage As clsDataStage)
        For Each frm In mPropertyWindows.Values.OfType(Of IDataItemTreeRefresher)
            frm.Remove(stage)
        Next
    End Sub

    Public Sub LaunchSelectedStageProperties()
        Try
            If mProcess.GetSelectionType() = clsProcessSelection.SelectionType.Stage Then
                Dim gStageID As Guid
                gStageID = CType(mProcess.SelectionContainer.PrimarySelection, clsProcessSelection).mgStageID
                LaunchStageProperties(CType(mProcess.GetStage(gStageID), clsProcessStage))
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_ThereWasAnErrorLaunchingStagePropertiesTheErrorMessageWas0, ex.Message), ex)
        End Try
    End Sub

#End Region

#Region "CalculationZoom"
    ''' <summary>
    ''' Displays the calculation zoom form for the specified stage. If another calc
    ''' zoom form is already being displayed elsewhere then it will be replaced.
    ''' </summary>
    ''' <param name="objStage">The stage for which a calc zoom form should be displayed.
    ''' </param>
    Public Sub ShowCalculationZoomForStage(ByVal objStage As clsProcessStage)

        If mProcess.RunState = ProcessRunState.Paused Then
            mobjCalculationZoom = New frmCalculationZoom(objStage)
        ElseIf mProcess.RunState = ProcessRunState.Running Then
            mobjCalculationZoom = New frmCalculationZoom(objStage, Timer1.Interval)
        Else
            mobjCalculationZoom = Nothing
        End If

        If mobjCalculationZoom IsNot Nothing Then
            mobjCalculationZoom.SetParentProcessViewer(Me)
            mobjParentForm.AddOwnedForm(mobjCalculationZoom)
            mobjCalculationZoom.Show()
            'But Focus the frmProcess Window:
            mobjParentForm.Focus()
        End If

    End Sub

    Public Sub SetCalcZoomMenuOption(ByVal value As Boolean)
        If TypeOf mobjParentForm Is frmProcess Then
            CType(mobjParentForm, frmProcess).SetCalcZoomEnabledChecked(value)
        End If
    End Sub

#End Region

#Region "Cut, Copy, Paste, Undo, Redo etc"

    ''' <summary>
    ''' Selects all stages on the current sheet.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DoSelectAll()

        mProcess.SelectAll(CType(tcPages.SelectedTab.Tag, Guid))
        InvalidateView()
    End Sub

    ''' <summary>
    ''' Checks that the current selection is valid for copying to the clipboard.
    ''' 
    ''' Any selection which does not contain a grouped stage is valid.
    ''' Where the selection includes a stage belonging to a group, the user is
    ''' prevented from copying the selection to the clipboard unless all stages
    ''' in that group are contained in the selection. A message is shown in the 
    ''' event that validation fails.
    ''' 
    ''' If the selection is valid, copies the XML representing the current
    ''' on-screen selection to the windows clipboard.
    ''' </summary>
    Public Sub DoCopy()
        Dim sErr As String = Nothing
        If mProcess.CanCopy(sErr) Then

            'inform the user of missing stages from the selection and quit


            'finally copy the selection xml to the clipboard
            Dim sXML As String
            sXML = mProcess.GenerateSelectionXML()
            Clipboard.SetDataObject(sXML)

            If TypeOf mobjParentForm Is frmProcess Then
                'this is always true,true since there is a valid
                'selection, and valid data on the clipboard.
                CType(mobjParentForm, frmProcess).SetEditMenuOptions(True, True)
            End If
        Else
            ShowFloatingMessage(My.Resources.ctlProcessViewer_CopyError, sErr, ToolTipIcon.Info)
        End If
    End Sub

    ''' <summary>
    ''' Removes the current selection from the process diagram and adds its XML
    ''' representation to the clipboard.
    ''' </summary>
    Public Sub DoCut()

        If Not ModeIsEditable() Then
            Return
        End If

        Dim sErr As String = Nothing
        If mProcess.CanCut(sErr) Then
            Dim sXML As String
            sXML = mProcess.GenerateSelectionXML()
            Clipboard.SetDataObject(sXML)
            DoDeleteSelection()
            ValidateOpenPropertiesWindows()
            InvalidateView()
        Else
            ShowFloatingMessage(My.Resources.ctlProcessViewer_CutError, sErr, ToolTipIcon.Info)
        End If
    End Sub


    ''' <summary>
    ''' Perform a paste operation.
    ''' </summary>
    Public Sub DoPaste()
        DoPaste(PointF.Empty)
    End Sub

    ''' <summary>
    ''' Perform a paste operation.
    ''' </summary>
    ''' <param name="Position">The position to paste at</param>
    Public Sub DoPaste(ByVal Position As PointF)

        If Not ModeIsEditable() Then Return

        If mProcess.CanPaste() Then

            Dim sNarrative As String = Nothing
            Dim objData As IDataObject = Clipboard.GetDataObject
            Dim sXML As String = CStr(objData.GetData(DataFormats.StringFormat, True))

            If String.IsNullOrEmpty(sXML) Then
                sNarrative = My.Resources.ctlProcessViewer_CanTPasteThatDataIntoAProcess
            Else
                Position = SnapTo(Position)
                Dim sErr As String = Nothing
                Dim sReport As String = Nothing

                'Communicate the potential paste to the renderer. It
                'takes another user interaction to confirm the paste,
                'possibly after moving the clipboard process round a bit
                Dim objClipboardProcess As clsProcess = mProcess.GetPastableProcess(sXML, sReport, sErr)
                If objClipboardProcess Is Nothing Then
                    UserMessage.Show(sErr)
                    Exit Sub
                Else
                    mProcess.SelectionContainer.ClearAllSelections()
                    Me.mobjMouseDragStage = Nothing
                    ClipboardProcess = objClipboardProcess
                    Me.MouseDragging = True
                End If

                If Not String.IsNullOrEmpty(sReport) Then
                    Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_PasteInformation, sReport, ToolTipIcon.Info)
                Else
                    Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_PasteInformation, My.Resources.ctlProcessViewer_MoveTheMouseToPositionTheClipboardStagesThenClickToConfirmYourChosenPositionPre, ToolTipIcon.Info)
                End If
            End If

            Me.ClipboardProcessLocation = Me.mpMouseDownWorldPos
            InvalidateView()
        Else
            ShowFloatingMessage(My.Resources.ctlProcessViewer_PasteError, My.Resources.ctlProcessViewer_PasteIsNotAvailableBecauseThereIsNoStageDataOnTheClipboard, ToolTipIcon.Info)
        End If
    End Sub

    ''' <summary>
    ''' Commits the half-complete paste operation, if any.
    ''' </summary>
    ''' <returns>Returns true if a paste operation was commited.</returns>
    Public Function CommitPaste() As Boolean
        If Me.ClipboardProcess IsNot Nothing Then
            Dim sNarrative As String = Nothing
            Dim sErr As String = Nothing
            If mProcess.PasteProcess(ClipboardProcess, Me.SnapTo(Me.mpMouseDownWorldPos), sNarrative, sErr) Then
                'Add the latest state to the undo/redo buffer
                mProcess.SaveUndoPosition(clsUndo.ActionType.Paste)

                If Not String.IsNullOrEmpty(sNarrative) Then
                    UserMessage.Show(sNarrative)
                End If

                Me.CancelPaste(False)
                Return True
            Else
                UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_PasteFailed0, sErr))
                Return False
            End If
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Cancels any paste operation underway.
    ''' </summary>
    ''' <param name="ShowCancelationMessage">When true, a non-intrusive
    ''' message will be shown to the user to indicate that the paste
    ''' operation was canceled.
    ''' </param>
    ''' <remarks>Safe to call any time - if no paste
    ''' operation is underway then no action is taken.
    ''' </remarks>
    Public Sub CancelPaste(Optional ByVal ShowCancelationMessage As Boolean = True)
        If Me.ClipboardProcess IsNot Nothing Then
            Me.ClipboardProcess.Dispose()
            Me.ClipboardProcess = Nothing
        End If
        If ShowCancelationMessage Then
            Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_Cancelation, My.Resources.ctlProcessViewer_PasteOperationCanceled, ToolTipIcon.Info)
        Else
            UserMessage.CancelFloatingMessage(Me.pbview)
        End If
        Me.MouseDragging = False
        Me.InvalidateView()
    End Sub

    ''' <summary>
    ''' Shows a floating message at the top-left of the process
    ''' display picturebox.
    ''' </summary>
    ''' <param name="Title">The title of the message.</param>
    ''' <param name="Message">Th emessage content.</param>
    ''' <param name="IconType">The type of icon to be displayed.</param>
    Public Sub ShowFloatingMessage(ByVal Title As String, ByVal Message As String, ByVal IconType As ToolTipIcon)
        UserMessage.ShowFloating(Me.pbview, IconType, Title, Message, Point.Empty)
    End Sub

    ''' <summary>
    ''' Determine if an edit->copy operation is valid at this point
    ''' in time.
    ''' </summary>
    ''' <returns>True if there is something that can be copied
    ''' or False otherwise.</returns>
    Public Function CanCopy() As Boolean
        If mProcess Is Nothing Then
            CanCopy = False
        Else
            Dim sErr As String = Nothing
            CanCopy = mProcess.CanCopy(sErr)
        End If
    End Function

    ''' <summary>
    ''' Restores the state of the process object and the user interface to the
    ''' previous states in the undo buffer.
    ''' </summary>
    Public Function Undo(n As Integer) As Boolean

        Try
            Dim sErr As String = Nothing
            If Not mProcess Is Nothing Then
                If mPropertyWindows.Any() AndAlso
                    UserMessage.YesNo(My.Resources.PerformingAnUndoRedoOperationWillCloseAllOpenPropertyWindowsWithoutSavingChanges) = MsgBoxResult.No Then
                    Return False
                End If

                Dim SubSheetIDBeforeUndo As System.Guid = mProcess.GetActiveSubSheet
                Dim currentZoom = mProcess.ZoomPercent

                If Not mProcess.Undo(n, sErr) Then
                    UserMessage.Show(sErr)
                Else
                    'Restore the view to the last-viewed page, if it still exists after the undo
                    Dim SheetToView As clsProcessSubSheet = mProcess.GetSubSheetByID(SubSheetIDBeforeUndo)
                    If SheetToView IsNot Nothing Then
                        mProcess.SetActiveSubSheet(SubSheetIDBeforeUndo)
                    Else
                        mProcess.SetActiveSubSheet(Guid.Empty)
                    End If
                    mProcess.ZoomPercent = currentZoom
                    Me.UpdateSheetTabs()
                    CloseAllOpenPropertiesWindows()
                End If
            End If
            InvalidateView()
            Return True
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_AnUnexpectedErrorOccurredDuringTheUndoOperationTheErrorMessageWas0, ex.Message), ex)
        End Try
        Return False
    End Function

    Public Function IsPropertyWindowOpen(stageGuid As Guid) As Boolean
        If mPropertyWindows.ContainsKey(stageGuid) Then Return True Else Return False
    End Function

    Public Sub ValidateOpenPropertiesWindows()

        Dim toRemove = New List(Of Guid)
        For Each propWindowKvp In mPropertyWindows
            Dim stage = mProcess.GetStage(propWindowKvp.Key)
            If stage Is Nothing Then toRemove.Add(propWindowKvp.Key)
        Next
        'now remove the entry from the entry from the store and close the window.
        For Each id In toRemove
            Dim window = mPropertyWindows(id)
            mPropertyWindows.Remove(id)
            'For now just code the window, 
            window.Close()
        Next
    End Sub

    ''' <summary>
    ''' Close all open property windows
    ''' </summary>
    Private Sub CloseAllOpenPropertiesWindows()
        For Each propertyWindows In mPropertyWindows.Values.ToList()
            propertyWindows.Close()
        Next
        mPropertyWindows.Clear()
    End Sub

    ''' <summary>
    ''' Get a list of all open properties windows on a given subsheet
    ''' </summary>
    ''' <param name="sheet">subsheet</param>
    ''' <returns>list of stage id's</returns>
    Public Function GetOpenPropertiesWindowsOnSubSheet(sheet As clsProcessSubSheet) As List(Of Guid)
        Dim openProperties = New List(Of Guid)
        For Each propWindowKvp In mPropertyWindows
            Dim stage = sheet.GetStage(propWindowKvp.Key)
            If stage Is Nothing Then openProperties.Add(propWindowKvp.Key)
        Next
        Return openProperties
    End Function

    ''' <summary>
    ''' Restores the state of the process object and the user interface to the
    ''' previous state in the undo buffer.
    ''' </summary>
    Public Function Undo() As Boolean
        Return Undo(1)
    End Function

    ''' <summary>
    ''' Restores the state of the process object and the user interface to the
    ''' next state in the undo buffer.
    ''' </summary>
    Public Function Redo() As Boolean
        Return Redo(1)
    End Function

    ''' <summary>
    ''' Restores the state of the process object and the user interface to the
    ''' next states in the undo buffer.
    ''' </summary>
    Public Function Redo(n As Integer) As Boolean
        Try
            Dim sErr As String = Nothing
            If Not mProcess Is Nothing Then

                If mPropertyWindows.Any() Then
                    If UserMessage.YesNo(My.Resources.PerformingAnUndoRedoOperationWillCloseAllOpenPropertyWindowsWithoutSavingChanges) = MsgBoxResult.No Then
                        Return False
                    End If
                End If

                Dim SubSheetIDBeforeUndo As System.Guid = mProcess.GetActiveSubSheet
                If Not mProcess.Redo(n, sErr) Then
                    UserMessage.Show(sErr)
                Else
                    'Restore the view to the last-viewed page, if it still exists after the undo
                    Dim SheetToView As clsProcessSubSheet = mProcess.GetSubSheetByID(SubSheetIDBeforeUndo)
                    If SheetToView IsNot Nothing Then
                        mProcess.SetActiveSubSheet(SubSheetIDBeforeUndo)
                    Else
                        mProcess.SetActiveSubSheet(Guid.Empty)
                    End If
                    Me.UpdateSheetTabs()
                    CloseAllOpenPropertiesWindows()
                End If
            End If
            InvalidateView()
            Return True
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_AnUnexpectedErrorOccurredDuringTheRedoOperationTheErrorMessageWas0, ex.Message), ex)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Delete the current selection.
    ''' </summary>
    ''' <remarks>
    ''' This functionality should be moved to clsProcess in
    ''' AutomateProcessCore.
    ''' </remarks>
    Public Sub DoDeleteSelection()

        Dim sErr As String = Nothing
        Dim selected As clsProcessSelection
        Dim iDeletions As Integer
        Dim sDeleteSummary As String = ""
        Dim sDeleteDescription As String = ""

        'Delete the selection...

        Dim selectionContainer = mProcess.SelectionContainer

        If selectionContainer.Count > 0 Then
            Dim stagesToDelete As New List(Of clsProcessStage)
            For Each selected In selectionContainer

                'Invalidate the delete action if it's properties is open unless its a link
                If Not selected.mtType.HasFlag(clsProcessSelection.SelectionType.ChoiceLink) AndAlso
                    Not selected.mtType.HasFlag(clsProcessSelection.SelectionType.Link) AndAlso
                    mPropertyWindows.ContainsKey(selected.mgStageID) Then
                    UserMessage.OK(String.Format(My.Resources.ctlProcessViewer_The0StageCannotBeDeletedWhileThePropertiesDialogIsOpen,
                                                 mPropertyWindows(selected.mgStageID).ProcessStage.Name))
                    Return
                End If

                Select Case selected.mtType

                    Case clsProcessSelection.SelectionType.Stage

                        'We need to verify that we haven't already
                        'deleted the stage during this operation.
                        Dim objSelStage = mProcess.GetStage(selected.mgStageID)
                        If Not objSelStage Is Nothing Then

                            'We Cannot delete processinfo, subsheetinfo stages, or start stages.
                            If objSelStage.StageType = StageTypes.SubSheetInfo Then Exit Select
                            If objSelStage.StageType = StageTypes.Start Then Exit Select
                            If objSelStage.StageType = StageTypes.ProcessInfo Then Exit Select

                            stagesToDelete.Add(objSelStage)

                        End If

                    Case clsProcessSelection.SelectionType.Link
                        Dim objStage As clsProcessStage
                        objStage = mProcess.GetStage(selected.mgStageID)
                        'Make sure the stage still exists, since we may well
                        'have deleted it as part of this process already!
                        If Not objStage Is Nothing Then
                            objStage.DeleteLink(selected.msLinkType)
                            iDeletions += 1
                            sDeleteSummary = "link"
                        End If

                    Case clsProcessSelection.SelectionType.ChoiceLink
                        Dim objChoiceStart As clsChoiceStartStage = TryCast(mProcess.GetStage(selected.mgStageID), clsChoiceStartStage)

                        If Not objChoiceStart Is Nothing Then
                            Dim objChoice As clsChoice = objChoiceStart.Choices(selected.miChoiceIndex)
                            objChoice.LinkTo = Guid.Empty
                            iDeletions += 1
                            sDeleteSummary = "choice link"
                        End If

                    Case clsProcessSelection.SelectionType.ChoiceNode
                        Dim objChoiceStart As clsChoiceStartStage = TryCast(mProcess.GetStage(selected.mgStageID), clsChoiceStartStage)

                        If Not objChoiceStart Is Nothing Then
                            objChoiceStart.Choices.RemoveAt(selected.miChoiceIndex)
                            iDeletions += 1
                            sDeleteSummary = "choice node"
                        End If

                End Select
            Next

            'This was seperated from the loop above to prevent the selection from being
            'changed while the loop is running.
            For Each stage In stagesToDelete
                If mProcess.DeleteStage(stage, sErr) Then
                    'If there are any stages with a matching group ID delete them too...
                    If TypeOf stage Is clsGroupStage Then
                        Dim groupId = CType(stage, clsGroupStage).GetGroupID()
                        If groupId.CompareTo(Guid.Empty) <> 0 Then
                            For Each gs As clsGroupStage In mProcess.GetGroupStages(groupId)
                                If Not mProcess.DeleteStage(gs, sErr) Then
                                    UserMessage.Show(sErr)
                                End If
                            Next
                        End If
                    End If
                    If TypeOf stage Is clsDataStage
                        RemoveTreeDataItem(DirectCast(stage, clsDataStage))
                    End If
                    iDeletions += 1
                    sDeleteSummary = clsUtility.GetUnCamelled(clsStageTypeName.GetLocalizedFriendlyName(stage.StageType.ToString()).ToConditionalLowerNoun())
                    sDeleteDescription = sDeleteSummary & " '" & stage.GetName & "'"
                Else
                    UserMessage.Show(sErr)
                End If
            Next

            If selectionContainer.Count > 1 Then
                sDeleteSummary = "selection"
                sDeleteDescription = String.Format(My.Resources.ctlProcessViewer_Selected0Items, CStr(selectionContainer.Count))
            End If

            'Add the latest state to the undo/redo buffer
            If iDeletions > 0 Then
                mProcess.SaveUndoPosition(clsUndo.ActionType.Delete, sDeleteSummary, sDeleteDescription)
            End If

        Else
            ShowFloatingMessage(My.Resources.ctlProcessViewer_DeleteError, My.Resources.ctlProcessViewer_CanNotDeleteBecauseThereAreNoStagesSelected, ToolTipIcon.Info)
        End If
    End Sub

    ''' <summary>
    ''' Clears the current selection on the process diagram.
    ''' </summary>
    Public Sub ClearSelection()
        mProcess.SelectNone()
    End Sub

#End Region

#Region "Debugging"

    ''' <summary>
    ''' Perform a debug action.
    ''' </summary>
    ''' <param name="action">The action to perform - possible values
    ''' are the same as for clsProcess.RunAction().</param>
    ''' <param name="noActivate">Set to true to disable activation of the parent
    ''' form after the step.</param>
    Public Function DebugAction(action As ProcessRunAction, Optional ByVal noActivate As Boolean = False) As Boolean

        'Force the user to save if necessary. This is because sessions logged
        'to the database have a foreign key constraint against the process that was run.
        'See bug 2084.
        If Me.mThisProcessHasNeverEverBeenSaved Then
            UserMessage.Show(String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_DebugAction_OperationCanNotBePerformedUntilSave, GetOpenObjectType))
            Return False
        End If

        If mProcess Is Nothing Then
            Return False
        End If

        'start logging if appropriate, this only does something if we
        'are in a process rather than an object
        SetupDebugSessionAndLogging()

        Dim executeResult = ExecuteAction(action)

        If executeResult.Success Then

            If action <> ProcessRunAction.RunNextStep OrElse Not RunAtNearFullSpeed Then
                FocusCurrentStage()
            End If

            If action = ProcessRunAction.Reset Then
                ResetCalculationZoom()

                ReloadParentObject()

                mProcess.ExternalObjectsInfo = Options.Instance.GetExternalObjectsInfo()

                If gSv.GetPref(PreferenceNames.AutoValidation.AutoValidateOnReset, True) Then _
                    UpdateValidationCount()
            End If

            RefreshDataItemWatches()
        End If

        'Refreshes user interface
        If Not noActivate And Not mProcess.ChildWaiting Then
            mobjParentForm.Activate()
        End If

        'Switch to current page if we're still running...
        If mProcess.RunState <> ProcessRunState.Off Then
            If action <> ProcessRunAction.RunNextStep OrElse Not RunAtNearFullSpeed Then
                ShowStage(mProcess.GetStage(mProcess.RunStageID), False)
            End If
        End If

        'Show the message now if we failed - we've delayed doing it until after 
        'the UI updates above to provide additional cues, because the message box
        'may be hidden behind a target application.
        If Not executeResult.Success Then
            UserMessage.Show(executeResult.GetText())
        End If

        Dim processForm = TryCast(Me.mobjParentForm, frmProcess)
        If processForm IsNot Nothing Then
            processForm.UpdateDebugButtons()
        End If

        Return executeResult.Success
    End Function

    Private Shared Function IsExecuteAction(action As ProcessRunAction) As Boolean
        Return _
            {
                ProcessRunAction.Go,
                ProcessRunAction.RunNextStep,
                ProcessRunAction.StepIn,
                ProcessRunAction.StepOut,
                ProcessRunAction.StepOver
            }.
            Contains(action)
    End Function

    Private Function ExecuteAction(action As ProcessRunAction) As StageResult

        Do
            If mProcess.RunStage IsNot Nothing Then
                If action = ProcessRunAction.StepIn AndAlso Not CanStepIntoStage() Then
                    action = ProcessRunAction.StepOver
                End If

                If IsExecuteAction(action) AndAlso Not CanExecuteStage() Then
                    mProcess.RunState = ProcessRunState.Failed
                    Return New StageResult(False, My.Resources.ctlProcessViewer_Internal, My.Resources.ctlProcessViewer_UserDoesNotHaveExecutePermissionOnTheReferencedObject)
                End If
            End If

            Dim bpInfo As clsProcessBreakpointInfo = Nothing
            Dim result = mProcess.RunAction(action, bpInfo, True)
            If Not result.Success Then
                Return result
            End If
            If bpInfo IsNot Nothing Then
                If Not bpInfo.IsTransient Then ShowBreakpointInfoToUser(bpInfo)
                Exit Do
            End If

            'Repeat the execution if we're waiting for a child stage and we're
            'in run mode.
        Loop While action = ProcessRunAction.RunNextStep AndAlso mProcess.ChildWaiting

        Return StageResult.OK

    End Function

    Private Function CanStepIntoStage() As Boolean

        Select Case mProcess.RunStage.StageType
            Case StageTypes.Action
                Return CanStepIntoActionStage()

            Case StageTypes.Process
                Return CanStepIntoProcessStage()

            Case Else
                Return True
        End Select

    End Function

    Private Function CanStepIntoProcessStage() As Boolean
        Dim processStage = TryCast(mProcess.RunStage, clsSubProcessRefStage)
        If processStage Is Nothing Then Return False

        Dim perms = gSv.GetEffectiveMemberPermissionsForProcess(processStage.ReferenceId)

        Return perms.HasPermission(User.Current, Permission.ProcessStudio.ImpliedViewProcess) AndAlso
        perms.HasPermission(User.Current, Permission.ProcessStudio.ImpliedExecuteProcess)
    End Function

    Private Function CanStepIntoActionStage() As Boolean
        Dim actionStage = TryCast(mProcess.RunStage, clsActionStage)
        If actionStage Is Nothing Then Return False
        Dim vbo = TryCast(mProcess.GetBusinessObjectRef(actionStage.ObjectName), clsVBO)
        If vbo Is Nothing Then Return False

        Dim perms = gSv.GetEffectiveMemberPermissionsForProcess(vbo.ProcessID)

        Return (perms.HasPermission(User.Current, Permission.ObjectStudio.ImpliedViewBusinessObject) AndAlso
           perms.HasPermission(User.Current, Permission.ObjectStudio.ImpliedExecuteBusinessObject))

    End Function

    Private Function CanExecuteStage() As Boolean

        Select Case mProcess.RunStage.StageType
            Case StageTypes.Action
                Return CanExecuteActionStage()

            Case StageTypes.Process
                Return CanExecuteProcessStage()

            Case Else
                Return True
        End Select

    End Function

    Private Function CanExecuteActionStage() As Boolean

        Dim actionStage = TryCast(mProcess.RunStage, clsActionStage)
        If actionStage Is Nothing Then Return False
        Dim vbo = TryCast(mProcess.GetBusinessObjectRef(actionStage.ObjectName), clsVBO)
        If vbo Is Nothing Then Return True

        Return gSv.
            GetEffectiveMemberPermissionsForProcess(vbo.ProcessID).
            HasPermission(
                User.Current,
                Permission.ObjectStudio.ImpliedExecuteBusinessObject)

    End Function

    Private Function CanExecuteProcessStage() As Boolean
        Dim processStage = TryCast(mProcess.RunStage, clsSubProcessRefStage)
        If processStage Is Nothing Then Return False

        Return gSv.
            GetEffectiveMemberPermissionsForProcess(processStage.ReferenceId).
            HasPermission(
                User.Current,
                Permission.ProcessStudio.ImpliedExecuteProcess)

    End Function

    Private Sub FocusCurrentStage()
        Me.ShowStage(mProcess.GetStage(mProcess.RunStageID), False)
        InvalidateView()
    End Sub

    Private Sub ResetCalculationZoom()
        If Not mobjCalculationZoom Is Nothing Then
            mobjCalculationZoom.Close()
            mobjCalculationZoom = Nothing
        End If
    End Sub

    Private Sub ReloadParentObject()
        If mProcess.ParentObject IsNot Nothing Then
            If LoadParentApplicationModel() Then
                ShowFloatingMessage(
                    My.Resources.ctlProcessViewer_SharedApplicationModel,
                    My.Resources.ctlProcessViewer_ReLoadedFromObject & mProcess.ParentObject,
                    ToolTipIcon.Info)
            Else
                ShowFloatingMessage(
                    My.Resources.ctlProcessViewer_SharedApplicationModel,
                    My.Resources.ctlProcessViewer_MissingParentObject & mProcess.ParentObject,
                    ToolTipIcon.Warning)
            End If
        End If
    End Sub

    Private Sub RefreshDataItemWatches()
        Dim objParentForm = TryCast(Me.mobjParentForm, frmProcess)
        If (objParentForm IsNot Nothing) Then
            Dim datawatchform = objParentForm.mDataWatchForm
            If Not datawatchform Is Nothing Then datawatchform.ctlDataItemWatches.RefreshDataValues()
        End If
    End Sub

    ''' <summary>
    ''' Closes the breakpoint info form. This can be called safely at 
    ''' any time, regardless of whether the breakpoint info form is 
    ''' still open.
    ''' </summary>
    Public Sub CloseBreakpointInfo()
        If Not Me.mBreakpointInfoForm Is Nothing Then
            Me.mBreakpointInfoForm.Close()
            Me.mBreakpointInfoForm = Nothing
        End If
    End Sub

    ''' <summary>
    ''' The form we pop up after reaching a breakpoint
    ''' </summary>
    Private mBreakpointInfoForm As frmBreakpointInfoForm

    ''' <summary>
    ''' Handles debug keys when the breakpoint form is showing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mBreakpointInfoForm_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)

        Select Case e.KeyCode
            Case Keys.F5

                If Not mobjParentForm Is Nothing And TypeOf mobjParentForm Is frmProcess Then
                    CType(mobjParentForm, frmProcess).mnuDebugGo.PerformClick()
                End If

            Case Keys.F10

                If Not mobjParentForm Is Nothing And TypeOf mobjParentForm Is frmProcess Then
                    CType(mobjParentForm, frmProcess).mnuDebugStepOver.PerformClick()
                End If

            Case Keys.F11

                If Not mobjParentForm Is Nothing And TypeOf mobjParentForm Is frmProcess Then
                    CType(mobjParentForm, frmProcess).mnuDebugStep.PerformClick()
                End If

        End Select

    End Sub

    ''' <summary>
    ''' Shows user the supplied breakpoint info in a non-modal popup window.
    ''' Does nothing if null reference passed.
    ''' </summary>
    ''' <param name="BPI">Breakpoint info to show. Null reference is
    ''' acceptable; nothing is done in this case.</param>
    Private Sub ShowBreakpointInfoToUser(ByVal BPI As clsProcessBreakpointInfo)
        If (Not BPI Is Nothing) Then
            If Not mBreakpointInfoForm Is Nothing Then
                Me.CloseBreakpointInfo()
            End If

            'create form and set location
            mBreakpointInfoForm = New frmBreakpointInfoForm(BPI)
            AddHandler mBreakpointInfoForm.rtbExpression.KeyUp, AddressOf mBreakpointInfoForm_KeyUp
            ChooseLocationForBreakpointInfoForm(BPI)

            'Bring this window to front - it may be behind target application
            mobjParentForm.Activate()

            'make the form owned by process studio form and show it
            If Not Me.mobjParentForm Is Nothing Then
                Me.mobjParentForm.AddOwnedForm(mBreakpointInfoForm)
            End If

            mBreakpointInfoForm.Show()

        End If
    End Sub





#End Region

#Region "ChooseLocationForBreakpointInfoForm"

    ''' <summary>
    ''' Chooses where to put the member form mBreakpointInfo, using the supplied
    ''' breakpointinfo object.
    ''' </summary>
    ''' <param name="BPI">The breakpointinfo object.</param>
    Private Sub ChooseLocationForBreakpointInfoForm(ByVal BPI As clsProcessBreakpointInfo)

        'chooses where to locate the form. Preferably right next
        'to the stage, in one of the corners.
        mBreakpointInfoForm.StartPosition = FormStartPosition.Manual
        With BPI.BreakpointStage
            Dim ScreenPointToPutFormAtBottomRightOfPictureBox As Point = Me.pbview.PointToScreen(New Point(Me.pbview.Width - mBreakpointInfoForm.Width, Me.pbview.Height - mBreakpointInfoForm.Height))

            'get a list of points in the corners of the stage of interest
            Dim StageLowerRightXCoord As Single = .GetDisplayX + (.GetDisplayWidth / 2)
            Dim StageLowerRightYCoord As Single = .GetDisplayY + (.GetDisplayHeight / 2)
            Dim StageLowerLeftXCoord As Single = .GetDisplayX - (.GetDisplayWidth / 2)
            Dim StageLowerLeftYCoord As Single = .GetDisplayY + (.GetDisplayHeight / 2)
            Dim StageUpperRightXCoord As Single = .GetDisplayX + (.GetDisplayWidth / 2)
            Dim StageUpperRightYCoord As Single = .GetDisplayY - (.GetDisplayHeight / 2)
            Dim StageUpperLeftXCoord As Single = .GetDisplayX - (.GetDisplayWidth / 2)
            Dim StageUpperLeftYCoord As Single = .GetDisplayY - (.GetDisplayHeight / 2)
            mProcess.TranslateWorldToScreen(StageLowerRightXCoord, StageLowerRightYCoord, Me.pbview.Width, Me.pbview.Height)
            mProcess.TranslateWorldToScreen(StageLowerLeftXCoord, StageLowerLeftYCoord, Me.pbview.Width, Me.pbview.Height)
            mProcess.TranslateWorldToScreen(StageUpperRightXCoord, StageUpperRightYCoord, Me.pbview.Width, Me.pbview.Height)
            mProcess.TranslateWorldToScreen(StageUpperLeftXCoord, StageUpperLeftYCoord, Me.pbview.Width, Me.pbview.Height)

            'Different places we could put the form. Respectively:
            ' 1) To immediate lower right of stage
            ' 2) To immediate lower left 
            ' 3) To immediate upper right
            ' 4) To immediate upper left
            Dim CandidateFormLocations As Point() = {
              Me.pbview.PointToScreen(New Point(CInt(StageLowerRightXCoord), CInt(StageLowerRightYCoord))),
              Me.pbview.PointToScreen(New Point(CInt(StageLowerLeftXCoord) - mBreakpointInfoForm.Width, CInt(StageLowerLeftYCoord))),
              Me.pbview.PointToScreen(New Point(CInt(StageUpperRightXCoord), CInt(StageUpperRightYCoord) - mBreakpointInfoForm.Height)),
              Me.pbview.PointToScreen(New Point(CInt(StageUpperLeftXCoord) - mBreakpointInfoForm.Width, CInt(StageUpperLeftYCoord) - mBreakpointInfoForm.Height))
             }

            'for each point see if the form would be on the screen.
            'if so then choose it by setting chosenlocation variable
            Dim ChosenLocation As Integer = -1
            For i As Integer = 0 To CandidateFormLocations.Length - 1
                Dim p As Point = CandidateFormLocations(i)
                If Screen.PrimaryScreen.Bounds.Contains(New Rectangle(p, Me.mBreakpointInfoForm.Size)) Then
                    ChosenLocation = i
                    Exit For
                End If
            Next

            'if a location was chosen then use it
            If ChosenLocation > -1 Then
                mBreakpointInfoForm.Location = CandidateFormLocations(ChosenLocation)
            Else
                'otherwise give up and slap it in the middle
                mBreakpointInfoForm.StartPosition = FormStartPosition.CenterScreen
            End If
        End With

    End Sub

#End Region

#Region "Visual Components (scrollbars, pointers, invalidating etc)"

#Region "Tab Stuff"

    ''' <summary>
    ''' Selects the tab corresponding to the guid of the subsheet supplied.
    ''' </summary>
    ''' <param name="gTabToSelect">The guid of a subsheet in the process object.</param>
    Private Sub SelectTabByGuid(ByVal gTabToSelect As Guid)
        Try
            Me.tcPages.SelectedTab = Me.GetTabByGuid(gTabToSelect)
        Catch ex As Exception
            'do nothing
        End Try
    End Sub

    Private Sub SelectTabSelectorByGuid(ByVal pageId As Guid)
        For Each item As ComboBoxItem In cmbTabSelector.Items
            If pageId = CType(item.Tag, Guid) Then
                cmbTabSelector.SelectedItem = item
                item.Style = FontStyle.Bold
            Else
                item.Style = FontStyle.Regular
            End If
        Next
    End Sub

    ''' <summary>
    ''' Renames the tab corresponding to the guid of the subsheet specified.
    ''' If no such tab is found then no action is taken.
    ''' </summary>
    ''' <param name="id">The guid of the subsheet to be renamed.
    ''' </param>
    ''' <param name="name">The new name to be applied to the tab.</param>
    Private Sub RenameTabByGuid(ByVal id As Guid, ByVal name As String)
        Try
            GetTabByGuid(id).Text = name
        Catch ex As Exception
            'do nothing
        End Try
    End Sub

    ''' <summary>
    ''' Removes a tab from the tab control using the guid specified. The guid must be
    ''' that of a subsheet which has existed in the process. If no matching tab is
    ''' found then no action is taken.
    ''' 
    ''' Note that this merely removes the tab from the tab control. The process object
    ''' must be updated using a separate call.
    ''' </summary>
    ''' <param name="id">The guid of the subsheet to be deleted.
    ''' </param>
    Private Sub DeleteTabByGuid(ByVal id As Guid)
        Try
            tcPages.TabPages.Remove(GetTabByGuid(id))
            For i As Integer = 0 To cmbTabSelector.Items.Count - 1
                Dim item As ComboBoxItem =
                 DirectCast(cmbTabSelector.Items(i), ComboBoxItem)
                If id.Equals(item.Tag) Then
                    cmbTabSelector.Items.RemoveAt(i)
                    ValidateOpenPropertiesWindows()
                    Return
                End If
            Next
        Catch
            'do nothing
        End Try
    End Sub

    ''' <summary>
    ''' Retrieves the tab corresponding to the subsheet specified.
    ''' </summary>
    ''' <param name="gSheetID">The guid of a subsheet in the process.</param>
    ''' <returns>The tab matching the guid specified.</returns>
    Private Function GetTabByGuid(ByVal gSheetID As Guid) As TabPage
        For Each t As TabPage In tcPages.TabPages
            If gSheetID.Equals(t.Tag) Then Return t
        Next

        'if we get here then the tab was not found
        UserMessage.Show(My.Resources.ctlProcessViewer_InternalErrorTheTabPageRequestedWasNotFound)
        Return Nothing
    End Function

    ''' <summary>
    ''' Creates a new tab to represent the given subsheet, inserting it at the index
    ''' at which the sheet resides within its process.
    ''' </summary>
    ''' <param name="sheet">The sheet to create a tab for</param>
    Private Sub CreateNewTab(ByVal sheet As clsProcessSubSheet)
        Dim t As New TabPageEx()
        t.Text = sheet.Name
        t.Name = sheet.ID.ToString()
        t.Text = t.Text       'limits tab width. see bug 581
        t.Tag = sheet.ID
        If sheet.Published Then _
         t.BackColor = ColourScheme.Default.TabPublishedSubSheet

        ' Insert the tab page and combo item at the appropriate index
        Dim ix As Integer = sheet.Index
        tcPages.TabPages.Insert(ix, t)
        cmbTabSelector.Items.Insert(ix, New ComboBoxItem(sheet.Name, sheet.ID))

    End Sub

    ''' <summary>
    ''' Sets the given tab page highlight on or off as specified.
    ''' </summary>
    ''' <param name="t">The tab page to highlight or un-highlight</param>
    ''' <param name="highlight">True to highlight the page, False to ensure that the
    ''' page is not highlighted</param>
    Private Sub SetTabPageHighlight(ByVal t As TabPage, ByVal highlight As Boolean)
        SetTabPageHighlight(TryCast(t, TabPageEx), highlight)
    End Sub

    ''' <summary>
    ''' Sets the given tab page highlight on or off as specified.
    ''' </summary>
    ''' <param name="t">The tab page to highlight or un-highlight</param>
    ''' <param name="highlight">True to highlight the page, False to ensure that the
    ''' page is not highlighted</param>
    Private Sub SetTabPageHighlight(ByVal t As TabPageEx, ByVal highlight As Boolean)
        If t IsNot Nothing Then t.ImageKey =
            CStr(IIf(highlight, ImageLists.Keys.Publishing.Published, Nothing))
    End Sub

    ''' <summary>
    ''' Refreshes the tab control by clearing all tabs and re-adding them using
    ''' the list of sheets in the process object.
    ''' </summary>
    Public Sub UpdateSheetTabs()

        Me.tcPages.SuspendLayout()
        RemoveHandler tcPages.SelectedIndexChanged, AddressOf tcPages_SelectedIndexChanged

        tcPages.TabPages.Clear()
        Me.cmbTabSelector.Items.Clear()
        Dim t As AutomateControls.TabPageEx


        For Each objSub As clsProcessSubSheet In mProcess.SubSheets
            If Not objSub Is Nothing Then
                t = New AutomateControls.TabPageEx()
                t.Text = LTools.GetC(objSub.Name, "misc", "page")
                t.Name = objSub.ID.ToString
                t.Tag = objSub.ID
                tcPages.TabPages.Add(t)
                SetTabPageHighlight(t, objSub.Published)
                If mProcess.GetActiveSubSheetRef() Is objSub Then _
                 tcPages.SelectedTab = t

                cmbTabSelector.Items.Add(New ComboBoxItem(LTools.GetC(objSub.Name, "misc", "page"), objSub.ID))
            End If
        Next

        AddHandler tcPages.SelectedIndexChanged, AddressOf tcPages_SelectedIndexChanged
        Me.tcPages.ResumeLayout(True)

        InvalidateView()
    End Sub


#End Region

    ''' <summary>
    ''' Takes X, Y coordinates and determines the nearest point on the process grid
    ''' when the bSnapToGrid() property is set.
    ''' </summary>
    ''' <param name="sngX">The X coordinate to be updated.</param>
    ''' <param name="sngY">The Y coordinate to be updated.</param>
    Private Sub SnapTo(ByRef sngX As Single, ByRef sngY As Single)
        sngX = Snap(sngX)
        sngY = Snap(sngY)
    End Sub

    ''' <summary>
    ''' Snaps the supplied point to the current grid.
    ''' </summary>
    ''' <param name="Location">The point to be modified.</param>
    ''' <returns>Gets the snap point on the grid for the
    ''' supplied value.</returns>
    Private Function SnapTo(ByVal Location As PointF) As PointF
        Location.X = Snap(Location.X)
        Location.Y = Snap(Location.Y)
        Return Location
    End Function

    Private Function Snap(ByVal s As Single) As Single
        If mSnapToGrid Then
            Return CSng(Math.Round(s / ciGridSize) * ciGridSize)
        Else
            Return s
        End If
    End Function


    Public Sub UpdateScrollbars()
        If Not mProcess Is Nothing Then
            Dim val As Single, max As Single, min As Single
            Dim rectExtent As New Rectangle
            mProcess.GetExtent(rectExtent, mProcess.GetActiveSubSheet)

            Dim viewWidth As Single = pbview.Width / mProcess.Zoom
            Me.ScrollH.LargeChange = CInt(viewWidth)
            ScrollH.SmallChange = CInt(ScrollH.LargeChange / 5)
            max = rectExtent.Right - viewWidth / 2
            min = rectExtent.Left + viewWidth / 2
            If max < min Then max = min
            val = mProcess.GetCameraX()
            If val < min Then
                val = min
                mProcess.SetCameraX(val)
            ElseIf val > max Then
                val = max
                mProcess.SetCameraX(val)
            End If
            ScrollH.Maximum = CInt(max + ScrollH.LargeChange)
            ScrollH.Minimum = CInt(min)
            ScrollH.Value = CInt(val)

            Dim viewHeight As Single = pbview.Height / mProcess.Zoom
            ScrollV.LargeChange = CInt(viewHeight)
            ScrollV.SmallChange = CInt(ScrollV.LargeChange / 5)
            max = rectExtent.Bottom - viewHeight / 2
            min = rectExtent.Top + viewHeight / 2
            If max < min Then max = min
            val = mProcess.GetCameraY()
            If val < min Then
                val = min
                mProcess.SetCameraY(val)
            ElseIf val > max Then
                val = max
                mProcess.SetCameraY(val)
            End If
            ScrollV.Maximum = CInt(max + ScrollV.LargeChange)
            ScrollV.Minimum = CInt(min)
            ScrollV.Value = CInt(val)

        End If
    End Sub

    Private Shared SuppressFurtherCalls As Boolean
    ''' <summary>
    ''' Updates the control to registers changes to scrollbars,
    ''' zoom menus and invalidates picturebox showing the process.
    ''' </summary>
    Public Sub InvalidateView()
        'Sometimes invalidateview is called several times in quick succession
        'eg this method calls updatescrollbars, which fires an event on the scrollbar,
        'which in turn calls this method. Here we want to make sure that the
        'current invocation is complete before we refresh all over again

        If Not SuppressFurtherCalls Then
            Try
                SuppressFurtherCalls = True

                'If viewer has no window handle it cannot be updated.
                If Not IsHandleCreated Then Return

                'Always update with the latest zoom and camera location
                mRenderer.CoordinateTranslator = New clsRenderer.clsCoordinateTranslator(mProcess)

                UpdateScrollbars()
                If TypeOf mobjParentForm Is IProcessViewingForm Then
                    CType(mobjParentForm, IProcessViewingForm).ZoomUpdate(mProcess.ZoomPercent)
                End If

                'Select the right sheet
                Dim CurrentSubsheet As Guid = mProcess.GetActiveSubSheet
                If CType(Me.tcPages.SelectedTab.Tag, Guid) <> CurrentSubsheet Then
                    Me.SelectTabByGuid(CurrentSubsheet)
                    Me.SelectTabSelectorByGuid(CurrentSubsheet)
                End If

                pbview.Refresh()
            Catch ex As Exception
                'do nothing
            Finally
                SuppressFurtherCalls = False
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Changes the current pointer based on the DynamicCursor() property and the 
    ''' current tool selection.
    ''' </summary>
    Public Sub ChangePointer()

        Dim sCursorPath As String

        sCursorPath = Path.Combine(clsFileSystem.GetGraphicsPath(), "cursors")

        If mbDynamicCursor = False Then
            pbview.Cursor = Cursors.Default
            myCurrentPointer = pbview.Cursor
            Exit Sub
        End If

        ' Dim ms As MemoryStream = Nothing
        Dim bytes() As Byte = Nothing
        Select Case mTool
            Case StudioTool.Action : bytes = StudioCursors.Action
            Case StudioTool.Decision : bytes = StudioCursors.Decision
            Case StudioTool.Link : bytes = StudioCursors.Link
            Case StudioTool.Anchor : bytes = StudioCursors.Anchor
            Case StudioTool.Data : bytes = StudioCursors.Data
            Case StudioTool.Note : bytes = StudioCursors.Note
            Case StudioTool.Start : bytes = StudioCursors.Terminate
            Case StudioTool.End : bytes = StudioCursors.Terminate
            Case StudioTool.Zoom : bytes = StudioCursors.Zoom
            Case StudioTool.Calculation : bytes = StudioCursors.Calc
            Case StudioTool.Collection : bytes = StudioCursors.Collection
            Case StudioTool.Loop : bytes = StudioCursors.Loop
            Case StudioTool.Process : bytes = StudioCursors.Process
            Case StudioTool.Page : bytes = StudioCursors.SubSheet
            Case StudioTool.Alert : bytes = StudioCursors.Alert
            Case StudioTool.Read : bytes = StudioCursors.Read
            Case StudioTool.Write : bytes = StudioCursors.Write
            Case StudioTool.Navigate : bytes = StudioCursors.Navigate
            Case StudioTool.Code : bytes = StudioCursors.Code
            Case StudioTool.Choice : bytes = StudioCursors.Choice
            Case StudioTool.Wait : bytes = StudioCursors.Wait
            Case StudioTool.MultipleCalculation : bytes = StudioCursors.Multicalc
            Case StudioTool.Exception : bytes = StudioCursors.Exception
            Case StudioTool.Recover : bytes = StudioCursors.Recover
            Case StudioTool.Resume : bytes = StudioCursors.Resume
            Case StudioTool.Block : bytes = StudioCursors.Block
            Case StudioTool.Pan : bytes = StudioCursors.Hand
            Case Else : pbview.Cursor = Cursors.Default
        End Select

        If bytes IsNot Nothing Then
            Using ms As New MemoryStream(bytes)
                pbview.Cursor = New Cursor(ms)
            End Using
        End If

        myCurrentPointer = pbview.Cursor

    End Sub

    ''' <summary>
    ''' Retrieves information determining:
    ''' 1) Whether gridlines should be displayed.
    ''' 2) Whether objects should be snapped to the grid.
    ''' 3) The spacing between gridlines in process world coordinates.
    ''' </summary>
    ''' <param name="bDisplay">True when gridlines should be displayed.</param>
    ''' <param name="bSnap">True when we should snap to a grid.</param>
    ''' <param name="sngSize">The spacing between gridlines.</param>
    Public Sub GetGridInfo(ByRef bDisplay As Boolean, ByRef bSnap As Boolean, ByRef sngSize As Single)
        bDisplay = ShowGridLines
        bSnap = SnapToGrid()
        sngSize = ciGridSize
    End Sub

#End Region

#Region "Visual Zoom"


    ''' <summary>
    ''' Calls SetZoom() and updates the user interface menus with the current
    ''' selection.
    ''' </summary>
    ''' <param name="percent">The zoom percentage to be applied.</param>
    Public Sub DoZoom(ByVal percent As Integer)
        mProcess.ZoomPercent = percent
        Dim viewer As IProcessViewingForm = TryCast(mobjParentForm, IProcessViewingForm)
        If viewer IsNot Nothing Then viewer.ZoomUpdate(percent)
        InvalidateView()
    End Sub

    Public Sub ZoomIn()
        DoZoom(mProcess.IncrementZoom())
    End Sub

    Public Sub ZoomOut()
        DoZoom(mProcess.DecrementZoom())
    End Sub

#End Region

#Region "Printing"

    Private moPrintPreview As frmPrintPreview

    Public Sub PrintPreview()

        If moPrintPreview Is Nothing Then
            moPrintPreview = New frmPrintPreview
        End If
        moPrintPreview.PrintPreview(mobjPrintDocument, mProcess, mRenderer)
        If moPrintPreview.SetupDocumentDone Then
            moPrintPreview.ShowInTaskbar = False
            moPrintPreview.ShowDialog()
        End If
        moPrintPreview.Dispose()
        moPrintPreview = Nothing
    End Sub

    Public Sub PrintProcess()

        If moPrintPreview Is Nothing Then
            moPrintPreview = New frmPrintPreview
        End If
        moPrintPreview.PrintProcess(mobjPrintDocument, mProcess, mRenderer)

    End Sub

    Public Sub PrintOnOnePage()

        If moPrintPreview Is Nothing Then
            moPrintPreview = New frmPrintPreview
        End If
        moPrintPreview.PrintOnOnePage(mobjPrintDocument, mProcess, mRenderer)

    End Sub

#End Region

#Region "Start Params"

    ''' <summary>
    ''' Updates the menu option of this viewer's parent, to enable/disable the
    ''' "set start parameters" option, depending on whether it is appropriate
    ''' for the supplied subsheet.
    ''' </summary>
    ''' <param name="gSubSheetID">The subsheet currently being viewed.</param>
    Private Sub UpdateStartParamsEnabled(ByVal gSubSheetID As Guid)
        Dim objStage As clsProcessStage
        If gSubSheetID.Equals(Guid.Empty) Then
            objStage = mProcess.GetStage(mProcess.GetStartStage)
        Else
            objStage = mProcess.GetStage(mProcess.GetSubSheetStartStage(gSubSheetID))
        End If

        Debug.Assert(Not objStage Is Nothing)
        'Just having an assert here means that developers get notified, while real users
        'get a crash and can't access the page at all if this happens, which it just has
        'as described in bug #3219. It is better than they can get into the page!!
        If objStage Is Nothing Then Exit Sub

        If TypeOf mobjParentForm Is frmProcess Then
            CType(mobjParentForm, frmProcess).SetStartParamsEnabled(objStage.GetParameters.Count() > 0)
        End If
    End Sub



#End Region

#Region "Logging"

    Dim mLogger As clsDBLoggingEngine

    ''' <summary>
    ''' If neccesary, Sets up a debug session and attaches a logger to the process.
    ''' </summary>
    Public Sub SetupDebugSessionAndLogging()

        ' If we have no process, don't do anything
        If mProcess Is Nothing Then Return

        ' If we have a process which is already part of a session, don't do anything
        Dim session As clsSession = mProcess.Session
        If session IsNot Nothing Then Return

        ' So we're creating a new session, set it up as required
        Dim name As String = ResourceMachine.GetName() & "_debug"

        gSv.RegisterResourcePC(name, clsUtility.GetFQDN(),
         ResourceDBStatus.Ready, False, ResourceAttribute.Debug, True,
         User.CurrentId, CultureInfo.CurrentCulture.NativeName
         )

        Dim resId As Guid = gSv.GetResourceId(name)
        Dim sessNo As Integer
        Dim sessId = Guid.NewGuid
        Try
            gSv.CreateDebugSession(
             mProcessId, resId, resId, DateTimeOffset.Now, sessId, sessNo)
        Catch
            'Ignore exception
        End Try
        Dim sessionConnectionSettings = gSv.GetWebConnectionSettings()
        session = New clsSession(sessId, sessNo, sessionConnectionSettings)

        mProcess.Session = session

        mLogger = New clsDBLoggingEngine(
         Environment.UserName, resId, mProcess, True, sessId, sessNo)

    End Sub

    ''' <summary>
    ''' If necessary, finishes the debug session and removes the logger from the
    ''' process.
    ''' </summary>
    Public Sub FinishDebugSessionAndLogging()
        If Not mProcess Is Nothing Then
            If Not mOpenedAsDebugSubProcess AndAlso mProcess.Session IsNot Nothing Then
                Try
                    gSv.FinishDebugSession(mProcess.Session.ID, DateTimeOffset.Now)
                Catch
                    'Ignore Exception
                End Try
            End If
            If mLogger IsNot Nothing Then mLogger.Dispose() : mLogger = Nothing
        End If
    End Sub

#End Region

#Region "database and xml"


#Region "Database"


    Public Event ValidationErrorCountUpdated(ByVal Count As Integer)

    Public Enum DoValidateAction
        Close
        Save
        SaveAs
        PublishProcess
        PublishAction
    End Enum

    Public Enum DoValidateOutcome

        ''' <summary>
        '''  Allow the action requested by the user.
        ''' </summary>
        Allow

        ''' <summary>
        ''' Cancel the action requested by the user.
        ''' </summary>
        Cancel

        ''' <summary>
        ''' Discard the changes implemented by the user which have caused the
        ''' validation to fail.
        ''' </summary>
        Discard

    End Enum

    ''' <summary>
    ''' Validates the process and checks against design controls
    ''' </summary>
    ''' <param name="userAction">The action that the user is trying to perform.</param>
    ''' <returns>Validation outcome which can be to cancel the action discard
    ''' the changes, or Ignore</returns>
    Private Function DoValidate(ByVal userAction As DoValidateAction,
                                Optional ByVal subsheet As clsProcessSubSheet = Nothing) _
        As DoValidateOutcome

        Try
            clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesOnce

            Dim rules = gSv.GetValidationInfo()
            Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)

            Dim exTypes As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)

            ' if we are performing exception type validation we need to pass in the
            ' list of existing exception types
            Try
                If validationInfo(142).Enabled Then
                    Dim exceptionTypes = gSv.GetExceptionTypes()
                    exTypes.UnionWith(exceptionTypes)
                End If
            Catch ex As Exception
                UserMessage.ShowExceptionMessage(ex)
            End Try

            Dim validationResults As ICollection(Of ValidateProcessResult) =
         clsValidationInfo.FilteredValidateProcess(mProcess, validationInfo,
                                                   exTypes)

            Dim actionSettings As IDictionary(Of Integer, IDictionary(Of Integer, Integer))
            actionSettings = gSv.GetValidationAllActionSettings()


            'Fortunately for now the highest action id is also the most restrictive
            'So we can simply get the highest actionid and this will give the most
            'restrictive action of all the errors.
            Dim actionID As Integer = 0
            Dim errorCount As Integer = 0
            For Each Result As ValidateProcessResult In validationResults
                If subsheet IsNot Nothing Then
                    Dim st As clsProcessStage = mProcess.GetStage(Result.StageId)
                    If st IsNot Nothing AndAlso st.GetSubSheetID <> subsheet.ID Then
                        Continue For
                    End If
                End If

                Dim info As clsValidationInfo = validationInfo(Result.CheckID)
                Dim currentActionID As Integer = actionSettings(info.CatID)(info.TypeID)
                If info.TypeID = clsValidationInfo.Types.Error Then
                    errorCount += 1
                End If
                If currentActionID > actionID Then
                    actionID = currentActionID
                End If
            Next

            FireValidationErrorCountUpdated(errorCount)

            Dim mostRestrictiveAction As clsValidationInfo.Actions = CType(actionID, clsValidationInfo.Actions)
            Dim tp As DiagramType = DiagramType.Process
            If ModeIsObjectStudio() Then tp = DiagramType.Object
            Dim f As frmValidateAndSaveError = frmValidateAndSaveError.CreateForm(
             TryCast(Me.TopLevelControl, Form), tp, mostRestrictiveAction, userAction)
            f.SetEnvironmentColoursFromAncestor(TopLevelControl)
            f.ShowInTaskbar = False


            Select Case mostRestrictiveAction

            ' If the most restrictive thing is validation, there's no save error going to
            ' occur - allow the action, no matter what it is
                Case Actions.Ignore, Actions.Validate
                    Return DoValidateOutcome.Allow

                Case Actions.PreventPublication
                    Select Case userAction
                        Case DoValidateAction.PublishProcess
                            f.ShowDialog(Me)
                            mProcess.Attributes = mProcess.Attributes And (Not ProcessAttributes.Published)
                            Return DoValidateOutcome.Allow

                        Case DoValidateAction.PublishAction
                            f.ShowDialog(Me)
                            If Not subsheet Is Nothing Then
                                subsheet.Published = False
                            End If
                            Return DoValidateOutcome.Allow

                        Case Else
                            If mProcess.Attributes = ProcessAttributes.Published Then
                                If f.ShowDialog(Me) = DialogResult.Yes Then
                                    mProcess.Attributes = mProcess.Attributes And (Not ProcessAttributes.Published)
                                    Return DoValidateOutcome.Allow
                                End If
                            Else
                                Return DoValidateOutcome.Allow
                            End If
                    End Select

                Case Actions.PreventSave
                    Select Case userAction
                        Case DoValidateAction.PublishProcess, DoValidateAction.PublishAction
                            Return DoValidateOutcome.Allow
                        Case DoValidateAction.Close
                            If f.ShowDialog(Me) = DialogResult.Yes Then
                                Return DoValidateOutcome.Discard
                            Else
                                Return DoValidateOutcome.Cancel
                            End If
                        Case Else
                            f.ShowDialog()
                            Return DoValidateOutcome.Cancel
                    End Select
            End Select

            Return DoValidateOutcome.Cancel
        Finally
            clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime
        End Try

    End Function

    Private Sub UpdateValidationCount()

        Dim rules = gSv.GetValidationInfo()
        Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)
        Dim exTypes As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)

        ' if we are performing exception type validation we need to pass in the
        ' list of existing exception types
        Try
            If validationInfo(142).Enabled Then
                Dim exList = gSv.GetExceptionTypes()
                exTypes.UnionWith(exList)
            End If
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
        End Try


        Dim validationResults As ICollection(Of ValidateProcessResult) =
         clsValidationInfo.FilteredValidateProcess(mProcess, validationInfo,
                                                   exTypes)

        Dim errorCount As Integer = 0
        For Each Result As ValidateProcessResult In validationResults
            Dim info As clsValidationInfo = validationInfo(Result.CheckID)
            If info.TypeID = clsValidationInfo.Types.Error Then
                errorCount += 1
            End If
        Next
        FireValidationErrorCountUpdated(errorCount)
    End Sub

    Public Sub FireValidationErrorCountUpdated(ByVal errorCount As Integer)
        mValidationErrorCount = errorCount
        RaiseEvent ValidationErrorCountUpdated(errorCount)
    End Sub
    Private mValidationErrorCount As Integer

    ''' <summary>
    ''' Validates the process and if allowed saves it.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DoValidateAndSave(ByVal userAction As DoValidateAction) As Boolean
        If gSv.GetPref(PreferenceNames.AutoValidation.AutoValidateOnSave, True) Then
            If mProcess.ParentObject IsNot Nothing Then
                'Reload shared model (for validation)
                LoadParentApplicationModel()
            End If
            If Not DoValidate(userAction) = DoValidateOutcome.Allow Then Return False
        End If
        Return DoSave()
    End Function

    ''' <summary>
    ''' Raises the <see cref="Saved"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the saved process</param>
    Protected Overridable Sub OnSaved(e As ProcessEventArgs)
        RaiseEvent Saved(Me, e)
    End Sub

    ''' <summary>
    ''' Saves the current process after prompting for an edit summary (where
    ''' appropriate).
    ''' </summary>
    Public Function DoSave() As Boolean

        Dim bUseBusinessObjects, bShowSummaryTextBox, bShowBackupMessage As Boolean
        Dim sMessage As String = ""
        Dim sSummary, sThing, sTitle As String
        Dim iResult As MsgBoxResult

        If Not CanExecuteAllLinkedProcesses() Then
            UserMessage.Show(String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_CannotBeSavedPermissionToExecuteAllLinked, GetOpenObjectType))
            Return False
        End If

        bUseBusinessObjects = ModeIsObjectStudio()

        If bUseBusinessObjects Then
            sTitle = My.Resources.ctlProcessViewer_SaveBusinessObject
            sSummary = My.Resources.ctlProcessViewer_BusinessObjectCreated
            sThing = My.Resources.ctlProcessViewer_BusinessObjectNoun
        Else
            sTitle = My.Resources.ctlProcessViewer_SaveProcess
            sSummary = My.Resources.ctlProcessViewer_ProcessCreated
            sThing = My.Resources.ctlProcessViewer_ProcessNoun
        End If

        If Not Me.mThisProcessHasNeverEverBeenSaved Then
            sSummary = ""
        End If
        Dim configOptions = Options.Instance
        bShowSummaryTextBox = Not Me.mThisProcessHasNeverEverBeenSaved _
         AndAlso (configOptions.EditSummariesAreCompulsory Or Me.mbSummariesEnabled)

        bShowBackupMessage = False


        'Don't want to show this text if only back up text is going to be shown.
        If bShowSummaryTextBox OrElse bShowBackupMessage Then
            sMessage = My.Resources.ctlProcessViewer_PleaseEnterASummaryOfTheChangesThatYouHaveMadeBeforeSaving
        End If

        'If an original back up exists then this edit session is the first 
        'since a 'restore from back up' was executed by the administrator. 
        'If a previously restored back up exists then this edit session is  
        'the first since an 'undo restore' was done. In either case the user 
        'must be made aware that saving now will make the change permanent.

        If Not ModeIsCloning() Then
            'stop cloned process geting originals backups

            If Me.mbAutosaveBackupExistedAtOpeningTime Then
                sMessage = String.Format(My.Resources.ctlProcessViewer_NoteThatBySavingThis0YouWillBeCommittedToTheSavedVersionAnyAutoSavedBackupWillN, sThing)
                bShowBackupMessage = True
            End If

        End If

        If bShowSummaryTextBox OrElse bShowBackupMessage Then

            iResult = frmPromptToSave.ShowForm(Me.TopLevelControl, bShowSummaryTextBox, bShowSummaryTextBox,
            configOptions.EditSummariesAreCompulsory, False,
            sSummary, sTitle, sMessage:=sMessage, bUseBusinessObjects:=bUseBusinessObjects)

            If Not iResult = MsgBoxResult.Yes Then
                Return False
            End If

        End If

        If Not mLostLock Then
            gSv.DeleteProcessAutoSaves(mProcessId)
        End If
        If Me.mbAutosaveBackupExistedAtOpeningTime Then
            mbAutosaveBackupExistedAtOpeningTime = False
            If mAutoSaver Is Nothing Then
                'If we have saved an autosaved process version, then autosave
                'ontop of the autosave has been disabled up to now. However when
                'saving the autosaved version, we need to start doing backups again
                mAutoSaver = New clsAutoSaver(mProcess, mProcessId)
            End If
        End If
        Return DoSaveToDataBase(sSummary)
    End Function

    ''' <summary>
    ''' Pops up a wizard inviting the user to choose a name and a description
    ''' for the new process, and saves the process on completion.
    ''' 
    ''' From that point forward, future saves will be to the process saved to,
    ''' not to the process originally opened.
    ''' </summary>
    Public Function DoSaveAs() As Boolean

        Dim objectStudio = ModeIsObjectStudio()

        Dim permission = If(objectStudio,
            Auth.Permission.ObjectStudio.CreateBusinessObject,
            Auth.Permission.ProcessStudio.CreateProcess)

        If Not User.Current.HasPermission(permission) Then
            Dim name = If(objectStudio, My.Resources.ctlProcessViewer_BusinessObjectPronoun, My.Resources.ctlProcessViewer_ProcessPronoun)
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_YouDoNotHavePermissionToCreateA0, name))
            Return False
        End If

        If gSv.GetPref(PreferenceNames.AutoValidation.AutoValidateOnSave, True) _
         AndAlso Not DoValidate(DoValidateAction.SaveAs) = DoValidateOutcome.Allow Then
            Return False
        End If

        Dim wizardType = If(objectStudio,
            frmWizard.WizardType.BusinessObject,
            frmWizard.WizardType.Process)

        Using createProcessWizard As New frmProcessCreate(wizardType, True)

            createProcessWizard.SetEnvironmentColoursFromAncestor(Me)
            createProcessWizard.ShowInTaskbar = False
            createProcessWizard.ShowDialog(Me)

            If createProcessWizard.DialogResult = DialogResult.OK Then
                mMode = If(objectStudio,
                    ProcessViewMode.CloneObject,
                    ProcessViewMode.CloneProcess)

                msDescription = createProcessWizard.GetChosenProcessDescription
                msName = createProcessWizard.GetChosenProcessName
                msSaveInGroup = createProcessWizard.GetChosenGroupName
                mThisProcessHasNeverEverBeenSaved = True

                If DoSave() Then
                    mProcess.Attributes = ProcessAttributes.None
                    mProcessAttributesAtLastSave = ProcessAttributes.None
                    Return True
                Else
                    Return False
                End If

            Else
                Return False
            End If
        End Using
    End Function

    ''' <summary>
    ''' Fires the specified audit event for the current process / object. This will
    ''' ensure that the appropriate type of event is fired depending on the current
    ''' mode, and will pass the current process / object ID to the audit event.
    ''' 
    ''' If the auditing fails, a warning user message is displayed and False is
    ''' returned, otherwise there is no message and it will return True.
    ''' </summary>
    ''' <param name="eventId">The <em>Number</em> part of the audit event ID.
    ''' This will be prefixed by "P" or "B" depending on whether the current subject
    ''' is a process or a business object, respectively.</param>
    ''' <param name="comments">The comments for the audit event.</param>
    ''' <param name="newXml">The current XML for the audit event, if appropriate.
    ''' </param>
    ''' <param name="summary">Any summary info for the subject process/object</param>
    ''' <returns>True if the audit event was written to the database successfully;
    ''' False otherwise.</returns>
    Private Function FireAuditEvent(
     ByVal eventID As AuditEventID,
     ByVal comments As String,
     ByVal newXML As String,
     ByVal summary As String) _
     As Boolean

        Try
            If ModeIsObjectStudio() Then
                Dim eventCode As BusinessObjectEventCode = CType(eventID, BusinessObjectEventCode)
                gSv.AuditRecordBusinessObjectEvent(eventCode, mProcessId, comments, newXML, summary)
            Else
                Dim eventCode As ProcessEventCode = CType(eventID, ProcessEventCode)
                gSv.AuditRecordProcessEvent(eventCode, mProcessId, comments, newXML, summary)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_WarningAuditRecordingFailedWithError0, ex.Message))
            Return False
        End Try

        Return True

    End Function

    ''' <summary>
    ''' Enum used to unify Process Audit and Business Object Audit event IDs,
    ''' used by the FireAuditEvent Function.
    ''' </summary>
    Private Enum AuditEventID
        Create = 1
        Delete = 2
        Clone = 3
        Modify = 4
        Unlock = 5
        Import = 6
        Export = 7
        ChangedAttributes = 9
    End Enum

    ''' <summary>
    ''' Saves the current process object as XML to the database.
    ''' </summary>
    ''' <param name="summary">A summary of the changes made in the since the last
    ''' save. To be stored in the audit table.</param>
    Private Function DoSaveToDataBase(Optional ByVal summary As String = "") As Boolean
        If mProcess Is Nothing Then Return False

        Dim thing As String = CStr(IIf(ModeIsObjectStudio(), My.Resources.ctlProcessViewer_Object, My.Resources.ctlProcessViewer_Process))

        If mProcess.IsDisposed Then
            ' It shouldn't ever reach here.
            Debug.Fail($"Trying to save a disposed process : {mProcess.Name}")
            ' Better than overwriting their data with nothing. Let the user know what
            ' happened and return
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_UnableToSaveThe01ItHasAlreadyBeenDisposedOf, thing, mProcess.Name))
            Return False
        End If

        Dim sXml As String
        Dim gCreatedBy As Guid
        Dim gModifiedBy As Guid
        Dim createDate As DateTime
        Dim modifiedDate As DateTime
        Dim bSuccess As Boolean = True

        If ModeIsCloning() Then

            Dim oldName = mProcess.Name
            Dim oldDescription = mProcess.Description
            Dim oldCreatedBy = mProcess.CreatedBy
            Dim oldCreatedDate = mProcess.CreatedDate

            'Overwrite the process name and description with the new one since we are cloning.
            mProcess.Name = msName
            mProcess.Description = msDescription
            mProcess.CreatedBy = User.Current.Name
            mProcess.CreatedDate = Now

            sXml = mProcess.GenerateXML()
            mgNewProcessID = Guid.NewGuid

            Try
                gSv.CloneProcess(mgNewProcessID, Me.msName, mProcess.Version, Me.msDescription,
                                  sXml, False, ModeIsObjectStudio, mProcess.GetDependencies(False), msSaveInGroup, summary)
            Catch ex As Exception

                UserMessage.Err(ex, ex.Message)
                mProcess.Name = oldName
                mProcess.Description = oldDescription
                mProcess.CreatedBy = oldCreatedBy
                mProcess.CreatedDate = oldCreatedDate

                Return False
            End Try

            'The new process is locked in the create method, but we need to unlock the original one (if we still have it)
            If Not mLostLock Then gSv.UnlockProcess(mProcessId)

            'Set the process data state to 'unchanged'
            mProcess.ResetChanged()

            'Change mode to edit, since it is not new any
            'more - otherwise subsequent attempts to save will fail
            If ModeIsObjectStudio() Then
                mMode = ProcessViewMode.EditObject
            Else
                mMode = ProcessViewMode.EditProcess
            End If

            'and make the new process the one we are working on
            Dim oldProcessID = mProcessId
            mProcessId = mgNewProcessID
            mLostLock = False
            If mAutoSaver IsNot Nothing Then
                gSv.DeleteProcessAutoSaves(oldProcessID)
                mAutoSaver.Dispose()
                mAutoSaver = New clsAutoSaver(mProcess, mProcessId)
            End If

            'set process attribtutes
            If mProcess.Attributes <> mProcessAttributesAtLastSave Then
                Try
                    gSv.SetProcessAttributes(mgNewProcessID, mProcess.Attributes)
                    'record audit event for new attributes
                    FireAuditEvent(AuditEventID.ChangedAttributes, String.Format(My.Resources.ctlProcessViewer_NewAttributesAre0AndOldAttributesWere1,
                    CInt(mProcess.Attributes).ToString, CInt(Me.mProcessAttributesAtLastSave).ToString), Nothing, Nothing)
                Catch ex As Exception
                    UserMessage.ShowExceptionMessage(ex)
                    bSuccess = False
                End Try
                Me.mProcessAttributesAtLastSave = Me.mProcess.Attributes
            End If

            'set status bar message
            Dim StatusBarMessage As String = String.Format(My.Resources.ctlProcessViewer_ProcessCloned0, Date.Now.ToString)
            If ModeIsObjectStudio() Then StatusBarMessage = String.Format(My.Resources.ctlProcessViewer_BusinessObjectCloned0, Date.Now.ToString)
            If TypeOf mobjParentForm Is IProcessViewingForm Then CType(mobjParentForm, IProcessViewingForm).SetStatusBarText(StatusBarMessage, 0)

            'lock the process if this is the first ever time it has
            'been saved
            If Me.mThisProcessHasNeverEverBeenSaved Then
                Try
                    gSv.LockProcess(mProcessId)
                Catch ex As Exception
                    UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_DatabaseErrorFailedToLockThisNewProcessBusinessObjectTheErrorMessageWas0, ex.Message))
                    bSuccess = False
                End Try

            End If

            gCreatedBy = User.Current.Id
            gModifiedBy = User.Current.Id
            createDate = DateTime.Now
            modifiedDate = DateTime.Now
            gSv.SetProcessInfo(mProcessId, gCreatedBy, createDate, gModifiedBy, modifiedDate, True)
        Else
            sXml = mProcess.GenerateXML()

            Try
                gSv.EditProcess(mProcessId, mProcess.Name, mProcess.Version, mProcess.Description,
                                   sXml, Nothing, summary, modifiedDate, mProcess.GetDependencies(False))

                'Reset the changed status of the process
                mProcess.ResetChanged()

                'set process attribtutes
                If mProcess.Attributes <> mProcessAttributesAtLastSave Then
                    Try
                        gSv.SetProcessAttributes(mProcessId, mProcess.Attributes)
                        'record audit event for new attributes
                        FireAuditEvent(AuditEventID.ChangedAttributes, String.Format(My.Resources.ctlProcessViewer_NewAttributesAre0AndOldAttributesWere1, CInt(mProcess.Attributes).ToString,
                        CInt(Me.mProcessAttributesAtLastSave).ToString), Nothing, Nothing)
                    Catch ex As Exception
                        UserMessage.ShowExceptionMessage(ex)
                        bSuccess = False
                    End Try
                    Me.mProcessAttributesAtLastSave = Me.mProcess.Attributes
                End If

                SaveExceptionTypes()

                If TypeOf mobjParentForm Is IProcessViewingForm Then

                    If ModeIsObjectStudio() Then
                        CType(mobjParentForm, IProcessViewingForm).SetStatusBarText(String.Format(My.Resources.ctlProcessViewer_BusinessObjectSaved0, Date.Now.ToString), 0)
                    Else
                        CType(mobjParentForm, IProcessViewingForm).SetStatusBarText(String.Format(My.Resources.ctlProcessViewer_ProcessSaved0, Date.Now.ToString), 0)
                    End If

                End If

                'If we are recovering a session backup, then the backup
                'can be deleted once the save is made.
                gSv.DeleteProcessAutoSaves(mProcessId)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_AnErrorOccurredWhilstEditingTheProcess0))
                bSuccess = False
            End Try
        End If

        mProcess.ModifiedBy = User.Current.Name
        mProcess.ModifiedDate = modifiedDate

        'If the process has been saved then it can be exported
        If TypeOf mobjParentForm Is frmProcess Then
            CType(mobjParentForm, frmProcess).SetProcessAsExportable(True)
        End If

        'the process has been saved so no need for this anymore:
        Me.mThisProcessHasNeverEverBeenSaved = False

        UpdateParentTitle()

        If bSuccess Then OnSaved(New ProcessEventArgs(mProcess))

        Return bSuccess

    End Function

    ''' <summary>
    ''' Finds all the exception stages in the current process and saves the exception 
    ''' types used within them to the database table. 
    ''' </summary>
    Private Sub SaveExceptionTypes()
        ' find all distinct exception types from the process
        Dim exceptionTypesToSave As IEnumerable(Of String) =
                mProcess.
                GetStages(Of clsExceptionStage).
                Select(Function(e) e.ExceptionType).
                Distinct(StringComparer.CurrentCultureIgnoreCase).
                Where(Function(e) e IsNot Nothing).
                ToList()

        If exceptionTypesToSave.Any Then _
            gSv.AddExceptionTypes(exceptionTypesToSave)
    End Sub

#End Region

    ''' <summary>
    ''' Opens the XML from a string of XML
    ''' </summary>
    ''' <param name="sXML"></param>
    ''' <returns>True if successful, False otherwise</returns>
    Private Function DoOpenXML(ByVal sXML As String) As Boolean
        Dim sErr As String = Nothing
        Dim configOptions = Options.Instance
        mProcess = clsProcess.FromXML(configOptions.GetExternalObjectsInfo(), sXML, True, sErr)
        If mProcess Is Nothing Then
            UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_ErrorLoading0, sErr))
            Return False
        End If
        'If object uses a shared model then load it too
        If mProcess.ParentObject IsNot Nothing Then
            If LoadParentApplicationModel() Then
                Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_SharedApplicationModel, My.Resources.ctlProcessViewer_LoadedFromObject _
                 & mProcess.ParentObject, ToolTipIcon.Info)
            Else
                Me.ShowFloatingMessage(My.Resources.ctlProcessViewer_SharedApplicationModel, My.Resources.ctlProcessViewer_MissingParentObject _
                 & mProcess.ParentObject, ToolTipIcon.Warning)
            End If
        End If
        mProcess.MaxUndoLevels = configOptions.MaxUndoLevels
        mProcess.Id = mProcessId

        If gSv.GetPref(PreferenceNames.AutoValidation.AutoValidateOnOpen, True) Then
            UpdateValidationCount()
        End If

        InvalidateView()
        Return True
    End Function

    ''' <summary>
    ''' Loads the object's parent XML and accesses shared model
    ''' </summary>
    ''' <returns>True if successful, False otherwise</returns>
    Public Function LoadParentApplicationModel() As Boolean

        GetParentApplicationModelId()
        'Check parent exists & load it
        If mParentObjectID = Guid.Empty Then Return False
        mProcess.LoadParent(mParentObjectID)
        Return True
    End Function

    Public Sub GetParentApplicationModelId()
        If Not String.IsNullOrEmpty(mProcess.ParentObject) Then
            mParentObjectID = gSv.GetProcessIDByName(mProcess.ParentObject, True)
        End If
    End Sub

#End Region

#Region "Loading and Closing"

    ''' <summary>
    ''' Opens the process supplied.
    ''' </summary>
    ''' <param name="sXML">The full XML representation of the process to be
    ''' opened.</param>
    ''' <param name="gProcessID">The ID of the process corresponding to the XML
    ''' supplied.</param>
    Public Sub Startup(ByVal mode As ProcessViewMode, ByVal sXML As String, ByVal gProcessID As Guid, ByRef bStartOK As Boolean)
        msXML = sXML
        Startup(mode, gProcessID, "", "", bStartOK, Nothing)
    End Sub

    ''' <summary>
    ''' When true, we are viewing a process for which an autosave record
    ''' existed upon opening. The user may now be viewing the original
    ''' or the autosaved version.
    ''' </summary>
    ''' <remarks>Gets reset when the user saves, etc.</remarks>
    Private mbAutosaveBackupExistedAtOpeningTime As Boolean

    ''' <summary>
    ''' Opens a process and draws it for viewing; sets the initial state of class
    ''' members.
    ''' </summary>
    ''' <param name="imode">The mode in which the process will be available (e.g.
    ''' readonly, editable etc)</param>
    ''' <param name="gProcessID">The identifier of the process to be opened.</param>
    ''' <param name="sName">Used when creating a new process - the name to give to
    ''' the new process.</param>
    ''' <param name="sDescription">Used when creating a new process - the description
    ''' to give to the new process.</param>
    ''' <param name="objProcess">If not Nothing, this is an already constructed
    ''' process which we should use. Supplying this makes no sense if used in
    ''' conjunction with a 'new process/object' mode.</param>
    Public Sub Startup(ByVal imode As ProcessViewMode, ByVal gProcessID As Guid, ByVal sname As String, ByVal sdescription As String, ByRef bStartOK As Boolean, ByVal objProcess As clsProcess)

        clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesOnce

        bStartOK = True
        mProcessId = gProcessID
        mMode = imode

        If objProcess IsNot Nothing Then objProcess.MaxUndoLevels = Options.Instance.MaxUndoLevels
        mProcess = objProcess

        'Create renderer...
        mRenderer = New clsRenderer(Me)

        MouseDragging = False
        frmCalculationZoom.Disabled(Me) = True
        mobjPrintDocument = New PrintDocument

        
        Dim sCreatedBy As String = Nothing
        Dim dCreateDate As Date
        Dim sModifiedBy As String = User.Current.Name
        Dim dModifiedDate As Date = Now
        mobjParentForm.Hide()
        'refresh env vars on process load 
        clsAPC.ProcessLoader.GetEnvVars(True)
        mbAutosaveBackupExistedAtOpeningTime = gSv.AutoSaveBackupSessionExistsForProcess(gProcessID)

selectagain:
        Select Case mMode

            Case ProcessViewMode.EditProcess, ProcessViewMode.EditObject

                If mProcessId = Guid.Empty Then
                    UserMessage.Show(My.Resources.ctlProcessViewer_InternalErrorNoProcessIDFoundForCurrentProcess)
                    bStartOK = False
                    Exit Sub
                End If
RetryLock:
                Try
                    gSv.LockProcess(mProcessId)
                Catch ex As Exception
                    Dim Message As String = ex.Message & String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_Startup_CannotEditWhileLockedViewInstead, GetOpenObjectType)
                    Dim DialogResult As MsgBoxResult = UserMessage.YesNoCancelWithLinkLabel(Message, My.Resources.ctlProcessViewer_UnlockingWizard, AddressOf UnlockProcessesLinkLabelClicked_Click, CancelButtonText:=My.Resources.ctlProcessViewer_Retry)
                    Select Case DialogResult
                        Case MsgBoxResult.Yes
                            If ModeIsObjectStudio() Then
                                mMode = ProcessViewMode.PreviewObject
                            Else
                                mMode = ProcessViewMode.PreviewProcess
                            End If
                            GoTo selectagain
                        Case MsgBoxResult.Cancel
                            GoTo RetryLock
                        Case Else
                            mMode = ProcessViewMode.PreviewProcess
                            mobjParentForm.Close()
                            bStartOK = False
                            Exit Sub
                    End Select
                End Try

                If mProcess Is Nothing Then
                    If Not TryOpenProcessXML(msXML) Then
                        bStartOK = False
                        Exit Sub
                    End If
                End If

                gSv.GetProcessInfo(mProcessId, sCreatedBy, dCreateDate, sModifiedBy, dModifiedDate)
                mProcess.ModifiedBy = sModifiedBy
                mProcess.ModifiedDate = dModifiedDate
                mProcess.CreatedBy = sCreatedBy
                mProcess.CreatedDate = dCreateDate

                If Not CanViewAllObjects() Then
                    UserMessage.Show(String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_Startup_ContainsReferencesThatAreNotAvailable, GetOpenObjectType))
                ElseIf Not HasRequiredPermissions() Then
                    UserMessage.Show(String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_CannotBeSavedPermissionToExecuteAllLinked, GetOpenObjectType))
                End If

                If TypeOf mobjParentForm Is frmProcess Then
                    CType(mobjParentForm, frmProcess).SetNTFYDebugText(mProcess.Name)
                End If

                'make an auto save object
                If Not Me.mbAutosaveBackupExistedAtOpeningTime Then
                    mAutoSaver = New clsAutoSaver(mProcess, mProcessId)
                End If

            Case ProcessViewMode.CloneProcess, ProcessViewMode.CloneObject
                Throw New InvalidArgumentException(My.Resources.ctlProcessViewer_InternalErrorProcessStudioShouldNoLongerBeOpenedInCloneMode)

            Case ProcessViewMode.PreviewProcess, ProcessViewMode.PreviewObject

                If msXML = Nothing Then
                    If mProcess Is Nothing Then
                        If Not OpenProcessXML(mProcessId) Then
                            bStartOK = False
                            Exit Sub
                        End If
                    End If
                Else
                    'we let the DoOpenXML method get the corresponding info
                    'to this line above
                    If mProcess Is Nothing Then
                        If Not DoOpenXML(msXML) Then
                            bStartOK = False
                            Exit Sub
                        End If
                    End If
                End If

                gSv.GetProcessInfo(mProcessId, sCreatedBy, dCreateDate, sModifiedBy, dModifiedDate)
                mProcess.ModifiedBy = sModifiedBy
                mProcess.ModifiedDate = dModifiedDate
                mProcess.CreatedBy = sCreatedBy
                mProcess.CreatedDate = dCreateDate

                mnuTabRename.Enabled = False
                Me.mnuTabDelete.Enabled = False


            Case ProcessViewMode.DebugProcess, ProcessViewMode.DebugObject

                If mProcess Is Nothing Then
                    If Not OpenProcessXML(mProcessId) Then
                        bStartOK = False
                        Exit Sub
                    End If
                End If

                gSv.GetProcessInfo(mProcessId, sCreatedBy, dCreateDate, sModifiedBy, dModifiedDate)
                mProcess.ModifiedBy = sModifiedBy
                mProcess.ModifiedDate = dModifiedDate
                mProcess.CreatedBy = sCreatedBy
                mProcess.CreatedDate = dCreateDate

                If TypeOf mobjParentForm Is frmProcess Then
                    CType(mobjParentForm, frmProcess).SetNTFYDebugText(mProcess.Name)
                End If

            Case ProcessViewMode.AdHocTestProcess, ProcessViewMode.AdHocTestObject
                If mProcess Is Nothing Then
                    If Not OpenProcessXML(mProcessId) Then
                        bStartOK = False
                        Exit Sub
                    End If
                End If

                If TypeOf mobjParentForm Is frmProcess Then
                    CType(mobjParentForm, frmProcess).SetNTFYDebugText(mProcess.Name)
                End If

            Case ProcessViewMode.CompareProcesses, ProcessViewMode.CompareObjects

                If mProcess Is Nothing Then
                    If Not DoOpenXML(msXML) Then
                        bStartOK = False
                        Exit Sub
                    End If
                End If

                gSv.GetProcessInfo(mProcessId, sCreatedBy, dCreateDate, sModifiedBy, dModifiedDate)
                mProcess.ModifiedBy = sModifiedBy
                mProcess.ModifiedDate = dModifiedDate
                mProcess.CreatedBy = sCreatedBy
                mProcess.CreatedDate = dCreateDate

        End Select



        'get process' attributes
        Try
            Me.mProcess.Attributes = gSv.GetProcessAttributes(Me.mProcessId)
        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlProcessViewer_CouldNotGetProcessAttributes, ex)
            bStartOK = False
            Exit Sub
        End Try

        Me.mProcessAttributesAtLastSave = Me.mProcess.Attributes

        MouseDragging = False
        mbLoaded = True
        UpdateSheetTabs()

        UpdateStartParamsEnabled(Guid.Empty)
        InvalidateView()

        ' Reset the changed indicator in the process
        mProcess.ResetChanged()

        UpdateParentTitle()

        clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime
    End Sub

    Private Function TryOpenProcessXML(xml As String) As Boolean
        If String.IsNullOrEmpty(xml) Then
            Return OpenProcessXML(mProcessId)
        Else
            Return DoOpenXML(xml)
        End If
    End Function

    Private Function OpenProcessXML(processId As Guid) As Boolean
        Dim xml = String.Empty
        Try

            If clsAPC.ProcessLoader.GetProcessXML(processId, xml, Nothing, Nothing) AndAlso
                 DoOpenXML(xml) Then
                Return True
            End If
        Catch ex As Exception
            Dim errorMessage = $"{String.Format(My.Resources.ctlProcessViewer_CouldNotGetProcessDefinitionForItem, processId)}{Environment.NewLine}{ex.ToString()}"

            Log.Error(errorMessage)
            UserMessage.ShowDetail(My.Resources.ctlProcessViewer_CouldNotGetProcessDefinition, errorMessage)
        End Try
        Return False
    End Function

    Private Function CanExecuteAllLinkedProcesses() As Boolean
        Dim existingCacheSetting = clsAPC.ProcessLoader.CacheBehaviour

        Try
            clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.NeverCheckForUpdates
            Return CanViewAllObjects() AndAlso HasRequiredPermissions()
        Finally
            clsAPC.ProcessLoader.CacheBehaviour = existingCacheSetting
        End Try
    End Function

    Private Function CanViewAllObjects() As Boolean

        If mProcess Is Nothing Then Return False

        Return Not _
            mProcess.GetVisualBusinessObjectsUsedNames() _
            .Except(mProcess.GetBusinessObjectsUsed(True).Select(Function(x) x.FriendlyName)) _
            .Any()
    End Function

    Private Function HasRequiredPermissions() As Boolean

        If mProcess Is Nothing Then Return False

        Dim testResult = gSv.TestMemberCanAccessProcesses(
            mProcess.GetProcessesUsed().ToList())

        If testResult Then
            testResult = gSv.TestMemberCanAccessObjects(
                mProcess.GetBusinessObjectsUsedIds(True).ToList())
        End If

        Return testResult
    End Function

    ''' <summary>
    ''' Show the 'Save/Discard/Cancel' form, if changes have been made, and do the
    ''' save if 'Save' is selected.
    ''' </summary>
    ''' <param name="bForceSave">True if changes have been made that we don't know
    ''' about. A nasty hack to deal with the fact the ctlProcessViewer doesn't know
    ''' about Integration Assistant changes.</param>
    ''' <returns>True if the control can now be closed, False if the user has said
    ''' it should remain open.</returns>
    Public Function PromptToSave(Optional ByVal bForceSave As Boolean = False) As Boolean

        'Check if the user has already discarded any changes...
        If mChangesDiscarded = True Then Return True

        Dim sMessage As String = ""
        Dim sSummary, sTitle, sThing As String
        Dim bUseBusinessObjects As Boolean = (Me.mProcess IsNot Nothing AndAlso mProcess.ProcessType = DiagramType.Object)
        Dim iResult As MsgBoxResult

        If (bForceSave OrElse If(mProcess?.HasChanged(), False)) AndAlso CanExecuteAllLinkedProcesses() Then

            If gSv.GetPref(PreferenceNames.AutoValidation.AutoValidateOnSave, True) Then
                Select Case DoValidate(DoValidateAction.Close)
                    Case DoValidateOutcome.Discard
                        Return True
                    Case DoValidateOutcome.Cancel
                        Return False
                End Select
            End If

            If Me.ModeIsEditable Then

                If bUseBusinessObjects Then
                    sThing = My.Resources.ctlProcessViewer_BusinessObjectNoun
                    sTitle = My.Resources.ctlProcessViewer_SaveBusinessObject
                    sSummary = ""
                Else
                    sThing = My.Resources.ctlProcessViewer_ProcessNoun
                    sTitle = My.Resources.ctlProcessViewer_SaveProcess
                    sSummary = ""
                End If

                'Warn the user, if by saving now we are committing to a fixed backup
                'version
                If Me.mbAutosaveBackupExistedAtOpeningTime Then
                    sMessage = frmPromptToSave.DefaultMessage _
                    & String.Format(My.Resources.ctlProcessViewer_NoteThatBySavingThis0YouWillBeCommittedToTheSavedVersionAndWillNotBeAbleToRever, sThing)

                End If
                Dim configOptions = Options.Instance
                iResult = frmPromptToSave.ShowForm(mobjParentForm, (Me.mbSummariesEnabled Or configOptions.EditSummariesAreCompulsory),
                (Me.mbSummariesEnabled Or configOptions.EditSummariesAreCompulsory),
                configOptions.EditSummariesAreCompulsory, True,
                sSummary, sTitle, sMessage:=sMessage, bUseBusinessObjects:=bUseBusinessObjects)

                Select Case iResult
                    Case MsgBoxResult.Yes
                        DoSaveToDataBase(sSummary)
                    Case MsgBoxResult.No
                        'do nothing - no save needed but we are still going to close
                        If Not Me.mbAutosaveBackupExistedAtOpeningTime Then
                            gSv.DeleteProcessAutoSaves(mProcessId)
                        End If
                        mChangesDiscarded = True
                    Case Else
                        'don't close the form
                        Return False
                End Select

            End If
        ElseIf (bForceSave OrElse If(mProcess?.HasChanged(), False)) AndAlso Not CanExecuteAllLinkedProcesses() Then
            Return UserMessage.OkCancel(String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_CannotBeSavedPermissionToExecuteAllLinkedOnClose, GetOpenObjectType, mProcess.Name)) = MsgBoxResult.Ok
        End If
        Return True

    End Function

    ''' <summary>
    ''' Performs closing action such as prompting the user to save.
    ''' </summary>
    ''' <param name="bForceSave">True if changes have been made that we don't know
    ''' about. A nasty hack to deal with the fact the ctlProcessViewer doesn't know
    ''' about Integration Assistant changes.</param>
    ''' <returns>Returns True if process studio should be closed; otherwise returns
    ''' False (e.g. in the event that the user clicks cancel when prompted to save).
    ''' </returns>
    Public Function CloseDown(Optional ByVal bForceSave As Boolean = False) As Boolean
        Try
            Dim sErr As String = ""
            Me.CloseBreakpointInfo()

            clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.NeverCheckForUpdates

            'if saving is necessary then prompt to save
            If Not mLostLock Then
                If Not PromptToSave(bForceSave) Then
                    Return False
                End If
            End If

            If Not mAutoSaver Is Nothing Then
                mAutoSaver.Dispose()
            End If

            If Me.ModeIsEditable AndAlso Not mLostLock Then
                ' Unlock process
                Dim procId As Guid = If(ModeIsCloning(), mgNewProcessID, mProcessId)
                Try
                    gSv.UnlockProcess(procId)
                Catch ex As Exception
                    UserMessage.Err(ex, String.Format(My.Resources.ctlProcessViewer_ErrorUnlockingProcess0, ex.Message))
                End Try
            End If

            FinishDebugSessionAndLogging()

            Timer1.Enabled = False
            Timer2.Enabled = False
            mClosed = True
            Return True
        Finally
            clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime

            For Each kvp In mPropertyWindows
                kvp.Value.Dispose()
            Next
        End Try
    End Function

    ''' <summary>
    ''' Checks to see if the process is still locked by this user.
    ''' </summary>
    ''' <param name="silent">Provided to allow the caller to suppress the user
    ''' message if the lock has been lost (e.g. when closing form down)</param>
    ''' <returns>Returns true if the lock is still valid.
    ''' </returns>
    ''' <remarks></remarks>
    Public Function UserHasLock(Optional ByVal silent As Boolean = False) As Boolean
        If mThisProcessHasNeverEverBeenSaved Then Return True

        Dim lockUserName As String = Nothing
        Dim lockMachineName As String = Nothing
        gSv.ProcessIsLocked(mProcessId, lockUserName, lockMachineName)
        If lockUserName = User.CurrentName Then Return True

        If Not silent Then
            UserMessage.Show(String.Format(My.Resources.AutomateUI_Controls.ctlProcessViewer_UserHasLock_LockRemovedByAnotherUser, GetOpenObjectType))
        End If


        mLostLock = True
        Return False

    End Function

#End Region

#Region "User Interface"


    ''' <summary>
    ''' Update the parent window's title according to the current status.
    ''' </summary>
    Private Sub UpdateParentTitle()

        Dim sSuffix As String = String.Empty
        If Not mProcess Is Nothing Then
            sSuffix = mProcess.Name
            If mProcess.RunState <> ProcessRunState.Off Then
                If mProcess.ChildWaiting Then
                    sSuffix &= My.Resources.ctlProcessViewer_WAITING
                Else
                    sSuffix &= String.Format(My.Resources.ctlProcessViewer_TitleWithRunState0, TryGetProcessRunStateAsInternationalisedString())
                End If
            End If
        End If
        SetParentTitle(sSuffix)

    End Sub

    ''' <summary>
    ''' Sets the parent window title using the default main title (according
    ''' to the current mode) and appends the supplied suffix.
    ''' </summary>
    ''' <param name="Suffix">The suffix, as defined in overloaded method.
    ''' </param>
    Private Sub SetParentTitle(ByVal Suffix As String)
        If Me.ModeIsObjectStudio Then
            Me.SetParentTitle(My.Resources.ctlProcessViewer_ObjectStudio, Suffix)
        Else
            Me.SetParentTitle(My.Resources.ctlProcessViewer_ProcessStudio, Suffix)
        End If
    End Sub

    ''' <summary>
    ''' Sets the title of the parent window. The title will take the form
    ''' MainTitle - Suffix
    ''' </summary>
    ''' <param name="sTitle">The main title of the form</param>
    ''' <param name="sSuffix">The text to appear after the hyphen in the title.
    ''' </param>
    Private Sub SetParentTitle(ByVal sTitle As String, ByVal sSuffix As String)
        Dim sNewTitle As String
        If Not TypeOf mobjParentForm Is frmProcessComparison Then
            sNewTitle = sTitle & My.Resources.ctlProcessViewer_Hyphen & GetModeShortName()
            If Not sSuffix = "" Then sNewTitle &= My.Resources.ctlProcessViewer_Hyphen & sSuffix
        Else
            sNewTitle = My.Resources.ctlProcessViewer_ProcessComparison
        End If
        If mobjParentForm.Text <> sNewTitle Then mobjParentForm.Text = sNewTitle
    End Sub

    ''' <summary>
    ''' Takes the camera to the specified stage, and updates the tab
    ''' bar to show the correct page.
    ''' </summary>
    ''' <param name="objstage">The stage to show. May be null - nothing happens
    ''' in this case.</param>
    Public Sub ShowStage(ByVal objstage As clsProcessStage, Optional ByVal UseAnimation As Boolean = True)
        Const MinZoom As Single = 0.4!
        If (objstage IsNot Nothing) AndAlso (Not objstage.IsVisible(pbview.Size)) Then

            'Cancel any old animation and get rid of the timer
            If mAnimationTimer IsNot Nothing Then
                EndAnimationTimer()
            End If

            If (Not UseAnimation) OrElse (objstage.GetSubSheetID <> mProcess.GetActiveSubSheet) Then
                mProcess.FocusCameraOnStage(Me.pbview.Width, Me.pbview.Height, objstage)
            Else
                mAnimationStartLocation = mProcess.GetCameraLocation
                mAnimationEndLocation = objstage.Location
                mAnimationDistance = Math.Sqrt(Math.Pow(mAnimationEndLocation.X - mAnimationStartLocation.X, 2) + Math.Pow(mAnimationEndLocation.Y - mAnimationStartLocation.Y, 2))
                mAnimationMinZoom = Math.Max(MinZoom, pbview.Width / mAnimationDistance)
                manimationDirectionVector = New SizeF(mAnimationEndLocation.X - mAnimationStartLocation.X, mAnimationEndLocation.Y - mAnimationStartLocation.Y)

                If mAnimationTargetZoomLevel = -1 Then
                    mAnimationTargetZoomLevel = mProcess.Zoom
                Else
                    'we just cancelled another animation so need to
                    'respect the old target - don't change it.
                End If

                'Prepare new timer
                mAnimationTimer = New System.Windows.Forms.Timer()
                mAnimationTimer.Interval = 60 'approx 16 frames a second
                AddHandler mAnimationTimer.Tick, AddressOf HandleAnimationTimerTick
                mAnimationDuration = mAnimationDefaultDuration
                mAnimationStopwatch = Stopwatch.StartNew()
                mAnimationTimer.Start()
            End If

        End If

        Me.InvalidateView()
    End Sub

    ''' <summary>
    ''' The default duration of any 'goto' animation
    ''' </summary>
    Private Const mAnimationDefaultDuration As Integer = 1000


    ''' <summary>
    ''' The number of ticks for which the animation
    ''' of the 'goto' of Showstage should last for.
    ''' </summary>
    Private mAnimationStopwatch As Stopwatch

    ''' <summary>
    ''' The number of ticks that the animation should
    ''' last for.
    ''' </summary>
    Private mAnimationDuration As Integer

    ''' <summary>
    ''' The starting point of any animation underway.
    ''' </summary>
    Private mAnimationStartLocation As PointF

    ''' <summary>
    ''' The end point of any animated 'goto' underway.
    ''' </summary>
    Private mAnimationEndLocation As PointF

    ''' <summary>
    ''' The direction of the path followed by the 
    ''' animated 'goto'.
    ''' </summary>
    Private manimationDirectionVector As SizeF

    ''' <summary>
    ''' Timer that drives the animated 'goto' for 
    ''' showstage method.
    ''' </summary>
    Private mAnimationTimer As System.Windows.Forms.Timer

    ''' <summary>
    ''' The zoom level that should be in place after
    ''' animation ends.
    ''' </summary>
    ''' <remarks>By convention the value is -1 if no other
    ''' animation is underway. If one animation is started
    ''' whilst another is already underway then the old
    ''' target needs to be retained by the new animation.</remarks>
    Private mAnimationTargetZoomLevel As Double = -1

    ''' <summary>
    ''' The maximum we zoom out during animation.
    ''' </summary>
    Private Const mAnimationOuterMostZoom As Double = 0.4

    ''' <summary>
    ''' The distance (in world coordinates) over which the
    ''' animation will take place. Calculated as the diagonal
    ''' from start point to end point.
    ''' </summary>
    Private mAnimationDistance As Double

    ''' <summary>
    ''' The minimum zoom to use during animation.
    ''' </summary>
    Private mAnimationMinZoom As Double

    Private Sub HandleAnimationTimerTick(ByVal sender As Object, ByVal e As EventArgs)

        ' If we don't have a stopwatch running, we can't calculate the time elapsed
        If mAnimationStopwatch Is Nothing Then Return

        Dim ProportionTimeElapsed As Double = mAnimationStopwatch.ElapsedMilliseconds / mAnimationDuration

        If ProportionTimeElapsed < 1 Then
            Dim ProportionTravelled As Double = (1 - Math.Cos(ProportionTimeElapsed * Math.PI)) / 2 'ie the integral of sin(TimeElapsed) over interval [0,PI/2], scaled to [0,1] by dividing by 2.0
            mProcess.SetCameraLocation(New PointF(CSng(mAnimationStartLocation.X + ProportionTravelled * manimationDirectionVector.Width), CSng(mAnimationStartLocation.Y + ProportionTravelled * manimationDirectionVector.Height)))
            mProcess.Zoom = (CSng(mAnimationTargetZoomLevel - Math.Max(0, (mAnimationTargetZoomLevel - mAnimationMinZoom)) * (Math.Sin(ProportionTimeElapsed * Math.PI))))
            Me.InvalidateView()
        Else
            'Make sure we are actually there
            mProcess.SetCameraLocation(mAnimationEndLocation)
            mProcess.Zoom = (CSng(mAnimationTargetZoomLevel))
            Me.InvalidateView()

            EndAnimationTimer()
        End If
    End Sub

    ''' <summary>
    ''' Stops the timer that drives the UI animation
    ''' </summary>
    Private Sub EndAnimationTimer()
        If mAnimationTimer IsNot Nothing Then
            mAnimationTimer.Stop()
            RemoveHandler mAnimationTimer.Tick, AddressOf HandleAnimationTimerTick
            If mAnimationStopwatch IsNot Nothing Then mAnimationStopwatch.Stop()
            mAnimationStopwatch = Nothing
            mAnimationTimer = Nothing
            mAnimationTargetZoomLevel = -1
        End If
    End Sub

    ''' <summary>
    ''' As for overloaded method.
    ''' </summary>
    ''' <param name="StageID">The ID of the stage to show. If 
    ''' empty, then simply no action is taken.</param>
    Public Sub ShowStage(ByVal StageID As Guid)
        Me.ShowStage(mProcess.GetStage(StageID))
    End Sub

    ''' <summary>
    ''' Focuses the camera on the current debug stage.
    ''' </summary>
    Public Sub FocusDebugStage()
        Dim gID As Guid
        gID = mProcess.RunStageID
        If Not gID.Equals(Guid.Empty) Then
            ShowStage(mProcess.GetStage(gID), False)
        End If
    End Sub

#End Region

#End Region

#Region "Gets and Sets"

    ''' <summary>
    ''' Sets the instance of frmApplication to which this control belongs.
    ''' </summary>
    ''' <param name="objparent">The instance.</param>
    Public Sub SetSuperParent(ByVal objparent As frmApplication)
        mSuperParent = objparent
    End Sub

    ''' <summary>
    ''' Sets the form on which this control resides (we don't use the corresponding
    ''' dotnet property because we want to enforce the use of the Form class in the
    ''' Automate namespace).
    ''' </summary>
    ''' <param name="objparent">The form.</param>
    Public Sub SetParent(ByVal objparent As Form)
        mobjParentForm = objparent
    End Sub


    ''' <summary>
    ''' Indicates that the tool used to manipulate the process diagram has been
    ''' changed.
    ''' </summary>
    ''' <param name="NewTool">The new tool in use.</param>
    Public Event ToolChanged(ByVal NewTool As StudioTool)

    ''' <summary>
    ''' Sets the current tool.
    ''' </summary>
    ''' <param name="newtool">The tool.</param>
    Public Sub SetCurrentTool(ByVal newtool As StudioTool)
        mTool = newtool
        Me.ChangePointer()
        RaiseEvent ToolChanged(newtool)
    End Sub

    ''' <summary>
    ''' Gets the current type of tool being used.
    ''' </summary>
    ''' <returns>The tool currently in use.</returns>
    Public Function GetCurrentTool() As StudioTool
        Return mTool
    End Function

    Public Function IsMouseDragging() As Boolean
        Return MouseDragging AndAlso (mTool <> StudioTool.Pan)
    End Function

    Public Function GetMouseX() As Single
        Return mpMousePos.X
    End Function

    Public Function GetMouseY() As Single
        Return mpMousePos.Y
    End Function

    Public Function GetMouseLocation() As PointF
        Return mpMousePos
    End Function

    Public Function GetMouseDownX() As Single
        Return mpMouseDownWorldPos.X
    End Function

    Public Function GetMouseDownY() As Single
        Return mpMouseDownWorldPos.Y
    End Function

    Public Function GetMouseDownLocation() As PointF
        Return mpMouseDownWorldPos
    End Function

    Public Function GetMouseDragStage() As clsProcessStage
        Return mobjMouseDragStage
    End Function

    Public Function GetMinRubberBandSize() As Single
        Return msngMinRubberBandSize
    End Function

    Public Function GetCalculationZoom() As frmCalculationZoom
        Return mobjCalculationZoom
    End Function

    ''' <summary>
    ''' Enables or disables calculation zoom on this control.
    ''' </summary>
    ''' <param name="bEnabled"></param>
    Public Sub SetCalculationZoomEnabled(ByVal bEnabled As Boolean)
        frmCalculationZoom.Disabled(Me) = Not bEnabled
        If bEnabled Then
            InvalidateView()
        Else
            If Not mobjCalculationZoom Is Nothing AndAlso Not mobjCalculationZoom.IsClosing Then
                'A form is on display but is not in the process of closing, so close it down.
                mobjCalculationZoom.Close(False)
                mobjCalculationZoom = Nothing
            End If
        End If
    End Sub

    Public Sub CloseCalculationZoom()
        'Same as SetCalculationZoomEnabled but does not disable zoom
        If Not mobjCalculationZoom Is Nothing AndAlso Not mobjCalculationZoom.IsClosing Then
            'A form is on display but is not in the process of closing, so close it down.
            mobjCalculationZoom.Close(True)
            mobjCalculationZoom = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Updates the user preference for a dynamic cursor.
    ''' </summary>
    ''' <param name="value">The value.</param>
    Public Sub SetDynamicCursor(ByVal value As Boolean)
        mbDynamicCursor = value
    End Sub

    ''' <summary>
    ''' Sets a preference of whether or not the user should be prompted to
    ''' provide a comment summarising the changes that they have made when they
    ''' save a process.
    ''' </summary>
    ''' <param name="value"></param>
    Public Sub SetSummariesEnabled(ByVal value As Boolean)
        mbSummariesEnabled = value
    End Sub

    ''' <summary>
    ''' Returns a shortened name for the mode (i.e. Edit instead of EditProcess)
    ''' that can be used when setting frmprocess.text.
    ''' </summary>
    ''' <returns>String containing short name for current mode</returns>
    Public Function GetModeShortName() As String
        Dim sReturn As String = ""
        Select Case mMode
            Case ProcessViewMode.AdHocTestProcess, ProcessViewMode.AdHocTestObject
                sReturn = My.Resources.ctlProcessViewer_AdhocTest
            Case ProcessViewMode.CloneProcess, ProcessViewMode.CloneObject
                sReturn = My.Resources.ctlProcessViewer_Clone
            Case ProcessViewMode.CompareProcesses, ProcessViewMode.CompareObjects
                sReturn = My.Resources.ctlProcessViewer_Compare
            Case ProcessViewMode.DebugProcess, ProcessViewMode.DebugObject
                sReturn = My.Resources.ctlProcessViewer_Debug
            Case ProcessViewMode.EditProcess, ProcessViewMode.EditObject
                sReturn = My.Resources.ctlProcessViewer_Edit
            Case ProcessViewMode.PreviewProcess, ProcessViewMode.PreviewObject
                sReturn = My.Resources.ctlProcessViewer_Preview
        End Select
        Return sReturn
    End Function

#End Region

#Region "Process Interactions"

    ''' <summary>
    ''' Provides a centralised point of access for adding stages to process
    ''' object.
    ''' 
    ''' Performs extra tasks such as setting logging levels for particular modes
    ''' etc.
    ''' </summary>
    ''' <param name="stageType">The type of stage to add.</param>
    ''' <param name="name">The name of the new stage.</param>
    ''' <returns>Returns the newly created stage.</returns>
    Public Function AddStage(stageType As StageTypes, name As String) As clsProcessStage
        Dim s As clsProcessStage = mProcess.AddStage(stageType, name)

        If s.Process.ProcessType = Processes.DiagramType.Process Then
            Select Case s.StageType
                Case StageTypes.Decision,
                     StageTypes.ChoiceStart,
                     StageTypes.ChoiceEnd,
                     StageTypes.Exception,
                     StageTypes.Note,
                     StageTypes.Collection
                    s.LogInhibit = LogInfo.InhibitModes.Never
                Case StageTypes.Recover,
                     StageTypes.Resume
                    s.LogInhibit = LogInfo.InhibitModes.Always
                Case Else
                    s.LogInhibit = LogInfo.InhibitModes.OnSuccess
            End Select
        Else
            s.LogInhibit = LogInfo.InhibitModes.Never
        End If

        If ModeIsObjectStudio() Then
            If Not s.StageType = StageTypes.Exception Then
                s.LogInhibit = LogInfo.InhibitModes.Always
            End If
        End If

        Return s
    End Function

#End Region

    Private Sub AsyncError(ByVal res As StageResult) Handles mProcess.AsyncError

        'Popping up errors makes no sense if this process viewer is not even visible. In
        'particular, Process Studio always has two ctlProcessViewer instances, even when
        'in normal use, so without this all errors would appear twice.
        If Not Visible Then Return

        If InvokeRequired Then
            Invoke(New Action(Of String)(AddressOf UserMessage.Show), res.GetText())
        Else
            UserMessage.Show(res.GetText())
        End If
    End Sub

    Private Sub RunModeChanged(ByVal sMode As ProcessRunState) Handles mProcess.RunStateChanged
        UpdateParentTitle()
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbTabSelector.SelectedIndexChanged
        If cmbTabSelector.SelectedIndex = -1 Then Return

        Dim item As ComboBoxItem =
         TryCast(cmbTabSelector.SelectedItem, ComboBoxItem)
        Debug.Assert(item IsNot Nothing)
        If item Is Nothing Then Return

        For Each it As ComboBoxItem In cmbTabSelector.Items
            If it Is item _
             Then it.Style = FontStyle.Bold _
             Else it.Style = FontStyle.Regular
        Next

        Dim id As Guid = DirectCast(item.Tag, Guid)

        SelectTabByGuid(id)
        InvalidateView()
        pbview.Focus()

    End Sub

    Private Function GetOpenObjectType() As String
        Return If(
            ModeIsObjectStudio(),
            My.Resources.AutomateUI_Controls.ctlProcessViewer_GetOpenObjectType_BusinessObject,
            My.Resources.AutomateUI_Controls.ctlProcessViewer_GetOpenObjectType_Process)
    End Function

    ''' <summary>
    ''' Returns an internationalised string based on the process run state
    ''' If no ProcessRunState is found it returns the current enum as a string
    ''' </summary>
    Private Function TryGetProcessRunStateAsInternationalisedString() As String
        Select Case mProcess.RunState
            Case ProcessRunState.Off
                Return My.Resources.clsProcess_RunState_Off.ToUpper()
            Case ProcessRunState.Paused
                Return My.Resources.clsProcess_RunState_Paused.ToUpper()
            Case ProcessRunState.Failed
                Return My.Resources.clsProcess_RunState_Failed.ToUpper()
            Case ProcessRunState.Stepping
                Return My.Resources.clsProcess_RunState_Stepping.ToUpper()
            Case ProcessRunState.SteppingOver
                Return My.Resources.clsProcess_RunState_SteppingOver.ToUpper()
            Case ProcessRunState.Running
                Return My.Resources.clsProcess_RunState_Running.ToUpper()
            Case ProcessRunState.Completed
                Return My.Resources.clsProcess_RunState_Completed.ToUpper()
            Case ProcessRunState.Aborted
                Return My.Resources.clsProcess_RunState_Aborted.ToUpper()
        End Select
        Return mProcess.RunStateDisplay.ToUpper()
    End Function

End Class
