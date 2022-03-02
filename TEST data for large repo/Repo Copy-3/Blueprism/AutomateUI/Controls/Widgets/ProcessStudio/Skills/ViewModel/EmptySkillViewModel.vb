Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Interfaces
Imports BluePrism.Skills

Namespace Controls.Widgets.ProcessStudio.Skills.ViewModel
    Public Class EmptySkillViewModel
        Implements INotifyPropertyChanged, ICategoryViewModel

        Private mMessage As String
        Private mTitle As String

        Public Property Message As String
            Get
                Return mMessage
            End Get
            Set(value As String)
                mMessage = value
                OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Message)))
            End Set
        End Property

        Public Property Title As String Implements ICategoryViewModel.Title
            Get
                Return mTitle
            End Get
            Set
                mTitle = Value
                OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Title)))
            End Set
        End Property

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub New(category As SkillCategory, message As String)
            mTitle = $"{SkillCategoryExtensions.GetDescription(category)} (0)"
            mMessage = message
        End Sub

        Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements ICategoryViewModel.OnPropertyChanged
            RaiseEvent PropertyChanged(sender, e)
        End Sub

    End Class
End Namespace