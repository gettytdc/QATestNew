Imports System.Security
Imports System.Threading.Tasks
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens

Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
    Public Interface IAccessTokenRequester
        Function RequestAccessToken(identityServerUrl As String, clientId As String, clientSecret As SecureString) As Task(Of AccessToken)
    End Interface
End Namespace
