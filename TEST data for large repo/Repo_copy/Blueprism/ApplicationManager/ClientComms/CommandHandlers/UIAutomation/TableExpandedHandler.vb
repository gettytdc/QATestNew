Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Gets whether the UIA cell element is expanded or not.
    ''' </summary>
    <CommandId("UIATableExpanded")>
    Friend Class TableExpandedHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp, 
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim pattern = GridHelper.EnsureCellElementPattern(Of IExpandCollapsePattern)(
                                                              element,
                                                              context.Query,
                                                              PatternType.ExpandCollapsePattern)

            Dim state = pattern.CurrentExpandCollapseState
            Return If(state = ExpandCollapseState.Expanded, Reply.True, Reply.False)

        End Function
    End Class
End Namespace
