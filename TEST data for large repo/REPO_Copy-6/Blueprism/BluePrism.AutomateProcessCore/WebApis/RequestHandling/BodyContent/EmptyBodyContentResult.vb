Imports System.Net

Namespace WebApis.RequestHandling.BodyContent

    Public Class EmptyBodyContentResult
        Implements IBodyContentResult

        ''' <inheritdoc/>
        Public ReadOnly Property Content As String Implements IBodyContentResult.Content
            Get
                Return Nothing
            End Get
        End Property

        ''' <inheritdoc/>
        Public ReadOnly Property Headers As IReadOnlyCollection(Of HttpHeader) Implements IBodyContentResult.Headers
            Get
                Return New List(Of HttpHeader)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Sub Write(request As HttpWebRequest) Implements IBodyContentResult.Write
        End Sub

    End Class
End Namespace