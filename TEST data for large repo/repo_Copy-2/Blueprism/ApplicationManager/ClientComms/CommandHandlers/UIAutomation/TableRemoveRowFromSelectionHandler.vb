Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Remove the specified row from the selected rows collection of a table.
    ''' </summary>
    <CommandId("UIATableRemoveRowFromSelection")>
    Friend Class TableRemoveRowFromSelectionHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply
            Dim rowNumber = context.Query.GetIntParam(ParameterNames.RowNumber)

            Dim rows = GridHelper.GetAllSelectableRowElements(element)

            Dim pattern = rows(rowNumber - 1).GetCurrentPattern(Of SelectionItemPattern)
            pattern.RemoveFromSelection()

            Return Reply.True
        End Function

    End Class
End Namespace