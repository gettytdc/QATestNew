Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.ApplicationManager.Exceptions
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Sets this checkbox to be the only one checked
    ''' Required parameters: Those required to uniquely identify the element
    ''' </summary>
    <CommandId("UIACheckOnly")>
    Friend Class CheckOnlyHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CheckOnlyHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
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

            Dim parent = mAutomationFactory.GetParentElement(element)
            Dim togglePattern = element.EnsurePattern(Of ITogglePattern)()

            Dim children = parent.FindAll(TreeScope.Children, mAutomationFactory.CreatePropertyCondition(PropertyType.ControlType, ControlType.CheckBox))

            For Each child In children
                child.GetCurrentPattern(Of ITogglePattern)()?.SetToggle(ToggleState.Off)
            Next

            togglePattern.SetToggle(ToggleState.On)

            Return Reply.Ok
        End Function
    End Class
End Namespace
