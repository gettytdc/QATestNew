Imports System.IO
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security

''' <summary>
''' Static class for formatting clsProcessValues when building HTTP requests.
''' </summary>
Public Class ParameterValueFormatter

    ''' <summary>
    ''' Formats the specified value.
    ''' </summary>
    ''' <param name="value">The value in question. </param>
    ''' <returns>The formatted value. </returns>
    Public Shared Function FormatValue(value As clsProcessValue) As String

        Select Case value.DataType
            Case DataType.image
                Return FormatImage(value)
            Case DataType.password
                Return FormatPassword(value)
            Case Else
                Return value.EncodedValue
        End Select

    End Function

    Private Shared Function FormatPassword(value As clsProcessValue) As String
        Return CType(value, SafeString).AsString()
    End Function

    Private Shared Function FormatImage(value As clsProcessValue) As String

        If String.IsNullOrWhiteSpace(value.EncodedValue) Then Return ""

        Using tempBitmap = clsPixRect.ParseBitmap(value.EncodedValue)
            Using stream As New MemoryStream()
                tempBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp)

                Dim result = Convert.ToBase64String(stream.ToArray())
                Return result
            End Using
        End Using

    End Function
End Class
