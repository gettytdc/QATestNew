Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class ctlHttpOutputOptions
    Inherits UserControl
    Implements IDataGatewaysOutputOptions

    Public Event OptionsValidChanged(isCorrect As Boolean) Implements IDataGatewaysOutputOptions.OptionsValidChanged

    Private ReadOnly mHttpMethods As IList(Of String) = New List(Of String) From {"PUT", "POST", "PATCH", "DELETE", "GET", "HEAD"}
    Private ReadOnly mConfig As DataPipelineOutputConfig
    Private mCredentialNames As IList(Of String)

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig, credentialNames As IList(Of String))
        mConfig = dataPipelineConfigOutput
        mCredentialNames = credentialNames

        InitializeComponent()

        txtUrl.Text = mConfig.GetOrCreateOutputOptionById("url").Value

        cmbHttpMethod.DataSource = mHttpMethods
        cmbHttpMethod.SelectedItem = mConfig.GetOrCreateOutputOptionById("http_method", cmbHttpMethod.SelectedItem.ToString.ToLower).Value.ToUpper()
        AddHandler cmbHttpMethod.SelectedIndexChanged, AddressOf cmbHttpMethod_SelectedIndexChanged

        If mCredentialNames.Any Then
            cmbCredential.DataSource = mCredentialNames
            cmbCredential.SelectedItem = mConfig.GetOrCreateOutputOptionById("credential", cmbCredential.SelectedItem.ToString).Value
            AddHandler cmbCredential.SelectedIndexChanged, AddressOf cmbCredential_SelectedIndexChanged
        End If
    End Sub

    Private Sub cmbCredential_SelectedIndexChanged(sender As Object, e As EventArgs)
        mConfig.AddOrReplaceOption(New AuthorizationOutputOption("credential", cmbCredential.SelectedItem.ToString))
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Private Sub cmbHttpMethod_SelectedIndexChanged(sender As Object, e As EventArgs)
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Public Function AreOptionsValid() As Boolean Implements IDataGatewaysOutputOptions.AreOptionsValid
        Try
            Dim uri = New Uri(txtURL.Text)
            imgExclamation.Visible = False
            lblUrlValidation.Visible = False
        Catch ex As UriFormatException
            imgExclamation.Visible = True
            lblUrlValidation.Visible = True
            Return False
        End Try

        lblUrlValidation.Text = String.Empty
        Return Not String.IsNullOrWhiteSpace(mConfig.GetOrCreateOutputOptionById("http_method").Value) AndAlso Not String.IsNullOrWhiteSpace(mConfig.GetOrCreateOutputOptionById("credential").Value)
    End Function

    Private Sub txtUrl_TextChanged(sender As Object, e As EventArgs) Handles txtUrl.TextChanged
        mConfig.AddOrReplaceOption(New OutputOption("url", txtUrl.Text))
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Public Sub Closing() Implements IDataGatewaysOutputOptions.Closing
        mConfig.AddOrReplaceOption(New OutputOption("http_method", cmbHttpMethod.SelectedItem.ToString.ToLower))
    End Sub
End Class
