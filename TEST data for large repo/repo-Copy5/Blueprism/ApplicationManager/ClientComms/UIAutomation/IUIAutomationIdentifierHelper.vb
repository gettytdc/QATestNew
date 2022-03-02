Imports System.Collections.Generic
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.UIAutomation
Imports BluePrism.Server.Domain.Models

Public Interface IUIAutomationIdentifierHelper

    ''' <summary>
    ''' Append identifiers to a string builder, that will form the spied
    ''' attributes of the Automation Element
    ''' </summary>
    ''' <param name="element">The automation element whose information should be
    ''' appended to the stringbuilder</param>
    ''' <param name="sb">The string builder to which the information should be
    ''' appended.</param>
    ''' <returns>
    ''' The given string builder with the information appended to it.
    ''' </returns>
    Function AppendIdentifiers(
        element As IAutomationElement, sb As StringBuilder) As StringBuilder

    ''' <summary>
    ''' Returns whether the specified AutomationElement matches the identifiers
    ''' passed in
    ''' </summary>
    ''' <param name="element">The automation element to try and match</param>
    ''' <param name="identifiers">The identifiers to match against the automation
    ''' element</param>
    ''' <param name="includeBounds">Indicates whether to perform match on
    ''' bounds elements</param>
    Function Matches(
        element As IAutomationElement,
        identifiers As IEnumerable(Of clsIdentifierMatchTarget),
        cache As IAutomationCacheRequest,
        includeBounds As Boolean) As Boolean

    ''' <summary>
    ''' Checks if the given automation element matches the given constraint,
    ''' optionally including bounds checks.
    ''' </summary>
    ''' <param name="element">The automation element to test</param>
    ''' <param name="cache">The cache request to use to retrieve information
    ''' about the automation element.</param>
    ''' <param name="includeBounds">True to include bounds checks; False to
    ''' disregard bounds information when checking this element.</param>
    ''' <param name="constraint">The constraint to test against the automation
    ''' element.</param>
    ''' <returns>True if the information regarding the automation element passes
    ''' the given constraint or if it is to be disregarded; False otherwise.
    ''' </returns>
    ''' <remarks>Note that if the constraint is an ancestor constraint and this
    ''' element has no parent, it will match if the constraint matches against an
    ''' empty string (signifying 'no parent attribute value'), and fail to match
    ''' if it is matches against anything else.</remarks>
    Function Matches(
        element As IAutomationElement,
        cache As IAutomationCacheRequest,
        includeBounds As Boolean,
        constraint As clsIdentifierMatchTarget) As Boolean

    ''' <summary>
    ''' Finds the UIAutomation element which matches the given query for the
    ''' process running with the given ID.
    ''' </summary>
    ''' <param name="q">The query to match against</param>
    ''' <param name="pid">The ID of the process to search.</param>
    ''' <returns>The single automation element which matches the given query.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no UIA element could be found
    ''' which matched the constraints set in the query.</exception>
    ''' <exception cref="TooManyElementsException">If more than one UIA element
    ''' was found which matched the constraints set in the query.</exception>
    Function FindUIAutomationElement(q As clsQuery, pid As Integer) As IAutomationElement

    ''' <summary>
    ''' Find all the automation elements that match the search query
    ''' </summary>
    ''' <param name="q">Query containing information about what to elements on
    ''' </param>
    ''' <param name="pid">The process id of the application containing the element
    ''' </param>
    ''' <returns>The automation elements that match the query</returns>
    Function FindAllUIAutomationElements(
         q As clsQuery, pid As Integer) As IReadOnlyCollection(Of IAutomationElement)

    ''' <summary>
    ''' Find the top level automation element associated with the specified process.
    ''' </summary>
    ''' <param name="pid">The process id to search for</param>
    ''' <returns>The UI Automation element of the process's top level window
    ''' </returns>
    Function FindProcessWindowElements(pid As Integer) _
         As IReadOnlyCollection(Of IAutomationElement)

    ''' <summary>
    ''' Find the top level automation element associated with the specified process.
    ''' </summary>
    ''' <param name="pid">The process id to search for</param>
    ''' <param name="windowId">The AutomationID of the top level window of
    ''' the application or null to look in all windows
    ''' </param>
    ''' <returns>The UI Automation element of the process window</returns>
    Function FindProcessWindowElements(pid As Integer, windowId As String) _
         As IReadOnlyCollection(Of IAutomationElement)

    ''' <summary>
    ''' Find the top level automation element associated with the specified process.
    ''' </summary>
    ''' <param name="pid">The process id to search for</param>
    ''' <param name="windowId">The AutomationID of the top level window of
    ''' the application or null to look in all windows
    ''' </param>
    ''' <param name="cache">The cache request to use to retrieve information
    ''' about the automation element.</param>
    ''' <returns>The UI Automation element of the process window</returns>
    Function FindProcessWindowElements(pid As Integer, windowId As String,
                                       cache As IAutomationCacheRequest) _
        As IReadOnlyCollection(Of IAutomationElement)

    ''' <summary>
    ''' Gets the comparison function for the given constraint.
    ''' </summary>
    ''' <param name="constraint">The constraint.</param>
    ''' <returns></returns>
    Function GetComparisonFunction(constraint As clsIdentifierMatchTarget) _
        As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean)

    ''' <summary>
    ''' Gets cache request ready for comparison.
    ''' </summary>
    ''' <returns>The cache request</returns>
    Function GetCacheRequestForComparison() As IAutomationCacheRequest


End Interface
