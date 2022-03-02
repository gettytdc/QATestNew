Imports AutomateControls.Forms
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models

Public Module LogstashConfigHelper

    Public Function TryGetLogstashConfig(ByRef newConfig As String) As Boolean

        Try
            newConfig = gSv.ProduceLogstashConfig()

        Catch ex As NoSuchCredentialException
            ShowSQLUserExceptionPopup(My.Resources.ctlDataPipelineConfig_DataGatewayNoCredential)
            Return False
        Catch ex As BluePrismException
            ShowSQLUserExceptionPopup(ex.Message)
            Return False
        End Try
        Return True

    End Function

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Private Sub ShowSQLUserExceptionPopup(exceptionMessage As String)
        Dim popup = New PopupForm(My.Resources.frmDataGateways_This, exceptionMessage, My.Resources.btnOk)
        AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.ShowInTaskbar = False
        popup.ShowDialog()
    End Sub

End Module
