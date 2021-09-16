Imports System.Drawing

Public Class clsTerminalScreenshotAnalyser


    Private mTerminal As AbstractTerminal
    Private mImage As Bitmap

    Public WriteOnly Property Screenshot() As Bitmap
        Set(ByVal value As Bitmap)
            mImage = value
            AnalyseScreenshot()
        End Set
    End Property


    Public Sub New(ByVal term As AbstractTerminal, ByVal ScreenShot As Bitmap)
        mTerminal = term
        Me.Screenshot = ScreenShot
    End Sub

    Private Sub AnalyseScreenshot()

        Const StripHeight As Integer = 3

        'A sub-image of the screenshot we took
        Dim StripImage As Bitmap

        'A copy of the main image, rotated 90 degrees anticlockwise
        Dim RotatedImage As Bitmap = CType(mImage.Clone, Bitmap)
        RotatedImage.RotateFlip(RotateFlipType.Rotate270FlipNone)

        'A horizontal strip across the centre, of height StripHeight
        Dim HorizStrip As Rectangle = New Rectangle(0, (mImage.Height \ 2) - (StripHeight \ 2), mImage.Width - 1, StripHeight)
        Dim CopyHorizStrip As Rectangle = New Rectangle(0, (RotatedImage.Height \ 2) - (StripHeight \ 2), RotatedImage.Width - 1, StripHeight)

        StripImage = mImage.Clone(HorizStrip, mImage.PixelFormat)
        Me.mLeftMargin = FindEdge(StripImage)

        StripImage.RotateFlip(RotateFlipType.RotateNoneFlipX)
        Me.mRightMargin = FindEdge(StripImage)

        'StripImage = New Bitmap(MiddleVerticalStrip.Width, MiddleVerticalStrip.Height)
        'For i As Integer = 0 To MiddleVerticalStrip.Width - 1
        '   For j As Integer = 0 To MiddleVerticalStrip.Height - 1
        '       StripImage.SetPixel(i, j, mImage.GetPixel(MiddleVerticalStrip.Left + i - 1, MiddleVerticalStrip.Top + j - 1))
        '   Next
        'Next
        StripImage = RotatedImage.Clone(CopyHorizStrip, RotatedImage.PixelFormat)
        Me.mTopMargin = FindEdge(StripImage)

        StripImage.RotateFlip(RotateFlipType.RotateNoneFlipX)
        Me.mBottomMargin = FindEdge(StripImage)
    End Sub


    Private Function FindEdge(ByVal BM As Bitmap) As Integer
        Try
            Dim Anchor As Integer = -1
            Dim Creeper As Integer = 0

            'Temp comparison colours during creeping
            Dim c1 As Color
            Dim c2 As Color

            'Records whether we have yet seen a black pixel in the anchor position.
            'Minor hack to escape white borders before edge of terminal window
            Dim SeenBlackAnchor As Boolean

            'Creep until contrast is detected
            Do
                Anchor += 1
                Creeper += 1
                c1 = Me.GetColumnAverage(BM, Anchor)
                c2 = Me.GetColumnAverage(BM, Creeper)
                If c1.R = 0 AndAlso c1.G = 0 AndAlso c1.B = 0 Then SeenBlackAnchor = True
            Loop Until SeenBlackAnchor AndAlso Me.DetectColourDifference(c1, c2)

            Return Anchor

            'Catch acts as crude control over how many iterations
            'we can perform - exception thrown when try to read out
            'of the bounds of the bitmap
        Catch
            Return 0
        End Try

    End Function


    Private Function GetColumnAverage(ByVal BM As Bitmap, ByVal ColumnIndex As Integer) As Color
        Dim R As Integer
        Dim G As Integer
        Dim B As Integer
        For i As Integer = 0 To BM.Height - 1
            Dim c As Color = BM.GetPixel(ColumnIndex, i)
            R += c.R
            G += c.G
            B += c.B
        Next
        R = R \ BM.Height
        G = G \ BM.Height
        B = B \ BM.Height
        Return Color.FromArgb(R, G, B)
    End Function

    Private Function DetectColourDifference(ByVal pixel1 As Color, ByVal pixel2 As Color) As Boolean
        'The following naive colour comparison based on
        'http://www.wat-c.org/tools/CCA/1.1/
        'to detect very obvious changes in colour/brightness contrast

        Const BrightnessThreshold As Integer = 85
        Const ContrastThreshold As Integer = 255

        Dim Brightness1 As Integer = (pixel1.R * 299 + pixel1.G * 587 + pixel1.B * 114) \ 1000
        Dim Brightness2 As Integer = (pixel2.R * 299 + pixel2.G * 587 + pixel2.B * 114) \ 1000

        Dim ColourContrast As Integer = Math.Abs(CInt(pixel1.R) - CInt(pixel2.R)) + Math.Abs(CInt(pixel1.G) - CInt(pixel2.G)) + Math.Abs(CInt(pixel1.B) - CInt(pixel2.B))
        Dim BrightnessDifference As Integer = Math.Abs(Brightness1 - Brightness2)

        Return (BrightnessDifference >= BrightnessThreshold) AndAlso (ColourContrast >= ContrastThreshold)
    End Function


    Private mLeftMargin As Integer
    Public ReadOnly Property LeftMarginEstimate() As Integer
        Get
            Return Me.mLeftMargin
        End Get
    End Property

    Private mRightMargin As Integer
    Public ReadOnly Property RightMarginEstimate() As Integer
        Get
            Return mRightMargin
        End Get
    End Property

    Private mTopMargin As Integer
    Public ReadOnly Property TopMarginEstimate() As Integer
        Get
            Return mTopMargin
        End Get
    End Property

    Private mBottomMargin As Integer
    Public ReadOnly Property BottomMarginEstimate() As Integer
        Get
            Return mBottomMargin
        End Get
    End Property

End Class
