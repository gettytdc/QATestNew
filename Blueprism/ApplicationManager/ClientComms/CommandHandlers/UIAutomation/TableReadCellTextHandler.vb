Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Reads the text from a cell in a table, defaults to the first
    ''' row or column index if one is not provided.
    ''' </summary>
    <CommandId("UIATableReadCellText")>
    Friend Class TableReadCellTextHandler : Inherits UIAutomationHandlerBase

        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper,
                       automationFactory As IAutomationFactory)
            MyBase.New(application, identifierHelper)
            mAutomationFactory = automationFactory
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim cellChildElement = GridHelper.EnsureCellElement(element, context.Query)

            Dim text = GridHelper.GetInnerElementsText(cellChildElement, mAutomationFactory)
            Return Reply.Result(text)

        End Function
    End Class
End Namespace

