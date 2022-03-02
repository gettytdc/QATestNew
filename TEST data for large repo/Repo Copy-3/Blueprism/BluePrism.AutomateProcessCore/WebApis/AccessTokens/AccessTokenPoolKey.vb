Namespace WebApis.AccessTokens

    ''' <summary>
    ''' Class to hold data that is used as a key for the Access Token Pool
    ''' </summary>
    Public Class AccessTokenPoolKey

        Public ReadOnly Property WebApiId As Guid

        Public ReadOnly Property CredentialName As String

        Public Sub New(webApiId As Guid, credentialName As String)
            Me.WebApiId = webApiId
            Me.CredentialName = credentialName
        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim key = TryCast(obj, AccessTokenPoolKey)
            If key Is Nothing Then Return False
            Return key.WebApiId = WebApiId AndAlso key.CredentialName = CredentialName
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return WebApiId.GetHashCode() Xor CredentialName.GetHashCode()
        End Function

    End Class

End Namespace
