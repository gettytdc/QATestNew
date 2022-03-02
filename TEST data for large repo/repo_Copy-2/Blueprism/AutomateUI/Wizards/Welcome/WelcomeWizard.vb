Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Events
Imports BluePrism.BPCoreLib
Imports BluePrism.Config
Imports BluePrism.Core.Utility
Imports BluePrism.DatabaseInstaller
Imports BluePrism.AutomateAppCore.Utility

Public Class WelcomeWizard

    Const HelpTrialLearningHtml = "Guides/trial-learning/tr-introduction.htm"

    Public Property LocalDB As LocalDatabaseInstaller
    Public Property UpgradeOnly As Boolean = False
    Private Property mErrorMessage As Exception
    Private Property mMouseDownLocation As Point
    Private Property mDigitalExchangeLicensingUrl As String = "https://digitalexchange.blueprism.com/site/global/software/index.gsp#"

    Private Sub WelcomeWizard_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        AddHandler LocalDB.ReportProgress, AddressOf OnReportProgress
        AddHandler LocalDB.ReportCurrentStep, AddressOf OnReportCurrentStep
        AddHandler LocalDB.ChangeProgressBarStyle, AddressOf OnChangeProgressBarStyle
        AddDatabaseFailureBodyLinks()
        AddDatabaseSuccessBodyLinks()

        If UpgradeOnly Then
            PrepareUpgradeUi()
            BackgroundDatabaseInstaller.RunWorkerAsync()
        End If

        'This must be hard coded as default username is not localised
        CreateUsernameTextBox.Text = "admin"
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If Not UacHelper.IsProcessElevated Then
            Me.InstallDatabaseNextButton.Image = My.Resources.shield
        End If

        SetLocalisedText()
    End Sub

    Private Sub SetLocalisedText()
        DatabaseSuccessBodyLinkLabel.Text = String.Format(My.Resources.BluePrismInstalled_LicenseLink0,
                                                          My.Resources.BluePrismInstalled_LicenseLinkText)
    End Sub

    Private Sub AddDatabaseFailureBodyLinks()
        Dim link = HelpLauncher.GetHelpUrl(Me, HelpTrialLearningHtml)
        Dim linkText = My.Resources.WelcomeWizard_Help
        Dim linkIndex = DatabaseFailureBodyLinkLabel.Text.IndexOf(linkText, StringComparison.Ordinal)
        If linkIndex >= 0 Then DatabaseFailureBodyLinkLabel.Links.Add(linkIndex, linkText.Length, link)
    End Sub

    Private Sub PrepareUpgradeUi()
        Me.AcceptButton = Nothing
        InstallDatabaseNextButton.Enabled = False
        BuildingDatabaseTitle.Title = My.Resources.UpgradingDatabaseTitle
        BuildingDatabaseSubTitle.Text = My.Resources.UpgradingDatabaseText
        BuildingDatabasePictureBox.Visible = False
        UpgradeDatabasePictureBox.Visible = True
        BuildingDatabaseLabel.Text = My.Resources.WelcomeWizard_UpgradingDatabase
        WizardSteps.SelectedTab = BuildingDatabaseStep
    End Sub

    Private Sub AddDatabaseSuccessBodyLinks()
        Dim licenseLinkText = My.Resources.BluePrismInstalled_LicenseLinkText
        Dim licenseLinkTextStartIndex = DatabaseSuccessBodyLinkLabel.Text.IndexOf(licenseLinkText)
        If licenseLinkTextStartIndex >= 0 Then _
            DatabaseSuccessBodyLinkLabel.LinkArea = New LinkArea(licenseLinkTextStartIndex, licenseLinkText.Length)
    End Sub

    Private Sub InstallDatabaseNextButton_Click(sender As Object, e As EventArgs) Handles InstallDatabaseNextButton.Click
        Me.AcceptButton = Nothing
        InstallDatabaseNextButton.Enabled = False
        WizardSteps.SelectedTab = BuildingDatabaseStep
        BackgroundDatabaseInstaller.RunWorkerAsync()
    End Sub

    Private Sub CreatePasswordNextButton_Click(sender As Object, e As EventArgs) Handles CreatePasswordNextButton.Click

        Select Case CheckPasswords()

            Case PasswordRules.RuleViolation.None

                CreatePasswordNextButton.Enabled = False
                WizardSteps.SelectedTab = InstallDatabaseStep
                Me.AcceptButton = InstallDatabaseNextButton

            Case PasswordRules.RuleViolation.NonMatching

                CreatePasswordWarningLabel.Text =
                    My.Resources.WelcomeWizard_PasswordsDoNotMatch
                WarningVisible = True

            Case PasswordRules.RuleViolation.TooShort

                CreatePasswordWarningLabel.Text =
                   My.Resources.WelcomeWizard_PasswordTooShort
                WarningVisible = True

            Case PasswordRules.RuleViolation.NoUppercase

                CreatePasswordWarningLabel.Text =
                   My.Resources.WelcomeWizard_PasswordNoUppercase
                WarningVisible = True

            Case PasswordRules.RuleViolation.NoLowercase

                CreatePasswordWarningLabel.Text =
                   My.Resources.WelcomeWizard_PasswordNoLowercase
                WarningVisible = True

            Case PasswordRules.RuleViolation.NoDigits

                CreatePasswordWarningLabel.Text =
                   My.Resources.WelcomWizard_PasswordNoDigits
                WarningVisible = True

        End Select

    End Sub

    Private Property WarningVisible As Boolean
        Get
            Return CreatePasswordWarningContainer.Visible
        End Get
        Set(value As Boolean)
            CreatePasswordPictureBox.Visible = Not value
            PasswordRequirementsLabel.Visible = Not value
            PasswordRequiementsDetailsLabel.Visible = Not value

            CreatePasswordWarningContainer.Visible = value
            CreatePasswordWarningPictureBox.Visible = value
            RepeatPasswordWarningPictureBox.Visible = value

            If value Then
                CreatePasswordTextBox.BorderColor = Color.FromArgb(203, 98, 0)
                RepeatPasswordTextBox.BorderColor = Color.FromArgb(203, 98, 0)
            Else
                CreatePasswordTextBox.BorderColor = Color.FromArgb(11, 117, 183)
                RepeatPasswordTextBox.BorderColor = Color.FromArgb(11, 117, 183)
            End If
        End Set
    End Property



    Private Function CheckPasswords() As PasswordRules.RuleViolation
        Dim rules As New PasswordRules With {
        .PasswordLength = 8,
        .UseDigits = True,
        .UseUpperCase = True,
        .UseLowerCase = True
        }

        Return rules.CheckPassword(CreatePasswordTextBox.SecurePassword, RepeatPasswordTextBox.SecurePassword)
    End Function

    Private Sub BackgroundDatabaseInstaller_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundDatabaseInstaller.DoWork
        Try
            If UpgradeOnly Then
                'Include artificial delay so the user understands what is going on as sometimes the upgrade can
                'take < 1 second to complete.
                Dim timer = Stopwatch.StartNew
                LocalDB.Upgrade()
                If timer.Elapsed < TimeSpan.FromSeconds(5) Then
                    Threading.Thread.Sleep(TimeSpan.FromSeconds(5) - timer.Elapsed)
                End If

            Else
                LocalDB.FullInstall(CreateUsernameTextBox.Text, CreatePasswordTextBox.SecurePassword)
            End If
            e.Result = True
        Catch ex As Exception
            e.Result = False
            mErrorMessage = ex
        End Try
    End Sub

    Private Sub OnChangeProgressBarStyle(sender As Object, e As EventArgs)
        Me.Invoke(Sub() BuildingDatabaseProgressBar.Style = ProgressBarStyle.Continuous)
        AddHandler LocalDB.ReportProgress, AddressOf OnReportProgress
    End Sub

    Private Sub OnReportCurrentStep(sender As Object, e As ReportCurrentStepEventArgs)
        Dim message As String = Nothing
        Select Case e.CurrentStep
            Case CurrentStepTypes.Install
                message = My.Resources.WelcomeWizard_InstallingLocalDatabase
            Case CurrentStepTypes.CreateInstance
                message = My.Resources.WelcomeWizard_CreatingLocalDatabaseInstance
            Case CurrentStepTypes.ConfigureDatabase
                message = My.Resources.WelcomeWizard_ConfiguringDatabase
            Case CurrentStepTypes.CreateDatabase
                message = My.Resources.WelcomeWizard_CreatingDatabase
            Case CurrentStepTypes.UpgradeDatabase
                message = My.Resources.WelcomeWizard_UpgradingDatabase
            Case CurrentStepTypes.Complete
                message = My.Resources.WelcomeWizard_InstallComplete
        End Select

        Me.Invoke(Sub() BuildingDatabaseLabel.Text = message)
    End Sub

    Private Sub OnReportProgress(sender As Object, e As PercentageProgressEventArgs)
        BackgroundDatabaseInstaller.ReportProgress(e.PercentProgress)
    End Sub

    Private Sub BackgroundDatabaseInstaller_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundDatabaseInstaller.ProgressChanged
        BuildingDatabaseProgressBar.Value = e.ProgressPercentage
    End Sub

    Private Sub BackgroundDatabaseInstaller_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BackgroundDatabaseInstaller.RunWorkerCompleted
        If CBool(e.Result) Then
            If UpgradeOnly Then
                WizardSteps.SelectedTab = DatabaseUpgradeSuccess
            Else
                WizardSteps.SelectedTab = DatabaseBuildSuccess
            End If
        Else
            WizardSteps.SelectedTab = DatabaseBuildFailure
        End If
    End Sub

    Private Sub CreatePasswordTextBox_TextChanged(sender As Object, e As EventArgs) Handles CreatePasswordTextBox.TextChanged, RepeatPasswordTextBox.TextChanged
        WarningVisible = False
    End Sub
    Private Sub DatabaseFailureBodyLinkLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles DatabaseFailureBodyLinkLabel.LinkClicked
        ExternalBrowser.OpenUrl(e.Link.LinkData.ToString())
    End Sub

    Private Sub DatabaseFailureErrorButton_Click(sender As Object, e As EventArgs) Handles DatabaseFailureErrorButton.Click
        Me.Invoke(Sub() UserMessage.Err(mErrorMessage, mErrorMessage.Message))
    End Sub

    Private Sub DatabaseSuccessBodyLinkLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles DatabaseSuccessBodyLinkLabel.LinkClicked
        ExternalBrowser.OpenUrl(mDigitalExchangeLicensingUrl)
    End Sub

    Private Sub DatabaseSuccessNextButton_Click(sender As Object, e As EventArgs) Handles DatabaseSuccessNextButton.Click, UpgradeFinishButton.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub closeButton_Click(sender As Object, e As EventArgs) Handles CreatePasswordCloseButton.Click,
        DatabaseFailureCloseButton.Click,
        DatabaseSuccessCloseButton.Click,
        InstallDatabaseCloseButton.Click,
        UpgradeCloseButton.Click,
        ExitFailureButton.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub WizardSteps_MouseDown(sender As Object, e As MouseEventArgs) Handles CreatePasswordStep.MouseDown,
        InstallDatabaseStep.MouseDown,
        BuildingDatabaseStep.MouseDown,
        DatabaseBuildSuccess.MouseDown,
        DatabaseBuildFailure.MouseDown
        If e.Button = MouseButtons.Left Then mMouseDownLocation = e.Location
    End Sub
    Private Sub WizardSteps_MouseMove(sender As Object, e As MouseEventArgs) Handles CreatePasswordStep.MouseMove,
        InstallDatabaseStep.MouseMove,
        BuildingDatabaseStep.MouseMove,
        DatabaseBuildSuccess.MouseMove,
        DatabaseBuildFailure.MouseMove
        If e.Button = MouseButtons.Left Then
            Left += e.Location.X - mMouseDownLocation.X
            Top += e.Location.Y - mMouseDownLocation.Y
        End If
    End Sub

    Private Sub ChangeLanguageButton_Click(sender As Object, e As EventArgs) Handles ChangeLanguageButton.Click
        Using localeConfigForm As New SelectLanguageForm(ctlLogin.PseudoLocalization)
            localeConfigForm.StartPosition = FormStartPosition.CenterParent
            localeConfigForm.ShowInTaskbar = False

            Dim configOptions = Options.Instance
            If localeConfigForm.ShowDialog() = DialogResult.OK AndAlso localeConfigForm.NewLocale IsNot Nothing AndAlso localeConfigForm.NewLocale <> configOptions.CurrentLocale Then

                Dim password As BluePrism.Common.Security.SafeString = CreatePasswordTextBox.SecurePassword
                Dim repeatPassword As BluePrism.Common.Security.SafeString = RepeatPasswordTextBox.SecurePassword
                Dim newLocale As Globalization.CultureInfo = Nothing
                Dim newLocaleFormat As Globalization.CultureInfo = Nothing

                If localeConfigForm.NewLocale = configOptions.SystemLocale Then
                    newLocale = New Globalization.CultureInfo(configOptions.SystemLocale)
                    newLocaleFormat = New Globalization.CultureInfo(configOptions.SystemLocaleFormat)
                Else
                    newLocale = New Globalization.CultureInfo(localeConfigForm.NewLocale)
                    newLocaleFormat = New Globalization.CultureInfo(localeConfigForm.NewLocale)
                End If

                Threading.Thread.CurrentThread.CurrentUICulture = newLocale
                Threading.Thread.CurrentThread.CurrentCulture = newLocaleFormat
                configOptions.CurrentLocale = localeConfigForm.NewLocale

                frmApplication.UpdateGlobalAppProperties()

                Me.Controls.Clear()
                InitializeComponent()
                WelcomeWizard_Load(sender, e)

                CreatePasswordTextBox.SecurePassword = password
                RepeatPasswordTextBox.SecurePassword = repeatPassword

                BluePrism.AMI.clsAMI.StaticBuildTypes()
                BluePrism.AutomateProcessCore.clsProcessDataTypes.RebuildTypeInfo()
                BluePrism.ApplicationManager.WindowSpy.clsWindowSpy.InvalidateInfoCache()
            End If

        End Using
    End Sub

    Private Sub ViewHelpFlatStyleStyledButton_Click(sender As Object, e As EventArgs) Handles ViewHelpFlatStyleStyledButton.Click
        Try
            OpenHelpFile(Me, HelpTrialLearningHtml)
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
        End Try
    End Sub
End Class
