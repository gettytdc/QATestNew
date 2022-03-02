<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WelcomeWizard
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WelcomeWizard))
        Me.BackgroundDatabaseInstaller = New System.ComponentModel.BackgroundWorker()
        Me.BorderPanel = New System.Windows.Forms.Panel()
        Me.WizardSteps = New AutomateControls.SwitchPanel()
        Me.CreatePasswordStep = New System.Windows.Forms.TabPage()
        Me.CreatePasswordWarningContainer = New System.Windows.Forms.TableLayoutPanel()
        Me.WarningIconLabel = New System.Windows.Forms.Label()
        Me.CreatePasswordWarningLabel = New System.Windows.Forms.Label()
        Me.ChangeLanguageButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.CreatePasswordCloseButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.RepeatPasswordWarningPictureBox = New System.Windows.Forms.PictureBox()
        Me.CreatePasswordWarningPictureBox = New System.Windows.Forms.PictureBox()
        Me.CreatePasswordTextBox = New AutomateControls.SecurePasswordTextBox()
        Me.PasswordRequiementsDetailsLabel = New System.Windows.Forms.Label()
        Me.PasswordRequirementsLabel = New System.Windows.Forms.Label()
        Me.CreatePasswordSubTitle = New System.Windows.Forms.Label()
        Me.RepeatPasswordLabel = New System.Windows.Forms.Label()
        Me.RepeatPasswordTextBox = New AutomateControls.SecurePasswordTextBox()
        Me.CreateUsernameLabel = New System.Windows.Forms.Label()
        Me.CreateUsernameTextBox = New AutomateControls.Textboxes.StyledTextBox()
        Me.CreatePasswordTitle = New AutomateControls.TitleBar()
        Me.CreatePasswordNextButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.CreatePasswordPictureBox = New System.Windows.Forms.PictureBox()
        Me.CreatePasswordLabel = New System.Windows.Forms.Label()
        Me.InstallDatabaseStep = New System.Windows.Forms.TabPage()
        Me.InstallDatabaseCloseButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.ClickYesLabel = New System.Windows.Forms.Label()
        Me.InstallDatabasePictureBox = New System.Windows.Forms.PictureBox()
        Me.InstallDatabaseTitle = New AutomateControls.TitleBar()
        Me.InstallDatabaseNextButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.InstallDatabaseSubtitle = New System.Windows.Forms.Label()
        Me.BuildingDatabaseStep = New System.Windows.Forms.TabPage()
        Me.UpgradeDatabasePictureBox = New System.Windows.Forms.PictureBox()
        Me.BuildingDatabaseProgressBar = New AutomateControls.ColorProgressBar()
        Me.BuildingDatabasePictureBox = New System.Windows.Forms.PictureBox()
        Me.BuildingDatabaseSubTitle = New System.Windows.Forms.Label()
        Me.BuildingDatabaseTitle = New AutomateControls.TitleBar()
        Me.BuildingDatabaseStatusLabel = New System.Windows.Forms.Label()
        Me.BuildingDatabaseLabel = New System.Windows.Forms.Label()
        Me.DatabaseBuildSuccess = New System.Windows.Forms.TabPage()
        Me.ViewHelpFlatStyleStyledButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.DatabaseSuccessCloseButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.DatabaseSuccessBodyLinkLabel = New System.Windows.Forms.LinkLabel()
        Me.DatabaseSuccessImage = New System.Windows.Forms.PictureBox()
        Me.DatabaseSuccessNextButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.DatabaseSuccessTitleLabel = New System.Windows.Forms.Label()
        Me.DatabaseBuildFailure = New System.Windows.Forms.TabPage()
        Me.DatabaseFailureErrorButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.ExitFailureButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.DatabaseFailureCloseButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.DatabaseFailureBodyLinkLabel = New System.Windows.Forms.LinkLabel()
        Me.DatabaseFailureImage = New System.Windows.Forms.PictureBox()
        Me.DatabaseFailureTitleLabel = New System.Windows.Forms.Label()
        Me.DatabaseUpgradeSuccess = New System.Windows.Forms.TabPage()
        Me.UpgradeCloseButton = New AutomateControls.Buttons.FlatStyleStyledButton()
        Me.UpgradeFinishButton = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.UpgradeSuccessfulImage = New System.Windows.Forms.PictureBox()
        Me.UpgradedSuccessfullyLabel = New System.Windows.Forms.Label()
        Me.UpgradedTitleLabel = New System.Windows.Forms.Label()
        Me.BorderPanel.SuspendLayout()
        Me.WizardSteps.SuspendLayout()
        Me.CreatePasswordStep.SuspendLayout()
        Me.CreatePasswordWarningContainer.SuspendLayout()
        CType(Me.RepeatPasswordWarningPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CreatePasswordWarningPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CreatePasswordPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.InstallDatabaseStep.SuspendLayout()
        CType(Me.InstallDatabasePictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.BuildingDatabaseStep.SuspendLayout()
        CType(Me.UpgradeDatabasePictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.BuildingDatabasePictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.DatabaseBuildSuccess.SuspendLayout()
        CType(Me.DatabaseSuccessImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.DatabaseBuildFailure.SuspendLayout()
        CType(Me.DatabaseFailureImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.DatabaseUpgradeSuccess.SuspendLayout()
        CType(Me.UpgradeSuccessfulImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'BackgroundDatabaseInstaller
        '
        Me.BackgroundDatabaseInstaller.WorkerReportsProgress = True
        '
        'BorderPanel
        '
        Me.BorderPanel.BackColor = System.Drawing.Color.White
        Me.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.BorderPanel.Controls.Add(Me.WizardSteps)
        resources.ApplyResources(Me.BorderPanel, "BorderPanel")
        Me.BorderPanel.Name = "BorderPanel"
        '
        'WizardSteps
        '
        Me.WizardSteps.Controls.Add(Me.CreatePasswordStep)
        Me.WizardSteps.Controls.Add(Me.InstallDatabaseStep)
        Me.WizardSteps.Controls.Add(Me.BuildingDatabaseStep)
        Me.WizardSteps.Controls.Add(Me.DatabaseBuildSuccess)
        Me.WizardSteps.Controls.Add(Me.DatabaseBuildFailure)
        Me.WizardSteps.Controls.Add(Me.DatabaseUpgradeSuccess)
        Me.WizardSteps.DisableArrowKeys = True
        resources.ApplyResources(Me.WizardSteps, "WizardSteps")
        Me.WizardSteps.Name = "WizardSteps"
        Me.WizardSteps.SelectedIndex = 0
        Me.WizardSteps.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
        '
        'CreatePasswordStep
        '
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordWarningContainer)
        Me.CreatePasswordStep.Controls.Add(Me.ChangeLanguageButton)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordCloseButton)
        Me.CreatePasswordStep.Controls.Add(Me.RepeatPasswordWarningPictureBox)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordWarningPictureBox)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordTextBox)
        Me.CreatePasswordStep.Controls.Add(Me.PasswordRequiementsDetailsLabel)
        Me.CreatePasswordStep.Controls.Add(Me.PasswordRequirementsLabel)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordSubTitle)
        Me.CreatePasswordStep.Controls.Add(Me.RepeatPasswordLabel)
        Me.CreatePasswordStep.Controls.Add(Me.RepeatPasswordTextBox)
        Me.CreatePasswordStep.Controls.Add(Me.CreateUsernameLabel)
        Me.CreatePasswordStep.Controls.Add(Me.CreateUsernameTextBox)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordTitle)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordNextButton)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordPictureBox)
        Me.CreatePasswordStep.Controls.Add(Me.CreatePasswordLabel)
        resources.ApplyResources(Me.CreatePasswordStep, "CreatePasswordStep")
        Me.CreatePasswordStep.Name = "CreatePasswordStep"
        Me.CreatePasswordStep.UseVisualStyleBackColor = True
        '
        'CreatePasswordWarningContainer
        '
        Me.CreatePasswordWarningContainer.BackColor = System.Drawing.Color.FromArgb(CType(CType(203, Byte), Integer), CType(CType(98, Byte), Integer), CType(CType(0, Byte), Integer))
        resources.ApplyResources(Me.CreatePasswordWarningContainer, "CreatePasswordWarningContainer")
        Me.CreatePasswordWarningContainer.Controls.Add(Me.WarningIconLabel, 0, 0)
        Me.CreatePasswordWarningContainer.Controls.Add(Me.CreatePasswordWarningLabel, 1, 0)
        Me.CreatePasswordWarningContainer.Name = "CreatePasswordWarningContainer"
        '
        'WarningIconLabel
        '
        resources.ApplyResources(Me.WarningIconLabel, "WarningIconLabel")
        Me.WarningIconLabel.ForeColor = System.Drawing.Color.White
        Me.WarningIconLabel.Image = Global.AutomateUI.My.Resources.Resources.alert
        Me.WarningIconLabel.Name = "WarningIconLabel"
        '
        'CreatePasswordWarningLabel
        '
        resources.ApplyResources(Me.CreatePasswordWarningLabel, "CreatePasswordWarningLabel")
        Me.CreatePasswordWarningLabel.ForeColor = System.Drawing.Color.White
        Me.CreatePasswordWarningLabel.Name = "CreatePasswordWarningLabel"
        '
        'ChangeLanguageButton
        '
        Me.ChangeLanguageButton.BackColor = System.Drawing.Color.White
        Me.ChangeLanguageButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.ChangeLanguageButton.FlatAppearance.BorderSize = 0
        Me.ChangeLanguageButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.ChangeLanguageButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.ChangeLanguageButton, "ChangeLanguageButton")
        Me.ChangeLanguageButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.ChangeLanguageButton.Image = Global.AutomateUI.My.Resources.Resources.bubble
        Me.ChangeLanguageButton.Name = "ChangeLanguageButton"
        Me.ChangeLanguageButton.UseVisualStyleBackColor = False
        '
        'CreatePasswordCloseButton
        '
        resources.ApplyResources(Me.CreatePasswordCloseButton, "CreatePasswordCloseButton")
        Me.CreatePasswordCloseButton.BackColor = System.Drawing.Color.White
        Me.CreatePasswordCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.CreatePasswordCloseButton.FlatAppearance.BorderSize = 0
        Me.CreatePasswordCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.CreatePasswordCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.CreatePasswordCloseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.CreatePasswordCloseButton.Name = "CreatePasswordCloseButton"
        Me.CreatePasswordCloseButton.UseVisualStyleBackColor = False
        '
        'RepeatPasswordWarningPictureBox
        '
        resources.ApplyResources(Me.RepeatPasswordWarningPictureBox, "RepeatPasswordWarningPictureBox")
        Me.RepeatPasswordWarningPictureBox.Name = "RepeatPasswordWarningPictureBox"
        Me.RepeatPasswordWarningPictureBox.TabStop = False
        '
        'CreatePasswordWarningPictureBox
        '
        resources.ApplyResources(Me.CreatePasswordWarningPictureBox, "CreatePasswordWarningPictureBox")
        Me.CreatePasswordWarningPictureBox.Name = "CreatePasswordWarningPictureBox"
        Me.CreatePasswordWarningPictureBox.TabStop = False
        '
        'CreatePasswordTextBox
        '
        Me.CreatePasswordTextBox.BackColor = System.Drawing.Color.White
        Me.CreatePasswordTextBox.BorderColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        resources.ApplyResources(Me.CreatePasswordTextBox, "CreatePasswordTextBox")
        Me.CreatePasswordTextBox.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.CreatePasswordTextBox.Name = "CreatePasswordTextBox"
        '
        'PasswordRequiementsDetailsLabel
        '
        resources.ApplyResources(Me.PasswordRequiementsDetailsLabel, "PasswordRequiementsDetailsLabel")
        Me.PasswordRequiementsDetailsLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.PasswordRequiementsDetailsLabel.Name = "PasswordRequiementsDetailsLabel"
        '
        'PasswordRequirementsLabel
        '
        resources.ApplyResources(Me.PasswordRequirementsLabel, "PasswordRequirementsLabel")
        Me.PasswordRequirementsLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.PasswordRequirementsLabel.Name = "PasswordRequirementsLabel"
        '
        'CreatePasswordSubTitle
        '
        resources.ApplyResources(Me.CreatePasswordSubTitle, "CreatePasswordSubTitle")
        Me.CreatePasswordSubTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.CreatePasswordSubTitle.Name = "CreatePasswordSubTitle"
        '
        'RepeatPasswordLabel
        '
        resources.ApplyResources(Me.RepeatPasswordLabel, "RepeatPasswordLabel")
        Me.RepeatPasswordLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.RepeatPasswordLabel.Name = "RepeatPasswordLabel"
        '
        'RepeatPasswordTextBox
        '
        Me.RepeatPasswordTextBox.BackColor = System.Drawing.Color.White
        Me.RepeatPasswordTextBox.BorderColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        resources.ApplyResources(Me.RepeatPasswordTextBox, "RepeatPasswordTextBox")
        Me.RepeatPasswordTextBox.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.RepeatPasswordTextBox.Name = "RepeatPasswordTextBox"
        '
        'CreateUsernameLabel
        '
        resources.ApplyResources(Me.CreateUsernameLabel, "CreateUsernameLabel")
        Me.CreateUsernameLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.CreateUsernameLabel.Name = "CreateUsernameLabel"
        '
        'CreateUsernameTextBox
        '
        Me.CreateUsernameTextBox.BackColor = System.Drawing.SystemColors.ControlLight
        Me.CreateUsernameTextBox.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.CreateUsernameTextBox, "CreateUsernameTextBox")
        Me.CreateUsernameTextBox.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.CreateUsernameTextBox.Name = "CreateUsernameTextBox"
        Me.CreateUsernameTextBox.ShortcutsEnabled = False
        '
        'CreatePasswordTitle
        '
        resources.ApplyResources(Me.CreatePasswordTitle, "CreatePasswordTitle")
        Me.CreatePasswordTitle.BackColor = System.Drawing.Color.White
        Me.CreatePasswordTitle.Name = "CreatePasswordTitle"
        Me.CreatePasswordTitle.TabStop = False
        Me.CreatePasswordTitle.TitleColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.CreatePasswordTitle.TitlePosition = New System.Drawing.Point(0, 0)
        '
        'CreatePasswordNextButton
        '
        resources.ApplyResources(Me.CreatePasswordNextButton, "CreatePasswordNextButton")
        Me.CreatePasswordNextButton.BackColor = System.Drawing.Color.White
        Me.CreatePasswordNextButton.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.CreatePasswordNextButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.CreatePasswordNextButton.Name = "CreatePasswordNextButton"
        Me.CreatePasswordNextButton.UseVisualStyleBackColor = False
        '
        'CreatePasswordPictureBox
        '
        resources.ApplyResources(Me.CreatePasswordPictureBox, "CreatePasswordPictureBox")
        Me.CreatePasswordPictureBox.Name = "CreatePasswordPictureBox"
        Me.CreatePasswordPictureBox.TabStop = False
        '
        'CreatePasswordLabel
        '
        resources.ApplyResources(Me.CreatePasswordLabel, "CreatePasswordLabel")
        Me.CreatePasswordLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.CreatePasswordLabel.Name = "CreatePasswordLabel"
        '
        'InstallDatabaseStep
        '
        Me.InstallDatabaseStep.Controls.Add(Me.InstallDatabaseCloseButton)
        Me.InstallDatabaseStep.Controls.Add(Me.ClickYesLabel)
        Me.InstallDatabaseStep.Controls.Add(Me.InstallDatabasePictureBox)
        Me.InstallDatabaseStep.Controls.Add(Me.InstallDatabaseTitle)
        Me.InstallDatabaseStep.Controls.Add(Me.InstallDatabaseNextButton)
        Me.InstallDatabaseStep.Controls.Add(Me.InstallDatabaseSubtitle)
        resources.ApplyResources(Me.InstallDatabaseStep, "InstallDatabaseStep")
        Me.InstallDatabaseStep.Name = "InstallDatabaseStep"
        Me.InstallDatabaseStep.UseVisualStyleBackColor = True
        '
        'InstallDatabaseCloseButton
        '
        resources.ApplyResources(Me.InstallDatabaseCloseButton, "InstallDatabaseCloseButton")
        Me.InstallDatabaseCloseButton.BackColor = System.Drawing.Color.White
        Me.InstallDatabaseCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.InstallDatabaseCloseButton.FlatAppearance.BorderSize = 0
        Me.InstallDatabaseCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.InstallDatabaseCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.InstallDatabaseCloseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.InstallDatabaseCloseButton.Name = "InstallDatabaseCloseButton"
        Me.InstallDatabaseCloseButton.UseVisualStyleBackColor = False
        '
        'ClickYesLabel
        '
        resources.ApplyResources(Me.ClickYesLabel, "ClickYesLabel")
        Me.ClickYesLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.ClickYesLabel.Name = "ClickYesLabel"
        '
        'InstallDatabasePictureBox
        '
        resources.ApplyResources(Me.InstallDatabasePictureBox, "InstallDatabasePictureBox")
        Me.InstallDatabasePictureBox.Name = "InstallDatabasePictureBox"
        Me.InstallDatabasePictureBox.TabStop = False
        '
        'InstallDatabaseTitle
        '
        resources.ApplyResources(Me.InstallDatabaseTitle, "InstallDatabaseTitle")
        Me.InstallDatabaseTitle.BackColor = System.Drawing.Color.White
        Me.InstallDatabaseTitle.Name = "InstallDatabaseTitle"
        Me.InstallDatabaseTitle.TabStop = False
        Me.InstallDatabaseTitle.TitleColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.InstallDatabaseTitle.TitlePosition = New System.Drawing.Point(0, 0)
        '
        'InstallDatabaseNextButton
        '
        resources.ApplyResources(Me.InstallDatabaseNextButton, "InstallDatabaseNextButton")
        Me.InstallDatabaseNextButton.BackColor = System.Drawing.Color.White
        Me.InstallDatabaseNextButton.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.InstallDatabaseNextButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.InstallDatabaseNextButton.Name = "InstallDatabaseNextButton"
        Me.InstallDatabaseNextButton.UseVisualStyleBackColor = False
        '
        'InstallDatabaseSubtitle
        '
        resources.ApplyResources(Me.InstallDatabaseSubtitle, "InstallDatabaseSubtitle")
        Me.InstallDatabaseSubtitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.InstallDatabaseSubtitle.Name = "InstallDatabaseSubtitle"
        '
        'BuildingDatabaseStep
        '
        Me.BuildingDatabaseStep.Controls.Add(Me.UpgradeDatabasePictureBox)
        Me.BuildingDatabaseStep.Controls.Add(Me.BuildingDatabaseProgressBar)
        Me.BuildingDatabaseStep.Controls.Add(Me.BuildingDatabasePictureBox)
        Me.BuildingDatabaseStep.Controls.Add(Me.BuildingDatabaseSubTitle)
        Me.BuildingDatabaseStep.Controls.Add(Me.BuildingDatabaseTitle)
        Me.BuildingDatabaseStep.Controls.Add(Me.BuildingDatabaseStatusLabel)
        Me.BuildingDatabaseStep.Controls.Add(Me.BuildingDatabaseLabel)
        resources.ApplyResources(Me.BuildingDatabaseStep, "BuildingDatabaseStep")
        Me.BuildingDatabaseStep.Name = "BuildingDatabaseStep"
        Me.BuildingDatabaseStep.UseVisualStyleBackColor = True
        '
        'UpgradeDatabasePictureBox
        '
        resources.ApplyResources(Me.UpgradeDatabasePictureBox, "UpgradeDatabasePictureBox")
        Me.UpgradeDatabasePictureBox.Name = "UpgradeDatabasePictureBox"
        Me.UpgradeDatabasePictureBox.TabStop = False
        '
        'BuildingDatabaseProgressBar
        '
        Me.BuildingDatabaseProgressBar.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        resources.ApplyResources(Me.BuildingDatabaseProgressBar, "BuildingDatabaseProgressBar")
        Me.BuildingDatabaseProgressBar.Name = "BuildingDatabaseProgressBar"
        Me.BuildingDatabaseProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        '
        'BuildingDatabasePictureBox
        '
        resources.ApplyResources(Me.BuildingDatabasePictureBox, "BuildingDatabasePictureBox")
        Me.BuildingDatabasePictureBox.Name = "BuildingDatabasePictureBox"
        Me.BuildingDatabasePictureBox.TabStop = False
        '
        'BuildingDatabaseSubTitle
        '
        resources.ApplyResources(Me.BuildingDatabaseSubTitle, "BuildingDatabaseSubTitle")
        Me.BuildingDatabaseSubTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.BuildingDatabaseSubTitle.Name = "BuildingDatabaseSubTitle"
        '
        'BuildingDatabaseTitle
        '
        resources.ApplyResources(Me.BuildingDatabaseTitle, "BuildingDatabaseTitle")
        Me.BuildingDatabaseTitle.BackColor = System.Drawing.Color.White
        Me.BuildingDatabaseTitle.Name = "BuildingDatabaseTitle"
        Me.BuildingDatabaseTitle.TabStop = False
        Me.BuildingDatabaseTitle.TitleColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.BuildingDatabaseTitle.TitlePosition = New System.Drawing.Point(0, 0)
        '
        'BuildingDatabaseStatusLabel
        '
        resources.ApplyResources(Me.BuildingDatabaseStatusLabel, "BuildingDatabaseStatusLabel")
        Me.BuildingDatabaseStatusLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.BuildingDatabaseStatusLabel.Name = "BuildingDatabaseStatusLabel"
        '
        'BuildingDatabaseLabel
        '
        resources.ApplyResources(Me.BuildingDatabaseLabel, "BuildingDatabaseLabel")
        Me.BuildingDatabaseLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.BuildingDatabaseLabel.Name = "BuildingDatabaseLabel"
        '
        'DatabaseBuildSuccess
        '
        Me.DatabaseBuildSuccess.Controls.Add(Me.ViewHelpFlatStyleStyledButton)
        Me.DatabaseBuildSuccess.Controls.Add(Me.DatabaseSuccessCloseButton)
        Me.DatabaseBuildSuccess.Controls.Add(Me.DatabaseSuccessBodyLinkLabel)
        Me.DatabaseBuildSuccess.Controls.Add(Me.DatabaseSuccessImage)
        Me.DatabaseBuildSuccess.Controls.Add(Me.DatabaseSuccessNextButton)
        Me.DatabaseBuildSuccess.Controls.Add(Me.DatabaseSuccessTitleLabel)
        resources.ApplyResources(Me.DatabaseBuildSuccess, "DatabaseBuildSuccess")
        Me.DatabaseBuildSuccess.Name = "DatabaseBuildSuccess"
        Me.DatabaseBuildSuccess.UseVisualStyleBackColor = True
        '
        'ViewHelpFlatStyleStyledButton
        '
        resources.ApplyResources(Me.ViewHelpFlatStyleStyledButton, "ViewHelpFlatStyleStyledButton")
        Me.ViewHelpFlatStyleStyledButton.BackColor = System.Drawing.Color.White
        Me.ViewHelpFlatStyleStyledButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.ViewHelpFlatStyleStyledButton.FlatAppearance.BorderSize = 0
        Me.ViewHelpFlatStyleStyledButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.ViewHelpFlatStyleStyledButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.ViewHelpFlatStyleStyledButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.ViewHelpFlatStyleStyledButton.Name = "ViewHelpFlatStyleStyledButton"
        Me.ViewHelpFlatStyleStyledButton.UseVisualStyleBackColor = False
        '
        'DatabaseSuccessCloseButton
        '
        resources.ApplyResources(Me.DatabaseSuccessCloseButton, "DatabaseSuccessCloseButton")
        Me.DatabaseSuccessCloseButton.BackColor = System.Drawing.Color.White
        Me.DatabaseSuccessCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.DatabaseSuccessCloseButton.FlatAppearance.BorderSize = 0
        Me.DatabaseSuccessCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.DatabaseSuccessCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.DatabaseSuccessCloseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.DatabaseSuccessCloseButton.Name = "DatabaseSuccessCloseButton"
        Me.DatabaseSuccessCloseButton.UseVisualStyleBackColor = False
        '
        'DatabaseSuccessBodyLinkLabel
        '
        resources.ApplyResources(Me.DatabaseSuccessBodyLinkLabel, "DatabaseSuccessBodyLinkLabel")
        Me.DatabaseSuccessBodyLinkLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.DatabaseSuccessBodyLinkLabel.LinkColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.DatabaseSuccessBodyLinkLabel.Name = "DatabaseSuccessBodyLinkLabel"
        Me.DatabaseSuccessBodyLinkLabel.TabStop = True
        Me.DatabaseSuccessBodyLinkLabel.UseCompatibleTextRendering = True
        '
        'DatabaseSuccessImage
        '
        resources.ApplyResources(Me.DatabaseSuccessImage, "DatabaseSuccessImage")
        Me.DatabaseSuccessImage.Name = "DatabaseSuccessImage"
        Me.DatabaseSuccessImage.TabStop = False
        '
        'DatabaseSuccessNextButton
        '
        resources.ApplyResources(Me.DatabaseSuccessNextButton, "DatabaseSuccessNextButton")
        Me.DatabaseSuccessNextButton.BackColor = System.Drawing.Color.White
        Me.DatabaseSuccessNextButton.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.DatabaseSuccessNextButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.DatabaseSuccessNextButton.Name = "DatabaseSuccessNextButton"
        Me.DatabaseSuccessNextButton.UseVisualStyleBackColor = False
        '
        'DatabaseSuccessTitleLabel
        '
        resources.ApplyResources(Me.DatabaseSuccessTitleLabel, "DatabaseSuccessTitleLabel")
        Me.DatabaseSuccessTitleLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.DatabaseSuccessTitleLabel.Name = "DatabaseSuccessTitleLabel"
        '
        'DatabaseBuildFailure
        '
        Me.DatabaseBuildFailure.Controls.Add(Me.DatabaseFailureErrorButton)
        Me.DatabaseBuildFailure.Controls.Add(Me.ExitFailureButton)
        Me.DatabaseBuildFailure.Controls.Add(Me.DatabaseFailureCloseButton)
        Me.DatabaseBuildFailure.Controls.Add(Me.DatabaseFailureBodyLinkLabel)
        Me.DatabaseBuildFailure.Controls.Add(Me.DatabaseFailureImage)
        Me.DatabaseBuildFailure.Controls.Add(Me.DatabaseFailureTitleLabel)
        resources.ApplyResources(Me.DatabaseBuildFailure, "DatabaseBuildFailure")
        Me.DatabaseBuildFailure.Name = "DatabaseBuildFailure"
        Me.DatabaseBuildFailure.UseVisualStyleBackColor = True
        '
        'DatabaseFailureErrorButton
        '
        resources.ApplyResources(Me.DatabaseFailureErrorButton, "DatabaseFailureErrorButton")
        Me.DatabaseFailureErrorButton.BackColor = System.Drawing.Color.White
        Me.DatabaseFailureErrorButton.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.DatabaseFailureErrorButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.DatabaseFailureErrorButton.Name = "DatabaseFailureErrorButton"
        Me.DatabaseFailureErrorButton.UseVisualStyleBackColor = False
        '
        'ExitFailureButton
        '
        resources.ApplyResources(Me.ExitFailureButton, "ExitFailureButton")
        Me.ExitFailureButton.BackColor = System.Drawing.Color.White
        Me.ExitFailureButton.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.ExitFailureButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.ExitFailureButton.Name = "ExitFailureButton"
        Me.ExitFailureButton.UseVisualStyleBackColor = False
        '
        'DatabaseFailureCloseButton
        '
        resources.ApplyResources(Me.DatabaseFailureCloseButton, "DatabaseFailureCloseButton")
        Me.DatabaseFailureCloseButton.BackColor = System.Drawing.Color.White
        Me.DatabaseFailureCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.DatabaseFailureCloseButton.FlatAppearance.BorderSize = 0
        Me.DatabaseFailureCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.DatabaseFailureCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.DatabaseFailureCloseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.DatabaseFailureCloseButton.Name = "DatabaseFailureCloseButton"
        Me.DatabaseFailureCloseButton.UseVisualStyleBackColor = False
        '
        'DatabaseFailureBodyLinkLabel
        '
        resources.ApplyResources(Me.DatabaseFailureBodyLinkLabel, "DatabaseFailureBodyLinkLabel")
        Me.DatabaseFailureBodyLinkLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.DatabaseFailureBodyLinkLabel.LinkColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.DatabaseFailureBodyLinkLabel.Name = "DatabaseFailureBodyLinkLabel"
        '
        'DatabaseFailureImage
        '
        resources.ApplyResources(Me.DatabaseFailureImage, "DatabaseFailureImage")
        Me.DatabaseFailureImage.Name = "DatabaseFailureImage"
        Me.DatabaseFailureImage.TabStop = False
        '
        'DatabaseFailureTitleLabel
        '
        resources.ApplyResources(Me.DatabaseFailureTitleLabel, "DatabaseFailureTitleLabel")
        Me.DatabaseFailureTitleLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.DatabaseFailureTitleLabel.Name = "DatabaseFailureTitleLabel"
        '
        'DatabaseUpgradeSuccess
        '
        Me.DatabaseUpgradeSuccess.Controls.Add(Me.UpgradeCloseButton)
        Me.DatabaseUpgradeSuccess.Controls.Add(Me.UpgradeFinishButton)
        Me.DatabaseUpgradeSuccess.Controls.Add(Me.UpgradeSuccessfulImage)
        Me.DatabaseUpgradeSuccess.Controls.Add(Me.UpgradedSuccessfullyLabel)
        Me.DatabaseUpgradeSuccess.Controls.Add(Me.UpgradedTitleLabel)
        resources.ApplyResources(Me.DatabaseUpgradeSuccess, "DatabaseUpgradeSuccess")
        Me.DatabaseUpgradeSuccess.Name = "DatabaseUpgradeSuccess"
        Me.DatabaseUpgradeSuccess.UseVisualStyleBackColor = True
        '
        'UpgradeCloseButton
        '
        resources.ApplyResources(Me.UpgradeCloseButton, "UpgradeCloseButton")
        Me.UpgradeCloseButton.BackColor = System.Drawing.Color.White
        Me.UpgradeCloseButton.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.UpgradeCloseButton.FlatAppearance.BorderSize = 0
        Me.UpgradeCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White
        Me.UpgradeCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White
        Me.UpgradeCloseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.UpgradeCloseButton.Name = "UpgradeCloseButton"
        Me.UpgradeCloseButton.UseVisualStyleBackColor = False
        '
        'UpgradeFinishButton
        '
        resources.ApplyResources(Me.UpgradeFinishButton, "UpgradeFinishButton")
        Me.UpgradeFinishButton.BackColor = System.Drawing.Color.White
        Me.UpgradeFinishButton.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.UpgradeFinishButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.UpgradeFinishButton.Name = "UpgradeFinishButton"
        Me.UpgradeFinishButton.UseVisualStyleBackColor = False
        '
        'UpgradeSuccessfulImage
        '
        resources.ApplyResources(Me.UpgradeSuccessfulImage, "UpgradeSuccessfulImage")
        Me.UpgradeSuccessfulImage.Name = "UpgradeSuccessfulImage"
        Me.UpgradeSuccessfulImage.TabStop = False
        '
        'UpgradedSuccessfullyLabel
        '
        resources.ApplyResources(Me.UpgradedSuccessfullyLabel, "UpgradedSuccessfullyLabel")
        Me.UpgradedSuccessfullyLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.UpgradedSuccessfullyLabel.Name = "UpgradedSuccessfullyLabel"
        '
        'UpgradedTitleLabel
        '
        resources.ApplyResources(Me.UpgradedTitleLabel, "UpgradedTitleLabel")
        Me.UpgradedTitleLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.UpgradedTitleLabel.Name = "UpgradedTitleLabel"
        '
        'WelcomeWizard
        '
        Me.AcceptButton = Me.CreatePasswordNextButton
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.BorderPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "WelcomeWizard"
        Me.BorderPanel.ResumeLayout(False)
        Me.WizardSteps.ResumeLayout(False)
        Me.CreatePasswordStep.ResumeLayout(False)
        Me.CreatePasswordStep.PerformLayout()
        Me.CreatePasswordWarningContainer.ResumeLayout(False)
        Me.CreatePasswordWarningContainer.PerformLayout()
        CType(Me.RepeatPasswordWarningPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CreatePasswordWarningPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CreatePasswordPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.InstallDatabaseStep.ResumeLayout(False)
        Me.InstallDatabaseStep.PerformLayout()
        CType(Me.InstallDatabasePictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.BuildingDatabaseStep.ResumeLayout(False)
        Me.BuildingDatabaseStep.PerformLayout()
        CType(Me.UpgradeDatabasePictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.BuildingDatabasePictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.DatabaseBuildSuccess.ResumeLayout(False)
        Me.DatabaseBuildSuccess.PerformLayout()
        CType(Me.DatabaseSuccessImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.DatabaseBuildFailure.ResumeLayout(False)
        Me.DatabaseBuildFailure.PerformLayout()
        CType(Me.DatabaseFailureImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.DatabaseUpgradeSuccess.ResumeLayout(False)
        Me.DatabaseUpgradeSuccess.PerformLayout()
        CType(Me.UpgradeSuccessfulImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents InstallDatabaseNextButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents InstallDatabaseSubtitle As Label
    Friend WithEvents InstallDatabaseTitle As AutomateControls.TitleBar
    Friend WithEvents WizardSteps As AutomateControls.SwitchPanel
    Friend WithEvents InstallDatabaseStep As TabPage
    Friend WithEvents BuildingDatabaseStep As TabPage
    Friend WithEvents BuildingDatabaseProgressBar As AutomateControls.ColorProgressBar
    Friend WithEvents BackgroundDatabaseInstaller As BackgroundWorker
    Friend WithEvents BuildingDatabaseTitle As AutomateControls.TitleBar
    Friend WithEvents BuildingDatabaseSubTitle As Label
    Friend WithEvents BorderPanel As Panel
    Friend WithEvents BuildingDatabasePictureBox As PictureBox
    Friend WithEvents InstallDatabasePictureBox As PictureBox
    Friend WithEvents ClickYesLabel As Label
    Friend WithEvents BuildingDatabaseLabel As Label
    Friend WithEvents BuildingDatabaseStatusLabel As Label
    Friend WithEvents CreatePasswordStep As TabPage
    Friend WithEvents CreatePasswordTitle As AutomateControls.TitleBar
    Friend WithEvents CreatePasswordNextButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents CreatePasswordSubTitle As Label
    Friend WithEvents CreateUsernameLabel As Label
    Friend WithEvents CreateUsernameTextBox As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents RepeatPasswordLabel As Label
    Friend WithEvents CreatePasswordLabel As Label
    Friend WithEvents RepeatPasswordTextBox As AutomateControls.SecurePasswordTextBox
    Friend WithEvents CreatePasswordTextBox As AutomateControls.SecurePasswordTextBox
    Friend WithEvents PasswordRequiementsDetailsLabel As Label
    Friend WithEvents PasswordRequirementsLabel As Label
    Friend WithEvents CreatePasswordPictureBox As PictureBox
    Friend WithEvents CreatePasswordWarningLabel As Label
    Friend WithEvents CreatePasswordWarningPictureBox As PictureBox
    Friend WithEvents RepeatPasswordWarningPictureBox As PictureBox
    Friend WithEvents DatabaseBuildSuccess As TabPage
    Friend WithEvents DatabaseSuccessTitleLabel As Label
    Friend WithEvents DatabaseBuildFailure As TabPage
    Friend WithEvents DatabaseSuccessImage As PictureBox
    Friend WithEvents DatabaseSuccessNextButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents DatabaseFailureImage As PictureBox
    Friend WithEvents DatabaseFailureTitleLabel As Label
    Friend WithEvents DatabaseSuccessBodyLinkLabel As LinkLabel
    Friend WithEvents DatabaseFailureBodyLinkLabel As LinkLabel
    Friend WithEvents CreatePasswordCloseButton As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents InstallDatabaseCloseButton As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents DatabaseSuccessCloseButton As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents DatabaseFailureCloseButton As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents ChangeLanguageButton As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents UpgradeDatabasePictureBox As PictureBox
    Friend WithEvents DatabaseUpgradeSuccess As TabPage
    Friend WithEvents UpgradeSuccessfulImage As PictureBox
    Friend WithEvents UpgradedSuccessfullyLabel As Label
    Friend WithEvents UpgradedTitleLabel As Label
    Friend WithEvents UpgradeFinishButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents UpgradeCloseButton As AutomateControls.Buttons.FlatStyleStyledButton
    Friend WithEvents CreatePasswordWarningContainer As TableLayoutPanel
    Friend WithEvents WarningIconLabel As Label
    Friend WithEvents ExitFailureButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents DatabaseFailureErrorButton As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ViewHelpFlatStyleStyledButton As AutomateControls.Buttons.FlatStyleStyledButton
End Class
