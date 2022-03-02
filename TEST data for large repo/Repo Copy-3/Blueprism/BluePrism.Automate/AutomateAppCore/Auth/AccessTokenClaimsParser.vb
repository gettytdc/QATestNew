Imports System.Security.Claims
Imports IdentityModel
Imports Microsoft.IdentityModel.Tokens

Namespace Auth

    Public Class AccessTokenClaimsParser
        Implements IAccessTokenClaimsParser

        Public Function GetUserId(claims As ClaimsPrincipal) As Guid Implements IAccessTokenClaimsParser.GetUserId
            Dim userId As Guid

            Dim claimValue = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value
            If String.IsNullOrEmpty(claimValue) Then
                Throw New SecurityTokenValidationException("Subject claim not found")
            End If

            If Not Guid.TryParse(claimValue, userId) Then
                Throw New SecurityTokenValidationException("Unable to parse user id")
            End If

            Return userId
        End Function

        Public Function GetId(claims As ClaimsPrincipal) As Guid? Implements IAccessTokenClaimsParser.GetId

            Dim claimValue = claims.FindFirst(JwtClaimTypes.Id)

            If claimValue Is Nothing Then
                Return Nothing
            End If

            Dim id As Guid
            If Not Guid.TryParse(claimValue.Value, id) Then
                Throw New SecurityTokenValidationException("Unable to parse id")
            End If

            Return id
        End Function

        Public Function GetClientId(claims As ClaimsPrincipal) As String Implements IAccessTokenClaimsParser.GetClientId
            Dim claimValue = claims.FindFirst("client_id")?.Value

            If String.IsNullOrEmpty(claimValue) Then
                Throw New SecurityTokenValidationException("Client_Id claim not found")
            End If

            Return claimValue
        End Function

        Public Function GetAuthenticationTime(claims As ClaimsPrincipal) As DateTimeOffset? Implements IAccessTokenClaimsParser.GetAuthenticationTime
            Dim claimValue = claims.FindFirst(JwtClaimTypes.AuthenticationTime)

            If claimValue Is Nothing Then
                Return Nothing
            End If

            Dim epochSeconds As Long
            If Not Long.TryParse(claimValue.Value, epochSeconds) Then
                Throw New SecurityTokenValidationException("Unable to parse auth_time")
            End If

            Dim authenticationTime = DateTimeOffset.FromUnixTimeSeconds(epochSeconds)
            Return authenticationTime
        End Function

        Public Function GetIssuer(claims As ClaimsPrincipal) As String Implements IAccessTokenClaimsParser.GetIssuer
            Dim claimValue = claims.FindFirst(JwtClaimTypes.Issuer)?.Value
            If String.IsNullOrEmpty(claimValue) Then
                Throw New SecurityTokenValidationException("iss claim not found")
            End If
            Return claimValue
        End Function

    End Class
End Namespace
