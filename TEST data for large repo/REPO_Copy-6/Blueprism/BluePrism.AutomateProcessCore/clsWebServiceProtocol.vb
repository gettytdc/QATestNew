Imports System.Net
Imports System.Security.Cryptography.X509Certificates

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsWebServiceProtocol
''' 
''' <summary>
''' The base class for all web service protocols
''' </summary>
''' <remarks></remarks>
Public MustInherit Class clsWebServiceProtocol

    ''' <summary>
    ''' Event raised with diagnostics messages (details of SOAP communication, etc).
    ''' (plus a helper to raise the event from a derived class)
    ''' </summary>
    Public Event Diags(ByVal msg As String)
    Protected Sub RaiseDiags(ByVal msg As String)
        RaiseEvent Diags(msg)
    End Sub

    ''' <summary>
    ''' Web service protocols must override the DoAction method to provide a specific
    ''' implementation depending on the protocol in use.
    ''' </summary>
    ''' <param name="colWebParameters">The names of the parameters</param>
    ''' <param name="inputs">The input parameter values</param>
    ''' <param name="outputs">The output parameter values</param>
    Public MustOverride Sub DoAction(ByVal colWebParameters As IList(Of clsProcessParameter), ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList)

    ''' <summary>
    ''' The action of the webservice
    ''' </summary>
    Public Property Action() As clsWebServiceAction
        Get
            Return mAction
        End Get
        Set(ByVal value As clsWebServiceAction)
            mAction = value
        End Set
    End Property
    Protected mAction As clsWebServiceAction

    ''' <summary>
    ''' The location of the web service
    ''' </summary>
    Public Property Location() As String
        Get
            Return mLocation
        End Get
        Set(ByVal value As String)
            mLocation = value
        End Set
    End Property
    Private mLocation As String

    ''' <summary>
    ''' The namespace of the web service
    ''' </summary>
    Public Property [Namespace]() As String
        Get
            Return msNamespace
        End Get
        Set(ByVal value As String)
            msNamespace = value
        End Set
    End Property
    Private msNamespace As String

    ''' <summary>
    ''' Gets a new WebRequest based on the location URL
    ''' </summary>
    ''' <param name="sLocation">The URL of the web service</param>
    ''' <param name="timeout">The configured timeout, in milliseconds</param>
    ''' <returns>A WebRequest instance</returns>
    Protected Function GetWebRequest(ByVal sLocation As String, ByVal timeout As Integer) As WebRequest
        Dim request As WebRequest = WebRequest.Create(sLocation)
        Dim httpRequest As HttpWebRequest = TryCast(request, HttpWebRequest)
        If httpRequest IsNot Nothing AndAlso Me.Certificate IsNot Nothing Then
            httpRequest.ClientCertificates.Add(Me.Certificate)
        End If
        request.Credentials = Me.Credentials
        request.Timeout = timeout
        Return request
    End Function

    ''' <summary>
    ''' Gets a Web Response given a web request
    ''' </summary>
    ''' <param name="request">The request to get the response for</param>
    ''' <returns>A WebResponse instance</returns>
    Protected Overridable Function GetWebResponse(ByVal request As WebRequest) As WebResponse
        Dim response As WebResponse = Nothing
        Try
            response = request.GetResponse
        Catch exception As WebException
            If (exception.Response Is Nothing) Then
                Throw exception
            End If
            response = exception.Response
        End Try
        Return response
    End Function

    ''' <summary>
    ''' The network credentials for the web service.
    ''' </summary>
    Public Property Credentials() As NetworkCredential
        Get
            Return mCredentials
        End Get
        Set(ByVal value As Net.NetworkCredential)
            mCredentials = value
        End Set
    End Property
    Private mCredentials As NetworkCredential

    ''' <summary>
    ''' The client certificate for the web service.
    ''' </summary>
    Public Property Certificate() As X509Certificate2
        Get
            Return mCertificate
        End Get
        Set(ByVal value As X509Certificate2)
            mCertificate = value
        End Set
    End Property
    Private mCertificate As X509Certificate2

    ''' <summary>
    ''' The timeout to be used, in milliseconds.
    ''' </summary>
    Public Property Timeout() As Integer
        Get
            Return mTimeout
        End Get
        Set(ByVal value As Integer)
            mTimeout = value
        End Set
    End Property
    Protected mTimeout As Integer

End Class
