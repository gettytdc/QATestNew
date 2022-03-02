Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports AutomateControls
Imports BluePrism.Server.Domain.Models

Public Class ctlSystemSettings : Implements IChild, IPermission, IHelp

#Region " Class-scope Members "

    ''' <summary>
    ''' The major interval to use in backup periods - at values below this value,
    ''' the step between values is 1 (minute), above this value, the step is this
    ''' interval.
    ''' eg. with a MajorInterval of 5, the values in the combo box are :
    ''' 1, 2, 3, 4, 5, 10, 15, 20 ... etc ...
    ''' </summary>
    Private Const MajorInterval As Integer = 5
    Private mOldTesseractComboxIndex As Integer = -1

#End Region

#Region " Member Variables "

    Private mParent As frmApplication

    Private mLoadingSettings As Boolean

    Private mOrigEnvName As String
    Private mOrigBackground As Color
    Private mOrigForeground As Color
    Private mOfflineHelpEnabled As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new system settings control
    ''' </summary>
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' Populate the colour combo boxes - we don't use the named colours directly
        ' because we want 'Equals()' to work for non-named colours and unfortunately
        ' MS saw fit to make Color.Black unequal to Color.FromArgb(0, 0, 0) even
        ' though they have the same value

        cmbBackground.BeginUpdate()
        With cmbBackground.Items
            .Add(ColourScheme.Default.EnvironmentBackColor)
            .Add(Color.FromArgb(Color.DarkRed.ToArgb()))
            .Add(Color.FromArgb(Color.Orange.ToArgb()))
            .Add(Color.FromArgb(Color.Yellow.ToArgb()))
            .Add(Color.FromArgb(Color.DarkGreen.ToArgb()))
            .Add(Color.FromArgb(Color.Purple.ToArgb()))
            .Add(Color.FromArgb(Color.Black.ToArgb()))
            .Add(Color.FromArgb(Color.DarkGray.ToArgb()))
            .Add(Color.FromArgb(Color.White.ToArgb()))
            ' add in the 'old' default colour so it can be retained for upgrades
            .Add(Color.FromArgb(0, 114, 198))
        End With
        cmbBackground.EndUpdate()

        cmbForeground.BeginUpdate()
        With cmbForeground.Items
            .Add(ColourScheme.Default.EnvironmentForeColor)
            .Add(Color.FromArgb(Color.Orange.ToArgb()))
            .Add(Color.FromArgb(Color.Yellow.ToArgb()))
            .Add(Color.FromArgb(Color.DarkGreen.ToArgb()))
            .Add(Color.FromArgb(Color.Purple.ToArgb()))
            .Add(Color.FromArgb(Color.DarkGray.ToArgb()))
            .Add(Color.FromArgb(Color.Black.ToArgb()))
        End With
        cmbForeground.EndUpdate()

    End Sub

#End Region

#Region " Properties "

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System - Settings")
        End Get
    End Property

#End Region

#Region " Other Methods "

    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Populates the checkbox that controls whether the process engine should
    ''' run on this machine at startup, by reading the registry value.
    ''' </summary>
    Private Sub PopulateStartEngine()
        chkStartProcEngine.Checked = Options.Instance.StartProcessEngine
    End Sub

    Private Sub PopulateBackUpInterval()

        Dim interval As Integer = gSv.AutoSaveReadInterval()

        If interval > MajorInterval Then
            cmbBackUp.SelectedIndex = MajorInterval +
                CInt(Math.Floor((interval - MajorInterval) / MajorInterval))
        Else
            cmbBackUp.SelectedIndex = interval
        End If

        cmbBackUp.Enabled = cmbBackUp.SelectedIndex > 0
        chkBackUp.Checked = cmbBackUp.Enabled
    End Sub

    Private Sub EnableComboBox(ByVal cb As CheckBox,
                               ByVal combo As ComboBox,
                               Optional ByVal defaultInd As Integer = 1)
        If cb.Checked Then
            combo.Enabled = True
            combo.SelectedIndex = defaultInd
        Else
            combo.Enabled = False
            combo.SelectedIndex = 0
        End If
    End Sub

    Private Sub PopulateEnforceSummaries()
        Try
            chkEnforceSummaries.Checked = gSv.GetEnforceEditSummariesSetting()
        Catch ex As Exception
            UserMessage.Show(
                String.Format(My.Resources.ctlSystemSettings_ThereWasAnErrorWhilstReadingTheSystemPreferenceUseEditSummariesFromTheDatabase0, ex.Message))
        End Try
    End Sub

    Private Sub PopulateUnicodeLogging()
        Try
            chkUnicodeLogging.Checked = gSv.UnicodeLoggingEnabled()
        Catch ex As Exception
            UserMessage.Err(String.Format(My.Resources.ctlSystemSettings_ErrorRetrievingUnicodeLoggingFlag0, ex.Message), ex)
        End Try
    End Sub

    Private Sub PopulateRecordEnvironmentSettings()
        chkEnableEnvironmentRecording.Checked = gSv.GetEnableBpaEnvironmentDataSetting(True)
    End Sub

    Private Sub PopulateResourceSettings()
        cmbResourceReg.SelectedIndex = gSv.GetResourceRegistrationMode()
        AddHandler cmbResourceReg.SelectionChangeCommitted, AddressOf cmbResourceReg_SelectionChangeCommitted
        chkPreventResourceRegistration.Checked = gSv.GetPreventResourceRegistrationSetting()
        chkRequireSecuredResource.Checked = gSv.GetRequireSecuredResourceConnections()
        chkAllowAnonymousResources.Checked = gSv.GetAllowAnonymousResources()
    End Sub

    Private Sub PopulateApplicationManagerSettings()
        TesseractEngineComboBox.SelectedIndex = gSv.GetTesseractEngine()
    End Sub

    Private Sub PopulateShowHideDigitalExchangeTab()
        chkHideDigitalExchange.Checked = gSv.GetHideDigitalExchangeSetting(False)
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "sysman-settings.html"
    End Function

#End Region

#Region " Event Handler Methods "

    Private Sub ctlSystemSettings_Load() Handles Me.Load
        mLoadingSettings = True

        PopulateBackUpInterval()
        PopulateStartEngine()
        PopulateEnforceSummaries()
        PopulateUnicodeLogging()
        PopulateRecordEnvironmentSettings()
        PopulateResourceSettings()
        PopulateApplicationManagerSettings()
        PopulateShowHideDigitalExchangeTab()
        PopulateOfflineHelpSettings()

        mOrigEnvName = gSv.GetPref(PreferenceNames.Env.EnvironmentName, My.Resources.Home)

        chkAllowPasswordPasting.Checked = gSv.GetAllowPasswordPasting()
        chkExceptionScreenshot.Checked = gSv.GetAllowResourceScreenshot()

        numDefaultStageWarning.Value = CDec(gSv.GetStageWarningThreshold()) / 60

        gSv.GetEnvironmentColors(mOrigBackground, mOrigForeground)
        txtEnvName.Text = mOrigEnvName
        cmbBackground.SelectedColor = mOrigBackground
        cmbForeground.SelectedColor = mOrigForeground

        mLoadingSettings = False
    End Sub

    Private Sub HandleStartProcEngineChanged(sender As Object, e As EventArgs) Handles chkStartProcEngine.CheckedChanged
        If mLoadingSettings Then Exit Sub
        Try
            Dim hasStateChanged = True
            Dim configOptions = Options.Instance
            configOptions.StartProcessEngine = chkStartProcEngine.Checked
            Try
                configOptions.Save()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlSystemSettings_TheSettingWillNotBeRememberedUnableToSave0, ex.Message))
            End Try

            If chkStartProcEngine.Checked Then
                mParent.InitProcessEngine()
            Else
                If Not mParent.ShutDownProcessEngine(True) Then
                    RemoveHandler chkStartProcEngine.CheckedChanged, AddressOf HandleStartProcEngineChanged
                    chkStartProcEngine.Checked = True
                    hasStateChanged = False
                    AddHandler chkStartProcEngine.CheckedChanged, AddressOf HandleStartProcEngineChanged
                End If
            End If

            If Not mLoadingSettings AndAlso hasStateChanged Then gSv.AuditStartProcEngineSettingChange(chkStartProcEngine.Checked)

        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.ctlSystemSettings_CouldNotApplyNewSettingAnErrorOccurred0, ex.Message))
        End Try
    End Sub

    Private Sub cmbBackUp_SelectedIndexChanged() Handles cmbBackUp.SelectedIndexChanged
        If mLoadingSettings Then Return

        Dim mins As Integer
        If cmbBackUp.SelectedIndex > MajorInterval Then
            mins = (cmbBackUp.SelectedIndex - MajorInterval + 1) * MajorInterval
        Else
            mins = cmbBackUp.SelectedIndex
        End If
        gSv.AutoSaveWriteInterval(mins)

        cmbBackUp.Enabled = cmbBackUp.SelectedIndex > 0
        chkBackUp.Checked = cmbBackUp.Enabled

    End Sub

    Private Sub chkBackUp_CheckedChanged() Handles chkBackUp.CheckedChanged
        If mLoadingSettings Then Exit Sub
        EnableComboBox(chkBackUp, cmbBackUp, cmbBackUp.Items.Count - 1)
    End Sub

    Private Sub chkEnforceSummaries_CheckedChanged() Handles chkEnforceSummaries.CheckedChanged
        If mLoadingSettings Then Exit Sub

        Try
            gSv.SetEnforceEditSummariesSetting(chkEnforceSummaries.Checked)
            Options.Instance.EditSummariesAreCompulsory = chkEnforceSummaries.Checked
        Catch ex As Exception
            mLoadingSettings = True
            UserMessage.Show(String.Format(My.Resources.ctlSystemSettings_ThereWasAnErrorWhilstSettingTheSystemPreferenceUseEditSummariesToTheDatabase0, ex.Message))
            chkEnforceSummaries.Checked = Not chkEnforceSummaries.Checked
            mLoadingSettings = False
        End Try

    End Sub

    Private Sub chkUnicodeLogging_CheckedChanged(sender As Object, e As EventArgs) Handles chkUnicodeLogging.CheckedChanged
        If mLoadingSettings Then Exit Sub
        Dim msg As String
        If chkUnicodeLogging.Checked Then
            msg = String.Format(
             My.Resources.ctlSystemSettings_EnablingUnicodeLoggingProvidesTheCapabilityToStoreAdditionalCharactersInTheLogs, vbCrLf)
        Else
            msg = String.Format(
             My.Resources.ctlSystemSettings_DisablingUnicodeLoggingWillResultInARestrictedCodePageBeingUsedToStoreSessionLo, vbCrLf)
        End If
        Try
            If UserMessage.TwoButtonsWithCustomText(msg, My.Resources.ctlSystemSettings_Continue, My.Resources.ctlSystemSettings_Cancel) = MsgBoxResult.Yes Then
                gSv.UpdateUnicodeLogging(chkUnicodeLogging.Checked)
            End If
        Catch ex As BluePrismException
            UserMessage.Err(String.Format(
                CStr(IIf(chkUnicodeLogging.Checked, My.Resources.ctlSystemSettings_UnableToEnableUnicodeLogging0, My.Resources.ctlSystemSettings_UnableToDisableUnicodeLogging0)), ex.Message))
        Finally
            PopulateUnicodeLogging()
        End Try
    End Sub

    ''' <summary>
    ''' Handles a background combobox selection being made
    ''' </summary>
    Private Sub HandleBackgroundChanged() Handles cmbBackground.SelectedIndexChanged
        lblPreview.BackColor = cmbBackground.SelectedColor
    End Sub

    ''' <summary>
    ''' Handles a foreground combobox selection being made
    ''' </summary>
    Private Sub HandleForegroundChanged() Handles cmbForeground.SelectedIndexChanged
        lblPreview.ForeColor = cmbForeground.SelectedColor
    End Sub

    ''' <summary>
    ''' Handles the theme 'Apply' button being pressed, saving the changes to the
    ''' database and informing the UI that the environment theme has changed
    ''' </summary>
    Private Sub HandleApplyTheme() Handles btnApply.Click
        Try
            ' If environment name has changed, save it
            Dim name = txtEnvName.Text
            If Not name.Equals(mOrigEnvName) Then
                gSv.SetSystemPref(PreferenceNames.Env.EnvironmentName, name)
            End If

            ' If background has changed, save that
            Dim bg = cmbBackground.SelectedColor
            If Not bg.Equals(mOrigBackground) Then
                gSv.SetSystemPref(PreferenceNames.Env.EnvironmentBackColor, bg)
                If mParent IsNot Nothing Then mParent.EnvironmentBackColor = bg
            End If

            ' If foreground has changed, save that
            Dim fg = cmbForeground.SelectedColor
            If Not fg.Equals(mOrigForeground) Then
                gSv.SetSystemPref(PreferenceNames.Env.EnvironmentForeColor, fg)
                If mParent IsNot Nothing Then mParent.EnvironmentForeColor = fg
            End If

            ' Update the 'orig' colours so we can check for changes against the
            ' current system preference
            mOrigEnvName = name
            mOrigBackground = bg
            mOrigForeground = fg

        Catch ex As Exception
            UserMessage.Err(
                ex,
                My.Resources.ctlSystemSettings_AnErrorOccurredWhileTryingToSaveTheEnvironmentTheme0,
                ex.Message)

        End Try

    End Sub

    Private Sub cmbResourceReg_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If UserMessage.OkCancel(My.Resources.ctlSystemSettings_ThisSettingDeterminesHowResourcesAreRegisteredPresentedAndConnectedYouMayNeedTo) = MsgBoxResult.Ok Then
            gSv.SetResourceRegistrationMode(CType(cmbResourceReg.SelectedIndex, ResourceRegistrationMode))
        Else
            cmbResourceReg.SelectedIndex = gSv.GetResourceRegistrationMode()
        End If
    End Sub

    Private Sub chkPreventResourceRegistration_CheckedChanged(sender As Object, e As EventArgs) Handles chkPreventResourceRegistration.CheckedChanged
        If mLoadingSettings Then Exit Sub
        gSv.SetPreventResourceRegistrationSetting(chkPreventResourceRegistration.Checked)
    End Sub

    Private Sub chkRequireSecuredResource_CheckedChanged(sender As Object, e As EventArgs) Handles chkRequireSecuredResource.CheckedChanged
        If Not mLoadingSettings Then gSv.SetRequireSecuredResourceConnections(chkRequireSecuredResource.Checked)
    End Sub

    Private Sub chkAllowAnonymousResources_CheckedChanged(sender As Object, e As EventArgs) Handles chkAllowAnonymousResources.CheckedChanged
        If Not mLoadingSettings Then gSv.SetAllowAnonymousResources(chkAllowAnonymousResources.Checked)
    End Sub

    ''' <summary>
    ''' Handles the 'Allow pasting of passwords' checkbox changing value.
    ''' </summary>
    Private Sub HandleAllowPastePasswordChanged(sender As Object, e As EventArgs) Handles chkAllowPasswordPasting.CheckedChanged
        If Not mLoadingSettings Then gSv.SetAllowPasswordPasting(chkAllowPasswordPasting.Checked)
    End Sub

    ''' <summary>
    ''' Handles the 'Allow exception screenshots' checkbox changing value
    ''' </summary>
    Private Sub HandleExceptionScreenshotChanged(sender As Object, e As EventArgs) _
        Handles chkExceptionScreenshot.CheckedChanged
        If Not mLoadingSettings Then _
            gSv.SetAllowResourceScreenshot(chkExceptionScreenshot.Checked)
    End Sub

    Private Sub HandleDefaultStageWarningThreshold(sender As Object, e As EventArgs) Handles numDefaultStageWarning.Validated
        gSv.SetStageWarningThreshold(CInt(numDefaultStageWarning.Value * 60))
    End Sub

    Private Sub TesseractEngineComboBox_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles TesseractEngineComboBox.SelectionChangeCommitted
        If mOldTesseractComboxIndex <> TesseractEngineComboBox.SelectedIndex Then
            gSv.SetTesseractEngine(TesseractEngineComboBox.SelectedIndex)
        End If
    End Sub

    Private Sub TesseractEngineComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TesseractEngineComboBox.SelectedIndexChanged
        mOldTesseractComboxIndex = TesseractEngineComboBox.SelectedIndex
    End Sub

    Private Sub HandleHideDigitalExchange_CheckedChanged(sender As Object, e As EventArgs) Handles chkHideDigitalExchange.CheckedChanged
        If mLoadingSettings Then Return
        gSv.UpdateHideDigitalExchangeSetting(chkHideDigitalExchange.Checked)
        If chkHideDigitalExchange.Checked AndAlso gMainForm.tcModuleSwitcher.TabPages.Contains(gMainForm.tpDigitalExchange) Then
            gMainForm.tcModuleSwitcher.Controls.Remove(gMainForm.tpDigitalExchange)
        ElseIf Not chkHideDigitalExchange.Checked AndAlso Not gMainForm.tcModuleSwitcher.TabPages.Contains(gMainForm.tpDigitalExchange) Then
            gMainForm.tcModuleSwitcher.TabPages.Insert(gMainForm.tcModuleSwitcher.TabPages.IndexOf(gMainForm.tpSystemManager), gMainForm.tpDigitalExchange)
        End If
    End Sub

    Private Sub ChkOfflineHelp_CheckedChanged(sender As Object, e As EventArgs) Handles chkOfflineHelp.CheckedChanged
        Try
            If mOfflineHelpEnabled = chkOfflineHelp.Checked Then
                Return
            End If

            mOfflineHelpEnabled = chkOfflineHelp.Checked

            If Not chkOfflineHelp.Checked Then

                If String.IsNullOrWhiteSpace(txtOfflineHelpBaseUrl.Text) Then
                    gSv.UpdateEnableOfflineHelp(chkOfflineHelp.Checked)
                Else
                    txtOfflineHelpBaseUrl.Text = String.Empty
                    gSv.UpdateOfflineHelpData(chkOfflineHelp.Checked, txtOfflineHelpBaseUrl.Text.Trim)
                End If

                txtOfflineHelpBaseUrl.Enabled = False
            Else
                gSv.UpdateEnableOfflineHelp(chkOfflineHelp.Checked)
                txtOfflineHelpBaseUrl.Enabled = True
            End If
        Catch ex As Exception
            UserMessage.Err(String.Format(My.Resources.ctlSystemSettings_ErrorUpdatingOfflineHelpInformation0, ex.Message), ex)
        End Try
    End Sub

    Private Sub TxtOfflineHelpBaseUrl_Validating(sender As Object, e As EventArgs) Handles txtOfflineHelpBaseUrl.Validating
        Try
            Dim url = String.Empty

            If chkOfflineHelp.Checked AndAlso Not String.IsNullOrEmpty(txtOfflineHelpBaseUrl.Text) Then
                Dim validUrl = New Uri(txtOfflineHelpBaseUrl.Text.Trim)
                url = validUrl.OriginalString
            End If

            gSv.UpdateOfflineHelpBaseUrl(url)
        Catch ex As Exception
            UserMessage.Err(String.Format(My.Resources.ctlSystemSettings_ErrorUpdatingOfflineHelpInformation0, ex.Message), ex)
        End Try
    End Sub

    Private Sub PopulateOfflineHelpSettings()
        Try
            mOfflineHelpEnabled = gSv.OfflineHelpEnabled()
            chkOfflineHelp.Checked = mOfflineHelpEnabled

            If Not chkOfflineHelp.Checked Then
                txtOfflineHelpBaseUrl.Enabled = False
            Else
                txtOfflineHelpBaseUrl.Text = gSv.GetOfflineHelpBaseUrl()
            End If
        Catch ex As Exception
            UserMessage.Err(String.Format(My.Resources.ctlSystemSettings_ErrorRetrievingOfflineHelpInformation0, ex.Message), ex)
        End Try
    End Sub

    Private Sub chkEnableEnvironmentRecording_CheckedChanged(sender As Object, e As EventArgs) Handles chkEnableEnvironmentRecording.CheckedChanged
        If mLoadingSettings Then Return
        gSv.UpdateEnableBpaEnvironmentDataSetting(chkEnableEnvironmentRecording.Checked)
    End Sub

#End Region

End Class
