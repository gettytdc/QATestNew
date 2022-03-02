Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesResume
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesResume.htm"
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
End Class
