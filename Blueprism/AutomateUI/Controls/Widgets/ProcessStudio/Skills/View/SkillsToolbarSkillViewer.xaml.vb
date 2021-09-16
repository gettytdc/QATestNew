Imports System.Windows
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Interfaces
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.ViewModel

Public Class SkillsToolbarSkillViewer
    Private ReadOnly mParent As SkillsToolbar

    Public Sub New(viewModel As ICategoryViewModel, parent As SkillsToolbar)
        DataContext = viewModel
        mParent = parent

        InitializeComponent()

        DisplaySkillButtons()
    End Sub

    Private Sub DisplaySkillButtons()
        For Each skill In CType(DataContext, CategorySkillsViewModel).Skills
            Dim skillButton = New SkillButton(Me)
            skillButton.DataContext = skill

            SkillsUniformGrid.Children.Add(skillButton)
        Next
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        SkillsUniformGrid.Children.Clear()
        DataContext = Nothing
    End Sub

    Public Sub NotifyParentInformationPage(skillId As Guid)
        mParent.DisplayInformationForSkill(skillId)
    End Sub
End Class
