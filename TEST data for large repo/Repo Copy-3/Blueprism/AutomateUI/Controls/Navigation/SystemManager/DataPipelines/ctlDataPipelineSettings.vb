Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.DataPipeline
Imports BluePrism.DataPipeline.UI
Imports Internationalisation
Imports AutomateControls.Forms
Imports BluePrism.Core.Utility

Public Class ctlDataPipelineSettings : Implements IStubbornChild, IPermission, IHelp

    Private mSettings As DataPipelineSettingsDetails
    Private mNoCredentialSet As String = String.Empty
    Private ReadOnly mChangeTracker As IChangeTracker = New ChangeTracker()

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        mNoCredentialSet = $"<{My.Resources.None}>"

        RefreshUI()
        SetDefaultSort()

        RecordControlStateForChanges()
    End Sub

    Private Sub RecordControlStateForChanges()
        mChangeTracker.Reset()
        mChangeTracker.RecordValue(cbWriteToDatabase)
        mChangeTracker.RecordValue(cbEmitToDataGateways)
        mChangeTracker.RecordValue(cbSendPublishedDashboardToDataGateway)
        mChangeTracker.RecordValue(cbSendWorkQueueAnalysisToDataGateway)
        mChangeTracker.RecordValue(numFrequency)
        mChangeTracker.RecordValue(cboDGCredentials)
        mChangeTracker.RecordValue(radIntegratedSecurity)
        mChangeTracker.RecordValue(radSQLAuth)
        mChangeTracker.RecordValue(sqlServerDatagatewaysPort)
    End Sub

    Private Sub RefreshUI()
        mSettings = DataPipelineSettingsDetails.FromSettings(gSv.GetDataPipelineSettings())

        cbWriteToDatabase.Checked = mSettings.WriteSessionLogsToDatabase
        cbEmitToDataGateways.Checked = mSettings.EmitSessionLogsToDatagateways
        cbSendPublishedDashboardToDataGateway.Checked = mSettings.SendPublishedDashboardsToDatagateways
        cbSendWorkQueueAnalysisToDataGateway.Checked = mSettings.SendWorkQueueAnalysisToDatagateways

        dgvPublishedDashboards.Rows.Clear()
        For Each dashboardSetting In mSettings.PublishedDashboardSettings
            dgvPublishedDashboards.Rows.Add(dashboardSetting.DashboardId, dashboardSetting.DashboardName, dashboardSetting.PublishToDataGatewayInterval / 60)
        Next

        dgvPublishedDashboards.Enabled = cbSendPublishedDashboardToDataGateway.Checked
        numFrequency.Value = mSettings.MonitoringFrequency

        radSQLAuth.Checked = Not mSettings.UseIntegratedSecurity
        radIntegratedSecurity.Checked = mSettings.UseIntegratedSecurity

        cboDGCredentials.Items.Clear()

        Dim gatewaysCredentials = gSv.GetDataGatewayCredentials().ToArray()

        If String.IsNullOrEmpty(mSettings.DatabaseSqlUser) OrElse (Not gatewaysCredentials.Contains(mSettings.DatabaseSqlUser)) Then
            cboDGCredentials.Items.Add(mNoCredentialSet)
            cboDGCredentials.Items.AddRange(gatewaysCredentials)
            cboDGCredentials.SelectedItem = mNoCredentialSet
        Else
            cboDGCredentials.Items.AddRange(gatewaysCredentials)
            cboDGCredentials.SelectedItem = mSettings.DatabaseSqlUser
        End If

        sqlServerDatagatewaysPort.Value = mSettings.ServerPort

        UpdateSecuritySettingsDisplay()

        RecordControlStateForChanges()
    End Sub

    Private Sub UpdateSecuritySettingsDisplay()
        cboDGCredentials.Visible = Not radIntegratedSecurity.Checked
        lblWindowsAuthWarning.Visible = radIntegratedSecurity.Checked

        If radIntegratedSecurity.Checked = False And cboDGCredentials.Items.Count > 0 Then
            If String.IsNullOrEmpty(mSettings.DatabaseSqlUser) OrElse (Not cboDGCredentials.Items.Contains(mSettings.DatabaseSqlUser)) Then
                cboDGCredentials.SelectedItem = mNoCredentialSet
            Else
                cboDGCredentials.SelectedItem = mSettings.DatabaseSqlUser
            End If
        End If
    End Sub

    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.DataGateways.ImpliedConfiguration)
        End Get
    End Property

    Private Sub btnApply_Click(sender As Object, e As EventArgs) Handles btnApply.Click
        If Not HasChanged() Then Return
        AttemptSaveChanges()
        RefreshUI()
    End Sub

    Private Function AttemptSaveChanges() As Boolean
        Try
            Dim updateConfigFile As Boolean = False

            If (mSettings.UseIntegratedSecurity <> radIntegratedSecurity.Checked OrElse
               CStr(cboDGCredentials.SelectedItem) <> mSettings.DatabaseSqlUser OrElse
               CInt(sqlServerDatagatewaysPort.Value) <> mSettings.ServerPort) Then
                updateConfigFile = True
            End If

            Dim dashboards = mSettings.PublishedDashboardSettings.
                Where(Function(x) HasDashboardSettingChanged(x.DashboardId, x.PublishToDataGatewayInterval)).
                Select(Function(x) New PublishedDashboardSettings(x.DashboardId, x.DashboardName, GetUpdatedDashboardInterval(x.DashboardId), x.LastSent)).
                ToList()

            Dim databaseSqlUser = String.Empty
            If (Not radIntegratedSecurity.Checked) Then
                databaseSqlUser = If((cboDGCredentials.SelectedItem Is Nothing OrElse cboDGCredentials.SelectedItem.ToString() = mNoCredentialSet),
                    String.Empty, cboDGCredentials.SelectedItem.ToString())
            End If

            Dim settings = New DataPipelineSettings(cbWriteToDatabase.Checked, cbEmitToDataGateways.Checked,
                                                    CInt(numFrequency.Value), cbSendPublishedDashboardToDataGateway.Checked, dashboards,
                                                     cbSendWorkQueueAnalysisToDataGateway.Checked, databaseSqlUser, radIntegratedSecurity.Checked, Convert.ToInt32(sqlServerDatagatewaysPort.Value))
            settings.Validate()
            gSv.UpdateDataPipelineSettings(settings)
            RefreshUI()

            If (updateConfigFile = True) Then
                Dim newConfig = String.Empty
                If LogstashConfigHelper.TryGetLogstashConfig(newConfig) = False Then
                    Return True
                End If

                Dim dataPipelineConfiguration As DataPipelineProcessConfigDetails = DataPipelineProcessConfigDetails.FromConfig(gSv.GetConfigurationByName("Default"))
                If dataPipelineConfiguration Is Nothing Then dataPipelineConfiguration = New DataPipelineProcessConfigDetails() With {.Name = "Default"}
                dataPipelineConfiguration.LogstashConfigFile = newConfig
                gSv.SaveConfig(DataPipelineProcessConfigDetails.ToConfig(dataPipelineConfiguration))

                Dim popup As PopupForm
                popup = New PopupForm(My.Resources.frmDataGateways_This, My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg, My.Resources.btnOk)
                AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
                popup.StartPosition = FormStartPosition.CenterParent
                popup.ShowInTaskbar = False
                popup.ShowDialog()
            End If

            Return True
        Catch ex As Exception
            UserMessage.Err(String.Format(ResMan.GetString("ctlDataPipelineSettings_failedToSaveSettings_template"), ex.Message))
            Return False
        End Try
    End Function

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Private Function HasChanged() As Boolean
        Return mChangeTracker.HasChanged OrElse
                mSettings.PublishedDashboardSettings.Any(Function(x) HasDashboardSettingChanged(x.DashboardId, x.PublishToDataGatewayInterval))
    End Function


    Private Function HasDashboardSettingChanged(dashboardId As Guid, savedInterval As Integer) As Boolean
        Dim updatedInterval = GetUpdatedDashboardInterval(dashboardId)
        Return updatedInterval <> savedInterval
    End Function

    Private Function GetUpdatedDashboardInterval(dashboardId As Guid) As Integer
        Return dgvPublishedDashboards.Rows.Cast(Of DataGridViewRow).
            Where(Function(row) DirectCast(row.Cells("colId").Value, Guid) = dashboardId).
            Select(Function(x) CInt(x.Cells("colInterval").Value) * 60).
            Single()
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        If Not HasChanged() Then Return True
        Dim message As String =
            My.Resources.ctlDataPipelineConfiguration_ThereAreUnsavedChanges

        Dim response = UserMessage.YesNoCancel(message, True)
        Select Case response
            Case MsgBoxResult.No
                RefreshUI()
                Return True
            Case MsgBoxResult.Yes
                Return AttemptSaveChanges()
            Case Else : Return False
        End Select
    End Function

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "dg-settings.htm"
    End Function

    Private Sub cbSendPublishedDashboardToDataGateway_CheckedChanged(sender As Object, e As EventArgs) Handles cbSendPublishedDashboardToDataGateway.CheckedChanged
        dgvPublishedDashboards.Enabled = cbSendPublishedDashboardToDataGateway.Checked
    End Sub

    Private Sub dgvPublishedDashboards_EnabledChanged(sender As Object, e As EventArgs) Handles dgvPublishedDashboards.EnabledChanged
        SetPublishedDashboardDatagridDisabledColor(dgvPublishedDashboards.Enabled)
    End Sub

    Private Sub SetPublishedDashboardDatagridDisabledColor(enabled As Boolean)

        Dim controlColor As Color = SystemColors.Control
        Dim grayTextColor As Color = SystemColors.GrayText
        Dim windowColor As Color = SystemColors.Window
        Dim controlTextColor As Color = SystemColors.ControlText

        If Not dgvPublishedDashboards.Enabled Then
            dgvPublishedDashboards.SelectedRow = Nothing
            dgvPublishedDashboards.CurrentCell = Nothing
            dgvPublishedDashboards.ReadOnly = True
            dgvPublishedDashboards.EnableHeadersVisualStyles = False

            SetDataGridCellStyles(controlColor, grayTextColor)
        Else
            dgvPublishedDashboards.SelectedRow = Nothing
            dgvPublishedDashboards.ReadOnly = False
            dgvPublishedDashboards.EnableHeadersVisualStyles = True

            SetDataGridCellStyles(windowColor, controlTextColor)
        End If
    End Sub

    Private Sub SetDataGridCellStyles(backgroundColor As Color, foregroundColor As Color)

        dgvPublishedDashboards.DefaultCellStyle.BackColor = backgroundColor
        dgvPublishedDashboards.DefaultCellStyle.ForeColor = foregroundColor
        dgvPublishedDashboards.ColumnHeadersDefaultCellStyle.BackColor = backgroundColor
        dgvPublishedDashboards.ColumnHeadersDefaultCellStyle.ForeColor = foregroundColor
        dgvPublishedDashboards.DefaultCellStyle.SelectionBackColor = backgroundColor
        dgvPublishedDashboards.DefaultCellStyle.SelectionForeColor = foregroundColor
        dgvPublishedDashboards.RowsDefaultCellStyle.BackColor = backgroundColor
        dgvPublishedDashboards.RowsDefaultCellStyle.ForeColor = foregroundColor
        dgvPublishedDashboards.RowsDefaultCellStyle.SelectionBackColor = backgroundColor
        dgvPublishedDashboards.RowsDefaultCellStyle.SelectionForeColor = foregroundColor

    End Sub

    Private Sub SetDefaultSort()
        Dim dashboardNameCol = dgvPublishedDashboards.Columns.Item("colName")
        dgvPublishedDashboards.Sort(dashboardNameCol, ListSortDirection.Ascending)
    End Sub

    Private Sub radIntegratedSecurity_CheckedChanged_1(sender As Object, e As EventArgs) Handles radIntegratedSecurity.CheckedChanged
        UpdateSecuritySettingsDisplay()
    End Sub

    Private Sub radSQLAuth_CheckedChanged(sender As Object, e As EventArgs) Handles radSQLAuth.CheckedChanged
        UpdateSecuritySettingsDisplay()
    End Sub
End Class
