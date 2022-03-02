Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    '''<summary>
    ''' Gets the selected items in the list/table
    ''' Required parameters: Those required to uniquely identify the list/table
    ''' Result: """RESULT:&lt;xml&gt;"" where &lt;xml&gt; is the collections xml containing the selected items."
    ''' </summary>
    <CommandId("UIAGetSelectedItems")>
    Friend Class GetSelectedItemsHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetSelectedItemsHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Return _
                GetListItems(element).
                Select(Function(x) New With {
                    Key .Element = x,
                    Key .Pattern = x.GetCurrentPattern(Of ISelectionItemPattern)()}).
                Where(Function(x) x.Pattern.CurrentIsSelected).
                Select(Function(x) x.Element).
                AsCollectionXml().
                Map(AddressOf Reply.Result)

        End Function
    End Class
End Namespace
