Imports BluePrism.CharMatching

Public Class frmSysFontPicker

    Public Sub DoLoad(ByVal Sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim fonts As New Text.InstalledFontCollection()

        For Each f As FontFamily In fonts.Families
            cmbFontFamily.Items.Add(f.Name)
        Next
        If cmbFontFamily.Items.Count > 0 Then
            cmbFontFamily.SelectedIndex = 0
        End If

        For s As Single = 7 To 20 Step 0.5
            cmbSize.Items.Add(s.ToString)
        Next
        cmbSize.Items.Add("22")
        cmbSize.SelectedItem = "10"
    End Sub

    Public ReadOnly Property SelectedFont() As String
        Get
            Return CStr(cmbFontFamily.SelectedItem)
        End Get
    End Property

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        If FontConfig.ValidateEnvironment(TopLevelControl) Then
            If cmbFontFamily.SelectedItem Is Nothing Then
                MsgBox(My.Resources.PleaseSelectAFont)
                Exit Sub
            End If

            DialogResult = Windows.Forms.DialogResult.OK
            Close()
        End If
    End Sub

    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        DialogResult = Windows.Forms.DialogResult.Cancel
        Close()
    End Sub

End Class