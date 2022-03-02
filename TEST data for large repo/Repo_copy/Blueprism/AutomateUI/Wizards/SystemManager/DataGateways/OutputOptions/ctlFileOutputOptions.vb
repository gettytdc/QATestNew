Imports BluePrism.DataPipeline.DataPipelineOutput

Public Class ctlFileOutputOptions
    Inherits UserControl
    Implements IDataGatewaysOutputOptions

    Public Event OptionsValidChanged(isCorrect As Boolean) Implements IDataGatewaysOutputOptions.OptionsValidChanged

    Private ReadOnly mConfig As DataPipelineOutputConfig

    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig)
        InitializeComponent()

        mConfig = dataPipelineConfigOutput
        txtPath.Text = mConfig.GetOrCreateOutputOptionById("path").Value
    End Sub

    Private Sub txtPath_TextChanged(sender As Object, e As EventArgs) Handles txtPath.TextChanged
        mConfig.AddOrReplaceOption(New OutputOption("path", txtPath.Text))
        mConfig.AddOrReplaceOption(New OutputOption("codec", "line { format => ""%{event}""}", True))
        RaiseEvent OptionsValidChanged(AreOptionsValid())
    End Sub

    Public Function AreOptionsValid() As Boolean Implements IDataGatewaysOutputOptions.AreOptionsValid
        Return Not String.IsNullOrWhiteSpace(mConfig.GetOrCreateOutputOptionById("path").Value)
    End Function

    Public Sub Closing() Implements IDataGatewaysOutputOptions.Closing
    End Sub
End Class