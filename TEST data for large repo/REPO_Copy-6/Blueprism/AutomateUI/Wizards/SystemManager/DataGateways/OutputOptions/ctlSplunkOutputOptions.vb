Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class ctlSplunkOutputOptions
    Inherits UserControl
    Implements IDataGatewaysOutputOptions

    Private mConfig As DataPipelineOutputConfig

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig)
        InitializeComponent()
        mConfig = dataPipelineConfigOutput

        txtURL.Text = mConfig.GetOrCreateOutputOptionById(SplunkOutput.URL).Value
        txtToken.Text = mConfig.GetOrCreateOutputOptionById(SplunkOutput.Token).Value
    End Sub

    Public Event OptionsValidChanged(isCorrect As Boolean) Implements IDataGatewaysOutputOptions.OptionsValidChanged

    Public Sub Closing() Implements IDataGatewaysOutputOptions.Closing
        mConfig.AddOrReplaceOption(New OutputOption(SplunkOutput.URL, txtURL.Text))
        mConfig.AddOrReplaceOption(New OutputOption(SplunkOutput.Token, txtToken.Text))
    End Sub

    Public Function AreOptionsValid() As Boolean Implements IDataGatewaysOutputOptions.AreOptionsValid
        Try
            Dim uri = New Uri(txtURL.Text)
            imgExclamation.Visible = False
            lblInvalidURL.Visible = False
        Catch ex As UriFormatException
            imgExclamation.Visible = True
            lblInvalidURL.Visible = True
            Return False
        End Try

        Return True
    End Function

    Private Sub btnPaste_Click(sender As Object, e As EventArgs) Handles btnPaste.Click
        If My.Computer.Clipboard.ContainsText() Then
            txtToken.Text = My.Computer.Clipboard.GetText()
        End If
    End Sub

    Private Sub txtURL_TextChanged(sender As Object, e As EventArgs) Handles txtURL.TextChanged
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Private Sub txtToken_TextChanged(sender As Object, e As EventArgs) Handles txtToken.TextChanged
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub
End Class