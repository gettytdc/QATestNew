Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports LocaleTools

Public Class ctlSecurityOptions
    Implements IStubbornChild
    Implements IPermission

    Private ReadOnly mPasswordRules As PasswordRules
    Private ReadOnly mLogonOptions As LogonOptions
    Private ReadOnly mUseAuthenticationServerSignIn As Boolean
    Private ReadOnly mChangeTracker As IChangeTracker = New ChangeTracker()
    Private ReadOnly mOAuth2ClientCredentials As String = "OAuth2ClientCredentials"

    ''' <summary>
    ''' Class to wrap a warning interval for use in a combo box.
    ''' </summary>
    Private Class WarningInterval
        Private ReadOnly mDays As Integer
        Private ReadOnly mFormatString As String
        Private mCache As String
        Public Sub New(ByVal days As Integer)
            Me.New(days, My.Resources.ctlSecurityOptions_plural_InTheNextDay)
        End Sub
        Public Sub New(ByVal days As Integer, ByVal formatString As String)
            mDays = days
            mFormatString = formatString
        End Sub
        Public ReadOnly Property Days() As Integer
            Get
                Return mDays
            End Get
        End Property
        Public Overrides Function ToString() As String
            If mCache Is Nothing Then
                mCache = LTools.Format(mFormatString, "COUNT", mDays)
            End If
            Return mCache
        End Function
    End Class

    ''' <summary>
    ''' The interval objects mapped against the number of days that the object
    ''' represents.
    ''' </summary>
    Private Shared ReadOnly sIntervals As IDictionary(Of Integer, WarningInterval) =
     GenerateIntervals()

    ''' <summary>
    ''' Generates the intervals required for this control
    ''' </summary>
    ''' <returns>A readonly map of warning intervals mapped against the interval (in
    ''' days) that they represent.</returns>
    Private Shared Function GenerateIntervals() As IDictionary(Of Integer, WarningInterval)
        Dim map As New clsOrderedDictionary(Of Integer, WarningInterval)
        map(0) = New WarningInterval(0, "")
        For Each i As Integer In New Integer() {1, 2, 3, 4, 5, 6, 7, 14}
            map(i) = New WarningInterval(i)
        Next
        Return GetReadOnly.IDictionary(map)

    End Function
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        cmbExpiryWarningInterval.Items.AddRange(
           CollectionUtil.ToArray(Of Object, WarningInterval)(sIntervals.Values))

        gSv.GetSignonSettings(mPasswordRules, mLogonOptions)

        PopulatePasswordExpiryInterval()
        PopulateLoginAttempts()
        PopulateSignInOptions()
        PopulatePasswordRules()
        PopulateCredentialList()
        PopulateAuthenticationTypes()
        RecordControlStateForChanges()
    End Sub

    Private Sub PopulatePasswordRules()
        chkUpperCase.Checked = mPasswordRules.UseUpperCase
        chkLowerCase.Checked = mPasswordRules.UseLowerCase
        chkDigits.Checked = mPasswordRules.UseDigits
        chkSpecial.Checked = mPasswordRules.UseSpecialCharacters
        chkBrackets.Checked = mPasswordRules.UseBrackets

        chkPasswordMinLength.Checked = mPasswordRules.PasswordLength > 0
        EnablePasswordLength()
        updnPasswordMinLength.Value = mPasswordRules.PasswordLength

        txtAdditional.Text = mPasswordRules.AdditionalCharacters

        chkNoRepeats.Checked = mPasswordRules.noRepeats
        chkNoRepeatsDays.Checked = mPasswordRules.noRepeatsDays

        If mPasswordRules.noRepeats Then
            updnNumberOfRepeats.Value = mPasswordRules.numberOfRepeats
        End If

        If mPasswordRules.noRepeatsDays Then
            updnNumberOfDays.Value = mPasswordRules.numberOfDays
        End If

        EnableNumberOfRepeatsOrDays()

        AddHandler chkUpperCase.CheckedChanged, AddressOf UpdatePasswordRules
        AddHandler chkLowerCase.CheckedChanged, AddressOf UpdatePasswordRules
        AddHandler chkDigits.CheckedChanged, AddressOf UpdatePasswordRules
        AddHandler chkSpecial.CheckedChanged, AddressOf UpdatePasswordRules
        AddHandler chkBrackets.CheckedChanged, AddressOf UpdatePasswordRules
        AddHandler chkPasswordMinLength.CheckedChanged, AddressOf UpdatePasswordRules
        AddHandler updnPasswordMinLength.ValueChanged, AddressOf UpdatePasswordRules
        AddHandler updnPasswordMinLength.KeyPress, AddressOf UpdatePasswordRules
        AddHandler txtAdditional.KeyPress, AddressOf UpdatePasswordRules
        AddHandler txtAdditional.Validated, AddressOf UpdatePasswordRules
        AddHandler updnNumberOfRepeats.ValueChanged, AddressOf UpdatePasswordRules
        AddHandler updnNumberOfDays.ValueChanged, AddressOf UpdatePasswordRules
        AddHandler updnNumberOfRepeats.KeyPress, AddressOf UpdatePasswordRules
        AddHandler updnNumberOfDays.KeyPress, AddressOf UpdatePasswordRules
        AddHandler chkNoRepeats.CheckedChanged, AddressOf chkNoRepeatsOrDays_CheckedChanged
        AddHandler chkNoRepeatsDays.CheckedChanged, AddressOf chkNoRepeatsOrDays_CheckedChanged
    End Sub


    Private Sub UpdatePasswordRules(ByVal sender As Object, ByVal e As EventArgs)
        mPasswordRules.UseUpperCase = chkUpperCase.Checked
        mPasswordRules.UseLowerCase = chkLowerCase.Checked
        mPasswordRules.UseDigits = chkDigits.Checked
        mPasswordRules.UseSpecialCharacters = chkSpecial.Checked
        mPasswordRules.UseBrackets = chkBrackets.Checked

        If chkPasswordMinLength.Checked Then
            mPasswordRules.PasswordLength = CInt(updnPasswordMinLength.Value)
        Else
            mPasswordRules.PasswordLength = 0
        End If
        EnablePasswordLength()

        mPasswordRules.AdditionalCharacters = txtAdditional.Text

        mPasswordRules.noRepeats = chkNoRepeats.Checked
        mPasswordRules.noRepeatsDays = chkNoRepeatsDays.Checked

        If mPasswordRules.noRepeats Then
            mPasswordRules.numberOfRepeats = CInt(updnNumberOfRepeats.Value)
        Else
            mPasswordRules.numberOfRepeats = 0
        End If

        If mPasswordRules.noRepeatsDays Then
            mPasswordRules.numberOfDays = CInt(updnNumberOfDays.Value)
        Else
            mPasswordRules.numberOfDays = 0
        End If
        SettingsChanged()
    End Sub

    Private Sub ResetUpDown(control As NumericUpDown)
        control.ReadOnly = True
        control.Enabled = False
        control.Value = control.Minimum
    End Sub

    Private Sub SettingsChanged()
        btnApply.Enabled = mChangeTracker.HasChanged()
    End Sub

    Private Sub PopulateCredentialList()
        Dim clientCredentials = New List(Of clsCredential)(gSv.GetAllCredentialsInfo())
        Dim clientCredentialOptions = (From c In clientCredentials
                                       Where c.Type.Name = mOAuth2ClientCredentials
                                       Select New KeyValuePair(Of String, Guid)(c.Name, c.ID)).ToList()

        Dim blankOption As New List(Of KeyValuePair(Of String, Guid)) From {New KeyValuePair(Of String, Guid)("", Guid.Empty)}

        cmbSelectCredential.DataSource = blankOption.Union(clientCredentialOptions).ToList()
        cmbSelectCredential.DisplayMember = "Key"
        cmbSelectCredential.ValueMember = "Value"
    End Sub

    Private Sub PopulatePasswordExpiryInterval()
        Try
            cmbExpiryWarningInterval.SelectedItem = sIntervals(mPasswordRules.PasswordExpiryInterval)

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityOptions_ErrorWhilstRetrievingPasswordExpiryWarning0, ex.Message), ex)
            cmbExpiryWarningInterval.SelectedIndex = 0

        End Try

        cmbExpiryWarningInterval.Enabled = cmbExpiryWarningInterval.SelectedIndex > 0
        chkExpiryWarning.Checked = cmbExpiryWarningInterval.Enabled

        AddHandler cmbExpiryWarningInterval.SelectedIndexChanged, AddressOf cmbExpiryWarningInterval_SelectedIndexChanged
        AddHandler chkExpiryWarning.CheckedChanged, AddressOf chkExpiryWarning_CheckedChanged

    End Sub

    Private Sub PopulateLoginAttempts()
        Dim attempts As Integer? = mPasswordRules.MaxAttempts
        If attempts.HasValue Then
            chkUseLoginAttempts.Checked = True
            updnLoginAttempts.Minimum = 1
            updnLoginAttempts.Value = attempts.Value
        Else
            chkUseLoginAttempts.Checked = False
            updnLoginAttempts.Minimum = 0
            updnLoginAttempts.Value = 0
            updnLoginAttempts.ReadOnly = True
            updnLoginAttempts.Enabled = False
        End If
        AddHandler chkUseLoginAttempts.CheckedChanged, AddressOf chkUseLoginAttempts_CheckedChanged
        AddHandler updnLoginAttempts.ValueChanged, AddressOf updnLoginAttempts_ValueChanged
        AddHandler updnLoginAttempts.KeyPress, AddressOf updnLoginAttempts_TypedNewValue
    End Sub

    Private Sub chkUseLoginAttempts_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
        If chkUseLoginAttempts.Checked Then
            updnLoginAttempts.Minimum = 1
            updnLoginAttempts.Value = 1
            updnLoginAttempts.ReadOnly = False
            updnLoginAttempts.Enabled = True
            mPasswordRules.MaxAttempts = CInt(updnLoginAttempts.Value)
        Else
            updnLoginAttempts.Minimum = 0
            updnLoginAttempts.Value = 0
            updnLoginAttempts.ReadOnly = True
            updnLoginAttempts.Enabled = False
            mPasswordRules.MaxAttempts = Nothing
        End If
        SettingsChanged()
    End Sub

    Private Sub updnLoginAttempts_ValueChanged(ByVal sender As Object, ByVal e As EventArgs)
        If chkUseLoginAttempts.Checked Then
            mPasswordRules.MaxAttempts = CInt(updnLoginAttempts.Value)
        Else
            mPasswordRules.MaxAttempts = Nothing
        End If
        SettingsChanged()
    End Sub

    Private Sub updnLoginAttempts_TypedNewValue(ByVal sender As Object, ByVal e As EventArgs)
        mPasswordRules.MaxAttempts = If(chkUseLoginAttempts.Checked, CInt(updnLoginAttempts.Value), Nothing)
        SettingsChanged()
    End Sub

    Private Sub cmbExpiryWarningInterval_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim interval As WarningInterval =
             DirectCast(cmbExpiryWarningInterval.SelectedItem, WarningInterval)
            mPasswordRules.PasswordExpiryInterval = interval.Days

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityOptions_ErrorWhilstUpdatingDatabase0, ex.Message), ex)
        End Try

        RemoveHandler cmbExpiryWarningInterval.SelectedIndexChanged, AddressOf cmbExpiryWarningInterval_SelectedIndexChanged
        RemoveHandler chkExpiryWarning.CheckedChanged, AddressOf chkExpiryWarning_CheckedChanged

        cmbExpiryWarningInterval.Enabled = cmbExpiryWarningInterval.SelectedIndex > 0
        chkExpiryWarning.Checked = cmbExpiryWarningInterval.Enabled

        AddHandler cmbExpiryWarningInterval.SelectedIndexChanged, AddressOf cmbExpiryWarningInterval_SelectedIndexChanged
        AddHandler chkExpiryWarning.CheckedChanged, AddressOf chkExpiryWarning_CheckedChanged

        SettingsChanged()
    End Sub

    Private Sub chkExpiryWarning_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        EnableComboBox(chkExpiryWarning, cmbExpiryWarningInterval, cmbExpiryWarningInterval.Items.Count - 1)
        SettingsChanged()
    End Sub

    Private Sub EnableComboBox(ByVal CheckBox As CheckBox, ByVal ComboBox As ComboBox, Optional ByVal DefaultIndex As Integer = 1)
        If CheckBox.Checked Then
            ComboBox.Enabled = True
            ComboBox.SelectedIndex = DefaultIndex
        Else
            ComboBox.Enabled = False
            ComboBox.SelectedIndex = 0
        End If
    End Sub

    Private Sub PopulateSignInOptions()

        Try
            cmbSignIn.SelectedIndex = CInt(mLogonOptions.AutoPopulate)
        Catch ex As Exception
            UserMessage.Show(ex.Message, ex)
            cmbSignIn.SelectedIndex = 0
        End Try
        cmbSignIn.Enabled = cmbSignIn.SelectedIndex > 0
        chkSignIn.Checked = cmbSignIn.Enabled

        chkShowUsersOnLogin.Checked = mLogonOptions.ShowUserList

        AddHandler cmbSignIn.SelectedIndexChanged, AddressOf cmbSignIn_SelectedIndexChanged
        AddHandler chkSignIn.CheckedChanged, AddressOf chkSignIn_CheckedChanged
        AddHandler chkShowUsersOnLogin.CheckedChanged, AddressOf chkShowUsersOnLogin_CheckedChanged

    End Sub

    Private Sub cmbSignIn_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)

        Try
            mLogonOptions.AutoPopulate = DirectCast(cmbSignIn.SelectedIndex, AutoPopulateMode)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityOptions_ErrorSettingLogonOptions0, ex.Message), ex)
        End Try

        RemoveHandler cmbSignIn.SelectedIndexChanged, AddressOf cmbSignIn_SelectedIndexChanged
        RemoveHandler chkSignIn.CheckedChanged, AddressOf chkSignIn_CheckedChanged

        cmbSignIn.Enabled = cmbSignIn.SelectedIndex > 0
        chkSignIn.Checked = cmbSignIn.Enabled

        AddHandler cmbSignIn.SelectedIndexChanged, AddressOf cmbSignIn_SelectedIndexChanged
        AddHandler chkSignIn.CheckedChanged, AddressOf chkSignIn_CheckedChanged

        SettingsChanged()
    End Sub

    Private Sub chkSignIn_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        EnableComboBox(chkSignIn, cmbSignIn, cmbSignIn.Items.Count - 1)
        SettingsChanged()
    End Sub

    Private Sub chkShowUsersOnLogin_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        mLogonOptions.ShowUserList = Me.chkShowUsersOnLogin.Checked
        SettingsChanged()
    End Sub

    Private Sub EnablePasswordLength()
        If chkPasswordMinLength.Checked Then
            updnPasswordMinLength.Minimum = 1
            updnPasswordMinLength.Value = mPasswordRules.PasswordLength
            updnPasswordMinLength.ReadOnly = False
            updnPasswordMinLength.Enabled = True
        Else
            updnPasswordMinLength.Minimum = 0
            updnPasswordMinLength.Value = 0
            updnPasswordMinLength.ReadOnly = True
            updnPasswordMinLength.Enabled = False
        End If
    End Sub

    Private Sub chkNoRepeatsOrDays_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        EnableNumberOfRepeatsOrDays()
        UpdatePasswordRules(sender, e)
    End Sub

    Private Sub EnableNumberOfRepeatsOrDays()
        updnNumberOfRepeats.Enabled = chkNoRepeats.Checked
        updnNumberOfDays.Enabled = chkNoRepeatsDays.Checked
    End Sub

    Private Sub PopulateAuthenticationTypes()
        PopulateActiveDirectorySettings()
        PopulateAuthenticationServerSettings()
    End Sub

    Private Sub PopulateActiveDirectorySettings()
        chkEnableActiveDirectoryAuth.Checked = mLogonOptions.MappedActiveDirectoryAuthenticationEnabled
        AddHandler chkEnableActiveDirectoryAuth.CheckedChanged, AddressOf HandleActiveDirectoryAuthSettingsChanged
    End Sub

    Private Sub PopulateAuthenticationServerSettings()
        chkUseAuthServer.Checked = mLogonOptions.AuthenticationServerAuthenticationEnabled
        txtAuthServerUrl.Text = mLogonOptions.AuthenticationServerUrl
        cmbSelectCredential.SelectedValue = If(mLogonOptions.AuthenticationServerApiCredentialId, Guid.Empty)

        AddHandler chkUseAuthServer.CheckedChanged, AddressOf HandleAuthenticationServerAuthSettingsChanged
        AddHandler txtAuthServerUrl.TextChanged, AddressOf HandleAuthenticationServerAuthSettingsChanged
        AddHandler cmbSelectCredential.SelectedIndexChanged, AddressOf HandleAuthenticationServerAuthSettingsChanged
    End Sub

    Private Sub RecordControlStateForChanges()
        mChangeTracker.Reset()
        mChangeTracker.RecordValue(chkUpperCase)
        mChangeTracker.RecordValue(chkLowerCase)
        mChangeTracker.RecordValue(chkDigits)
        mChangeTracker.RecordValue(chkSpecial)
        mChangeTracker.RecordValue(chkBrackets)
        mChangeTracker.RecordValue(txtAdditional)

        mChangeTracker.RecordValue(chkPasswordMinLength)
        mChangeTracker.RecordValue(updnPasswordMinLength)
        mChangeTracker.RecordValue(chkNoRepeats)
        mChangeTracker.RecordValue(updnNumberOfRepeats)
        mChangeTracker.RecordValue(chkNoRepeatsDays)
        mChangeTracker.RecordValue(updnNumberOfDays)

        mChangeTracker.RecordValue(chkShowUsersOnLogin)
        mChangeTracker.RecordValue(chkSignIn)
        mChangeTracker.RecordValue(cmbSignIn)

        mChangeTracker.RecordValue(chkUseLoginAttempts)
        mChangeTracker.RecordValue(updnLoginAttempts)
        mChangeTracker.RecordValue(chkExpiryWarning)
        mChangeTracker.RecordValue(cmbExpiryWarningInterval)

        mChangeTracker.RecordValue(chkEnableActiveDirectoryAuth)

        mChangeTracker.RecordValue(chkUseAuthServer)
        mChangeTracker.RecordValue(txtAuthServerUrl)
        mChangeTracker.RecordValue(cmbSelectCredential)
    End Sub

    Private Sub HandleAuthenticationServerAuthSettingsChanged(sender As Object, e As EventArgs)
        mLogonOptions.AuthenticationServerAuthenticationEnabled = chkUseAuthServer.Checked
        mLogonOptions.AuthenticationServerUrl = txtAuthServerUrl.Text
        mLogonOptions.AuthenticationServerApiCredentialId = CType(cmbSelectCredential.SelectedValue(), Guid)
    End Sub

    Private Sub HandleActiveDirectoryAuthSettingsChanged(sender As Object, e As EventArgs)

        If chkEnableActiveDirectoryAuth.Checked Then
            Try
                DirectoryServices.ActiveDirectory.Domain.GetComputerDomain()
            Catch ex As Exception
                chkEnableActiveDirectoryAuth.Checked = False
                UserMessage.Show(My.Resources.ctlSecurityOptions_NotMemberOfAnActiveDirectoryDomain, ex)
            End Try
        End If

        mLogonOptions.MappedActiveDirectoryAuthenticationEnabled = chkEnableActiveDirectoryAuth.Checked
        SettingsChanged()

    End Sub

    Private Sub btnApply_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnApply.Click
        SaveChanges()
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Security - Sign-on Settings")
        End Get
    End Property

    Private Function SaveChanges() As Boolean
        Try
            SetAuthenticationServerUrlToLowerCase()
            gSv.SetSignonSettings(mPasswordRules, mLogonOptions)
            btnApply.Enabled = False
            gpLoginOptions.Enabled = Not mLogonOptions.AuthenticationServerAuthenticationEnabled
            RecordControlStateForChanges()
            Return True
        Catch invalidUrlException As InvalidUrlException When invalidUrlException.Message = AuthMode.AuthenticationServer.ToString()
            UserMessage.OK(My.Resources.AuthenticationServerUrlIsNotAValidFormat)
        Catch disableExternalAuthException As CannotDisableAuthTypeException When disableExternalAuthException.AuthType = AuthMode.AuthenticationServer
            UserMessage.OK(String.Format(My.Resources._0CannotBeTurnedOffWhilstActiveUsersWithThisAuthType, My.Resources.AuthenticationServer))
        Catch disableMappedAdAuthException As CannotDisableAuthTypeException When disableMappedAdAuthException.AuthType = AuthMode.MappedActiveDirectory
            UserMessage.OK(String.Format(My.Resources._0CannotBeTurnedOffWhilstActiveUsersWithThisAuthType, My.Resources.ActiveDirectoryAuthentication))
        Catch blankUrlException As BlankUrlException When blankUrlException.Message = AuthMode.AuthenticationServer.ToString()
            UserMessage.OK(My.Resources.PleaseEnterAnAuthenticationServerUrl)
        Catch invalidProtocolException As InvalidProtocolException When invalidProtocolException.Message = AuthMode.AuthenticationServer.ToString()
            UserMessage.OK(My.Resources.AuthenticationServerUrlIsNotSecuredOverHttps)
        Catch ex As Exception
            UserMessage.Err(ex.Message)
        End Try

        Return False
    End Function

    Private Sub SetAuthenticationServerUrlToLowerCase()
        txtAuthServerUrl.Text = txtAuthServerUrl.Text.ToLower()
        mLogonOptions.AuthenticationServerUrl = txtAuthServerUrl.Text.ToLower()
    End Sub

    Private Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        ' Check if there are any unsaved changes
        If Not mChangeTracker.HasChanged() Then Return True

        Dim warning As String =
            My.Resources.ctlSecuirtyOptions_ThereAreUnsavedChangesToTheSignOnSettingsDoYouWantToSaveTheChanges

        ' And see what they want to do
        Dim popup = New AutomateControls.Forms.YesNoCancelPopupForm(My.Resources.PopupForm_UnsavedChanges, warning, String.Empty)
        Select Case popup.ShowDialog()
            Case DialogResult.No
                RecordControlStateForChanges()
                Return True
            Case DialogResult.Yes : Return SaveChanges()
            Case Else : Return False
        End Select

    End Function

    Private Sub ChkPasswordMinLength_CheckedChanged(sender As Object, e As EventArgs) Handles chkPasswordMinLength.CheckedChanged
        If Not chkPasswordMinLength.Checked Then
            ResetUpDown(updnPasswordMinLength)
        End If
    End Sub

    Private Sub ChkNoRepeats_CheckedChanged(sender As Object, e As EventArgs) Handles chkNoRepeats.CheckedChanged
        If Not chkNoRepeats.Checked Then
            ResetUpDown(updnNumberOfRepeats)
        End If
    End Sub

    Private Sub ChkNoRepeatsDays_CheckedChanged(sender As Object, e As EventArgs) Handles chkNoRepeatsDays.CheckedChanged
        If Not chkNoRepeatsDays.Checked Then
            ResetUpDown(updnNumberOfDays)
        End If
    End Sub

    Private Sub chkUseAuthServer_CheckedChanged(sender As Object, e As EventArgs) Handles chkUseAuthServer.CheckedChanged
        SettingsChanged()
    End Sub

    Private Sub txtAuthServerUrl_TextChanged(sender As Object, e As EventArgs) Handles txtAuthServerUrl.TextChanged
        SettingsChanged()
    End Sub

    Private Sub cmbSelectCredential_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSelectCredential.SelectedIndexChanged
        SettingsChanged()
    End Sub
End Class
