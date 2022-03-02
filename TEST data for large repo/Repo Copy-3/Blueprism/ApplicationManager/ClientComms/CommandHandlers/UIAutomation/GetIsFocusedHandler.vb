Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Reads the current state of whether the element has keyboard focus.
    ''' Required parameters: Those required to uniquely identify the element
    ''' Result: "RESULT:&lt;value&gt;" where &lt;value&gt; a value indicating whether the element has keyboard focus.
    ''' </summary>
    <CommandId("UIAGetIsFocused")>
    Friend Class GetIsFocusedHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetIsFocusedHandler"/> class.
        ''' </summary>
        ''' <param name="application">The current application</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(
                         application As ILocalTargetApp, 
                         identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply
            Return _
                If(element.CurrentHasKeyboardFocus,
                    Reply.True,
                    Reply.False)
                
        End Function
    End Class
End Namespace
