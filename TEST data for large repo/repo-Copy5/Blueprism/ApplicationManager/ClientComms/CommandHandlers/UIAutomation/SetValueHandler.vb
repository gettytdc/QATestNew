Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <Category(Category.UIAutomation)>
    <Command("Sets the value of the UI Automation element.")>
    <Parameters("Those required to uniquely identify the element, plus 'NewText'")>
    <CommandId("UIASetValue")>
    Friend Class SetValueHandler : Inherits UIAutomationHandlerBase

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

            Dim value = context.Query.GetParameter(ParameterNames.NewText)
            Dim pattern = element.EnsurePattern(Of IValuePattern)()
            pattern.SetValue(value)
            Return Reply.Ok

        End Function


    End Class

End Namespace