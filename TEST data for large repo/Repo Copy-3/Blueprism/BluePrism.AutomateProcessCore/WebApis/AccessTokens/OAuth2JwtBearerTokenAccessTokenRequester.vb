Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Web
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.Core.Utility
Imports Newtonsoft.Json

Namespace WebApis.AccessTokens

    Public Class OAuth2JwtBearerTokenAccessTokenRequester : Implements IOAuth2JwtBearerTokenAccessTokenRequester

        Private ReadOnly mJwtBuilder As IJwtBuilder

        ''' <summary>
        ''' Creates a new instance of the <see cref="OAuth2JwtBearerTokenAccessTokenRequester"/> 
        ''' class.
        ''' </summary>
        ''' <param name="jwtBuilder">The <see cref="JwtBuilder"/>to be used to create 
        ''' the signed jwt</param>
        Sub New(jwtBuilder As IJwtBuilder)
            mJwtBuilder = jwtBuilder
        End Sub

        '''<inheritdoc/>
        Public Function RequestAccessToken(context As ActionContext, credential As ICredential) _
            As AccessToken Implements IAccessTokenRequester.RequestAccessToken

            Dim oAuthWithJwt = TryCast(context.Configuration.CommonAuthentication, OAuth2JwtBearerTokenAuthentication)
            If oAuthWithJwt Is Nothing Then _
                Throw New ArgumentException("Expected OAuth2JwtBearerTokenAccessTokenRequester", NameOf(context))

            Dim request As HttpWebRequest = WebRequest.CreateHttp(oAuthWithJwt.AuthorizationServer)
            request.Timeout = context.Configuration.ConfigurationSettings.AuthServerRequestConnectionTimeout * 1000

            Dim jwt = mJwtBuilder.BuildJwt(oAuthWithJwt.JwtConfiguration, credential, context.Parameters)
            BuildAccessTokenRequest(request, jwt)

            Return RequestOAuth2WithJwtAccessToken(request)

        End Function

        Public Function RequestOAuth2WithJwtAccessToken(request As HttpWebRequest) As AccessToken
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

        Public Sub BuildAccessTokenRequest(request As HttpWebRequest, jwt As String)
            request.Method = "POST"
            request.ContentType = "application/x-www-form-urlencoded"
            Dim content = GetRequestContent(jwt)
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

        Public Function GetRequestContent(jwt As String) As Byte()
            Dim jwtBearerGrantType = HttpUtility.UrlEncode("urn:ietf:params:oauth:grant-type:jwt-bearer")
            Dim jwtEncoded = HttpUtility.UrlEncode(jwt)
            Dim requestParameters = $"grant_type={jwtBearerGrantType}&assertion={jwtEncoded}"

            Return Encoding.UTF8.GetBytes(requestParameters)
        End Function
    End Class
End Namespace