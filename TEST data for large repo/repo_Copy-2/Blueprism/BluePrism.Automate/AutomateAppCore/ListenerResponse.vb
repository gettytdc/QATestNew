Imports System.Xml
Imports System.Net
Imports BluePrism.AutomateProcessCore

''' <summary>
''' The ListenerResponse class is used within the ListenThread to encapsulate the
''' details of a response to a received command.
''' </summary>
Public Class ListenerResponse

    ''' <summary>
    ''' Constructor for a new default response, which is actually no response
    ''' at all. The HTTPStatus defaults to 200, so if the HTTP flag is set, the
    ''' status need not be set unless the status is something other than that.
    ''' Likewise, the HTTPContentType defaults to "text/plain".
    ''' </summary>
    Public Sub New()
        Text = Nothing
        HTTP = False
        HTTPStatus = HttpStatusCode.OK
        HTTPContentType = "text/plain"
        HTTPCharset = "utf-8"
        HTTPHeaders = New List(Of String)
    End Sub

    ''' <summary>
    ''' Make this response object a SOAP Fault.
    ''' </summary>
    ''' <param name="sMessage"></param>
    ''' <remarks></remarks>
    Public Sub SOAPFault(ByVal sMessage As String)
        HTTP = True
        HTTPStatus = HttpStatusCode.OK
        HTTPContentType = "text/xml"
        HTTPCharset = "utf-8"
        Dim xBody As XmlElement = Nothing
        Dim x As XmlDocument = clsSoap.SetupSoapEnvelope(xBody, Nothing)
        Dim xFault As XmlElement = x.CreateElement(clsSoap.Prefix, "Fault", clsSoap.NamespaceURI)
        Dim xFaultString As XmlElement = x.CreateElement(clsSoap.Prefix, "faultstring", clsSoap.NamespaceURI)
        Dim txtText As XmlNode = x.CreateTextNode(sMessage)
        xBody.AppendChild(xFault)
        xFault.AppendChild(xFaultString)
        xFaultString.AppendChild(txtText)
        Text = x.OuterXml
    End Sub

    'The text of the response - relevant regardless of the response type. If this
    'is Nothing, no response at all will be sent.
    Public Text As String

    'If True, the response is in HTTP format. Otherwise, it is plain text.
    Public HTTP As Boolean

    'When in HTTP mode, this is the status code for the response
    Public HTTPStatus As HttpStatusCode

    'When in HTTP mode, this is the content type (MIME) for the response, e.g.
    '"text/plain"
    Public HTTPContentType As String

    'When in HTTP mode, this is the character set for the response, e.g.
    '"utf-8"
    Public HTTPCharset As String

    'A list of additional HTTP headers to send with the response.
    Public HTTPHeaders As List(Of String)

    ''' <summary>
    ''' Returns the length (in bytes) of the response text
    ''' </summary>
    Public ReadOnly Property ContentLength As Integer
        Get
            Return Encoding.UTF8.GetByteCount(Text)
        End Get
    End Property

End Class
