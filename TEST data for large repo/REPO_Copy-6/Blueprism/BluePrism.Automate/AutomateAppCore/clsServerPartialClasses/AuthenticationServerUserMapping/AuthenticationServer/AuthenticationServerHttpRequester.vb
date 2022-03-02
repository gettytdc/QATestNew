Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateProcessCore.WebApis.AccessTokens
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.Core.Utility
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports NLog

Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer

    Public Class AuthenticationServerHttpRequester
        Implements IAuthenticationServerHttpRequester

        Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

        Private ReadOnly mContractResolver As DefaultContractResolver = New DefaultContractResolver With {
            .NamingStrategy = New CamelCaseNamingStrategy()
            }

        Private ReadOnly mAccessTokenRequester As IAccessTokenRequester
        Private ReadOnly mHttpClientWrapper As IHttpClientWrapper
        Private mPreviousAccessToken As AccessToken
        Private ReadOnly mAccessTokenSemaphore As New Semaphore(1, 1)

        ' Limit the number of concurrent requests to the Authentication Server to match the ServicePointManager.DefaultConnectionLimit
        ' so our request doesn't timeout waiting for a connection from the connection pool
        Private ReadOnly mRequestPool As New Semaphore(2, 2)

        Public Sub New(httpClientWrapper As IHttpClientWrapper, accessTokenRequester As IAccessTokenRequester)
            If httpClientWrapper Is Nothing Then Throw New ArgumentNullException(NameOf(httpClientWrapper))
            mHttpClientWrapper = httpClientWrapper

            If accessTokenRequester Is Nothing Then Throw New ArgumentNullException(NameOf(accessTokenRequester))
            mAccessTokenRequester = accessTokenRequester
        End Sub

        Public Async Function PostUser(record As UserMappingRecord, authenticationServerUrl As String, clientCredential As clsCredential) _
            As Task(Of AuthenticationServerUser) Implements IAuthenticationServerHttpRequester.PostUser

            Using request = New HttpRequestMessage(HttpMethod.Post, $"{authenticationServerUrl}/api/user")
                Dim mappedUser = New AuthenticationServerUser(record)
                Dim content = New StringContent(JsonConvert.SerializeObject(mappedUser, New JsonSerializerSettings With {.ContractResolver = mContractResolver}),
                                                Encoding.UTF8, "application/json")

                Try
                    Using response As HttpResponseMessage = Await MakeAuthenticatedRequest(request, authenticationServerUrl, clientCredential, content, True)
                        response.EnsureSuccessStatusCode()
                        Dim responseObject = JsonConvert.DeserializeObject(Of CreateUserResponse)(Await response.Content.ReadAsStringAsync())
                        mappedUser.Id = responseObject.PublicId
                        Return mappedUser
                    End Using
                Catch ex As Exception
                    Log.Error(ex, "Error creating Authentication Server user")
                    Return Nothing
                End Try
            End Using

        End Function

        Public Async Function GetUser(authenticationServerUserId As Guid?, authenticationServerUrl As String, clientCredential As clsCredential) _
            As Task(Of AuthenticationServerUser) Implements IAuthenticationServerHttpRequester.GetUser

            Using request = New HttpRequestMessage(HttpMethod.Get, $"{authenticationServerUrl}/api/user/{authenticationServerUserId}")

                Try
                    Using response As HttpResponseMessage = Await MakeAuthenticatedRequest(request, authenticationServerUrl, clientCredential, Nothing, True)
                        response.EnsureSuccessStatusCode()
                        Dim userResponse = JsonConvert.DeserializeObject(Of AuthenticationServerUser)(Await response.Content.ReadAsStringAsync(),
                                                                                              New JsonSerializerSettings With {.ContractResolver = mContractResolver})

                        Return userResponse
                    End Using
                Catch ex As Exception
                    Log.Error(ex, "Error getting Authentication Server user")
                    Return Nothing
                End Try
            End Using
        End Function

        Private Async Function MakeAuthenticatedRequest(request As HttpRequestMessage, authenticationServerUrl As String, clientCredential As ICredential, content As HttpContent, Optional retryOn401 As Boolean = True) As Task(Of HttpResponseMessage)

            Dim accessToken = String.Empty
            Try
                accessToken = Await GetAccessToken(authenticationServerUrl, clientCredential)
                request.Headers.Remove("Authorization")
                request.Headers.Add("Authorization", $"Bearer {accessToken}")

                request.Content = content

                Return Await MakePooledRequest(request)

            Catch ex As WebException When ex.Is401WebException
                If Not String.IsNullOrEmpty(accessToken) Then InvalidateAccessToken(accessToken)
                If Not retryOn401 Then Throw
            End Try

            Return Await MakeAuthenticatedRequest(request, authenticationServerUrl, clientCredential, content, False)

        End Function

        Private Async Function GetAccessToken(authenticationServerUrl As String, clientCredential As ICredential) As Task(Of String)
            mAccessTokenSemaphore.WaitOne()
            Try
                If mPreviousAccessToken Is Nothing OrElse mPreviousAccessToken.ExpiryDate <= Date.UtcNow Then
                    mPreviousAccessToken = Await mAccessTokenRequester.RequestAccessToken(authenticationServerUrl, clientCredential.Username, clientCredential.Password)
                End If

                Return mPreviousAccessToken.AccessToken

            Catch ex As Exception
                Log.Error(ex, "Could not get authentication token for Authentication Server")
                Throw
            Finally
                mAccessTokenSemaphore.Release()
            End Try
        End Function

        Private Async Function MakePooledRequest(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
            Try
                mRequestPool.WaitOne()

                Return Await mHttpClientWrapper.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None)
            Catch ex As Exception
                Throw
            Finally
                mRequestPool.Release()
            End Try
        End Function

        Private Sub InvalidateAccessToken(accessToken As String)
            mAccessTokenSemaphore.WaitOne()
            Try
                If mPreviousAccessToken.AccessToken.Equals(accessToken) Then
                    mPreviousAccessToken = Nothing
                End If
            Finally
                mAccessTokenSemaphore.Release()
            End Try
        End Sub

    End Class
End Namespace
