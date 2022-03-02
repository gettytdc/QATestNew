﻿Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation
Imports BluePrism.Utilities.Functional

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebGetSelectedItemsText")>
    Friend Class GetSelectedItemsTextHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply
            
            Return _
                element.GetListItems().
                    Where(Function(x) x.IsSelected).
                    Select(Function(x) x.Text).
                    AsCollectionXml().
                    Map(AddressOf Reply.Result)

        End Function
    End Class
End NameSpace