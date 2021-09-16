Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Checks/unchecks a UIA radio button according to the flag passed in as the new value
    ''' Required parameters: Those required to uniquely identify the element
    ''' </summary>
    <CommandId("UIARadioSetChecked")>
    Friend Class RadioSetCheckedHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RadioSetCheckedHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim pattern = element.EnsurePattern(Of ISelectionItemPattern)()

            Dim shouldCheck = context.Query.GetBoolParam(ParameterNames.NewText)

            Dim isChecked = CBool(element.GetCurrentPropertyValue(PropertyType.SelectionItemIsSelected))

            If shouldCheck Then
                If Not isChecked Then pattern.Select()
            Else
                If isChecked Then pattern.RemoveFromSelection()
            End If

            Return Reply.Ok


        End Function


    End Class
End Namespace
