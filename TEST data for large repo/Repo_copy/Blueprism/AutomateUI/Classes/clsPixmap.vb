Option Strict On

Imports BluePrism.AutomateAppCore.clsUtility

''' Project  : Automate
''' Class    : clsPixmap
''' 
''' <summary>
''' Creates an abstract pixmap to be used in combination with a control.
''' Essentially this class provides a lookup for objects per pixel. A range of 
''' pixels can be specified for an object, using a rectangle.
''' 
''' Rectangular blocks sharing the same object can be added to a collection,
''' this mapping class then intelligently decides which object to return in the
''' case of intersections by comparing the proximity to centres etc.
''' </summary>
Public Class clsPixmap

#Region "Members"

    ''' Project  : Automate
    ''' Class    : clsTooltipMap.BoundedTooltip
    ''' 
    ''' <summary>
    ''' Here we provide means of associating a tooltip with a particular section of 
    ''' a control by setting bounds for the domain of the tooltip (using a rectangle);
    ''' a friendly name for the tooltip; and an integer id for it.
    ''' </summary>
    Public Structure PixmapItem
        Public Sub New(ByVal ItemArea As Rectangle, ByVal Item As Object)
            Me.New(ItemArea, Item, "", 0)
        End Sub
        Public Sub New(ByVal ItemArea As Rectangle, ByVal Item As Object, ByVal Name As String, ByVal ID As Integer)
            Me.ItemArea = ItemArea
            Me.Item = Item
            Me.Name = Name
            Me.ID = ID
        End Sub

        ''' <summary>
        ''' The area defining which pixels the tooltip is relevant for.
        ''' </summary>
        Public ItemArea As Rectangle

        ''' <summary>
        ''' The object stored at in this retangle
        ''' </summary>
        Public Item As Object

        ''' <summary>
        ''' A friendly name referencing this object.
        ''' </summary>
        Public Name As String

        ''' <summary>
        ''' An identifier for this object.
        ''' </summary>
        Public ID As Integer
    End Structure


#End Region

    ''' <summary>
    ''' Private member to store public property ItemCollection()
    ''' </summary>
    Private mTooltipCol As New Collection
    ''' <summary>
    ''' The set of PixmapItem objects.
    ''' </summary>
    ''' <value>The collection.</value>
    Public Property ItemCollection() As Collection
        Get
            Return mTooltipCol
        End Get
        Set(ByVal value As Collection)
            mTooltipCol = value
        End Set
    End Property

#Region "Public Methods"

    ''' <summary>
    ''' Gets the the object stored at the specified point.
    ''' </summary>
    ''' <param name="x">The X coordinate of the point.</param>
    ''' <param name="y">The Y coordinate of the point.</param>
    ''' <returns>Returns the object stored at the specified point.
    ''' </returns>
    Public Function GetItemForPoint(ByVal x As Integer, ByVal y As Integer) As PixmapItem
        'the returned object is never nothing so this is safe
        Return Me.GetObjectForPoint(x, y)
    End Function


    ''' <summary>
    ''' Returns the PixmapItem object at the specified point. If no object is
    ''' found then a new and empty (but not null) PixmapItem is returned.
    ''' </summary>
    ''' <param name="x">The X coordinate of the point.</param>
    ''' <param name="y">The Y coordinate of the point.</param>
    ''' <returns>The object found.</returns>
    Public Function GetObjectForPoint(ByVal x As Integer, ByVal y As Integer) As PixmapItem
        If Not Me.mTooltipCol Is Nothing Then
            Dim TempCol As New Collection
            For Each bt As PixmapItem In Me.mTooltipCol
                If bt.ItemArea.Contains(x, y) Then TempCol.Add(bt)
            Next
            If TempCol.Count = 0 Then
                'essentially we are returning nothing here
                Return New PixmapItem
            Else
                If TempCol.Count = 1 Then
                    Return CType(TempCol.Item(1), PixmapItem)
                Else
                    Return GetPixmapItemWithCentreClosestTo(x, y, TempCol)
                End If
            End If
        Else
            'essentially we are returning nothing here.
            Return New PixmapItem
        End If
    End Function

#End Region

#Region "Private Methods"


    ''' <summary>
    ''' Takes a collection of pixmapitem objects and a point. Finds the tooltip in
    ''' the collection whose centre is closest to the specified point.
    ''' </summary>
    ''' <param name="x">The X coordinate of the point of interest.</param>
    ''' <param name="y">The Y coordinate of the point of interest.</param>
    ''' <param name="ItemCollection">The collection of items of interest.</param>
    ''' <returns>Returns the pixmapitem object whose rectangle centre is closest
    ''' to the specified point. If the supplied collection is nothing, returns nothing.
    ''' </returns>
    Private Function GetPixmapItemWithCentreClosestTo(ByVal x As Integer, ByVal y As Integer, ByVal ItemCollection As Collection) As PixmapItem
        Dim dblMinDistance As Double = Double.PositiveInfinity
        Dim ChosenItem As PixmapItem = Nothing
        Dim dblTemp As Double
        For Each bt As PixmapItem In ItemCollection
            If Not bt.ItemArea.Equals(Rectangle.Empty) Then
                dblTemp = MathsUtil.GetDistanceBetweenTwoPoints(x, y, bt.ItemArea.X, bt.ItemArea.Y)
            Else
                dblTemp = Double.PositiveInfinity
            End If
            If dblTemp < dblMinDistance Then
                dblMinDistance = dblTemp
                ChosenItem = bt
            End If
        Next
        'careful - this may be nothing
        Return ChosenItem
    End Function

#End Region

End Class
