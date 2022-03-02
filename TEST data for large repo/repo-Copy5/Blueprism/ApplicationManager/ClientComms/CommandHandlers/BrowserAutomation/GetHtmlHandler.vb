Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    '''<summary>
    ''' Gets the outer HTML of the UI Automation element.
    ''' Required parameters: Those required to uniquely identify the element.
    ''' Result: "RESULT:&lt;HTML&gt;" where &lt;HTML&gt; is the HTML of the element.
    ''' </summary>
    <CommandId("WebGetHtml")>
    Friend Class GetHtmlHandler : Inherits BrowserAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetHtmlHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply
            Dim result = element.GetHtml()
            Return Reply.Result(result)
        End Function

    End Class
End Namespace
