Imports System.Windows
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Model

Public Class SkillsToolbarInformationViewer

    Private mParent As SkillsToolbar

    Public Sub New(viewModel As SkillViewModel, ByRef parent As SkillsToolbar)
        DataContext = viewModel
        mParent = parent

        InitializeComponent()
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        DataContext = Nothing
    End Sub

    Private Sub CloseInformationButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseInformationButton.Click
        mParent.CloseInformationPage()
    End Sub
End Class
