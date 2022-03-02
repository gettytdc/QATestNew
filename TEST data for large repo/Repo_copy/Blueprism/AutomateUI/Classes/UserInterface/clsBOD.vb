
Imports BluePrism.AutomateProcessCore

Public Class clsBOD

    ''' <summary>
    ''' Opens a business object's documentation in a COM-initiated Internet
    ''' Explorer window.
    ''' </summary>
    Public Shared Sub OpenAPIDocumentation(ByVal bo As clsBusinessObject)
        If Not bo.Valid Then
            UserMessage.Show(My.Resources.CannotGenerateDocumentationObjectIsNotValid)
            Return
        End If
        Dim ie As Object = CreateObject("InternetExplorer.application")
        CallByName(ie, "Navigate", CallType.Method, New Object() {"about:blank"})
        Dim doc As Object = CallByName(ie, "Document", CallType.Get)
        CallByName(ie, "Visible", CallType.Let, New Object() {True})
        CallByName(doc, "Write", CallType.Method, New Object() {bo.GetDocumentationHTML()})
        CallByName(doc, "Title", CallType.Set, New Object() {bo.FriendlyName})
    End Sub

End Class
