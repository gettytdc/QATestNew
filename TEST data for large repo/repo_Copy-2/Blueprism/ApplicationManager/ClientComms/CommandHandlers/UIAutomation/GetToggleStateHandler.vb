Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets the toggle state of a UI Automation Element
    ''' Required parameters: Those required to uniquely identify the element
    ''' Result: """RESULT:&lt;value&gt;"" where &lt;value&gt; is the value of the element."
    ''' </summary>
    <CommandId("UIAGetToggleState")>
    Friend Class GetToggleStateHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetToggleStateHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim pattern = element.EnsurePattern(Of ITogglePattern)
            Dim state = pattern.CurrentToggleState
            Return If(state = ToggleState.On, Reply.True, Reply.False)

        End Function
    End Class
End Namespace
