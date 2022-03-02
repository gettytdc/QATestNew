Imports BluePrism.Common.Security

Namespace WebApis.RequestHandling

    Public Class BearerTokenAuthenticationHelper

        ''' <summary>
        ''' Get the Authorization header value that can be used when using
        ''' Bearer Token Authentication.
        ''' </summary>
        ''' <param name="accessToken">The access token</param>
        ''' <returns>A string in the form "Bearer [accesstoken]</returns>
        Public Shared Function GetAuthorizationHeaderValue(accessToken As SafeString) As String
            Return $"Bearer {accessToken.AsString()}"
        End Function
    End Class

End Namespace
