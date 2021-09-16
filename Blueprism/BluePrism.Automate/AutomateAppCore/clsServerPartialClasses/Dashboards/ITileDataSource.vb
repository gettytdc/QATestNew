Imports BluePrism.Data

Namespace clsServerPartialClasses.Dashboards

    ''' <summary>
    ''' Provides data displayed within a dashboard tile 
    ''' </summary>
    Friend Interface ITileDataSource

        ''' <summary>
        ''' Loads the data
        ''' </summary>
        ''' <param name="con">The database connection</param>
        ''' <param name="params">Parameters to include in the query</param>
        ''' <returns>A DataTable containing the data loaded</returns>
        Function GetChartData(con As IDatabaseConnection, params As Dictionary(Of String, String)) As DataTable
    End Interface

End Namespace
