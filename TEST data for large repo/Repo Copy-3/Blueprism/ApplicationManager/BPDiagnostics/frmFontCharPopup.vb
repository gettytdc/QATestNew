Imports BluePrism.CharMatching

Friend Class frmFontCharPopup

    ''' <summary>
    ''' The width, in pixels, of the area used to display one of the
    ''' pixels in the mCharImage member.
    ''' </summary>
    Private Const PixelDisplayWidth As Integer = 15

    ''' <summary>
    ''' The character image to be displayed.
    ''' </summary>
    Private mCharImage As Image

    ''' <summary>
    ''' The statemask of the character being displayed.
    ''' </summary>
    ''' <remarks></remarks>
    Private mStateMask As PixelStateMask

    Friend Sub SetCharacterImage(ByVal CharImage As Image, ByVal StateMask As PixelStateMask)
        If StateMask IsNot Nothing Then
            Dim StateMaskWidth As Integer = StateMask.Width
            Dim StateMaskHeight As Integer = StateMask.Height
            If StateMaskWidth <> CharImage.Width OrElse StateMaskHeight <> CharImage.Height Then
                Throw New ArgumentException(My.Resources.DimensionsOfTheStatemaskMustMatchTheSuppliedImage)
            End If
            mStateMask = StateMask
        End If

        Me.ClientSize = New Size(PixelDisplayWidth * (CharImage.Width + 2), PixelDisplayWidth * (CharImage.Height + 2))
        pbCharImage.Dock = DockStyle.None
        pbCharImage.Location = New Point(PixelDisplayWidth, PixelDisplayWidth)
        pbCharImage.Size = New Size(PixelDisplayWidth * CharImage.Width, PixelDisplayWidth * CharImage.Height)
        Me.BackColor = Color.DarkGray

        mCharImage = CharImage
        pbCharImage.Image = CharImage
    End Sub



    Private Sub pbCharImage_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles pbCharImage.Paint

        If Me.pbCharImage.Image IsNot Nothing Then
            e.Graphics.Clear(Color.Green)

            '   'Had some trouble using Graphics.DrawImage with InterpolationMode = NearestNeighbor,
            '   '(it would chop off the first half pixel), so drawing this myself. (PJW)
            Dim BM As Bitmap = CType(mCharImage, Bitmap)
            For i As Integer = 0 To mCharImage.Width - 1
                For j As Integer = 0 To mCharImage.Height - 1
                    e.Graphics.FillRectangle(New SolidBrush(BM.GetPixel(i, j)), New Rectangle(i * PixelDisplayWidth, j * PixelDisplayWidth, PixelDisplayWidth, PixelDisplayWidth))
                Next
            Next

            'Draw some gridlines showing where the boundaries are.
            Static P As New System.Drawing.Pen(Color.Red)
            P.DashStyle = Drawing2D.DashStyle.Dash
            For i As Integer = 1 To pbCharImage.Image.Width
                Dim X As Integer = i * PixelDisplayWidth
                e.Graphics.DrawLine(P, X, 0, X, pbCharImage.Height)
            Next
            For j As Integer = 1 To pbCharImage.Height
                Dim Y As Integer = j * PixelDisplayWidth
                e.Graphics.DrawLine(P, 0, Y, pbCharImage.Width, Y)
            Next
        End If
    End Sub
End Class
