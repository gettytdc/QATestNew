Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <Category(Category.UIAutomation)>
    <Command("Gets the selected item in a list of UI Automation elements")>
    <Parameters("Those required to uniquely identify the element")>
    <Response("""RESULT:<value>"" where <value> is the text of the selected item of the element.")>
    <CommandId("UIAGetSelectedItemText")>
    Friend Class GetSelectedItemTextHandler : Inherits UIAutomationHandlerBase
        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetSelectedItemTextHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                          context As CommandContext) As Reply

            Dim selectionPattern = element.GetCurrentPattern(Of ISelectionPattern)()

            If selectionPattern Is Nothing Then
                Dim selectionItemPattern = element.EnsurePattern(Of ISelectionItemPattern)

                selectionPattern = selectionItemPattern.CurrentSelectionContainer?.
                    GetCurrentPattern(Of ISelectionPattern)

                If selectionPattern Is Nothing Then
                    Throw New PatternNotFoundException(Of ISelectionPattern)()
                End If
            End If

            Dim selectedItem = selectionPattern.GetCurrentSelection().SingleOrDefault()

            Dim name = selectedItem?.CurrentName
            Return Reply.Result(If(name, String.Empty))

        End Function


    End Class
End Namespace