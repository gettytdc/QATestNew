Imports System.Collections.Generic
Imports System.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared

    ''' <summary>
    ''' Provides helper methods for interacting with UI Automation combo boxes
    ''' </summary>
    Module ComboBoxHelper

        ''' <summary>
        ''' Expands/collapses the given combo box.
        ''' </summary>
        ''' <param name="element">The combo box to expand/collapse.</param>
        ''' <param name="automationFactory">The automation factory.</param>
        ''' <exception cref="PatternNotFoundException(Of IExpandCollapsePattern)">
        ''' Thrown when no pattern can be found to expand/collapse the combo box
        ''' </exception>
        Friend Sub ExpandCollapseCombo(element As IAutomationElement, automationFactory As IAutomationFactory)
            Dim expandCollapsePattern = element.GetCurrentPattern(Of IExpandCollapsePattern)()

            If expandCollapsePattern IsNot Nothing
                expandCollapsePattern.ExpandCollapse()
            Else
                Dim expandButton = element.FindFirst(
                    TreeScope.Children,
                    automationFactory.CreatePropertyCondition(
                        PropertyType.ControlType,
                        ControlType.Button))

                If expandButton Is Nothing
                    Throw New PatternNotFoundException(Of IExpandCollapsePattern)()
                End If

                expandButton.EnsurePattern(Of IInvokePattern)().
                    Invoke()
            End If
        End Sub

        ''' <summary>
        ''' Gets the combo box's items.
        ''' </summary>
        ''' <param name="element">The combo box element.</param>
        ''' <param name="automationFactory">The automation factory.</param>
        ''' <param name="attemptCollapse">
        ''' If <c>true</c> then attempt to collapse the combo box if it needs to be opened</param>
        ''' <returns>A collection of items retrieved from the combo box</returns>
        ''' <exception cref="BluePrismException">
        ''' Thrown if no list can be found inside of the given element
        ''' </exception>
        Friend Function GetComboItems(
            element As IAutomationElement,
            automationFactory As IAutomationFactory,
            attemptCollapse As Boolean) _
            As IReadOnlyCollection(Of IAutomationElement)

            Dim hasExpanded = False

            Try
                Dim cacheRequest = automationFactory.CreateCacheRequest()
                Dim list = GetChildList(element, cacheRequest)

                If list Is Nothing
                    ExpandCollapseCombo(element, automationFactory)

                    hasExpanded = True

                    list = GetChildList(element, cacheRequest)

                    If list Is Nothing

                        '64-bit processes can put the list on the desktop so we need to do
                        'a search for a list from root. Very little else should put lists
                        'on the desktop so this will hopefully be safe.
                        list = GetChildList(automationFactory.GetRootElement(), cacheRequest)

                        If list Is Nothing
                            Throw New BluePrismException(UIAutomationErrorResources.ComboGetAllItemsHandler_ListNotInComboBox)
                        End If
                    End If
                End If

                'Enumerate here whilst combo items are still visible. If combo items disappear before
                'enumeration then it can lead to inconsistent behaviour.
                Return GetListItems(list, cacheRequest).ToList()

            Finally
                element.SetFocus()
                If hasExpanded AndAlso attemptCollapse
                    ExpandCollapseCombo(element, automationFactory)
                End If
            End Try

        End Function

        Private Function GetChildList(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As IAutomationElement
            Return _
                element.FindAll(TreeScope.Subtree, cacheRequest).
                    FirstOrDefault(Function(x) x.PatternIsSupported(PatternType.SelectionPattern))

        End Function

    End Module

End Namespace
