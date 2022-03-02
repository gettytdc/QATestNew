Imports System.Drawing

''' <summary>
''' Represents a field in a terminal emulator.
''' </summary>
Public Class clsTerminalField

    ''' <summary>
    ''' The starting column of the field, in grid coordinates.
    ''' </summary>
    Public StartColumn As Integer

    ''' <summary>
    ''' The starting row of the field, in grid coordinates.
    ''' </summary>
    Public StartRow As Integer

    ''' <summary>
    ''' The end column of the field, in grid coordinates.
    ''' </summary>
    Public EndColumn As Integer

    ''' <summary>
    ''' The end row of the field, in grid coordinates.
    ''' </summary>
    Public EndRow As Integer

    ''' <summary>
    ''' The dimensions of the grid, in rows and columns.
    ''' </summary>
    Public SessionSize As SessionDimensions

    ''' <summary>
    ''' The type of this field.
    ''' </summary>
    Public FieldType As FieldTypes

    ''' <summary>
    ''' The text contained in this field.
    ''' </summary>
    Public FieldText As String

    ''' <summary>
    ''' Indicates whether the field is highlighted.
    ''' </summary>
    Public Property Highlighted() As Boolean
        Get
            Return bHighlighted OrElse PermanentlyHighlighted
        End Get
        Set(ByVal value As Boolean)
            bHighlighted = value
        End Set
    End Property
    Private bHighlighted As Boolean


    ''' <summary>
    ''' If True, overrides the Highlighted setting and draws as highlighted
    ''' regardless.
    ''' </summary>
    Public PermanentlyHighlighted As Boolean


    Public Sub New(sessionSize As SessionDimensions)
        Me.SessionSize = sessionSize
    End Sub

    Public Sub New(sessionSize As SessionDimensions, fieldType As FieldTypes, startRow As Integer, startCol As Integer, endRow As Integer, endCol As Integer)
        Me.New(sessionSize)
        Me.FieldType = fieldType
        Me.StartRow = startRow
        Me.StartColumn = startCol
        Me.EndRow = endRow
        Me.EndColumn = endCol
    End Sub

    ''' <summary>
    ''' The width of the field, if it is a rectangle; returns the length if it is
    ''' mulitilinewrapped.
    ''' </summary>
    Public ReadOnly Property Width() As Integer
        Get
            Select Case FieldType
                Case FieldTypes.Rectangular
                    Return 1 + (EndColumn - StartColumn)
                Case FieldTypes.MultilineWrapped
                    Dim intermediateRows As Integer = Me.Height - 2
                    If intermediateRows < 0 Then
                        Return 1 + EndColumn - StartColumn
                    Else
                        Return Me.Length
                    End If
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Returns the height of the field. That is, the difference between the
    ''' end location y  coordinate and the start location y coordinate.
    ''' </summary>
    Public ReadOnly Property Height() As Integer
        Get
            Return 1 + (EndRow - StartRow)
        End Get
    End Property

    ''' <summary>
    ''' Returns the length of this field.
    ''' </summary>
    ''' <returns>Returns the length of this field, i.e. the number of cells between
    ''' the startlocation and endlocation, inclusive.</returns>
    ''' <remarks>Relevant only for multiline wrapped fields. If not a multiline
    ''' wrapped field, then the field will be treated as such for the purposes
    ''' of this method.</remarks>
    Public ReadOnly Property Length() As Integer
        Get
            Dim intermediateRows As Integer = Me.Height - 2
            Dim firstRowLength, endRowLength As Integer
            Select Case intermediateRows
                Case Is >= 0
                    firstRowLength = SessionSize.Columns - StartColumn + 1
                    endRowLength = EndColumn
                Case Else
                    'We have a single row field
                    intermediateRows = 0
                    firstRowLength = Me.Width
                    endRowLength = 0
            End Select

            Return (intermediateRows * SessionSize.Columns) + firstRowLength + endRowLength
        End Get
    End Property

    Public Enum FieldTypes
        ''' <summary>
        ''' Default. A field which has rectangular shape.
        ''' </summary>
        Rectangular
        ''' <summary>
        ''' A field which may be viewed as a start point
        ''' and a length, wrapping across as many lines
        ''' as necessary.
        ''' </summary>
        MultilineWrapped
    End Enum


    ''' <summary>
    ''' Clears the field coordinates.
    ''' </summary>
    Public Sub Clear()
        StartRow = 0
        StartColumn = 0
        EndRow = 0
        EndColumn = 0
    End Sub


    ''' <summary>
    ''' Indicates whether a point is inside the field.
    ''' </summary>
    ''' <param name="p">The grid point, ranging from (1,1) to (GridSize.Width,
    ''' GridSize.Height)</param>
    Public Function ContainsGridPoint(ByVal p As Point) As Boolean

        Dim r As New Rectangle(StartColumn, StartRow, Width, Height)
        Select Case FieldType
            Case FieldTypes.Rectangular
                Return r.Contains(p)
            Case FieldTypes.MultilineWrapped
                Dim intermediateRows As Integer = Me.Height - 2
                Select Case intermediateRows
                    Case Is < 0
                        Return r.Contains(p)
                    Case Else
                        Dim r1 As New Rectangle(StartColumn, StartRow, 1 + SessionSize.Columns - StartColumn, 1)
                        Dim r2 As New Rectangle(New Point(0, StartRow + 1), New Size(SessionSize.Columns, intermediateRows))
                        Dim r3 As New Rectangle(New Point(0, EndRow), New Size(EndColumn, 1))
                        Return r1.Contains(p) OrElse r2.Contains(p) OrElse r3.Contains(p)
                End Select
        End Select

        Return False
    End Function


    Public Function IsEmpty() As Boolean
        Return (Me.Width = 0) OrElse (StartRow > EndRow)
    End Function


    ''' <summary>
    ''' Draws the field rectangle on the supplied graphics object.
    ''' </summary>
    ''' <param name="g">The graphics object to draw with.</param>
    ''' <param name="p">The pen to draw with.</param>
    Public Sub Draw(ByVal g As Graphics, ByVal p As Pen, ByVal GridUnitWidth As Double, ByVal GridUnitHeight As Double)
        If Not ((StartColumn = 0 AndAlso StartRow = 0) OrElse (EndColumn = 0 AndAlso EndRow = 0)) Then
            Dim Poly As Point() = Nothing

            Select Case Me.FieldType
                Case FieldTypes.Rectangular
                    Poly = GetPixelRectangle(GridUnitWidth, GridUnitHeight)
                Case FieldTypes.MultilineWrapped
                    Dim IntermediateRows As Integer = Me.Height - 2
                    Select Case IntermediateRows
                        Case Is < 0
                            Poly = GetPixelRectangle(GridUnitWidth, GridUnitHeight)
                        Case Else
                            Poly = Me.GetPixelMultilineShape(GridUnitWidth, GridUnitHeight)
                    End Select
            End Select

            g.DrawPolygon(p, Poly)
        End If
    End Sub


    Private Function GetPixelRectangle(ByVal GridUnitWidth As Double, ByVal GridUnitHeight As Double) As Point()
        Dim Poly(3) As Point
        'Each field is re-indexed from 1 to 0 because the *first* 
        'cell should be drawn at index *zero*.
        Poly(0) = New Point(CInt((Me.StartColumn - 1) * GridUnitWidth), CInt((Me.StartRow - 1) * GridUnitHeight))
        Poly(1) = New Point(CInt((Me.EndColumn) * GridUnitWidth), CInt((Me.StartRow - 1) * GridUnitHeight))
        Poly(2) = New Point(CInt((Me.EndColumn) * GridUnitWidth), CInt((Me.EndRow) * GridUnitHeight))
        Poly(3) = New Point(CInt((Me.StartColumn - 1) * GridUnitWidth), CInt((Me.EndRow) * GridUnitHeight))
        Return Poly
    End Function

    Private Function GetPixelMultilineShape(ByVal GridUnitWidth As Double, ByVal GridUnitHeight As Double) As Point()
        Dim Poly(7) As Point
        'Each field is re-indexed from 1 to 0 because the *first* 
        'cell should be drawn at index *zero*.
        Poly(0) = New Point(CInt((Me.StartColumn - 1) * GridUnitWidth), CInt((Me.StartRow - 1) * GridUnitHeight))
        Poly(1) = New Point(CInt((1 + Me.SessionSize.Columns) * GridUnitWidth), CInt((Me.StartRow - 1) * GridUnitHeight))
        Poly(2) = New Point(CInt((1 + Me.SessionSize.Columns) * GridUnitWidth), CInt((Me.EndRow - 1) * GridUnitHeight))
        Poly(3) = New Point(CInt(Me.EndColumn * GridUnitWidth), CInt((Me.EndRow - 1) * GridUnitHeight))
        Poly(4) = New Point(CInt(Me.EndColumn * GridUnitWidth), CInt(Me.EndRow * GridUnitHeight))
        Poly(5) = New Point(0, CInt(Me.EndRow * GridUnitHeight))
        Poly(6) = New Point(0, CInt(((Me.StartRow + 1) - 1) * GridUnitHeight))
        Poly(7) = New Point(CInt((Me.StartColumn - 1) * GridUnitWidth), CInt(((Me.StartRow + 1) - 1) * GridUnitHeight))
        Return Poly
    End Function


    Public Overrides Function ToString() As String
        Return StartColumn.ToString & "," & StartRow.ToString & "," & EndColumn.ToString() & "," & EndRow.ToString() & "," & FieldType.ToString()
    End Function

End Class
