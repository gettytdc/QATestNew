Imports System.Data.SqlClient
Imports BluePrism.Data

Public Class StoredProcedureObjectValueDataSource
    Private ReadOnly mDataSourceName As String

    Public Sub New(dataSourceName As String)
        mDataSourceName = dataSourceName
    End Sub

    Public Function GetChartData(con As IDatabaseConnection, params As Dictionary(Of String, Object)) As DataTable

        Using cmd As New SqlCommand(mDataSourceName)
            cmd.CommandType = CommandType.StoredProcedure
            For Each param As KeyValuePair(Of String, Object) In params
                cmd.Parameters.AddWithValue(param.Key, param.Value)
            Next
            Return con.ExecuteReturnDataTable(cmd)
        End Using

    End Function
End Class
