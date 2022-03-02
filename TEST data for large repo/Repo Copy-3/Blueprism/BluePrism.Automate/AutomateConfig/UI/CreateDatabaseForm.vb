Imports AutomateControls
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports AutomateControls.Forms
Imports System.ComponentModel
Imports System.IO
Imports BluePrism.DatabaseInstaller
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.AutomateAppCore.Utility


Public Class CreateDatabaseForm
    Inherits HelpButtonForm

    ''' <summary>
    ''' Set this after construction, but before the Form is shown, to make this a
    ''' form for configuring a database, rather than creating a new one.
    ''' </summary>
    Public Property Configure As Boolean = False

    Public Property ConnectionSetting As clsDBConnectionSetting

    Private mPurgeExistingDB As Boolean
    Private mDomain As String
    Private mAdminGroupId As String
    Private mAdminGroupName As String
    Private mAdminGroupPath As String
    Private mInstaller As IInstaller

    Private Sub frmWizard_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        MyBase.OnPaint(e)
        GraphicsUtil.Draw3DLine(e.Graphics, New Point(0, btnCancel.Top - 10),
         ListDirection.LeftToRight, Width)
    End Sub

    Private Sub mBackgroundWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles mBackgroundWorker.DoWork

        Dim activeDirectoryOptions As DatabaseActiveDirectorySettings = Nothing

        If Not String.IsNullOrEmpty(mDomain) Then
            activeDirectoryOptions = New DatabaseActiveDirectorySettings(mDomain,
                                               mAdminGroupId,
                                               mAdminGroupName,
                                               mAdminGroupPath,
                                               Auth.Role.DefaultNames.SystemAdministrators)
        End If

        mInstaller.CreateDatabase(activeDirectoryOptions, mPurgeExistingDB, Configure, 0)
    End Sub

    Private Sub mBackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles mBackgroundWorker.RunWorkerCompleted
        If e.Error Is Nothing Then
            MessageBox.Show(If(Configure, My.Resources.DatabaseConfigured, My.Resources.DatabaseCreated))
            Close()
        Else
            UserMessage.Err(If(Configure, String.Format(My.Resources.FailedToConfigureDatabase0, e.Error.Message), String.Format(My.Resources.FailedToCreateDatabase0, e.Error.Message)), e.Error)
        End If
    End Sub


    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            If mBackgroundWorker.IsBusy Then Return

            Dim setting = Me.ConnectionSetting
            If Not setting.WindowsAuth AndAlso Not setting.ConfirmDBPassword(txtPassword.SecurePassword) Then
                MessageBox.Show(My.Resources.IncorrectPassword)
                txtPassword.Clear()
                txtPassword.Focus()
                Return
            End If

            If Not rdoMultiAuth.Checked Then
                'test the values entered for active directory before commiting them
                If Not objActiveDirectorySettings.VerifyDomain() Then Return
                If Not objActiveDirectorySettings.VerifyAdminGroup() Then Return
            End If

            mPurgeExistingDB = chkPurgeExistingDB.Checked

            If rdoSingleAuth.Checked Then
                mDomain = objActiveDirectorySettings.DomainName
                mAdminGroupId = objActiveDirectorySettings.AdminGroupSid.ToString
                mAdminGroupName = objActiveDirectorySettings.txtAdminGroupName.Text
                mAdminGroupPath = objActiveDirectorySettings.txtAdminGroupPath.Text
            End If


            Dim databaseExists = mInstaller.CheckDatabaseExists()
            If databaseExists Then
                Dim overWriteDatabasePopup = New YesNoCancelPopupForm( My.Resources.DatabaseExistsCaption, String.Format(My.Resources.DatabaseExistsDoYouWantToOverwrite, setting.DatabaseName))
                If overWriteDatabasePopup.ShowDialog() <> DialogResult.Yes Then return
            End If

            Using pd = ProgressDialog.Prepare(Me, mBackgroundWorker, My.Resources.CreateDatabaseForm_CreatingDatabase, "", Me)
                pd.ProgressDisplayFunction = AddressOf mInstaller.CreateProgressLabel
                pd.Width = 300
                pd.ShowInTaskbar = False
                pd.ShowDialog(Me)
            End Using
        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub OnAdminGroupPopulated() Handles objActiveDirectorySettings.AdminGroupPopulated
        EnableDisableOKButton()
    End Sub

    Private Sub EnableDisableOKButton()
        'Only enable for SSO if the Admin Group Name has been successfully retrieved 
        'from Active Directory
        btnOK.Enabled = (rdoMultiAuth.Checked OrElse
                         objActiveDirectorySettings.txtAdminGroupName.Text.Length > 0)

    End Sub

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        chkPurgeExistingDB.Checked = False
        With Me.ConnectionSetting
            If .WindowsAuth Then
                lblRetypePassword.Visible = False
                txtPassword.Visible = False
            Else
                txtPassword.PasswordChar = BPUtil.PasswordChar
                txtPassword.Select()
            End If

            lblConnectionName.Text = .ConnectionName
            lblDatabaseName.Text = .DatabaseName
        End With

        If Configure Then
            chkPurgeExistingDB.Visible = False
            lblExplanation.Text = My.Resources.TheDatabaseWillBeConfiguredAccordingToTheSettingsYouSupplyBelowOnceSetTheseCann
            titleBar.Title = My.Resources.ConfigureDatabase
            Me.Text = My.Resources.ConfigureDatabase
        End If

        EnableDisableOKButton()

    End Sub

    Private Sub RdoMultiAuth_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles rdoMultiAuth.CheckedChanged
        objActiveDirectorySettings.Enabled = False
        EnableDisableOKButton()
    End Sub

    Private Sub RdoSingleAuth_CheckedChanged(sender As Object, e As EventArgs) Handles rdoSingleAuth.CheckedChanged
        objActiveDirectorySettings.Enabled = True
        If objActiveDirectorySettings.Enabled Then
            Try
                objActiveDirectorySettings.Populate()
            Catch ex As Exception
                'No domain found for machine - so just allow user to specify the domain
            End Try
        End If
        EnableDisableOKButton()
    End Sub

    Private Sub CreateDatabaseForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim setting = Me.ConnectionSetting
        Try
            Dim dbSettings = setting.CreateSqlSettings()
            Dim factory = DependencyResolver.Resolve(Of Func(Of ISqlDatabaseConnectionSetting, TimeSpan, String, String, IInstaller))

            mInstaller = factory(
                    dbSettings,
                    Options.Instance.DatabaseInstallCommandTimeout,
                    ApplicationProperties.ApplicationName,
                    clsServer.SingleSignOnEventCode)

            AddHandler mInstaller.ReportProgress, AddressOf OnInstallerReportProgress

        Catch ex As Exception
            UserMessage.Err(ex)
            Me.Close()
        End Try

    End Sub

    Private Sub OnInstallerReportProgress(sender As Object, e As PercentageProgressEventArgs)
        mBackgroundWorker.ReportProgress(e.PercentProgress, e.Message)
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerateScript.Click
        Try
            If mSaveFileDialog.ShowDialog = DialogResult.OK Then
                Dim script = mInstaller.GenerateCreateScript()
                File.WriteAllText(mSaveFileDialog.FileName, script)
                Me.Close()
            End If
        Catch ex As Exception
            UserMessage.Err(ex)
        End Try
    End Sub

    Public Overrides Function GetHelpFile() As String
        Return "frmCreateDatabase.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OfflineHelpHelper.OpenHelpFile(Me, GetHelpFile())
        Catch ex As Exception
            UserMessage.Err(ex)
        End Try
    End Sub
End Class
