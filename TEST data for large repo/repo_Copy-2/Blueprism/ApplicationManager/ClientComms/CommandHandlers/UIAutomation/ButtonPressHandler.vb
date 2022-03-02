Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Presses a button element
    ''' Required parameters: Those required to uniquely identify the element
    ''' </summary>
    <CommandId("UIAButtonPress")>
    Friend Class ButtonPressHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ButtonPressHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            If element.PatternIsSupported(PatternType.InvokePattern) Then
                Dim pattern = CType(element.GetCurrentPattern(PatternType.InvokePattern), IInvokePattern)
                pattern.Invoke()
            ElseIf element.PatternIsSupported(PatternType.TogglePattern) Then
                Dim pattern = CType(element.GetCurrentPattern(PatternType.TogglePattern), ITogglePattern)
                pattern.Toggle()
            ElseIf element.PatternIsSupported(PatternType.ExpandCollapsePattern) Then
                Dim pattern = CType(element.GetCurrentPattern(PatternType.ExpandCollapsePattern), IExpandCollapsePattern)
                pattern.ExpandCollapse()
            Else
                Throw New PatternNotFoundException(Of IInvokePattern)()
            End If

            Return Reply.Ok
        End Function
    End Class
End Namespace
