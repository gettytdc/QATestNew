Imports BluePrism.AutomateAppCore
Imports AutomateControls.Forms
Imports System.IO
Imports BluePrism.DatabaseInstaller
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.BPCoreLib

''' Project  : Automate
''' Class    : frmUpgradeDatabase
''' 
''' <summary>
''' A form to upgrade the database version.
''' </summary>
Friend Class UpgradeDatabaseForm

    Private Const SessionLogMigrationSizeLimitKB As Integer = 10_485_760
    Private Const SessionLogMigrationRowLimit As Long = 10_000_000

    ''' <summary>
    ''' The version to upgrade from
    ''' </summary>
    Private miCurVersion As Integer

    ''' <summary>
    ''' The version to upgraded to
    ''' </summary>
    Private miRequiredVersion As Integer

    ''' <summary>
    ''' The connection to upgrade
    ''' </summary>
    Public Property ConnectionSetting As clsDBConnectionSetting

    Private mInstaller As IInstaller

    Private mInstallerOptions As DatabaseInstallerOptions

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        Cursor.Current = Cursors.WaitCursor
        Try
            MyBase.OnLoad(e)
            Dim setting As clsDBConnectionSetting = Me.ConnectionSetting
            Dim factory = DependencyResolver.Resolve(Of Func(Of ISqlDatabaseConnectionSetting, TimeSpan, String, String, IInstaller))
            mInstaller = factory(
                setting.CreateSqlSettings(),
                Options.Instance.DatabaseInstallCommandTimeout,
                ApplicationProperties.ApplicationName,
                clsServer.SingleSignOnEventCode)

            Try
                miCurVersion = mInstaller.GetCurrentDBVersion()
                miRequiredVersion = mInstaller.GetRequiredDBVersion()
            Catch
                Throw new DatabaseUpdateAccessException(My.Resources.UpgradeDatabaseForm_CannotAccessDatabase)
            End Try
            AddHandler mInstaller.ReportProgress, AddressOf OnInstallerReportProgress

            Dim upgradeAvailable As Boolean =
                mInstaller.IsUpgradeAvailable(miCurVersion, miRequiredVersion)

            If mInstaller.SessionLogMigrationRequired(miCurVersion, miRequiredVersion) Then
                Try
                    SessionMigrationGroupBox.Visible = True
                    If mInstaller.GetSessionLogRowCount > SessionLogMigrationRowLimit OrElse
                        mInstaller.GetSessionLogSizeKB > SessionLogMigrationSizeLimitKB Then
                        NoMigrationRadio.Checked = True
                        MigrationRadio.Enabled = False
                        MigrationDescription.Enabled = False
                        MigrationWarningLabel.Visible = True
                    End If
                Catch
                    NoMigrationRadio.Checked = True
                    MigrationRadio.Enabled = False
                    MigrationDescription.Enabled = False
                    MigrationWarningLabel.Visible = False
                End Try
            Else
                SessionMigrationGroupBox.Visible = False
                Me.Height = 350
            End If

            'update user interface labels
            lblCurrentValue.Text =
             String.Format("{0}", IIf(miCurVersion = 0, My.Resources.Unknown, miCurVersion))

            lblRequiredValue.Text =
             String.Format("{0}", IIf(miRequiredVersion = 0, My.Resources.Unknown, miRequiredVersion))

            If Not upgradeAvailable Then
                btnUpgrade.Enabled = False
                btnGenerateScript.Enabled = False
                txtPassword.Enabled = False

                txtPassword.Visible = False
                lblPasswordPrompt.Text = My.Resources.NoUpgradeIsAvailableAtThisTime
            End If

            If setting.WindowsAuth Then
                txtPassword.Visible = False
                If upgradeAvailable Then _
                    lblPasswordPrompt.Visible = False
            Else
                'Upgrade is not available until password has been
                'typed. See also txtPassword TextChanged event
                btnUpgrade.Enabled = False
            End If
        Catch ex As Exception
            UserMessage.Err(ex.Message, ex)
            Me.Close()
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub

    Private Sub OnInstallerReportProgress(sender As Object, e As PercentageProgressEventArgs)
        mBackgroundWorker.ReportProgress(e.PercentProgress, e.Message)
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Close()
    End Sub

    Private mPd As ProgressDialog
    Private Sub btnUpgrade_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUpgrade.Click
        Try
            btnUpgrade.Enabled = False

            If mBackgroundWorker.IsBusy Then Return

            mBackgroundWorker.WorkerSupportsCancellation = True

            Dim setting As clsDBConnectionSetting = Me.ConnectionSetting
            If Not setting.WindowsAuth AndAlso Not setting.ConfirmDBPassword(txtPassword.SecurePassword) Then
                MessageBox.Show(String.Format(My.Resources.ThePasswordThatYouEnteredDoesNotMatchThePasswordEnteredInTheConnectionsDialogAv, vbCrLf, vbCrLf))
                Return
            End If

            If MigrationRadio.Checked AndAlso mInstaller.SessionLogMigrationRequired(miCurVersion, miRequiredVersion) Then
                mInstallerOptions = mInstallerOptions Or DatabaseInstallerOptions.MigrateSessionsPre65
            End If

            mPd = ProgressDialog.Prepare(Me, mBackgroundWorker, My.Resources.UpgradeDatabase, "", Me)
            mPd.ProgressDisplayFunction = AddressOf mInstaller.CreateProgressLabel
            mPd.OverrideCancel = True
            AddHandler mPd.OverriddenCancel, AddressOf OnCancelUpgrade
            AddHandler mInstaller.CancelProgress, AddressOf OnInstallerCancelProgress
            mPd.ShowInTaskbar = False
            mPd.ShowDialog(Me)

        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub OnInstallerCancelProgress(sender As Object, e As CancelProgressEventArgs)
        UpdateCancelProgress(e.Status)
    End Sub

    Private Sub txtPassword_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPassword.TextChanged

        btnUpgrade.Enabled = (txtPassword.SecurePassword.Length > 0)
    End Sub


    Private Sub OnCancelUpgrade(sender As Object, e As EventArgs)
        Me.Invoke(Sub() mPd.UpdateTitle(My.Resources.UpgradeCancelling))
        Me.Invoke(Sub() mPd.EnableCancel(False))
        mInstaller.IsCancelling = True
    End Sub

    Private Sub UpdateCancelProgress(status As CancelStatus)
        Select Case status
            Case CancelStatus.Cancelled
                Select Case (mPd.UpdateCancelProgress())
                    Case True
                        mInstaller.IsCancelled = True
                    Case Else
                        mInstaller.IsCancelled = False
                        mInstaller.IsCancelling = False
                End Select
            Case CancelStatus.CancelConfirmed
                Me.Invoke(Sub() mPd.UpdateTitle(My.Resources.UpgradeCancelled))
                Me.Invoke(Sub() mPd.EnableCancel(False))
            Case CancelStatus.Continued
                Me.Invoke(Sub() mPd.UpdateTitle(My.Resources.UpgradeDatabase))
                Me.Invoke(Sub() mPd.EnableCancel(True))
                mInstaller.IsCancelled = False

        End Select
    End Sub

    Private Sub mBackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles mBackgroundWorker.DoWork
        Dim setting As clsDBConnectionSetting = Me.ConnectionSetting

        mInstaller.ConfigurableDatabaseUpgrade(mInstallerOptions)

        'Rebuild dependency data (if required)
        ServerFactory.ClientInit(setting)
        gSv.RebuildDependencies()
    End Sub


    Private Sub mBackgroundWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles mBackgroundWorker.RunWorkerCompleted
        If e.Error Is Nothing Then
            MessageBox.Show(My.Resources.UpgradeSucceeded)
        Else
            MessageBox.Show(String.Format(My.Resources.UpgradeFailed0, e.Error.Message))
        End If
        Close()
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerateScript.Click
        Try
            If mSaveFileDialog.ShowDialog = DialogResult.OK Then
                Dim script = mInstaller.GenerateUpgradeScript()
                File.WriteAllText(mSaveFileDialog.FileName, script)
                Me.Close()
            End If
        Catch ex As Exception
            UserMessage.Err(ex)
        End Try
    End Sub

End Class
