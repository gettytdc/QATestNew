Imports BluePrism.BPCoreLib.Collections
Imports DataType = BluePrism.AutomateProcessCore.DataType

    Public Class DataItemColour

    ' The map of colors to their respective data items.
    Private Shared mDataItemColours As IDictionary(Of DataType, Color)

    ''' <summary>
    ''' Gets the colour which represents the given data item.
    ''' </summary>
    ''' <param name="dataType">The datatype for which a colour is required.
    ''' </param>
    ''' <returns>The colour which represents the given data type, or 
    ''' <see cref="SystemColors.WindowText"/> if no colour is explicitly set for
    ''' the given datatype.</returns>
    Public Shared Function GetDataItemColor(ByVal dataType As DataType) As Drawing.Color
        Dim c As Color
        If (DataItemColours.TryGetValue(dataType, c)) Then Return c
        Return SystemColors.WindowText
    End Function

    ''' <summary>
    ''' The map of data types to the colours which represent them.
    ''' </summary>
    Public Shared ReadOnly Property DataItemColours() As IDictionary(Of DataType, Color)
        Get
            If mDataItemColours Is Nothing Then
                Dim map As New Dictionary(Of DataType, Color)

                map(DataType.text) = Color.Green
                map(DataType.password) = Color.Green
                map(DataType.number) = Color.Blue
                map(DataType.collection) = Color.FromArgb(255, 179, 57, 175)
                map(DataType.flag) = Color.FromArgb(255, 57, 145, 159)
                map(DataType.timespan) = Color.DarkRed

                Dim colDate As Color = Color.FromArgb(255, 224, 128, 31)
                map(DataType.date) = colDate
                map(DataType.datetime) = colDate
                map(DataType.time) = colDate

                Dim colDefault As Color = SystemColors.WindowText
                map(DataType.binary) = colDefault
                map(DataType.image) = colDefault
                map(DataType.unknown) = colDefault

                mDataItemColours = GetReadOnly.IDictionary(Of DataType, Color)(map)
            End If
            Return mDataItemColours
        End Get
    End Property


    End Class