Imports System.Text
Imports BluePrism.AutomateProcessCore.WebApis.TemplateProcessing

Namespace WebApis.RequestHandling.BodyContent

    Public Class TemplateContentGenerator
        Inherits BodyContentGenerator(Of TemplateBodyContent)

        Private Shared ReadOnly BodyEncoding As Encoding = Encoding.UTF8

        ''' <inheritdoc />
        Public Overrides Function GetBodyContent(context As ActionContext,
                                                 content As TemplateBodyContent) _
            As IBodyContentResult

            If Not String.IsNullOrEmpty(content.Template) Then
                Dim processedTemplate = ProcessTemplate(content.Template, context.Parameters)
                Return New StringBodyContentResult(processedTemplate, GetContentTypeHeaders())
            Else
                Return New EmptyBodyContentResult()
            End If

        End Function

        Private Iterator Function GetContentTypeHeaders() As IEnumerable(Of HttpHeader)
            Yield New HttpHeader("Content-Type", $"text/plain; charset={BodyEncoding.WebName}")
        End Function


    End Class

End Namespace
