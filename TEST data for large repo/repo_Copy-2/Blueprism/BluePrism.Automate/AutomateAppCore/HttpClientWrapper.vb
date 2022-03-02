Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
Imports IdentityModel.Client

Public Class HttpClientWrapper
    Implements IHttpClientWrapper

    Private ReadOnly mHttpClient As HttpClient

    Public Sub New(httpClient As HttpClient)
        mHttpClient = httpClient
    End Sub

    Public Function SendAsync(request As HttpRequestMessage, completionOption As HttpCompletionOption,
                              cancellationToken As CancellationToken) As Task (Of HttpResponseMessage) Implements IHttpClientWrapper.SendAsync
        Return mHttpClient.SendAsync(request, completionOption, cancellationToken)
    End Function

    public Async Function RequestClientCredentialsTokenAsync(request As ClientCredentialsTokenRequest) _
        As Task(Of ITokenResponseWrapper) Implements IHttpClientWrapper.RequestClientCredentialsTokenAsync
        Dim token = await mHttpClient.RequestClientCredentialsTokenAsync(request)
        Return New TokenResponseWrapper(token)
    End Function
End Class
