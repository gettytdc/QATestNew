''' <summary>
''' Simple static class to show messages to the user
''' </summary>
Public Class UserMessage

    ''' <summary>
    ''' Show a message
    ''' </summary>
    ''' <param name="msg">The message</param>
    Public Shared Sub Show(msg As String)
        Dim f As New ErrorMessage
        f.Message = msg
        f.ShowInTaskbar = False
        f.StartPosition = FormStartPosition.CenterParent
        f.ShowDialog()
    End Sub

    ''' <summary>
    ''' Show an error message
    ''' </summary>
    ''' <param name="msg">The error message</param>
    ''' <param name="ex">The exception</param>
    Public Shared Sub Err(msg As String, ex As Exception)
        Dim f As New ErrorMessage
        f.btnDetail.Visible = True
        f.Message = msg
        f.Detail = ex.ToString()
        f.ShowInTaskbar = False
        f.StartPosition = FormStartPosition.CenterParent
        f.ShowDialog()
    End Sub

    ''' <summary>
    ''' Show an error message
    ''' </summary>
    ''' <param name="ex">The exception</param>
    Public Shared Sub Err(ex As Exception)
        Err(String.Format(My.Resources.FollowingErrorHasOccuredMessage, Environment.NewLine, ex.Message), ex)
    End Sub
End Class
