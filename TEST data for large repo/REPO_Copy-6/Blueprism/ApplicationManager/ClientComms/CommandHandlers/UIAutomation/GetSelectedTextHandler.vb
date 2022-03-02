Imports System.Linq

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <Category(Category.UIAutomation)>
    <Command("Gets the selected text in the element")>
    <Parameters("Those required to uniquely identify the element")>
    <Response("""RESULT:<value>"" where <value> is the selected text of the element.")>
    <CommandId("UIAGetSelectedText")>
    Friend Class GetSelectedTextHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetSelectedTextHandler"/> class.
        ''' </summary>
        ''' <param name="application">The current application</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim pattern = element.EnsurePattern(Of ITextPattern)()

            Dim text =
                pattern.GetSelection().
                    Select(Function(x) x.GetText(Integer.MaxValue)).
                    Map(AddressOf String.Concat)

            Return Reply.Result(text)

        End Function
    End Class
End Namespace