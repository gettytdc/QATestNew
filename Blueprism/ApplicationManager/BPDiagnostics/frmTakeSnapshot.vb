Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI

Public Class frmTakeSnapshot

    Public mAMI As clsAMI

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Close()
    End Sub

    Private Sub btnElementTree_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnElementTree.Click
        Dim err As clsAMIMessage = Nothing
        Dim res As String = Nothing
        If Not mAMI.DoDiagnosticAction("WindowsSnapshot", res, err) Then
            MessageBox.Show(String.Format(My.Resources.FailedToGetSnapshot0, err.Message))
            Exit Sub
        End If
        Dim f As New frmSnapshot()
        f.mText = res
        f.Show()
    End Sub

    Private Sub btnHTMLSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHTMLSource.Click
        Dim err As clsAMIMessage = Nothing
        Dim res As String = Nothing
        If Not mAMI.DoDiagnosticAction("HTMLSourceCap", res, err) Then
            MessageBox.Show(String.Format(My.Resources.FailedToGetSnapshot0, err.Message))
            Exit Sub
        End If
        Dim f As New frmSnapshot()
        f.mText = res
        f.Show()
    End Sub
End Class
