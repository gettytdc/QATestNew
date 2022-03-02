
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebSetValue")>
    Friend Class SetValueHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply

            Dim value = context.Query.GetParameter(clsQuery.ParameterNames.NewText)
            element.SetValue(value)

            Return Reply.Ok
        End Function

    End Class
End Namespace