Imports System.drawing

''' <summary>
''' Utility methods and structures for 2D geometry calculations.
''' </summary>
Public Class clsGeometry


    ''' <summary>
    ''' Gets the intersection of two lines as a point, if such a unique point
    ''' exists. Throws an exception otherwise.
    ''' </summary>
    ''' <param name="L1">The first of the two lines under consideration.</param>
    ''' <param name="L2">The second of the two lines under consideration.</param>
    ''' <returns>Returns the point at which the two lines intersect, or throws
    ''' an exception if no such point exists..</returns>
    Public Shared Function GetIntersectionOfLines(ByVal L1 As LineF, ByVal L2 As LineF) As PointF
        If L1.IsParallelTo(L2) Then
            Throw New InvalidOperationException(My.Resources.Resources.clsGeometry_LinesAreParellelSoDoNotHaveAUniqueIntersection)
        End If

        'We solve simultaneous eqn:  A + rM = B + sN (where each cap letter
        'represents a point in 2D, and each lower case letter represents a scalar)
        'M,N are unit vectors indicating the direction of each line and X,Y are
        'points on the two lines.
        Dim A As PointF = L1.Location
        Dim B As PointF = L2.Location
        Dim M As Vector = L1.UnitVector
        Dim N As Vector = L2.UnitVector
        Dim s As Single = (M.X * (A.Y - B.Y) + M.Y * (B.X - A.X)) / (M.X * N.Y - N.X * M.Y)

        Return PointF.Add(B, New SizeF(N.X * s, N.Y * s))
    End Function

    ''' <summary>
    ''' Gets the intersection of a line from the centre of a rectangle to the edge
    ''' of the rectangle.
    ''' </summary>
    ''' <param name="LineDirection">A unit vector specifying the direction of the
    ''' line (which goes through the centre of the rectangle).</param>
    ''' <param name="R">The rectangle of interest.</param>
    ''' <returns>Gets the point at which the specified line intersects
    ''' the rectangle.</returns>
    Public Shared Function GetIntersectionOfLineFromRectCentreWithRectEdge(ByVal LineDirection As Vector, ByVal R As RectangleF) As PointF

        If R.Height = 0 OrElse R.Width = 0 Then
            Return R.Location
        End If
        If LineDirection.IsEmpty Then
            Throw New ArgumentException(My.Resources.Resources.clsGeometry_LineDirectionMustNotBeZero, NameOf(LineDirection))
        End If


        'We are going to calculate the intersection of the specified line
        'with one of the bounding lines of the rectangle, but which one?
        'We compare angle of line, to angle of line which goes from centre
        'to top/right corner. Be careful of vertical line!
        Dim LineToUse As LineF
        Dim TanOfAngleToTopRightCorner As Double = R.Height / R.Width
        Dim LineIsVertical As Boolean = (LineDirection.X = 0)
        If (LineIsVertical) OrElse (Math.Abs(LineDirection.Y / LineDirection.X) > Math.Abs(TanOfAngleToTopRightCorner)) Then
            'Use horiz line
            LineToUse.UnitVector = New Vector(1, 0)

            'But choose top or bottom edge of rectangle?
            If LineDirection.Y > 0 Then
                LineToUse.Location = New PointF(R.Left, R.Bottom)
            Else
                LineToUse.Location = R.Location
            End If
        Else
            'Use vertical line
            LineToUse.UnitVector = New Vector(0, 1)

            'But choose left or right hand edge of rectangle?
            If LineDirection.X > 0 Then
                LineToUse.Location = New PointF(R.Right, R.Top)
            Else
                LineToUse.Location = R.Location
            End If
        End If

        Dim LineFromRectCentre As New LineF(GetRectangleCentre(R), LineDirection)
        Return GetIntersectionOfLines(LineFromRectCentre, LineToUse)
    End Function

    ''' <summary>
    ''' Structure representing the concept of a line, specified
    ''' by a point on the line, and the direction in which the
    ''' line points.
    ''' </summary>
    Public Structure LineF
        ''' <summary>
        ''' A point through which the line runs.
        ''' </summary>
        Public Location As PointF
        ''' <summary>
        ''' A unit vector indicating the direction of the line.
        ''' </summary>
        Public Property UnitVector() As Vector
            Get
                Return mUnitVector
            End Get
            Set(ByVal value As Vector)
                mUnitVector = value.ToUnitVector

                mUnitNormalVector = mUnitVector.Normal.ToUnitVector
            End Set
        End Property
        Private mUnitVector As Vector
        Public ReadOnly Property UnitNormalVector() As Vector
            Get
                Return mUnitNormalVector
            End Get
        End Property
        Private mUnitNormalVector As Vector

        ''' <summary>
        ''' Determines whether this line is parallel to the supplied one.
        ''' </summary>
        ''' <param name="Line2">The line to which this one should be compared.</param>
        ''' <returns>Returns true if the lines are parallel, false otherwise.</returns>
        Public Function IsParallelTo(ByVal Line2 As LineF) As Boolean
            Return Math.Abs(Me.UnitVector.X) = Math.Abs(Line2.UnitVector.X) AndAlso _
              Math.Abs(Me.UnitVector.Y) = Math.Abs(Line2.UnitVector.Y)
        End Function

        Public Sub New(ByVal Location As PointF, ByVal UnitVector As Vector)
            Me.Location = Location
            Me.UnitVector = UnitVector
        End Sub
    End Structure

    ''' <summary>
    ''' Represents a 2D vector in cartesian coordinates
    ''' </summary>
    Public Structure Vector
        ''' <summary>
        ''' The X component of the vector.
        ''' </summary>
        Public X As Single
        ''' <summary>
        ''' The Y components of the vector.
        ''' </summary>
        Public Y As Single
        ''' <summary>
        ''' The magnitude of the vector.
        ''' </summary>
        Public ReadOnly Property Length() As Double
            Get
                Return Math.Sqrt(X * X + Y * Y)
            End Get
        End Property

        Public Sub New(ByVal X As Single, ByVal Y As Single)
            Me.X = X
            Me.Y = Y
        End Sub

        ''' <summary>
        ''' Gets the equivalent unit vector.
        ''' </summary>
        ''' <returns>Returns a unit vector having the same direction as this
        ''' vector with unit magnitude, unless this vector is zero (in which
        ''' case the zero vector is returned instead).</returns>
        ''' <remarks></remarks>
        Public Function ToUnitVector() As Vector
            Dim L As Double = Me.Length
            Return New Vector(CSng(X / L), CSng(Y / L))
        End Function

        ''' <summary>
        ''' Returns the vector that is a ninety degree rotation
        ''' from this one; if this vector is empty then returns
        ''' the empty vector.
        ''' </summary>
        Public ReadOnly Property Normal() As Vector
            Get
                Return New Vector(Y, -X)
            End Get
        End Property

        ''' <summary>
        ''' Determines whether this vector is empty.
        ''' </summary>
        ''' <returns>Returns true if this vector is empty; false otherwise.</returns>
        Public Function IsEmpty() As Boolean
            Return X = 0 AndAlso Y = 0
        End Function

        ''' <summary>
        ''' Determines whether the supplied vector is parallel to this one.
        ''' </summary>
        ''' <param name="V">The vector to which this one should be compared.</param>
        ''' <returns>Returns true if the two vectors are parallel;
        ''' false otherwise.</returns>
        Public Function IsParallelTo(ByVal V As Vector) As Boolean
            Return (Me.Y / Me.X) = (V.Y / V.X)
        End Function

        ''' <summary>
        ''' Implements the unary 'minus' operator on the vector;
        ''' gets the same vector in the opposite direction (180
        ''' degree rotation).
        ''' </summary>
        ''' <param name="V">The vector whose opposite is sought.</param>
        ''' <returns>Returns the vector in which each component is
        ''' the negated value of V, in the same component.</returns>
        Public Shared Operator -(ByVal V As Vector) As Vector
            Return New Vector(-V.X, -V.Y)
        End Operator

        ''' <summary>
        ''' Implements the 'minus' operator for two vectors.
        ''' </summary>
        ''' <param name="V1">The vector from which to subtract the second.</param>
        ''' <param name="V2">The to be subtracted from the first.</param>
        ''' <returns>Gets the vector in which each component is equal
        ''' to the difference of the two respective components in the supplied
        ''' vectors.</returns>
        Public Shared Operator -(ByVal V1 As Vector, ByVal V2 As Vector) As Vector
            Return New Vector(V1.X - V2.X, V1.Y - V2.Y)
        End Operator

        ''' <summary>
        ''' Implements the 'sum' operator for two vectors.
        ''' </summary>
        ''' <param name="V1">The first vector to be summed.</param>
        ''' <param name="V2">The second vector to be summed.</param>
        ''' <returns>Gets the vector in which each component is equal
        ''' to the sum of the two respective components in the supplied
        ''' vectors.</returns>
        Public Shared Operator +(ByVal V1 As Vector, ByVal V2 As Vector) As Vector
            Return New Vector(V1.X + V2.X, V1.Y + V2.Y)
        End Operator

        ''' <summary>
        ''' Implements the 'dot product' of two vectors.
        ''' </summary>
        ''' <param name="V1">The first vector to be 'dotted'.</param>
        ''' <param name="V2">The second vector to be 'dotted'.</param>
        ''' <returns>Returns the 'dot product' of the two vectors.</returns>
        Public Shared Operator *(ByVal V1 As Vector, ByVal V2 As Vector) As Single
            Return V1.X * V2.X + V1.Y * V2.Y
        End Operator
    End Structure

    ''' <summary>
    ''' Gets the centre of the supplied rectangle
    ''' </summary>
    ''' <param name="R">The rectangle whose centre is sought.</param>
    ''' <returns>Returns the point which is at the centre of the supplied
    ''' rectangle.</returns>
    Public Shared Function GetRectangleCentre(ByVal R As RectangleF) As PointF
        Return New PointF(R.X + R.Width / 2, R.Y + R.Height / 2)
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
