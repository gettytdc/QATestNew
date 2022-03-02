Imports System.Net
Imports System.Text
Imports System.Linq

Namespace WebApis.RequestHandling.BodyContent

    Public Class RawDataBodyContentResult
        Implements IBodyContentResult

        Private ReadOnly mData As Byte()

        Public Sub New(data() As Byte, headers As IEnumerable(Of HttpHeader))
            mData = data
            Me.Headers = headers.ToList().AsReadOnly()
        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property Content As String Implements IBodyContentResult.Content
            Get
                ' Need to replace the null char, otherwise the string will be truncated
                ' when displayed in a Win Forms text box
                Return Encoding.UTF8.GetString(mData).Replace(vbNullChar, "1")
            End Get
        End Property

        ''' <inheritdoc/>
        Public ReadOnly Property Headers As IReadOnlyCollection(Of HttpHeader) Implements IBodyContentResult.Headers

        ''' <inheritdoc/>
        Public Sub Write(request As HttpWebRequest) Implements IBodyContentResult.Write
            request.ContentLength = mData.Length
            Dim stream = request.GetRequestStream()
            stream.Write(mData, 0, mData.Length)
        End Sub

    End Class
End Namespace