Imports System.Net
Imports System.Runtime.Serialization
Imports System.Text
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

<Serializable()>
Public Class WebRequestException
    Inherits BluePrismException
    Public Sub New(ex As WebException)
        MyBase.New(FormatMessage(ex))
    End Sub

    Public Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

    Private Shared Function FormatMessage(ex As WebException) As String
        Dim response = TryCast(ex.Response, HttpWebResponse)

        Dim message = New StringBuilder()
        message.AppendLine(My.Resources.Resources.WebRequestException_ErrorDuringWebAPIHTTPRequest)

        If response Is Nothing Then
            message.AppendLine(String.Format(My.Resources.Resources.WebRequestException_WebExceptionStatus0, ex.Status))
            Return message.ToString()
        End If

        message.AppendLine(String.Format(My.Resources.Resources.WebRequestException_HTTPStatusCode0, CInt(response.StatusCode)))
        Dim content = ""
        Try
            content = response.GetResponseBodyAsString()
        Catch
            content = My.Resources.Resources.WebRequestException_NotAvailable
        End Try

        message.AppendLine(String.Format(My.Resources.Resources.WebRequestException_HTTPResponseContent0, content))

        Return message.ToString()

    End Function

End Class
