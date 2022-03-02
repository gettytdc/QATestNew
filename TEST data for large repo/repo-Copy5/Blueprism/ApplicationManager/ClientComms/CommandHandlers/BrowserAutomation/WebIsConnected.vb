Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation.Exceptions

Namespace CommandHandlers.BrowserAutomation
    <CommandId("WebIsConnected")>
    Friend  Class WebIsConnected: Inherits CommandHandlerBase
        Private ReadOnly mIdentifierHelper As IBrowserAutomationIdentifierHelper
        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application)

            mIdentifierHelper = identifierHelper
        End Sub

         Public Overrides Function Execute(context As CommandContext) As Reply
            Try
                Dim trackingId = If(context.Query.GetParameter("trackingid"), "")
                If mIdentifierHelper.IsTracking(trackingId) Then
                    Return Reply.True
                End If
                Return Reply.False
            Catch be As BrowserAutomationException
                Throw New ApplicationException(be.Message, be)
            Catch
                Return Reply.False
            End Try
        End Function
    End Class
End Namespace
