Imports BluePrism.AutomateAppCore
Imports LiveCharts

Namespace Controls.Charts
    ''' <summary>
    ''' Introduces a common object throughout charts.
    ''' This can be extracted and queried against outside the scope of the chart's class. 
    ''' Abstract inheritance could not have been used due to xaml behaviour and UserControl
    ''' </summary>
    Public Interface IBaseChart
        Property ChartTitle As String
        Property ChartSubTitle As String
        Property SeriesCollection As SeriesCollection
        Property SharedMethods As SharedFunctions
        Property TileSize As TileSize

        ReadOnly Property HasChartData As Boolean

        Sub SetTileSize(s As Size)
        Sub CreateChartData(procedure As String, parameters As Dictionary(Of String, String),
                                    tile As Tile)
    End Interface

    <Flags>
    Public Enum TileSize
        Small = 1
        Medium = 2
        Large = 3
    End Enum
End Namespace
