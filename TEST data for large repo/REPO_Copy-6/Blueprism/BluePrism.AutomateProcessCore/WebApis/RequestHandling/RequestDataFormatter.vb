Imports System.Text

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Formats HTTP request details to string form used in the Request Data output
    ''' parameter value
    ''' </summary>
    Friend Module RequestDataFormatter

        ''' <summary>
        ''' Formats the parameter value
        ''' </summary>
        ''' <param name="requestData">Contains details of the request</param>
        ''' <returns>The formatted value</returns>
        Public Function Format(requestData As HttpRequestData) As String
            Dim request = requestData.Request
            Dim content = requestData.Content
            Dim data As New StringBuilder()
            data.AppendLine($"{request.Method.ToUpperInvariant()} {request.RequestUri}")
            For Each headerKey As String In request.Headers.Keys
                data.AppendLine($"{headerKey}: {request.Headers(headerKey)}")
            Next
            If content IsNot Nothing Then
                data.Append(content)
            End If
            Return data.ToString()
        End Function

    End Module
End Namespace