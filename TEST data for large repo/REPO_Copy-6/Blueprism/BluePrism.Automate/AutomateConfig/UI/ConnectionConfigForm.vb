Imports BluePrism.AutomateAppCore.Utility

''' <summary>
''' The form which shows the configuration for the connections.
''' </summary>
Public Class ConnectionConfigForm

    ''' <summary>
    ''' Handles the load event for this form. This simply ensures that the
    ''' connections are loaded when this form is loaded.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        If Not DesignMode Then connManager.LoadConnections()
    End Sub


    ''' <summary>
    ''' Handles the OK button being clicked
    ''' </summary>
    Private Sub HandleOkClick(ByVal sender As Object, ByVal e As EventArgs) Handles btnOK.Click
        Try
            If ConnectionDetail.IsConnectionNameEmpty Then
                MessageBox.Show(My.Resources.YouMustEnterAValidConnectionNameTheNameMayNotBeBlank)
                Return
            End If
            If Not ConnectionManagerPanel.IsConnectionNameUnique Then
                MessageBox.Show(My.Resources.TheNameOfThisConnectionIsNotUniquePleaseChooseAnother)
                Return
            End If
            connManager.SaveConnections()
            DialogResult = DialogResult.OK
            Close()

        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.CouldNotSaveLocalMachineConfiguration0, ex.Message),
             My.Resources.SaveError, MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try
    End Sub

    ''' <summary>
    ''' Handles the Cancel button being clicked
    ''' </summary>
    Private Sub HandleCancelClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Abort
        Close()
    End Sub

    Public Overrides Function GetHelpFile() As String
        Return "helpConnections.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch ex As Exception
            UserMessage.Err(ex)
        End Try
    End Sub

End Class
