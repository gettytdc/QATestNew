Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation
Imports BluePrism.Utilities.Functional
Imports System.Linq
Imports System.Collections.Generic

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebGetFormValues")>
    Friend Class GetFormValuesHandler : Inherits BrowserAutomationHandlerBase

        Private Shared formElements As New List(Of String) From {"input", "select", "textarea"}

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply



            Return element.
                GetDescendants().
                Where(Function(x) IncludeInputElement(x)).
                ValuesAsCollectionXml().
                Map(Function(x) Reply.Result(x))

        End Function

        Function IncludeInputElement(element As IWebElement) As Boolean
            If Not formElements.Contains(element.GetElementType().ToLower()) Then Return False

            Dim elementType = element.GetAttribute("type")
            If elementType Is Nothing Then Return True
            If elementType.ToLower() = "submit" OrElse elementType.ToLower() = "button" Then Return False
            Return True
        End Function
    End Class
End Namespace
