Imports System.Drawing
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Clicks the mouse in an UIA element.
    ''' Required parameters: Those required to uniquely identify the element, plus 'TargX' and 'TargY',
    ''' plus optionally (defaulting to left) 'newtext' which specifies the button, 'left' or 'right'.
    ''' </summary>
    <CommandId("UIAMouseClick")>
    Friend Class MouseClickHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The mouse operations provider
        ''' </summary>
        Private ReadOnly mMouseOperationsProvider As IMouseOperationsProvider

        ''' <summary>
        ''' Initializes a new instance of the <see cref="MouseClickHandler"/> class.
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
            Dim elementBounds = element.CurrentBoundingRectangle

            Dim mouseButton =
                context.Query.GetParameter(ParameterNames.NewText).
                    Map(AddressOf mMouseOperationsProvider.ParseMouseButton)

            Dim x As Integer = context.Query.GetIntParam(ParameterNames.TargX, False)
            Dim y As Integer = context.Query.GetIntParam(ParameterNames.TargY, False)

            Dim clickPoint = Point.Add(elementBounds.Location, New Size(x, y))

            mMouseOperationsProvider.ClickAt(clickPoint.X, clickPoint.Y, False, mouseButton)

            Return Reply.Ok
        End Function
    End Class
End Namespace
