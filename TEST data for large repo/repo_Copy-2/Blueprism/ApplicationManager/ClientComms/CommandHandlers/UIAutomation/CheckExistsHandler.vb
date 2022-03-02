Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Checks to see if the element can be identified. Required parameters: Those 
    ''' required to uniquely identify the element
    ''' </summary>
    <CommandId("UIACheckExists")>
    Friend Class CheckExistsHandler : Inherits CommandHandlerBase

        Private ReadOnly mUiAutomationIdHelper As IUIAutomationIdentifierHelper

        ''' <param name="application">The application against which the handler is
        ''' running</param>
        Public Sub New(
                      application As ILocalTargetApp,
                      uiAutomationIdHelper As IUIAutomationIdentifierHelper)
            MyBase.New(application)
            mUiAutomationIdHelper = uiAutomationIdHelper
        End Sub

        ''' <inheritdoc/>
        Public Overrides Function Execute(context As CommandContext) As Reply
            Try
                mUiAutomationIdHelper.FindUIAutomationElement(
                    context.Query, Application.PID)
                Return Reply.True
            Catch ex As ApplicationException
                Return Reply.False
            End Try
        End Function

    End Class
End NameSpace