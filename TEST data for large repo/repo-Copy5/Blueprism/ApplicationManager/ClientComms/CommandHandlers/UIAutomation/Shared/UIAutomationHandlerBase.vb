Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation.Shared

   ''' <summary>
    ''' Base class with common functionality for UIAutomation handlers
    ''' </summary>
    Friend MustInherit Class UIAutomationHandlerBase
        Inherits CommandHandlerBase

        ''' <summary>
        ''' Creates a new instance of this handler
        ''' </summary>
        ''' <param name="application">The current application</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Protected Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application)
            Me.IdentifierHelper = identifierHelper
        End Sub

        ''' <summary>
        ''' IUIAutomationIdentifierHelper instance for the current application
        ''' </summary>
        Protected ReadOnly Property IdentifierHelper As IUIAutomationIdentifierHelper

       ''' <summary>
        ''' Finds element and calls a separate Execute function with the element
        ''' that must be implemented by subclasses
        ''' </summary>
        ''' <param name="context">Details about the application and query to execute</param>
        ''' <returns>The result of calling the Execute function with the element</returns>
        Public Overrides NotOverridable Function Execute(context As CommandContext) As Reply
            
           Dim element = IdentifierHelper.FindUIAutomationElement(
                context.Query, Application.PID)
            Return Execute(element, context)

        End Function

        ''' <summary>
        ''' Executes the command with the specified element
        ''' </summary>
        ''' <param name="element">The target IAutomationElement</param>
        ''' <param name="context">Details about the application and query to execute</param>
        ''' <returns></returns>
        Protected Overloads MustOverride Function Execute(element As IAutomationElement, context As CommandContext) As Reply

   End Class

End NameSpace