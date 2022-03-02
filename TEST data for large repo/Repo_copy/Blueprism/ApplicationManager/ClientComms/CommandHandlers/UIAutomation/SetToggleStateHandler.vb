Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Sets the toggle state of a UI Automation Element
    ''' Required parameters: Those required to uniquely identify the element, plus a NewText parameter
    ''' containing the state value to set
    ''' </summary>
    <CommandId("UIASetToggleState")>
    Friend Class SetToggleStateHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SetToggleStateHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim pattern = element.EnsurePattern(Of ITogglePattern)
            Dim state = If(context.Query.GetBoolParam(ParameterNames.NewText), ToggleState.On, ToggleState.Off)
            ToggleHelper.SetState(pattern, state)
            Return Reply.Ok

        End Function
    End Class
End Namespace
