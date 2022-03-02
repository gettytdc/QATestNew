
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Server.Domain.Models

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebCheckExists")>
    Friend Class CheckExistsHandler : Inherits CommandHandlerBase

        Private ReadOnly mIdentifierHelper As IBrowserAutomationIdentifierHelper

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application)

            mIdentifierHelper = identifierHelper
        End Sub

        Public Overrides Function Execute(context As CommandContext) As Reply
            Try
                mIdentifierHelper.FindSingleElement(context.Query)
                Return Reply.True
            Catch ex As NoSuchElementException
                Return Reply.False
            End Try
        End Function

    End Class
End Namespace
