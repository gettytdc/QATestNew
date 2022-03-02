Imports System.Data.SqlClient
Imports BluePrism.Data

Namespace clsServerPartialClasses.Dashboards

    ''' <summary>
    ''' Loads data from a stored procedure based on the name of the data source
    ''' </summary>
    Friend Class StoredProcedureDataSource
        Implements ITileDataSource

        Private ReadOnly mDataSourceName As String

        ''' <summary>
        ''' Creates a new data source
        ''' </summary>
        ''' <param name="dataSourceName">The name of the data source</param>
        Public Sub New(dataSourceName As String)
            mDataSourceName = dataSourceName
        End Sub

        ''' <summary>
        ''' Loads the chart data
        ''' </summary>
        ''' <param name="con">The database connection</param>
        ''' <param name="params">Any parameters specified</param>
        ''' <returns>A DataTable with the results of the query</returns>
        Public Function GetChartData(con As IDatabaseConnection, params As Dictionary(Of String, String)) As DataTable Implements ITileDataSource.GetChartData

            Using cmd As New SqlCommand(mDataSourceName)
                cmd.CommandType = CommandType.StoredProcedure
                For Each param As KeyValuePair(Of String, String) In params
                    cmd.Parameters.AddWithValue(param.Key, param.Value)
                Next
                Return con.ExecuteReturnDataTable(cmd)
            End Using

        End Function
    End Class
End Namespace
