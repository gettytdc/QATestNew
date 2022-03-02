Public Class ButtonWithTick

    Public Event ButtonClicked As EventHandler


    Private Sub btnButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnButton.Click
        RaiseEvent ButtonClicked(sender, e)
    End Sub

    Public Property Checked() As Boolean
        Get
            Return pbTick.Visible
        End Get
        Set(ByVal value As Boolean)
            pbTick.Visible = value
        End Set
    End Property


    ''' <summary>
    ''' Indicates whether or not the button displayed should 
    ''' be enabled or not.
    ''' </summary>
    ''' <returns>As summary.</returns>
    Public Property ButtonEnabled() As Boolean
        Get
            Return Me.btnButton.Enabled
        End Get
        Set(ByVal value As Boolean)
            Me.btnButton.Enabled = value
        End Set
    End Property


End Class
