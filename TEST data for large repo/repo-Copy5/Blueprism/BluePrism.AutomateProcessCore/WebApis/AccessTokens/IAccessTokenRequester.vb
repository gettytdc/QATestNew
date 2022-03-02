Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling

Namespace WebApis.AccessTokens
    Public Interface IAccessTokenRequester

        ''' <summary>
        ''' Executes a request for an AccessToken for a given context using the 
        ''' configured authentication and the credential provided.
        ''' </summary>
        ''' <param name="context">The Action Context of the action requiring the access token</param>
        ''' <param name="credential">The credential to be used to obtain the access token</param>
        ''' <returns>An <see cref="AccessToken"/></returns>
        ''' <exception cref="AccessTokenException"></exception>
        Function RequestAccessToken(context As ActionContext, credential As ICredential) As AccessToken

    End Interface
End Namespace
