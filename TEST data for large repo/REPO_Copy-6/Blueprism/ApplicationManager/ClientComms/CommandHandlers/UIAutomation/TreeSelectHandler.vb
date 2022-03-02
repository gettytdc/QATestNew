Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <Category(clsLocalTargetApp.Category.UIAutomation)>
    <Command("Selects an item in a tree.")>
    <Parameters("Those required to uniquely identify the element, plus 'IDName' identifying the item to select")>
    <CommandId("UIATreeSelect")>
    Friend Class TreeSelectHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SetValueHandler"/> class.
        ''' </summary>
        ''' <param name="application">The current application</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            FindNode(element, context.Query).
                Map(Function(x) x.EnsurePattern(Of ISelectionItemPattern)).
                Select()

            Return Reply.Ok

        End Function

    End Class
End Namespace