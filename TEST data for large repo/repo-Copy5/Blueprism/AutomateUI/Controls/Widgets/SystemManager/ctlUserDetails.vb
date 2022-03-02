Imports System.DirectoryServices
Imports System.DirectoryServices.ActiveDirectory
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.Core.ActiveDirectory
Imports BluePrism.Server.Domain.Models

Public Class ctlUserDetails : Implements IMenuButtonHandler

    Public Event RefreshRequested As EventHandler
    Public Event EditUserRequested As GroupMemberEventHandler
    Public Event DeleteUserRequested As GroupMemberEventHandler
    Public Event UnlockUserRequested As GroupMemberEventHandler

    Private mUser As UserGroupMember
    Private mCachedUser As User
    Private mRestartWorker As Boolean

    Public Sub New()
        InitializeComponent()

        Dim logonOptions = gSv.GetLogonOptions(Nothing)
        AuthType.Text = My.Resources.ctlUserDetails_AccountType

    End Sub

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property UserMember As UserGroupMember
        Get
            Return mUser
        End Get
        Set(value As UserGroupMember)
            mUser = value
            mCachedUser = Nothing
            UpdateView()
        End Set
    End Property

    Private ReadOnly Property AuthUser As User
        Get
            If mCachedUser Is Nothing AndAlso mUser IsNot Nothing Then
                mCachedUser = gSv.GetUser(mUser.IdAsGuid)
            End If
            Return mCachedUser
        End Get
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property MenuStrip As ContextMenuStrip _
     Implements IMenuButtonHandler.MenuStrip
        Get
            Return ctxUserMaintenance
        End Get
    End Property

    Protected Overridable Sub OnRefreshRequested(e As EventArgs)
        RaiseEvent RefreshRequested(Me, e)
    End Sub

    Protected Overridable Sub OnEditUserRequested(e As GroupMemberEventArgs)
        RaiseEvent EditUserRequested(Me, e)
    End Sub

    Protected Overridable Sub OnDeleteUserRequested(e As GroupMemberEventArgs)
        RaiseEvent DeleteUserRequested(Me, e)
    End Sub

    Protected Overridable Sub OnUnlockUserRequested(e As GroupMemberEventArgs)
        RaiseEvent UnlockUserRequested(Me, e)
    End Sub

    Friend Sub UpdateView()
        If mUser Is Nothing Then Exit Sub

        With mUser
            lblAuthType.Text = AuthUser.AuthType.ToLocalizedDisplayName()
            lblUserName.Text = .Name
            lblLastSignedIn.Text = .LastSignInDisplay

            If User.IsLoggedInto(DatabaseType.SingleSignOn) Then
                If fullNameWorker.IsBusy Then
                    mRestartWorker = True
                Else
                    fullNameWorker.RunWorkerAsync()
                End If
            Else
                lblPasswordExpiry.Text = .PasswordExpiryDisplay
                lblValidFrom.Text = .ValidFromDisplay
                lblValidTo.Text = .ValidToDisplay
                lblExternalId.Text = AuthUser.ExternalId
                lblExternalIdentityProvider.Text = AuthUser.IdentityProviderName
                lblIdentityProviderType.Text = AuthUser.IdentityProviderType
            End If

            UpdateViewByAuthType()

            pbUserIcon.Image = If(AuthUser.AuthType = AuthMode.AuthenticationServerServiceAccount,
                If(AuthUser.Deleted() OrElse AuthUser.IsLocked(), ComponentImages.Key_32x32_Disabled, ComponentImages.Key_32x32),
                ImageLists.Components_32x32.Images(.ImageKey))
        End With
    End Sub
    Private Sub UpdateViewByAuthType()
        Select Case AuthUser.AuthType
            Case AuthMode.Native
                pnlExternalAuth.Hide()
                pnlNativeAuth1.Show()
                pnlNativeAuth2.Show()
            Case AuthMode.External
                pnlExternalAuth.Show()
                pnlNativeAuth1.Hide()
                pnlNativeAuth2.Hide()
            Case AuthMode.ActiveDirectory,
                 AuthMode.MappedActiveDirectory
                pnlExternalAuth.Hide()
                pnlNativeAuth1.Show()
                pnlNativeAuth2.Hide()
            Case AuthMode.AuthenticationServer,
                 AuthMode.AuthenticationServerServiceAccount
                pnlExternalAuth.Hide()
                pnlNativeAuth1.Hide()
                pnlNativeAuth2.Hide()
        End Select
    End Sub

    Private Sub ctlUserDetails_Load(sender As Object, e As EventArgs) Handles Me.Load

        Dim databaseIsSingleSignOn = User.IsLoggedInto(DatabaseType.SingleSignOn)

        DeleteToolStripMenuItem.Visible = Not databaseIsSingleSignOn
        NewToolStripMenuItem.Visible = Not databaseIsSingleSignOn
        EditMenuItem.Visible = Not databaseIsSingleSignOn
        UnlockToolStripMenuItem.Visible = Not databaseIsSingleSignOn

        ADRefreshToolstripMenuItem.Visible = databaseIsSingleSignOn
        ADViewGroupMembershipToolStripMenuItem.Visible = databaseIsSingleSignOn

        UpdateViewByAuthType()
        pnlAD.Visible = databaseIsSingleSignOn
    End Sub

    Private Sub HandleContextMenuOpening() Handles ctxUserMaintenance.Opening
        ProcessAlertsToolStripMenuItem.Enabled =
            AuthUser IsNot Nothing AndAlso
            Licensing.License.CanUse(LicenseUse.ProcessAlerts) AndAlso
            AuthUser.HasPermission(Permission.ProcessAlerts.SubscribeToProcessAlerts) AndAlso
            Not AuthUser.Deleted

        If Not User.IsLoggedInto(DatabaseType.SingleSignOn) Then
            EditMenuItem.Enabled = True
            UnlockToolStripMenuItem.Enabled =
                mUser IsNot Nothing AndAlso mUser.IsLocked
            NewToolStripMenuItem.Enabled = False
            DeleteToolStripMenuItem.Enabled =
                mUser IsNot Nothing AndAlso
                Not mUser.IsDeleted AndAlso
                Not mUser.AuthenticationServiceAccount
        Else
            ADRefreshToolstripMenuItem.Enabled = False
            ADViewGroupMembershipToolStripMenuItem.Enabled = True
        End If

    End Sub

    Private Sub HandleDoWork(sender As Object, e As DoWorkEventArgs) _
        Handles fullNameWorker.DoWork

        Using gc = Forest.GetCurrentForest().FindGlobalCatalog()
            Using ds = gc.GetDirectorySearcher()
                ds.Filter = String.Format(
                    "(&(objectCategory=user)(|(userPrincipalName={0})))",
                    LdapEscaper.EscapeSearchTerm(mUser.Name))

                ds.PropertiesToLoad.Add("displayName")
                ds.PropertiesToLoad.Add("userPrincipalName")

                Dim sr As SearchResult = ds.FindOne()
                Try
                    Dim fullName = CStr(sr.Properties("displayName")(0))
                    e.Result = fullName
                Catch ex As Exception
                    Debug.Print(My.Resources.ctlUserDetails_FailedToGetDisplayName0, ex)
                    e.Result = String.Empty
                End Try

            End Using
        End Using
    End Sub

    Private Sub fullNameWorker_RunWorkerCompleted _
        (sender As Object, e As RunWorkerCompletedEventArgs) _
        Handles fullNameWorker.RunWorkerCompleted

        lblFullName.Text = e.Result.ToString

        If mRestartWorker AndAlso Not fullNameWorker.IsBusy Then
            mRestartWorker = False
            fullNameWorker.RunWorkerAsync()
        End If

    End Sub

    Private Sub EditToolStripMenuItem_Click() Handles EditMenuItem.Click
        OnEditUserRequested(New GroupMemberEventArgs(Me.UserMember))
    End Sub

    Private Sub ProcessAlertsToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles ProcessAlertsToolStripMenuItem.Click

        If frmAlertConfig.InstanceExists Then
            Dim alertForm = frmAlertConfig.GetInstance(
                AuthUser, frmAlertConfig.ViewMode.ProcessConfig)
            alertForm.Visible = True
            If alertForm.WindowState = FormWindowState.Minimized Then _
                alertForm.WindowState = FormWindowState.Normal
            alertForm.BringToFront()
        Else
            Dim alertForm As _
                    New frmAlertConfig(AuthUser, frmAlertConfig.ViewMode.ProcessConfig)
            alertForm.DisablePermissionChecking()
            ' Ensure that the form opens correctly - ie. that the user has the
            ' correct permissions to open it. If not, dispose of it immediately.
            If DirectCast(Me.ParentForm, frmApplication).StartForm(alertForm) = DialogResult.Abort Then _
                alertForm.Dispose()
        End If

    End Sub

    Private Sub UnlockToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles UnlockToolStripMenuItem.Click
        OnUnlockUserRequested(New GroupMemberEventArgs(UserMember))
    End Sub

    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles DeleteToolStripMenuItem.Click
        OnDeleteUserRequested(New GroupMemberEventArgs(UserMember))
    End Sub

    Private Sub ViewUserGroupsMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) _
        Handles ADViewGroupMembershipToolStripMenuItem.Click
        Try
            Using f As New frmUserGroupMembership(AuthUser.Id, AuthUser.Name)
                f.SetEnvironmentColours(DirectCast(Me.ParentForm, frmApplication))
                f.ShowInTaskbar = False
                f.ShowDialog()
            End Using
        Catch ex As Exception
            UserMessage.Show(String.Format("Unexpected error - {0}", ex.Message), ex)
        End Try
    End Sub

    Private Sub HandleRefreshClick(sender As Object, e As EventArgs) Handles menuRefresh.Click
        OnRefreshRequested(EventArgs.Empty)
    End Sub

End Class
