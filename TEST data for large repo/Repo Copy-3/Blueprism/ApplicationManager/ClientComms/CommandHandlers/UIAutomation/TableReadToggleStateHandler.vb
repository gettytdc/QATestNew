Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Reads the current toggle state of the UIA cell element.
    ''' </summary>
    <CommandId("UIATableReadToggleState")>
    Friend Class TableReadToggleStateHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim pattern = GridHelper.EnsureCellElementPattern(Of ITogglePattern)(
                                                              element,
                                                              context.Query,
                                                              PatternType.TogglePattern)

            Dim state = pattern.CurrentToggleState
            Dim result = If(state = ToggleState.On, Reply.True, Reply.False)
            Return result
        End Function
    End Class
End Namespace
