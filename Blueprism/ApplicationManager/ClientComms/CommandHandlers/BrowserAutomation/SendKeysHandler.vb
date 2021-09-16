Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebSendKeys")>
    Friend Class SendKeysHandler : Inherits BrowserAutomationHandlerBase

        Private ReadOnly mKeyboardOperationsProvider As IKeyboardOperationsProvider

        Public Sub New(
                          application As ILocalTargetApp,
                          identifierHelper As IBrowserAutomationIdentifierHelper,
                          keyboardOperationsProvider As IKeyboardOperationsProvider)
            MyBase.New(application, identifierHelper)
            mKeyboardOperationsProvider = keyboardOperationsProvider
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply
            
            element.Focus()
            SendKeysFromQuery(context.Query, mKeyboardOperationsProvider)

            Return Reply.Ok

        End Function
    End Class
End NameSpace