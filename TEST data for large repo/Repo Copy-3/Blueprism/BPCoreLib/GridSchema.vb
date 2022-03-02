Imports System.Drawing
Imports System.ComponentModel
Imports System.Text.RegularExpressions

Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Public Class GridSchema

#Region " Class Scope Declarations "

    ''' <summary>
    ''' Regex for capturing 2 groups of comma-separated numbers, the groups being
    ''' separated by a semicolon.
    ''' </summary>
    Private Shared ReadOnly sRowColRegex As New Regex("^([\d, %]+);([\d, %]+)$", RegexOptions.None, DefaultRegexTimeout)

    ''' <summary>
    ''' Regex for capturing a comma separated set of integers. 
    ''' Group 1 has a single capture, the first value (reqd).
    ''' Group 2 has the remaining captures representing the other values.
    ''' Whitespace and commas between values are filtered out of the captures
    ''' </summary>
    Private Shared ReadOnly sValueSetRegex As New Regex("^(\d+%?)(?:,\s*(\d+%?))*$", RegexOptions.None, DefaultRegexTimeout)

    ''' <summary>
    ''' Extracts the ascending list of non-duplicate values from the given string
    ''' </summary>
    ''' <param name="str">The string from which a comma-separated list of integer
    ''' values should be extracted</param>
    ''' <returns>A list of integers representing the values found in the given string
    ''' </returns>
    ''' <exception cref="InvalidFormatException">If the given string was in an
    ''' invalid format - ie. it did not contain a comma separated list of integers,
    ''' or if the integer list found contained duplicates or out of order values.
    ''' </exception>
    Private Shared Function ExtractValuesInto(Of T As {GridVector, New})( _
     ByVal str As String, ByVal vectors As IList(Of T)) As IList(Of T)

        Dim m As Match = sValueSetRegex.Match(str)
        If Not m.Success Then Throw New InvalidFormatException(
         My.Resources.GridSchema_CannotParseTheValue0IntoASetOfIntegers, str)

        ' Go through each capture in each group and convert them into vectors
        ' Omit group 1 (ie. the entire string matching the regex)
        For i As Integer = 1 To m.Groups.Count - 1
            For Each c As Capture In m.Groups(i).Captures
                Dim strVal As String = c.Value
                If strVal.Length = 0 Then Continue For
                Dim proportional As Boolean = False
                If strVal(strVal.Length - 1) = "%"c Then
                    proportional = True
                    strVal = strVal.Left(strVal.Length - 1)
                End If
                Dim val As Integer
                If Not Integer.TryParse(strVal, val) Then
                    Throw New InvalidFormatException(
                     My.Resources.GridSchema_CannotConvert0IntoARowColumnFromValue1,
                     c.Value, str)
                End If

                Dim v As New T()
                If proportional _
                 Then v.SizeType = VectorSizeType.Proportional _
                 Else v.SizeType = VectorSizeType.Absolute
                v.Value = val
                vectors.Add(v)
            Next
        Next

        Return vectors

    End Function

#End Region

#Region "- Members and Event Definitions -"

    ''' <summary>
    ''' Event fired when the schema is changed
    ''' </summary>
    Public Event SchemaChanged As EventHandler

    ' The list of rows defined in this schema
    Private _rows As IList(Of GridRow)

    ' The list of columns defined in this schema
    Private _cols As IList(Of GridColumn)

#End Region

#Region "- Constructors -"

    ''' <summary>
    ''' Creates a new schema for a grid spy region with 2 rows and 2 columns,
    ''' each occupying 50% of their respective measurements
    ''' </summary>
    Public Sub New()
        Me.New(2, 2)
    End Sub

    ''' <summary>
    ''' Creates a new schema for a grid spy region with the given number of
    ''' rows and columns.
    ''' </summary>
    ''' <param name="cols">The number of columns to create in the new schema.
    ''' <param name="rows">The number of rows to create in the new schema.</param>
    ''' </param>
    Public Sub New(ByVal cols As Integer, ByVal rows As Integer)
        _rows = New List(Of GridRow)()
        _cols = New List(Of GridColumn)()
        RowCount = rows
        ColumnCount = cols
    End Sub


    ''' <summary>
    ''' Creates a new grid schema from the given encoded string representation.
    ''' </summary>
    ''' <param name="schema">The schema as a string. The schema should be a set of
    ''' comma-separated column sizes, followed by a semi-colon, followed by a
    ''' set of comma-separated row sizes. Neither set should contain a 0, since
    ''' the first value is treated as the first non-origin column index. Equally,
    ''' the last value is treated as the outside of the grid - ie. the right or
    ''' bottom edge of the last column or row, respectively.</param>
    ''' <exception cref="InvalidFormatException">If the given string was in an
    ''' invalid format - ie. it did not contain a comma separated list of
    ''' integers, or if the integer list found contained duplicates or out of
    ''' order values.</exception>
    Public Sub New(ByVal schema As String)
        Dim m As Match = sRowColRegex.Match(schema)
        If Not m.Success Then Throw New InvalidFormatException(
         My.Resources.GridSchema_CannotParse0AsAGridSchema, schema)

        _cols = ExtractValuesInto(m.Groups(1).Value, New List(Of GridColumn))
        _rows = ExtractValuesInto(m.Groups(2).Value, New List(Of GridRow))
    End Sub

#End Region

#Region "- Properties -"

    ''' <summary>
    ''' The list of rows in this schema
    ''' </summary>
    <Browsable(False)>
    Public ReadOnly Property Rows() As IList(Of GridRow)
        Get
            Return _rows
        End Get
    End Property

    ''' <summary>
    ''' The list of columns in this schema
    ''' </summary>
    <Browsable(False)>
    Public ReadOnly Property Columns() As IList(Of GridColumn)
        Get
            Return _cols
        End Get
    End Property

    ''' <summary>
    ''' The number of rows described in this schema
    ''' </summary>
    <Description("The number of rows described in this schema")>
    Public Property RowCount() As Integer
        Get
            Return _rows.Count
        End Get
        Set(ByVal value As Integer)
            SetSize(_rows, value)
        End Set
    End Property

    ''' <summary>
    ''' The number of columns described in this schema
    ''' </summary>
    <Description("The number of columns described in this schema")>
    Public Property ColumnCount() As Integer
        Get
            Return _cols.Count
        End Get
        Set(ByVal value As Integer)
            SetSize(_cols, value)
        End Set
    End Property

    ''' <summary>
    ''' This schema encoded into a string
    ''' </summary>
    <Browsable(False)>
    Public ReadOnly Property EncodedValue() As String
        Get
            Return String.Format("{0};{1}",
             CollectionUtil.Join(_cols, ","), CollectionUtil.Join(_rows, ","))
        End Get
    End Property

#End Region

#Region "- Column/Row Count Manipulation Methods -"

    ''' <summary>
    ''' Adds a row to the end of this schema - typically bisecting the last row to
    ''' make space for it
    ''' </summary>
    Public Sub AddRow()
        RowCount += 1
    End Sub

    ''' <summary>
    ''' Adds a column to the end of this schema
    ''' </summary>
    Public Sub AddColumn()
        ColumnCount += 1
    End Sub

    ''' <summary>
    ''' Deletes the last row of this schema
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If there is only one row
    ''' defined in the schema when this method is called</exception>
    Public Sub DeleteLastRow()
        If RowCount < 2 Then Throw New InvalidOperationException(
         My.Resources.GridSchema_AtLeast1RowMustExistInTheSchema)
        RowCount -= 1
    End Sub

    ''' <summary>
    ''' Deletes the last column of this schema
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If there is only one column
    ''' defined in the schema when this method is called</exception>
    Public Sub DeleteLastColumn()
        If ColumnCount < 2 Then Throw New InvalidOperationException(
         My.Resources.GridSchema_AtLeast1RowMustExistInTheSchema)
        ColumnCount -= 1
    End Sub

    ''' <summary>
    ''' Sets the size of the given list of vectors to the new size, adding or
    ''' removing vectors as appropriate.
    ''' </summary>
    ''' <typeparam name="T">The type of vector - a subclass of
    ''' <see cref="GridVector"/> with a parameterless constructor.</typeparam>
    ''' <param name="vectors">The list of vectors</param>
    ''' <param name="size">The size that the vector list should be set to.</param>
    Private Sub SetSize(Of T As {GridVector, New})(
     ByVal vectors As IList(Of T), ByVal size As Integer)
        If size < 0 Then
            size = 0
        End If

        ' Figure out if we currently have too many / too few and
        ' either remove them or add new ones as appropriate
        Dim currSize As Integer = vectors.Count
        Dim changed As Boolean = (size <> currSize)

        Dim tooManyBy As Integer = currSize - size
        While System.Threading.Interlocked.Decrement(tooManyBy) >= 0
            vectors.RemoveAt(vectors.Count - 1)
        End While

        Dim tooFewBy As Integer = size - currSize
        While System.Threading.Interlocked.Decrement(tooFewBy) >= 0
            vectors.Add(New T())
        End While
        If changed Then
            OnSchemaChanged(EventArgs.Empty)
        End If
    End Sub

#End Region

#Region "- Other Methods -"

    ''' <summary>
    ''' Throws an out of range exception with the given properties
    ''' </summary>
    ''' <param name="argName">The name of the out of range argument</param>
    ''' <param name="type">The type of number it represents</param>
    ''' <param name="count">The count of elements that the argument should fall
    ''' within.</param>
    Private Sub ThrowOutOfRange(
     ByVal argName As String, ByVal type As String, ByVal count As Integer)
        Throw New ArgumentOutOfRangeException(argName,
         String.Format(My.Resources.GridSchema_The0NumberMustBeBetween0And1, type, count - 1))
    End Sub

    ''' <summary>
    ''' Gets the rectangle corresponding to the given column and row number.
    ''' The column and row numbers are zero-indexed, so that (colNo=0,rowNo=0)
    ''' corresponds to the top left cell in the grid.
    ''' </summary>
    ''' <param name="colNo">The zero-based column number of the cell required
    ''' </param>
    ''' <param name="rowNo">The zero-based row number of the cell required
    ''' </param>
    ''' <param name="gridSize">The size of the whole grid governed by this schema -
    ''' used to divide up the proportional cells</param>
    ''' <returns>A rectangle which describes the cell in this schema
    ''' corresponding to the given column and row numbers.</returns>
    Public Function GetCell( _
     ByVal colNo As Integer, ByVal rowNo As Integer, ByVal gridSize As Size) _
     As Rectangle

        ' Sanity checking
        If colNo < 0 OrElse colNo > _cols.Count - 1 Then _
         ThrowOutOfRange("colNo", "column", _cols.Count)

        If rowNo < 0 OrElse rowNo > _rows.Count - 1 Then _
         ThrowOutOfRange("rowNo", "row", _rows.Count)

        ' The return area
        Dim area As New Rectangle()

        ' Get the absolute sizes of the cells, and increment a counter until we
        ' reach the target cell. If we haven't reached it add the size of the
        ' current cell to the area location and continue. When we do, set the
        ' size of the area to the current cell's size and exit
        Dim i As Integer = 0
        For Each width As Integer In GridVector.GetAbsolutes(_cols, gridSize.Width)
            If i >= colNo Then area.Width = width : Exit For
            area.X += width
            i += 1
        Next
        i = 0
        For Each height As Integer In GridVector.GetAbsolutes(_rows, gridSize.Height)
            If i >= rowNo Then area.Height = height : Exit For
            area.Y += height
            i += 1
        Next

        Return area

    End Function

    ''' <summary>
    ''' Gets the location of the cell with the given column and row number
    ''' </summary>
    ''' <param name="colNo">The zero-based column number of the cell required
    ''' </param>
    ''' <param name="rowNo">The zero-based row number of the cell required
    ''' </param>
    ''' <returns>A point describing the location of the cell referred to 
    ''' </returns>
    Public Function GetCellLocation(ByVal colNo As Integer, ByVal rowNo As Integer, _
     ByVal gridSize As Size) As Point
        Return GetCell(colNo, rowNo, gridSize).Location
    End Function

    ''' <summary>
    ''' Fires a <see cref="SchemaChanged"/> event with the given args.
    ''' </summary>
    Protected Overridable Sub OnSchemaChanged(ByVal e As EventArgs)
        RaiseEvent SchemaChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Gets a string representation of this schema
    ''' </summary>
    ''' <returns>"a x b" where 'a' represents the number of rows and 'b'
    ''' represents the number of columns defined in this schema</returns>
    Public Overrides Function ToString() As String
        Return String.Format("{1} x {0}", RowCount, ColumnCount)
    End Function

    ''' <summary>
    ''' Creates and returns a deep clone of this schema.
    ''' </summary>
    ''' <returns>A clone of this schema with the same values but a new set of
    ''' objects which are holding those values.</returns>
    Public Function Clone() As GridSchema
        ' Memberwise clone this schema into a copy
        Dim copy As GridSchema = _
         TryCast(MemberwiseClone(), GridSchema)

        ' Create new lists on the copy to hold the cols/rows and clone the
        ' contents from our lists into them
        copy._cols = New List(Of GridColumn)
        copy._rows = New List(Of GridRow)
        CollectionUtil.CloneInto(_cols, copy._cols)
        CollectionUtil.CloneInto(_rows, copy._rows)

        Return copy
    End Function

    ''' <summary>
    ''' Copies the schema details from the given schema, creating clones of all
    ''' affected objects so that there is no reference equality between this
    ''' schema and the given one.
    ''' </summary>
    ''' <param name="other">The schema from which to draw the row/column data
    ''' </param>
    ''' <exception cref="ArgumentNullException">If the given 'other' schema is
    ''' null.</exception>
    ''' <remarks>This method does nothing if the other schema passed is the same
    ''' object as the schema that the method is called on.</remarks>
    Public Sub CopySchemaFrom(ByVal other As GridSchema)

        ' Checks for silliness
        If Me Is other Then Return
        If other Is Nothing Then Throw New ArgumentNullException( _
         NameOf(other), "Cannot copy from a null schema")

        ' Clone the data over to our lists
        CollectionUtil.CloneInto(other._cols, _cols)
        CollectionUtil.CloneInto(other._rows, _rows)

        ' Raise a schema changed event for interested parties
        OnSchemaChanged(EventArgs.Empty)
    End Sub

#End Region

End Class
