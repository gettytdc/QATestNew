Imports System.Web.Services.Description

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsSoapParameter
''' 
''' <summary>
''' A class that represents a soap parameter for the an operation within the web
''' service.
''' </summary>
Public Class clsSoapParameter
    Inherits clsWebParameter

    ''' <summary>
    ''' The required XSD datatype of the parameter.
    ''' </summary>
    Public XSDType As String

    ''' <summary>
    ''' The namespace URI of the parameter.
    ''' </summary>
    Public NamespaceURI As String

    ''' <summary>
    ''' The binding use of the parameter
    ''' </summary>
    ''' <remarks>
    ''' This could do with being abstracted off somewhere else, as we may well be
    ''' supporting other protocols than soap.
    ''' </remarks>
    Public SoapUse As SoapBindingUse

    ''' <summary>
    ''' The encoding style of the parameter
    ''' </summary>
    ''' <remarks>
    ''' This could do with being abstracted off somewhere else, as we may well be
    ''' supporting other protocols than soap.
    ''' </remarks>
    Public EncodingStyle As String = "http://schemas.xmlsoap.org/soap/encoding/"

End Class
