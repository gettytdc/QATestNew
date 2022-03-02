Option Strict On

Imports BluePrism.AutomateAppCore.clsUtility

''' Project  : Automate
''' Class    : clsTooltipMap
''' 
''' <summary>
''' Creates an abstract pixmap to be used in combination with a control.
''' Essentially this class provides a lookup for tooltips per pixel.
''' 
''' Rectangular blocks sharing the same tooltip can be added to a collection,
''' this mapping class then intelligently decides which tooltip to return in the
''' case of intersections by comparing the proximity to centres etc.
''' </summary>
Public Class clsTooltipMap


#Region "Class BoundedTooltipCollection"

    ''' Project  : Automate
    ''' Class    : clsTooltipMap.BoundedTooltipCollection
    ''' 
    ''' <summary>
    ''' We define a collection of the boundedtooltip structure, so that the parent
    ''' class can keep more than one such object.
    ''' </summary>
    Public Class BoundedTooltipCollection
        Inherits CollectionBase

        ''' <summary>
        ''' Adds a new boundedtooltip object to the collection.
        ''' </summary>
        ''' <param name="value">The object to be added.</param>
        ''' <returns>Returns the index at which the object was added.</returns>
        Public Function Add(ByVal value As BoundedTooltip) As Integer
            Return MyBase.List.Add(value)
        End Function

        ''' <summary>
        ''' Determines whether the supplied object is already contained in the collection.
        ''' </summary>
        ''' <param name="value">The object to test for existance.</param>
        ''' <returns>Returns true if the object exists in the collection; false otherwise.
        ''' </returns>
        Public Function Contains(ByVal value As BoundedTooltip) As Boolean
            Return MyBase.List.Contains(value)
        End Function

        ''' <summary>
        ''' References objects in the collection by index.
        ''' </summary>
        ''' <param name="index">The index of the object of interest.</param>
        ''' <value>The object of interest.</value>
        Default Public Property Item(ByVal index As Integer) As BoundedTooltip
            Get
                Return CType(MyBase.List.Item(index), BoundedTooltip)
            End Get
            Set(ByVal Value As BoundedTooltip)
                MyBase.List.Item(index) = Value
            End Set
        End Property

        ''' <summary>
        ''' Removes the specified object fro the collection.
        ''' </summary>
        ''' <param name="value">The object to be removed.</param>
        Public Sub Remove(ByVal value As BoundedTooltip)
            MyBase.List.Remove(value)
        End Sub

        ''' <summary>
        ''' Removes all objects in the collection sharing the name specified.
        ''' </summary>
        ''' <param name="s">The name.</param>
        Public Sub Remove(ByVal s As String)
            For Each o As BoundedTooltip In MyBase.List
                If o.Name = s Then MyBase.List.Remove(o)
            Next
        End Sub

    End Class

#End Region

#Region "Members"

    ''' Project  : Automate
    ''' Class    : clsTooltipMap.BoundedTooltip
    ''' 
    ''' <summary>
    ''' Here we provide means of associating a tooltip with a particular section of 
    ''' a control by setting bounds for the domain of the tooltip (using a rectangle);
    ''' a friendly name for the tooltip; and an integer id for it.
    ''' </summary>
    Public Class BoundedTooltip
        ''' <summary>
        ''' The area defining which pixels the tooltip is relevant for.
        ''' </summary>
        Public TooltipArea As Rectangle

        ''' <summary>
        ''' The text to be displayed in the tooltip.
        ''' </summary>
        Public TooltipText As String

        ''' <summary>
        ''' A friendly name referencing this object.
        ''' </summary>
        Public Name As String

        ''' <summary>
        ''' An identifier for this object. No uniqueness checking is performed.
        ''' </summary>
        Public ID As Integer
    End Class


#End Region

    ''' <summary>
    ''' Private member to store public property TooltipCol()
    ''' </summary>
    Private mTooltipCol As New BoundedTooltipCollection
    ''' <summary>
    ''' The set of bounded tooltip objects.
    ''' </summary>
    ''' <value>The collection.</value>
    Public Property TooltipCol() As BoundedTooltipCollection
        Get
            Return mTooltipCol
        End Get
        Set(ByVal value As BoundedTooltipCollection)
            mTooltipCol = value
        End Set
    End Property

#Region "Public Methods"

    ''' <summary>
    ''' Gets the text to be displayed at the point specified.
    ''' </summary>
    ''' <param name="x">The X coordinate of the point.</param>
    ''' <param name="y">The Y coordinate of the point.</param>
    ''' <returns>Returns the string to be displayed at the specified point.
    ''' </returns>
    Public Function GetTooltipTextForPoint(ByVal x As Integer, ByVal y As Integer) As String
        'the returned object is never nothing so this is safe
        Return Me.GetTooltipObjectForPoint(x, y).TooltipText
    End Function


    ''' <summary>
    ''' Returns the boundedtooltip object at the specified point. If no object is
    ''' found then a new and empty (but not null) collection is returned.
    ''' </summary>
    ''' <param name="x">The X coordinate of the point.</param>
    ''' <param name="y">The Y coordinate of the point.</param>
    ''' <returns>The object found.</returns>
    Public Function GetTooltipObjectForPoint(ByVal x As Integer, ByVal y As Integer) As BoundedTooltip
        If Not Me.mTooltipCol Is Nothing Then
            Dim TempCol As New BoundedTooltipCollection
            For Each bt As BoundedTooltip In Me.mTooltipCol
                If bt.TooltipArea.Contains(x, y) Then TempCol.Add(bt)
            Next
            If TempCol.Count = 0 Then
                'essentially we are returning nothing here
                Return New BoundedTooltip
            Else
                If TempCol.Count = 1 Then
                    Return TempCol.Item(0)
                Else
                    Return GetBoundedTooltipWithCentreClosestTo(x, y, TempCol)
                End If
            End If
        Else
            'essentially we are returning nothing here.
            Return New BoundedTooltip
        End If
    End Function

#End Region

#Region "Private Methods"

    ''' <summary>
    ''' Takes a collection of boundedtooltip objects and a point. Finds the tooltip in
    ''' the collection whose centre is closest to the specified point.
    ''' </summary>
    ''' <param name="x">The X coordinate of the point of interest.</param>
    ''' <param name="y">The Y coordinate of the point of interest.</param>
    ''' <param name="TooltipCollection">The collection of tooltips of interest.</param>
    ''' <returns>Returns the boundedtooltip object whose rectangle centre is closest
    ''' to the specified point. If the supplied collection is nothing, returns nothing.
    ''' </returns>
    Private Function GetBoundedTooltipWithCentreClosestTo(ByVal x As Integer, ByVal y As Integer, ByVal TooltipCollection As BoundedTooltipCollection) As BoundedTooltip
        Dim dblMinDistance As Double = Double.PositiveInfinity
        Dim ChosenTooltip As BoundedTooltip = Nothing
        Dim dblTemp As Double
        For Each bt As BoundedTooltip In TooltipCollection
            If Not bt.TooltipArea.Equals(Rectangle.Empty) Then
                dblTemp = MathsUtil.GetDistanceBetweenTwoPoints(x, y, bt.TooltipArea.X, bt.TooltipArea.Y)
            Else
                dblTemp = Double.PositiveInfinity
            End If
            If dblTemp < dblMinDistance Then
                dblMinDistance = dblTemp
                ChosenTooltip = bt
            End If
        Next
        'careful - this may be nothing
        Return ChosenTooltip
    End Function

#End Region

End Class
