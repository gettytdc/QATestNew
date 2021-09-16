Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Server.Domain.Models

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebCheckParentDocumentLoaded")>
    Friend Class CheckParentDocumentLoadedHandler : Inherits CommandHandlerBase

        Private ReadOnly mIdentifierHelper As IBrowserAutomationIdentifierHelper

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application)

            mIdentifierHelper = identifierHelper
        End Sub

        Public Overrides Function Execute(context As CommandContext) As Reply
            Try
                Dim element = mIdentifierHelper.FindSingleElement(context.Query)
                Return If(element.CheckParentDocumentLoaded(),Reply.True,Reply.False)
            Catch ex As NoSuchElementException
                Return Reply.False
            End Try
        End Function

    End Class
End Namespace
