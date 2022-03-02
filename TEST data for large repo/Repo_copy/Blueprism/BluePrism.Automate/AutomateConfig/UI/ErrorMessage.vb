''' <summary>
''' Simple form to show messages to the user
''' </summary>
Public Class ErrorMessage

    ''' <summary>
    ''' The message to display
    ''' </summary>
    Public Property Message As String
        Get
            Return txtMessage.Text
        End Get
        Set(value As String)
            txtMessage.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The detailed message
    ''' </summary>
    Public Property Detail As String
        Get
            Return txtDetail.Text
        End Get
        Set(value As String)
            txtDetail.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the ok button click
    ''' </summary>
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Close()
    End Sub

    ''' <summary>
    ''' Handles the details button click
    ''' </summary>
    Private Sub btnDetail_Click(sender As Object, e As EventArgs) Handles btnDetail.Click

        If txtDetail.Visible Then
            CollapseFormAndHideDetail()
        Else
            ExpandFormAndShowDetail()
        End If

    End Sub

    Private Sub CollapseFormAndHideDetail()
        Dim collapsedWhiteSpace = 16

        btnDetail.Text = My.Resources.Detail
        Me.ClientSize =
         New Size(Me.ClientSize.Width, txtMessage.Bottom + btnCopy.Height + collapsedWhiteSpace)
        txtDetail.Visible = False
    End Sub

    Private Sub ExpandFormAndShowDetail()
        Dim expandedWhiteSpace = 10

        btnDetail.Text = My.Resources.Hide
        Me.ClientSize =
         New Size(Me.ClientSize.Width, txtDetail.Bottom + expandedWhiteSpace)
        txtDetail.Visible = True
    End Sub

    ''' <summary>
    ''' Handles the copy button click
    ''' </summary>
    Private Sub btnCopy_Click(sender As Object, e As EventArgs) Handles btnCopy.Click
        Try
            Dim msg = txtMessage.Text

            If txtDetail.Visible Then
                msg &= vbCrLf & vbCrLf & vbCrLf & vbCrLf & txtDetail.Text
            End If

            Clipboard.SetText(msg)
        Catch
        End Try
    End Sub
End Class