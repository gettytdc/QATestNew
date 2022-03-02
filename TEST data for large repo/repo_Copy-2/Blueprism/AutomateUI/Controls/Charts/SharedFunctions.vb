Imports BluePrism.AutomateAppCore
Imports LiveCharts
Imports LiveCharts.Wpf
Imports LocaleTools

Namespace Controls.Charts
    Public Class SharedFunctions
        Public Function HasDataToDisplay(seriesCollection As SeriesCollection) As Boolean
            For Each actualValues In seriesCollection.Select(Function(t) t.ActualValues)
                For Each values In actualValues
                    Dim convertible As IConvertible = TryCast(values, IConvertible)
                    If convertible IsNot Nothing Then
                        If convertible.ToDouble(Nothing) > 0 Then
                            Return True
                        End If
                    End If
                Next
            Next
            Return False
        End Function

        Public Function TranslateErrorMessage(errorMessage As String) As Object
            Return LTools.Get(errorMessage, "tile", Options.Instance.CurrentLocale, "error")
        End Function

        Public Function SetTileSize(s As Size) As TileSize
            Return CType(s.Width, TileSize)
        End Function


        Public Function GenerateLegendList(ByRef chartData As DataTable,
                                           series As SeriesCollection,
                                           chartType As ChartTypes,
                                           colorCollection As ColorsCollection,
                                           maxItems As Integer) As List(Of LegendItem)

            Dim legendItems As New List(Of LegendItem)
            Dim generator As New LegendGenerator(series, colorCollection)

            Select Case chartType
                Case ChartTypes.Pie, ChartTypes.Doughnut
                    legendItems = generator.CreateParticipationItemValuePair(chartData, maxItems)


            End Select

            Return legendItems
        End Function

        Public Function GetLabelRotation(size As TileSize, labelCount As Integer) As Integer
            Const rotation = -20

            If size = TileSize.Large AndAlso
                    labelCount > 13 Then
                Return rotation
            ElseIf size = TileSize.Medium AndAlso
                    labelCount > 8 Then
                Return rotation
            ElseIf size = TileSize.Small AndAlso
                    labelCount > 5 Then
                Return rotation
            End If

            Return 0
        End Function

        Public Function SetSeriesTypeValues(ByRef setValueAxis As String, ByRef setLabelAxis As String,
                                            columns As DataColumnCollection,
                                            seriesValues As IEnumerable(Of EnumerableRowCollection(Of DataRow))) As ChartSeriesType

            If columns Is Nothing Then Throw New ArgumentNullException(My.Resources.SharedMethodArgumentNullException_2OrMoreColumnsHaveToBePresent)
            If seriesValues Is Nothing Then Throw New ArgumentNullException(My.Resources.SharedMethodArgumentNullException_AtLeastOneSeriesHasToBePreset)
            If columns.Count < 2 Then Throw New IndexOutOfRangeException(My.Resources.SharedMethodArgumentNullException_2OrMoreColumnsHaveToBePresent)
            If seriesValues.Count < 1 Then Throw New ArgumentNullException(My.Resources.SharedMethodArgumentNullException_AtLeastOneSeriesHasToBePreset)

            Dim hasValueLabelColumn = columns(1).ColumnName.Equals("ValueLabel")
            Dim shouldHaveDynmicLabel = seriesValues(0).Any() AndAlso hasValueLabelColumn
            Dim firstSeries As Integer, firstColumn As Integer, firstRow As Integer
            Dim valueAxis = If(shouldHaveDynmicLabel, seriesValues(firstSeries)(firstColumn)(firstRow).ToString(), LTools.Get("Value", "tile", Options.Instance.CurrentLocale))

            Dim isMultiSeries = columns.Count() > 2
            setLabelAxis = columns(0).ColumnName
            setValueAxis = If(isMultiSeries, valueAxis, columns(1).ColumnName)

            Dim shouldSkipValueLabel = isMultiSeries AndAlso hasValueLabelColumn
            Dim seriesType = If(shouldSkipValueLabel, seriesValues.Skip(1), seriesValues)
            Dim index = If(shouldSkipValueLabel, 2, 1)
            Return New ChartSeriesType With {.seriesType = seriesType, .StartIndex = index}
        End Function
    End Class
End Namespace