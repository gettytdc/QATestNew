Namespace WebApis.RequestHandling.BodyContent

    Public Class SingleFileContentGenerator
        Inherits BodyContentGenerator(Of SingleFileBodyContent)

        Private Iterator Function GetContentTypeHeaders() As IEnumerable(Of HttpHeader)
            Yield New HttpHeader("Content-Type", "application/octet-stream")
        End Function

        ''' <inheritdoc />
        Public Overrides Function GetBodyContent(context As ActionContext, 
                                                 content As SingleFileBodyContent) _
            As IBodyContentResult

            Dim fileInputParameterName = content.FileInputParameterName
            Dim file As clsProcessValue = Nothing

            If Not context.Parameters.TryGetValue(fileInputParameterName, file) Then _
                Throw New ArgumentException($"Could not find the '{content.FileInputParameterName}' input parameter", NameOf(context))

            If file.IsNull Or file.DataType <> DataType.binary Then _
                Throw New ArgumentException("No valid 'File' parameter found in the current context, unable to process body content")

            Return New RawDataBodyContentResult(CType(file, Byte()), GetContentTypeHeaders())
        End Function

    End Class

End Namespace
