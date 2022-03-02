
Imports System.Net
Imports System.Text
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Applies HTTP basic authentication during HTTP requests made when executing a 
    ''' Web API action
    ''' </summary>
    Public Class BasicAuthenticationHandler
        Inherits CredentialAuthenticationHandler(Of BasicAuthentication)

        Sub New(credentialHelper As IAuthenticationCredentialHelper)
            MyBase.New(credentialHelper)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Handle(request As HttpWebRequest, context As ActionContext)

            Dim basicAuth = TryCast(context.Configuration.CommonAuthentication, BasicAuthentication)
            If basicAuth Is Nothing Then
                Throw New ArgumentException("Expected BasicAuthentication", NameOf(context))
            End If
            Dim credential = GetCredential(context)

            If basicAuth.IsPreEmptive Then
                ' Default encoding is ASCII as this is what is specified in the HTTP spec
                Dim headerValue = BasicAuthenticationHelper.GetAuthorizationHeaderValue(credential.Username, credential.Password, Encoding.ASCII)
                request.Headers.Add("Authorization", headerValue)
            Else
                request.Credentials = New NetworkCredential(credential.Username, credential.Password)
            End If

        End Sub

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property RetryAttemptsOnUnauthorizedException As Integer
            Get
                Return 0
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Sub BeforeRetry(CONTEXT As ActionContext)
            Throw New NotImplementedException()
        End Sub

    End Class

End NameSpace