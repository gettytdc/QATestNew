Imports Newtonsoft.Json

Public Class CitrixAppManMessage
    Public Enum CitrixMode
        Citrix32
        Citrix64
    End Enum

    <JsonProperty("query")>
    Public Property Query As String

    <JsonProperty("id")>
    Public Property Id As Guid

    <JsonProperty("mode")>
    Public Property Mode As CitrixMode

    Public Sub New(query As String, id As Guid, mode As CitrixMode)
        Me.Query = query
        Me.Id = id
        Me.Mode = mode
    End Sub

End Class
