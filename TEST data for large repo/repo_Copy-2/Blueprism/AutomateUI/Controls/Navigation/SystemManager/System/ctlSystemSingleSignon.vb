Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions

Public Class ctlSystemSingleSignon
    Implements IChild
    Implements IPermission
    Implements IHelp

    ''' <summary>
    ''' Indicates the AD Settings need saving
    ''' </summary>
    Private mNeedsSaving As Boolean = False

    Private WithEvents mConnectionManager As IResourceConnectionManager

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mADSettings.Populate()

        'Set apply button to be initially disabled (until changes are made to AD settings)
        EnableDisableApplyButton()

        'Once the form is populated, add handler to check for any more changes to
        'the AD settings
        AddHandler mADSettings.DomainChanged, AddressOf OnADSettingsChanged
        AddHandler mADSettings.AdminGroupPopulated, AddressOf OnADSettingsChanged

        pbWarningIcon.Image = ToolImages.Warning_16x16
        btnConvert.Enabled = User.Current.HasPermission(Permission.SystemManager.Security.Users) AndAlso
                             User.Current.HasPermission(Permission.SystemManager.Security.UserRoles)
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
            If mParent IsNot Nothing Then mConnectionManager = mParent.ConnectionManager
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Security - Sign-on Settings")
        End Get
    End Property

    Private Sub HandleSingleSignOnCommitted(
     ByVal sender As Object, ByVal e As EventArgs) Handles btnCommitChanges.Click
        Try
            Dim domChanged As Boolean
            If Me.mADSettings.CommitConfiguration(domChanged) Then
                gSv.SetSignonSettings(Me.mADSettings.txtActiveDirectoryDomain.Text,
                                      domChanged,
                                      Me.mADSettings.AdminGroupSid.Value,
                                      Me.mADSettings.txtAdminGroupName.Text,
                                      Me.mADSettings.txtAdminGroupPath.Text)
                Dim msg As New StringBuilder(My.Resources.ctlSystemSingleSignon_ConfigurationSaved)
                If domChanged Then msg.AppendLine(
                    My.Resources.ctlSystemSingleSignon_AsTheDomainHasChangedTheMappingBetweenBluePrismRolesAndActiveDirectorySecurityG).AppendLine()
                msg.Append(My.Resources.ctlSystemSingleSignon_AllBluePrismDevicesIncludingBluePrismServersMustBeRestartedForTheChangesToBeImm)
                UserMessage.Show(msg.ToString())

                mNeedsSaving = False
                'Set the apply button to be disabled again, as changes have been saved successfully
                EnableDisableApplyButton()
            End If
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSystemSingleSignon_UnexpectedError0, Ex.Message), Ex)
        End Try
    End Sub

    Private Sub btnConvert_Click(sender As Object, e As EventArgs) Handles btnConvert.Click
        If LoggedInUsersChecked() AndAlso RunningSessionsChecked() AndAlso UserCanLogout() Then
            Dim createAdminUserForm = New DatabaseConversionCreateNativeAdminUser()
            createAdminUserForm.StartPosition = FormStartPosition.CenterParent
            createAdminUserForm.SetEnvironmentColoursFromAncestor(mParent)

            Dim adminUser = createAdminUserForm.DisplayDialog()
            If adminUser Is Nothing Then Return

            Dim confirmationForm = New frmConfirmDatabaseConversion()
            confirmationForm.StartPosition = FormStartPosition.CenterParent
            confirmationForm.SetEnvironmentColoursFromAncestor(mParent)

            If confirmationForm.ShowDialog() <> DialogResult.OK Then Return

            Dim performConversionForm = New frmPerformDatabaseConversion(adminUser)
            performConversionForm.SetEnvironmentColours(mParent)
            performConversionForm.ShowDialog()
            If performConversionForm.ConversionSuccessful Then
                Dim currentForm = frmApplication.GetCurrent()
                If (Not currentForm Is Nothing) Then
                    currentForm.LogOutRequested()
                End If
            End If
        End If
    End Sub

    Private Sub OnADSettingsChanged(ByVal sender As Object, ByVal e As EventArgs)
        mNeedsSaving = True
        EnableDisableApplyButton()
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSingleSignonSettings.htm"
    End Function

    Private Sub EnableDisableApplyButton()
        btnCommitChanges.Enabled = mNeedsSaving
    End Sub

    Private Function LoggedInUsersChecked() As Boolean
        Dim loggedInUsers = gSv.GetLoggedInUsersAndMachines().Where(
            Function(x) (x.id <> User.CurrentId) OrElse (x.id = User.CurrentId AndAlso x.transient = True))
        If loggedInUsers.Any() Then
            UserMessage.OK(ctlSystemSingleSignon_CannotConvertDatabase_UsersAreLoggedIn)
            Return False
        End If
        Return True
    End Function

    Private Function RunningSessionsChecked() As Boolean
        Dim runningSessions = gSv.GetRunningSessions()
        If Not runningSessions.Any Then Return True

        If Not User.Current.HasPermission(Permission.Resources.ControlResource) Then
            UserMessage.OK(String.Format(ThereAre0SessionsRunningAndYouDoNotHavePermissionToStopThem, runningSessions.Count))
            Return False
        End If

        If UserMessage.YesNo(
                String.Format(ThereAre0SessionsRunningDoYouWishToStopThemToContinueTheDatabaseConversion, runningSessions.Count)
                ) = DialogResult.No Then
            Return False
        End If

        Dim errorMessage = String.Empty
        For Each session In runningSessions
            If Not TryStopSession(session, errorMessage) Then Return False
        Next
        Return True
    End Function

    Private Function UserCanLogout() As Boolean
        Dim logoutDeniedMessage = String.Empty
        If Not User.CurrentUserCanLogout(logoutDeniedMessage) Then
            If Not String.IsNullOrEmpty(logoutDeniedMessage) Then
                UserMessage.OK(UnableToLogOutWhileWorkIsInProgress)
            End If
            Return False
        End If
        Return True
    End Function

    Private Function TryStopSession(session As clsProcessSession, errorMessage As String) As Boolean
        Dim resourceOrPoolControllerId As Guid
        If Not session.GetTargetResourceID(resourceOrPoolControllerId, errorMessage) Then
            UserMessage.Show(errorMessage)
            Return False
        End If

        Try
            mConnectionManager.SendStopSession({New StopSessionData(resourceOrPoolControllerId, session.SessionID, 0)})
        Catch ex As Exception
            UserMessage.OK(String.Format(ctlSessionManagement_FailedToSendStopInstructionToResourcePC0, ex.Message))
            Return False
        End Try

        Return True
    End Function

End Class
