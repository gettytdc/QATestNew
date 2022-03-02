Imports BluePrism.AutomateProcessCore

Friend Class ctlProcessDiagramPanView
    Inherits PictureBox

    Public Sub New()
        MyBase.New()

        MyBase.DoubleBuffered = True
    End Sub

    Public Sub New(ByVal ProcessViewer As ctlProcessViewer)
        Me.New()
        Me.ProcessViewer = ProcessViewer
    End Sub

    ''' <summary>
    ''' The zoom level used.
    ''' </summary>
    ''' <remarks>This is just the minimum of the ratio
    ''' of localwidth:universewidth and 
    ''' localheight:universeheight.
    ''' 
    ''' Call UpdateLocalViewportBounds to update this 
    ''' member</remarks>
    Private mZoomFactor As Single

    ''' <summary>
    ''' The whole 'world' or 'universe' of the target
    ''' process, expressed in local coordinates.
    ''' </summary>
    Private mLocalUniverseBounds As Rectangle

    ''' <summary>
    ''' Represents the location of the processviewer
    ''' viewport in the local diagram, relative to the
    ''' mLocalUniverseBounds
    ''' </summary>
    Private mLocalViewportBounds As Rectangle

    ''' <summary>
    ''' Private member to store public property ProcessViewer()
    ''' </summary>
    Private WithEvents mProcessViewer As ctlProcessViewer
    ''' <summary>
    ''' The process viewer that this pan view represents,
    ''' must not be null.
    ''' </summary>
    Public Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            mProcessViewer = value
            Me.mDiagramPictureBox = value.pbview
            Me.mRenderer = New clsRenderer(mProcessViewer)
            Me.mRenderer.CoordinateTranslator = New clsRenderer.clsCoordinateTranslator(mProcessViewer.Process)
            Me.mRenderer.DrawText = False
            Me.mRenderer.DrawIcons = False
            Me.mRenderer.DrawSelections = False
            Me.mRenderer.DrawGridLines = False
        End Set
    End Property

    ''' <summary>
    ''' The picturebox on which the target process
    ''' is usually drawn.
    ''' </summary>
    Private WithEvents mDiagramPictureBox As PictureBox

    ''' <summary>
    ''' Indicates whether the mouse is down.
    ''' </summary>
    Private mbMouseDown As Boolean

    ''' <summary>
    ''' The point at which the mouse is depressed.
    ''' </summary>
    Private mMouseDownLocalPoint As Point

    ''' <summary>
    ''' The location of the camera (in world coords)
    ''' at the time the mouse was depressed.
    ''' </summary>
    Private mMouseDownCameraPosition As PointF

    ''' <summary>
    ''' The renderer to use when drawing the process on
    ''' the control.
    ''' </summary>
    Private mRenderer As clsRenderer


    Private Sub ctlProcessDiagramPanView_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If e.Button = System.Windows.Forms.MouseButtons.Left Then
            If Me.IsInViewportBounds(e.Location) Then
                Me.mbMouseDown = True
                Me.mMouseDownLocalPoint = e.Location
                Me.mMouseDownCameraPosition = Me.ProcessViewer.Process.GetCameraLocation
                Me.Capture = True
            End If
        End If
    End Sub

    ''' <summary>
    ''' Determines if the supplied point, (in local
    ''' coordinates relative to the entire control)
    ''' is within the viewport bounds.
    ''' </summary>
    ''' <param name="P">The point to test.</param>
    ''' <returns>True if within the bounds, false
    ''' otherwise.</returns>
    Private Function IsInViewportBounds(ByVal P As Point) As Boolean
        Dim ViewportBounds As Rectangle = Me.mLocalViewportBounds
        ViewportBounds.Location = Point.Add(Me.mLocalViewportBounds.Location, New Size(Me.mLocalUniverseBounds.X, Me.mLocalUniverseBounds.Y))
        Return ViewportBounds.Contains(P)
    End Function

    Private Sub ctlProcessDiagramPanView_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If mbMouseDown Then
            Dim Offset As Size = New Size(e.X - mMouseDownLocalPoint.X, e.Y - mMouseDownLocalPoint.Y)
            Me.mRenderer.CoordinateTranslator.ZoomLevel = Me.mZoomFactor
            Dim WorldOffset As SizeF = Me.mRenderer.CoordinateTranslator.TranslateScreenRectToWorldRect(New Rectangle(Point.Empty, Offset), Me.mLocalUniverseBounds.Size).Size()
            Dim NewCameraLocation As PointF = PointF.Add(Me.mMouseDownCameraPosition, WorldOffset)

            'Restrict new camera location to within process bounds
            Dim WorldExtent As Rectangle
            Me.ProcessViewer.Process.GetExtent(WorldExtent, Me.ProcessViewer.Process.GetActiveSubSheet)
            Dim TargetViewportSize As SizeF = Me.mProcessViewer.Renderer.CoordinateTranslator.TranslateScreenRectToWorldRect(Me.mDiagramPictureBox.ClientRectangle, Me.mDiagramPictureBox.ClientSize).Size
            WorldExtent.Inflate(CInt(-TargetViewportSize.Width / 2), CInt(-TargetViewportSize.Height / 2))
            NewCameraLocation.X = Math.Min(WorldExtent.Right, NewCameraLocation.X)
            NewCameraLocation.X = Math.Max(WorldExtent.Left, NewCameraLocation.X)
            NewCameraLocation.Y = Math.Min(WorldExtent.Bottom, NewCameraLocation.Y)
            NewCameraLocation.Y = Math.Max(WorldExtent.Top, NewCameraLocation.Y)

            Me.ProcessViewer.Process.SetCameraLocation(NewCameraLocation)
            Me.ProcessViewer.InvalidateView()
        Else
            If Me.IsInViewportBounds(e.Location) Then
                Me.Cursor = Cursors.SizeAll
            Else
                Me.Cursor = Cursors.Default
            End If
        End If
    End Sub

    Private Sub ctlProcessDiagramPanView_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        Me.Capture = False
        Me.mbMouseDown = False
        Me.mMouseDownCameraPosition = PointF.Empty
        Me.mMouseDownLocalPoint = Point.Empty
    End Sub

    Private Sub ctlProcessDiagramPanView_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        'Start afresh
        e.Graphics.Clear(Color.White)

        If Me.ProcessViewer IsNot Nothing Then

            'PJW: It would be nice to do the following,
            'but it causes refresh problems
            'Do not draw outside our universe
            'e.Graphics.Clip = New Region(Me.mLocalUniverseBounds)

            'Update our geometry
            UpdateLocalUniverseBounds()
            Me.UpdateLocalViewportBounds()

            'Do the drawing using our own renderer (so that it can be at our own zoom level)
            e.Graphics.TranslateTransform(Me.mLocalUniverseBounds.X, Me.mLocalUniverseBounds.Y)
            mRenderer.UpdateView(e.Graphics, New Rectangle(Point.Empty, mLocalUniverseBounds.Size), ProcessViewer.Process, True, False)

            'Put a border on the entire process bounds
            e.Graphics.TranslateTransform(-mLocalUniverseBounds.X, -mLocalUniverseBounds.Y)
            e.Graphics.DrawRectangle(New Pen(Color.Black, 2), New Rectangle(mLocalUniverseBounds.Location, New Size(mLocalUniverseBounds.Width - 1, mLocalUniverseBounds.Height - 1)))

            'Draw the viewport rectangle, over the top of the outer border
            e.Graphics.TranslateTransform(mLocalUniverseBounds.X, mLocalUniverseBounds.Y)
            e.Graphics.DrawRectangle(New Pen(Color.Red, 2), Me.mLocalViewportBounds)
        End If
    End Sub


    ''' <summary>
    ''' Updates the 
    ''' </summary>
    ''' <remarks>The returned rectangle will be scaled and
    ''' centred to fit in this control, whilst maintaining
    ''' the aspect ratio of the target process 'universe'
    ''' extent.</remarks>
    Private Sub UpdateLocalUniverseBounds()
        Dim TargetProcess As clsProcess = ProcessViewer.Process

        Dim ParentProcessBounds As Rectangle
        TargetProcess.GetExtent(ParentProcessBounds, TargetProcess.GetActiveSubSheet)
        Dim TargetAspectRatio As Double = ParentProcessBounds.Width / ParentProcessBounds.Height
        Dim LocalAspectRatio As Double = Me.ClientRectangle.Width / Me.ClientRectangle.Height
        Dim ViewingRect As Rectangle
        If TargetAspectRatio > LocalAspectRatio Then
            ViewingRect.Size = New Size(Me.ClientRectangle.Width, CInt(Me.ClientRectangle.Width / TargetAspectRatio))
        Else
            ViewingRect.Size = New Size(CInt(Me.ClientRectangle.Height * TargetAspectRatio), Me.ClientRectangle.Height)
        End If
        ViewingRect.Location = New Point((Me.ClientSize.Width - ViewingRect.Width) \ 2, (Me.ClientSize.Height - ViewingRect.Height) \ 2)

        Dim RelativeRatio As SizeF = New SizeF(CSng(ViewingRect.Width / ParentProcessBounds.Width), CSng(ViewingRect.Height / ParentProcessBounds.Height))
        mZoomFactor = Math.Min(RelativeRatio.Width, RelativeRatio.Height)

        mRenderer.CoordinateTranslator.CameraLocation = Point.Add(ParentProcessBounds.Location, New Size(ParentProcessBounds.Width \ 2, ParentProcessBounds.Height \ 2))
        mRenderer.CoordinateTranslator.ZoomLevel = Me.mZoomFactor

        mLocalUniverseBounds = ViewingRect
    End Sub

    ''' <summary>
    ''' Updates the local representation of the viewport.
    ''' </summary>
    ''' <remarks>The member variable mLocalViewportBounds
    ''' will contain a representation (in local coords)
    ''' of the bounds of the viewport. This method assumes
    ''' that the member mLocalUniverseBounds is up to date.</remarks>
    Private Sub UpdateLocalViewportBounds()
        Dim TargetProcess As clsProcess = ProcessViewer.Process

        Dim TargetViewPort As RectangleF = Me.ProcessViewer.Renderer.CoordinateTranslator.TranslateScreenRectToWorldRect(Me.mDiagramPictureBox.ClientRectangle, Me.mDiagramPictureBox.ClientSize)
        TargetViewPort = Me.mRenderer.CoordinateTranslator.TranslateWorldRectToScreenRect(TargetViewPort, Me.mLocalUniverseBounds.Size)

        mLocalViewportBounds = Rectangle.Round(TargetViewPort)
    End Sub

    Private Sub mProcessViewer_Invalidated(ByVal sender As Object, ByVal e As System.Windows.Forms.InvalidateEventArgs) Handles mDiagramPictureBox.Invalidated
        Me.Refresh()
    End Sub

    Private Sub WhenCircumstancesChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged, mDiagramPictureBox.SizeChanged
        Me.Refresh()
    End Sub


    Private Sub ctlProcessDiagramPanView_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
        If Me.Visible Then
            Me.Refresh()
        End If
    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not mRenderer Is Nothing Then
            mRenderer.Dispose()
            mRenderer = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub


End Class
