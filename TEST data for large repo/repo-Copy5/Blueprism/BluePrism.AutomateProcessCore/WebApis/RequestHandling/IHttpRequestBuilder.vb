Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Use to build a <see cref="HttpWebRequest"/> that can then be used to call a 
    ''' particular Web API action.
    ''' </summary>
    Public Interface IHttpRequestBuilder

        ''' <summary>
        ''' Builds a <see cref="HttpWebRequest"/> based on a Web API action
        ''' </summary>
        ''' <param name="context">Context containing information about the action</param>
        ''' <param name="authenticationHandler">The handler to use when adding the
        ''' authentication to the request</param>
        ''' <param name="bodyContentHandler">The handler to use when adding the body
        ''' content to the request</param>
        ''' <returns>The HttpWebRequest that can be used to call the Web API action</returns>
        Function Build(context As ActionContext, 
                       authenticationHandler As IAuthenticationHandler,
                       bodyContentHandler As IBodyContentGenerator) As HttpRequestData
    End Interface
End Namespace
