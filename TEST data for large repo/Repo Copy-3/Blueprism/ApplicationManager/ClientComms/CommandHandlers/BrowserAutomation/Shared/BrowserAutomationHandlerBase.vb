Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation.Shared

    Friend MustInherit Class BrowserAutomationHandlerBase
        Inherits CommandHandlerBase

        Protected ReadOnly Property IdentifierHelper As IBrowserAutomationIdentifierHelper

        Protected Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application)
            Me.IdentifierHelper = identifierHelper
        End Sub

        Public Overrides NotOverridable Function Execute(context As CommandContext) As Reply
            Dim element = IdentifierHelper.FindSingleElement(context.Query)
            Return Execute(element, context)
        End Function

        ''' <summary>
        ''' Executes the command with the specified element
        ''' </summary>
        ''' <param name="element">The target IAutomationElement</param>
        ''' <param name="context">Details about the application and query to execute</param>
        ''' <returns></returns>
        Protected Overloads MustOverride Function Execute(element As IWebElement, context As CommandContext) As Reply

    End Class

End Namespace