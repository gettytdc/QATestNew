Imports System.Windows
Imports System.Windows.Input
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.Model
Imports AutomateUI.ctlProcessViewer

Public Class SkillButton
    Private ReadOnly mParent As SkillsToolbarSkillViewer

    'Mouse starting point (for drag & drop)
    Private mouseStart As Point

    Public Sub New(parent As SkillsToolbarSkillViewer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mParent = parent

        AddHandler SkillButton.PreviewMouseLeftButtonDown, AddressOf button_PreviewMouseLeftButtonDown
        AddHandler SkillButton.PreviewMouseMove, AddressOf button_PreviewMouseMove
    End Sub

    Private Sub button_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        'Record start position (for drag & drop)
        mouseStart = e.GetPosition(Nothing)
    End Sub

    Private Sub button_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        'Start drag event if mouse has moved sufficiently
        Dim mousePosition As Point = e.GetPosition(Nothing)
        Dim diff As Vector = mousePosition - mouseStart

        If e.LeftButton = MouseButtonState.Pressed AndAlso
             (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance OrElse
             Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) Then
            Dim data = New StageDropContainer() With {
                .Tool = StudioTool.Skill,
                .Context = CType(DataContext, SkillViewModel).ID}
            System.Windows.DragDrop.DoDragDrop(SkillButton, data, DragDropEffects.Copy)
        End If
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        SkillLogoImage.Source = Nothing
    End Sub

    Private Sub InformationButton_Click(sender As Object, e As RoutedEventArgs) Handles InformationButton.Click
        Dim skillViewModel = CType(DataContext, SkillViewModel)

        mParent.NotifyParentInformationPage(skillViewModel.ID)
    End Sub
End Class
