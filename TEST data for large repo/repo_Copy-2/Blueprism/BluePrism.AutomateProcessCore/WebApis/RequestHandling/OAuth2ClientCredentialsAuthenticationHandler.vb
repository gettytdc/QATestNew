Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.RequestHandling
    ''' <summary>
    ''' Class that handles OAuth 2.0 authentication using Client Credentials
    ''' (client id and client secret) as an Authorization Grant during HTTP requests 
    ''' made when executing a Web API action
    ''' </summary>
    Public Class OAuth2ClientCredentialsAuthenticationHandler
        Inherits OAuth2AuthenticationHandler(Of IOAuth2ClientCredentialsAccessTokenRequester, OAuth2ClientCredentialsAuthentication)

        Sub New(accessTokenPool As IAccessTokenPool,
            credentialHelper As IAuthenticationCredentialHelper,
            requester As IOAuth2ClientCredentialsAccessTokenRequester)
            MyBase.New(accessTokenPool, credentialHelper, requester)
        End Sub

    End Class

End Namespace
