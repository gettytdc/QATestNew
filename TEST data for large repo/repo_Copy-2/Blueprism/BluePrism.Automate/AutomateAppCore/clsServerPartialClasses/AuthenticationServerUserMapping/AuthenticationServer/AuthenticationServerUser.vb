Imports Newtonsoft.Json

Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer

    Public Class AuthenticationServerUser
        Public Property CurrentPassword As String
        Public Property Password As String
        Public Property ConfirmPassword As String
        <JsonProperty("PublicId")>
        Public Property Id As Guid?
        Public Property Username As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property Email As String

        Public Sub New()

        End Sub

        Public Sub New(mappingRecord As UserMappingRecord)
            CurrentPassword = String.Empty
            Password = String.Empty
            ConfirmPassword = String.Empty
            Username = mappingRecord.BluePrismUsername
            FirstName = mappingRecord.FirstName
            LastName = mappingRecord.LastName
            Email = mappingRecord.Email
        End Sub

    End Class
End Namespace
