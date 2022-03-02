Imports BluePrism.AutomateAppCore

Public Class frmConflictDataHandler

#Region "Constructors"

    Public Sub New(ByVal conflict As Conflict, ByVal conflictOption As ConflictOption, ByVal handler As ConflictDataHandler)
        InitializeComponent()
        Me.Text = conflictOption.Text
        Me.conflictDataHandler.ConflictDataHandler = handler
        Me.conflictDataHandler.Conflict = conflict
        Me.conflictDataHandler.ConflictOption = conflictOption
    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Me.Close()
    End Sub

#End Region

End Class