
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation.Exceptions
Imports BluePrism.Server.Domain.Models

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebCloseApplication")>
    Friend Class WebCloseApplicationHandler : Inherits CommandHandlerBase

        Private ReadOnly mIdentifierHelper As IBrowserAutomationIdentifierHelper

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application)

            mIdentifierHelper = identifierHelper
        End Sub

        Public Overrides Function Execute(context As CommandContext) As Reply
            Try
                Dim pages = mIdentifierHelper.GetWebPages(context.Query)
                For Each p In pages
                    Logger.Info($"WebCloseApplicationHandler closing page: {p.Id}")
                    p.CloseWebPage()
                    mIdentifierHelper.RemoveWebPage(p.Id)
                Next

                Dim trackingId = If(context.Query.GetParameter("trackingid"), "")
                If String.IsNullOrWhiteSpace(trackingId) Then
                    Logger.Info($"WebCloseApplicationHandler DetachAllTrackedPages")
                    mIdentifierHelper.DetachAllTrackedPages()
                    CType(Application, clsLocalTargetApp).Disconnect()
                    Return Reply.True
                End If

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
