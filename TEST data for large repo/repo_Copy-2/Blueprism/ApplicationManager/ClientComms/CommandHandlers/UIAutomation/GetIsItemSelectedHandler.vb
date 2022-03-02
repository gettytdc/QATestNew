Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Checks to see if the item is currently selected. Required parameters: Those 
    ''' required to uniquely identify the element
    ''' </summary>
    <CommandId("UIAGetIsItemSelected")>
    <Category(Category.UIAutomation)>
    <Command("Checks to see if the UI Automation element is selected.")>
    <Parameters("Those required to uniquely identify the element.")>
    Friend Class GetIsItemSelectedHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetIsItemSelectedHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper 
        ''' for the current application</param>

        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                          context As CommandContext) As Reply
            Dim isSelected =
            element.EnsurePattern(Of ISelectionItemPattern)().CurrentIsSelected

            Return CType(IIf(isSelected, Reply.True, Reply.False), Reply)

        End Function

    End Class
End Namespace