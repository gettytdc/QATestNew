Imports System.Windows.Media


Public Structure LegendItem
    Private mColor As Color

    Public Sub New(col As Color)
        mColor = col
    End Sub
    Public Sub New(a As Byte, r As Byte, g As Byte, b As Byte)
        mColor = Color.FromArgb(a, r, g, b)
    End Sub

    Public Property SeriesName As String
    Public Property ValueString As String
    Public ReadOnly Property Fill As SolidColorBrush
        Get
            Return New SolidColorBrush(mColor)
        End Get
    End Property
    Public ReadOnly Property Value As Double
        Get
            Return CDbl(ValueString.TrimEnd("%"c))
        End Get
    End Property

End Structure
