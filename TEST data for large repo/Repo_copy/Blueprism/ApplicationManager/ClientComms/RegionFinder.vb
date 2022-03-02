
Imports System.Drawing
Imports BluePrism.CharMatching
Imports BluePrism.CharMatching.UI
Imports BluePrism.Core.Media.Images

''' <summary>
''' Identifies coordinates of an application region within an application window. 
''' </summary>
Public Class RegionFinder

    ''' <summary>
    ''' Function used to capture bitmap of the application window
    ''' </summary>
    ''' <remarks>The window image is only captured if needed (i.e., a region or relative parent
    ''' is located using images)</remarks>
    Private mCaptureWindow As Func(Of Bitmap)

    ''' <summary>
    ''' The captured Window image, which is retained so that it can be disposed
    ''' </summary>
    Private mWindowBitmap As Bitmap

    ''' <summary>
    ''' An ImageSearcher instance based on captured application Window image.
    ''' </summary>
    ''' <remarks></remarks>
    Private mImageSearcher As ImageSearcher

    ''' <summary>
    ''' Creates a new RegionFinder instance for a specific application window. 
    ''' </summary>
    ''' <param name="captureWindow">Function used to capture the application window, which 
    ''' will be invoked if the application Window image is required to find the region</param>
    ''' <remarks></remarks>
    Sub New(captureWindow As Func(Of Bitmap))
        mCaptureWindow = captureWindow
    End Sub

    ''' <summary>
    ''' The ImageSearcher used to search the captured Window Bitmap - the image is captured
    ''' lazily the first time it is requested.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private ReadOnly Property ImageSearcher As ImageSearcher
        Get
            If mImageSearcher Is Nothing Then
                mWindowBitmap = CaptureWindow()
                mImageSearcher = New ImageSearcher(mWindowBitmap)
            End If
            Return mImageSearcher
        End Get
    End Property

    ''' <summary>
    ''' Captures the application window using the specified function
    ''' </summary>
    Private Function CaptureWindow() As Bitmap
        Return mCaptureWindow()
    End Function


    ''' <summary>
    ''' Identifies the rectangle containing a region within the application window
    ''' </summary>
    ''' <param name="locationParams">A RegionLocationParams object containing details of the region</param>
    ''' <returns>A rectangle representing the location and size of the region</returns>
    Public Function FindRegion(locationParams As RegionLocationParams) As Rectangle
        Try
            Return FindInternal(locationParams, 0)
        Finally
            CleanUp()
        End Try
    End Function

    ''' <summary>
    ''' Finds the location of a region using a provided set of location parameters,
    ''' keeping track of the ancestral level of the region that we are searching for.
    ''' </summary>
    ''' <param name="locn">The location parameters to use to identify the location
    ''' of the region</param>
    ''' <param name="level">The current level of the search, denoting what level of
    ''' ancestor this search is for. A value of 0 represents the final region we are
    ''' looking for; 1 represents that region's parent</param>
    ''' <returns>A rectangle describing the region found</returns>
    Private Function FindInternal(
     locn As RegionLocationParams, level As Integer) As Rectangle

        Dim parentOffset As Point = Point.Empty
        If (locn.Parent IsNot Nothing) Then

            Dim originalParentCoordinates = locn.Parent.Coordinates
            Dim actualParentCoordinates = FindInternal(locn.Parent, level + 1)

            If actualParentCoordinates.Equals(Rectangle.Empty) Then
                ' Parent not found so we can't locate current region
                Return Rectangle.Empty
            Else
                ' Calculate offset between actual parent location and location in model
                ' This offset determines where we will look for the current region
                Dim leftOffset = 0 - (originalParentCoordinates.Left - actualParentCoordinates.Left)
                Dim topOffset = 0 - (originalParentCoordinates.Top - actualParentCoordinates.Top)
                parentOffset = New Point(leftOffset, topOffset)
            End If
        End If

        ' Adjust current region coordinates according to where parent found if applicable
        Dim regionCoordinates As New Rectangle(locn.Coordinates.Location, locn.Coordinates.Size)
        regionCoordinates.Offset(parentOffset)

        If locn.LocationMethod = RegionLocationMethod.Coordinates Then
            ' Region located via coordinates - simply return the original coordinates (we don't
            ' validate regions located using coordinates by checking for an image). If this region is
            ' located relative to a parent region, the area will have been adjusted 
            ' relative to the location where the parent was found
            Return regionCoordinates
        Else
            ' Region located via search for region image within the application window
            ' The image can now also be greyscaled.

            Using regionBitmap = locn.Image.ToBitmap()

                Dim colorMatcher = CreateColorMatcher(locn.ColourTolerance, locn.Greyscale)
                Dim searchArea As Rectangle = GetImageSearchArea(locn, regionCoordinates)
                Dim startPoint = regionCoordinates.Location

                Dim imageStartPoint = ImageSearcher.FindSubImage(
                    regionBitmap, startPoint, searchArea, colorMatcher)
                If imageStartPoint Is Nothing Then _
                 Throw New NoSuchImageRegionException(level)

                Return New Rectangle(imageStartPoint.Value, locn.Coordinates.Size)

            End Using
        End If

    End Function

    ''' <summary>
    ''' Gets a Rectangle representing the area of the application window within which to search for
    ''' a region image. This is based on the original area of the region recorded during modelling
    ''' (translated according to parent region if applicable) expanded according to any padding
    ''' configured for use during the search.
    ''' </summary>
    ''' <param name="locationParams">A RegionLocationParams object containing details of the region</param>
    ''' <param name="regionCoordinates">The expected coordinates of the region. This is based on the original 
    ''' coordinates recorded during modelling, which are translated according to the location of the relative 
    ''' parent region if applicable.</param>
    ''' <returns>A Rectangle representing the area to search</returns>
    Private Function GetImageSearchArea(locationParams As RegionLocationParams, regionCoordinates As Rectangle) _
        As Rectangle

        Dim searchArea As Rectangle
        If locationParams.Position <> RegionPosition.Anywhere Then
            searchArea = New Rectangle(regionCoordinates.Location, regionCoordinates.Size)
            searchArea.X -= locationParams.Padding.Left
            searchArea.Y -= locationParams.Padding.Top
            searchArea.Width += locationParams.Padding.Horizontal
            searchArea.Height += locationParams.Padding.Vertical
        Else
            searchArea = Rectangle.Empty
        End If
        Return searchArea
    End Function

    ''' <summary>
    ''' Factory method that will return an appropriate implementation of IColorMatcher based up on
    ''' parameters.
    ''' </summary>
    ''' <param name="tolerance">Value indicating how much tolerance to use. </param>
    ''' <param name="useGreyScale">Flag that indicates where to use GreyScale color matching. </param>
    ''' <returns>An implementation of IColorMatcher. </returns>
    Private Function CreateColorMatcher(tolerance As Integer, useGreyscale As Boolean) As IColorMatcher

        Return If(useGreyscale, New GreyScaleColorMatcher(tolerance), CType(New FullColorMatcher(tolerance), IColorMatcher))
    End Function

    ''' <summary>
    ''' Disposes any resources used during search
    ''' </summary>
    Private Sub CleanUp()
        If mImageSearcher IsNot Nothing Then
            mImageSearcher.Dispose()
            mImageSearcher = Nothing
        End If
        If mWindowBitmap IsNot Nothing Then
            mWindowBitmap.Dispose()
            mWindowBitmap = Nothing
        End If
    End Sub
End Class
