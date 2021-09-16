Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Interfaces
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.ViewModel
Imports BluePrism.Skills

Public Class SkillsToolbar

    Private Const SkillCategoryColumnWidth As Integer = 70
    Private Const ToolbarBodyWidth As Integer = 253

    Private currentInformationPage As SkillsToolbarInformationViewer

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        DataContext = New SkillModel()
    End Sub

    Private Sub OnLoaded() Handles Me.Loaded
        SetupCategoryIconTooltips()
        VisualPerceptionButton.IsChecked = True
        MinimiseToolbar()
    End Sub

    Private Sub SetupCategoryIconTooltips()
        VisualPerceptionButton.DataContext = CreateIconViewModel(SkillCategory.VisualPerception)
        PlanningAndSequencingButton.DataContext = CreateIconViewModel(SkillCategory.PlanningAndSequencing)
        CollaborationButton.DataContext = CreateIconViewModel(SkillCategory.Collaboration)
        KnowledgeAndInsightButton.DataContext = CreateIconViewModel(SkillCategory.KnowledgeAndInsight)
        ProblemSolvingButton.DataContext = CreateIconViewModel(SkillCategory.ProblemSolving)
        LearningButton.DataContext = CreateIconViewModel(SkillCategory.Learning)
    End Sub

    Private Function CreateIconViewModel(category As SkillCategory) As CategoryIconViewModel
        Return New CategoryIconViewModel(GetTooltipTextForCategory(category))
    End Function

    Private Function GetTooltipTextForCategory(category As SkillCategory) As String
        Dim skillModel = TryCast(DataContext, SkillModel)
        Return $"{SkillCategoryExtensions.GetDescription(category)} ({skillModel.GetSkillsFor(category).Count()})"
    End Function

    Private Sub ChangeSkillToolbarPage(sender As Object, e As EventArgs)
        Dim selectedButton = CType(sender, ToggleButton)
        Dim selectedCategory = CType(selectedButton.Tag, SkillCategory)

        MaximiseToolbarIfRequired()
        ChangeInnerPage(selectedCategory)
    End Sub

    Private Sub MaximiseToolbarIfRequired()
        If SkillsToolbarBody.Visibility.Equals(Visibility.Collapsed) Then
            SkillsToolbarBody.Visibility = Visibility.Visible
            Width = ToolbarBodyWidth
        End If
    End Sub

    Private Sub MinimiseToolbar()
        SkillsToolbarBody.Visibility = Visibility.Collapsed
        Width = SkillCategoryColumnWidth
        HiddenButton.IsChecked = True
    End Sub

    Private Sub ChangeInnerPage(skillCategory As SkillCategory)
        SkillsToolbarContent.Children.Clear()
        Dim skillsForCategory = CType(DataContext, SkillModel).GetSkillsFor(skillCategory)
        Dim viewModel As ICategoryViewModel
        Dim contentPage As UIElement

        If (skillsForCategory.Count > 0) Then
            viewModel = New CategorySkillsViewModel(skillCategory, skillsForCategory.ToList())
            contentPage = New SkillsToolbarSkillViewer(viewModel, Me)
        Else
            viewModel = New EmptySkillViewModel(skillCategory, My.Resources.NoSkillsAvailable)
            contentPage = New SkillsToolbarEmptySkillViewer(viewModel)
        End If

        SkillsToolbarContent.Children.Add(contentPage)
    End Sub

    Private Sub SkillToolbarControl_Unloaded(sender As Object, e As RoutedEventArgs) Handles SkillToolbarControl.Unloaded
        DataContext = Nothing
        SkillsToolbarContent.Children.Clear()
    End Sub

    Private Sub SkillToolbarContractButton_Click(sender As Object, e As RoutedEventArgs)
        MinimiseToolbar()
    End Sub

    Public Sub DisplayInformationForSkill(skillId As Guid)
        Dim skillViewModel = CType(DataContext, SkillModel).GetSkill(skillId)
        currentInformationPage = New SkillsToolbarInformationViewer(skillViewModel, Me)

        SkillsToolbarContent.Children.Add(currentInformationPage)
    End Sub

    Public Sub CloseInformationPage()
        SkillsToolbarContent.Children.Remove(currentInformationPage)
    End Sub

End Class
