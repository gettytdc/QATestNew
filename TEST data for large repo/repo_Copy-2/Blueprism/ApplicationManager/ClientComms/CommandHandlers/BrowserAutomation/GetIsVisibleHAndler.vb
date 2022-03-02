Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation
Imports BluePrism.Utilities.Functional

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebGetIsVisible")>
    Friend Class GetIsVisibleHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply
            Return _
                element.GetIsVisible.
                Map(AddressOf Reply.Result)
        End Function
    End Class
End NameSpace