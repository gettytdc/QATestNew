Public Class NewActiveDirectoryUserViewModel
    Public Property Sid As String
    Public Property Upn As String

    Public Sub New()

    End Sub

    Public Sub New(sid As String, upn As String)
        Me.Sid = sid
        Me.Upn = upn
    End Sub
End Class
