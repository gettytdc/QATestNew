Imports System.Xml

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.SoapException
''' 
''' <summary>
''' Class for holding soap exceptions.
''' </summary>
<Serializable>
Public Class clsSoapException
    Inherits Exception
    Private msMessage As String

    Public Sub New(ByVal soap As XmlDocument)
        Dim nsm As New XmlNamespaceManager(soap.NameTable)
        nsm.AddNamespace("s", clsSoap.NamespaceURI)
        Dim xFault As XmlNode = soap.SelectSingleNode("/s:Envelope/s:Body/s:Fault", nsm)
        Dim xFaultstring As XmlNode = xFault.SelectSingleNode("faultstring")
        If Not xFaultstring Is Nothing Then
            msMessage = xFaultstring.InnerText
        Else
            msMessage = xFault.InnerText
        End If

        If msMessage = "" Then
            msMessage = xFault.InnerXml
        End If

        If msMessage = "" Then
            msMessage = My.Resources.Resources.clsSoapException_AnExceptionOccouredInTheWebServiceButTheWebServiceDidNotReturnAnyFurtherDetails
        End If
    End Sub

    Public Overrides ReadOnly Property Message() As String
        Get
            Return msMessage
        End Get
    End Property
End Class

