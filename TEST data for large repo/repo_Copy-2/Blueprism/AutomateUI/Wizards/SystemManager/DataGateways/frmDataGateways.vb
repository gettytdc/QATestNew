Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports AutomateControls.Forms
Imports AutomateControls.Wizard
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports BluePrism.AutomateAppCore.Utility

Public Class frmDataGateways
    Inherits StandardWizard
    Implements IEnvironmentColourManager, IHelp

    Public Property ShowAdvanced As Boolean = False

    Private mController As clsDataGatewaysWizardController = Nothing
    Private mSummaryPanel As ctlChooseSummary = Nothing

    Public Sub Setup(dataPipelineConfigOutput As DataPipelineOutputConfig, dataGatawayCredentialNames As IList(Of String), existingConfigNames As IList(Of String))
        Text = My.Resources.frmDataGateways_title

        mController = New clsDataGatewaysWizardController(dataPipelineConfigOutput)
        mController.SetDialog(Me)

        Dim ot As New ctlChooseOutputType(dataPipelineConfigOutput, dataGatawayCredentialNames, existingConfigNames)
        mController.AddPanel(ot)

        Dim dt As New ctlChooseDataType(dataPipelineConfigOutput)
        mController.AddPanel(dt)

        Dim sl As New ctlChooseSessionLogFields(dataPipelineConfigOutput)
        mController.AddPanel(sl)

        Dim pd As New ctlChoosePublishedDashboards(dataPipelineConfigOutput, gSv.GetDashboardList())
        mController.AddPanel(pd)

        mSummaryPanel = New ctlChooseSummary(dataPipelineConfigOutput, mController)
        mController.AddPanel(mSummaryPanel)

        mController.StartWizard()
    End Sub

    Public Overrides Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "data-gateways.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return Bluebar.BackColor
        End Get
        Set(value As Color)
            Bluebar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return Bluebar.TitleColor
        End Get
        Set(value As Color)
            Bluebar.TitleColor = value
        End Set
    End Property

    Public Overrides Sub OnClosing(sender As Object, e As FormClosingEventArgs)

        ShowAdvanced = mSummaryPanel.ShowAdvancedConfig

        If Not ShowAdvanced AndAlso DialogResult = DialogResult.Cancel Then
            Dim confirmationForm As New YesNoPopupForm(
                My.Resources.frmDataGateways_This,
                My.Resources.frmDataGateways_CancelConfirm, String.Empty)
            confirmationForm.ShowInTaskbar = False
            confirmationForm.ShowDialog()
            If confirmationForm.DialogResult = DialogResult.No Then
                e.Cancel = True
            End If
        End If
    End Sub

End Class
