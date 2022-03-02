Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets all items from the combobox within a table cell
    ''' Required parameters: Those required to uniquely identify the cell
    ''' Result: "RESULT:&lt;xml&gt;" where &lt;xml&gt; is the collections xml 
    ''' containing all items in the combobox.
    ''' </summary>
    <CommandId("UIATableGetAllComboboxItems")>
    Friend Class TableGetAllComboboxItemsHandler : Inherits UIAutomationHandlerBase

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
            ExpandCollapseCombo(cellChildElement, mAutomationFactory)
            Try
                Return GetComboItems(cellChildElement, mAutomationFactory, True).
                                       AsCollectionXml().
                                       Map(AddressOf Reply.Result)
            Finally
                ExpandCollapseCombo(cellChildElement, mAutomationFactory)
            End Try

        End Function
    End Class
End Namespace
