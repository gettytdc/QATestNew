Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Interfaces
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Model
Imports BluePrism.Skills

Namespace Controls.Widgets.ProcessStudio.Skills.ViewModel

    Public Class CategorySkillsViewModel : Implements INotifyPropertyChanged, ICategoryViewModel
        Private mTitle As String

        Public ReadOnly Property Skills As New List(Of SkillViewModel)
        Public ReadOnly Property Category As SkillCategory
        Public Property Title As String Implements ICategoryViewModel.Title
            Get
                Return mTitle
            End Get
            Set
                If mTitle <> Value Then
                    mTitle = Value
                    OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Title)))
                End If
            End Set
        End Property

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements ICategoryViewModel.OnPropertyChanged
            RaiseEvent PropertyChanged(sender, e)
        End Sub

        Public Sub New(skillCategory As SkillCategory, newSkills As List(Of SkillViewModel))
            Category = skillCategory
            Skills = newSkills
            Title = $"{SkillCategoryExtensions.GetDescription(Category)} ({Skills.Count})"
        End Sub
    End Class
End Namespace