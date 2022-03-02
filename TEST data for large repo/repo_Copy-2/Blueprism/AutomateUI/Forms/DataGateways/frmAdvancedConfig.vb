Imports AutomateControls
Imports AutomateControls.Forms
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.DataPipeline

Public Class frmAdvancedConfig
    Implements IHelp

    Private mIsInReadonlyMode As Boolean = True
    Private mPopup As PopupForm

    Private Const mHelpFileName As String = "data-gateways.htm"

    Public Sub New(configuration As String, isInReadonlyMode As Boolean)
        InitializeComponent()
        Me.KeyPreview = True
        ctlConfigEditor.mEditor.BorderStyle = BorderStyle.FixedSingle
        ctlConfigEditor.mEditor.Font = New Font(FontFamily.GenericSansSerif, 10)
        ctlConfigEditor.mEditor.IsBraceMatching = True
        ctlConfigEditor.mEditor.Margins(0).Width = 32
        mIsInReadonlyMode = isInReadonlyMode
        ctlConfigEditor.Code = configuration
        SetStyles()
        btnSave.Enabled = User.Current.HasPermission(Permission.SystemManager.DataGateways.AdvancedConfiguration)
        FormBorderStyle = FormBorderStyle.None
    End Sub

    Private Sub SetStyles()
        If mIsInReadonlyMode Then
            btnCancel.Visible = False
            btnSave.Text = My.Resources.frmAdvancedConfig_btnEditConfig
            Me.ctlConfigEditor.BackgroundColour = Color.Gainsboro
        Else
            Me.ctlConfigEditor.ReadOnly = False
            btnCancel.Visible = True
            btnSave.Text = My.Resources.frmAdvancedConfig_btnSaveConfig
            Me.ctlConfigEditor.BackgroundColour = Color.White
        End If

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If mIsInReadonlyMode Then
            mIsInReadonlyMode = False
            SetStyles()
        Else
            ExecuteCustomConfigSave()
        End If
    End Sub

    Private Sub ExecuteCustomConfigSave()

        Dim dataPipelineConfiguration As DataPipelineProcessConfigDetails = DataPipelineProcessConfigDetails.FromConfig(gSv.GetConfigurationByName("Default"))
        If dataPipelineConfiguration Is Nothing Then dataPipelineConfiguration = New DataPipelineProcessConfigDetails() With {.Name = "Default"}

        If Not dataPipelineConfiguration.IsCustom Then
            Dim confirmationForm As New YesNoPopupForm(
                My.Resources.ConfirmCustomConfigTitle,
                My.Resources.ConfirmCustomConfig)
            confirmationForm.ShowInTaskbar = False
            confirmationForm.ShowDialog()
            If confirmationForm.DialogResult = MsgBoxResult.No Then
                Return
            End If
        End If

        dataPipelineConfiguration.LogstashConfigFile = ctlConfigEditor.Code
        dataPipelineConfiguration.IsCustom = True
        gSv.SaveConfig(DataPipelineProcessConfigDetails.ToConfig(dataPipelineConfiguration))
        DialogResult = DialogResult.OK

        mPopup = New PopupForm(My.Resources.frmDataGateways_This, My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg, My.Resources.btnOk)
        AddHandler mPopup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        mPopup.ShowInTaskbar = False
        mPopup.ShowDialog()
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click, btnClose.Click

        If Not mIsInReadonlyMode Then
            Dim confirmationForm As New YesNoPopupForm(
                My.Resources.DataGatewayAdvancedConfig_ExitCaption,
                My.Resources.DataGatewayAdvancedConfig_ExitText)
            confirmationForm.ShowInTaskbar = False
            confirmationForm.ShowDialog()

            If confirmationForm.DialogResult = DialogResult.No Then Return
        End If
        DialogResult = DialogResult.Cancel
        Close()

    End Sub

#Region "Drag And Drop Window"

    Dim mMouseDownLocation As Point

    Private Sub BorderPanel_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel.MouseDown
        If e.Button = MouseButtons.Left Then mMouseDownLocation = e.Location
    End Sub
    Private Sub BorderPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel.MouseMove
        If e.Button = MouseButtons.Left Then
            Left += e.Location.X - mMouseDownLocation.X
            Top += e.Location.Y - mMouseDownLocation.Y
        End If
    End Sub

#End Region

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return mHelpFileName
    End Function

    Private Sub frmAdvancedConfig_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.F1 Then
            Try
                OpenHelpFile(Me, GetHelpFile())
            Catch
                UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
            End Try
        End If
    End Sub
End Class