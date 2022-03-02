Imports System.Collections.Generic
Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared
    ''' <summary>
    ''' Provides helper methods for interacting with UI Automation lists
    ''' </summary>
    Module ListHelper
        ''' <summary>
        ''' Gets the list item with the given name.
        ''' </summary>
        ''' <param name="listItems">The list items.</param>
        ''' <param name="itemName">Name of the item.</param>
        ''' <returns>The requested list item</returns>
        ''' <exception cref="ListItemOutOfRangeException">
        ''' Thrown if no list item can be found matching the given string
        ''' </exception>
        Friend Function GetListItem(listItems As IReadOnlyCollection(Of IAutomationElement), itemName As String) _
            As IAutomationElement

            Dim listItem =
                listItems.
                    Where(Function(x) x.PatternIsSupported(PatternType.SelectionItemPattern)).
                    Where(Function(x) x.CurrentName.Equals(itemName, StringComparison.OrdinalIgnoreCase)).
                    FirstOrDefault()

            If listItem Is Nothing
                Throw New ListItemOutOfRangeException(ParameterNames.IDName)
            End If

            Return listItem
        End Function

        ''' <summary>
        ''' Gets the list item at the given 1-based index.
        ''' </summary>
        ''' <param name="listItems">The list items.</param>
        ''' <param name="itemIndex">Index of the item (1-based).</param>
        ''' <returns>The requested list item</returns>
        ''' <exception cref="ListItemOutOfRangeException">
        ''' Thrown if the requested index is outside of the range of the list items
        ''' </exception>
        Friend Function GetListItem(listItems As IReadOnlyCollection(Of IAutomationElement), itemIndex As Integer) _
            As IAutomationElement

            If listItems.Count < itemIndex
                Throw New ListItemOutOfRangeException(ParameterNames.Index)
            End If

            Return listItems.ElementAt(itemIndex - 1)
        End Function

        ''' <summary>
        ''' Gets the list item based on the given query.
        ''' </summary>
        ''' <param name="listItems">The list items.</param>
        ''' <param name="query">The query.</param>
        ''' <returns>The requested list item</returns>
        ''' <exception cref="ListItemOutOfRangeException">
        ''' Thrown if no list item can be found matching the given query
        ''' </exception>
        Friend Function GetListItem(listItems As IReadOnlyCollection(Of IAutomationElement), query As clsQuery) _
            As IAutomationElement

            Dim index = query.GetIntParam(ParameterNames.Index)

            Dim item As IAutomationElement

            If index > 0
                item = GetListItem(listItems, index)
            Else
                Dim name = query.GetParameter(ParameterNames.IDName)
                item = GetListItem(listItems, name)
            End If

            Return item
        End Function

        ''' <summary>
        ''' Gets the list's items.
        ''' </summary>
        ''' <param name="element">The list element.</param>
        ''' <returns>
        ''' A collection of the list's children which implement ISelectionItemPattern
        ''' </returns>
        Friend Function GetListItems(element As IAutomationElement) As IEnumerable(Of IAutomationElement)

            Return _
                element.
                    FindAll(TreeScope.Children).
                    Where(Function(x) x.PatternIsSupported(PatternType.SelectionItemPattern))

        End Function


        ''' <summary>
        ''' Gets the list's items.
        ''' </summary>
        ''' <param name="element">The list element.</param>
        ''' <param name="cacheRequest">The cache request.</param>
        ''' <returns>
        ''' A collection of the list's children which implement ISelectionItemPattern
        ''' </returns>
        Friend Function GetListItems(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As IEnumerable(Of IAutomationElement)

            Return _
                element.
                    FindAll(TreeScope.Children, cacheRequest).
                    Where(Function(x) x.PatternIsSupported(PatternType.SelectionItemPattern))

        End Function

    End Module

End Namespace
