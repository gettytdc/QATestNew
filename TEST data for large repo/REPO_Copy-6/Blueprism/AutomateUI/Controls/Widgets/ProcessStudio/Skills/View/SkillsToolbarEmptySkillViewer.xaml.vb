Imports System.Windows
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Interfaces

Public Class SkillsToolbarEmptySkillViewer

    Public Sub New(viewModel As ICategoryViewModel)
        DataContext = viewModel

        InitializeComponent()
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        DataContext = Nothing
    End Sub
End Class
