
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    <clsLocalTargetApp.Category(clsLocalTargetApp.Category.UIAutomation)>
    <clsLocalTargetApp.Command("Gets the value of the UI Automation element.")>
    <clsLocalTargetApp.Parameters("Those required to uniquely identify the element.")>
    <clsLocalTargetApp.Response("""RESULT:<value>"" where <value> is the value of the element.")>
    <CommandId("UIAGetValue")>
    Friend Class GetValueHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetValueHandler"/> class.
        ''' </summary>
        ''' <param name="application">The current application</param>
        ''' <param name="identifierHelper">A IUIAutomationIdentifierHelper for the current application</param>
        Public Sub New(application As ILocalTargetApp, identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritDoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            Dim textPattern = element.GetCurrentPattern(Of ITextPattern)()
            Dim valuePattern = element.GetCurrentPattern(Of IValuePattern)()
            Dim gridItemPattern = element.GetCurrentPattern(Of ITextChildPattern)()

            Dim result As String
            If textPattern IsNot Nothing Then
                result = textPattern.DocumentRange.GetText(Integer.MaxValue)
            ElseIf valuePattern IsNot Nothing Then
                result = valuePattern.CurrentValue
            ElseIf gridItemPattern IsNot Nothing Then
                result = gridItemPattern.TextRange.GetText(Integer.MaxValue)
            Else
                result = element.CurrentName
            End If

            Return Reply.Result(result)

        End Function

    End Class

End Namespace