

Public Class ctlChooseWebService
    Private Sub lvServices_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvServices.SelectedIndexChanged
        NavigateNext = (lvServices.SelectedItems.Count > 0)
        UpdateNavigate()
    End Sub
End Class
