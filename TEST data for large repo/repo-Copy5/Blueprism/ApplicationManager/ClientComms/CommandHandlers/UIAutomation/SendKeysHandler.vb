Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Activates the parent window of the UIA element, gives focus to the UIA element, and performs global send keys.
    ''' Required parameters: Those required to uniquely identify the window, plus 'newtext' (keys to send) and, optionally, 'interval' (seconds to wait between each key).
    ''' </summary>
    <CommandId("UIASendKeys")>
    Friend Class SendKeysHandler : Inherits UIAutomationHandlerBase

        Private ReadOnly mWindowOperationsProvider As IWindowOperationsProvider
        Private ReadOnly mKeyboardOperationsProvider As IKeyboardOperationsProvider

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SendKeysHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        ''' <param name="windowOperationsProvider">The IWindowOperationsProvider object used by the handler</param>
        ''' <param name="keyboardOperationsProvider">The IKeyOperationsProvider object used by the handler</param>
        Public Sub New(application As ILocalTargetApp, 
                       identifierHelper As IUIAutomationIdentifierHelper,
                       windowOperationsProvider As IWindowOperationsProvider, 
                       keyboardOperationsProvider As IKeyboardOperationsProvider)
            MyBase.New(application, identifierHelper)
            mWindowOperationsProvider = windowOperationsProvider
            mKeyboardOperationsProvider = keyboardOperationsProvider
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply
            
            mWindowOperationsProvider.ForceForeground(element.CurrentNativeWindowHandle)
            element.SetFocus()
            KeyHelper.SendKeysFromQuery(context.Query, mKeyboardOperationsProvider)
            Return Reply.Ok

        End Function
    End Class
End Namespace
