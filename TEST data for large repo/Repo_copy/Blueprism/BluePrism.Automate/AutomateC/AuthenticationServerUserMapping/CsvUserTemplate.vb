Public Class CsvUserTemplate

    Public ReadOnly Path As String
    Public ReadOnly HeadingFields As String()
    Public ReadOnly UserMapping As List(Of UserMappingCsvRecord)

    Public Sub New(path As String, heading As String, userMapping As List(Of UserMappingCsvRecord))
        Me.Path = path
        Me.HeadingFields = heading.Split(CChar(","))
        Me.UserMapping = userMapping
    End Sub
End Class
