Imports System.Net

Namespace WebApis.RequestHandling.BodyContent

    ''' <summary>
    ''' Defines the content that will be sent with a HTTP request and handles writing
    ''' the content
    ''' </summary>
    Public Interface IBodyContentResult

        ''' <summary>
        ''' A copy of the contnet that can be made available if displaying the request
        ''' data
        ''' </summary>
        ReadOnly Property Content As String

        ''' <summary>
        ''' Write the body content result to the http stream
        ''' </summary>
        Sub Write(request As HttpWebRequest)

        ''' <summary>
        ''' Any HTTP headers related to the content that will need to be added to
        ''' the HTTP Request
        ''' </summary>
        ReadOnly Property Headers As IReadOnlyCollection(Of HttpHeader)

    End Interface

End NameSpace