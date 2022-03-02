Imports BluePrism.AutomateAppCore.Utility

Friend Class frmStagePropertiesBlock
    Public Sub New()
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.LogInhibitVisible = False

    End Sub
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesBlock.htm"
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
