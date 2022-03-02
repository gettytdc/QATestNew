
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.AccessTokens
    ''' <summary>
    ''' Interface describing class used to build a signed JSON Web Token used to 
    ''' request an access token for authenticating a web api
    ''' </summary>
    Public Interface IJwtBuilder

        ''' <summary>
        ''' Builds a signed Json Web Token using the passed in JwtConfiguration and 
        ''' credential to supply the issuer and private key values. 
        ''' </summary>
        ''' <param name="config">The <see cref="JwtConfiguration"> to be used to build 
        ''' the JWT</see>/></param>
        ''' <param name="credential">The credential to be used to provide issuer and 
        ''' private key to the resulting Jwt</param>
        ''' <param name="actionParams">A collection of parameter values</param>
        ''' <returns>A Jwt as a string</returns>
        Function BuildJwt(config As JwtConfiguration, credential As ICredential,
                          actionParams As Dictionary(Of String, clsProcessValue)) As String

    End Interface
End Namespace

