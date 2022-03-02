Imports System.Net

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Contains the result of creating and executing an HTTP request and the 
    ''' resulting response. 
    ''' </summary>
    Public Class HttpResponseData : Implements IDisposable

        ''' <summary>
        ''' Creates a new <see cref="HttpResponseData" />
        ''' </summary>
        ''' <param name="requestData">Contains details of the request</param>
        ''' <param name="response">Contains details of the HTTP response. This can 
        ''' be null if the request has not been sent.</param>
        Public Sub New(requestData As HttpRequestData, response As HttpWebResponse)
            Me.RequestData = requestData
            Me.Response = response
        End Sub

        ''' <summary>
        ''' Contains details of the request being made
        ''' </summary>
        ''' <remarks>The requestStream associated with this object may not be 
        ''' accessible through GetRequestStream if the request is a GET request 
        ''' or if the stream has been correctly closed prior to getting the response.</remarks>
        Public ReadOnly Property RequestData As HttpRequestData

        ''' <summary>
        ''' Contains details of the HTTP response. This can be null if the request
        ''' has not been sent.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Response As HttpWebResponse

        Public Sub Dispose() Implements IDisposable.Dispose
            Response?.Dispose()
        End Sub
    End Class

End NameSpace