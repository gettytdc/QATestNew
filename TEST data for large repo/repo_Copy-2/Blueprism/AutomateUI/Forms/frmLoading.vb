Imports BluePrism.AutomateAppCore

Public Class frmLoading
    Private Sub frmLoading_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        gSv.GetEnvironmentColors(BackColor, ForeColor)
    End Sub
End Class