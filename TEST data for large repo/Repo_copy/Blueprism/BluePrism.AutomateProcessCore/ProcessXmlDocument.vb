Imports System.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Xml
Imports BluePrism.AutomateProcessCore.Processes

''' <summary>
''' Class to encapsulate a process XML document.
''' It really only adds a constructor which takes the string XML for the process and
''' performs basic validation on that XML.
''' </summary>
Public Class ProcessXmlDocument : Inherits ReadableXmlDocument

    ''' <summary>
    ''' Creates a new XmlDocument representing a process from the given XML.
    ''' </summary>
    ''' <param name="xml">The XML which describes the process to put into an
    ''' XmlDocument object.</param>
    ''' <exception cref="ArgumentNullException">If the given XML is null or empty.
    ''' </exception>
    ''' <exception cref="XmlException">If the given string could not be parsed into
    ''' XML.</exception>
    ''' <exception cref="InvalidFormatException">If the given XML did not represent
    ''' a process - ie. its root node was something other than "process"</exception>
    Public Sub New(ByVal xml As String)

        If xml = "" Then Throw New ArgumentNullException(NameOf(xml),
         My.Resources.Resources.ProcessXmlDocument_TheXMLDefinitionForTheProcessWasNotAvailable)

        ' Make sure it looks sensible before even trying to parse the XML, because we
        ' try and load the clipboard here and don't want to keep trying to parse the
        ' whole string if it's obviously nonsense.
        If xml(0) <> "<"c Then Throw New XmlException(My.Resources.Resources.ProcessXmlDocument_NotAValidProcess)

        LoadXml(xml)

        Dim root As XmlElement = DocumentElement
        If root.Name <> "process" Then
            Throw New InvalidFormatException(
             My.Resources.Resources.ProcessXmlDocument_ThisIsNotAValidProcessDefinitionRootElementIs0ItShouldBeProcess, root.Name)
        End If
    End Sub

    ''' <summary>
    ''' Gets the process type defined in this XML
    ''' </summary>
    ''' <returns>The process type set in the root element of this document, falling
    ''' back to <see cref="DiagramType.Process"/> if no process type is set in
    ''' this document</returns>
    Public ReadOnly Property ProcessType() As DiagramType
        Get
            ' It would be nicer if this would return "object","process" or "unset"
            ' but I don't know what's relying on the current fallback behaviour
            Select Case DocumentElement.GetAttribute("type")
                Case "object" : Return DiagramType.Object
                Case Else : Return DiagramType.Process
            End Select
        End Get
    End Property

    Public ReadOnly Property Version As String
        Get
            Return DocumentElement.GetAttribute("version")
        End Get
    End Property
End Class
