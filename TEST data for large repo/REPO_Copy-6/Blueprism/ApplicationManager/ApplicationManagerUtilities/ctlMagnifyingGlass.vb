Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Public Class ctlMagnifyingGlass

#Region " Private Declarations "
    Private _UpdateTimer As System.Windows.Forms.Timer = New System.Windows.Forms.Timer
    Private _PixelSize As Integer = 5
    Private _ShowPixel As Boolean = True
    Private _ShowPosition As Boolean = False
    Private _PosFormat As String = "[#xg, #yg], (#xl, #yl)"
    Private _LastPosition As Point = Point.Empty
    Private _PosAlign As ContentAlignment = ContentAlignment.TopLeft
#End Region

#Region " Public Properties "
    ''' <summary>
    ''' Set or get the magnifying ratio. For example 4 = 4x magnification
    ''' </summary>
    ''' <value>The magnification to use</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Description("Magnifying ratio (calculate PixelRange*PixelSize*2+PixelSize for the final control size, min. 3)")> _
    Public Property PixelSize() As Integer
        Get
            Return _PixelSize
        End Get
        Set(ByVal value As Integer)
            Dim temp As Integer = value
            If temp < 3 Then
                temp = 3
            End If
            If CType(temp, Double) / 2 = CType(Math.Floor(CType(temp, Double) / 2), Double) Then
                System.Math.Min(System.Threading.Interlocked.Increment(temp), temp - 1)
            End If
            _PixelSize = temp
        End Set
    End Property

    ''' <summary>
    ''' Is this control enabled?
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Browsable(False)> _
    Public ReadOnly Property IsEnabled() As Boolean
        Get
            Return Visible AndAlso Enabled AndAlso Not DesignMode
        End Get
    End Property


    ''' <summary>
    ''' Set to true of you want to see the current (active) pixal in the
    ''' middle of the magnification image.  Set to false if you don't want
    ''' to see the active pixel.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Description("Get/set if the active pixel should be shown")> _
    Public Property ShowPixel() As Boolean
        Get
            Return _ShowPixel
        End Get
        Set(ByVal value As Boolean)
            _ShowPixel = value
            Invalidate()
        End Set
    End Property

    <Description("Get/set if the current cursor position should be shown")> _
    Public Property ShowPosition() As Boolean
        Get
            Return _ShowPosition
        End Get
        Set(ByVal value As Boolean)
            _ShowPosition = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' If showposition is true than a box will be displayed showing the current pixel position
    ''' relative to the desktop (x,y).  Use this property to get or set the position of this box
    ''' on the control
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Description("Get/set the align of the position (choose everything, but not the middle")> _
    Public Property PosAlign() As ContentAlignment
        Get
            Return _PosAlign
        End Get
        Set(ByVal value As ContentAlignment)
            _PosAlign = CType(IIf((Not value.ToString.ToLower.StartsWith("middle")), value, ContentAlignment.TopLeft), ContentAlignment)
        End Set
    End Property

    ''' <summary>
    ''' If showposition is true than a box will be displayed showing the current pixel position
    ''' relative to the desktop (x,y).  Use this property to get or set the string formate of the
    ''' text in this box.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Description("Get/set the position display string format (you have to use #x and #y for the corrdinates values)")> _
    Public Property PosFormat() As String
        Get
            Return _PosFormat
        End Get
        Set(ByVal value As String)
            _PosFormat = CType(IIf((Not (value Is Nothing) AndAlso value <> "" AndAlso value.Contains("#x") AndAlso value.Contains("#y")), value, "#x ; #y"), String)
            Invalidate()
        End Set
    End Property
#End Region

#Region " Timer "
    <Description("Get the timer that updates the display in an interval")> _
    Public ReadOnly Property UpdateTimer() As Timer
        Get
            Return _UpdateTimer
        End Get
    End Property

    ''' <summary>
    ''' If you are using the timer than this routine will refresh the magnifying glass
    ''' image if the mouse has moved
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub _UpdateTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Try
            UpdateTimer.Stop()
            If IsEnabled Then
                If _LastPosition = Cursor.Position Then
                    Return
                End If
                _LastPosition = Cursor.Position
                Invalidate()
            End If
        Finally
            UpdateTimer.Start()
        End Try
    End Sub
#End Region

#Region " Paint "
    Protected Overloads Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        MyBase.OnPaint(e)
        If Not IsEnabled Then
            Return
        End If
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor
        Dim pos As Point = Cursor.Position
        Dim scr As Rectangle = Screen.PrimaryScreen.Bounds
        Dim zeroPoint As Point = New Point(0, 0)
        Dim shot As Rectangle = New Rectangle(zeroPoint, New Size(CInt(Size.Width / PixelSize), CInt(Size.Height / PixelSize)))

        'defaultlocation is top left position of rectangle on screen to capture/screenshot
        Dim defaultlocation As Point = New Point(CInt(pos.X - ((Size.Width / PixelSize) / 2)), CInt(pos.Y - ((Size.Height / PixelSize) / 2)))
        shot.Location = defaultlocation
        If shot.Location.X < 0 Then
            shot.Size = New Size(shot.Size.Width + shot.Location.X, shot.Size.Height)
            shot.Location = New Point(0, shot.Location.Y)
        Else
            If shot.Location.X > scr.Width Then
                shot.Size = New Size(shot.Location.X - scr.Width, shot.Size.Height)
            End If
        End If
        If shot.Location.Y < 0 Then
            shot.Size = New Size(shot.Size.Width, shot.Size.Height + shot.Location.Y)
            shot.Location = New Point(shot.Location.X, 0)
        Else
            If shot.Location.Y > scr.Height Then
                shot.Size = New Size(shot.Size.Width, shot.Location.Y - scr.Height)
            End If
        End If
        Dim screenShot As Bitmap = New Bitmap(shot.Width, shot.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        ' Using 
        Dim g As Graphics = Graphics.FromImage(screenShot)
        Try
            g.CopyFromScreen(shot.Location, zeroPoint, shot.Size)

        Finally
            CType(g, IDisposable).Dispose()
        End Try

        Dim display As Rectangle = New Rectangle(zeroPoint, Size)
        Dim displaySize As Size = New Size(shot.Width * PixelSize, shot.Height * PixelSize)
        If defaultlocation.X < 0 OrElse defaultlocation.X > scr.Width Then
            If defaultlocation.X < 0 Then
                display.Location = New Point(display.Width - displaySize.Width, display.Location.Y)
            End If
            display.Size = New Size(displaySize.Width, display.Size.Height)
        End If
        If defaultlocation.Y < 0 OrElse defaultlocation.Y > scr.Height Then
            If defaultlocation.Y < 0 Then
                display.Location = New Point(display.Location.X, display.Height - displaySize.Height)
            End If
            display.Size = New Size(display.Size.Width, displaySize.Height)
        End If
        If Not (displaySize = Size) Then
            e.Graphics.FillRectangle(New SolidBrush(BackColor), New Rectangle(zeroPoint, Size))
        End If
        e.Graphics.DrawImage(screenShot, display)

        screenShot.Dispose()
        If ShowPixel Then
            'Dim xy As Integer = PixelSize * PixelRange
            Dim x As Integer = CInt((Size.Width) / 2) - (Size.Width Mod PixelSize)
            Dim y As Integer = CInt((Size.Height) / 2) - (Size.Height Mod PixelSize)
            e.Graphics.DrawRectangle(New Pen(New SolidBrush(Color.Black)), New Rectangle(New Point(x, y), New Size(PixelSize, PixelSize)))
            e.Graphics.DrawRectangle(New Pen(New SolidBrush(Color.White)), New Rectangle(New Point(x + 1, y + 1), New Size(PixelSize - 2, PixelSize - 2)))
        End If
        If ShowPosition Then
            Dim posText As String = PosFormat
            posText = posText.Replace("#xg", mGlobalCursorPos.X.ToString)
            posText = posText.Replace("#yg", mGlobalCursorPos.Y.ToString)
            posText = posText.Replace("#xl", mLocalCursorPos.X.ToString)
            posText = posText.Replace("#yl", mLocalCursorPos.Y.ToString)
            Dim textSize As Size = e.Graphics.MeasureString(posText, Font).ToSize
            If textSize.Width + 6 <= Width AndAlso textSize.Height + 6 <= Height Then
                Dim posString As String = PosAlign.ToString.ToLower
                Dim posZero As Point = Point.Empty
                If posString.StartsWith("top") Then
                    posZero = New Point(0, 0)
                Else
                    posZero = New Point(0, Height - textSize.Height)
                End If
                If posString.Contains("center") Then
                    posZero = New Point(CType(Math.Ceiling(CType((Width - textSize.Width), Double) / 2), Integer), posZero.Y)
                Else
                    If posString.Contains("right") Then
                        posZero = New Point(Width - textSize.Width - 6, posZero.Y)
                    End If
                End If
                e.Graphics.FillRectangle(New SolidBrush(Color.Wheat), New Rectangle(posZero, New Size(textSize.Width + 6, textSize.Height + 6)))
                e.Graphics.DrawString(posText, Font, New SolidBrush(ForeColor), New PointF(posZero.X + 3, posZero.Y + 3))
            End If
        End If
    End Sub
#End Region

    ''' <summary>
    ''' The location of the mouse cursor, relative to the screen.
    ''' </summary>
    Private mGlobalCursorPos As Point


    ''' <summary>
    ''' The location of the mouse cursor, relative to its containing window.
    ''' </summary>
    Private mLocalCursorPos As Point


    ''' <summary>
    ''' If you don't want to use the timer than this subroutine can be called instead 
    ''' everytime you want to refresh the mouse zoom image
    ''' </summary>
    ''' <param name="GlobalCursorPos">The location of the mouse cursor, relative to the screen.</param>
    ''' <param name="LocalCursorPos">The location of the mouse cursor, relative to its containing window.</param>
    ''' <remarks></remarks>
    Public Sub RefreshImage(ByVal GlobalCursorPos As Point, ByVal LocalCursorPos As Point)
        mGlobalCursorPos = GlobalCursorPos
        mLocalCursorPos = LocalCursorPos

        If IsEnabled Then
            Invalidate()
        End If
    End Sub

    Public Delegate Sub DisplayUpdatedDelegate(ByVal sender As ctlMagnifyingGlass)



    Protected Overloads Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
        If Not IsEnabled Then
            MyBase.OnPaintBackground(e)
        End If
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'ctlMagnifyingGlass
        '
        Me.Name = "ctlMagnifyingGlass"
        Me.ResumeLayout(False)

    End Sub
End Class
