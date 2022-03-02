Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Applies authentication behaviour during the HTTP request that is made when
    ''' executing a Web API action
    ''' </summary>
    Public Interface IAuthenticationHandler
    
        ''' <summary>
        ''' Indicates whether this IAuthenticationHandler can handle authentication 
        ''' based on the configuration specified
        ''' </summary>
        ''' <param name="authentication">The authentication that applies to the Web API action</param>
        ''' <returns>A boolean value indicating whether authentication can be handled</returns>
        Function CanHandle(authentication As IAuthentication) As Boolean

        ''' <summary>
        ''' Executes any behaviour needed to apply the authentication and modifies
        ''' the request as required
        ''' </summary>
        ''' <param name="request">The HTTP request to apply changes to based on
        ''' authentication behaviour</param>
        ''' <param name="context">The information about the Web API request that is
        ''' being made</param>
        Sub Handle(request As HttpWebRequest, context As ActionContext)

        ''' <summary>
        ''' Indicates the number of times the HTTP request should be re-attempted if 
        ''' the Web API request gets a 401 status code
        ''' </summary>
        ''' <returns>Indicates whether the HTTP request should be re-attempted if the 
        ''' Web API request gets a 401 status code</returns>
        ReadOnly Property RetryAttemptsOnUnauthorizedException() As Integer

        ''' <summary>
        ''' Method that is called before a new HTTP Request is re-attempted (when
        ''' the first attempt resulted in an exception)
        ''' </summary>
        ''' <param name="context">The information about the Web API request that is
        ''' being made</param>
        Sub BeforeRetry(context As ActionContext)

        ''' <summary>
        ''' The parameters constructed from the credential selected for this authentication configuration.
        ''' </summary>
        ''' <param name="context">The information about the Web API request that is
        ''' being made</param>
        ''' <returns>A dictionary of parameters, where the key is the parameter placeholder 
        ''' and the value is its replacement</returns>
        Function GetCredentialParameters(context As ActionContext) As Dictionary(Of String, clsProcessValue)

    End Interface
End NameSpace