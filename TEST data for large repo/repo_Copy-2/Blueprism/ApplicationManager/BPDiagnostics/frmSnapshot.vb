Public Class frmSnapshot

    Public mText As String


    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Close()
    End Sub

    Private Sub frmSnapshot_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtSnapshot.Text = mText
    End Sub

    Private Sub btnCopyToClipboard_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCopyToClipboard.Click
        Clipboard.SetText(txtSnapshot.Text)
    End Sub

End Class