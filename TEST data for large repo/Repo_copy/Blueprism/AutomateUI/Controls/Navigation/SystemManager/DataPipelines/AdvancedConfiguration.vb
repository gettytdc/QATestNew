Imports AutomateControls
Imports AutomateControls.Forms
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class AdvancedConfigurationForm
    Implements IChild, IPermission, IHelp

    Private mOutputConfiguration As DataPipelineOutputConfig
    Private mOtherConfigNames As IEnumerable(Of String)

    Private Const mHelpFileName As String = "dg-custom-config.htm"

    Public Sub New(config As DataPipelineOutputConfig, otherConfigNames As IList(Of String))
        InitializeComponent()
        Me.KeyPreview = True
        mOutputConfiguration = config
        mOtherConfigNames = otherConfigNames
    End Sub

    Private Sub AdvancedConfigurationForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        LoadConfig()
    End Sub

#Region "Drag And Drop Window"

    Dim mMouseDownLocation As Point

    Private Sub BorderPanel_MouseDown(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseDown
        If e.Button = MouseButtons.Left Then mMouseDownLocation = e.Location
    End Sub
    Private Sub BorderPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseMove
        If e.Button = MouseButtons.Left Then
            Left += e.Location.X - mMouseDownLocation.X
            Top += e.Location.Y - mMouseDownLocation.Y
        End If
    End Sub

#End Region

#Region "Event Handlers"

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim confirmAdvanced As Boolean = True
        Dim confirmationForm As New YesNoPopupForm(
            My.Resources.frmAdvancedConfigConfirm_Title,
            My.Resources.frmAdvancedConfigConfirm_Message)

        If Not mOutputConfiguration.IsAdvanced Then
            confirmationForm.ShowInTaskbar = False
            confirmationForm.ShowDialog()
            confirmAdvanced = confirmationForm.DialogResult = DialogResult.Yes
        End If

        If confirmAdvanced Then
            DialogResult = DialogResult.OK
            SaveConfig()
            Close()
        End If
    End Sub

    Private Sub btnPaste_Click(sender As Object, e As EventArgs) Handles btnPaste.Click
        If Clipboard.ContainsText(TextDataFormat.Text) Then
            txtConfiguration.Text = Clipboard.GetText(TextDataFormat.Text)
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click, btnCancel.Click

        If txtConfigurationName.Text <> mOutputConfiguration.Name OrElse
            txtConfiguration.Text <> mOutputConfiguration.AdvancedConfiguration Then
            Dim confirmationForm As New YesNoPopupForm(
                My.Resources.frmDataGateways_This,
                My.Resources.frmDataGateways_CancelConfirm, String.Empty)
            confirmationForm.ShowInTaskbar = False
            confirmationForm.ShowDialog()

            If confirmationForm.DialogResult = DialogResult.No Then Return
        End If

        DialogResult = DialogResult.Cancel
        Close()

    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        Dim fileDialog = New OpenFileDialog() With {
            .Title = My.Resources.AdvancedDataPipeLineConfig_FileDialog,
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            .Filter = My.Resources.AdvancedDataPipeLineConfig_FileDialogFilter,
            .FilterIndex = 2,
            .RestoreDirectory = True
        }

        If fileDialog.ShowDialog() = DialogResult.OK Then
            Dim fileContents = My.Computer.FileSystem.ReadAllText(fileDialog.FileName)
            If fileContents.Length > 10000 Then
                UserMessage.Show(My.Resources.AdvancedDataPipeLineConfig_FileDialogTooLarge)
            Else
                txtConfiguration.Text = fileContents
            End If
        End If
    End Sub

#End Region

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.System.Settings)
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return mHelpFileName
    End Function

    Private Sub AdvancedConfiguration_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = Keys.F1 Then
            Try
                OpenHelpFile(Me, GetHelpFile())
            Catch
                UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
            End Try
        End If
    End Sub

    Private Sub LoadConfig()
        If mOutputConfiguration Is Nothing Then Throw New InvalidOperationException("Cannot render config, object is null")

        txtConfigurationName.Text = mOutputConfiguration.Name
        If mOutputConfiguration.IsAdvanced Then
            txtConfiguration.Text = mOutputConfiguration.AdvancedConfiguration
        Else
            txtConfiguration.Text = mOutputConfiguration.GetLogstashConfig
        End If
    End Sub

    Private Sub SaveConfig()
        If mOutputConfiguration Is Nothing Then Throw New InvalidOperationException("Cannot save config, object is null")

        mOutputConfiguration.AdvancedConfiguration = txtConfiguration.Text
        mOutputConfiguration.Name = txtConfigurationName.Text
        mOutputConfiguration.IsAdvanced = True
    End Sub

    Private Function IsConfigNameUnique() As Boolean

        imgExclamation.Visible = False
        lblInvalidConfigName.Visible = False

        btnSave.Enabled = True

        If (mOtherConfigNames.Any(Function(c)
                                      Return c = txtConfigurationName.Text
                                  End Function)) Then
            imgExclamation.Visible = True
            lblInvalidConfigName.Visible = True

            btnSave.Enabled = False

            Return False
        End If

        Return True
    End Function

    Private Sub txtConfigurationName_TextChanged(sender As Object, e As EventArgs) Handles txtConfigurationName.TextChanged
        IsConfigNameUnique()
    End Sub

End Class