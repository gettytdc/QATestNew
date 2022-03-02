Imports System.Collections.Generic
Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared

    ''' <summary>
    ''' Provides helper methods for interacting with tree controls
    ''' </summary>
    Public Module TreeViewHelper

        ''' <summary>
        ''' Gets all tree items below the given node, including the node itself.
        ''' </summary>
        ''' <param name="rootNode">The root node.</param>
        ''' <returns>A collection of tree items</returns>
        Public Function GetTreeItems(rootNode As IAutomationElement) As IEnumerable(Of IAutomationElement)
            Dim test = rootNode.FindAll(TreeScope.Subtree)
            Return _
                rootNode.
                    FindAll(TreeScope.Subtree).
                    Where(Function(x) x.PatternIsSupported(PatternType.SelectionItemPattern))

        End Function

        ''' <summary>
        ''' Finds the requested node in the tree.
        ''' </summary>
        ''' <param name="element">The root element to start the search at.</param>
        ''' <param name="query">The query identifying the node to find.</param>
        ''' <returns>
        ''' The requested node.
        ''' </returns>
        ''' <exception cref="NoSuchUIAutomationElementException">
        ''' Thrown if the requested node cannot be found
        ''' </exception>
        Public Function FindNode(element As IAutomationElement, query As clsQuery) As IAutomationElement

            Dim targetName = query.GetParameter(clsQuery.ParameterNames.IDName)
            Dim elementPosition = query.GetIntParam(clsQuery.ParameterNames.Position)

            Dim result = GetTreeItems(element).Map(
                Function(elements)

                    ' Find the element by Name unless the Name is empty then search by number which is 1 based.
                    Return If(String.IsNullOrEmpty(targetName),
                              elements.ElementAtOrDefault(elementPosition - 1),
                              elements.Where(Function(e) e.CurrentName = targetName
                              ).FirstOrDefault)
                End Function)

            If result Is Nothing Then
                Throw New NoSuchUIAutomationElementException()
            End If

            Return result

        End Function

        ''' <summary>
        ''' Tries to expand the node.
        ''' </summary>
        ''' <param name="element">The element to try to expand.</param>
        Private Sub TryExpandNode(element As IAutomationElement)
            Try
                element.GetCurrentPattern(Of IExpandCollapsePattern)()?.Expand()
            Catch ex As Exception
            End Try
        End Sub

    End Module

End Namespace