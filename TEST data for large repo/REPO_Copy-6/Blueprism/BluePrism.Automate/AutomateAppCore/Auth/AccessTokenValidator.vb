Imports System.Security.Claims
Imports Microsoft.IdentityModel.Tokens

Namespace Auth
    Public Class AccessTokenValidator
        Implements IAccessTokenValidator

        Private ReadOnly mTokenHandler As ISecurityTokenValidator
        Private ReadOnly mDiscoveryDocumentRetriever As IDiscoveryDocumentRetriever

        Public Sub New(tokenHandler As ISecurityTokenValidator, discoveryDocumentRetriever As IDiscoveryDocumentRetriever)
            mTokenHandler = tokenHandler
            mDiscoveryDocumentRetriever = discoveryDocumentRetriever
        End Sub

        Public Function Validate(accessToken As String, issuerUrl As String) As ClaimsPrincipal Implements IAccessTokenValidator.Validate
            Try
                Dim discoveryDocumentUrl = $"{issuerUrl}/.well-known/openid-configuration"
                Dim discoveryDocument = mDiscoveryDocumentRetriever.GetDiscoveryDocument(discoveryDocumentUrl)

                Dim validationParameters = New TokenValidationParameters With {
                    .ValidateIssuer = True,
                    .ValidAudiences = {"bpserver", "bp-api"},
                    .ValidIssuer = issuerUrl?.ToLower(),
                    .IssuerSigningKeys = discoveryDocument.SigningKeys,
                    .NameClaimType = "sub"
                    }

                Dim validatedToken As SecurityToken = Nothing
                Return mTokenHandler.ValidateToken(accessToken, validationParameters, validatedToken)

            Catch ex As Exception
                Throw New SecurityTokenValidationException(ex.Message, ex.InnerException)
            End Try
        End Function

    End Class
End Namespace
