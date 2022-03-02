
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebGetValue")>
    Friend Class GetValueHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply
            Dim result = element.GetText()
            If String.IsNullOrEmpty(result)
                result = element.GetValue()
            End If

            Return Reply.Result(result)
        End Function

    End Class
End Namespace