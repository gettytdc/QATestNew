#If UNITTESTS

Imports System.IO
Imports System.Net
Imports System.Reflection
Imports Moq

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Helper class for creating mock HttpWebRequests for use in unit tests
    ''' </summary>
    Public Class MockRequestHelper

        Private Shared ReadOnly HeadersField As FieldInfo

        ''' <summary>
        ''' Static constructor used for initialisation of shared fields used for 
        ''' reflection
        ''' </summary>
        Shared Sub New()
            HeadersField = GetType(HttpWebRequest).
                GetField("_HttpRequestHeaders", BindingFlags.Instance Or
                BindingFlags.NonPublic)
            If HeadersField Is Nothing Then
                Throw New MissingFieldException("Unable to access HttpWebRequest field required for testing")
            End If
        End Sub

        ''' <summary>
        ''' Creates a mock HttpWebRequest object using Moq, setting up properties 
        ''' and methods so that its state can be checked in unit tests. The Headers 
        ''' property is initialised with a WebHeaderCollection. A MemoryStream is 
        ''' substituted for the default stream returned by GetRequestStream. 
        ''' </summary>
        ''' <remarks>The mock object setup may need to be extended as tests for 
        ''' other aspects of an HttpWebRequest's state are required.</remarks>
        Public Shared Function Create(Optional headers As WebHeaderCollection = Nothing) As Mock(Of HttpWebRequest)

            Dim mock As New Mock(Of HttpWebRequest)
            mock.CallBase = True

            ' Headers setup - HttpWebRequest's own methods access headers directly 
            ' via a field hence the reflection
            headers = If(headers, New WebHeaderCollection())
            mock.Setup(Function(r) r.Headers).Returns(headers)
            HeadersField.SetValue(mock.Object, headers)

            Dim requestStream As New MemoryStream()
            mock.Setup(Function(r) r.GetRequestStream()).Returns(requestStream)

            mock.Setup(Function(r) r.Method).Returns("POST")

            Return mock

        End Function
    End Class

End Namespace

#End If
