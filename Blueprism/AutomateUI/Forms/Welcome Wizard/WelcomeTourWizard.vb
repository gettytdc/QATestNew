
Imports BluePrism.AutomateAppCore
Imports BluePrism.Core.Utility

Public Class WelcomeTourWizard

    Private ReadOnly DigitalExchangeUrl As String = "https://digitalexchange.blueprism.com"
    Private ReadOnly LearningUrl As String = "https://portal.blueprism.com/learning"
    Private ReadOnly mShowTourAtStartup As Boolean
    Private ReadOnly mShowLearningTab As Boolean = False

    Public Sub New(showAtStartup As Boolean)
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        mShowTourAtStartup = showAtStartup

        DigitalExchangeLink.LinkArea = New LinkArea(If(DigitalExchangeLink.Text.IndexOf(My.Resources.WelcomeTourWizardDigitalExchange) >= 0, DigitalExchangeLink.Text.IndexOf(My.Resources.WelcomeTourWizardDigitalExchange), 27),
                                                    My.Resources.WelcomeTourWizardDigitalExchange.Length)
        DigitalExchangeLink2.LinkArea = New LinkArea(If(DigitalExchangeLink2.Text.IndexOf(DigitalExchangeUrl) >= 0, DigitalExchangeLink2.Text.IndexOf(DigitalExchangeUrl), 7),
                                                     DigitalExchangeUrl.Length)
        LearningPortalLink.LinkArea = New LinkArea(If(LearningPortalLink.Text.IndexOf(LearningUrl) >= 0, LearningPortalLink.Text.IndexOf(LearningUrl), 7),
                                                   LearningUrl.Length)

        BackButton.Visible = False
        ShowWizardAtStartup.Visible = False
        ShowWizardAtStartup.Checked = mShowTourAtStartup
    End Sub

    Private Sub WizardSwitchPanel_SelectedIndexChanged(sender As Object, e As EventArgs) Handles WizardSwitchPanel.SelectedIndexChanged
        BackButton.Visible = WizardSwitchPanel.SelectedTab.Name <> WelcomeTab.Name

        'Next Lines logic is determined by the mShowLearningTab feature switch.
        If (mShowLearningTab AndAlso WizardSwitchPanel.SelectedTab.Name = LearnMoreTab.Name) OrElse (Not mShowLearningTab AndAlso WizardSwitchPanel.SelectedTab.Name = DigitalExchangeTab.Name) Then
            ShowWizardAtStartup.Visible = True
            NextButton.Text = My.Resources.WelcomeTourWizardGetStarted
        Else
            ShowWizardAtStartup.Visible = False
            NextButton.Text = My.Resources.WelcomeTourWizardNext
        End If

        ActiveControl = Nothing
    End Sub

    Private Sub NextButton_Click(sender As Object, e As EventArgs) Handles NextButton.Click
        If (mShowLearningTab AndAlso WizardSwitchPanel.SelectedTab.Name = LearnMoreTab.Name) OrElse (Not mShowLearningTab AndAlso WizardSwitchPanel.SelectedTab.Name = DigitalExchangeTab.Name) Then
            Close()
        Else
            WizardSwitchPanel.SelectedIndex = WizardSwitchPanel.SelectedIndex + 1
        End If
    End Sub

    Private Sub BackButton_Click(sender As Object, e As EventArgs) Handles BackButton.Click
        WizardSwitchPanel.SelectedIndex = WizardSwitchPanel.SelectedIndex - 1
    End Sub

    Private Sub DigitalExchangeLink2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles DigitalExchangeLink2.LinkClicked, DigitalExchangeLink.LinkClicked
        ShowDigitalExchange()
    End Sub

    Private Sub LearningPortalLink_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LearningPortalLink.LinkClicked
        ExternalBrowser.OpenUrl(LearningUrl)
    End Sub

    Private Sub ShowWizardAtStartup_CheckedChanged(sender As Object, e As EventArgs) Handles ShowWizardAtStartup.CheckedChanged
        gSv.SetUserPref(PreferenceNames.UI.ShowTourAtStartup, ShowWizardAtStartup.Checked)
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        ShowDigitalExchange()
    End Sub

    Private Sub ShowDigitalExchange()
        ExternalBrowser.OpenUrl(DigitalExchangeUrl)
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        Close()
    End Sub
End Class