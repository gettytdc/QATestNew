
Imports BluePrism.AutomateAppCore.Groups

Public Class ctlChooseWebExposeProcess

    Friend Sub Setup(ByVal mode As ProcessType)
        ctlProcesses.TreeType = mode.TreeType
        ctlProcesses.Filter = ProcessBackedGroupMember.NotRetiredAndNotExposed
        Me.Title = String.Format(My.Resources.ctlChooseWebExposeProcess_ChooseA0, mode.ModeStringLowerCase())
    End Sub

    Private Sub ctlProcesses_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ctlProcesses.SelectedIndexChanged
        If ctlProcesses.GetSelectedMembers(Of ProcessBackedGroupMember).Count > 0 Then
            NavigateNext = True
        Else
            NavigateNext = False
        End If
        UpdateNavigate()
    End Sub

End Class
