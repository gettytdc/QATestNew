Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Handler for the <c>UIAMenuItemPress</c> command.
    ''' This deals with a menu item, trying to either expand or collapse it, then
    ''' invoking it if it cannot be expanded/collapsed.
    ''' </summary>
    <CommandId("UIAMenuItemPress")>
    Friend Class MenuItemPressHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim expander = element.GetCurrentPattern(Of ExpandCollapsePattern)
            If expander IsNot Nothing Then
                expander.ExpandCollapse()

            Else
                Dim invoker = element.GetCurrentPattern(Of IInvokePattern)
                If invoker IsNot Nothing Then
                    invoker.Invoke()

                Else
                    Throw New MissingPatternException(
                     PatternType.ExpandCollapsePattern, PatternType.InvokePattern)

                End If
            End If

            Return Reply.Ok

        End Function

    End Class

End Namespace
