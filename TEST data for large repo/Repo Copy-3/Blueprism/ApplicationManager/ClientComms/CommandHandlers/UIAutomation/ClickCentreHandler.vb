Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Sends a mouse click to the centre point of the identified UIAutomation
    ''' element.
    ''' The <see cref="clsQuery.ParameterNames.NewText">newtext</see> parameter
    ''' indicates which mouse button to effect the click with - either "left" or
    ''' "right".
    ''' </summary>
    <CommandId("UIAClickCentre")>
    Friend Class ClickCentreHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The mouse operations provider
        ''' </summary>
        Private ReadOnly mMouseOperationsProvider As IMouseOperationsProvider

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ClickCentreHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        ''' <param name="mouseOperationsProvider">The mouse operations provider.</param>
        Public Sub New(
                      application As ILocalTargetApp,
                      identifierHelper As IUIAutomationIdentifierHelper,
                      mouseOperationsProvider As IMouseOperationsProvider)
            MyBase.New(application, identifierHelper)

            mMouseOperationsProvider = mouseOperationsProvider
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim button =
                context.Query.GetParameter(clsQuery.ParameterNames.NewText).
                Map(AddressOf mMouseOperationsProvider.ParseMouseButton)

            mMouseOperationsProvider.ClickAt(
                element.CurrentCentrePoint.X,
                element.CurrentCentrePoint.Y,
                False,
                button)

            Return Reply.Ok

        End Function

    End Class
End Namespace