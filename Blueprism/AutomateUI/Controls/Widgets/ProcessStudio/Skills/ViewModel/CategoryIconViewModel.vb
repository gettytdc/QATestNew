Public Class CategoryIconViewModel : Implements INotifyPropertyChanged
    Private mToolTipText As String

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        RaiseEvent PropertyChanged(sender, e)
    End Sub

    Public Property TooltipText As String
        Get
            Return mToolTipText
        End Get
        Set(value As String)
            mToolTipText = value
            OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(TooltipText)))
        End Set
    End Property

    Public Sub New(tooltip As String)
        TooltipText = tooltip
    End Sub
End Class
