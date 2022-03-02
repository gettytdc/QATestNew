Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets the name of the UI Automation element.
    ''' Required parameters: Those required to uniquely identify the element.
    ''' Result: "RESULT:&lt;name&gt;" where &lt;name&gt; is the name of the element.
    ''' </summary>
    <CommandId("UIAGetName")>
    Friend Class GetNameHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetNameHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Return _
                element.GetCurrentPropertyValue(PropertyType.Name).
                ToString().
                Map(AddressOf Reply.Result)
        End Function
    End Class
End Namespace
