Imports AutomateControls.Wizard
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports AutomateUI.Wizards.SystemManager.DataGateways.Helpers

Public Class ctlChooseDataType
    Inherits WizardPanel
    Implements IDataGatewaysWizardPanel

    Private ReadOnly mConfig As DataPipelineOutputConfig

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig)
        InitializeComponent()
        mConfig = dataPipelineConfigOutput

        chkSessionLogs.DataBindings.Add(New Binding("Checked", mConfig, "IsSessions"))
        chkPublishedDashboards.DataBindings.Add(New Binding("Checked", mConfig, "IsDashboards"))
        chkWqaSnapshotData.DataBindings.Add(New Binding("Checked", mConfig, "IsWqaSnapshotData"))
        chkCustomObjectData.DataBindings.Add(New Binding("Checked", mConfig, "IsCustomObjectData"))
    End Sub

    Private Sub RefreshLabels()
        lblSelectedConfigName.Text = mConfig.Name
        lblChooseDataToSend.Text = String.Format(My.Resources.lblChooseDataToSend_text, GetLocalizedFriendlyNameToLower(mConfig.OutputType.Id))
    End Sub

    Private Sub chkSessionLogs_CheckedChanged(sender As Object, e As EventArgs) Handles chkSessionLogs.CheckedChanged
        mConfig.IsSessions = chkSessionLogs.Checked
        IsInputCorrect()
    End Sub

    Private Sub chkPublishedDashboards_CheckedChanged(sender As Object, e As EventArgs) Handles chkPublishedDashboards.CheckedChanged
        mConfig.IsDashboards = chkPublishedDashboards.Checked
        IsInputCorrect()
    End Sub

    Private Sub chkWqaSnapshotData_CheckedChanged(sender As Object, e As EventArgs) Handles chkWqaSnapshotData.CheckedChanged
        mConfig.IsWqaSnapshotData = chkWqaSnapshotData.Checked
        IsInputCorrect()
    End Sub

    Private Sub chkCustomObjectData_CheckedChanged(sender As Object, e As EventArgs) Handles chkCustomObjectData.CheckedChanged
        mConfig.IsCustomObjectData = chkCustomObjectData.Checked
        IsInputCorrect()
    End Sub

    Private Sub IsInputCorrect()
        NavigateNext = mConfig.IsDashboards OrElse mConfig.IsSessions OrElse mConfig.IsCustomObjectData OrElse mConfig.IsWqaSnapshotData
        UpdateNavigate()
    End Sub

    Public Sub OnOpen() Implements IDataGatewaysWizardPanel.OnOpen
        RefreshLabels()
    End Sub

    Public Sub Closing() Implements IDataGatewaysWizardPanel.Closing
    End Sub


End Class
