Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Toggles the expand/collapse state Of the UI Automation element.
    ''' Required parameters: Those required to uniquely identify the element.
    ''' </summary>
    <CommandId("UIAExpandCollapse")>
    Friend Class ExpandCollapseHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ExpandCollapseHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply
    
            Dim pattern = element.EnsurePattern(Of IExpandCollapsePattern)()
            pattern.ExpandCollapse()
            Return Reply.Ok

        End Function
    End Class
End Namespace
