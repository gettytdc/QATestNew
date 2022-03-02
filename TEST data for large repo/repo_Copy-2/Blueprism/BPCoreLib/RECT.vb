Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports BluePrism.Server.Domain.Models

<CLSCompliant(True)> _
<StructLayout(LayoutKind.Sequential)> _
Public Structure RECT
    ' Empty, externally unmodifiable rectangle
    Private Shared ReadOnly sEmpty As New RECT()

    ''' <summary>
    ''' An empty RECT - ie. with all properties set to zero
    ''' </summary>
    Public Shared ReadOnly Property Empty() As RECT
        Get
            Return sEmpty
        End Get
    End Property

    Public Left As Integer
    Public Top As Integer
    Public Right As Integer
    Public Bottom As Integer
    Public Sub New(ByVal rect As Rectangle)
        Me.New(rect.Left, rect.Right, rect.Top, rect.Bottom)
    End Sub
    Public Sub New(ByVal fromPt As Point, ByVal toPt As Point)
        Me.New(fromPt.X, toPt.X, fromPt.Y, toPt.Y)
    End Sub
    Public Sub New(ByVal left As Integer, ByVal right As Integer, ByVal top As Integer, ByVal bottom As Integer)
        Me.Left = left
        Me.Right = right
        Me.Top = top
        Me.Bottom = bottom
    End Sub
    Public Property Width() As Integer
        Get
            Return Right - Left
        End Get
        Set(ByVal value As Integer)
            Right = Left + value
        End Set
    End Property
    Public Property Height() As Integer
        Get
            Return Bottom - Top
        End Get
        Set(ByVal value As Integer)
            Bottom = Top + value
        End Set
    End Property
    Public ReadOnly Property Centre() As Point
        Get
            Return New Point(Left + Width \ 2, Top + Height \ 2)
        End Get
    End Property
    Public ReadOnly Property Location() As Point
        Get
            Return New Point(Left, Top)
        End Get
    End Property
    Public ReadOnly Property Size() As Size
        Get
            Return New Size(Width, Height)
        End Get
    End Property

    ''' <summary>
    ''' Offsets this rectangle by the given size.
    ''' </summary>
    ''' <param name="offsetSz">The size of the offset to apply to this rectangle
    ''' </param>
    Public Sub Offset(ByVal offsetSz As Size)
        Offset(offsetSz, True)
    End Sub

    ''' <summary>
    ''' Offsets this rectangle by the given size.
    ''' </summary>
    ''' <param name="offsetSz">The size of the offset to apply to this rectangle
    ''' </param>
    ''' <param name="positive">True to apply the offset positively; False to
    ''' apply it negatively. ie. True would cause {5,5} to offset a rectangle
    ''' {10, 10, 100, 30} to {15, 15, 100, 30}; False would cause it to offset
    ''' to {5, 5, 100, 30}
    ''' </param>
    Public Sub Offset(ByVal offsetSz As Size, ByVal positive As Boolean)
        Offset(CType(offsetSz, Point), positive)
    End Sub

    ''' <summary>
    ''' Offsets this rectangle by the given point.
    ''' </summary>
    ''' <param name="offsetPt">The offset which should be applied to this
    ''' rectangle </param>
    Public Sub Offset(ByVal offsetPt As Point)
        Offset(offsetPt, True)
    End Sub

    ''' <summary>
    ''' Offsets this rectangle by the given point, possibly negatively.
    ''' </summary>
    ''' <param name="offset">The offset which should be applied to this
    ''' rectangle</param>
    ''' <param name="positive">True to apply the offset positively; False to
    ''' apply it negatively. ie. True would cause {5,5} to offset a rectangle
    ''' {10, 10, 100, 30} to {15, 15, 100, 30}; False would cause it to offset
    ''' to {5, 5, 100, 30} (in these examples, the rectangles are described in
    ''' the more typical <see cref="Rectangle"/> style: {X, Y, Width, Height})
    ''' </param>
    Public Sub Offset(ByVal offset As Point, ByVal positive As Boolean)
        Dim mult As Integer = CInt(IIf(positive, 1, -1))
        Left += offset.X * mult
        Right += offset.X * mult
        Top += offset.Y * mult
        Bottom += offset.Y * mult
    End Sub

    ''' <summary>
    ''' Checks if this RECT contains the given point.
    ''' </summary>
    ''' <param name="pt">The point to check against this rect.</param>
    ''' <returns>True if the given point falls within this rectangle (including
    ''' on the borders of the rectangle); False if it falls outside it </returns>
    Public Function Contains(ByVal pt As Point) As Boolean
        Return pt.X >= Left AndAlso pt.X <= Right _
         AndAlso pt.Y >= Top AndAlso pt.Y <= Bottom
    End Function

    ''' <summary>
    ''' Expands this RECT by the given size - note that this only alters the
    ''' bottom or right of the RECT, it does not expand it up or left.
    ''' </summary>
    ''' <param name="sz">The amount by which the RECT should be expanded.</param>
    Public Sub Expand(ByVal sz As Size)
        Expand(sz.Width, sz.Height)
    End Sub

    ''' <summary>
    ''' Expands this RECT by the given size - note that this only alters the
    ''' bottom or right of the RECT, it does not expand it up or left.
    ''' </summary>
    ''' <param name="w">The width by which the rectangle should expand.</param>
    ''' <param name="h">The height by which the rectangle should expand.</param>
    Public Sub Expand(ByVal w As Integer, ByVal h As Integer)
        Right += w
        Bottom += h
    End Sub

    ''' <summary>
    ''' Amends size of this RECT using the padding specified
    ''' </summary>
    ''' <param name="padding"></param>
    ''' <remarks></remarks>
    Public Sub Pad(padding As Padding)
        Top -= padding.Top
        Right += padding.Right
        Bottom += padding.Bottom
        Left -= padding.Left
    End Sub

    ''' <summary>
    ''' Gets a string representation of this rectangle
    ''' </summary>
    ''' <returns>This rectangle encoded into a string</returns>
    Public Overrides Function ToString() As String
        Return String.Format("RECT:{0},{1},{2},{3}", Left, Top, Bottom, Right)
    End Function


    ''' <summary>
    ''' Checks if this object is equal to the given object.
    ''' </summary>
    ''' <param name="obj">The object to test for equality against</param>
    ''' <returns>True if the given object is a RECT or Rectangle structure with
    ''' the same value as this object.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        If TypeOf obj Is RECT Then Return Equals(DirectCast(obj, RECT))
        If TypeOf obj Is Rectangle Then Return Equals(DirectCast(obj, Rectangle))
        Return False
    End Function

    ''' <summary>
    ''' Checks if this object is equal to the given object.
    ''' </summary>
    ''' <param name="r">The RECT to test for equality against</param>
    ''' <returns>True if the given RECT structure has the same value as this
    ''' object.</returns>
    Public Overloads Function Equals(ByVal r As RECT) As Boolean
        Return (r.Top = Me.Top AndAlso r.Right = Me.Right AndAlso _
         r.Bottom = Me.Bottom AndAlso r.Left = Me.Left)
    End Function

    ''' <summary>
    ''' Gets an integer hash representing this RECT structure.
    ''' </summary>
    ''' <returns>A hash of the value of this RECT</returns>
    Public Overrides Function GetHashCode() As Integer
        Return (Top << 24 Or Right << 16 << Bottom << 8 Or Left) Xor &HEFE
    End Function

    ''' <summary>
    ''' Parses a string representation of this rectangle into a rectangle
    ''' </summary>
    ''' <param name="str">The string to parse. In order to be successful, this
    ''' should be in the format defined by RECT.ToString() - ie.
    ''' "RECT: {Left},{Top},{Bottom},{Right}". The 'RECT:' is optional, a space
    ''' in between it and the numbers is ignored. Extraneaous whitespace is
    ''' trimmed from the string before attempting parse.</param>
    ''' <returns>The rectangle created from the given string</returns>
    Public Shared Function Parse(ByVal str As String) As RECT
        Dim r As RECT
        If TryParse(str, r) Then Return r
        ' Failed to parse. Downer.
        Throw New InvalidFormatException("Invalid rectangle string '{0}'", str)
    End Function

    ''' <summary>
    ''' Attempts to parse a string into a RECT structure
    ''' </summary>
    ''' <param name="str">The string to parse. In order to be successful, this
    ''' should be in the format defined by RECT.ToString() - ie.
    ''' "RECT:{Left},{Top},{Bottom},{Right}". The 'RECT:' is optional, all space
    ''' characters in the string are ignored.</param>
    ''' <param name="r">The rectangle parsed from the given string, this will be
    ''' <see cref="RECT.Empty"/> if parsing failed.</param>
    ''' <returns>True if the parse was successful, False otherwise.</returns>
    Public Shared Function TryParse( _
     ByVal str As String, ByRef r As RECT) As Boolean
        If str = "" Then r = RECT.Empty : Return False
        str = str.Replace(" ", "") ' Strip all space chars

        Dim rx As New Regex("^(?:RECT:)?(\d+),(\d+),(\d+),(\d+)$", RegexOptions.None, DefaultRegexTimeout)
        Dim m As Match = rx.Match(str)
        If Not m.Success Then r = RECT.Empty : Return False

        r = New RECT( _
         CInt(m.Groups(1).Value), _
         CInt(m.Groups(4).Value), _
         CInt(m.Groups(2).Value), _
         CInt(m.Groups(3).Value))
        Return True

    End Function

    ''' <summary>
    ''' Converts the given RECT into a Rectangle structure.
    ''' This is widening since all RECT values can be safely represented as
    ''' a Rectangle. As such, the compiler will implicitly convert a RECT to
    ''' a Rectangle without an explicit cast.
    ''' </summary>
    ''' <param name="val">The RECT value to convert</param>
    ''' <returns>A Rectangle with the same value as the given RECT</returns>
    Public Shared Widening Operator CType(ByVal val As RECT) As Rectangle
        Return New Rectangle(val.Left, val.Top, val.Width, val.Height)
    End Operator

    ''' <summary>
    ''' Converts the given Rectangle into a RECT structure.
    ''' This is widening since all Rectangle values can be safely represented as
    ''' a RECT. As such, the compiler will implicitly convert a Rectangle to
    ''' a RECT without an explicit cast.
    ''' </summary>
    ''' <param name="val">The Rectangle value to convert</param>
    ''' <returns>A RECT with the same value as the given Rectangle</returns>
    Public Shared Widening Operator CType(ByVal val As Rectangle) As RECT
        Return New RECT(val.Left, val.Right, val.Top, val.Bottom)
    End Operator

End Structure
