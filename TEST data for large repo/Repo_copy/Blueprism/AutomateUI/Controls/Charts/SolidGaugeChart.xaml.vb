Imports System.Windows.Controls
Imports BluePrism.AutomateAppCore

Namespace Controls.Charts
    Partial Public Class SolidGaugeChart
        Inherits UserControl
        Implements IBaseChart

        Public Sub New(size As Size, tile As Tile, procedure As String,
                           parameters As Dictionary(Of String, String))
            InitializeComponent()
            DataContext = Me
            CreateChartData(procedure, parameters, tile)
            SetTileSize(size)

            Height = 350 * size.Height
            Width = 555 * size.Width
        End Sub
        Private Sub SetTileSize(s As Size) Implements IBaseChart.SetTileSize
            TileSize = SharedMethods.SetTileSize(s)
        End Sub

        Public Property TileSize As _
            TileSize Implements IBaseChart.TileSize
        Public Property ChartTitle As _
            String Implements IBaseChart.ChartTitle
        Public Property ChartSubTitle As _
            String Implements IBaseChart.ChartSubTitle
        Public Property SeriesCollection As _
            New LiveCharts.SeriesCollection Implements IBaseChart.SeriesCollection
        Public Property SharedMethods As _
            New SharedFunctions Implements IBaseChart.SharedMethods

        Public ReadOnly Property HasChartData As Boolean Implements IBaseChart.HasChartData
            Get
                Return GaugeCollection.Any()
            End Get
        End Property


        Public Property GaugeCollection As List(Of Gauge)

        Public Class Gauge
            Public Property Value As Double
            Public Property GaugeText As String
            Public Property FormatFunc As Func(Of Double, String) = Function(x) String.Format("{0}%", x)
        End Class

        Private Sub CreateChartData(procedure As String, parameters As Dictionary(Of String, String),
                                    tile As Tile) Implements IBaseChart.CreateChartData

            If tile Is Nothing Then
                Throw New ArgumentNullException(NameOf(tile))
            End If

            Dim chartData As DataTable
            GaugeCollection = New List(Of Gauge)
            ChartTitle = tile.Name
            ChartSubTitle = tile.Description

            Try
                chartData = gSv.GetChartData(procedure, parameters)

                If chartData Is Nothing Then Throw New ChartNoDataException()

                For Each dataRow As DataRow In chartData.AsEnumerable
                    Dim gaugeValue As Double = 0
                    Double.TryParse(dataRow.Item(1).ToString, gaugeValue)
                    GaugeCollection.Add(New Gauge With {
                    .GaugeText = dataRow.Item(0).ToString,
                    .Value = gaugeValue})
                Next

                If Not HasChartData Then Throw New ChartNoDataException()
            Catch ex As ChartNoDataException
                GaugeCollection.Clear()
                ChartSubTitle = My.Resources.NoChartDataAvailable
            Catch ex As Exception
                ' clear any series data so that no data image displays
                GaugeCollection.Clear()

                Dim messages = ex.Message.Split(Environment.NewLine.ToCharArray()).
                    Select(Function(message) SharedMethods.TranslateErrorMessage(message))
                Dim errorMessage = String.Join(Environment.NewLine, messages)
                ChartSubTitle = String.Format(My.Resources.Error0, errorMessage)
            End Try
        End Sub
    End Class
End Namespace
