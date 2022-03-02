Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Scrolls the grid vertically.
    ''' </summary>
    <CommandId("UIAScrollVertical")>
    Friend Class ScrollVerticalHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim scrollPattern = element.EnsurePattern(Of IScrollPattern)
            Dim scrollValue = ScrollHelper.GetScrollAmountFromQuery(context.Query)
            scrollPattern.Scroll(ScrollAmount.NoAmount, scrollValue)
            Return Reply.Ok

        End Function
    End Class
End Namespace
