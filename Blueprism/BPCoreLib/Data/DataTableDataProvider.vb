Imports BluePrism.Server.Domain.Models

Namespace Data

    Public Class DataTableDataProvider : Inherits BaseMultipleDataProvider

        ' The table from which the data is being provided
        Private mTable As DataTable

        ' The current row number
        Private mRowNo As Integer = -1

        ''' <summary>
        ''' Creates a new data provider for the given data table.
        ''' </summary>
        ''' <param name="dt">The table to draw the data from</param>
        Public Sub New(ByVal dt As DataTable)
            If dt Is Nothing Then Throw New ArgumentNullException(NameOf(dt))
            mTable = dt
        End Sub

        ''' <summary>
        ''' Gets the value, indexed by the given name in the current row of the table
        ''' </summary>
        ''' <param name="name">The name of the item for which the value is required.
        ''' </param>
        ''' <returns>The value of the item</returns>
        ''' <exception cref="InvalidStateException">If this provider has provided all
        ''' of the data available in the table.</exception>
        Protected Overrides Function GetItem(ByVal name As String) As Object
            If Not mTable.Columns.Contains(name) Then Return Nothing
            Return mTable.Rows(mRowNo)(name)
        End Function

        ''' <summary>
        ''' Attempts to move to the next row in the table.
        ''' </summary>
        ''' <returns>True if a row containing data was moved to; False if there is
        ''' no more data to traverse in this table.</returns>
        Protected Overrides Function InnerMoveNext() As Boolean
            Dim rowCount As Integer = mTable.Rows.Count
            If mRowNo >= rowCount Then Return False
            mRowNo += 1
            If mRowNo >= rowCount Then Return False
        End Function

    End Class

End Namespace
