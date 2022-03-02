Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets the selected item from the combobox within a table cell
    ''' Required parameters: Those required to uniquely identify the cell
    ''' </summary>
    <CommandId("UIATableGetSelectedComboboxItem")>
    Friend Class TableGetSelectedComboboxItemHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetAllItemsHandler" /> class.
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
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim cellChildElement = GridHelper.EnsureCellElement(element, context.Query)
            Dim selectionPattern = GridHelper.EnsureCellElementPattern(Of ISelectionPattern)(
                                                                       element,
                                                                       context.Query,
                                                                       PatternType.SelectionPattern)

            If selectionPattern Is Nothing Then
                Dim selectionItemPattern = cellChildElement.EnsurePattern(Of ISelectionItemPattern)

                selectionPattern = selectionItemPattern.CurrentSelectionContainer?.
                    GetCurrentPattern(Of ISelectionPattern)

                If selectionPattern Is Nothing Then
                    Throw New PatternNotFoundException(Of ISelectionPattern)()
                End If
            End If

            Dim selectedItem = selectionPattern.GetCurrentSelection().SingleOrDefault()

            Dim name = selectedItem?.CurrentName
            Return Reply.Result(If(name, String.Empty))

        End Function
    End Class
End Namespace
