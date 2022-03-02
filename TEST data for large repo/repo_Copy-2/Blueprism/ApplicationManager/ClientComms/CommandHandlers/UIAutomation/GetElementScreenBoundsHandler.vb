Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets the screen relative bounds of a UIA element.
    ''' Required parameters: Those required to uniquely identify the window.
    ''' Result: "RESULT:&lt;xml&gt;" where &lt;xml&gt; is the collections xml representing the windows screen bounds."
    ''' </summary>
    <CommandId("UIAGetElementScreenBounds")>
    Friend Class GetElementScreenBoundsHandler : Inherits [Shared].UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetElementScreenBoundsHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply
            Return _
                element.CurrentBoundingRectangle.
                    AsCollectionXML().
                    Map(AddressOf Reply.Result)
        End Function
    End Class
End Namespace
