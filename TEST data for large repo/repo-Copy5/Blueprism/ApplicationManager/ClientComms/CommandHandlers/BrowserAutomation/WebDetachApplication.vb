Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation.Exceptions

Namespace CommandHandlers.BrowserAutomation
    <CommandId("WebDetachApplication")>
    Friend Class WebDetachApplication : Inherits CommandHandlerBase
        Private ReadOnly mIdentifierHelper As IBrowserAutomationIdentifierHelper
        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application)

            mIdentifierHelper = identifierHelper
        End Sub

        Public Overrides Function Execute(context As CommandContext) As Reply
            Try
                Dim trackingId = If(context.Query.GetParameter("trackingid"), "")
                If String.IsNullOrWhiteSpace(trackingId) Then
                    Logger.Info($"WebDetachApplication DetachAllTrackedPages")
                    mIdentifierHelper.DetachAllTrackedPages()
                    CType(Application, clsLocalTargetApp).Disconnect()
                    Return Reply.True
                End If

                Logger.Info($"WebDetachApplication DetachTrackedPage for tracking ID {trackingId}")
                mIdentifierHelper.DetachTrackedWebPage(trackingId)
                If Not mIdentifierHelper.IsTracking Then
                    CType(Application, clsLocalTargetApp).Disconnect()
                End If
                Return Reply.True
            Catch be As BrowserAutomationException
                Throw New ApplicationException(be.Message, be)
            Catch
                Return Reply.False
            End Try
        End Function
    End Class
End Namespace
