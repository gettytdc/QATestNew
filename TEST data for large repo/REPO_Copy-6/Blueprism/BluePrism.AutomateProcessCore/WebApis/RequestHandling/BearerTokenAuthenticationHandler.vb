
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Applies bearer token authentication during HTTP requests made when executing a 
    ''' Web API action
    ''' </summary>
    Public Class BearerTokenAuthenticationHandler
        Inherits CredentialAuthenticationHandler(Of BearerTokenAuthentication)

        Sub New(credentialHelper As IAuthenticationCredentialHelper)
            MyBase.New(credentialHelper)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Handle(request As HttpWebRequest, context As ActionContext)

            Dim accessToken = GetCredential(context).Password
            request.Headers.Add("Authorization",
                                BearerTokenAuthenticationHelper.GetAuthorizationHeaderValue(accessToken))

        End Sub

        ''' <inheritdoc />
        Public Overrides Sub BeforeRetry(context As ActionContext)
            Throw New NotImplementedException()
        End Sub

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property RetryAttemptsOnUnauthorizedException As Integer
            Get
                Return 0
            End Get
        End Property
    End Class

End Namespace