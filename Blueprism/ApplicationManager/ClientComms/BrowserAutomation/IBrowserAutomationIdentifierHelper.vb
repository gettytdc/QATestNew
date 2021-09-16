Imports System.Collections.Generic

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BrowserAutomation
Imports BluePrism.BrowserAutomation.Events

Namespace BrowserAutomation

    Public Interface IBrowserAutomationIdentifierHelper

        ''' <summary>
        ''' Gets web element attribute identifiers
        ''' </summary>
        ''' <param name="element">The web element whose information should be retrieved</param>
        ''' <returns>
        ''' A collection of strings.
        ''' </returns>
        Function GetIdentifiers(element As IWebElement) As IReadOnlyCollection(Of String)

        ''' <summary>
        ''' Finds the element based on the parmeters in the query.
        ''' </summary>
        ''' <param name="query">The query.</param>
        ''' <returns>The found element or <c>null</c> if no element is found.</returns>
        Function FindSingleElement(query As clsQuery) As IWebElement

        Function FindElements(query As clsQuery) As IReadOnlyCollection(Of IWebElement)

        Function CheckParentDocumentLoaded() As Boolean

        Function GetActiveWebPages() As IReadOnlyCollection(Of IWebPage)

        Function ActiveMessagingHost() As IWebPage

        Function GetWebPages(query As clsQuery) As IReadOnlyCollection(Of IWebPage)

        Sub DetachTrackedWebPage(trackingId As String)

        Sub DetachAllTrackedPages()
        Function IsTracking() As Boolean

        Function IsTracking(trackingId As string) As Boolean

        Sub  RemoveWebPage(pageId As Guid)

        Sub SetProcHandle(procHandle As IntPtr)

        Sub CloseActivePages()

        Event OnWebPageCreated As WebPageCreatedDelegate
    End Interface
End Namespace
