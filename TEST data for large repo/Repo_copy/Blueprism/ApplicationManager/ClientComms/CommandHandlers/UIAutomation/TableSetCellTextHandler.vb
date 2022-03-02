Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <inheritdoc/>
    <CommandId("UIATableSetCellText")>
    Friend Class TableSetCellTextHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' Creates a new handler instance
        ''' </summary>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim pattern = GridHelper.EnsureCellElementPattern(Of IValuePattern)(
                                                              element,
                                                              context.Query,
                                                              PatternType.ValuePattern)

            Dim newText = context.Query.GetParameter(clsQuery.ParameterNames.NewText)
            Try
                pattern.SetValue(newText)
            Catch ex As InvalidOperationException
                Throw New OperationFailedException(UIAutomationErrorResources.OperationFailedException_ElementValueNotSetMessage)
            End Try

            Return Reply.Ok

        End Function
    End Class
End Namespace

