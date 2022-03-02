Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets the whether a radio button is checked
    ''' Required parameters: Those required to uniquely identify the radio button
    ''' Result: """RESULT:&lt;value&gt;"" where &lt;value&gt; is whether the radio button is checked."
    ''' </summary>
    <CommandId("UIAGetRadioCheckedState")>
    Friend Class GetRadioCheckedStateHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetRadioCheckedStateHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim pattern = element.EnsurePattern(Of ISelectionItemPattern)()
            Dim isSelected = pattern.CurrentIsSelected
            Return Reply.Result(isSelected.ToString())

        End Function

    End Class
End Namespace
