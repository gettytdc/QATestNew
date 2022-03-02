Imports LiveCharts
Imports System.Windows.Media
Imports LiveCharts.Wpf

Public Class LegendGenerator

    Public Sub New(series As SeriesCollection, col As ColorsCollection)
        mColors = col
        SeriesCollection = series
    End Sub

    Private ReadOnly mColors As ColorsCollection

    Public Property SeriesCollection As SeriesCollection

    ''' <summary>
    ''' Populates LegendItems as Item and Value pairs where value is the percentile of the 
    ''' whole series
    ''' </summary>
    Public Function CreateParticipationItemValuePair(ByRef chartData As DataTable, maxItems As Integer) As List(Of LegendItem)
        Dim total As Double = chartData.AsEnumerable.Sum(Function(x)
                                                             Dim val As Double = 0.0
                                                             Double.TryParse(x.Item(1).ToString(), val)
                                                             Return val
                                                         End Function)
        Dim legendItems As New List(Of LegendItem)(maxItems)

        Dim count As Integer = 0
        For Each series As Series In SeriesCollection.Take(maxItems)
            ' get the right color (LVC repeats the series so use Mod)
            Dim fill As Color = mColors(count Mod mColors.Count)
            Dim value As Double = 0
            Double.TryParse(chartData.AsEnumerable(count).Item(1).ToString(), value)

            Dim li As New LegendItem(fill) With {
                .SeriesName = series.Title,
                .ValueString = $"{(value / total):p2}"
            }
            legendItems.Add(li)

            count += 1
        Next

        ' Do we need to overwrite the last legend item with 
        'the sum of the rest as 'Other' ?
        If chartData.AsEnumerable.Count > maxItems Then
            'use the last
            Dim fill = mColors(mColors.Count - 1)
            Dim sumRest As Double = chartData.AsEnumerable.Skip(count - 1).Sum(Function(x)
                                                                                   Dim val As Double = 0.0
                                                                                   Double.TryParse(x.Item(1).ToString(), val)
                                                                                   Return val
                                                                               End Function)
            
            Dim li As New LegendItem(fill) With {
                .SeriesName = My.Resources.Other,
                .ValueString = $"{(sumRest / total):p2}"
                }
            legendItems(legendItems.Count - 1) = li
        End If

        ' sort by descending and keep Other at the bottom
        Return legendItems.OrderByDescending(Function(x) x.Value).OrderByDescending(Function(x) x.SeriesName.Contains(My.Resources.Other) = False).ToList()
    End Function
End Class
