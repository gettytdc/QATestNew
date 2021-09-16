''' Project  : Automate
''' Class    : clsSortImages
''' 
''' <summary>
''' Singleton class to hold the images used to display sort direction.
''' </summary>
''' <remarks></remarks>
Public Class clsSortImages

    ''' <summary>
    ''' Access the single instance of the SortImages class
    ''' </summary>
    ''' <returns>The SortImages singleton containing the single instance of the sort
    ''' images.</returns>
    Public Shared ReadOnly Property Instance() As clsSortImages
        Get
            Return smInstance
        End Get
    End Property
    Private Shared smInstance As clsSortImages = New clsSortImages()

    ''' <summary>
    ''' Enumeration of the different arrow types - this value is used as an index in
    ''' the image list held by instances of this class
    ''' </summary>
    Public Enum ArrowType
        Ascending
        Descending
        Blank
    End Enum

    ''' <summary>
    ''' 
    ''' </summary>
    Private Sub New()
        images = New ImageList()
        images.ImageSize = New Size(8, 8)
        images.TransparentColor = System.Drawing.Color.Magenta
        images.Images.Add(GetArrowBitmap(ArrowType.Ascending))
        images.Images.Add(GetArrowBitmap(ArrowType.Descending))
        images.Images.Add(GetArrowBitmap(ArrowType.Blank))
    End Sub

    ''' <summary>
    ''' The images used for showing sort direction
    ''' </summary>
    ''' <remarks></remarks>
    Private images As ImageList

    ''' <summary>
    ''' Gets the image list held by this object.
    ''' </summary>
    ''' <returns>An image list with the images for showing 
    ''' sort direction.</returns>
    ''' <remarks></remarks>
    Public Function GetImageList() As ImageList
        Return images
    End Function

    ''' <summary>
    ''' Gets the image associated with the supplied sort order.
    ''' </summary>
    ''' <param name="Order">The sort order of interest.</param>
    ''' <returns>The image that is used with the supplied sort
    ''' order, if any.</returns>
    Public Function GetImage(ByVal Order As SortOrder) As Image
        Return images.Images(GetImageIndex(Order))
    End Function

    ''' <summary>
    ''' Gets the index within the image list of the supplied sort order.
    ''' </summary>
    ''' <param name="Order">The sort order of interest</param>
    ''' <returns>The index within this object's image list of the image
    ''' representing the desired sort order</returns>
    Public Function GetImageIndex(ByVal Order As SortOrder) As Integer
        Select Case Order
            Case SortOrder.Ascending
                Return ArrowType.Ascending
            Case SortOrder.Descending
                Return ArrowType.Descending
            Case Else
                Return ArrowType.Blank
        End Select
    End Function

    ''' <summary>
    ''' Gets the bitmap of the arrow which corresponds to the given arrow
    ''' type value.
    ''' </summary>
    ''' <param name="type">The arrow type required</param>
    ''' <returns>A bitmap of the required arrow.</returns>
    Private Function GetArrowBitmap(ByVal type As ArrowType) As Bitmap
        ' Deal with blank bitmap first
        If type = ArrowType.Blank Then
            Return New Bitmap(8, 8)
        End If

        Dim bmp As Bitmap = New Bitmap(8, 8)
        Dim gfx As Graphics = Graphics.FromImage(bmp)
        Dim lightPen As Pen = SystemPens.ControlLightLight
        Dim shadowpen As Pen = SystemPens.ControlDark

        gfx.FillRectangle(System.Drawing.Brushes.Magenta, 0, 0, 8, 8)

        If type = ArrowType.Ascending Then
            gfx.DrawLine(lightPen, 0, 7, 7, 7)
            gfx.DrawLine(lightPen, 7, 7, 4, 0)
            gfx.DrawLine(shadowpen, 3, 0, 0, 7)
        Else
            gfx.DrawLine(lightPen, 4, 7, 7, 0)
            gfx.DrawLine(shadowpen, 3, 7, 0, 0)
            gfx.DrawLine(shadowpen, 0, 0, 7, 0)
        End If

        gfx.Dispose()

        Return bmp
    End Function

End Class
