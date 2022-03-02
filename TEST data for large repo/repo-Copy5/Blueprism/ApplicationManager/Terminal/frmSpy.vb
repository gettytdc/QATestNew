Imports System.Drawing
Imports System.Windows.Forms
Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports System.Collections.Generic

''' <summary>
''' Used to spy on an application. As it loads, the form takes a snapshot
''' of the desktop and displays the image in a picture box. The form is 
''' superimposed on top of the target application so that the form appears
''' transparent. The user is then able to draw rectangles over positions of 
''' interest on the image of target appliaction.
''' </summary>
''' <remarks></remarks>q
Public Class frmSpy
    Inherits Windows.Forms.Form

#Region " Windows Form Designer generated code "


    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)

        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
            If Not Me.mMouseHook Is Nothing Then
                Me.mMouseHook.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)

    End Sub
    Private components As System.ComponentModel.IContainer

    'Required by the Windows Form Designer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents pbScreenShot As System.Windows.Forms.PictureBox
    Friend WithEvents ttToolTip As System.Windows.Forms.ToolTip
    Friend WithEvents lblGridCoords As System.Windows.Forms.Label
    Friend WithEvents ctxContextMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuSelectField As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuRemove As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuRemoveAll As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents pnlScreenShot As System.Windows.Forms.Panel
    Friend WithEvents lnkClose As System.Windows.Forms.LinkLabel
    Friend WithEvents tmrTimer As System.Windows.Forms.Timer
    Friend WithEvents lblMove As System.Windows.Forms.Label
    Friend WithEvents mnuSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuRightToLeft As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents lblResize As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Protected Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSpy))
        Me.pbScreenShot = New System.Windows.Forms.PictureBox()
        Me.ttToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.lblGridCoords = New System.Windows.Forms.Label()
        Me.ctxContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuSelectField = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRemove = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRemoveAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRightToLeft = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.pnlScreenShot = New System.Windows.Forms.Panel()
        Me.tmrTimer = New System.Windows.Forms.Timer(Me.components)
        Me.lnkClose = New System.Windows.Forms.LinkLabel()
        Me.lblResize = New System.Windows.Forms.Label()
        Me.lblMove = New System.Windows.Forms.Label()
        CType(Me.pbScreenShot, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctxContextMenu.SuspendLayout()
        Me.pnlScreenShot.SuspendLayout()
        Me.SuspendLayout()
        '
        'pbScreenShot
        '
        resources.ApplyResources(Me.pbScreenShot, "pbScreenShot")
        Me.pbScreenShot.BackColor = System.Drawing.Color.White
        Me.pbScreenShot.Name = "pbScreenShot"
        Me.pbScreenShot.TabStop = False
        Me.ttToolTip.SetToolTip(Me.pbScreenShot, resources.GetString("pbScreenShot.ToolTip"))
        '
        'lblGridCoords
        '
        resources.ApplyResources(Me.lblGridCoords, "lblGridCoords")
        Me.lblGridCoords.ForeColor = System.Drawing.Color.Black
        Me.lblGridCoords.Name = "lblGridCoords"
        Me.ttToolTip.SetToolTip(Me.lblGridCoords, resources.GetString("lblGridCoords.ToolTip"))
        '
        'ctxContextMenu
        '
        resources.ApplyResources(Me.ctxContextMenu, "ctxContextMenu")
        Me.ctxContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuSeparator, Me.mnuSelectField, Me.mnuRemove, Me.mnuRemoveAll, Me.mnuRightToLeft, Me.mnuSeparator1, Me.mnuExit})
        Me.ctxContextMenu.Name = "ContextMenu1"
        Me.ttToolTip.SetToolTip(Me.ctxContextMenu, resources.GetString("ctxContextMenu.ToolTip"))
        '
        'mnuSeparator
        '
        resources.ApplyResources(Me.mnuSeparator, "mnuSeparator")
        Me.mnuSeparator.Name = "mnuSeparator"
        '
        'mnuSelectField
        '
        resources.ApplyResources(Me.mnuSelectField, "mnuSelectField")
        Me.mnuSelectField.Name = "mnuSelectField"
        '
        'mnuRemove
        '
        resources.ApplyResources(Me.mnuRemove, "mnuRemove")
        Me.mnuRemove.Name = "mnuRemove"
        '
        'mnuRemoveAll
        '
        resources.ApplyResources(Me.mnuRemoveAll, "mnuRemoveAll")
        Me.mnuRemoveAll.Name = "mnuRemoveAll"
        '
        'mnuRightToLeft
        '
        resources.ApplyResources(Me.mnuRightToLeft, "mnuRightToLeft")
        Me.mnuRightToLeft.CheckOnClick = True
        Me.mnuRightToLeft.Name = "mnuRightToLeft"
        '
        'mnuSeparator1
        '
        resources.ApplyResources(Me.mnuSeparator1, "mnuSeparator1")
        Me.mnuSeparator1.Name = "mnuSeparator1"
        '
        'mnuExit
        '
        resources.ApplyResources(Me.mnuExit, "mnuExit")
        Me.mnuExit.Name = "mnuExit"
        '
        'pnlScreenShot
        '
        resources.ApplyResources(Me.pnlScreenShot, "pnlScreenShot")
        Me.pnlScreenShot.Controls.Add(Me.pbScreenShot)
        Me.pnlScreenShot.Name = "pnlScreenShot"
        Me.ttToolTip.SetToolTip(Me.pnlScreenShot, resources.GetString("pnlScreenShot.ToolTip"))
        '
        'tmrTimer
        '
        Me.tmrTimer.Enabled = True
        Me.tmrTimer.Interval = 200
        '
        'lnkClose
        '
        resources.ApplyResources(Me.lnkClose, "lnkClose")
        Me.lnkClose.Name = "lnkClose"
        Me.lnkClose.TabStop = True
        Me.ttToolTip.SetToolTip(Me.lnkClose, resources.GetString("lnkClose.ToolTip"))
        '
        'lblResize
        '
        resources.ApplyResources(Me.lblResize, "lblResize")
        Me.lblResize.BackColor = System.Drawing.Color.White
        Me.lblResize.Name = "lblResize"
        Me.ttToolTip.SetToolTip(Me.lblResize, resources.GetString("lblResize.ToolTip"))
        '
        'lblMove
        '
        resources.ApplyResources(Me.lblMove, "lblMove")
        Me.lblMove.BackColor = System.Drawing.Color.White
        Me.lblMove.Name = "lblMove"
        Me.ttToolTip.SetToolTip(Me.lblMove, resources.GetString("lblMove.ToolTip"))
        '
        'frmSpy
        '
        resources.ApplyResources(Me, "$this")
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Controls.Add(Me.lblMove)
        Me.Controls.Add(Me.lblResize)
        Me.Controls.Add(Me.lnkClose)
        Me.Controls.Add(Me.pnlScreenShot)
        Me.Controls.Add(Me.lblGridCoords)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "frmSpy"
        Me.ttToolTip.SetToolTip(Me, resources.GetString("$this.ToolTip"))
        CType(Me.pbScreenShot, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctxContextMenu.ResumeLayout(False)
        Me.pnlScreenShot.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region "Members"

    ''' <summary>
    ''' The number of grid rows.
    ''' </summary>
    ''' <remarks></remarks>
    Private mNumGridRows As Integer

    ''' <summary>
    ''' The number of grid columns.
    ''' </summary>
    ''' <remarks></remarks>
    Private mNumGridColumns As Integer

    ''' <summary>
    ''' The column width.
    ''' </summary>
    ''' <remarks></remarks>
    Private mdGridColumnWidth As Double

    ''' <summary>
    ''' The row height.
    ''' </summary>
    ''' <remarks></remarks>
    Private mdGridRowHeight As Double

    ''' <summary>
    ''' Field colour
    ''' </summary>
    ''' <remarks></remarks>
    Private mFieldHighlightColour As Color = Color.Blue

    ''' <summary>
    ''' Focussed field colour
    ''' </summary>
    ''' <remarks></remarks>
    Private mFocussedFieldHighlightColour As Color = Color.Red

    ''' <summary>
    ''' Rubber band colour
    ''' </summary>
    ''' <remarks></remarks>
    Private mRubberBandColour As Color = Color.LimeGreen

    ''' <summary>
    ''' Field pen width
    ''' </summary>
    ''' <remarks></remarks>
    Private miPenWidth As Integer = 2

    ''' <summary>
    ''' Field pen
    ''' </summary>
    ''' <remarks></remarks>
    Private mFieldPen As Pen

    ''' <summary>
    ''' Focussed field pen
    ''' </summary>
    ''' <remarks></remarks>
    Private mFocusPen As Pen

    ''' <summary>
    ''' Rubber band pen
    ''' </summary>
    ''' <remarks></remarks>
    Private mRubberBandPen As Pen

    ''' <summary>
    ''' The field currently in focus
    ''' </summary>
    ''' <remarks></remarks>
    Private mFocusField As clsTerminalField

    ''' <summary>
    ''' The current rubber band
    ''' </summary>
    ''' <remarks></remarks>
    Private mRubberBand As clsTerminalField

    ''' <summary>
    ''' The mouse down point, in grid coordinates.
    ''' </summary>
    ''' <remarks>Used for creating fields
    ''' using a rubber band.</remarks>
    Private mGridMouseDownLocation As Point

    ''' <summary>
    ''' The world coordinates of the view.
    ''' </summary>
    ''' <remarks>The view is ...?</remarks>
    Private mViewOrigin As Point

    ''' <summary>
    ''' The current fields.
    ''' </summary>
    ''' <remarks></remarks>
    Public mFields As List(Of clsTerminalField)


    ''' <summary>
    ''' The grid colour.
    ''' </summary>
    ''' <remarks></remarks>
    Private mGridColour As Color = Color.DarkGray

    ''' <summary>
    ''' An additional context menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private mnuAddGridRow As ToolStripMenuItem

    ''' <summary>
    ''' An additional context menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private mnuAddGridColumn As ToolStripMenuItem

    ''' <summary>
    ''' An additional context menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private mnuRemoveGridRow As ToolStripMenuItem

    ''' <summary>
    ''' An additional context menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private mnuRemoveGridColumn As ToolStripMenuItem

    ''' <summary>
    ''' Tooltip shown to user in spy operations
    ''' </summary>
    ''' <remarks></remarks>
    Private mTooltip As frmToolTip

    ''' <summary>
    ''' Determines whether the user has chosen
    ''' a custom size for the grid and thus whether
    ''' the grid's size should be automatically
    ''' updated.
    ''' </summary>
    Private mbCustomSizeSelected As Boolean

    ''' <summary>
    ''' Determines whether the user has chosen a
    ''' custom location relative to the target
    ''' terminal window and thus whether the 
    ''' grid's location should be automatically
    ''' updated.
    ''' </summary>
    Private mbCustomLocationSelected As Boolean

    ''' <summary>
    ''' If the user has chosen a custom location
    ''' for the grid, relative to the target
    ''' terminal window, the offset is stored
    ''' here.
    ''' </summary>
    ''' <remarks>Relevant only when the member
    ''' mbCustomLocationSelected is true.
    ''' </remarks>
    Private mCustomLocationOffset As Size

    ''' <summary>
    ''' A desktop mouse hook helping to perform
    ''' resizing of this form.
    ''' </summary>
    ''' <remarks>This may be a sledgehammer
    ''' to crack a peanut, but I can't 
    ''' seem to achieve it using features of 
    ''' System.Windows.Forms.Form.
    ''' Apologies PJW 26/02/07</remarks>
    Private mMouseHook As clsGlobalMouseHook

    ''' <summary>
    ''' A desktop keyboard hook helping to perform
    ''' resizing of this form.
    ''' </summary>
    Private mKeyHook As clsGlobalKeyboardHook

    ''' <summary>
    ''' Used to store the state of the control key
    ''' </summary>
    ''' <remarks></remarks>
    Private Ctrl As Boolean

    ''' <summary>
    ''' Used to store the state of the shift key
    ''' </summary>
    ''' <remarks></remarks>
    Private Shift As Boolean

    ''' <summary>
    ''' Used to store the state of the alt key
    ''' </summary>
    ''' <remarks></remarks>
    Private Alt As Boolean

    ''' <summary>
    ''' The types of movement the mouse can makee.
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum MouseMovementType
        ''' <summary>
        ''' Ordinary, insignificant mouse movements.
        ''' </summary>
        [Default]
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to move the spy grid.
        ''' </summary>
        Move
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the top.
        ''' </summary>
        SizeN
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the top right.
        ''' </summary>
        SizeNE
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the right.
        ''' </summary>
        SizeE
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the bottom right.
        ''' </summary>
        SizeSE
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the bottom.
        ''' </summary>
        SizeS
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the bottom left.
        ''' </summary>
        SizeSW
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the left.
        ''' </summary>
        SizeW
        ''' <summary>
        ''' The user's mouse actions correspond to an 
        ''' attempt to resize the spy grid at the top left.
        ''' </summary>
        SizeNW
    End Enum

    ''' <summary>
    ''' The different types of intention communicable via
    ''' key presses.
    ''' </summary>
    Private Enum KeypressConfiguration
        ''' <summary>
        ''' Default, when no keys are pressed.
        ''' </summary>
        RectangularSelect
        ''' <summary>
        ''' Indicates that a key combo is held choosing
        ''' a multiline wrapped selection.
        ''' </summary>
        MultilineSelect
        ''' <summary>
        ''' Indicates that a key combo is held choosing
        ''' a single line rectangular selection. 
        ''' </summary>
        SingleRowSelect
    End Enum

    ''' <summary>
    ''' The event raised when a field is selected.
    ''' </summary>
    ''' <param name="f">The field that is selected.</param>
    Public Event FieldSelected(ByVal f As clsTerminalField)

    ''' <summary>
    ''' Event raised when the spy operation is cancelled.
    ''' </summary>
    Public Event SpyCancelled()

    ''' <summary>
    ''' Event raised when the target terminal window
    ''' cannot be found.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event TerminalWindowMissing()

    ''' <summary>
    ''' The handle of the terminal window over which this grid
    ''' sits.
    ''' </summary>
    ''' <remarks></remarks>
    Private mTargetWindowHandle As IntPtr

#End Region

    Public Property LocationOffset() As Size
        Get
            Return Me.mCustomLocationOffset
        End Get
        Set(ByVal value As Size)
            mCustomLocationOffset = value
            Me.mbCustomLocationSelected = True
        End Set
    End Property


    Public Property Customsize() As Boolean
        Get
            Return Me.mbCustomSizeSelected
        End Get
        Set(ByVal value As Boolean)
            Me.mbCustomSizeSelected = value
        End Set
    End Property


#Region "New"

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

        MyBase.New()
        InitializeComponent()

        If Not DesignMode Then
            Initialise(100, 100, 500, 400)
        End If

        'Ensure shared value is represented by checkbox
        mnuRightToLeft.Checked = mRightToLeft

    End Sub

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="Loc">The location of the screenshot, in screen coords.</param>
    ''' <param name="Siz">The size of the screenshot.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal TargetWindowHandle As IntPtr, ByVal Loc As Point, ByVal Siz As Size)
        MyBase.New()
        InitializeComponent()

        Me.ShowInTaskbar = False
        Me.StartPosition = FormStartPosition.Manual
        mTargetWindowHandle = TargetWindowHandle

        If Not DesignMode Then
            Initialise(Loc.X, Loc.Y, Siz.Width, Siz.Height)
        End If
    End Sub

    Public Sub New(ByVal TargetWindowHandle As IntPtr, ByVal Loc As Point, ByVal Siz As Size, ByVal c As Integer, ByVal r As Integer)
        Me.New(TargetWindowHandle, Loc, Siz)
        Initialise(c, r)
    End Sub


#End Region


#Region "SetSize"


    ''' <summary>
    ''' Sets the location and size of the spy grid area.
    ''' </summary>
    ''' <param name="x">The left hand edge in screen coordinates
    ''' of the spy grid area.</param>
    ''' <param name="y">The top edge in screen coordinates
    ''' of the spy grid area.</param>
    ''' <param name="w">The width of the spy grid area.</param>
    ''' <param name="h">The height of the spy grid area.</param>
    Public Sub SetSize(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer)

        Dim iLeftMargin, iRightMargin, iTopMargin, iBottomMargin As Integer
        UpdateScreenShot()

        iLeftMargin = pnlScreenShot.Left
        iRightMargin = Me.Width - pnlScreenShot.Left - pnlScreenShot.Width
        iTopMargin = pnlScreenShot.Top
        iBottomMargin = Me.Height - pnlScreenShot.Top - pnlScreenShot.Height
        Width = iLeftMargin + w + iRightMargin
        Height = iTopMargin + h + iBottomMargin

        Location = New Point(x - iLeftMargin, y - iRightMargin)
    End Sub

    ''' <summary>
    ''' Sets the size of the spy grid area.
    ''' </summary>
    ''' <param name="Location">The location of the grid in screen
    ''' coordinates.</param>
    ''' <param name="Size">The size of the grid in pixels.</param>
    Public Sub SetSize(ByVal Location As Point, ByVal Size As Size)
        Me.SetSize(Location.X, Location.Y, Size.Width, Size.Height)
    End Sub

#End Region

    ''' <summary>
    ''' Updates the screenshot displayed in the picture box.
    ''' </summary>
    Public Sub UpdateScreenShot()
        Dim PreviouslyVisible As Boolean = Me.Visible
        Try
            Me.Visible = False
            Dim imgScreenShot As Image = (New clsScreenShot()).GetScreenShot(mTargetWindowHandle, Me.mCustomLocationOffset)
            pbScreenShot.BackgroundImage = imgScreenShot
            pbScreenShot.Width = imgScreenShot.Width
            pbScreenShot.Height = imgScreenShot.Height
        Catch
            'do nothing
        Finally
            Me.Visible = PreviouslyVisible
        End Try
    End Sub



#Region "Initialise"

    Private Sub Initialise(ByVal x As Integer, ByVal y As Integer, ByVal w As Integer, ByVal h As Integer)

        SetSize(x, y, w, h)
        'Initialise the field collection
        ClearFields()

        ttToolTip.Active = True
        ttToolTip.AutomaticDelay = 500
        ttToolTip.AutoPopDelay = 1000
        ttToolTip.InitialDelay = 500
        ttToolTip.ShowAlways = True

        'Set up the pens
        mFieldPen = New Pen(mFieldHighlightColour, miPenWidth)
        mFocusPen = New Pen(mFocussedFieldHighlightColour, miPenWidth)
        mRubberBandPen = New Pen(mRubberBandColour, miPenWidth)
        mRubberBandPen.DashStyle = Drawing2D.DashStyle.Dash

        'Add event handlers
        If Not DesignMode Then
            AddHandler pbScreenShot.Paint, AddressOf Me.pbScreenShot_Paint
            AddHandler pbScreenShot.MouseUp, AddressOf Me.pbScreenShot_MouseUp
            AddHandler pbScreenShot.MouseDown, AddressOf Me.pbScreenShot_MouseDown
            AddHandler pbScreenShot.MouseMove, AddressOf Me.pbScreenShot_MouseMove
        End If

    End Sub

    ''' <summary>
    ''' Initialisation
    ''' </summary>
    ''' <param name="c">Columns</param>
    ''' <param name="r">Rows</param>
    ''' <remarks></remarks>
    Private Sub Initialise(ByVal c As Integer, ByVal r As Integer)

        Me.BackColor = mGridColour

        'Work out the column and row sizes.
        mNumGridColumns = c
        mNumGridRows = r
        If mNumGridColumns > 0 And mNumGridRows > 0 Then
            mdGridColumnWidth = pnlScreenShot.Width / mNumGridColumns
            mdGridRowHeight = pnlScreenShot.Height / mNumGridRows
        Else
            mdGridColumnWidth = 1
            mdGridRowHeight = 1
        End If

        'Add the new menu items
        mnuAddGridRow = New ToolStripMenuItem(My.Resources.AddRow, Nothing, AddressOf mnuAddGridRow_Click)
        mnuRemoveGridRow = New ToolStripMenuItem(My.Resources.RemoveRow, Nothing, AddressOf mnuRemoveGridRow_Click)
        mnuAddGridColumn = New ToolStripMenuItem(My.Resources.AddColumn, Nothing, AddressOf mnuAddGridColumn_Click)
        mnuRemoveGridColumn = New ToolStripMenuItem(My.Resources.RemoveColumn, Nothing, AddressOf mnuRemoveGridColumn_Click)

        mnuAddGridRow.Enabled = False
        mnuRemoveGridRow.Enabled = False
        mnuAddGridColumn.Enabled = False
        mnuRemoveGridColumn.Enabled = False

        ctxContextMenu.Items.Clear()
        ctxContextMenu.Items.Add(mnuAddGridRow)
        ctxContextMenu.Items.Add(mnuRemoveGridRow)
        ctxContextMenu.Items.Add(mnuAddGridColumn)
        ctxContextMenu.Items.Add(mnuRemoveGridColumn)
        ctxContextMenu.Items.Add(mnuSeparator)
        ctxContextMenu.Items.Add(mnuSelectField)
        ctxContextMenu.Items.Add(mnuRemove)
        ctxContextMenu.Items.Add(mnuRemoveAll)
        ctxContextMenu.Items.Add(mnuRightToLeft)
        ctxContextMenu.Items.Add(mnuSeparator1)
        ctxContextMenu.Items.Add(mnuExit)

    End Sub

#End Region

    Private Sub Debug(ByVal Message As String)
        System.Diagnostics.Debug.WriteLine(Message)
    End Sub

#Region "frmSpy_Load"

    Private Sub frmSpy_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Me.Visible = False
        e.Cancel = True
    End Sub

    ''' <summary>
    ''' Positions the picture box at 0,0 in world units
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmSpy_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If Not DesignMode Then
            pbScreenShot.Left -= Me.Location.X + pnlScreenShot.Left
            pbScreenShot.Top -= Me.Location.Y + pnlScreenShot.Top
        End If

    End Sub

#End Region

#Region "frmSpy_Move"

    ''' <summary>
    ''' Positions the picture box at 0,0 in world units and initiates a redraw.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmSpy_Move(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Move

        Dim pOrigin As Point = Me.PointToScreen(pnlScreenShot.Location)

        pbScreenShot.Left = -pOrigin.X
        pbScreenShot.Top = -pOrigin.Y
        mViewOrigin = New Point(-pbScreenShot.Left, -pbScreenShot.Top)

    End Sub

#End Region

#Region "Resizing and Moving"


    ''' <summary>
    ''' The type of sizing underway, if any
    ''' </summary>
    ''' <remarks></remarks>
    Private mSizingStyle As frmSpy.MouseMovementType
    ''' <summary>
    ''' The point in screen coords, at which sizing began.
    ''' </summary>
    Private mMouseSizingDownLocation As Point
    ''' <summary>
    ''' The bounds of the screenshot panel
    ''' before sizing/moving began.
    ''' </summary>
    ''' <remarks></remarks>
    Private mBeforeSizingBounds As Rectangle
    ''' <summary>
    ''' The bounds of the screenshot panel during and after
    ''' a sizing/moving operation.
    ''' </summary>
    ''' <remarks></remarks>
    Private mAfterSizingBounds As Rectangle
    ''' <summary>
    ''' The number of pixels away from the edge of the form
    ''' at which sizing begins.
    ''' </summary>
    Private Const SizingTolerance As Integer = 8
    ''' <summary>
    ''' Last drawn reversible frame during resize
    ''' </summary>
    ''' <remarks></remarks>
    Private mLastDrawnFrame As Rectangle
    ''' <summary>
    ''' The location of the mouse when the moving operation
    ''' began.
    ''' </summary>
    Private mMouseMovingDownLocation As Point


    Private Sub MyBase_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseDown, lblMove.MouseDown, lblResize.MouseDown

        Dim ScreenLocation As Point = CType(sender, Control).PointToScreen(e.Location)
        mSizingStyle = Me.GetMouseConfig(ScreenLocation)
        Me.SetCursor(mSizingStyle)
        If mSizingStyle <> MouseMovementType.Default Then
            Select Case mSizingStyle
                Case MouseMovementType.Move
                    Me.mMouseMovingDownLocation = Cursor.Position
                Case Else
                    Me.mMouseSizingDownLocation = Cursor.Position
            End Select

            Me.mBeforeSizingBounds = New Rectangle(Me.PointToScreen(Me.pnlScreenShot.Location), Me.pnlScreenShot.Size)
            Me.mAfterSizingBounds = Me.mBeforeSizingBounds
            Me.Visible = False
        End If

    End Sub

    Private Sub mMouseHook_MouseDown(ByVal e As ApplicationManagerUtilities.clsGlobalMouseEventArgs)
        If e.Button = MouseButtons.Right AndAlso Ctrl Then
            EndSpy()
            RaiseEvent SpyCancelled()
        End If
    End Sub

    Private Sub mMouseHook_MouseMoved(ByVal p As Point)

        If Me.IsSizing Then
            If Me.mMouseSizingDownLocation <> Point.Empty Then
                'We are already sizing - update proposed size
                Dim Offset As Size = New Size(Cursor.Position.X - Me.mMouseSizingDownLocation.X, Cursor.Position.Y - Me.mMouseSizingDownLocation.Y)
                Select Case Me.mSizingStyle
                    Case MouseMovementType.SizeN
                        Me.mAfterSizingBounds.Y = Me.mBeforeSizingBounds.Top + Offset.Height
                        Me.mAfterSizingBounds.Height = Me.mBeforeSizingBounds.Height - Offset.Height
                    Case MouseMovementType.SizeNE
                        Me.mAfterSizingBounds.Y = Me.mBeforeSizingBounds.Top + Offset.Height
                        Me.mAfterSizingBounds.Height = Me.mBeforeSizingBounds.Height - Offset.Height
                        Me.mAfterSizingBounds.Width = Me.mBeforeSizingBounds.Width + Offset.Width
                    Case MouseMovementType.SizeE
                        Me.mAfterSizingBounds.Width = Me.mBeforeSizingBounds.Width + Offset.Width
                    Case MouseMovementType.SizeSE
                        Me.mAfterSizingBounds.Width = Me.mBeforeSizingBounds.Width + Offset.Width
                        Me.mAfterSizingBounds.Height = Me.mBeforeSizingBounds.Height + Offset.Height
                    Case MouseMovementType.SizeS
                        Me.mAfterSizingBounds.Height = Me.mBeforeSizingBounds.Height + Offset.Height
                    Case MouseMovementType.SizeSW
                        Me.mAfterSizingBounds.Height = Me.mBeforeSizingBounds.Height + Offset.Height
                        Me.mAfterSizingBounds.X = Me.mBeforeSizingBounds.Left + Offset.Width
                        Me.mAfterSizingBounds.Width = Me.mBeforeSizingBounds.Width - Offset.Width
                    Case MouseMovementType.SizeW
                        Me.mAfterSizingBounds.X = Me.mBeforeSizingBounds.Left + Offset.Width
                        Me.mAfterSizingBounds.Width = Me.mBeforeSizingBounds.Width - Offset.Width
                    Case MouseMovementType.SizeNW
                        Me.mAfterSizingBounds.Y = Me.mBeforeSizingBounds.Top + Offset.Height
                        Me.mAfterSizingBounds.Height = Me.mBeforeSizingBounds.Height - Offset.Height
                        Me.mAfterSizingBounds.X = Me.mBeforeSizingBounds.Left + Offset.Width
                        Me.mAfterSizingBounds.Width = Me.mBeforeSizingBounds.Width - Offset.Width
                End Select

                Me.mbCustomSizeSelected = True
                Me.DrawScreenShotBoundsRectangle()
            End If
        ElseIf Me.IsMoving Then
            Me.mbCustomLocationSelected = True
            Dim Offset As Size = New Size(Cursor.Position.X - Me.mMouseMovingDownLocation.X, Cursor.Position.Y - Me.mMouseMovingDownLocation.Y)
            Me.mAfterSizingBounds.Location = Point.Add(Me.mBeforeSizingBounds.Location, Offset)
            Me.DrawScreenShotBoundsRectangle()
        End If
    End Sub

    Private Sub Mybase_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseMove, lblResize.MouseMove, lblMove.MouseMove
        If (Not IsSizing) AndAlso (Not IsMoving) Then
            mSizingStyle = Me.GetMouseConfig(CType(sender, Control).PointToScreen(e.Location))
            Me.SetCursor(mSizingStyle)
        End If
    End Sub

    Private Sub MyBase_MouseLeave(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.MouseLeave
        mSizingStyle = MouseMovementType.Default
        Me.SetCursor(mSizingStyle)
    End Sub

    Private ReadOnly Property IsMoving() As Boolean
        Get
            Return (Not Me.mMouseMovingDownLocation.IsEmpty) AndAlso mSizingStyle = MouseMovementType.Move
        End Get
    End Property

    Private ReadOnly Property IsSizing() As Boolean
        Get
            Return Not Me.mMouseSizingDownLocation.IsEmpty
        End Get
    End Property

    ''' <summary>
    ''' Determines what type of interaction is relevant
    ''' given the current mouse position, eg resize,
    ''' eg move, eg none.
    ''' </summary>
    ''' <param name="MouseLocation">The location of the mouse, in
    ''' screen coordinates.</param>
    ''' <returns>Returns the mouse movement type.</returns>
    Private Function GetMouseConfig(ByVal MouseLocation As Point) As MouseMovementType
        'Not yet sizing or moving - see if user is hovering mouse on size point
        Dim OuterRect As Rectangle = Me.Bounds
        Dim InnerRect As Rectangle = OuterRect
        InnerRect.Inflate(-SizingTolerance, -SizingTolerance)

        If OuterRect.Contains(MouseLocation) AndAlso (Not InnerRect.Contains(MouseLocation)) Then
            Dim HorizontalCentre As Integer = Me.Left + (Me.Width \ 2)
            Dim VerticalCentre As Integer = Me.Top + (Me.Height \ 2)
            Dim AtHorizontalCentre As Boolean = Math.Abs(MouseLocation.X - HorizontalCentre) < SizingTolerance
            Dim AtVerticalCentre As Boolean = Math.Abs(MouseLocation.Y - VerticalCentre) < SizingTolerance

            Dim ResizeRect As Rectangle = New Rectangle(Me.PointToScreen(lblResize.Location), Me.lblResize.Size)
            Dim MoveRect As Rectangle = (New Rectangle(Me.PointToScreen(lblMove.Location), Me.lblMove.Size))

            Dim InLeftStrip As Boolean = MouseLocation.X < Me.Left + SizingTolerance
            Dim InRightStrip As Boolean = MouseLocation.X > Me.Right - SizingTolerance
            Dim InTopStrip As Boolean = MouseLocation.Y < Me.Top + SizingTolerance
            Dim InBottomStrip As Boolean = MouseLocation.Y > Me.Bottom - SizingTolerance

            If MoveRect.Contains(MouseLocation) Then
                Return MouseMovementType.Move
            ElseIf ResizeRect.Contains(MouseLocation) Then
                Return MouseMovementType.SizeSE
            ElseIf InTopStrip Then
                If InLeftStrip Then
                    Return MouseMovementType.SizeNW
                ElseIf InRightStrip Then
                    Return MouseMovementType.SizeNE
                Else
                    Return MouseMovementType.SizeN
                End If
            ElseIf InBottomStrip Then
                If InLeftStrip Then
                    Return MouseMovementType.SizeSW
                ElseIf InRightStrip Then
                    Return MouseMovementType.SizeSE
                Else
                    Return MouseMovementType.SizeS
                End If
            ElseIf InLeftStrip Then
                Return MouseMovementType.SizeW
            ElseIf InRightStrip Then
                Return MouseMovementType.SizeE
            Else
                Return MouseMovementType.Default
            End If
        End If

        Return MouseMovementType.Default
    End Function

    Private Sub SetCursor(ByVal Movement As MouseMovementType)
        Select Case Movement
            Case MouseMovementType.SizeE, MouseMovementType.SizeW
                Me.Cursor = Cursors.SizeWE
            Case MouseMovementType.SizeN, MouseMovementType.SizeS
                Me.Cursor = Cursors.SizeNS
            Case MouseMovementType.SizeNE, MouseMovementType.SizeSW
                Me.Cursor = Cursors.SizeNESW
            Case MouseMovementType.SizeSE, MouseMovementType.SizeNW
                Me.Cursor = Cursors.SizeNWSE
            Case MouseMovementType.Move
                Me.Cursor = Cursors.SizeAll
            Case Else
                Me.Cursor = Cursors.Default
        End Select
    End Sub

    Private Sub mMouseHook_MouseUp(ByVal e As clsGlobalMouseEventArgs)
        Select Case e.Button

            Case Windows.Forms.MouseButtons.Left
                'Window sizing/moving

                Dim UpdateLocation As Boolean
                If Me.IsMoving Then
                    'we have been moving 
                    e.Cancel = True
                    If Me.mAfterSizingBounds.Location <> Me.mBeforeSizingBounds.Location Then
                        UpdateLocation = True
                    End If
                End If

                Dim UpdateSize As Boolean
                If Me.IsSizing Then
                    'we have been resizing
                    e.Cancel = True
                    If Me.mAfterSizingBounds.Location <> Me.mBeforeSizingBounds.Location Then
                        UpdateLocation = True
                    End If
                    UpdateSize = Me.mAfterSizingBounds.Size <> Me.mBeforeSizingBounds.Size
                    If UpdateSize Then
                        Me.mbCustomSizeSelected = True
                    End If
                End If

                If UpdateLocation Then
                    Me.mbCustomLocationSelected = True
                    Dim Offset As Size = New Size(Me.mAfterSizingBounds.X - Me.mBeforeSizingBounds.X, Me.mAfterSizingBounds.Y - Me.mBeforeSizingBounds.Y)
                    Me.mCustomLocationOffset += Offset
                End If

                If UpdateSize OrElse UpdateLocation Then
                    Me.SetSize(Me.mAfterSizingBounds.Location, Me.mAfterSizingBounds.Size)
                End If

                Me.mSizingStyle = MouseMovementType.Default
                Me.mMouseSizingDownLocation = Point.Empty
                Me.mBeforeSizingBounds = Rectangle.Empty
                Me.mMouseMovingDownLocation = Point.Empty
                Me.Cursor = Cursors.Default

                If Me.mLastDrawnFrame <> Rectangle.Empty Then
                    DrawRectangle(Rectangle.Empty)
                    Me.mLastDrawnFrame = Rectangle.Empty
                End If

                If e.Cancel Then
                    Me.UpdateScreenShot()
                    Me.Visible = True
                End If
        End Select
    End Sub

    ''' <summary>
    ''' Draws the specified rectangle in screen
    ''' coords, erasing any previously drawn rectangle.
    ''' </summary>
    ''' <param name="RectToDraw">The rectangle to be drawn.
    ''' </param>
    ''' <remarks>The rectangle is stored in the member
    ''' mLastDrawnRectangle, for reference so that it
    ''' can be erased at the next invocation.</remarks>
    Private Sub DrawRectangle(ByVal RectToDraw As Rectangle)
        If Not Me.mLastDrawnFrame = Rectangle.Empty Then
            ControlPaint.DrawReversibleFrame(mLastDrawnFrame, Color.Black, FrameStyle.Thick)
        End If
        mLastDrawnFrame = RectToDraw
        If Not Me.mLastDrawnFrame = Rectangle.Empty Then
            ControlPaint.DrawReversibleFrame(mLastDrawnFrame, Color.Black, FrameStyle.Thick)
        End If
    End Sub

    ''' <summary>
    ''' Draws the outline of the proposed screenshot
    ''' bounds.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DrawScreenShotBoundsRectangle()
        Me.DrawRectangle(mAfterSizingBounds)
    End Sub

#End Region

#Region "DrawGrid"

    ''' <summary>
    ''' Draws the grid of rows and columns.
    ''' </summary>
    ''' <param name="graphics"></param>
    ''' <remarks></remarks>
    Private Sub DrawGrid(ByRef graphics As Graphics)

        Dim sngX, sngY As Single
        Dim oPen As Pen

        oPen = New Pen(mGridColour)

        sngX = -pbScreenShot.Location.X
        sngY = -pbScreenShot.Location.Y
        For i As Integer = 0 To mNumGridColumns + 1
            graphics.DrawLine(oPen, sngX, sngY, sngX, CSng(sngY + pnlScreenShot.Height))
            sngX = CSng(sngX + mdGridColumnWidth)
        Next

        sngX = -pbScreenShot.Location.X
        sngY = -pbScreenShot.Location.Y
        For i As Integer = 0 To mNumGridRows + 1
            graphics.DrawLine(oPen, sngX, sngY, CSng(sngX + pnlScreenShot.Width), sngY)
            sngY = CSng(sngY + mdGridRowHeight)
        Next


    End Sub

#End Region

#Region "pbScreenShot_Paint"

    ''' <summary>
    ''' Draws any fields and the rubber band.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub pbScreenShot_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs)
        DrawGrid(e.Graphics)
        DrawFields(e.Graphics)
        DrawRubberBand(e.Graphics)
    End Sub

#End Region

#Region "pbScreenShot_MouseMove"

    ''' <summary>
    ''' Highlights the field the mouse is in by drawing it in a 
    ''' different colour and displaying a tooltip.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks> If the mouse 
    ''' is dragging, a rubber band is drawn. The coordinates are
    ''' also tracked and displayed at the bottom of the form.</remarks>
    Private Sub pbScreenShot_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        Dim fieldToSelect As clsTerminalField = Nothing

        Me.Cursor = Cursors.Default

        Dim gridPoint = Me.GetSnappedMousePoint(e)
        For Each f As clsTerminalField In mFields
            If f.ContainsGridPoint(gridPoint) Then
                'The mouse is in a field rectangle, we choose,
                'the most recently added field, of all the fields'
                'which match
                fieldToSelect = f
            End If
            f.Highlighted = False
        Next

        'Unhighlight the old field, if such exists
        If Not mFocusField Is Nothing Then
            mFocusField.Highlighted = False
        End If

        'Highlight new field, if appropriate
        If Spying Then
            If Not fieldToSelect Is Nothing Then
                fieldToSelect.Highlighted = True
            End If
        End If

        mFocusField = fieldToSelect
        SetRubberBand(mGridMouseDownLocation, GetSnappedMousePoint(e.X, e.Y))
        DisplayCoords(e.X, e.Y)
        Me.UpdateGrid()
    End Sub

#End Region

    Private Sub UpdateGrid()
        Me.pbScreenShot.Invalidate()
        Me.pbScreenShot.Refresh()
    End Sub

#Region "pbScreenShot_MouseUp"

    ''' <summary>
    ''' Creates a new field if a rubber band has been made.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub pbScreenShot_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If Spying Then
            SetRubberBand(mGridMouseDownLocation, GetSnappedMousePoint(e.X, e.Y))
            AddField(mRubberBand)
        End If

        mGridMouseDownLocation = Point.Empty
        mRubberBand = Nothing

        Me.UpdateGrid()

    End Sub

#End Region

#Region "pbScreenShot_MouseDown"

    ''' <summary>
    ''' Starts a rubber band, selects the field clicked,
    ''' or shows the context menu.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub pbScreenShot_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If Me.Spying Then
            'Select a field if one exists and appropriate keys and mouse combo used
            If (Not mFocusField Is Nothing) AndAlso (Not mFocusField.IsEmpty) Then
                If Ctrl Then
                    If e.Button = MouseButtons.Left Then
                        EndSpy()
                        OnFieldSelected()
                        Exit Sub
                    End If
                End If
            End If


            Select Case e.Button
                Case Windows.Forms.MouseButtons.Left
                    mGridMouseDownLocation = GetSnappedMousePoint(e)
                    mRubberBand = CreateField()
                Case MouseButtons.Right
                    ShowContextMenu(e.X, e.Y)
            End Select
        End If

    End Sub

    ''' <summary>
    ''' Raises the FeildSelected event adjusting the
    ''' feild for right to left mode if needed.
    ''' </summary>
    Private Sub OnFieldSelected()
        Dim field = mFocusField
        If mRightToLeft Then
            Dim cols = (mFocusField.SessionSize.Columns + 1)
            Dim startCol = cols - mFocusField.EndColumn
            Dim endCol = cols - mFocusField.StartColumn

            field = New clsTerminalField(field.SessionSize, field.FieldType, field.StartRow, startCol, field.EndRow, endCol)
        End If
        RaiseEvent FieldSelected(field)
    End Sub

#End Region

#Region "GetField"

    ''' <summary>
    ''' Returns the type of field used in this form.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CreateField() As clsTerminalField
        Return New clsTerminalField(New SessionDimensions(mNumGridRows, mNumGridColumns))
    End Function

#End Region

#Region "GetSnapX"

    ''' <summary>
    ''' Gets the nearest grid x coord.
    ''' </summary>
    ''' <param name="x">The true x coord</param>
    ''' <returns></returns>
    ''' <remarks>The grid coords are integer coords,
    ''' indexed from 1</remarks>
    Private Function GetSnapX(ByVal x As Integer) As Integer
        Try
            Return 1 + CInt(Math.Round(x / mdGridColumnWidth))
        Catch
            Return 0
        End Try

    End Function

#End Region

#Region "GetSnapY"

    ''' <summary>
    ''' Gets the nearest grid y coord.
    ''' </summary>
    ''' <param name="y">The true y coord</param>
    ''' <returns></returns>
    ''' <remarks>The grid coords are integer coords,
    ''' indexed from 1</remarks>
    Private Function GetSnapY(ByVal y As Integer) As Integer
        Try
            Return 1 + CInt(Math.Round(y / mdGridRowHeight))
        Catch
            Return 0
        End Try
    End Function

#End Region

#Region "GetMousePoint"

    ''' <summary>
    ''' Returns the mouse location, in this case the nearest snap point.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetSnappedMousePoint(ByVal e As System.Windows.Forms.MouseEventArgs) As Point
        Return New Point(GetSnapX(e.X), GetSnapY(e.Y))
    End Function

    ''' <summary>
    ''' Gets the corresponding grid coordinate from the supplied
    ''' screen coordinates.
    ''' </summary>
    ''' <param name="x">The x mouse coordinate.</param>
    ''' <param name="y">The x mouse coordinate.</param>
    ''' <returns></returns>
    ''' <remarks>The grid coordinates are integer coords,
    ''' indexed from 1.</remarks>
    Private Function GetSnappedMousePoint(ByVal x As Integer, ByVal y As Integer) As Point
        Return New Point(GetSnapX(x), GetSnapY(y))
    End Function

#End Region


#Region "SetRubberBand"


    ''' <summary>
    ''' Interprets the current keyboard state as a type
    ''' of selection from the user.
    ''' </summary>
    ''' <returns>Returns the type of selection
    ''' to be made.</returns>
    Private Function GetKeyConfig() As KeypressConfiguration
        If Shift Then
            Return KeypressConfiguration.MultilineSelect
        ElseIf Alt Then
            Return KeypressConfiguration.SingleRowSelect
        Else
            Return KeypressConfiguration.RectangularSelect
        End If
    End Function

    ''' <summary>
    ''' Creates the rubber band using two points.
    ''' </summary>
    ''' <param name="pFrom">The start point, in grid coordinates</param>
    ''' <param name="pTo">The end point, in grid coordinates</param>
    ''' <remarks></remarks>
    Private Sub SetRubberBand(ByVal pFrom As Point, ByVal pTo As Point)

        If mRubberBand Is Nothing Then
            'Mouse button has not been pressed
        Else
            Dim leftMost As Integer = Math.Min(pFrom.X, pTo.X)
            Dim rightMost As Integer = Math.Max(pFrom.X, pTo.X)
            Dim topMost As Integer = Math.Min(pFrom.Y, pTo.Y)
            Dim bottomMost As Integer = Math.Max(pFrom.Y, pTo.Y)

            mRubberBand.Clear()

            Select Case Me.GetKeyConfig
                Case KeypressConfiguration.SingleRowSelect
                    mRubberBand.FieldType = clsTerminalField.FieldTypes.Rectangular
                    bottomMost = Math.Min(bottomMost, topMost + 1)
                Case KeypressConfiguration.RectangularSelect
                    mRubberBand.FieldType = clsTerminalField.FieldTypes.Rectangular
                Case KeypressConfiguration.MultilineSelect
                    mRubberBand.FieldType = clsTerminalField.FieldTypes.MultilineWrapped
            End Select

            'Brings fields back into boundary of acceptable grid size
            mRubberBand.StartColumn = Math.Max(leftMost, 1)
            mRubberBand.StartRow = Math.Max(topMost, 1)

            'Adjustment of -1 for conversion from grid points to cell references
            mRubberBand.EndColumn = Math.Min(rightMost - 1, mNumGridColumns)
            mRubberBand.EndRow = Math.Min(bottomMost - 1, mNumGridRows)

            If Not mRubberBand.IsEmpty Then
                UpdateGrid()
            End If
        End If

    End Sub

#End Region


#Region "ShowContextMenu"

    ''' <summary>
    ''' Displays a context menu. Some items will only be
    ''' enabled when the mouse is in a field.
    ''' </summary>
    ''' <param name="x">The x coord</param>
    ''' <param name="y">The y coord</param>
    ''' <remarks></remarks>
    Private Sub ShowContextMenu(ByVal x As Integer, ByVal y As Integer)

        'Decide whether to enabled addition/removal of rows/columns
        Me.mnuAddGridColumn.Enabled = True
        Me.mnuAddGridRow.Enabled = True
        Me.mnuRemoveGridColumn.Enabled = (mNumGridColumns > 0)
        Me.mnuRemoveGridRow.Enabled = (Me.mNumGridRows > 0)


        'decide whether to enable removal of fields
        mnuRemove.Enabled = False
        mnuRemoveAll.Enabled = False
        mnuSelectField.Enabled = False
        If Me.Spying Then
            For Each oField As clsTerminalField In mFields
                mnuRemoveAll.Enabled = True
                If oField.ContainsGridPoint(GetSnappedMousePoint(x, y)) Then
                    mnuSelectField.Enabled = True
                    mnuRemove.Enabled = True
                    Exit For
                End If
            Next
        End If

        ctxContextMenu.Show(pbScreenShot, New Point(x, y))
    End Sub

#End Region


#Region "mnuAddGridRow_Click"

    ''' <summary>
    ''' Adds a rows to the grid.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuAddGridRow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mNumGridRows += 1
        ResizeFields()
    End Sub

#End Region

#Region "mnuRemoveGridRow_Click"

    ''' <summary>
    ''' Removes a row from the grid.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuRemoveGridRow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        mNumGridRows -= 1
        ResizeFields()

    End Sub

#End Region

#Region "mnuAddGridColumn_Click"

    ''' <summary>
    ''' Adds a column to the grid.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuAddGridColumn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        mNumGridColumns += 1
        ResizeFields()

    End Sub

#End Region

#Region "mnuRemoveGridColumn_Click"

    ''' <summary>
    ''' Removes a column to the grid.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuRemoveGridColumn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        mNumGridColumns -= 1
        ResizeFields()

    End Sub

#End Region

    Private Sub mnuRightToLeft_CheckChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRightToLeft.CheckedChanged
        mRightToLeft = mnuRightToLeft.Checked
    End Sub

#Region "DrawFields"

    ''' <summary>
    ''' Calls the Draw method of each field object.
    ''' </summary>
    ''' <param name="graphics"></param>
    ''' <remarks></remarks>
    Private Sub DrawFields(ByRef graphics As Graphics)

        Dim p As Pen = Nothing
        For Each oField As clsTerminalField In mFields
            If oField.Highlighted Then
                p = mFocusPen
            Else
                p = mFieldPen
            End If
            oField.Draw(graphics, p, Me.mdGridColumnWidth, Me.mdGridRowHeight)
        Next

    End Sub

#End Region

#Region "DrawRubberBand"

    ''' <summary>
    ''' Calls the Draw method of the rubber band object.
    ''' </summary>
    ''' <param name="graphics"></param>
    ''' <remarks></remarks>
    Private Sub DrawRubberBand(ByRef graphics As Graphics)
        If Not mRubberBand Is Nothing Then
            mRubberBand.Draw(graphics, mRubberBandPen, Me.mdGridColumnWidth, Me.mdGridRowHeight)
        End If
    End Sub

#End Region

    ''' <summary>
    ''' Adds a field to the collection.
    ''' </summary>
    ''' <param name="field">The field object</param>
    Public Sub AddField(field As clsTerminalField)
        If field Is Nothing OrElse field.IsEmpty Then Return

        If Not mFields.Contains(field) Then
            mFields.Add(field)
        End If
    End Sub


    ''' <summary>
    ''' Clears the current collection of fields.
    ''' </summary>
    Public Sub ClearFields()
        mFields = New List(Of clsTerminalField)
    End Sub

#Region "RemoveField"

    ''' <summary>
    ''' Removes a field from the collection.
    ''' </summary>
    ''' <param name="oField"></param>
    ''' <remarks></remarks>
    Public Sub RemoveField(ByRef oField As clsTerminalField)

        If mFields.Contains(oField) Then
            mFields.Remove(oField)
            oField = Nothing

            UpdateGrid()
            Me.UpdateGrid()

        End If

    End Sub

#End Region


#Region "DisplayCoords"

    ''' <summary>
    ''' Displays the mouse position.
    ''' </summary>
    ''' <param name="x">The x coord</param>
    ''' <param name="y">The y coord</param>
    ''' <remarks></remarks>
    Private Sub DisplayCoords(ByVal x As Integer, ByVal y As Integer)
        Try
            Dim iX, iY, iR, iC, iP As Integer

            iX = x - mViewOrigin.X
            iY = y - mViewOrigin.Y
            iC = CInt(Math.Floor(iX / mdGridColumnWidth)) + 1
            iR = CInt(Math.Floor(iY / mdGridRowHeight)) + 1
            iP = CInt((iR - 1) * mNumGridColumns) + iC - 1

            lblGridCoords.Text = String.Format(My.Resources.R0C1, CStr(iR).PadLeft(2, "0"c), CStr(iC))
        Catch
        End Try
    End Sub

#End Region


#Region "mnuSelectField_Click"

    ''' <summary>
    ''' Raises a field selection event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuSelectField_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSelectField.Click

        If Not mFocusField Is Nothing Then
            EndSpy()
            OnFieldSelected()
        End If

    End Sub

#End Region

#Region "mnuRemoveAll_Click"

    ''' <summary>
    ''' Removes all fields and redraws the form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuRemoveAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemoveAll.Click

        ClearFields()
        Me.UpdateGrid()
    End Sub

#End Region

#Region "mnuRemove_Click"

    ''' <summary>
    ''' Removes the field in focus.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRemove.Click

        If Not mFocusField Is Nothing Then
            RemoveField(mFocusField)
        End If

    End Sub

#End Region

#Region "mnuExit_Click"

    ''' <summary>
    ''' Closes the form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    Handles mnuExit.Click
        EndSpy()
        Me.Close()
    End Sub

#End Region

#Region "frmTerminalSpy_Resize"

    ''' <summary>
    ''' Resizes any fields to fit the new grid.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmTerminalSpy_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If Not DesignMode Then
            ResizeFields()
            Me.UpdateScreenShot()
        End If
    End Sub

#End Region

#Region "frmTerminalSpy_Move"

    ''' <summary>
    ''' Resizes any fields to fit the new grid.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub frmTerminalSpy_Move(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If Not DesignMode Then
            ResizeFields()
            Me.UpdateScreenShot()
        End If
    End Sub

#End Region

    ''' <summary>
    ''' Indicates whether the grid column values should be reversed for right to left
    ''' layouts (e.g. Arabic)
    ''' </summary>
    Private Shared mRightToLeft As Boolean = False

#Region "Timer Stuff"
    Private mLastRect As RECT
    ''' <summary>
    ''' Handle to the top level form, containing the terminal
    ''' window.
    ''' </summary>
    Private mTerminalParentFormHandle As IntPtr



    Private Sub tmrTimer_Tick(sender As Object, ByVal e As EventArgs) Handles tmrTimer.Tick
        Dim R As RECT
        Dim RetVal As Integer = modWin32.GetWindowRect(mTargetWindowHandle, R)

        'We assume it failed because terminal window no longer exists
        If RetVal = 0 Then
            If Me.Spying Then
                EndSpy()
                RaiseEvent SpyCancelled()
            End If
            RaiseEvent TerminalWindowMissing()
        End If

        PositionGrid(False, R)
        mLastRect = R
    End Sub

    ''' <summary>
    ''' Positions the grid according to the supplied bounds.
    ''' </summary>
    ''' <param name="OverrideUserPref">Determines whether to
    ''' apply the bounds irrespective of whether the user has
    ''' adjusted the bounds manually. If set to false, then the
    ''' bounds will not be updated if the user has set their own.
    ''' </param>
    ''' <param name="NewBounds">The new bounds of the grid, in 
    ''' screen coordinates.</param>
    Private Sub PositionGrid(ByVal OverrideUserPref As Boolean, ByVal NewBounds As RECT)

        'We only update when necessary, because updating causes
        'screenshot to be updated, which is an expensive operation
        If ((mLastRect.Left <> NewBounds.Left) OrElse (mLastRect.Right <> NewBounds.Right) OrElse (mLastRect.Top <> NewBounds.Top) OrElse (mLastRect.Bottom <> NewBounds.Bottom)) Then
            Dim location As Point = CType(IIf(Me.mbCustomLocationSelected, Point.Add(NewBounds.Location, Me.mCustomLocationOffset), NewBounds.Location), Point)
            Dim Size As Size = CType(IIf((Not OverrideUserPref) AndAlso Me.mbCustomSizeSelected, pnlScreenShot.Size, NewBounds.Size), Size)
            Me.SetSize(location, Size)
        End If

        'Insert the grid just in front of the terminal form,
        'in the desktop z-order
        If mTerminalParentFormHandle = IntPtr.Zero Then
            mTerminalParentFormHandle = modWin32.GetAncestor(Me.mTargetWindowHandle, modWin32.GA_ROOT)
        End If
        If mTerminalParentFormHandle <> IntPtr.Zero Then
            'Get the handle of the window above in the z-order
            Dim NextWindow As IntPtr = modWin32.GetWindow(mTerminalParentFormHandle, modWin32.GW_HWNDPREV)
            If NextWindow <> IntPtr.Zero Then
                'inserts the spy grid window just after that window, so that
                'it is in front of the terminal window
                SetWindowPos(Me.Handle, NextWindow, 0, 0, 0, 0, modWin32.SWP.SWP_NOACTIVATE Or modWin32.SWP.SWP_NOSIZE Or SWP.SWP_NOMOVE)
            Else
                'Terminal window is already at top, so just
                'have to put grid to top of z-order
                SetWindowPos(Me.Handle, New IntPtr(modWin32.SWP.HWND_TOP), 0, 0, 0, 0, modWin32.SWP.SWP_NOACTIVATE Or modWin32.SWP.SWP_NOSIZE Or modWin32.SWP.SWP_NOMOVE)
            End If
        End If
    End Sub

#End Region

#Region "DesignMode"

    ''' <summary>
    ''' Shadowed property to fix what looks liek a VS2005 problem.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shadows ReadOnly Property DesignMode() As Boolean
        Get
            If AppDomain.CurrentDomain.DomainManager IsNot Nothing Then
                Return AppDomain.CurrentDomain.DomainManager.ToString.
                 Contains("Microsoft.VisualStudio.CommonIDE")
            Else
                Return False
            End If
        End Get
    End Property

#End Region


#Region "ResizeFields"

    ''' <summary>
    ''' Resizes each field to the new grid dimensions.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ResizeFields()
        mdGridColumnWidth = pnlScreenShot.Width / mNumGridColumns
        mdGridRowHeight = pnlScreenShot.Height / mNumGridRows
        Me.UpdateGrid()
    End Sub

#End Region

    ''' <summary>
    ''' Starts the spying operation installing global mouse and keyboard hooks.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub StartSpy()
        Spying = True
        ShowHideToolTip()
        Me.mMouseHook = New clsGlobalMouseHook
        Me.mKeyHook = New clsGlobalKeyboardHook

        AddHandler mMouseHook.MouseUp, AddressOf mMouseHook_MouseUp
        AddHandler mMouseHook.MouseDown, AddressOf mMouseHook_MouseDown
        AddHandler mMouseHook.MouseMoved, AddressOf mMouseHook_MouseMoved
        AddHandler mKeyHook.KeyUp, AddressOf mKeyHook_KeyUp
        AddHandler mKeyHook.KeyDown, AddressOf mKeyHook_KeyDown
    End Sub

    ''' <summary>
    ''' Indicates whether a spy operation is underway.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Spying As Boolean

    ''' <summary>
    ''' Shows or hides the tooltip depending on mbSpying
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ShowHideToolTip()
        'Show/hide the tooltip
        If Spying Then
            If Me.mTooltip Is Nothing Then mTooltip = New frmToolTip
            Me.mTooltip.ChangeTooltipBackColor(Color.LightSeaGreen)
            Me.mTooltip.SetToolTip(My.Resources.UsingTheIdentificationTool, My.Resources.YouMayDrawANewFieldByDraggingTheMouseOverARegionByHoldingOneOfShiftOrAltYouMayM)
            mTooltip.ToolTipActive = True
            mTooltip.PositionTooltip()
        Else
            If (Not Me.mTooltip Is Nothing) Then
                Me.mTooltip.ToolTipActive = False
            End If
        End If
    End Sub

    ''' <summary>
    ''' Ends the spying operation removing global mouse and keyboard hooks.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub EndSpy()
        Ctrl = False
        Shift = False
        Alt = False
        Spying = False
        ShowHideToolTip()

        RemoveHandler mMouseHook.MouseUp, AddressOf mMouseHook_MouseUp
        RemoveHandler mMouseHook.MouseDown, AddressOf mMouseHook_MouseDown
        RemoveHandler mMouseHook.MouseMoved, AddressOf mMouseHook_MouseMoved
        RemoveHandler mKeyHook.KeyUp, AddressOf mKeyHook_KeyUp
        RemoveHandler mKeyHook.KeyDown, AddressOf mKeyHook_KeyDown

        mMouseHook.Dispose()
        mKeyHook.Dispose()
    End Sub


    Private Sub lnkClose_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lnkClose.LinkClicked
        If Me.Spying Then
            EndSpy()
            RaiseEvent SpyCancelled()
        End If
        Me.Close()
    End Sub

    Private Sub lnkClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lnkClose.Click
        Me.lnkClose_LinkClicked(sender, Nothing)
    End Sub

    Private Sub pbScreenShot_SizeChanged(ByVal sender As Object, ByVal e As EventArgs) Handles pbScreenShot.SizeChanged
        RemoveHandler pbScreenShot.SizeChanged, AddressOf pbScreenShot_SizeChanged
        Me.UpdateScreenShot()
        Me.ResizeFields()
        AddHandler pbScreenShot.SizeChanged, AddressOf pbScreenShot_SizeChanged
    End Sub

    Private Sub mKeyHook_KeyDown(ByVal sender As Object, ByVal g As GlobalKeyEventArgs)
        Select Case g.Key
            Case Keys.LControlKey, Keys.RControlKey : Ctrl = True
            Case Keys.LShiftKey, Keys.RShiftKey, Keys.Shift : Shift = True
            Case Keys.LMenu, Keys.RMenu, Keys.Menu : Alt = True
        End Select
    End Sub

    Private Sub mKeyHook_KeyUp(ByVal sender As Object, ByVal g As GlobalKeyEventArgs)
        If Shift Then
            'Move
            Select Case g.Key
                Case Keys.Up : Top -= 1
                Case Keys.Down : Top += 1
                Case Keys.Left : Left -= 1
                Case Keys.Right : Left += 1
            End Select
        ElseIf Ctrl Then
            'Resize
            Select Case g.Key
                Case Keys.Up : Height -= 1
                Case Keys.Down : Height += 1
                Case Keys.Left : Width -= 1
                Case Keys.Right : Width += 1
            End Select
        End If

        Select Case g.Key
            Case Keys.LControlKey, Keys.RControlKey : Ctrl = False
            Case Keys.LShiftKey, Keys.RShiftKey, Keys.Shift : Shift = False
            Case Keys.LMenu, Keys.RMenu, Keys.Menu : Alt = False
        End Select
    End Sub
End Class
