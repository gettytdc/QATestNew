Imports System.Text.RegularExpressions
Imports BluePrism.DataPipeline.DataPipelineOutput


Public Class ctlDatabaseOutputOptions
    Inherits UserControl
    Implements IDataGatewaysOutputOptions

    Private Shared ReadOnly mRegexForTableName As Regex = New Regex("^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$")

    Private mConfig As DataPipelineOutputConfig
    Private mCredentialNames As IList(Of String)

    Private Function ValidateTableName(tableName As String) As Boolean
        Return mRegexForTableName.IsMatch(tableName)
    End Function

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig, credentialNames As IList(Of String))
        InitializeComponent()
        mConfig = dataPipelineConfigOutput
        mCredentialNames = credentialNames
        cboCredential.DataSource = mCredentialNames

        PopulateOutputConfig()
    End Sub

    Private Sub PopulateOutputConfig()
        txtServer.Text = mConfig.GetOrCreateOutputOptionById(DatabaseOutput.Server).Value
        txtDatabaseName.Text = mConfig.GetOrCreateOutputOptionById(DatabaseOutput.DatabaseName).Value
        txtTableName.Text = mConfig.GetOrCreateOutputOptionById(DatabaseOutput.TableName).Value

        Dim security = mConfig.GetOrCreateOutputOptionById(DatabaseOutput.SecurityType).Value

        If Not (String.IsNullOrEmpty(security) OrElse security = DatabaseOutput.IntegratedSecurity) Then
            radCredential.Checked = True
            Dim savedCredentialName = mConfig.GetOrCreateOutputOptionById(DatabaseOutput.Credential).Value
            If mCredentialNames.Contains(savedCredentialName) Then
                cboCredential.SelectedItem = savedCredentialName
            ElseIf String.IsNullOrWhiteSpace(savedCredentialName) Then
                cboCredential.SelectedItem = ""
            Else
                UserMessage.Show("Saved Credential has been deleted, please select another")
                cboCredential.SelectedItem = ""
            End If
        Else
            radIntegratedSecurity.Checked = True
        End If
    End Sub

    Private Sub SaveOutputConfig()
        mConfig.AddOrReplaceOption(New OutputOption(DatabaseOutput.Server, txtServer.Text))
        mConfig.AddOrReplaceOption(New OutputOption(DatabaseOutput.DatabaseName, txtDatabaseName.Text))
        mConfig.AddOrReplaceOption(New OutputOption(DatabaseOutput.TableName, txtTableName.Text))

        If radCredential.Checked Then
            mConfig.AddOrReplaceOption(New OutputOption(DatabaseOutput.SecurityType, DatabaseOutput.CredentialSecurity))
            mConfig.AddOrReplaceOption(New OutputOption(DatabaseOutput.Credential, cboCredential.SelectedItem.ToString))
        Else
            mConfig.AddOrReplaceOption(New OutputOption(DatabaseOutput.SecurityType, DatabaseOutput.IntegratedSecurity))
        End If
    End Sub

    Public Event OptionsValidChanged(isCorrect As Boolean) Implements IDataGatewaysOutputOptions.OptionsValidChanged

    Public Function AreOptionsValid() As Boolean Implements IDataGatewaysOutputOptions.AreOptionsValid
        If Not String.IsNullOrWhiteSpace(txtTableName.Text) AndAlso Not ValidateTableName(txtTableName.Text) Then
            imgExclamation.Visible = True
            lblInvalidTableName.Visible = True
            Return False
        Else
            imgExclamation.Visible = False
            lblInvalidTableName.Visible = False
        End If
        If String.IsNullOrWhiteSpace(txtServer.Text) Then Return False
        If String.IsNullOrWhiteSpace(txtDatabaseName.Text) Then Return False
        If String.IsNullOrWhiteSpace(txtTableName.Text) Then Return False

        If radCredential.Checked AndAlso cboCredential.SelectedIndex = -1 Then Return False

        Return True

    End Function

    Public Sub Closing() Implements IDataGatewaysOutputOptions.Closing
        SaveOutputConfig()
    End Sub

    Private Sub radSpecificUser_CheckedChanged(sender As Object, e As EventArgs) _
            Handles radIntegratedSecurity.CheckedChanged, radCredential.CheckedChanged
        cboCredential.Enabled = radCredential.Checked
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Private Sub txtServer_TextChanged(sender As Object, e As EventArgs) Handles txtServer.TextChanged
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Private Sub txtDatabaseName_TextChanged(sender As Object, e As EventArgs) Handles txtDatabaseName.TextChanged
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Private Sub txtTableName_TextChanged(sender As Object, e As EventArgs) Handles txtTableName.TextChanged
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Private Sub cboCredential_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCredential.SelectedIndexChanged
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

End Class