
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.Common.Security
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens

Namespace WebApis.RequestHandling
    ''' <summary>
    ''' Abstract class that applies OAuth 2.0 authentication (using bearer tokens)
    ''' during HTTP requests made when executing a Web API action
    ''' </summary>
    Public MustInherit Class OAuth2AuthenticationHandler(Of TRequester As IAccessTokenRequester,
                                                             TAuthentication As {IAuthentication, ICredentialAuthentication, Class})
        Inherits CredentialAuthenticationHandler(Of TAuthentication)

        Private ReadOnly mAccessTokenRequester As IAccessTokenRequester
        Private ReadOnly mAccessTokenPool As IAccessTokenPool
        Private mAccessToken As AccessToken
        Private mCredential As ICredential

        Protected Sub New(accessTokenPool As IAccessTokenPool,
                credentialHelper As IAuthenticationCredentialHelper,
                accessTokenRequester As TRequester)
            MyBase.New(credentialHelper)
            mAccessTokenPool = accessTokenPool
            mAccessTokenRequester = accessTokenRequester
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Handle(request As HttpWebRequest, context As ActionContext)

            mCredential = GetCredential(context)

            mAccessToken = mAccessTokenPool.GetAccessToken(context, mCredential, mAccessTokenRequester)

            request.Headers.Add("Authorization",
                                BearerTokenAuthenticationHelper.
                                    GetAuthorizationHeaderValue(mAccessToken.AccessToken.AsSecureString()))

        End Sub

        ''' <inheritdoc/>
        ''' <remarks>
        ''' Allow one retry attempt if the HTTP Status Code is 401 unauthorized. This 
        ''' is primarily used to handle access tokens that may have expired or been 
        ''' revoked without us being made aware (such as when the access token 
        ''' returned from the authorization server doesn't specify an expiry date) 
        ''' </remarks>
        Public Overrides ReadOnly Property RetryAttemptsOnUnauthorizedException As Integer
            Get
                Return 1
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub BeforeRetry(context As ActionContext)
            ' Mark the access token used in the current context as invalid in the 
            ' pool, so the next time a web api request is made it will first try and 
            ' get a new access token from the authorization server
            mAccessTokenPool.InvalidateToken(context.WebApiId, mCredential.Name, mAccessToken)
        End Sub

    End Class

End Namespace