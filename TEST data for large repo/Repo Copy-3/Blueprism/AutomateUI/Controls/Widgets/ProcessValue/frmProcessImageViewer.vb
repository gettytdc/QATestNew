Friend Class frmProcessImageViewer
    Inherits frmForm

    Private mImage As Bitmap

    Sub New(ByVal b As Bitmap)
        InitializeComponent()
        mImage = b
    End Sub

    Private Sub frmProcessImageViewer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If Not mImage Is Nothing Then
            ClientSize = New Size(mImage.Width, mImage.Height)
            PictureBox1.Image = mImage
        End If
    End Sub

End Class
