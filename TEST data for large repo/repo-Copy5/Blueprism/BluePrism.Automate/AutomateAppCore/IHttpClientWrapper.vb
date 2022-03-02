Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
Imports IdentityModel.Client

Public interface IHttpClientWrapper
    Function SendAsync(request As HttpRequestMessage, completionOption As HttpCompletionOption,
                       cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)

    Function RequestClientCredentialsTokenAsync(request As ClientCredentialsTokenRequest) As Task(Of ITokenResponseWrapper)
end interface
