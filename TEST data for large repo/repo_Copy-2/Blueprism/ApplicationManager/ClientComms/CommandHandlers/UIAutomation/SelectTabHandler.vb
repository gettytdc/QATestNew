Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.ApplicationManager.clsLocalTargetApp
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <Category(Category.UIAutomation)>
    <Command("Selects the specified Tab within a UIA Tab Control.")>
    <Parameters("Those required to uniquely identify the element, 
    plus the Name or Index of the tab to be selected.")>
    <CommandId("UIASelectTab")>
    Friend Class SelectTabHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SelectTabHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        ''' <param name="automationFactory">The automation factory.</param>

        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper,
                       automationFactory As IAutomationFactory)

            MyBase.New(application, identifierHelper)
            mAutomationFactory = automationFactory

        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                          context As CommandContext) As Reply
            Dim MinTabIndex = 1

            Dim index = context.Query.GetIntParam(ParameterNames.TabIndex)
            Dim text = context.Query.GetParameter(ParameterNames.TabText)

            If index < MinTabIndex AndAlso text Is Nothing Then
                ' We have nothing to search with
                Return Reply.False
            End If

            Dim tab As IAutomationElement = Nothing
            ' get all the child tabs of the control
            Dim childTabs = element.FindAll(
                      TreeScope.Children,
                      mAutomationFactory.CreatePropertyCondition(
                      PropertyType.ControlType, ControlType.TabItem)).ToList()

            ' Search using text first (if we have it)
            If text IsNot Nothing Then
                tab = childTabs.Find(Function(t) t.CurrentName = text)
                If tab Is Nothing Then Throw New TabItemOutOfRangeException(
                    ParameterNames.TabText)
            Else
                ' The index parameter is 1-based so adjust for the collection 
                If index - 1 > childTabs.Count Then
                    Throw New TabItemOutOfRangeException(ParameterNames.TabIndex)
                End If
                tab = childTabs(index - 1)
            End If

            tab.EnsurePattern(Of ISelectionItemPattern).Select()

            Return Reply.Ok

        End Function

    End Class

End Namespace