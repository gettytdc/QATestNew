Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebSelectListItem")>
    Friend Class SelectListItemHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply
            
            Dim index = context.Query.GetIntParam(clsQuery.ParameterNames.ItemIndex)
            Dim name = context.Query.GetParameter(clsQuery.ParameterNames.ItemText)

            element.SelectListItem(index, name)

            Return Reply.Ok

        End Function
    End Class
End NameSpace