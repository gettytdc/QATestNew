''' <summary>
''' Class used for building CSV files
''' </summary>
''' <remarks></remarks>
Public Class clsCSVCreator

    ''' <summary>
    ''' Serialised a datatable to the supplied stream.
    ''' </summary>
    ''' <param name="Writer">The stream writer to which the datatable should
    ''' be serialised. Eg this may correspond to a file stream.</param>
    ''' <param name="Table">The datatable to be serialised.</param>
    ''' <param name="IncludeHeaderRow">When true, the column headers will be written
    ''' out on the first row.</param>
    Public Shared Sub WriteDataTable(ByVal Writer As IO.StreamWriter, ByVal Table As DataTable, ByVal IncludeHeaderRow As Boolean)
        If IncludeHeaderRow Then
            For i As Integer = 0 To Table.Columns.Count - 1
                WriteItem(Writer, Table.Columns(i).ColumnName)
                If i < Table.Columns.Count - 1 Then
                    Writer.Write(",")
                Else
                    Writer.Write(vbCrLf)
                End If
            Next
        End If

        For Each Row As DataRow In Table.Rows
            For i As Integer = 0 To Table.Columns.Count - 1
                WriteItem(Writer, Row(i).ToString)
                If i < Table.Columns.Count - 1 Then
                    Writer.Write(",")
                Else
                    Writer.Write(vbCrLf)
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Writes a csv data cell, escaping quotes and new lines where necessary.
    ''' </summary>
    ''' <param name="Writer">The writer to which the item should be written.</param>
    ''' <param name="Value">The value to be written.</param>
    Private Shared Sub WriteItem(ByVal Writer As System.IO.StreamWriter, ByVal Value As String)
        If Value.IndexOfAny(("""," & vbCrLf).ToCharArray) > -1 Then
            Writer.Write("""" & Value.Replace("""", """""") & """")
        Else
            Writer.Write(Value)
        End If
    End Sub

End Class
