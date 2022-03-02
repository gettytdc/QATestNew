Public Class ctlChooseWebServiceLocation

    Private Sub txtUrl_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtUrl.TextChanged
        Dim result As Uri = Nothing
        NavigateNext = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, result)
        UpdateNavigate()
    End Sub
End Class
