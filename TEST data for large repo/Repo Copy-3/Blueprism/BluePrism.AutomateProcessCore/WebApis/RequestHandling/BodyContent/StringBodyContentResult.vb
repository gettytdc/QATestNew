Imports System.Net
Imports System.Text
Imports System.Linq

Namespace WebApis.RequestHandling.BodyContent

    Public Class StringBodyContentResult
        Implements IBodyContentResult

        Private ReadOnly mBodyEncoding As Encoding = Encoding.UTF8

        Public Sub New(content As String, headers As IEnumerable(Of HttpHeader))
            Me.Content = content
            Me.Headers = headers.ToList().AsReadOnly()
        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property Content As String Implements IBodyContentResult.Content

        ''' <inheritdoc/>
        Public ReadOnly Property Headers As IReadOnlyCollection(Of HttpHeader) Implements IBodyContentResult.Headers

        ''' <inheritdoc/>
        Public Sub Write(request As HttpWebRequest) Implements IBodyContentResult.Write
            If Not String.IsNullOrWhiteSpace(Content) Then
                Dim byteArray = mBodyEncoding.GetBytes(Content)
                request.ContentLength = byteArray.Length
                Dim stream = request.GetRequestStream()
                stream.Write(byteArray, 0, byteArray.Length)
            End If
        End Sub

    End Class

End NameSpace