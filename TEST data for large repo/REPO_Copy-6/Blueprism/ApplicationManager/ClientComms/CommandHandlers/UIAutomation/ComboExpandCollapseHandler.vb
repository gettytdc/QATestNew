Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    <CommandId("UIAComboExpandCollapse")>
    Friend Class ComboExpandCollapseHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ComboExpandCollapseHandler"/> class.
        ''' </summary>
        ''' <param name="application">The current application</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        ''' <param name="automationFactory">The automation factory.</param>
        Public Sub New(
                         application As ILocalTargetApp, 
                         identifierHelper As IUIAutomationIdentifierHelper,
                         automationFactory As IAutomationFactory)
            MyBase.New(application, identifierHelper)

            mAutomationFactory = automationFactory
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply
            
            ExpandCollapseCombo(element, mAutomationFactory)

            Return Reply.Ok

        End Function
    End Class

End Namespace