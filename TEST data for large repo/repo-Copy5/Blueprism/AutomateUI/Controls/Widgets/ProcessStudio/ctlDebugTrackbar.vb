
Public Class ctlDebugTrackbar

    Public Property Maximum() As Integer
        Get
            Maximum = TrackBar.Maximum
        End Get
        Set(ByVal Maximum As Integer)
            TrackBar.Maximum = Maximum
        End Set
    End Property

    Public Property Minimum() As Integer
        Get
            Minimum = TrackBar.Minimum
        End Get
        Set(ByVal Minimum As Integer)
            TrackBar.Minimum = Minimum
        End Set
    End Property

    Public Property SmallChange() As Integer
        Get
            SmallChange = TrackBar.SmallChange
        End Get
        Set(ByVal SmallChange As Integer)
            TrackBar.SmallChange = SmallChange
        End Set
    End Property

    Public Property Value() As Integer
        Get
            Value = TrackBar.Value
        End Get
        Set(ByVal Value As Integer)
            TrackBar.Value = Value
        End Set
    End Property

    Public Property TopLabel() As String
        Get
            TopLabel = lblFast.Text
        End Get
        Set(ByVal TopLabel As String)
            lblFast.Text = TopLabel
        End Set
    End Property

    Public Property MiddleLabel() As String
        Get
            MiddleLabel = lblNormal.Text
        End Get
        Set(ByVal MiddleLabel As String)
            lblNormal.Text = MiddleLabel
        End Set
    End Property

    Public Property BottomLabel() As String
        Get
            BottomLabel = lblSlow.Text
        End Get
        Set(ByVal BottomLabel As String)
            lblSlow.Text = BottomLabel
        End Set
    End Property

    Public Property Title() As String
        Get
            Title = lblDebugSpeed.Text
        End Get
        Set(ByVal Title As String)
            lblDebugSpeed.Text = Title
        End Set
    End Property

    Public Shadows Event Scroll(ByVal sender As Object, ByVal e As System.EventArgs)


    Private Sub TrackBar_Scroll(ByVal sender As Object, ByVal e As System.EventArgs) Handles TrackBar.Scroll
        RaiseEvent Scroll(sender, e)
    End Sub
End Class
