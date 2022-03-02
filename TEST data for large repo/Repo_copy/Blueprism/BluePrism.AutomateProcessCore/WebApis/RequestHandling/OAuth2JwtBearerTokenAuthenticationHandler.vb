Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Class that handles OAuth 2.0 authentication using a JWT (JSON Web Token) as 
    ''' an Authorization Grant during HTTP requests made when executing a Web API 
    ''' action
    ''' </summary>
    Public Class OAuth2JwtBearerTokenAuthenticationHandler
        Inherits OAuth2AuthenticationHandler(Of IOAuth2JwtBearerTokenAccessTokenRequester, OAuth2JwtBearerTokenAuthentication)

        Sub New(accessTokenPool As IAccessTokenPool,
                credentialHelper As IAuthenticationCredentialHelper,
                requester As IOAuth2JwtBearerTokenAccessTokenRequester)
            MyBase.New(accessTokenPool, credentialHelper, requester)
        End Sub

    End Class

End Namespace
