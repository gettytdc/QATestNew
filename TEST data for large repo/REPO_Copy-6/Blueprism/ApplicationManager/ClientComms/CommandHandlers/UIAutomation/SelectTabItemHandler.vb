Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <Category(Category.UIAutomation)>
    <Command("Selects the UIA Tab Item within the parent TabControl.")>
    <Parameters("Those required to uniquely identify the element.")>
    <CommandId("UIASelectTabItem")>
    Friend Class SelectTabItemHandler : Inherits UIAutomationHandlerBase
        ''' <summary>
        ''' Initializes a new instance of the <see cref="SelectTabItemHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for 
        ''' the current application</param>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, 
                                             context As CommandContext) As Reply

            element.EnsurePattern(Of ISelectionItemPattern).Select()
            Return Reply.Ok

        End Function

    End Class

End Namespace