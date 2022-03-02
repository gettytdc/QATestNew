Imports System.Globalization
Imports System.Linq
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Gets the selected column index.
    ''' </summary>
    <CommandId("UIATableSelectedColumnNumber")>
    Friend Class TableSelectedColumnNumberHandler : Inherits UIAutomationHandlerBase

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp, 
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement, context As CommandContext) As Reply

            ' Get first selected element
            Dim selected = element.FindAll(TreeScope.Children).FirstOrDefault(
                Function(e)
                    Dim itemPattern = e.GetCurrentPattern(Of ISelectionItemPattern)
                    Return itemPattern IsNot Nothing AndAlso
                           itemPattern.CurrentIsSelected
                                                                                 End Function)
            Const emptySelectionIndex = -1
            If selected Is Nothing Then
                Return Reply.Result(emptySelectionIndex)
            End If
            Dim pattern = selected.EnsurePatternFromSubtree(Of IGridItemPattern)(
                PatternType.GridItemPattern)
            Dim number = pattern.CurrentColumn + 1
            Return Reply.Result(number.ToString(CultureInfo.InvariantCulture))

        End Function
    End Class
End Namespace
