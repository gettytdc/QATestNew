
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling

Namespace WebApis.AccessTokens
    ''' <summary>
    ''' Interface describing a pool that stores access tokens that can be used
    ''' for authenticating Web API requests. Access tokens are uniquely identified 
    ''' by the Web API they will be authenticating requests to and the credential
    ''' that identifies who is making the request.
    ''' </summary>
    Public Interface IAccessTokenPool

        ''' <summary>
        ''' Get a valid access token from the pool that can be used to authenticate
        ''' a Web API request. If it can't find one then a new access token will
        ''' be requested.
        ''' </summary>
        ''' <param name="context">The specific data relating to the Web API request 
        ''' the access token will be used to authenticate</param>
        ''' <param name="credential">The credential containing the information that 
        ''' identifies who is making the request</param>
        Function GetAccessToken(context As ActionContext, credential As ICredential, requester As IAccessTokenRequester) As AccessToken

        ''' <summary>
        ''' Sets the specified access token to be invalid, if it can be found in the 
        ''' pool.
        ''' </summary>
        ''' <param name="webApiId">The id of the Web API that the access token is 
        ''' used for</param>
        ''' <param name="credentialName">The name of credential containing the 
        ''' information that identifies who is making the request</param>
        ''' <param name="accessToken">The access token that we want to invalidate</param>
        Sub InvalidateToken(webApiId As Guid, credentialName As String, accessToken As AccessToken)

        ''' <summary>
        ''' Returns True, if the access token can be used to authenticate a Web API
        ''' request.
        ''' </summary>
        ''' <param name="token">The token to check</param>
        ''' <returns>Returns True, if the access token can be used to authenticate a 
        ''' Web API request.</returns>
        Function CanAccessTokenBeUsed(token As AccessToken) As Boolean

    End Interface
End Namespace
