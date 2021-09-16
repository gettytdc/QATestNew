Imports System.IO
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

Public Class frmAppManConfig

    Private Sub frmAppManConfig_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If File.Exists(clsConfig.ConfigFilespec) Then
            txtConfig.Text = File.ReadAllText(clsConfig.ConfigFilespec)
            txtConfig.Select(0, 0)
        End If
    End Sub

    Private Sub btnSetDefault_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetDefault.Click
        txtConfig.Text = "<?xml version=""1.0""?>" & vbCrLf & _
         "<config>" & vbCrLf & _
         "  <loggingenabled>false</loggingenabled>" & vbCrLf & 
         "  <logdir>" & Path.Combine(Path.GetTempPath(), "appmanlog") & "</logdir>" & vbCrLf & _
         "  <logtimings>false</logtimings>" & vbCrLf & _
         "  <logwin32>false</logwin32>" & vbCrLf & _
         "  <logwait>false</logwait>" & vbCrLf & _
         "  <logfontrec>false</logfontrec>" & vbCrLf & _
         "  <loghook>false</loghook>" & vbCrLf & _
         "  <logexceptions>false</logexceptions>" & vbCrLf & _
         "  <logretries>false</logretries>" & vbCrLf & _
         "  <agentdiags>false</agentdiags>" & vbCrLf & _
         "  <userfontdirectory></userfontdirectory>" & vbCrLf & _
         "  <logjab>false</logjab>" & vbCrLf & _
         "  <loghtml>false</loghtml>" & vbCrLf & _
         "  <ThoroughJAB>false</ThoroughJAB>" & vbCrLf & _
         "</config>" & vbCrLf
    End Sub


    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
        txtConfig.Text = ""
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Close()
    End Sub


    Private Sub btnSaveAndExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSaveAndExit.Click
        If txtConfig.Text = "" Then
            If File.Exists(clsConfig.ConfigFilespec) Then
                File.Delete(clsConfig.ConfigFilespec)
            End If
        Else
            File.WriteAllText(clsConfig.ConfigFilespec, txtConfig.Text)
        End If

        Dim sErr As String = Nothing
        If Not clsConfig.Init(sErr) Then
            MessageBox.Show(sErr, My.Resources.xError)
            Exit Sub
        End If
        Close()
    End Sub

End Class
