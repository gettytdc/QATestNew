Public Class ctlChooseWebServiceTimeout

    Private Sub txtTimeout_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTimeout.TextChanged
        Dim result As Integer = 0
        NavigateNext = Integer.TryParse(txtTimeout.Text, result)
        UpdateNavigate()
    End Sub
End Class
