Imports System.Windows.Controls
Imports BluePrism.AutomateAppCore
Imports LiveCharts
Imports LiveCharts.Wpf

Namespace Controls.Charts
    Partial Public Class BasicStackedColumnChart
        Inherits UserControl
        Implements IBaseChart, IBaseGraph

        Public Sub New(size As Size, tile As Tile,
                       procedure As String, parameters As Dictionary(Of String, String))
            InitializeComponent()
            DataContext = Me
            CreateChartData(procedure, parameters, tile)
            SetTileSize(size)

            Height = 350 * size.Height
            Width = 555 * size.Width
        End Sub

        Public Property LabelPoint As Func(Of ChartPoint, String) = Function(chartPoint)
                                                                        Dim chart = CType(chartPoint.ChartView, Wpf.Charts.Base.Chart)
                                                                        Return $"{chart.AxisX(0).Labels(CInt(chartPoint.X))} - {chartPoint.Y}"
                                                                    End Function

        Private Sub SetTileSize(s As Size) Implements IBaseChart.SetTileSize
            TileSize = SharedMethods.SetTileSize(s)
        End Sub

        Public Property TileSize As _
            TileSize Implements IBaseChart.TileSize
        Public Property Labels As _
            New List(Of String) Implements IBaseGraph.Labels
        Public Property Formatter As _
            Func(Of Double, String) Implements IBaseGraph.Formatter
        Public Property XAxis As _
            String Implements IBaseGraph.XAxis
        Public Property YAxis As _
            String Implements IBaseGraph.YAxis

        Public Property ChartTitle As _
            String Implements IBaseChart.ChartTitle
        Public Property ChartSubTitle As _
            String Implements IBaseChart.ChartSubTitle
        Public Property SeriesCollection As _
            New SeriesCollection Implements IBaseChart.SeriesCollection
        Public Property SharedMethods As _
            New SharedFunctions Implements IBaseChart.SharedMethods
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

                Dim seriesValues As New List(Of EnumerableRowCollection(Of DataRow))
                For i As Integer = 1 To chartData.Columns.Count - 1
                    seriesValues.Add(New DataView(chartData).ToTable(False, chartData.Columns(i).ColumnName).AsEnumerable)
                Next

                Dim seriesType = SharedMethods.SetSeriesTypeValues(YAxis, XAxis, chartData.Columns, seriesValues)
                Dim count = seriesType.StartIndex
                For Each seriesValue As EnumerableRowCollection In seriesType.SeriesType.Where(Function(x) x.Any())
                    Dim values As New List(Of Double)
                    For Each dataRow As DataRow In seriesValue
                        Dim val = 0.0R
                        Double.TryParse(dataRow.Item(0).ToString, val)
                        values.Add(val)
                    Next

                    SeriesCollection.Add(New StackedColumnSeries With {
                                         .Values = New ChartValues(Of Double)(values.AsEnumerable),
                                         .Title = chartData.Columns(count).ColumnName,
                                         .LabelPoint = LabelPoint,
                                         .DataLabels = False
                        })
                    count += 1
                Next

                Dim dataLabels As New List(Of String)

                Dim categoryColumn = New DataView(chartData).ToTable(False, chartData.Columns(0).ColumnName).AsEnumerable
                For Each dataRow As DataRow In categoryColumn
                    dataLabels.Add(dataRow.Item(0).ToString)
                Next

                Labels = dataLabels
                Formatter = Function(value) value.ToString("N")
                
                If Not HasChartData Then Throw New ChartNoDataException()
            Catch ex As ChartNoDataException
                ChartSubTitle = My.Resources.NoChartDataAvailable
            Catch ex As Exception
                ' clear any series data so that no data image displays
                SeriesCollection = New SeriesCollection() '.clear() throws null except?

                Dim messages = ex.Message.Split(Environment.NewLine.ToCharArray()).
                    Select(Function(message) SharedMethods.TranslateErrorMessage(message))
                Dim errorMessage = String.Join(Environment.NewLine, messages)
                ChartSubTitle = String.Format(My.Resources.Error0, errorMessage)
            End Try
        End Sub

        Public ReadOnly Property LabelRotation As Integer
            Get
                Return SharedMethods.GetLabelRotation(TileSize, Labels.Count)
            End Get
        End Property
    End Class
End Namespace
