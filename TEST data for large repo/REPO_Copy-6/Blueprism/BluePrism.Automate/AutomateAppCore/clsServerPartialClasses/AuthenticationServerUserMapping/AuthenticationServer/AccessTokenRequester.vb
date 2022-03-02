Imports System.Security
Imports System.Threading.Tasks
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens
Imports BluePrism.Common.Security
Imports BluePrism.Core.Utility
Imports IdentityModel.Client
Imports NLog

Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
    Public Class AccessTokenRequester
        Implements IAccessTokenRequester

        Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()
        Private ReadOnly mHttpClientWrapper As IHttpClientWrapper
        Private ReadOnly mSystemClock As ISystemClock

        Public Sub New(httpClientWrapper As IHttpClientWrapper, systemClock As ISystemClock)
            If httpClientWrapper Is Nothing Then Throw New ArgumentNullException(NameOf(httpClientWrapper))
            mHttpClientWrapper = httpClientWrapper

            If systemClock Is Nothing Then Throw New ArgumentNullException(NameOf(systemClock))
            mSystemClock = systemClock
        End Sub

        Public Async Function RequestAccessToken(authenticationServerUrl As String, clientId As String, clientSecret As SecureString) _
            As Task(Of AccessToken) Implements IAccessTokenRequester.RequestAccessToken

            Try
                Dim response = Await mHttpClientWrapper.RequestClientCredentialsTokenAsync(New ClientCredentialsTokenRequest With {
                                                                                       .Address = If(authenticationServerUrl.Contains("/connect/token"), authenticationServerUrl, $"{authenticationServerUrl}/connect/token"),
                                                                                       .ClientId = clientId,
                                                                                       .ClientSecret = clientSecret.AsString()
                                                                                       })

                If response.IsError Then
                    If response.Exception Is Nothing Then
                        Log.Error(response.Error)
                    Else
                        Log.Error(response.Exception, response.Error)
                    End If

                    Return Nothing
                End If

                Return New AccessToken(response.AccessToken, mSystemClock.UtcNow.UtcDateTime.AddSeconds(response.ExpiresIn), True)
            Catch ex As Exception
                Log.Error(ex, "Could not get authentication token for Authentication Server")

                Return Nothing
            End Try
        End Function
    End Class
End Namespace
