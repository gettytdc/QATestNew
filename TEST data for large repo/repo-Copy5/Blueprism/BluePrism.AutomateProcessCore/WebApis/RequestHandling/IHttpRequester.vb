Imports System.Net

Namespace WebApis.RequestHandling
    ''' <summary>
    ''' Co-ordinates the building and sending of an HTTP Request to a Web API action
    ''' </summary>
    Public Interface IHttpRequester
        ''' <summary>
        ''' Builds an HTTP request based on the supplied configuration, executes the
        ''' request and returns the response. If the action is configured to disable
        ''' sending of the request, then the request will not be sent and the 
        ''' <see cref="HttpWebResponse"/> stored within the <see cref="HttpResponseData"/>
        ''' will be empty. 
        ''' </summary>
        ''' <param name="context">Contains details about the Web API action</param>
        ''' <returns><see cref="HttpResponseData"/> object containing information 
        ''' about the HTTP request and response. If sending of the request is 
        ''' disabled, then the response will be null</returns>
        Function GetResponse(context As ActionContext) As HttpResponseData
    End Interface

End Namespace
