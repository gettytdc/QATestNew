Imports BluePrism.Server.Domain.Models

''' <summary>
''' Structure to encapsulate session dimentions the maximum number of rows and
''' columns
''' </summary>
Public Structure SessionDimensions

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="r">Rows</param>
    ''' <param name="c">Columns</param>
    Public Sub New(r As Integer, c As Integer)
        Rows = r
        Columns = c
    End Sub

    ''' <summary>
    ''' The maximum number of rows in the session
    ''' </summary>
    Public Rows As Integer

    ''' <summary>
    ''' The maximum number of columns in the session
    ''' </summary>
    Public Columns As Integer

    ''' <summary>
    ''' Returns true if the location given by row index and column index is within
    ''' the bounds of the session
    ''' </summary>
    ''' <param name="r">The row index</param>
    ''' <param name="c">The column index</param>
    Public Function Contains(r As Integer, c As Integer) As Boolean
        Return r <= Rows AndAlso c <= Columns AndAlso r > 0 AndAlso c > 0
    End Function

    ''' <summary>
    ''' Throws an exception if the location given by row index and column index is
    ''' not within the bounds of the session
    ''' </summary>
    ''' <param name="r">The row index</param>
    ''' <param name="c">The column index</param>
    Public Sub CheckContains(r As Integer, c As Integer)
        If Not Contains(r, c) Then
            Dim which As String = Nothing
            Dim val = 0
            If r > Rows OrElse r <= 0 Then
                which = My.Resources.Row
                val = r
            End If
            If c > Columns OrElse c <= 0 Then
                which = My.Resources.Column
                val = c
            End If
            Throw New OutOfRangeException(My.Resources.The0Index1IsOutsideTheBoundaryOfThe2TerminalEmulatorScreen, which, val, ToString())
        End If
    End Sub

    ''' <summary>
    ''' Converts the session dimentions to a string
    ''' </summary>
    Public Overrides Function ToString() As String
        Return String.Format(My.Resources.x0X1, Columns, Rows)
    End Function

End Structure
