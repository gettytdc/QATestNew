Imports System.Windows.Controls
Imports System.Windows.Media
Imports BluePrism.AutomateAppCore
Imports LiveCharts
Imports LiveCharts.Wpf

Namespace Controls.Charts
    Partial Public Class DoughnutChart
        Inherits UserControl
        Implements ICustomLegend, IBaseChart

        Public Sub New(size As Size, tile As Tile, procedure As String,
                       parameters As Dictionary(Of String, String))
            InitializeComponent()
            DataContext = Me
            CreateChartData(procedure, parameters, tile)
            SetTileSize(size)

            Height = 350 * size.Height
            Width = 555 * size.Width
        End Sub

        Public Property PointLabel As Func(Of ChartPoint, String) = Function(chartPoint) $"{chartPoint.Y}"

        Private Sub SetTileSize(s As Size) Implements IBaseChart.SetTileSize
            TileSize = SharedMethods.SetTileSize(s)
        End Sub
        Public Property TileSize As _
            TileSize Implements IBaseChart.TileSize
        Public Property LegendItems As New List(Of LegendItem) _
            Implements ICustomLegend.LegendItems
        Private Property MaxItemsInSeries As Integer = 6 _
            Implements ICustomLegend.MaxItemsInSeries

        Public Property ChartTitle As String _
            Implements IBaseChart.ChartTitle
        Public Property ChartSubTitle As String _
            Implements IBaseChart.ChartSubTitle
        Public Property SeriesCollection As New SeriesCollection _
            Implements IBaseChart.SeriesCollection
        Public Property SharedMethods As New SharedFunctions _
            Implements IBaseChart.SharedMethods
        Public ReadOnly Property HasChartData As Boolean Implements IBaseChart.HasChartData
            Get
                Return SharedMethods.HasDataToDisplay(SeriesCollection)
            End Get
        End Property

        Private Sub CreateChartData(procedure As String, parameters As Dictionary(Of String, String),
                                    tile As Tile) Implements IBaseChart.CreateChartData
            If tile Is Nothing Then
                Throw New ArgumentNullException(NameOf(tile))
            End If

            Dim chartData As DataTable
            SeriesCollection = New SeriesCollection
            ChartTitle = tile.Name
            ChartSubTitle = tile.Description

            Try                
                chartData = gSv.GetChartData(procedure, parameters)
                If chartData Is Nothing Then Throw New ChartNoDataException()

                PointLabel = Function(chartPoint) String.Format("{0}", chartPoint.Y)

                For Each dataRow As DataRow In chartData.AsEnumerable.Take(MaxItemsInSeries)

                    Dim chartValue As Double = 0.0
                    Double.TryParse(dataRow.Item(1).ToString(), chartValue)

                    SeriesCollection.Add(New PieSeries() With {
                                            .Title = dataRow.Item(0).ToString,
                                            .Values = New ChartValues(Of Double) From {chartValue},
                                            .DataLabels = False,
                                            .LabelPoint = PointLabel
                                            })
                Next

                ' overwrite the last segment with Other
                If chartData.AsEnumerable.Count > MaxItemsInSeries Then
                    Dim sumRest As Double = chartData.AsEnumerable.
                        Skip(SeriesCollection.Count - 1).
                        Sum(Function(x)
                                Dim val As Double = 0.0
                                Double.TryParse(x.Item(1).ToString(), val)
                                Return val
                            End Function)

                    ' this is necessary due to a null reference exception inside LVC 
                    ' being thrown when overwriting the last Series in SeriesCollection
                    ' and Title being readonly
                    Dim sc As New SeriesCollection()
                    sc.AddRange(SeriesCollection.Take(MaxItemsInSeries - 1))
                    sc.Add(New PieSeries() With {
                                         .Title = My.Resources.Other,
                                         .Values = New ChartValues(Of Double) From {sumRest},
                                         .DataLabels = False,
                                         .LabelPoint = PointLabel
                                         })
                    SeriesCollection = sc
                End If

                LegendItems = SharedMethods.GenerateLegendList(chartData,
                                                 SeriesCollection,
                                                 ChartTypes.Pie,
                                                 CType(FindResource("SeriesColors"), ColorsCollection),
                                                 MaxItemsInSeries)

                If Not HasChartData Then Throw New ChartNoDataException()
            Catch ex As ChartNoDataException
                ChartSubTitle = My.Resources.NoChartDataAvailable
            Catch ex As Exception
                ' clear any series data so that no data image displays
                SeriesCollection = New SeriesCollection() '.clear() throws null except?
                LegendItems.Clear()

                Dim messages = ex.Message.Split(Environment.NewLine.ToCharArray()).
                    Select(Function(message) SharedMethods.TranslateErrorMessage(message))
                Dim errorMessage = String.Join(Environment.NewLine, messages)
                ChartSubTitle = String.Format(My.Resources.Error0, errorMessage)
            End Try
        End Sub
    End Class
End Namespace
