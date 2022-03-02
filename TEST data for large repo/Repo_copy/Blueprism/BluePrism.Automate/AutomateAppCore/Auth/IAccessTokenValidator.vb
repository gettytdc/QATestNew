Imports System.Security.Claims

Namespace Auth
    Public Interface IAccessTokenValidator
        Function Validate(accessToken As String, issuerUrl As String) As ClaimsPrincipal
    End Interface
End Namespace
