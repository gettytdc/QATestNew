Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Operations
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Finishes a drag and drop by dropping at point.
    ''' Required parameters: Those required to uniquely identify the window, plus 'TargX' and 'TargY' to define point at which to drop.
    ''' </summary>
    <CommandId("UIADrop")>
    Friend Class DropHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The mouse operations provider
        ''' </summary>
        Private ReadOnly mMouseOperationsProvider As IMouseOperationsProvider

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DropHandler" /> class.
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
            Dim bounds = element.CurrentBoundingRectangle

            Dim x As Integer = context.Query.GetIntParam(ParameterNames.TargX, False)
            Dim y As Integer = context.Query.GetIntParam(ParameterNames.TargY, False)
            Dim absoluteX As Integer = bounds.Left + x
            Dim absoluteY As Integer = bounds.Top + y

            mMouseOperationsProvider.DropAt(absoluteX, absoluteY)

            Return Reply.Ok
        End Function
    End Class
End Namespace
