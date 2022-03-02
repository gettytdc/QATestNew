Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Reads the current state of whether the element is expanded or collapsed.
    ''' Required parameters: Those required to uniquely identify the element
    ''' Result: "RESULT:&lt;value&gt;" where &lt;value&gt; a value indicating whether the element is expanded.
    ''' </summary>
    <CommandId("UIAGetExpanded")>
    Friend Class GetExpandedHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetExpandedHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(
            application As ILocalTargetApp,
            identifierHelper As IUIAutomationIdentifierHelper)

            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim expandCollapsePattern = element.GetCurrentPattern(Of IExpandCollapsePattern)()
            Dim result As Boolean

            If expandCollapsePattern Is Nothing
                result = HasVisibleChildOfType(element, ControlType.List, ControlType.Menu)
            Else
                result =
                    expandCollapsePattern.CurrentExpandCollapseState <> ExpandCollapseState.Collapsed
            End If

            Return If(result, Reply.True, Reply.False)

        End Function

        Private Shared Function HasVisibleChildOfType(
            element As IAutomationElement,
            ParamArray controlType As ControlType()) _
            As Boolean

            Return _
                controlType.
                    SelectMany(Function(x) _
                        element.
                           FindAll(TreeScope.Children, x).
                           Where(AddressOf ElementIsOnScreen)).
                    Any()

        End Function

        Private Shared Function ElementIsOnScreen(element As IAutomationElement) As Boolean
            Return Not element.CurrentIsOffscreen
        End Function
    End Class
End Namespace
