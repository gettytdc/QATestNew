Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Web
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.Common.Security
Imports BluePrism.Core.Utility
Imports Newtonsoft.Json

Public Class OAuth2ClientCredentialsAccessTokenRequester : Implements IOAuth2ClientCredentialsAccessTokenRequester


    ''' <inheritdoc />
    Public Function RequestAccessToken(context As ActionContext, credential As ICredential) _
        As AccessToken Implements IOAuth2ClientCredentialsAccessTokenRequester.RequestAccessToken

        Dim oAuth = TryCast(context.Configuration.CommonAuthentication, OAuth2ClientCredentialsAuthentication)
        If oAuth Is Nothing Then _
            Throw New ArgumentException("Expected OAuth2Authentication", NameOf(context))

        Dim request As HttpWebRequest = WebRequest.CreateHttp(oAuth.AuthorizationServer)
        request.Timeout = context.Configuration.ConfigurationSettings.AuthServerRequestConnectionTimeout * 1000

        BuildAccessTokenRequest(request, oAuth.Scope, credential.Username, credential.Password)

        Return RequestOAuth2AccessToken(request)

    End Function

    ''' <summary>
    ''' Builds the Http Web Request for an OAuth2 Access Token, using the 
    ''' supplied scope, client id and secret.
    ''' </summary>
    ''' <param name="request">The HttpWebRequest created from the authorization uri 
    ''' which will is built up with the correct headers, method and content.)</param>
    ''' <param name="scope">The scope to be used for the acccess token request</param>
    ''' <param name="clientId">The client ID to be used for the acccess token request</param>
    ''' <param name="clientSecret">The sclient secret to be used for the acccess 
    ''' token request</param>
    ''' <exception cref="AccessTokenException">If the remote server cannot be 
    ''' contacted GetRequestStream will throw a WebException.</exception>
    Public Sub BuildAccessTokenRequest(ByRef request As HttpWebRequest, scope As String,
                                       clientId As String, clientSecret As SafeString)
        request.Method = "POST"
        request.Headers.Add(
            "Authorization",
            BasicAuthenticationHelper.GetAuthorizationHeaderValue(
                clientId, clientSecret, Encoding.ASCII))
        request.ContentType = "application/x-www-form-urlencoded"
        Dim content = GetRequestContent(scope)
        request.ContentLength = content.Length
        Try
            Using stream As Stream = request.GetRequestStream()
                stream.Write(content, 0, content.Length)
            End Using
        Catch ex As WebException
            Throw New AccessTokenException(
                WebApiResources.OAuth2AccessTokenExceptionTemplate, ex.Message, Nothing)
        End Try
    End Sub

    ''' <summary>
    ''' Gets the response from the access token httpwebrequest and returns the 
    ''' deserialized <see cref="AccessToken"/>.
    ''' </summary>
    ''' <param name="request">The httpWebRequest for an OAuth2 access token</param>
    ''' <returns>An <see cref="AccessToken"/> deserialized from the HttpWebResponse returned by 
    ''' the authorization server </returns>
    ''' <exception cref="AccessTokenException">If the request fails or the resulting 
    ''' <see cref="AccessToken"/> contains no AccessToken value or cannot be deserialized.</exception>
    Public Function RequestOAuth2AccessToken(request As HttpWebRequest) As AccessToken

        Try
            Using response = DirectCast(request.GetResponse(), HttpWebResponse)
                Dim responseBody = response.GetResponseBodyAsString()
                Dim token = JsonConvert.DeserializeObject(Of AccessToken)(responseBody)
                If String.IsNullOrEmpty(token.AccessToken) Then
                    Throw New AccessTokenException(WebApiResources.OAuth2InvalidTokenException)
                End If
                Return token
            End Using
        Catch ex As WebException
            If ex.Response Is Nothing Then
                Throw New AccessTokenException(WebApiResources.OAuth2AccessTokenExceptionTemplate, ex.Message, Nothing)
            Else
                Using response = DirectCast(ex.Response, HttpWebResponse)
                    Dim responseBody = response.GetResponseBodyAsString()
                    Throw New AccessTokenException(
                        WebApiResources.OAuth2AccessTokenExceptionTemplate, ex.Status, responseBody)
                End Using
            End If
        Catch ex As Exception
            Throw New AccessTokenException(WebApiResources.OAuth2AccessTokenExceptionTemplate, ex.Message, Nothing)
        End Try

    End Function

    ''' <summary>
    ''' Gets the correctly formatted request content, including any specified scope, 
    ''' as a byte array encoded with UTF8
    ''' </summary>
    ''' <param name="scope">Any scope to be included in the acccess token request</param>
    ''' <returns>A UTF8 encoded byte array representing the request content.</returns>
    Public Function GetRequestContent(scope As String) As Byte()
        Dim requestParameters As String =
                                  If(String.IsNullOrWhiteSpace(scope),
                                      "grant_type=client_credentials",
                                      $"grant_type=client_credentials&scope={HttpUtility.UrlEncode(scope)}")

        Return Encoding.UTF8.GetBytes(requestParameters)

    End Function





End Class



