Imports AutomateControls
Imports AutomateControls.Forms
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.DataPipeline

Public Class ctlDataGatewayCustomConfiguration
    Implements IStubbornChild, IPermission, IHelp


    Private mConfig As String
    Private mParentControl As ctlDataGatewayConfigurationContainer

    Private Const DefaultConfigName As String = "Default"

    Public Sub New(parent As ctlDataGatewayConfigurationContainer)

        ' This call is required by the designer.
        InitializeComponent()
        mParentControl = parent
        ' Add any initialization after the InitializeComponent() call.
        RefreshUI()
        btnEdit.Enabled = User.Current.HasPermission(Permission.SystemManager.DataGateways.AdvancedConfiguration)
        btnDelete.Enabled = User.Current.HasPermission(Permission.SystemManager.DataGateways.AdvancedConfiguration)
    End Sub

    Private Sub RefreshUI()
        mConfig = app.gSv.GetConfigurationByName(DefaultConfigName).LogstashConfigFile
        ctlConfigEditor.Code = mConfig
        ctlConfigEditor.mEditor.IsReadOnly = True
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        Dim form = New frmAdvancedConfig(mConfig, False)
        form.ShowInTaskbar = False
        form.ShowDialog()
        If form.DialogResult = DialogResult.OK Then
            RefreshUI()
        End If
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim confirmationForm As New YesNoPopupForm(
                My.Resources.DeleteCustomConfigurationTitle,
                My.Resources.DeleteCustomConfiguration)
        confirmationForm.ShowInTaskbar = False
        confirmationForm.ShowDialog()
        If confirmationForm.DialogResult = MsgBoxResult.No Then
            Return
        End If

        Dim newConfig = String.Empty
        If LogstashConfigHelper.TryGetLogstashConfig(newConfig) = False Then
            Return
        End If

        Dim dataPipelineConfiguration As DataPipelineProcessConfigDetails = DataPipelineProcessConfigDetails.FromConfig(gSv.GetConfigurationByName(DefaultConfigName))
        If dataPipelineConfiguration Is Nothing Then dataPipelineConfiguration = New DataPipelineProcessConfigDetails() With {.Name = DefaultConfigName}
        dataPipelineConfiguration.LogstashConfigFile = newConfig
        dataPipelineConfiguration.IsCustom = False
        gSv.SaveConfig(DataPipelineProcessConfigDetails.ToConfig(dataPipelineConfiguration))

        Dim popup As PopupForm
        popup = New PopupForm(My.Resources.frmDataGateways_This, My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg, My.Resources.btnOk)
        AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.ShowInTaskbar = False
        popup.ShowDialog()

        mParentControl.RefreshUI()
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "dg-custom-config.htm"
    End Function
End Class
