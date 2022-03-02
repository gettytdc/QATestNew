Imports System.Security.Cryptography
Imports AutomateControls
Imports AutomateControls.Forms
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Utility
Imports BluePrism.DataPipeline
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports AutomateUI.Wizards.SystemManager.DataGateways.Helpers
Imports BluePrism.Server.Domain.Models

Public Class ctlDataPipelineConfiguration
    Implements IStubbornChild, IPermission, IHelp

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
    Private mParent As ctlDataGatewayConfigurationContainer
    Private mDataPipelineOutputConfigs As IEnumerable(Of DataPipelineOutputConfig)
    Private mHasAdvancedPermission As Boolean
    Private ReadOnly mDataGatawayCredentialNames As IList(Of String)
    Private ReadOnly mHasValidConfig As Boolean
    Private ReadOnly mSelectColumn As Integer = 1
    Private ReadOnly mCopyColumn As Integer = 5
    Private ReadOnly mDeleteColumn As Integer = 6
    Private ReadOnly mEditColumn As Integer = 7
    Private ReadOnly mHasAdvancedColumn As Integer = 3

    Private Property manageConfigsMode As Boolean = False


    Public Sub New(parent As ctlDataGatewayConfigurationContainer)
        ' This call is required by the designer.
        InitializeComponent()
        mParent = parent
        dgvPipelineOutputConfigs.ColumnHeadersVisible = False
        dgvPipelineOutputConfigs.RowHeadersVisible = False

        ' Add any initialization after the InitializeComponent() call.
        mDataGatawayCredentialNames = gSv.GetDataGatewayCredentials
        RefreshUI()
        mHasValidConfig = gSv.DefaultEnctryptionSchemeValid()
    End Sub

    Private Sub RefreshUI()

        panPicture.Dock = DockStyle.Fill
        panConfigs.Dock = DockStyle.Fill

        mDataPipelineOutputConfigs = gSv.GetDataPipelineOutputConfigs().ToList()
        dgvPipelineOutputConfigs.Rows.Clear()
        SelectAll.Checked = False
        DeleteSelected.Enabled = False

        If mDataPipelineOutputConfigs.Count > 0 Then
            panPicture.Visible = False
            panConfigs.Visible = True


            Dim allAdvanced = True
            mHasAdvancedPermission = User.Current.HasPermission(Permission.SystemManager.DataGateways.AdvancedConfiguration)
            For Each outputConfig In mDataPipelineOutputConfigs
                If Not outputConfig.IsAdvanced Then allAdvanced = False

                Dim index = dgvPipelineOutputConfigs.Rows.Add(outputConfig.Id,
                                                              False,
                                                              outputConfig.Name,
                                                              IIf(outputConfig.IsAdvanced,
                                                                  My.Resources.ViewDataPipeline_TypeAdvanced,
                                                                  GetLocalizedFriendlyName(outputConfig.OutputType.Id)),
                                                              FormatDateTime(outputConfig.DateCreated.Value, DateFormat.ShortDate)
                                                              )
                If outputConfig.IsAdvanced AndAlso Not mHasAdvancedPermission Then
                    dgvPipelineOutputConfigs.Rows(index).Cells(mSelectColumn) = New DataGridViewTextBoxCell() With {.Value = ""}
                    dgvPipelineOutputConfigs.Rows(index).Cells(mCopyColumn) = New DataGridViewTextBoxCell() With {.Value = ""}
                    dgvPipelineOutputConfigs.Rows(index).Cells(mDeleteColumn) = New DataGridViewTextBoxCell() With {.Value = ""}
                    dgvPipelineOutputConfigs.Rows(index).Cells(mEditColumn) = New DataGridViewTextBoxCell() With {.Value = ""}
                End If
            Next

            Dim canManage = Not allAdvanced OrElse mHasAdvancedPermission
            btnManageConfigs.Enabled = canManage

            If canManage Then
                If manageConfigsMode Then
                    btnManageConfigs.BackColor = Color.FromArgb(CType(CType(228, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(251, Byte), Integer))
                Else
                    btnManageConfigs.BackColor = Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
                End If
            Else
                btnManageConfigs.BackColor = Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
            End If

            dgvPipelineOutputConfigs.Columns(mSelectColumn).Visible = manageConfigsMode
            dgvPipelineOutputConfigs.Columns(mCopyColumn).Visible = manageConfigsMode
            dgvPipelineOutputConfigs.Columns(mDeleteColumn).Visible = manageConfigsMode
            dgvPipelineOutputConfigs.Columns(mEditColumn).Visible = manageConfigsMode
            DeleteSelected.Visible = manageConfigsMode
            SelectAllPanel.Visible = manageConfigsMode
            CheckBoxSpacerPanel.Visible = Not manageConfigsMode
        Else
            manageConfigsMode = False
            panPicture.Visible = True
            panConfigs.Visible = False
        End If
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "data-gateways.htm"
    End Function

    Private Sub SaveAndUpdateConfig(config As DataPipelineOutputConfig)
        gSv.SaveDataPipelineOutputConfig(config)
        ProduceAndSaveConfigFile()
    End Sub

    Private Sub ProduceAndSaveConfigFile()

        Dim newConfig = String.Empty
        If LogstashConfigHelper.TryGetLogstashConfig(newConfig) = False Then
            Return
        End If

        Dim dataPipelineConfiguration As DataPipelineProcessConfigDetails = DataPipelineProcessConfigDetails.FromConfig(gSv.GetConfigurationByName("Default"))
        If dataPipelineConfiguration Is Nothing Then dataPipelineConfiguration = New DataPipelineProcessConfigDetails() With {.Name = "Default"}
        dataPipelineConfiguration.LogstashConfigFile = newConfig
        gSv.SaveConfig(DataPipelineProcessConfigDetails.ToConfig(dataPipelineConfiguration))
    End Sub

    Private Sub StartWizard(config As DataPipelineOutputConfig, ByRef showAdvanced As Boolean)
        Try
            Using frmDataGatewaysAdd As New frmDataGateways()
                frmDataGatewaysAdd.Setup(config, mDataGatawayCredentialNames, GetOtherConfigNames(config.Name))
                frmDataGatewaysAdd.SetEnvironmentColoursFromAncestor(TopLevelControl)
                frmDataGatewaysAdd.ShowInTaskbar = False
                frmDataGatewaysAdd.ShowDialog()

                showAdvanced = frmDataGatewaysAdd.ShowAdvanced
                If Not showAdvanced AndAlso frmDataGatewaysAdd.DialogResult = DialogResult.OK Then
                    SaveAndUpdateConfig(config)
                End If

            End Using
        Catch ex As Exception
            UserMessage.Show(String.Format(ex.Message))
        End Try
    End Sub

    Private Sub ShowAdvancedConfig(config As DataPipelineOutputConfig)
        Dim advancedConfig = New AdvancedConfigurationForm(config, GetOtherConfigNames(config.Name))
        advancedConfig.ShowInTaskbar = False
        Dim result = advancedConfig.ShowDialog()

        If result = DialogResult.OK Then
            SaveAndUpdateConfig(config)
            Dim popup = New PopupForm(My.Resources.frmDataGateways_This, My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg, My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowInTaskbar = False
            popup.ShowDialog()
        End If
    End Sub

    Private Function CheckDefaultEncryptionScheme(isEditAction As Boolean) As Boolean
        If mHasValidConfig = False Then
            Dim popup = New PopupForm(My.Resources.ctlDataPipelineConfiguration_Error,
                                      CType(IIf(isEditAction, My.Resources.ctlDataPipelineConfiguration_DefaultEncryptionSchemeInvalidEdit, My.Resources.ctlDataPipelineConfiguration_DefaultEncryptionSchemeInvalidView), String), My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowInTaskbar = False
            popup.ShowDialog()
            Return False
        End If
        Return True
    End Function

    Private Sub CopyConfig(config As DataPipelineOutputConfig)
        If Not CheckDBEncryptionSchemesAreFipsCompliant() Then Return

        config.Name += My.Resources.ctlDataPipelineConfiguration_AdditionalCopyTag
        config.Id = 0
        If config.IsAdvanced Then
            ShowAdvancedConfig(config)
        Else
            Dim showAdvanced As Boolean = False
            StartWizard(config, showAdvanced)
            If showAdvanced Then ShowAdvancedConfig(config)
        End If

        RefreshUI()
    End Sub

    Private Sub DeleteConfig(config As DataPipelineOutputConfig)
        If Not CheckDBEncryptionSchemesAreFipsCompliant() Then Return

        Dim confirmationForm As New YesNoPopupForm(
                My.Resources.ctlDataPipelineConfiguration_DeletionConfirmTitle,
                My.Resources.ctlDataPipelineConfiguration_DeletionConfirmMessage)
        confirmationForm.ShowInTaskbar = False
        confirmationForm.ShowDialog()
        If confirmationForm.DialogResult = DialogResult.Yes Then
            Dim popup = New PopupForm(My.Resources.frmDataGateways_This,
                                      My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg,
                                      My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowInTaskbar = False
            popup.ShowDialog()
            gSv.DeleteDataPipelineOutputConfig(config)
            ProduceAndSaveConfigFile()
            RefreshUI()
        End If
    End Sub

    Private Sub StartConfigEdit(config As DataPipelineOutputConfig)
        If Not CheckDBEncryptionSchemesAreFipsCompliant() Then Return

        If config.IsAdvanced Then
            ShowAdvancedConfig(config)
        Else
            Dim showAdvanced As Boolean = False
            StartWizard(config, showAdvanced)
            If showAdvanced Then ShowAdvancedConfig(config)
        End If

        RefreshUI()
    End Sub

    Private Sub dgvPipelineOutputConfigs_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPipelineOutputConfigs.CellContentClick
        If Not CheckDefaultEncryptionScheme(True) Then
            Return
        End If

        If e.RowIndex >= 0 AndAlso e.ColumnIndex = mSelectColumn Then
            Dim cellCheckBox As DataGridViewCell = dgvPipelineOutputConfigs.Rows(e.RowIndex).Cells(mSelectColumn)
            cellCheckBox.Value = Not cellCheckBox.ValueToBool()
            DeleteSelected.Enabled = AnyCheckBoxValue(True)
            SelectAll.Checked = Not AnyCheckBoxValue(False)
        End If

        Dim senderGrid = DirectCast(sender, DataGridView)
        If TypeOf senderGrid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn AndAlso
           e.RowIndex >= 0 Then

            Dim row = dgvPipelineOutputConfigs.Rows.Item(e.RowIndex)
            Dim cell = row.Cells(e.ColumnIndex)

            If TypeOf cell IsNot DataGridViewButtonCell Then
                Return
            End If

            Dim configId = Integer.Parse(row.Cells.Item(0).Value.ToString)

            Dim config As DataPipelineOutputConfig = mDataPipelineOutputConfigs.Single(Function(c)
                                                                                           Return c.Id = configId
                                                                                       End Function)

            Select Case e.ColumnIndex
                Case mCopyColumn
                    CopyConfig(config)
                Case mDeleteColumn
                    DeleteConfig(config)
                Case mEditColumn
                    StartConfigEdit(config)
            End Select

        End If
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Private Sub btnCustomConfig_Click(sender As Object, e As EventArgs) Handles btnCustom2.Click, btnCustomConfig.Click

        If Not CheckDefaultEncryptionScheme(False) Then
            Return
        End If

        Dim newConfig = String.Empty
        If LogstashConfigHelper.TryGetLogstashConfig(newConfig) = False Then
            Return
        End If

        Dim form = New frmAdvancedConfig(newConfig, mDataPipelineOutputConfigs.Any)
        form.ShowInTaskbar = False
        form.ShowDialog()
        If form.DialogResult = DialogResult.OK Then
            mParent.RefreshUI()
        End If

    End Sub

    Private Function GetOtherConfigNames(nameToExclude As String) As IList(Of String)
        Return mDataPipelineOutputConfigs.Where(Function(c)
                                                    Return Not c.Name.Equals(nameToExclude)
                                                End Function).Select(Function(c)
                                                                         Return c.Name
                                                                     End Function).ToList()
    End Function

    Private Sub btnAddConfig_Click(sender As Object, e As EventArgs) Handles btnAdd2.Click, btnAddConfig.Click
        If Not CheckDefaultEncryptionScheme(True) Then
            Return
        End If

        Dim newConfig = New DataPipelineOutputConfig()
        Dim showAdvanced As Boolean = False

        If Not CheckDBEncryptionSchemesAreFipsCompliant() Then Return

        StartWizard(newConfig, showAdvanced)

        If showAdvanced Then ShowAdvancedConfig(newConfig)

        RefreshUI()
    End Sub

    Private Sub btnManageConfigs_Click(sender As Object, e As EventArgs) Handles btnManageConfigs.Click
        manageConfigsMode = Not manageConfigsMode
        RefreshUI()
    End Sub

    Private Sub SelectAll_CheckedChanged(sender As Object, e As EventArgs) Handles SelectAll.Click
        DeleteSelected.Enabled = SelectAll.Checked
        For Each row As DataGridViewRow In dgvPipelineOutputConfigs.Rows
            If mHasAdvancedPermission _
                OrElse dgvPipelineOutputConfigs.Rows(row.Index).Cells(mHasAdvancedColumn).Value.ToString() _
                IsNot My.Resources.ViewDataPipeline_TypeAdvanced _
                Then
                dgvPipelineOutputConfigs.Rows(row.Index).Cells(mSelectColumn) = New DataGridViewCheckBoxCell() With {.Value = SelectAll.Checked}
            End If
        Next
    End Sub

    Private Sub DeleteSelected_Click(sender As Object, e As EventArgs) Handles DeleteSelected.Click
        If Not CheckDBEncryptionSchemesAreFipsCompliant() Then Return

        Dim confirmationForm As New YesNoPopupForm(
                My.Resources.ctlDataPipelineConfiguration_DeletionConfirmTitle,
                My.Resources.ctlDataPipelineConfiguration_DeletionConfirmMessage)
        confirmationForm.ShowInTaskbar = False
        confirmationForm.ShowDialog()
        If confirmationForm.DialogResult = DialogResult.Yes Then
            Dim popup = New PopupForm(My.Resources.frmDataGateways_This,
                          My.Resources.clsDataGatewaysWizardController_DataGatewaysProcessNeedsRestarningMsg,
                          My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowInTaskbar = False
            popup.ShowDialog()
            Dim hasDeleted As Boolean = False
            For Each row As DataGridViewRow In dgvPipelineOutputConfigs.Rows
                Dim cellCheckBox As DataGridViewCell = dgvPipelineOutputConfigs.Rows(row.Index).Cells(mSelectColumn)
                If cellCheckBox.Value.Equals(True) Then
                    Dim configId = Integer.Parse(row.Cells.Item(0).Value.ToString)
                    Dim config As DataPipelineOutputConfig = mDataPipelineOutputConfigs.Single(Function(c)
                                                                                                   Return c.Id = configId
                                                                                               End Function)
                    gSv.DeleteDataPipelineOutputConfig(config)
                    hasDeleted = True
                End If
            Next
            If hasDeleted Then
                ProduceAndSaveConfigFile()
            End If
            RefreshUI()
        End If
    End Sub

    Private Function AnyCheckBoxValue(valueCheck As Boolean) As Boolean
        Return dgvPipelineOutputConfigs.Rows.Cast(Of DataGridViewRow) _
            .Where(Function(row) mHasAdvancedPermission _
            OrElse row.Cells(mHasAdvancedColumn).Value.ToString() _
            IsNot My.Resources.ViewDataPipeline_TypeAdvanced) _
            .Select(Function(row) row.Cells(mSelectColumn).ValueToBool()) _
            .Any(Function(x) x = valueCheck)
    End Function

    Private Function CheckDBEncryptionSchemesAreFipsCompliant() As Boolean
        Try
            If CryptoConfig.AllowOnlyFipsAlgorithms AndAlso gSv.DBEncryptionSchemesAreFipsCompliant().Count > 0 Then
                Throw New BluePrismException(My.Resources.ctlDataPipelineConfiguration_AlgorithmIsNotFIPSCompliant_Decrypt)
            End If
            Return True
        Catch ex As BluePrismException
            HandleDBEncryptionSchemesFipsCompliantException(ex)
            Return False
        End Try
    End Function

    Private Sub HandleDBEncryptionSchemesFipsCompliantException(ex As BluePrismException)
        Dim popup = New PopupForm(My.Resources.frmDataGateways_This, ex.Message, My.Resources.btnOk)
        AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.ShowInTaskbar = False
        popup.ShowDialog()
    End Sub

    Private Sub SelectAll_CheckedLoaded(sender As Object, e As EventArgs) Handles SelectAll.HandleCreated
        Dim CheckBox = CType(sender, CheckBox)
        CheckBox.Text = "   " + CheckBox.Text
    End Sub
End Class
