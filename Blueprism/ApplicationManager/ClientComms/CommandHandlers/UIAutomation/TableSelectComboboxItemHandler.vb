Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Selects an item in a combobox within a UIA cell element.
    ''' </summary>
    <CommandId("UIATableSelectComboboxItem")>
    Friend Class TableSelectComboboxItemHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <inheritdoc/>
        Public Sub New(
                      application As ILocalTargetApp,
                      identifierHelper As IUIAutomationIdentifierHelper,
                      automationFactory As IAutomationFactory)
            MyBase.New(application, identifierHelper)

            mAutomationFactory = automationFactory
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim cellChildElement = GridHelper.EnsureCellElement(element, context.Query)

            Dim expandPattern = GridHelper.EnsureCellElementPattern(Of IExpandCollapsePattern)(
                                                                    element,
                                                                    context.Query,
                                                                    PatternType.ExpandCollapsePattern)

            Const MinItemIndex = 1

            Dim index = context.Query.GetIntParam(ParameterNames.ItemIndex)
            Dim text = context.Query.GetParameter(ParameterNames.ItemText)

            If index < MinItemIndex AndAlso text Is Nothing Then
                ' We have nothing to search with
                Return Reply.False
            End If
            expandPattern.ExpandCollapse()
            Try
                Dim cboItems = GetComboItems(cellChildElement, mAutomationFactory, True).ToList()

                Dim item As IAutomationElement = Nothing

                ' Search using text first (if we have it)
                If text IsNot Nothing Then
                    item = cboItems.Find(Function(t) t.CurrentName = text)
                    If item Is Nothing Then Throw New ListItemOutOfRangeException(
                        ParameterNames.ItemText)
                Else
                    ' The index parameter is 1-based so adjust for the collection 
                    If index - 1 > cboItems.Count Then
                        Throw New ListItemOutOfRangeException(ParameterNames.ItemIndex)
                    End If
                    item = cboItems(index - 1)
                End If

                item.EnsurePattern(Of ISelectionItemPattern).Select()

                Return Reply.Ok
            Finally
                If expandPattern.CurrentExpandCollapseState = ExpandCollapseState.Expanded Then
                    expandPattern.Collapse()
                End If
            End Try
            
        End Function


    End Class
End Namespace