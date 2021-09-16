Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Clear the selected rows collection of a table.
    ''' </summary>
    <CommandId("UIATableClearSelection")>
    Friend Class TableClearSelectionHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim rows = GridHelper.GetAllSelectableRowElements(element)

            For Each r In rows
                Dim pattern = r.GetCurrentPattern(Of SelectionItemPattern)
                pattern.RemoveFromSelection()
            Next

            Return Reply.True
        End Function

    End Class
End Namespace