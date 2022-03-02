Imports System.Linq

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Sets the selected item in the combo box
    ''' Required parameters: Those required to uniquely identify the combo box
    ''' </summary>
    <CommandId("UIAComboSelect")>
    Friend Class ComboSelectHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ComboSelectHandler" /> class.
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

            Dim items = GetComboItems(element, mAutomationFactory, False).ToList()
            Dim item = GetListItem(items, context.Query)

            item.
                GetCurrentPattern(Of ISelectionItemPattern)()?.
                Select()

            Try
                item.
                    GetCurrentPattern(Of IInvokePattern)()?.
                    Invoke()
            Catch
            End Try

            element.
                GetCurrentPattern(Of IValuePattern)()?.
                SetValue(item.CurrentName)

            element.SetFocus()

            Return Reply.Ok
        End Function

    End Class
End Namespace
