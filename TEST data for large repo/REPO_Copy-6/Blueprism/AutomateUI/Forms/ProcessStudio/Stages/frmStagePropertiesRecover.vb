Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesRecover

    Private ReadOnly Property RecoverStage() As clsRecoverStage
        Get
            Return DirectCast(mProcessStage, clsRecoverStage)
        End Get
    End Property

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        Dim limitAttempts = RecoverStage.LimitAttempts
        chkLimitAttempts.Checked = limitAttempts
        numMaxAttempts.Enabled = limitAttempts
        lblMaxAttempts.Enabled = limitAttempts
        numMaxAttempts.Value = RecoverStage.MaxAttempts

    End Sub

    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False

        RecoverStage.LimitAttempts = chkLimitAttempts.Checked
        RecoverStage.MaxAttempts = CInt(numMaxAttempts.Value)

        Return True
    End Function


    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesRecover.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub chkLimitAttempts_CheckedChanged(sender As Object, e As EventArgs) Handles chkLimitAttempts.CheckedChanged
        lblMaxAttempts.Enabled = Not lblMaxAttempts.Enabled
        numMaxAttempts.Enabled = Not numMaxAttempts.Enabled
    End Sub
End Class
