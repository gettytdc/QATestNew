Imports System.Text
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode

Namespace WebApis.RequestHandling.BodyContent

    Public Class CustomCodeContentGenerator
        Inherits BodyContentGenerator(Of CustomCodeBodyContent)

        Private ReadOnly mBuilder As ICustomCodeBuilder
        Sub New(builder As ICustomCodeBuilder)
            mBuilder = builder
        End Sub

        Private Shared ReadOnly BodyEncoding As Encoding = Encoding.UTF8

        Public Overrides Function GetBodyContent(context As ActionContext,
                                                 content As CustomCodeBodyContent) _
            As IBodyContentResult

            Dim output = ExecuteCode(context)
            Return New StringBodyContentResult(output, GetContentTypeHeaders())
        End Function

        Private Function ExecuteCode(context As ActionContext) As String
            Dim assemblyData = mBuilder.GetAssembly(context)
            Dim requestContent = CustomCodeMethodType.RequestContent.Invoke(assemblyData, context)
            Return requestContent
        End Function

        Private Iterator Function GetContentTypeHeaders() As IEnumerable(Of HttpHeader)
            Yield New HttpHeader("Content-Type", $"text/plain; charset={BodyEncoding.WebName}")
        End Function
    End Class
End Namespace
