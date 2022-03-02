Public Class frmDocs

    Private mDocument As String

    Friend Sub New(ByVal document As String)
        MyBase.New()
        InitializeComponent()

        mDocument = document
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub frmDocs_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtDocs.Text = mDocument
    End Sub
End Class
