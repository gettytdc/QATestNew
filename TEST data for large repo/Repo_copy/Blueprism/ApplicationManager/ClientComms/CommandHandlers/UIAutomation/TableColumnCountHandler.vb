Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Retrieves the number of columns in the table.
    ''' </summary>
    <CommandId("UIATableColumnCount")>
    Friend Class TableColumnCountHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim gridPattern = element.EnsurePattern(Of IGridPattern)
            Return Reply.Result(gridPattern.CurrentColumnCount)

        End Function
    End Class
End Namespace
