Imports BluePrism.AutomateAppCore
Imports AutomateUI.Controls.Charts

''' <summary>
''' Represents a chart tile
''' </summary>
Public Class ChartTile

    'Reference to the underlying generic tile
    Public Property Tile() As Tile

    'Chart type (Bar, Pie etc.)
    Public Property ChartType() As ChartTypes

    'Are axes switched
    Public Property AxesSwitched() As Boolean

    'Stored procedure (supplying the data)
    Public Property Procedure() As String

    'Stored procedure parameters (optional)
    Public Property Parameters() As Dictionary(Of String, String)

    ''' <summary>
    ''' Creates a new chart object based on the passed generic tile.
    ''' </summary>
    ''' <param name="baseTile">A tile object</param>
    Public Sub New(baseTile As Tile)
        Tile = baseTile
        AxesSwitched = False
        Procedure = String.Empty
        Parameters = New Dictionary(Of String, String)()

        'If tile is blank, initialise XML properties for a chart
        If Tile.XMLProperties Is Nothing Then
            SaveProperties()
            Return
        End If

        'Otherwise unpack XML properties
        Dim doc As XDocument = XDocument.Parse(Tile.XMLProperties)

        Dim chartElement As XElement = doc.Element("Chart")
        ChartType = CType(chartElement.Attribute("type").Value, ChartTypes)
        If chartElement.Attributes("plotByRow").Count > 0 Then _
            AxesSwitched = CBool(chartElement.Attribute("plotByRow").Value)

        Dim procElement As XElement = chartElement.Element("Procedure")
        Procedure = procElement.Attribute("name").Value

        For Each paramElement As XElement In procElement.Elements("Param")
            Parameters.Add(paramElement.Attribute("name").Value, paramElement.Value)
        Next
    End Sub

    ''' <summary>
    ''' Pack chart specific properties into tile's XML properties
    ''' </summary>
    Public Sub SaveProperties()
        Dim procElement As New XElement("Procedure", New XAttribute("name", Procedure))
        For Each param As KeyValuePair(Of String, String) In Parameters
            procElement.Add(New XElement("Param", New XAttribute("name", param.Key), param.Value))
        Next
        Dim chart As New XElement("Chart", New XAttribute("type", CInt(ChartType)), New XAttribute("plotByRow", AxesSwitched))
        chart.Add(procElement)

        Tile.XMLProperties = chart.ToString(SaveOptions.DisableFormatting)
    End Sub

    '' <summary>
    '' Render the chart according to the tile size.'' </summary>
    '' <param name="size">Tile size</param>
    '' <returns>A chart UIElement</returns>
    Public Function Build(size As Size) As System.Windows.UIElement

        'Return Chart Type
        Select Case ChartType
            Case ChartTypes.Column
                Return New BasicColumnChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.StackedColumn
                Return New BasicStackedColumnChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.StackedColumn100
                Return New BasicColumnChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.Pie
                Return New PieChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.Doughnut
                Return New DoughnutChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.Gauge
                Return New SolidGaugeChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.Bar
                Return New BasicRowChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.StackedBar
                Return New BasicStackedRowChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.StackedBar100
                Return New BasicRowChart(size, Tile, Procedure, Parameters)
            Case ChartTypes.Line
                Return New BasicLineChart(size, Tile, Procedure, Parameters)
            Case Else
                Return Nothing
        End Select
    End Function
End Class
