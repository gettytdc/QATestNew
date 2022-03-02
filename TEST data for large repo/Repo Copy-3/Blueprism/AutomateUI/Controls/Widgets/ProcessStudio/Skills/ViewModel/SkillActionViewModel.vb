Public Class SkillActionViewModel
    Implements INotifyPropertyChanged
    Private mActionName As String

    Public Property ActionName As String
        Get
            Return mActionName
        End Get
        Set(value As String)
            If mActionName <> value Then
                mActionName = value
                OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(ActionName)))
            End If
        End Set
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Sub New(skillActionName As String)
        ActionName = skillActionName
    End Sub

    Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        RaiseEvent PropertyChanged(sender, e)
    End Sub
End Class
