Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Expands or collapses the UIA cell element.
    ''' </summary>
    <CommandId("UIATableExpandCollapse")>
    Friend Class TableExpandCollapseHandler : Inherits UIAutomationHandlerBase

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

            pattern.ExpandCollapse()
            Return Reply.Ok

        End Function
    End Class
End Namespace