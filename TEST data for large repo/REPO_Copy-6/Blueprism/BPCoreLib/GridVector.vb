Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Enumeration describing the type of vector
''' </summary>
Public Enum VectorSizeType
    Proportional
    Absolute
End Enum

''' <summary>
''' Class which describes a vector - that is a row or column marker.
''' </summary>
Public MustInherit Class GridVector
    Implements ICloneable

#Region " Class Scope Declarations "

    ''' <summary>
    ''' Gets the absolute sizes of the given vectors given the corresponding
    ''' size that they are vectors of - eg. if they are columns, the region
    ''' size should be the width of the containing element
    ''' </summary>
    ''' <typeparam name="T">The type of vector - typically row or column.
    ''' </typeparam>
    ''' <param name="vectors">The collection of vectors for which the absolute
    ''' sizes are required.</param>
    ''' <param name="regionSize">The total size of the region that the
    ''' vectors describe.</param>
    ''' <returns>A collection of absolute sizes of the vectors after translating
    ''' from possible proportional vectors and absolutes.</returns>
    Public Shared Function GetAbsolutes(Of T As GridVector)( _
     ByVal vectors As ICollection(Of T), ByVal regionSize As Integer) _
     As ICollection(Of Integer)

        ' map to hold all absolute sizes after the proportional values have
        ' been resolved
        Dim sizes As IDictionary(Of GridVector, Integer) = _
         New clsOrderedDictionary(Of GridVector, Integer)()

        ' Set to hold the proportionals prior to calculating their absolute sizes
        Dim proportionals As ICollection(Of GridVector) = _
         New clsOrderedSet(Of GridVector)()

        ' Total of all proportional values (to calculate percentage from)
        Dim totalProportional As Integer = 0

        ' Grand total - only fixed is added in first pass, proportional later
        Dim total As Integer = 0

        ' The last proportional vector handled by this method, or, if there are
        ' no proportional vectors, the last vector.
        Dim lastVector As GridVector = Nothing

        ' Go through each vector and add it to the sizes map with their absolute
        ' sizes or -1 if their sizes are not yet known - in the latter case,
        ' add them to the 'proportionals' set too.
        For Each vector As GridVector In vectors
            ' First pass sets lastVector to the last vector regardless of type
            lastVector = vector

            If vector.SizeType = VectorSizeType.Absolute Then
                sizes(vector) = vector.Value
                total += vector.Value
            Else
                sizes(vector) = -1
                ' the marker for later
                proportionals.Add(vector)
                totalProportional += vector.Value
            End If
        Next

        ' Now calculate the absolute sizes of the proportional vectors - we take
        ' the remaining space left over from applying the fixed vectors, and
        ' distribute it amongst the proportional vectors according to their
        ' relative size.
        ' For example:-
        ' --- Vectors ---   - regionSize -
        ' [20% ; 150 ; 30%] <=    250    =>
        ' .. creates 2 sets:
        ' [-1 ; 150 ; -1] and [20 ; 30]
        ' [20 ; 30] = proportional values, they give the following as absolutes:-
        '   [20 ; ...]: (250 - 150) * (20 / 50) == 100 * 0.4 == 40;
        '   [... ; 30]: (250 - 150) * (30 / 50) == 100 * 0.6 == 60
        ' ie. [propor]: (total - totalFixed) * (propor / totalPropor)

        Dim remainingSize As Double = regionSize - total
        For Each v As GridVector In proportionals
            ' The second pass ensures lastVector is the last proportional vector
            lastVector = v

            Dim absSize As Integer = _
             CInt(Math.Round((remainingSize * (CDbl(v.Value) / totalProportional))))
            sizes(v) = absSize
            total += absSize
        Next

        ' If there is some left over space (rounding errors, not specified), add
        ' it to the last proportional vector, or the last vector of any type if
        ' there are no proportional ones. Conversely, if we've used too much
        ' space (rounding errors only), take if off the last vector.
        If regionSize - total <> 0 AndAlso lastVector IsNot Nothing Then
            sizes(lastVector) += (regionSize - total)
        End If

        Return sizes.Values
    End Function

#End Region

#Region " Member Variables "

    ' The size type of this vector
    Private _type As VectorSizeType

    ' The size described by this vector
    Private _size As Integer

    ' The padding used by this vector (not actually implemented)
    Private _padding As Integer

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new proportional grid vector with a weight of 50
    ''' </summary>
    Protected Sub New()
        Me.New(VectorSizeType.Proportional, 50)
    End Sub


    ''' <summary>
    ''' Creates a new grid vector of the given type and with a default size of 50
    ''' </summary>
    ''' <param name="type">The size type of the vector</param>
    Protected Sub New(ByVal type As VectorSizeType)
        Me.New(type, 50)
    End Sub

    ''' <summary>
    ''' Creates a new grid vector with the given properties
    ''' </summary>
    ''' <param name="type">The size type of the vector</param>
    ''' <param name="size">The size of the vector - in pixels for absolute
    ''' vectors, in units to compare to other vectors for proportional ones.
    ''' </param>
    Protected Sub New(ByVal type As VectorSizeType, ByVal size As Integer)
        SizeType = type
        Value = size
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The value of this vector. For absolute vectors, this number represents the
    ''' number of pixels represented by this object. For proportional vectors, it
    ''' represents a proportion of the available size to use for this object, and
    ''' only becomes a concrete value when compared against other proportional
    ''' vectors covering the same measurement.
    ''' </summary>
    Public Property Value() As Integer
        Get
            Return _size
        End Get
        Set(ByVal value As Integer)
            If value < 1 Then
                Throw New ArgumentOutOfRangeException(My.Resources.GridVector_Size, String.Format(
                 My.Resources.GridVector_TheValue0IsNotAllowedOnlySizesOf1OrGreaterAreSupported, value))
            End If
            _size = value
        End Set
    End Property

    ''' <summary>
    ''' A string representing the value of this vector - this (over)simplifies
    ''' proportional vectors as percentages.
    ''' </summary>
    Public ReadOnly Property ValueString() As String
        Get
            Select Case _type
                Case VectorSizeType.Absolute
                    Return _size.ToString()
                Case VectorSizeType.Proportional
                    Return String.Format("{0}%", _size)
                Case Else
                    Return ""
            End Select
        End Get
    End Property

    ''' <summary>
    ''' The size type of this vector
    ''' </summary>
    Public Property SizeType() As VectorSizeType
        Get
            Return _type
        End Get
        Set(ByVal value As VectorSizeType)
            _type = value
        End Set
    End Property

    ''' <summary>
    ''' The label for the size type of this vector - used to display it in the
    ''' UI which manipulates the vectors.
    ''' </summary>
    Public ReadOnly Property SizeTypeLabel() As String
        ' if we want to be precise, the TableLayoutPanel UI uses "Percent" rather
        ' than "Proportional", so we could change that here.
        Get
            Return _type.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The padding assigned to this vector. Currently not used.
    ''' </summary>
    Public Property Padding() As Integer
        Get
            Return _padding
        End Get
        Set(ByVal value As Integer)
            _padding = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Clones this object and returns the newly created copy
    ''' </summary>
    ''' <returns>A deep clone of this vector</returns>
    ''' <remarks>Only directly used via the ICloneable interface - actually used
    ''' when a schema is cloned or copies the data from another schema via the
    ''' <see cref="CollectionUtil.CloneInto"/> utility method.</remarks>
    Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
        Return Clone()
    End Function

    ''' <summary>
    ''' Clones this object and returns the newly created copy
    ''' </summary>
    ''' <returns>A deep clone of this vector</returns>
    Public Function Clone() As GridVector
        Return TryCast(MemberwiseClone(), GridVector)
    End Function

    ''' <summary>
    ''' Gets a string representation of this vector - this is just the value string,
    ''' which gives both size type and value of the vector.
    ''' </summary>
    ''' <returns>The value of this vector as a string</returns>
    Public Overrides Function ToString() As String
        Return ValueString
    End Function

#End Region

End Class

#Region " Concrete Implementations (GridColumn / GridRow) "

''' <summary>
''' Concrete implementation of a grid vector for columns
''' </summary>
Public Class GridColumn
    Inherits GridVector
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal type As VectorSizeType)
        MyBase.New(type)
    End Sub
    Public Sub New(ByVal type As VectorSizeType, ByVal size As Integer)
        MyBase.New(type, size)
    End Sub
End Class

''' <summary>
''' Concrete implementation of a grid vector for rows
''' </summary>
Public Class GridRow
    Inherits GridVector
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal type As VectorSizeType)
        MyBase.New(type)
    End Sub
    Public Sub New(ByVal type As VectorSizeType, ByVal size As Integer)
        MyBase.New(type, size)
    End Sub
End Class

#End Region

