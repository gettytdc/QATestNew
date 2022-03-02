Imports AutomateControls.Wizard
Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class ctlChooseOutputType
    Inherits WizardPanel
    Implements IDataGatewaysWizardPanel

    Private mOutputOptions As IDataGatewaysOutputOptions
    Private mIsOutputSettingsCorrect As Boolean
    Private ReadOnly mConfig As DataPipelineOutputConfig
    Private ReadOnly mCredentialNames As IList(Of String)
    Private ReadOnly mExistingConfigNames As IList(Of String)

    Private ReadOnly mOutputTypes As List(Of OutputType) = New List(Of OutputType) _
        From {New OutputType(My.Resources.ctlChooseOutputType_File, FileOutputId),
        New OutputType(My.Resources.ctlChooseOutputType_HTTP, HTTPOutputId),
        New SplunkOutput(My.Resources.ctlChooseOutputType_Splunk, SplunkOutputId),
        New DatabaseOutput(My.Resources.ctlChooseOutputType_Database, DatabaseOutputId)}

    Private Const DatabaseOutputId = "database"
    Private Const FileOutputId = "file"
    Private Const SplunkOutputId = "splunk"
    Private Const HTTPOutputId = "http"

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig, dataGatawayCredentialNames As IList(Of String), existingConfigNames As IList(Of String))

        ' This call is required by the designer.
        InitializeComponent()
        mCredentialNames = dataGatawayCredentialNames
        mConfig = dataPipelineConfigOutput
        mExistingConfigNames = existingConfigNames

        cmbOutputTypes.DropDownStyle = ComboBoxStyle.DropDownList
        cmbOutputTypes.DataSource = mOutputTypes
        cmbOutputTypes.DisplayMember = "Name"
        cmbOutputTypes.ValueMember = "Id"

        If mConfig.OutputType Is Nothing Then
            mConfig.OutputType = mOutputTypes.First
        End If

        cmbOutputTypes.SelectedValue = mConfig.OutputType.Id
        SetOutputOptions(mConfig.OutputType)

        txtName.DataBindings.Add(New Binding("Text", mConfig, "Name"))
        AddHandler cmbOutputTypes.SelectedIndexChanged, AddressOf cmbOutputTypes_SelectedIndexChanged

    End Sub

    Private Sub cmbOutputTypes_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim selectedItem = CType(CType(sender, ComboBox).SelectedItem, OutputType)
        mConfig.OutputOptions.Clear()
        SetOutputOptions(selectedItem)
    End Sub

    Private Sub SetOutputOptions(outputType As OutputType)
        If mOutputOptions IsNot Nothing Then
            RemoveHandler mOutputOptions.OptionsValidChanged, AddressOf HandleOptionsValidChanged
        End If

        pnlOutputSettings.Controls.Clear()
        mConfig.OutputType = outputType
        mOutputOptions = GetIDataGatewaysOutputOptions(outputType)
        AddHandler mOutputOptions.OptionsValidChanged, AddressOf HandleOptionsValidChanged

        pnlOutputSettings.Controls.Add(CType(mOutputOptions, Control))

        HandleOptionsValidChanged(mOutputOptions.AreOptionsValid())

    End Sub

    Private Function GetIDataGatewaysOutputOptions(outputType As OutputType) As IDataGatewaysOutputOptions
        Dim outputOptionsController As IDataGatewaysOutputOptions = Nothing
        Select Case outputType.Id
            Case HTTPOutputId
                outputOptionsController = New ctlHttpOutputOptions(mConfig, mCredentialNames)
            Case DatabaseOutputId
                outputOptionsController = New ctlDatabaseOutputOptions(mConfig, mCredentialNames)
            Case SplunkOutputId
                outputOptionsController = New ctlSplunkOutputOptions(mConfig)
            Case Else
                outputOptionsController = New ctlFileOutputOptions(mConfig)

        End Select
        Return outputOptionsController
    End Function

    Private Sub HandleOptionsValidChanged(isCorrect As Boolean)
        mIsOutputSettingsCorrect = isCorrect
        IsInputCorrect()
    End Sub

    Private Sub txtName_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged
        mConfig.Name = txtName.Text
        IsInputCorrect()
    End Sub

    Private Function IsConfigNameUnique() As Boolean
        lblInvalidConfigName.Visible = False
        imgExclamation.Visible = False

        If (mExistingConfigNames.Any(Function(c)
                                         Return c = txtName.Text
                                     End Function)) Then

            lblInvalidConfigName.Visible = True
            imgExclamation.Visible = True

            Return False
        End If

        Return True
    End Function

    Private Sub IsInputCorrect()
        If Not String.IsNullOrEmpty(mConfig.Name) And mIsOutputSettingsCorrect And IsConfigNameUnique() Then
            NavigateNext = True
        Else
            NavigateNext = False
        End If
        UpdateNavigate()
    End Sub

    Public Sub OnOpen() Implements IDataGatewaysWizardPanel.OnOpen
    End Sub

    Public Sub Closing() Implements IDataGatewaysWizardPanel.Closing
        mOutputOptions.Closing()
    End Sub
End Class