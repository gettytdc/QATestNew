Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets all items in the combo box
    ''' Required parameters: Those required to uniquely identify the combo box
    ''' </summary>
    <CommandId("UIAComboGetAllItems")>
    Friend Class ComboGetAllItemsHandler : Inherits UIAutomationHandlerBase

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

            Return _
                GetComboItems(element, mAutomationFactory, True).
                    AsCollectionXml().
                    Map(AddressOf Reply.Result)

        End Function

    End Class
End Namespace
