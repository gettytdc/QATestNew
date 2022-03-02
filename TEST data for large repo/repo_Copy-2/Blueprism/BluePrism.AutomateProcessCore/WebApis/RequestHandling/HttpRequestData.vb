Imports System.Net

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Contains details of an HttpRequest, containing the request itself together
    ''' with content written when making the request.
    ''' </summary>
    Public Class HttpRequestData

        ''' <summary>
        ''' Creates a new <see cref="HttpRequestData"/>
        ''' </summary>
        ''' <param name="request">The HttpWebRequest object</param>
        ''' <param name="content">The content written when making the request, which 
        ''' is available if request content capture was enabled</param>
       Sub New(request As HttpWebRequest, content As String)

            If request Is Nothing Then
                Throw New ArgumentNullException(NameOf(request))
            End If

            Me.Request = request
            Me.Content = content
        End Sub

        ''' <summary>
        ''' The HttpWebRequest object 
        ''' </summary>
        ''' <remarks>The requestStream associated with this object may not be 
        ''' accessible through GetRequestStream if the request is a GET request 
        ''' or if the stream has been correctly closed prior to getting the response.</remarks>
        Public ReadOnly Property Request As HttpWebRequest

        ''' <summary>
        ''' The content written when making the request, which is available if request 
        ''' content capture was enabled
        ''' </summary>
        ''' <returns>A string containing the raw request content or nothing if 
        ''' capturing was not enabled</returns>
        Public ReadOnly Property Content As String

        
    End Class

End NameSpace