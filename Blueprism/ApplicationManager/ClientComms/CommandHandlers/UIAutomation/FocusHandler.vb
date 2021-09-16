Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gives focus to the UI Automation element.
    ''' Required parameters: Identifiers to uniquely identify the element.
    ''' </summary>
    <CommandId("UIAFocus")>
    Friend Class FocusHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The UI Automation helper
        ''' </summary>
        Private ReadOnly mAutomationHelper As IAutomationHelper

        ''' <summary>
        ''' The window operations provider
        ''' </summary>
        Private ReadOnly mWindowOperationsProvider As IWindowOperationsProvider

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FocusHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        ''' <param name="automationHelper">The UI Automation helper.</param>
        ''' <param name="windowOperationsProvider">The window operations provider.</param>
        Public Sub New(
                application As ILocalTargetApp,
                identifierHelper As IUIAutomationIdentifierHelper,
                automationHelper As IAutomationHelper,
                windowOperationsProvider As IWindowOperationsProvider)
            MyBase.New(application, identifierHelper)

            mAutomationHelper = automationHelper
            mWindowOperationsProvider = windowOperationsProvider
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply
            Dim window = mAutomationHelper.GetWindowHandle(element)

            mWindowOperationsProvider.ForceForeground(window)

            element.SetFocus()

            Return Reply.Ok
        End Function
    End Class
End Namespace
