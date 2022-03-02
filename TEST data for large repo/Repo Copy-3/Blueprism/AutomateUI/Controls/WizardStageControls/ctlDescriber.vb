''' <summary>
''' Control to accept a description within the scope of a wizard
''' </summary>
Public Class ctlDescriber : Inherits ctlWizardStageControl

    ''' <summary>
    ''' The current description held in this control
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return txtDescription.Text
        End Get
        Set(ByVal value As String)
            txtDescription.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the description text changing. This forwards the event on to this
    ''' controls TextChanged event.
    ''' </summary>
    Private Sub HandleTextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDescription.TextChanged
        OnTextChanged(e)
    End Sub

    ''' <summary>
    ''' The prompt used to request the description from the user.
    ''' </summary>
    Public Property Prompt() As String
        Get
            Return lblPrompt.Text
        End Get
        Set(ByVal value As String)
            lblPrompt.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Handles Ctrl+A being pressed inside the description box to select all text.
    ''' </summary>
    Private Sub HandleSelectAllPressed(ByVal sender As Object, ByVal e As KeyEventArgs) _
     Handles txtDescription.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.A Then
            txtDescription.SelectAll()
            ' Suppress the press from the control - by default, it generates an
            ' "asterisk" sound event to indicate Ctrl+A is not supported - we want
            ' to ensure that doesn't happen.
            e.SuppressKeyPress = True
        End If
    End Sub
End Class
