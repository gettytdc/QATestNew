Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Scrolls the grid to the selected item within the grid.
    ''' </summary>
    <CommandId("UIATableScrollIntoView")>
    Friend Class TableScrollIntoViewHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp, 
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim cell = GridHelper.EnsureCell(element, context.Query)
            Dim pattern = cell.EnsurePattern(Of IScrollItemPattern)
            pattern.ScrollIntoView()
            Return Reply.Ok

        End Function
    End Class
End Namespace
