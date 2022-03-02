Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Server.Domain.Models
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebGetTableItem")>
    Friend Class TableGetItemHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply

            Dim columnNumber = context.Query.GetIntParam(clsQuery.ParameterNames.ColumnNumber)
            Dim rowNumber = context.Query.GetIntParam(clsQuery.ParameterNames.RowNumber)

            Dim elementColMax = element.GetColumnCount(0) + 1
            Dim elementRowMax = element.GetRowCount + 1

            If columnNumber > elementColMax OrElse columnNumber < 1 Then
                Throw _
                    New NoSuchElementException(
                        String.Format(My.Resources.TheSpecifiedColumnsDoNotExistColumnsAvailable0, elementColMax))
            End If

            If rowNumber > elementRowMax OrElse rowNumber < 1 Then
                Throw New NoSuchElementException(String.Format(My.Resources.TheSpecifiedRowsDoNotExistRowsAvailable0, elementRowMax))
            End If

            Return Reply.Result(element.GetTableItemText(rowNumber - 1, columnNumber - 1))
        End Function

    End Class
End Namespace
