Option Strict On

Imports System.IO
Imports System.Resources
Imports System.Drawing.Imaging

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

Imports BluePrism.Images
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Skills
Imports LocaleTools
Imports AutomateControls

''' Project  : Automate
''' Class    : clsRenderer
''' 
''' <summary>
''' A rendering utility class.
''' </summary>
Friend Class clsRenderer
    Implements IDisposable

#Region " The clsShape class "

    ''' Project  : Automate
    ''' Class    : clsRenderer.clsShape
    ''' 
    ''' <summary>
    ''' A class to hold the different colors for each shape
    ''' </summary>
    Private Class clsShape : Implements IDisposable

#Region " Member Variables "

        ' The root shape name for this object.
        Private mShapeName As String

        ' The images - these are lazily loaded as required by their respective properties

        ' The "standard" view of this shape
        Private mStandard As Image

        ' The view of this shape if it's the current step (ie. when debugging)
        Private mCurrent As Image

        ' The 'red' view of this shape - to indicate a stage has been deleted
        Private mRed As Image

        ' The 'green' view of this shape - to indicate a stage has been created
        Private mGreen As Image

        ' The 'yellow' view of this shape - to indicate a stage has changed
        Private mYellow As Image

        ' The 'bright yellow' view of this shape - to focus on a searched for stage
        Private mBrightYellow As Image

        'Flag to indicate if this object has already been disposed of.
        Private mDisposed As Boolean

#End Region

        ''' <summary>
        ''' Gets the standard representation of this shape, or null if no such
        ''' representation is found in the process-render resource file.
        ''' </summary>
        Public ReadOnly Property Standard() As Image
            Get
                If mStandard Is Nothing Then mStandard = LoadDirect(mShapeName)
                Return mStandard
            End Get
        End Property

        ''' <summary>
        ''' Gets the 'current step' representation of this shape. If no such
        ''' representation is found in the process-render resource file, this will just
        ''' default to using the <see cref="Standard"/> representation.
        ''' </summary>
        Public ReadOnly Property Current() As Image
            Get
                'If we haven't loaded it yet...
                If mCurrent Is Nothing Then
                    ' attempt to load it.
                    mCurrent = LoadDirect(mShapeName & "_cur")
                    ' If there isn't one, just use 'Standard' as a stand-in image.
                    If mCurrent Is Nothing Then mCurrent = Me.Standard
                End If
                Return mCurrent
            End Get
        End Property

        ''' <summary>
        ''' Gets the red representation of this shape, or null if no such
        ''' representation is found in the process-render resource file.
        ''' </summary>
        Public ReadOnly Property Red() As Image
            Get
                If mRed Is Nothing Then mRed = LoadDirect(mShapeName & "_red")
                Return mRed
            End Get
        End Property

        ''' <summary>
        ''' Gets the green representation of this shape, or null if no such
        ''' representation is found in the process-render resource file.
        ''' </summary>
        Public ReadOnly Property Green() As Image
            Get
                If mGreen Is Nothing Then mGreen = LoadDirect(mShapeName & "_green")
                Return mGreen
            End Get
        End Property

        ''' <summary>
        ''' Gets the yellow representation of this shape, or null if no such
        ''' representation is found in the process-render resource file.
        ''' </summary>
        Public ReadOnly Property Yellow() As Image
            Get
                If mYellow Is Nothing Then mYellow = LoadDirect(mShapeName & "_yellow")
                Return mYellow
            End Get
        End Property

        ''' <summary>
        ''' Gets the bright yellow representation of this shape, or null if no such
        ''' representation is found in the process-render resource file.
        ''' </summary>
        Public ReadOnly Property BrightYellow() As Image
            Get
                If mBrightYellow Is Nothing Then mBrightYellow = LoadDirect(mShapeName & "_brightyellow")
                Return mBrightYellow
            End Get
        End Property

        ''' <summary>
        ''' Creates a new Shape object representing the given shape name.
        ''' </summary>
        ''' <param name="shapeName">The shape name for the new object.</param>
        Public Sub New(ByVal shapeName As String)
            mShapeName = shapeName
        End Sub

        ''' <summary>
        ''' Loads the specific shape as a metafile rather than a clsShape.
        ''' </summary>
        ''' <param name="shapeName">The shape to load.</param>
        ''' <returns>The metafile representing the required shape</returns>
        Friend Shared Function LoadDirect(ByVal shapeName As String) As Metafile
            Dim rm As ResourceManager = DiagramShapes.ResourceManager

            Dim imgBytes As Byte() = TryCast(rm.GetObject(shapeName), Byte())
            If imgBytes Is Nothing Then Return Nothing
            Using s As Stream = New MemoryStream(imgBytes)
                Return New Metafile(s)
            End Using
        End Function

        ''' <summary>
        ''' Gets the image used to represent this shape for the given show mode.
        ''' </summary>
        ''' <param name="mode">The mode in which this shape should be rendered.</param>
        ''' <returns>The image which represents this shape in the given mode, or null
        ''' if this shape is not supported in the given mode.</returns>
        Public Function GetImage(ByVal mode As StageShowMode) As Image
            Select Case mode
                Case StageShowMode.CurrentDebug
                    Return Current
                Case StageShowMode.Audit_Deleted
                    Return Red
                Case StageShowMode.Audit_Modified
                    Return Yellow
                Case StageShowMode.Audit_New
                    Return Green
                Case StageShowMode.Search_Highlight
                    Return BrightYellow
                Case Else
                    Return Standard
            End Select
        End Function

        ''' <summary>
        ''' Disposes of the given disposable object if it is not null.
        ''' </summary>
        ''' <param name="disp">The object to be disposed of.</param>
        Private Sub DisposeIfPresent(ByVal disp As IDisposable)
            If disp IsNot Nothing Then disp.Dispose()
        End Sub

        ''' <summary>
        ''' Convenience method for disposing all the shapes at once, note that this is 
        ''' only called dispose for naming convention, this class does not implement
        ''' the disposeable pattern.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Not mDisposed Then
                mDisposed = True
                DisposeIfPresent(mStandard)
                DisposeIfPresent(mCurrent)
                DisposeIfPresent(mRed)
                DisposeIfPresent(mGreen)
                DisposeIfPresent(mYellow)
                DisposeIfPresent(mBrightYellow)
            End If
        End Sub
    End Class

#End Region

#Region "Members"
    'provide a small margin around the text inside the Note UI stage object
    Private Const NoteMargin As Integer = 5

    'This is True when an exception message has already been shown
    'during display generation. This is to stop them being displayed
    'repeatedly if there is a problem.
    Private mbShownDisplayException As Boolean = False

    ''' <summary>
    ''' Get the shape object which deals with the given stage type.
    ''' </summary>
    ''' <param name="type">The type of stage for which the shape is required.
    ''' </param>
    ''' <returns>The shape representing the given stage type.</returns>
    Private Function GetShape(ByVal type As StageTypes) As clsShape
        ' Try and get the shape from the dictionary.
        Dim shp As clsShape = Nothing
        If mShapes.TryGetValue(type, shp) Then Return shp
        ' Return the default
        Return mShapes(StageTypes.Undefined)
    End Function

    'icons used
    Private mPublishedIcon As Image
    Private mConditionalBreakpointIcon As Image
    Private mSingleRowCollectionIcon As Image

    ' The shapes which are used to render the stages
    Private ReadOnly mShapes As IDictionary(Of StageTypes, clsShape)
    Private mBreakpointImage As Image

    'Tooltips
    Private mbToolTipIn As Boolean
    Private mbToolTipActive As Boolean
    Private msToolTipText As String
    Private mpToolTipPoint As Point 'Rectangle
    Private mtsTooltipDurations As TimeSpan = TimeSpan.FromSeconds(4)
    Private mdtTimeToolTipWasFirstDisplayed As DateTime

    'our master:
    Private mobjparent As ctlProcessViewer
    Private mimode As ProcessViewMode

    'Globals used to draw a loop iteration tooltip ("Iteration x of y") in UpdateView()

    ''' <summary>
    ''' Object keeping a record of what is where so that clients
    ''' can query such information. This helps when discovering toolty
    ''' for particular screen location etc.
    ''' </summary>
    Private mObjectMap As clsPixmap

    'variables for determining the tooltip on a loopstart stage
    Dim gStageAfterLoopStart, mgStageAfterLoopEnd As Guid
    Dim bDrawLoopIterationLabel As Boolean
    Dim objLoopCollectionStage As clsCollectionStage

    ''' <summary>
    ''' The pen used to draw stages, etc
    ''' </summary>
    ''' <remarks></remarks>
    Private mStagePen As New Pen(Color.FromArgb(127, 127, 127), 1)


    ''' <summary>
    ''' The font used during drawing
    ''' </summary>
    ''' <remarks></remarks>
    Private mFont As New Font("Segoe UI", 10, GraphicsUnit.Pixel)
#End Region

#Region "Gets and Sets"

    ''' <summary>
    ''' The time for which a tooltip should be visible after it is first displayed.
    ''' </summary>
    ''' <value>The timespan.</value>
    Public ReadOnly Property ToolTipDurations() As TimeSpan
        Get
            Return mtsTooltipDurations
        End Get
    End Property

    ''' <summary>
    ''' The time at which the tooltip was last shown.
    ''' </summary>
    Public ReadOnly Property TimeLastToolTipWasShown() As DateTime
        Get
            Return Me.mdtTimeToolTipWasFirstDisplayed
        End Get
    End Property

    ''' <summary>
    ''' Sets the tooltip text.
    ''' </summary>
    ''' <param name="s">The text</param>
    Public Sub SetToolTipText(ByVal s As String)
        msToolTipText = s
    End Sub

    ''' <summary>
    ''' Sets the tooltip position.
    ''' </summary>
    ''' <param name="p">The position</param>
    Public Sub SetToolTipPoint(ByVal p As Point)
        mpToolTipPoint = p
    End Sub

    ''' <summary>
    ''' Makes the tooltip active.
    ''' </summary>
    ''' <param name="Active">The activation flag</param>
    Public Sub SetToolTipActive(ByVal Active As Boolean)
        mbToolTipActive = Active
    End Sub

    ''' <summary>
    ''' Sets the tooltip 'in'.
    ''' </summary>
    ''' <param name="value">The 'in' flag</param>
    Public Sub SetToolTipIn(ByVal value As Boolean)
        mbToolTipIn = value
    End Sub

    ''' <summary>
    ''' Gets the tooltip 'in' setting.
    ''' </summary>
    ''' <returns>The tooltip 'in' setting</returns>
    Public Function IsToolTipIn() As Boolean
        Return mbToolTipIn
    End Function

    ''' <summary>
    ''' Gets the tooltip 'active' setting.
    ''' </summary>
    ''' <returns>The tooltip 'active' setting</returns>
    Public Function IsToolTipActive() As Boolean
        Return mbToolTipActive
    End Function


    ''' <summary>
    ''' Private member to store public property AlphaTransparency()
    ''' </summary>
    Private mAlphaTransparency As Integer = 255
    ''' <summary>
    ''' A value between 0..255 indicating the level
    ''' of transparency desired in the rendering of
    ''' stages. 255 indicates no transparency; 0 indicates
    ''' full transparency.
    ''' </summary>
    ''' <value>This setting applies only to stages, links,
    ''' text, etc.</value>
    Public WriteOnly Property AlphaTransparency() As Byte
        Set(ByVal value As Byte)
            mAlphaTransparency = Math.Max(Math.Min(255, value), 0)
            mStagePen.Color = Color.FromArgb(mAlphaTransparency, mStagePen.Color)
            mLinkPen.Color = Color.FromArgb(mAlphaTransparency, mLinkPen.Color)
            mGridPen.Color = Color.FromArgb(mAlphaTransparency, Color.LightGray)
            mSelectedItemBrush.Color = Color.FromArgb(Me.mAlphaTransparency, mSelectedItemBrush.Color)
        End Set
    End Property

    ''' <summary>
    ''' Pen used for drawing the grid.
    ''' </summary>
    Dim mGridPen As New Pen(Color.FromArgb(mAlphaTransparency, Color.LightGray))

    ''' <summary>
    ''' A basic black pen.
    ''' </summary>
    Dim mBlackPen As New Pen(Color.Black)

    ''' <summary>
    ''' Brushes for process MI
    ''' </summary>
    Dim mBoxBrush As New Drawing.SolidBrush(Color.Black)
    Dim mTextBrush As New Drawing.SolidBrush(Color.White)
    Dim mShadowBrush As New Drawing.SolidBrush(Color.Gray)
    Private ReadOnly mGDICache As New GDICache

    Private mLinkPen As New Pen(Color.FromArgb(Me.mAlphaTransparency, Color.DimGray), 1)
    Private ReadOnly Property LinkPen() As Pen
        Get
            Return mLinkPen
        End Get
    End Property

    Private mSelectedItemBrush As New SolidBrush(Color.Maroon)
    Private ReadOnly Property SelectedItemBrush() As Brush
        Get
            Return mSelectedItemBrush
        End Get
    End Property

    Private mBackgroundBrush As New SolidBrush(Color.White)
    ''' <summary>
    ''' The brush used for the basic background, on which the
    ''' gridlines and stages are drawn. Not to be confused with
    ''' StageBrush, used for drawing the background of a stage.
    ''' </summary>
    Private ReadOnly Property BackgroundBrush() As SolidBrush
        Get
            Return mBackgroundBrush
        End Get
    End Property

#End Region

#Region " Constructors "


    ''' <summary>
    ''' Creates a renderer for the given process viewer.
    ''' </summary>
    ''' <param name="viewer">The process viewer to render</param>
    Public Sub New(ByVal viewer As ctlProcessViewer)
        Me.New(viewer, viewer.ViewMode)
    End Sub

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="parent">The process viewer</param>
    ''' <param name="mode">The viewer mode</param>
    Private Sub New(ByVal parent As ctlProcessViewer, ByVal mode As ProcessViewMode)

        mobjparent = parent
        mimode = mode

        'Load the graphics for the boxes etc...
        Try
            mShapes = BuildShapes()

            ' The above won't actually load the images until they are required,
            ' The below will indicate some kind of problem with loading the graphics
            ' (though that should be less common now that they are part of this
            ' assembly rather than loaded from disk)
            mBreakpointImage = clsShape.LoadDirect("shp_break")

            mPublishedIcon = ToolImages.Tick_16x16
            mConditionalBreakpointIcon = ToolImages.Function_16x16
            mSingleRowCollectionIcon = MediaImages.mm_Record_16x16_Disabled

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.FailedToLoadGraphics0, ex.Message), ex)

        End Try

    End Sub
#End Region

#Region "Graphics"

    ''' <summary>
    ''' Builds the shape objects up for the stage types.
    ''' </summary>
    ''' <returns>A dictionary mapping shape objects onto stage types.
    ''' Note that some stage types will be represented by the same shapes.
    ''' Only one instance of each shape will be created by this method.
    ''' </returns>
    Private Function BuildShapes() As IDictionary(Of StageTypes, clsShape)
        Dim map As New Dictionary(Of StageTypes, clsShape)

        ' The "default" shape to use if there's no other...
        map(StageTypes.Undefined) = New clsShape("shp_rect")
        ' And the rest of them...
        map(StageTypes.Action) = map(StageTypes.Undefined) ' depends on Undefined
        map(StageTypes.SubSheetInfo) = map(StageTypes.Undefined) ' depends on Undefined
        map(StageTypes.ProcessInfo) = map(StageTypes.Undefined) ' depends on Undefined
        map(StageTypes.Process) = New clsShape("shp_extproc")
        map(StageTypes.SubSheet) = New clsShape("shp_subsheet")
        map(StageTypes.Note) = New clsShape("shp_rectdotted")
        map(StageTypes.Anchor) = New clsShape("shp_anchor")
        map(StageTypes.Decision) = New clsShape("shp_dec")
        map(StageTypes.ChoiceStart) = map(StageTypes.Decision) ' depends on Decision
        map(StageTypes.ChoiceEnd) = map(StageTypes.Decision) ' depends on Decision
        map(StageTypes.Data) = New clsShape("shp_data")
        map(StageTypes.Collection) = New clsShape("shp_collection")
        map(StageTypes.Start) = New clsShape("shp_term")
        map(StageTypes.End) = map(StageTypes.Start) ' depends on Start
        map(StageTypes.LoopStart) = New clsShape("shp_loopstart")
        map(StageTypes.LoopEnd) = New clsShape("shp_loopend")
        map(StageTypes.Calculation) = New clsShape("shp_calc")
        map(StageTypes.WaitStart) = New clsShape("shp_wait")
        map(StageTypes.WaitEnd) = map(StageTypes.WaitStart) ' depends on WaitStart
        map(StageTypes.Code) = New clsShape("shp_code")
        map(StageTypes.Read) = New clsShape("shp_read")
        map(StageTypes.Write) = New clsShape("shp_write")
        map(StageTypes.Navigate) = New clsShape("shp_navigate")
        map(StageTypes.Alert) = New clsShape("shp_alert")
        map(StageTypes.Exception) = New clsShape("shp_exception")
        map(StageTypes.Recover) = New clsShape("shp_recover")
        map(StageTypes.Resume) = New clsShape("shp_resume")
        map(StageTypes.MultipleCalculation) = New clsShape("shp_multicalc")

        Return GetReadOnly.IDictionary(map)

    End Function

#End Region


    ''' <summary>
    ''' Private member to store public property DrawText()
    ''' </summary>
    ''' <remarks>Defaults to true</remarks>
    Private mbDrawText As Boolean = True

    ''' <summary>
    ''' Indicates whether text should be drawn.
    ''' </summary>
    Public Property DrawText() As Boolean
        Get
            Return mbDrawText
        End Get
        Set(ByVal value As Boolean)
            mbDrawText = value
        End Set
    End Property


    ''' <summary>
    ''' Private member to store public property DrawSelections()
    ''' </summary>
    Private mbDrawSelections As Boolean = True
    ''' <summary>
    ''' Indicates whether selection handles should be
    ''' drawn.
    ''' </summary>
    ''' <remarks>Defaults to true</remarks>
    Public Property DrawSelections() As Boolean
        Get
            Return mbDrawSelections
        End Get
        Set(ByVal value As Boolean)
            mbDrawSelections = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property DrawGridLines()
    ''' </summary>
    ''' <remarks>Defaults to true</remarks>
    Private mbDrawGridLines As Boolean = True
    ''' <summary>
    ''' Indicates whether gridlines should be drawn. This
    ''' setting overrides the bShowGridLines of the 
    ''' corresponding <see cref="ctlProcessViewer">ProcessViewer</see>
    ''' object.
    ''' </summary>
    Public Property DrawGridLines() As Boolean
        Get
            Return mbDrawGridLines
        End Get
        Set(ByVal value As Boolean)
            mbDrawGridLines = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property Icons()()
    ''' </summary>
    Private mbDrawIcons As Boolean = True
    ''' <summary>
    ''' Indicates whether icons should be drawn.
    ''' </summary>
    ''' <remarks>Defaults to true</remarks>
    Public Property DrawIcons() As Boolean
        Get
            Return mbDrawIcons
        End Get
        Set(ByVal value As Boolean)
            mbDrawIcons = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property CoordinateTranslator()
    ''' </summary>
    Private mCoordinateTranslator As clsCoordinateTranslator
    ''' <summary>
    ''' The coordinate translator to be used during drawing.
    ''' Usually this just contains the zoom level of the process,
    ''' and the default camera view. However you may wish to 
    ''' draw on a different object using a custom camera
    ''' and zoom location.
    ''' </summary>
    Public Property CoordinateTranslator() As clsCoordinateTranslator
        Get
            Return mCoordinateTranslator
        End Get
        Set(ByVal value As clsCoordinateTranslator)
            mCoordinateTranslator = value
        End Set
    End Property

#Region "Draw Methods"

    ''' <summary>
    ''' Draws a 'rubber band' selection rectangle on the screen.
    ''' </summary>
    ''' <param name="objGraphics">The graphics object</param>
    ''' <param name="ViewableRectangle">The viewable rectangle</param>
    ''' <param name="StartLocation">The start of the rectangle - ie the point
    ''' at which the mouse went down, expressed in world coordinates.</param>
    ''' <param name="EndLocation">The start of the rectangle - ie the point
    ''' at which the mouse went down, expressed in world coordinates.</param>
    Private Sub DrawRubberBand(ByVal objGraphics As Graphics, ByVal ViewableRectangle As Rectangle, ByVal StartLocation As PointF, ByVal EndLocation As PointF)
        Dim WorldRect As RectangleF = New RectangleF(StartLocation, New SizeF(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y))
        Dim ScreenRect As Rectangle = Me.mCoordinateTranslator.TranslateWorldRectToScreenRect(WorldRect, ViewableRectangle.Size)

        'Correct the rectangle if it was drawn from bottom-right to top-left.
        'In this case it has negative width/height
        If ScreenRect.Width < 0 Then
            ScreenRect.X = ScreenRect.Right
            ScreenRect.Width = -ScreenRect.Width
        End If
        If ScreenRect.Height < 0 Then
            ScreenRect.Y = ScreenRect.Bottom
            ScreenRect.Height = -ScreenRect.Height
        End If

        Dim p As Pen = System.Drawing.Pens.Blue
        objGraphics.DrawRectangle(p, ScreenRect)
    End Sub

    ''' <summary>
    ''' Draws a block on the screen. Note. This function is used both for drawing the 
    ''' blocks on the diagram that are part of the process and also for drawing 
    ''' blocks as they are being dragged.
    ''' </summary>
    ''' <param name="objGraphics">The graphics object</param>
    ''' <param name="ViewableRectangle">The viewable rectangle</param>
    ''' <param name="sName">The name of the block, set this to nothing when drawing 
    ''' blocks during dragging.</param>
    Private Sub DrawBlock(ByVal objGraphics As Graphics, ByVal WorldRect As RectangleF, ByVal viewablerectangle As Rectangle, ByVal sName As String, ByVal cColor As Color)
        Dim ScreenRect As Rectangle = Me.mCoordinateTranslator.TranslateWorldRectToScreenRect(WorldRect, viewablerectangle.Size)

        'Correct the rectangle if it was drawn from bottom-right to top-left.
        'In this case it has negative width/height
        If ScreenRect.Width < 0 Then
            ScreenRect.X = ScreenRect.Right
            ScreenRect.Width = -ScreenRect.Width
        End If
        If ScreenRect.Height < 0 Then
            ScreenRect.Y = ScreenRect.Bottom
            ScreenRect.Height = -ScreenRect.Height
        End If

        Dim c As Color = Color.FromArgb(100, cColor)

        Dim b = mGDICache.GetBrush(c)
        objGraphics.FillRectangle(b, ScreenRect)
        objGraphics.DrawRectangle(Pens.Gray, ScreenRect)

        If sName IsNot Nothing Then
            Dim labelSize As SizeF = Me.MeasureScaledString(objGraphics, sName, mFont)
            Dim sngWidth As Single = Math.Min(labelSize.Width, ScreenRect.Width)
            Dim layoutrect As New RectangleF(ScreenRect.X, ScreenRect.Y, sngWidth, labelSize.Height)

            objGraphics.FillRectangle(Brushes.White, layoutrect)
            Me.DrawScaledString(objGraphics, sName, mFont, Brushes.Black, layoutrect)
            objGraphics.DrawRectangle(Pens.Black, ScreenRect.X, ScreenRect.Y, sngWidth, labelSize.Height)
        End If
    End Sub


    ''' <summary>
    ''' Draws a ProcessInfo stage on the screen.
    ''' </summary>
    ''' <param name="g">The graphics object</param>
    ''' <param name="ViewableRectangle">The viewable rectangle</param>
    ''' <param name="objStage">The stage object</param>
    ''' <param name="objProcess">The process object</param>
    Private Sub DrawProcessInfo(ByVal g As Graphics, ByVal ViewableRectangle As Rectangle, ByVal objStage As clsProcessStage, ByVal objProcess As clsProcess)

        Dim ParentProcess As clsProcess = objStage.Process
        Dim bSub As Boolean = False

        Dim sName As String
        If objStage.StageType = StageTypes.SubSheetInfo Then bSub = True
        sName = ParentProcess.Name
        If bSub Then
            sName = String.Format(My.Resources.Process0StageName1, sName, objStage.GetName())
        End If

        Dim sDescription As String = ""
        If ParentProcess.GetActiveSubSheet.Equals(Guid.Empty) Then
            'The main page is on display, so use the process description.
            sDescription = ParentProcess.Description
        Else
            'Another page is on display, so find the corresponding SubSheetInfo and use its narrative.
            Dim aSubSheetInfos As List(Of clsProcessStage) = ParentProcess.GetStages(StageTypes.SubSheetInfo)
            For Each objSubSheetInfo As clsProcessStage In aSubSheetInfos
                If objSubSheetInfo.GetSubSheetID.Equals(ParentProcess.GetActiveSubSheet) Then
                    sDescription = objSubSheetInfo.GetNarrative()
                    Exit For
                End If
            Next
        End If

        Dim font1 As Font = objStage.Font
        Dim sCreated As String = String.Format(My.Resources.CreatedBy0at1, ParentProcess.CreatedBy, ParentProcess.CreatedDate)
        Dim sLastChanged As String = String.Format(My.Resources.LastChangedBy0at1, ParentProcess.ModifiedBy, ParentProcess.ModifiedDate)

        Dim bSelected As Boolean = objProcess.IsStageSelected(objStage.GetStageID())

        Dim ShapeWorldBounds As RectangleF = objStage.GetDisplayBounds
        Dim ShapeScreenBounds As Rectangle = Me.mCoordinateTranslator.TranslateWorldRectToScreenRect(ShapeWorldBounds, ViewableRectangle.Size)

        Dim fOffset As Integer = 4
        Dim fSize As Integer
        Using scaledFont = GetScaledFont(objStage.Font)
            fSize = CInt(scaledFont.Size) + fOffset
        End Using
        Dim rRect0 As New Rectangle(ShapeScreenBounds.Location, New Size(ShapeScreenBounds.Width + 1, ShapeScreenBounds.Height + 1))
        Dim rRect1 As New Rectangle(ShapeScreenBounds.X + 1, ShapeScreenBounds.Y + 1, ShapeScreenBounds.Width - 1, fSize)
        Dim rRect2 As New Rectangle(ShapeScreenBounds.X + 1, ShapeScreenBounds.Y + fSize, ShapeScreenBounds.Width - 1, ShapeScreenBounds.Height - fSize)
        Dim rRect3 As New Rectangle(ShapeScreenBounds.X + 1, ShapeScreenBounds.Bottom - (fSize * 4), ShapeScreenBounds.Width, fSize * 2)
        Dim rRect4 As New Rectangle(ShapeScreenBounds.X + 1, ShapeScreenBounds.Bottom - (fSize * 2), ShapeScreenBounds.Width, fSize * 2)

        If objStage.DisplayMode = StageShowMode.Search_Highlight Then
            g.FillRectangle(Brushes.Yellow, rRect0)
        Else
            g.FillRectangle(Me.BackgroundBrush, rRect0)
        End If
        DrawRectangleF(g, mStagePen, rRect0)
        Using textBrush As New SolidBrush(objStage.Color)
            If Me.DrawText Then
                Me.DrawScaledString(g, sName, font1, textBrush, rRect1)
                g.DrawLine(mStagePen, rRect0.Left, rRect0.Y + fSize, rRect0.Right, rRect0.Y + fSize)
                Me.DrawScaledString(g, sDescription, font1, textBrush, rRect2)
            End If
            If Not bSub Then
                g.DrawLine(mStagePen, rRect0.Left, rRect3.Top, rRect0.Right, rRect3.Top)
                If Me.DrawText Then Me.DrawScaledString(g, sCreated, font1, textBrush, rRect3)
                g.DrawLine(mStagePen, rRect0.Left, rRect4.Top, rRect0.Right, rRect4.Top)
                If Me.DrawText Then Me.DrawScaledString(g, sLastChanged, font1, textBrush, rRect4)
            End If
        End Using

        'draw live icon
        If Me.DrawIcons Then
            If (objProcess.Attributes And ProcessAttributes.Published) > 0 Then
                Dim imageRect As New Rectangle(ShapeScreenBounds.Right - fSize, ShapeScreenBounds.Top, fSize, fSize)
                g.DrawImage(Me.mPublishedIcon, imageRect)
            End If
        End If

        'Highlight (i.e. as selection) if requested by caller...
        If Me.DrawSelections Then
            If bSelected Then
                DrawSelectionHandles(g, ShapeScreenBounds)
            End If
        End If

    End Sub

    Private Sub DrawRectangleF(ByVal g As Graphics, ByVal p As Pen, ByVal Rect As RectangleF)
        g.DrawRectangle(p, Rect.X, Rect.Y, Rect.Width, Rect.Height)
    End Sub

    ''' <summary>
    ''' Draw a link
    ''' </summary>
    ''' <param name="LinkSource">The source object. This might be a stage, a choice
    ''' node, etc.</param>
    ''' <param name="objDestStage">The destination stage, or
    ''' Nothing to use the coordinates sngDestX,Y</param>
    ''' <param name="objGraphics">The graphics object to draw
    ''' to</param>
    ''' <param name="bSelected">Whether the link is selected or
    ''' not</param>
    ''' <param name="sLabel">The label text to display next to
    ''' the link</param>
    ''' <param name="LinkDestination">The destination point for the
    ''' link, in world coordinates. Only used if objDestStage is nothing;
    ''' otherwise the link destination is inferred from that stage.</param>
    Private Sub DrawLink(ByVal LinkSource As Object, ByVal objDestStage As clsProcessStage, ByVal objGraphics As Graphics, ByVal ViewableRectangle As Rectangle, ByVal bSelected As Boolean, ByVal sLabel As String, ByVal LinkDestination As PointF, Optional ByVal ExcludeLinkArrow As Boolean = False)

        Dim src As PointF, dest As PointF
        Dim rSource, rDest As RectangleF
        Dim LinkMode As StageLinkMode = StageLinkMode.Normal

        Select Case True
            Case TypeOf LinkSource Is clsProcessStage
                Dim Stage As clsProcessStage = CType(LinkSource, clsProcessStage)
                src = Stage.Location
                rSource = Stage.GetDisplayBounds
                LinkMode = Stage.LinkColour
            Case TypeOf LinkSource Is clsChoice
                Dim Choice As clsChoice = CType(LinkSource, clsChoice)
                src = Choice.Location()
                rSource = Choice.DisplayBounds
            Case Else
                Exit Sub
        End Select

        If objDestStage Is Nothing Then
            dest = LinkDestination
            rDest = New RectangleF(LinkDestination, New Size(0, 0))
        Else
            dest = objDestStage.Location
            rDest = objDestStage.GetDisplayBounds
        End If

        DrawLink(rSource, rDest, objGraphics, ViewableRectangle, bSelected, sLabel, LinkMode, ExcludeLinkArrow)
    End Sub




    ''' <summary>
    ''' Draws a link.
    ''' </summary>
    ''' <param name="rSource">The bounding rectangle of the source object, in world coordinates.</param>
    ''' <param name="rDest">The bounding rectangle of the target object, in world coordinates.</param>
    ''' <param name="objGraphics">The graphics object</param>
    ''' <param name="ViewableRectangle">The display area</param>
    ''' <param name="bSelected">Whether the link is selected or not</param>
    ''' <param name="sLabel">The link label text</param>
    ''' <param name="tLinkMode">The linkmode of the link</param>
    ''' <remarks></remarks>
    Private Sub DrawLink(ByVal rSource As RectangleF, ByVal rDest As RectangleF, ByVal objGraphics As Graphics, ByVal ViewableRectangle As Rectangle, ByVal bSelected As Boolean, ByVal sLabel As String, ByVal tLinkMode As StageLinkMode, Optional ByVal ExcludeLinkArrow As Boolean = False)


        Dim pSource As PointF = clsGeometry.GetRectangleCentre(rSource)
        Dim pDest As PointF = clsGeometry.GetRectangleCentre(rDest)

        'Do not bother drawing a link source and destination points are the same point.
        'This fixes bug 3244
        If pSource = pDest Then Exit Sub

        Dim pScreenSource As Point = Me.mCoordinateTranslator.TranslateWorldPointToScreenPoint(pSource, ViewableRectangle.Size)
        Dim pScreenDest As Point = Me.mCoordinateTranslator.TranslateWorldPointToScreenPoint(pDest, ViewableRectangle.Size)

        'Draw the line...
        If mobjparent.ViewMode = ProcessViewMode.CompareProcesses AndAlso tLinkMode = StageLinkMode.DestinationChanged Then
            objGraphics.DrawLine(Pens.DarkOrange, pScreenSource, pScreenDest)
        Else
            objGraphics.DrawLine(Pens.DimGray, pScreenSource, pScreenDest)
        End If


        'Get the location of the arrow on the link
        Dim ArrowLocation As PointF = clsProcessStage.GetLinkArrowPosition(rSource, rDest)
        Dim UnitDirectionVect As clsGeometry.Vector = New clsGeometry.Vector(pDest.X - pSource.X, pDest.Y - pSource.Y).ToUnitVector
        Dim UnitNormalDirectionVect As clsGeometry.Vector = UnitDirectionVect.Normal

        'Draw any label text.
        If sLabel <> "" Then
            'Measure size of label
            Dim LabelPointF As PointF = ArrowLocation
            Dim LabelSizeF As SizeF = Me.MeasureScaledString(objGraphics, sLabel, mFont)
            Const iLabelOffset As Integer = 5

            'Decide on label placement
            LabelPointF = New PointF(ArrowLocation.X, ArrowLocation.Y)
            Select Case True
                Case UnitDirectionVect.X = 0    'Line vertical
                    If UnitDirectionVect.Y < 0 Then
                        LabelPointF.X -= iLabelOffset + LabelSizeF.Width
                    Else
                        LabelPointF.X += iLabelOffset
                    End If
                    LabelPointF.Y -= (LabelSizeF.Height / 2)
                Case UnitDirectionVect.Y = 0    'Line Horizontal
                    LabelPointF.X -= (LabelSizeF.Width / 2)
                    If UnitDirectionVect.X > 0 Then
                        'Link points right, label on top.
                        LabelPointF.Y -= iLabelOffset + LabelSizeF.Height
                    Else
                        'Link points left, label underneath.
                        LabelPointF.Y += iLabelOffset
                    End If
                Case Else           'Line at angle
                    If UnitDirectionVect.X > 0 Then
                        If UnitDirectionVect.Y < 0 Then
                            LabelPointF.X -= LabelSizeF.Width
                        Else
                            LabelPointF.X += iLabelOffset
                        End If
                        LabelPointF.Y -= LabelSizeF.Height
                    Else
                        If UnitDirectionVect.Y < 0 Then
                            LabelPointF.X -= iLabelOffset + LabelSizeF.Width
                            LabelPointF.Y += iLabelOffset
                        Else
                            LabelPointF.X += iLabelOffset
                        End If
                    End If
            End Select

            'Draw the label
            Dim objFormat As New StringFormat
            objFormat.Alignment = StringAlignment.Center
            objFormat.LineAlignment = StringAlignment.Center
            Dim LabelScreenDest As Point = mCoordinateTranslator.TranslateWorldPointToScreenPoint(LabelPointF, ViewableRectangle.Size)
            Dim rLabelDest As New Rectangle(LabelScreenDest, Size.Ceiling(LabelSizeF))
            Me.DrawScaledString(objGraphics, sLabel, mFont, LinkPen.Brush, rLabelDest, objFormat)
        End If

        'Now calculate the three points that form the arrow...
        Dim arrowpoints(2) As PointF
        Dim ArrowHeight As Single = Math.Max(1, 3 * Me.CoordinateTranslator.ZoomLevel)
        Dim ArrowWidth As Single = Math.Max(2, 2 * ArrowHeight - (ArrowHeight Mod 2))
        Dim ScreenLineMid As Point = Me.CoordinateTranslator.TranslateWorldPointToScreenPoint(ArrowLocation, ViewableRectangle.Size)
        arrowpoints(2).X = ScreenLineMid.X + UnitNormalDirectionVect.X * ArrowWidth - UnitDirectionVect.X * ArrowHeight
        arrowpoints(2).Y = ScreenLineMid.Y + UnitNormalDirectionVect.Y * ArrowWidth - UnitDirectionVect.Y * ArrowHeight
        arrowpoints(1).X = ScreenLineMid.X - UnitNormalDirectionVect.X * ArrowWidth - UnitDirectionVect.X * ArrowHeight
        arrowpoints(1).Y = ScreenLineMid.Y - UnitNormalDirectionVect.Y * ArrowHeight - UnitDirectionVect.Y * ArrowHeight
        arrowpoints(0).X = ScreenLineMid.X + UnitDirectionVect.X * ArrowHeight
        arrowpoints(0).Y = ScreenLineMid.Y + UnitDirectionVect.Y * ArrowHeight

        'Now finally we can draw the triangle...
        If Not ExcludeLinkArrow Then
            If bSelected Then
                objGraphics.FillPolygon(Me.SelectedItemBrush, arrowpoints)
            Else
                objGraphics.FillPolygon(Me.LinkPen.Brush, arrowpoints)
            End If
        End If
    End Sub

    Private Sub DrawGridLine(ByVal src As PointF, ByVal dest As PointF, ByVal objGraphics As Graphics, ByVal ViewableRectangle As Rectangle)
        'Translate to screen coordinates...
        src = Me.mCoordinateTranslator.TranslateWorldPointToScreenPoint(src, ViewableRectangle.Size)
        dest = Me.mCoordinateTranslator.TranslateWorldPointToScreenPoint(dest, ViewableRectangle.Size)

        'Draw the line...
        objGraphics.DrawLine(mGridPen, src, dest)
    End Sub

    Private Sub DrawObject(ByVal objImage As Image, ByVal objStage As clsProcessStage, ByVal objGraphics As Graphics, ByVal ViewableRectangle As Rectangle)
        Dim sText As String = LTools.GetC(objStage.GetShortText, "misc", "stage")
        Dim bSelected As Boolean = objStage.IsSelected()
        Dim bHasBreakpoint As Boolean = objStage.HasBreakPoint
        Dim tBreakPointType As clsProcessBreakpoint.BreakEvents
        Dim sBreakPointCondition As String = Nothing
        If bHasBreakpoint Then
            tBreakPointType = objStage.BreakPoint.BreakPointType
            sBreakPointCondition = clsExpression.NormalToLocal(objStage.BreakPoint.Condition)
        End If
        Dim cFontColor As Color = objStage.Color
        Dim objFont As Font = objStage.Font

        DrawObject(objImage, objStage, objStage.Location, objStage.GetDisplayBounds.Size, objGraphics, ViewableRectangle, sText, bSelected, bHasBreakpoint, tBreakPointType, sBreakPointCondition, cFontColor, objFont)
    End Sub

    ''' <summary>
    ''' Draws the specified object.
    ''' </summary>
    ''' <param name="img">The image to be drawn on the screen; its
    ''' bounds will be determined by the ObjectLocation and ObjectSize
    ''' parameters.</param>
    ''' <param name="stg">The stage that this object represents,
    ''' if any (may be null).</param>
    ''' <param name="locn">The centre-point of the object's intended
    ''' location, in world coordinates.</param>
    ''' <param name="size">The size of the object, in world coordinates</param>
    ''' <param name="g">The graphics object to be used.</param>
    ''' <param name="rect">The size of the viewport, in screen coordinates.</param>
    ''' <param name="txt">The text to be rendered, in the middle of the shape,
    ''' if any.</param>
    ''' <param name="selected">True if the object is to be rendered as selected.</param>
    ''' <param name="hasBreakPoint">True if the object is to be rendered with 
    ''' a breakpoint rectangle around it.</param>
    ''' <param name="breakpointType">The type of breakpoint, if any.</param>
    ''' <param name="breakPointCondition">The Automate expression used
    ''' as a breakpoint condition, if any.</param>
    ''' <param name="fontColor">The colour of the object's text.</param>
    ''' <param name="font">The font to use when rendering the text.</param>
    Private Sub DrawObject(
     ByVal img As Image, ByVal stg As clsProcessStage, ByVal locn As PointF, ByVal size As SizeF,
     ByVal g As Graphics, ByVal rect As Rectangle, ByVal txt As String, ByVal selected As Boolean,
     ByVal hasBreakPoint As Boolean, ByVal breakpointType As clsProcessBreakpoint.BreakEvents,
     ByVal breakPointCondition As String, ByVal fontColor As Color, ByVal font As Font)

        Dim DestinationBounds = GetDestinationBoundaries(locn, size)
        Dim DestinationScreenBounds = GetDestinationScreenBoundaries(DestinationBounds, rect.Size)

        If hasBreakPoint Then
            size.Width += 5
            size.Height += 5
        End If

        g.DrawImage(img, DestinationScreenBounds)

        If hasBreakPoint Then
            g.DrawImage(mBreakpointImage, DestinationScreenBounds)
        End If

        AddPixmapItem(DestinationBounds, stg)

        If txt <> "" Then
            Dim objFormat As New StringFormat
            objFormat.Alignment = StringAlignment.Center
            objFormat.LineAlignment = StringAlignment.Center
            If Me.DrawText Then
                Using textBrush As New SolidBrush(fontColor)
                    If txt.Length > 10000 Then
                        txt = txt.Substring(0, 10000)
                    End If
                    Dim rectTxt = DestinationScreenBounds
                    If stg.StageType = StageTypes.Note Then
                        rectTxt.Location = New Point(rectTxt.Location.X + NoteMargin, rectTxt.Location.Y + NoteMargin)
                        rectTxt.Size = New Size(rectTxt.Size.Width - NoteMargin * 2, rectTxt.Size.Height - NoteMargin * 2)
                        If DestinationScreenBounds.Contains(rectTxt) = False Then rectTxt = DestinationScreenBounds
                    End If
                    DrawScaledString(g, txt, font, textBrush, rectTxt, objFormat)
                End Using
            End If
        End If

        If Me.DrawIcons Then
            'draw conditional breakpoint icon
            If hasBreakPoint AndAlso ((breakpointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0) Then
                Dim ConditionalIconBounds As New RectangleF(DestinationBounds.Right - Me.mConditionalBreakpointIcon.Width, DestinationBounds.Bottom - Me.mConditionalBreakpointIcon.Height, Me.mConditionalBreakpointIcon.Width, Me.mConditionalBreakpointIcon.Height)
                g.DrawImage(Me.mConditionalBreakpointIcon, mCoordinateTranslator.TranslateWorldRectToScreenRect(ConditionalIconBounds, rect.Size))
                Me.AddPixmapItem(ConditionalIconBounds, My.Resources.BreakpointCondition & breakPointCondition)
            End If

            ' And collection's "single row" indicator
            Dim collStg As clsCollectionStage = TryCast(stg, clsCollectionStage)
            If collStg IsNot Nothing AndAlso collStg.SingleRow Then
                Dim bounds As New RectangleF(
                     DestinationBounds.Right - mSingleRowCollectionIcon.Width,
                     DestinationBounds.Top,
                     mSingleRowCollectionIcon.Width, mSingleRowCollectionIcon.Height)
                g.DrawImage(mSingleRowCollectionIcon,
                     mCoordinateTranslator.TranslateWorldRectToScreenRect(bounds, rect.Size))
            End If
        End If

        'Highlight (i.e. as selection) if requested by caller...
        If Me.DrawSelections Then
            If selected Then
                Me.DrawSelectionHandles(g, DestinationScreenBounds)
            End If
        End If

    End Sub

    Private Function GetDestinationBoundaries(location As PointF, size As SizeF) As RectangleF
        Dim newLocation = New PointF(location.X - (size.Width / 2), location.Y - (size.Height / 2))
        Return New RectangleF(newLocation, size)
    End Function
    Private Function GetDestinationScreenBoundaries(worldRectangle As RectangleF, screenSize As Size) As Rectangle
        Return mCoordinateTranslator.TranslateWorldRectToScreenRect(worldRectangle, screenSize)
    End Function

    Private Sub DrawRectangleWithRoundedCorders(graphics As Graphics, bounds As Rectangle, lineThickness As Integer, cornerRadius As Integer)
        Dim pen = New Pen(New SolidBrush(Color.FromArgb(80, 170, 221)), lineThickness)
        Dim arcDimension = cornerRadius * 2
        'top left corner
        graphics.DrawArc(pen, bounds.Left, bounds.Top, arcDimension, arcDimension, 180, 90)
        'top line
        graphics.DrawLine(pen, bounds.Left + cornerRadius, bounds.Top, bounds.Right, bounds.Top)
        'top right corner 
        graphics.DrawArc(pen, bounds.Right - (2 * cornerRadius), bounds.Top, arcDimension, arcDimension, 270, 90)
        'right line
        graphics.DrawLine(pen, bounds.Right, bounds.Top + cornerRadius, bounds.Right, bounds.Bottom)
        'bottom Right corner
        graphics.DrawArc(pen, bounds.Right - (cornerRadius * 2), bounds.Bottom - (cornerRadius * 2), arcDimension, arcDimension, 0, 90)
        'bottom border
        graphics.DrawLine(pen, bounds.Left + cornerRadius, bounds.Bottom, bounds.Right, bounds.Bottom)
        'bottom left corner
        graphics.DrawArc(pen, bounds.Left, bounds.Bottom - (cornerRadius * 2), arcDimension, arcDimension, 90, 90)
        'left border
        graphics.DrawLine(pen, bounds.Left, bounds.Top + cornerRadius, bounds.Left, bounds.Bottom)
    End Sub

    Private Sub DrawSkill(stage As clsSkillStage, g As Graphics, rectangle As Rectangle, mode As StageShowMode)

        Dim destinationBounds = GetDestinationBoundaries(stage.Location, stage.GetDisplayBounds.Size)
        Dim destinationScreenBounds = GetDestinationScreenBoundaries(destinationBounds, rectangle.Size)

        DrawSkillOutline(g, destinationScreenBounds, mode, stage.HasBreakPoint)
        DrawSkillText(g, stage, destinationScreenBounds)

        Dim skillBytes = stage.GetImageBytes()

        Dim paddingX = Math.Min(10, Convert.ToInt32(Math.Floor(destinationScreenBounds.Width * 0.07)))
        Dim paddingY = Math.Min(10, Convert.ToInt32(Math.Floor(destinationScreenBounds.Height * 0.07)))
        Dim padding = Math.Min(paddingX, paddingY)
        If skillBytes IsNot Nothing Then
            Using stream = New MemoryStream(skillBytes)
                Using skillIcon = Image.FromStream(stream)
                    Dim skillIconHeight = GetIconHeight(
                        skillIcon,
                        Convert.ToInt32(Math.Floor(destinationScreenBounds.Height * 0.3)),
                        Convert.ToInt32(Math.Floor(destinationScreenBounds.Width * 0.8)))
                    Dim skillIconWidth = GetIconWidth(skillIcon, skillIconHeight)
                    Dim iconRectangle = New RectangleF(
                        destinationScreenBounds.Left + padding,
                        destinationScreenBounds.Bottom - skillIconHeight - padding,
                        skillIconWidth,
                        skillIconHeight)
                    g.DrawImage(skillIcon, iconRectangle)
                End Using
            End Using
        End If

        Using categoryIconImage = GetCategoryIcon(stage.GetCategory())
            If categoryIconImage IsNot Nothing Then
                Dim iconHeight = GetIconHeight(
                    categoryIconImage,
                    Convert.ToInt32(Math.Floor(destinationScreenBounds.Height * 0.3)),
                    Convert.ToInt32(Math.Floor(destinationScreenBounds.Width * 0.3)))
                Dim iconWidth = GetIconWidth(categoryIconImage, iconHeight)
                Dim iconRectangle = New RectangleF(
                    destinationScreenBounds.Right - iconWidth - padding,
                    destinationScreenBounds.Top + padding,
                    iconWidth,
                    iconHeight)
                g.DrawImage(categoryIconImage, iconRectangle)
            End If
        End Using

        If Me.DrawIcons _
            AndAlso stage.HasBreakPoint _
            AndAlso ((stage.BreakPoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0) Then
            DrawConditionalBreakpoint(g, destinationBounds, rectangle.Size, clsExpression.NormalToLocal(stage.BreakPoint.Condition))
        End If

        If Me.DrawSelections AndAlso stage.IsSelected() Then
            DrawSelectionHandles(g, destinationScreenBounds)
        End If
    End Sub

    Private Sub DrawSkillOutline(graphics As Graphics, destinationScreenBounds As Rectangle, mode As StageShowMode, hasBreakpoint As Boolean)
        DrawRectangleWithRoundedCorders(graphics, destinationScreenBounds, 1, 3)
        Dim smallerBoundary = New Rectangle(destinationScreenBounds.X + 5,
                                            destinationScreenBounds.Y + 5,
                                            destinationScreenBounds.Width - 10,
                                            destinationScreenBounds.Height - 10)
        DrawRectangleWithRoundedCorders(graphics, smallerBoundary, 3, 3)

        Dim fillColour = Color.White
        Select Case mode
            Case StageShowMode.CurrentDebug
                fillColour = Color.FromArgb(255, 192, 0) 'Orange
            Case StageShowMode.Audit_Deleted
                fillColour = Color.FromArgb(255, 0, 0) 'Red
            Case StageShowMode.Audit_Modified
                fillColour = Color.FromArgb(255, 255, 0) 'Yellow
            Case StageShowMode.Audit_New
                fillColour = Color.FromArgb(0, 176, 80) 'Green
            Case StageShowMode.Search_Highlight
                fillColour = Color.FromArgb(255, 255, 0) ' Bright Yellow
        End Select
        Using fillBrush = New SolidBrush(fillColour)
            graphics.FillRectangle(fillBrush, New Rectangle(smallerBoundary.X + 2,
                                            smallerBoundary.Y + 2,
                                            smallerBoundary.Width - 3,
                                            smallerBoundary.Height - 3))
        End Using


        If hasBreakpoint Then
            graphics.DrawImage(mBreakpointImage, destinationScreenBounds)
        End If
    End Sub

    Private Sub DrawSkillText(graphics As Graphics, stage As clsSkillStage, destinationScreenBounds As Rectangle)
        Dim text = New StringBuilder()
        text.AppendLine(stage.Name.ToUpper())
        text.AppendLine(stage.ActionName)
        Dim stageNameRectangle = New RectangleF(destinationScreenBounds.Left + 10,
                                                destinationScreenBounds.Top + 10,
                                                destinationScreenBounds.Width - 40,
                                                destinationScreenBounds.Height - 40)

        Using textBrush As New SolidBrush(Color.Black)
            Me.DrawScaledString(graphics, text.ToString(), stage.Font, textBrush, stageNameRectangle)
        End Using
    End Sub

    Private Sub DrawConditionalBreakpoint(graphics As Graphics, destinationBounds As RectangleF, size As Size, expression As String)
        Dim conditionalIconBounds = New RectangleF(destinationBounds.Right - mConditionalBreakpointIcon.Width,
                                                       destinationBounds.Bottom - mConditionalBreakpointIcon.Height,
                                                       mConditionalBreakpointIcon.Width,
                                                       mConditionalBreakpointIcon.Height)
        graphics.DrawImage(mConditionalBreakpointIcon, mCoordinateTranslator.TranslateWorldRectToScreenRect(conditionalIconBounds, size))
        AddPixmapItem(conditionalIconBounds, My.Resources.BreakpointCondition & expression)
    End Sub

    Private Function GetCategoryIcon(category As SkillCategory) As Bitmap
        Select Case category
            Case SkillCategory.VisualPerception
                Return My.Resources.SkillToolbarIcons.VisualPerception
            Case SkillCategory.PlanningAndSequencing
                Return My.Resources.SkillToolbarIcons.PlanningAndSequencing
            Case SkillCategory.Collaboration
                Return My.Resources.SkillToolbarIcons.Collaboration
            Case SkillCategory.KnowledgeAndInsight
                Return My.Resources.SkillToolbarIcons.KnowledgeAndInsight
            Case SkillCategory.ProblemSolving
                Return My.Resources.SkillToolbarIcons.ProblemSolving
            Case SkillCategory.Learning
                Return My.Resources.SkillToolbarIcons.Learning
            Case Else
                Return My.Resources.SkillToolbarIcons.UnknownSkillCategory
        End Select
    End Function

    Private Function GetIconWidth(icon As Image, skillIconHeight As Integer) As Integer
        Dim iconHeight = icon.Height
        Dim iconWidth = icon.Width

        Dim width = (iconWidth / iconHeight) * skillIconHeight
        Return Int32.Parse(Math.Truncate(width).ToString())
    End Function

    Private Function GetIconHeight(icon As Image, maxHeight As Integer, maxWidth As Integer) As Integer
        Dim iconHeight = icon.Height
        Dim iconWidth = icon.Width

        Dim width = (iconWidth / iconHeight) * maxHeight
        Dim height = If(width <= maxWidth, maxHeight, (iconHeight / iconWidth) * maxWidth)

        Return Int32.Parse(Math.Truncate(height).ToString())
    End Function

    ''' <summary>
    ''' Draws the maroon selection handles at the corners of
    ''' the specified screen rectangles.
    ''' </summary>
    ''' <param name="DestinationScreenBounds">The screen bounds
    ''' around which the selection rectangles should be drawn.</param>
    Private Sub DrawSelectionHandles(ByVal objgraphics As Graphics, ByVal DestinationScreenBounds As Rectangle)
        Dim b As Brush = Me.SelectedItemBrush
        Const SizeParam As Integer = 2
        Dim SelectionRect As New Rectangle(DestinationScreenBounds.Location, New Size(2 * SizeParam + 1, 2 * SizeParam + 1))
        Dim HalfSize As New Size(SizeParam, SizeParam)

        SelectionRect.Location = New Point(DestinationScreenBounds.Left - HalfSize.Width, DestinationScreenBounds.Top - HalfSize.Height)
        objgraphics.FillRectangle(b, SelectionRect)

        SelectionRect.Location = New Point(DestinationScreenBounds.Right - HalfSize.Width, DestinationScreenBounds.Top - HalfSize.Height)
        objgraphics.FillRectangle(b, SelectionRect)

        SelectionRect.Location = New Point(DestinationScreenBounds.Right - HalfSize.Width, DestinationScreenBounds.Bottom - HalfSize.Height)
        objgraphics.FillRectangle(b, SelectionRect)

        SelectionRect.Location = New Point(DestinationScreenBounds.Left - HalfSize.Width, DestinationScreenBounds.Bottom - HalfSize.Height)
        objgraphics.FillRectangle(b, SelectionRect)
    End Sub

    ''' <summary>
    ''' Draws a tooltip on the process picturebox using the tooltip text in the 
    ''' class member msToolTipText at the point mpToolTipPoint
    ''' </summary>
    ''' <param name="g">The graphics object to use to draw the tooltip.</param>
    Private Sub DrawToolTip(ByVal g As Graphics)
        clsUserInterfaceUtils.GraphicsUtils.DrawToolTip(g, msToolTipText, mpToolTipPoint)
        Me.mdtTimeToolTipWasFirstDisplayed = DateTime.Now
    End Sub

#Region "UpdateProcessMI"
    ''' <summary>
    ''' Draws Process MI for stages in a clsProcessMI data object.
    ''' </summary>
    ''' <param name="oGraphics"></param>
    ''' <param name="dWidth"></param>
    ''' <param name="dHeight"></param>
    ''' <param name="oProcessMI"></param>
    Public Sub UpdateProcessMI(ByVal oGraphics As Graphics, ByVal dWidth As Single, ByVal dHeight As Single, ByVal oProcessMI As clsProcessMI)
        Try
            Dim oProcess As clsProcess
            Dim aStages As Generic.List(Of clsProcessStage)
            Dim oNextStage As clsProcessStage = Nothing
            Dim gNextStage As Guid
            Dim oSizeF As SizeF
            Dim pText As PointF
            Dim pScreen As Point
            Dim colMajor, colMinor As Color

            Dim sText As String = ""
            Dim sTotal As String = ""
            Dim sAverage As String = ""
            Dim sCount As String = ""

            oProcess = oProcessMI.Process

            If oProcessMI.DataExists Then

                'Draw the MI for each stage
                aStages = oProcessMI.GetStages
                For Each oStage As clsProcessStage In aStages

                    If oStage Is Nothing OrElse Not oStage.GetSubSheetID.Equals(oProcess.GetActiveSubSheet) Then
                        'Nothing to draw
                    Else

                        Select Case oStage.StageType

                            Case StageTypes.Decision
                                Dim iTrueCount, iFalseCount As Integer
                                Dim oDecision As clsDecisionStage = CType(oStage, clsDecisionStage)
                                Dim dblPercent As Double

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)

                                If sCount = "" Then
                                    'Nothing to draw
                                Else
                                    iTrueCount = CInt(oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.true))
                                    iFalseCount = CInt(oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.false))

                                    'This loop is only to save code, there are always 2 statistics - True and False.
                                    For j As Integer = 1 To 2
                                        If j = 1 Then
                                            'Get the True statistics.
                                            gNextStage = oDecision.OnTrue
                                            If Not gNextStage.Equals(Guid.Empty) Then
                                                oNextStage = oProcess.GetStage(gNextStage)
                                                colMajor = AutomateControls.ColourScheme.ProcessMI.DecisionTrueMajor
                                                colMinor = AutomateControls.ColourScheme.ProcessMI.DecisionTrueMinor
                                                dblPercent = iTrueCount / (iTrueCount + iFalseCount)
                                                sText = String.Format(My.Resources.TrueFalseCountPercentage, CStr(iTrueCount), sCount, vbCrLf, CInt(100 * dblPercent))
                                            End If
                                        Else
                                            'Get the False statistics.
                                            gNextStage = oDecision.OnFalse
                                            If Not gNextStage.Equals(Guid.Empty) Then
                                                oNextStage = oProcess.GetStage(gNextStage)
                                                colMajor = AutomateControls.ColourScheme.ProcessMI.DecisionFalseMajor
                                                colMinor = AutomateControls.ColourScheme.ProcessMI.DecisionFalseMinor
                                                dblPercent = iFalseCount / (iTrueCount + iFalseCount)
                                                sText = String.Format(My.Resources.TrueFalseCountPercentage, CStr(iFalseCount), sCount, vbCrLf, CInt(100 * dblPercent))
                                            End If
                                        End If

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawDecisionStageMI(sText, pScreen.X, pScreen.Y, oGraphics, mFont, colMajor, colMinor, dblPercent)
                                    Next

                                End If

                            Case StageTypes.Calculation

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    Dim oCalculation As clsCalculationStage = CType(oStage, clsCalculationStage)
                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess

                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName & vbCrLf & My.Resources.StepsLabel & sCount

                                        Dim oResultStage As clsDataStage = CType(oStage.Process.GetStage(oCalculation.StoreIn), clsDataStage)
                                        If oResultStage IsNot Nothing Then
                                            Select Case oResultStage.GetDataType()

                                                Case DataType.number

                                                    sText &= vbCrLf & My.Resources.MaxLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.maximum) _
                                                     & vbCrLf & My.Resources.MinLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.minimum)

                                                Case DataType.flag

                                                    sText &= vbCrLf & My.Resources.TrueLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.true) _
                                                     & vbCrLf & My.Resources.FalseLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.false)

                                                Case DataType.date

                                                    sText &= vbCrLf & My.Resources.MaxLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.maximum) _
                                                     & vbCrLf & My.Resources.MinLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.minimum)

                                                Case DataType.datetime

                                                    sText &= vbCrLf & My.Resources.MaxLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.maximum) _
                                                     & vbCrLf & My.Resources.MinLabel & oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.minimum)

                                                Case Else

                                            End Select
                                        Else
                                            sText &= vbCrLf & String.Format(My.Resources.NoInformationAvailableCRtheDataItemToStoreInNoLongerExists, vbCrLf)
                                        End If

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Calculation)

                                    End If
                                End If
                            Case StageTypes.MultipleCalculation

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.MultipleCalculation)

                                    End If
                                End If
                            Case StageTypes.Action, StageTypes.Skill

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else
                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sTotal = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.total)
                                        sAverage = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.average)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount _
                                         & vbCrLf & My.Resources.TotalTimeLabel & sTotal _
                                         & vbCrLf & My.Resources.AvgTimeLabel & sAverage

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Action)

                                    End If
                                End If

                            Case StageTypes.Process

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else
                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sTotal = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.total)
                                        sAverage = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.average)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount _
                                         & vbCrLf & My.Resources.TotalTimeLabel & sTotal _
                                         & vbCrLf & My.Resources.AvgTimeLabel & sAverage

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Process)

                                    End If
                                End If

                            Case StageTypes.SubSheet

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else
                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sTotal = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.total)
                                        sAverage = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.average)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount _
                                         & vbCrLf & My.Resources.TotalTimeLabel & sTotal _
                                         & vbCrLf & My.Resources.AvgTimeLabel & sAverage

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.SubSheet)

                                    End If
                                End If

                            Case StageTypes.ChoiceStart

                                'Choices are handled slightly differently from other stage types.
                                'Because a choice has more than one part (start, nodes, end) the
                                'data can't be keyed on stage ID. To get around this the key has
                                'been made from 'choice start ID' + 'node position'. The end is
                                'treated as another node.

                                Dim sTrue As String
                                Dim oChoice As clsChoice
                                Dim oStart As clsChoiceStartStage = CType(oStage, clsChoiceStartStage)
                                Dim oEnd As clsChoiceEndStage = oStart.Process.GetChoiceEnd(oStart)
                                Dim pChoice As PointF

                                If oEnd Is Nothing OrElse oEnd.OnSuccess.Equals(Guid.Empty) Then
                                    'Nothing to draw
                                Else
                                    gNextStage = oEnd.OnSuccess
                                    oNextStage = oProcess.GetStage(gNextStage)

                                    sCount = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.Count + 1), clsProcessMI.AttributeType.count)

                                    'If the choice end doesn't have any data, look through the choices.
                                    For Each oChoice In oStart.Choices
                                        If sCount = "" Then
                                            sCount = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.IndexOf(oChoice) + 1), clsProcessMI.AttributeType.count)
                                        Else
                                            Exit For
                                        End If
                                    Next

                                    'If none of the choices have any data there is nothing to draw.
                                    If sCount = "" Then
                                        Exit For
                                    End If

                                    sTrue = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.Count + 1), clsProcessMI.AttributeType.true)

                                    If sTrue = "" Then
                                        sTrue = "0"
                                    End If

                                    sText = oEnd.GetName _
                                     & vbCrLf & My.Resources.TrueLabel & sTrue & My.Resources.Slash & sCount

                                    'Draw the choice end MI.
                                    oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                    pText = GetStageMIPosition(oEnd, oNextStage, oSizeF)
                                    pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                    DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Choice)

                                    For c As Integer = 0 To oStart.Choices.Count - 1

                                        oChoice = oStart.Choices(c)
                                        gNextStage = oChoice.LinkTo

                                        If Not gNextStage.Equals(Guid.Empty) Then
                                            oNextStage = oProcess.GetStage(gNextStage)
                                            sTrue = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.IndexOf(oChoice) + 1), clsProcessMI.AttributeType.true)

                                            If sTrue = "" Then
                                                sTrue = "0"
                                            End If

                                            sText = oChoice.Name _
                                             & vbCrLf & My.Resources.TrueLabel & sTrue & My.Resources.Slash & sCount

                                            'Draw the choice node MI.
                                            oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                            pChoice = oChoice.Location
                                            pChoice.X += oStart.GetDisplayX
                                            pChoice.Y += oStart.GetDisplayY
                                            pText = GetStageMIPosition(pChoice, oNextStage, oSizeF)
                                            pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                            DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Choice)
                                        End If

                                    Next
                                End If


                            Case StageTypes.WaitStart

                                'Waits are handled slightly differently from other stage types.
                                'Because a choice has more than one part (start, nodes, end) the
                                'data can't be keyed on stage ID. To get around this the key has
                                'been made from 'wait start ID' + 'node position'. The end is
                                'treated as another node.

                                Dim sTrue As String
                                Dim oChoice As clsChoice
                                Dim oStart As clsWaitStartStage = CType(oStage, clsWaitStartStage)
                                Dim oEnd As clsWaitEndStage = CType(oStart.Process.GetChoiceEnd(oStart), clsWaitEndStage)
                                Dim pChoice As PointF

                                If oEnd Is Nothing OrElse oEnd.OnSuccess.Equals(Guid.Empty) Then
                                    'Nothing to draw
                                Else
                                    gNextStage = oEnd.OnSuccess
                                    oNextStage = oProcess.GetStage(gNextStage)

                                    sCount = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.Count + 1), clsProcessMI.AttributeType.count)

                                    'If the wait end doesn't have any data, look through the choices.
                                    For Each oChoice In oStart.Choices
                                        If sCount = "" Then
                                            sCount = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.IndexOf(oChoice) + 1), clsProcessMI.AttributeType.count)
                                        Else
                                            Exit For
                                        End If
                                    Next

                                    'If none of the choices have any data there is nothing to draw.
                                    If sCount = "" Then
                                        Exit For
                                    End If

                                    sTrue = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.Count + 1), clsProcessMI.AttributeType.true)

                                    If sTrue = "" Then
                                        sTrue = "0"
                                    End If

                                    sText = oEnd.GetName _
                                     & vbCrLf & My.Resources.TrueLabel & sTrue & My.Resources.Slash & sCount

                                    'Draw the wait end MI.
                                    oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                    pText = GetStageMIPosition(oEnd, oNextStage, oSizeF)
                                    pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                    DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Wait)

                                    For c As Integer = 0 To oStart.Choices.Count - 1

                                        oChoice = oStart.Choices(c)
                                        gNextStage = oChoice.LinkTo

                                        If Not gNextStage.Equals(Guid.Empty) Then
                                            oNextStage = oProcess.GetStage(gNextStage)
                                            sTrue = oProcessMI.GetData(oStage.GetStageID.ToString & CStr(oStart.Choices.IndexOf(oChoice) + 1), clsProcessMI.AttributeType.true)

                                            If sTrue = "" Then
                                                sTrue = "0"
                                            End If

                                            sText = oChoice.Name _
                                             & vbCrLf & My.Resources.TrueLabel & sTrue & My.Resources.Slash & sCount

                                            'Draw the wait node MI.
                                            oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                            pChoice = oChoice.Location
                                            pChoice.X += oStart.GetDisplayX
                                            pChoice.Y += oStart.GetDisplayY
                                            pText = GetStageMIPosition(pChoice, oNextStage, oSizeF)
                                            pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                            DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Wait)
                                        End If

                                    Next
                                End If


                            Case StageTypes.End

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    sText = oStage.GetName _
                                     & vbCrLf & My.Resources.StepsLabel & sCount

                                    'Draw the MI.
                                    oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                    pText = oStage.Location
                                    pText.X += CInt(oStage.GetDisplayWidth / 2) + 5
                                    pText.Y += CInt(oStage.GetDisplayHeight / 2) + 5

                                    pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                    DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.End)

                                End If


                            Case StageTypes.Start

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Start)

                                    End If

                                End If


                            Case StageTypes.Alert

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Alert)

                                    End If

                                End If


                            Case StageTypes.Code

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Code)

                                    End If

                                End If

                            Case StageTypes.Read

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Read)

                                    End If

                                End If


                            Case StageTypes.Write

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Write)

                                    End If

                                End If


                            Case StageTypes.Navigate

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Navigate)

                                    End If

                                End If
                            Case StageTypes.Exception

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    sText = oStage.GetName _
                                     & vbCrLf & My.Resources.StepsLabel & sCount

                                    'Draw the MI.
                                    oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                    pText = oStage.Location
                                    pText.X += CInt(oStage.GetDisplayWidth / 2) + 5
                                    pText.Y += CInt(oStage.GetDisplayHeight / 2) + 5

                                    pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                    DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Exception)

                                End If
                            Case StageTypes.Recover

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Recover)

                                    End If
                                End If
                            Case StageTypes.Resume

                                sCount = oProcessMI.GetData(oStage.GetStageID, clsProcessMI.AttributeType.count)
                                If sCount = "" Then
                                    'Nothing to draw
                                Else

                                    gNextStage = CType(oStage, clsLinkableStage).OnSuccess
                                    If Not gNextStage.Equals(Guid.Empty) Then
                                        oNextStage = oProcess.GetStage(gNextStage)

                                        sText = oStage.GetName _
                                         & vbCrLf & My.Resources.StepsLabel & sCount

                                        'Draw the MI.
                                        oSizeF = Me.MeasureScaledString(oGraphics, sText, mFont)
                                        pText = GetStageMIPosition(oStage, oNextStage, oSizeF)

                                        pScreen = mCoordinateTranslator.TranslateWorldPointToScreenPoint(pText, New Size(CInt(dWidth), CInt(dHeight)))
                                        DrawStageMI(sText, pScreen, oGraphics, mFont, AutomateControls.ColourScheme.ProcessMI.Resume)

                                    End If
                                End If
                        End Select

                    End If

                Next

            End If
        Catch ex As Exception
            If Not mbProcessMIException Then
                ShowProcessMIException(ex)
                mbProcessMIException = True
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Indicates whether an exception has occured in process mi.
    ''' </summary>
    Private mbProcessMIException As Boolean

    ''' <summary>
    ''' Shows the user a message explaining that Process MI
    ''' has encountered an exception.
    ''' </summary>
    ''' <param name="Ex">The exception which caused the problem.</param>
    Private Sub ShowProcessMIException(ByVal Ex As Exception)
        UserMessage.Show(String.Format(My.Resources.InternalErrorAnExceptionHasOccuredInTheProcessMIrendererFromThisPointForwardSome & vbCrLf & vbCrLf & My.Resources.TheErrorMessageWas0, Ex.Message))
    End Sub


#End Region

#Region "DrawStageMI"

    ''' <summary>
    ''' Draws stage MI as white text in a chamfered box.
    ''' </summary>
    ''' <param name="s">The string to draw</param>
    ''' <param name="p">The point</param>
    ''' <param name="g">The graphic object</param>
    ''' <param name="f">The font</param>
    ''' <param name="c">The box colour</param>
    Private Sub DrawStageMI(ByVal s As String, ByVal p As PointF, ByVal g As Graphics, ByVal f As Font, ByVal c As Color)
        Try
            Dim oSizeF As SizeF
            Dim oRectF As RectangleF
            Dim iChamfer, iShadowOffset As Integer
            Dim aLines As String()
            Dim dOffsetY As Single

            iChamfer = 5
            iShadowOffset = 3

            'Get the size of the area cover by the text.
            oSizeF = Me.MeasureScaledString(g, s, f)

            'Make rectangle with room for the chamfer on all sides
            oRectF = New RectangleF(p.X - iChamfer, p.Y - iChamfer, oSizeF.Width + 2 * iChamfer, oSizeF.Height + 2 * iChamfer)

            'Draw the shadow slightly offset left and down
            oRectF.X += iShadowOffset
            oRectF.Y += iShadowOffset
            DrawChamferedBox(g, oRectF, iChamfer, mShadowBrush)

            'Draw the box back in its original position
            oRectF.X -= iShadowOffset
            oRectF.Y -= iShadowOffset
            mBoxBrush.Color = c
            DrawChamferedBox(g, oRectF, iChamfer, mBoxBrush, mBlackPen)

            'Draw the text in white, left justified.
            aLines = s.Split(CChar(vbCrLf))
            If aLines.Length > 0 Then

                If Me.DrawText Then
                    Me.DrawScaledString(g, aLines(0), f, mTextBrush, p.X, p.Y)
                    For i As Integer = 1 To aLines.Length - 1
                        Me.DrawScaledString(g, aLines(i), f, mTextBrush, p.X, p.Y + dOffsetY)
                        dOffsetY += oSizeF.Height / aLines.Length
                    Next
                End If
            End If
        Catch ex As Exception
            If Not mbProcessMIException Then
                ShowProcessMIException(ex)
                mbProcessMIException = True
            End If
        End Try
    End Sub


#End Region

#Region "DrawDecisionStageMI"

    ''' <summary>
    ''' Draws decision stage MI in a chamfered box. Two colours are used to fill the
    ''' box according to the percentage of True and False decisions.
    ''' </summary>
    ''' <param name="s">The string to draw</param>
    ''' <param name="x">The x coord</param>
    ''' <param name="y">The y coord</param>
    ''' <param name="g">The graphic object</param>
    ''' <param name="f">The font</param>
    ''' <param name="c1">The primary (True) box colour</param>
    ''' <param name="c2">The secondary (False) box colour</param>
    ''' <param name="p">The percentage of True decisions</param>
    Private Sub DrawDecisionStageMI(ByVal s As String, ByVal x As Single, ByVal y As Single, ByVal g As Graphics, ByVal f As Font, ByVal c1 As Color, ByVal c2 As Color, ByVal p As Double)

        Dim aLines As String()
        Dim dOffsetX, dOffsetY As Single
        Dim oSizeF As SizeF
        Dim iChamfer, iShadowOffset As Integer
        Dim oRectF As RectangleF
        Dim oPath As Drawing2D.GraphicsPath
        Dim oBoxBlend, oTextBlend As Drawing2D.ColorBlend
        Dim aPositions() As Single

        iChamfer = 5
        iShadowOffset = 3

        'Get the size of the area cover by the text.
        oSizeF = Me.MeasureScaledString(g, s, f)

        'Define a rectangle of a slightly bigger size.
        oRectF = New RectangleF(x - iChamfer, y - iChamfer, oSizeF.Width + (2 * iChamfer), oSizeF.Height + (2 * iChamfer))

        oPath = New Drawing2D.GraphicsPath
        oPath.AddRectangle(oRectF)

        'Put the centre of the PathGradientBrush in the middle on 
        'the RHS of the area and scale 100% in the Y direction.
        Using oBoxBrush = New Drawing2D.PathGradientBrush(oPath) With {
            .CenterPoint = New PointF(x + oSizeF.Width + iChamfer, y + oSizeF.Height / 2),
            .FocusScales = New PointF(0.0F, 1.0F)
        }
            'Make a clone for the text brush
            Using oTextBrush = CType(oBoxBrush.Clone, Drawing2D.PathGradientBrush)

                'Define the color blend for the box and use the inverse blend for the text.
                oBoxBlend = New Drawing2D.ColorBlend
                oBoxBlend.Colors = New Color() {c1, c1, c2, c2}
                oTextBlend = New Drawing2D.ColorBlend
                oTextBlend.Colors = New Color() {Color.White, Color.White, c1, c1}

                'Base the blend position on the percentage parameter.
                Select Case p
                    Case 0
                        'Only secondary colour.
                        aPositions = New Single() {0, 0, 0, 1}
                    Case 1
                        'Only primary colour.
                        aPositions = New Single() {0, 1, 1, 1}
                    Case Else
                        'Reduce the width of the color gradient to just either side of the percentage p.
                        aPositions = New Single() {0, CSng(p), CSng(p), 1}
                End Select

                oBoxBlend.Positions = aPositions
                oBoxBrush.InterpolationColors = oBoxBlend
                oTextBlend.Positions = aPositions
                oTextBrush.InterpolationColors = oTextBlend

                'Draw the shadow slightly offset left and down
                oRectF.X += iShadowOffset
                oRectF.Y += iShadowOffset
                DrawChamferedBox(g, oRectF, iChamfer, mShadowBrush)

                'Draw the box back in its original position
                oRectF.X -= iShadowOffset
                oRectF.Y -= iShadowOffset
                DrawChamferedBox(g, oRectF, iChamfer, oBoxBrush, mBlackPen)

                'Draw the text in white, centre justified.
                aLines = s.Split(CChar(vbCrLf))
                If aLines.Length > 0 Then

                    If Me.DrawText Then
                dOffsetX = (oSizeF.Width - Me.MeasureScaledString(g, aLines(0), f).Width) / 2
                Me.DrawScaledString(g, aLines(0), f, oTextBrush, x + dOffsetX, y)
                        For i As Integer = 1 To aLines.Length - 1
                    dOffsetX = (oSizeF.Width - Me.MeasureScaledString(g, aLines(i), f).Width) / 2
                    Me.DrawScaledString(g, aLines(i), f, oTextBrush, x + dOffsetX, y + dOffsetY)
                            dOffsetY += oSizeF.Height / aLines.Length
                        Next
                    End If
                End If
            End Using
        End Using
    End Sub

#End Region

#Region "GetStageMIPosition"

    ''' <summary>
    ''' Gets a point near the middle of the link between two stages.
    ''' </summary>
    ''' <param name="oStage">The stage displaying MI</param>
    ''' <param name="oNextStage">The next stage after the MI stage</param>
    ''' <param name="oTextSizeF">The size covered by the MI data</param>
    ''' <returns></returns>
    Private Function GetStageMIPosition(ByVal oStage As clsProcessStage, ByVal oNextStage As clsProcessStage, ByVal oTextSizeF As SizeF) As PointF
        Return GetStageMIPosition(New PointF(oStage.GetDisplayX, oStage.GetDisplayY), New PointF(oNextStage.GetDisplayX, oNextStage.GetDisplayY), oTextSizeF)
    End Function

    Private Function GetStageMIPosition(ByVal pStage As PointF, ByVal oNextStage As clsProcessStage, ByVal oTextSizeF As SizeF) As PointF
        Return GetStageMIPosition(pStage, New PointF(oNextStage.GetDisplayX, oNextStage.GetDisplayY), oTextSizeF)
    End Function

    Private Function GetStageMIPosition(ByVal pStage As PointF, ByVal pNextStage As PointF, ByVal oTextSizeF As SizeF) As PointF

        Dim dMidX, dMidY, dNormalX, dNormalY, dJustifyX, dJustifyY As Single
        Dim dMidLength, dNormalScale, dNormalLength As Single
        Dim pOffset As PointF

        dJustifyX = 0
        dJustifyY = 0

        'Get the distances to the mid point.
        dMidX = (pNextStage.X - pStage.X) / 2
        dMidY = (pNextStage.Y - pStage.Y) / 2
        dMidLength = CSng(Math.Sqrt(dMidX * dMidX + dMidY * dMidY))

        'Find the scale to change the mid point length to the default offset
        dNormalLength = 15
        dNormalScale = dNormalLength / dMidLength

        If dMidX > 0 And dMidY > 0 Then
            'The line angle is between 0 and 90 degrees.

            If dMidX >= dMidY Then
                'The line angle is between 0 and 45 degrees.
                'Include the height of the text itself in the offset.
                dNormalScale = (dNormalLength + oTextSizeF.Height) / dMidLength
            Else
                'The line angle is between 45 and 90 degrees.
            End If

        Else

            'The line angle is between 90 and 360 degrees.
            If dMidY = 0 Then
                'The line is horizontal. Centre justify text.
                dJustifyX = -oTextSizeF.Width / 2
                If dMidX >= 0 Then
                    'Include the height of the text itself in the offset.
                    dNormalScale = (dNormalLength + oTextSizeF.Height) / dMidLength
                End If

            Else

                If dMidY < 0 Then
                    'The line angle is between 180 and 360 degrees. 
                    'Right justify text.
                    dJustifyX = -oTextSizeF.Width

                    If dMidX > 0 And dMidX > Math.Abs(dMidY) Then
                        'The line angle is between 315 and 360 degrees. 
                        'Include the height of the text itself in the normal offset.
                        dNormalScale = (dNormalLength + oTextSizeF.Height) / dMidLength
                    End If

                Else
                    'The line angle is between 90 and 180 degrees.
                End If

                If dMidX = 0 Then
                    'The line is vertical. Centre justify text.
                    dJustifyY = -oTextSizeF.Height / 2
                End If

            End If

        End If

        'Get the relative coords of the offset point
        dNormalX = dMidY * dNormalScale
        dNormalY = -dMidX * dNormalScale

        'Return the new point
        pOffset = New PointF
        pOffset.X = pStage.X + dMidX + dNormalX + dJustifyX
        pOffset.Y = pStage.Y + dMidY + dNormalY + dJustifyY
        Return pOffset

    End Function

#End Region

#Region "DrawChamferedBox"
    Public Sub DrawChamferedBox(ByVal g As Graphics, ByVal r As RectangleF, ByVal c As Integer, ByVal b As Brush, Optional ByVal p As Pen = Nothing)
        Try
            Dim oGraphicsPath As New Drawing2D.GraphicsPath

            Dim dia As Integer = c + c

            oGraphicsPath.AddLine(r.X + c, r.Y, r.X + r.Width - dia, r.Y)
            oGraphicsPath.AddArc(r.X + r.Width - dia, r.Y, dia, dia, 270, 90)
            oGraphicsPath.AddLine(r.X + r.Width, r.Y + c, r.X + r.Width, r.Y + r.Height - dia)
            oGraphicsPath.AddArc(r.X + r.Width - dia, r.Y + r.Height - dia, dia, c * 2, 0, 90)
            oGraphicsPath.AddLine(r.X + r.Width - dia, r.Y + r.Height, r.X + c, r.Y + r.Height)
            oGraphicsPath.AddArc(r.X, r.Y + r.Height - dia, dia, dia, 90, 90)
            oGraphicsPath.AddLine(r.X, r.Y + r.Height - dia, r.X, r.Y + c)
            oGraphicsPath.AddArc(r.X, r.Y, dia, dia, 180, 90)
            oGraphicsPath.CloseFigure()

            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.FillPath(b, oGraphicsPath)
            If Not p Is Nothing Then
                g.DrawPath(p, oGraphicsPath)
            End If
            g.SmoothingMode = Drawing2D.SmoothingMode.Default
            oGraphicsPath.Dispose()
        Catch ex As Exception
            If Not mbProcessMIException Then
                ShowProcessMIException(ex)
                mbProcessMIException = True
            End If
        End Try
    End Sub

#End Region

    ''' <summary>
    ''' Draw the current view into the given graphics object.
    ''' </summary>
    ''' <param name="objGraphics">The graphics object to draw
    ''' to.</param>
    ''' <param name="objProcess">The process to display</param>
    ''' <param name="bEditMode">True if the view is editable.</param>
    ''' <param name="bShowDrag">If True, any dragging that is
    ''' underway will be highlighted.</param>
    Public Sub UpdateView(ByVal objGraphics As Graphics, ByVal ViewableRectangle As Rectangle, ByVal objProcess As clsProcess, ByVal bEditMode As Boolean, ByVal bShowDrag As Boolean, Optional ByVal PreserveExistingLayout As Boolean = False)

        Try
            Me.mObjectMap = New clsPixmap

            If objProcess Is Nothing Then
                Exit Sub
            End If

            Dim objStage As clsProcessStage = Nothing
            Dim sTool As ctlProcessViewer.StudioTool = mobjparent.GetCurrentTool()

            'Clear background, where appropriate
            If Not PreserveExistingLayout Then
                objGraphics.Clear(Me.BackgroundBrush.Color)
            End If

            'Display grid if enabled...
            If bEditMode Then
                Dim sngGridSize As Single
                Dim bGridDisplay As Boolean, bGridSnap As Boolean
                mobjparent.GetGridInfo(bGridDisplay, bGridSnap, sngGridSize)
                If Me.DrawGridLines AndAlso bGridDisplay Then

                    Dim VisibleBounds As RectangleF = Me.mCoordinateTranslator.TranslateScreenRectToWorldRect(ViewableRectangle, ViewableRectangle.Size)
                    VisibleBounds.X = VisibleBounds.X - (VisibleBounds.X Mod sngGridSize) - sngGridSize
                    VisibleBounds.Y = VisibleBounds.Y - (VisibleBounds.Y Mod sngGridSize) - sngGridSize
                    VisibleBounds.Width += 2 * sngGridSize
                    VisibleBounds.Height += 2 * sngGridSize

                    Dim Temp As Single = VisibleBounds.Y
                    While (Temp <= VisibleBounds.Bottom)
                        Temp += sngGridSize
                        DrawGridLine(New PointF(VisibleBounds.X, Temp), New PointF(VisibleBounds.Right, Temp), objGraphics, ViewableRectangle)
                    End While

                    Temp = VisibleBounds.X
                    While (Temp <= VisibleBounds.Right)
                        Temp += sngGridSize
                        DrawGridLine(New PointF(Temp, VisibleBounds.Y), New PointF(Temp, VisibleBounds.Bottom), objGraphics, ViewableRectangle)
                    End While

                End If
            End If

            'Draw rubber band if we're dragging one...
            If mobjparent.IsMouseDragging AndAlso (mobjparent.GetMouseDragStage Is Nothing) AndAlso (mobjparent.ClipboardProcess Is Nothing) Then
                If Math.Abs(mobjparent.GetMouseDownX - mobjparent.GetMouseX) > mobjparent.GetMinRubberBandSize Or Math.Abs(mobjparent.GetMouseDownY - mobjparent.GetMouseY) > mobjparent.GetMinRubberBandSize Then
                    If mobjparent.GetCurrentTool = ctlProcessViewer.StudioTool.Block Then
                        'Draw a block if we are dragging one.
                        Dim objDownLoc As PointF = mobjparent.GetMouseDownLocation
                        Dim objLoc As PointF = mobjparent.GetMouseLocation
                        DrawBlock(objGraphics, New RectangleF(objDownLoc, New SizeF(objLoc.X - objDownLoc.X, objLoc.Y - objDownLoc.Y)), ViewableRectangle, Nothing, clsProcess.DefaultBlockColour)
                    Else
                        DrawRubberBand(objGraphics, ViewableRectangle, mobjparent.GetMouseDownLocation, mobjparent.GetMouseLocation)
                    End If
                End If
            End If

            'Display link being created if there is one...
            If mobjparent.GetCurrentTool = ctlProcessViewer.StudioTool.Link Then
                If mobjparent.LinkSource IsNot Nothing Then
                    DrawLink(mobjparent.LinkSource, Nothing, objGraphics, ViewableRectangle, True, "", mobjparent.GetMouseLocation)
                End If
            End If

            'Draw the blocks under the stages.
            Dim iStage As Integer
            iStage = objProcess.GetNumStages()
            Do While iStage > 0
                iStage = iStage - 1
                objStage = objProcess.GetStage(iStage)
                If objStage.GetSubSheetID().CompareTo(objProcess.GetActiveSubSheet()) = 0 Then
                    If objStage.StageType = StageTypes.Block Then
                        DrawBlock(objGraphics, New RectangleF(objStage.GetDisplayX, objStage.GetDisplayY, objStage.GetDisplayWidth, objStage.GetDisplayHeight), ViewableRectangle, objStage.GetName, objStage.Color)
                    End If
                End If
            Loop

            'Display all connections between stages...
            iStage = objProcess.GetNumStages()
            Do While iStage > 0
                iStage = iStage - 1
                objStage = objProcess.GetStage(iStage)
                'Check the stage is on the subsheet we are viewing before
                'we draw any links...
                If objStage.GetSubSheetID().CompareTo(objProcess.GetActiveSubSheet()) = 0 Then
                    Select Case objStage.StageType
                        Case StageTypes.Decision
                            Dim objDecision As clsDecisionStage = CType(objStage, clsDecisionStage)
                            Dim OnTrueStage As clsProcessStage = objProcess.GetStage(objDecision.OnTrue)
                            If OnTrueStage IsNot Nothing Then
                                DrawLink(objStage, OnTrueStage, objGraphics, ViewableRectangle, objProcess.IsLinkSelected(objStage.GetStageID(), "True"), My.Resources.YesLabel, PointF.Empty)
                            End If
                            Dim OnFalseStage As clsProcessStage = objProcess.GetStage(objDecision.OnFalse)
                            If OnFalseStage IsNot Nothing Then
                                DrawLink(objStage, OnFalseStage, objGraphics, ViewableRectangle, objProcess.IsLinkSelected(objStage.GetStageID(), "False"), My.Resources.NoLabel, PointF.Empty)
                            End If
                        Case StageTypes.ChoiceStart, StageTypes.WaitStart
                            Dim objChoiceStart As clsChoiceStartStage = CType(objStage, clsChoiceStartStage)
                            Dim objChoiceEnd As clsChoiceEndStage = objProcess.GetChoiceEnd(objChoiceStart)

                            'Draw line from choice start to choice end
                            Me.DrawLink(objChoiceStart, objChoiceEnd, objGraphics, ViewableRectangle, False, String.Empty, PointF.Empty, True)


                            'Draw each choice link, and each choice node in turn
                            For Each objchoice As clsChoice In objChoiceStart.Choices


                                Dim LinkIsSelected As Boolean = objProcess.IsChoiceNodeLinkSelected(objChoiceStart.GetStageID, objChoiceStart.Choices.IndexOf(objchoice))
                                'Draw link from node to its destination, if any
                                Dim onTrue As clsProcessStage = objProcess.GetStage(objchoice.LinkTo)
                                If Not onTrue Is Nothing Then
                                    DrawLink(objchoice.DisplayBounds, onTrue.GetDisplayBounds, objGraphics, ViewableRectangle, LinkIsSelected, objchoice.Name, StageLinkMode.Normal)
                                End If


                                Dim NodeIsSelected As Boolean = objProcess.IsChoiceNodeSelected(objStage.GetStageID, objChoiceStart.Choices.IndexOf(objchoice))
                                If TypeOf objStage Is clsWaitStartStage Then
                                    'Draw wait blobs
                                    DrawObject(Me.GetStageShape(StageTypes.Anchor, objChoiceStart.DisplayMode), objStage, objchoice.Location, objchoice.DisplayBounds.Size, objGraphics, ViewableRectangle, "", NodeIsSelected, False, Nothing, "", Nothing, Nothing)
                                Else
                                    'Draw choice diamonds
                                    DrawObject(Me.GetStageShape(StageTypes.Decision, objChoiceStart.DisplayMode), objStage, objchoice.Location, objchoice.DisplayBounds.Size, objGraphics, ViewableRectangle, "", NodeIsSelected, False, Nothing, "", Nothing, Nothing)
                                End If

                            Next

                        Case Else
                            Dim Linkable As ILinkable = TryCast(objStage, ILinkable)
                            If Linkable IsNot Nothing Then
                                Dim Destination As clsProcessStage = objProcess.GetStage(Linkable.OnSuccess)
                                If Destination IsNot Nothing Then
                                    DrawLink(Linkable, Destination, objGraphics, ViewableRectangle, objProcess.IsLinkSelected(objStage.GetStageID(), "Success"), "", PointF.Empty)
                                End If
                            End If
                    End Select
                End If
            Loop

            'Display all stages...
            Dim objShape As Image
            iStage = objProcess.GetNumStages()
            Do While iStage > 0
                iStage = iStage - 1
                objStage = objProcess.GetStage(iStage)

                'Only draw stages on the current subsheet...
                If objStage.GetSubSheetID().CompareTo(objProcess.GetActiveSubSheet()) = 0 Then
                    If objStage.StageType = StageTypes.SubSheetInfo OrElse objStage.StageType = StageTypes.ProcessInfo Then
                        DrawProcessInfo(objGraphics, ViewableRectangle, objStage, objProcess)
                    ElseIf objStage.StageType = StageTypes.Block Then
                        'All the blocks should have been drawn by now, all that is left to do is draw selection handles
                        If objStage.IsSelected() Then
                            Dim objBlockRect As Rectangle = Me.mCoordinateTranslator.TranslateWorldRectToScreenRect(New RectangleF(objStage.GetDisplayX, objStage.GetDisplayY, objStage.GetDisplayWidth, objStage.GetDisplayHeight), ViewableRectangle.Size)
                            Me.DrawSelectionHandles(objGraphics, objBlockRect)
                        End If
                    ElseIf objStage.StageType = StageTypes.Skill Then
                        Dim mode = If(objStage.GetStageID().CompareTo(objProcess.RunStageID()) = 0,
                                        StageShowMode.CurrentDebug,
                                        objStage.DisplayMode)

                        DrawSkill(CType(objStage, clsSkillStage), objGraphics, ViewableRectangle, mode)
                    Else
                        If objStage.GetStageID().CompareTo(objProcess.RunStageID()) = 0 Then
                            objStage.DisplayMode = StageShowMode.CurrentDebug
                            objShape = GetStageShape(objStage.StageType, objStage.DisplayMode)
                            objStage.DisplayMode = StageShowMode.Normal
                        Else
                            objShape = GetStageShape(objStage.StageType, objStage.DisplayMode)
                        End If

                        'Draw the object itself...
                        DrawObject(objShape, objStage, objGraphics, ViewableRectangle)
                    End If
                End If
            Loop


            If mobjparent.ToolDragging Then
                Dim dragStage As clsProcessStage = mobjparent.GetMouseDragStage
                If dragStage IsNot Nothing Then
                    If dragStage.StageType = StageTypes.Skill Then
                        DrawSkill(CType(dragStage, clsSkillStage), objGraphics, ViewableRectangle, StageShowMode.Normal)
                    Else
                        Dim dragShape As Image = GetStageShape(dragStage.StageType, dragStage.DisplayMode)
                        DrawObject(dragShape, dragStage, objGraphics, ViewableRectangle)
                    End If
                End If
            End If

            'DrawToolTip if active
            If mbToolTipActive Then
                If mbToolTipIn Then
                    DrawToolTip(objGraphics)
                End If
            End If

            'Show calculation zoom if necessary.
            If objProcess.RunState = ProcessRunState.Paused _
             Or objProcess.RunState = ProcessRunState.Running Then

                If Not frmCalculationZoom.Disabled(mobjparent) Then
                    'Calculation zoom is enabled
                    objStage = objProcess.GetStage(objProcess.RunStageID)
                    If objStage.StageType = StageTypes.Calculation Then
                        'The debug stage is a calculation

                        If Not mobjparent.GetCalculationZoom Is Nothing _
                         AndAlso Not mobjparent.GetCalculationZoom.ProcessStage.GetStageID.Equals(objStage.GetStageID) _
                         AndAlso Not mobjparent.GetCalculationZoom.IsClosing Then
                            'A calculation form for another stage exists and is not in the 
                            'process of closing, so close it down.
                            mobjparent.CloseCalculationZoom()
                        End If

                        If mobjparent.GetCalculationZoom Is Nothing _
                         OrElse Not mobjparent.GetCalculationZoom.ProcessStage.GetStageID.Equals(objStage.GetStageID) Then
                            'Make a new form for this stage and display it.
                            mobjparent.ShowCalculationZoomForStage(objStage)
                        End If

                    ElseIf Not mobjparent.GetCalculationZoom Is Nothing _
                     AndAlso Not mobjparent.GetCalculationZoom.IsClosing Then
                        'The highlighted stage is not a calculation but a calculation form is on display 
                        'from a previous stage. The form is not in the process of closing, so close it down.
                        mobjparent.CloseCalculationZoom()
                    End If


                End If

            End If


            'This section is to draw a loop iteration tooltip ("Iteration x of y") if necessary.
            Dim pIteration As Point
            Dim sIteration As String = Nothing

            If objProcess.RunState = ProcessRunState.Paused OrElse objProcess.RunState = ProcessRunState.Running Then

                If Not objStage Is Nothing Then

                    Select Case objStage.StageType
                        Case StageTypes.LoopStart
                            Dim objLoop As clsLoopStartStage = CType(objStage, clsLoopStartStage)
                            'Get the id of the stage after the LoopStart.
                            bDrawLoopIterationLabel = True
                            gStageAfterLoopStart = CType(objStage, clsLinkableStage).OnSuccess
                            'NB There could be more than one collection with the same name. Dodgy!
                            Dim loopstage As String = objLoop.LoopData
                            If Not String.IsNullOrEmpty(loopstage) Then
                                objLoopCollectionStage = CType(objLoop.Process.GetStage(loopstage), clsCollectionStage)
                            End If
                        Case StageTypes.LoopEnd
                            'Get the id of the stage after the LoopEnd.
                            mgStageAfterLoopEnd = CType(objStage, clsLinkableStage).OnSuccess
                    End Select

                    Dim ViewableRectHalfSize As New SizeF(ViewableRectangle.Width / 2.0F, ViewableRectangle.Height / 2.0F)

                    If objStage.GetStageID.Equals(gStageAfterLoopStart) Then
                        If (Not objLoopCollectionStage Is Nothing) AndAlso (objLoopCollectionStage.CollectionIsInIteration) Then
                            'This is the first stage inside the loop. The loop collection 
                            'stage is known, so the iteration label details can be prepared.

                            sIteration = String.Format(My.Resources.Iteration0of1Label, CStr(objLoopCollectionStage.CollectionCurrentIterationIndex + 1), CStr(objLoopCollectionStage.CollectionRowCount))
                            sIteration &= vbCrLf & My.Resources.CollectionLabel & objLoopCollectionStage.GetName
                            Dim Temp As Point = Me.mCoordinateTranslator.TranslateWorldPointToScreenPoint(objStage.Location, Size.Empty)
                            pIteration = New Point(CInt(Temp.X + ViewableRectHalfSize.Width + 0.6 * objStage.GetDisplayWidth), CInt(Temp.Y + ViewableRectHalfSize.Height))
                        End If

                    ElseIf objStage.GetStageID.Equals(mgStageAfterLoopEnd) Then
                        'This is the first stage after the loop has finished, so the label is no longer required.
                        bDrawLoopIterationLabel = False
                        gStageAfterLoopStart = Guid.Empty
                        mgStageAfterLoopEnd = Guid.Empty
                    End If

                End If

                If bDrawLoopIterationLabel Then
                    clsUserInterfaceUtils.GraphicsUtils.DrawToolTip(objGraphics, sIteration, pIteration)
                End If

            End If
        Catch ex As Exception
            If Not mbShownDisplayException Then
                mbShownDisplayException = True
                UserMessage.Show(String.Format(My.Resources.ExceptionWhenGeneratingDisplay0, ex.Message))
            End If
        End Try

    End Sub


    ''' <summary>
    ''' Get the shape (emf) used to display the specified stage type in the current mode.
    ''' </summary>
    ''' <param name="stagetype">The stage type</param>
    ''' <param name="mode">The display mode in which the stage is to be drawn.
    ''' This affects the colour chosen.</param>
    ''' <returns>Returns the image to be drawn.</returns>
    Public Function GetStageShape(ByVal stagetype As StageTypes, ByVal mode As StageShowMode) As Image
        Return GetShape(stagetype).GetImage(mode)
    End Function

#End Region

    ''' <summary>
    ''' Gets the item at the specified point, if there is any.
    ''' </summary>
    ''' <param name="x">The x coordinate of interest, in world coordinates.</param>
    ''' <param name="y">The y coordinate of interest, in world coordinates.</param>
    ''' <returns>The result item found at the specified point, if any.</returns>
    Public Function GetItemAt(ByVal x As Single, ByVal y As Single) As clsPixmap.PixmapItem
        If mObjectMap Is Nothing Then
            Return Nothing
        Else
            Return Me.mObjectMap.GetItemForPoint(CInt(x), CInt(y))
        End If
    End Function

    Public Function GetItemAt(ByVal P As PointF) As clsPixmap.PixmapItem
        Return GetItemAt(P.X, P.Y)
    End Function

    ''' <summary>
    ''' Adds an item to the pixmap.
    ''' </summary>
    ''' <param name="R">Bounding rectangle of pixmap item, in world coordinates.</param>
    ''' <param name="Item">Item.</param>
    Private Sub AddPixmapItem(ByVal R As RectangleF, ByVal Item As Object)
        'this should never be
        Debug.Assert(Not Me.mObjectMap Is Nothing)

        If Not Me.mObjectMap Is Nothing Then
            Me.mObjectMap.ItemCollection.Add(New clsPixmap.PixmapItem(Rectangle.Ceiling(R), Item))
        End If
    End Sub

    Private Sub DrawScaledString(graphics As Graphics, s As String, font As Font, brush As Brush, layoutRectangle As RectangleF, format As StringFormat)
        Using scaledFont = GetScaledFont(font)
            graphics.DrawString(s, scaledFont, brush, layoutRectangle, format)
        End Using
    End Sub

    Private Sub DrawScaledString(graphics As Graphics, s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
        Using scaledFont = GetScaledFont(font)
            graphics.DrawString(s, scaledFont, brush, layoutRectangle)
        End Using
    End Sub

    Private Sub DrawScaledString(graphics As Graphics, s As String, font As Font, brush As Brush, x As Single, y As Single)
        Using scaledFont = GetScaledFont(font)
            graphics.DrawString(s, scaledFont, brush, x, y)
        End Using
    End Sub

    Private Function GetScaledFont(font As Font) As Font
        Return New Font(font.Name, font.Size * (Me.CoordinateTranslator.ZoomLevel / clsProcess.ScaleFactor), font.Style, font.Unit)
    End Function

    Private Function MeasureScaledString(graphics As Graphics, text As String, font As Font) As SizeF
        Using scaledFont = GetScaledFont(font)
            Return graphics.MeasureString(text, scaledFont)
        End Using
    End Function

    ''' <summary>
    ''' Facilitates the translation of coordinates to/from
    ''' world coordinates and screen coordinates.
    ''' </summary>
    Public Class clsCoordinateTranslator
        Public ZoomLevel As Single

        ''' <summary>
        ''' The location of the viewport, in world coordinates.
        ''' </summary>
        Public CameraLocation As PointF

        ''' <summary>
        ''' The offset to apply to the coordinate system
        ''' origin, in world coordinates.
        ''' </summary>
        ''' <remarks>Eg if this is set to (15,15) then
        ''' the point (15,15) is regarded as the origin, 
        ''' rather than the point (0,0).</remarks>
        Public OriginOffset As SizeF

        Public Sub New(ByVal CameraLocation As PointF, ByVal ZoomLevel As Single)
            Me.CameraLocation = CameraLocation
            Me.ZoomLevel = ZoomLevel
        End Sub

        Public Sub New(ByVal Process As clsProcess)
            Me.New(Process.GetCameraLocation, Process.Zoom)
        End Sub

        Public Function TranslateWorldPointToScreenPoint(ByVal WorldPoint As PointF, ByVal ScreenSize As Size) As Point
            WorldPoint = PointF.Subtract(WorldPoint, OriginOffset)
            WorldPoint = PointF.Subtract(WorldPoint, New SizeF(CameraLocation.X, CameraLocation.Y))
            WorldPoint = ScaleWorldPointToScreenPoint(WorldPoint)
            Return Point.Truncate(PointF.Add(WorldPoint, New Size(ScreenSize.Width \ 2, ScreenSize.Height \ 2)))
        End Function

        Public Function TranslateScreenPointToWorldPoint(ByVal ScreenPoint As Point, ByVal ScreenSize As Size) As PointF
            ScreenPoint = Point.Subtract(ScreenPoint, New Size(ScreenSize.Width \ 2, ScreenSize.Height \ 2))
            Dim WorldPoint As PointF = ScaleScreenPointToWorldPoint(ScreenPoint)
            WorldPoint = PointF.Add(WorldPoint, New SizeF(CameraLocation.X, CameraLocation.Y))
            Return PointF.Add(WorldPoint, OriginOffset)
        End Function

        Private Function ScaleWorldPointToScreenPoint(ByVal WorldPoint As PointF) As Point
            Return Point.Truncate(New PointF(WorldPoint.X * ZoomLevel, WorldPoint.Y * ZoomLevel))
        End Function

        Private Function ScaleScreenPointToWorldPoint(ByVal Point As Point) As PointF
            Return New PointF(Point.X / ZoomLevel, Point.Y / ZoomLevel)
        End Function

        Public Function TranslateWorldRectToScreenRect(ByVal WorldRect As RectangleF, ByVal ScreenSize As Size) As Rectangle
            Dim TopLeft As Point = Me.TranslateWorldPointToScreenPoint(WorldRect.Location, ScreenSize)
            Dim BottomRight As Point = Me.TranslateWorldPointToScreenPoint(New PointF(WorldRect.Right, WorldRect.Bottom), ScreenSize)
            Return New Rectangle(TopLeft, New Size(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y))
        End Function

        Public Function TranslateScreenRectToWorldRect(ByVal ScreenRect As Rectangle, ByVal ScreenSize As Size) As RectangleF
            Dim TopLeft As PointF = Me.TranslateScreenPointToWorldPoint(ScreenRect.Location, ScreenSize)
            Dim BottomRight As PointF = Me.TranslateScreenPointToWorldPoint(New Point(ScreenRect.Right, ScreenRect.Bottom), ScreenSize)
            Return New RectangleF(TopLeft, New SizeF(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y))
        End Function
    End Class

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)

        If disposing Then
            'Fonts, pens and brushes...
            mGDICache.Dispose()
            mFont.Dispose()
            mLinkPen.Dispose()
            mGridPen.Dispose()
            mBlackPen.Dispose()
            mStagePen.Dispose()
            mSelectedItemBrush.Dispose()
            mBackgroundBrush.Dispose()
            mBoxBrush.Dispose()
            mTextBrush.Dispose()
            mShadowBrush.Dispose()

            'Metafile objects for drawing with...
            Try
                For Each shp As clsShape In mShapes.Values
                    shp.Dispose()
                Next

                mBreakpointImage.Dispose()
                mPublishedIcon.Dispose()
                mConditionalBreakpointIcon.Dispose()
                mSingleRowCollectionIcon.Dispose()

            Catch nre As NullReferenceException
                ' Can't do much about it - the graphics weren't loaded correctly,
                ' so they're not there...

            End Try
        End If

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

End Class
