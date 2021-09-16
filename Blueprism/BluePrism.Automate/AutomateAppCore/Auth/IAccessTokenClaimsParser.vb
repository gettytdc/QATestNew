Imports System.Security.Claims

Namespace Auth
    Public Interface IAccessTokenClaimsParser
        Function GetUserId(claims As ClaimsPrincipal) As Guid
        Function GetId(claims As ClaimsPrincipal) As Guid?
        Function GetClientId(claims As ClaimsPrincipal) As String
        Function GetAuthenticationTime(claims As ClaimsPrincipal) As DateTimeOffset?
        Function GetIssuer(claims As ClaimsPrincipal) As String
    End Interface
End Namespace
