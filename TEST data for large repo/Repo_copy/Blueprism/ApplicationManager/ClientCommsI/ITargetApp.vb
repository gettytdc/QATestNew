
Imports System.Drawing
Imports BluePrism.ApplicationManager.HTML

''' Project  : ApplicationManagerUtilities
''' Interface: ITargetApp
''' <summary>
''' Defines interface for services provided by ClientComms.clsTargetApp to
''' modules below it in the Application Manager hierarchy.
''' </summary>
Public Interface ITargetApp

    ''' <summary>
    ''' Get the PID of the target application currently connected.
    ''' </summary>
    ReadOnly Property PID() As Integer

    ''' <summary>
    ''' Get a list of all IHTMLDocument interfaces exposed by the target application.
    ''' </summary>
    ''' <returns>A List(Of clsHTMLDocument)</returns>
    Function GetHtmlDocuments() As List(Of clsHTMLDocument)

    ''' <summary>
    ''' Determine if SAP Gui API connectivity is available on this application.
    ''' </summary>
    ReadOnly Property SAPAvailable() As Boolean

    Function GetSapComponentFromPoint(ByVal pt As Point) As String

    Function GetSapComponentScreenRect(ByVal id As String) As Rectangle

End Interface
