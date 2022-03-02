Imports System.Text

Namespace WebApis.RequestHandling.BodyContent.Multipart

    ''' <summary>
    ''' Class that represents a section from within the body of a multi-part form 
    ''' HTTP Request when sending multiple files
    ''' </summary>
    Public Class MultiPartFileBodySection

        Private Const DefaultContentType = "application/octet-stream"
        Public ReadOnly Property FieldName As String
        Public ReadOnly Property FileName As String
        Public ReadOnly Property ContentType As String
        Public ReadOnly Property Content As Byte()
        Public ReadOnly Property Header() As String

        Sub New(fieldName As String,
                fileName As String,
                content As Byte(),
                contentType As String)

            Me.FieldName = fieldName
            Me.FileName = fileName
            Me.Content = content

            Me.ContentType = If(String.IsNullOrEmpty(contentType), DefaultContentType, contentType)

            Header = GenerateHeader()
        End Sub

        Private Function GenerateHeader() As String
            Dim result = New StringBuilder()

            result.AppendLine($"Content-Disposition: {GenerateContentDispositionHeader()}")
            result.AppendLine($"Content-Type: {ContentType}")
            result.AppendLine()

            Return result.ToString()
        End Function

        Private Function GenerateContentDispositionHeader() As String

            Dim parameters As New List(Of String) From {
                "form-data"
            }
            If Not String.IsNullOrEmpty(FieldName) Then parameters.Add($"name=""{FieldName}""")
            If Not String.IsNullOrEmpty(FileName) Then parameters.Add($"filename=""{FileName}""")

            Return String.Join("; ", parameters)

        End Function

    End Class

End Namespace
