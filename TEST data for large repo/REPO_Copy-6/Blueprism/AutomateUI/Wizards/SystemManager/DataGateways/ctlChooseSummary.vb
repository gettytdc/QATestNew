Imports AutomateControls.Wizard
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class ctlChooseSummary
    Inherits WizardPanel
    Implements IDataGatewaysWizardPanel, IShowAdvancedConfigOption

    Private ReadOnly mConfig As DataPipelineOutputConfig

    Private mAdvanced As Boolean
    Private mController As clsDataGatewaysWizardController

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig, controller As clsDataGatewaysWizardController)
        InitializeComponent()
        mConfig = dataPipelineConfigOutput
        mController = controller
        btnAdvanced.Enabled = User.Current.HasPermission(Permission.SystemManager.DataGateways.AdvancedConfiguration)
    End Sub

    Public Sub OnOpen() Implements IDataGatewaysWizardPanel.OnOpen
        txtSummary.Text = mConfig.GetLogstashConfig()
    End Sub

    Public Sub Closing() Implements IDataGatewaysWizardPanel.Closing
    End Sub

    Private Sub btnAdvanced_Click(sender As Object, e As EventArgs) Handles btnAdvanced.Click
        mAdvanced = True
        mController.Finish()
    End Sub

    Public Function ShowAdvancedConfig() As Boolean Implements IShowAdvancedConfigOption.ShowAdvancedConfig
        Return mAdvanced
    End Function

End Class
