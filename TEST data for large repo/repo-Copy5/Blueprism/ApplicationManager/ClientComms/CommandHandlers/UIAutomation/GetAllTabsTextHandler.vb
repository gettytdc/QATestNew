Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation

Namespace CommandHandlers.UIAutomation
    <Category(Category.UIAutomation)>
    <Command("Returns the text value of all tab items in a UIA Tab control.")>
    <Parameters("Those required to uniquely identify the element.")>
    <CommandId("UIAGetAllTabsText")>
    Friend Class GetAllTabsTextHandler : Inherits UIAutomationHandlerBase
        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetAllTabsTextHandler"/> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                          context As CommandContext) As Reply

            Dim automationFactory As IAutomationFactory =
                AutomationTypeProvider.GetType(Of IAutomationFactory)
            Return _
            element.FindAll(
                TreeScope.Children,
                automationFactory.CreatePropertyCondition(
                PropertyType.ControlType, ControlType.TabItem)).
                AsCollectionXml.
                Map(AddressOf Reply.Result)

        End Function

    End Class
End Namespace