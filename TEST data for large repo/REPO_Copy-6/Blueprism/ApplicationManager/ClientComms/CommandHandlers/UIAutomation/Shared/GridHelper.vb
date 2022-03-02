Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns
Imports System.Linq
Imports System.Collections.Generic

Namespace CommandHandlers.UIAutomation.Shared

    ''' <summary>
    ''' Helper functionality for working with UIAutomation grid elements
    ''' </summary>
    Public Module GridHelper

        Private Const EmptyElementNumber = 0
            
        ''' <summary>
        ''' Method to get text from the cell element as well as the child component 
        ''' elements of the cell, such as a user controls.
        ''' </summary>
        ''' <param name="element"></param>
        ''' <param name="factory"></param>
        ''' <returns>A concatenated string of the values within the cell. </returns>
        Public Function GetInnerElementsText(element As IAutomationElement,
                                             factory As IAutomationFactory) As String

            Dim cellTextProvider = factory.GetGridTextProvider()
            Dim builder As New StringBuilder

            Dim parentText = cellTextProvider.GetTextFromElement(element)
            builder.Append($"{parentText} ")

            Dim elements = element.FindAll(TreeScope.Children)
            For Each innerElement In elements

                Dim childText = cellTextProvider.GetTextFromElement(innerElement)

                ' Some cells will have child elements that have the same text as the parent.
                If Not childText = parentText Then
                    builder.Append($"{childText} ")
                End If
            Next

            Return builder.ToString().TrimEnd()
        End Function


        ''' <summary>
        ''' If no element number is specified then the subtree of the cell element is searched for the first 
        ''' element that supports the required pattern.        
        ''' </summary>
        ''' <typeparam name="TPattern">Pattern required from the element. </typeparam>
        ''' <param name="element">The top level element with children that is a cell. </param>
        ''' <param name="query">The query containing the parameters to find the element. </param>
        ''' <param name="patternType">Pattern type enumeration. </param>
        ''' <returns>A UIA pattern of type TPattern. </returns>
        ''' <exception cref="NoSuchElementException"></exception>
        ''' <exception cref="PatternNotFoundException(Of TPattern)"></exception>
        Public Function EnsureCellElementPattern(Of TPattern As IAutomationPattern) _
                                                (element As IAutomationElement,
                                                 query As clsQuery,
                                                 patternType As PatternType) _
                                                As TPattern

            Dim childElement = EnsureCellElement(element, query)
            Dim cellElementNumber = GetElementNumberFromQuery(query)

            If cellElementNumber = EmptyElementNumber Then
                ' No element number specified - use first element in subtree that
                ' supports pattern
                Return childElement.EnsurePatternFromSubtree(Of TPattern)(patternType)
            Else
                ' The element must implement the pattern
                Return childElement.EnsurePattern(Of TPattern)
            End If

        End Function

        ''' <summary>
        '''Retrieves a cell element within a grid element at the coordinates 
        '''specified within the query. If the 
        '''ElementNumber parameter Is given within the query, Then this Is used To identify a descendant element 
        '''within the cell, which Is returned instead Of the cell. An exception Is thrown If the cell Or 
        '''descendant element specified In the query does Not exist. Used To enable more concise handler code.
        ''' </summary>
        ''' <param name="element">The top level element with children that is a cell. </param>
        ''' <param name="query">The query containing the parameters to find the element</param>
        ''' <returns>The child element</returns>
        ''' <exception cref="NoSuchElementException"></exception>
        Public Function EnsureCellElement(element As IAutomationElement,
                                          query As clsQuery) As IAutomationElement

            Dim cell = EnsureCell(element, query)

            Dim cellElementNumber = GetElementNumberFromQuery(query)

            If cellElementNumber = EmptyElementNumber Then
                Return cell
            End If

            Return GetChildElementFromElement(cell, cellElementNumber)

        End Function

        Private Function GetElementNumberFromQuery(query As clsQuery) As Integer
            Dim elementNumber = (query.Parameters(clsQuery.ParameterNames.ElementNumber))

            If String.IsNullOrEmpty(elementNumber) Then
                Return EmptyElementNumber
            End If

            Return CInt(elementNumber)
        End Function

        ''' <summary>
        ''' Retrieves a child element from the element provided, if none is found at the element
        ''' number then an exception is thrown.
        ''' </summary>
        ''' <param name="element">The top level element with children. </param>
        ''' <param name="elementNumber">The number of the occurring element. </param>
        ''' <returns>A UI component from within the element. </returns>
        ''' <exception cref="NoSuchElementException"></exception>
        Private Function GetChildElementFromElement(element As IAutomationElement,
                                                   elementNumber As Integer) As IAutomationElement

            ' Skip is 0 based
            Dim cellComponentElement = element.FindAll(TreeScope.Descendants) _
                                              .ElementAtOrDefault(elementNumber - 1)

            If cellComponentElement Is Nothing Then
                Dim message = UIAutomationErrorResources.NoSuchChildFoundException_CellChildNotFound
                Throw New NoSuchElementException(message)
            End If

            Return cellComponentElement
        End Function

        ''' <summary>
        ''' Retrieves a cell element within a grid element at the coordinates 
        ''' specified within the query
        ''' </summary>
        ''' <param name="query">The query containing the parameters to find the element</param>
        ''' <returns>The child element</returns>
        Public Function GetCell(element As IAutomationElement,
                                           query As clsQuery) As IAutomationElement

            Dim coordinates = GetGridCoordinates(query)
            Dim rowIndex = coordinates.Row
            Dim columnIndex = coordinates.Column
            Dim cell = element.FromPattern(
                Function(pattern As IGridPattern)
                    Return pattern.GetItem(rowIndex, columnIndex)
                End Function)

            Return cell

        End Function

        ''' <summary>
        ''' Retrieves a cell element within a grid element at the coordinates 
        ''' specified within the query, throwing an exception if the cell 
        ''' does not exist. Used to enable more concise handler code.
        ''' </summary>
        ''' <param name="query">The query containing the parameters to find the element</param>
        ''' <returns>The child element</returns>
        ''' <exception cref="NoSuchElementException"></exception>
        Public Function EnsureCell(element As IAutomationElement,
                                   query As clsQuery) As IAutomationElement

            Dim cell = GetCell(element, query)
            If cell Is Nothing Then
                Dim message = UIAutomationErrorResources.NoSuchElementException_TableCellNotFoundMessage
                Throw New NoSuchElementException(message)
            End If
            Return cell

        End Function

        ''' <summary>
        ''' Gets a GridCoordinates object based on the ColumnNumber and RowNumber
        ''' parameters in a query. Note that the GridCoordinates class uses zero-based
        ''' column and row numbers
        ''' </summary>
        ''' <param name="query">The query containing the parameters</param>
        ''' <returns>A GridCoordinates containing the values</returns>
        Public Function GetGridCoordinates(query As clsQuery) As GridCoordinates
            Dim columnNumber = CInt(query.Parameters(clsQuery.ParameterNames.ColumnNumber))
            Dim rowNumber = CInt(query.Parameters(clsQuery.ParameterNames.RowNumber))
            Return New GridCoordinates(columnNumber - 1, rowNumber - 1)
        End Function

        ''' <summary>
        ''' Helper method to return all IAutomationElement representing the selectable 
        ''' rows from a grid element.
        ''' </summary>
        ''' <param name="element">The grid element from which to get the rows.</param>
        ''' <returns>IEnumerable of IAutomation elements representing the selectable 
        ''' rows from the grid.</returns>
        Public Function GetAllSelectableRowElements(element As IAutomationElement) _
            As IEnumerable(Of IAutomationElement)

            Dim grid = EnsurePattern(Of IGridPattern)(element)

            Dim result As New List(Of IAutomationElement)

            For i = 0 To grid.CurrentRowCount - 1
                result.Add(grid.GetItem(i, 0).GetCurrentParent)
            Next

            Return result.Where(Function(e) e.PatternIsSupported(PatternType.SelectionItemPattern))

        End Function

        ''' <summary>
        ''' Helper method to get all the elements which belong to the currently 
        ''' selected rows of a grid element.
        ''' </summary>
        ''' <param name="element">The grid element to get the selected rows from</param>
        ''' <returns>IEnumerable of IAutomation elements representing the selected 
        ''' rows in the grid</returns>
        Public Function GetSelectedRowElements(element As IAutomationElement) _
            As IEnumerable(Of IAutomationElement)

            Return GetAllSelectableRowElements(element).
                                             Where(Function(e)
                                                       Return If(e.GetCurrentPattern(Of ISelectionItemPattern)?.
                                                                 CurrentIsSelected, False)
                                                   End Function)

        End Function

    End Module
End Namespace
