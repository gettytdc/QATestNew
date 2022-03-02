
Imports System.Linq
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Applies custom authentication during HTTP requests made when executing a 
    ''' Web API action
    ''' </summary>
    Public Class CustomAuthenticationHandler
        Inherits CredentialAuthenticationHandler(Of CustomAuthentication)

        Sub New(credentialHelper As IAuthenticationCredentialHelper)
            MyBase.New(credentialHelper)
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub Handle(request As HttpWebRequest, context As ActionContext)
            Dim auth = TryCast(context.Configuration.CommonAuthentication, CustomAuthentication)
            If auth Is Nothing Then
                Throw New ArgumentException("Expected CustomAuthenctication", NameOf(context))
            End If
        End Sub

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property RetryAttemptsOnUnauthorizedException As Integer
            Get
                Return 0
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides Function GetCredentialParameters(context As ActionContext) As Dictionary(Of String, clsProcessValue)
            Dim credential = GetCredential(context)

            Dim result = New Dictionary(Of String, clsProcessValue)
            result.Add("Credential.Username", credential.Username)
            result.Add("Credential.Password", credential.Password)
            credential.Properties.ToList().ForEach(Sub(k) result.Add($"Credential.AdditionalProperties.{k.Key}", k.Value))
            Return result
        End Function

        ''' <inheritdoc/>
        Public Overrides Sub BeforeRetry(context As ActionContext)
            Throw New NotImplementedException()
        End Sub

    End Class

End Namespace