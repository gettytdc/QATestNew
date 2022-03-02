Imports System.Linq

Namespace WebApis.RequestHandling.BodyContent

    Public Class NoBodyContentGenerator
        Inherits BodyContentGenerator(Of NoBodyContent)

        ''' <inheritdoc />
        Public Overrides Function GetBodyContent(context As ActionContext,
                                                 content As NoBodyContent) _
            As IBodyContentResult

            Return New EmptyBodyContentResult()
        End Function

        Private Function GetContentTypeHeaders() As IEnumerable(Of HttpHeader)
            Return Enumerable.Empty(Of HttpHeader)
        End Function

    End Class

End Namespace
