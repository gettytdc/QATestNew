Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports BluePrism.Core.Utility
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Server.Domain.Models

' ReSharper disable once CheckNamespace
Public Class FirstRunWizard

    Const HelpTrialLearningHtml = "licence.html"

    Private Const ActivationTextFileFilter As String = "Blue Prism Activation files (*.bpAct)|*.BpAct"

    Private ReadOnly mBlueColor As Color = Color.FromArgb(&HB, &H75, &HB7)
    Private ReadOnly mBluePrismWarningColor As Color = Color.FromArgb(&HCB, &H62, &H0)
    Private ReadOnly mCharcoalGreyColor As Color = Color.FromArgb(&H43, &H4A, &H4F)
    Private ReadOnly mLightGreyColor As Color = Color.FromArgb(&HD4, &HD4, &HD4)
    Private ReadOnly mTextFont As Font = New Font("Segoe UI", 6.5!, FontStyle.Regular, GraphicsUnit.Pixel)
    Private ReadOnly mFileToolTip As New ToolTip()

    Private ReadOnly mCommunityBluePrismHomeUrl As String = "https://community.blueprism.com/home"
    Private ReadOnly mActivationUrl As String = "https://portal.blueprism.com/products/activation"
    Private ReadOnly mPortalProductsURL As String = "https://portal.blueprism.com/products"
    Private ReadOnly mCustomerSupportUrl As String = "https://www.blueprism.com/support"
    Private ReadOnly mActivationSupportUrl As String = "https://portal.blueprism.com/products/activation"
    Private ReadOnly mPortalHomePageUrl As String = "https://portal.blueprism.com"
    Private ReadOnly mDisplayBackOnGenerate As Boolean = True

    Private mHasOpenedPortal As Boolean = False
    Private mLicenseKeyInfo As KeyInfo
    Private mLastTabIndex As Integer = 0 'initial value is first tab
    Private mActivationCode As String
    Private mValidationCode As String
    Private mActivationHistory As Integer
    Private mMouseDownLocation As Point
    Private mCopyImageVisible As Boolean = False
    Private mSaveImageVisible As Boolean = False
    Private mErrorMessage As Exception

    Dim mShowWarning As Boolean = False

#Region "Constructor"
    'This constructor is when calling the License Activation process directly
    Public Sub New(licenseToActivate As KeyInfo)
        ' This call is required by the designer.
        InitializeComponent()
        SetTabOrder()

        'Hide the tab strip
        SetWizardPanelAppearance()

        BackButton.Visible = False
        mDisplayBackOnGenerate = False
        ' Set the license that needs to be activated.
        mLicenseKeyInfo = licenseToActivate
        UpdateActivationHistoryLinks()
        UpdateLicenseName()

        WizardSwitchPanel.SelectedTab = GenerateTab
    End Sub
    'This constructor will be called for the Import license process
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        SetTabOrder()

        'Hide the tab strip
        SetWizardPanelAppearance()

        BackButton.Visible = False

        LicenseNameLabel.Text = ""
        LicenseNameLabel.ForeColor = mBlueColor

        mActivationUrl = "https://portal.blueprism.com/products/activation"
        ImportLicenseTitle.Title = My.Resources.LicenseActivationActivateBluePrismText
        WizardSwitchPanel.SelectedTab = ImportLicenseTab
    End Sub
#End Region

#Region "Drag And Drop Window"

    Private Sub BorderPanel_MouseDown(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseDown
        If e.Button = MouseButtons.Left Then mMouseDownLocation = e.Location
    End Sub
    Private Sub BorderPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseMove
        If e.Button = MouseButtons.Left Then
            Left += e.Location.X - mMouseDownLocation.X
            Top += e.Location.Y - mMouseDownLocation.Y
        End If
    End Sub

#End Region

#Region "Methods"
    Private Sub SetTabOrder()
        ImportLicenseTab.TabIndex = 0
        ImportLicenseErrorTab.TabIndex = 1
        GenerateTab.TabIndex = 2
        CopyTab.TabIndex = 3
        PortalTab.TabIndex = 4
        ActivateTab.TabIndex = 5
        ErrorTab.TabIndex = 6
        ImportLicenseSuccessTab.TabIndex = 7
        ImportLicenseFinishedTab.TabIndex = 8
    End Sub

    Private Sub UpdateActivationHistoryLinks()
        Dim activationHistoryEvents = gSv.GetAuditLogDataForLicense(mLicenseKeyInfo.Id).ToList()
        mActivationHistory = activationHistoryEvents.Where(Function(x) x.EventType = LicenseEventTypes.LicenseActivationRequestGenerated AndAlso DateDiff(DateInterval.Day, x.EventDateTime, DateTime.UtcNow) < 5).Count

        mShowWarning = mActivationHistory > 0

        ActivationHistoryLink.Location = New Point(GenerateTab.Width \ 2 - ActivationHistoryLink.Width \ 2, ActivationHistoryLink.Location.Y)
        ActivationHistoryLinkIcon.Location = New Point(ActivationHistoryLink.Location.X - ActivationHistoryLinkIcon.Width, ActivationHistoryLinkIcon.Location.Y)
        ActivationHistoryLinkIcon.Visible = mShowWarning
        ActivationHistoryLink.Visible = mShowWarning

        GenerateTab.Invalidate()
    End Sub
    Private Sub GotoErrorTab()
        WizardSwitchPanel.SelectedTab = ErrorTab
    End Sub

    Private Sub PasteActivationKey()
        Try
            SetButtonSelected(ImportFileButton, False)
            SetButtonSelected(PasteButton, True)
            Dim clipboardText = Clipboard.GetText()
            If Not String.IsNullOrWhiteSpace(clipboardText) Then
                If IsActivationRequest(clipboardText) Then
                    EnterActivationCodeLabel.Text = My.Resources.LicenseActivationYouveCopiedYourCode
                    PortalKeyImage.Image = ActivationWizardResources.key_in_error
                    PastePanel.Height = 161
                    PastePanel.Top = 24
                Else
                    mValidationCode = clipboardText
                    SetButtonActive(VerifyActivationCodeButton, True)
                    PasteLabel.Visible = False
                    PasteImage.Visible = False
                    PastePanel.Height = 185
                    PastePanel.Top = 0
                    PortalKeyImage.Image = ActivationWizardResources.key_in
                    EnterActivationCodeLabel.Visible = False
                End If
            Else
                'Empty Clipboard, show error
                mValidationCode = String.Empty
                PasteLabel.Visible = True
                PasteLabel.Text = My.Resources.ActivationWizardUhOhEmpltyClipboard
                PasteLabel.LinkArea = New LinkArea(0, 0)
                PasteImage.Visible = True
                PasteImage.Image = ActivationWizardResources.empty_clipboard
                PastePanel.Height = 161
                PastePanel.Top = 24
                PortalKeyImage.Image = ActivationWizardResources.key_in_error
                EnterActivationCodeLabel.Visible = False
            End If
            PastePanel.Invalidate()
        Catch ex As Exception
            GotoErrorTab()
        End Try
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub BackClicked(sender As Object, e As EventArgs) Handles BackButton.Click

        If WizardSwitchPanel.SelectedTab.Name = GenerateTab.Name Then
            gSv.RemoveLicenseKey(mLicenseKeyInfo)
            If File.Exists(ImportLicensePathLabel.Text) Then
                ImportLicense(ImportLicensePathLabel.Text)
            Else
                UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.ImportLicenseFloatingFailedTitle,
                                         My.Resources.ImportLicenseFloatingFileNotFound, Point.Empty, 10000)
                mLicenseKeyInfo = Nothing
                RegisterButton.Visible = True
                SetButtonActive(ImportLicenseNextButton, False)
                SetButtonActive(FailedImportLicenseNextButton, False)
            End If

        End If

        WizardSwitchPanel.SelectedIndex = if(mLastTabIndex =1,0,mLastTabIndex)
        Select Case WizardSwitchPanel.SelectedTab.Name
            Case CopyTab.Name, PortalTab.Name, ActivateTab.Name, ErrorTab.Name, GenerateTab.Name
                mLastTabIndex = If(mLastTabIndex <= 0, 0, mLastTabIndex - 1)
            Case GenerateTab.Name
                UpdateActivationHistoryLinks()
        End Select
    End Sub
    Sub HorizontalLinePaint(s As Object, e As PaintEventArgs) _
        Handles PortalTab.Paint, ImportLicenseSuccessTab.Paint, GenerateTab.Paint, ErrorTab.Paint, CopyTab.Paint,
                ActivateTab.Paint

        Dim tRectangle As Rectangle = New Rectangle(75, 163, 645, 2)
        Dim lineColor As Color = mCharcoalGreyColor

        Try
            Select Case CType(s, TabPage).Name
                Case GenerateTab.Name
                    If mShowWarning Then
                        lineColor = mBluePrismWarningColor
                        tRectangle = New Rectangle(75, 163, 645, 8)
                    End If
                Case CopyTab.Name
                    tRectangle = New Rectangle(75, 225, 645, 2)
            End Select
        Catch ex As Exception
            lineColor = mCharcoalGreyColor
        End Try

        If tRectangle <> Nothing AndAlso lineColor <> Nothing Then
            Dim brush = New SolidBrush(lineColor)
            e.Graphics.FillRectangle(brush, tRectangle)
        End If
    End Sub
    Sub CopyTextPaint(s As Object, e As PaintEventArgs) Handles CopyPanel.Paint
        Dim textRectangle = New Rectangle(30, 30, 450, 126)
        Dim textFont = New Font(mTextFont.FontFamily, 8)
        Dim textColor = If(mCopyImageVisible OrElse mSaveImageVisible, mLightGreyColor, mCharcoalGreyColor)

        e.Graphics.DrawString(mActivationCode, textFont, New SolidBrush(textColor), textRectangle)

        If mCopyImageVisible Then
            e.Graphics.DrawImageUnscaled(ActivationWizardResources.copy, New Rectangle(New Point(231, 69), ActivationWizardResources.copy.Size))
        ElseIf mSaveImageVisible Then
            e.Graphics.DrawImage(ActivationWizardResources.heart, New Rectangle(New Point(231, 69), ActivationWizardResources.heart.Size))
        End If
    End Sub

    Sub ActivateTextPaint(s As Object, e As PaintEventArgs) Handles PastePanel.Paint
        Dim textRectangle = New Rectangle(30, 30, 450, 126)
        Dim textFont = New Font(mTextFont.FontFamily, 8)

        e.Graphics.DrawString(mValidationCode, textFont, New SolidBrush(mCharcoalGreyColor), textRectangle)
    End Sub

    Private Sub CloseButtonClicked(sender As Object, e As EventArgs) Handles ImportLicenseCancelButton.Click, CloseButton.Click
        Close()
    End Sub
    Private Sub CopyToClipboard(sender As Object, e As EventArgs) Handles CopyToClipboardButton.Click
        If Not String.IsNullOrWhiteSpace(mActivationCode) Then
            Clipboard.SetText(mActivationCode)
            SetButtonSelected(CopyToClipboardButton, True)
            SetButtonSelected(SaveToFileButton, False)
            mCopyImageVisible = True
            mSaveImageVisible = False
            CopyActionLabel.Visible = True
            CopyActionLabel.Text = My.Resources.LicenseActivationCopiedToClipboardText
            LicenseOutImageUnticked.Visible = False
            LicenseOutImageTicked.Visible = True
            SetButtonActive(CopyNextStepButton, True)
        End If

        CopyPanel.Invalidate()
    End Sub

    Private Sub FinishAndClose(sender As Object, e As EventArgs) Handles FinishedButton.Click
        SetUpTrialEncryptionScheme()
        Close()
    End Sub

    Private Sub LicenseActivationWizardFormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If WizardSwitchPanel.SelectedTab.Name = ImportLicenseFinishedTab.Name Then
            DialogResult = DialogResult.OK
        End If
    End Sub

    Private Sub GenerateActivationCode(sender As Object, e As EventArgs) Handles GenerateCodeButton.Click
        mLastTabIndex = WizardSwitchPanel.SelectedIndex
        mActivationCode = gSv().GetLicenseActivationRequest(mLicenseKeyInfo)
        WizardSwitchPanel.SelectedTab = CopyTab
        UpdateActivationHistoryLinks()
    End Sub
    Private Sub GotoCompleteActivationTabViaPortal(sender As Object, e As EventArgs) Handles GotActivationCodeButton.Click
        If mHasOpenedPortal = False Then
            ExternalBrowser.OpenUrl(mActivationUrl)
            GotActivationCodeButton.Text = My.Resources.LicenseActivationEnterYourKey
            GotActivationCodeButton.Image = Nothing
            GetKeyHelpButton.Visible = True
            mHasOpenedPortal = True
        Else
            mLastTabIndex = WizardSwitchPanel.SelectedIndex
            WizardSwitchPanel.SelectedTab = ActivateTab
        End If
    End Sub

    Private Sub GotoCompleteActivationTab(sender As Object, e As EventArgs) Handles GotActivationKeyButton.Click
        mLastTabIndex = WizardSwitchPanel.SelectedIndex
        WizardSwitchPanel.SelectedTab = ActivateTab
    End Sub

    Private Sub ImportFile(sender As Object, e As EventArgs) Handles ImportFileButton.Click
        Try
            Using openFileDialog As New OpenFileDialog With {
                    .Filter = ActivationTextFileFilter,
                    .Multiselect = False}
                If openFileDialog.ShowDialog() = DialogResult.OK Then
                    SetButtonSelected(ImportFileButton, True)
                    SetButtonSelected(PasteButton, False)
                    mValidationCode = My.Computer.FileSystem.ReadAllText(openFileDialog.FileName)
                    If Not String.IsNullOrWhiteSpace(mValidationCode) Then
                        If IsActivationRequest(mValidationCode) Then
                            SetButtonActive(VerifyActivationCodeButton, False)
                            mValidationCode = String.Empty
                            EnterActivationCodeLabel.Text = My.Resources.LicenceActivationImportedYourCode
                            EnterActivationCodeLabel.Visible = True
                            PortalKeyImage.Image = ActivationWizardResources.key_in_error
                            PastePanel.Height = 161
                            PastePanel.Top = 24
                        Else
                            SetButtonActive(VerifyActivationCodeButton, True)
                            PasteLabel.Visible = False
                            PasteImage.Visible = False
                            PastePanel.Height = 185
                            PastePanel.Top = 0
                            PortalKeyImage.Image = ActivationWizardResources.key_in
                            EnterActivationCodeLabel.Visible = False
                        End If
                    Else
                        'Bad File, show error
                        SetButtonActive(VerifyActivationCodeButton, False)
                        SetButtonSelected(ImportFileButton, False)
                        SetButtonSelected(PasteButton, False)
                        PastePanelBackground.BackColor = mBluePrismWarningColor
                        mValidationCode = String.Empty
                        PasteLabel.Visible = True
                        PasteLabel.Text = My.Resources.ActivationWizardNotTheFile
                        PasteImage.Visible = True
                        PasteImage.Image = ActivationWizardResources.not_the_file
                        PastePanel.Height = 161
                        PastePanel.Top = 24
                        PortalKeyImage.Image = ActivationWizardResources.key_in_error
                        EnterActivationCodeLabel.Visible = False
                    End If
                End If
            End Using
            PastePanel.Invalidate()
        Catch ex As Exception
            GotoErrorTab()
        End Try
    End Sub

    Private Sub PasteValidationCode(sender As Object, e As EventArgs) Handles PasteButton.Click
        PasteActivationKey()
    End Sub
    Private Sub WizardSwitchPanel_KeyDown(sender As Object, e As KeyEventArgs) Handles WizardSwitchPanel.KeyDown, MyBase.KeyDown
        If WizardSwitchPanel.SelectedTab.Name = ActivateTab.Name Then
            If e.Control AndAlso e.KeyCode = Keys.V Then
                PasteActivationKey()
            End If
        End If
    End Sub

    Private Sub CopyNextStep(sender As Object, e As EventArgs) Handles CopyNextStepButton.Click
        mLastTabIndex = WizardSwitchPanel.SelectedIndex
        WizardSwitchPanel.SelectedTab = PortalTab
    End Sub

    Private Function IsActivationRequest(pastedText As String) As Boolean
        Return gSv.GetActivationRequestsForLicense(mLicenseKeyInfo.Id).Contains(pastedText)
    End Function
    Private Sub ReturnToLicenseCopyLicenseTab(sender As Object, e As EventArgs) Handles WizardSwitchPanel.Click, LicenseErrorCloseButton.Click
        Close()
    End Sub

    Private Sub SaveToFile(sender As Object, e As EventArgs) Handles SaveToFileButton.Click
        Using saveFileDialog1 As New SaveFileDialog With {
            .Filter = ActivationTextFileFilter,
            .FilterIndex = 2,
            .RestoreDirectory = True,
            .FileName = $"{mLicenseKeyInfo.LicenseOwner} (Activation File).bpAct"
            }

            If saveFileDialog1.ShowDialog() = DialogResult.OK Then
                My.Computer.FileSystem.WriteAllText(saveFileDialog1.FileName, mActivationCode, False)
                SetButtonSelected(SaveToFileButton, True)
                SetButtonSelected(CopyToClipboardButton, False)
                SetButtonActive(CopyNextStepButton, True)
                mCopyImageVisible = False
                mSaveImageVisible = True
                CopyActionLabel.Visible = True
                CopyActionLabel.Text = My.Resources.LicenseActivationSavedToFileText
                mFileToolTip.SetToolTip(CopyActionLabel, Path.GetFileNameWithoutExtension(saveFileDialog1.FileName))
                LicenseOutImageUnticked.Visible = False
                LicenseOutImageTicked.Visible = True
                mFileToolTip.UseFading = True
            End If
        End Using
        CopyPanel.Invalidate()
    End Sub
    Private Sub SetButtonSelected(button As Button, active As Boolean)
        Select Case active
            Case True
                button.BackColor = mBlueColor
                button.ForeColor = Color.White
                button.FlatAppearance.BorderColor = mBlueColor
            Case False
                button.BackColor = Color.White
                button.ForeColor = mBlueColor
                button.FlatAppearance.BorderColor = mBlueColor
        End Select
    End Sub

    Private Sub SetButtonActive(button As Button, active As Boolean)
        button.Enabled = active
        Select Case active
            Case True
                button.FlatAppearance.BorderColor = mBlueColor
            Case False
                button.FlatAppearance.BorderColor = mCharcoalGreyColor
        End Select
    End Sub
    Private Sub WizardSwitchPanelTabIndexChanged(sender As Object, e As EventArgs) Handles WizardSwitchPanel.TabIndexChanged, WizardSwitchPanel.SelectedIndexChanged
        If WizardSwitchPanel.SelectedTab IsNot Nothing Then
            Select Case WizardSwitchPanel.SelectedTab.Name
                Case ImportLicenseFinishedTab.Name
                    BackButton.Visible = False
                Case ImportLicenseTab.Name, ImportLicenseErrorTab.Name, ErrorTab.Name, ImportLicenseSuccessTab.Name
                    BackButton.Visible = False
                Case CopyTab.Name
                    LicenseOutImageTicked.Visible = False
                    LicenseOutImageUnticked.Visible = True
                    BackButton.Visible = True
                    SetButtonSelected(SaveToFileButton, False)
                    SetButtonSelected(CopyToClipboardButton, False)
                    SetButtonActive(CopyNextStepButton, False)
                    CopyActionLabel.Visible = False
                    mCopyImageVisible = False
                    mSaveImageVisible = False
                Case PortalTab.Name
                    BackButton.Visible = True
                Case ActivateTab.Name
                    YourUniqueCodeLabel.LinkArea = New LinkArea(YourUniqueCodeLabel.Text.Length - mActivationSupportUrl.Length, mActivationSupportUrl.Length)
                    EnterActivationCodeLabel.Visible = True
                    EnterActivationCodeLabel.Text = My.Resources.LicenseActivationEnterYourPortalKey
                    PasteImage.Visible = False
                    PasteLabel.Visible = False
                    BackButton.Visible = True
                    SetButtonSelected(ImportFileButton, False)
                    SetButtonSelected(PasteButton, False)
                    SetButtonActive(VerifyActivationCodeButton, False)
                    mValidationCode = ""
                Case GenerateTab.Name
                    BackButton.Visible = mDisplayBackOnGenerate
                    UpdateActivationHistoryLinks()
            End Select
            ActiveControl = Nothing
        End If
    End Sub

    Private Sub VerifyActivationCode(sender As Object, e As EventArgs) Handles VerifyActivationCodeButton.Click
        mLastTabIndex = WizardSwitchPanel.SelectedIndex
        Try
            If String.IsNullOrWhiteSpace(mValidationCode) Then
                UserMessage.Show("Please provide License Activation Response")
            Else
                Dim verified = gSv().ValidateLicenseActivationResponseForLicense(mLicenseKeyInfo, mValidationCode)

                If verified Then
                    'Move to the complete tab
                    SetButtonSelected(ImportFileButton, False)
                    SetButtonSelected(PasteButton, True)
                    WizardSwitchPanel.SelectedTab = ImportLicenseSuccessTab
                    clsLicenseQueries.RefreshLicense()
                Else
                    'Set Error Screen and switch to it
                    Dim linkText = My.Resources.ImportLicenseContactTechnicalSupportText
                    linkText = linkText.Substring(0, linkText.Length - 1) 'all translations end with a period character
                    TechnicalSupportLinkLabel.LinkArea = New LinkArea(TechnicalSupportLinkLabel.Text.IndexOf(linkText, StringComparison.Ordinal), linkText.Length)
                    WizardSwitchPanel.SelectedTab = ErrorTab
                End If
            End If
        Catch ex As Exception
            GotoErrorTab()
        End Try
    End Sub

    Private Sub ActivationHistoryLabel_Click(sender As Object, e As EventArgs) Handles ActivationHistoryLink.Click
        Using licenseHistory As New LicenseActivationHistory(mLicenseKeyInfo)
            licenseHistory.ShowInTaskbar = False
            licenseHistory.ShowDialog()
        End Using
    End Sub

    Private Sub ImportLicenseBrowse_Click(sender As Object, e As EventArgs) Handles ImportLicenseBrowse.Click, FailedImportLicenceBrowseButton.Click
        Try
            Using openFileDialog As New OpenFileDialog()
                openFileDialog.Filter = My.Resources.ctlSystemLicense_BluePrismLicenseLicLic
                openFileDialog.FilterIndex = 1
                If openFileDialog.ShowDialog() = DialogResult.OK Then
                    'Lets import the license file but not apply just yet
                    ImportLicense(openFileDialog.FileName)
                Else
                    SetButtonActive(ImportLicenseNextButton, False)
                    SetButtonActive(FailedImportLicenseNextButton, False)
                    ImportLicensePathLabel.Text = String.Empty
                End If
            End Using
        Catch
            UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.ImportLicenseFloatingFailedTitle,
                                     My.Resources.ImportLicenseFloatingInvalidLicense, Point.Empty)
            mLicenseKeyInfo = Nothing
            RegisterButton.Visible = True
            SetButtonActive(ImportLicenseNextButton, False)
            SetButtonActive(FailedImportLicenseNextButton, False)
            ImportLicensePathLabel.Text = String.Empty
        End Try
    End Sub

    Private Sub ImportLicense(fileName As String)
        Dim newKey As String = File.ReadAllText(fileName)
        mLicenseKeyInfo = New KeyInfo(newKey, DateTime.UtcNow, User.CurrentId)
        ImportLicensePathLabel.Font = New Font(ImportLicensePathLabel.Font.FontFamily, CType(16, Single))
        While TextRenderer.MeasureText(fileName, ImportLicensePathLabel.Font).Width > ImportLicensePathLabel.Width - 25
            ImportLicensePathLabel.Font = New Font(ImportLicensePathLabel.Font.FontFamily, CType((ImportLicensePathLabel.Font.Size - 0.5), Single))
        End While
        ImportLicenseErrorPathLabel.Font = ImportLicensePathLabel.Font
        ImportLicensePathLabel.Text = fileName
        ImportLicenseErrorPathLabel.Text = fileName
        ActivationWizardToolTip.SetToolTip(ImportLicensePathLabel, fileName)
        ActivationWizardToolTip.SetToolTip(ImportLicenseErrorPathLabel, fileName)
        SetButtonActive(ImportLicenseNextButton, True)
        SetButtonActive(FailedImportLicenseNextButton, True)
        UpdateLicenseName()
    End Sub

    Private Sub UpdateLicenseName()
        LicenseNameLabel.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel.Left = CInt(400 - (LicenseNameLabel.Width / 2))
        LicenseNameLabel.ForeColor = mBlueColor

        LicenseNameLabel2.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel2.Left = CInt(400 - (LicenseNameLabel2.Width / 2))
        LicenseNameLabel2.ForeColor = mBlueColor

        LicenseNameLabel3.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel3.Left = CInt(400 - (LicenseNameLabel3.Width / 2))
        LicenseNameLabel3.ForeColor = mBlueColor

        LicenseNameLabel4.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel4.Left = CInt(400 - (LicenseNameLabel4.Width / 2))
        LicenseNameLabel4.ForeColor = mBlueColor

        LicenseNameLabel5.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel5.Left = CInt(400 - (LicenseNameLabel5.Width / 2))
        LicenseNameLabel5.ForeColor = mBlueColor

        LicenseNameLabel6.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel6.Left = CInt(400 - (LicenseNameLabel6.Width / 2))
        LicenseNameLabel6.ForeColor = mBlueColor
    End Sub

    Private Sub ImportLicenseNextButton_Click(sender As Object, e As EventArgs) Handles ImportLicenseNextButton.Click, FailedImportLicenseNextButton.Click
        Try
            Dim keys = gSv.AddLicenseKey(mLicenseKeyInfo, ImportLicensePathLabel.Text)
            SetLicenseKeys(keys)

            mLicenseKeyInfo = keys.FirstOrDefault(Function(x) x.LicenseOwner = mLicenseKeyInfo.LicenseOwner AndAlso x.LicenseRequestID = mLicenseKeyInfo.LicenseRequestID)

            If License.FirstKey.ActivationStatus = LicenseActivationStatus.NotActivated AndAlso Not License.FirstKey.Expired Then
                WizardSwitchPanel.SelectedTab = GenerateTab
            Else
                WizardSwitchPanel.SelectedTab = ImportLicenseSuccessTab
            End If
        Catch ex As Exception
            UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.ImportLicenseFloatingFailedTitle,
                                     My.Resources.ImportLicenseFloatingFailedPrompt1, Point.Empty)
            mErrorMessage = ex
            SetButtonActive(FailedImportLicenseNextButton, False)
            WizardSwitchPanel.SelectedTab = ImportLicenseErrorTab
        End Try
    End Sub

    Private Sub ImportSuccessNextButton_Click(sender As Object, e As EventArgs) Handles ImportSuccessNextButton.Click
        GenerateImportLicenseSuccessHyperlink(My.Resources.ImportLicenseBluePrismPortal, New Uri(mPortalHomePageUrl))
        GenerateImportLicenseSuccessHyperlink(My.Resources.ImportLicenseCommunityPortalText, New Uri(mCommunityBluePrismHomeUrl))
        WizardSwitchPanel.SelectedTab = ImportLicenseFinishedTab
    End Sub

    Private Sub GenerateImportLicenseSuccessHyperlink(linkText As String, url As Uri)
        Dim linkIndex = ImportLicenseFinishedLinkLabel.Text.ToUpper.IndexOf(linkText.ToUpper, StringComparison.Ordinal)
        ImportLicenseFinishedLinkLabel.Links.Add(linkIndex, linkText.Length, url.AbsoluteUri)
    End Sub

    Private Sub ImportLicenseFinishedLinkLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles ImportLicenseFinishedLinkLabel.LinkClicked
        ExternalBrowser.OpenUrl(e.Link.LinkData.ToString())
    End Sub

    Private Sub RegisterButton_Click(sender As Object, e As EventArgs) Handles RegisterButton.Click
        ExternalBrowser.OpenUrl(mPortalProductsURL)
    End Sub

    Private Sub SetWizardPanelAppearance()
        WizardSwitchPanel.Appearance = TabAppearance.FlatButtons
        WizardSwitchPanel.ItemSize = New Size(0, 1)
        WizardSwitchPanel.SizeMode = TabSizeMode.Fixed
    End Sub

    Private Sub TechnicalSupportLinkLabel_Click(sender As Object, e As EventArgs) Handles TechnicalSupportLinkLabel.Click
        ExternalBrowser.OpenUrl(mCustomerSupportUrl)
    End Sub
    Private Sub ErrorDetailsLabel_Click(sender As Object, e As EventArgs) Handles ErrorDetailsLabel.Click
        Invoke(Sub() UserMessage.Err(mErrorMessage, mErrorMessage.Message))
    End Sub

    Private Function SetUpTrialEncryptionScheme() As Boolean
        If Licensing.License.LicenseType = LicenseTypes.Evaluation OrElse Licensing.License.LicenseType = LicenseTypes.Education Then
            Try
                Dim schemeNoKeys = gSv.GetEncryptionSchemesExcludingKey()
                Dim count = schemeNoKeys.Count()
                Dim schemeNoKeyExisting = schemeNoKeys.FirstOrDefault()

                If count <> 1 Or schemeNoKeyExisting Is Nothing Then
                    Return True 'unexpected configuration
                End If

                Dim algorithmName As String = gSv.GetAlgorithmName(schemeNoKeyExisting)
                If Not (String.IsNullOrWhiteSpace(algorithmName) = False AndAlso algorithmName.Contains(clsEncryptionScheme.UnresolvedKeyName) = True AndAlso schemeNoKeyExisting.KeyLocation = EncryptionKeyLocation.Server) Then
                    Return True 'unexpected configuration
                End If

                Dim schemeName As String = clsEncryptionScheme.TempEncryptionSchemeName
                Dim scheme As New clsEncryptionScheme(schemeName)
                scheme.Algorithm = EncryptionAlgorithm.AES256
                scheme.GenerateKey()
                Dim plainKey As New StringBuilder()
                Using pinned = scheme.Key.Pin()
                    For Each c As Char In pinned.Chars
                        plainKey.Append(c)
                    Next
                End Using
                scheme.IsAvailable = True
                scheme.KeyLocation = EncryptionKeyLocation.Database
                scheme.Key = New SafeString(plainKey.ToString())

                gSv.StoreEncryptionScheme(scheme)

                Dim schemeNoKeyTemp = gSv.GetEncryptionSchemesExcludingKey().Where(Function(schemeNoKey) schemeNoKey.Name = schemeName AndAlso schemeNoKey.KeyLocation = EncryptionKeyLocation.Database).FirstOrDefault()
                If schemeNoKeyTemp Is Nothing Then Throw New BluePrismException(My.Resources.FailedToStoreTempDBEncryptionScheme)
                gSv.SetDefaultEncrypter(schemeNoKeyTemp.ID, schemeName)

                gSv.DeleteEncryptionScheme(schemeNoKeyExisting)

                schemeName = clsEncryptionScheme.DefaultEncryptionSchemeName
                scheme = New clsEncryptionScheme(schemeName)
                scheme.Algorithm = EncryptionAlgorithm.AES256
                scheme.GenerateKey()
                plainKey = New StringBuilder()
                Using pinned = scheme.Key.Pin()
                    For Each c As Char In pinned.Chars
                        plainKey.Append(c)
                    Next
                End Using
                scheme.IsAvailable = True
                scheme.KeyLocation = EncryptionKeyLocation.Database
                scheme.Key = New SafeString(plainKey.ToString())

                gSv.StoreEncryptionScheme(scheme)

                Dim schemeNoKeyDefault = gSv.GetEncryptionSchemesExcludingKey().Where(Function(schemeNoKey) schemeNoKey.Name = schemeName AndAlso schemeNoKey.KeyLocation = EncryptionKeyLocation.Database).FirstOrDefault()
                If schemeNoKeyDefault Is Nothing Then Throw New BluePrismException(My.Resources.FailedToStoreDefaultDBEncryptionScheme)
                gSv.SetDefaultEncrypter(schemeNoKeyDefault.ID, schemeName)

                gSv.DeleteEncryptionScheme(schemeNoKeyTemp)
            Catch ex As Exception
                UserMessage.Show(My.Resources.FailedToStoreEncryptionScheme, ex)
                Return False
            End Try
        End If

        Return True
    End Function

    Private Sub HelpButton_Click(sender As Object, e As EventArgs) Handles HelpMeButton.Click, GetKeyHelpButton.Click
        ExternalBrowser.OpenUrl(mActivationSupportUrl)
    End Sub

    Private Sub YourUniqueCodeLabel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles YourUniqueCodeLabel.LinkClicked
        ExternalBrowser.OpenUrl(mActivationSupportUrl)
    End Sub

    Private Sub HelpGuideButton_Click(sender As Object, e As EventArgs) Handles HelpGuideButton.Click
        Try
            OpenHelpFile(Me, HelpTrialLearningHtml)
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
        End Try
    End Sub

#End Region

End Class
