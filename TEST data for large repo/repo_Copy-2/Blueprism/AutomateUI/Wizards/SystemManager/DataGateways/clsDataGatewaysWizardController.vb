Imports AutomateControls.Forms
Imports AutomateControls.Wizard
Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class clsDataGatewaysWizardController
    Inherits WizardController

    Friend Property Config As DataPipelineOutputConfig
    Friend Property SelectedDashboards As List(Of String)
    Friend Property ShowAdvanced As Boolean

    Private Enum WizardControlIndices
        ChooseOutputType = 0
        ChooseDataType
        ChooseSessionLogFields
        ChooseDashboards
        ChooseSummary
    End Enum

    Public ReadOnly Property Dialog() As frmDataGateways
        Get
            Return CType(m_WizardDialog, frmDataGateways)
        End Get
    End Property

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig)
        Config = dataPipelineConfigOutput
        ShowAdvanced = False
    End Sub

    Protected Overrides Sub OnNavigateEnd(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim panel = TryCast(CurrentPanel, IDataGatewaysWizardPanel)
        panel.OnOpen()
    End Sub

    Protected Overrides Sub OnNavigateFinish(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim popup = New PopupForm(My.Resources.frmDataGateways_This, My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg, My.Resources.btnOk)
        AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.ShowInTaskbar = False
        popup.ShowDialog()

        Dialog.DialogResult = DialogResult.OK
        MyBase.OnNavigateFinish(sender, e)
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Protected Overrides Sub OnNavigateCancel(sender As Object, e As EventArgs)
        m_WizardDialog.Close()

        Dim advancedSelected = TryCast(CurrentPanel, IShowAdvancedConfigOption)
        If advancedSelected IsNot Nothing Then ShowAdvanced = advancedSelected.ShowAdvancedConfig
    End Sub

    Protected Overrides Sub OnNavigateNext(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim panel = TryCast(CurrentPanel, IDataGatewaysWizardPanel)
        panel.Closing()

        Select Case GetWizardProgressIndex()
            Case WizardControlIndices.ChooseDataType
                If Config.IsSessions Then
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseDataType)
                ElseIf Config.IsDashboards Then
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseSessionLogFields)
                Else
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseDashboards)
                End If
            Case WizardControlIndices.ChooseSessionLogFields
                If Config.IsDashboards Then
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseSessionLogFields)
                Else
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseDashboards)
                End If

            Case WizardControlIndices.ChooseDashboards
                MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseDashboards)
        End Select

        MyBase.OnNavigateNext(sender, e)
    End Sub

    Protected Overrides Sub OnNavigatePrevious(ByVal sender As Object, ByVal e As System.EventArgs)

        Dim panel = TryCast(CurrentPanel, IDataGatewaysWizardPanel)
        panel.Closing()

        Select Case GetWizardProgressIndex()
            Case WizardControlIndices.ChooseSummary
                If Config.IsDashboards Then
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseSummary)
                ElseIf Config.IsSessions Then
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseDashboards)
                Else
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseSessionLogFields)
                End If
            Case WizardControlIndices.ChooseDashboards
                If Config.IsSessions Then
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseDashboards)
                Else
                    MyBase.SetWizardProgressIndex(WizardControlIndices.ChooseSessionLogFields)
                End If

        End Select

        MyBase.OnNavigatePrevious(sender, e)
    End Sub
End Class
